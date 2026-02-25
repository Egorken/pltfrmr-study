using Game.Player;
using Game.Weather;
using TMPro;
using UnityEngine;

namespace Game.UI.DebugUI
{
    public class PlayerDebugUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Rigidbody2D playerRb;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private WeatherManager weatherManager;

        [Header("UI Texts")]
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text weatherText;
        [SerializeField] private TMP_Text weatherTimerText;
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private TMP_Text groundedText;
        [SerializeField] private TMP_Text jumpsText;

        private void Reset()
        {
            if (playerController == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerController = playerObj.GetComponent<PlayerController>();
                    playerRb = playerObj.GetComponent<Rigidbody2D>();
                    playerHealth = playerObj.GetComponent<PlayerHealth>();
                }
            }

            if (weatherManager == null)
            {
                weatherManager = FindObjectOfType<WeatherManager>();
            }
        }

        private void Awake()
        {
            TryFindPlayer();
        }

        private void Start()
        {
            TryFindPlayer();
        }

        private void TryFindPlayer()
        {
            if (playerController != null && playerHealth != null) return;

            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) return;

            if (playerController == null) playerController = playerObj.GetComponent<PlayerController>();
            if (playerRb == null) playerRb = playerObj.GetComponent<Rigidbody2D>();
            if (playerHealth == null) playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            UpdateState();
            UpdateHealth();
            UpdateWeather();
            UpdateWeatherTimer();
            UpdateSpeed();
            UpdateGrounded();
            UpdateJumps();
        }

        private void UpdateState()
        {
            if (stateText == null) return;

            string stateName = "None";
            if (playerController != null)
            {
                stateName = playerController.CurrentStateName;
            }

            stateText.text = $"State: {stateName}";
        }

        private void UpdateHealth()
        {
            if (healthText == null) return;
            if (playerHealth == null)
            {
                TryFindPlayer();
                if (playerHealth == null) return;
            }

            healthText.text = $"HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";
        }

        private void UpdateWeather()
        {
            if (weatherText == null) return;

            string w = weatherManager != null
                ? weatherManager.CurrentWeatherType.ToString()
                : "None";

            weatherText.text = $"Weather: {w}";
        }

        private void UpdateWeatherTimer()
        {
            if (weatherTimerText == null || weatherManager == null) return;

            float timeLeft = weatherManager.TimeToNextChange;

            if (timeLeft < 0f)
            {
                weatherTimerText.text = "Next weather: -";
            }
            else
            {
                weatherTimerText.text = $"Next weather in: {timeLeft:0.0}s";
            }
        }

        private void UpdateSpeed()
        {
            if (speedText == null) return;

            float speed = 0f;
            if (playerRb != null)
            {
                speed = playerRb.velocity.magnitude;
            }

            speedText.text = $"Speed: {speed:0.00}";
        }

        private void UpdateGrounded()
        {
            if (groundedText == null) return;

            bool grounded = playerController != null && playerController.IsGrounded;
            groundedText.text = $"Grounded: {grounded}";
        }

        private void UpdateJumps()
        {
            if (jumpsText == null) return;

            int left = playerController != null ? playerController.AirJumpsLeft : 0;
            jumpsText.text = $"Jumps: {left}";
        }
    }
}

