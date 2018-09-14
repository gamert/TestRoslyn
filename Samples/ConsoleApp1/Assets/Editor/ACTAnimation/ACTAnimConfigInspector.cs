using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ACTAnimConfig))]
public class ACTAnimConfigInspector : Editor
{
    string MoveNodeName = "Dummy_foot/Bip001";
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ACTAnimConfig animcfg = target as ACTAnimConfig;
        if (null == animcfg)
            return;
        if (GUILayout.Button("Refresh"))
        {
            Refresh(animcfg);
        }
        if (GUILayout.Button(string.Format("Reset Anim")))
        {
            Animator animator = animcfg.gameObject.GetComponentInChildren<Animator>();
            if (animator == null)
                return;
            animator.SetInteger("Attack", 0);
        }
        MoveNodeName = GUILayout.TextField(MoveNodeName, GUILayout.Width(300));
        for(int i = 0;i<animcfg.keyDatas.Count;i++)
        {
            if (animcfg.keyDatas[i] == null)
                continue;
            if (GUILayout.Button(string.Format("test anim {0} {1}",animcfg.keyDatas[i].AnimclipName, animcfg.keyDatas[i].ParamValue)))
            {
                animcfg.TestAnim(animcfg.keyDatas[i]);
            }

        }
    }

   

    private void Refresh(ACTAnimConfig animcfg)
    {
        int animcounter = animcfg.AnimClips.Count;
        animcfg.keyDatas = new System.Collections.Generic.List<AnimClipKeyData>(animcounter);
        for (int i = 0;i< animcounter; i++)
        {
            if (animcfg.AnimClips[i] == null)
                continue;
            ExportAnim(animcfg.AnimClips[i], animcfg);
            var so = new SerializedObject(animcfg.AnimClips[i]);
            so.FindProperty("m_EditorCurves").arraySize = 0;
            so.FindProperty("m_EulerEditorCurves").arraySize = 0;
            so.ApplyModifiedProperties();           
            EditorUtility.SetDirty(animcfg.AnimClips[i]);
        }
        AssetDatabase.SaveAssets();
    }
    int maxcounter = 0;
    void ExportAnim(AnimationClip animclip, ACTAnimConfig animcfg)
    {
        if (animclip == null)
            return;
        maxcounter = (int)(animclip.length / (1f / 30f)+0.5f) + 1;
        int[] xs = new int[maxcounter];
        int[] ys = new int[maxcounter];
        int[] zs = new int[maxcounter];
        foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(animclip))
        {
            string name = theCurveBinding.propertyName.ToLower();
            //Debug.Log("************** " + theCurveBinding.path + " : " + name);
            if (theCurveBinding.path == MoveNodeName && name.Contains("position"))
            {
                Debug.Log("************** " + theCurveBinding.path + " : " + name);

                AnimationCurve testcurve = AnimationUtility.GetEditorCurve(animclip, theCurveBinding);
                if(name.Contains(".x"))
                {
                    ExportKeyData(ref xs, testcurve);
                }
                else if(name.Contains(".y"))
                {
                    //ExportKeyData(ref ys, testcurve);
                }
                else if(name.Contains(".z"))
                {
                    ExportKeyData(ref zs, testcurve);
                }

                //hasposition = true;    

                if (!name.Contains(".y"))
                {
                    AnimationUtility.SetEditorCurve(animclip, theCurveBinding, null);
                }
            }
        }
        AnimClipKeyData data = new AnimClipKeyData(xs, ys, zs, animclip.name);
        animcfg.keyDatas.Add(data);

       
    }

    void ExportKeyData(ref int[] keydatas, AnimationCurve testcurve)
    {
        int t = 0;
        for (int i = 0; i < testcurve.keys.Length; i++)
        {
            int framecounter = (int)((testcurve.keys[i].time / (1f / 30f)) + 0.5f);
            if(t == 0)
            {
                if (framecounter == 0)
                    keydatas[t++] = (int)(testcurve.keys[i].value * 1000);
                else if (framecounter > 0)
                {
                    while (t < framecounter)
                    {
                        keydatas[t++] = 0;
                    }
                    keydatas[t++] =  (int)(testcurve.keys[i].value * 1000);
                }
            }
            else
            {
                if(framecounter == t)
                {
                    keydatas[t++] = (int)(testcurve.keys[i].value * 1000);
                }
                else if(framecounter  > t)
                {
                    while(t < framecounter)
                    {
                        keydatas[t] = keydatas[t - 1];
                        t++;
                    }
                    keydatas[t++] = (int)(testcurve.keys[i].value * 1000); 
                }
            }
            Debug.Log(name + " : " + framecounter + " : " + testcurve.keys[i].value);
        }
        while(t< maxcounter)
        {
            keydatas[t] = keydatas[t - 1];
            t++;
        }
    }
}