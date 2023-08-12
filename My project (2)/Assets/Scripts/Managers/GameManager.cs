using System.Collections.Generic;
using UnityEngine;
using User;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] public List<Player> allPlayers = new List<Player>();
    }
}