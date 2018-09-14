using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[System.Serializable]
public class AnimClipKeyData
{
    public Vector3[] KeyRootPos;
    public string AnimclipName;
    public int ParamValue;
    public int FrameCounter
    {
        get { return KeyRootPos.Length; }
    }

    public Vector3 EndPos
    {
        get { return KeyRootPos[KeyRootPos.Length - 1]; }
    }

    public int Length
    {
        get { return (KeyRootPos.Length-1) * 33; }
    }

    public int GetFrameTime(int FrameNum)
    {
         return FrameNum * 34; 
    }

    public AnimClipKeyData(int[] x,int[] y,int[] z,string name  )
    {
        KeyRootPos = new Vector3[x.Length];
        AnimclipName = name;
        if(name.Contains("attack_"))
        {
            AnimclipName = "Attack";
            string[] strs = name.Split('_');
            ParamValue = int.Parse(strs[strs.Length - 1]);
        }
        
        for (int i = 0; i<x.Length; i++)
        {
            KeyRootPos[i] = new Vector3(x[i], y[i], z[i]);
        }
    }
}

public class ACTAnimConfig : MonoBehaviour
{
    public List<AnimClipKeyData> keyDatas = new List<AnimClipKeyData>();
    public List<AnimationClip> AnimClips = new List<AnimationClip>();


#if UNITY_EDITOR
    public enum AnimTestState
    {
        None,
        Start,
        Running,
        End,
    } 

    Animator animator;
    AnimClipKeyData CurClipData;
    int CurClipFrameNum;
    int CurRunningTime;

    public void Update()
    {
        if (CurClipData == null)
            return;
        CurRunningTime += (int)(Time.deltaTime*1000);
        while(CurClipData.GetFrameTime(CurClipFrameNum) <= CurRunningTime)
        {
            Run();
            CurClipFrameNum++;
            
        }
        if(CurRunningTime >= CurClipData.Length)
        {
            CurClipData = null;
        }
    }

    void Run()
    {
        Debug.Log("************** " + CurClipFrameNum + " : " + CurRunningTime);
        //if(CurClipFrameNum > 0)
        //    transform.position = (transform.position + (CurClipData.KeyRootPos[CurClipFrameNum].vec3 - CurClipData.KeyRootPos[CurClipFrameNum-1].vec3));
        //else
        //    transform.position = (transform.position + (CurClipData.KeyRootPos[CurClipFrameNum].vec3));
    }

    public void TestAnim(AnimClipKeyData animkey)
    {
        animator = this.gameObject.GetComponentInChildren<Animator>();
        if (animator == null)
            return;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        animator.SetInteger(animkey.AnimclipName, animkey.ParamValue);
        
        CurClipData = animkey;
        CurClipFrameNum = 0;
        CurRunningTime = 0;
    }
#endif
}
