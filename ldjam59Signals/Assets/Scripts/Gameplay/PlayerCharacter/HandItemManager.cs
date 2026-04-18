using UnityEngine;
using System.Collections.Generic;

public class HandItemManager : MonoBehaviour
{
    [Header("Item Slots")]
    [SerializeField] private AbstractItem[] availableItems;
    
    [Header("Settings")]
    [SerializeField] private float switchCooldown = 0.1f;
    [SerializeField] private Transform handSocket; // Точка крепления предметов
    
    private FirstPersonController playerController;
    private Camera playerCamera;
    private AbstractItem currentItem;
    private int currentItemIndex = -1; // -1 означает пустые руки
    private float lastSwitchTime;
    
    // События для других систем
    public System.Action<AbstractItem> OnItemSwitched;
    public System.Action OnHandsEmptied;
    
    // Публичные свойства
    public AbstractItem CurrentItem => currentItem;
    public int CurrentItemIndex => currentItemIndex;
    public bool HasItem => currentItem != null;
    
    private void Awake()
    {
        playerController = GetComponent<FirstPersonController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Создаем сокет для рук если не назначен
        if (handSocket == null && playerCamera != null)
        {
            GameObject socket = new GameObject("HandSocket");
            socket.transform.SetParent(playerCamera.transform);
            socket.transform.localPosition = new Vector3(0.5f, -0.3f, 0.5f);
            socket.transform.localRotation = Quaternion.Euler(0, -90, 0);
            handSocket = socket.transform;
        }
        
        // Инициализируем все доступные предметы
        InitializeItems();
    }
    
    private void Start()
    {
        // Начинаем с пустыми руками
        SetEmptyHands();
    }
    
    private void Update()
    {
        HandleItemSelection();
        HandleItemActions();
        
        // Обновляем текущий предмет если он есть
        if (currentItem != null)
        {
            currentItem.OnUpdate();
        }
    }
    
    private void InitializeItems()
    {
        for (int i = 0; i < availableItems.Length; i++)
        {
            if (availableItems[i] != null)
            {
                // Создаем экземпляр предмета если это префаб
                if (availableItems[i].gameObject.scene.rootCount == 0)
                {
                    availableItems[i] = Instantiate(availableItems[i]);
                }
                
                // Размещаем в сокете и выключаем
                availableItems[i].transform.SetParent(handSocket);
                availableItems[i].transform.localPosition = Vector3.zero;
                availableItems[i].transform.localRotation = Quaternion.identity;
                availableItems[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void HandleItemSelection()
    {
        // Проверяем кулдаун переключения
        if (Time.time - lastSwitchTime < switchCooldown)
            return;
        
        // Проверяем нажатие ESC для пустых рук
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentItem != null)
            {
                SetEmptyHands();
                lastSwitchTime = Time.time;
            }
            return;
        }
        
        for (int i = 0; i < availableItems.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectItemSlot(i);
                break;
            }
        }
    }
    
    private void SelectItemSlot(int slotIndex)
    {
        // Проверяем, выбран ли уже этот слот
        if (slotIndex == currentItemIndex)
        {
            // Повторное нажатие - убираем предмет
            SetEmptyHands();
            return;
        }
        
        // Проверяем валидность слота
        if (slotIndex < 0 || slotIndex >= availableItems.Length)
        {
            Debug.LogWarning($"Invalid item slot: {slotIndex}");
            return;
        }
        
        // Проверяем наличие предмета в слоте
        if (availableItems[slotIndex] == null)
        {
            Debug.LogWarning($"No item in slot {slotIndex}");
            return;
        }
        
        // Переключаем предмет
        SwitchToItem(slotIndex);
        lastSwitchTime = Time.time;
    }
    
    private void SwitchToItem(int slotIndex)
    {
        // Убираем текущий предмет
        if (currentItem != null)
        {
            currentItem.OnHolster();
        }
        
        // Берем новый предмет
        currentItem = availableItems[slotIndex];
        currentItemIndex = slotIndex;
        currentItem.OnEquip(playerController);
        
        OnItemSwitched?.Invoke(currentItem);
        Debug.Log($"Switched to item: {currentItem.ItemName} (Slot {slotIndex + 1})");
    }
    
    public void SetEmptyHands()
    {
        if (currentItem != null)
        {
            currentItem.OnHolster();
            currentItem = null;
            currentItemIndex = -1;
            OnHandsEmptied?.Invoke();
            Debug.Log("Hands emptied");
        }
    }
    
    private void HandleItemActions()
    {
        if (currentItem == null) return;
        
        // Основное действие (ЛКМ)
        if (Input.GetButtonDown("Fire1"))
        {
            currentItem.PrimaryAction();
        }
        
        // Дополнительное действие (ПКМ)
        if (Input.GetButtonDown("Fire2"))
        {
            currentItem.SecondaryAction();
        }
    }
    
    // Публичные методы для внешнего управления
    
    /// <summary>
    /// Добавляет новый предмет в указанный слот
    /// </summary>
    public void AddItemToSlot(int slot, AbstractItem itemPrefab)
    {
        if (slot < 0 || slot >= availableItems.Length)
        {
            Debug.LogError($"Invalid slot index: {slot}");
            return;
        }
        
        if (availableItems[slot] != null)
        {
            Debug.LogWarning($"Slot {slot} is already occupied");
            return;
        }
        
        AbstractItem newItem = Instantiate(itemPrefab);
        newItem.transform.SetParent(handSocket);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localRotation = Quaternion.identity;
        newItem.gameObject.SetActive(false);
        
        availableItems[slot] = newItem;
    }
    
    /// <summary>
    /// Удаляет предмет из слота
    /// </summary>
    public void RemoveItemFromSlot(int slot)
    {
        if (slot < 0 || slot >= availableItems.Length) return;
        
        if (availableItems[slot] != null)
        {
            if (currentItemIndex == slot)
            {
                SetEmptyHands();
            }
            
            Destroy(availableItems[slot].gameObject);
            availableItems[slot] = null;
        }
    }
    
    /// <summary>
    /// Принудительно переключает на предмет по индексу
    /// </summary>
    public void ForceSwitchToSlot(int slot)
    {
        if (slot >= 0 && slot < availableItems.Length && availableItems[slot] != null)
        {
            SwitchToItem(slot);
        }
    }
    
    /// <summary>
    /// Возвращает предмет из указанного слота
    /// </summary>
    public AbstractItem GetItemInSlot(int slot)
    {
        if (slot >= 0 && slot < availableItems.Length)
            return availableItems[slot];
        return null;
    }
}