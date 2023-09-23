using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshCombinerWindow : EditorWindow
{
    private string folderPath = "Assets/YourMeshesFolder";
    private Vector2 scroll;

    [MenuItem("Window/MeshCombiner")]
    public static void ShowWindow()
    {
        GetWindow<MeshCombinerWindow>("Mesh Combiner");
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        GUILayout.Label("Mesh Folder Path", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Folder Path:", folderPath);

        if (GUILayout.Button("Combine Submeshes in Folder"))
        {
            string[] assetPaths = AssetDatabase.FindAssets("t:Mesh", new[] { folderPath });

            foreach (string assetPath in assetPaths)
            {
                string fullPath = AssetDatabase.GUIDToAssetPath(assetPath);
                Mesh originalMesh = AssetDatabase.LoadAssetAtPath<Mesh>(fullPath);

                if (originalMesh != null)
                {
                    Mesh newMesh = new Mesh
                    {
                        vertices = originalMesh.vertices,
                        normals = originalMesh.normals,
                        uv = originalMesh.uv
                    };

                    int[] combinedTriangles = CombineSubmeshTriangles(originalMesh);
                    newMesh.subMeshCount = 1;
                    newMesh.SetTriangles(combinedTriangles, 0);
                    newMesh.Optimize();

                    // Save the new mesh as an asset
                    string newAssetPath = folderPath + "/" + originalMesh.name + "_Combined.asset";
                    AssetDatabase.CreateAsset(newMesh, newAssetPath);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Submeshes combined and assets saved.");
        }

        EditorGUILayout.EndScrollView();
    }

    private int[] CombineSubmeshTriangles(Mesh mesh)
    {
        int subMeshCount = mesh.subMeshCount;
        int[] combinedTriangles = new int[0];

        for (int i = 0; i < subMeshCount; i++)
        {
            int[] subMeshTriangles = mesh.GetTriangles(i);
            int oldLength = combinedTriangles.Length;
            System.Array.Resize(ref combinedTriangles, combinedTriangles.Length + subMeshTriangles.Length);

            for (int j = 0; j < subMeshTriangles.Length; j++)
            {
                combinedTriangles[oldLength + j] = subMeshTriangles[j];
            }
        }

        return combinedTriangles;
    }
}
