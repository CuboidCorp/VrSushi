using UnityEngine;
using UnityEditor;
using System.IO;

public class IngredientSnapshotGenerator : EditorWindow
{
    private Camera renderCam;
    private string modelsPath = "Assets/Prefab/Ingredients";
    private string outputPath = "Assets/Icons";
    private Vector2 textureSize = new Vector2(512, 512);
    private Color backgroundColor = new Color(0, 0, 0, 0); // Transparent

    private Vector3 defaultRotation = new Vector3(0, 180, 0);
    private Vector3 defaultPositionOffset = Vector3.zero;
    private float zoomMultiplier = 1.2f;

    // Manual mode
    private GameObject selectedModel;
    private GameObject manualInstance;

    [MenuItem("Tools/Generate Ingredient Icons")]
    public static void ShowWindow()
    {
        GetWindow<IngredientSnapshotGenerator>("Ingredient Snapshot Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Settings", EditorStyles.boldLabel);

        renderCam = EditorGUILayout.ObjectField("Render Camera", renderCam, typeof(Camera), true) as Camera;
        modelsPath = EditorGUILayout.TextField("Models Folder", modelsPath);
        outputPath = EditorGUILayout.TextField("Output Folder", outputPath);
        textureSize = EditorGUILayout.Vector2Field("Image Size", textureSize);

        GUILayout.Space(10);
        GUILayout.Label("Default Transform Settings", EditorStyles.boldLabel);
        defaultRotation = EditorGUILayout.Vector3Field("Rotation", defaultRotation);
        defaultPositionOffset = EditorGUILayout.Vector3Field("Position Offset", defaultPositionOffset);
        zoomMultiplier = EditorGUILayout.FloatField("Zoom Multiplier", zoomMultiplier);

        if (GUILayout.Button("Generate All Icons (Batch Mode)"))
        {
            GenerateIcons();
        }

        GUILayout.Space(20);
        GUILayout.Label("Manual Mode", EditorStyles.boldLabel);

        selectedModel = EditorGUILayout.ObjectField("Select Model", selectedModel, typeof(GameObject), false) as GameObject;

        if (GUILayout.Button("Instantiate Selected Model"))
        {
            InstantiateManualModel();
        }

        if (manualInstance != null)
        {
            if (GUILayout.Button("Capture Screenshot (Manual)"))
            {
                CaptureManualScreenshot();
            }
        }
    }

    private void InstantiateManualModel()
    {
        if (manualInstance != null)
            DestroyImmediate(manualInstance);

        if (selectedModel != null)
        {
            manualInstance = Instantiate(selectedModel);
            manualInstance.transform.position = Vector3.zero;
            manualInstance.transform.rotation = Quaternion.Euler(defaultRotation);

            Bounds bounds = CalculateBounds(manualInstance);
            CenterModel(manualInstance, bounds);
            manualInstance.transform.position += defaultPositionOffset;
        }
    }

    private void CaptureManualScreenshot()
    {
        if (renderCam == null)
        {
            Debug.LogError("Render Camera not assigned.");
            return;
        }

        Directory.CreateDirectory(outputPath);

        RenderTexture rt = new RenderTexture((int)textureSize.x, (int)textureSize.y, 24, RenderTextureFormat.ARGB32);
        renderCam.targetTexture = rt;
        renderCam.clearFlags = CameraClearFlags.SolidColor;
        renderCam.backgroundColor = backgroundColor;

        Texture2D screenShot = new Texture2D((int)textureSize.x, (int)textureSize.y, TextureFormat.RGBA32, false);

        renderCam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        screenShot.Apply();

        string fileName = Path.Combine(outputPath, (selectedModel != null ? selectedModel.name : "ManualCapture") + ".png");
        File.WriteAllBytes(fileName, screenShot.EncodeToPNG());
        Debug.Log($"Screenshot saved: {fileName}");

        renderCam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        AssetDatabase.Refresh();
    }

    private void GenerateIcons()
    {
        if (renderCam == null)
        {
            Debug.LogError("Render Camera is not assigned.");
            return;
        }

        string[] modelGuids = AssetDatabase.FindAssets("t:Prefab", new[] { modelsPath });
        Directory.CreateDirectory(outputPath);

        RenderTexture rt = new RenderTexture((int)textureSize.x, (int)textureSize.y, 24, RenderTextureFormat.ARGB32);
        renderCam.targetTexture = rt;
        renderCam.clearFlags = CameraClearFlags.SolidColor;
        renderCam.backgroundColor = backgroundColor;

        Texture2D screenShot = new Texture2D((int)textureSize.x, (int)textureSize.y, TextureFormat.RGBA32, false);

        foreach (string guid in modelGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            GameObject instance = Instantiate(model, Vector3.zero, Quaternion.identity);
            instance.transform.rotation = Quaternion.Euler(defaultRotation);

            Bounds bounds = CalculateBounds(instance);
            CenterModel(instance, bounds);
            instance.transform.position += defaultPositionOffset;

            AdjustCamera(renderCam, bounds);

            renderCam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            screenShot.Apply();

            byte[] bytes = screenShot.EncodeToPNG();
            string fileName = Path.Combine(outputPath, model.name + ".png");
            File.WriteAllBytes(fileName, bytes);
            Debug.Log($"Saved icon: {fileName}");

            DestroyImmediate(instance);
        }

        renderCam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        AssetDatabase.Refresh();
        Debug.Log("All batch icons generated!");
    }

    private Bounds CalculateBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(go.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    private void CenterModel(GameObject go, Bounds bounds)
    {
        Vector3 offset = bounds.center;
        foreach (Transform child in go.transform)
        {
            child.position -= offset;
        }
    }

    private void AdjustCamera(Camera cam, Bounds bounds)
    {
        float maxSize = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);

        if (cam.orthographic)
        {
            cam.orthographicSize = maxSize * zoomMultiplier;
            cam.transform.position = new Vector3(0, 0, -5f);
            cam.transform.rotation = Quaternion.identity;
        }
        else
        {
            float distance = maxSize / Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView * 0.5f);
            cam.transform.position = new Vector3(0, 0, -distance * zoomMultiplier);
            cam.transform.LookAt(Vector3.zero);
        }
    }
}
