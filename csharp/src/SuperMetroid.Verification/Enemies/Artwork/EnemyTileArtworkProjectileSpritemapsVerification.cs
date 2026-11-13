using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.SaveStation;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Enemies.Projectiles.EnemyProjectileProgramOracleVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Compares installed enemy-projectile drawing compositions with native OAM and guards their runtime visual reads.</summary>
internal static class EnemyTileArtworkProjectileSpritemapsVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>The graphics index the drawing fixtures give every projectile: palette five, tile offset $0004.</summary>
    private const ushort FixtureGraphicsIndex = 0x0a04;

    /// <summary>
    /// The graphics the production-drawn checks give a projectile: the enemy projectiles' colours, palette row five, at
    /// character zero, where every set composes each object it can spawn (#1392).
    /// </summary>
    private static readonly SpriteGraphics SpawnedGraphics = SpriteGraphics.SharedIn(SuperMetroid.Core.Enemies.Drawing.EnemyShades.InProjectileLook);

    /// <summary>Projectile kinds whose typed programs (#641) present frames by key instead of running a native list.</summary>
    private static readonly HashSet<RoomEnemyProjectileKind> TypedProgramKinds =
    [
        RoomEnemyProjectileKind.MotherBrainGlassShard,
        RoomEnemyProjectileKind.MotherBrainGlassSparkle,
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
        RoomEnemyProjectileKind.MotherBrainHandBeamFired,
        RoomEnemyProjectileKind.MotherBrainRoomTurret,
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
        RoomEnemyProjectileKind.CeresRidleyFireball,
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter,
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter,
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight,
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft,
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp,
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown,
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool,
        RoomEnemyProjectileKind.BombTorizoInitialDrool,
        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
        RoomEnemyProjectileKind.BombTorizoDeathExplosion,
        RoomEnemyProjectileKind.BombTorizoStatueBreaking,
        RoomEnemyProjectileKind.BombTorizoChozoOrb,
        RoomEnemyProjectileKind.BombTorizoSonicBoom,
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb,
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom,
        RoomEnemyProjectileKind.BombTorizoRightFootDust,
        RoomEnemyProjectileKind.BombTorizoLeftFootDust,
        RoomEnemyProjectileKind.GoldenTorizoEgg,
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
        RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
        RoomEnemyProjectileKind.CeresFallingDebrisLight,
        RoomEnemyProjectileKind.CeresFallingDebrisDark,
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud,
    ];

    /// <summary>Checks imported projectile maps and representative animated effects against cartridge OAM at ordinary and wrapped screen coordinates.</summary>
    internal static void VerifyInstalledEnemyProjectileSpritemaps(
        ISnesAddressSpace bus, string extractedDirectory, ImportEnemyArtwork stock)
    {
        EnemyProjectileSpritemapCatalog installed = stock.Enemies.ProjectileSpritemaps
            ?? throw new InvalidDataException("Installed enemy-projectile compositions are missing.");
        foreach ((ushort pointer, _) in EnemyProjectileSpritemapDefinitions.Frames)
        foreach ((ushort originX, ushort originY, ushort graphicsIndex) in new[]
                 {
                     new EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase2(0x0080, 0x0004, 0x0000),
                     new EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase2(0x01ff, 0x00fc, FixtureGraphicsIndex),
                 })
        {
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            nativeOam.BeginFrame();
            installedOam.BeginFrame();
            bool onScreen = (LittleEndianBytes.HighByteInPlace(originY)) == 0;
            DrawImportedEnemyProjectileSpritemap(bus, nativeOam, pointer,
                new(originX, originY), graphicsIndex, onScreen);
            installedOam.AddEnemySpritemap(installed.Get(pointer).Span,
                new Position16<SpritePixels>(originX, originY),
                unchecked((ushort)(LittleEndianBytes.HighByteInPlace(graphicsIndex))),
                unchecked((byte)graphicsIndex),
                clipVerticalWrap: true, originYIsOnScreen: onScreen);
            AssertEqual(nativeOam.NextByteOffset, installedOam.NextByteOffset,
                $"installed $8D:{pointer:X4} OAM part count");
            AssertTrue(nativeOam.ImageEquals(installedOam),
                $"installed $8D:{pointer:X4} OAM pixels, wrapping, tile base and palette");
        }

        foreach ((ushort operand, ushort pointer) in new[]
                 {
                     new EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase3((ushort)SkreeMetareeParticleOperand.Skree,
                         EnemyProjectileCompositionSources.Spritemap(EnemyProjectileFrameId.SkreeDebris)),
                     new EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase3((ushort)SkreeMetareeParticleOperand.Metaree,
                         EnemyProjectileCompositionSources.Spritemap(EnemyProjectileFrameId.MetareeDebris)),
                 })
        {
            ushort nativePointer = ReadNativeEnemyProjectileProgramWord(bus, operand);
            AssertEqual(nativePointer, SkreeMetareeParticleVisualDefinitionsAccess.Resolve(operand),
                $"particle visual operand $86:{operand:X4} matches the pinned cartridge");
            AssertEqual(nativePointer, pointer,
                $"particle visual operand $86:{operand:X4} selects its editable composition");
        }
        Suite(nameof(VerifyBurstVisuals), () => VerifyBurstVisuals(stock));
        Suite(nameof(VerifySharedProgramVisuals), () => VerifySharedProgramVisuals(stock));
        ushort ceresOperand = EnemyProjectileProgramFrameSources.Operand(RidleyFireballVisualDefinitions.ShownProjectileFrames[RidleyFireballVisualDefinitions.ProjectileFrames[0]]);
        OamBuffer nativeCeres = DrawReferenceProgramFrame(stock, ceresOperand, bus,
            RoomEnemyProjectileKind.CeresRidleyFireball);
        OamBuffer installedCeres = DrawProgramFrame(stock, ceresOperand,
            NewEnemyProjectileVisualGuard(bus),
            RoomEnemyProjectileKind.CeresRidleyFireball);
        AssertTrue(nativeCeres.NextByteOffset > 0 &&
            nativeCeres.ImageEquals(installedCeres),
            "Ceres Ridley fireball uses installed OAM without visual ROM reads");
        for (int index = 0;
             index < AlcoonFireballFrameToolingDefinitions.Frames.Length;
             index++)
        {
            ushort operand = EnemyProjectileProgramFrameSources.Operand(AlcoonFireballFrameToolingDefinitions.Frames[index]);
            OamBuffer nativeAlcoon = DrawReferenceProgramFrame(stock, operand, bus,
                RoomEnemyProjectileKind.AlcoonFireball);
            OamBuffer installedAlcoon = DrawProgramFrame(stock, operand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball);
            AssertTrue(nativeAlcoon.NextByteOffset > 0 &&
                InstalledStandsFor(nativeAlcoon, installedAlcoon, stock, operand, RoomEnemyProjectileKind.AlcoonFireball),
                $"Alcoon fireball frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(nativeAlcoon)}, installed {OamRecords(installedAlcoon)}");
        }
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in new VerifyInstalledEnemyProjectileSpritemapsCase[]
                 {
                     new VerifyInstalledEnemyProjectileSpritemapsCase("Golden Torizo Super Missile",
                         RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
                         TorizoFrameCount(TorizoProjectileFamily.SuperMissile),
                         TorizoFrameKey(TorizoProjectileFamily.SuperMissile)),
                     new VerifyInstalledEnemyProjectileSpritemapsCase("Golden Torizo eye beam",
                         RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
                         TorizoFrameCount(TorizoProjectileFamily.EyeBeam),
                         TorizoFrameKey(TorizoProjectileFamily.EyeBeam)),
                 })
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                if (IsOnNoSheet(operand))
                    continue;
                OamBuffer native = DrawReferenceProgramFrame(stock, operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(InstalledStandsFor(native, extracted, stock, operand, kind),
                $"{family} frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
            }
        }
        for (int index = 0;
             index < TorizoFrameCount(TorizoProjectileFamily.Egg);
             index++)
        {
            ushort operand = TorizoFrameKey(TorizoProjectileFamily.Egg)(index);
            OamBuffer native = DrawReferenceProgramFrame(stock, operand, bus,
                RoomEnemyProjectileKind.GoldenTorizoEgg);
            OamBuffer extracted = DrawProgramFrame(stock, operand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg);
            AssertTrue(InstalledStandsFor(native, extracted, stock, operand, RoomEnemyProjectileKind.GoldenTorizoEgg),
                $"Golden Torizo egg frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
        }
        var torizoFamilies = new TorizoFamily[]
        {
            new TorizoFamily("drool", RoomEnemyProjectileKind.BombTorizoLowHealthDrool,
                TorizoFrameCount(TorizoProjectileFamily.Drool),
                TorizoFrameKey(TorizoProjectileFamily.Drool)),
            new TorizoFamily("explosive swipe", RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
                TorizoFrameCount(TorizoProjectileFamily.Swipe),
                TorizoFrameKey(TorizoProjectileFamily.Swipe)),
            new TorizoFamily("sonic boom", RoomEnemyProjectileKind.BombTorizoSonicBoom,
                TorizoFrameCount(TorizoProjectileFamily.SonicBoom),
                TorizoFrameKey(TorizoProjectileFamily.SonicBoom)),
            new TorizoFamily("landing dust", RoomEnemyProjectileKind.BombTorizoRightFootDust,
                TorizoFrameCount(TorizoProjectileFamily.LandingDust),
                TorizoFrameKey(TorizoProjectileFamily.LandingDust)),
            new TorizoFamily("explosion", RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
                TorizoFrameCount(TorizoProjectileFamily.Explosion),
                TorizoFrameKey(TorizoProjectileFamily.Explosion)),
            new TorizoFamily("Chozo orb", RoomEnemyProjectileKind.BombTorizoChozoOrb,
                TorizoFrameCount(TorizoProjectileFamily.ChozoOrb),
                TorizoFrameKey(TorizoProjectileFamily.ChozoOrb)),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in torizoFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                if (IsOnNoSheet(operand))
                    continue;
                OamBuffer native = DrawReferenceProgramFrame(stock, operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(InstalledStandsFor(native, extracted, stock, operand, kind),
                $"Torizo {family} frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
            }
        }
        var genericDeathFamilies = new TorizoFamily[]
        {
            ShownFamily("enemy pickup", RoomEnemyProjectileKind.EnemyDeathPickup, PickupFrames),
            ShownFamily("enemy death", RoomEnemyProjectileKind.EnemyDeathExplosion,
                EnemyDeathProjectileAnimations.Segments.Frames.Except(PickupFrames)),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in genericDeathFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                if (IsOnNoSheet(operand))
                    continue;
                OamBuffer native = DrawReferenceProgramFrame(stock, operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(InstalledStandsFor(native, extracted, stock, operand, kind),
                $"{family} frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
            }
        }
        var environmentAndAttackFamilies = new TorizoFamily[]
        {
            new TorizoFamily("Ceres falling debris", RoomEnemyProjectileKind.CeresFallingDebrisLight,
                CeresFallingDebrisVisualDefinitions.Frames.Count,
                index => EnemyProjectileProgramFrameSources.Operand(CeresFallingDebrisVisualDefinitions.ShownProjectileFrames[CeresFallingDebrisVisualDefinitions.Frames[index]])),
            ShownFamily("save-station electricity", RoomEnemyProjectileKind.SaveStationElectricity, SaveStationElectricityAnimations.Segments.Frames),
            new TorizoFamily("gunship dust", RoomEnemyProjectileKind.GunshipLiftoffDustCloud,
                GunshipDustVisualDefinitions.Frames.Count,
                index => EnemyProjectileProgramFrameSources.Operand(GunshipDustVisualDefinitions.ShownProjectileFrames[GunshipDustVisualDefinitions.Frames[index]])),
            ShownFamily("falling spark", RoomEnemyProjectileKind.FallingSpark, FallingSparkAnimations.Segments.Frames),
            ShownFamily("Magdollite lava", RoomEnemyProjectileKind.LavaThrownByMagdollite, MagdolliteLavaAnimations.Segments.Frames),
            ShownFamily("Chozo/Tourian dust", RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep, ChozoTourianDustAnimations.Segments.Frames),
            ShownFamily("eye-door sweat", RoomEnemyProjectileKind.EyeDoorSweat, EyeDoorSweatAnimations.Segments.Frames),
            ShownFamily("Ki Hunter acid spit", RoomEnemyProjectileKind.KiHunterAcidSpitLeft, KiHunterAcidSpitAnimations.Segments.Frames),
            ShownFamily("Fune/Namihe fireball", RoomEnemyProjectileKind.FuneFireball, FuneNamiheFireballAnimations.Segments.Frames),
            ShownFamily("Dragon fireball", RoomEnemyProjectileKind.DragonFireball, DragonFireballAnimations.Segments.Frames),
            ShownFamily("Powamp spike", RoomEnemyProjectileKind.PowampSpike, PowampSpikeAnimations.Segments.Frames),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in environmentAndAttackFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                if (IsOnNoSheet(operand))
                    continue;
                OamBuffer native = DrawReferenceProgramFrame(stock, operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(InstalledStandsFor(native, extracted, stock, operand, kind),
                $"{family} frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
            }
        }
        var motherBrainAndStatueFamilies = new TorizoFamily[]
        {
            new TorizoFamily("Bomb Torizo statue fragments", RoomEnemyProjectileKind.BombTorizoStatueBreaking,
                TorizoFrameCount(TorizoProjectileFamily.StatueFragment),
                TorizoFrameKey(TorizoProjectileFamily.StatueFragment)),
            new TorizoFamily("Mother Brain glass", RoomEnemyProjectileKind.MotherBrainGlassShard,
                MotherBrainGlassVisualDefinitions.ProjectileFrameCount,
                index => EnemyProjectileProgramFrameSources.Operand(MotherBrainGlassVisualDefinitions.ShownProjectileFrames[MotherBrainGlassVisualDefinitions.ProjectileFrames[index]])),
            new TorizoFamily("Mother Brain hand beam", RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
                MotherBrainHandBeamVisualDefinitions.ProjectileFrameCount,
                index => EnemyProjectileProgramFrameSources.Operand(MotherBrainHandBeamVisualDefinitions.ShownProjectileFrames[MotherBrainHandBeamVisualDefinitions.ProjectileFrames[index]])),
            new TorizoFamily("Mother Brain top tube", RoomEnemyProjectileKind.MotherBrainTopRightTube,
                MotherBrainTopTubeInstructionProgramDefinitionsTooling.PresentationWordCount,
                MotherBrainTopTubeInstructionProgramDefinitionsTooling.PresentationWordAddress),
            new TorizoFamily("Mother Brain turret", RoomEnemyProjectileKind.MotherBrainRoomTurret,
                MotherBrainTurretVisualDefinitions.ProjectileFrameCount,
                index => EnemyProjectileProgramFrameSources.Operand(MotherBrainTurretVisualDefinitions.ShownProjectileFrames[MotherBrainTurretVisualDefinitions.ProjectileFrames[index]])),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in motherBrainAndStatueFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                if (IsOnNoSheet(operand))
                    continue;
                OamBuffer native = DrawReferenceProgramFrame(stock, operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand, NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(InstalledStandsFor(native, extracted, stock, operand, kind),
                    $"{family} frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
            }
        }
        // $86:D5F2 selects the second operand on this even frame; reflected X remains 128.
        OamBuffer nativeShard = DrawReferenceProgramFrame(stock, 0xd485, bus);
        OamBuffer installedShard = DrawNoobTubeShard(stock,
            NewEnemyProjectileVisualGuard(bus));
        AssertTrue(nativeShard.NextByteOffset > 0 &&
            nativeShard.ImageEquals(installedShard),
            "n00b-tube flicker instruction uses installed OAM without visual ROM reads");

        var installedSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0)) };
        var installedArrival = new CeresElevatorArrivalState(installedSamus);
        for (int frame = 0; frame < 3; frame++)
        {
            installedArrival.Step(installedSamus);
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            nativeOam.BeginFrame();
            installedOam.BeginFrame();
            // During the native sixty-frame wait, the pad stays at Y=28 and alternates
            // its two one-tick maps; the concealer stays at Y=97 with one fixed map.
            ushort padOperand = (frame & 1) == 0 ? (ushort)0xa28f : (ushort)0xa293;
            ushort padPointer = ReadNativeEnemyProjectileProgramWord(bus, padOperand);
            ushort platformPointer = ReadNativeEnemyProjectileProgramWord(bus, 0xa29b);
            DrawImportedEnemyProjectileSpritemap(bus, nativeOam, padPointer, new(128, 28), 0, true);
            DrawImportedEnemyProjectileSpritemap(bus, nativeOam, platformPointer, new(128, 97), 0, true);
            installedArrival.DrawAsOam(installedOam, SuperMetroid.TestSupport.RepositoryInstallation.Verification.SpawnedObjectFixtures.Presentation(SuperMetroid.Core.World.RoomStates.CeresRoomStateDefinitions.CeresElevDefault), Position16<RoomPixels>.Zero);
            AssertTrue(nativeOam.ImageEquals(installedOam),
                $"Ceres elevator arrival frame {frame} draws from installed $8D compositions");
        }

        EnemyProjectileSpritemapDocument document =
            RequiredJson.Deserialize<EnemyProjectileSpritemapDocument>(
                File.ReadAllBytes(Path.Combine(extractedDirectory,
                    EnemyProjectileSpritemapDefinitions.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        EnemyProjectilePresentationFrameDefinition omittedProgramFrame =
            EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames[0];
        var incompleteCurrent = document.Frames
            .Where(entry => entry.Key != "skree_debris")
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        byte[] invalidCurrentJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDocuments.Version,
                Frames = incompleteCurrent,
                ProgramFrames = document.ProgramFrames,
            }, CamelCaseJson);
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileSpritemapDocuments.Load(new MemoryStream(invalidCurrentJson)),
            "current projectile compositions cannot silently omit Skree debris");
        var incompleteProgramFrames = (document.ProgramFrames ?? throw new InvalidOperationException("Verification requires document.ProgramFrames."))
            .Where(entry => entry.Key != omittedProgramFrame.Name)
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        byte[] invalidProgramJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDocuments.Version,
                Frames = document.Frames,
                ProgramFrames = incompleteProgramFrames,
            }, CamelCaseJson);
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileSpritemapDocuments.Load(new MemoryStream(invalidProgramJson)),
            "current projectile compositions cannot silently omit a shared program frame");

        Console.WriteLine($"  Enemy projectile visuals: " +
            $"{EnemyProjectilePresentationFrameSets.All.Length} catalogued timed/flicker frames " +
            "match native OAM; installed draws and incomplete-document rejection pass.");

        void VerifySharedProgramVisuals(ImportEnemyArtwork artwork)
        {
            ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> frames =
                EnemyProjectilePresentationFrameSets.All;
            AssertTrue(frames.Length > 300,
                "translated enemy projectile programs expose their complete catalogued visual frame set");
            foreach (EnemyProjectilePresentationFrameDefinition frame in frames)
            {
                ushort nativePointer = ReadNativeEnemyProjectileProgramWord(bus, frame.OperandAddress);
                foreach ((ushort originX, ushort originY) in new[]
                         {
                             new EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase3(128, 96),
                             new EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase3(0x01ff, 0x00fc),
                         })
                {
                    var nativeOam = new OamBuffer();
                    var installedOam = new OamBuffer();
                    nativeOam.BeginFrame();
                    installedOam.BeginFrame();
                    DrawImportedEnemyProjectileSpritemap(bus, nativeOam, nativePointer,
                        new(originX, originY), FixtureGraphicsIndex, originYIsOnScreen: true);
                    installedOam.AddEnemySpritemap(
                        installed.GetProgramFrame(frame.OperandAddress).Span,
                        new Position16<SpritePixels>(originX, originY), 0x0a00, 4,
                        clipVerticalWrap: true, originYIsOnScreen: true);
                    AssertTrue(nativeOam.NextByteOffset == installedOam.NextByteOffset &&
                        nativeOam.ImageEquals(installedOam),
                        $"installed shared frame $86:{frame.OperandAddress:X4} matches ROM OAM at {originX:X4},{originY:X4}");
                }
                if (EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames.Any(manifest => manifest.OperandAddress == frame.OperandAddress))
                {
                    EnemyProjectileFrameId shown = EnemyProjectileProgramFrameOperands.Frame(frame.OperandAddress);
                    if (SuperMetroid.AssetExtraction.Enemies.Artwork.SpawnedObjectFrameDefinitions.Common.Any(common => common.Objects.Projectiles.Contains(shown)))
                    {
                        OamBuffer sharedReference = DrawReferenceProgramFrame(artwork, frame.OperandAddress, bus);
                        OamBuffer sharedDrawn = DrawSharedFrame(artwork, frame.OperandAddress, NewEnemyProjectileVisualGuard(bus));
                        AssertTrue(InstalledStandsFor(sharedReference, sharedDrawn, artwork, frame.OperandAddress, RoomEnemyProjectileKind.MiscDustExplosion),
                            $"shared frame $86:{frame.OperandAddress:X4} runs without visual ROM reads: " +
                            $"records (x, y, tile, attributes) native {OamRecords(sharedReference)}, installed {OamRecords(sharedDrawn)}");
                    }
                    else
                    {
                        // A frame only some enemy spawns is drawn as the game draws it: from that spawner's set, at an
                        // artwork step it is drawn, under each graphics the spawner spawns it with (#1513).
                        foreach ((SpriteGraphics graphics, Func<RoomEnemySystem, ushort> word, EnemyDefinitionId spawner) in SpawnerDraws(shown))
                        {
                            RoomEnemySystem system = SpawnerSystem(artwork, spawner, shown);
                            OamBuffer reference = DrawReferenceProgramFrameWith(frame.OperandAddress, bus, word(system));
                            OamBuffer drawn = DrawSpawnerFrame(system, frame.OperandAddress, graphics);
                            AssertTrue(reference.LowTable.SequenceEqual(drawn.LowTable) || SharedPictureStandsFor(system, reference),
                                $"{spawner}'s frame $86:{frame.OperandAddress:X4} runs without visual ROM reads under {graphics.Owner}: " +
                                $"records (x, y, tile, attributes) native {OamRecords(reference)}, installed {OamRecords(drawn)}");
                        }
                    }
                }
            }
        }

        void VerifyBurstVisuals(ImportEnemyArtwork artwork)
        {
            foreach (ushort program in new[]
                     {
                         SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleProgram.Skree),
                         SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleProgram.Metaree),
                     })
            {
                EnemyDefinitionId spawner = BurstSpawner(program);
                RoomEnemySystem burstSystem = BurstSystem(artwork, spawner);
                OamBuffer native = DrawReferenceBurst(program, bus, NativeSpriteGraphicsWords.GraphicsWord(
                    burstSystem.GraphicsSet.TileBaseOf(spawner), burstSystem.GraphicsSet.DrawRow(spawner)));
                OamBuffer installedBurst = DrawBurst(artwork, program,
                    NewEnemyProjectileVisualGuard(bus));
                AssertTrue(native.NextByteOffset > 0 &&
                    installedBurst.StandsFor(native),
                    $"installed Skree/Metaree burst ${program:X4} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(installedBurst)}");
            }
        }

        // Expected OAM comes from the cartridge's visual operand and bank-$8D
        // composition, not the installed catalog or production draw callback.
        // The cartridge's draw of a program frame under the graphics word its spawn writes: the shared sheet's for an object
        // any room spawns, else its spawner's own sheet's in the set that loads it, with the row its loaded colours select.
        static OamBuffer DrawReferenceProgramFrame(ImportEnemyArtwork artwork, ushort operand, ISnesAddressSpace source,
            RoomEnemyProjectileKind _ = RoomEnemyProjectileKind.MiscDustExplosion)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork.Bosses };
            SpriteGraphics graphics = SpawnedObjectFixtures.LoadSetSpawning(enemies,
                EnemyProjectileProgramFrameOperands.Frame(operand), SpawnedGraphics);
            ushort word = graphics.Owner == SpawnedObjectOwner.Shared
                ? NativeSpriteGraphicsWords.GraphicsWord(SpawnedGraphics, SpawnedGraphics.FixedPaletteRow)
                : NativeSpriteGraphicsWords.GraphicsWord(graphics, enemies, graphics.PaletteRowIn(enemies));
            ushort pointer = ReadNativeEnemyProjectileProgramWord(source, operand);
            var oam = new OamBuffer();
            oam.BeginFrame();
            DrawImportedEnemyProjectileSpritemap(source, oam, pointer, new(128, 96), word, originYIsOnScreen: true);
            return oam;
        }

        // The native particles draw with their spawner's graphics word (#1455): the row holding its colours and its set's
        // tile base.
        static OamBuffer DrawReferenceBurst(ushort program, ISnesAddressSpace source, ushort spawnerWord)
        {
            ushort pointer = ReadNativeEnemyProjectileProgramWord(source, unchecked((ushort)(program + 2)));
            var oam = new OamBuffer();
            oam.BeginFrame();
            // Native Skree/Metaree spawn order: two right particles then two left.
            // Initialization offsets X by six; no motion tick precedes this draw.
            foreach (ushort x in new ushort[] { 134, 134, 122, 122 })
                DrawImportedEnemyProjectileSpritemap(source, oam, pointer, new(x, 96),
                    spawnerWord, originYIsOnScreen: true);
            return oam;
        }

        static OamBuffer DrawBurst(ImportEnemyArtwork artwork, ushort program,
            ISnesAddressSpace source)
        {
            EnemyDefinitionId spawner = BurstSpawner(program);
            bool skree = spawner == EnemyDefinitionId.Skree;
            RoomEnemySystem enemies = BurstSystem(artwork, spawner);
            Action<RoomEnemySlot> spawn = slot => enemies.Projectiles.SpawnMetalParticleBurst(slot,
                skree ? SuperMetroid.Core.Enemies.Skree.SkreeParticleDefinitions.Burst : SuperMetroid.Core.Enemies.Metaree.MetareeParticleDefinitions.Burst);
            var enemy = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)),
            };
            // The spawner is the enemy its set loads, in its sheet's standard colours, as a room's spawn would leave it;
            // its particles draw from its own spawned-object sheet (#1513).
            enemy.Bind(spawner, CompiledRoomEnemyDefinitions.Instance.ReadEnemyDefinition(spawner));
            enemy.Shading = PictureShading.Standard;
            spawn(enemy);
            foreach (EnemyProjectile particle in enemies.Projectiles.EnemyProjectiles
                         .Where(particle => particle.IsActive))
            {
                particle.FrameHold = WrappingCountdown.Start(1);
                enemies.Projectiles.ProcessEnemyProjectileInstructions(particle, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), Position16<RoomPixels>.Zero);
            }
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, new(0, 0));
            return oam;
        }

        static EnemyDefinitionId BurstSpawner(ushort program) =>
            program == SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleProgram.Skree)
                ? EnemyDefinitionId.Skree : EnemyDefinitionId.Metaree;

        // The particles draw from the sheets a set loading their spawner composes from this artwork.
        static RoomEnemySystem BurstSystem(ImportEnemyArtwork artwork, EnemyDefinitionId spawner)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork.Bosses };
            SpawnedObjectFixtures.LoadSetDrawing(enemies, spawner);
            return enemies;
        }

        // The native frame under a graphics word: the tile base and the row the game's draw ORs into every part.
        // Whether the frame drawn last stands for the native records through another frame whose identical picture its
        // sheet keeps once: the OAM of one of the compositions that picture stands for is the native OAM (#1513).
        static bool SharedPictureStandsFor(RoomEnemySystem enemies, OamBuffer reference)
        {
            var frame = new SpriteFrameBuffer();
            frame.BeginFrame();
            enemies.Projectiles.DrawEnemyProjectilesAt(frame, Position16<RoomPixels>.Zero, EnemyProjectileDepth.InFrontOfSamus, timeIsFrozen: false);
            enemies.Projectiles.DrawEnemyProjectilesAt(frame, Position16<RoomPixels>.Zero, EnemyProjectileDepth.BehindSamus, timeIsFrozen: false);
            frame.FinalizeFrame();
            if (frame.Draws is not [ImageSpriteDraw image])
                return false;
            return SheetFrameSources.CharacterDrawCandidates(image).Any(candidate =>
                SpriteFrameBufferAccess.Assemble([candidate], parkUnused: false).LowTable.SequenceEqual(reference.LowTable));
        }

        // The used records of an OAM low table, for a failure message.
        static string OamRecords(OamBuffer oam) => string.Join(" ",
            Enumerable.Range(0, oam.NextByteOffset / 4).Select(record =>
                $"({oam.LowTable[record * 4]:X2},{oam.LowTable[(record * 4) + 1]:X2},{oam.LowTable[(record * 4) + 2]:X2},{oam.LowTable[(record * 4) + 3]:X2})"));

        // A frame on no sheet, so no set loads it (#1513): one showing the empty spritemap draws nothing, and these are never
        // started: no code spawns the Wrecked Ship Chozo's spike-clearing explosions or Shaktool's attack circles. Any other
        // frame a spawn table lacks fails loudly in the set load, as a frame the game can draw must be on a sheet.
        static bool IsOnNoSheet(ushort operand)
        {
            EnemyProjectileFrameId frame = EnemyProjectileProgramFrameOperands.Frame(operand);
            return SuperMetroid.Core.Enemies.Projectiles.Artwork.EnemyProjectileEmptyFrameDefinitions.EmptySpritemapFrames.Contains(frame)
                || frame is EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsFirst or EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsSecond
                    or EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsThird or EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsFourth
                    or EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsFifth or EnemyProjectileFrameId.ChozoTourianDustSpikeClearingExplosionsSixth
                    or EnemyProjectileFrameId.ShaktoolFrontFirst or EnemyProjectileFrameId.ShaktoolFrontSecond
                    or EnemyProjectileFrameId.ShaktoolFrontLoop or EnemyProjectileFrameId.ShaktoolMiddleFirst
                    or EnemyProjectileFrameId.ShaktoolMiddleSecond or EnemyProjectileFrameId.ShaktoolMiddleLoop
                    or EnemyProjectileFrameId.ShaktoolBack or EnemyProjectileFrameId.ShaktoolBackLoop;
        }

        static OamBuffer DrawReferenceProgramFrameWith(ushort operand, ISnesAddressSpace source, ushort word)
        {
            ushort pointer = ReadNativeEnemyProjectileProgramWord(source, operand);
            var oam = new OamBuffer();
            oam.BeginFrame();
            DrawImportedEnemyProjectileSpritemap(source, oam, pointer, new(128, 96), word, originYIsOnScreen: true);
            return oam;
        }

        // Each way a spawner draws a frame only it spawns: from its sets' shared sheet when it spawns the frame with room
        // graphics, from its own sheet when it spawns it with its own; the first spawner a retail room draws.
        static IEnumerable<(SpriteGraphics Graphics, Func<RoomEnemySystem, ushort> Word, EnemyDefinitionId Spawner)> SpawnerDraws(EnemyProjectileFrameId frame)
        {
            var tables = SuperMetroid.AssetExtraction.Enemies.Artwork.SpawnedObjectFrameDefinitions.SharedByEnemy
                .Where(pair => pair.Value.Projectiles.Contains(frame) && SpawnedObjectFixtures.Drawn(pair.Key)).Select(pair => (pair.Key, Shared: true))
                .Concat(SuperMetroid.AssetExtraction.Enemies.Artwork.SpawnedObjectFrameDefinitions.ByEnemy
                    .Where(pair => pair.Value.Projectiles.Contains(frame) && SpawnedObjectFixtures.Drawn(pair.Key)).Select(pair => (pair.Key, Shared: false)))
                .ToList();
            if (tables.Count == 0)
                throw new InvalidOperationException($"No retail-drawn enemy spawns projectile frame {frame}.");
            // The reference draws under the graphics word the game writes for those graphics in the loaded set: their
            // owner's tile base and the row their shading selects there.
            static ushort Word(SpriteGraphics graphics, RoomEnemySystem system) =>
                NativeSpriteGraphicsWords.GraphicsWord(graphics, system, graphics.PaletteRowIn(system));
            if (tables.Any(table => table.Shared))
            {
                EnemyDefinitionId sharer = tables.First(table => table.Shared).Key;
                var shared = SpriteGraphics.SharedIn(SharedSpawnShading(sharer));
                yield return (shared, system => Word(shared, system), sharer);
            }
            if (tables.Any(table => !table.Shared))
            {
                EnemyDefinitionId owner = tables.First(table => !table.Shared).Key;
                // SpawnedBy(spawner): its own sheet, in the shading its slot has on load.
                var own = new SpriteGraphics(SpawnedObjectOwner.Of(owner), PictureShading.Standard);
                yield return (own, system => Word(own, system), owner);
            }
        }

        // The shading a spawner hands its room-graphics spawns as its room loads: Mother Brain's parts spawn theirs in her
        // own colours (SharedIn(Colours.OwnShading)), which nothing has laid shades over yet; other spawners' room-graphics
        // spawns show the enemy-projectile colours.
        static PictureShading SharedSpawnShading(EnemyDefinitionId spawner) =>
            spawner is EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead or EnemyDefinitionId.MotherBrainTubes
                ? new MotherBrainColours().OwnShading
                : EnemyShades.InProjectileLook;

        // The spawner's set loaded, its artwork sequences at a step where the frame is drawn.
        static RoomEnemySystem SpawnerSystem(ImportEnemyArtwork artwork, EnemyDefinitionId spawner, EnemyProjectileFrameId frame)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork.Bosses };
            SpawnedObjectFixtures.LoadSetDrawing(enemies, spawner);
            SpawnedObjectFixtures.ShowDrawnLooks(enemies, SuperMetroid.AssetExtraction.Enemies.FrameDrawings.Of(frame));
            return enemies;
        }

        // The frame drawn from the loaded set's sheets under the spawner's graphics.
        static OamBuffer DrawSpawnerFrame(RoomEnemySystem enemies, ushort presentationKey, SpriteGraphics graphics)
        {
            EnemyProjectile slot = enemies.Projectiles.EnemyProjectiles[0];
            slot.Kind = RoomEnemyProjectileKind.MiscDustExplosion;
            slot.Position = slot.Position.WithWhole(new(128, 96));
            slot.Graphics = graphics;
            slot.Frame = EnemyProjectileProgramFrameOperands.Frame(presentationKey);
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            return oam;
        }

        static OamBuffer DrawSharedFrame(ImportEnemyArtwork artwork,
            ushort operand, ISnesAddressSpace source) =>
            DrawProgramFrame(artwork, operand, source,
                RoomEnemyProjectileKind.MiscDustExplosion);

        static OamBuffer DrawProgramFrame(ImportEnemyArtwork artwork,
            ushort operand, ISnesAddressSpace source, RoomEnemyProjectileKind kind)
        {
            // Typed programs (#641) present their frames by key.
            return TypedProgramKinds.Contains(kind) || EnemyProjectileTypedStarts.Initial(kind) is not null ||
                TourianStatueProjectileStarts.ForKind(kind) is not null || MotherBrainProjectileStarts.ForKind(kind) is not null
                ? DrawPresentedFrame(artwork, operand, source, kind)
                : throw new InvalidOperationException($"Projectile {kind} plays no typed program.");
        }

        static OamBuffer DrawPresentedFrame(ImportEnemyArtwork artwork,
            ushort presentationKey, ISnesAddressSpace source, RoomEnemyProjectileKind kind)
        {
            RoomEnemySystem enemies = PresentedSystem(artwork, presentationKey, kind);
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            return oam;
        }

        // A room whose first projectile shows the program frame the key installs, drawn from the sheets of a set spawning it.
        static RoomEnemySystem PresentedSystem(ImportEnemyArtwork artwork, ushort presentationKey, RoomEnemyProjectileKind kind)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork.Bosses };
            EnemyProjectile slot = enemies.Projectiles.EnemyProjectiles[0];
            slot.Kind = kind;
            slot.Position = slot.Position.WithWhole(new(128, 96));
            // As the typed runner does when it shows the program frame the key installs.
            slot.Frame = EnemyProjectileProgramFrameOperands.Frame(presentationKey);
            // It draws from the sheets a set spawning it composes from this artwork, under the graphics that spawn gives it.
            EnemyProjectileFrameId shown = slot.Frame ?? throw new InvalidOperationException($"Key ${presentationKey:X4} shows a frame.");
            slot.Graphics = SpawnedObjectFixtures.LoadSetSpawning(enemies, shown, SpawnedGraphics);
            SpawnedObjectFixtures.ShowDrawnLooks(enemies, SuperMetroid.AssetExtraction.Enemies.FrameDrawings.Of(shown));
            return enemies;
        }

        // Whether the installed draw of a program frame stands for the native OAM: the same image, or the OAM of another
        // composition the frame's shared picture stands for (#1513).
        static bool InstalledStandsFor(OamBuffer native, OamBuffer installed, ImportEnemyArtwork artwork, ushort operand,
            RoomEnemyProjectileKind kind) =>
            native.NextByteOffset == installed.NextByteOffset &&
            (native.ImageEquals(installed) || SharedPictureStandsFor(PresentedSystem(artwork, operand, kind), native));

        static OamBuffer DrawNoobTubeShard(ImportEnemyArtwork artwork,
            ISnesAddressSpace source)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork.Bosses };
            EnemyProjectile slot = enemies.Projectiles.EnemyProjectiles[0];
            slot.Kind = RoomEnemyProjectileKind.NoobTubeShard;
            slot.State = new NoobTubeDebrisState(column: 128);
            slot.Position = slot.Position.WithWhole(new(128, 96));
            slot.Graphics = SpawnedGraphics;
            // The first shard's animation opens on its flying loop, whose first beat is the reflecting flicker.
            slot.StartAnimation(new NoobTubeProjectileAnimation(NoobTubeProjectileProgram.Shard0));
            enemies.Projectiles.ProcessEnemyProjectileInstructions(slot, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), Position16<RoomPixels>.Zero);
            // It draws from the sheets a set spawning it composes from this artwork.
            slot.Graphics = SpawnedObjectFixtures.LoadSetSpawning(enemies,
                slot.Frame ?? throw new InvalidOperationException("The shard shows a frame."), SpawnedGraphics);
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            return oam;
        }
    }

    /// <summary>Rejects runtime reads of bank-$8D compositions and bank-$86 projectile presentation bytes covered by installed artwork.</summary>
    private static CartridgeReadGuard NewEnemyProjectileVisualGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                ushort low = unchecked((ushort)address);
                return (SnesAddressMath.BankBase(address)) == 0x8d0000 ||
                    (SnesAddressMath.BankBase(address)) == 0x860000 &&
                    (EnemyProjectileProgramFrameOperands.Contains(low) ||
                     EnemyProjectileProgramFrameOperands.Contains(
                         unchecked((ushort)(low - 1))) ||
                     low is (ushort)SkreeMetareeParticleOperand.Skree or
                        (ushort)SkreeMetareeParticleOperand.Metaree ||
                     low == (ushort)SkreeMetareeParticleOperand.Skree + 1 ||
                     low == (ushort)SkreeMetareeParticleOperand.Metaree + 1);
            },
            address => $"Installed enemy projectile reread visual byte ${address:X6}.");

    /// <summary>The installed frames of Torizo projectile family <paramref name="family"/>.</summary>
    private static int TorizoFrameCount(TorizoProjectileFamily family) => TorizoProjectileVisualDefinitions.Frames(family).Count;

    /// <summary>The installed artwork key of each frame of Torizo projectile family <paramref name="family"/>, by index.</summary>
    private static Func<int, ushort> TorizoFrameKey(TorizoProjectileFamily family) =>
        index => EnemyProjectileProgramFrameSources.Operand(TorizoProjectileVisualDefinitions.ShownProjectileFrames[TorizoProjectileVisualDefinitions.Frames(family)[index]]);

    /// <summary>The drops a dying enemy leaves: the frames of the pickup animations.</summary>
    private static readonly EnemyProjectileFrameId[] PickupFrames =
    [
        .. new[]
        {
            EnemyDeathProjectileProgram.SmallEnergy, EnemyDeathProjectileProgram.BigEnergy, EnemyDeathProjectileProgram.Missiles,
            EnemyDeathProjectileProgram.SuperMissiles, EnemyDeathProjectileProgram.PowerBombs,
        }.SelectMany(program => EnemyDeathProjectileAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyProjectileFrameId>().Distinct(),
    ];

    /// <summary>A projectile family drawn as <paramref name="kind"/>, whose frames are <paramref name="frames"/>.</summary>
    private static TorizoFamily ShownFamily(string name, RoomEnemyProjectileKind kind, IEnumerable<EnemyProjectileFrameId> frames)
    {
        EnemyProjectileFrameId[] shown = [.. frames];
        return new TorizoFamily(name, kind, shown.Length, index => EnemyProjectileProgramFrameSources.Operand(shown[index]));
    }
}
