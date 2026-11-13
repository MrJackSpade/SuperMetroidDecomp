using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Verification.Fx.PaletteFx;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>
/// Checks the Ceres cinematics' colour effects (#1733) against the cartridge: the station's navigation lights in the flight
/// (on its sprites) and in the destruction (on its turning floor), and the PLANET ZEBES caption's fades. Each is wired to the
/// spawn the cartridge makes, starts on the update the scene reaches that spawn, shows the record the cartridge's list shows
/// on every update from then until it ends, and draws with that record's installed shade, whose colours are the record's.
/// </summary>
internal static class CeresColourEffectsVerification
{
    /// <summary>The spawn of the flight's navigation lights in the rear view's setup, <c>$8B:BE6C</c>.</summary>
    private const int FlightLightsSpawn = 0x8bbe6c;
    /// <summary>The spawn of the destruction's navigation lights in the station's setup, <c>$8B:C29E</c>.</summary>
    private const int DestructionLightsSpawn = 0x8bc29e;
    /// <summary>The Zebes reveal's setup's clearing of every colour effect, <c>$8B:C771</c>.</summary>
    private const int RevealClear = 0x8bc771;
    /// <summary>The Zebes reveal's setup's spawn of the engine flicker, <c>$8B:C781</c>.</summary>
    private const int RevealEngineSpawn = 0x8bc781;
    /// <summary>The caption's fade-in instruction's spawn, <c>$8B:C9A6</c>.</summary>
    private const int FadeInSpawn = 0x8bc9a6;
    /// <summary>The caption's fade-out instruction's spawn, <c>$8B:C9BE</c>.</summary>
    private const int FadeOutSpawn = 0x8bc9be;
    /// <summary>The PLANET ZEBES caption's instruction list, <c>$8B:CCBB</c>.</summary>
    private const int CaptionList = 0x8bccbb;
    /// <summary>The caption's fade-in instruction.</summary>
    private const ushort FadeInInstruction = 0xc9a5;
    /// <summary>The caption's fade-out instruction.</summary>
    private const ushort FadeOutInstruction = 0xc9bd;
    /// <summary>The flight's navigation lights' definition in bank $8D.</summary>
    private const ushort FlightLightsDefinition = 0xe1ac;
    /// <summary>The destruction's navigation lights' definition in bank $8D.</summary>
    private const ushort DestructionLightsDefinition = 0xe1b8;
    /// <summary>The engine flicker's definition in bank $8D.</summary>
    private const ushort EngineDefinition = 0xe1a8;
    /// <summary>The caption's fade-in definition in bank $8D.</summary>
    private const ushort FadeInDefinition = 0xe1b0;
    /// <summary>The caption's fade-out definition in bank $8D.</summary>
    private const ushort FadeOutDefinition = 0xe1b4;
    /// <summary><c>Spawn_PaletteFXObject</c>, which each spawn calls.</summary>
    private const int SpawnRoutine = 0x8dc4e9;
    /// <summary><c>Clear_PaletteFXObjects</c>.</summary>
    private const int ClearRoutine = 0x8dc4d8;
    /// <summary>The immediate-Y load and long-call opcodes each spawn is made of.</summary>
    private const byte LoadY = 0xa0, LongCall = 0x22;

    /// <summary>
    /// The station's navigation lights: the flight's rear view's setup spawns the sprite lights and the destruction's setup
    /// the floor's; each scene starts them on the update it reaches that setup and shows the cartridge list's record on
    /// every update; the flight draws its station, and only its station, with the record's shade over its drawn colours, and
    /// the destruction its floor until the Zebes reveal clears the lights and starts the engine's flicker. Each installed
    /// record shade shows the record's colours.
    /// </summary>
    internal static void VerifyCeresNavigationLights(CartridgeImportAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        AssertSpawn(cartridge, FlightLightsSpawn, FlightLightsDefinition, CeresFlightSceneDefinitions.NavigationLights, "the flight's rear view");
        AssertSpawn(cartridge, DestructionLightsSpawn, DestructionLightsDefinition, CeresDestructionSceneDefinitions.NavigationLights, "the station's setup");
        AssertEqual(LongCall, Byte(cartridge, RevealClear), "the Zebes reveal's setup calls the colour effects' clear");
        AssertEqual(ClearRoutine, Long(cartridge, RevealClear + 1), "the Zebes reveal's setup clears every colour effect");
        AssertSpawn(cartridge, RevealEngineSpawn, EngineDefinition, CeresDestructionSceneDefinitions.GunshipEngineFlicker, "the Zebes reveal");

        VerifyFlightLights(cartridge);
        VerifyDestructionLights(cartridge);

        var native = new ReferencePaletteFxColorSource(bus);
        IPaletteFxColorSource installed = InstalledFixtures.ImportMaps.RoomPaletteFx;
        AssertTrue(InstalledShadeRecolourings(InstalledFixtures.SpriteSheets.Get(SpriteSheetId.CeresFlightSprites).Image,
            CeresSheetInks.StationNavigationLightShades(installed), CeresSheetInks.StationNavigationLightShades(native), "the station's sprite lights") > 0,
            "the installed flight sheet's station shows its lights");
        AssertTrue(InstalledShadeRecolourings(InstalledFixtures.SpriteSheets.Get(SpriteSheetId.CeresFloor).Image,
            CeresSheetInks.FloorNavigationLightShades(installed), CeresSheetInks.FloorNavigationLightShades(native), "the floor's lights") > 0,
            "the installed floor's station shows its lights");
        Console.WriteLine("  Ceres navigation lights: the flight's and the destruction's lights start at the cartridge's spawns, show the " +
            "cartridge list's record on every update until the Zebes reveal clears them, and their shades show the records' colours.");
    }

    /// <summary>
    /// The PLANET ZEBES caption's fades: the caption's list spawns the fade in as it first shows and the fade out as its hold
    /// ends; the scene starts each on that update, shows the record the lists show on every update to the scene's end, the
    /// fade out's last staying, and draws the caption with it. Each installed record shade shows the record's colours.
    /// </summary>
    internal static void VerifyPlanetZebesTextFades(CartridgeImportAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        // The caption's list: the blank hold's duration and pose, the fade in, the first hold's duration and pose, the
        // Japanese caption, the second hold's duration and pose, the fade out, then its last hold before the caption deletes
        // itself and starts the flight.
        ushort Caption(int word) => cartridge.ReadCartridgeWord(CaptionList + word * sizeof(ushort));
        AssertEqual(0, Caption(1), "the caption's list first holds the blank pose");
        AssertEqual(FadeInInstruction, Caption(2), "the caption's blank hold ends with the fade in");
        AssertEqual(FadeOutInstruction, Caption(8), "the caption's second hold ends with the fade out");
        int fadeOutAfter = Caption(3) + Caption(6);
        int lastHold = Caption(9);
        AssertSpawn(cartridge, FadeInSpawn, FadeInDefinition, CeresDestructionSceneDefinitions.ZebesTextFadeIn, "the fade-in instruction");
        AssertSpawn(cartridge, FadeOutSpawn, FadeOutDefinition, CeresDestructionSceneDefinitions.ZebesTextFadeOut, "the fade-out instruction");

        CeresDestructionCinematicState scene = InstalledFixtures.CreateDestruction();
        PictureShade drawn = CeresShades.Drawn(CeresSpriteColouring.ZebesTitle);
        NativeColourEffectRun? fadeIn = null, fadeOut = null;
        int? fadeInStart = null, fadeOutStart = null, deleted = null;
        for (int update = 0; !scene.Finished; update++)
        {
            AssertTrue(update < 6_000, "the Ceres destruction reaches its end");
            scene.Step();
            ImageSpriteDraw[] captions = [.. scene.CaptureRenderSnapshot().SpriteDraws.OfType<ImageSpriteDraw>()
                .Where(draw => draw.Shading is { } shading && shading.Shades.Contains(drawn))];
            if (fadeInStart is null && captions.Length > 0)
            {
                fadeInStart = update;
                fadeIn = new NativeColourEffectRun(cartridge, FadeInDefinition);
            }
            if (fadeInStart is { } fadedIn && fadeOutStart is null && update - fadedIn == fadeOutAfter)
            {
                fadeOutStart = update;
                fadeOut = new NativeColourEffectRun(cartridge, FadeOutDefinition);
            }
            fadeIn?.Step();
            fadeOut?.Step();
            PaletteFxFrame? shown = scene.PaletteFx.FrameOf(PaletteFxColours.PlanetZebesText);
            NativeColourEffectRun? showing = fadeOut ?? fadeIn;
            if (showing is null)
            {
                AssertTrue(shown is null, $"no caption fade shows before the caption first shows (update {update})");
                continue;
            }
            PaletteFxFrame frame = shown ?? throw new InvalidOperationException($"Destruction update {update} shows no caption fade.");
            AssertEqual(showing.Record, NativeColourEffectRun.RecordOf(frame),
                $"destruction update {update} shows the record the cartridge's {(fadeOut is null ? "fade in" : "fade out")} list shows");
            // The caption deletes itself once its last hold ends; the fade out's last record stays on its colours.
            if (deleted is null && captions.Length == 0 && fadeOutStart is { } fadedOut)
            {
                deleted = update;
                AssertEqual(lastHold, update - fadedOut, "the caption deletes itself as its last hold ends");
            }
            if (deleted is not null)
            {
                AssertEqual(0, captions.Length, $"destruction update {update} draws no caption once it has deleted itself");
                continue;
            }
            AssertEqual(1, captions.Length, $"destruction update {update} draws the caption once");
            AssertEqual(PictureShading.Shaded(drawn).With(PaletteFxShades.Of(PaletteFxColours.PlanetZebesText, frame)), captions[0].Shading,
                $"destruction update {update} draws the caption with the fade's record over its drawn colours");
        }
        AssertTrue(fadeIn is { Deleted: true } && fadeOut is { Deleted: true } && deleted is not null,
            "both caption fades run to their ends, and the caption deletes itself, before the scene ends");

        var native = new ReferencePaletteFxColorSource(bus);
        IPaletteFxColorSource installed = InstalledFixtures.ImportMaps.RoomPaletteFx;
        AssertTrue(InstalledShadeRecolourings(InstalledFixtures.SpriteSheets.Get(SpriteSheetId.ZebesRevealSprites).Image,
            CeresSheetInks.ZebesTitleFadeShades(installed), CeresSheetInks.ZebesTitleFadeShades(native), "the caption's fades") > 0,
            "the installed reveal sheet's caption shows its fades");
        Console.WriteLine($"  PLANET ZEBES fades: the fade in starts as the caption first shows and the fade out {fadeOutAfter} updates later, " +
            "each shows the cartridge list's record on every update, the fade out's last staying, and their shades show the records' colours.");
    }

    /// <summary>Runs the Ceres cinematics' colour effect checks against the retail cartridge.</summary>
    [VerificationCommand("--ceres-colour-effects")]
    internal static int CeresColourEffectsCommand()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCeresNavigationLights), () => VerifyCeresNavigationLights(bus));
        Suite(nameof(VerifyPlanetZebesTextFades), () => VerifyPlanetZebesTextFades(bus));
        return 0;
    }

    /// <summary>
    /// The flight to Ceres to its end: no lights before the rear view; from the update its setup runs, the cartridge list's
    /// record on every update, and the station drawn with it.
    /// </summary>
    private static void VerifyFlightLights(IImportCartridgeSource cartridge)
    {
        var audio = AudioQueues.Standalone();
        var flight = new IntroCeresFlightState(audio, InstalledFixtures.IntroArtwork.BoundCatalog.CeresFlight, sheets: InstalledFixtures.SpriteSheets);
        PictureShade station = CeresShades.Drawn(CeresSpriteColouring.Station);
        NativeColourEffectRun? lights = null;
        int stationDraws = 0;
        for (int update = 0; !flight.Finished; update++)
        {
            AssertTrue(update < 6_000, "the flight to Ceres reaches its end");
            bool rearView = flight.ShowsRearView;
            flight.Step();
            audio.AdvanceFrame(default);
            if (!rearView && flight.ShowsRearView)
                lights = new NativeColourEffectRun(cartridge, FlightLightsDefinition);
            lights?.Step();
            PaletteFxFrame? shown = flight.ColourEffects.FrameOf(PaletteFxColours.CeresNavigationLightsSprite);
            PictureShade? shade = shown is { } frame ? PaletteFxShades.Of(PaletteFxColours.CeresNavigationLightsSprite, frame) : null;
            if (lights is null)
                AssertTrue(shown is null, $"the flight shows no navigation lights before its rear view (update {update})");
            else
                AssertEqual(lights.Record, NativeColourEffectRun.RecordOf(shown ?? throw new InvalidOperationException($"Flight update {update} shows no lights.")),
                    $"flight update {update} shows the record the cartridge's lights list shows");
            foreach (ImageSpriteDraw draw in flight.CaptureRenderSnapshot().SpriteDraws.OfType<ImageSpriteDraw>())
            {
                PictureShading shading = draw.Shading ?? throw new InvalidOperationException($"Flight update {update} drew {draw} in no shading.");
                if (shading.Shades.Contains(station) && shade is { } lit)
                {
                    stationDraws++;
                    AssertEqual(PictureShading.Shaded(station).With(lit), shading, $"flight update {update} draws the station with the lights' record");
                }
                else
                {
                    AssertTrue(shade is null || !shading.Shades.Contains(shade.Value), $"flight update {update} draws nothing but the station with the lights");
                }
            }
        }
        AssertTrue(lights is not null && stationDraws > 0, "the flight's rear view draws the station with its lights");
    }

    /// <summary>
    /// The destruction from its first update to the Zebes reveal's fade in: the floor's lights from the station's setup, the
    /// cartridge list's record on every update and the floor shaded with it, then none once the reveal starts the engine.
    /// </summary>
    private static void VerifyDestructionLights(IImportCartridgeSource cartridge)
    {
        CeresDestructionCinematicState scene = InstalledFixtures.CreateDestruction();
        NativeColourEffectRun? lights = null;
        bool revealed = false;
        for (int update = 0; !revealed; update++)
        {
            AssertTrue(update < 6_000, "the Ceres destruction reaches the Zebes reveal");
            bool setUp = scene.FloorShot is not null;
            scene.Step();
            revealed = scene.ZebesRevealed;
            if (!setUp && scene.FloorShot is not null)
                lights = new NativeColourEffectRun(cartridge, DestructionLightsDefinition);
            lights?.Step();
            PaletteFxFrame? shown = scene.PaletteFx.FrameOf(PaletteFxColours.CeresNavigationLightsBackground);
            if (revealed)
            {
                AssertTrue(shown is null && scene.PaletteFx.FrameOf(PaletteFxColours.CutsceneGunshipEngine) is not null,
                    $"the Zebes reveal (update {update}) clears the navigation lights and shows the engine's flicker");
                AssertEqual(1, scene.FloorShading.Shades.Length, "the revealed floor shows the engine's flicker alone");
                continue;
            }
            if (lights is null)
            {
                AssertTrue(shown is null, $"the destruction shows no lights before the station is set up (update {update})");
                continue;
            }
            PaletteFxFrame frame = shown ?? throw new InvalidOperationException($"Destruction update {update} shows no lights.");
            AssertEqual(lights.Record, NativeColourEffectRun.RecordOf(frame), $"destruction update {update} shows the record the cartridge's lights list shows");
            AssertEqual(PictureShading.Standard.With(PaletteFxShades.Of(PaletteFxColours.CeresNavigationLightsBackground, frame)), scene.FloorShading,
                $"destruction update {update} shades the floor with the lights' record");
        }
        AssertTrue(lights is not null, "the destruction sets the station up before the Zebes reveal");
    }

    /// <summary>
    /// Checks that <paramref name="picture"/>, an installed sheet's picture, holds every record shade of an effect, each the
    /// import composes from the installed colours equal to the one it composes from the cartridge's records, ink for ink,
    /// and each recolouring the picture only to that record's colours; returns how many colours the shades recolour there.
    /// </summary>
    internal static int InstalledShadeRecolourings(SpriteImage picture, IReadOnlyList<SheetShade> installed, IReadOnlyList<SheetShade> native, string effect)
    {
        AssertTrue(installed.Count > 0 && installed.Count == native.Count, $"{effect}: one shade per record");
        int recoloured = 0;
        for (int index = 0; index < installed.Count; index++)
        {
            SheetShade shade = installed[index];
            SheetShade expected = native[index];
            AssertTrue(shade.Shade == expected.Shade && shade.Inks.SequenceEqual(expected.Inks) && shade.Colours.SequenceEqual(expected.Colours),
                $"{effect}: shade {shade.Shade.Key} shows the cartridge record's colours at its inks");
            PictureRecolouring shown = picture.Shade(shade.Shade);
            foreach (KeyedSubstitution substitution in shown.Substitutions)
            {
                AssertTrue(expected.Colours.Contains(substitution.Shown), $"{effect}: shade {shade.Shade.Key} recolours the sheet only to its record's colours");
                recoloured++;
            }
        }
        return recoloured;
    }

    /// <summary>
    /// Checks the spawn at <paramref name="spawn"/>: an immediate load of <paramref name="definition"/> and a call of the
    /// colour effects' spawn, whose definition runs the list <paramref name="effect"/> starts with.
    /// </summary>
    internal static void AssertSpawn(IImportCartridgeSource cartridge, int spawn, ushort definition, PaletteFxObject effect, string site)
    {
        AssertEqual(LoadY, Byte(cartridge, spawn), $"{site} loads a colour effect's definition");
        AssertEqual(definition, cartridge.ReadCartridgeWord(spawn + 1), $"{site} names {effect}'s definition");
        AssertEqual(LongCall, Byte(cartridge, spawn + 3), $"{site} calls the colour effects' spawn");
        AssertEqual(SpawnRoutine, Long(cartridge, spawn + 4), $"{site} spawns the definition it names");
        AssertEqual(PaletteFxNativeListAddresses.Entry(RoomPaletteFxSelection.Get(effect).InitialAnimation),
            cartridge.ReadCartridgeWord(PaletteFxListWords.ListBank | (definition + sizeof(ushort))), $"{effect} runs its definition's list");
    }

    /// <summary>The cartridge byte at <paramref name="address"/>.</summary>
    internal static byte Byte(IImportCartridgeSource cartridge, int address) => cartridge.ReadCartridgeByte(address);

    /// <summary>The 24-bit cartridge address stored at <paramref name="address"/>.</summary>
    internal static int Long(IImportCartridgeSource cartridge, int address) =>
        cartridge.ReadCartridgeWord(address) | cartridge.ReadCartridgeByte(address + 2) << 16;
}
