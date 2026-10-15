using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// The native bank-$84 lists of the room PLM programs typed by #1296, for the ROM oracles that walk them beside the
/// typed programs. Only verification names these lists.
/// </summary>
internal static class RoomPlmNativeListAddresses
{
    /// <summary>The n00b tube's lists (<c>InstList_PLM_NoobTube_0..4</c>, $84:D4D4..D524).</summary>
    internal static IReadOnlyList<(NoobTubeProgram Program, ushort Entry)> NoobTubeEntries { get; } =
    [
        (NoobTubeProgram.Main, 0xd4d4),
        (NoobTubeProgram.WaitForInput, 0xd4e8),
        (NoobTubeProgram.Break, 0xd4f2),
        (NoobTubeProgram.AlreadyBroken, 0xd521),
    ];

    /// <summary>The Brinstar plants' lists (<c>InstList_PLM_BrinstarFloorPlant_0..1</c>, <c>..CeilingPlant_0..1</c>, $84:ACB8..AD37).</summary>
    internal static IReadOnlyList<(SamusEaterProgram Program, ushort Entry)> SamusEaterEntries { get; } =
    [
        (SamusEaterProgram.FloorBite, 0xacb8),
        (SamusEaterProgram.FloorChew, 0xacbf),
        (SamusEaterProgram.CeilingBite, 0xacf8),
        (SamusEaterProgram.CeilingChew, 0xacff),
    ];

    /// <summary>The shared one-instruction lists (<c>InstList_PLM_Delete</c> $84:AAE3, the metroids-cleared sleep $84:DB42).</summary>
    internal static IReadOnlyList<(RoomPlmSharedProgram Program, ushort Entry)> SharedEntries { get; } =
    [
        (RoomPlmSharedProgram.Delete, 0xaae3),
        (RoomPlmSharedProgram.Sleep, 0xdb42),
    ];

    /// <summary>The Speed Booster lavaquake's list (<c>InstList_PLM_SpeedBoosterEscape</c>, $84:B88A), one phase per sleep.</summary>
    internal static IReadOnlyList<(SpeedBoosterEscapeProgram Program, ushort Entry)> SpeedBoosterEscapeEntries { get; } =
    [
        (SpeedBoosterEscapeProgram.WaitForSpeedBooster, 0xb88a),
        (SpeedBoosterEscapeProgram.WaitForSamusLeft, 0xb890),
        (SpeedBoosterEscapeProgram.AdvanceLava, 0xb896),
    ];

    /// <summary>The lavaquake list's length in words: three install-and-sleep phases.</summary>
    internal const int SpeedBoosterEscapeWordCount = 9;
}
