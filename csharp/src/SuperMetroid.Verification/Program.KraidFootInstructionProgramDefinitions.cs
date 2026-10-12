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
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;
using static KraidFootCollisionDefinitionsVerification;
using static KraidFootGeneratedProgramsVerification;
using static SuiteTimingVerification;

internal static class KraidFootInstructionProgramDefinitionsVerification
{
    internal static void VerifyKraidFootInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKraidFootInstructionProgramDefinitions), () => VerifyKraidFootInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyKraidFootInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidFootCollisionDefinitions), () => VerifyKraidFootCollisionDefinitions());
        Suite(nameof(VerifyKraidFootGeneratedMechanics), () => VerifyKraidFootGeneratedMechanics(rom));
        Suite(nameof(VerifyKraidFootGeneratedPresentation), () => VerifyKraidFootGeneratedPresentation(rom));

        var guard = NewKraidFootInstructionGuard(rom);
        var executedOperands = new HashSet<ushort>();
        RoomEnemySystem enemies = CreateKraidFootInstructionSystem(guard);
        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot foot = enemies.Slots[5];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        RunKraidFootProgram(rom, executedOperands, process, enemies, foot,
            KraidFootInstructionProgramDefinitions.Initial, calls: 2);
        AssertEqual((ushort)0x86eb, foot.CurrentInstruction,
            "initial Kraid foot program reaches terminal sleep");

        RunKraidFootProgram(rom, executedOperands, process, enemies, foot,
            KraidFootInstructionProgramDefinitions.Neutral, calls: 2);
        AssertEqual((ushort)0x86f1, foot.CurrentInstruction,
            "neutral Kraid foot program reaches terminal sleep");

        body.Position = body.Position.WithWhole(new(500, 500));
        RunKraidFootProgram(rom, executedOperands, process, enemies, foot,
            KraidFootInstructionProgramDefinitions.WalkingForward, calls: 36);
        AssertEqual(KraidFootInstructionProgramDefinitions.WalkingForwardFinished,
            foot.CurrentInstruction,
            "walking-forward Kraid foot reaches terminal sleep command");
        AssertEqual((ushort)458, body.Position.X.Whole,
            "walking-forward callbacks move Kraid left fourteen times");
        AssertEqual((ushort)500, body.Position.Y.Whole,
            "walking-forward callbacks balance vertical movement");
        AssertKraidFootSoundAndQuake(enemies, "walking-forward");
        RunKraidFootProgram(rom, executedOperands, process, enemies, foot,
            KraidFootInstructionProgramDefinitions.WalkingForwardFinished, calls: 1);
        AssertEqual(KraidFootInstructionProgramDefinitions.WalkingForwardFinished,
            foot.CurrentInstruction,
            "walking-forward terminal sleep retains its cursor");

        body.Position = body.Position.WithWhole(new(500, 500));
        RunKraidFootProgram(rom, executedOperands, process, enemies, foot,
            KraidFootInstructionProgramDefinitions.LungeForward, calls: 36);
        AssertEqual(KraidFootInstructionProgramDefinitions.LungeForwardFinished,
            foot.CurrentInstruction,
            "lunge Kraid foot reaches terminal sleep command");
        AssertEqual((ushort)458, body.Position.X.Whole,
            "lunge callbacks move Kraid left fourteen times");
        AssertEqual((ushort)500, body.Position.Y.Whole,
            "lunge callbacks balance vertical movement");
        AssertKraidFootSoundAndQuake(enemies, "lunge");
        RunKraidFootProgram(rom, executedOperands, process, enemies, foot,
            KraidFootInstructionProgramDefinitions.LungeForwardFinished, calls: 1);
        AssertEqual(KraidFootInstructionProgramDefinitions.LungeForwardFinished,
            foot.CurrentInstruction,
            "lunge terminal sleep retains its cursor");

        body.Position = body.Position.WithWhole(new(500, 500));
        RunKraidFootProgram(rom, executedOperands, process, enemies, foot,
            KraidFootInstructionProgramDefinitions.WalkingBackward, calls: 32);
        AssertEqual(KraidFootInstructionProgramDefinitions.WalkingBackwardLoop,
            foot.CurrentInstruction,
            "walking-backward Kraid foot reaches loop command");
        AssertEqual((ushort)542, body.Position.X.Whole,
            "walking-backward callbacks move Kraid right fourteen times");
        AssertEqual((ushort)500, body.Position.Y.Whole,
            "walking-backward callbacks balance vertical movement");
        AssertKraidFootSoundAndQuake(enemies, "walking-backward");
        foot.InstructionTimer = 1;
        InvokeKraidFootInstructionProcessor(rom, executedOperands, process, enemies, foot);
        AssertEqual((ushort)0x888f, foot.CurrentInstruction,
            "walking-backward goto begins the next real frame");
        AssertEqual((ushort)545, body.Position.X.Whole,
            "walking-backward goto executes the next loop's first movement callback");

        AssertEqual(106, executedOperands.Count,
            "all executed Kraid foot selectors match the cartridge");
        AssertEqual(0, guard.ObservedWords.Count,
            "Kraid foot visual selection requires no runtime ROM reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Kraid foot execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < KraidFootInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                KraidFootInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution covers Kraid foot presentation $A7:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => KraidFootInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Kraid foot presentation $A7:{address:X4} is rejected as mechanics");
        }
        AssertThrows<InvalidDataException>(
            () => KraidFootInstructionProgramDefinitions.ReadMechanicsWord(
                KraidFootInstructionProgramDefinitions.AdjacentUnusedFastBackward),
            "adjacent unused fast-backwards program is rejected as foot mechanics");

        _ = ProbeKraidFootInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKraidFootInstructionAllocation();
        AssertTrue(checksum != 0, "Kraid foot allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kraid foot mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kraid foot instruction mechanics: 193 compiled words, all five production " +
            "entries, seven callbacks, and 106 executed cartridge-matching selectors pass.");
    }

    private static RoomEnemySystem CreateKraidFootInstructionSystem(
        ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField(
            "_bus",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField(
            "<Kraid>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(
                enemies,
                new KraidEnemyState());

        enemies.Slots[0].EnemyDefinitionPointer = EnemyDefinitionId.Kraid;
        RoomEnemySlot foot = enemies.Slots[5];
        foot.EnemyDefinitionPointer = EnemyDefinitionId.KraidFoot;
        foot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
        return enemies;
    }

    private static void RunKraidFootProgram(
        SuperMetroidAddressSpace rom,
        HashSet<ushort> executedOperands,
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot foot,
        ushort entry,
        int calls)
    {
        foot.CurrentInstruction = entry;
        foot.InstructionTimer = 1;
        for (int call = 0; call < calls; call++)
        {
            foot.InstructionTimer = 1;
            InvokeKraidFootInstructionProcessor(rom, executedOperands, process, enemies, foot);
        }
    }

    private static void InvokeKraidFootInstructionProcessor(
        SuperMetroidAddressSpace rom,
        HashSet<ushort> executedOperands,
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot foot)
    {
        ushort previousSprite = foot.SpritemapPointer;
        process.Invoke(enemies, [foot, null, null, Position16<RoomPixels>.Zero, (ushort)0]);
        ushort record = unchecked((ushort)(foot.CurrentInstruction - 4));
        if ((ReadKraidFootInstructionWord(rom, record) & 0x8000) == 0)
            VerifyExecutedEnemySelector(rom, foot, executedOperands);
        else
            AssertEqual(previousSprite, foot.SpritemapPointer,
                "Kraid foot callback-to-sleep retains its previous sprite");
    }

    private static void AssertKraidFootSoundAndQuake(
        RoomEnemySystem enemies,
        string program)
    {
        AssertEqual((byte)0x76,
            enemies.LastKraidSoundEffect!.Value.SoundEffect.Value,
            $"{program} Kraid foot queues native step sound");
        AssertEqual((ushort)1, enemies.EarthquakeType,
            $"{program} Kraid foot selects native quake type");
        AssertEqual((ushort)10, enemies.EarthquakeTimer,
            $"{program} Kraid foot selects native quake duration");
    }

    private static int ProbeKraidFootInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KraidFootInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KraidFootInstructionProgramDefinitions.Initial
                    : KraidFootInstructionProgramDefinitions.WalkingBackwardLoop);
        }
        return checksum;
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid foot instruction words")]
    internal static ushort ReadKraidFootInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa70000 | address);

    private static CartridgeReadGuard NewKraidFootInstructionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(KraidFootInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Kraid foot mechanics byte ${address:X6}.")
            .ObserveWords(0xa70000, KraidFootInstructionProgramDefinitionsTooling.PresentationWordCount, KraidFootInstructionProgramDefinitionsTooling.PresentationWordAddress);

    [VerificationCommand("--kraid-foot-instruction-program-definitions")]
    internal static int KraidFootInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyKraidFootInstructionProgramDefinitions), () => VerifyKraidFootInstructionProgramDefinitions());
        return 0;
    }
}
