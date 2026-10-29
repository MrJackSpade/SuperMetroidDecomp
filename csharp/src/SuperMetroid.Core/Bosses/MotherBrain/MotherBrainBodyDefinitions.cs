namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>How much of Mother Brain's staged body her second phase shows, and how much of it her death clears (#1466).</summary>
internal static class MotherBrainBodyDefinitions
{
    /// <summary>The cells of her staged body her phase-two transition shows: its first page, the whole of what her room shows.</summary>
    internal const int ShownCells = 1024;

    /// <summary>The cells her death clears from the start of her body once its fade ends.</summary>
    internal const int DeathClearedCells = 356;
}
