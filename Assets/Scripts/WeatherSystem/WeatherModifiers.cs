using UnityEngine;

namespace Game.Weather
{
    public struct WeatherModifiers
    {
        public float moveSpeedMultiplier;
        public float accelerationMultiplier;
        public float decelerationMultiplier;
        public float gravityScaleMultiplier;
        public float controlMultiplier;
        public float frictionMultiplier;
        public Vector2 windForcePerSecond;
        public float fogVisibility;
        public float meleeCooldownMultiplier;

        public static WeatherModifiers FromDefinition(WeatherDefinition def)
        {
            if (def == null)
            {
                return Identity;
            }

            return new WeatherModifiers
            {
                moveSpeedMultiplier = def.moveSpeedMultiplier,
                accelerationMultiplier = def.accelerationMultiplier,
                decelerationMultiplier = def.decelerationMultiplier,
                gravityScaleMultiplier = def.gravityScaleMultiplier,
                controlMultiplier = def.controlMultiplier,
                frictionMultiplier = def.frictionMultiplier,
                windForcePerSecond = def.windForcePerSecond,
                fogVisibility = def.fogVisibility,
                meleeCooldownMultiplier = def.meleeCooldownMultiplier
            };
        }

        public static WeatherModifiers Identity => new WeatherModifiers
        {
            moveSpeedMultiplier = 1f,
            accelerationMultiplier = 1f,
            decelerationMultiplier = 1f,
            gravityScaleMultiplier = 1f,
            controlMultiplier = 1f,
            frictionMultiplier = 1f,
            windForcePerSecond = Vector2.zero,
            fogVisibility = 1f,
            meleeCooldownMultiplier = 1f
        };
    }
}

