using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // UI refs are set by BindUI; keep them public so initializer can set them
    [Header("UI (assigned at runtime by InventoryUIInitializer)")]
    public GameObject InventoryPanel;
    public InventoryItemButton[] inventorySlots;

    private const string OWNED_KEY = "OwnedSkins";

    public bool resetOnStart = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // manager persists, UI will be rebound
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // === Public API used by UI initializer ===
    public void BindUI(GameObject panel, InventoryItemButton[] slots, Button openButton, Button exitButton)
    {
        InventoryPanel = panel;
        inventorySlots = slots;

        // hook open/exit safely (remove old listeners)
        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OpenInventory);
            Debug.Log("[InventoryManager] Bound Open button.");
        }
        else
            Debug.LogWarning("[InventoryManager] openButton is null when binding.");

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(CloseInventory);
            Debug.Log("[InventoryManager] Bound Exit button.");
        }
        else
            Debug.LogWarning("[InventoryManager] exitButton is null when binding.");

        // Ensure panel is initially closed
        if (InventoryPanel != null) InventoryPanel.SetActive(false);

        // Refresh UI to reflect saved data
        ReloadOwnedItems();
    }

    // Show/hide
    public void OpenInventory()
    {
        if (InventoryPanel == null)
        {
            Debug.LogError("[InventoryManager] OpenInventory: InventoryPanel is null!");
            return;
        }
        InventoryPanel.SetActive(true);
    }
    public void CloseInventory()
    {
        if (InventoryPanel != null)
            InventoryPanel.SetActive(false);
    }

    // Set item into slot AND save mapping to PlayerPrefs
    public void SetItemInSlotAndSave(int slotIndex, Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.LogError("[InventoryManager] SetItemInSlotAndSave: sprite is null");
            return;
        }
        SetItemInSlot(slotIndex, sprite);
        // save mapping: slot_i -> spriteName
        PlayerPrefs.SetString("slot_" + slotIndex, sprite.name);
        PlayerPrefs.Save();
        Debug.Log("[InventoryManager] Saved slot_" + slotIndex + " = " + sprite.name);
    }

    public void SetItemInSlot(int slotIndex, Sprite itemSprite)
    {
        if (inventorySlots == null || slotIndex < 0 || slotIndex >= inventorySlots.Length)
        {
            Debug.LogWarning("[InventoryManager] SetItemInSlot: invalid index or slots not assigned");
            return;
        }

        var slot = inventorySlots[slotIndex];
        if (slot == null)
        {
            Debug.LogWarning("[InventoryManager] SetItemInSlot: slot is null at index " + slotIndex);
            return;
        }

        slot.SetItem(itemSprite, OnSelectItem);
    }

    // Called when user clicks a slot; this sets the chosen skin in your game
    private void OnSelectItem(UnityEngine.Sprite skin)
    {
        SkinSelection.SelectedStoneSkin = skin;
        Debug.Log("[InventoryManager] Selected skin: " + (skin ? skin.name : "null"));
    }

    // Reload saved mapping -> apply sprites to current UI slots
    public void ReloadOwnedItems()
    {
        if (inventorySlots == null)
        {
            Debug.LogWarning("[InventoryManager] ReloadOwnedItems: inventorySlots is null. UI not bound yet.");
            return;
        }

        Debug.Log("[InventoryManager] ReloadOwnedItems: slots = " + inventorySlots.Length);

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            var slot = inventorySlots[i];
            if (slot == null)
            {
                Debug.LogWarning("[InventoryManager] ReloadOwnedItems: slot " + i + " is null.");
                continue;
            }

            string key = "slot_" + i;
            if (!PlayerPrefs.HasKey(key))
            {
                // not purchased/assigned -> lock
                slot.Lock();
                continue;
            }

            string spriteName = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(spriteName))
            {
                slot.Lock();
                continue;
            }

            Sprite loaded = Resources.Load<Sprite>("Skins/" + spriteName);
            if (loaded == null)
            {
                Debug.LogWarning("[InventoryManager] ReloadOwnedItems: sprite not found for " + spriteName);
                slot.Lock();
                continue;
            }

            slot.SetItem(loaded, OnSelectItem);
            slot.Unlock();
        }

        Debug.Log("[InventoryManager] ReloadOwnedItems done.");
    }
}
