using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using User;
using Random = UnityEngine.Random;

namespace Bots
{
    public class BotManager : NetworkBehaviour
    {
        public List<Bot> bots = new();
        public List<Player> allPlayers = new();
        public List<Transform> spawnPositions=new();
        public AnimatedMeshScriptableObject AnimationSO;
        private readonly WaitForSecondsRealtime waitForAnimUpdate = new (0.012f);
        private readonly WaitForEndOfFrame waitForEndOfFrame=new WaitForEndOfFrame();
        public void AggroBots()
        {
            StartCoroutine(ChaseCoroutine());
            StartCoroutine(UpdateAnimations());
        }
        private void Start()
        {
            bots = FindObjectsOfType<Bot>().ToList();
        }
        public Vector3 GetRandomSpawnPos()
        {
            int randomIndex = Random.Range(0, spawnPositions.Count);
            return spawnPositions[randomIndex].position;
        }
        private IEnumerator UpdateAnimations()
        {
            while (true)
            {
                foreach (var bot in bots)
                {
                    bot.AnimatedMesh.UpdateMesh(); // Make sure to add a reference to the AnimatedMesh script in your Bot class
                }
                yield return waitForAnimUpdate;
            }
        }
        public void NotifyBotsOfNewPlayer(Player newPlayer)
        {
            allPlayers.Add(newPlayer);
            foreach(Bot bot in bots)
            {
                bot.EvaluateTarget();
            }
            AggroBots();
        }
        private IEnumerator ChaseCoroutine()
        {
            while (true)
            {
                foreach (Bot bot in bots)
                {
                    if (bot.gameObject.activeInHierarchy)
                    {
                        bot.agent.SetDestination(bot.targetPlayer.transform.position);
                    }
                    
                }
                yield return waitForEndOfFrame;
            }
           
        }
    }
}