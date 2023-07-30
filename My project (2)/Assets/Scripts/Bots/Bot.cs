using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using User;

namespace Bots
{
    public class Bot : NetworkBehaviour
    {
        [SerializeField] private int health;
        [SerializeField] private int speed;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;
        [SerializeField] public Player aggroPlayer;

        public void AggroPlayer(Player player)
        {
            StartCoroutine(ChaseCoroutine(player));
        }
        private IEnumerator ChaseCoroutine(Player player)
        {
            aggroPlayer = player;
            while (true)
            {
                agent.SetDestination(aggroPlayer.transform.position);
                yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
            }
        }
    }
}