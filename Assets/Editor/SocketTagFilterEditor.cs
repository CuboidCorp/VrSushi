using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SocketTagFilter))]
public class SocketTagFilterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SocketTagFilter filter = (SocketTagFilter)target;

        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

        int selectedIndex = Mathf.Max(0, System.Array.IndexOf(tags, filter.requiredTag));

        selectedIndex = EditorGUILayout.Popup("Required Tag", selectedIndex, tags);
        filter.requiredTag = tags[selectedIndex];

        if (GUI.changed)
        {
            EditorUtility.SetDirty(filter);
        }
    }
}
