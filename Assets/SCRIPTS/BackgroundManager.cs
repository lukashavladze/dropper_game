using UnityEngine;


public class BackgroundManager : MonoBehaviour
{
    public SpriteRenderer bgRenderer; // full-screen sprite
    public Sprite[] backgrounds; // assign ordered: underwater, earth, air, space
    public int levelsPerTheme = 5;


    public void UpdateTheme(int placedCount)
    {
        if (backgrounds == null || backgrounds.Length == 0) return;

        // Loop through backgrounds or clamp to last
        int index = Mathf.Min(placedCount / 3, backgrounds.Length - 1);
        bgRenderer.sprite = backgrounds[index];
    }

}