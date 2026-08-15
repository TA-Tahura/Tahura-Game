#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(BoolEvent))]
public class BoolEventEditor : BaseGameEventEditor<bool>
{
    protected override bool DrawTestValueField(string label, bool value)
    {
        return EditorGUILayout.Toggle(label, value);
    }
}
#endif