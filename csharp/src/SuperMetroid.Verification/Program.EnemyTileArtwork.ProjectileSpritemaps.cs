using System.Text.Json;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledEnemyProjectileSpritemaps(
        ISnesAddressSpace bus, string directory, EnemyTileArtworkCatalog stock)
    {
        EnemyProjectileSpritemapCatalog installed = stock.ProjectileSpritemaps
            ?? throw new InvalidDataException("Installed enemy-projectile compositions are missing.");
        foreach ((ushort pointer, _) in EnemyProjectileSpritemapDefinitions.Frames)
        foreach ((ushort originX, ushort originY, ushort graphicsIndex) in new[]
                 {
                     ((ushort)0x0080, (ushort)0x0004, (ushort)0x0000),
                     ((ushort)0x01ff, (ushort)0x00fc, (ushort)0x0a04),
                 })
        {
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            nativeOam.BeginFrame();
            installedOam.BeginFrame();
            bool onScreen = (originY & 0xff00) == 0;
            nativeOam.AddEnemyProjectileSpritemap(bus, pointer,
                originX, originY, graphicsIndex, onScreen);
            installedOam.AddEnemySpritemap(installed.Get(pointer).Span,
                originX, originY,
                unchecked((ushort)(graphicsIndex & 0xff00)),
                unchecked((byte)graphicsIndex),
                clipVerticalWrap: true, originYIsOnScreen: onScreen);
            AssertEqual(nativeOam.NextByteOffset, installedOam.NextByteOffset,
                $"installed $8D:{pointer:X4} OAM part count");
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable),
                $"installed $8D:{pointer:X4} OAM pixels, wrapping, tile base and palette");
        }

        foreach ((ushort operand, ushort pointer) in new[]
                 {
                     (SkreeMetareeParticleVisualDefinitions.SkreeOperand,
                         SkreeMetareeParticleVisualDefinitions.SkreeComposition),
                     (SkreeMetareeParticleVisualDefinitions.MetareeOperand,
                         SkreeMetareeParticleVisualDefinitions.MetareeComposition),
                 })
        {
            ushort nativePointer = unchecked((ushort)(
                bus.ReadByte(0x860000 | operand) |
                bus.ReadByte(0x860000 | unchecked((ushort)(operand + 1))) << 8));
            AssertEqual(nativePointer, SkreeMetareeParticleVisualDefinitions.Resolve(operand),
                $"particle visual operand $86:{operand:X4} matches the pinned cartridge");
            AssertEqual(nativePointer, pointer,
                $"particle visual operand $86:{operand:X4} selects its editable composition");
        }
        VerifyBurstVisuals(stock);
        VerifySharedProgramVisuals(stock);
        ushort ceresOperand = CeresRidleyProjectileInstructionProgramDefinitions
            .PresentationWordAddress(0);
        OamBuffer nativeCeres = DrawProgramFrame(null, ceresOperand, bus,
            RoomEnemyProjectileKind.CeresRidleyFireball);
        OamBuffer installedCeres = DrawProgramFrame(stock, ceresOperand,
            new EnemyProjectileVisualReadGuard(bus),
            RoomEnemyProjectileKind.CeresRidleyFireball);
        AssertTrue(nativeCeres.NextByteOffset > 0 &&
            nativeCeres.LowTable.SequenceEqual(installedCeres.LowTable) &&
            nativeCeres.HighTable.SequenceEqual(installedCeres.HighTable),
            "Ceres Ridley fireball uses installed OAM without visual ROM reads");
        for (int index = 0;
             index < AlcoonFireballInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = AlcoonFireballInstructionProgramDefinitions
                .PresentationWordAddress(index);
            OamBuffer nativeAlcoon = DrawProgramFrame(null, operand, bus,
                RoomEnemyProjectileKind.AlcoonFireball);
            OamBuffer installedAlcoon = DrawProgramFrame(stock, operand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball);
            AssertTrue(nativeAlcoon.NextByteOffset > 0 &&
                nativeAlcoon.NextByteOffset == installedAlcoon.NextByteOffset &&
                nativeAlcoon.LowTable.SequenceEqual(installedAlcoon.LowTable) &&
                nativeAlcoon.HighTable.SequenceEqual(installedAlcoon.HighTable),
                $"Alcoon fireball frame {index} draws stock OAM without visual ROM reads");
        }
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in new (string, RoomEnemyProjectileKind,
                         int, Func<int, ushort>)[]
                 {
                     ("Golden Torizo Super Missile",
                         RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
                         GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordCount,
                         GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordAddress),
                     ("Golden Torizo eye beam",
                         RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
                         GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordCount,
                         GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordAddress),
                 })
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                OamBuffer native = DrawProgramFrame(null, operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind);
                AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                    native.LowTable.SequenceEqual(extracted.LowTable) &&
                    native.HighTable.SequenceEqual(extracted.HighTable),
                $"{family} frame {index} draws stock OAM without visual ROM reads");
            }
        }
        for (int index = 0;
             index < GoldenTorizoEggInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = GoldenTorizoEggInstructionProgramDefinitions
                .PresentationWordAddress(index);
            OamBuffer native = DrawProgramFrame(null, operand, bus,
                RoomEnemyProjectileKind.GoldenTorizoEgg);
            OamBuffer extracted = DrawProgramFrame(stock, operand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg);
            AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                native.LowTable.SequenceEqual(extracted.LowTable) &&
                native.HighTable.SequenceEqual(extracted.HighTable),
                $"Golden Torizo egg frame {index} draws stock OAM without visual ROM reads");
        }
        var torizoFamilies = new (string Name, RoomEnemyProjectileKind Kind, int Count,
            Func<int, ushort> AddressAt)[]
        {
            ("drool", RoomEnemyProjectileKind.BombTorizoLowHealthDrool,
                BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount,
                BombTorizoDroolInstructionProgramDefinitions.PresentationWordAddress),
            ("explosive swipe", RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
                TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordCount,
                TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordAddress),
            ("sonic boom", RoomEnemyProjectileKind.BombTorizoSonicBoom,
                TorizoSonicBoomInstructionProgramDefinitions.PresentationWordCount,
                TorizoSonicBoomInstructionProgramDefinitions.PresentationWordAddress),
            ("landing dust", RoomEnemyProjectileKind.BombTorizoRightFootDust,
                TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount,
                TorizoLandingDustInstructionProgramDefinitions.PresentationWordAddress),
            ("explosion", RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
                TorizoExplosionInstructionProgramDefinitions.PresentationWordCount,
                TorizoExplosionInstructionProgramDefinitions.PresentationWordAddress),
            ("Chozo orb", RoomEnemyProjectileKind.BombTorizoChozoOrb,
                TorizoChozoOrbInstructionProgramDefinitions.PresentationWordCount,
                TorizoChozoOrbInstructionProgramDefinitions.PresentationWordAddress),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in torizoFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                OamBuffer native = DrawProgramFrame(null, operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind);
                AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                    native.LowTable.SequenceEqual(extracted.LowTable) &&
                    native.HighTable.SequenceEqual(extracted.HighTable),
                $"Torizo {family} frame {index} draws stock OAM without visual ROM reads");
            }
        }
        var genericDeathFamilies = new (string Name, RoomEnemyProjectileKind Kind, int Count,
            Func<int, ushort> AddressAt)[]
        {
            ("enemy pickup", RoomEnemyProjectileKind.EnemyDeathPickup,
                EnemyPickupInstructionProgramDefinitions.PresentationWordCount,
                EnemyPickupInstructionProgramDefinitions.PresentationWordAddress),
            ("enemy death", RoomEnemyProjectileKind.EnemyDeathExplosion,
                EnemyDeathInstructionProgramDefinitions.PresentationWordCount,
                EnemyDeathInstructionProgramDefinitions.PresentationWordAddress),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in genericDeathFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                OamBuffer native = DrawProgramFrame(null, operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind);
                AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                    native.LowTable.SequenceEqual(extracted.LowTable) &&
                    native.HighTable.SequenceEqual(extracted.HighTable),
                $"{family} frame {index} draws stock OAM without visual ROM reads");
            }
        }
        var environmentAndAttackFamilies = new (string Name, RoomEnemyProjectileKind Kind,
            int Count, Func<int, ushort> AddressAt)[]
        {
            ("Ceres falling debris", RoomEnemyProjectileKind.CeresFallingDebrisLight,
                CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount,
                CeresFallingDebrisInstructionProgramDefinitions.PresentationWordAddress),
            ("save-station electricity", RoomEnemyProjectileKind.SaveStationElectricity,
                SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount,
                SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress),
            ("gunship dust", RoomEnemyProjectileKind.GunshipLiftoffDustCloud,
                GunshipDustInstructionProgramDefinitions.PresentationWordCount,
                GunshipDustInstructionProgramDefinitions.PresentationWordAddress),
            ("falling spark", RoomEnemyProjectileKind.FallingSpark,
                FallingSparkInstructionProgramDefinitions.PresentationWordCount,
                FallingSparkInstructionProgramDefinitions.PresentationWordAddress),
            ("Magdollite lava", RoomEnemyProjectileKind.LavaThrownByMagdollite,
                MagdolliteLavaInstructionProgramDefinitions.PresentationWordCount,
                MagdolliteLavaInstructionProgramDefinitions.PresentationWordAddress),
            ("Chozo/Tourian dust", RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep,
                ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount,
                ChozoTourianDustInstructionProgramDefinitions.PresentationWordAddress),
            ("eye-door sweat", RoomEnemyProjectileKind.EyeDoorSweat,
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount,
                EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress),
            ("Ki Hunter acid spit", RoomEnemyProjectileKind.KiHunterAcidSpitLeft,
                KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount,
                KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordAddress),
            ("Fune/Namihe fireball", RoomEnemyProjectileKind.FuneFireball,
                FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount,
                FuneNamiheFireballInstructionProgramDefinitions.PresentationWordAddress),
            ("Dragon fireball", RoomEnemyProjectileKind.DragonFireball,
                DragonFireballInstructionProgramDefinitions.PresentationWordCount,
                DragonFireballInstructionProgramDefinitions.PresentationWordAddress),
            ("Powamp spike", RoomEnemyProjectileKind.PowampSpike,
                PowampSpikeInstructionProgramDefinitions.PresentationWordCount,
                PowampSpikeInstructionProgramDefinitions.PresentationWordAddress),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in environmentAndAttackFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                OamBuffer native = DrawProgramFrame(null, operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind);
                AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                    native.LowTable.SequenceEqual(extracted.LowTable) &&
                    native.HighTable.SequenceEqual(extracted.HighTable),
                $"{family} frame {index} draws stock OAM without visual ROM reads");
            }
        }
        var motherBrainAndStatueFamilies = new (string Name, RoomEnemyProjectileKind Kind,
            int Count, Func<int, ushort> AddressAt)[]
        {
            ("Bomb Torizo statue fragments", RoomEnemyProjectileKind.BombTorizoStatueBreaking,
                BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount,
                BombTorizoStatueInstructionProgramDefinitions.PresentationWordAddress),
            ("Mother Brain glass", RoomEnemyProjectileKind.MotherBrainGlassShard,
                MotherBrainGlassInstructionProgramDefinitions.PresentationWordCount,
                MotherBrainGlassInstructionProgramDefinitions.PresentationWordAddress),
            ("Mother Brain hand beam", RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
                MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordCount,
                MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordAddress),
            ("Mother Brain top tube", RoomEnemyProjectileKind.MotherBrainTopRightTube,
                MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount,
                MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordAddress),
            ("Mother Brain turret", RoomEnemyProjectileKind.MotherBrainRoomTurret,
                MotherBrainTurretInstructionProgramDefinitions.PresentationWordCount,
                MotherBrainTurretInstructionProgramDefinitions.PresentationWordAddress),
        };
        foreach ((string family, RoomEnemyProjectileKind kind, int count,
                     Func<int, ushort> addressAt) in motherBrainAndStatueFamilies)
        {
            for (int index = 0; index < count; index++)
            {
                ushort operand = addressAt(index);
                OamBuffer native = DrawProgramFrame(null, operand, bus, kind);
                OamBuffer extracted = DrawProgramFrame(stock, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind);
                AssertTrue(native.NextByteOffset == extracted.NextByteOffset &&
                    native.LowTable.SequenceEqual(extracted.LowTable) &&
                    native.HighTable.SequenceEqual(extracted.HighTable),
                    $"{family} frame {index} draws stock OAM without visual ROM reads");
            }
        }
        OamBuffer nativeShard = DrawNoobTubeShard(null, bus);
        OamBuffer installedShard = DrawNoobTubeShard(stock,
            new EnemyProjectileVisualReadGuard(bus));
        AssertTrue(nativeShard.NextByteOffset > 0 &&
            nativeShard.LowTable.SequenceEqual(installedShard.LowTable) &&
            nativeShard.HighTable.SequenceEqual(installedShard.HighTable),
            "n00b-tube flicker instruction uses installed OAM without visual ROM reads");

        var nativeSamus = new SamusState { XPosition = 0x0080, YPosition = 0 };
        var installedSamus = new SamusState { XPosition = 0x0080, YPosition = 0 };
        var nativeArrival = new CeresElevatorArrivalState(bus, nativeSamus);
        var installedArrival = new CeresElevatorArrivalState(
            new EnemyProjectileVisualReadGuard(bus), installedSamus, installed);
        for (int frame = 0; frame < 3; frame++)
        {
            nativeArrival.Step(nativeSamus);
            installedArrival.Step(installedSamus);
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            nativeOam.BeginFrame();
            installedOam.BeginFrame();
            nativeArrival.Draw(nativeOam, 0, 0);
            installedArrival.Draw(installedOam, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable),
                $"Ceres elevator arrival frame {frame} draws from installed $8D compositions");
        }

        string overrides = Path.Combine(directory, "eproj-overrides");
        Directory.CreateDirectory(overrides);
        EnemyProjectileSpritemapDocument document =
            JsonSerializer.Deserialize<EnemyProjectileSpritemapDocument>(
                File.ReadAllBytes(Path.Combine(directory,
                    EnemyProjectileSpritemapDefinitions.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        SpriteVisualPart[] pad = document.Frames["ceres_elevator_pad_0"];
        pad[0] = pad[0] with { OffsetX = pad[0].OffsetX + 1 };
        SpriteVisualPart[] skree = document.Frames["skree_debris"];
        skree[0] = skree[0] with { OffsetX = skree[0].OffsetX + 1 };
        EnemyProjectilePresentationFrameDefinition editableProgramFrame =
            EnemyProjectileInstructionMechanicsDefinitions.VisualFrames[0];
        SpriteVisualPart[] shared = document.ProgramFrames![editableProgramFrame.Name];
        shared[0] = shared[0] with { OffsetX = shared[0].OffsetX + 1 };
        string ceresFrameName = EnemyProjectilePresentationFrameDefinitions.All.ToArray()
            .Single(frame => frame.OperandAddress == ceresOperand).Name;
        SpriteVisualPart[] ceres = document.ProgramFrames[ceresFrameName];
        ceres[0] = ceres[0] with { OffsetX = ceres[0].OffsetX + 1 };
        ushort alcoonOperand = AlcoonFireballInstructionProgramDefinitions
            .PresentationWordAddress(0);
        string alcoonFrameName = EnemyProjectilePresentationFrameDefinitions.All.ToArray()
            .Single(frame => frame.OperandAddress == alcoonOperand).Name;
        SpriteVisualPart[] alcoon = document.ProgramFrames[alcoonFrameName];
        alcoon[0] = alcoon[0] with { OffsetX = alcoon[0].OffsetX + 1 };
        ushort goldenMissileOperand = GoldenTorizoSuperMissileInstructionProgramDefinitions
            .PresentationWordAddress(0);
        string goldenMissileFrameName = EnemyProjectilePresentationFrameDefinitions.All.ToArray()
            .Single(frame => frame.OperandAddress == goldenMissileOperand).Name;
        SpriteVisualPart[] goldenMissile = document.ProgramFrames[goldenMissileFrameName];
        goldenMissile[0] = goldenMissile[0] with { OffsetX = goldenMissile[0].OffsetX + 1 };
        ushort goldenEyeOperand = GoldenTorizoEyeBeamInstructionProgramDefinitions
            .PresentationWordAddress(0);
        string goldenEyeFrameName = EnemyProjectilePresentationFrameDefinitions.All.ToArray()
            .Single(frame => frame.OperandAddress == goldenEyeOperand).Name;
        SpriteVisualPart[] goldenEye = document.ProgramFrames[goldenEyeFrameName];
        goldenEye[0] = goldenEye[0] with { OffsetX = goldenEye[0].OffsetX + 1 };
        ushort goldenEggOperand = GoldenTorizoEggInstructionProgramDefinitions
            .PresentationWordAddress(0);
        string goldenEggFrameName = EnemyProjectilePresentationFrameDefinitions.All.ToArray()
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
                string frameName = EnemyProjectilePresentationFrameDefinitions.All.ToArray()
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
                string frameName = EnemyProjectilePresentationFrameDefinitions.All.ToArray()
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
                string frameName = EnemyProjectilePresentationFrameDefinitions.All.ToArray()
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
                string frameName = EnemyProjectilePresentationFrameDefinitions.All.ToArray()
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
        EnemyTileArtworkCatalog editedArt = EnemyTileArtworkFiles.Load(directory, overrides);
        EnemyProjectileSpritemapCatalog edited = editedArt.ProjectileSpritemaps!;
        var originalOam = new OamBuffer();
        var editedOam = new OamBuffer();
        originalOam.BeginFrame();
        editedOam.BeginFrame();
        originalOam.AddEnemySpritemap(installed.Get(0xb1ba).Span,
            0x0080, 0x0004, 0, 0, clipVerticalWrap: true);
        editedOam.AddEnemySpritemap(edited.Get(0xb1ba).Span,
            0x0080, 0x0004, 0, 0, clipVerticalWrap: true);
        AssertEqual((byte)(originalOam.LowTable[0] + 1), editedOam.LowTable[0],
            "enemy-projectile composition override moves the live Ceres pad OAM part");
        OamBuffer stockBurst = DrawBurst(stock,
            SkreeMetareeParticleInstructionProgramDefinitions.Skree, bus);
        OamBuffer editedBurst = DrawBurst(editedArt,
            SkreeMetareeParticleInstructionProgramDefinitions.Skree, bus);
        AssertTrue(!stockBurst.LowTable.SequenceEqual(editedBurst.LowTable),
            "edited Skree debris composition changes production burst OAM");
        AssertTrue(!DrawSharedFrame(stock, editableProgramFrame.OperandAddress, bus)
                .LowTable.SequenceEqual(DrawSharedFrame(editedArt,
                    editableProgramFrame.OperandAddress,
                    new EnemyProjectileVisualReadGuard(bus)).LowTable),
            "edited shared projectile frame changes production OAM without a ROM visual read");
        AssertTrue(!installedCeres.LowTable.SequenceEqual(
                DrawProgramFrame(editedArt, ceresOperand,
                    new EnemyProjectileVisualReadGuard(bus),
                    RoomEnemyProjectileKind.CeresRidleyFireball).LowTable),
            "edited Ceres Ridley frame changes production OAM without a ROM visual read");
        AssertTrue(!DrawProgramFrame(stock, alcoonOperand,
                    new EnemyProjectileVisualReadGuard(bus),
                    RoomEnemyProjectileKind.AlcoonFireball).LowTable
                .SequenceEqual(DrawProgramFrame(editedArt, alcoonOperand,
                    new EnemyProjectileVisualReadGuard(bus),
                    RoomEnemyProjectileKind.AlcoonFireball).LowTable),
            "edited Alcoon fireball frame changes production OAM without a ROM visual read");
        foreach ((string family, ushort operand, RoomEnemyProjectileKind kind) in new[]
                 {
                     ("Super Missile", goldenMissileOperand,
                         RoomEnemyProjectileKind.GoldenTorizoSuperMissile),
                     ("eye beam", goldenEyeOperand,
                         RoomEnemyProjectileKind.GoldenTorizoEyeBeam),
                 })
        {
            AssertTrue(!DrawProgramFrame(stock, operand,
                        new EnemyProjectileVisualReadGuard(bus), kind).LowTable
                    .SequenceEqual(DrawProgramFrame(editedArt, operand,
                        new EnemyProjectileVisualReadGuard(bus), kind).LowTable),
                $"edited Golden Torizo {family} frame changes production OAM");
        }
        AssertTrue(!DrawProgramFrame(stock, goldenEggOperand,
                    new EnemyProjectileVisualReadGuard(bus),
                    RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable
                .SequenceEqual(DrawProgramFrame(editedArt, goldenEggOperand,
                    new EnemyProjectileVisualReadGuard(bus),
                    RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable),
            "edited Golden Torizo egg frame changes production OAM");
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in torizoFamilies)
        {
            ushort operand = torizoEditableOperands[family];
            AssertTrue(!DrawProgramFrame(stock, operand,
                        new EnemyProjectileVisualReadGuard(bus), kind).LowTable
                    .SequenceEqual(DrawProgramFrame(editedArt, operand,
                        new EnemyProjectileVisualReadGuard(bus), kind).LowTable),
                $"edited Torizo {family} frame changes production OAM");
        }
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in genericDeathFamilies)
        {
            ushort operand = genericDeathEditableOperands[family];
            AssertTrue(!DrawProgramFrame(stock, operand,
                        new EnemyProjectileVisualReadGuard(bus), kind).LowTable
                    .SequenceEqual(DrawProgramFrame(editedArt, operand,
                        new EnemyProjectileVisualReadGuard(bus), kind).LowTable),
                $"edited {family} frame changes production OAM");
        }
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in environmentAndAttackFamilies)
        {
            ushort operand = environmentEditableOperands[family];
            AssertTrue(!DrawProgramFrame(stock, operand,
                        new EnemyProjectileVisualReadGuard(bus), kind).LowTable
                    .SequenceEqual(DrawProgramFrame(editedArt, operand,
                        new EnemyProjectileVisualReadGuard(bus), kind).LowTable),
                $"edited {family} frame changes production OAM");
        }
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in motherBrainAndStatueFamilies)
        {
            ushort operand = motherBrainEditableOperands[family];
            AssertTrue(!DrawProgramFrame(stock, operand,
                        new EnemyProjectileVisualReadGuard(bus), kind).LowTable
                    .SequenceEqual(DrawProgramFrame(editedArt, operand,
                        new EnemyProjectileVisualReadGuard(bus), kind).LowTable),
                $"edited {family} frame changes production OAM");
        }
        var incompleteCurrent = document.Frames
            .Where(entry => entry.Key != "skree_debris")
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        byte[] invalidCurrentJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.Version,
                Frames = incompleteCurrent,
                ProgramFrames = document.ProgramFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileSpritemapCatalog.Load(new MemoryStream(invalidCurrentJson),
                installed),
            "current projectile compositions cannot silently omit Skree debris");
        var incompleteProgramFrames = document.ProgramFrames!
            .Where(entry => entry.Key != editableProgramFrame.Name)
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        byte[] invalidProgramJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.Version,
                Frames = document.Frames,
                ProgramFrames = incompleteProgramFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileSpritemapCatalog.Load(new MemoryStream(legacyJson)),
            "a partial version-one composition cannot stand alone as stock content");
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), legacyJson);
        EnemyTileArtworkCatalog migrated = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawBurst(migrated,
                SkreeMetareeParticleInstructionProgramDefinitions.Skree, bus)
            .LowTable.SequenceEqual(stockBurst.LowTable),
            "version-one Ceres override inherits stock Skree debris composition");
        var migratedPad = new OamBuffer();
        migratedPad.BeginFrame();
        migratedPad.AddEnemySpritemap(migrated.ProjectileSpritemaps!.Get(0xb1ba).Span,
            0x0080, 0x0004, 0, 0, clipVerticalWrap: true);
        AssertEqual(editedOam.LowTable[0], migratedPad.LowTable[0],
            "version-one Ceres composition edit survives the version-three catalog upgrade");
        byte[] versionTwoJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = 2,
                Frames = document.Frames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionTwoJson);
        EnemyTileArtworkCatalog migratedV2 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawBurst(migratedV2,
                SkreeMetareeParticleInstructionProgramDefinitions.Skree, bus)
            .LowTable.SequenceEqual(editedBurst.LowTable),
            "version-two edits retain the Skree debris frame");
        AssertTrue(DrawSharedFrame(stock, editableProgramFrame.OperandAddress, bus)
                .LowTable.SequenceEqual(DrawSharedFrame(migratedV2,
                    editableProgramFrame.OperandAddress,
                    new EnemyProjectileVisualReadGuard(bus)).LowTable),
            "version-two edits inherit stock shared-program frames");
        var versionThreeFrames = EnemyProjectileInstructionMechanicsDefinitions.VisualFrames
            .ToArray().ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames![frame.Name], StringComparer.Ordinal);
        byte[] versionThreeJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = 3,
                Frames = document.Frames,
                ProgramFrames = versionThreeFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionThreeJson);
        EnemyTileArtworkCatalog migratedV3 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawSharedFrame(migratedV3, editableProgramFrame.OperandAddress,
                new EnemyProjectileVisualReadGuard(bus)).LowTable
            .SequenceEqual(DrawSharedFrame(editedArt, editableProgramFrame.OperandAddress,
                new EnemyProjectileVisualReadGuard(bus)).LowTable),
            "version-three shared-program edits survive the expanded projectile catalog");
        AssertTrue(DrawProgramFrame(migratedV3, ceresOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.CeresRidleyFireball).LowTable
            .SequenceEqual(installedCeres.LowTable),
            "version-three overrides inherit newly extracted Ceres Ridley frames");
        var alcoonOperands = Enumerable.Range(0,
                AlcoonFireballInstructionProgramDefinitions.PresentationWordCount)
            .Select(AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress)
            .ToHashSet();
        var versionFourFrames = EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizo
            .ToArray()
            .Where(frame => !alcoonOperands.Contains(frame.OperandAddress))
            .ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames![frame.Name], StringComparer.Ordinal);
        byte[] versionFourJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.PreAlcoonVersion,
                Frames = document.Frames,
                ProgramFrames = versionFourFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionFourJson);
        EnemyTileArtworkCatalog migratedV4 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawProgramFrame(migratedV4, ceresOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.CeresRidleyFireball).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, ceresOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.CeresRidleyFireball).LowTable),
            "version-four overrides retain their edited projectile frames");
        AssertTrue(DrawProgramFrame(migratedV4, alcoonOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball).LowTable
            .SequenceEqual(DrawProgramFrame(stock, alcoonOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball).LowTable),
            "version-four overrides inherit newly extracted Alcoon fireball frames");
        var versionFiveFrames = EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizo
            .ToArray().ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames![frame.Name], StringComparer.Ordinal);
        byte[] versionFiveJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.PreGoldenTorizoVersion,
                Frames = document.Frames,
                ProgramFrames = versionFiveFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionFiveJson);
        EnemyTileArtworkCatalog migratedV5 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawProgramFrame(migratedV5, alcoonOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, alcoonOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.AlcoonFireball).LowTable),
            "version-five overrides retain their edited Alcoon fireball frames");
        foreach ((string family, ushort operand, RoomEnemyProjectileKind kind) in new[]
                 {
                     ("Super Missile", goldenMissileOperand,
                         RoomEnemyProjectileKind.GoldenTorizoSuperMissile),
                     ("eye beam", goldenEyeOperand,
                         RoomEnemyProjectileKind.GoldenTorizoEyeBeam),
                 })
        {
            AssertTrue(DrawProgramFrame(migratedV5, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind).LowTable
                .SequenceEqual(DrawProgramFrame(stock, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind).LowTable),
                $"version-five overrides inherit extracted Golden Torizo {family} frames");
        }
        var versionSixFrames = EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizoEgg
            .ToArray().ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames![frame.Name], StringComparer.Ordinal);
        byte[] versionSixJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.PreGoldenTorizoEggVersion,
                Frames = document.Frames,
                ProgramFrames = versionSixFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionSixJson);
        EnemyTileArtworkCatalog migratedV6 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawProgramFrame(migratedV6, goldenMissileOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoSuperMissile).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, goldenMissileOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoSuperMissile).LowTable),
            "version-six overrides retain their edited Golden Torizo missile frame");
        AssertTrue(DrawProgramFrame(migratedV6, goldenEggOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable
            .SequenceEqual(DrawProgramFrame(stock, goldenEggOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable),
            "version-six overrides inherit extracted Golden Torizo egg frames");
        var versionSevenFrames = EnemyProjectilePresentationFrameDefinitions.PreTorizoEffects
            .ToArray().ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames![frame.Name], StringComparer.Ordinal);
        byte[] versionSevenJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.PreTorizoEffectsVersion,
                Frames = document.Frames,
                ProgramFrames = versionSevenFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionSevenJson);
        EnemyTileArtworkCatalog migratedV7 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawProgramFrame(migratedV7, goldenEggOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, goldenEggOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.GoldenTorizoEgg).LowTable),
            "version-seven overrides retain their edited Golden Torizo egg frame");
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in torizoFamilies)
        {
            ushort operand = torizoEditableOperands[family];
            AssertTrue(DrawProgramFrame(migratedV7, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind).LowTable
                .SequenceEqual(DrawProgramFrame(stock, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind).LowTable),
                $"version-seven overrides inherit Torizo {family} frames");
        }
        var versionEightFrames = EnemyProjectilePresentationFrameDefinitions.PreGenericEnemyDeath
            .ToArray().ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames![frame.Name], StringComparer.Ordinal);
        byte[] versionEightJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.PreGenericEnemyDeathVersion,
                Frames = document.Frames,
                ProgramFrames = versionEightFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionEightJson);
        EnemyTileArtworkCatalog migratedV8 = EnemyTileArtworkFiles.Load(directory, overrides);
        ushort editedTorizoOperand = torizoEditableOperands["Chozo orb"];
        AssertTrue(DrawProgramFrame(migratedV8, editedTorizoOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.BombTorizoChozoOrb).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, editedTorizoOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.BombTorizoChozoOrb).LowTable),
            "version-eight overrides retain their edited Torizo Chozo-orb frame");
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in genericDeathFamilies)
        {
            ushort operand = genericDeathEditableOperands[family];
            AssertTrue(DrawProgramFrame(migratedV8, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind).LowTable
                .SequenceEqual(DrawProgramFrame(stock, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind).LowTable),
                $"version-eight overrides inherit {family} frames");
        }
        var versionNineFrames = EnemyProjectilePresentationFrameDefinitions.PreEnvironmentAndAttack
            .ToArray().ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames![frame.Name], StringComparer.Ordinal);
        byte[] versionNineJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.PreEnvironmentAndAttackVersion,
                Frames = document.Frames,
                ProgramFrames = versionNineFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionNineJson);
        EnemyTileArtworkCatalog migratedV9 = EnemyTileArtworkFiles.Load(directory, overrides);
        ushort editedDeathOperand = genericDeathEditableOperands["enemy death"];
        AssertTrue(DrawProgramFrame(migratedV9, editedDeathOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.EnemyDeathExplosion).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, editedDeathOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.EnemyDeathExplosion).LowTable),
            "version-nine overrides retain their edited generic enemy-death frame");
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in environmentAndAttackFamilies)
        {
            ushort operand = environmentEditableOperands[family];
            AssertTrue(DrawProgramFrame(migratedV9, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind).LowTable
                .SequenceEqual(DrawProgramFrame(stock, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind).LowTable),
                $"version-nine overrides inherit {family} frames");
        }
        var versionTenFrames = EnemyProjectilePresentationFrameDefinitions.PreMotherBrainAndStatue
            .ToArray().ToDictionary(frame => frame.Name,
                frame => document.ProgramFrames![frame.Name], StringComparer.Ordinal);
        byte[] versionTenJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.PreMotherBrainAndStatueVersion,
                Frames = document.Frames,
                ProgramFrames = versionTenFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionTenJson);
        EnemyTileArtworkCatalog migratedV10 = EnemyTileArtworkFiles.Load(directory, overrides);
        ushort editedEnvironmentOperand = environmentEditableOperands["falling spark"];
        AssertTrue(DrawProgramFrame(migratedV10, editedEnvironmentOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.FallingSpark).LowTable
            .SequenceEqual(DrawProgramFrame(editedArt, editedEnvironmentOperand,
                new EnemyProjectileVisualReadGuard(bus),
                RoomEnemyProjectileKind.FallingSpark).LowTable),
            "version-ten overrides retain their edited falling-spark frame");
        foreach ((string family, RoomEnemyProjectileKind kind, int _,
                     Func<int, ushort> _) in motherBrainAndStatueFamilies)
        {
            ushort operand = motherBrainEditableOperands[family];
            AssertTrue(DrawProgramFrame(migratedV10, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind).LowTable
                .SequenceEqual(DrawProgramFrame(stock, operand,
                    new EnemyProjectileVisualReadGuard(bus), kind).LowTable),
                $"version-ten overrides inherit {family} frames");
        }
        Console.WriteLine($"  Enemy projectile visuals: " +
            $"{EnemyProjectilePresentationFrameDefinitions.All.Length} catalogued timed/flicker frames " +
            "match native OAM; installed draws, editable frames, and v1-v10 migrations pass.");

        void VerifySharedProgramVisuals(EnemyTileArtworkCatalog artwork)
        {
            ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> frames =
                EnemyProjectilePresentationFrameDefinitions.All;
            AssertTrue(frames.Length > 300,
                "translated enemy projectile programs expose their complete catalogued visual frame set");
            foreach (EnemyProjectilePresentationFrameDefinition frame in frames)
            {
                ushort nativePointer = unchecked((ushort)(
                    bus.ReadByte(0x860000 | frame.OperandAddress) |
                    bus.ReadByte(0x860000 | unchecked((ushort)(frame.OperandAddress + 1))) << 8));
                foreach ((ushort originX, ushort originY) in new[]
                         {
                             ((ushort)128, (ushort)96),
                             ((ushort)0x01ff, (ushort)0x00fc),
                         })
                {
                    var nativeOam = new OamBuffer();
                    var installedOam = new OamBuffer();
                    nativeOam.BeginFrame();
                    installedOam.BeginFrame();
                    nativeOam.AddEnemyProjectileSpritemap(bus, nativePointer,
                        originX, originY, 0x0a04, originYIsOnScreen: true);
                    installedOam.AddEnemySpritemap(
                        installed.GetProgramFrame(frame.OperandAddress).Span,
                        originX, originY, 0x0a00, 4,
                        clipVerticalWrap: true, originYIsOnScreen: true);
                    AssertTrue(nativeOam.NextByteOffset == installedOam.NextByteOffset &&
                        nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                        nativeOam.HighTable.SequenceEqual(installedOam.HighTable),
                        $"installed shared frame $86:{frame.OperandAddress:X4} matches ROM OAM at {originX:X4},{originY:X4}");
                }
                if (EnemyProjectileInstructionMechanicsDefinitions.IsVisualOperand(
                        frame.OperandAddress))
                {
                    AssertTrue(DrawSharedFrame(null, frame.OperandAddress, bus)
                            .LowTable.SequenceEqual(DrawSharedFrame(artwork,
                                frame.OperandAddress,
                                new EnemyProjectileVisualReadGuard(bus)).LowTable),
                        $"shared frame $86:{frame.OperandAddress:X4} runs without visual ROM reads");
                }
            }
        }

        void VerifyBurstVisuals(EnemyTileArtworkCatalog artwork)
        {
            foreach (ushort program in new[]
                     {
                         SkreeMetareeParticleInstructionProgramDefinitions.Skree,
                         SkreeMetareeParticleInstructionProgramDefinitions.Metaree,
                     })
            {
                OamBuffer native = DrawBurst(null, program, bus);
                OamBuffer installedBurst = DrawBurst(artwork, program,
                    new EnemyProjectileVisualReadGuard(bus));
                AssertTrue(native.NextByteOffset > 0 &&
                    native.NextByteOffset == installedBurst.NextByteOffset &&
                    native.LowTable.SequenceEqual(installedBurst.LowTable) &&
                    native.HighTable.SequenceEqual(installedBurst.HighTable),
                    $"installed Skree/Metaree burst ${program:X4} draws stock OAM without visual ROM reads");
            }
        }

        static OamBuffer DrawBurst(EnemyTileArtworkCatalog? artwork, ushort program,
            ISnesAddressSpace source)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
            string producer = program == SkreeMetareeParticleInstructionProgramDefinitions.Skree
                ? "SpawnSkreeParticleBurst" : "SpawnMetareeParticleBurst";
            var spawn = typeof(RoomEnemySystem).GetMethod(producer, flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            var process = typeof(RoomEnemySystem).GetMethod(
                "ProcessEnemyProjectileInstructions", flags)!;
            var enemy = new RoomEnemySlot(0)
            {
                XPosition = 128, YPosition = 96,
                VramTilesIndex = 0x0200, PaletteIndex = 0x0c00,
            };
            spawn(enemy);
            foreach (RoomEnemyProjectileSlot particle in enemies.EnemyProjectiles
                         .Where(particle => particle.IsActive))
            {
                particle.InstructionTimer = 1;
                process.Invoke(enemies, [particle, new SamusState(), (ushort)0, (ushort)0]);
            }
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, cameraX: 0, cameraY: 0);
            return oam;
        }

        static OamBuffer DrawSharedFrame(EnemyTileArtworkCatalog? artwork,
            ushort operand, ISnesAddressSpace source) =>
            DrawProgramFrame(artwork, operand, source,
                RoomEnemyProjectileKind.MiscDustExplosion);

        static OamBuffer DrawProgramFrame(EnemyTileArtworkCatalog? artwork,
            ushort operand, ISnesAddressSpace source, RoomEnemyProjectileKind kind)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
            RoomEnemyProjectileSlot slot = enemies.EnemyProjectiles[0];
            slot.Kind = kind;
            slot.XPosition = 128;
            slot.YPosition = 96;
            slot.GraphicsIndex = 0x0a04;
            slot.InstructionPointer = unchecked((ushort)(operand - 2));
            slot.InstructionTimer = 1;
            typeof(RoomEnemySystem).GetMethod(
                "ProcessEnemyProjectileInstructions", flags)!
                .Invoke(enemies, [slot, new SamusState(), (ushort)0, (ushort)0]);
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, 0, 0);
            return oam;
        }

        static OamBuffer DrawNoobTubeShard(EnemyTileArtworkCatalog? artwork,
            ISnesAddressSpace source)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
            RoomEnemyProjectileSlot slot = enemies.EnemyProjectiles[0];
            slot.Kind = RoomEnemyProjectileKind.NoobTubeShard;
            slot.XPosition = slot.Variable1 = 128;
            slot.YPosition = 96;
            slot.GraphicsIndex = 0x0a04;
            slot.InstructionPointer = unchecked((ushort)(
                NoobTubeProjectileInstructionProgramDefinitions.ShardInstructionLists[0] + 4));
            slot.InstructionTimer = 1;
            typeof(RoomEnemySystem).GetMethod(
                "ProcessEnemyProjectileInstructions", flags)!
                .Invoke(enemies, [slot, new SamusState(), (ushort)0, (ushort)0]);
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, 0, 0);
            return oam;
        }
    }

    private sealed class EnemyProjectileVisualReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            ushort low = unchecked((ushort)address);
            if ((address & 0xff0000) == 0x8d0000 ||
                (address & 0xff0000) == 0x860000 &&
                (EnemyProjectilePresentationFrameDefinitions.Contains(low) ||
                 EnemyProjectilePresentationFrameDefinitions.Contains(
                     unchecked((ushort)(low - 1))) ||
                 low is SkreeMetareeParticleVisualDefinitions.SkreeOperand or
                    SkreeMetareeParticleVisualDefinitions.MetareeOperand ||
                 low == SkreeMetareeParticleVisualDefinitions.SkreeOperand + 1 ||
                 low == SkreeMetareeParticleVisualDefinitions.MetareeOperand + 1))
                throw new InvalidOperationException(
                    $"Installed enemy projectile reread visual byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
