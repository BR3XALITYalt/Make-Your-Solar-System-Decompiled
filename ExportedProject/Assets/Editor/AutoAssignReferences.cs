using UnityEngine;
using UnityEditor;
using System.Reflection;

public class AutoAssignReferences : EditorWindow
{
    [MenuItem("Tools/Auto Assign References")]
    static void Assign()
    {
        // Find the GameController in the scene
        GameController gc = FindObjectOfType<GameController>();
        if (gc == null)
        {
            Debug.LogError("No GameController found in scene.");
            return;
        }

        // Get all public fields of type GameObject in GameController
        FieldInfo[] fields = typeof(GameController).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(GameObject))
            {
                // Try to find a GameObject with the same name as the field
                GameObject go = GameObject.Find(field.Name);
                if (go != null)
                {
                    field.SetValue(gc, go);
                    Debug.Log($"Assigned {field.Name} -> {go.name}");
                }
                else
                {
                    Debug.LogWarning($"Could not find GameObject named '{field.Name}'");
                }
            }
        }

        // Optionally, do the same for CameraControl
        CameraControl camCtrl = FindObjectOfType<CameraControl>();
        if (camCtrl != null)
        {
            FieldInfo[] camFields = typeof(CameraControl).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in camFields)
            {
                if (field.FieldType == typeof(GameObject) || field.FieldType == typeof(Transform))
                {
                    // For Transform fields, search by name and assign transform
                    if (field.FieldType == typeof(Transform))
                    {
                        Transform t = GameObject.Find(field.Name)?.transform;
                        if (t != null) field.SetValue(camCtrl, t);
                    }
                    else
                    {
                        GameObject go = GameObject.Find(field.Name);
                        if (go != null) field.SetValue(camCtrl, go);
                    }
                }
            }
        }

        // Mark the objects as dirty so changes are saved
        EditorUtility.SetDirty(gc);
        if (camCtrl) EditorUtility.SetDirty(camCtrl);
        Debug.Log("Auto-assign complete. Check for warnings.");
    }
}