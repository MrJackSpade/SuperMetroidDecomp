using static SuperMetroid.Core.Frontend.Intro.IntroBabyDiscoveryCollisionDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Builds the intro baby-discovery room's collision (<see cref="IntroBabyDiscoveryCollisionDefinitions"/>) from its two
/// horizontal strips.
/// </summary>
internal static class IntroBabyDiscoveryCollision
{
    /// <summary>Construct mutable room state directly from its two horizontal collision strips.</summary>
    internal static ushort[] CreateForeground()
    {
        var foreground = new ushort[Columns * RoomRows];
        foreground.AsSpan(ZeroRows * Columns, Columns).Fill(PenultimateRowWord);
        foreground.AsSpan((SourceRows - 1) * Columns, Columns).Fill(FinalRowWord);
        return foreground;
    }
}
