using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UB;

[CustomEditor(typeof(HitFX))]
public class HitFXEditor : Editor
{
    private VisualElement _root;
    private VisualTreeAsset _template;
    private StyleSheet _styleSheet;

    private Label _titleLabel;
    private Button _testButton;
    
    private HitFX _hitFx;

    private void OnEnable()
    {
        var uxmlPath = AssetDatabase.GUIDToAssetPath("91aa4d8143dc040918ea91b119f6a5d7");
        _template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

        var ussPath = AssetDatabase.GUIDToAssetPath("07cb7bcf2462144098e7263c8f833764");
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
        _hitFx = (HitFX)target;

        if (!EditorApplication.isPlaying)
            _testButton.SetEnabled(false);
        else 
            _testButton.SetEnabled(true);
        
        _titleLabel.RegisterCallback<ClickEvent>(evt => {
            OpenBehaviour(_hitFx);
        });
        
        _testButton.clicked += () => {
            _hitFx.PlayHit();
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
