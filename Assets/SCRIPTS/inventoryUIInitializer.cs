using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class InventoryUIInitializer : MonoBehaviour
{
    [Header("Assign these in the Menu scene inspector (preferred)")]
    public GameObject inventoryPanel;            // drag the panel GameObject
    public Button inventoryOpenButton;           // drag the open button GameObject (Button component)
    public Button inventoryExitButton;           // drag exit/close button
    public Transform slotParent;                 // optional parent that contains slot buttons

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // If menu already active at start, bind now
        if (SceneManager.GetActiveScene().name == "menu")
            StartCoroutine(BindWhenReady());
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        if (s.name == "menu")
            StartCoroutine(BindWhenReady());
    }

    private IEnumerator BindWhenReady()
    {
        // Wait one frame to let UI be created
        yield return null;

        // Ensure InventoryManager exists
        while (InventoryManager.Instance == null)
            yield return null;

        // --- gather slot buttons ---
        InventoryItemButton[] slotButtons = null;
        if (slotParent != null)
        {
            slotButtons = slotParent.GetComponentsInChildren<InventoryItemButton>(true);
        }
        else
        {
            // fallback: find all visible slot components in scene
            slotButtons = UnityEngine.Object.FindObjectsByType<InventoryItemButton>(
    FindObjectsSortMode.None
);
        }

        // If inspector fields are null, try to find them by name (best effort)
        if (inventoryPanel == null)
            inventoryPanel = GameObject.Find("InventoryPanel");

        if (inventoryOpenButton == null)
        {
            var openObj = GameObject.Find("InventoryOpenButton");
            if (openObj != null) inventoryOpenButton = openObj.GetComponent<Button>();
        }

        if (inventoryExitButton == null)
        {
            var exitObj = GameObject.Find("InventoryExitButton");
            if (exitObj != null) inventoryExitButton = exitObj.GetComponent<Button>();
        }

        // final checks
        if (inventoryPanel == null)
            Debug.LogError("[InventoryUIInitializer] inventoryPanel is still null. Assign it in inspector.");

        if (slotButtons == null || slotButtons.Length == 0)
            Debug.LogWarning("[InventoryUIInitializer] No slot buttons found. Ensure InventoryItemButton components exist under slotParent or scene.");

        if (InventoryManager.Instance != null)
        {
            // bind UI (this will also call ReloadOwnedItems)
            InventoryManager.Instance.BindUI(inventoryPanel, slotButtons, inventoryOpenButton, inventoryExitButton);
            Debug.Log("[InventoryUIInitializer] Bound UI to InventoryManager (slots=" + (slotButtons?.Length ?? 0) + ")");
        }
        else
        {
            Debug.LogError("[InventoryUIInitializer] InventoryManager.Instance not found when binding.");
        }
    }
}
