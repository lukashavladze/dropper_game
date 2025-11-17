using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryItemButton : MonoBehaviour
{
    public Image iconImage; // assign in inspector
    private Button button;
    private Sprite currentSprite;
    private Action<Sprite> onClickCallback;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null) Debug.LogError("Button component missing on " + gameObject.name);
        if (iconImage == null) Debug.LogError("iconImage not assigned on " + gameObject.name);
    }

    void Start()
    {
        if (button != null)
            button.onClick.AddListener(() => Debug.Log("Button clicked: " + gameObject.name));
    }

    public void SetItem(Sprite sprite, Action<Sprite> clickCallback)
    {
        if (iconImage == null)
        {
            Debug.LogError("iconImage not assigned on " + gameObject.name);
            return;
        }

        if (button == null)
        {
            Debug.LogError("Button component missing on " + gameObject.name);
            return;
        }

        currentSprite = sprite;
        iconImage.sprite = sprite;
        iconImage.color = Color.white; // make visible

        onClickCallback = clickCallback;

        button.onClick.RemoveAllListeners();
        if (clickCallback != null)
            button.onClick.AddListener(() => clickCallback(currentSprite));
    }
}