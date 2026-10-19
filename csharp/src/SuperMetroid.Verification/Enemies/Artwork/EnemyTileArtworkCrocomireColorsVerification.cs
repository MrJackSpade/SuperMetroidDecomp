using SuperMetroid.Core.Programs;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed Crocomire palette bands against native RGB5 data
/// and live boss CGRAM writes under a cartridge-read guard.</summary>
internal static class EnemyTileArtworkCrocomireColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Checks five color transfers, isolated overrides, hurt whiteout,
    /// death-phase palette use, reload persistence, and invalid asset rejection.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Crocomire palette bands")]
    internal static void VerifyInstalledCrocomireColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        CrocomireColorCatalog native = stock.CrocomireColors ??
            throw new InvalidDataException("Installed enemy art has no Crocomire colors.");
        Suite(nameof(VerifyBand), () => VerifyBand(CrocomirePaletteRomData.FightBodySource,
            CrocomirePaletteRomData.FightBodyCount, native.ResolveFightBody, "fight body"));
        Suite(nameof(VerifyBand), () => VerifyBand(CrocomirePaletteRomData.InitialWallSource,
            CrocomirePaletteRomData.InitialWallCount, native.ResolveInitialWall, "initial wall"));
        Suite(nameof(VerifyBand), () => VerifyBand(CrocomirePaletteRomData.InitialProjectileSource,
            CrocomirePaletteRomData.InitialProjectileCount, native.ResolveInitialProjectile,
            "initial projectile"));
        Suite(nameof(VerifyBand), () => VerifyBand(CrocomirePaletteRomData.SkeletonArmSource,
            CrocomirePaletteRomData.SkeletonArmCount, native.ResolveSkeletonArm, "skeleton arm"));
        Suite(nameof(VerifyBand), () => VerifyBand(CrocomirePaletteRomData.WallSpikesSource,
            CrocomirePaletteRomData.WallSpikesCount, native.ResolveWallSpikes, "wall spikes"));

        string file = Path.Combine(stockDirectory, CrocomireColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        CrocomireColorDocument visual = JsonSerializer.Deserialize<CrocomireColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Crocomire color JSON is null.");
        visual.FightBody[1] = ChangeRed(visual.FightBody[1]);
        visual.InitialWall[1] = ChangeRed(visual.InitialWall[1]);
        visual.InitialProjectile[1] = ChangeRed(visual.InitialProjectile[1]);
        visual.SkeletonArm[1] = ChangeRed(visual.SkeletonArm[1]);
        visual.WallSpikes[1] = ChangeRed(visual.WallSpikes[1]);
        string overrides = Path.Combine(stockDirectory, "crocomire-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, CrocomireColorFormat.FileName);
        File.WriteAllBytes(overrideFile, CrocomireColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        CrocomireColorCatalog colors = edited.CrocomireColors ??
            throw new InvalidDataException("Edited enemy art has no Crocomire colors.");
        CheckEditedBand(CrocomirePaletteRomData.FightBodyCount,
            native.ResolveFightBody, colors.ResolveFightBody, "fight body");
        CheckEditedBand(CrocomirePaletteRomData.InitialWallCount,
            native.ResolveInitialWall, colors.ResolveInitialWall, "initial wall");
        CheckEditedBand(CrocomirePaletteRomData.InitialProjectileCount,
            native.ResolveInitialProjectile, colors.ResolveInitialProjectile,
            "initial projectile");
        CheckEditedBand(CrocomirePaletteRomData.SkeletonArmCount,
            native.ResolveSkeletonArm, colors.ResolveSkeletonArm, "skeleton arm");
        CheckEditedBand(CrocomirePaletteRomData.WallSpikesCount,
            native.ResolveWallSpikes, colors.ResolveWallSpikes, "wall spikes");
        AssertEqual(colors.ResolveFightBody(1),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .CrocomireColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .CrocomireColors.")).ResolveFightBody(1),
            "Crocomire color override survives catalog reload");

        // Execute the real boss initializer and hurt-flash hook with palette ROM reads
        // forbidden. Their CGRAM writes must come from the installed resource, while the
        // native phase, instruction, and white-flash decisions remain unchanged.
        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= CrocomirePaletteRomData.FightBodySource and
                < (CrocomirePaletteRomData.WallSpikesSource +
                    CrocomirePaletteRomData.WallSpikesCount * sizeof(ushort)),
            address => $"Crocomire accessed migrated palette ROM ${address:X6}.");
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { TileArtwork = edited };
        Type type = typeof(RoomEnemySystem);
        enemies.Presentation.BindCgram(cgram);
        enemies.VerificationServices().IsAreaMiniBossDefeated = (Func<bool>)(() => false);
        enemies.VerificationServices().SetRoomScrollState = (Action<int, RoomScrollState>)((_, _) => { });
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Crocomire, body.Definition);
        enemies.CrocomireBehavior.InitializeCrocomire(body);
        CrocomireEnemyState state = (CrocomireEnemyState)(enemies.CrocomireBehavior.Crocomire ?? throw new InvalidOperationException("Verification requires enemies.CrocomireBehavior.Crocomire."));
        AssertEqual(CrocomireDeathPhase.Fighting, (state ?? throw new InvalidOperationException("Verification requires state.")).DeathSequenceIndex,
            "installed Crocomire colors leave living fight phase unchanged");
        AssertEqual(AnimationProgramCursor<CrocomireBodyProgram>.Start(CrocomireBodyProgram.Initial),
            state.BodyCursor,
            "installed Crocomire colors leave the initial program unchanged");
        AssertBand(CrocomirePaletteRomData.InitialWallDestination,
            CrocomirePaletteRomData.InitialWallCount, colors.ResolveInitialWall,
            "initial wall CGRAM");
        AssertBand(CrocomirePaletteRomData.InitialProjectileDestination,
            CrocomirePaletteRomData.InitialProjectileCount, colors.ResolveInitialProjectile,
            "initial projectile CGRAM");
        enemies.CrocomireBehavior.ApplyCrocomireHurtPalette(body);
        AssertBand(CrocomirePaletteRomData.FightBodyDestination,
            CrocomirePaletteRomData.FightBodyCount, colors.ResolveFightBody,
            "restored fight-body CGRAM");
        body.FlashTimer = NativeCountdown.Start(1);
        enemies.FrameContext.SeedRandomEnemyCounter(2);
        enemies.CrocomireBehavior.ApplyCrocomireHurtPalette(body);
        for (int color = 0; color < CrocomirePaletteRomData.FightBodyCount; color++)
            AssertEqual((ushort)0x7fff,
                cgram.Colors[CrocomirePaletteRomData.FightBodyDestination + color],
                $"Crocomire native white hurt flash {color}");
        AssertEqual(CrocomireDeathPhase.Fighting, state.DeathSequenceIndex,
            "white hurt flash leaves fight phase unchanged");
        state.DeathPhaseDelay = NativeCountdown.Start(0);
        enemies.CrocomireBehavior.RunCrocomireSkeletonTileLoadAndWallBreak(state);
        AssertBand(CrocomirePaletteRomData.SkeletonArmDestination,
            CrocomirePaletteRomData.SkeletonArmCount, colors.ResolveSkeletonArm,
            "skeleton-arm CGRAM");
        AssertEqual(CrocomireDeathPhase.CrumbleBridgeAndSink, state.DeathSequenceIndex,
            "installed skeleton-arm colors leave death-phase handoff unchanged");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Crocomire color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Crocomire color property fails loudly");
        visual.FightBody[1] = visual.FightBody[1] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => CrocomireColorCatalog.Write(visual),
            "Crocomire RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Crocomire colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Crocomire colors: five native RGB5 transfers, live boss CGRAM, white flash, persistent override, and strict failures pass.");

        void VerifyBand(int source, int count, Func<int, Bgr555> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed Crocomire {name} color {color} preserves native RGB5");
        }

        void AssertBand(int destination, int count, Func<int, Bgr555> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(resolve(color), cgram.Colors[destination + color],
                    $"installed Crocomire {name} color {color}");
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };

        static void CheckEditedBand(int count, Func<int, Bgr555> original,
            Func<int, Bgr555> changed, string name)
        {
            AssertTrue(original(1) != changed(1),
                $"Crocomire {name} edit changes selected color");
            for (int color = 0; color < count; color++)
                if (color != 1)
                    AssertEqual(original(color), changed(color),
                        $"Crocomire {name} edit leaves color {color} unchanged");
        }
    }
}
