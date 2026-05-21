using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HidePlaceholderOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public TMP_Text placeholder;

    public void OnSelect(BaseEventData eventData)
    {
        if (placeholder != null)
            placeholder.gameObject.SetActive(false);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        TMP_InputField input = GetComponent<TMP_InputField>();

        if (input != null &&
            string.IsNullOrEmpty(input.text))
        {
            placeholder.gameObject.SetActive(true);
        }
    }
}