namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Native one-frame reveal/delete lists $84:C8EC-C902 and $84:C91C-C926.
/// Though two lists are named unused in the disassembly, bomb-reaction setup
/// explicitly selects them to reveal a gated parent without breaking it.
/// </summary>
internal static class RoomPlmBombedRevealProgramDefinitions
{
    internal static bool TryReadWord(ushort address, out ushort value)
    {
        // Each supported list is one duration/draw pair followed by deletion.
        // The intervening four unused bomb-reveal lists are outside this owner.
        int relative = address - RoomPlmInstructionLists.CrumbleReveal1x1;
        if ((uint)relative >= 4 * 6)
        {
            relative = address - RoomPlmInstructionLists.BombedPowerBombBlockUnused;
            if ((uint)relative >= 2 * 6)
            {
                value = 0;
                return false;
            }
        }
        int offset = relative % 6;
        if ((offset & 1) != 0)
        {
            value = 0;
            return false;
        }
        ushort start = (ushort)(address - offset);
        value = offset switch
        {
            0 => 1,
            4 => RoomPlmInstructionCodes.Delete,
            _ => start switch
            {
                RoomPlmInstructionLists.CrumbleReveal1x1 => RoomPlmBombedRevealDrawDefinitions.CrumbleSingle,
                RoomPlmInstructionLists.CrumbleReveal2x1 => RoomPlmContactCrumbleRestoreDrawDefinitions.Horizontal,
                RoomPlmInstructionLists.CrumbleReveal1x2 => RoomPlmContactCrumbleRestoreDrawDefinitions.Vertical,
                RoomPlmInstructionLists.CrumbleReveal2x2 => RoomPlmContactCrumbleRestoreDrawDefinitions.Square,
                RoomPlmInstructionLists.BombedPowerBombBlockUnused => RoomPlmBombedRevealDrawDefinitions.PowerBomb,
                RoomPlmInstructionLists.BombedSuperMissileBlockUnused => RoomPlmBombedRevealDrawDefinitions.SuperMissile,
                _ => throw new InvalidOperationException("Reveal list bounds admitted an unknown program."),
            },
        };
        return true;
    }
}
