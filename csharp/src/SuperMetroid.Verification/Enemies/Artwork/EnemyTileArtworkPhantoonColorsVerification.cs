using SuperMetroid.Core.Assets;
using System.Text.Json;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Phantoon health, fade, and power-on colors against
/// native RGB5 data and live palette transitions.</summary>
internal static class EnemyTileArtworkPhantoonColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Checks installed colors through boss fade and hurt handlers,
    /// with native source reads forbidden and malformed documents rejected.</summary>
    internal static void VerifyInstalledPhantoonColors(
        ISnesAddressSpace rom, string stockDirectory, ImportEnemyArtwork stock)
    {
        PhantoonColorCatalog native = stock.Bosses.PhantoonColors ??
            throw new InvalidDataException("Installed enemy art has no Phantoon colors.");
        Suite(nameof(VerifyBand), () => VerifyBand(PhantoonSourceDefinitions.FadeOutColors,
            PhantoonColorFormat.FadeOutCount, native.ResolveFadeOut, "fade-out"));
        Suite(nameof(VerifyBand), () => VerifyBand(PhantoonSourceDefinitions.PowerOnColors,
            PhantoonSourceDefinitions.PowerOnColourCount, color => native.ResolvePowerOnAtNative(color), "power-on"));
        for (int band = 0; band < PhantoonColorDefinitions.HealthBandCount; band++)
        for (int color = 0; color < PhantoonColorFormat.HealthBandColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    PhantoonSourceDefinitions.HealthBandColors +
                    (band * PhantoonColorFormat.HealthBandColorCount + color) *
                        sizeof(ushort)),
                native.ResolveHealth(band, color),
                $"installed Phantoon health palette {band} color {color} preserves RGB5");

        string file = Path.Combine(stockDirectory, PhantoonColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        PhantoonColorDocument visual = JsonSerializer.Deserialize<PhantoonColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Phantoon color JSON is null.");
        PhantoonColorCatalog colors = native;

        // Drive the actual boss palette handlers. The complete authored color range is
        // forbidden on the bus, proving no installed runtime path silently falls back.
        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= PhantoonSourceDefinitions.FadeOutColors and
                < (PhantoonSourceDefinitions.HealthBandColors +
                    PhantoonColorDefinitions.HealthBandCount *
                    PhantoonColorFormat.HealthBandColorCount * sizeof(ushort)),
            address => $"Phantoon accessed migrated palette ROM ${address:X6}.");
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: stock.Bosses);
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        RoomEnemyPlace eye = enemies.Places.InUpdateOrder[1];
        RoomEnemyPlace tentacles = enemies.Places.InUpdateOrder[2];
        var state = new PhantoonEnemyState(body.BoundFor(EnemyDefinitionId.PhantoonBody), body, eye, tentacles, enemies.Places.InUpdateOrder[3]);
        enemies.BeginPhantoonAsSetUp(state);
        // His live colour handlers name the shades of his body and room and the typed fades between them (#1512, #2251);
        // the import pre-colours each settled colouring from the installed colours, which this compares with the catalog.
        IReadOnlyList<SheetShade> bodyShades = PhantoonSheetShades.Body(stock.Bosses);
        PictureShading Body(PhantoonBodyLook look) => PictureShading.Shaded(PhantoonShades.Body(look));
        const int steps = PhantoonColorDefinitions.BodyFadeSteps;
        for (int row = 0; row < PhantoonColorDefinitions.HealthBandCount; row++)
        {
            // The cartridge's band at health 312 times its row plus one is the row's band (#2259).
            PhantoonHealthBand band = PhantoonHealthBandDefinitions.LowestFirst[row];
            body.Health = unchecked((ushort)(row * 312 + 1));
            PhantoonBehavior.ShowPhantoonHealthColours(state);
            AssertEqual(PictureShading.Shaded(PhantoonShades.Body(PhantoonBodyLook.Health(band))), Shown(state), $"Phantoon live health restore band {band}");
            AssertShade(bodyShades, PhantoonShades.Body(PhantoonBodyLook.Health(band)), color => colors.ResolveHealth(row, color),
                $"Phantoon health band {band}");
            state.BodyFade.Steps = 1;
            state.BodyFade.Resume();
            enemies.PhantoonBehavior.Presentation.AdvancePhantoonFadeIn(body, state, steps);
            AssertEqual(PictureShading.Standard.With(new PictureShadeFade(Body(PhantoonBodyLook.Health(band)), Body(PhantoonBodyLook.Health(band)), 1, steps)),
                Shown(state), $"Phantoon live materialization band {band}: a typed fade from its first moving step");
            AssertEqual(2, state.BodyFade.Steps, $"Phantoon fade-in band {band} takes one step");
        }

        body.Health = unchecked(2 * 312 + 1);
        body.HurtFlash.Restart(8);
        state.Round.HurtFlashShown = true;
        PhantoonBehavior.ApplyPhantoonHurt(body, state);
        AssertEqual(Body(PhantoonBodyLook.Health(PhantoonHealthBandDefinitions.BelowThreeEighths)), Shown(state), "Phantoon live hurt restore");
        AssertTrue(!state.Round.HurtFlashShown, "Phantoon hurt restore clears the flash latch");

        // The step past a fade's length reaches its goal, and the step after completes the fade, which then shows its goal.
        state.BodyFade.Steps = steps + 1;
        state.BodyFade.Resume();
        enemies.PhantoonBehavior.Patterns.AdvancePhantoonFadeOut(state, steps);
        AssertEqual(PictureShading.Standard.With(new PictureShadeFade(Body(PhantoonBodyLook.Health(PhantoonHealthBandDefinitions.BelowThreeEighths)), Body(PhantoonBodyLook.FadedOut), 1, 1)),
            Shown(state), "Phantoon's fade-out restarted at its last step reaches its goal");
        AssertEqual(steps + 2, state.BodyFade.Steps, "Phantoon's fade-out takes its last step");
        enemies.PhantoonBehavior.Patterns.AdvancePhantoonFadeOut(state, steps);
        AssertTrue(state.BodyFade.IsFinished && state.BodyFade.Steps == 0, "the next step completes the fade and starts the next over");
        AssertEqual(Body(PhantoonBodyLook.FadedOut), Shown(state), "a completed fade shows its goal");
        AssertShade(bodyShades, PhantoonShades.Body(PhantoonBodyLook.FadedOut), colors.ResolveFadeOut, "Phantoon's faded-out colours");

        // A hurt colouring written mid-fade restarts the fade from it over the steps it has left (#1512, #2251).
        state.Colours.Show(PhantoonBodyLook.Health(PhantoonHealthBandDefinitions.BelowThreeEighths));
        for (int step = 1; step <= 4; step++)
            state.Colours.Fade(PhantoonBodyLook.FadedOut, step, steps);
        PictureShadeFade fromHealth = new(Body(PhantoonBodyLook.Health(PhantoonHealthBandDefinitions.BelowThreeEighths)), Body(PhantoonBodyLook.FadedOut), 4, steps);
        AssertEqual(PictureShading.Standard.With(fromHealth), Shown(state), "Phantoon's fade has come four of its twelve steps from his health colours");
        state.Colours.Show(PhantoonBodyLook.White);
        state.Colours.Fade(PhantoonBodyLook.FadedOut, 5, steps);
        AssertEqual(PictureShading.Standard.With(new PictureShadeFade(Body(PhantoonBodyLook.White), Body(PhantoonBodyLook.FadedOut), 1, 8)),
            Shown(state), "Phantoon's fade restarts from the hurt flash's white over the eight steps it has left");
        AssertShade(bodyShades, PhantoonShades.Body(PhantoonBodyLook.White), _ => RgbColor.White, "Phantoon's white hurt flash");
        PictureShadeFade toFadedOut = new(Body(PhantoonBodyLook.White), Body(PhantoonBodyLook.FadedOut), 1, 8);
        state.Colours.Fade(PhantoonBodyLook.Health(PhantoonHealthBandDefinitions.BelowThreeEighths), 6, steps);
        AssertEqual(PictureShading.Standard.With(new PictureShadeFade(PictureShading.Standard.With(toFadedOut), Body(PhantoonBodyLook.Health(PhantoonHealthBandDefinitions.BelowThreeEighths)), 1, 7)),
            Shown(state), "a fade toward another goal starts from the running fade as it shows");

        // The Wrecked Ship's power coming on is its own typed fade with its own progress.
        state.ShipPowerOn.Steps = PhantoonColorDefinitions.PowerOnSteps + 1;
        AssertTrue(!PhantoonDeath.AdvanceWreckedShipPowerOn(state), "Phantoon ship power-on remains active on its last step");
        AssertEqual(PictureShading.Standard.With(new PictureShadeFade(PictureShading.Standard, PictureShading.Shaded(PhantoonShades.Powered),
            PhantoonColorDefinitions.PowerOnSteps, PhantoonColorDefinitions.PowerOnSteps)), Shown(state, GameplayPictures.Room),
            "the power's last step reaches the room's powered colours");
        AssertEqual(PhantoonColorDefinitions.PowerOnSteps + 2, state.ShipPowerOn.Steps, "Phantoon ship power-on takes its last step");
        AssertTrue(PhantoonDeath.AdvanceWreckedShipPowerOn(state), "the next step completes the power coming on");
        AssertEqual(PictureShading.Shaded(PhantoonShades.Powered), Shown(state, GameplayPictures.FxOverlay),
            "the completed power shows the powered colours on the room's effects too");
        SheetShade powered = PhantoonSheetShades.PowerOn(stock.Bosses).Single();
        AssertEqual(PhantoonShades.Powered, powered.Shade, "the import pre-colours the room's powered colours");
        for (int color = 0; color < PhantoonSourceDefinitions.PowerOnColourCount; color++)
            AssertEqual(colors.ResolvePowerOnAtNative(color),
                powered.Colours[PhantoonColorDestinations.PowerOnDestination + color],
                $"Phantoon live ship power-on target {color}");

        AssertThrows<InvalidDataException>(
            () => PhantoonColorFile.Read(new MemoryStream("{\"version\":1,\"version\":1}"u8.ToArray(), writable: false)),
            "duplicate Phantoon color property fails loudly");
        visual.HealthBands[2][1] = visual.HealthBands[2][1] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => PhantoonColorFile.Read(new MemoryStream(PhantoonColorFile.Write(visual), writable: false)),
            "reading Phantoon colors rejects an RGB5 channel outside five-bit precision");
        Console.WriteLine("  Phantoon colors: 256 native RGB5 targets, all health bands and three live transitions, ROM guard, and strict document failures pass.");

        void VerifyBand(int source, int count, Func<int, RgbColor> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed Phantoon {name} color {color} preserves native RGB5");
        }
    }
    /// <summary>The shading <paramref name="pictures"/> show now under <paramref name="state"/>'s colours: his body's by default.</summary>
    private static PictureShading Shown(PhantoonEnemyState state, GameplayPictures pictures = GameplayPictures.BossBody)
    {
        var frame = new GameplayShadingBuilder();
        state.Colours.ShadeGameplay(frame);
        // The room picture whose backdrop the build reads is pre-coloured in the powered shade the power coming on names.
        return frame.Build(SpriteImageFixtures.Copied("Room", new(1, 1), [default(RgbaPixel)],
            new PictureColours
            {
                Backdrop = ColourKeyConstants.Of(RgbColor.Black),
                Shades = System.Collections.Immutable.ImmutableDictionary<PictureShade, PictureRecolouring>.Empty
                    .Add(PhantoonShades.Powered, new PictureRecolouring([])),
            }))
            .Of(pictures);
    }

    /// <summary>Asserts that the import's <paramref name="shade"/> among <paramref name="made"/> shows <paramref name="expected"/>'s colours.</summary>
    private static void AssertShade(IReadOnlyList<SheetShade> made, PictureShade shade, Func<int, RgbColor> expected, string label)
    {
        SheetShade found = made.Single(candidate => candidate.Shade == shade);
        for (int color = 0; color < found.Colours.Length; color++)
            AssertEqual(expected(color), found.Colours[color], $"{label} colour {color}");
    }
}
