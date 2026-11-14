using SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks six Ceres planet, star, and title actor records and the Japanese subtitle's against native setup metadata.</summary>
internal static class CeresZebesActorMetadataVerification
{
    /// <summary>Compares spawn, slide, and completion callbacks while rejecting roles outside the reveal.</summary>
    internal static void VerifyCeresZebesActorMetadata(ISnesAddressSpace retail)
    {
        int[] attributeAddresses = [0x8bc848, 0x8bc950, 0x8bc964, 0x8bc978, 0x8bc98c, 0x8bc99f];
        // The reveal's setup spawns its actors with consecutive spawn instructions, in spawn order.
        for (int index = 0; index < CeresDestructionActorDefinitions.RevealSpawnOrder.Count; index++)
        {
            ZebesRevealActorRole role = CeresDestructionActorDefinitions.RevealSpawnOrder[index];
            CeresDestructionActorDefinition actor = CeresDestructionActors.ZebesActor(role);
            int spawnOperand = 0x8bc811 + 6 * index;
            AssertEqual((byte)0xa0, retail.ReadCartridgeByte(spawnOperand - 1), "native spawn LDY");
            ushort pointer = CartridgeImportSource.Require(retail).ReadCartridgeWord(spawnOperand);
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(((0x8b0000 | pointer) + 2)), actor.ActiveFrameBehavior.NativeAddress,
                "native reveal callback, which the initializer leaves unchanged");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(((0x8b0000 | pointer) + 4)), CinematicSpriteNativeListAddresses.Entry(actor.Animation), "native reveal program");
            AssertEqual(SignedSixteenSixteen.Zero, actor.HorizontalDelta, "reveal actor has no horizontal motion");
            AssertEqual(false, actor.WrapX, "reveal actor has no horizontal wrap");
            AssertEqual(role == ZebesRevealActorRole.StarsLowerRight, actor.CompletesScene,
                "only the lower-right stars' native callback at C8F2 loads game");
            PaletteRowWords.AssertSelectsOnlyPaletteRow(CartridgeImportSource.Require(retail).ReadCartridgeWord(attributeAddresses[index]), CeresSheetInks.SpriteSet(actor.Colouring),
                $"Zebes reveal {role} attributes");
        }

        CeresDestructionActorDefinition planet =
            CeresDestructionActors.ZebesActor(ZebesRevealActorRole.Planet);
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc857), planet.SlideFrameBehavior.NativeAddress,
            "Zebes planet slide callback");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc862), planet.SlideAcceleration.RawValue,
            "Zebes planet slide acceleration");
        foreach (ZebesRevealActorRole quarter in new[]
            { ZebesRevealActorRole.StarsUpperLeft, ZebesRevealActorRole.StarsUpperRight, ZebesRevealActorRole.StarsLowerLeft })
        {
            CeresDestructionActorDefinition star = CeresDestructionActors.ZebesActor(quarter);
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc902), star.SlideFrameBehavior.NativeAddress,
                $"Zebes {quarter} slide callback");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc90d), star.SlideAcceleration.RawValue,
                $"Zebes {quarter} slide acceleration");
        }
        CeresDestructionActorDefinition completionStar =
            CeresDestructionActors.ZebesActor(ZebesRevealActorRole.StarsLowerRight);
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc8b3), completionStar.SlideFrameBehavior.NativeAddress,
            "Zebes completion star slide callback");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc8be), completionStar.SlideAcceleration.RawValue,
            "Zebes completion star slide acceleration");
        AssertTrue(completionStar.CompletesScene,
            "Zebes lower-right stars retain scene-completion ownership");
        CeresDestructionActorDefinition title =
            CeresDestructionActors.ZebesActor(ZebesRevealActorRole.Title);
        AssertEqual((ushort)0, title.SlideFrameBehavior.NativeAddress,
            "PLANET ZEBES title cannot enter slide motion");
        AssertEqual(SignedEightEight.Zero, title.SlideAcceleration,
            "PLANET ZEBES title has no slide acceleration");

        foreach (int undefined in new[] { int.MinValue, -1, Enum.GetValues<ZebesRevealActorRole>().Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(
                () => CeresDestructionActors.ZebesActor((ZebesRevealActorRole)undefined), "Zebes metadata undefined role");

        // The title's Japanese-text instruction ($8B:C9AF) spawns the subtitle's definition ($8B:CEB5), whose initializer
        // ($8B:C9E6) selects the title's sprite colours and whose list ends before the slide (#1733).
        CeresDestructionActorDefinition subtitle = CeresDestructionActors.ZebesActor(ZebesRevealActorRole.JapaneseSubtitle);
        AssertEqual((byte)0xa0, retail.ReadCartridgeByte(0x8bc9b5), "subtitle spawn LDY");
        ushort subtitleDefinition = CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc9b6);
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((0x8b0000 | subtitleDefinition) + 2), subtitle.ActiveFrameBehavior.NativeAddress,
            "native subtitle callback");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((0x8b0000 | subtitleDefinition) + 4), CinematicSpriteNativeListAddresses.Entry(subtitle.Animation),
            "native subtitle program");
        PaletteRowWords.AssertSelectsOnlyPaletteRow(CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc9f3), CeresSheetInks.SpriteSet(subtitle.Colouring),
            "Japanese subtitle attributes");
        AssertEqual((ushort)0, subtitle.SlideFrameBehavior.NativeAddress, "the Japanese subtitle cannot enter slide motion");
        AssertTrue(!subtitle.CompletesScene && !subtitle.WrapX && subtitle.HorizontalDelta == SignedSixteenSixteen.Zero,
            "the Japanese subtitle neither moves nor ends the scene");

        // Native lower-right slide callback alone installs the load-game function.
        AssertEqual((byte)0xa9, retail.ReadCartridgeByte(0x8bc8f2), "completion load immediate opcode");
        AssertEqual((ushort)0xcadf, CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc8f3), "completion load-game function");
        AssertEqual((byte)0x8d, retail.ReadCartridgeByte(0x8bc8f5), "completion function store opcode");
        AssertEqual((ushort)0x1f51, CartridgeImportSource.Require(retail).ReadCartridgeWord(0x8bc8f6), "completion cinematic function destination");
    }
}
