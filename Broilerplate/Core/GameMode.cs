
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
        private Pawn defaultPawnType;

        private List<PlayerController> playerControllers = new();
        

        // register player
        public PlayerController GetPlayerControllerType() {
            return defaultPlayerControllerType;
        }

        public virtual Pawn GetPawnType() {
            return defaultPawnType;
        }

        public PlayerController SpawnPlayer(PlayerInfo playerInfo, Vector3 spawnPosition, Quaternion spawnRotation) {
            PlayerController pc = SpawnPlayerController();
            playerInfo.SetPlayerController(pc);
            Pawn p = SpawnPlayPawn();
            pc.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            p.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            pc.Possess(p);
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
            Pawn pawnType = GetPawnType();
            
            if (pawnType != null) {
                p = GetWorld().SpawnActor(pawnType, Vector3.zero, Quaternion.identity);
            }
            else {
                var go = new GameObject("Default Player Pawn");
                p = GetWorld().SpawnActorOn<Pawn>(go);
            }

            return p;
        }
    }
}