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

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Phantoon health, fade, and power-on colors against
/// native RGB5 data and live palette transitions.</summary>
internal static class EnemyTileArtworkPhantoonColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Checks isolated color edits through boss fade and hurt handlers,
    /// with native source reads forbidden and malformed overrides rejected.</summary>
    internal static void VerifyInstalledPhantoonColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        PhantoonColorCatalog native = stock.Bosses.PhantoonColors ??
            throw new InvalidDataException("Installed enemy art has no Phantoon colors.");
        Suite(nameof(VerifyBand), () => VerifyBand(PhantoonSourceDefinitions.FadeOutColors,
            PhantoonColorDefinitions.FadeOutCount, native.ResolveFadeOut, "fade-out"));
        Suite(nameof(VerifyBand), () => VerifyBand(PhantoonSourceDefinitions.PowerOnColors,
            PhantoonSourceDefinitions.PowerOnColourCount, color => native.ResolvePowerOnAtNative(color), "power-on"));
        for (int band = 0; band < PhantoonColorDefinitions.HealthBandCount; band++)
        for (int color = 0; color < PhantoonColorDefinitions.HealthBandColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    PhantoonSourceDefinitions.HealthBandColors +
                    (band * PhantoonColorDefinitions.HealthBandColorCount + color) *
                        sizeof(ushort)),
                native.ResolveHealth(band, color),
                $"installed Phantoon health palette {band} color {color} preserves RGB5");

        string file = Path.Combine(stockDirectory, PhantoonColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        PhantoonColorDocument visual = JsonSerializer.Deserialize<PhantoonColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Phantoon color JSON is null.");
        visual.HealthBands[2][1] = ChangeRed(visual.HealthBands[2][1]);
        visual.FadeOut[1] = ChangeRed(visual.FadeOut[1]);
        visual.PowerOn[1] = ChangeRed(visual.PowerOn[1]);
        string overrides = Path.Combine(stockDirectory, "phantoon-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, PhantoonColorFormat.FileName);
        File.WriteAllBytes(overrideFile, PhantoonColorFile.Write(visual));
        InstalledEnemyArtwork edited = stock with { Bosses = stock.Bosses with { PhantoonColors = EnemyTileArtworkFiles.Open(stockDirectory, overrides).PhantoonColors() } };
        PhantoonColorCatalog colors = edited.Bosses.PhantoonColors ??
            throw new InvalidDataException("Edited enemy art has no Phantoon colors.");
        AssertTrue(colors.ResolveHealth(2, 1) != native.ResolveHealth(2, 1),
            "Phantoon health color edit changes selected RGB5 word");
        AssertTrue(colors.ResolveFadeOut(1) != native.ResolveFadeOut(1),
            "Phantoon fade-out edit changes selected RGB5 word");
        AssertTrue(colors.ResolvePowerOnAtNative(1) != native.ResolvePowerOnAtNative(1),
            "Phantoon power-on edit changes selected RGB5 word");
        for (int band = 0; band < PhantoonColorDefinitions.HealthBandCount; band++)
        for (int color = 0; color < PhantoonColorDefinitions.HealthBandColorCount; color++)
            if (band != 2 || color != 1)
                AssertEqual(native.ResolveHealth(band, color),
                    colors.ResolveHealth(band, color),
                    $"Phantoon health edit leaves palette {band} color {color} unchanged");
        CheckUnchangedBand(PhantoonColorDefinitions.FadeOutCount,
            native.ResolveFadeOut, colors.ResolveFadeOut, "fade-out");
        CheckUnchangedBand(PhantoonSourceDefinitions.PowerOnColourCount,
            color => native.ResolvePowerOnAtNative(color), color => colors.ResolvePowerOnAtNative(color), "power-on");

        // Drive the actual boss palette handlers. The complete authored color range is
        // forbidden on the bus, proving no installed runtime path silently falls back.
        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= PhantoonSourceDefinitions.FadeOutColors and
                < (PhantoonSourceDefinitions.HealthBandColors +
                    PhantoonColorDefinitions.HealthBandCount *
                    PhantoonColorDefinitions.HealthBandColorCount * sizeof(ushort)),
            address => $"Phantoon accessed migrated palette ROM ${address:X6}.");
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses };
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot eye = enemies.SlotTable.Slots[1];
        RoomEnemySlot tentacles = enemies.SlotTable.Slots[2];
        var state = new PhantoonEnemyState(body)
        {
            Eye = eye,
            Tentacles = tentacles,
        };
        enemies.PhantoonBehavior.Combat.Phantoon = state;
        // His live colour handlers name the shades of his body and room (#1512); the import pre-colours each from the
        // edited colours, which this compares with the catalog.
        IReadOnlyList<SheetShade> bodyShades = PhantoonSheetShades.Body(edited.Bosses);
        for (int band = 0; band < PhantoonColorDefinitions.HealthBandCount; band++)
        {
            body.Health = unchecked((ushort)(band * 312 + 1));
            enemies.PhantoonBehavior.CopyPhantoonHealthPalette(body);
            AssertEqual(PhantoonShades.Body(PhantoonBodyLook.Health(band)), Shown(state), $"Phantoon live health restore band {band}");
            AssertShade(bodyShades, PhantoonShades.Body(PhantoonBodyLook.Health(band)), color => colors.ResolveHealth(band, color),
                $"Phantoon health band {band}");
            state.Parts[eye.SlotIndex].FadeNumerator = 1;
            state.Parts[eye.SlotIndex].FadeComplete = false;
            enemies.PhantoonBehavior.Presentation.AdvancePhantoonFadeIn(body, state, 12);
            AssertEqual(PhantoonShades.Fading(PhantoonBodyLook.Health(band), PhantoonBodyLook.Health(band), 12, 1, 1), Shown(state),
                $"Phantoon live materialization band {band}");
            AssertEqual(2, state.Parts[eye.SlotIndex].FadeNumerator,
                $"Phantoon fade-in band {band} retains native numerator advance");
        }

        body.Health = unchecked(2 * 312 + 1);
        body.FlashTimer = WrappingCountdown.Start(8);
        state.Parts[tentacles.SlotIndex].HurtFlashShown = true;
        enemies.PhantoonBehavior.ApplyPhantoonHurt(body, state);
        AssertEqual(PhantoonShades.Body(PhantoonBodyLook.Health(2)), Shown(state), "Phantoon live hurt restore");
        AssertTrue(!state.Parts[tentacles.SlotIndex].HurtFlashShown, "Phantoon hurt restore clears the flash latch");

        state.Parts[eye.SlotIndex].FadeNumerator = 13;
        state.Parts[eye.SlotIndex].FadeComplete = false;
        enemies.PhantoonBehavior.Patterns.AdvancePhantoonFadeOut(state, 12);
        PictureShade fadedOut = PhantoonShades.Fading(PhantoonBodyLook.Health(2), PhantoonBodyLook.FadedOut, 12, 13, 13);
        AssertEqual(fadedOut, Shown(state), "Phantoon live fade-out names its last step");
        AssertShade(bodyShades, fadedOut, colors.ResolveFadeOut, "Phantoon live fade-out target");
        AssertEqual(14, state.Parts[eye.SlotIndex].FadeNumerator,
            "Phantoon fade-out retains native numerator advance");

        // A hurt colouring written mid-fade restarts the fade from it at the value the fade stands at (#1512): the
        // cartridge steps each colour from the one shown, so the next step moves white once, not through the steps before.
        state.Colours.Show(PhantoonBodyLook.Health(2));
        for (int numerator = 1; numerator <= 4; numerator++)
            state.Colours.Fade(PhantoonBodyLook.FadedOut, 12, numerator);
        AssertEqual(PhantoonShades.Fading(PhantoonBodyLook.Health(2), PhantoonBodyLook.FadedOut, 12, 1, 4), Shown(state),
            "Phantoon's fade names the steps it took from his health colours");
        state.Colours.Show(PhantoonBodyLook.White);
        state.Colours.Fade(PhantoonBodyLook.FadedOut, 12, 5);
        PictureShade restarted = PhantoonShades.Fading(PhantoonBodyLook.White, PhantoonBodyLook.FadedOut, 12, 5, 5);
        AssertEqual(restarted, Shown(state), "Phantoon's fade restarts from the hurt flash's white at the value it stands at");
        AssertShade(bodyShades, restarted, color => CartridgeFadeStep(5, RgbColor.White, colors.ResolveFadeOut(color), 12),
            "Phantoon's fade restarted from white");
        AssertThrows<InvalidOperationException>(() => state.Colours.Fade(PhantoonBodyLook.Health(2), 12, 6),
            "A fade toward another target while his colours are mid-fade is refused");

        state.Parts[eye.SlotIndex].FadeDenominator = 12;
        state.Parts[eye.SlotIndex].FadeNumerator = 13;
        bool complete = PhantoonDeath.AdvanceWreckedShipPowerPalette(state, state.Parts[eye.SlotIndex]);
        AssertTrue(!complete, "Phantoon ship power-on remains active on target frame");
        SheetShade powered = PhantoonSheetShades.PowerOn(edited.Bosses, new RgbColor[SpriteImage.SpriteColourCount])
            .Single(shade => shade.Shade == PhantoonShades.PowerOn(13));
        for (int color = 0; color < PhantoonSourceDefinitions.PowerOnColourCount; color++)
            AssertEqual(colors.ResolvePowerOnAtNative(color),
                powered.Colours[PhantoonColorDestinations.PowerOnDestination + color],
                $"Phantoon live ship power-on target {color}");
        AssertEqual(14, state.Parts[eye.SlotIndex].FadeNumerator,
            "Phantoon ship power-on retains native numerator advance");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Open(stockDirectory, overrides).PhantoonColors(),
            "invalid Phantoon override fails loudly");
        AssertThrows<InvalidDataException>(
            () => PhantoonColorFile.Read(new MemoryStream("{\"version\":1,\"version\":1}"u8.ToArray(), writable: false)),
            "duplicate Phantoon color property fails loudly");
        visual.HealthBands[2][1] = visual.HealthBands[2][1] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => PhantoonColorFile.Read(new MemoryStream(PhantoonColorFile.Write(visual), writable: false)),
            "reading Phantoon colors rejects an RGB5 channel outside five-bit precision");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Open(stockDirectory, null).PhantoonColors(),
            "corrupt stock Phantoon colors fail loudly where it is read");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Phantoon colors: 256 native RGB5 targets, all health bands and three live transitions, persistent override, ROM guard, and strict failures pass.");

        void VerifyBand(int source, int count, Func<int, RgbColor> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed Phantoon {name} color {color} preserves native RGB5");
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };

        static void CheckUnchangedBand(int count, Func<int, RgbColor> original,
            Func<int, RgbColor> changed, string name)
        {
            for (int color = 0; color < count; color++)
                if (color != 1)
                    AssertEqual(original(color), changed(color),
                        $"Phantoon {name} edit leaves color {color} unchanged");
        }
    }
    /// <summary>The shade Phantoon's body shows now under <paramref name="state"/>'s colours.</summary>
    private static PictureShade Shown(PhantoonEnemyState state)
    {
        var frame = new GameplayShadingBuilder();
        state.Colours.ShadeGameplay(frame);
        return frame.Build(new SpriteImage("Room", new(1, 1), [default], [0],
            new SpriteImagePalette(SpriteImageColours.Background, [.. new RgbColor[SpriteImage.SpriteColourCount]]), [0]))
            .Of(GameplayPictures.BossBody).Shades.Single();
    }

    /// <summary>
    /// One step of value <paramref name="step"/> of Phantoon's fade from <paramref name="shown"/> toward
    /// <paramref name="goal"/>, as the cartridge's per-channel transition moves each channel by its distance in 256ths over
    /// the steps left.
    /// </summary>
    private static RgbColor CartridgeFadeStep(int step, RgbColor shown, RgbColor goal, int denominator) =>
        shown.Zip(goal, (_, from, to) => step == 0 ? from : step - 1 == denominator ? to
            : ((from << 8) + Math.Sign(to - from) * (Math.Abs(to - from) * 256 / (denominator - step + 1))) >> 8);

    /// <summary>Asserts that the import's <paramref name="shade"/> among <paramref name="made"/> shows <paramref name="expected"/>'s colours.</summary>
    private static void AssertShade(IReadOnlyList<SheetShade> made, PictureShade shade, Func<int, RgbColor> expected, string label)
    {
        SheetShade found = made.Single(candidate => candidate.Shade == shade);
        for (int color = 0; color < found.Colours.Length; color++)
            AssertEqual(expected(color), found.Colours[color], $"{label} colour {color}");
    }
}
