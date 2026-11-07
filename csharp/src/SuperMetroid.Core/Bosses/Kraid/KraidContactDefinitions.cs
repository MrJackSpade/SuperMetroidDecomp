namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Contact geometry and response definitions for Kraid's private AI.</summary>
internal static class KraidContactDefinitions
{
    /// <summary>
    /// The mouth's upper bound the next shot is tested against after a shot hits Kraid's mouth: 3750 pixels above the
    /// room's top, so every following shot passes the upper test. The common shot callback leaves its vulnerability
    /// lookup over the bound, and the next shot's signed compare reads that leftover as this coordinate (#1413).
    /// </summary>
    internal const int ShotUpperBoundAfterHit = -3750;

    /// <summary>
    /// The mouth's left bound the next shot is tested against after a rejected shot spawns its dud flash: six pixels
    /// from the room's left. Spawning the flash leaves its object selector over the bound, and the next shot's compare
    /// reads that leftover as this coordinate.
    /// </summary>
    internal const int ShotLeftBoundAfterDud = 6;

    /// <summary>The lint hitbox's left edge (-24) minus two.</summary>
    internal const int LintLeadingEdge = -26;
    /// <summary>The lint hitbox's top edge (-4) plus two.</summary>
    internal const int LintTop = -2;
    /// <summary>The lint hitbox's bottom edge (6) minus two.</summary>
    internal const int LintBottom = 4;
    /// <summary>Added lint push width and signed upper clamp.</summary>
    internal const int LintPush = 16;
    /// <summary>X threshold below which body contact does not subtract eight.</summary>
    internal const int MinimumBodyEjectionX = 40;
    /// <summary>Whole-pixel body ejection on both axes.</summary>
    internal const int BodyEjection = 8;
    /// <summary>PushSamusBack horizontal extra displacement.</summary>
    internal const int ExtraX = 4;
    /// <summary>PushSamusBack vertical extra displacement.</summary>
    internal const int ExtraY = -8;
}
