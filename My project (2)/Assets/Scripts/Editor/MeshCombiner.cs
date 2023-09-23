// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEngine;
//
// namespace Editor
// {
//     [CustomEditor(typeof(MeshCombiner))]
//     public class MeshCombinerEditor : UnityEditor.Editor
//     {
//         public override void OnInspectorGUI()
//         {
//             MeshCombiner meshCombiner = (MeshCombiner)target;
//
//             // Show default inspector properties
//             DrawDefaultInspector();
//
//             // Add a button to combine meshes
//             if (GUILayout.Button("Combine Submeshes"))
//             {
//                 meshCombiner.Combine();
//             }
//         }
//     }
// }
// #endif
