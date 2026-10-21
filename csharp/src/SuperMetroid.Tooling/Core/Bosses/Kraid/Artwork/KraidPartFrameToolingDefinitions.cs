
namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Development-tool members of <see cref="KraidPartFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class KraidPartFrameToolingDefinitions
{
    /// <summary>$A7:A565: the foot's standing pose.</summary>
    internal const ushort FootStanding = 0xa565;
    /// <summary>$A7:8CE3: the first of 33 eighteen-byte foot roots.</summary>
    internal const ushort FootStart = 0x8ce3;
    /// <summary>$A7:8F47: the last foot pose, one unused root past the run.</summary>
    internal const ushort FootLast = 0x8f47;
    /// <summary>The foot poses in the eighteen-byte run.</summary>
    internal const int FootRunCount = 33;
    /// <summary>$A7:8F59: the first of 21 forty-two-byte arm roots.</summary>
    internal const ushort ArmStart = 0x8f59;
    /// <summary>$A7:92AB: the last arm pose, outside the forty-two-byte run.</summary>
    internal const ushort ArmLast = 0x92ab;
    /// <summary>The arm poses in the forty-two-byte run.</summary>
    internal const int ArmRunCount = 21;
    /// <summary>$A7:A5DF: the initial belly-lint pose.</summary>
    internal const ushort LintInitial = 0xa5df;
    /// <summary>$A7:8C6C: the belly-lint pose after Kraid grows.</summary>
    internal const ushort LintPostGrowth = 0x8c6c;
    /// <summary>$A7:A617: the first fingernail pose; poses alternate twelve- and twenty-two-byte roots.</summary>
    internal const ushort NailStart = 0xa617;
    /// <summary>The fingernail poses.</summary>
    internal const int NailCount = 8;
    /// <summary>The installed extended spritemap of <paramref name="frame"/>.</summary>
    internal static ushort Spritemap(KraidPartFrame frame) => frame switch
    {
        { Kind: KraidPartFrameKind.FootStanding, Index: 0 } => FootStanding,
        { Kind: KraidPartFrameKind.Foot, Index: >= 0 and < FootRunCount } => unchecked((ushort)(FootStart + 18 * frame.Index)),
        { Kind: KraidPartFrameKind.Foot, Index: FootRunCount } => FootLast,
        { Kind: KraidPartFrameKind.Arm, Index: >= 0 and < ArmRunCount } => unchecked((ushort)(ArmStart + 42 * frame.Index)),
        { Kind: KraidPartFrameKind.Arm, Index: ArmRunCount } => ArmLast,
        { Kind: KraidPartFrameKind.Lint, Index: 0 } => LintInitial,
        { Kind: KraidPartFrameKind.Lint, Index: 1 } => LintPostGrowth,
        { Kind: KraidPartFrameKind.Nail, Index: >= 0 and < NailCount } =>
            unchecked((ushort)(NailStart + 34 * (frame.Index / 2) + 12 * (frame.Index % 2))),
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kraid part frame."),
    };
}
