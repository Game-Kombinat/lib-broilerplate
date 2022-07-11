
using System.Collections.Generic;
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
        private Pawn defaultPlayerPawnType;

        private List<PlayerController> playerControllers = new List<PlayerController>();
        
        
        /// <summary>
        /// Called after when booting the world.
        /// This happens after player controller was initialised and before
        /// actors get their BeginPlay calls.
        /// </summary>
        public virtual void LateBeginPlay() {}

        // register player
        public PlayerController GetPlayerControllerType() {
            return defaultPlayerControllerType;
        }

        public virtual Pawn GetPlayerPawnType() {
            return defaultPlayerPawnType;
        }

        public PlayerController SpawnPlayer(PlayerInfo playerInfo, Vector3 spawnPosition, Quaternion spawnRotation) {
            PlayerController pc = SpawnPlayerController();
            // SpawnPlayerPawn might have components on it that require the player controller,
            // and by extension all of its systems, to be accessible. So Add this first thing.
            playerControllers.Add(pc);
            
            playerInfo.SetPlayerController(pc);
            Pawn p = SpawnPlayerPawn(spawnPosition, spawnRotation);
            pc.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            p.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            pc.ControlPawn(p);
            return pc;
        }

        private PlayerController SpawnPlayerController() {
            PlayerController pc;
            PlayerController pcType = GetPlayerControllerType();
            if (pcType != null) {
                pc = GetWorld().SpawnActor(pcType, Vector3.zero, Quaternion.identity);
            }
            else {
                var go = new GameObject("Default Player Controller");
                pc = GetWorld().SpawnActorOn<PlayerController>(go);
            }

            return pc;
        }

        private Pawn SpawnPlayerPawn(Vector3 spawnPosition, Quaternion spawnRotation) {
            Pawn p;
            Pawn pawnType = GetPlayerPawnType();
            
            if (pawnType != null) {
                p = GetWorld().SpawnActor(pawnType, spawnPosition, spawnRotation);
            }
            else {
                var go = new GameObject("Default Player Pawn");
                p = GetWorld().SpawnActorOn<Pawn>(go);
            }

            return p;
        }

        public PlayerController GetMainPlayerController() {
            return GetPlayerController(0);
        }

        public PlayerController GetPlayerController(int controllerIndex) {
            for (int i = 0; i < playerControllers.Count; i++) {
                if (playerControllers[i].PlayerInfo.PlayerId == controllerIndex) {
                    return playerControllers[i];
                }
            }

            return null;
        }
    }
}