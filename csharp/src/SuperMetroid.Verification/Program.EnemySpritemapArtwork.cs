using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledEnemySpritemaps(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertTrue(stock.Spritemaps is not null,
            "installed enemy catalog contains named OAM compositions");
        AssertEqual(EnemySpritemapDefinitions.PreCeresDoorFrameCount,
            EnemySpritemapDefinitions.PreDisplayBindingsFrameCount,
            "Ceres-door expansion retains the previous display-binding identities");
        AssertEqual(EnemySpritemapDefinitions.PreCeresDoorFrameCount + 15,
            EnemySpritemapDefinitions.PreCeresBabyFrameCount,
            "Ceres door adds its thirteen selected poses, initial pose, and private overlay");
        AssertEqual(EnemySpritemapDefinitions.PreCeresBabyFrameCount + 3,
            EnemySpritemapDefinitions.Frames.Length,
            "Ceres Baby adds its three authored OAM poses");
        AssertTrue(!EnemySpritemapDefinitions.TryFrameAt(0xffff, 0xe312, out _),
            "unknown enemy family keeps the existing cartridge selector path");
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        {
            AssertTrue(stock.Spritemaps!.TryGet(frame.Bank, frame.Pointer, out var parts),
                $"installed enemy frame {frame.Name} exists");
            foreach ((ushort x, ushort y, ushort palette, ushort baseTile) in
                     new (ushort, ushort, ushort, ushort)[]
                     {
                         (0x0010, 0x0020, 0, 0),
                         (0x01f8, 0x00fc, 0x0c00, 0x01ff),
                         (0x0000, 0x0000, 0x0400, 0x0010),
                     })
            {
                var native = new OamBuffer();
                var installed = new OamBuffer();
                native.AddEnemySpritemap(rom, frame.Bank, frame.Pointer,
                    x, y, palette, baseTile);
                installed.AddEnemySpritemap(parts.Span, x, y, palette, baseTile);
                AssertTrue(native.LowTable.SequenceEqual(installed.LowTable) &&
                           native.HighTable.SequenceEqual(installed.HighTable) &&
                           native.NextByteOffset == installed.NextByteOffset,
                    $"installed {frame.Name} OAM matches native at {x:X4},{y:X4}");
            }
            var room = DrawEnemy(stock, new FrameReadGuard(rom), frame.Pointer,
                frame.Name.StartsWith("boyon_", StringComparison.Ordinal)
                    ? RoomEnemySystem.BoyonDefinition
                    : frame.Name.StartsWith("cacatac_", StringComparison.Ordinal)
                        ? RoomEnemySystem.CacatacDefinition
                        : frame.Name.StartsWith("boulder_", StringComparison.Ordinal)
                            ? RoomEnemySystem.BoulderDefinition
                            : frame.Name.StartsWith("skultera_", StringComparison.Ordinal)
                                ? RoomEnemySystem.SkulteraDefinition
                                : frame.Name.StartsWith("waver_", StringComparison.Ordinal)
                                    ? RoomEnemySystem.WaverDefinition
                                    : frame.Name.StartsWith("zoa_", StringComparison.Ordinal)
                                        ? RoomEnemySystem.ZoaDefinition
                                        : frame.Name.StartsWith("metaree_", StringComparison.Ordinal)
                                            ? RoomEnemySystem.MetareeDefinition
                                            : frame.Name.StartsWith("skree_", StringComparison.Ordinal)
                                                ? RoomEnemySystem.SkreeDefinition
                                                : frame.Name.StartsWith("pipe_brinstar_strong_", StringComparison.Ordinal)
                                                    ? PipeBugDefinitions.StrongBrinstarEnemyDefinition
                                                    : frame.Name.StartsWith("pipe_brinstar_", StringComparison.Ordinal)
                                                        ? PipeBugDefinitions.BrinstarEnemyDefinition
                                                        : frame.Name.StartsWith("pipe_norfair_", StringComparison.Ordinal)
                                                            ? PipeBugDefinitions.NorfairEnemyDefinition
                                                            : frame.Name.StartsWith("pipe_yellow_", StringComparison.Ordinal)
                                                                ? PipeBugDefinitions.YellowEnemyDefinition
                                                                : frame.Name.StartsWith("fake_kraid_", StringComparison.Ordinal)
                                                                    ? RoomEnemySystem.FakeKraidDefinition
                                                                : frame.Name.StartsWith("kraid_nail_", StringComparison.Ordinal)
                                                                        ? RoomEnemySystem.KraidGoodNailDefinition
                                                                        : frame.Name.StartsWith("owtch_", StringComparison.Ordinal)
                                                                            ? RoomEnemySystem.OwtchDefinition
                                                                            : frame.Name.StartsWith("stoke_", StringComparison.Ordinal)
                                                                                ? RoomEnemySystem.StokeDefinition
                                                                                : frame.Name.StartsWith("ripper_shared_", StringComparison.Ordinal)
                                                                                    ? RoomEnemySystem.GRipperDefinition
                                                                                    : frame.Name.StartsWith("ripper_", StringComparison.Ordinal)
                                                                                        ? RoomEnemySystem.RipperDefinition
                                                                                        : frame.Name.StartsWith("fireflea_", StringComparison.Ordinal)
                                                                                            ? RoomEnemySystem.FirefleaDefinition
                                                                : frame.Name.StartsWith("ceres_door_", StringComparison.Ordinal) ||
                                                                  frame.Name.StartsWith("ceres_baby_", StringComparison.Ordinal)
                                                                    ? CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer
                                                                : frame.Name.StartsWith("magdollite_", StringComparison.Ordinal)
                                                                                                ? RoomEnemySystem.MagdolliteDefinition
                                : RoomEnemySystem.AtomicDefinition);
            var nativeRoom = new OamBuffer();
            nativeRoom.AddEnemySpritemap(rom, frame.Bank, frame.Pointer,
                0x0040, 0x0080, 0, 0);
            AssertTrue(room.LowTable.SequenceEqual(nativeRoom.LowTable) &&
                       room.HighTable.SequenceEqual(nativeRoom.HighTable),
                $"production room draws installed {frame.Name} without visual ROM reads");
        }

        string fileName = EnemySpritemapDefinitions.FileName;
        string stockPath = Path.Combine(stockDirectory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "stock enemy compositions are manifest-hash checked");
        File.WriteAllBytes(stockPath, original);

        EnemySpritemapDocument document = JsonSerializer.Deserialize<EnemySpritemapDocument>(
            original, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        SpriteVisualPart originalPart = document.Frames["boyon_idle_0"][0];
        document.Frames["boyon_idle_0"][0] = originalPart with
        {
            OffsetX = originalPart.OffsetX + 1,
            TileColumn = originalPart.TileColumn + 1,
        };
        SpriteVisualPart cacatacPart = document.Frames["cacatac_upright_idle_0"][0];
        document.Frames["cacatac_upright_idle_0"][0] = cacatacPart with
        {
            OffsetY = cacatacPart.OffsetY + 1,
        };
        SpriteVisualPart boulderPart = document.Frames["boulder_roll_0"][0];
        document.Frames["boulder_roll_0"][0] = boulderPart with
        {
            OffsetY = boulderPart.OffsetY + 1,
        };
        SpriteVisualPart atomicPart = document.Frames["atomic_up_right_0"][0];
        document.Frames["atomic_up_right_0"][0] = atomicPart with
        {
            OffsetY = atomicPart.OffsetY + 1,
        };
        SpriteVisualPart skulteraPart = document.Frames["skultera_swim_left_0"][0];
        document.Frames["skultera_swim_left_0"][0] = skulteraPart with
        {
            OffsetY = skulteraPart.OffsetY + 1,
        };
        SpriteVisualPart waverPart = document.Frames["waver_steady_left"][0];
        document.Frames["waver_steady_left"][0] = waverPart with
        {
            OffsetY = waverPart.OffsetY + 1,
        };
        foreach (string frameName in new[]
                 { "zoa_shoot_left_0", "metaree_idle_0", "skree_idle_0" })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        foreach (string frameName in new[]
                 {
                     "pipe_brinstar_normal_left_0", "pipe_brinstar_strong_rise_left_0",
                     "pipe_norfair_left_0", "pipe_yellow_fly_left_0",
                 })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        foreach (string frameName in new[]
                 { "fake_kraid_walk_left_0", "kraid_nail_0",
                   "owtch_left_0", "stoke_walk_left_0" })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        foreach (string frameName in new[]
                 { "ripper_shared_left_0", "ripper_shared_frozen_left",
                   "ripper_right_0" })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        SpriteVisualPart firefleaPart = document.Frames["fireflea_cycle_0"][0];
        document.Frames["fireflea_cycle_0"][0] = firefleaPart with
        {
            OffsetY = firefleaPart.OffsetY + 1,
        };
        SpriteVisualPart magdollitePart = document.Frames["magdollite_left_idle_0"][0];
        document.Frames["magdollite_left_idle_0"][0] = magdollitePart with
        {
            OffsetY = magdollitePart.OffsetY + 1,
        };
        SpriteVisualPart ceresDoorPart = document.Frames["ceres_door_right_hold"][0];
        document.Frames["ceres_door_right_hold"][0] = ceresDoorPart with
        {
            OffsetY = ceresDoorPart.OffsetY + 1,
        };
        SpriteVisualPart ceresBabyPart = document.Frames["ceres_baby_round"][0];
        document.Frames["ceres_baby_round"][0] = ceresBabyPart with
        {
            OffsetY = ceresBabyPart.OffsetY + 1,
        };
        string overrideDirectory = Path.Combine(stockDirectory, "spritemap-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort framePointer = EnemySpritemapDefinitions.BoyonFrameAt(0x86ad);
        var stockOam = DrawEnemy(stock, new FrameReadGuard(rom), framePointer,
            RoomEnemySystem.BoyonDefinition);
        var editedOam = DrawEnemy(edited, new FrameReadGuard(rom), framePointer,
            RoomEnemySystem.BoyonDefinition);
        AssertEqual(unchecked((byte)(stockOam.LowTable[0] + 1)),
            editedOam.LowTable[0], "authored Boyon X offset changes live room OAM");
        AssertEqual(unchecked((byte)(stockOam.LowTable[2] + 1)),
            editedOam.LowTable[2], "authored Boyon tile changes live room OAM");
        AssertEqual(stockOam.LowTable[1], editedOam.LowTable[1],
            "visual override leaves Boyon Y unchanged");
        OamBuffer stockCeresDoor = DrawEnemy(stock, new FrameReadGuard(rom),
            0xfa13, CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer);
        OamBuffer editedCeresDoor = DrawEnemy(edited, new FrameReadGuard(rom),
            0xfa13, CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer,
            slot =>
            {
                AssertEqual((ushort)0xfa13, slot.SpritemapPointer,
                    "Ceres door art edit retains the native selected pose");
                AssertEqual((ushort)7, slot.InstructionTimer,
                    "Ceres door art edit preserves instruction timing");
            });
        AssertEqual(unchecked((byte)(stockCeresDoor.LowTable[1] + 1)),
            editedCeresDoor.LowTable[1],
            "authored Ceres door Y offset changes live room OAM");
        AssertEqual(stockCeresDoor.LowTable[0], editedCeresDoor.LowTable[0],
            "Ceres door visual edit leaves physical X unchanged");
        OamBuffer stockCeresBaby = DrawEnemy(stock, new FrameReadGuard(rom),
            CeresBabyInstructionProgramDefinitions.RoundFrame,
            CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer);
        OamBuffer editedCeresBaby = DrawEnemy(edited, new FrameReadGuard(rom),
            CeresBabyInstructionProgramDefinitions.RoundFrame,
            CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer);
        AssertEqual(unchecked((byte)(stockCeresBaby.LowTable[1] + 1)),
            editedCeresBaby.LowTable[1],
            "authored Ceres Baby Y offset changes installed OAM");
        ushort cacatacPointer = EnemySpritemapDefinitions.CacatacFrameAt(0x9e8e);
        var stockCacatac = DrawEnemy(stock, new FrameReadGuard(rom),
            cacatacPointer, RoomEnemySystem.CacatacDefinition);
        var editedCacatac = DrawEnemy(edited, new FrameReadGuard(rom),
            cacatacPointer, RoomEnemySystem.CacatacDefinition);
        AssertEqual(unchecked((byte)(stockCacatac.LowTable[1] + 1)),
            editedCacatac.LowTable[1],
            "authored Cacatac Y offset changes live room OAM");
        AssertEqual(stockCacatac.LowTable[0], editedCacatac.LowTable[0],
            "Cacatac visual override leaves X unchanged");
        ushort boulderPointer = EnemySpritemapDefinitions.BoulderFrameAt(0x86a9);
        var stockBoulder = DrawEnemy(stock, new FrameReadGuard(rom),
            boulderPointer, RoomEnemySystem.BoulderDefinition);
        var editedBoulder = DrawEnemy(edited, new FrameReadGuard(rom),
            boulderPointer, RoomEnemySystem.BoulderDefinition);
        AssertEqual(unchecked((byte)(stockBoulder.LowTable[1] + 1)),
            editedBoulder.LowTable[1],
            "authored Boulder Y offset changes live room OAM");
        AssertEqual(stockBoulder.LowTable[0], editedBoulder.LowTable[0],
            "Boulder visual override leaves X unchanged");
        ushort atomicPointer = EnemySpritemapDefinitions.AtomicFrameAt(0xe312);
        var stockAtomic = DrawEnemy(stock, new FrameReadGuard(rom),
            atomicPointer, RoomEnemySystem.AtomicDefinition);
        var editedAtomic = DrawEnemy(edited, new FrameReadGuard(rom),
            atomicPointer, RoomEnemySystem.AtomicDefinition);
        AssertEqual(unchecked((byte)(stockAtomic.LowTable[1] + 1)),
            editedAtomic.LowTable[1],
            "authored Atomic Y offset changes live room OAM");
        AssertEqual(stockAtomic.LowTable[0], editedAtomic.LowTable[0],
            "Atomic visual override leaves X unchanged");
        ushort skulteraPointer = EnemySpritemapDefinitions.SkulteraFrameAt(0x902e);
        var stockSkultera = DrawEnemy(stock, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        var editedSkultera = DrawEnemy(edited, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        AssertEqual(unchecked((byte)(stockSkultera.LowTable[1] + 1)),
            editedSkultera.LowTable[1],
            "authored Skultera Y offset changes live room OAM");
        AssertEqual(stockSkultera.LowTable[0], editedSkultera.LowTable[0],
            "Skultera visual override leaves X unchanged");
        ushort waverPointer = EnemySpritemapDefinitions.WaverFrameAt(0x86a9);
        var stockWaver = DrawEnemy(stock, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        var editedWaver = DrawEnemy(edited, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        AssertEqual(unchecked((byte)(stockWaver.LowTable[1] + 1)),
            editedWaver.LowTable[1],
            "authored Waver Y offset changes live room OAM");
        AssertEqual(stockWaver.LowTable[0], editedWaver.LowTable[0],
            "Waver visual override leaves X unchanged");
        foreach ((ushort pointer, ushort definition, string name) in new[]
                 {
                     (EnemySpritemapDefinitions.ZoaFrameAt(0xb3c5),
                         RoomEnemySystem.ZoaDefinition, "Zoa"),
                     (EnemySpritemapDefinitions.SkreeMetareeFrameAt(true, 0x8912),
                         RoomEnemySystem.MetareeDefinition, "Metaree"),
                     (EnemySpritemapDefinitions.SkreeMetareeFrameAt(false, 0xc660),
                         RoomEnemySystem.SkreeDefinition, "Skree"),
                 })
        {
            OamBuffer nativeFrame = DrawEnemy(stock, new FrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, new FrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((ushort definition, ushort operand, string name) in new[]
                 {
                     (PipeBugDefinitions.BrinstarEnemyDefinition, (ushort)0x87ad,
                         "Brinstar Pipe Bug"),
                     (PipeBugDefinitions.StrongBrinstarEnemyDefinition, (ushort)0x8a1f,
                         "strong Brinstar Pipe Bug"),
                     (PipeBugDefinitions.NorfairEnemyDefinition, (ushort)0x8ae3,
                         "Norfair Pipe Bug"),
                     (PipeBugDefinitions.YellowEnemyDefinition, (ushort)0x8efe,
                         "yellow Pipe Bug"),
                 })
        {
            ushort pointer = PipeBugVisualDefinitions.FrameAt(definition, operand);
            OamBuffer nativeFrame = DrawEnemy(stock, new FrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, new FrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((ushort definition, ushort operand, string name) in new[]
                 {
                     (RoomEnemySystem.FakeKraidDefinition, (ushort)0x99b0, "Fake Kraid"),
                     (RoomEnemySystem.KraidGoodNailDefinition, (ushort)0x8b0c,
                         "Kraid fingernail"),
                 })
        {
            ushort pointer = KraidVisualDefinitions.FrameAt(definition, operand);
            OamBuffer nativeFrame = DrawEnemy(stock, new FrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, new FrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((ushort definition, ushort operand, string name) in new[]
                 {
                     (RoomEnemySystem.OwtchDefinition, (ushort)0xa3af, "Owtch"),
                     (RoomEnemySystem.StokeDefinition, (ushort)0x8936, "Stoke"),
                 })
        {
            ushort pointer = OwtchStokeVisualDefinitions.FrameAt(definition, operand);
            OamBuffer nativeFrame = DrawEnemy(stock, new FrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, new FrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((ushort definition, ushort operand, string name) in new[]
                 {
                     (RoomEnemySystem.GRipperDefinition, (ushort)0xe19d, "GRipper"),
                     (RoomEnemySystem.Ripper2Definition, (ushort)0xe2e2, "Ripper II"),
                     (RoomEnemySystem.RipperDefinition, (ushort)0xe479, "Ripper"),
                 })
        {
            ushort pointer = RipperVisualDefinitions.FrameAt(definition, operand);
            OamBuffer nativeFrame = DrawEnemy(stock, new FrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, new FrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        OamBuffer stockFrozen = DrawEnemy(stock, new FrameReadGuard(rom),
            RipperInstructionProgramDefinitions.FrozenFacingLeftSpritemap,
            RoomEnemySystem.GRipperDefinition);
        OamBuffer editedFrozen = DrawEnemy(edited, new FrameReadGuard(rom),
            RipperInstructionProgramDefinitions.FrozenFacingLeftSpritemap,
            RoomEnemySystem.GRipperDefinition);
        AssertEqual(unchecked((byte)(stockFrozen.LowTable[1] + 1)),
            editedFrozen.LowTable[1], "authored frozen GRipper Y offset changes live room OAM");
        ushort firefleaPointer = EnemySpritemapDefinitions.FirefleaFrameAt(
            FirefleaInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockFireflea = DrawEnemy(stock, new FrameReadGuard(rom),
            firefleaPointer, RoomEnemySystem.FirefleaDefinition);
        OamBuffer editedFireflea = DrawEnemy(edited, new FrameReadGuard(rom),
            firefleaPointer, RoomEnemySystem.FirefleaDefinition);
        AssertEqual(unchecked((byte)(stockFireflea.LowTable[1] + 1)),
            editedFireflea.LowTable[1],
            "authored Fireflea Y offset changes live room OAM");
        AssertEqual(stockFireflea.LowTable[0], editedFireflea.LowTable[0],
            "Fireflea visual edit leaves X position unchanged");
        ushort magdollitePointer = EnemySpritemapDefinitions.MagdolliteFrameAt(
            MagdolliteInstructionProgramDefinitions.PresentationWordAddress(0));
        OamBuffer stockMagdollite = DrawEnemy(stock, new FrameReadGuard(rom),
            magdollitePointer, RoomEnemySystem.MagdolliteDefinition);
        OamBuffer editedMagdollite = DrawEnemy(edited, new FrameReadGuard(rom),
            magdollitePointer, RoomEnemySystem.MagdolliteDefinition);
        AssertEqual(unchecked((byte)(stockMagdollite.LowTable[1] + 1)),
            editedMagdollite.LowTable[1],
            "authored Magdollite Y offset changes live room OAM");
        AssertEqual(stockMagdollite.LowTable[0], editedMagdollite.LowTable[0],
            "Magdollite visual edit leaves X position unchanged");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Spritemaps!.TryGet(EnemySpritemapDefinitions.BoyonBank, framePointer, out _),
            "enemy composition override survives catalog reload");
        var preCeresBabyFrames = document.Frames
            .Where(pair => !pair.Key.StartsWith("ceres_baby_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreCeresBabyFrameCount,
            preCeresBabyFrames.Count, "pre-Ceres-Baby composition schema frame count");
        var preCeresBabyBindings = document.DisplayFrames!
            .Where(pair => !pair.Key.StartsWith("ceres_baby_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preCeresBabyBindings["ceres_door_right_hold"] =
            "ceres_door_right_transition_0";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreCeresBabyVersion,
                Frames = preCeresBabyFrames,
                DisplayFrames = preCeresBabyBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preCeresBabyUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer priorCeresRemap = DrawEnemy(preCeresBabyUpgraded,
            new FrameReadGuard(rom), 0xfa13,
            CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer);
        OamBuffer priorCeresSelected = DrawEnemy(stock, new FrameReadGuard(rom),
            0xfa3d, CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer);
        AssertTrue(priorCeresSelected.LowTable.SequenceEqual(priorCeresRemap.LowTable),
            "version-fifteen override retains its Ceres door display binding");
        AssertTrue(preCeresBabyUpgraded.Spritemaps!.TryGet(
                CeresBabyInstructionProgramDefinitions.Bank,
                CeresBabyInstructionProgramDefinitions.RoundFrame, out _),
            "version-fifteen override gains stock Ceres Baby artwork");

        var preCeresDoorFrames = preCeresBabyFrames
            .Where(pair => !pair.Key.StartsWith("ceres_door_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreCeresDoorFrameCount,
            preCeresDoorFrames.Count, "pre-Ceres-door composition schema frame count");
        var preCeresDoorBindings = preCeresBabyBindings
            .Where(pair => !pair.Key.StartsWith("ceres_door_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preCeresDoorBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreCeresDoorVersion,
                Frames = preCeresDoorFrames,
                DisplayFrames = preCeresDoorBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preCeresDoorUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer priorRemap = DrawEnemy(preCeresDoorUpgraded, new FrameReadGuard(rom),
            0x88da, RoomEnemySystem.BoyonDefinition);
        OamBuffer priorSelected = DrawEnemy(stock, new FrameReadGuard(rom),
            0x88e1, RoomEnemySystem.BoyonDefinition);
        AssertTrue(priorSelected.LowTable.SequenceEqual(priorRemap.LowTable),
            "version-fourteen override retains its edited display binding");
        OamBuffer priorEditedCacatac = DrawEnemy(preCeresDoorUpgraded,
            new FrameReadGuard(rom), cacatacPointer, RoomEnemySystem.CacatacDefinition);
        AssertTrue(editedCacatac.LowTable.SequenceEqual(priorEditedCacatac.LowTable),
            "version-fourteen override also retains its edited frame parts");
        AssertTrue(preCeresDoorUpgraded.Spritemaps!.TryGet(
                CeresDoorInstructionProgramDefinitions.Bank, 0xfa13, out _),
            "version-fourteen override gains stock Ceres door artwork");
        preCeresDoorBindings.Remove("boyon_idle_0");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreCeresDoorVersion,
                Frames = preCeresDoorFrames,
                DisplayFrames = preCeresDoorBindings,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "version-fourteen override rejects a missing authored display binding");

        var preMagdolliteFrames = preCeresDoorFrames
            .Where(pair => !pair.Key.StartsWith("magdollite_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMagdolliteFrameCount,
            preMagdolliteFrames.Count, "pre-Magdollite composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreMagdolliteVersion,
                Frames = preMagdolliteFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preMagdolliteUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer upgradedMagdollite = DrawEnemy(preMagdolliteUpgraded,
            new FrameReadGuard(rom), magdollitePointer,
            RoomEnemySystem.MagdolliteDefinition);
        AssertTrue(stockMagdollite.LowTable.SequenceEqual(upgradedMagdollite.LowTable),
            "version-twelve override gains stock Magdollite composition");
        OamBuffer retainedFireflea = DrawEnemy(preMagdolliteUpgraded,
            new FrameReadGuard(rom), firefleaPointer,
            RoomEnemySystem.FirefleaDefinition);
        AssertEqual(editedFireflea.LowTable[1], retainedFireflea.LowTable[1],
            "version-twelve override retains edited Fireflea composition");
        var preFirefleaFrames = preMagdolliteFrames
            .Where(pair => !pair.Key.StartsWith("fireflea_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreFirefleaFrameCount,
            preFirefleaFrames.Count, "pre-Fireflea composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreFirefleaVersion,
                Frames = preFirefleaFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preFirefleaUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer upgradedFireflea = DrawEnemy(preFirefleaUpgraded,
            new FrameReadGuard(rom), firefleaPointer, RoomEnemySystem.FirefleaDefinition);
        AssertTrue(stockFireflea.LowTable.SequenceEqual(upgradedFireflea.LowTable),
            "version-eleven override gains stock Fireflea composition");
        OamBuffer retainedRipper = DrawEnemy(preFirefleaUpgraded,
            new FrameReadGuard(rom), 0xe54b, RoomEnemySystem.RipperDefinition);
        OamBuffer editedRipper = DrawEnemy(edited,
            new FrameReadGuard(rom), 0xe54b, RoomEnemySystem.RipperDefinition);
        AssertEqual(editedRipper.LowTable[1], retainedRipper.LowTable[1],
            "version-eleven override retains edited Ripper composition");
        var preRipperFrames = preFirefleaFrames
            .Where(pair => !pair.Key.StartsWith("ripper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRipperFrameCount,
            preRipperFrames.Count, "pre-Ripper composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreRipperVersion,
                Frames = preRipperFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preRipperUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer upgradedRipper = DrawEnemy(preRipperUpgraded, new FrameReadGuard(rom),
            0xe54b, RoomEnemySystem.RipperDefinition);
        OamBuffer stockRipper = DrawEnemy(stock, new FrameReadGuard(rom),
            0xe54b, RoomEnemySystem.RipperDefinition);
        AssertTrue(stockRipper.LowTable.SequenceEqual(upgradedRipper.LowTable),
            "version-ten override gains stock Ripper composition");
        OamBuffer retainedStoke = DrawEnemy(preRipperUpgraded,
            new FrameReadGuard(rom), 0x8aca, RoomEnemySystem.StokeDefinition);
        OamBuffer editedStoke = DrawEnemy(edited, new FrameReadGuard(rom),
            0x8aca, RoomEnemySystem.StokeDefinition);
        AssertEqual(editedStoke.LowTable[1], retainedStoke.LowTable[1],
            "version-ten override retains edited Stoke composition");
        var preOwtchStokeFrames = preRipperFrames
            .Where(pair => !pair.Key.StartsWith("owtch_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("stoke_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreOwtchStokeFrameCount,
            preOwtchStokeFrames.Count, "pre-Owtch/Stoke composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreOwtchStokeVersion,
                Frames = preOwtchStokeFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog preOwtchStokeUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort owtchPointer = OwtchStokeVisualDefinitions.FrameAt(
            RoomEnemySystem.OwtchDefinition, 0xa3af);
        OamBuffer stockOwtch = DrawEnemy(stock, new FrameReadGuard(rom),
            owtchPointer, RoomEnemySystem.OwtchDefinition);
        OamBuffer upgradedOwtch = DrawEnemy(preOwtchStokeUpgraded,
            new FrameReadGuard(rom), owtchPointer, RoomEnemySystem.OwtchDefinition);
        AssertTrue(stockOwtch.LowTable.SequenceEqual(upgradedOwtch.LowTable),
            "version-nine override gains stock Owtch composition");
        var previousFrames = preOwtchStokeFrames
            .Where(pair => !pair.Key.StartsWith("fake_kraid_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("kraid_nail_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreviousFrameCount,
            previousFrames.Count, "previous enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreviousVersion,
                Frames = previousFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort nailPointer = KraidVisualDefinitions.InitialNailFrame;
        OamBuffer stockNail = DrawEnemy(stock, new FrameReadGuard(rom),
            nailPointer, RoomEnemySystem.KraidGoodNailDefinition);
        OamBuffer upgradedNail = DrawEnemy(upgraded, new FrameReadGuard(rom),
            nailPointer, RoomEnemySystem.KraidGoodNailDefinition);
        AssertTrue(stockNail.LowTable.SequenceEqual(upgradedNail.LowTable),
            "previous-version override gains stock Kraid fingernail composition");
        OamBuffer retainedWaver = DrawEnemy(upgraded, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        AssertEqual(editedWaver.LowTable[1], retainedWaver.LowTable[1],
            "previous-version override retains edited Waver composition");
        ushort retainedPipePointer = PipeBugVisualDefinitions.FrameAt(
            PipeBugDefinitions.NorfairEnemyDefinition, 0x8ae3);
        OamBuffer editedPipe = DrawEnemy(edited, new FrameReadGuard(rom),
            retainedPipePointer, PipeBugDefinitions.NorfairEnemyDefinition);
        OamBuffer retainedPipe = DrawEnemy(upgraded, new FrameReadGuard(rom),
            retainedPipePointer, PipeBugDefinitions.NorfairEnemyDefinition);
        AssertEqual(editedPipe.LowTable[1], retainedPipe.LowTable[1],
            "previous-version override retains edited Pipe Bug composition");
        var priorFrames = previousFrames
            .Where(pair => !pair.Key.StartsWith("pipe_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PriorFrameCount,
            priorFrames.Count, "prior enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PriorVersion,
                Frames = priorFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog priorUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort pipePointer = PipeBugVisualDefinitions.FrameAt(
            PipeBugDefinitions.NorfairEnemyDefinition, 0x8ae3);
        OamBuffer stockPipe = DrawEnemy(stock, new FrameReadGuard(rom),
            pipePointer, PipeBugDefinitions.NorfairEnemyDefinition);
        OamBuffer upgradedPipe = DrawEnemy(priorUpgraded, new FrameReadGuard(rom),
            pipePointer, PipeBugDefinitions.NorfairEnemyDefinition);
        AssertTrue(stockPipe.LowTable.SequenceEqual(upgradedPipe.LowTable),
            "prior-version override gains stock Pipe Bug composition");
        var earlierFrames = priorFrames
            .Where(pair => !pair.Key.StartsWith("zoa_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("metaree_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("skree_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.EarlierFrameCount,
            earlierFrames.Count, "earlier enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.EarlierVersion,
                Frames = earlierFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog earlierUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        var upgradedBoyon = DrawEnemy(earlierUpgraded, new FrameReadGuard(rom),
            framePointer, RoomEnemySystem.BoyonDefinition);
        var upgradedSkultera = DrawEnemy(earlierUpgraded, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        AssertEqual(editedOam.LowTable[0], upgradedBoyon.LowTable[0],
            "previous-version override retains edited Boyon composition");
        AssertEqual(editedSkultera.LowTable[1], upgradedSkultera.LowTable[1],
            "previous-version override retains edited Skultera composition");
        var upgradedWaver = DrawEnemy(earlierUpgraded, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        AssertEqual(editedWaver.LowTable[1], upgradedWaver.LowTable[1],
            "previous-version override retains edited Waver composition");
        ushort zoaPointer = EnemySpritemapDefinitions.ZoaFrameAt(0xb3c5);
        var stockZoa = DrawEnemy(stock, new FrameReadGuard(rom),
            zoaPointer, RoomEnemySystem.ZoaDefinition);
        var upgradedZoa = DrawEnemy(earlierUpgraded, new FrameReadGuard(rom),
            zoaPointer, RoomEnemySystem.ZoaDefinition);
        AssertTrue(stockZoa.LowTable.SequenceEqual(upgradedZoa.LowTable),
            "previous-version override gains stock Zoa composition");
        var intermediateFrames = earlierFrames
            .Where(pair => !pair.Key.StartsWith("waver_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.IntermediateFrameCount,
            intermediateFrames.Count, "intermediate enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.IntermediateVersion,
                Frames = intermediateFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog intermediateUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        var intermediateBoyon = DrawEnemy(intermediateUpgraded, new FrameReadGuard(rom),
            framePointer, RoomEnemySystem.BoyonDefinition);
        var intermediateWaver = DrawEnemy(intermediateUpgraded, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        AssertEqual(editedOam.LowTable[0], intermediateBoyon.LowTable[0],
            "intermediate override retains edited Boyon composition");
        AssertTrue(stockWaver.LowTable.SequenceEqual(intermediateWaver.LowTable),
            "intermediate override gains stock Waver composition");
        var legacyFrames = intermediateFrames
            .Where(pair => !pair.Key.StartsWith("skultera_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.LegacyFrameCount,
            legacyFrames.Count, "legacy enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.LegacyVersion,
                Frames = legacyFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog legacyUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        var legacyBoyon = DrawEnemy(legacyUpgraded, new FrameReadGuard(rom),
            framePointer, RoomEnemySystem.BoyonDefinition);
        var legacySkultera = DrawEnemy(legacyUpgraded, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        AssertEqual(editedOam.LowTable[0], legacyBoyon.LowTable[0],
            "legacy override retains edited Boyon composition");
        AssertTrue(stockSkultera.LowTable.SequenceEqual(legacySkultera.LowTable),
            "legacy override gains stock Skultera composition");
        var preBindings = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.PreDisplayBindingsVersion,
            Frames = preCeresDoorFrames,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            preBindings, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedBindings = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertEqual(editedOam.LowTable[0],
            DrawEnemy(upgradedBindings, new FrameReadGuard(rom), framePointer,
                RoomEnemySystem.BoyonDefinition).LowTable[0],
            "version-thirteen art override retains edits with stock display bindings");

        // A binding may change only the presentation frame. The native pointer,
        // instruction timer, and AI timer must remain untouched by drawing.
        EnemySpritemapDocument remapped = JsonSerializer.Deserialize<EnemySpritemapDocument>(
            original, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        remapped.DisplayFrames!["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog swappedArt = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer nativeFirst = DrawEnemy(stock, new FrameReadGuard(rom), 0x88da,
            RoomEnemySystem.BoyonDefinition);
        OamBuffer nativeSecond = DrawEnemy(stock, new FrameReadGuard(rom), 0x88e1,
            RoomEnemySystem.BoyonDefinition);
        OamBuffer displayedSecond = DrawEnemy(swappedArt, new FrameReadGuard(rom),
            0x88da, RoomEnemySystem.BoyonDefinition, slot =>
            {
                AssertEqual((ushort)0x88da, slot.SpritemapPointer,
                    "display override retains native collision frame pointer");
                AssertEqual((ushort)7, slot.InstructionTimer,
                    "display override does not move an instruction frame boundary");
                AssertEqual((ushort)9, slot.Timer,
                    "display override does not move an enemy AI timer");
            });
        AssertTrue(!nativeFirst.LowTable.SequenceEqual(nativeSecond.LowTable),
            "selected Boyon test frames are visually distinct");
        AssertTrue(nativeSecond.LowTable.SequenceEqual(displayedSecond.LowTable) &&
                   nativeSecond.HighTable.SequenceEqual(displayedSecond.HighTable),
            "authored frame binding changes live OAM without a ROM visual read");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Spritemaps!.TryGetDisplay(EnemySpritemapDefinitions.BoyonBank,
                    0x88da, out _),
            "display override survives a catalog reload");
        remapped.DisplayFrames["boyon_idle_0"] = "magdollite_left_idle_0";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "cross-bank display binding fails loudly");
        remapped.DisplayFrames.Remove("boyon_idle_0");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "missing display binding fails loudly");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1,\"frames\":{}}");
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "duplicate enemy composition keys fail loudly");
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "malformed enemy composition override fails loudly");

        static OamBuffer DrawEnemy(EnemyTileArtworkCatalog art,
            ISnesAddressSpace guard, ushort pointer, ushort definition,
            Action<RoomEnemySlot>? inspect = null)
        {
            var enemies = new RoomEnemySystem { TileArtwork = art };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var queues = (List<ushort>[])typeof(RoomEnemySystem)
                .GetField("_drawQueues", flags)!.GetValue(enemies)!;
            queues[0].Add(0);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = definition;
            slot.Definition = default(RoomEnemyDefinition) with
                { Bank = definition == RoomEnemySystem.FakeKraidDefinition
                    ? EnemySpritemapDefinitions.FakeKraidBank
                    : definition is RoomEnemySystem.KraidGoodNailDefinition or
                        RoomEnemySystem.KraidBadNailDefinition
                        ? EnemySpritemapDefinitions.KraidNailBank
                    : definition is PipeBugDefinitions.BrinstarEnemyDefinition or
                    PipeBugDefinitions.StrongBrinstarEnemyDefinition or
                    PipeBugDefinitions.NorfairEnemyDefinition or
                    PipeBugDefinitions.YellowEnemyDefinition
                    ? EnemySpritemapDefinitions.PipeBugBank
                    : definition == CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer
                        ? CeresDoorInstructionProgramDefinitions.Bank
                    : definition == RoomEnemySystem.BoulderDefinition
                    ? EnemySpritemapDefinitions.BoulderBank
                    : definition == RoomEnemySystem.AtomicDefinition
                        ? EnemySpritemapDefinitions.AtomicBank
                    : definition == RoomEnemySystem.MagdolliteDefinition
                        ? EnemySpritemapDefinitions.MagdolliteBank
                        : definition == RoomEnemySystem.SkulteraDefinition ||
                          definition == RoomEnemySystem.WaverDefinition ||
                          definition == RoomEnemySystem.FirefleaDefinition ||
                          definition == RoomEnemySystem.ZoaDefinition ||
                          definition == RoomEnemySystem.MetareeDefinition ||
                          definition == RoomEnemySystem.SkreeDefinition
                            ? EnemySpritemapDefinitions.SkulteraBank
                        : EnemySpritemapDefinitions.BoyonBank };
            slot.SpritemapPointer = pointer;
            slot.InstructionTimer = 7;
            slot.Timer = 9;
            slot.XPosition = 0x0040;
            slot.YPosition = 0x0080;
            var oam = new OamBuffer();
            enemies.DrawLayers(oam, 0, 0, 0, 0);
            inspect?.Invoke(slot);
            return oam;
        }
    }

    private sealed class FrameReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0xa288da and < 0xa2890b or
                >= 0xa2a0bb and < 0xa2a377 or
                >= 0xa68a59 and < 0xa68b09 or
                >= 0xa8e489 and < 0xa8e587 or
                >= 0xa3881e and < 0xa388f0 or
                >= 0xa38b65 and < 0xa38c0f or
                >= 0xa3b55f and < 0xa3b5b3 or
                >= 0xa3c842 and < 0xa3c8a6 or
                >= 0xa3928a and < 0xa394aa or
                >= 0xb389b7 and < 0xb389fd or
                >= 0xb38a6d and < 0xb38ac1 or
                >= 0xb38e96 and < 0xb38edc or
                >= 0xb392ad and < 0xb39301 or
                >= 0xa69c64 and < 0xa6a0e0 or
                >= 0xa7a617 and < 0xa7a69f or
                >= 0xa2a589 and < 0xa2a59e or
                >= 0xa28aca and < 0xa28b60 or
                >= 0xa2e3c5 and < 0xa2e457 or
                >= 0xa2e527 and < 0xa2e56f or
                >= 0xa38ea5 and < 0xa3900a or
                >= 0xa8b448 and < 0xa8b65e or
                >= 0xa6f921 and < 0xa6fb72 or
                >= 0xa6a329 and < 0xa6a353 or
                >= 0xa6bffd and < 0xa6c04e)
                throw new InvalidOperationException(
                    $"Installed enemy draw read native visual byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
