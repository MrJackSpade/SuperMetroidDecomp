using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using System.Text.Json;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
/// <summary>Checks installed Crocomire palette bands against native RGB5 data
/// and live boss CGRAM writes under a cartridge-read guard.</summary>
internal static class EnemyTileArtworkCrocomireColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Checks five color transfers, hurt whiteout, death-phase palette use, and out-of-range channel rejection.</summary>
    internal static void VerifyInstalledCrocomireColors(
        ISnesAddressSpace rom, string stockDirectory, ImportEnemyArtwork stock)
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

        CrocomireColorCatalog colors = native;
        CrocomireColorDocument visual = JsonSerializer.Deserialize<CrocomireColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, CrocomireColorFormat.FileName)), CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Crocomire color JSON is null.");

        // Execute the real boss initializer and hurt-flash hook with palette ROM reads
        // forbidden. Their CGRAM writes must come from the installed resource, while the
        // native phase, instruction, and white-flash decisions remain unchanged.
        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= CrocomireSourceDefinitions.FightBodyColors and
                < (CrocomireSourceDefinitions.WallSpikesColors +
                    CrocomireColorDefinitions.WallSpikesCount * sizeof(ushort)),
            address => $"Crocomire accessed migrated palette ROM ${address:X6}.");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { BossArtwork = stock.Bosses };
        Type type = typeof(RoomEnemySystem);
        enemies.BossArtworkBinding.BodySheets = BossBodySheetFixtures.Stock.Sheets;
        enemies.VerificationServices().IsAreaMiniBossDefeated = () => false;
        enemies.VerificationServices().SetRoomScrollState = (_, _) => { };
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Crocomire, body.Definition);
        enemies.CrocomireBehavior.Fight.InitializeCrocomire(body);
        CrocomireEnemyState state = enemies.CrocomireBehavior.Access.Crocomire ?? throw new InvalidOperationException("Verification requires enemies.CrocomireBehavior.Crocomire.");
        AssertEqual(CrocomireDeathPhase.Fighting, state.DeathPhase,
            "installed Crocomire colors leave living fight phase unchanged");
        AssertEqual(new AnimationPlayback<CrocomireBodySegment>(CrocomireBodyAnimations.StartingWith(CrocomireBodySegment.Initial), AnimationPlace.StartOfAnimation),
            state.BodyPlayback,
            "installed Crocomire colors leave the initial program unchanged");
        // The import pre-colours his pictures in each load of his colours, which his colour state names (#1512).
        BossSourceArtwork bosses = stock.Bosses;
        AssertBand(BossSheetShades.SpawnedObjects(bosses, EnemyDefinitionId.Crocomire), CrocomireShades.Wall, 16, colors.ResolveInitialWall, "initial wall");
        enemies.CrocomireBehavior.Combat.ApplyCrocomireHurtFlash(body);
        AssertTrue(state.Colours.ShowsFightBody && !state.Colours.HurtFlash, "a hurt update shows the restored fight body");
        IReadOnlyList<SheetShade> bodyShades = BossSheetShades.Body(BossBodyOwner.Crocomire, bosses, [], []);
        AssertBand(bodyShades, CrocomireShades.FightBody, CrocomireColorDefinitions.FightBodyCount, colors.ResolveFightBody, "restored fight-body");
        body.FlashTimer = 1;
        enemies.FrameContext.SeedRandomEnemyCounter(2);
        enemies.CrocomireBehavior.Combat.ApplyCrocomireHurtFlash(body);
        AssertTrue(state.Colours.HurtFlash, "a flashing hurt update shows the white hurt flash");
        AssertBand(bodyShades, CrocomireShades.BodyHurt, CrocomireColorDefinitions.FightBodyCount, _ => RgbColor.White, "native white hurt flash");
        AssertEqual(CrocomireDeathPhase.Fighting, state.DeathPhase,
            "white hurt flash leaves fight phase unchanged");
        state.DeathPhaseDelay = 0;
        enemies.CrocomireBehavior.Skeleton.RunCrocomireSkeletonRevealAndWallBreak(state);
        AssertTrue(state.Colours.ShowsSkeletonArm, "the wall break shows the skeleton arm's colours");
        AssertBand(BossSheetShades.OwnRow(bosses, EnemyDefinitionId.Crocomire, 0), CrocomireShades.SkeletonArm,
            CrocomireColorDefinitions.SkeletonArmCount, colors.ResolveSkeletonArm, "skeleton-arm");
        AssertEqual(CrocomireDeathPhase.CrumbleBridgeAndSink, state.DeathPhase,
            "installed skeleton-arm colors leave death-phase handoff unchanged");

        visual.FightBody[1] = visual.FightBody[1] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => CrocomireColorFile.Read(new MemoryStream(CrocomireColorFile.Write(visual), writable: false)),
            "reading Crocomire colors rejects an RGB5 channel outside five-bit precision");
        Console.WriteLine("  Crocomire colors: five native RGB5 transfers, live boss CGRAM, white flash, and channel rejection pass.");

        void VerifyBand(int source, int count, Func<int, RgbColor> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed Crocomire {name} color {color} preserves native RGB5");
        }

        void AssertBand(IReadOnlyList<SheetShade> made, PictureShade shade, int count, Func<int, RgbColor> resolve, string name)
        {
            SheetShade found = made.Single(candidate => candidate.Shade == shade);
            for (int color = 0; color < count; color++)
                AssertEqual(resolve(color), found.Colours[color],
                    $"installed Crocomire {name} color {color}");
        }
    }
}
