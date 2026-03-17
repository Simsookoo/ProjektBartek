using UnityEngine;
using UnityEditor;

public static class AnimationConstantTangents
{
    [MenuItem("Tools/Animation/Set Both Tangents Constant For Selected Clip")]
    public static void SetConstantTangentsForSelectedClip()
    {
        var clip = Selection.activeObject as AnimationClip;
        if (clip == null)
        {
            Debug.LogWarning("Zaznacz AnimationClip w Project window.");
            return;
        }

        Undo.RecordObject(clip, "Set Constant Tangents");

        var bindings = AnimationUtility.GetCurveBindings(clip);

        foreach (var binding in bindings)
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null || curve.length == 0)
                continue;

            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyBroken(curve, i, true);
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
            }

            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();

        Debug.Log($"Ustawiono Constant tangents dla clipa: {clip.name}");
    }
}