
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

        private List<PlayerController> playerControllers = new();
        

        // register player
        public PlayerController GetPlayerControllerType() {
            return defaultPlayerControllerType;
        }

        public virtual Pawn GetPlayerPawnType() {
            return defaultPlayerPawnType;
        }

        public PlayerController SpawnPlayer(PlayerInfo playerInfo, Vector3 spawnPosition, Quaternion spawnRotation) {
            PlayerController pc = SpawnPlayerController();
            playerInfo.SetPlayerController(pc);
            Pawn p = SpawnPlayPawn();
            pc.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            p.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            pc.TakeControl(p);
            playerControllers.Add(pc);
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

        private Pawn SpawnPlayPawn() {
            Pawn p;
            Pawn pawnType = GetPlayerPawnType();
            
            if (pawnType != null) {
                p = GetWorld().SpawnActor(pawnType, Vector3.zero, Quaternion.identity);
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