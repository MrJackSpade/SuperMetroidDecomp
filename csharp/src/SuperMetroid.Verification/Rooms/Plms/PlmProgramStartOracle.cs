using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// Relates <see cref="PlmProgramStart"/> values to the native bank-$84 lists they replace (#1296): the shared delete
/// list became a typed program, every other list still runs natively.
/// </summary>
internal static class PlmProgramStartOracle
{
    /// <summary>The shared delete list's native entry (<c>InstList_PLM_Delete</c>).</summary>
    internal static ushort SharedDeleteEntry { get; } =
        RoomPlmNativeListAddresses.SharedEntries.Single(entry => entry.Program == RoomPlmSharedProgram.Delete).Entry;

    /// <summary>The start production must hold where the cartridge names native list <paramref name="list"/>.</summary>
    internal static PlmProgramStart ForNativeList(ushort list) =>
        list == SharedDeleteEntry ? PlmProgramStart.Delete : PlmProgramStart.Native(list);

    /// <summary>The native list <paramref name="start"/> stands for.</summary>
    internal static ushort NativeListOf(PlmProgramStart start) =>
        start == PlmProgramStart.Delete ? SharedDeleteEntry : start.NativeList;

    /// <summary>Whether <paramref name="slot"/> runs <paramref name="start"/> from its beginning.</summary>
    internal static bool IsRunning(PlmProgramStart start, RoomPlmSlotSnapshot slot) =>
        start.SharedProgram is { } program
            ? slot.Animation is RoomPlmSharedAnimation { Program: var running } && running == program
            : slot.Animation is null && slot.InstructionPointer == start.NativeList;
}
