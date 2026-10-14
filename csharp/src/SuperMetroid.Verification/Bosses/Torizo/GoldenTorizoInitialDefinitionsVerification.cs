using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoEyeBeamAttackDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoJumpLandingDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoLeftFootOrbDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoLeftOrbDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoLeftTurnDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoRightOrbDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoRightSonicDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoRightwardDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoStunnedDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoWalkingDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoFallingLeftDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoJumpBackDefinitionsVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Golden Torizo's initial and awakening control words, selected frames, and collision layout against ROM.</summary>
internal static class GoldenTorizoInitialDefinitionsVerification
{
    /// <summary>Compares the initial entry with bank AA, then runs the related Golden Torizo program suites.</summary>
    internal static void VerifyGoldenTorizoInitialDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = 0xaa;
        foreach (InstructionMechanicsWord word in
                 Enumerable.Range(0,
                     GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWordCount)
                     .Select(GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWord))
        {
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo initial mechanics $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoInitialInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo initial word $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoInitialInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, word.Address))) &&
                       GoldenTorizoInitialInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, unchecked((ushort)(word.Address + 1))))),
                $"Golden Torizo initial word $AA:{word.Address:X4} owns both bytes");
        }

        ushort visualOperand = GoldenTorizoInitialInstructionProgramDefinitions
            .PresentationWordAddress(0);
        AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, visualOperand,
                out ushort selectedFrame) &&
                   selectedFrame == ReadWord(visualOperand),
            "Golden Torizo initial frame selector is compiled from the cartridge");
        AssertTrue(!GoldenTorizoInitialInstructionProgramDefinitions
                .TryReadMechanicsWord(visualOperand, out _),
            "Golden Torizo visual operand is not executable mechanics");
        AssertTrue(!GoldenTorizoInitialInstructionProgramDefinitions
                .TryReadMechanicsWord(0xc9e2, out _),
            "later Golden Torizo program is not invented by initial catalog");

        Console.WriteLine(
            "Golden Torizo initial entry: seven mechanics words and one visual frame " +
            "match the pinned cartridge.");
        Suite(nameof(VerifyGoldenTorizoAwakeningDefinitions), () => VerifyGoldenTorizoAwakeningDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoWalkingDefinitions), () => VerifyGoldenTorizoWalkingDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoRightwardDefinitions), () => VerifyGoldenTorizoRightwardDefinitions(rom));
        Suite(nameof(VerifyTorizoJumpBackDefinitions), () => VerifyTorizoJumpBackDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoJumpLandingDefinitions), () => VerifyGoldenTorizoJumpLandingDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoRightOrbDefinitions), () => VerifyGoldenTorizoRightOrbDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoLeftOrbDefinitions), () => VerifyGoldenTorizoLeftOrbDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoRightSonicDefinitions), () => VerifyGoldenTorizoRightSonicDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoEyeBeamAttackDefinitions), () => VerifyGoldenTorizoEyeBeamAttackDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoStunnedDefinitions), () => VerifyGoldenTorizoStunnedDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoLeftTurnDefinitions), () => VerifyGoldenTorizoLeftTurnDefinitions(rom));
        Suite(nameof(VerifyTorizoFallingLeftDefinitions), () => VerifyTorizoFallingLeftDefinitions(rom));
        Suite(nameof(VerifyGoldenTorizoLeftFootOrbDefinitions), () => VerifyGoldenTorizoLeftFootOrbDefinitions(rom));

        ushort ReadWord(ushort address) =>
            (ushort)(SnesLittleEndian.Word(GoldenTorizoInitialDefinitionsRomByte(rom, (bank << 16) | address), GoldenTorizoInitialDefinitionsRomByte(rom, (bank << 16) |
                    unchecked((ushort)(address + 1)))));
    }

    /// <summary>Reads one bank-AA cartridge byte for the initial program's independent word comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Golden Torizo initial definitions")]
    private static byte GoldenTorizoInitialDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks awakening mechanics, visual selectors, seven physical frames, and their nine hitbox lists.</summary>
    internal static void VerifyGoldenTorizoAwakeningDefinitions(ISnesAddressSpace rom)
    {
        const byte bank = TorizoCollisionDefinitions.Bank;
        var mechanicsAddresses = new HashSet<ushort>();
        int transfers = 0;
        for (int index = 0;
             index < GoldenTorizoAwakeningInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord word =
                GoldenTorizoAwakeningInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertTrue(mechanicsAddresses.Add(word.Address),
                $"Golden Torizo awakening mechanics $AA:{word.Address:X4} is unique");
            AssertTrue(word.Address >= GoldenTorizoAwakeningInstructionProgramDefinitions.Start &&
                       word.Address < GoldenTorizoAwakeningInstructionProgramDefinitions.End,
                $"Golden Torizo awakening mechanics $AA:{word.Address:X4} is in the bounded program");
            AssertEqual(ReadWord(word.Address), word.Value,
                $"Golden Torizo awakening mechanics $AA:{word.Address:X4}");
            AssertTrue(GoldenTorizoAwakeningInstructionProgramDefinitions
                    .TryReadMechanicsWord(word.Address, out ushort selected) &&
                       selected == word.Value,
                $"Golden Torizo awakening word $AA:{word.Address:X4} lookup");
            AssertTrue(GoldenTorizoAwakeningInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, word.Address))) &&
                       GoldenTorizoAwakeningInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(((int)new SnesAddress(bank, unchecked((ushort)(word.Address + 1))))),
                $"Golden Torizo awakening word $AA:{word.Address:X4} owns both bytes");
            if (word.Value == (ushort)CommonEnemyInstruction.CopyToVram)
                transfers++;
        }
        AssertEqual(8, transfers, "Golden Torizo awakening transfer opcodes");
        AssertEqual(21,
            GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordCount,
            "Golden Torizo awakening presentation-operand count");
        var selectedFrames = new HashSet<ushort>();
        for (int index = 0;
             index < GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GoldenTorizoAwakeningInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(address);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, address,
                    out ushort compiled) && compiled == native,
                $"Golden Torizo awakening visual selector $AA:{address:X4}");
            AssertTrue(!mechanicsAddresses.Contains(address),
                $"Golden Torizo awakening visual selector $AA:{address:X4} is not mechanics");
            selectedFrames.Add(native);
        }
        AssertEqual(7, selectedFrames.Count,
            "Golden Torizo awakening selects all seven seated/standing frames");
        var hitboxLists = new HashSet<ushort>();
        foreach (ushort frame in selectedFrames.Order())
        {
            AssertTrue(selectedFrames.Contains(frame),
                $"Golden Torizo physical frame $AA:{frame:X4} is selected by the script");
            AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                    frame, out var components),
                $"Golden Torizo physical frame $AA:{frame:X4} is compiled");
            AssertEqual((ushort)components.Length, ReadWord(frame),
                $"Golden Torizo frame $AA:{frame:X4} component count");
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                GoldenTorizoCollisionComponent component = components[componentIndex];
                ushort address = unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((ushort)component.Offset.X), ReadWord(address),
                    $"Golden Torizo frame $AA:{frame:X4} component X {componentIndex}");
                AssertEqual(unchecked((ushort)component.Offset.Y), ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo frame $AA:{frame:X4} component Y {componentIndex}");
                AssertEqual(component.HitboxList, ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo frame $AA:{frame:X4} hitbox list {componentIndex}");
                hitboxLists.Add(component.HitboxList);
            }
        }
        AssertEqual(9, hitboxLists.Count,
            "Golden Torizo awakening has nine distinct physical hitbox lists");
        foreach (ushort pointer in hitboxLists)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> hitboxes =
                TorizoCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((ushort)hitboxes.Length, ReadWord(pointer),
                $"Golden Torizo hitbox list $AA:{pointer:X4} count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                GoldenTorizoCollisionHitbox hitbox = hitboxes[index];
                ushort address = unchecked((ushort)(pointer + 2 + index * 12));
                AssertEqual(unchecked((ushort)hitbox.Left), ReadWord(address),
                    $"Golden Torizo hitbox $AA:{pointer:X4} left");
                AssertEqual(unchecked((ushort)hitbox.Top), ReadWord(unchecked((ushort)(address + 2))),
                    $"Golden Torizo hitbox $AA:{pointer:X4} top");
                AssertEqual(unchecked((ushort)hitbox.Right), ReadWord(unchecked((ushort)(address + 4))),
                    $"Golden Torizo hitbox $AA:{pointer:X4} right");
                AssertEqual(unchecked((ushort)hitbox.Bottom), ReadWord(unchecked((ushort)(address + 6))),
                    $"Golden Torizo hitbox $AA:{pointer:X4} bottom");
                AssertEqual(hitbox.TouchAi, ReadWord(unchecked((ushort)(address + 8))),
                    $"Golden Torizo hitbox $AA:{pointer:X4} touch AI");
                AssertEqual(hitbox.ShotAi, ReadWord(unchecked((ushort)(address + 10))),
                    $"Golden Torizo hitbox $AA:{pointer:X4} shot AI");
            }
        }
        Console.WriteLine(
            $"Golden Torizo awakening: {mechanicsAddresses.Count} control words, 21 selectors, " +
            "seven physical frames and nine hitbox lists match the pinned cartridge.");

        ushort ReadWord(ushort address) =>
            (ushort)(SnesLittleEndian.Word(GoldenTorizoAwakeningDefinitionsRomByte(rom, (bank << 16) | address), GoldenTorizoAwakeningDefinitionsRomByte(rom, (bank << 16) |
                    unchecked((ushort)(address + 1)))));
    }

    /// <summary>Reads one bank-AA cartridge byte for awakening program and collision records.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Golden Torizo awakening definitions")]
    private static byte GoldenTorizoAwakeningDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the Golden Torizo awakening layout check through the verification command dispatcher.</summary>
    [VerificationCommand("--lookup-stream2-golden-awakening-layout")]
    internal static int LookupStream2GoldenAwakeningLayoutCommand()
    {
        Suite(nameof(VerifyGoldenTorizoAwakeningDefinitions), () => VerifyGoldenTorizoAwakeningDefinitions(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
