namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Contact geometry and response definitions for Kraid's private AI.</summary>
internal static class KraidContactDefinitions
{
    /// <summary>The lint hitbox's left edge (-24) minus two.</summary>
    internal const int LintLeadingEdge = -26;
    /// <summary>The lint hitbox's top edge (-4) plus two.</summary>
    internal const int LintTop = -2;
    /// <summary>The lint hitbox's bottom edge (6) minus two.</summary>
    internal const int LintBottom = 4;
    /// <summary>Added lint push width and signed upper clamp.</summary>
    internal const int LintPush = 16;
    /// <summary>The pixel beyond the lint's push distance that a pushed Samus's body clears it by.</summary>
    internal const int LintPushClearancePixels = 1;
    /// <summary>X threshold below which body contact does not subtract eight.</summary>
    internal const int MinimumBodyEjectionX = 40;
    /// <summary>Whole-pixel body ejection on both axes.</summary>
    internal const int BodyEjection = 8;
    /// <summary>The extra horizontal displacement when Kraid pushes Samus back.</summary>
    internal const int ExtraX = 4;
    /// <summary>The extra vertical displacement when Kraid pushes Samus back.</summary>
    internal const int ExtraY = -8;
}
