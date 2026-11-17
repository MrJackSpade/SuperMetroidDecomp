namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Contact geometry and response definitions for Kraid's private AI.</summary>
internal static class KraidContactDefinitions
{
    /// <summary>Two pixels left of the lint hitbox's left edge.</summary>
    internal const int LintLeadingEdge = -26;
    /// <summary>Two pixels below the lint hitbox's top edge.</summary>
    internal const int LintTop = -2;
    /// <summary>Two pixels above the lint hitbox's bottom edge.</summary>
    internal const int LintBottom = 4;
    /// <summary>Added lint push width and signed upper clamp.</summary>
    internal const int LintPush = 16;
    /// <summary>The pixel beyond the lint's push distance that a pushed Samus's body clears it by.</summary>
    internal const int LintPushClearancePixels = 1;
    /// <summary>X threshold below which body contact does not subtract eight.</summary>
    internal const int MinimumBodyEjectionX = 40;
    /// <summary>Whole-pixel body ejection on both axes.</summary>
    internal const int BodyEjection = 8;
    /// <summary>The extra displacement, right and up, when Kraid's body or arm pushes Samus back.</summary>
    internal static readonly Offset16<RoomPixels> PushBack = new(4, -8);
}
