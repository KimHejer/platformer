using Platformer.Game.Entities;

namespace Platformer.Game.Checkpoints;

public class CheckpointManager
{
    private PlayerSnapshot _savedState;

    public CheckpointManager(Player player)
    {
        _savedState = player.CaptureState();
    }

    public void Save(Player player)
    {
        _savedState = player.CaptureState();
    }

    public void Restore(Player player)
    {
        player.RestoreState(_savedState);
    }
}