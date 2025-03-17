using DG.DemiEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace UB.EDITOR
{
    public class AutoSettingTileMapLayer : Editor
    {
        [InitializeOnLoadMethod]
        private static void Trigger()
        {
            ObjectChangeEvents.changesPublished += ChangesPublished;
        }

        static void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; ++i)
            {
                ObjectChangeKind type = stream.GetEventType(i);
                switch (type)
                {
                    case ObjectChangeKind.CreateGameObjectHierarchy:
                        stream.GetCreateGameObjectHierarchyEvent(i,
                            out CreateGameObjectHierarchyEventArgs createGameObjectHierarchyEvent);

                        GameObject newGameObject =
                            EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId) as GameObject;

                        if (newGameObject != null)
                        {
                            // TileMap
                            var parent = newGameObject.GetComponentInParent<TilemapRenderer>();

                            if (parent)
                            {
                                var me = newGameObject.GetComponent<SpriteRenderer>();

                                if (me)
                                {
                                    me.sortingLayerID = parent.sortingLayerID;
                                    //FIXME - 진솔님의 억울한 소리
                                    //me.sortingOrder = parent.sortingOrder;
                                }
                            }
                            // NormalMap
                            var light2d = newGameObject.GetComponent<Light2D>();

                            if (light2d)
                            {
                                SerializedObject serializedObject = new SerializedObject(light2d);

                                SerializedProperty serializedProperty = serializedObject.FindProperty("m_NormalMapQuality");
                                serializedProperty.SetValue(Light2D.NormalMapQuality.Accurate);

                                serializedProperty = serializedObject.FindProperty("m_NormalMapDistance");
                                serializedProperty.SetValue(3.0f);

                                serializedObject.ApplyModifiedProperties();
                            }
                        }
                        break;
                }
            }
        }

        [InitializeOnLoadMethod]
        private static void LayerChangeDetector()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject != null)
            {
                CheckLayerChange(gameObject);
            }
        }

        private static void CheckLayerChange(GameObject changedGameObject)
        {
            if (changedGameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                if (!changedGameObject.TryGetComponent(out GroundOptions groundOptions))
                {
                    changedGameObject.AddComponent<GroundOptions>();
                    EditorUtility.SetDirty(changedGameObject);
                }
            }
            else
            {
                if (changedGameObject.TryGetComponent(out GroundOptions groundOptions))
                {
                    DestroyImmediate(groundOptions);
                    EditorUtility.SetDirty(changedGameObject);
                }
            }
        }
    }
}