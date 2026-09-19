using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresFlightActorDefinitions()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        CeresFlightActorDefinition front = CeresFlightActorDefinitions.FrontStars;
        VerifyActor(retail, front, "front stars");
        AssertEqual(ReadWord(retail, 0x8bbe84), front.InitialTimer, "front stars initial timer");
        AssertEqual(ReadWord(retail, 0x8bbe8a), front.X, "front stars X");
        AssertEqual(ReadWord(retail, 0x8bbe90), front.Y, "front stars Y");
        AssertEqual(ReadWord(retail, 0x8bbe96), front.Attributes, "front stars attributes");
        AssertEqual(ReadWord(retail, 0x8bbec3),
            CeresFlightActorDefinitions.FrontStarAcceleration,
            "front stars acceleration");

        for (int index = 0; index < CeresFlightActorDefinitions.RearViewActorCount; index++)
        {
            CeresFlightActorDefinition actor = CeresFlightActorDefinitions.RearViewActor(index);
            VerifyActor(retail, actor, $"rear actor {index}");
            switch (index)
            {
                case 0:
                    VerifyWrappedActor(retail, actor, 0x8bbf23, 0x8bbf29, 0x8bbf2f,
                        0x8bbf3a, 0x8bbf46, "large asteroids");
                    break;
                case 1:
                    VerifyWrappedActor(retail, actor, 0x8bbf4d, 0x8bbf53, 0x8bbf59,
                        0x8bbf64, 0x8bbf70, "Ceres under attack");
                    break;
                case 2:
                    VerifyWrappedActor(retail, actor, 0x8bbf77, 0x8bbf7d, 0x8bbf83,
                        0x8bbf8e, 0x8bbf9a, "small asteroids");
                    break;
                case 3:
                    AssertEqual(ReadWord(retail, 0x8bbfb4), actor.X, "rear vortex X");
                    AssertEqual(ReadWord(retail, 0x8bbfba), actor.Y, "rear vortex Y");
                    AssertEqual(ReadWord(retail, 0x8bbfc0), actor.Attributes,
                        "rear vortex attributes");
                    AssertEqual(ReadWord(retail, 0x8bbfcb),
                        unchecked((ushort)-actor.HorizontalDelta), "rear vortex speed magnitude");
                    AssertTrue(!actor.WrapX, "rear vortex uses signed non-wrapping motion");
                    break;
                case 4:
                    AssertEqual(ReadWord(retail, 0x8bbea3), actor.X, "rear stars X");
                    AssertEqual(ReadWord(retail, 0x8bbea9), actor.Y, "rear stars Y");
                    AssertEqual(ReadWord(retail, 0x8bbeaf), actor.Attributes,
                        "rear stars attributes");
                    AssertEqual(ReadWord(retail, 0x8bbe9d), actor.ActivePreInstruction,
                        "rear stars initializer callback override");
                    AssertEqual(ReadWord(retail, 0x8bbfcb),
                        unchecked((ushort)-actor.HorizontalDelta), "rear stars speed magnitude");
                    AssertTrue(!actor.WrapX, "rear stars use signed non-wrapping motion");
                    break;
            }
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresFlightActorDefinitions.RearViewActor(
                CeresFlightActorDefinitions.RearViewActorCount),
            "Ceres rear-view actor definition boundary");

        var guard = new CeresFlightActorDefinitionReadGuard(retail);
        var state = new IntroCeresFlightState(guard);
        for (int frame = 0; frame < 5000 && !state.Finished; frame++)
            state.Step();
        AssertTrue(state.Finished, "Ceres approach completes through production actor paths");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Ceres approach never rereads compiled actor definitions");

        Console.WriteLine(
            "  Ceres flight actors: 43 native words and the complete production approach pass with constructor, callback and physical motion metadata reads forbidden.");

        static void VerifyActor(
            SuperMetroidAddressSpace source,
            CeresFlightActorDefinition actor,
            string name)
        {
            int address = CeresFlightActorDefinitions.NativeBank | actor.Pointer;
            AssertEqual(ReadWord(source, address), actor.Initialization,
                $"{name} initialization callback");
            AssertEqual(ReadWord(source, address + 2), actor.DefinitionPreInstruction,
                $"{name} definition pre-instruction");
            AssertEqual(ReadWord(source, address + 4), actor.InstructionList,
                $"{name} instruction list");
        }

        static void VerifyWrappedActor(
            SuperMetroidAddressSpace source,
            CeresFlightActorDefinition actor,
            int xAddress,
            int yAddress,
            int attributeAddress,
            int deltaAddress,
            int maskAddress,
            string name)
        {
            AssertEqual(ReadWord(source, xAddress), actor.X, $"{name} X");
            AssertEqual(ReadWord(source, yAddress), actor.Y, $"{name} Y");
            AssertEqual(ReadWord(source, attributeAddress), actor.Attributes,
                $"{name} attributes");
            AssertEqual(ReadWord(source, deltaAddress), unchecked((ushort)actor.HorizontalDelta),
                $"{name} speed");
            AssertEqual((ushort)0x01ff, ReadWord(source, maskAddress), $"{name} wrap mask");
            AssertTrue(actor.WrapX, $"{name} uses 512-pixel wrapping motion");
        }

        static ushort ReadWord(ISnesAddressSpace source, int address) =>
            unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));
    }

    private sealed class CeresFlightActorDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (IsForbidden(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ceres flight reread compiled actor byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsForbidden(int address) =>
            address is >= 0x8bbe84 and < 0x8bbe86 or
                >= 0x8bbe8a and < 0x8bbe8c or
                >= 0x8bbe90 and < 0x8bbe92 or
                >= 0x8bbe96 and < 0x8bbe98 or
                >= 0x8bbe9d and < 0x8bbe9f or
                >= 0x8bbea3 and < 0x8bbea5 or
                >= 0x8bbea9 and < 0x8bbeab or
                >= 0x8bbeaf and < 0x8bbeb1 or
                >= 0x8bbec3 and < 0x8bbec5 or
                >= 0x8bbf23 and < 0x8bbf25 or
                >= 0x8bbf29 and < 0x8bbf2b or
                >= 0x8bbf2f and < 0x8bbf31 or
                >= 0x8bbf3a and < 0x8bbf3c or
                >= 0x8bbf46 and < 0x8bbf48 or
                >= 0x8bbf4d and < 0x8bbf4f or
                >= 0x8bbf53 and < 0x8bbf55 or
                >= 0x8bbf59 and < 0x8bbf5b or
                >= 0x8bbf64 and < 0x8bbf66 or
                >= 0x8bbf70 and < 0x8bbf72 or
                >= 0x8bbf77 and < 0x8bbf79 or
                >= 0x8bbf7d and < 0x8bbf7f or
                >= 0x8bbf83 and < 0x8bbf85 or
                >= 0x8bbf8e and < 0x8bbf90 or
                >= 0x8bbf9a and < 0x8bbf9c or
                >= 0x8bbfb4 and < 0x8bbfb6 or
                >= 0x8bbfba and < 0x8bbfbc or
                >= 0x8bbfc0 and < 0x8bbfc2 or
                >= 0x8bbfcb and < 0x8bbfcd or
                >= 0x8bce85 and < 0x8bce97 or
                >= 0x8bcf0f and < 0x8bcf15 or
                >= 0x8bcf39 and < 0x8bcf3f;
    }
}
