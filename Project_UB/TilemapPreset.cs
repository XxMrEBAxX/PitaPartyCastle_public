using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace UB
{
    public class TilemapPreset : EditorWindow
    {
        [MenuItem("Tools/ProjectUB/TilemapPreset")]
        private static void ShowExample()
        {
            TilemapPreset wnd = GetWindow<TilemapPreset>();
            wnd.titleContent = new GUIContent("EditorTest");
        }

        private Transform FindLevel()
        {
            GameObject level = GameObject.Find("Level");
            if (level == null)
            {
                level = new GameObject("Level");
            }

            return level.transform;
        }

        private void AddTilemapComponents(GameObject tilemap, string sortingLayer, int sortingOrder = 0)
        {
            tilemap.AddComponent<Tilemap>();
            var tilemapRenderer = tilemap.AddComponent<TilemapRenderer>();
            tilemapRenderer.sortingLayerName = sortingLayer;
            tilemapRenderer.sortingOrder = sortingOrder;
        }

        private void OnClickButton()
        {
            GameObject grid = null;
            GameObject tilemap = null;

            grid = new GameObject("Level Grid (Super Near)");
            grid.AddComponent<Grid>();
            grid.transform.SetParent(FindLevel());
            grid.transform.position = new Vector3(0,0,-2);

            tilemap = new GameObject("Tilemap (Super Near)");
            AddTilemapComponents(tilemap, "Super Near");
            tilemap.transform.SetParent(grid.transform);

            grid = new GameObject("Level Grid (Near)");
            grid.AddComponent<Grid>();
            grid.transform.SetParent(FindLevel());
            grid.transform.position = new Vector3(0, 0, 0);

            tilemap = new GameObject("Tilemap (Near Front)");
            AddTilemapComponents(tilemap, "Default", 1);
            tilemap.transform.SetParent(grid.transform);

            tilemap = new GameObject("Tilemap (Near Tile)");
            AddTilemapComponents(tilemap, "Default");
            tilemap.AddComponent<TilemapCollider2D>().usedByComposite = true;
            tilemap.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            tilemap.AddComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Polygons;
            tilemap.transform.SetParent(grid.transform);

            tilemap = new GameObject("Tilemap (Near Tile)_NotUB");
            AddTilemapComponents(tilemap, "Default");
            tilemap.AddComponent<TilemapCollider2D>().usedByComposite = true;
            tilemap.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            tilemap.AddComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Polygons;
            tilemap.transform.SetParent(grid.transform);

            tilemap = new GameObject("Tilemap (Near Back)");
            AddTilemapComponents(tilemap, "Default", -1);
            tilemap.transform.SetParent(grid.transform);

            tilemap = new GameObject("Tilemap(Near Wall)");
            AddTilemapComponents(tilemap, "Default", -10);
            tilemap.transform.SetParent(grid.transform);

            grid = new GameObject("Level Grid (Middle)");
            grid.AddComponent<Grid>();
            grid.transform.SetParent(FindLevel());
            grid.transform.position = new Vector3(0, 0, 10);

            tilemap = new GameObject("Tilemap (Middle)");
            AddTilemapComponents(tilemap, "Middle");
            tilemap.transform.SetParent(grid.transform);

            grid = new GameObject("Level Grid (Far 1)");
            grid.AddComponent<Grid>();
            grid.transform.SetParent(FindLevel());
            grid.transform.position = new Vector3(0, 0, 12);

            tilemap = new GameObject("Tilemap (Far 1 Back)");
            AddTilemapComponents(tilemap, "Far 1");
            tilemap.transform.SetParent(grid.transform);

            tilemap = new GameObject("Tilemap (Far 1 Wall)");
            AddTilemapComponents(tilemap, "Far 1", -10);
            tilemap.transform.SetParent(grid.transform);

            grid = new GameObject("Level Grid (Far 2)");
            grid.AddComponent<Grid>();
            grid.transform.SetParent(FindLevel());
            grid.transform.position = new Vector3(0, 0, 47);

            tilemap = new GameObject("Tilemap (Far 2)");
            AddTilemapComponents(tilemap, "Far 2");
            tilemap.transform.SetParent(grid.transform);

            grid = new GameObject("Level Grid (Far 3)");
            grid.AddComponent<Grid>();
            grid.transform.SetParent(FindLevel());
            grid.transform.position = new Vector3(0, 0, 60);

            tilemap = new GameObject("Tilemap (Far 3)");
            AddTilemapComponents(tilemap, "Far 3");
            tilemap.transform.SetParent(grid.transform);
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            Button button = new Button();
            button.name = "button";
            button.text = "타일맵 프리셋 만들기";
            button.clicked += OnClickButton;
            root.Add(button);
        }
    }
}