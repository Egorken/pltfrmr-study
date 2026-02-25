using UnityEngine;
using UnityEngine.UI;

namespace Game.Weather
{
    [RequireComponent(typeof(Canvas))]
    public class WeatherFogOverlay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private Transform player;
        [SerializeField] private Camera targetCamera;

        [Header("Fog appearance")]
        [SerializeField] private Color fogColor = new Color(0.85f, 0.88f, 0.9f, 1f);
        [SerializeField] private float visibilityRadiusWorld = 6f;
        [SerializeField, Range(0f, 2f)] private float softness = 0.15f;
        [SerializeField, Range(0f, 1f)] private float maxFogAlpha = 1f;

        [Header("Setup")]
        [SerializeField] private Shader fogShader;
        [SerializeField] private bool createOverlayAutomatically = true;

        private RawImage _overlayImage;
        private Material _material;
        private float _currentFogVisibility = 1f;
        private static readonly int FogColorId = Shader.PropertyToID("_FogColor");
        private static readonly int CenterId = Shader.PropertyToID("_Center");
        private static readonly int ClearRadiusId = Shader.PropertyToID("_ClearRadius");
        private static readonly int SoftnessId = Shader.PropertyToID("_Softness");
        private static readonly int MaxAlphaId = Shader.PropertyToID("_MaxAlpha");

        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (fogShader == null)
                fogShader = Shader.Find("Game/RadialFog");
            if (fogShader != null)
                _material = new Material(fogShader);

            if (createOverlayAutomatically)
                EnsureOverlayExists();
            else
                _overlayImage = GetComponentInChildren<RawImage>();
        }

        private void Start()
        {
            if (player == null)
                player = GameObject.FindWithTag("Player")?.transform;
            if (targetCamera == null)
                targetCamera = Camera.main;

            WeatherManager wm = weatherManager ?? FindObjectOfType<WeatherManager>();
            if (wm != null)
            {
                wm.OnWeatherChanged += OnWeatherChanged;
                _currentFogVisibility = wm.CurrentModifiers.fogVisibility;
            }
        }

        private void OnDestroy()
        {
            var wm = weatherManager ?? FindObjectOfType<WeatherManager>();
            if (wm != null)
                wm.OnWeatherChanged -= OnWeatherChanged;
            if (_material != null)
                Destroy(_material);
        }

        private void EnsureOverlayExists()
        {
            var img = GetComponentInChildren<RawImage>();
            if (img != null && img.transform != transform)
            {
                _overlayImage = img;
                ApplyMaterial();
                return;
            }

            GameObject go = new GameObject("FogOverlay");
            go.transform.SetParent(transform, false);
            go.transform.SetAsLastSibling();

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _overlayImage = go.AddComponent<RawImage>();
            _overlayImage.color = Color.white;
            _overlayImage.raycastTarget = false;
            ApplyMaterial();
        }

        private void ApplyMaterial()
        {
            if (_overlayImage != null && _material != null)
                _overlayImage.material = _material;
        }

        private void OnWeatherChanged(WeatherType _, WeatherModifiers mods)
        {
            _currentFogVisibility = mods.fogVisibility;
        }

        private void LateUpdate()
        {
            if (_material == null || _overlayImage == null || targetCamera == null) return;

            float radiusScale = 0.25f + 0.75f * Mathf.Clamp01(_currentFogVisibility);
            float radiusWorld = visibilityRadiusWorld * radiusScale;

            Vector2 center = new Vector2(0.5f, 0.5f);
            if (player != null && targetCamera != null)
            {
                Vector3 viewport = targetCamera.WorldToViewportPoint(player.position);
                center = new Vector2(viewport.x, viewport.y);
            }

            float orthoSize = targetCamera.orthographicSize;
            float aspect = (float)Screen.width / Screen.height;
            float radiusX = radiusWorld / (2f * orthoSize * aspect);
            float radiusY = radiusWorld / (2f * orthoSize);

            float fogIntensity = 1f - Mathf.Clamp01(_currentFogVisibility);
            float effectiveMaxAlpha = fogIntensity > 0.01f ? maxFogAlpha : 0f;

            _material.SetColor(FogColorId, fogColor);
            _material.SetVector(CenterId, new Vector4(center.x, center.y, 0, 0));
            _material.SetVector(ClearRadiusId, new Vector4(radiusX, radiusY, 0, 0));
            _material.SetFloat(SoftnessId, softness);
            _material.SetFloat(MaxAlphaId, effectiveMaxAlpha);
        }
    }
}
