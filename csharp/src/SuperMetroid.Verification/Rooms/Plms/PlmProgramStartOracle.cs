using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Verification.Rooms.Doors;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// Relates <see cref="PlmProgramStart"/> values to the native bank-$84 lists they replace (#1296): a list whose program
/// is typed starts that program, every other list still runs natively.
/// </summary>
internal static class PlmProgramStartOracle
{
    /// <summary>The shared delete list's native entry (<c>InstList_PLM_Delete</c>).</summary>
    internal static ushort SharedDeleteEntry { get; } =
        RoomPlmNativeListAddresses.SharedEntries.Single(entry => entry.Program == RoomPlmSharedProgram.Delete).Entry;

    /// <summary>Every typed start a native list can stand for, with that list.</summary>
    private static IReadOnlyList<(PlmProgramStart Start, ushort Entry)> TypedStarts { get; } =
    [
        (PlmProgramStart.Delete, SharedDeleteEntry),
        .. RoomPlmNativeListAddresses.SpeedBoosterBlockEntries.Select(entry => (PlmProgramStart.Typed(entry.Program), entry.Entry)),
        .. RoomPlmNativeListAddresses.BlockReactionEntries.Select(entry => (PlmProgramStart.Typed(entry.Program), entry.Entry)),
        .. RoomPlmNativeListAddresses.ScriptedRoomEntries.Select(entry => (PlmProgramStart.Typed(entry.Program), entry.Entry)),
        .. DoorPlmNativeListAddresses.BlueEntries.Select(entry => (PlmProgramStart.Typed(entry.Program), entry.Entry)),
    ];

    /// <summary>The start production must hold where the cartridge names native list <paramref name="list"/>.</summary>
    internal static PlmProgramStart ForNativeList(ushort list) =>
        TypedStarts.Where(typed => typed.Entry == list).Select(typed => (PlmProgramStart?)typed.Start).SingleOrDefault()
            ?? PlmProgramStart.Native(list);

    /// <summary>The native list <paramref name="start"/> stands for.</summary>
    internal static ushort NativeListOf(PlmProgramStart start) =>
        start.TypedProgram is null
            ? start.NativeList
            : TypedStarts.Single(typed => typed.Start == start).Entry;

    /// <summary>Whether <paramref name="slot"/> runs <paramref name="start"/>.</summary>
    internal static bool IsRunning(PlmProgramStart start, RoomPlmSlotSnapshot slot) =>
        start.TypedProgram is { } program
            ? slot.Animation is { } animation && animation.RunningProgram.Equals(program)
            : slot.Animation is null && slot.InstructionPointer == start.NativeList;
}
