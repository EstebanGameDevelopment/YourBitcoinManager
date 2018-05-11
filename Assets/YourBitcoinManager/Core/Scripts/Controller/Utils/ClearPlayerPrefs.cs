using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ClearPlayerPrefs {

#if UNITY_EDITOR
    [MenuItem("Your Bitcoin Manager/Clear PlayerPrefs")]
    private static void NewMenuOption()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs CLEARED!!!");
    }
#endif
}
