using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace AnimationBakers
{
    [RequireComponent(typeof(MeshFilter))]
    public class AnimatedMesh : NetworkBehaviour
    {
        [SerializeField]public AnimatedMeshScriptableObject AnimationSO;
        public MeshFilter Filter;
        [Header("Debug")]
        [SerializeField]public int Tick = 1;
        [SerializeField]public int AnimationIndex;
        [SerializeField]public string AnimationName;
        public List<Mesh> AnimationMeshes;
        public delegate void AnimationEndEvent(string Name);
        public float LastTickTime;
        private void Awake()
        {
            Filter = GetComponent<MeshFilter>();
            Play("BotRun");
        }
        public void Play(string AnimationName)
        {
            if (AnimationName != this.AnimationName)
            {
                this.AnimationName = AnimationName;
                Tick = 1;
                AnimationIndex = 0;
                AnimatedMeshScriptableObject.Animation animation = AnimationSO.Animations.Find((item) => item.Name.Equals(AnimationName));
                AnimationMeshes = animation.Meshes;
                if (string.IsNullOrEmpty(animation.Name))
                {
                    Debug.LogError($"Animated model {name} does not have an animation baked for {AnimationName}!");
                }
                AnimationIndex = Random.Range(0, AnimationMeshes.Count);
            }
        }
        public void UpdateMesh()  // Make this method public
        {
        
            if (AnimationMeshes != null)
            {
                Filter.mesh = AnimationMeshes[AnimationIndex];
                AnimationIndex++;
                if (AnimationIndex >= AnimationMeshes.Count)
                {
                    //OnAnimationEnd?.Invoke(AnimationName);
                    AnimationIndex = 0;
                }
                Tick++;
            }
        }
    
    }
}