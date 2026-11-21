using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Verification.Bosses;
using SuperMetroid.AssetExtraction.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Draygon palette bands against retail colors and installed intro, hurt, flash, and health CGRAM paths.</summary>
internal static class EnemyTileArtworkDraygonColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares 105 RGB5 words with ROM and checks production Draygon color writes under a source-read guard.</summary>
    internal static void VerifyInstalledDraygonColors(
        ISnesAddressSpace rom, string stockDirectory, ImportEnemyArtwork stock)
    {
        DraygonColorCatalog native = stock.Bosses.DraygonColors ??
            throw new InvalidDataException("Installed enemy art has no Draygon colors.");
        Suite(nameof(VerifyBand), () => VerifyBand(DraygonSourceDefinitions.IntroColors, DraygonColorDefinitions.IntroCount,
            native.ResolveIntro, "intro"));
        Suite(nameof(VerifyBand), () => VerifyBand(DraygonSourceDefinitions.BackgroundColors,
            DraygonColorDefinitions.BackgroundCount, native.ResolveBackground, "background"));
        Suite(nameof(VerifyBand), () => VerifyBand(DraygonSourceDefinitions.SpriteColors,
            DraygonColorDefinitions.SpriteCount, native.ResolveSprite, "sprite"));
        Suite(nameof(VerifyBand), () => VerifyBand(DraygonSourceDefinitions.WhiteFlashColors,
            DraygonColorDefinitions.WhiteFlashCount, native.ResolveWhiteFlash, "white flash"));
        for (int band = 0; band < SuperMetroid.Core.Bosses.Draygon.DraygonHealthBandDefinitions.HighestFirst.Count; band++)
        for (int color = 0; color < DraygonColorDefinitions.HealthBandColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    DraygonSourceDefinitions.HealthBandColors +
                    (band * DraygonColorDefinitions.HealthBandColorCount + color) * sizeof(ushort)),
                native.ResolveHealthBand(DraygonHealthBandDefinitions.HighestFirst[band], color),
                $"installed Draygon health band {band} color {color} preserves native RGB5");

        string file = Path.Combine(stockDirectory, DraygonColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        DraygonColorDocument visual = JsonSerializer.Deserialize<DraygonColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Draygon color JSON is null.");
        DraygonColorCatalog colors = native;

        // Invoke the actual body initializer and hurt program with the visual source
        // windows forbidden. Only the resource may supply colors during installed play.
        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => (address >= DraygonSourceDefinitions.HealthBandColors &&
                    address < DraygonSourceDefinitions.HealthBandColors +
                        SuperMetroid.Core.Bosses.Draygon.DraygonHealthBandDefinitions.HighestFirst.Count *
                        DraygonColorDefinitions.HealthBandColorCount * sizeof(ushort))
                || (address >= DraygonSourceDefinitions.SpriteColors &&
                    address < DraygonSourceDefinitions.WhiteFlashColors +
                        DraygonColorDefinitions.WhiteFlashCount * sizeof(ushort)),
            address => $"Draygon accessed migrated palette ROM ${address:X6}.");
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: stock.Bosses);
        Type type = typeof(RoomEnemySystem);
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.DraygonBody, body.KeptDefinition);
        // His hurt update shades every part he draws (#1512), so his eye, tail and arms set up after the body as in his room.
        EnemyDefinitionId[] parts = [EnemyDefinitionId.DraygonEye, EnemyDefinitionId.DraygonTail, EnemyDefinitionId.DraygonArms];
        for (int index = 0; index < parts.Length; index++)
        {
            RoomEnemyPlace part = enemies.Places.InUpdateOrder[index + 1];
            part.Bind(parts[index], part.KeptDefinition);
        }
        _ = enemies.LoadAuthoredPopulation();
        DraygonEnemyState state = enemies.Draygon ??
            throw new InvalidOperationException("Draygon body initialization lost its owner.");
        AssertEqual(DraygonPhase.IntroInitialDelay, state.Phase,
            "installed Draygon colors leave intro mechanics unchanged");
        AssertBand(DraygonColorDestinations.IntroDestination,
            DraygonColorDefinitions.IntroCount, colors.ResolveIntro, "intro CGRAM");
        AssertEqual(DraygonHealthBand.Of(body.Health), state.HealthBand, "Draygon's setup starts in the band his health starts in (#2304)");
        // His setup writes only his opening's colours (#2547): his body layer shows no band until a hurt update or reaction.
        AssertBand(DraygonColorDestinations.HealthDestination, DraygonColorDefinitions.HealthBandColorCount,
            _ => RgbColor.FromWord(0), "health band after his setup, which shows none");

        // A grapple's shock marks him hurt without a reaction (#2304): before any hit, his hurt updates show the band his
        // health starts in on every update the white flash does not cover.
        int flashBefore = body.HurtFlash.UpdatesLeft;
        EnemyAiStates aiBefore = body.AiStates;
        SamusState grappling = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus();
        grappling.Grapple.Phase = GrapplePhase.ConnectedLocked;
        enemies.DraygonBehavior.Grab.CarrySamusToDraygonSpiral(state, grappling);
        AssertTrue(body.AiStates.HasFlag(EnemyAiStates.Hurt), "the grapple's shock marks Draygon hurt");
        // His own update count is off the shock's eighth update, so these updates take no shock damage of their own.
        body.FrameCounter = 1;
        int nonWhite = 0;
        for (int flash = body.HurtFlash.UpdatesLeft; flash > 0; flash--)
        {
            body.HurtFlash.Restart(flash);
            enemies.DraygonBehavior.Combat.ApplyDraygonHurt(body, state, grappling);
            if (DraygonCadenceDefinitions.HurtWhiteFlashShown.IsInSecondHalfAt(flash))
                continue;
            nonWhite++;
            AssertStartingBandShown($"the grapple shock's hurt update at flash {flash}");
        }
        AssertTrue(nonWhite > 0, "the grapple shock's hurt shows his band on the updates its white flash does not cover");
        body.HurtFlash.Restart(flashBefore);
        body.AiStates = aiBefore;

        state.HealthBand = DraygonHealthBandDefinitions.HighestFirst[1];
        enemies.DraygonBehavior.Combat.ApplyDraygonHurt(body, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
        for (int color = 0; color < DraygonColorDefinitions.BackgroundCount; color++)
        {
            RgbColor expected = color is >= 9 and <= 12
                ? colors.ResolveHealthBand(DraygonHealthBandDefinitions.HighestFirst[1], color - 9)
                : colors.ResolveBackground(color);
            AssertEqual(expected,
                Shown()[DraygonColorDestinations.BackgroundDestination + color],
                $"Draygon hurt background color {color}");
        }
        AssertBand(DraygonColorDestinations.SpriteDestination,
            DraygonColorDefinitions.SpriteCount, colors.ResolveSprite,
            "normal hurt sprite CGRAM");
        body.HurtFlash.Restart(2);
        enemies.DraygonBehavior.Combat.ApplyDraygonHurt(body, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
        AssertBand(DraygonColorDestinations.BackgroundDestination,
            DraygonColorDefinitions.WhiteFlashCount, colors.ResolveWhiteFlash,
            "white hurt background CGRAM");
        AssertBand(DraygonColorDestinations.SpriteDestination,
            DraygonColorDefinitions.WhiteFlashCount, colors.ResolveWhiteFlash,
            "white hurt sprite CGRAM");
        // His white flash covers his band while it shows (#1597), so the bands are checked once the flash has ended.
        body.HurtFlash.End();
        enemies.DraygonBehavior.Combat.ApplyDraygonHurt(body, state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus());
        for (int band = 0; band < SuperMetroid.Core.Bosses.Draygon.DraygonHealthBandDefinitions.HighestFirst.Count; band++)
        {
            state.HealthBand = DraygonHealthBandDefinitions.HighestFirst[band];
            DraygonCombat.ShowDraygonHealthBand(state);
            for (int color = 0; color < DraygonColorDefinitions.HealthBandColorCount; color++)
                AssertEqual(colors.ResolveHealthBand(DraygonHealthBandDefinitions.HighestFirst[band], color),
                    Shown()[DraygonColorDestinations.HealthDestination + color],
                    $"Draygon live health band {band} color {color}");
        }
        AssertEqual(DraygonPhase.IntroInitialDelay, state.Phase,
            "visual health-band checks leave Draygon AI unchanged");

        visual.HealthBands.Values.ElementAt(1)[1] = visual.HealthBands.Values.ElementAt(1)[1] with { Blue = 32 };
        AssertThrows<InvalidDataException>(() => DraygonColorFile.Read(new MemoryStream(DraygonColorFile.Write(visual), writable: false)),
            "reading Draygon colors rejects an RGB5 channel outside five-bit precision");
        Console.WriteLine("  Draygon colors: 105 native RGB5 words, live intro/hurt/flash/eight health bands, ROM guard, and strict failures pass.");

        void VerifyBand(int source, int count, Func<int, RgbColor> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed Draygon {name} color {color} preserves native RGB5");
        }

        void AssertStartingBandShown(string when)
        {
            DraygonHealthBand shownBand = DraygonHealthBand.Of(body.Health);
            AssertBand(DraygonColorDestinations.HealthDestination, DraygonColorDefinitions.HealthBandColorCount,
                color => colors.ResolveHealthBand(shownBand, color), $"starting health band after {when}");
        }

        void AssertBand(int destination, int count, Func<int, RgbColor> resolve, string name)
        {
            RgbColor[] shown = Shown();
            for (int color = 0; color < count; color++)
                AssertEqual(resolve(color), shown[destination + color],
                    $"installed Draygon {name} color {color}");
        }

        // The colours Draygon shows: the import's shades of his installed colours, laid as his colour state names them over
        // his body layer and his sprites (#1512).
        RgbColor[] Shown()
        {
            IReadOnlyList<SheetShade> background = BossSheetShades.DraygonBackground(stock.Bosses);
            IReadOnlyList<SheetShade> sprites = BossSheetShades.OwnRow(stock.Bosses, EnemyDefinitionId.DraygonBody, 0);
            ushort[] colours = BossShadeColours.Laid(GameplayShadingProbe.ShadingOf(state.Colours, GameplayPictures.BossBody,
                background.Select(shade => shade.Shade)).Shades, background, BossShadeColours.Room, 0);
            foreach (PictureShade shade in state.Colours.Sprites.Shades)
                BossShadeColours.Lay(colours, sprites, shade, BossShadeColours.Sprites);
            return [.. colours.Select(RgbColor.FromWord)];
        }
    }
}
