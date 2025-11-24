#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class SpriteImporterHelper : EditorWindow
{
    [MenuItem("Tools/Sprite Importer Helper")]
    public static void ShowWindow()
    {
        GetWindow<SpriteImporterHelper>("Sprite Importer");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Sprite Import Helper", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This tool helps you import PNG sequences organized in folders (like back-attacking, front-idle, etc.)", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Import PNG Sequences from Folder", GUILayout.Height(30)))
        {
            ImportPNGSequences();
        }
        
        if (GUILayout.Button("Import All Folders (back-attacking, front-idle, etc.)", GUILayout.Height(30)))
        {
            ImportAllAnimationFolders();
        }
        
        if (GUILayout.Button("Import Spritesheets", GUILayout.Height(30)))
        {
            ImportSpritesheets();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Vector paths (SVG, AI, EPS) need to be converted to PNG first.\nUnity doesn't support vector graphics for sprites.", MessageType.Warning);
    }
    
    void ImportPNGSequences()
    {
        string folderPath = EditorUtility.OpenFolderPanel("Select PNG Sequence Folder", "Assets", "");
        
        if (string.IsNullOrEmpty(folderPath)) return;
        
        // Convert to relative path
        if (folderPath.StartsWith(Application.dataPath))
        {
            folderPath = "Assets" + folderPath.Substring(Application.dataPath.Length);
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Please select a folder inside your Assets directory!", "OK");
            return;
        }
        
        string[] pngFiles = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f)
            .ToArray();
        
        if (pngFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("No PNGs Found", "No PNG files found in the selected folder.", "OK");
            return;
        }
        
        int imported = 0;
        foreach (string filePath in pngFiles)
        {
            string relativePath = filePath.Replace('\\', '/');
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                
                AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
                imported++;
            }
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Import Complete", $"Imported {imported} PNG sequence files as sprites!", "OK");
    }
    
    void ImportAllAnimationFolders()
    {
        string parentFolderPath = EditorUtility.OpenFolderPanel("Select Parent Folder (contains back-attacking, front-idle, etc.)", "Assets", "");
        
        if (string.IsNullOrEmpty(parentFolderPath)) return;
        
        // Convert to relative path
        string relativeParentPath = "";
        if (parentFolderPath.StartsWith(Application.dataPath))
        {
            relativeParentPath = "Assets" + parentFolderPath.Substring(Application.dataPath.Length);
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Please select a folder inside your Assets directory!", "OK");
            return;
        }
        
        // Get all subdirectories
        string[] subDirectories = Directory.GetDirectories(relativeParentPath);
        
        if (subDirectories.Length == 0)
        {
            EditorUtility.DisplayDialog("No Folders Found", "No subfolders found in the selected directory.", "OK");
            return;
        }
        
        int totalImported = 0;
        int foldersProcessed = 0;
        
        foreach (string folderPath in subDirectories)
        {
            string folderName = Path.GetFileName(folderPath);
            string[] pngFiles = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f)
                .ToArray();
            
            if (pngFiles.Length > 0)
            {
                int imported = 0;
                foreach (string filePath in pngFiles)
                {
                    string relativePath = filePath.Replace('\\', '/');
                    TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
                    
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.mipmapEnabled = false;
                        importer.filterMode = FilterMode.Point;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        
                        AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
                        imported++;
                    }
                }
                
                totalImported += imported;
                foldersProcessed++;
                Debug.Log($"Imported {imported} sprites from folder: {folderName}");
            }
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Import Complete", 
            $"Imported {totalImported} sprites from {foldersProcessed} folders!\n\n" +
            "Next steps:\n" +
            "1. Select all sprites in each folder\n" +
            "2. Drag them onto your Player GameObject to create animation clips\n" +
            "3. Name them based on folder (e.g., 'back-attacking', 'front-idle')", 
            "OK");
    }
    
    void ImportSpritesheets()
    {
        string filePath = EditorUtility.OpenFilePanel("Select Spritesheet", "Assets", "png");
        
        if (string.IsNullOrEmpty(filePath)) return;
        
        // Convert to relative path
        if (filePath.StartsWith(Application.dataPath))
        {
            filePath = "Assets" + filePath.Substring(Application.dataPath.Length);
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Please select a file inside your Assets directory!", "OK");
            return;
        }
        
        TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
        
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            
            // Select the texture so user can easily open sprite editor
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            EditorGUIUtility.PingObject(Selection.activeObject);
            
            EditorUtility.DisplayDialog("Spritesheet Imported", 
                "Spritesheet imported!\n\n" +
                "To slice it:\n" +
                "1. The spritesheet is now selected in Project window\n" +
                "2. In Inspector, click 'Sprite Editor' button\n" +
                "3. Click 'Slice' → 'Grid By Cell Count' or 'Automatic'\n" +
                "4. Click 'Apply'", 
                "OK");
        }
    }
}
#endif

