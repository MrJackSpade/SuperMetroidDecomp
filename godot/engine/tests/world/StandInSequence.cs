using Engine.World;

namespace Engine.Tests.World;

/// <summary>A room sequence for the engine's own checks: it runs for a set number of updates and applies a terrain patch on its last.</summary>
public partial class StandInSequence : RoomSequence
{
    /// <summary>How many updates the sequence runs.</summary>
    public int Length { get; set; } = 3;

    /// <summary>The terrain patch applied on the last update; empty for none.</summary>
    public string Patch { get; set; } = "";

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed < Length - 1)
            return false;
        if (Patch.Length > 0)
            room.Terrain.ApplyPatch(Patch);
        return true;
    }
}
