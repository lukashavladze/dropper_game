using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CreateObj : MonoBehaviour
{
    public Image PP;
    public Sprite[] temp;
    public GameObject parent;
    public Color[] colorList;
    public int colorNo;

    void Start()
    {
        for (int i = 0; i < temp.Length; i++)
        {
            var createImage = Instantiate(PP) as Image;
            createImage.transform.SetParent(parent.transform, false);
            createImage.color = colorList[colorNo];
            createImage.gameObject.SetActive(true);

            Image tempNew = createImage.GetComponentsInChildren<Image>()[1];
            tempNew.sprite = temp[i];
            createImage.gameObject.name = temp[i].name;

            // EXAMPLE: If you want to generate prefabs again, uncomment:
            //
            // string localPath = $"Assets/UI button pack 3/Button round color {colorNo + 1}/{temp[i].name}.prefab";
            // CreateNew(createImage.gameObject, localPath);
        }
    }

#if UNITY_EDITOR
    static void CreateNew(GameObject obj, string localPath)
    {
        // Ensure the folder exists
        string folder = System.IO.Path.GetDirectoryName(localPath);
        if (!AssetDatabase.IsValidFolder(folder))
        {
            System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        // Save prefab with new API
        PrefabUtility.SaveAsPrefabAssetAndConnect(obj, localPath, InteractionMode.UserAction);

        Debug.Log($"Prefab created at: {localPath}");
    }
#endif
}
