using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Escape.Artwork;
using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Verification.Areas;

/// <summary>
/// Confirms #1415's area migration: each deleted Ceres, Maridia, Tourian and escape catalog's definitions still produce,
/// for one member of each kind, the values the catalog held; the sources that moved to extraction keep their cartridge
/// locations; and the native-word tables translate the arbitrary identities to the cartridge's routine and list words.
/// None reads the ROM.
/// </summary>
internal static class AreaDefinitionMigrationVerification
{
    /// <summary>Compares the new definitions, extraction sources and identity tables with the values the deleted catalogs held.</summary>
    internal static void VerifyAreaDefinitionMigration()
    {
        // The Ceres actors' routine words and the floor animation's list words are verification's; the identities' own
        // values are arbitrary.
        AssertEqual(0xf7a5, CeresNativeWords.DoorFunctions.Word(CeresDoorFunction.RidleyEscapeFloorWall), "escape-floor wall function word");
        AssertEqual(CeresDoorFunction.ElevatorAnimation, CeresNativeWords.DoorFunctions.Decode(0xf850, "door function"), "elevator animation decodes");
        AssertEqual(0xf019, CeresNativeWords.SteamFunctions.Word(CeresSteamFunction.ApplyRotatingElevatorOffset), "rotating steam function word");
        AssertEqual(0xad80, CeresNativeWords.FloorAnimationFrames.Word(CeresFloorAnimationFrame.WingFrame1), "second wing frame list word");
        AssertEqual(CeresFloorAnimationFrame.BabyFrame1, CeresFloorAnimation.BabyFrameForPhase(3), "the capsule's fourth phase reuses frame one");
        AssertTrue(CeresFloorDrawings.Cells(CeresFloorAnimationFrame.BabyFrame1).ContainsKey(11 * 128 + 4), "the capsule's lower run's floor cell");
        AssertEqual(6, CeresFloorDrawingDefinitions.Frames[CeresFloorAnimationFrame.WingFrame0].Count, "the wings' six runs");

        // CeresDoorVisualRomData: its counts are Core's, its sources extraction's.
        AssertEqual(0x0400, CeresDoorVisualFormat.CharacterBytes, "door character bytes");
        AssertEqual(15, CeresDoorVisualDefinitions.SetupColorCount, "door setup colours");
        AssertEqual(8, CeresDoorVisualDefinitions.AnimationStepCount, "door glow rows");
        AssertEqual(6, CeresDoorVisualDefinitions.AnimationColorCount, "door glow colours");
        AssertTrue(CeresFloorDrawings.Cells(CeresFloorAnimationFrame.ElevatorLight).Keys.Order().SequenceEqual(Enumerable.Range(12 * 128 + 14, 4)),
            "the platform's four floor cells");
        AssertEqual(0xb0c400, CeresSourceDefinitions.Door.Characters, "door character source");
        AssertEqual(0xa6f4ee, CeresSourceDefinitions.Door.NormalColors, "door normal colour source");
        AssertEqual(0x10, CeresSourceDefinitions.Door.AnimationRowBytes, "door glow row stride");
        AssertEqual(0xa6f918, CeresSourceDefinitionsDoorConstants.PlatformFrames, "platform strip source");
        AssertEqual(SuperMetroid.Core.Enemies.SpriteGraphics.SharedIn(
                SuperMetroid.Core.Assets.PictureShading.Shaded(SuperMetroid.Core.Areas.Ceres.Artwork.CeresElevatorShades.ConcealerCleared)),
            CeresElevatorArrivalDefinitions.StartingGraphics, "arrival graphics: tile base zero, with the concealer's colour cleared");

        // CeresEscapeTileRomData, EscapeTimerTileRomData and the escape text's source.
        AssertEqual(0x0900, CeresEscapeTileArtworkDefinitions.WarningText.ByteCount, "warning text bytes");
        AssertEqual(0x0600, CeresEscapeTileArtworkDefinitions.Doors.ByteCount, "escape door bytes");
        AssertEqual(0xb7da00, (int)EscapeSourceDefinitions.TextCharacters, "warning text source");
        AssertEqual(0xb0ba00, (int)EscapeSourceDefinitions.CeresEscapeDoorCharacters, "escape door source");
        AssertEqual(0xb0c000, (int)EscapeSourceDefinitions.TimerCharacters, "timer character source");
        AssertEqual(0xa6c43a, CeresEscapeNativeSources.OverlaySource(CeresEscapeWarningPictures.JapaneseFourth), "last subtitle row source");
        AssertEqual(0x0800, EscapeTextPages.PageOffset(4), "fifth escape text page offset");

        // MaridiaElevatubeRomData and MaridiaElevatubeObjectRomData.
        AssertEqual(0x0080, MaridiaElevatubeDefinitions.SamusCenterX, "elevatube Samus X");
        AssertEqual(0x09c0, MaridiaElevatubeDefinitions.SouthStartingY, "south entry position");
        AssertEqual(0xff00, MaridiaElevatubeDefinitions.SouthStartingVelocity.RawValue, "south entry velocity");
        AssertEqual(0xffe0, MaridiaElevatubeDefinitions.SouthAcceleration.RawValue, "south entry acceleration");
        AssertEqual(0x0040, MaridiaElevatubeDefinitions.NorthStartingY, "north entry position");
        AssertEqual(0x0100, MaridiaElevatubeDefinitions.NorthStartingVelocity.RawValue, "north entry velocity");
        AssertEqual(0x0020, MaridiaElevatubeDefinitions.NorthAcceleration.RawValue, "north entry acceleration");
        AssertEqual(0x0e20, MaridiaElevatubeDefinitions.MaximumSpeed.RawValue, "elevatube speed limit");
        AssertEqual(new Position16<RoomBlocks>(1, 0), MaridiaElevatubeDefinitions.RoomObjectBlock, "elevatube PLM block");

        // TourianStatueRomData and TourianStatuePaletteRomData.
        AssertEqual(300, TourianStatueSequenceDefinitions.DescentDelay, "descent delay");
        AssertEqual(SignedSixteenSixteen.FromWholePixels(1), TourianStatueSequenceDefinitions.DescentStep * 4, "the descent steps a quarter pixel");
        AssertEqual(240, TourianStatueSequenceDefinitions.DescentDistance, "descent distance");
        AssertEqual(13, NativeEarthquakeTypes.TypeOf(TourianStatueSequenceDefinitions.DescentEarthquake), "descent earthquake");
        AssertEqual(1, NativeEarthquakeTypes.TypeOf(TourianStatueSequenceDefinitions.ReleaseEarthquake), "release earthquake");
        AssertEqual(0x20, RoomEarthquakeDefinitions.SustainedShakeFrames, "descent and release earthquake timer bit");
        AssertEqual(16, TourianStatueColorFormat.BaseColorCount, "statue base colours");
        AssertEqual(16, TourianStatueColorFormat.StatueColorCount, "statue colours");
        AssertEqual(4, TourianStatueColorDefinitions.EyeGlowCount, "eye rows");
        AssertEqual(4, TourianStatueColorDefinitions.EyeColorCount, "eye colours");
        AssertEqual(8, TourianStatueColorDefinitions.GreyColorCount, "grey colours");
        AssertEqual(0x86b91e, EnemySourceDefinitions.TourianStatueEyeColors, "eye colour source");
        AssertEqual(0x87839c, EnemySourceDefinitions.TourianStatueGreyColors, "grey colour source");

        // EscapeTypewriterRomData, ZebesEscapeRomData and the escape timer's cleared origin.
        AssertEqual(0x2610, EscapeTypewriterTileDefinitions.ZebesTileBase.PackedWord, "Zebes glyph base");
        AssertEqual(65, EscapeTypewriterTileDefinitions.FirstLetter, "first letter");
        AssertEqual(91, EscapeTypewriterTileDefinitions.ExclamationGlyph, "exclamation glyph");
        AssertEqual<SoundEffectId>(RoomSound.IntroTypewriter, EscapeTypewriterDefinitions.ZebesClick, "Zebes click");
        AssertEqual<SoundEffectId>(EnemySound.CeresTypewriter, EscapeTypewriterDefinitions.CeresClick, "Ceres click");
        AssertEqual(42, ZebesEscapeDefinitions.DiagonalFrames, "diagonal shake frames");
        AssertEqual(18, NativeEarthquakeTypes.TypeOf(ZebesEscapeDefinitions.LightHorizontal), "light horizontal shake");
        AssertEqual(21, NativeEarthquakeTypes.TypeOf(ZebesEscapeDefinitions.MediumHorizontal), "medium horizontal shake");
        AssertEqual(23, NativeEarthquakeTypes.TypeOf(ZebesEscapeDefinitions.MediumDiagonal), "medium diagonal shake");
        AssertEqual(26, NativeEarthquakeTypes.TypeOf(ZebesEscapeDefinitions.StrongDiagonal), "strong diagonal shake");
        AssertEqual(24, NativeEarthquakeTypes.TypeOf(ZebesEscapeDefinitions.MainstreetQuake), "Mainstreet quake");
        // The cartridge skips outdoor explosions over its blank tile, 255: the block set's empty block shows nothing.
        AssertTrue(SuperMetroid.Core.Rooms.LevelData.RoomBlockAppearance.Of(255).ShowsNothing && !SuperMetroid.Core.Rooms.LevelData.RoomBlockAppearance.Of(254).ShowsNothing, "the blank explosion tile is the block that shows nothing");
        AssertEqual(6, NativeEarthquakeTypes.TypeOf(ZebesEscapeDefinitions.LandingQuake), "landing site quake");
        AssertEqual(128, EscapeTimerDefinitions.ClearedPixel, "timer cleared origin");
        var earthquake = new RoomEarthquake();
        earthquake.KeepShaking();
        AssertTrue(earthquake.IsShaking, "a continuous shake shakes the room without a length, as the timer's top bit did");
        Console.WriteLine("Area definition migration: native words, Ceres, Maridia, Tourian and escape definitions and extraction sources keep their values.");
    }

    /// <summary>Runs the focused area definition-migration check.</summary>
    [VerificationCommand("--area-definition-migration")]
    internal static int AreaDefinitionMigrationCommand()
    {
        VerifyAreaDefinitionMigration();
        return 0;
    }
}
