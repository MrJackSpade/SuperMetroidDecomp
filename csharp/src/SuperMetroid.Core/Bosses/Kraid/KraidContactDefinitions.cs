namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Contact geometry and response definitions for Kraid's private AI.</summary>
internal static class KraidContactDefinitions
{
    /// <summary>The lint's leading edge, just left of its hitbox's left edge.</summary>
    internal const int LintLeadingEdge = -26;
    /// <summary>The lint's top, just below its hitbox's top edge.</summary>
    internal const int LintTop = -2;
    /// <summary>The lint's bottom, just above its hitbox's bottom edge.</summary>
    internal const int LintBottom = 4;
    /// <summary>How far past Samus's edge a lint pushes her, and the farthest right its push may leave her carried.</summary>
    internal const int LintPush = 16;
    /// <summary>The pixel beyond the lint's push distance that a pushed Samus's body clears it by.</summary>
    internal const int LintPushClearancePixels = 1;
    /// <summary>The room X from which Kraid's body pushes Samus back left as it touches her; nearer the wall it only pushes her up.</summary>
    internal const int MinimumBodyEjectionX = 40;
    /// <summary>Whole-pixel body ejection on both axes.</summary>
    internal const int BodyEjection = 8;
    /// <summary>The extra displacement, right and up, when Kraid's body or arm pushes Samus back.</summary>
    internal static readonly Offset16<RoomPixels> PushBack = new(4, -8);
}
