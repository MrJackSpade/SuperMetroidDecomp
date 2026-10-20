namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>Cartridge values and helpers of <see cref="SporeSpawnFrameDefinitions"/> that only verification reads.</summary>
internal static class SporeSpawnFrameDefinitionsConstants
{
    /// <summary>$A5:EE65: the hardened husk.</summary>
    internal const ushort Dead = 0xee65;
    /// <summary>$A5:EE6F: the closed head.</summary>
    internal const ushort Closed = 0xee6f;
    /// <summary>$A5:EE79: the first of seven opening poses.</summary>
    internal const ushort FirstOpening = 0xee79;
    /// <summary>$A5:EF3D: the first of three fully open poses.</summary>
    internal const ushort FirstOpen = 0xef3d;
    /// <summary>The size of one opening or open root.</summary>
    internal const int RootBytes = 18;
    /// <summary>The opening poses.</summary>
    internal const int OpeningPoseCount = 7;
    /// <summary>The fully open poses.</summary>
    internal const int OpenPoseCount = 3;
    /// <summary>The extended spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(SporeSpawnFrame frame) => frame switch
    {
        { Kind: SporeSpawnFrameKind.Dead, Pose: 0 } => Dead,
        { Kind: SporeSpawnFrameKind.Closed, Pose: 0 } => Closed,
        { Kind: SporeSpawnFrameKind.Opening, Pose: >= 0 and < OpeningPoseCount } => (ushort)(FirstOpening + RootBytes * frame.Pose),
        { Kind: SporeSpawnFrameKind.Open, Pose: >= 0 and < OpenPoseCount } => (ushort)(FirstOpen + RootBytes * frame.Pose),
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Spore Spawn frame."),
    };
}
