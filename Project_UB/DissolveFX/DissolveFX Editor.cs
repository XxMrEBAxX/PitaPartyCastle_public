using UB.EFFECT;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(DissolveFX))]
public class DissolveFXEditor : Editor
{
    private VisualElement _root;
    private VisualTreeAsset _template;
    private StyleSheet _styleSheet;

    private Label _titleLabel;
    private Button _testButton;
    
    private DissolveFX _DissolveFX;

    private void OnEnable()
    {
        var uxmlPath = AssetDatabase.GUIDToAssetPath("0c79371b918abfa4c80aa7caa00903ea");
        _template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

        var ussPath = AssetDatabase.GUIDToAssetPath("fdecb37dfc129e049aff2ad8f5952753");
        _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
    }

    private void Init()
    {
        _root = new VisualElement();
        _template.CloneTree(_root);
        _root.styleSheets.Add(_styleSheet);

        _titleLabel = _root.Q<Label>("title-text");
        _testButton = _root.Q<Button>("hit-button");
    }

    public override VisualElement CreateInspectorGUI()
    {
        Init();
        _DissolveFX = target as DissolveFX;

        if (!EditorApplication.isPlaying)
            _testButton.SetEnabled(false);
        else 
            _testButton.SetEnabled(true);
        
        _titleLabel.RegisterCallback<ClickEvent>(evt => {
            OpenBehaviour(_DissolveFX);
        });
        
        _testButton.clicked += () => {
            _DissolveFX.PlayHit();
        };

        return _root;
    }
    
    private static void OpenBehaviour(MonoBehaviour targetBehaviour)
    {
        var scriptAsset = MonoScript.FromMonoBehaviour(targetBehaviour);
        var path = AssetDatabase.GetAssetPath(scriptAsset);

        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        AssetDatabase.OpenAsset(textAsset);
    }
}
