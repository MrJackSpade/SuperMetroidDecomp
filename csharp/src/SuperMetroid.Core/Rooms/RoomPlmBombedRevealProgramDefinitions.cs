namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Native one-frame reveal/delete lists $84:C8EC-C902 and $84:C91C-C926.
/// Though two lists are named unused in the disassembly, bomb-reaction setup
/// explicitly selects them to reveal a gated parent without breaking it.
/// </summary>
internal static class RoomPlmBombedRevealProgramDefinitions
{
    private static readonly (ushort Start, ushort Draw)[] Programs =
    [
        (RoomPlmInstructionLists.CrumbleReveal1x1, RoomPlmBombedRevealDrawDefinitions.CrumbleSingle),
        (RoomPlmInstructionLists.CrumbleReveal2x1, RoomPlmContactCrumbleRestoreDrawDefinitions.Horizontal),
        (RoomPlmInstructionLists.CrumbleReveal1x2, RoomPlmContactCrumbleRestoreDrawDefinitions.Vertical),
        (RoomPlmInstructionLists.CrumbleReveal2x2, RoomPlmContactCrumbleRestoreDrawDefinitions.Square),
        (RoomPlmInstructionLists.BombedPowerBombBlockUnused, RoomPlmBombedRevealDrawDefinitions.PowerBomb),
        (RoomPlmInstructionLists.BombedSuperMissileBlockUnused, RoomPlmBombedRevealDrawDefinitions.SuperMissile),
    ];

    internal static bool TryReadWord(ushort address, out ushort value)
    {
        foreach ((ushort start, ushort draw) in Programs)
        {
            int offset = address - start;
            if (offset is not (0 or 2 or 4)) continue;
            value = offset switch { 0 => 1, 2 => draw, _ => RoomPlmInstructionCodes.Delete };
            return true;
        }
        value = 0;
        return false;
    }
}
