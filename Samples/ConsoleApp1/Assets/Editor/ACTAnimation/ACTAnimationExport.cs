using UnityEditor;
using UnityEngine;

public class ACTAnimationEditor : EditorWindow
{
    public static ACTAnimationEditor agsAniEditorWindow;

    [MenuItem("Tools/ACTAnimation")]
    static void ShowWindow()
    {
        agsAniEditorWindow = GetWindow<ACTAnimationEditor>();
        agsAniEditorWindow.Show();
    }
    AnimationClip SampleObj;
    string MoveNodeName = "Bip001";
    void OnGUI()
    {
        SampleObj = (AnimationClip)(EditorGUILayout.ObjectField(new GUIContent("SampleObj"), SampleObj, typeof(AnimationClip), true, GUILayout.Width(position.width - 40)));

        MoveNodeName = EditorGUILayout.TextField(MoveNodeName, GUILayout.Width(position.width - 40));
        if (SampleObj != null)
        {
            if (GUILayout.Button(new GUIContent("ExportAnimClip"), GUILayout.Width(100)))
            {
                ExportAnim(SampleObj);
            }
        }
    }

    void ExportAnim(AnimationClip animclip)
    {
        if (animclip == null)
            return;
        foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(animclip))
        {
            string name = theCurveBinding.propertyName.ToLower();
            //Debug.Log("************** " + theCurveBinding.path + " : " + name);

            if (theCurveBinding.path == MoveNodeName && name.Contains("position"))
            {
                Debug.Log("************** " + theCurveBinding.path + " : " + name);
                AnimationCurve testcurve = AnimationUtility.GetEditorCurve(animclip, theCurveBinding);
                for(int i = 0;i<testcurve.keys.Length;i++)
                {
                    Debug.Log(name + " : " + testcurve.keys[i].time / (1f / 30f) + " : " + testcurve.keys[i].value); 
                }
                

                //hasposition = true;    

                //remove raw curve
                //AnimationUtility.SetEditorCurve(animclip, theCurveBinding, null);
            }
        }
    }
}