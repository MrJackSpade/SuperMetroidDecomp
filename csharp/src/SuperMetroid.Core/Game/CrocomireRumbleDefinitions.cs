namespace SuperMetroid.Core.Game;

/// <summary>
/// One target in Crocomire's hidden-wall rumble schedule. Negative targets carry the
/// cooldown and approach delta loaded when their oscillation completes.
/// </summary>
internal readonly record struct CrocomireRumbleDefinition(
    ushort TableOffset,
    short TargetYOffset,
    ushort NextTargetOffset,
    ushort Cooldown,
    ushort Delta,
    bool HasTiming,
    bool IsTerminator);

/// <summary>Fixed hidden-wall rumble schedule from <c>$A4:98CA-$A4:9909</c>.</summary>
internal static class CrocomireRumbleDefinitions
{
    private static readonly CrocomireRumbleDefinition[] Definitions =
    [
        new(0x00, 4, 0x02, 0, 0, HasTiming: false, IsTerminator: false),
        new(0x02, 1, 0x04, 0, 0, HasTiming: false, IsTerminator: false),
        new(0x04, 0, 0x06, 0, 0, HasTiming: false, IsTerminator: false),
        new(0x06, -1, 0x0c, 8, 1, HasTiming: true, IsTerminator: false),
        new(0x0c, 1, 0x0e, 0, 0, HasTiming: false, IsTerminator: false),
        new(0x0e, -1, 0x14, 12, 1, HasTiming: true, IsTerminator: false),
        new(0x14, 1, 0x16, 0, 0, HasTiming: false, IsTerminator: false),
        new(0x16, -2, 0x1c, 16, 2, HasTiming: true, IsTerminator: false),
        new(0x1c, 2, 0x1e, 0, 0, HasTiming: false, IsTerminator: false),
        new(0x1e, -2, 0x24, 16, 2, HasTiming: true, IsTerminator: false),
        new(0x24, 2, 0x26, 0, 0, HasTiming: false, IsTerminator: false),
        new(0x26, -4, 0x2c, 8, 1, HasTiming: true, IsTerminator: false),
        new(0x2c, 1, 0x2e, 0, 0, HasTiming: false, IsTerminator: false),
        new(0x2e, -2, 0x34, 3, 1, HasTiming: true, IsTerminator: false),
        new(0x34, 1, 0x36, 0, 0, HasTiming: false, IsTerminator: false),
        new(0x36, -1, 0x3c, 0x8080, 0x8080, HasTiming: true, IsTerminator: false),
        new(0x3c, unchecked((short)0x8080), 0x80, 0, 0,
            HasTiming: false, IsTerminator: true),
        // The cartridge includes a duplicate terminator word immediately after the
        // reachable one. Retain it for restored/debugger states that name offset $3E.
        new(0x3e, unchecked((short)0x8080), 0x80, 0, 0,
            HasTiming: false, IsTerminator: true),
    ];

    /// <summary>All authored target records, including the trailing terminator copy.</summary>
    internal static ReadOnlySpan<CrocomireRumbleDefinition> All => Definitions;

    /// <summary>Returns the target record at an authored even byte offset.</summary>
    internal static CrocomireRumbleDefinition AtOffset(ushort tableOffset) => tableOffset switch
    {
        0x00 => Definitions[0],
        0x02 => Definitions[1],
        0x04 => Definitions[2],
        0x06 => Definitions[3],
        0x0c => Definitions[4],
        0x0e => Definitions[5],
        0x14 => Definitions[6],
        0x16 => Definitions[7],
        0x1c => Definitions[8],
        0x1e => Definitions[9],
        0x24 => Definitions[10],
        0x26 => Definitions[11],
        0x2c => Definitions[12],
        0x2e => Definitions[13],
        0x34 => Definitions[14],
        0x36 => Definitions[15],
        0x3c => Definitions[16],
        0x3e => Definitions[17],
        _ => throw new InvalidDataException(
            $"Crocomire rumble offset ${tableOffset:X4} is not an authored target word."),
    };
}
