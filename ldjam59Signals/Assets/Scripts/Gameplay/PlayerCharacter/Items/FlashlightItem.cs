using UnityEngine;

public class FlashlightItem : AbstractItem
{
    [Header("Flashlight Settings")]
    [SerializeField] private Light flashlightLight;
    [SerializeField] private float batteryLife = 100f;
    [SerializeField] private float drainRate = 5f;
    [SerializeField] private AudioClip toggleSound;
    [SerializeField] private float intensityModeOne;
    [SerializeField] private float intensityModeTwo;
    
    private bool isOn = false;
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (flashlightLight == null)
            flashlightLight = GetComponentInChildren<Light>();
        SecondaryAction();
    }
    
    public override void OnEquip(FirstPersonController ownerController)
    {
        base.OnEquip(ownerController);
        // Включаем фонарик автоматически при взятии
        SetLightState(true);
    }
    
    public override void OnHolster()
    {
        // Выключаем при убирании
        SetLightState(false);
        base.OnHolster();
    }
    
    public override void PrimaryAction()
    {
        ToggleLight();
    }
    
    public override void SecondaryAction()
    {
        if (flashlightLight != null)
        {
            flashlightLight.intensity = flashlightLight.intensity == intensityModeOne? intensityModeTwo : intensityModeOne;
        }
    }
    
    public override void OnUpdate()
    {
        base.OnUpdate();
        
        // Расход батареи
        if (isOn && batteryLife > 0)
        {
            batteryLife -= drainRate * Time.deltaTime;
            if (batteryLife <= 0)
            {
                SetLightState(false);
            }
        }
    }
    
    private void ToggleLight()
    {
        SetLightState(!isOn);
    }
    
    private void SetLightState(bool state)
    {
        if (batteryLife <= 0 && state) return;
        
        isOn = state;
        if (flashlightLight != null)
            flashlightLight.enabled = isOn;
        
        if (audioSource != null && toggleSound != null)
            audioSource.PlayOneShot(toggleSound);
    }
}