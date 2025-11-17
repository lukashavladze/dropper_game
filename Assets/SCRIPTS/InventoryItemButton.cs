using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryItemButton : MonoBehaviour
{
    public Image iconImage; // assign in prefab inspector
    public string skinName; // optional label if you want

    private Button button;
    private Sprite currentSprite;
    private Action<Sprite> onClickCallback;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null) Debug.LogError("NO BUTTON ON " + gameObject.name);

        if (iconImage == null) Debug.LogError("NO ICON IMAGE ON " + gameObject.name);
    }


    public void SetItem(Sprite sprite, Action<Sprite> clickCallback)
    {
        if (iconImage == null || button == null)
        {
            Debug.LogWarning("[InventoryItemButton] SetItem failed: missing components on " + gameObject.name);
            return;
        }

        currentSprite = sprite;
        iconImage.sprite = sprite;
        iconImage.color = Color.white;

        onClickCallback = clickCallback;

        button.onClick.RemoveAllListeners();
        if (clickCallback != null)
            button.onClick.AddListener(() => clickCallback(currentSprite));
    }

    public void Lock()
    {
        if (iconImage != null)
            iconImage.color = new Color(1, 1, 1, 0.3f);
        if (button != null)
            button.interactable = false;
    }

    public void Unlock()
    {
        if (iconImage != null)
            iconImage.color = Color.white;
        if (button != null)
            button.interactable = true;
    }
}
