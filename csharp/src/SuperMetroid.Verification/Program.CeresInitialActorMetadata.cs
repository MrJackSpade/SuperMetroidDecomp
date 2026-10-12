using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class CeresInitialActorMetadataVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres initial actor metadata")]
    internal static void VerifyCeresInitialActorMetadata(ISnesAddressSpace retail)
    {
        for (int index = 0; index < CeresDestructionActorDefinitions.InitialActorCount; index++)
        {
            CeresDestructionActorDefinition actor =
                CeresDestructionActorDefinitions.InitialActor(index);
            int[] spawnOperands = [0x8bc27d, 0x8bc283, 0x8bc28e];
            AssertEqual((byte)0xa0, retail.ReadCartridgeByte(spawnOperands[index] - 1), "native destruction spawn LDY");
            // The spawn operand names the native definition this compiled actor must match.
            VerifyActor(retail, CartridgeImportSource.Require(retail).ReadCartridgeWord(spawnOperands[index]), actor,
                preservesDefinitionCallback: index != 2, $"initial destruction actor {index}");
            AssertEqual((ushort)0, actor.SlideAcceleration, "destruction actor has no slide acceleration");
            AssertEqual((ushort)0, actor.SlidePreInstruction, "destruction actor has no slide callback");
            AssertEqual(false, actor.CompletesScene, "destruction actor does not own Zebes completion");
        }
        Suite(nameof(VerifyWrappedActor), () => VerifyWrappedActor(retail, CeresDestructionActorDefinitions.InitialActor(0), new WrappedActorRomFields(0x8bbf23, 0x8bbf29, 0x8bbf2f, 0x8bbf3a, 0x8bbf46), "destruction large asteroids"));
        Suite(nameof(VerifyWrappedActor), () => VerifyWrappedActor(retail, CeresDestructionActorDefinitions.InitialActor(1), new WrappedActorRomFields(0x8bbf77, 0x8bbf7d, 0x8bbf83, 0x8bbf8e, 0x8bbf9a), "destruction small asteroids"));
        CeresDestructionActorDefinition vortex =
            CeresDestructionActorDefinitions.InitialActor(2);
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfa6), vortex.Position.X, "destruction vortex X");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfba), vortex.Position.Y, "destruction vortex Y");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfc0), vortex.Attributes,
            "destruction vortex attributes");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfac), vortex.ActivePreInstruction,
            "destruction vortex no-op callback override");

        AssertEqual((byte)0xa9, retail.ReadCartridgeByte(0x8bc292), "stationary vortex parameter opcode");
        AssertEqual((ushort)0, CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc293), "stationary vortex parameter zero");
        AssertEqual((byte)0x60, retail.ReadCartridgeByte(0x8b0000 | vortex.ActivePreInstruction), "stationary vortex RTS");
        AssertEqual(0, vortex.HorizontalDelta, "stationary vortex has no displacement");
        AssertEqual(false, vortex.WrapX, "stationary vortex has no wrap operation");
        foreach (int index in new[] { int.MinValue, -1, 3, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => CeresDestructionActorDefinitions.InitialActor(index), "destruction actor invalid index");

        static void VerifyActor(
            ISnesAddressSpace source,
            ushort definitionPointer,
            CeresDestructionActorDefinition actor,
            bool preservesDefinitionCallback,
            string name)
        {
            int address = CeresDestructionActorDefinitions.NativeBank | definitionPointer;
            // The asteroid initializers keep the definition's pre-instruction; only the
            // vortex's parameter-zero initializer replaces it with the no-op override.
            if (preservesDefinitionCallback)
                AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord((address + 2)), actor.ActivePreInstruction,
                    $"{name} initializer preserves definition pre-instruction");
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord((address + 4)), actor.InstructionList,
                $"{name} instruction list");
        }

        static void VerifyWrappedActor(
            ISnesAddressSpace source,
            CeresDestructionActorDefinition actor,
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
