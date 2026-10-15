using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Plms;
using static SuperMetroid.Verification.Rooms.Plms.NativePlmListDefinitions;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmNativeListAddresses;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// #1296: the room PLM programs typed in place of their bank-$84 lists. Each typed program must equal its native list
/// step by step: draws and durations, deletion, sleeps, and the cue of every other instruction with its operands. The
/// families' runtime suites check what those cues do.
/// </summary>
internal static class RoomPlmProgramsVerification
{
    /// <summary>The n00b tube's programs equal their native lists.</summary>
    internal static void VerifyNoobTubePrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), NoobTubePlmProgramDefinitions.Programs, NoobTubeEntries,
            draw => (ushort)draw, NoobTubeCueAt, "n00b tube");
        Console.WriteLine($"  N00b tube programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The shared delete and sleep programs equal their one-instruction native lists.</summary>
    internal static void VerifySharedPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), RoomPlmSharedProgramDefinitions.Programs, SharedEntries,
            draw => throw new InvalidOperationException($"The shared programs never draw; got {draw}."), NoCues<PlmNoCue>(), "shared PLM");
        Console.WriteLine($"  Shared PLM programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The lavaquake program equals its native list; each install names its phase's pre-instruction.</summary>
    internal static void VerifySpeedBoosterEscapePrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), SpeedBoosterEscapePlmProgramDefinitions.Programs,
            SpeedBoosterEscapeEntries, draw => throw new InvalidOperationException($"The lavaquake program never draws; got {draw}."),
            (IImportCartridgeSource cartridge, ushort cursor, out SpeedBoosterEscapePreInstruction cue, out int length) =>
            {
                (cue, length) = ((SpeedBoosterEscapePreInstruction)cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), 4);
                return InstructionAt<RoomPlmInstruction>(cartridge, cursor) == RoomPlmInstruction.InstallPreInstruction;
            }, "Speed Booster lavaquake");
        // A wake steps over the sleep that closes each phase, onto the next phase's list.
        for (int phase = 0; phase < SpeedBoosterEscapeEntries.Count; phase++)
        {
            (SpeedBoosterEscapeProgram program, ushort entry) = SpeedBoosterEscapeEntries[phase];
            SpeedBoosterEscapeProgram? expected = phase + 1 < SpeedBoosterEscapeEntries.Count ? SpeedBoosterEscapeEntries[phase + 1].Program : null;
            if (expected is { } next)
                AssertEqual(unchecked((ushort)(entry + 6)), SpeedBoosterEscapeEntries[phase + 1].Entry, $"lavaquake {next} follows {program}'s sleep");
            AssertEqual(expected, SpeedBoosterEscapePlmProgramDefinitions.PhaseAfter(program), $"lavaquake phase after {program}");
        }
        Console.WriteLine($"  Speed Booster lavaquake program: {steps} typed steps match bank $84.");
    }

    /// <summary>The Brinstar plants' programs equal their native lists.</summary>
    internal static void VerifySamusEaterPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), SamusEaterPlmProgramDefinitions.Programs, SamusEaterEntries,
            draw => (ushort)draw, SamusEaterCueAt, "Brinstar plant");
        Console.WriteLine($"  Brinstar plant programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The plants' hold, chew-sound, damage, release and clear instructions.</summary>
    private static bool SamusEaterCueAt(IImportCartridgeSource cartridge, ushort cursor, out SamusEaterCue cue, out int length)
    {
        (SamusEaterCue Cue, int Length)? decoded = InstructionAt<RoomPlmInstruction>(cartridge, cursor) switch
        {
            RoomPlmInstruction.InstallPreInstruction => (SamusEaterCue.HoldSamus, 4),
            RoomPlmInstruction.QueueSoundLibrary2Maximum6 => (SamusEaterCue.QueueChewSound, 3),
            RoomPlmInstruction.SamusEaterDamage => (SamusEaterCue.DamageSamus, 2),
            RoomPlmInstruction.SamusEaterReleaseImmunity => (SamusEaterCue.ReleaseSamus, 2),
            RoomPlmInstruction.ClearPreInstruction => (SamusEaterCue.ClearPreInstruction, 2),
            _ => null,
        };
        (cue, length) = decoded ?? default;
        if (decoded is null)
            return false;
        if (cue == SamusEaterCue.HoldSamus)
            AssertEqual(SamusEaterPlmRomData.HoldPreInstruction, cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), "plant hold pre-instruction");
        if (cue == SamusEaterCue.QueueChewSound)
        {
            AssertEqual(SamusEaterPlmRomData.ChewSound, ReadOperandByte(cartridge, cursor + 2), "plant chew sound");
            AssertEqual((byte)6, SamusEaterPlmProgramDefinitions.ChewSoundMaximumQueued, "plant chew sound queue cap");
        }
        return true;
    }

    /// <summary>The n00b tube's native instructions, with their event, link, pre-instruction and sound operands.</summary>
    private static bool NoobTubeCueAt(IImportCartridgeSource cartridge, ushort cursor, out NoobTubeCue cue, out int length)
    {
        ushort Operand(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        NoobTubeProgram Entry(ushort address) => NoobTubeEntries.Single(entry => entry.Entry == address).Program;
        RoomPlmInstruction instruction = InstructionAt<RoomPlmInstruction>(cartridge, cursor);
        (NoobTubeCue Cue, int Length)? decoded = instruction switch
        {
            RoomPlmInstruction.GotoIfEventSet => (NoobTubeCue.SkipIfBroken, 6),
            RoomPlmInstruction.LinkInstruction => (Entry(Operand(2)) == NoobTubeProgram.WaitForInput ? NoobTubeCue.LinkInputWait : NoobTubeCue.LinkBreak, 4),
            RoomPlmInstruction.InstallPreInstruction =>
                ((NoobTubePlmPreInstruction)Operand(2) == NoobTubePlmPreInstruction.WakeOnPowerBomb ? NoobTubeCue.ArmPowerBombWake : NoobTubeCue.ArmInputWake, 4),
            RoomPlmInstruction.ClearPreInstruction => (NoobTubeCue.ClearPreInstruction, 2),
            RoomPlmInstruction.LockSamus => (NoobTubeCue.LockSamus, 2),
            RoomPlmInstruction.SpawnNoobTubeCrack => (NoobTubeCue.SpawnCrack, 2),
            RoomPlmInstruction.QueueSoundLibrary2Maximum6 => (NoobTubeCue.QueueBreakSound, 3),
            RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles => (NoobTubeCue.SpawnShardsAndBubbles, 2),
            RoomPlmInstruction.TriggerNoobTubeEarthquake => (NoobTubeCue.TriggerEarthquake, 2),
            RoomPlmInstruction.SetEvent => (NoobTubeCue.SetBrokenEvent, 4),
            RoomPlmInstruction.EnableNoobTubeWaterPhysics => (NoobTubeCue.EnableWaterPhysics, 2),
            RoomPlmInstruction.UnlockSamus => (NoobTubeCue.UnlockSamus, 2),
            _ => null,
        };
        (cue, length) = decoded ?? default;
        if (decoded is null)
            return false;
        switch (cue)
        {
            case NoobTubeCue.SkipIfBroken:
                AssertEqual((ushort)NoobTubePlmRomData.BrokenEvent, Operand(2), "n00b tube skip event");
                AssertEqual(NoobTubeProgram.AlreadyBroken, Entry(Operand(4)), "n00b tube skip target");
                break;
            case NoobTubeCue.LinkBreak:
                AssertEqual(NoobTubeProgram.Break, Entry(Operand(2)), "n00b tube break link");
                break;
            case NoobTubeCue.ArmInputWake:
                AssertEqual((ushort)NoobTubePlmPreInstruction.WakeOnAcceptedInput, Operand(2), "n00b tube input wake");
                break;
            case NoobTubeCue.QueueBreakSound:
                AssertEqual(NoobTubePlmRomData.BreakSound, ReadOperandByte(cartridge, cursor + 2), "n00b tube break sound");
                AssertEqual((byte)6, NoobTubePlmProgramDefinitions.BreakSoundMaximumQueued, "n00b tube break sound queue cap");
                break;
            case NoobTubeCue.SetBrokenEvent:
                AssertEqual((ushort)NoobTubePlmRomData.BrokenEvent, Operand(2), "n00b tube broken event");
                break;
        }
        return true;
    }
}
