using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Weather
{
    public class WeatherManager : MonoBehaviour
    {
        [SerializeField] private WeatherType startingWeather = WeatherType.Clear;
        [SerializeField] private List<WeatherDefinition> weatherDefinitions = new List<WeatherDefinition>();

        [Header("Auto Change")]
        [SerializeField] private bool autoChange = true;
        [SerializeField] private float weatherChangeInterval = 20f;

        private readonly Dictionary<WeatherType, WeatherDefinition> _definitionsByType =
            new Dictionary<WeatherType, WeatherDefinition>();

        public WeatherType CurrentWeatherType { get; private set; }
        public WeatherDefinition CurrentDefinition { get; private set; }
        public WeatherModifiers CurrentModifiers { get; private set; } = WeatherModifiers.Identity;

        private float _timer;

        public float WeatherChangeInterval => weatherChangeInterval;

        public float TimeToNextChange
        {
            get
            {
                if (!autoChange || weatherChangeInterval <= 0f)
                    return -1f;

                return Mathf.Clamp(weatherChangeInterval - _timer, 0f, weatherChangeInterval);
            }
        }

        public event Action<WeatherType, WeatherModifiers> OnWeatherChanged;

        private void Awake()
        {
            _definitionsByType.Clear();
            foreach (var def in weatherDefinitions)
            {
                if (def == null) continue;
                _definitionsByType[def.type] = def;
            }

            SetWeather(startingWeather);
        }

        private void Update()
        {
            if (!autoChange || weatherChangeInterval <= 0f)
                return;

            _timer += Time.deltaTime;
            if (_timer >= weatherChangeInterval)
            {
                _timer = 0f;
                CycleWeather();
            }
        }

        public void SetWeather(WeatherType type)
        {
            _timer = 0f;
            CurrentWeatherType = type;

            _definitionsByType.TryGetValue(type, out var def);
            CurrentDefinition = def;
            CurrentModifiers = WeatherModifiers.FromDefinition(def);

            OnWeatherChanged?.Invoke(CurrentWeatherType, CurrentModifiers);
        }

        public void CycleWeather()
        {
            int next = ((int)CurrentWeatherType + 1) % Enum.GetValues(typeof(WeatherType)).Length;
            SetWeather((WeatherType)next);
        }
    }
}

