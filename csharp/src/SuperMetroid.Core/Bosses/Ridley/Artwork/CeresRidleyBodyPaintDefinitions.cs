namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The channel ceiling of Ceres Ridley's body paint; <see cref="CeresRidleyBodyPaint"/> calculates the paint.</summary>
internal static class CeresRidleyBodyPaintDefinitions
{
    /// <summary>The five-bit channel ceiling used for the membrane's saturated red.</summary>
    internal const int MaximumChannel = (1 << 5) - 1;
}
