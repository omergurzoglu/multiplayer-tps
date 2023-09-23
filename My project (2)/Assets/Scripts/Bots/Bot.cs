using System.Collections;
using AnimationBakers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using User;
using Random = UnityEngine.Random;

namespace Bots
{
    public class Bot : NetworkBehaviour
    {
        [SerializeField] public NavMeshAgent agent;
        [SerializeField] public Player targetPlayer;
        [SerializeField] public AnimatedMesh AnimatedMesh;
        [SerializeField] private BotManager botManager;
        [SerializeField] public ParticleSystem deathFX;
        [SerializeField] public BoxCollider boxCollider;

        public NetworkVariable<int> health = new(1, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> IsVisible = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> ColliderEnabled = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly WaitForSecondsRealtime waitForSeconds = new (2f);
        private MeshRenderer meshRenderer;
        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            boxCollider = GetComponent<BoxCollider>();
            botManager = FindObjectOfType<BotManager>();
            health.Value = 2;
            IsVisible.OnValueChanged += OnVisibilityChanged;
            ColliderEnabled.OnValueChanged += OnColliderStateChanged;
        }
        private void OnColliderStateChanged(bool oldVal, bool newVal)
        {
            boxCollider.enabled = newVal;
        }
        private void OnVisibilityChanged(bool oldVal, bool newVal)
        {
            meshRenderer.enabled = newVal;
        }
        private void Start()
        {
            transform.position = botManager.GetRandomSpawnPos();
            agent.speed = Random.Range(6f, 6.5f);
        }
        [ServerRpc(RequireOwnership = false)]
        public void RecycleServerRpc()
        {
            botManager.bots.Remove(this);
            ColliderEnabled.Value = false;
            IsVisible.Value = false;
            deathFX.Play();
            agent.ResetPath();
            agent.enabled = false;
            PlayDeathEffectClientRpc();
            StartCoroutine(RespawnBot());
        }
        private IEnumerator RespawnBot()
        {
            yield return waitForSeconds;
            transform.position = botManager.GetRandomSpawnPos();
            targetPlayer = botManager.allPlayers[Random.Range(0, botManager.allPlayers.Count)];
            health.Value = 2;
            IsVisible.Value = true;
            ColliderEnabled.Value = true;
            boxCollider.enabled = true;
            agent.enabled = true;
            botManager.bots.Add(this);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void DamageBotServerRpc()
        {
            if (health.Value > 0)
            {
                health.Value--;
                if (health.Value <= 0)
                {
                    RecycleServerRpc();
                }
            }
        }
        [ClientRpc]
        private void PlayDeathEffectClientRpc()
        {
            deathFX.Play();
        }
        public void EvaluateTarget()
        {
            if(botManager.allPlayers.Count > 0)
            {
                int randomIndex = Random.Range(0, botManager.allPlayers.Count);
                targetPlayer = botManager.allPlayers[randomIndex];
            }
        }
    }
}