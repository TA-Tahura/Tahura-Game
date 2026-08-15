#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Generic editor base class for events with payload data
public abstract class BaseGameEventEditor<T> : Editor
{
    private T testValue;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = Application.isPlaying;

        testValue = DrawTestValueField("Test Value", testValue);

        if (GUILayout.Button("Raise Event"))
        {
            var eventTarget = target as BaseGameEvent<T>;
            if (eventTarget != null)
            {
                eventTarget.Raise(testValue);
            }
        }
    }

    protected abstract T DrawTestValueField(string label, T value);
}
#endif