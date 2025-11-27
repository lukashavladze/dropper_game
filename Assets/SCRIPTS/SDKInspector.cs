#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

public class SDKInspector : EditorWindow
{
    [MenuItem("Tools/SDK Inspector")]
    public static void ShowWindow()
    {
        GetWindow<SDKInspector>("SDK Inspector").Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Run LevelPlay/IronSource/Mediation Check"))
        {
            RunCheck();
        }
    }

    static void RunCheck()
    {
        Debug.Log("=== SDK Inspector started ===");

        // 1) Assemblies and types
        var keywords = new[] { "LevelPlay", "LevelPlayAds", "LevelPlay.Ads", "IronSource", "RewardedAd", "Unity.Services.LevelPlay", "Unity.Services.Mediation", "Mediation" };
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.FullName.StartsWith("Unity") || a.FullName.Contains("Editor"))
            .OrderBy(a => a.GetName().Name)
            .ToArray();

        Debug.Log($"Assemblies scanned: {assemblies.Length}");
        foreach (var kw in keywords)
        {
            var matches = new List<string>();
            foreach (var asm in assemblies)
            {
                try
                {
                    var types = asm.GetTypes();
                    foreach (var t in types)
                    {
                        if (t.FullName != null && t.FullName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                            matches.Add($"{t.FullName} (asm: {asm.GetName().Name})");
                    }
                }
                catch { /* ignore reflection issues */ }
            }
            Debug.Log($"Keyword '{kw}' - matches found: {matches.Count}");
            if (matches.Count > 0)
            {
                foreach (var m in matches.Take(30))
                    Debug.Log("  " + m);
                if (matches.Count > 30) Debug.Log($"  ...and {matches.Count - 30} more");
            }
        }

        // 2) List .aar files in Plugins/Android
        var pluginsAndroid = Path.Combine(Application.dataPath, "Plugins/Android");
        Debug.Log($"Plugins/Android folder: {pluginsAndroid}");
        if (Directory.Exists(pluginsAndroid))
        {
            var aars = Directory.GetFiles(pluginsAndroid, "*.aar", SearchOption.AllDirectories);
            Debug.Log($".aar files found: {aars.Length}");
            foreach (var f in aars) Debug.Log("  " + f.Replace(Application.dataPath, "Assets"));
        }
        else
        {
            Debug.Log("Plugins/Android folder not found.");
        }

        // 3) List assets folders named IronSource / LevelPlay / Mediation
        string[] checkFolders = { "Assets/IronSource", "Assets/LevelPlay", "Assets/LevelPlaySDK", "Assets/Plugins/LevelPlay", "Assets/Plugins/IronSource" };
        foreach (var folder in checkFolders)
            Debug.Log($"{folder} exists: {Directory.Exists(Path.Combine(Application.dataPath, folder.Substring("Assets/".Length)))}");

        // 4) Check Package Manager installed packages (Editor-only)
#if UNITY_2020_1_OR_NEWER
        try
        {
            var listRequest = UnityEditor.PackageManager.Client.List(true, true);
            while (!listRequest.IsCompleted) { System.Threading.Thread.Sleep(50); }
            if (listRequest.Status == UnityEditor.PackageManager.StatusCode.Success)
            {
                Debug.Log("Packages installed (Package Manager):");
                foreach (var pkg in listRequest.Result)
                    Debug.Log($"  {pkg.name} @ {pkg.version}");
            }
            else
            {
                Debug.Log("PackageManager list request failed: " + listRequest.Error.message);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("PackageManager check failed: " + ex.Message);
        }
#else
        Debug.Log("Package Manager check unavailable for this Unity version.");
#endif

        Debug.Log("=== SDK Inspector finished ===");
        EditorUtility.DisplayDialog("SDK Inspector", "Check console for results (Window → General → Console). Paste output here.", "OK");
    }
}
#endif
