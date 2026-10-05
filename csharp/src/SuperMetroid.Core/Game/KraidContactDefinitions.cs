namespace SuperMetroid.Core.Game;

/// <summary>Native contact geometry and response definitions for Kraid's private AI.</summary>
internal static class KraidContactDefinitions
{
    /// <summary>$A7:B97C-B983: Hitbox_KraidLint.left ($92B7, -24) minus two.</summary>
    internal const short LintLeadingEdge = -26;
    /// <summary>$A7:B99F-B9A6: Hitbox_KraidLint.top ($92B9, -4) plus two.</summary>
    internal const short LintTop = -2;
    /// <summary>$A7:B9B7-B9BE: Hitbox_KraidLint.bottom ($92BD, 6) minus two.</summary>
    internal const short LintBottom = 4;
    /// <summary>$A7:B9CF-B9DE: added lint push width and signed upper clamp.</summary>
    internal const ushort LintPush = 16;
    /// <summary>$A7:B12D: X threshold below which body contact does not subtract eight.</summary>
    internal const ushort MinimumBodyEjectionX = 40;
    /// <summary>$A7:B133/B140: whole-pixel body ejection on both axes.</summary>
    internal const ushort BodyEjection = 8;
    /// <summary>$A7:94A4: PushSamusBack horizontal extra displacement.</summary>
    internal const ushort ExtraX = 4;
    /// <summary>$A7:94AA: PushSamusBack vertical extra displacement, signed -8.</summary>
    internal const ushort ExtraY = 0xfff8;
}
