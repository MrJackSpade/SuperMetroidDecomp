using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static BabyMetroidVerification;
using static EnemyProjectileInstructionMechanicsDefinitionsVerification;
using static MotherBrainBeamWindowVerification;
using static MotherBrainDeathHandoffVerification;
using static MotherBrainFallingTubeInstructionDefinitionsVerification;
using static MotherBrainHeadInstructionProgramDefinitionsVerification;
using static MotherBrainRainbowVerification;
using static SamusDamageAndGrabVerification;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static class MotherBrainHandBeamBodyInstructionDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mother Brain hand-beam body instruction list")]
    internal static void VerifyMotherBrainHandBeamBodyInstructionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewMotherBrainHandBeamListReadGuard(rom));
        var body = new RoomEnemySlot(0)
        {
            EnemyDefinitionPointer = EnemyDefinitionId.MotherBrainBody,
            Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 },
        };

        int visualWords = 0;
        int mechanicsWords = 0;
        for (int pointer = MotherBrainHandBeamBodyInstructionDefinitions.Start;
            pointer <= MotherBrainHandBeamBodyInstructionDefinitions.End; pointer += 2)
        {
            ushort address = (ushort)pointer;
            ushort compiled;
            try
            {
                compiled = MotherBrainHandBeamBodyInstructionDefinitions.ReadMechanicsWord(
                    address);
                AssertEqual(compiled, (ushort)EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(body, address),
                    $"production Mother Brain hand-beam mechanics $A9:{pointer:X4}");
                mechanicsWords++;
            }
            catch (InvalidDataException)
            {
                compiled = MotherBrainHandBeamBodyInstructionDefinitions.ReadVisualSelector(
                    address);
                AssertEqual(compiled, (ushort)EnemyInstructionInterpreter.ReadEnemyVisualSelector(body, address),
                    $"production Mother Brain hand-beam visual selector $A9:{pointer:X4}");
                visualWords++;
            }
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), 0xa90000 | pointer),
                compiled, $"Mother Brain hand-beam source word $A9:{pointer:X4}");
        }
        AssertEqual(67, mechanicsWords + visualWords,
            "every hand-beam body list word has exactly one compiled owner");
        AssertEqual(15, visualWords,
            "all initial, dust-record, and terminal hand-beam visual selectors");
        AssertThrows<InvalidDataException>(
            () => MotherBrainHandBeamBodyInstructionDefinitions.ReadMechanicsWord(0x9a46),
            "hand-beam visual word is not mechanical");
        AssertThrows<InvalidDataException>(
            () => MotherBrainHandBeamBodyInstructionDefinitions.ReadVisualSelector(0x9a42),
            "hand-beam pose opcode is not a visual selector");
        AssertThrows<InvalidDataException>(
            () => MotherBrainHandBeamBodyInstructionDefinitions.ReadMechanicsWord(0x9ac8),
            "native code after the hand-beam list is not copied as data");

        Console.WriteLine(
            $"Mother Brain hand-beam body: {mechanicsWords} mechanics and " +
            $"{visualWords} visual-selector words match the pinned ROM without list reads.");
    }

    private static CartridgeReadGuard NewMotherBrainHandBeamListReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa99a42 and <= 0xa99ac7,
                address => $"Mother Brain reread compiled hand-beam list ${address:X6}.");

    [VerificationCommand("--mother-brain")]
    internal static int MotherBrainCommand()
    {
        Suite(nameof(VerifyMotherBrainHandBeamBodyInstructionDefinitions), () => VerifyMotherBrainHandBeamBodyInstructionDefinitions());
        Suite(nameof(VerifyMotherBrainFallingTubeInstructionDefinitions), () => VerifyMotherBrainFallingTubeInstructionDefinitions());
        Suite(nameof(VerifyMotherBrainHeadInstructionProgramDefinitions), () => VerifyMotherBrainHeadInstructionProgramDefinitions());
        Suite(nameof(VerifyMotherBrainBeamWindow), () => VerifyMotherBrainBeamWindow());
        Suite(nameof(VerifyMotherBrainDeathHandoff), () => VerifyMotherBrainDeathHandoff());
        Suite(nameof(VerifySamusDrainedController), () => VerifySamusDrainedController());
        Suite(nameof(VerifyMotherBrainRainbowBeamSamusMovement), () => VerifyMotherBrainRainbowBeamSamusMovement());
        Suite(nameof(VerifyMotherBrainRainbowBeamAttackSequence), () => VerifyMotherBrainRainbowBeamAttackSequence());
        Suite(nameof(VerifyEnemyProjectileInstructionMechanicsDefinitions), () => VerifyEnemyProjectileInstructionMechanicsDefinitions());
        Suite(nameof(VerifyBabyMetroidCutsceneEntrance), () => VerifyBabyMetroidCutsceneEntrance());
        return 0;
    }
}
