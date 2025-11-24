using UnityEngine;

/// <summary>
/// Helper script to check materials from FBX files
/// Materials should be automatically imported from your FBX if Material Creation Mode is set to "Standard"
/// </summary>
public class ModelMaterialHelper : MonoBehaviour
{
    [Header("Custom Material (Optional)")]
    [SerializeField] private Material customMaterial;
    
    [ContextMenu("Check Materials from FBX")]
    public void CheckMaterialsFromFBX()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No MeshRenderer found! Make sure your model is in the scene.");
            return;
        }
        
        Debug.Log($"Found {renderers.Length} MeshRenderer(s):");
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer != null)
            {
                Material[] materials = renderer.sharedMaterials;
                Debug.Log($"  {renderer.gameObject.name}: {materials.Length} material(s)");
                
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        Debug.Log($"    Material {i}: {materials[i].name} (Shader: {materials[i].shader.name})");
                    }
                    else
                    {
                        Debug.LogWarning($"    Material {i}: NULL - Material not imported! Check FBX import settings.");
                    }
                }
            }
        }
    }
    
    [ContextMenu("Apply Custom Material")]
    public void ApplyCustomMaterial()
    {
        if (customMaterial == null)
        {
            Debug.LogWarning("Custom Material is not assigned!");
            return;
        }
        
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No MeshRenderer found on this GameObject or its children!");
            return;
        }
        
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.material = customMaterial;
                Debug.Log($"Applied custom material to {renderer.gameObject.name}");
            }
        }
        
        Debug.Log("Custom material applied successfully!");
    }
}

