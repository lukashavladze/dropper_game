using UnityEngine;
using UnityEngine.UI;

public class MarketItem : MonoBehaviour
{
    public Sprite itemSprite;
    public int price = 10;

    //public Text priceText;

    public Button iconButton;
    public Button buyButton;
    public Button cancelButton;

    public Image itemSpriteImage;

    public int inventorySlotIndex;

    private bool selected = false;


    public Vector2 iconSize = new Vector2(100, 100);
    void Start()
    {
        // for testing
        PlayerPrefs.DeleteAll(); // need to delete


        //priceText.text = price.ToString();

        buyButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);

        iconButton.onClick.AddListener(OnSelect);
        buyButton.onClick.AddListener(OnBuy);
        cancelButton.onClick.AddListener(OnCancel);

        // Already bought?
        if (IsPurchased())
        {
            MarkAsPurchased();
        }

        // Set the button image to the item sprite
        iconButton.image.sprite = itemSprite;
        // Force consistent size
        iconButton.image.rectTransform.sizeDelta = iconSize;
        //iconButton.image.SetNativeSize();
    }

    void OnSelect()
    {
        if (IsPurchased()) return;

        selected = true;

        // Highlight by scaling up
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

            //PlayerPrefs.SetInt(itemSprite.name + "_Purchased", 1);
            //PlayerPrefs.Save();

            Debug.Log("[DEBUG] Purchased " + itemSprite.name);

            InventoryManager.Instance.SetItemInSlot(inventorySlotIndex, itemSprite);

            MarkAsPurchased();
            // Gray out the market button
            iconButton.interactable = false;
            if (itemSpriteImage != null) itemSpriteImage.color = Color.gray;
        }

    
        else
        {
            Debug.Log("[DEBUG] Not enough coins to buy " + itemSprite.name);
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

        // Reset scale when deselected
        iconButton.transform.localScale = Vector3.one;

        buyButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }

    bool IsPurchased()
    {
        return PlayerPrefs.GetInt(itemSprite.name + "_Purchased", 0) == 1;
    }

    void MarkAsPurchased()
    {
        iconButton.interactable = false;

        // Gray out the icon image (the child that shows the item)
        if (iconButton.image != null)
        {
            iconButton.image.color = Color.gray; // optional, for background
        }

        if (itemSpriteImage != null) // this should be the Image showing the item sprite
        {
            itemSpriteImage.color = Color.gray;
        }

        HideSelectionButtons();
    }
}
