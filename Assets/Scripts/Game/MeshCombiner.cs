using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour
{
   /* public void AdvancedCombineMeshes()
    {
        MeshFilter myMeshFilter = GetComponent<MeshFilter>();
        MeshRenderer myMeshRenderer = GetComponent<MeshRenderer>();
        Mesh finalMesh = myMeshFilter.sharedMesh;

        var transform1 = transform;
        Quaternion oldRot = transform1.rotation;
        Vector3 oldPos = transform1.position;

        transform1.rotation = Quaternion.identity;
        transform1.position = Vector3.zero;
        
        if (finalMesh == null)
        {
            finalMesh = new Mesh();
            myMeshFilter.sharedMesh = finalMesh;
        }
        else
        {
            finalMesh.Clear();
        }
        
        // Every MeshFilter (Parent included)
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(false);

        // All the meshes in our children (just a big list)
        List<Material> materials = new List<Material>();
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(false);
        
        foreach (MeshRenderer rend in renderers)
        {
            if (rend.transform == transform)
                continue;
            
            Material[] localMats = rend.sharedMaterials;
            
            foreach (Material localMat in localMats)
                if (!materials.Contains (localMat))
                    materials.Add (localMat);
        }

        // Each material will have a mesh for it.
        List<Mesh> subMeshes = new List<Mesh>();
        foreach (Material material in materials)
        {
            // Make a combiner for each (sub)mesh that is mapped to the right material.
            List<CombineInstance>combiners = new List<CombineInstance>();
            
            foreach (MeshFilter filter in filters)
            {
                if (filter.transform == transform) 
                    continue;
                
                // The mesh-renderer holds the reference to the material(s)
                MeshRenderer rend = filter.GetComponent<MeshRenderer>();
                if (rend == null)
                {
                    Debug.LogError (filter.name + " has no MeshRenderer");
                    continue;
                }

                // Let's see if their materials are the one we want right now.
                Material[] localMaterials = rend.sharedMaterials;
                
                for (int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++)
                {
                    if (localMaterials [materialIndex] != material)
                        continue;
                    
                    // This sub-mesh is the material we're looking for right now.
                    CombineInstance ci = new CombineInstance
                    {
                        mesh = filter.sharedMesh,
                        subMeshIndex = materialIndex,
                        transform = filter.transform.localToWorldMatrix
                    };
                    combiners.Add (ci);
                }
            }
            // Flatten into a single mesh.
            Mesh mesh = new Mesh ();
            mesh.CombineMeshes (combiners.ToArray(), true);
            subMeshes.Add(mesh);
        }

        // The final mesh: combine all the material-specific meshes as independent sub-meshes.
        List<CombineInstance>finalCombiners = new List<CombineInstance>();
        foreach (Mesh mesh in subMeshes)
        {
            CombineInstance ci = new CombineInstance
            {
                mesh = mesh, 
                subMeshIndex = 0, 
                transform = Matrix4x4.identity
            };
            
            finalCombiners.Add (ci);
        }
        
        finalMesh.CombineMeshes (finalCombiners.ToArray(), false);
        myMeshFilter.sharedMesh = finalMesh;
        myMeshRenderer.sharedMaterials = materials.ToArray();
        
        Debug.Log ("Final mesh has " + subMeshes.Count + " materials.");
        
        //Reset the position to how it was before
        transform1.rotation = oldRot;
        transform1.position = oldPos;
        
        //Hide the pieces (you can also delete them, but then you won't be able to make changes)
        for (int a = 0; a < transform.childCount; a++)
        {
            transform.GetChild(a).gameObject.SetActive(false);
        }
    }
    
    public void SaveMesh(string path)
    {
        Debug.Log ("Saving Mesh?");

        string fullPath = path + transform.name + ".asset";
        Mesh m1 = transform.GetComponent<MeshFilter>().sharedMesh;

        if (AssetDatabase.Contains(m1))
        {
            Debug.Log("Mesh is already saved! Mesh will be overridden on combine!");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath(fullPath, typeof(Mesh)) != null)
            AssetDatabase.DeleteAsset(fullPath);

        AssetDatabase.CreateAsset(m1, fullPath); // saves to "assets/"
        AssetDatabase.SaveAssets();
    }*/
}
