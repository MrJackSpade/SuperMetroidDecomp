using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Programs;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

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
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        CrocomireColorCatalog native = stock.Bosses.CrocomireColors ??
            throw new InvalidDataException("Installed enemy art has no Crocomire colors.");
        Suite(nameof(VerifyBand), () => VerifyBand(CrocomireSourceDefinitions.FightBodyColors,
            CrocomireColorDefinitions.FightBodyCount, native.ResolveFightBody, "fight body"));
        Suite(nameof(VerifyBand), () => VerifyBand(CrocomireSourceDefinitions.InitialWallColors,
            CrocomireColorDefinitions.InitialWallCount, native.ResolveInitialWall, "initial wall"));
        Suite(nameof(VerifyBand), () => VerifyBand(CrocomireSourceDefinitions.InitialProjectileColors,
            CrocomireColorDefinitions.InitialProjectileCount, native.ResolveInitialProjectile,
            "initial projectile"));
        Suite(nameof(VerifyBand), () => VerifyBand(CrocomireSourceDefinitions.SkeletonArmColors,
            CrocomireColorDefinitions.SkeletonArmCount, native.ResolveSkeletonArm, "skeleton arm"));
        Suite(nameof(VerifyBand), () => VerifyBand(CrocomireSourceDefinitions.WallSpikesColors,
            CrocomireColorDefinitions.WallSpikesCount, native.ResolveWallSpikes, "wall spikes"));

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
        File.WriteAllBytes(overrideFile, CrocomireColorFile.Write(visual));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        CrocomireColorCatalog colors = edited.Bosses.CrocomireColors ??
            throw new InvalidDataException("Edited enemy art has no Crocomire colors.");
        CheckEditedBand(CrocomireColorDefinitions.FightBodyCount,
            native.ResolveFightBody, colors.ResolveFightBody, "fight body");
        CheckEditedBand(CrocomireColorDefinitions.InitialWallCount,
            native.ResolveInitialWall, colors.ResolveInitialWall, "initial wall");
        CheckEditedBand(CrocomireColorDefinitions.InitialProjectileCount,
            native.ResolveInitialProjectile, colors.ResolveInitialProjectile,
            "initial projectile");
        CheckEditedBand(CrocomireColorDefinitions.SkeletonArmCount,
            native.ResolveSkeletonArm, colors.ResolveSkeletonArm, "skeleton arm");
        CheckEditedBand(CrocomireColorDefinitions.WallSpikesCount,
            native.ResolveWallSpikes, colors.ResolveWallSpikes, "wall spikes");
        AssertEqual(colors.ResolveFightBody(1),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Bosses.CrocomireColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .CrocomireColors.")).ResolveFightBody(1),
            "Crocomire color override survives catalog reload");

        // Execute the real boss initializer and hurt-flash hook with palette ROM reads
        // forbidden. Their CGRAM writes must come from the installed resource, while the
        // native phase, instruction, and white-flash decisions remain unchanged.
        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= CrocomireSourceDefinitions.FightBodyColors and
                < (CrocomireSourceDefinitions.WallSpikesColors +
                    CrocomireColorDefinitions.WallSpikesCount * sizeof(ushort)),
            address => $"Crocomire accessed migrated palette ROM ${address:X6}.");
        var cgram = new SamusColourRig();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        Type type = typeof(RoomEnemySystem);
        enemies.Presentation.BindColours(cgram.Gameplay);
        enemies.VerificationServices().IsAreaMiniBossDefeated = () => false;
        enemies.VerificationServices().SetRoomScrollState = (_, _) => { };
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Crocomire, body.Definition);
        enemies.CrocomireBehavior.Fight.InitializeCrocomire(body);
        CrocomireEnemyState state = enemies.CrocomireBehavior.Access.Crocomire ?? throw new InvalidOperationException("Verification requires enemies.CrocomireBehavior.Crocomire.");
        AssertEqual(CrocomireDeathPhase.Fighting, state.DeathSequenceIndex,
            "installed Crocomire colors leave living fight phase unchanged");
        AssertEqual(AnimationProgramCursor<CrocomireBodyProgram>.Start(CrocomireBodyProgram.Initial),
            state.BodyCursor,
            "installed Crocomire colors leave the initial program unchanged");
        AssertBand(CrocomireColorDestinations.InitialWallDestination,
            CrocomireColorDefinitions.InitialWallCount, colors.ResolveInitialWall,
            "initial wall CGRAM");
        AssertBand(CrocomireColorDestinations.InitialProjectileDestination,
            CrocomireColorDefinitions.InitialProjectileCount, colors.ResolveInitialProjectile,
            "initial projectile CGRAM");
        enemies.CrocomireBehavior.Combat.ApplyCrocomireHurtPalette(body);
        AssertBand(CrocomireColorDestinations.FightBodyDestination,
            CrocomireColorDefinitions.FightBodyCount, colors.ResolveFightBody,
            "restored fight-body CGRAM");
        body.FlashTimer = WrappingCountdown.Start(1);
        enemies.FrameContext.SeedRandomEnemyCounter(2);
        enemies.CrocomireBehavior.Combat.ApplyCrocomireHurtPalette(body);
        for (int color = 0; color < CrocomireColorDefinitions.FightBodyCount; color++)
            AssertEqual(0x7fff,
                cgram.Colors[CrocomireColorDestinations.FightBodyDestination + color],
                $"Crocomire native white hurt flash {color}");
        AssertEqual(CrocomireDeathPhase.Fighting, state.DeathSequenceIndex,
            "white hurt flash leaves fight phase unchanged");
        state.DeathPhaseDelay = WrappingCountdown.Start(0);
        enemies.CrocomireBehavior.Skeleton.RunCrocomireSkeletonTileLoadAndWallBreak(state);
        AssertBand(CrocomireColorDestinations.SkeletonArmDestination,
            CrocomireColorDefinitions.SkeletonArmCount, colors.ResolveSkeletonArm,
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
        AssertThrows<InvalidDataException>(() => CrocomireColorFile.Write(visual),
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
