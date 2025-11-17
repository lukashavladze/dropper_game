using UnityEngine;

public class MarketManager : MonoBehaviour
{
    public GameObject marketPanel; // assign MarketPanel in Inspector

    // Open market
    public void OpenMarket()
    {
        marketPanel.SetActive(true);
    }

    // Close market
    public void CloseMarket()
    {
        marketPanel.SetActive(false);
    }
}