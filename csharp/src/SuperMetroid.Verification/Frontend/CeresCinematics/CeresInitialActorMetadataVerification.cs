using SuperMetroid.Verification.Frontend.Intro;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;
namespace SuperMetroid.Verification.Frontend.CeresCinematics;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Checks the three Ceres destruction startup actors against their spawn operands and native definitions.</summary>
internal static class CeresInitialActorMetadataVerification
{
    /// <summary>The bank of the destruction's actor definitions, which these checks read.</summary>
    private const int ActorBank = 0x8b0000;

    /// <summary>Compares asteroid motion and vortex stationary metadata, including callback selection and undefined roles.</summary>
    internal static void VerifyCeresInitialActorMetadata(ISnesAddressSpace retail)
    {
        // The destruction's setup spawns its actors with these spawn instructions, in spawn order.
        int[] spawnOperands = [0x8bc27d, 0x8bc283, 0x8bc28e];
        AssertEqual(spawnOperands.Length, CeresDestructionActorDefinitions.DestructionSpawnOrder.Count, "one spawn instruction per actor");
        for (int index = 0; index < spawnOperands.Length; index++)
        {
            CeresDestructionActorRole role = CeresDestructionActorDefinitions.DestructionSpawnOrder[index];
            CeresDestructionActorDefinition actor = CeresDestructionActors.InitialActor(role);
            AssertEqual((byte)0xa0, retail.ReadCartridgeByte(spawnOperands[index] - 1), "native destruction spawn LDY");
            // The spawn operand names the native definition this compiled actor must match.
            VerifyActor(retail, CartridgeImportSource.Require(retail).ReadCartridgeWord(spawnOperands[index]), actor,
                preservesDefinitionCallback: role != CeresDestructionActorRole.Vortex, $"destruction {role}");
            AssertEqual(SignedEightEight.Zero, actor.SlideAcceleration, "destruction actor has no slide acceleration");
            AssertEqual((ushort)0, actor.SlideFrameBehavior.NativeAddress, "destruction actor has no slide callback");
            AssertEqual(false, actor.CompletesScene, "destruction actor does not own Zebes completion");
        }
        Suite(nameof(VerifyWrappedActor), () => VerifyWrappedActor(retail, CeresDestructionActors.InitialActor(CeresDestructionActorRole.LargeAsteroids), new WrappedActorRomFields(0x8bbf23, 0x8bbf29, 0x8bbf2f, 0x8bbf3a, 0x8bbf46), "destruction large asteroids"));
        Suite(nameof(VerifyWrappedActor), () => VerifyWrappedActor(retail, CeresDestructionActors.InitialActor(CeresDestructionActorRole.SmallAsteroids), new WrappedActorRomFields(0x8bbf77, 0x8bbf7d, 0x8bbf83, 0x8bbf8e, 0x8bbf9a), "destruction small asteroids"));
        CeresDestructionActorDefinition vortex =
            CeresDestructionActors.InitialActor(CeresDestructionActorRole.Vortex);
        PaletteRowWords.AssertSelectsOnlyPaletteRow(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfc0), CeresSheetInks.SpriteSet(vortex.Colouring),
            "destruction vortex attributes");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfac), vortex.ActiveFrameBehavior.NativeAddress,
            "destruction vortex no-op callback override");

        AssertEqual((byte)0xa9, retail.ReadCartridgeByte(0x8bc292), "stationary vortex parameter opcode");
        AssertEqual((ushort)0, CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc293), "stationary vortex parameter zero");
        AssertEqual((byte)0x60, retail.ReadCartridgeByte(0x8b0000 | vortex.ActiveFrameBehavior.NativeAddress), "stationary vortex RTS");
        AssertEqual(SignedSixteenSixteen.Zero, vortex.HorizontalDelta, "stationary vortex has no displacement");
        AssertEqual(false, vortex.WrapX, "stationary vortex has no wrap operation");
        foreach (int undefined in new[] { int.MinValue, -1, Enum.GetValues<CeresDestructionActorRole>().Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => CeresDestructionActors.InitialActor((CeresDestructionActorRole)undefined), "destruction actor undefined role");

        static void VerifyActor(
            ISnesAddressSpace source,
            ushort definitionPointer,
            CeresDestructionActorDefinition actor,
            bool preservesDefinitionCallback,
            string name)
        {
            int address = ActorBank | definitionPointer;
            // The asteroid initializers keep the definition's pre-instruction; only the
            // vortex's parameter-zero initializer replaces it with the no-op override.
            if (preservesDefinitionCallback)
                AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord((address + 2)), actor.ActiveFrameBehavior.NativeAddress,
                    $"{name} initializer preserves definition pre-instruction");
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord((address + 4)), CinematicSpriteNativeListAddresses.Entry(actor.Animation),
                $"{name} instruction list");
        }

        static void VerifyWrappedActor(
            ISnesAddressSpace source,
            CeresDestructionActorDefinition actor,
            WrappedActorRomFields fields,
            string name)
        {
            PaletteRowWords.AssertSelectsOnlyPaletteRow(CartridgeImportSource.Require(source).ReadCartridgeWord(fields.AttributeWord), CeresSheetInks.SpriteSet(actor.Colouring),
                $"{name} attributes");
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord(fields.DeltaWord), actor.HorizontalDelta.Fraction,
                $"{name} speed");
            AssertEqual((ushort)0x01ff, CartridgeImportSource.Require(source).ReadCartridgeWord(fields.MaskWord), $"{name} wrap mask");
            AssertTrue(actor.WrapX, $"{name} uses 512-pixel wrapping motion");
        }
    }

}
