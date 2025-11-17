using UnityEngine;
using UnityEngine.UI;

public class MarketItem : MonoBehaviour
{
    public Sprite itemSprite;
    public int price = 10;

    public Button iconButton;
    public Button buyButton;
    public Button cancelButton;

    public Image itemSpriteImage;

    public int inventorySlotIndex;

    private bool selected = false;

    // static reference to previously selected item
    private static MarketItem lastSelectedItem;

    public Vector2 iconSize = new Vector2(100, 100);

    void Start()
    {
        // Remove DeleteAll in production
        //PlayerPrefs.DeleteAll(); 

        buyButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);

        iconButton.onClick.AddListener(OnSelect);
        buyButton.onClick.AddListener(OnBuy);
        cancelButton.onClick.AddListener(OnCancel);

        // Already purchased?
        if (IsPurchased())
        {
            MarkAsPurchased();
        }

        // Set icon image
        iconButton.image.sprite = itemSprite;
        iconButton.image.rectTransform.sizeDelta = iconSize;
    }

    void OnSelect()
    {
        // hide buttons of previous selection
        if (lastSelectedItem != null && lastSelectedItem != this)
        {
            lastSelectedItem.HideSelectionButtons();
        }

        lastSelectedItem = this;

        if (IsPurchased()) return;

        selected = true;

        // Highlight
        iconButton.transform.localScale = Vector3.one * 1.1f;
        buyButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);

        Debug.Log("[DEBUG] Selected item: " + itemSprite.name);
    }

    void OnBuy()
    {
        if (CurrencyManager.Instance.CanAfford(price))
        {
            CurrencyManager.Instance.SpendCoins(price);

            // Save purchase permanence (optional extra key)
            PlayerPrefs.SetInt(itemSprite.name + "_Purchased", 1);
            PlayerPrefs.Save();

            // Assign to inventory and save mapping
            InventoryManager.Instance.SetItemInSlotAndSave(inventorySlotIndex, itemSprite);

            Debug.Log("[MarketItem] Purchased and saved " + itemSprite.name);

            MarkAsPurchased();
        }
        else
        {
            Debug.Log("[MarketItem] Not enough coins");
        }
        HideSelectionButtons();
    }

    void OnCancel()
    {
        Debug.Log("[DEBUG] Cancelled purchase: " + itemSprite.name);
        HideSelectionButtons();
    }

    void HideSelectionButtons()
    {
        selected = false;
        iconButton.transform.localScale = Vector3.one;
        buyButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);

        if (lastSelectedItem == this)
            lastSelectedItem = null;
    }

    bool IsPurchased()
    {
        return PlayerPrefs.GetInt(itemSprite.name + "_Purchased", 0) == 1;
    }

    void MarkAsPurchased()
    {
        iconButton.interactable = false;

        if (iconButton.image != null)
            iconButton.image.color = Color.gray;

        if (itemSpriteImage != null)
            itemSpriteImage.color = Color.gray;

        HideSelectionButtons();
    }
}
