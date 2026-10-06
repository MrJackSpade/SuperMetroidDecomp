using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresFlightActorMetadata(ISnesAddressSpace retail)
    {
        CeresFlightActorDefinition front = CeresFlightActorDefinitions.FrontStars;
        Suite(nameof(VerifyActor), () => VerifyActor(retail, front, "front stars"));
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
            int[] spawnOperands = [0x8bbe3c, 0x8bbe42, 0x8bbe48, 0x8bbe4e, 0x8bbe57];
            AssertEqual((byte)0xa0, retail.ReadByte(spawnOperands[index] - 1), "native rear spawn LDY");
            AssertEqual(ReadWord(retail, spawnOperands[index]), actor.Pointer, "native rear spawn identity");
            AssertEqual((ushort)0, actor.InitialTimer, "rear initializer leaves cleared timer");
            if (index != 4)
                AssertEqual(actor.DefinitionPreInstruction, actor.ActivePreInstruction, "rear initializer preserves callback");
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

        foreach (int index in new[] { int.MinValue, -1, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => CeresFlightActorDefinitions.RearViewActor(index), "rear metadata invalid index");
        AssertEqual((ushort)1, ReadWord(retail, 0x8bbe51), "rear vortex native initializer parameter");
        AssertEqual((ushort)1, ReadWord(retail, 0x8bbe5a), "rear stars native initializer parameter");
        AssertEqual(front.Pointer, CeresFlightActorDefinitions.RearViewActor(4).Pointer, "front/rear native definition alias");
        AssertEqual(front.InstructionList, CeresFlightActorDefinitions.RearViewActor(4).InstructionList, "front/rear program alias");
        AssertEqual(front.DefinitionPreInstruction, front.ActivePreInstruction, "front stars preserve callback");
        AssertEqual(0, front.HorizontalDelta, "front stars have callback-driven acceleration instead of fixed X velocity");
        AssertEqual(false, front.WrapX, "front stars have no nine-bit wrap");
        static void VerifyActor(
            ISnesAddressSpace source,
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
            ISnesAddressSpace source,
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

}
