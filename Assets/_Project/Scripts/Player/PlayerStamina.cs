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

    [Header("UI Animation References")]
    [SerializeField] private Animator staminaAnimator;

    private float _currentStamina;
    private float regenTimer;
    private float uiHideTimer;
    private PlayerMovement playerMovement;

    public float CurrentStamina => _currentStamina;

    private void Awake()
    {
        InitializeComponents();
        InitializeUI();
    }

    private void Update()
    {
        HandleStamina();
        HandleUIVisibility();
    }

    private void InitializeComponents()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component is missing on PlayerStamina.");
            enabled = false;
            return;
        }

        _currentStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.gameObject.SetActive(false); // Hide the slider initially
        }
        else
        {
            Debug.LogWarning("StaminaSlider is not assigned in PlayerStamina.");
        }
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

    private void HandleStamina()
    {
        if (playerMovement.IsRunning && playerMovement.IsMoving)
        {
            ConsumeStamina();
        }
        else if (_currentStamina < maxStamina)
        {
            RegenerateStamina();
        }

        _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);
        UpdateStaminaUI();
    }

    private void ConsumeStamina()
    {
        _currentStamina -= runCostPerSecond * Time.deltaTime;
        regenTimer = 0f;
        uiHideTimer = 0f;

        if (_currentStamina <= 0)
        {
            _currentStamina = 0;
            playerMovement.ToggleRun(false);
        }
    }

    private void RegenerateStamina()
    {
        regenTimer += Time.deltaTime;

        if (regenTimer >= delayBeforeRegen)
        {
            float regenRate = playerMovement.IsMoving ? walkRegenRate : idleRegenRate;
            _currentStamina += regenRate * Time.deltaTime;
        }
    }

    private void UpdateStaminaUI()
    {
        HandleUIAnimation();
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

        // Ensure slider is visible when needed
        bool shouldShow = playerMovement.IsRunning || _currentStamina < maxStamina || playerMovement.IsMoving || uiHideTimer < uiHideDelay;
        staminaSlider.gameObject.SetActive(shouldShow);

        // Update uiHideTimer
        if (!playerMovement.IsRunning && _currentStamina >= maxStamina && !playerMovement.IsMoving)
        {
            uiHideTimer += Time.deltaTime;
        }
        else
        {
            uiHideTimer = 0f;
        }
    }

    private void HandleUIAnimation()
    {
        if (staminaAnimator != null)
        {
            staminaAnimator.SetBool("Show", _currentStamina < maxStamina);
        }
    }

    public bool CanRun()
    {
        return _currentStamina > 0 && _currentStamina > runCostPerSecond * 0.1f;
    }
}
