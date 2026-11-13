using SuperMetroid.Verification.Frontend.Intro;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;
namespace SuperMetroid.Verification.Frontend.CeresCinematics;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Checks front and rear Ceres flight actor metadata against the native setup instructions.</summary>
internal static class CeresFlightActorMetadataVerification
{
    /// <summary>The bank of the flight's actor definitions and initializers, which these checks read.</summary>
    private const int ActorBank = 0x8b0000;

    /// <summary>Compares actor programs, positions, callbacks, speeds, and nine-bit wrap masks with bank $8B.</summary>
    internal static void VerifyCeresFlightActorMetadata(ISnesAddressSpace retail)
    {
        CeresFlightActorDefinition front = CeresFlightActorDefinitions.FrontStars;
        ushort frontDefinition = CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe57);
        Suite(nameof(VerifyActor), () => VerifyActor(retail, front, frontDefinition, "front stars"));
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe84), front.InitialVelocity.RawValue, "front stars initial speed");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe8a), CeresFlightActorDefinitions.FrontStarsStart.X, "front stars X");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe90), CeresFlightActorDefinitions.FrontStarsStart.Y, "front stars Y");
        PaletteRowWords.AssertSelectsOnlyPaletteRow(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe96), CeresSheetInks.SpriteSet(front.Colouring), "front stars attributes");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbec3),
            CeresFlightActorDefinitions.FrontStarAcceleration.RawValue,
            "front stars acceleration");

        for (int index = 0; index < CeresFlightActorDefinitions.RearViewActorCount; index++)
        {
            CeresFlightActorDefinition actor = CeresFlightActors.RearViewActor(index);
            int[] spawnOperands = [0x8bbe3c, 0x8bbe42, 0x8bbe48, 0x8bbe4e, 0x8bbe57];
            AssertEqual((byte)0xa0, retail.ReadCartridgeByte(spawnOperands[index] - 1), "native rear spawn LDY");
            ushort definition = CartridgeImportSource.Require(retail).ReadCartridgeWord(spawnOperands[index]);
            VerifyActor(retail, actor, definition, $"rear actor {index}");
            AssertEqual(SignedEightEight.Zero, actor.InitialVelocity, "rear initializer leaves cleared timer");
            if (index != 4)
                AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((ActorBank | (definition + 2))),
                    actor.ActiveFrameBehavior.NativeAddress, "rear initializer preserves the definition callback");
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
                    PaletteRowWords.AssertSelectsOnlyPaletteRow(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfc0), CeresSheetInks.SpriteSet(actor.Colouring),
                        "rear vortex attributes");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfcb),
                        (-actor.HorizontalDelta).Fraction, "rear vortex speed magnitude");
                    AssertTrue(!actor.WrapX, "rear vortex uses signed non-wrapping motion");
                    break;
                case 4:
                    PaletteRowWords.AssertSelectsOnlyPaletteRow(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbeaf), CeresSheetInks.SpriteSet(actor.Colouring),
                        "rear stars attributes");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe9d), actor.ActiveFrameBehavior.NativeAddress,
                        "rear stars initializer callback override");
                    AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbfcb),
                        (-actor.HorizontalDelta).Fraction, "rear stars speed magnitude");
                    AssertTrue(!actor.WrapX, "rear stars use signed non-wrapping motion");
                    break;
                default:
                    break;
            }
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresFlightActors.RearViewActor(
                CeresFlightActorDefinitions.RearViewActorCount),
            "Ceres rear-view actor definition boundary");

        foreach (int index in new[] { int.MinValue, -1, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => CeresFlightActors.RearViewActor(index), "rear metadata invalid index");
        AssertEqual((ushort)1, CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe51), "rear vortex native initializer parameter");
        AssertEqual((ushort)1, CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bbe5a), "rear stars native initializer parameter");
        AssertEqual(front.Animation, CeresFlightActors.RearViewActor(4).Animation, "front/rear program alias");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((ActorBank | (frontDefinition + 2))),
            front.ActiveFrameBehavior.NativeAddress, "front stars preserve the definition callback");
        AssertEqual(SignedSixteenSixteen.Zero, front.HorizontalDelta, "front stars have callback-driven acceleration instead of fixed X velocity");
        AssertEqual(false, front.WrapX, "front stars have no nine-bit wrap");
        static void VerifyActor(
            ISnesAddressSpace source,
            CeresFlightActorDefinition actor,
            ushort definitionPointer,
            string name)
        {
            int address = ActorBank | definitionPointer;
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWord((address + 4)), CinematicSpriteNativeListAddresses.Entry(actor.Animation),
                $"{name} instruction list");
        }

        static void VerifyWrappedActor(
            ISnesAddressSpace source,
            CeresFlightActorDefinition actor,
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
