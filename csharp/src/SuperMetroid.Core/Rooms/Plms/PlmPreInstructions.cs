using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The shared inert pre-instruction, and the routines a bank-$84 list installs through <c>$84:86C1</c>. Every other
/// pre-instruction is installed by name where its family's program or setup installs it.
/// </summary>
internal static class PlmPreInstructions
{
    /// <summary>The return every spawn installs and <c>$84:86CA</c> restores: no pre-instruction work.</summary>
    internal static PlmPreInstruction Inert { get; } = PlmPreInstruction.NoOp("inert", PlmPreInstructionDefinitions.SpawnDefault);

    /// <summary>The routines a list installs through <c>$84:86C1</c>, by operand.</summary>
    private static readonly Dictionary<ushort, IPlmPreInstruction> ListInstalled = Index(
        ShaktoolRoomPreInstructions.Controller,
        TourianPreInstructions.OldTourianEscapeShaftWallWaitForSamus,
        ChozoStatuePreInstructions.WaitForLowerNorfairHand);

    /// <summary>Converts a <c>$84:86C1</c> operand to its routine, where the list instruction is decoded.</summary>
    /// <exception cref="InvalidDataException">The operand names no translated list-installed routine.</exception>
    internal static IPlmPreInstruction FromListOperand(ushort operand) =>
        ListInstalled.TryGetValue(operand, out IPlmPreInstruction? routine)
            ? routine
            : throw new InvalidDataException($"A PLM list installs untranslated pre-instruction $84:{operand:X4}.");

    /// <summary>Indexes <paramref name="routines"/> by native address, rejecting an address registered twice.</summary>
    /// <exception cref="InvalidOperationException">Two routines share an address.</exception>
    private static Dictionary<ushort, IPlmPreInstruction> Index(params IPlmPreInstruction[] routines)
    {
        var index = new Dictionary<ushort, IPlmPreInstruction>();
        foreach (IPlmPreInstruction routine in routines)
        {
            if (!index.TryAdd(routine.NativeRoutine, routine))
                throw new InvalidOperationException($"PLM pre-instruction $84:{routine.NativeRoutine:X4} registered twice.");
        }
        return index;
    }
}
