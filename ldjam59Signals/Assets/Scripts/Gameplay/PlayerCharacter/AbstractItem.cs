using UnityEngine;

public abstract class AbstractItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] protected string itemName = "Unknown Item";
    [SerializeField] protected Sprite itemIcon;
    [SerializeField] protected bool canHolster = true;
    
    // Свойства для доступа к данным предмета
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;
    public bool CanHolster => canHolster;
    
    // Ссылка на владельца предмета
    protected FirstPersonController owner;
    protected Transform itemTransform;
    
    // События, на которые могут подписываться другие системы
    
    /// <summary>
    /// Вызывается когда предмет берут в руки
    /// </summary>
    /// <param name="ownerController">Контроллер персонажа-владельца</param>
    public virtual void OnEquip(FirstPersonController ownerController)
    {
        owner = ownerController;
        itemTransform = transform;
        
        // Активируем объект если он был выключен
        gameObject.SetActive(true);
        
        // Размещаем предмет в руках
        PositionItemInHands();
        
        // Воспроизводим анимацию взятия (если есть)
        PlayEquipAnimation();
        
        OnItemEquipped();
        Debug.Log($"[{itemName}] Equipped");
    }

    public abstract void PrimaryAction();
    public abstract void SecondaryAction();

    public virtual void PrimaryActionStop() { }
    public virtual void SecondaryActionStop() { }

    protected virtual void OnItemEquipped()
    {
        
    }

    public virtual void LightSwitch()
    {
        
    }

    protected virtual void OnItemHolstered()
    {
        
    }
    
    /// <summary>
    /// Вызывается когда предмет убирают из рук
    /// </summary>
    public virtual void OnHolster()
    {
        // Воспроизводим анимацию убирания
        PlayHolsterAnimation();

        OnItemHolstered();
        
        Debug.Log($"[{itemName}] Holstered");
        
        // Деактивируем объект
        gameObject.SetActive(false);
        
        owner = null;
    }
    
    
    /// <summary>
    /// Вызывается каждый кадр пока предмет в руках
    /// </summary>
    public virtual void OnUpdate()
    {
        // Может быть переопределено для постоянных эффектов
    }
    
    /// <summary>
    /// Размещает предмет в позиции рук
    /// </summary>
    protected virtual void PositionItemInHands()
    {
        if (owner == null) return;
        
        // По умолчанию ищем точку крепления "HandSocket" у камеры
        Transform handSocket = owner.GetComponentInChildren<Camera>()?.transform.Find("HandSocket");
        
        if (handSocket != null)
        {
            itemTransform.SetParent(handSocket);
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localRotation = Quaternion.identity;
        }
        else
        {
            // Если сокет не найден, крепим прямо к камере
            Camera playerCamera = owner.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                itemTransform.SetParent(playerCamera.transform);
                itemTransform.localPosition = new Vector3(0.5f, -0.3f, 0.5f);
                itemTransform.localRotation = Quaternion.Euler(0, -90, 0);
            }
        }
    }
    
    /// <summary>
    /// Воспроизводит анимацию взятия предмета
    /// </summary>
    protected virtual void PlayEquipAnimation()
    {
        // Может быть переопределено для проигрывания анимаций
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Equip");
        }
    }
    
    /// <summary>
    /// Воспроизводит анимацию убирания предмета
    /// </summary>
    protected virtual void PlayHolsterAnimation()
    {
        // Может быть переопределено для проигрывания анимаций
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Holster");
        }
    }
    
    /// <summary>
    /// Проверяет, можно ли использовать предмет сейчас
    /// </summary>
    public virtual bool CanUse()
    {
        return owner != null && gameObject.activeInHierarchy;
    }
}