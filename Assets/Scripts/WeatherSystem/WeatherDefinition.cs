using UnityEngine;

namespace Game.Weather
{
    [CreateAssetMenu(menuName = "Game/Weather/Weather Definition", fileName = "WeatherDefinition")]
    public class WeatherDefinition : ScriptableObject
    {
        [Header("General")]
        public WeatherType type = WeatherType.Clear;

        [Header("Movement Modifiers")]
        public float moveSpeedMultiplier = 1f;
        public float accelerationMultiplier = 1f;
        public float decelerationMultiplier = 1f;
        public float gravityScaleMultiplier = 1f;
        public float controlMultiplier = 1f;

        [Header("Surface / Friction")]
        public float frictionMultiplier = 1f;

        [Header("Wind")]
        public Vector2 windForcePerSecond = Vector2.zero;

        [Header("Fog")]
        [Range(0f, 1f)]
        public float fogVisibility = 1f;

        [Header("Melee")]
        public float meleeCooldownMultiplier = 1f;
    }
}

