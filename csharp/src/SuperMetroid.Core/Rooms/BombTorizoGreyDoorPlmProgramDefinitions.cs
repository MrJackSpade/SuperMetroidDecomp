using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bomb Torizo's bank-$84 grey-door instruction lists. The closing list is
/// interrupted by the native $BA6F Bombs-condition callback, whose machine
/// code is deliberately not part of this compiled instruction data.
/// </summary>
internal static class BombTorizoGreyDoorPlmProgramDefinitions
{
    /// <summary>Bomb-gated closing instruction list at $84:BA4C.</summary>
    internal const ushort ClosingStart = 0xba4c;
    /// <summary>Last closing-list byte before callback code at $84:BA6E.</summary>
    internal const ushort ClosingEnd = 0xba6e;
    /// <summary>Condition-gated resident instruction list at $84:BA7F.</summary>
    internal const ushort ResidentStart = 0xba7f;
    /// <summary>Last opening-list byte before unused setup code at $84:BAD0.</summary>
    internal const ushort ResidentEnd = 0xbad0;

    /// <summary>$84:BA8D: sleeping locked-door loop.</summary>
    private const ushort Locked = 0xba8d;
    /// <summary>$84:BA93: install the shot link after the room condition succeeds.</summary>
    private const ushort Activate = 0xba93;
    /// <summary>$84:BA9B: three pairs of three-tick blue/four-tick grey flashes.</summary>
    private const ushort Flash = 0xba9b;
    /// <summary>$84:BAB7: one hit sets the persistent door bit.</summary>
    private const ushort Hit = 0xbab7;
    /// <summary>$84:BABC: sound and opening frames.</summary>
    private const ushort Open = 0xbabc;
    /// <summary>$84:BD0F: follow the installed link when shot.</summary>
    private const ushort FollowLinkWhenShot = 0xbd0f;
    /// <summary>$84:A683: cleared right-facing door cap.</summary>
    private const ushort ClearDraw = 0xa683;
    /// <summary>$84:A6D7: closed right-facing grey cap; subsequent frames are twelve bytes apart.</summary>
    private const ushort ClosedDraw = 0xa6d7;
    /// <summary>$84:A9EF: closed right-facing blue cap used during flashing.</summary>
    private const ushort BlueDraw = 0xa9ef;

    /// <summary>
    /// Word starts outside the regular closing, flashing and opening frame runs, by
    /// bank-<c>$84</c> address. Each one-byte operand shifts the following alignment.
    /// </summary>
    private enum ProgramWord : ushort
    {
        /// <summary>$84:BA4C: first closing duration.</summary>
        ClosingDuration = ClosingStart,
        /// <summary>$84:BA4E: first closing cleared-cap draw.</summary>
        ClosingClearDraw = ClosingStart + 2,
        /// <summary>$84:BA50: goto-if-Samus-has-no-bombs opcode.</summary>
        ClosingGotoIfNoBombs = ClosingStart + 4,
        /// <summary>$84:BA52: goto-if-no-bombs target, the closing list itself.</summary>
        ClosingGotoIfNoBombsTarget = ClosingStart + 6,
        /// <summary>$84:BA54: closing wait duration.</summary>
        ClosingWaitDuration = ClosingStart + 8,
        /// <summary>$84:BA56: closing wait cleared-cap draw.</summary>
        ClosingWaitDraw = ClosingStart + 10,
        /// <summary>$84:BA58: library-three closing sound opcode.</summary>
        ClosingQueueSound = ClosingStart + 12,
        /// <summary>$84:BA6B: closing terminal goto opcode.</summary>
        ClosingGoto = ClosingEnd - 3,
        /// <summary>$84:BA6D: closing goto target, the resident list.</summary>
        ClosingGotoTarget = ClosingEnd - 1,
        /// <summary>$84:BA7F: goto-if-door-bit-set opcode.</summary>
        ResidentGotoIfDoorBitSet = ResidentStart,
        /// <summary>$84:BA81: goto-if-door-bit-set target, the blue right-facing door.</summary>
        ResidentGotoIfDoorBitSetTarget = ResidentStart + 2,
        /// <summary>$84:BA83: resident link-instruction opcode.</summary>
        ResidentLinkInstruction = ResidentStart + 4,
        /// <summary>$84:BA85: resident link target, the activation list.</summary>
        ResidentLinkTarget = ResidentStart + 6,
        /// <summary>$84:BA87: set-grey-door pre-instruction opcode.</summary>
        ResidentSetGreyDoorPreInstruction = ResidentStart + 8,
        /// <summary>$84:BA89: resident closed-cap duration.</summary>
        ResidentClosedDuration = ResidentStart + 10,
        /// <summary>$84:BA8B: resident closed-cap draw.</summary>
        ResidentClosedDraw = ResidentStart + 12,
        /// <summary>$84:BA8D: locked-loop sleep opcode.</summary>
        LockedSleep = Locked,
        /// <summary>$84:BA8F: locked-loop goto opcode.</summary>
        LockedGoto = Locked + 2,
        /// <summary>$84:BA91: locked-loop goto target.</summary>
        LockedGotoTarget = Locked + 4,
        /// <summary>$84:BA93: activation link-instruction opcode.</summary>
        ActivateLinkInstruction = Activate,
        /// <summary>$84:BA95: activation link target, the hit list.</summary>
        ActivateLinkTarget = Activate + 2,
        /// <summary>$84:BA97: install-pre-instruction opcode.</summary>
        ActivateInstallPreInstruction = Activate + 4,
        /// <summary>$84:BA99: follow-link-when-shot pre-instruction operand.</summary>
        ActivatePreInstructionOperand = Activate + 6,
        /// <summary>$84:BAB3: flashing terminal goto opcode.</summary>
        FlashGoto = Hit - 4,
        /// <summary>$84:BAB5: flashing goto target.</summary>
        FlashGotoTarget = Hit - 2,
        /// <summary>$84:BAB7: increment-door-hit-counter-and-goto opcode.</summary>
        HitIncrementAndGoto = Hit,
        /// <summary>$84:BABA: hit goto target after the one-byte hit count.</summary>
        HitGotoTarget = Hit + 3,
        /// <summary>$84:BABC: library-three opening sound opcode.</summary>
        OpenQueueSound = Open,
        /// <summary>$84:BACF: terminal delete opcode.</summary>
        Delete = ResidentEnd - 1,
    }

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (!Owns(address) || !Owns((ushort)(address + 1))) return false;
        TryReadMechanicsByte(address, out byte low);
        TryReadMechanicsByte((ushort)(address + 1), out byte high);
        value = (ushort)(low | high << 8);
        return true;
    }

    /// <summary>
    /// Decode the NTSC closing/resident lists, preserving all 117 bytes and 115
    /// overlapping word views. Closing walks grey frames 3..0, opening 1..3 then
    /// clears the cap, and flashing repeats three identical blue/grey pairs.
    /// One-byte sound/hit operands change alignment. Callback machine code in
    /// BA6F..BA7E is excluded. No stored program or generated cache remains.
    /// </summary>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (!Owns(address)) return false;
        if (address is 0xba5a or 0xbab9 or 0xbabe)
        {
            value = address == 0xba5a ? (byte)8 : address == 0xbab9 ? (byte)1 : (byte)7;
            return true;
        }
        int start;
        ushort word;
        if (address is >= 0xba5b and <= 0xba6a)
        {
            int offset = address - 0xba5b;
            int frame = 3 - offset / 4;
            word = offset % 4 < 2 ? (ushort)(frame == 0 ? 1 : 2) : (ushort)(ClosedDraw + frame * 12);
            start = 0xba5b + (offset / 2) * 2;
        }
        else if (address is >= Flash and < 0xbab3)
        {
            int offset = address - Flash;
            bool blue = offset % 8 < 4;
            word = offset % 4 < 2 ? (ushort)(blue ? 3 : 4) : blue ? BlueDraw : ClosedDraw;
            start = Flash + (offset / 2) * 2;
        }
        else if (address is >= 0xbabf and <= 0xbace)
        {
            int offset = address - 0xbabf;
            int frame = offset / 4 + 1;
            word = offset % 4 < 2 ? (ushort)(frame == 4 ? 1 : 4) :
                frame == 4 ? ClearDraw : (ushort)(ClosedDraw + frame * 12);
            start = 0xbabf + (offset / 2) * 2;
        }
        else
        {
            start = address < 0xba5a ? address & ~1 :
                address is >= 0xbaba and <= 0xbabd ? address & ~1 :
                ((address - 1) & ~1) + 1;
            word = ClosedNativeWords.Decode<ProgramWord>((ushort)start, "Bomb Torizo grey-door program word") switch
            {
                ProgramWord.ClosingDuration => 2,
                ProgramWord.ClosingClearDraw or ProgramWord.ClosingWaitDraw => ClearDraw,
                ProgramWord.ClosingGotoIfNoBombs => (ushort)RoomPlmInstruction.GotoIfSamusHasNoBombs,
                ProgramWord.ClosingGotoIfNoBombsTarget => ClosingStart,
                ProgramWord.ClosingWaitDuration => 40,
                ProgramWord.ClosingQueueSound or ProgramWord.OpenQueueSound => (ushort)RoomPlmInstruction.QueueSoundLibrary3Maximum6,
                ProgramWord.ClosingGoto or ProgramWord.LockedGoto or ProgramWord.FlashGoto => (ushort)RoomPlmInstruction.Goto,
                ProgramWord.ClosingGotoTarget => ResidentStart,
                ProgramWord.ResidentGotoIfDoorBitSet => (ushort)RoomPlmInstruction.GotoIfDoorBitSet,
                ProgramWord.ResidentGotoIfDoorBitSetTarget => BlueDoorPlmProgramDefinitions.ClosedRight,
                ProgramWord.ResidentLinkInstruction or ProgramWord.ActivateLinkInstruction => (ushort)RoomPlmInstruction.LinkInstruction,
                ProgramWord.ResidentLinkTarget => Activate,
                ProgramWord.ResidentSetGreyDoorPreInstruction => (ushort)RoomPlmInstruction.SetGreyDoorPreInstruction,
                ProgramWord.ResidentClosedDuration => 1,
                ProgramWord.ResidentClosedDraw => ClosedDraw,
                ProgramWord.LockedSleep => (ushort)RoomPlmInstruction.Sleep,
                ProgramWord.LockedGotoTarget => Locked,
                ProgramWord.ActivateLinkTarget => Hit,
                ProgramWord.ActivateInstallPreInstruction => (ushort)RoomPlmInstruction.InstallPreInstruction,
                ProgramWord.ActivatePreInstructionOperand => FollowLinkWhenShot,
                ProgramWord.FlashGotoTarget => Flash,
                ProgramWord.HitIncrementAndGoto => (ushort)RoomPlmInstruction.IncrementDoorHitCounterAndGoto,
                ProgramWord.HitGotoTarget => Open,
                ProgramWord.Delete => (ushort)RoomPlmInstruction.Delete,
                _ => throw new InvalidOperationException($"Undefined {nameof(ProgramWord)} {start:X4}."),
            };
        }
        value = (byte)(word >> ((address - start) * 8));
        return true;
    }

    private static bool Owns(ushort address) =>
        address is >= ClosingStart and <= ClosingEnd or >= ResidentStart and <= ResidentEnd;
}
