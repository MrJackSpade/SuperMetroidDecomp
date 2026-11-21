namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>Each of the Boulder's rolls (<see cref="BoulderRoll"/>).</summary>
internal static class BoulderRollDefinitions
{
    /// <summary>Each roll: rightward, leftward, and leftward breaking on its first impact.</summary>
    internal static readonly IdentityTable<BoulderRoll, BoulderRollDefinition> Rolls = new(new Dictionary<BoulderRoll, BoulderRollDefinition>
    {
        [BoulderRoll.Right] = new(HorizontalDirection.Right, BoulderSegment.Right, BreaksOnImpact: false),
        [BoulderRoll.Left] = new(HorizontalDirection.Left, BoulderSegment.Left, BreaksOnImpact: false),
        [BoulderRoll.LeftBreakingOnImpact] = new(HorizontalDirection.Left, BoulderSegment.Left, BreaksOnImpact: true),
    });
}
