namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a drop of Mother Brain's drool keeps while it hangs from her mouth: the mouth point it hangs from.</summary>
/// <param name="motherBrain">The encounter whose mouth the drool hangs from.</param>
/// <param name="mouthPoint">The mouth point, relative to her brain's centre.</param>
internal sealed class MotherBrainDroolState(MotherBrainEnemyState motherBrain, Offset16<RoomPixels> mouthPoint)
    : MotherBrainProjectileState(motherBrain)
{
    /// <summary>The mouth point the drool hangs from, relative to her brain's centre.</summary>
    internal Offset16<RoomPixels> MouthPoint { get; } = mouthPoint;
}
