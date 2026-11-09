using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Relates <see cref="RoomObjectProgramStart"/> values to the native bank-$84 lists they replace (#1296).</summary>
internal static class RoomObjectProgramStartOracle
{
    /// <summary>The shared delete list's native entry (<c>InstList_PLM_Delete</c>).</summary>
    internal static ushort SharedDeleteEntry { get; } =
        RoomObjectNativeListAddresses.SharedEntries.Single(entry => entry.Program == RoomObjectSharedProgram.Delete).Entry;

    /// <summary>Every typed start a native list can stand for, with that list.</summary>
    private static IReadOnlyList<RoomObjectProgramStartOracleTypedStart> TypedStarts { get; } =
    [
        new RoomObjectProgramStartOracleTypedStart(RoomObjectProgramStart.Delete, SharedDeleteEntry),
        .. RoomObjectNativeListAddresses.SpeedBoosterBlockEntries.Select(entry => new RoomObjectProgramStartOracleTypedStart(RoomObjectProgramStart.Typed(entry.Program), entry.Entry)),
        .. RoomObjectNativeListAddresses.BlockReactionEntries.Select(entry => new RoomObjectProgramStartOracleTypedStart(RoomObjectProgramStart.Typed(entry.Program), entry.Entry)),
        .. RoomObjectNativeListAddresses.ScriptedRoomEntries.Select(entry => new RoomObjectProgramStartOracleTypedStart(RoomObjectProgramStart.Typed(entry.Program), entry.Entry)),
    ];

    /// <summary>The start production must hold where the cartridge names native list <paramref name="list"/>.</summary>
    /// <exception cref="InvalidOperationException">No typed program stands for <paramref name="list"/>.</exception>
    internal static RoomObjectProgramStart ForNativeList(ushort list) =>
        TypedStarts.Where(typed => typed.Entry == list).Select(typed => (RoomObjectProgramStart?)typed.Start).SingleOrDefault()
            ?? throw new InvalidOperationException($"No typed PLM program stands for native list ${list:X4}.");

    /// <summary>The native list <paramref name="start"/> stands for.</summary>
    internal static ushort NativeListOf(RoomObjectProgramStart start) => TypedStarts.Single(typed => typed.Start == start).Entry;

    /// <summary>Whether <paramref name="slot"/> runs <paramref name="start"/>.</summary>
    internal static bool IsRunning(RoomObjectProgramStart start, RoomObjectSlotSnapshot slot) =>
        slot.Animation is { } animation && animation.RunningProgram.Equals(start.TypedProgram);
}
