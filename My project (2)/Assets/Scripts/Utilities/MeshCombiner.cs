// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
//
// public class MeshCombiner : MonoBehaviour
// {
//     public List<MeshFilter> meshFiltersToCombine;
//
//     public void Combine()
//     {
//         foreach (MeshFilter meshFilter in meshFiltersToCombine)
//         {
//             if (meshFilter != null)
//             {
//                 Mesh originalMesh = meshFilter.sharedMesh;
//                 Mesh newMesh = new Mesh();
//
//                 // Copy the original mesh attributes to the new mesh
//                 newMesh.vertices = originalMesh.vertices;
//                 newMesh.normals = originalMesh.normals;
//                 newMesh.uv = originalMesh.uv;
//
//                 int[] combinedTriangles = CombineSubmeshTriangles(originalMesh);
//
//                 newMesh.subMeshCount = 1;
//                 newMesh.SetTriangles(combinedTriangles, 0);
//                 newMesh.Optimize();
//
//                 // Save the new mesh as an asset
//                 string assetPath = "Assets/CombinedMeshes/" + meshFilter.gameObject.name + "_Combined.asset";
//                 AssetDatabase.CreateAsset(newMesh, assetPath);
//                 AssetDatabase.SaveAssets();
//             }
//         }
//     }
//
//     private int[] CombineSubmeshTriangles(Mesh mesh)
//     {
//         int subMeshCount = mesh.subMeshCount;
//         int[] combinedTriangles = new int[0];
//
//         for (int i = 0; i < subMeshCount; i++)
//         {
//             int[] subMeshTriangles = mesh.GetTriangles(i);
//             int oldLength = combinedTriangles.Length;
//             System.Array.Resize(ref combinedTriangles, combinedTriangles.Length + subMeshTriangles.Length);
//
//             for (int j = 0; j < subMeshTriangles.Length; j++)
//             {
//                 combinedTriangles[oldLength + j] = subMeshTriangles[j];
//             }
//         }
//
//         return combinedTriangles;
//     }
// }