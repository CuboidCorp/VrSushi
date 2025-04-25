using UnityEngine;
using UnityEditor;
using System.IO;

public class IngredientSnapshotGenerator : EditorWindow
{
    private Camera renderCam;
    private string prefabsPath = "Assets/Ingredients/Prefabs";
    private string outputPath = "Assets/Ingredients/Icons";
    private Vector2 textureSize = new Vector2(512, 512);

    [MenuItem("Tools/Generate Ingredient Icons")]
    public static void ShowWindow()
    {
        GetWindow<IngredientSnapshotGenerator>("Ingredient Snapshot Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        renderCam = EditorGUILayout.ObjectField("Render Camera", renderCam, typeof(Camera), true) as Camera;
        prefabsPath = EditorGUILayout.TextField("Prefabs Folder", prefabsPath);
        outputPath = EditorGUILayout.TextField("Output Folder", outputPath);
        textureSize = EditorGUILayout.Vector2Field("Image Size", textureSize);

        if (GUILayout.Button("Generate Icons"))
        {
            GenerateIcons();
        }
    }

    private void GenerateIcons()
    {
        if (renderCam == null)
        {
            Debug.LogError("Render Camera is not assigned.");
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Model", new[] { prefabsPath });
        Debug.Log($"Found {prefabGuids.Length} prefabs in {prefabsPath}");
        Directory.CreateDirectory(outputPath);

        RenderTexture rt = new RenderTexture((int)textureSize.x, (int)textureSize.y, 24);
        renderCam.targetTexture = rt;
        Texture2D screenShot = new Texture2D((int)textureSize.x, (int)textureSize.y, TextureFormat.RGBA32, false);

        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.Euler(-120, 0, 0); // Adjust as needed

            renderCam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            screenShot.Apply();

            byte[] bytes = screenShot.EncodeToPNG();
            string fileName = Path.Combine(outputPath, prefab.name + ".png");
            File.WriteAllBytes(fileName, bytes);
            Debug.Log($"Saved icon: {fileName}");

            DestroyImmediate(instance);
        }

        renderCam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        AssetDatabase.Refresh();
        Debug.Log("All icons generated!");
    }
}
