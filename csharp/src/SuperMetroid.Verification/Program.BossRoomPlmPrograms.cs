using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using static BossRoomPlmNativeListAddresses;
using static NativePlmListDefinitions;

/// <summary>
/// #1296: the boss-room PLM programs typed in place of their bank-$84 lists. Each typed program must equal its native
/// list step by step: draws and durations, loop counts, fall-through, deletion, and the cue of every other instruction.
/// The families' runtime suites check the room mutations those programs make.
/// </summary>
internal static class BossRoomPlmProgramsVerification
{
    /// <summary>Botwoon's wall programs equal their native lists.</summary>
    internal static void VerifyBotwoonWallPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), BotwoonWallPlmProgramDefinitions.Programs, BotwoonWallEntries,
            draw => (ushort)draw, BotwoonWallCueAt, "Botwoon wall");
        Console.WriteLine($"  Botwoon wall programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The Kraid room programs equal their native lists.</summary>
    internal static void VerifyKraidRoomPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), KraidRoomPlmProgramDefinitions.Programs, KraidRoomEntries,
            draw => (ushort)draw, KraidRoomCueAt, "Kraid room");
        Console.WriteLine($"  Kraid room programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Crocomire's arena programs equal their native lists.</summary>
    internal static void VerifyCrocomireArenaPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), CrocomireArenaPlmProgramDefinitions.Programs, CrocomireArenaEntries,
            draw => (ushort)draw, NoCues<PlmNoCue>(), "Crocomire arena");
        Console.WriteLine($"  Crocomire arena programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Mother Brain's fake-death programs equal their native lists.</summary>
    internal static void VerifyMotherBrainFakeDeathPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), MotherBrainFakeDeathPlmProgramDefinitions.Programs,
            MotherBrainFakeDeathEntries, draw => (ushort)draw, NoCues<PlmNoCue>(), "Mother Brain fake death");
        Console.WriteLine($"  Mother Brain fake-death programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Botwoon's scroll, crumble-sound and move-down instructions.</summary>
    private static bool BotwoonWallCueAt(IImportCartridgeSource cartridge, ushort cursor, out BotwoonWallCue cue, out int length)
    {
        if (InstructionAt<RoomPlmInstruction>(cartridge, cursor) == RoomPlmInstruction.QueueSoundLibrary2Maximum6)
        {
            AssertEqual(BotwoonWallPlmProgramDefinitions.CrumbleSound,
                SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)),
                "Botwoon wall crumble sound");
            AssertEqual((byte)6, BotwoonWallPlmProgramDefinitions.CrumbleSoundMaximumQueued, "Botwoon wall crumble sound queue cap");
            (cue, length) = (BotwoonWallCue.QueueCrumbleSound, 3);
            return true;
        }
        BossRoomPlmNativeRoutine routine = InstructionAt<BossRoomPlmNativeRoutine>(cartridge, cursor);
        (cue, length) = (routine == BossRoomPlmNativeRoutine.BotwoonWallMoveDown ? BotwoonWallCue.MoveDownOneBlock : BotwoonWallCue.SetScrollsBlue, 2);
        return routine is BossRoomPlmNativeRoutine.BotwoonWallScrollsBlue or BossRoomPlmNativeRoutine.BotwoonWallMoveDown;
    }

    /// <summary>The Kraid room's move-right instruction.</summary>
    private static bool KraidRoomCueAt(IImportCartridgeSource cartridge, ushort cursor, out KraidRoomCue cue, out int length)
    {
        (cue, length) = (KraidRoomCue.MoveRightOneBlock, 2);
        return InstructionAt<BossRoomPlmNativeRoutine>(cartridge, cursor) == BossRoomPlmNativeRoutine.KraidRoomMoveRight;
    }
}
