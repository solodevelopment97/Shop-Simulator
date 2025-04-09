using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float runCostPerSecond = 20f;
    [SerializeField] private float walkRegenRate = 10f;
    [SerializeField] private float idleRegenRate = 15f;
    [SerializeField] private float delayBeforeRegen = 1.5f;

    [Header("UI References")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image fillArea;
    [SerializeField] private Gradient staminaGradient;
    [SerializeField] private float uiHideDelay = 3f;

    public float currentStamina => _currentStamina;
    private float _currentStamina;
    private float regenTimer;
    private float uiHideTimer;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        _currentStamina = maxStamina;
        staminaSlider.gameObject.SetActive(false); // Hide the slider initially
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = maxStamina;
            UpdateStaminaColor();
        }
    }

    private void Update()
    {
        HandleStamina();
        HandleUIVisibility();
    }

    private void HandleStamina()
    {
        // Konsumsi stamina saat lari
        if (playerMovement.IsRunning && playerMovement.IsMoving)
        {
            _currentStamina -= runCostPerSecond * Time.deltaTime;
            regenTimer = 0f;
            uiHideTimer = 0f;

            if (currentStamina <= 0)
            {
                _currentStamina = 0;
                playerMovement.ToggleRun(false);
            }
        }
        // Regenerasi stamina
        else if (currentStamina < maxStamina)
        {
            regenTimer += Time.deltaTime;

            if (regenTimer >= delayBeforeRegen)
            {
                float regenRate = playerMovement.IsMoving ? walkRegenRate : idleRegenRate;
                _currentStamina += regenRate * Time.deltaTime;
            }
        }

        _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);
        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = _currentStamina;
            UpdateStaminaColor();
        }
    }

    private void UpdateStaminaColor()
    {
        if (fillArea != null)
        {
            float normalizedStamina = _currentStamina / maxStamina;
            fillArea.color = staminaGradient.Evaluate(normalizedStamina);
        }
    }

    private void HandleUIVisibility()
    {
        if (staminaSlider == null) return;

        bool shouldShow = playerMovement.IsRunning || _currentStamina < maxStamina || uiHideTimer < uiHideDelay;
        staminaSlider.gameObject.SetActive(shouldShow);

        if (!playerMovement.IsRunning && _currentStamina >= maxStamina)
        {
            uiHideTimer += Time.deltaTime;
        }
        else
        {
            uiHideTimer = 0f;
        }
    }

    public bool CanRun()
    {
        return _currentStamina > 0 && _currentStamina > runCostPerSecond * 0.1f;
    }
}