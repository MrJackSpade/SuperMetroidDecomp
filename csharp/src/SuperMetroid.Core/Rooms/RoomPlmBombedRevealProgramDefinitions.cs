namespace SuperMetroid.Core.Rooms;

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
        /// <summary>Identifies crumble1x1.</summary>
        Crumble1x1,
        /// <summary>Identifies crumble2x1.</summary>
        Crumble2x1,
        /// <summary>Identifies crumble1x2.</summary>
        Crumble1x2,
        /// <summary>Identifies crumble2x2.</summary>
        Crumble2x2,
        /// <summary>Identifies power bomb.</summary>
        PowerBomb,
        /// <summary>Identifies super missile.</summary>
        SuperMissile,
    }

    /// <summary>Bytes in each list: a duration/draw pair followed by deletion.</summary>
    private const int ListLength = 6;

    /// <summary>Starts of.</summary>
    private static ushort StartOf(Reveal reveal) => reveal switch
    {
        Reveal.Crumble1x1 => RoomPlmInstructionLists.CrumbleReveal1x1,
        Reveal.Crumble2x1 => RoomPlmInstructionLists.CrumbleReveal2x1,
        Reveal.Crumble1x2 => RoomPlmInstructionLists.CrumbleReveal1x2,
        Reveal.Crumble2x2 => RoomPlmInstructionLists.CrumbleReveal2x2,
        Reveal.PowerBomb => RoomPlmInstructionLists.BombedPowerBombBlockUnused,
        Reveal.SuperMissile => RoomPlmInstructionLists.BombedSuperMissileBlockUnused,
        _ => throw new InvalidOperationException($"Undefined bombed-reveal list {reveal}."),
    };

    /// <summary>Draws of.</summary>
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

    /// <summary>Attempts to read word.</summary>
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
