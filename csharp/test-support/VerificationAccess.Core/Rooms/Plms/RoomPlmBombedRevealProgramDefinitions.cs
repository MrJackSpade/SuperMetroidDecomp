namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Native one-frame reveal/delete lists $84:C8EC-C902 and $84:C91C-C926.
/// Though two lists are named unused in the disassembly, bomb-reaction setup
/// explicitly selects them to reveal a gated parent without breaking it.
/// </summary>
internal static class RoomPlmBombedRevealProgramDefinitions
{
    /// <summary>The six supported one-frame lists.</summary>
    private enum Reveal
    {
        /// <summary>One-block crumble reveal at $84:C8EC using the single-cell draw command.</summary>
        Crumble1x1,
        /// <summary>Two-column crumble reveal using the horizontal restore draw.</summary>
        Crumble2x1,
        /// <summary>Two-row crumble reveal using the vertical restore draw.</summary>
        Crumble1x2,
        /// <summary>Two-by-two crumble reveal using the square restore draw.</summary>
        Crumble2x2,
        /// <summary>One-frame reveal of a power-bomb-gated parent without breaking it.</summary>
        PowerBomb,
        /// <summary>One-frame reveal of a super-missile-gated parent without breaking it.</summary>
        SuperMissile,
    }

    /// <summary>Bytes in each list: a duration/draw pair followed by deletion.</summary>
    private const int ListLength = 6;

    /// <summary>Maps a supported reveal variant to its native bank-$84 list pointer.</summary>
    private static ushort StartOf(Reveal reveal) => reveal switch
    {
        Reveal.Crumble1x1 => RoomPlmInstructionListsConstants.CrumbleReveal1x1,
        Reveal.Crumble2x1 => RoomPlmInstructionListsConstants.CrumbleReveal2x1,
        Reveal.Crumble1x2 => RoomPlmInstructionListsConstants.CrumbleReveal1x2,
        Reveal.Crumble2x2 => RoomPlmInstructionListsConstants.CrumbleReveal2x2,
        Reveal.PowerBomb => RoomPlmInstructionListsConstants.BombedPowerBombBlockUnused,
        Reveal.SuperMissile => RoomPlmInstructionListsConstants.BombedSuperMissileBlockUnused,
        _ => throw new InvalidOperationException($"Undefined bombed-reveal list {reveal}."),
    };

    /// <summary>Selects the draw command for the reveal list's single display frame.</summary>
    private static ushort DrawOf(Reveal reveal) => reveal switch
    {
        Reveal.Crumble1x1 => (ushort)RoomPlmBombedRevealDraw.CrumbleSingle,
        Reveal.Crumble2x1 => (ushort)ContactCrumbleRestoreDraw.Horizontal,
        Reveal.Crumble1x2 => (ushort)ContactCrumbleRestoreDraw.Vertical,
        Reveal.Crumble2x2 => (ushort)ContactCrumbleRestoreDraw.Square,
        Reveal.PowerBomb => (ushort)RoomPlmBombedRevealDraw.PowerBomb,
        Reveal.SuperMissile => (ushort)RoomPlmBombedRevealDraw.SuperMissile,
        _ => throw new InvalidOperationException($"Undefined bombed-reveal list {reveal}."),
    };

    /// <summary>Reads a bounded one-frame reveal list while excluding intervening unowned lists.</summary>
    internal static bool TryReadWord(ushort address, out ushort value)
    {
        // The intervening four unused bomb-reveal lists are outside this owner.
        foreach (Reveal reveal in Enum.GetValues<Reveal>())
        {
            int offset = address - StartOf(reveal);
            if ((uint)offset >= ListLength || (offset & 1) != 0)
                continue;
            value = offset switch
            {
                0 => 1,
                2 => DrawOf(reveal),
                _ => (ushort)RoomPlmInstruction.Delete,
            };
            return true;
        }
        value = 0;
        return false;
    }
}
