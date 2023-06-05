
using System.Collections.Generic;
using Broilerplate.Gameplay;
using Broilerplate.Gameplay.Input;
using UnityEngine;

namespace Broilerplate.Core {
    /// <summary>
    /// Every map has a game mode on it.
    /// The game mode takes care of spawning the player object and
    /// prepping the map for play. It provides the rules for the game to work.
    /// </summary>
    public class GameMode : Actor {
        /// <summary>
        /// The prefab of the player controller we want to instantiate with this game mode
        /// </summary>
        [SerializeField]
        private PlayerController defaultPlayerControllerType;
        
        /// <summary>
        /// The prefab of the player pawn we want to instantiate along with the player controller.
        /// </summary>
        [SerializeField]
        private Pawn defaultPlayerPawnType;

        /// <summary>
        /// A list of player controllers on this game mode (in local multiplayer setups, entirely untested rn)
        /// </summary>
        private List<PlayerController> playerControllers = new List<PlayerController>();
        

        /// <summary>
        /// Retrieve the player controller prefab.
        /// </summary>
        /// <returns></returns>
        public PlayerController GetPlayerControllerType() {
            return defaultPlayerControllerType;
        }

        /// <summary>
        /// Retrieve the player pawn prefab.
        /// </summary>
        /// <returns></returns>
        public virtual Pawn GetPlayerPawnType() {
            return defaultPlayerPawnType;
        }

        /// <summary>
        /// Spawn the player controller and its pawn to possess.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Handles the logic of spawning the player controller as defined by the game mode
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Handles the logic of spawning the player pawn as defined by the game mode.
        /// </summary>
        /// <param name="spawnPosition"></param>
        /// <param name="spawnRotation"></param>
        /// <returns></returns>
        private Pawn SpawnPlayerPawn(Vector3 spawnPosition, Quaternion spawnRotation) {
            Pawn p;
            Pawn pawnType = GetPlayerPawnType();
            
            if (pawnType != null) {
                p = GetWorld().SpawnActor(pawnType, spawnPosition, spawnRotation);
            }
            else {
                var testPawn = GetWorld().FindActorOfType<Pawn>();

                if (testPawn) {
                    if (!testPawn.HasBegunPlaying) {
                        // that happens when the pawn was in the scene already. But the BeginPlay calls for in-scene actors are usually called after all
                        // the game mode bootstrapping is done.
                        // So we have to manually call it here.
                        testPawn.SetWorld(GetWorld());
                        testPawn.BeginPlay();
                    }
                    return testPawn;
                }
                // No pawn found, spawn a default one
                var go = new GameObject("Default Player Pawn");
                p = GetWorld().SpawnActorOn<Pawn>(go);
            }

            return p;
        }

        /// <summary>
        /// Shortcut to the the player controller with the ID 0.
        /// Which is, by design, the main player.
        /// </summary>
        /// <returns></returns>
        public PlayerController GetMainPlayerController() {
            return GetPlayerController(0);
        }

        /// <summary>
        /// Get any player controller by any id.
        /// Returns null if there is no player controller with the given id.
        /// </summary>
        /// <param name="controllerIndex"></param>
        /// <returns></returns>
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