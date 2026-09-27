using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static readonly (ushort Start, ushort End)[] MotherBrainHeadRegions =
    [
        (MotherBrainHeadInstructionProgramDefinitions.EarlyStart,
            MotherBrainHeadInstructionProgramDefinitions.EarlyEnd),
        (MotherBrainHeadInstructionProgramDefinitions.RainbowAndNeutralPhaseTwoStart,
            MotherBrainHeadInstructionProgramDefinitions.RainbowAndNeutralPhaseTwoEnd),
        (MotherBrainHeadInstructionProgramDefinitions.NeutralStart,
            MotherBrainHeadInstructionProgramDefinitions.NeutralRegionEnd),
        (MotherBrainHeadInstructionProgramDefinitions.CorpseAndRingsStart,
            MotherBrainHeadInstructionProgramDefinitions.CorpseAndRingsEnd),
        (MotherBrainHeadInstructionProgramDefinitions.BombAndLaserStart,
            MotherBrainHeadInstructionProgramDefinitions.BombAndLaserEnd),
        (MotherBrainHeadInstructionProgramDefinitions.RainbowChargeStart,
            MotherBrainHeadInstructionProgramDefinitions.RainbowChargeEnd),
    ];

    private static void VerifyMotherBrainHeadInstructionProgramDefinitions()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        int checkedWords = 0;
        foreach ((ushort start, ushort end) in MotherBrainHeadRegions)
        {
            for (int pointer = start; pointer <= end; pointer += 2)
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(rom, 0xa90000 | pointer),
                    MotherBrainHeadInstructionProgramDefinitions.ReadWord((ushort)pointer),
                    $"Mother Brain compiled head word $A9:{pointer:X4}");
                AssertTrue(MotherBrainHeadInstructionProgramDefinitions.ContainsWord(
                        (ushort)pointer),
                    $"Mother Brain head word $A9:{pointer:X4} is catalogued");
                checkedWords++;
            }
        }
        (ushort Start, ushort ActiveEnd)[] dedicatedLists =
        [
            (MotherBrainHeadInstructionProgramDefinitions.NeutralStart,
                MotherBrainHeadInstructionProgramDefinitions.NeutralActiveEnd),
            (MotherBrainHeadInstructionProgramDefinitions.BabyAttackStart,
                MotherBrainHeadInstructionProgramDefinitions.BabyAttackActiveEnd),
            (MotherBrainHeadInstructionProgramDefinitions.BombStart,
                MotherBrainHeadInstructionProgramDefinitions.BombActiveEnd),
        ];
        foreach ((ushort start, ushort activeEnd) in dedicatedLists)
        {
            AssertTrue(MotherBrainHeadInstructionProgramDefinitions.IsActivePointer(start),
                $"Mother Brain head list ${start:X4} starts inside its active window");
            AssertTrue(MotherBrainHeadInstructionProgramDefinitions.IsActivePointer(activeEnd),
                $"Mother Brain head list ${start:X4} includes its final current pointer");
            AssertTrue(!MotherBrainHeadInstructionProgramDefinitions.IsActivePointer(
                    unchecked((ushort)(activeEnd + sizeof(ushort)))),
                $"Mother Brain head list ${start:X4} does not execute its padding word");
        }
        AssertEqual(362, checkedWords, "all six bounded Mother Brain head-list regions");
        AssertThrows<InvalidDataException>(
            () => MotherBrainHeadInstructionProgramDefinitions.ReadWord(0x9cba),
            "misaligned neutral-head instruction pointer fails explicitly");
        AssertThrows<InvalidDataException>(
            () => MotherBrainHeadInstructionProgramDefinitions.ReadWord(0x9c65),
            "native head opcode routine is not copied into the data catalog");
        AssertThrows<InvalidDataException>(
            () => MotherBrainHeadInstructionProgramDefinitions.ReadWord(0x9e10),
            "uncatalogued Mother Brain head instruction pointer fails explicitly");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        typeof(MotherBrainRainbowBeamAttackSequence).GetMethod(
            "SetHeadInstructionList", flags)!.Invoke(sequence,
                [MotherBrainHeadInstructionProgramDefinitions.BabyAttackStart]);
        var samus = new SamusState { XPosition = 0x00e0, YPosition = 0x0078 };
        MotherBrainHeadAnimationStepResult firstBabyFrame = sequence.StepHeadAnimation(
            new MotherBrainHeadInstructionReadGuard(rom), samus,
            new BabyMetroidCutsceneState());
        AssertTrue(firstBabyFrame.LoadedFrame,
            "compiled Baby-attack head commands reach their first timed frame");
        AssertEqual((ushort)0x9dc5, firstBabyFrame.InstructionPointerAfter,
            "compiled Baby-attack head program reaches the native first-frame cursor");
        AssertEqual((ushort)0xa717, firstBabyFrame.SpritemapPointer,
            "compiled Baby-attack head program selects the native first-frame visual");

        MethodInfo processInstructions = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", flags)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;
        foreach ((ushort start, ushort duration, ushort spritemap) in new[]
        {
            ((ushort)0x9c21, (ushort)4, (ushort)0xa586),
            ((ushort)0x9c77, (ushort)1, (ushort)0xa5f8),
            ((ushort)0x9c87, (ushort)4, (ushort)0xa586),
            (MotherBrainHeadInstructionProgramDefinitions.NeutralStart,
                (ushort)4, (ushort)0xa69b),
            ((ushort)0x9d25, (ushort)2, (ushort)0xa69b),
            (MotherBrainHeadInstructionProgramDefinitions.BombAndLaserStart,
                (ushort)4, (ushort)0xa586),
            (MotherBrainHeadInstructionProgramDefinitions.BombStart,
                (ushort)4, (ushort)0xa69b),
            ((ushort)0x9f34, (ushort)16, (ushort)0xa5bf),
            ((ushort)0x9f6e, (ushort)4, (ushort)0xa5f8),
        })
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, new MotherBrainHeadInstructionReadGuard(rom));
            var head = new RoomEnemySlot(0)
            {
                EnemyDefinitionPointer = 0xec3f,
                Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 },
                CurrentInstruction = start,
                InstructionTimer = 1,
            };
            processInstructions.Invoke(enemies,
                [head, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            AssertEqual((ushort)(start + 4), head.CurrentInstruction,
                $"ordinary Mother Brain head owner advances ${start:X4} frame cursor");
            AssertEqual(duration, head.InstructionTimer,
                $"ordinary Mother Brain head owner retains ${start:X4} frame duration");
            AssertEqual(spritemap, head.SpritemapPointer,
                $"ordinary Mother Brain head owner selects ${start:X4} frame artwork");
        }

        Console.WriteLine(
            $"Mother Brain head programs: {checkedWords} native words, strict data gaps, " +
            "guarded Baby-attack and nine ordinary-enemy entry frames pass.");
    }

    private sealed class MotherBrainHeadInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            IsCompiledHeadByte(address)
                ? throw new InvalidOperationException(
                    $"Mother Brain reread compiled head instruction at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsCompiledHeadByte(int address) =>
            (address >> 16) == 0xa9 && MotherBrainHeadRegions.Any(region =>
                (address & 0xffff) >= region.Start &&
                (address & 0xffff) <= region.End + 1);
    }
}
