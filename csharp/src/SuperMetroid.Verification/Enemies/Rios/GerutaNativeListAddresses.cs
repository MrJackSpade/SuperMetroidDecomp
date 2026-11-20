using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>
/// The native bank-$A2 lists the Geruta ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class GerutaNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<GerutaSegment>[] Entries =
    [
        new NativeProgramEntry<GerutaSegment>(GerutaSegment.Idle, 0xc0f1),
        new NativeProgramEntry<GerutaSegment>(GerutaSegment.StartDescending, 0xc107),
        new NativeProgramEntry<GerutaSegment>(GerutaSegment.Descending, 0xc12f),
        new NativeProgramEntry<GerutaSegment>(GerutaSegment.StartAscending, 0xc145),
        new NativeProgramEntry<GerutaSegment>(GerutaSegment.Ascending, 0xc179),
        new NativeProgramEntry<GerutaSegment>(GerutaSegment.FlamesAscending, 0xc18f),
        new NativeProgramEntry<GerutaSegment>(GerutaSegment.FlamesDescending, 0xc1a3),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, GerutaNativeListAddressesInstruction> Instructions = new()
    {
        [0xc1c9] = new GerutaNativeListAddressesInstruction(new GerutaCue.SwoopStartFinished(), ""),
        [0xc1d4] = new GerutaNativeListAddressesInstruction(new GerutaCue.PlaceFlames(8, GerutaFlames.Ascending), ""),
        [0xc1df] = new GerutaNativeListAddressesInstruction(new GerutaCue.PlaceFlames(8, GerutaFlames.Ascending), ""),
        [0xc1ea] = new GerutaNativeListAddressesInstruction(new GerutaCue.PlaceFlames(12, GerutaFlames.Ascending), ""),
        [0xc1f5] = new GerutaNativeListAddressesInstruction(new GerutaCue.PlaceFlames(-12, GerutaFlames.Descending), ""),
        [0xc200] = new GerutaNativeListAddressesInstruction(new GerutaCue.PlaceFlames(4, GerutaFlames.Ascending), ""),
        [0xc20b] = new GerutaNativeListAddressesInstruction(new GerutaCue.PlaceFlames(0, GerutaFlames.Ascending), ""),
        [0xc216] = new GerutaNativeListAddressesInstruction(new GerutaCue.PlaceFlames(-4, GerutaFlames.Descending), ""),
        [0xc221] = new GerutaNativeListAddressesInstruction(new GerutaCue.PlaceFlames(-12, GerutaFlames.Descending), ""),
        [0xc22c] = new GerutaNativeListAddressesInstruction(new GerutaCue.PlaceFlames(-16, GerutaFlames.Descending), ""),
        [0xc237] = new GerutaNativeListAddressesInstruction(new GerutaCue.PlaceFlames(12, GerutaFlames.Ascending), ""),
    };
}
