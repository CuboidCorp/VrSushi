using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraPathRecorder))]
public class CameraPathRecorderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CameraPathRecorder recorder = (CameraPathRecorder)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Play/Stop (Space)"))
        {
            if (Application.isPlaying)
            {
                if (recorder.GetType().GetField("isPlaying", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(recorder).Equals(true))
                    recorder.StopPlayback();
                else
                    recorder.StartPlayback();
            }
        }

        if (GUILayout.Button("Set camera to first pos"))
        {
            recorder.SetCameraToFirstPos();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Controls:\n• SPACE: Play/Stop preview\n• R: Start recording\n• S: Stop recording and save", MessageType.Info);

        if (recorder.pathPoints == null || recorder.pathPoints.Length < 2)
        {
            EditorGUILayout.HelpBox("Add at least 2 transforms to the Path Points array to create a camera path.", MessageType.Warning);
        }
    }
}