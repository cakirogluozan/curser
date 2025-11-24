#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class CreateAnimationsFromFolders : EditorWindow
{
    [MenuItem("Tools/Create Animations from Folders")]
    public static void ShowWindow()
    {
        GetWindow<CreateAnimationsFromFolders>("Animation Creator");
    }
    
    private GameObject targetPlayer;
    private string animationsFolder = "Assets/Animations/Player";
    
    void OnGUI()
    {
        GUILayout.Label("Create Animations from PNG Folders", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This tool creates animation clips from your organized PNG folders (back-attacking, front-idle, etc.)", MessageType.Info);
        
        EditorGUILayout.Space();
        
        targetPlayer = (GameObject)EditorGUILayout.ObjectField("Player GameObject", targetPlayer, typeof(GameObject), true);
        
        EditorGUILayout.Space();
        
        animationsFolder = EditorGUILayout.TextField("Animations Save Folder", animationsFolder);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Animations from Selected Folder", GUILayout.Height(30)))
        {
            CreateAnimationsFromSelectedFolder();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("Select a folder containing subfolders (back-attacking, front-idle, etc.)\nEach subfolder should contain PNG sequence files.", MessageType.Info);
    }
    
    void CreateAnimationsFromSelectedFolder()
    {
        if (targetPlayer == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Player GameObject!", "OK");
            return;
        }
        
        string folderPath = EditorUtility.OpenFolderPanel("Select Folder with Animation Folders", "Assets", "");
        
        if (string.IsNullOrEmpty(folderPath)) return;
        
        // Convert to relative path
        string relativeParentPath = "";
        if (folderPath.StartsWith(Application.dataPath))
        {
            relativeParentPath = "Assets" + folderPath.Substring(Application.dataPath.Length);
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Please select a folder inside your Assets directory!", "OK");
            return;
        }
        
        // Create animations folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(animationsFolder))
        {
            string[] folders = animationsFolder.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
        
        // Get all subdirectories
        string[] subDirectories = Directory.GetDirectories(relativeParentPath);
        
        if (subDirectories.Length == 0)
        {
            EditorUtility.DisplayDialog("No Folders Found", "No subfolders found in the selected directory.", "OK");
            return;
        }
        
        int animationsCreated = 0;
        
        foreach (string subFolderPath in subDirectories)
        {
            string folderName = Path.GetFileName(subFolderPath);
            string[] pngFiles = Directory.GetFiles(subFolderPath, "*.png", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f)
                .ToArray();
            
            if (pngFiles.Length == 0) continue;
            
            // Load sprites
            Sprite[] sprites = new Sprite[pngFiles.Length];
            for (int i = 0; i < pngFiles.Length; i++)
            {
                string relativePath = pngFiles[i].Replace('\\', '/');
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);
            }
            
            // Filter out null sprites
            sprites = sprites.Where(s => s != null).ToArray();
            
            if (sprites.Length == 0) continue;
            
            // Create animation clip
            AnimationClip clip = new AnimationClip();
            clip.frameRate = 12f; // Default frame rate
            
            EditorCurveBinding spriteBinding = new EditorCurveBinding();
            spriteBinding.type = typeof(SpriteRenderer);
            spriteBinding.path = "";
            spriteBinding.propertyName = "m_Sprite";
            
            ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];
            float frameTime = 1f / clip.frameRate;
            
            for (int i = 0; i < sprites.Length; i++)
            {
                spriteKeyFrames[i] = new ObjectReferenceKeyframe();
                spriteKeyFrames[i].time = i * frameTime;
                spriteKeyFrames[i].value = sprites[i];
            }
            
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);
            
            // Set clip settings
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            
            // Save animation clip
            string clipPath = animationsFolder + "/" + folderName + ".anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            animationsCreated++;
            
            Debug.Log($"Created animation: {folderName} with {sprites.Length} frames");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Animations Created", 
            $"Created {animationsCreated} animation clips!\n\n" +
            "Next steps:\n" +
            "1. Create an Animator Controller\n" +
            "2. Add these animation clips to the controller\n" +
            "3. Set up transitions between animations", 
            "OK");
    }
}
#endif

