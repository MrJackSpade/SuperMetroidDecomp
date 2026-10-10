using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks compiled Ceres flight-actor identities, initializer branches, motion data, star aliases, and actor-index bounds against cartridge metadata.</summary>
    /// <param name="retail">Retail ROM address space used as the oracle for native bank-$8B actor operands and callbacks.</param>
    private static void VerifyCeresFlightActorMetadata(ISnesAddressSpace retail)
    {
        CeresFlightActorDefinition front = CeresFlightActorDefinitions.FrontStars;
        ushort frontDefinition = ReadWord(retail, 0x8bbe57);
        Suite(nameof(VerifyActor), () => VerifyActor(retail, front, frontDefinition, "front stars"));
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
            int[] spawnOperands = [0x8bbe3c, 0x8bbe42, 0x8bbe48, 0x8bbe4e, 0x8bbe57];
            AssertEqual((byte)0xa0, retail.ReadByte(spawnOperands[index] - 1), "native rear spawn LDY");
            ushort definition = ReadWord(retail, spawnOperands[index]);
            VerifyActor(retail, actor, definition, $"rear actor {index}");
            AssertEqual((ushort)0, actor.InitialTimer, "rear initializer leaves cleared timer");
            if (index != 4)
                AssertEqual(ReadWord(retail, CeresFlightActorDefinitions.NativeBank | (definition + 2)),
                    actor.ActivePreInstruction, "rear initializer preserves the definition callback");
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
        AssertEqual(front.InstructionList, CeresFlightActorDefinitions.RearViewActor(4).InstructionList, "front/rear program alias");
        AssertEqual(ReadWord(retail, CeresFlightActorDefinitions.NativeBank | (frontDefinition + 2)),
            front.ActivePreInstruction, "front stars preserve the definition callback");
        AssertEqual(0, front.HorizontalDelta, "front stars have callback-driven acceleration instead of fixed X velocity");
        AssertEqual(false, front.WrapX, "front stars have no nine-bit wrap");
        static void VerifyActor(
            ISnesAddressSpace source,
            CeresFlightActorDefinition actor,
            ushort definitionPointer,
            string name)
        {
            int address = CeresFlightActorDefinitions.NativeBank | definitionPointer;
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
