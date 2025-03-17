using UB.EFFECT;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(InvincibleFX))]
public class InvincibleFXEditor : Editor
{
    private VisualElement _root;
    private VisualTreeAsset _template;
    private StyleSheet _styleSheet;

    private Label _titleLabel;
    private Button _testButton;
    
    private InvincibleFX _invincibleFX;

    private void OnEnable()
    {
        var uxmlPath = AssetDatabase.GUIDToAssetPath("351cf2cb8e481bf479604d4f139bd570");
        _template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

        var ussPath = AssetDatabase.GUIDToAssetPath("26287c7f2ab05ae42aec1a47e3e90a73");
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
        _invincibleFX = target as InvincibleFX;

        if (!EditorApplication.isPlaying)
            _testButton.SetEnabled(false);
        else 
            _testButton.SetEnabled(true);
        
        _titleLabel.RegisterCallback<ClickEvent>(evt => {
            OpenBehaviour(_invincibleFX);
        });
        
        _testButton.clicked += () => {
            _invincibleFX.PlayFX();
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
