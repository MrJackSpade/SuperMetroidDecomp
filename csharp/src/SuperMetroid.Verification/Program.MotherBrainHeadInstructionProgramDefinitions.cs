using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static readonly (ushort Start, ushort End)[] MotherBrainHeadRegions =
    [
        (MotherBrainHeadInstructionProgramDefinitionsTooling.EarlyStart,
            MotherBrainHeadInstructionProgramDefinitionsTooling.EarlyEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowAndNeutralPhaseTwoStart,
            MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowAndNeutralPhaseTwoEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.NeutralStart,
            MotherBrainHeadInstructionProgramDefinitionsTooling.NeutralRegionEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.CorpseAndRingsStart,
            MotherBrainHeadInstructionProgramDefinitionsTooling.CorpseAndRingsEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.BombAndLaserStart,
            MotherBrainHeadInstructionProgramDefinitionsTooling.BombAndLaserEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowChargeStart,
            MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowChargeEnd),
    ];

    private static void VerifyMotherBrainHeadInstructionProgramDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        int checkedWords = 0;
        foreach ((ushort start, ushort end) in MotherBrainHeadRegions)
        {
            for (int pointer = start; pointer <= end; pointer += 2)
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), 0xa90000 | pointer),
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
            (MotherBrainHeadInstructionProgramDefinitionsTooling.NeutralStart,
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

        // The brain list runs in the draw-time processor ($A9:92AF), not on the head enemy.
        // $A9:C447 installs a list with timer one without drawing, so its first frame shows
        // for exactly its duration; frames the processor installs on a draw show one longer.
        MethodInfo advanceBrain = typeof(RoomEnemySystem).GetMethod(
            "AdvanceMotherBrainBrainInstructions", flags)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;
        FieldInfo motherBrainField = typeof(RoomEnemySystem).GetField("_motherBrain", flags)!;
        (RoomEnemySystem Enemies, MotherBrainEnemyState State) CreateBrain(ushort start)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, new MotherBrainHeadInstructionReadGuard(rom));
            var head = new RoomEnemySlot(1)
            {
                EnemyDefinitionPointer = 0xec3f,
                Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 },
            };
            var state = new MotherBrainEnemyState(new RoomEnemySlot(0)) { Head = head };
            motherBrainField.SetValue(enemies, state);
            state.BrainInstructionPointer = start;
            state.BrainInstructionTimer = 1;
            return (enemies, state);
        }
        ushort? Draw(RoomEnemySystem enemies, MotherBrainEnemyState state) =>
            (ushort?)advanceBrain.Invoke(enemies, [state]);

        foreach ((ushort start, ushort duration, ushort spritemap) in new[]
        {
            ((ushort)0x9c21, (ushort)4, (ushort)0xa586),
            ((ushort)0x9c77, (ushort)1, (ushort)0xa5f8),
            ((ushort)0x9c87, (ushort)4, (ushort)0xa586),
            (MotherBrainHeadInstructionProgramDefinitionsTooling.NeutralStart,
                (ushort)4, (ushort)0xa69b),
            ((ushort)0x9d25, (ushort)2, (ushort)0xa69b),
            (MotherBrainHeadInstructionProgramDefinitionsTooling.BombAndLaserStart,
                (ushort)4, (ushort)0xa586),
            (MotherBrainHeadInstructionProgramDefinitions.BombStart,
                (ushort)4, (ushort)0xa69b),
            ((ushort)0x9f34, (ushort)16, (ushort)0xa5bf),
            ((ushort)0x9f6e, (ushort)4, (ushort)0xa5f8),
        })
        {
            (RoomEnemySystem enemies, MotherBrainEnemyState state) = CreateBrain(start);
            for (int draw = 0; draw < duration; draw++)
            {
                AssertEqual(spritemap, Draw(enemies, state),
                    $"Mother Brain brain ${start:X4} shows its first frame on draw {draw}");
                AssertEqual(start, state.BrainInstructionPointer,
                    $"Mother Brain brain ${start:X4} holds its first frame for its duration");
            }
            AssertEqual((ushort)(duration + 1), state.BrainInstructionTimer,
                $"Mother Brain brain ${start:X4} timer counts up past the duration");
            Draw(enemies, state);
            AssertEqual((ushort)1, state.BrainInstructionTimer,
                $"Mother Brain brain ${start:X4} installs its next frame once the timer exceeds the duration");
        }

        // The first-frame tests above stop before $A9:9C25. Execute its branch too: the
        // target word at $A9:9C27 is compiled mechanics, so the guarded bus must not be read.
        (RoomEnemySystem loopingEnemies, MotherBrainEnemyState looping) = CreateBrain(0x9c21);
        for (int draw = 0; draw < 4; draw++)
            Draw(loopingEnemies, looping);
        AssertEqual((ushort?)0xa586, Draw(loopingEnemies, looping),
            "Mother Brain brain loop retains the authored visual frame");
        AssertEqual((ushort)0x9c21, looping.BrainInstructionPointer,
            "Mother Brain brain loops through the compiled goto operand");
        AssertEqual((ushort)1, looping.BrainInstructionTimer, "the looped frame restarts its timer");

        Console.WriteLine(
            $"Mother Brain head programs: {checkedWords} native words, strict data gaps, " +
            "nine ordinary-enemy entry frames and a branch loop pass.");
    }

    private sealed class MotherBrainHeadInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

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
