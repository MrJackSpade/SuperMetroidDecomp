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
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Areas.Ceres.Artwork;
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
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
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
        RoomEnemyProjectileKind.RidleyFireball,
        RoomEnemyProjectileKind.RidleyAfterburnCenter,
        RoomEnemyProjectileKind.RidleyHorizontalAfterburn,
        RoomEnemyProjectileKind.RidleyVerticalAfterburn,
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool,
        RoomEnemyProjectileKind.TorizoInitialDrool,
        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
        RoomEnemyProjectileKind.TorizoDeathExplosion,
        RoomEnemyProjectileKind.BombTorizoStatueBreaking,
        RoomEnemyProjectileKind.BombTorizoChozoOrb,
        RoomEnemyProjectileKind.BombTorizoSonicBoom,
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb,
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom,
        RoomEnemyProjectileKind.TorizoLandingDust,
        RoomEnemyProjectileKind.GoldenTorizoEgg,
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
        RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
        RoomEnemyProjectileKind.CeresFallingDebris,
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud,
        // The shared dust and explosion programs are typed (#1760): the misc-dust kind is spawned with a program, not one
        // its kind starts, and the shared-frame draws present each of their frames on it.
        RoomEnemyProjectileKind.DustOrExplosion,
        // Ki-Hunter acid's spawner starts the program of the way it spits it (#2278).
        RoomEnemyProjectileKind.KiHunterAcidSpit,
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
                new Position16<ScreenPixels>(originX, originY),
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
        EnemyProjectileFrameId ceresFireball = RidleyFireballAnimations.ProjectileFrames[0];
        OamBuffer nativeCeres = DrawReferenceProgramFrame(stock, ceresFireball, bus,
            RoomEnemyProjectileKind.RidleyFireball);
        OamBuffer installedCeres = DrawProgramFrame(stock, ceresFireball,
            NewEnemyProjectileVisualGuard(bus),
            RoomEnemyProjectileKind.RidleyFireball);
        AssertTrue(nativeCeres.NextByteOffset > 0 &&
            nativeCeres.ImageEquals(installedCeres),
            "Ceres Ridley fireball uses installed OAM without visual ROM reads");
        for (int index = 0;
             index < AlcoonFireballFrameToolingDefinitions.Frames.Length;
             index++)
        {
            EnemyProjectileFrameId fireball = AlcoonFireballFrameToolingDefinitions.Frames[index];
            OamBuffer nativeAlcoon = DrawReferenceProgramFrame(stock, fireball, bus,
                RoomEnemyProjectileKind.AlcoonFireball);
            OamBuffer installedAlcoon = DrawProgramFrame(stock, fireball,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball);
            AssertTrue(nativeAlcoon.NextByteOffset > 0 &&
                InstalledStandsFor(nativeAlcoon, installedAlcoon, stock, fireball, RoomEnemyProjectileKind.AlcoonFireball),
                $"Alcoon fireball frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(nativeAlcoon)}, installed {OamRecords(installedAlcoon)}");
        }
        foreach ((string family, RoomEnemyProjectileKind kind, IReadOnlyList<EnemyProjectileFrameId> frames) in new ProjectileFrameFamily[]
                 {
                     TorizoFamily("Golden Torizo Super Missile", RoomEnemyProjectileKind.GoldenTorizoSuperMissile, TorizoProjectileFamily.SuperMissile),
                     TorizoFamily("Golden Torizo eye beam", RoomEnemyProjectileKind.GoldenTorizoEyeBeam, TorizoProjectileFamily.EyeBeam),
                 })
        {
            for (int index = 0; index < frames.Count; index++)
            {
                EnemyProjectileFrameId programFrame = frames[index];
                if (IsOnNoSheet(programFrame))
                    continue;
                OamBuffer native = DrawReferenceProgramFrame(stock, programFrame, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, programFrame,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(InstalledStandsFor(native, extracted, stock, programFrame, kind),
                $"{family} frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
            }
        }
        IReadOnlyList<EnemyProjectileFrameId> eggFrames = TorizoProjectileAnimations.InstalledFrames(TorizoProjectileFamily.Egg);
        for (int index = 0; index < eggFrames.Count; index++)
        {
            EnemyProjectileFrameId egg = eggFrames[index];
            OamBuffer native = DrawReferenceProgramFrame(stock, egg, bus,
                RoomEnemyProjectileKind.GoldenTorizoEgg);
            OamBuffer extracted = DrawProgramFrame(stock, egg,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg);
            AssertTrue(InstalledStandsFor(native, extracted, stock, egg, RoomEnemyProjectileKind.GoldenTorizoEgg),
                $"Golden Torizo egg frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
        }
        var torizoFamilies = new ProjectileFrameFamily[]
        {
            TorizoFamily("drool", RoomEnemyProjectileKind.BombTorizoLowHealthDrool, TorizoProjectileFamily.Drool),
            TorizoFamily("explosive swipe", RoomEnemyProjectileKind.BombTorizoExplosiveSwipe, TorizoProjectileFamily.Swipe),
            TorizoFamily("sonic boom", RoomEnemyProjectileKind.BombTorizoSonicBoom, TorizoProjectileFamily.SonicBoom),
            TorizoFamily("landing dust", RoomEnemyProjectileKind.TorizoLandingDust, TorizoProjectileFamily.LandingDust),
            TorizoFamily("explosion", RoomEnemyProjectileKind.BombTorizoLowHealthExplosion, TorizoProjectileFamily.Explosion),
            TorizoFamily("Chozo orb", RoomEnemyProjectileKind.BombTorizoChozoOrb, TorizoProjectileFamily.ChozoOrb),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, IReadOnlyList<EnemyProjectileFrameId> frames) in torizoFamilies)
        {
            for (int index = 0; index < frames.Count; index++)
            {
                EnemyProjectileFrameId programFrame = frames[index];
                if (IsOnNoSheet(programFrame))
                    continue;
                OamBuffer native = DrawReferenceProgramFrame(stock, programFrame, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, programFrame,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(InstalledStandsFor(native, extracted, stock, programFrame, kind),
                $"Torizo {family} frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
            }
        }
        var genericDeathFamilies = new ProjectileFrameFamily[]
        {
            ShownFamily("enemy pickup", RoomEnemyProjectileKind.EnemyDeathPickup, PickupFrames),
            ShownFamily("enemy death", RoomEnemyProjectileKind.EnemyDeathExplosion,
                EnemyDeathAndDropAnimations.Segments.Frames.Except(PickupFrames)),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, IReadOnlyList<EnemyProjectileFrameId> frames) in genericDeathFamilies)
        {
            for (int index = 0; index < frames.Count; index++)
            {
                EnemyProjectileFrameId programFrame = frames[index];
                if (IsOnNoSheet(programFrame))
                    continue;
                OamBuffer native = DrawReferenceProgramFrame(stock, programFrame, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, programFrame,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(InstalledStandsFor(native, extracted, stock, programFrame, kind),
                $"{family} frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
            }
        }
        var environmentAndAttackFamilies = new ProjectileFrameFamily[]
        {
            ShownFamily("Ceres falling debris", RoomEnemyProjectileKind.CeresFallingDebris,
                CeresFallingDebrisVisualDefinitions.Frames.Select(debris => CeresFallingDebrisVisualDefinitions.ShownProjectileFrames[debris])),
            ShownFamily("save-station electricity", RoomEnemyProjectileKind.SaveStationElectricity, SaveStationElectricityAnimations.Segments.Frames),
            ShownFamily("gunship dust", RoomEnemyProjectileKind.GunshipLiftoffDustCloud, GunshipDustAnimations.Segments.Frames),
            ShownFamily("falling spark", RoomEnemyProjectileKind.FallingSpark, FallingSparkAnimations.Segments.Frames),
            ShownFamily("Magdollite lava", RoomEnemyProjectileKind.LavaThrownByMagdollite, MagdolliteLavaAnimations.Segments.Frames),
            ShownFamily("Chozo/Tourian dust", RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep, ChozoTourianDustAnimations.Segments.Frames),
            ShownFamily("eye-door sweat", RoomEnemyProjectileKind.EyeDoorSweat, EyeDoorSweatAnimations.Segments.Frames),
            ShownFamily("Ki Hunter acid spit", RoomEnemyProjectileKind.KiHunterAcidSpit, KiHunterAcidSpitAnimations.Segments.Frames),
            ShownFamily("Fune/Namihe fireball", RoomEnemyProjectileKind.FuneFireball, FuneNamiheFireballAnimations.Segments.Frames),
            ShownFamily("Dragon fireball", RoomEnemyProjectileKind.DragonFireball, DragonFireballAnimations.Segments.Frames),
            ShownFamily("Powamp spike", RoomEnemyProjectileKind.PowampSpike, PowampSpikeAnimations.Segments.Frames),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, IReadOnlyList<EnemyProjectileFrameId> frames) in environmentAndAttackFamilies)
        {
            for (int index = 0; index < frames.Count; index++)
            {
                EnemyProjectileFrameId programFrame = frames[index];
                if (IsOnNoSheet(programFrame))
                    continue;
                OamBuffer native = DrawReferenceProgramFrame(stock, programFrame, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, programFrame,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(InstalledStandsFor(native, extracted, stock, programFrame, kind),
                $"{family} frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
            }
        }
        var motherBrainAndStatueFamilies = new ProjectileFrameFamily[]
        {
            TorizoFamily("Bomb Torizo statue fragments", RoomEnemyProjectileKind.BombTorizoStatueBreaking, TorizoProjectileFamily.StatueFragment),
            ShownFamily("Mother Brain glass", RoomEnemyProjectileKind.MotherBrainGlassShard, MotherBrainGlassAnimations.ProjectileFrames),
            ShownFamily("Mother Brain hand beam", RoomEnemyProjectileKind.MotherBrainHandBeamCharging, MotherBrainHandBeamAnimations.ProjectileFrames),
            ShownFamily("Mother Brain top tube", RoomEnemyProjectileKind.MotherBrainTopRightTube, [.. MotherBrainTopTubeAnimations.Segments.Frames]),
            ShownFamily("Mother Brain turret", RoomEnemyProjectileKind.MotherBrainRoomTurret, MotherBrainTurretAnimations.ProjectileFrames),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, IReadOnlyList<EnemyProjectileFrameId> frames) in motherBrainAndStatueFamilies)
        {
            for (int index = 0; index < frames.Count; index++)
            {
                EnemyProjectileFrameId programFrame = frames[index];
                if (IsOnNoSheet(programFrame))
                    continue;
                OamBuffer native = DrawReferenceProgramFrame(stock, programFrame, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, programFrame, NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(InstalledStandsFor(native, extracted, stock, programFrame, kind),
                    $"{family} frame {index} draws stock OAM without visual ROM reads: records (x, y, tile, attributes) native {OamRecords(native)}, installed {OamRecords(extracted)}");
            }
        }
        // $86:D5F2 selects the second operand on this even frame; reflected X remains 128.
        OamBuffer nativeShard = DrawReferenceProgramFrame(stock, EnemyProjectileFrameId.NoobTubeShardDownOutwardMirrored, bus);
        OamBuffer installedShard = DrawNoobTubeShard(stock,
            NewEnemyProjectileVisualGuard(bus));
        AssertTrue(nativeShard.NextByteOffset > 0 &&
            nativeShard.ImageEquals(installedShard),
            "n00b-tube flicker instruction uses installed OAM without visual ROM reads");

        var installedSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0)) };
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
            installedArrival.DrawAsOam(installedOam, SuperMetroid.TestSupport.RepositoryInstallation.Verification.SpawnedObjectFixtures.Presentation(SuperMetroid.Core.World.RoomStates.CeresRoomStateDefinitions.CeresElevatorDefault), Position16<RoomPixels>.Zero);
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
                        installed.GetProgramFrame(frame.Frame).Span,
                        new Position16<ScreenPixels>(originX, originY), 0x0a00, 4,
                        clipVerticalWrap: true, originYIsOnScreen: true);
                    AssertTrue(nativeOam.NextByteOffset == installedOam.NextByteOffset &&
                        nativeOam.ImageEquals(installedOam),
                        $"installed shared frame $86:{frame.OperandAddress:X4} matches ROM OAM at {originX:X4},{originY:X4}");
                }
                if (EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames.Contains(frame))
                {
                    EnemyProjectileFrameId shown = frame.Frame;
                    if (SuperMetroid.AssetExtraction.Enemies.Artwork.SpawnedObjectFrameDefinitions.Common.Any(common => common.Objects.Projectiles.Contains(shown)))
                    {
                        OamBuffer sharedReference = DrawReferenceProgramFrame(artwork, shown, bus);
                        OamBuffer sharedDrawn = DrawSharedFrame(artwork, shown, NewEnemyProjectileVisualGuard(bus));
                        AssertTrue(InstalledStandsFor(sharedReference, sharedDrawn, artwork, shown, RoomEnemyProjectileKind.DustOrExplosion),
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
                            OamBuffer drawn = DrawSpawnerFrame(system, shown, graphics);
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
                         SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleSegment.Skree),
                         SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleSegment.Metaree),
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
        static OamBuffer DrawReferenceProgramFrame(ImportEnemyArtwork artwork, EnemyProjectileFrameId frame, ISnesAddressSpace source,
            RoomEnemyProjectileKind _ = RoomEnemyProjectileKind.DustOrExplosion)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork.Bosses);
            SpriteGraphics graphics = SpawnedObjectFixtures.LoadSetSpawning(enemies, frame, SpawnedGraphics);
            ushort word = graphics.Owner == SpawnedObjectOwner.Shared
                ? NativeSpriteGraphicsWords.GraphicsWord(SpawnedGraphics, SpawnedGraphics.FixedPaletteRow)
                : NativeSpriteGraphicsWords.GraphicsWord(graphics, enemies, graphics.PaletteRowIn(enemies));
            ushort pointer = ReadNativeEnemyProjectileProgramWord(source, EnemyProjectileProgramFrameSources.Operand(frame));
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
            Action<RoomEnemyPlace> spawn = place => SuperMetroid.Core.Enemies.Skree.SkreeMetareeParticles.SpawnBurst(enemies.Projectiles, place.HeldOccupant(), place,
                skree ? SkreeMetareeParticleSegment.Skree : SkreeMetareeParticleSegment.Metaree);
            // The burst's spawner binds through the system's own registry, which owns its species (#2010).
            RoomEnemyPlace enemy = enemies.Places.InUpdateOrder[0];
            enemy.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96));
            // The spawner is the enemy its set loads, in its sheet's standard colours, as a room's spawn would leave it;
            // its particles draw from its own spawned-object sheet (#1513).
            enemy.Bind(spawner, CompiledRoomEnemyDefinitions.Instance.ReadEnemyDefinition(spawner));
            enemy.Shading = PictureShading.Standard;
            spawn(enemy);
            foreach (EnemyProjectile particle in enemies.Projectiles.EnemyProjectiles
                         .Where(particle => particle.IsActive))
            {
                particle.FrameHold = WrappingCountdown.Start(1);
                enemies.Projectiles.AdvanceEnemyProjectileAnimations(particle, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), Position16<RoomPixels>.Zero, shortCounter: 0);
            }
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, new(0, 0));
            return oam;
        }

        static EnemyDefinitionId BurstSpawner(ushort program) =>
            program == SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleSegment.Skree)
                ? EnemyDefinitionId.Skree : EnemyDefinitionId.Metaree;

        // The particles draw from the sheets a set loading their spawner composes from this artwork.
        static RoomEnemySystem BurstSystem(ImportEnemyArtwork artwork, EnemyDefinitionId spawner)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork.Bosses);
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

        // A frame on no sheet, so no set loads it (#1513): the blank frame draws nothing (#2431). Any other frame a spawn
        // table lacks fails loudly in the set load, as a frame the game can draw must be on a sheet.
        static bool IsOnNoSheet(EnemyProjectileFrameId frame) => frame == EnemyProjectileFrameId.Blank;

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
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork.Bosses);
            SpawnedObjectFixtures.LoadSetDrawing(enemies, spawner);
            SpawnedObjectFixtures.ShowDrawnLooks(enemies, SuperMetroid.AssetExtraction.Enemies.FrameDrawings.Of(frame));
            return enemies;
        }

        // The frame drawn from the loaded set's sheets under the spawner's graphics.
        static OamBuffer DrawSpawnerFrame(RoomEnemySystem enemies, EnemyProjectileFrameId frame, SpriteGraphics graphics)
        {
            EnemyProjectile slot = enemies.Projectiles.Occupy(0, RoomEnemyProjectileKind.DustOrExplosion);
            slot.Position = slot.Position.WithWhole(new(128, 96));
            slot.Graphics = graphics;
            slot.Frame = frame;
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            return oam;
        }

        static OamBuffer DrawSharedFrame(ImportEnemyArtwork artwork,
            EnemyProjectileFrameId frame, ISnesAddressSpace source) =>
            DrawProgramFrame(artwork, frame, source,
                RoomEnemyProjectileKind.DustOrExplosion);

        static OamBuffer DrawProgramFrame(ImportEnemyArtwork artwork,
            EnemyProjectileFrameId frame, ISnesAddressSpace source, RoomEnemyProjectileKind kind)
        {
            // Typed programs (#641) present their frames by identity.
            return TypedProgramKinds.Contains(kind) || FamilyProjectileStarts.StartingAnimation(kind) is not null
                ? DrawPresentedFrame(artwork, frame, source, kind)
                : throw new InvalidOperationException($"Projectile {kind} plays no typed program.");
        }

        static OamBuffer DrawPresentedFrame(ImportEnemyArtwork artwork,
            EnemyProjectileFrameId frame, ISnesAddressSpace source, RoomEnemyProjectileKind kind)
        {
            RoomEnemySystem enemies = PresentedSystem(artwork, frame, kind);
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            return oam;
        }

        // A room whose first projectile shows the program frame, drawn from the sheets of a set spawning it.
        static RoomEnemySystem PresentedSystem(ImportEnemyArtwork artwork, EnemyProjectileFrameId frame, RoomEnemyProjectileKind kind)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork.Bosses);
            EnemyProjectile slot = enemies.Projectiles.Occupy(0, kind);
            slot.Position = slot.Position.WithWhole(new(128, 96));
            // As the typed runner does when it shows the program frame.
            slot.Frame = frame;
            // It draws from the sheets a set spawning it composes from this artwork, under the graphics that spawn gives it.
            slot.Graphics = SpawnedObjectFixtures.LoadSetSpawning(enemies, frame, SpawnedGraphics);
            SpawnedObjectFixtures.ShowDrawnLooks(enemies, SuperMetroid.AssetExtraction.Enemies.FrameDrawings.Of(frame));
            return enemies;
        }

        // Whether the installed draw of a program frame stands for the native OAM: the same image, or the OAM of another
        // composition the frame's shared picture stands for (#1513).
        static bool InstalledStandsFor(OamBuffer native, OamBuffer installed, ImportEnemyArtwork artwork, EnemyProjectileFrameId frame,
            RoomEnemyProjectileKind kind) =>
            native.NextByteOffset == installed.NextByteOffset &&
            (native.ImageEquals(installed) || SharedPictureStandsFor(PresentedSystem(artwork, frame, kind), native));

        static OamBuffer DrawNoobTubeShard(ImportEnemyArtwork artwork,
            ISnesAddressSpace source)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork.Bosses);
            EnemyProjectile slot = enemies.Projectiles.ProjectileInPlace(0);
            // The shard is spawned as the tube's spawner spawns it, bound to the tube pieces' behaviour that carries out its
            // cues (#1760). The first shard's animation opens on its flying loop, whose first beat is the reflecting flicker.
            FamilyProjectileStarts.Initialize(enemies, slot, RoomEnemyProjectileKind.NoobTubeShard, SpawnedGraphics);
            slot.State = new NoobTubeDebrisState(column: 128);
            slot.Position = slot.Position.WithWhole(new(128, 96));
            enemies.Projectiles.AdvanceEnemyProjectileAnimations(slot, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), Position16<RoomPixels>.Zero, shortCounter: 0);
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

    /// <summary>Torizo projectile family <paramref name="family"/>, drawn as <paramref name="kind"/>, with its installed frames.</summary>
    private static ProjectileFrameFamily TorizoFamily(string name, RoomEnemyProjectileKind kind, TorizoProjectileFamily family) =>
        new(name, kind, TorizoProjectileAnimations.InstalledFrames(family));

    /// <summary>The drops a dying enemy leaves: the frames of the pickup animations.</summary>
    private static readonly EnemyProjectileFrameId[] PickupFrames =
    [
        .. new[]
        {
            EnemyDeathAndDropSegment.SmallEnergy, EnemyDeathAndDropSegment.BigEnergy, EnemyDeathAndDropSegment.Missiles,
            EnemyDeathAndDropSegment.SuperMissiles, EnemyDeathAndDropSegment.PowerBombs,
        }.SelectMany(program => EnemyDeathAndDropAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyProjectileFrameId>().Distinct(),
    ];

    /// <summary>A projectile family drawn as <paramref name="kind"/>, whose frames are <paramref name="frames"/>.</summary>
    private static ProjectileFrameFamily ShownFamily(string name, RoomEnemyProjectileKind kind, IEnumerable<EnemyProjectileFrameId> frames) =>
        new(name, kind, [.. frames]);
}
