#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ModelMaterialHelper))]
public class ModelMaterialHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ModelMaterialHelper helper = (ModelMaterialHelper)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("If your FBX has colors but they're not showing:\n1. Select your FBX file in Project window\n2. In Inspector, go to Materials tab\n3. Set 'Material Creation Mode' to 'Standard'\n4. Click 'Apply'", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Check Materials from FBX", GUILayout.Height(30)))
        {
            helper.CheckMaterialsFromFBX();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Apply Custom Material", GUILayout.Height(30)))
        {
            helper.ApplyCustomMaterial();
        }
    }
}
#endif

