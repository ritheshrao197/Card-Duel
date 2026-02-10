using Unity.Netcode;

namespace CardDuel.Networking
{
    public class MatchSnapshotNetwork : NetworkBehaviour
    {
        [ClientRpc]
        public void SyncSnapshotClientRpc(string snapshotJson)
        {
            // TODO:
            // Deserialize snapshot
            // Rebuild UI state
            // Re-seed HUD, hand, board
        }
    }
}
