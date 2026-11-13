using SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks six Ceres planet, star, and title actor records against native setup metadata.</summary>
internal static class CeresZebesActorMetadataVerification
{
    /// <summary>Compares spawn, slide, and completion callbacks while rejecting invalid actor ordinals.</summary>
    internal static void VerifyCeresZebesActorMetadata(ISnesAddressSpace retail)
    {
        int[] attributeAddresses = [0x8bc848, 0x8bc950, 0x8bc964, 0x8bc978, 0x8bc98c, 0x8bc99f];
        for (int index = 0; index < CeresDestructionActorDefinitions.ZebesActorCount; index++)
        {
            CeresDestructionActorDefinition actor =
                CeresDestructionActors.ZebesActor(index);
            int spawnOperand = 0x8bc811 + 6 * index;
            AssertEqual((byte)0xa0, retail.ReadCartridgeByte(spawnOperand - 1), "native spawn LDY");
            ushort pointer = CartridgeImportSource.Require(retail).ReadCartridgeWord(spawnOperand);
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(((0x8b0000 | pointer) + 2)), actor.ActiveFrameBehavior.NativeAddress,
                "native reveal callback, which the initializer leaves unchanged");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(((0x8b0000 | pointer) + 4)), CinematicSpriteNativeListAddresses.Entry(actor.Animation), "native reveal program");
            AssertEqual(SignedSixteenSixteen.Zero, actor.HorizontalDelta, "reveal actor has no horizontal motion");
            AssertEqual(false, actor.WrapX, "reveal actor has no horizontal wrap");
            AssertEqual(index == 4, actor.CompletesScene, "only native star5 callback at C8F2 loads game");
            PaletteRowWords.AssertSelectsOnlyPaletteRow(CartridgeImportSource.Require(retail).ReadCartridgeWord(attributeAddresses[index]), CeresSheetInks.SpriteSet(actor.Colouring),
                $"Zebes reveal actor {index} attributes");
        }

        CeresDestructionActorDefinition planet =
            CeresDestructionActors.ZebesActor(0);
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc857), planet.SlideFrameBehavior.NativeAddress,
            "Zebes planet slide callback");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc862), planet.SlideAcceleration.RawValue,
            "Zebes planet slide acceleration");
        for (int index = 1; index <= 3; index++)
        {
            CeresDestructionActorDefinition star =
                CeresDestructionActors.ZebesActor(index);
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc902), star.SlideFrameBehavior.NativeAddress,
                $"Zebes star {index + 1} slide callback");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc90d), star.SlideAcceleration.RawValue,
                $"Zebes star {index + 1} slide acceleration");
        }
        CeresDestructionActorDefinition completionStar =
            CeresDestructionActors.ZebesActor(4);
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc8b3), completionStar.SlideFrameBehavior.NativeAddress,
            "Zebes completion star slide callback");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc8be), completionStar.SlideAcceleration.RawValue,
            "Zebes completion star slide acceleration");
        AssertTrue(completionStar.CompletesScene,
            "Zebes stars 5 retains scene-completion ownership");
        CeresDestructionActorDefinition title =
            CeresDestructionActors.ZebesActor(5);
        AssertEqual((ushort)0, title.SlideFrameBehavior.NativeAddress,
            "PLANET ZEBES title cannot enter slide motion");
        AssertEqual(SignedEightEight.Zero, title.SlideAcceleration,
            "PLANET ZEBES title has no slide acceleration");

        foreach (int index in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => CeresDestructionActors.ZebesActor(index), "Zebes metadata invalid index");

        // Native lower-right slide callback alone installs the load-game function.
        AssertEqual((byte)0xa9, retail.ReadCartridgeByte(0x8bc8f2), "completion load immediate opcode");
        AssertEqual((ushort)0xcadf, CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc8f3), "completion load-game function");
        AssertEqual((byte)0x8d, retail.ReadCartridgeByte(0x8bc8f5), "completion function store opcode");
        AssertEqual((ushort)0x1f51, CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc8f6), "completion cinematic function destination");
    }
}
