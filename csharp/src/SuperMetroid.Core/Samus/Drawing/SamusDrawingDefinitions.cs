namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>The definitions drawing Samus reads: the arm cannon's cover directions (#1411).</summary>
/// <remarks>
/// This catalog owns rendering data only. Animation state, the drawn body picture,
/// sprite positions, and frame counters remain on their runtime state objects. Where the body is placed is baked into its
/// installed frames by the import (#1517).
/// </remarks>
public static class SamusDrawingDefinitions
{
    /// <summary>The arm-cannon cover's aim directions.</summary>
    public static class ArmCannon
    {
        /// <summary>Number of direction selectors accepted by the dispatcher.</summary>
        public const int DirectionCount = 10;
    }
}
