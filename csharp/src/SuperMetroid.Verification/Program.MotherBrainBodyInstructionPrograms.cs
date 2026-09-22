using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Proves that the compiled Mother Brain body mechanics words match the pinned cartridge
    /// and that every supported native program can run while those ROM bytes are inaccessible.
    /// </summary>
    private static void VerifyMotherBrainBodyInstructionPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));

        foreach (MotherBrainBodyInstructionMechanicsWord definition in
            MotherBrainBodyInstructionProgramDefinitions.AllWords)
        {
            AssertEqual(
                ReadRetailWord(rom, 0xa90000 | definition.Address),
                definition.Word,
                $"Mother Brain body compiled mechanics word $A9:{definition.Address:X4}");

            if ((definition.Word & 0x8000) == 0)
            {
                AssertTrue(
                    !MotherBrainBodyInstructionProgramDefinitions.TryGetWord(
                        unchecked((ushort)(definition.Address + 2)),
                        out _),
                    $"Mother Brain body spritemap $A9:{definition.Address + 2:X4} remains presentation-owned");
            }
        }

        var guarded = new MotherBrainBodyMechanicsReadGuard(rom);
        ushort[] programs =
        [
            0x9730, 0x976a, 0x97a4, 0x97de, 0x9818,
            0x9852, 0x988c, 0x98c6, 0x9900, 0x993a,
            0x9974, 0x99aa, 0x99c6, 0x99e2, 0x99f2,
            0x9a02, 0x9a0a, 0x9a26,
        ];

        foreach (ushort program in programs)
        {
            var state = new MotherBrainBodyAnimationState
            {
                XPosition = 0x0080,
                YPosition = 0x0080,
                Form = 3,
            };
            state.SetInstructionList(program);
            int calls = 0;
            while (!state.Sleeping)
            {
                state.Step(guarded);
                calls++;
                AssertTrue(calls < 200,
                    $"Mother Brain body program $A9:{program:X4} reaches native sleep");
            }
        }

        var initial = new MotherBrainBodyAnimationState();
        initial.SetInstructionList(MotherBrainBodyInstructionProgramDefinitions.InitialDummy);
        MotherBrainBodyAnimationStepResult initialStep = initial.Step(guarded);
        AssertEqual((ushort)0x0000, initial.InstructionTimer,
            "Mother Brain initial dummy preserves its native zero duration");
        AssertEqual((ushort)0x9c17, initial.InstructionPointer,
            "Mother Brain initial dummy advances to its dormant sleep");
        AssertEqual((ushort)0xa320, initial.SpritemapPointer,
            "Mother Brain initial dummy retains its live presentation operand");
        AssertTrue(initialStep.LoadedFrame,
            "Mother Brain initial dummy publishes its presentation frame");
        MotherBrainBodyAnimationStepResult wrappedStep = initial.Step(guarded);
        AssertEqual((ushort)0xffff, initial.InstructionTimer,
            "Mother Brain initial dummy timer wraps on its dormant second call");
        AssertTrue(!wrappedStep.LoadedFrame && !wrappedStep.Sleeping,
            "Mother Brain initial dummy wrap does not execute its unreachable sleep");

        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Mother Brain body programs do not reread compiled mechanics words");
        AssertTrue(guarded.AllowedReadAttempts > 0,
            "Mother Brain body programs retain live presentation spritemap reads");
        Console.WriteLine(
            $"  Mother Brain: {MotherBrainBodyInstructionProgramDefinitions.AllWords.Count} " +
            "body command/duration words are compiled; 18 active programs and the " +
            "initial dummy run with mechanics ROM reads forbidden.");

        static ushort ReadRetailWord(ISnesAddressSpace source, int address) =>
            unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));
    }

    private sealed class MotherBrainBodyMechanicsReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public int AllowedReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            ushort bankAddress = unchecked((ushort)address);
            bool forbidden = (address & 0xff0000) == 0xa90000 &&
                (MotherBrainBodyInstructionProgramDefinitions.TryGetWord(bankAddress, out _) ||
                 MotherBrainBodyInstructionProgramDefinitions.TryGetWord(
                     unchecked((ushort)(bankAddress - 1)),
                     out _));
            if (forbidden)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Mother Brain body reread compiled mechanics byte ${address:X6}.");
            }

            AllowedReadAttempts++;
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
