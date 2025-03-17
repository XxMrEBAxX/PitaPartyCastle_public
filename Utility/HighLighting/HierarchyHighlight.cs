using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

/// <summary> Sets a background color for game objects in the Hierarchy tab</summary>
[UnityEditor.InitializeOnLoad]
public class HierarchyHighlight
{
    private static Vector2 offset = new Vector2(20, 1);

    static HierarchyHighlight()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {

        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj != null)
        {
            Color backgroundColor = Color.white;
            Color textColor = Color.white;
            Texture2D texture = null;

            // Or you can use switch case
            //switch (obj.name)
            //{
            //    case "Main Camera":
            //        backgroundColor = Color.red;
            //        textColor = new Color(0.9f, 0.9f, 0.9f);
            //        break;
            //}


            if (backgroundColor != Color.white)
            {
                Rect offsetRect = new Rect(selectionRect.position + offset, selectionRect.size);
                Rect bgRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width + 50, selectionRect.height);

                EditorGUI.DrawRect(bgRect, backgroundColor);
                EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = textColor },
                    fontStyle = FontStyle.Bold
                }
                );

                if (texture != null)
                    EditorGUI.DrawPreviewTexture(new Rect(selectionRect.position, new Vector2(selectionRect.height, selectionRect.height)), texture);
            }
        }
    }
}
#endif