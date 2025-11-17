using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory UI")]
    public GameObject InventoryPanel;
    public InventoryItemButton[] inventorySlots; // assign buttons in inspector

    private Sprite equippedSkin; // currently selected skin

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeInventory();
    }

    void Start()
    {
        InventoryPanel.SetActive(true);
        // Awake has run for all buttons
        InventoryPanel.SetActive(false);
    }

    private void InitializeInventory()
    {
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogWarning("Inventory slots not assigned in inspector!");
            return;
        }

        // Optional: sanity check buttons
        foreach (var slot in inventorySlots)
        {
            if (slot == null)
            {
                Debug.LogWarning("One of the inventory slots is null!");
                continue;
            }
            if (slot.GetComponent<Button>() == null)
            {
                Debug.LogError("Button missing on inventory slot: " + slot.name);
            }
        }
    }

    // Add a sprite to a specific slot safely
    public void SetItemInSlot(int slotIndex, Sprite itemSprite)
    {
        if (inventorySlots == null || slotIndex < 0 || slotIndex >= inventorySlots.Length)
        {
            Debug.LogWarning("Invalid inventory slot index or slots not assigned!");
            return;
        }

        var slot = inventorySlots[slotIndex];

        if (slot == null || slot.GetComponent<Button>() == null)
        {
            // Slot not ready, wait one frame
            StartCoroutine(DelayedSetItem(slotIndex, itemSprite));
            return;
        }

        slot.SetItem(itemSprite, OnSelectItem);
    }

    private IEnumerator DelayedSetItem(int slotIndex, Sprite itemSprite)
    {
        yield return null; // wait one frame for Awake() to run
        SetItemInSlot(slotIndex, itemSprite);
    }

    // When inventory button clicked
    private void OnSelectItem(Sprite skin)
    {
        SkinSelection.SelectedStoneSkin = skin; // store selection
        Debug.Log("Selected stone skin: " + skin.name);
    }

    // Open/close inventory panel
    public void OpenInventory() => InventoryPanel.SetActive(true);
    public void CloseInventory() => InventoryPanel.SetActive(false);

    // Get equipped skin
    public Sprite GetEquippedSkin() => equippedSkin;

    // Equip new skin programmatically
    public void EquipDropperSkin(Sprite newSkin)
    {
        PlayerPrefs.SetString("EquippedStoneSkin", newSkin.name);
        if (DropperController.Instance != null)
            DropperController.Instance.SetStoneSkin(newSkin);
    }
}