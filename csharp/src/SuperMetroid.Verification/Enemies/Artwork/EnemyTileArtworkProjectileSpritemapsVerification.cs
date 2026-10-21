using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Deaths.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Verification.Enemies.Skree;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.Core.Enemies.FallingSparks.Artwork;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
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
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.EyeDoor.Artwork;
using SuperMetroid.Core.Enemies.SaveStation.Artwork;
namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Compares installed enemy-projectile drawing compositions with native OAM and guards their runtime visual reads.</summary>
internal static class EnemyTileArtworkProjectileSpritemapsVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>The graphics index the drawing fixtures give every projectile: palette five, tile offset $0004.</summary>
    private const ushort FixtureGraphicsIndex = 0x0a04;

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
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyInstalledEnemyProjectileSpritemaps(
        ISnesAddressSpace bus, string directory, InstalledEnemyArtwork stock)
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
            bool onScreen = (SnesLittleEndian.HighByteInPlace(originY)) == 0;
            DrawImportedEnemyProjectileSpritemap(bus, nativeOam, pointer,
                new(originX, originY), graphicsIndex, onScreen);
            installedOam.AddEnemySpritemap(installed.Get(pointer).Span,
                new Position16<OamPixels>(originX, originY),
                unchecked((ushort)(SnesLittleEndian.HighByteInPlace(graphicsIndex))),
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
        ushort ceresOperand = EnemyProjectileFrames.ProgramFrameKey(RidleyFireballVisualDefinitions.ShownProjectileFrames[RidleyFireballVisualDefinitions.ProjectileFrames[0]]);
        OamBuffer nativeCeres = DrawReferenceProgramFrame(ceresOperand, bus,
            RoomEnemyProjectileKind.CeresRidleyFireball);
        OamBuffer installedCeres = DrawProgramFrame(stock, ceresOperand,
            NewEnemyProjectileVisualGuard(bus),
            RoomEnemyProjectileKind.CeresRidleyFireball);
        AssertTrue(nativeCeres.NextByteOffset > 0 &&
            nativeCeres.ImageEquals(installedCeres),
            "Ceres Ridley fireball uses installed OAM without visual ROM reads");
        for (int index = 0;
             index < AlcoonFireballFrameDefinitions.PoseCount;
             index++)
        {
            ushort operand = EnemyProjectileFrames.ProgramFrameKey(AlcoonFireballFrameDefinitions.ShownProjectileFrames[new(index)]);
            OamBuffer nativeAlcoon = DrawReferenceProgramFrame(operand, bus,
                RoomEnemyProjectileKind.AlcoonFireball);
            OamBuffer installedAlcoon = DrawProgramFrame(stock, operand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball);
            AssertTrue(nativeAlcoon.NextByteOffset > 0 &&
                nativeAlcoon.NextByteOffset == installedAlcoon.NextByteOffset &&
                nativeAlcoon.ImageEquals(installedAlcoon),
                $"Alcoon fireball frame {index} draws stock OAM without visual ROM reads");
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
                OamBuffer native = DrawReferenceProgramFrame(operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                    native.ImageEquals(extracted),
                $"{family} frame {index} draws stock OAM without visual ROM reads");
            }
        }
        for (int index = 0;
             index < TorizoFrameCount(TorizoProjectileFamily.Egg);
             index++)
        {
            ushort operand = TorizoFrameKey(TorizoProjectileFamily.Egg)(index);
            OamBuffer native = DrawReferenceProgramFrame(operand, bus,
                RoomEnemyProjectileKind.GoldenTorizoEgg);
            OamBuffer extracted = DrawProgramFrame(stock, operand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg);
            AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                native.ImageEquals(extracted),
                $"Golden Torizo egg frame {index} draws stock OAM without visual ROM reads");
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
                OamBuffer native = DrawReferenceProgramFrame(operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                    native.ImageEquals(extracted),
                $"Torizo {family} frame {index} draws stock OAM without visual ROM reads");
            }
        }
        var genericDeathFamilies = new TorizoFamily[]
        {
            new TorizoFamily("enemy pickup", RoomEnemyProjectileKind.EnemyDeathPickup,
                EnemyDeathProjectileFrameDefinitions.PoseCount - EnemyProjectilePresentationFrameDefinitions.DeathFrameCount,
                index => EnemyProjectileFrames.ProgramFrameKey(EnemyDeathProjectileFrameDefinitions.ShownProjectileFrames[new(EnemyProjectilePresentationFrameDefinitions.DeathFrameCount + index)])),
            new TorizoFamily("enemy death", RoomEnemyProjectileKind.EnemyDeathExplosion,
                EnemyProjectilePresentationFrameDefinitions.DeathFrameCount,
                index => EnemyProjectileFrames.ProgramFrameKey(EnemyDeathProjectileFrameDefinitions.ShownProjectileFrames[new(index)])),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in genericDeathFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                OamBuffer native = DrawReferenceProgramFrame(operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                    native.ImageEquals(extracted),
                $"{family} frame {index} draws stock OAM without visual ROM reads");
            }
        }
        var environmentAndAttackFamilies = new TorizoFamily[]
        {
            new TorizoFamily("Ceres falling debris", RoomEnemyProjectileKind.CeresFallingDebrisLight,
                CeresFallingDebrisVisualDefinitions.Frames.Count,
                index => EnemyProjectileFrames.ProgramFrameKey(CeresFallingDebrisVisualDefinitions.ShownProjectileFrames[CeresFallingDebrisVisualDefinitions.Frames[index]])),
            new TorizoFamily("save-station electricity", RoomEnemyProjectileKind.SaveStationElectricity,
                SaveStationElectricityFrameDefinitions.PoseCount,
                index => EnemyProjectileFrames.ProgramFrameKey(SaveStationElectricityFrameDefinitions.ShownProjectileFrames[new(index)])),
            new TorizoFamily("gunship dust", RoomEnemyProjectileKind.GunshipLiftoffDustCloud,
                GunshipDustVisualDefinitions.Frames.Count,
                index => EnemyProjectileFrames.ProgramFrameKey(GunshipDustVisualDefinitions.ShownProjectileFrames[GunshipDustVisualDefinitions.Frames[index]])),
            new TorizoFamily("falling spark", RoomEnemyProjectileKind.FallingSpark,
                FallingSparkFrameDefinitions.PoseCount,
                index => EnemyProjectileFrames.ProgramFrameKey(FallingSparkFrameDefinitions.ShownProjectileFrames[new(index)])),
            new TorizoFamily("Magdollite lava", RoomEnemyProjectileKind.LavaThrownByMagdollite,
                MagdolliteLavaFrameDefinitions.PoseCount,
                index => EnemyProjectileFrames.ProgramFrameKey(MagdolliteLavaFrameDefinitions.ShownProjectileFrames[new(index)])),
            new TorizoFamily("Chozo/Tourian dust", RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep,
                ChozoTourianDustFrameDefinitions.PoseCount,
                index => EnemyProjectileFrames.ProgramFrameKey(ChozoTourianDustFrameDefinitions.ShownProjectileFrames[new(index)])),
            new TorizoFamily("eye-door sweat", RoomEnemyProjectileKind.EyeDoorSweat,
                EyeDoorSweatFrameDefinitions.PoseCount,
                index => EnemyProjectileFrames.ProgramFrameKey(EyeDoorSweatFrameDefinitions.ShownProjectileFrames[new(index)])),
            new TorizoFamily("Ki Hunter acid spit", RoomEnemyProjectileKind.KiHunterAcidSpitLeft,
                KiHunterAcidSpitFrameDefinitions.PoseCount,
                index => EnemyProjectileFrames.ProgramFrameKey(KiHunterAcidSpitFrameDefinitions.ShownProjectileFrames[new(index)])),
            new TorizoFamily("Fune/Namihe fireball", RoomEnemyProjectileKind.FuneFireball,
                FuneNamiheFireballFrameToolingDefinitions.PoseCount,
                index => EnemyProjectileFrames.ProgramFrameKey(FuneNamiheFireballFrameDefinitions.ShownProjectileFrames[new(index)])),
            new TorizoFamily("Dragon fireball", RoomEnemyProjectileKind.DragonFireball,
                DragonFireballFrameDefinitions.PoseCount,
                index => EnemyProjectileFrames.ProgramFrameKey(DragonFireballFrameDefinitions.ShownProjectileFrames[new(index)])),
            new TorizoFamily("Powamp spike", RoomEnemyProjectileKind.PowampSpike,
                PowampSpikeFrameDefinitions.PoseCount,
                index => EnemyProjectileFrames.ProgramFrameKey(PowampSpikeFrameDefinitions.ShownProjectileFrames[new(index)])),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in environmentAndAttackFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                OamBuffer native = DrawReferenceProgramFrame(operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                    native.ImageEquals(extracted),
                $"{family} frame {index} draws stock OAM without visual ROM reads");
            }
        }
        var motherBrainAndStatueFamilies = new TorizoFamily[]
        {
            new TorizoFamily("Bomb Torizo statue fragments", RoomEnemyProjectileKind.BombTorizoStatueBreaking,
                TorizoFrameCount(TorizoProjectileFamily.StatueFragment),
                TorizoFrameKey(TorizoProjectileFamily.StatueFragment)),
            new TorizoFamily("Mother Brain glass", RoomEnemyProjectileKind.MotherBrainGlassShard,
                MotherBrainGlassVisualDefinitions.ProjectileFrameCount,
                index => EnemyProjectileFrames.ProgramFrameKey(MotherBrainGlassVisualDefinitions.ShownProjectileFrames[MotherBrainGlassVisualDefinitions.ProjectileFrames[index]])),
            new TorizoFamily("Mother Brain hand beam", RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
                MotherBrainHandBeamVisualDefinitions.ProjectileFrameCount,
                index => EnemyProjectileFrames.ProgramFrameKey(MotherBrainHandBeamVisualDefinitions.ShownProjectileFrames[MotherBrainHandBeamVisualDefinitions.ProjectileFrames[index]])),
            new TorizoFamily("Mother Brain top tube", RoomEnemyProjectileKind.MotherBrainTopRightTube,
                MotherBrainTopTubeInstructionProgramDefinitionsTooling.PresentationWordCount,
                MotherBrainTopTubeInstructionProgramDefinitionsTooling.PresentationWordAddress),
            new TorizoFamily("Mother Brain turret", RoomEnemyProjectileKind.MotherBrainRoomTurret,
                MotherBrainTurretVisualDefinitions.ProjectileFrameCount,
                index => EnemyProjectileFrames.ProgramFrameKey(MotherBrainTurretVisualDefinitions.ShownProjectileFrames[MotherBrainTurretVisualDefinitions.ProjectileFrames[index]])),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in motherBrainAndStatueFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                OamBuffer native = DrawReferenceProgramFrame(operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand, NewEnemyProjectileVisualGuard(bus), kind);
                AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                    native.ImageEquals(extracted),
                    $"{family} frame {index} draws stock OAM without visual ROM reads");
            }
        }
        // $86:D5F2 selects the second operand on this even frame; reflected X remains 128.
        OamBuffer nativeShard = DrawReferenceProgramFrame(0xd485, bus);
        OamBuffer installedShard = DrawNoobTubeShard(stock,
            NewEnemyProjectileVisualGuard(bus));
        AssertTrue(nativeShard.NextByteOffset > 0 &&
            nativeShard.ImageEquals(installedShard),
            "n00b-tube flicker instruction uses installed OAM without visual ROM reads");

        var installedSamus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0)) };
        var installedArrival = new CeresElevatorArrivalState(
            installedSamus, installed);
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
            installedArrival.DrawAsOam(installedOam, Position16<RoomPixels>.Zero);
            AssertTrue(nativeOam.ImageEquals(installedOam),
                $"Ceres elevator arrival frame {frame} draws from installed $8D compositions");
        }

        string overrides = Path.Combine(directory, "eproj-overrides");
        Directory.CreateDirectory(overrides);
        EnemyProjectileSpritemapDocument document =
            RequiredJson.Deserialize<EnemyProjectileSpritemapDocument>(
                File.ReadAllBytes(Path.Combine(directory,
                    EnemyProjectileSpritemapDefinitions.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        SpriteVisualPart[] pad = document.Frames["ceres_elevator_pad_0"];
        pad[0] = pad[0] with { OffsetX = pad[0].OffsetX + 1 };
        SpriteVisualPart[] skree = document.Frames["skree_debris"];
        skree[0] = skree[0] with { OffsetX = skree[0].OffsetX + 1 };
        EnemyProjectilePresentationFrameDefinition editableProgramFrame =
            EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames[0];
        SpriteVisualPart[] shared = (document.ProgramFrames ?? throw new InvalidOperationException("Verification requires document.ProgramFrames."))[editableProgramFrame.Name];
        shared[0] = shared[0] with { OffsetX = shared[0].OffsetX + 1 };
        string ceresFrameName = EnemyProjectilePresentationFrameSets.All
            .Single(frame => frame.OperandAddress == ceresOperand).Name;
        SpriteVisualPart[] ceres = document.ProgramFrames[ceresFrameName];
        ceres[0] = ceres[0] with { OffsetX = ceres[0].OffsetX + 1 };
        ushort alcoonOperand = EnemyProjectileFrames.ProgramFrameKey(AlcoonFireballFrameDefinitions.ShownProjectileFrames[new(0)]);
        string alcoonFrameName = EnemyProjectilePresentationFrameSets.All
            .Single(frame => frame.OperandAddress == alcoonOperand).Name;
        SpriteVisualPart[] alcoon = document.ProgramFrames[alcoonFrameName];
        alcoon[0] = alcoon[0] with { OffsetX = alcoon[0].OffsetX + 1 };
        ushort goldenMissileOperand = TorizoFrameKey(TorizoProjectileFamily.SuperMissile)(0);
        string goldenMissileFrameName = EnemyProjectilePresentationFrameSets.All
            .Single(frame => frame.OperandAddress == goldenMissileOperand).Name;
        SpriteVisualPart[] goldenMissile = document.ProgramFrames[goldenMissileFrameName];
        goldenMissile[0] = goldenMissile[0] with { OffsetX = goldenMissile[0].OffsetX + 1 };
        ushort goldenEyeOperand = TorizoFrameKey(TorizoProjectileFamily.EyeBeam)(0);
        string goldenEyeFrameName = EnemyProjectilePresentationFrameSets.All
            .Single(frame => frame.OperandAddress == goldenEyeOperand).Name;
        SpriteVisualPart[] goldenEye = document.ProgramFrames[goldenEyeFrameName];
        goldenEye[0] = goldenEye[0] with { OffsetX = goldenEye[0].OffsetX + 1 };
        ushort goldenEggOperand = TorizoFrameKey(TorizoProjectileFamily.Egg)(0);
        string goldenEggFrameName = EnemyProjectilePresentationFrameSets.All
            .Single(frame => frame.OperandAddress == goldenEggOperand).Name;
        SpriteVisualPart[] goldenEgg = document.ProgramFrames[goldenEggFrameName];
        goldenEgg[0] = goldenEgg[0] with { OffsetX = goldenEgg[0].OffsetX + 1 };
        var torizoEditableOperands = new Dictionary<string, ushort>(StringComparer.Ordinal);
        foreach ((string family, RoomEnemyProjectileKind _, int count,
                     Func<int, ushort> addressAt) in
                 torizoFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                string frameName = EnemyProjectilePresentationFrameSets.All
                    .Single(frame => frame.OperandAddress == operand).Name;
                SpriteVisualPart[] frame = document.ProgramFrames[frameName];
                if (frame.Length == 0)
                    continue;
                frame[0] = frame[0] with { OffsetX = frame[0].OffsetX + 1 };
                torizoEditableOperands.Add(family, operand);
                break;
            }
            AssertTrue(torizoEditableOperands.ContainsKey(family),
                $"Torizo {family} has an editable OAM part in at least one frame");
        }
        var genericDeathEditableOperands = new Dictionary<string, ushort>(StringComparer.Ordinal);
        foreach ((string family, RoomEnemyProjectileKind _, int count,
                     Func<int, ushort> addressAt) in genericDeathFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                string frameName = EnemyProjectilePresentationFrameSets.All
                    .Single(frame => frame.OperandAddress == operand).Name;
                SpriteVisualPart[] frame = document.ProgramFrames[frameName];
                if (frame.Length == 0)
                    continue;
                frame[0] = frame[0] with { OffsetX = frame[0].OffsetX + 1 };
                genericDeathEditableOperands.Add(family, operand);
                break;
            }
            AssertTrue(genericDeathEditableOperands.ContainsKey(family),
                $"{family} has an editable OAM part in at least one frame");
        }
        var environmentEditableOperands = new Dictionary<string, ushort>(StringComparer.Ordinal);
        foreach ((string family, RoomEnemyProjectileKind _, int count,
                     Func<int, ushort> addressAt) in environmentAndAttackFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                string frameName = EnemyProjectilePresentationFrameSets.All
                    .Single(frame => frame.OperandAddress == operand).Name;
                SpriteVisualPart[] frame = document.ProgramFrames[frameName];
                if (frame.Length == 0)
                    continue;
                frame[0] = frame[0] with { OffsetX = frame[0].OffsetX + 1 };
                environmentEditableOperands.Add(family, operand);
                break;
            }
            AssertTrue(environmentEditableOperands.ContainsKey(family),
                $"{family} has an editable OAM part in at least one frame");
        }
        var motherBrainEditableOperands = new Dictionary<string, ushort>(StringComparer.Ordinal);
        foreach ((string family, RoomEnemyProjectileKind _, int count,
                     Func<int, ushort> addressAt) in motherBrainAndStatueFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                string frameName = EnemyProjectilePresentationFrameSets.All
                    .Single(frame => frame.OperandAddress == operand).Name;
                SpriteVisualPart[] frame = document.ProgramFrames[frameName];
                if (frame.Length == 0)
                    continue;
                frame[0] = frame[0] with { OffsetX = frame[0].OffsetX + 1 };
                motherBrainEditableOperands.Add(family, operand);
                break;
            }
            AssertTrue(motherBrainEditableOperands.ContainsKey(family),
                $"{family} has an editable OAM part in at least one frame");
        }
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName),
            EnemyProjectileSpritemapCatalog.Write(document));
        InstalledEnemyArtwork editedArt = EnemyTileArtworkFiles.Load(directory, overrides);
        EnemyProjectileSpritemapCatalog edited = (editedArt.Enemies.ProjectileSpritemaps ?? throw new InvalidOperationException("Verification requires editedArt.ProjectileSpritemaps."));
        var originalOam = new OamBuffer();
        var editedOam = new OamBuffer();
        originalOam.BeginFrame();
        editedOam.BeginFrame();
        originalOam.AddEnemySpritemap(installed.Get(0xb1ba).Span,
            new Position16<OamPixels>(0x0080, 0x0004), 0, 0, clipVerticalWrap: true);
        editedOam.AddEnemySpritemap(edited.Get(0xb1ba).Span,
            new Position16<OamPixels>(0x0080, 0x0004), 0, 0, clipVerticalWrap: true);
        AssertEqual((byte)(originalOam.LowTable[0] + 1), editedOam.LowTable[0],
            "enemy-projectile composition override moves the live Ceres pad OAM part");
        OamBuffer stockBurst = DrawBurst(stock,
            SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleProgram.Skree), bus);
        OamBuffer editedBurst = DrawBurst(editedArt,
            SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleProgram.Skree), bus);
        AssertTrue(!stockBurst.LowTable.SequenceEqual(editedBurst.LowTable),
            "edited Skree debris composition changes production burst OAM");
        AssertTrue(!DrawSharedFrame(stock, editableProgramFrame.OperandAddress, bus)
                .LowTable.SequenceEqual(DrawSharedFrame(editedArt,
                    editableProgramFrame.OperandAddress,
                    NewEnemyProjectileVisualGuard(bus)).LowTable),
            "edited shared projectile frame changes production OAM without a ROM visual read");
        AssertTrue(!installedCeres.LowTable.SequenceEqual(
                DrawProgramFrame(editedArt, ceresOperand,
                    NewEnemyProjectileVisualGuard(bus),
                    RoomEnemyProjectileKind.CeresRidleyFireball).LowTable),
            "edited Ceres Ridley frame changes production OAM without a ROM visual read");
        AssertTrue(!DrawProgramFrame(stock, alcoonOperand,
                    NewEnemyProjectileVisualGuard(bus),
                    RoomEnemyProjectileKind.AlcoonFireball).LowTable
                .SequenceEqual(DrawProgramFrame(editedArt, alcoonOperand,
                    NewEnemyProjectileVisualGuard(bus),
                    RoomEnemyProjectileKind.AlcoonFireball).LowTable),
            "edited Alcoon fireball frame changes production OAM without a ROM visual read");
        foreach ((string family, ushort operand, RoomEnemyProjectileKind kind) in new[]
                 {
                     new EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase("Super Missile", goldenMissileOperand,
                         RoomEnemyProjectileKind.GoldenTorizoSuperMissile),
                     new EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase("eye beam", goldenEyeOperand,
                         RoomEnemyProjectileKind.GoldenTorizoEyeBeam),
                 })
        {
            AssertTrue(!DrawProgramFrame(stock, operand,
                        NewEnemyProjectileVisualGuard(bus), kind).LowTable
                    .SequenceEqual(DrawProgramFrame(editedArt, operand,
                        NewEnemyProjectileVisualGuard(bus), kind).LowTable),
                $"edited Golden Torizo {family} frame changes production OAM");
        }
        AssertTrue(!DrawProgramFrame(stock, goldenEggOperand,
                    NewEnemyProjectileVisualGuard(bus),
                    RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable
                .SequenceEqual(DrawProgramFrame(editedArt, goldenEggOperand,
                    NewEnemyProjectileVisualGuard(bus),
                    RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable),
            "edited Golden Torizo egg frame changes production OAM");
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in torizoFamilies)
        {
            ushort operand = torizoEditableOperands[family];
            AssertTrue(!DrawProgramFrame(stock, operand,
                        NewEnemyProjectileVisualGuard(bus), kind).LowTable
                    .SequenceEqual(DrawProgramFrame(editedArt, operand,
                        NewEnemyProjectileVisualGuard(bus), kind).LowTable),
                $"edited Torizo {family} frame changes production OAM");
        }
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in genericDeathFamilies)
        {
            ushort operand = genericDeathEditableOperands[family];
            AssertTrue(!DrawProgramFrame(stock, operand,
                        NewEnemyProjectileVisualGuard(bus), kind).LowTable
                    .SequenceEqual(DrawProgramFrame(editedArt, operand,
                        NewEnemyProjectileVisualGuard(bus), kind).LowTable),
                $"edited {family} frame changes production OAM");
        }
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in environmentAndAttackFamilies)
        {
            ushort operand = environmentEditableOperands[family];
            AssertTrue(!DrawProgramFrame(stock, operand,
                        NewEnemyProjectileVisualGuard(bus), kind).LowTable
                    .SequenceEqual(DrawProgramFrame(editedArt, operand,
                        NewEnemyProjectileVisualGuard(bus), kind).LowTable),
                $"edited {family} frame changes production OAM");
        }
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in motherBrainAndStatueFamilies)
        {
            ushort operand = motherBrainEditableOperands[family];
            AssertTrue(!DrawProgramFrame(stock, operand,
                        NewEnemyProjectileVisualGuard(bus), kind).LowTable
                    .SequenceEqual(DrawProgramFrame(editedArt, operand,
                        NewEnemyProjectileVisualGuard(bus), kind).LowTable),
                $"edited {family} frame changes production OAM");
        }
        var incompleteCurrent = document.Frames
            .Where(entry => entry.Key != "skree_debris")
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        byte[] invalidCurrentJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.Current,
                Frames = incompleteCurrent,
                ProgramFrames = document.ProgramFrames,
            }, CamelCaseJson);
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileSpritemapCatalog.Load(new MemoryStream(invalidCurrentJson),
                installed),
            "current projectile compositions cannot silently omit Skree debris");
        var incompleteProgramFrames = document.ProgramFrames
            .Where(entry => entry.Key != editableProgramFrame.Name)
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        byte[] invalidProgramJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.Current,
                Frames = document.Frames,
                ProgramFrames = incompleteProgramFrames,
            }, CamelCaseJson);
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileSpritemapCatalog.Load(new MemoryStream(invalidProgramJson),
                installed),
            "current projectile compositions cannot silently omit a shared program frame");

        var legacyFrames = document.Frames
            .Where(entry => entry.Key.StartsWith("ceres_elevator_", StringComparison.Ordinal))
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        byte[] legacyJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = 1,
                Frames = legacyFrames,
            }, CamelCaseJson);
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileSpritemapCatalog.Load(new MemoryStream(legacyJson)),
            "a partial version-one composition cannot stand alone as stock content");
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), legacyJson);
        InstalledEnemyArtwork migrated = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawBurst(migrated,
                SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleProgram.Skree), bus)
            .LowTable.SequenceEqual(stockBurst.LowTable),
            "version-one Ceres override inherits stock Skree debris composition");
        var migratedPad = new OamBuffer();
        migratedPad.BeginFrame();
        migratedPad.AddEnemySpritemap((migrated.Enemies.ProjectileSpritemaps ?? throw new InvalidOperationException("Verification requires migrated.ProjectileSpritemaps.")).Get(0xb1ba).Span,
            new Position16<OamPixels>(0x0080, 0x0004), 0, 0, clipVerticalWrap: true);
        AssertEqual(editedOam.LowTable[0], migratedPad.LowTable[0],
            "version-one Ceres composition edit survives the version-three catalog upgrade");
        byte[] versionTwoJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = 2,
                Frames = document.Frames,
            }, CamelCaseJson);
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionTwoJson);
        InstalledEnemyArtwork migratedV2 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawBurst(migratedV2,
                SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleProgram.Skree), bus)
            .LowTable.SequenceEqual(editedBurst.LowTable),
            "version-two edits retain the Skree debris frame");
        AssertTrue(DrawSharedFrame(stock, editableProgramFrame.OperandAddress, bus)
                .LowTable.SequenceEqual(DrawSharedFrame(migratedV2,
                    editableProgramFrame.OperandAddress,
                    NewEnemyProjectileVisualGuard(bus)).LowTable),
            "version-two edits inherit stock shared-program frames");
        var versionThreeFrames = EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames
            .ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames[frame.Name], StringComparer.Ordinal);
        byte[] versionThreeJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = 3,
                Frames = document.Frames,
                ProgramFrames = versionThreeFrames,
            }, CamelCaseJson);
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionThreeJson);
        InstalledEnemyArtwork migratedV3 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawSharedFrame(migratedV3, editableProgramFrame.OperandAddress,
                NewEnemyProjectileVisualGuard(bus)).LowTable
            .SequenceEqual(DrawSharedFrame(editedArt, editableProgramFrame.OperandAddress,
                NewEnemyProjectileVisualGuard(bus)).LowTable),
            "version-three shared-program edits survive the expanded projectile catalog");
        AssertTrue(DrawProgramFrame(migratedV3, ceresOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.CeresRidleyFireball).LowTable
            .SequenceEqual(installedCeres.LowTable),
            "version-three overrides inherit newly extracted Ceres Ridley frames");
        var alcoonOperands = Enumerable.Range(0,
                AlcoonFireballFrameDefinitions.PoseCount)
            .Select(index => EnemyProjectileFrames.ProgramFrameKey(AlcoonFireballFrameDefinitions.ShownProjectileFrames[new(index)]))
            .ToHashSet();
        var versionFourFrames = EnemyProjectilePresentationFrameSets.PreGoldenTorizo
            
            .WhereToArray(frame => !alcoonOperands.Contains(frame.OperandAddress))
            .ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames[frame.Name], StringComparer.Ordinal);
        byte[] versionFourJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.PreAlcoon,
                Frames = document.Frames,
                ProgramFrames = versionFourFrames,
            }, CamelCaseJson);
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionFourJson);
        InstalledEnemyArtwork migratedV4 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawProgramFrame(migratedV4, ceresOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.CeresRidleyFireball).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, ceresOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.CeresRidleyFireball).LowTable),
            "version-four overrides retain their edited projectile frames");
        AssertTrue(DrawProgramFrame(migratedV4, alcoonOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball).LowTable
            .SequenceEqual(DrawProgramFrame(stock, alcoonOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball).LowTable),
            "version-four overrides inherit newly extracted Alcoon fireball frames");
        var versionFiveFrames = EnemyProjectilePresentationFrameSets.PreGoldenTorizo
            .ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames[frame.Name], StringComparer.Ordinal);
        byte[] versionFiveJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.PreGoldenTorizo,
                Frames = document.Frames,
                ProgramFrames = versionFiveFrames,
            }, CamelCaseJson);
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionFiveJson);
        InstalledEnemyArtwork migratedV5 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawProgramFrame(migratedV5, alcoonOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, alcoonOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball).LowTable),
            "version-five overrides retain their edited Alcoon fireball frames");
        foreach ((string family, ushort operand, RoomEnemyProjectileKind kind) in new[]
                 {
                     new EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase("Super Missile", goldenMissileOperand,
                         RoomEnemyProjectileKind.GoldenTorizoSuperMissile),
                     new EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase("eye beam", goldenEyeOperand,
                         RoomEnemyProjectileKind.GoldenTorizoEyeBeam),
                 })
        {
            AssertTrue(DrawProgramFrame(migratedV5, operand,
                    NewEnemyProjectileVisualGuard(bus), kind).LowTable
                .SequenceEqual(DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind).LowTable),
                $"version-five overrides inherit extracted Golden Torizo {family} frames");
        }
        var versionSixFrames = EnemyProjectilePresentationFrameSets.PreGoldenTorizoEgg
            .ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames[frame.Name], StringComparer.Ordinal);
        byte[] versionSixJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.PreGoldenTorizoEgg,
                Frames = document.Frames,
                ProgramFrames = versionSixFrames,
            }, CamelCaseJson);
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionSixJson);
        InstalledEnemyArtwork migratedV6 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawProgramFrame(migratedV6, goldenMissileOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoSuperMissile).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, goldenMissileOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoSuperMissile).LowTable),
            "version-six overrides retain their edited Golden Torizo missile frame");
        AssertTrue(DrawProgramFrame(migratedV6, goldenEggOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable
            .SequenceEqual(DrawProgramFrame(stock, goldenEggOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable),
            "version-six overrides inherit extracted Golden Torizo egg frames");
        var versionSevenFrames = EnemyProjectilePresentationFrameSets.PreTorizoEffects
            .ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames[frame.Name], StringComparer.Ordinal);
        byte[] versionSevenJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.PreTorizoEffects,
                Frames = document.Frames,
                ProgramFrames = versionSevenFrames,
            }, CamelCaseJson);
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionSevenJson);
        InstalledEnemyArtwork migratedV7 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawProgramFrame(migratedV7, goldenEggOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, goldenEggOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable),
            "version-seven overrides retain their edited Golden Torizo egg frame");
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in torizoFamilies)
        {
            ushort operand = torizoEditableOperands[family];
            AssertTrue(DrawProgramFrame(migratedV7, operand,
                    NewEnemyProjectileVisualGuard(bus), kind).LowTable
                .SequenceEqual(DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind).LowTable),
                $"version-seven overrides inherit Torizo {family} frames");
        }
        var versionEightFrames = EnemyProjectilePresentationFrameSets.PreGenericEnemyDeath
            .ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames[frame.Name], StringComparer.Ordinal);
        byte[] versionEightJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.PreGenericEnemyDeath,
                Frames = document.Frames,
                ProgramFrames = versionEightFrames,
            }, CamelCaseJson);
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionEightJson);
        InstalledEnemyArtwork migratedV8 = EnemyTileArtworkFiles.Load(directory, overrides);
        ushort editedTorizoOperand = torizoEditableOperands["Chozo orb"];
        AssertTrue(DrawProgramFrame(migratedV8, editedTorizoOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.BombTorizoChozoOrb).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, editedTorizoOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.BombTorizoChozoOrb).LowTable),
            "version-eight overrides retain their edited Torizo Chozo-orb frame");
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in genericDeathFamilies)
        {
            ushort operand = genericDeathEditableOperands[family];
            AssertTrue(DrawProgramFrame(migratedV8, operand,
                    NewEnemyProjectileVisualGuard(bus), kind).LowTable
                .SequenceEqual(DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind).LowTable),
                $"version-eight overrides inherit {family} frames");
        }
        var versionNineFrames = EnemyProjectilePresentationFrameSets.PreEnvironmentAndAttack
            .ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames[frame.Name], StringComparer.Ordinal);
        byte[] versionNineJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.PreEnvironmentAndAttack,
                Frames = document.Frames,
                ProgramFrames = versionNineFrames,
            }, CamelCaseJson);
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionNineJson);
        InstalledEnemyArtwork migratedV9 = EnemyTileArtworkFiles.Load(directory, overrides);
        ushort editedDeathOperand = genericDeathEditableOperands["enemy death"];
        AssertTrue(DrawProgramFrame(migratedV9, editedDeathOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.EnemyDeathExplosion).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, editedDeathOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.EnemyDeathExplosion).LowTable),
            "version-nine overrides retain their edited generic enemy-death frame");
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in environmentAndAttackFamilies)
        {
            ushort operand = environmentEditableOperands[family];
            AssertTrue(DrawProgramFrame(migratedV9, operand,
                    NewEnemyProjectileVisualGuard(bus), kind).LowTable
                .SequenceEqual(DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind).LowTable),
                $"version-nine overrides inherit {family} frames");
        }
        var versionTenFrames = EnemyProjectilePresentationFrameSets.PreMotherBrainAndStatue
            .ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames[frame.Name], StringComparer.Ordinal);
        byte[] versionTenJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.PreMotherBrainAndStatue,
                Frames = document.Frames,
                ProgramFrames = versionTenFrames,
            }, CamelCaseJson);
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionTenJson);
        InstalledEnemyArtwork migratedV10 = EnemyTileArtworkFiles.Load(directory, overrides);
        ushort editedEnvironmentOperand = environmentEditableOperands["falling spark"];
        AssertTrue(DrawProgramFrame(migratedV10, editedEnvironmentOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.FallingSpark).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, editedEnvironmentOperand,
                NewEnemyProjectileVisualGuard(bus),
                RoomEnemyProjectileKind.FallingSpark).LowTable),
            "version-ten overrides retain their edited falling-spark frame");
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in motherBrainAndStatueFamilies)
        {
            ushort operand = motherBrainEditableOperands[family];
            AssertTrue(DrawProgramFrame(migratedV10, operand,
                    NewEnemyProjectileVisualGuard(bus), kind).LowTable
                .SequenceEqual(DrawProgramFrame(stock, operand,
                    NewEnemyProjectileVisualGuard(bus), kind).LowTable),
                $"version-ten overrides inherit {family} frames");
        }
        Console.WriteLine($"  Enemy projectile visuals: " +
            $"{EnemyProjectilePresentationFrameSets.All.Length} catalogued timed/flicker frames " +
            "match native OAM; installed draws, editable frames, and v1-v10 migrations pass.");

        void VerifySharedProgramVisuals(InstalledEnemyArtwork artwork)
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
                        new Position16<OamPixels>(originX, originY), 0x0a00, 4,
                        clipVerticalWrap: true, originYIsOnScreen: true);
                    AssertTrue(nativeOam.NextByteOffset == installedOam.NextByteOffset &&
                        nativeOam.ImageEquals(installedOam),
                        $"installed shared frame $86:{frame.OperandAddress:X4} matches ROM OAM at {originX:X4},{originY:X4}");
                }
                if (EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames.Any(manifest => manifest.OperandAddress == frame.OperandAddress))
                {
                    AssertTrue(DrawReferenceProgramFrame(frame.OperandAddress, bus)
                            .LowTable.SequenceEqual(DrawSharedFrame(artwork,
                                frame.OperandAddress,
                                NewEnemyProjectileVisualGuard(bus)).LowTable),
                        $"shared frame $86:{frame.OperandAddress:X4} runs without visual ROM reads");
                }
            }
        }

        void VerifyBurstVisuals(InstalledEnemyArtwork artwork)
        {
            foreach (ushort program in new[]
                     {
                         SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleProgram.Skree),
                         SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleProgram.Metaree),
                     })
            {
                OamBuffer native = DrawReferenceBurst(program, bus);
                OamBuffer installedBurst = DrawBurst(artwork, program,
                    NewEnemyProjectileVisualGuard(bus));
                AssertTrue(native.NextByteOffset > 0 &&
                    native.NextByteOffset == installedBurst.NextByteOffset &&
                    native.ImageEquals(installedBurst),
                    $"installed Skree/Metaree burst ${program:X4} draws stock OAM without visual ROM reads");
            }
        }

        // Expected OAM comes from the cartridge's visual operand and bank-$8D
        // composition, not the installed catalog or production draw callback.
        static OamBuffer DrawReferenceProgramFrame(ushort operand, ISnesAddressSpace source,
            RoomEnemyProjectileKind _ = RoomEnemyProjectileKind.MiscDustExplosion)
        {
            ushort pointer = ReadNativeEnemyProjectileProgramWord(source, operand);
            var oam = new OamBuffer();
            oam.BeginFrame();
            DrawImportedEnemyProjectileSpritemap(source, oam, pointer, new(128, 96),
                FixtureGraphicsIndex, originYIsOnScreen: true);
            return oam;
        }

        static OamBuffer DrawReferenceBurst(ushort program, ISnesAddressSpace source)
        {
            ushort pointer = ReadNativeEnemyProjectileProgramWord(source, unchecked((ushort)(program + 2)));
            var oam = new OamBuffer();
            oam.BeginFrame();
            // Native Skree/Metaree spawn order: two right particles then two left.
            // Initialization offsets X by six; no motion tick precedes this draw.
            foreach (ushort x in new ushort[] { 134, 134, 122, 122 })
                DrawImportedEnemyProjectileSpritemap(source, oam, pointer, new(x, 96),
                    0x0e00, originYIsOnScreen: true);
            return oam;
        }

        static OamBuffer DrawBurst(InstalledEnemyArtwork artwork, ushort program,
            ISnesAddressSpace source)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
            Action<RoomEnemySlot> spawn = program == SkreeMetareeParticleNativeListAddresses.Entry(SkreeMetareeParticleProgram.Skree)
                ? enemies.Projectiles.SpawnSkreeParticleBurst : enemies.Projectiles.SpawnMetareeParticleBurst;
            var enemy = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)),
                VramTilesIndex = 0x0200,
                PaletteIndex = 0x0c00,
            };
            spawn(enemy);
            foreach (RoomEnemyProjectileSlot particle in enemies.Projectiles.EnemyProjectiles
                         .Where(particle => particle.IsActive))
            {
                particle.InstructionTimer = WrappingCountdown.Start(1);
                enemies.Projectiles.ProcessEnemyProjectileInstructions(particle, new SamusState(), Position16<RoomPixels>.Zero);
            }
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, new(0, 0));
            return oam;
        }

        static OamBuffer DrawSharedFrame(InstalledEnemyArtwork artwork,
            ushort operand, ISnesAddressSpace source) =>
            DrawProgramFrame(artwork, operand, source,
                RoomEnemyProjectileKind.MiscDustExplosion);

        static OamBuffer DrawProgramFrame(InstalledEnemyArtwork artwork,
            ushort operand, ISnesAddressSpace source, RoomEnemyProjectileKind kind)
        {
            // Typed programs (#641) present their frames by key.
            return TypedProgramKinds.Contains(kind) || EnemyProjectileTypedStarts.Initial(kind) is not null ||
                TourianStatueProjectileStarts.ForKind(kind) is not null || MotherBrainProjectileStarts.ForKind(kind) is not null
                ? DrawPresentedFrame(artwork, operand, source, kind)
                : throw new InvalidOperationException($"Projectile {kind} plays no typed program.");
        }

        static OamBuffer DrawPresentedFrame(InstalledEnemyArtwork artwork,
            ushort presentationKey, ISnesAddressSpace source, RoomEnemyProjectileKind kind)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
            RoomEnemyProjectileSlot slot = enemies.Projectiles.EnemyProjectiles[0];
            slot.Kind = kind;
            slot.Position = slot.Position.WithWhole(new(128, 96));
            slot.GraphicsIndex = FixtureGraphicsIndex;
            // As the typed runner does when it shows the program frame the key installs.
            slot.Frame = EnemyProjectileSlotFrameAccess.ProgramFrameAt(presentationKey);
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            return oam;
        }

        static OamBuffer DrawNoobTubeShard(InstalledEnemyArtwork artwork,
            ISnesAddressSpace source)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
            RoomEnemyProjectileSlot slot = enemies.Projectiles.EnemyProjectiles[0];
            slot.Kind = RoomEnemyProjectileKind.NoobTubeShard;
            slot.Variable1 = 128;
            slot.Position = slot.Position.WithWhole(new(128, 96));
            slot.GraphicsIndex = FixtureGraphicsIndex;
            // The first shard's flying loop opens with its reflecting flicker.
            slot.StartAnimation(new NoobTubeProjectileAnimation(NoobTubeProjectileProgram.Shard0Loop));
            enemies.Projectiles.ProcessEnemyProjectileInstructions(slot, new SamusState(), Position16<RoomPixels>.Zero);
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
                    (EnemyProjectilePresentationFrameSetsTooling.Contains(low) ||
                     EnemyProjectilePresentationFrameSetsTooling.Contains(
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
        index => EnemyProjectileFrames.ProgramFrameKey(TorizoProjectileVisualDefinitions.ShownProjectileFrames[TorizoProjectileVisualDefinitions.Frames(family)[index]]);
}
