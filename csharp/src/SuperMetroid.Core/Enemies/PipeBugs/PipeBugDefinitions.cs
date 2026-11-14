namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The Norfair Pipe Bug formation's roles (#1840). Each member departs when its formation count reaches its target,
/// centred on the leader's 104, eight updates per place above or below it; the European release shifts the same targets
/// down 32 updates.
/// </summary>
internal static class PipeBugDefinitions
{
    /// <summary>The leader: it holds its height after rising and departs at 104.</summary>
    internal static readonly NorfairPipeBugRole LeaderRole = new(104, PipeBugAction.NorfairLeaderStagger);

    /// <summary>The member just above the leader: it rises 16 pixels further and departs at 96.</summary>
    internal static readonly NorfairPipeBugRole UpperNearRole = new(96, PipeBugAction.NorfairUpperNearStagger);

    /// <summary>The member farthest above the leader: it rises 32 pixels further and departs at 88.</summary>
    internal static readonly NorfairPipeBugRole UpperFarRole = new(88, PipeBugAction.NorfairUpperFarStagger);

    /// <summary>The member just below the leader: it sinks 16 pixels back and departs at 112.</summary>
    internal static readonly NorfairPipeBugRole LowerNearRole = new(112, PipeBugAction.NorfairLowerNearStagger);

    /// <summary>The member farthest below the leader: it sinks 32 pixels back and departs at 120.</summary>
    internal static readonly NorfairPipeBugRole LowerFarRole = new(120, PipeBugAction.NorfairLowerFarStagger);
}
