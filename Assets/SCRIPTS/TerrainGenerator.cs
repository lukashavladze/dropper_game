using UnityEngine;


public class TerrainGenerator : MonoBehaviour
{
    public GameObject platformPrefab;
    public int width = 10;
    public float y = -4f;


    void Start()
    {
        GenerateBase();
    }


    void GenerateBase()
    {
        var p = Instantiate(platformPrefab, new Vector3(0, y, 0), Quaternion.identity);
        p.transform.localScale = new Vector3(width, 1, 1);
        p.tag = "Platform";
    }
}