using UnityEngine;

namespace Game.LevelSystem
{
    [RequireComponent(typeof(Collider2D))]
    public class BreakablePlatform : MonoBehaviour
    {
        [Header("Таймер")]
        [SerializeField] private float breakDelay = 0.8f;
        [SerializeField] private float respawnDelay = 3f;
        [SerializeField] private float destroyAfterBreak = 0.3f;

        [Header("Мигание")]
        [SerializeField] private Color blinkColor = new Color(1f, 0.5f, 0.5f);
        [SerializeField] private float blinkInterval = 0.1f;
        [SerializeField] private string onlyBreakForTag = "Player";

        private Collider2D _collider;
        private SpriteRenderer[] _renderers;
        private Color[] _originalColors;
        private float _breakTimer = -1f;
        private float _blinkAccum;
        private float _respawnTimer = -1f;
        private bool _broken;
        private bool _blinkOn;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _renderers = GetComponentsInChildren<SpriteRenderer>();
            _originalColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _originalColors[i] = _renderers[i].color;
            _blinkOn = false;
        }

        private void SetBlinkColor(bool useBlink)
        {
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].color = useBlink ? blinkColor : _originalColors[i];
            }
        }

        private void Update()
        {
            if (_broken)
            {
                if (respawnDelay > 0f && _respawnTimer >= 0f)
                {
                    _respawnTimer -= Time.deltaTime;
                    if (_respawnTimer <= 0f)
                        Respawn();
                }
                return;
            }
            if (_breakTimer < 0f) return;

            _breakTimer -= Time.deltaTime;
            _blinkAccum += Time.deltaTime;
            if (_blinkAccum >= blinkInterval)
            {
                _blinkAccum -= blinkInterval;
                _blinkOn = !_blinkOn;
                SetBlinkColor(_blinkOn);
            }

            if (_breakTimer <= 0f)
                Break();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (_broken) return;
            if (_breakTimer >= 0f) return; // уже заведён таймер

            if (col.rigidbody == null) return;
            if (!string.IsNullOrEmpty(onlyBreakForTag) && !col.gameObject.CompareTag(onlyBreakForTag))
                return;

            foreach (ContactPoint2D contact in col.contacts)
            {
                if (contact.normal.y >= 0.5f || contact.normal.y <= -0.5f)
                {
                    _breakTimer = breakDelay;
                    return;
                }
            }

            float myY = transform.position.y;
            float otherY = col.rigidbody.position.y;
            if (otherY > myY + 0.1f)
                _breakTimer = breakDelay;
        }

        private void Break()
        {
            _broken = true;
            if (_collider != null)
                _collider.enabled = false;

            SetRenderersVisible(false);

            if (respawnDelay > 0f)
            {
                _respawnTimer = respawnDelay;
            }
            else
            {
                Invoke(nameof(DestroySelf), destroyAfterBreak);
            }
        }

        private void Respawn()
        {
            _broken = false;
            _breakTimer = -1f;
            _respawnTimer = -1f;
            _blinkOn = false;

            if (_collider != null)
                _collider.enabled = true;

            SetRenderersVisible(true);
            SetBlinkColor(false);
        }

        private void SetRenderersVisible(bool visible)
        {
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].enabled = visible;
            }
        }

        private void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
