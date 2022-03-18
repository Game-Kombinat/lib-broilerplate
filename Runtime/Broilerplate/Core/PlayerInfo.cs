using Broilerplate.Gameplay.Input;

namespace Broilerplate.Core {
    /// <summary>
    /// Represents a player in a world.
    /// This is is used to register new players,
    /// however their source, with the current world.
    /// From this a player controller is created and a pawn is spawned.
    /// </summary>
    public class PlayerInfo {
        private PlayerController controller;
        private readonly int playerId;

        public int PlayerId => playerId;

        public PlayerController PlayerController => controller;

        public PlayerInfo(int id) {
            playerId = id;
        }

        /// <summary>
        /// Change player controller for this player info.
        /// Player Info persists throughout levels whereas player controllers
        /// are (normally) bound to a level - this allows to update
        /// </summary>
        /// <param name="inController"></param>
        public void SetPlayerController(PlayerController inController) {
            if (controller) {
                controller.SetPlayerInfo(null);
            }

            controller = inController;
            inController.SetPlayerInfo(this);
        }
    }
}