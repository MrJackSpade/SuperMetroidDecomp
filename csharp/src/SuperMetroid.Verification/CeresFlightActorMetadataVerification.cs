using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;

/// <summary>Checks front and rear Ceres flight actor metadata against the native setup instructions.</summary>
internal static class CeresFlightActorMetadataVerification
{
    /// <summary>Compares actor programs, positions, callbacks, speeds, and nine-bit wrap masks with bank $8B.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres flight actor metadata")]
    internal static void VerifyCeresFlightActorMetadata(ISnesAddressSpace retail)
    {
        CeresFlightActorDefinition front = CeresFlightActorDefinitions.FrontStars;
        ushort frontDefinition = CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe57);
        Suite(nameof(VerifyActor), () => VerifyActor(retail, front, frontDefinition, "front stars"));
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe84), front.InitialTimer, "front stars initial timer");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe8a), front.Position.X, "front stars X");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe90), front.Position.Y, "front stars Y");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe96), front.Attributes, "front stars attributes");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbec3),
            CeresFlightActorDefinitions.FrontStarAcceleration,
            "front stars acceleration");

        for (int index = 0; index < CeresFlightActorDefinitions.RearViewActorCount; index++)
        {
            CeresFlightActorDefinition actor = CeresFlightActorDefinitions.RearViewActor(index);
            int[] spawnOperands = [0x8bbe3c, 0x8bbe42, 0x8bbe48, 0x8bbe4e, 0x8bbe57];
            AssertEqual((byte)0xa0, retail.ReadCartridgeByte(spawnOperands[index] - 1), "native rear spawn LDY");
            ushort definition = CartridgeImportSource.Require(retail).ReadCartridgeWord(spawnOperands[index]);
            VerifyActor(retail, actor, definition, $"rear actor {index}");
            AssertEqual((ushort)0, actor.InitialTimer, "rear initializer leaves cleared timer");
            if (index != 4)
                AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((CeresFlightActorDefinitions.NativeBank | (definition + 2))),
                    actor.ActivePreInstruction, "rear initializer preserves the definition callback");
            switch (index)
            {
                case 0:
                    VerifyWrappedActor(retail, actor, new WrappedActorRomFields(0x8bbf23, 0x8bbf29, 0x8bbf2f, 0x8bbf3a, 0x8bbf46), "large asteroids");
                    break;
                case 1:
                    VerifyWrappedActor(retail, actor, new WrappedActorRomFields(0x8bbf4d, 0x8bbf53, 0x8bbf59, 0x8bbf64, 0x8bbf70), "Ceres under attack");
                    break;
                case 2:
                    VerifyWrappedActor(retail, actor, new WrappedActorRomFields(0x8bbf77, 0x8bbf7d, 0x8bbf83, 0x8bbf8e, 0x8bbf9a), "small asteroids");
                    break;
                case 3:
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfb4), actor.Position.X, "rear vortex X");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfba), actor.Position.Y, "rear vortex Y");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfc0), actor.Attributes,
                        "rear vortex attributes");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfcb),
                        unchecked((ushort)-actor.HorizontalDelta), "rear vortex speed magnitude");
                    AssertTrue(!actor.WrapX, "rear vortex uses signed non-wrapping motion");
                    break;
                case 4:
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbea3), actor.Position.X, "rear stars X");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbea9), actor.Position.Y, "rear stars Y");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbeaf), actor.Attributes,
                        "rear stars attributes");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe9d), actor.ActivePreInstruction,
                        "rear stars initializer callback override");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfcb),
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
        AssertEqual((ushort)1, CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe51), "rear vortex native initializer parameter");
        AssertEqual((ushort)1, CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe5a), "rear stars native initializer parameter");
        AssertEqual(front.InstructionList, CeresFlightActorDefinitions.RearViewActor(4).InstructionList, "front/rear program alias");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((CeresFlightActorDefinitions.NativeBank | (frontDefinition + 2))),
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
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord((address + 4)), actor.InstructionList,
                $"{name} instruction list");
        }

        static void VerifyWrappedActor(
            ISnesAddressSpace source,
            CeresFlightActorDefinition actor,
            WrappedActorRomFields fields,
            string name)
        {
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord(fields.XWord), actor.Position.X, $"{name} X");
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord(fields.YWord), actor.Position.Y, $"{name} Y");
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord(fields.AttributeWord), actor.Attributes,
                $"{name} attributes");
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord(fields.DeltaWord), unchecked((ushort)actor.HorizontalDelta),
                $"{name} speed");
            AssertEqual((ushort)0x01ff, CartridgeImportSource.Require(source).ReadCartridgeWord(fields.MaskWord), $"{name} wrap mask");
            AssertTrue(actor.WrapX, $"{name} uses 512-pixel wrapping motion");
        }
    }

}
