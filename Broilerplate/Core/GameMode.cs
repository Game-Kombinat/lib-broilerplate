
using Broilerplate.Gameplay;
using Broilerplate.Gameplay.Input;
using UnityEngine;

namespace Broilerplate.Core {
    /// <summary>
    /// Every map has a game mode on it.
    /// The game mode takes care of spawning the player object and
    /// prepping the map for play.
    /// </summary>
    public class GameMode : Actor {
        [SerializeField]
        private PlayerController defaultPlayerControllerType;
        
        [SerializeField]
        private Pawn defaultPawnType;
    }
}