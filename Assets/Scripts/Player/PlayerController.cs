using System;
using System.Reflection;
using Game.LevelSystem;
using Game.Weather;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private WeatherManager weatherManager;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Movement")]
        [SerializeField] private float baseMaxMoveSpeed = 8f;
        [SerializeField] private float baseDeceleration = 30f;

        [Header("Jump")]
        [SerializeField] private float baseJumpForce = 15f;
        [SerializeField] private int airJumpsCount = 2;
        [SerializeField] private float baseGravityScale = 4f;
        [SerializeField] private float baseFallGravityScale = 6f;
        [SerializeField] private float baseLowJumpGravityScale = 8f;

        [Header("Wall Cling")]
        [SerializeField] private float wallCheckDistance = 0.25f;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private float wallSlideSpeed = 1.5f;
        [SerializeField] private float wallStickSpeed = 0.5f;
        [SerializeField] private float wallJumpHorizontalForce = 6f;
        [SerializeField] private float airDeceleration = 15f;
        [SerializeField] private PhysicsMaterial2D noFrictionMaterialForWallSlide;
        [SerializeField] private float wallJumpClingCooldown = 0.25f;

        [Header("Dash (рывок)")]
        [SerializeField] private float dashSpeed = 18f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

        [Header("Ladder")]
        [SerializeField] private float ladderClimbSpeed = 4f;
        [SerializeField] private float ladderDismountCooldown = 0.25f;

        [Header("One-Way Platforms")]
        [SerializeField] private float dropThroughIgnoreTime = 0.4f;
        [SerializeField] private float jumpThroughRayDistance = 1.2f;
        [SerializeField] private float jumpThroughIgnoreTime = 0.35f;

        private WeatherModifiers _mods = WeatherModifiers.Identity;
        private PhysicsMaterial2D _defaultPhysicsMaterial;
        private float _baseFriction;
        private float _inputX;
        private float _inputXRaw;
        private bool _jumpPressed;
        private bool _jumpHeld;
        private bool _jumpReleased;
        private float _groundedTime;
        private int _airJumpsLeft;
        private int _wallDirection;
        private float _lastWallJumpTime = -999f;
        private float _timeLeftGround = -999f;
        private bool _didStepOffRefillThisFall;
        private bool _leftGroundByJumping; // true = оторвались прыжком, не пополняем в step-off window
        private bool _wasGroundedLastFrame;
        private bool _wasWallClingingLastFrame;
        private Collider2D _ignoredPlatformCollider;
        private float _ignorePlatformUntilTime = -999f;
        private Collider2D _ignoredPlatformJumpThrough;
        private float _ignorePlatformJumpUntilTime = -999f;
        private int _ladderTriggerCount;
        private float _lastLadderDismountTime = -999f;
        private bool _dashPressed;
        private float _lastDashTime = -999f;
        private float _dashUntilTime = -999f;
        private float _dashDirection;
        private float _lastFacingDirection = 1f;

        public float InputX => _inputX;
        public bool JumpPressed => _jumpPressed;
        public bool JumpReleased => _jumpReleased;
        public bool JumpHeld => _jumpHeld;
        public bool IsGrounded { get; private set; }
        public bool IsWallClinging { get; private set; }
        public bool IsOnLadder => IsOverlappingLadder() && (Time.time - _lastLadderDismountTime) >= ladderDismountCooldown;
        public bool IsDashing => Time.time < _dashUntilTime;
        public int AirJumpsLeft => _airJumpsLeft;
        public float DashCooldownLeft => Mathf.Max(0f, dashCooldown - (Time.time - _lastDashTime));

        public void EnterLadder() => _ladderTriggerCount++;
        public void ExitLadder() { _ladderTriggerCount--; if (_ladderTriggerCount < 0) _ladderTriggerCount = 0; }

        private bool IsOverlappingLadder()
        {
            if (playerCollider == null) return _ladderTriggerCount > 0;
            Vector2 size = playerCollider.bounds.size;
            Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, size, 0f);
            foreach (Collider2D c in hits)
            {
                if (c == playerCollider) continue;
                if (c.isTrigger && c.GetComponent<Ladder>() != null) return true;
            }
            return _ladderTriggerCount > 0;
        }
        public float VerticalVelocity => rb != null ? rb.velocity.y : 0f;

        private PlayerStateMachine _stateMachine;
        public PlayerIdleState IdleState { get; private set; }
        public PlayerRunState RunState { get; private set; }
        public PlayerJumpState JumpState { get; private set; }
        public PlayerFallState FallState { get; private set; }
        public string CurrentStateName => _stateMachine?.CurrentState?.GetType().Name ?? "None";

        private void Reset()
        {
            rb = GetComponent<Rigidbody2D>();
            if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
        }

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
            rb.gravityScale = baseGravityScale;
            _defaultPhysicsMaterial = rb.sharedMaterial;
            if (rb.sharedMaterial != null) _baseFriction = rb.sharedMaterial.friction;
            if (weatherManager != null)
            {
                _mods = weatherManager.CurrentModifiers;
                weatherManager.OnWeatherChanged += OnWeatherChanged;
            }
            _stateMachine = new PlayerStateMachine();
            IdleState = new PlayerIdleState(this, _stateMachine);
            RunState = new PlayerRunState(this, _stateMachine);
            JumpState = new PlayerJumpState(this, _stateMachine);
            FallState = new PlayerFallState(this, _stateMachine);
        }

        private void OnDestroy()
        {
            if (weatherManager != null) weatherManager.OnWeatherChanged -= OnWeatherChanged;
        }

        private void Start() { _stateMachine.Initialize(IdleState); }

        private void OnWeatherChanged(WeatherType t, WeatherModifiers mods)
        {
            _mods = mods;
            PhysicsMaterial2D toApply = _defaultPhysicsMaterial != null ? _defaultPhysicsMaterial : rb.sharedMaterial;
            if (toApply != null)
            {
                float baseF = _baseFriction > 0f ? _baseFriction : toApply.friction;
                toApply.friction = baseF * _mods.frictionMultiplier;
            }
        }

        private void Update()
        {
            ReadInput();
            IsGrounded = CheckGrounded();
            UpdateWallCling();

            if (IsGrounded)
            {
                _groundedTime += Time.deltaTime;
                _didStepOffRefillThisFall = false;
                _leftGroundByJumping = false;
                // Пополняем только когда стоим/падаем (velocity.y <= 0). Иначе в кадр после прыжка рейкаст ещё в землю — и мы бы дали 3-й прыжок.
                if (rb == null || rb.velocity.y <= 0f)
                    _airJumpsLeft = airJumpsCount;
            }
            else
            {
                _groundedTime = 0f;
                if (_wasGroundedLastFrame)
                    _timeLeftGround = Time.time;
                bool inStepOffWindow = rb != null && (Time.time - _timeLeftGround) < stepOffRefillWindow && rb.velocity.y <= 0f;
                if (inStepOffWindow && !_didStepOffRefillThisFall && !_leftGroundByJumping)
                {
                    _airJumpsLeft = airJumpsCount;
                    _didStepOffRefillThisFall = true;
                }
                else if ((Time.time - _lastWallJumpTime) >= wallJumpClingCooldown && (
                    (IsWallClinging || _wasWallClingingLastFrame) ||
                    (!_leftGroundByJumping && TouchingWall())))
                    _airJumpsLeft = airJumpsCount;
            }
            _wasGroundedLastFrame = IsGrounded;
            _wasWallClingingLastFrame = IsWallClinging;

            _stateMachine.CurrentState?.HandleInput();
            _stateMachine.CurrentState?.LogicUpdate();
        }

        private void FixedUpdate()
        {
            if (_ignoredPlatformCollider != null && Time.time >= _ignorePlatformUntilTime)
            {
                if (playerCollider != null) Physics2D.IgnoreCollision(playerCollider, _ignoredPlatformCollider, false);
                _ignoredPlatformCollider = null;
            }
            if (_ignoredPlatformJumpThrough != null && Time.time >= _ignorePlatformJumpUntilTime)
            {
                if (playerCollider != null) Physics2D.IgnoreCollision(playerCollider, _ignoredPlatformJumpThrough, false);
                _ignoredPlatformJumpThrough = null;
            }

            if (rb != null && rb.velocity.y > 0f && playerCollider != null)
                TryIgnorePlatformAboveForJumpThrough();

            // Отцепление, если перестал держать в сторону стены (проверяем в Update тоже; здесь — на случай рассинхрона)
            float hFixed = GetHorizontalInputForWallRelease();
            if (IsWallClinging)
            {
                if (_wallDirection == -1 && hFixed >= -wallReleaseInputThreshold) IsWallClinging = false;
                else if (_wallDirection == 1 && hFixed <= wallReleaseInputThreshold) IsWallClinging = false;
            }

            // Трение о стену/врага даёт «залипание». В воздухе при контакте со стеной — всегда материал с Friction = 0 (скатывание, а не зависание).
            if (noFrictionMaterialForWallSlide != null)
            {
                bool airAndTouchingWall = !IsGrounded && TouchingWall();
                rb.sharedMaterial = airAndTouchingWall ? noFrictionMaterialForWallSlide : _defaultPhysicsMaterial;
            }

            if (Mathf.Abs(_inputXRaw) > 0.01f)
                _lastFacingDirection = Mathf.Sign(_inputXRaw);

            bool canStartDash = _dashPressed && (Time.time - _lastDashTime) >= dashCooldown && !IsOnLadder;
            if (canStartDash)
            {
                float dir = Mathf.Abs(_inputXRaw) > 0.01f ? Mathf.Sign(_inputXRaw) : _lastFacingDirection;
                _lastDashTime = Time.time;
                _dashUntilTime = Time.time + dashDuration;
                _dashDirection = dir;
            }
            _dashPressed = false;

            if (IsOnLadder)
            {
                float v = GetVerticalInputRaw();
                float h = GetHorizontalInputForWallRelease();
                float moveSpeed = baseMaxMoveSpeed * _mods.moveSpeedMultiplier;

                if (_jumpPressed)
                {
                    if (v < -0.5f)
                    {
                        rb.velocity = new Vector2(rb.velocity.x, 0f);
                        _lastLadderDismountTime = Time.time;
                        _ladderTriggerCount = 0;
                    }
                    else if (_airJumpsLeft > 0)
                    {
                        _airJumpsLeft--;
                        DoJump();
                        _lastLadderDismountTime = Time.time;
                        _ladderTriggerCount = 0;
                    }
                }
                else
                {
                    rb.gravityScale = 0f;
                    rb.velocity = new Vector2(h * moveSpeed, ladderClimbSpeed * v);
                }
            }
            else if (IsDashing)
            {
                rb.velocity = new Vector2(_dashDirection * dashSpeed, 0f);
            }
            else
            {
                ApplyJumpAndGravity();
                if (IsWallClinging)
                    ApplyWallSlide();
                else
                {
                    ApplyHorizontalMovement();
                    ApplyAirDeceleration();
                }
            }

            _jumpPressed = false;
            _jumpReleased = false;
            ApplyWind();
        }

        private void ReadInput()
        {
            float raw = Input.GetAxisRaw("Horizontal");
            _inputXRaw = raw;
            _inputX = raw * _mods.controlMultiplier;
            if (Input.GetButtonDown("Jump")) { _jumpPressed = true; _jumpHeld = true; }
            if (Input.GetButtonUp("Jump")) { _jumpReleased = true; _jumpHeld = false; }
            if (Input.GetKeyDown(dashKey)) _dashPressed = true;
        }

        private bool CheckGrounded()
        {
            Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : rb.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
            if (hit.collider == null) return false;
            if (hit.collider == _ignoredPlatformCollider) return false;
            return true;
        }

        private Collider2D GetGroundCollider()
        {
            Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : rb.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
            return hit.collider;
        }

        private static bool IsOneWayPlatform(Collider2D col)
        {
            if (col == null) return false;
            return col.GetComponent<PlatformEffector2D>() != null || col.GetComponent<OneWayPlatform>() != null;
        }

        private static float GetVerticalInputRaw()
        {
            float v = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(v) > 0.01f) return v;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) return -1f;
            return 0f;
        }

        private bool TryDropThroughPlatform()
        {
            Collider2D groundCol = GetGroundCollider();
            if (groundCol == null || !IsOneWayPlatform(groundCol)) return false;
            if (playerCollider == null) return false;
            if (_ignoredPlatformCollider != null) return false;

            Physics2D.IgnoreCollision(playerCollider, groundCol, true);
            _ignoredPlatformCollider = groundCol;
            _ignorePlatformUntilTime = Time.time + dropThroughIgnoreTime;
            return true;
        }

        private void TryIgnorePlatformAboveForJumpThrough()
        {
            if (_ignoredPlatformJumpThrough != null) return;

            float topOffset = playerCollider != null ? playerCollider.bounds.extents.y : 0.5f;
            Vector2 origin = (Vector2)transform.position + Vector2.up * topOffset;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, jumpThroughRayDistance, groundLayer);
            if (hit.collider == null) return;
            if (!IsOneWayPlatform(hit.collider)) return;

            Physics2D.IgnoreCollision(playerCollider, hit.collider, true);
            _ignoredPlatformJumpThrough = hit.collider;
            _ignorePlatformJumpUntilTime = Time.time + jumpThroughIgnoreTime;
        }

        private LayerMask GetWallLayer() => wallLayer.value != 0 ? wallLayer : groundLayer;

        private bool TouchLeftWall()
        {
            Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : rb.position;
            return Physics2D.Raycast(origin, Vector2.left, wallCheckDistance, GetWallLayer()).collider != null;
        }

        private bool TouchRightWall()
        {
            Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : rb.position;
            return Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, GetWallLayer()).collider != null;
        }

        private bool TouchingWall() => TouchLeftWall() || TouchRightWall();

        [Header("Wall release by input")]
        [SerializeField] private float wallReleaseInputThreshold = 0.15f;
        [SerializeField] private float stepOffRefillWindow = 0.2f;

        public static float HorizontalInputOverride { get; set; }

        private float GetHorizontalInputForWallRelease()
        {
            if (Mathf.Abs(HorizontalInputOverride) > 0.01f) return HorizontalInputOverride;
            float v = TryGetNewInputSystemHorizontal();
            if (Mathf.Abs(v) > 0.01f) return v;
            v = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(v) > 0.01f) return v;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) return -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) return 1f;
            return 0f;
        }

        private static float TryGetNewInputSystemHorizontal()
        {
            try
            {
                var keyboardType = System.Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
                if (keyboardType == null) return 0f;
                var currentProp = keyboardType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
                if (currentProp?.GetValue(null) == null) return 0f;
                object keyboard = currentProp.GetValue(null);
                var aKey = keyboardType.GetProperty("aKey")?.GetValue(keyboard);
                var dKey = keyboardType.GetProperty("dKey")?.GetValue(keyboard);
                var leftKey = keyboardType.GetProperty("leftArrowKey")?.GetValue(keyboard);
                var rightKey = keyboardType.GetProperty("rightArrowKey")?.GetValue(keyboard);
                var isPressed = aKey?.GetType().GetMethod("IsPressed", Type.EmptyTypes);
                if (isPressed != null)
                {
                    if ((bool)isPressed.Invoke(aKey, null) || (leftKey != null && (bool)isPressed.Invoke(leftKey, null)))
                        return -1f;
                    if ((bool)isPressed.Invoke(dKey, null) || (rightKey != null && (bool)isPressed.Invoke(rightKey, null)))
                        return 1f;
                }
            }
            catch { }
            return 0f;
        }

        private void UpdateWallCling()
        {
            if (IsGrounded) { IsWallClinging = false; return; }
            if (Time.time - _lastWallJumpTime < wallJumpClingCooldown) { IsWallClinging = false; return; }
            float h = GetHorizontalInputForWallRelease();
            // Цепляние только когда жмёшь В СТОРОНУ СТЕНЫ (влево у левой стены, вправо у правой). Пробел и «просто в воздухе» не цепляют.
            if (TouchLeftWall() && h < -wallReleaseInputThreshold)
            {
                IsWallClinging = true;
                _wallDirection = -1;
                return;
            }
            if (TouchRightWall() && h > wallReleaseInputThreshold)
            {
                IsWallClinging = true;
                _wallDirection = 1;
                return;
            }
            IsWallClinging = false;
        }

        private void ApplyWallSlide()
        {
            rb.velocity = new Vector2(_wallDirection * wallStickSpeed, -wallSlideSpeed);
        }

        private void ApplyAirDeceleration()
        {
            if (IsGrounded || Mathf.Abs(_inputX) > 0.01f) return;
            float vx = rb.velocity.x;
            float step = airDeceleration * Time.fixedDeltaTime;
            if (Mathf.Abs(vx) <= step) vx = 0f;
            else vx -= Mathf.Sign(vx) * step;
            rb.velocity = new Vector2(vx, rb.velocity.y);
        }

        private void ApplyHorizontalMovement()
        {
            float maxSpeed = baseMaxMoveSpeed * _mods.moveSpeedMultiplier;
            float decel = baseDeceleration * _mods.decelerationMultiplier * _mods.frictionMultiplier;
            float dt = Time.fixedDeltaTime;
            float current = rb.velocity.x;
            bool hasInput = Mathf.Abs(_inputX) > 0.01f;

            if (hasInput)
                current = _inputX * maxSpeed;
            else if (IsGrounded && decel > 0f)
            {
                float delta = decel * dt;
                if (Mathf.Abs(current) <= delta) current = 0f;
                else current -= Mathf.Sign(current) * delta;
            }

            rb.velocity = new Vector2(current, rb.velocity.y);
        }

        private void ApplyJumpAndGravity()
        {
            if (_jumpPressed && IsGrounded && GetVerticalInputRaw() < -0.5f && TryDropThroughPlatform())
                return;

            if (_jumpPressed && _airJumpsLeft > 0)
            {
                _airJumpsLeft--;
                if (IsWallClinging) DoWallJump();
                else
                {
                    DoJump();
                    if (IsGrounded) _leftGroundByJumping = true; // только что прыгнули с земли — не пополнять запас
                }
            }
            if (_jumpReleased && rb.velocity.y > 0f && !IsWallClinging)
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

            float gBase = baseGravityScale * _mods.gravityScaleMultiplier;
            float gFall = baseFallGravityScale * _mods.gravityScaleMultiplier;
            float gLow = baseLowJumpGravityScale * _mods.gravityScaleMultiplier;

            if (IsWallClinging) rb.gravityScale = 0f;
            else if (IsGrounded && VerticalVelocity <= 0f) rb.gravityScale = gBase;
            else if (VerticalVelocity < 0f) rb.gravityScale = gFall;
            else if (VerticalVelocity > 0f && !_jumpHeld) rb.gravityScale = gLow;
            else rb.gravityScale = gBase;
        }

        private void ApplyWind()
        {
            if (weatherManager == null) return;
            rb.AddForce(_mods.windForcePerSecond, ForceMode2D.Force);
        }

        public void DoJump()
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * baseJumpForce, ForceMode2D.Impulse);
        }

        public void DoWallJump()
        {
            if (!IsWallClinging) return;
            rb.velocity = new Vector2(-_wallDirection * wallJumpHorizontalForce, 0f);
            rb.AddForce(Vector2.up * baseJumpForce, ForceMode2D.Impulse);
            IsWallClinging = false;
            _lastWallJumpTime = Time.time;
        }

        public void ApplyJumpCut()
        {
            if (rb.velocity.y > 0f) rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        public void MoveHorizontally(float direction) { }
        public void StopHorizontalMovement() { }

        private void OnDrawGizmosSelected()
        {
            Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + Vector2.down * groundCheckDistance);
            Gizmos.DrawLine(origin, origin + Vector2.left * wallCheckDistance);
            Gizmos.DrawLine(origin, origin + Vector2.right * wallCheckDistance);
        }
    }
}
