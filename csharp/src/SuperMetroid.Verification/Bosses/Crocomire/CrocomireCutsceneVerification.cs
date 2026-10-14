using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireTongueCollisionDefinitionsVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Crocomire corpse collision data and the two death cutscene camera boundaries.</summary>
internal static class CrocomireCutsceneVerification
{
    /// <summary>Compares all 33 skeleton frames' hitboxes and callbacks with the cartridge, including the empty E6BC frame.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire corpse collision")]
    internal static void VerifyCrocomireCorpseCollision()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewCrocomireTongueNoReadBus());
        var slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Crocomire;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa4 };
        slot.SpritemapPointer = 0xe6bc;
        object?[] arguments = [slot, Position16<RoomPixels>.Zero, new Extent16<RoomPixels>((ushort)8, (ushort)8), false, (ushort)0];
        AssertTrue(!ExtendedHitboxWalker.Find(enemies, arguments), "river corpse has native empty collision");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (var frame in CrocomireSkeletonVisualDefinitions.Frames)
        {
            var components = CrocomireBodyCollisionDefinitions.ComponentsAt(frame.Pointer);
            AssertEqual((int)rom.ReadCartridgeWordFixedBank(0xa40000 | frame.Pointer), components.Length, "corpse native component count");
            slot.SpritemapPointer = frame.Pointer;
            slot.Position = slot.Position.WithWhole(new(4096, 4096));
            bool anyHitbox = false;
            for (int index = 0; index < components.Length; index++)
            {
                var component = components[index];
                int address = frame.Pointer + 2 + 8 * index;
                AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xa40000 | address)), component.Offset.X, "corpse native component X");
                AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xa40000 | (address + 2))), component.Offset.Y, "corpse native component Y");
                AssertEqual(rom.ReadCartridgeWordFixedBank(0xa40000 | (address + 6)), component.HitboxPointer, "corpse native hitbox pointer");
                var boxes = CrocomireBodyCollisionDefinitions.HitboxesAt(component.HitboxPointer);
                AssertEqual((int)rom.ReadCartridgeWordFixedBank(0xa40000 | component.HitboxPointer), boxes.Length, "corpse native hitbox count");
                for (int boxIndex = 0; boxIndex < boxes.Length; boxIndex++)
                {
                    var box = boxes[boxIndex];
                    int boxAddress = component.HitboxPointer + 2 + 12 * boxIndex;
                    ushort[] words = [unchecked((ushort)box.Left), unchecked((ushort)box.Top), unchecked((ushort)box.Right), unchecked((ushort)box.Bottom), box.TouchAi, box.ShotAi];
                    for (int word = 0; word < words.Length; word++)
                        AssertEqual(rom.ReadCartridgeWordFixedBank(0xa40000 | (boxAddress + word * 2)), words[word], "corpse native rectangle/callback");
                }
                if (!anyHitbox && boxes.Length != 0)
                {
                    var box = boxes[0];
                    ushort x = (ushort)(4096 + component.Offset.X + (box.Left + box.Right) / 2);
                    ushort y = (ushort)(4096 + component.Offset.Y + (box.Top + box.Bottom) / 2);
                    foreach (bool shot in new[] { false, true })
                    {
                        object?[] hit = [slot, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>((ushort)0, (ushort)0), shot, (ushort)0];
                        AssertTrue(ExtendedHitboxWalker.Find(enemies, hit), "corpse physical component is visited");
                        AssertEqual(shot ? box.ShotAi : box.TouchAi, (ushort)(hit[^1] ?? throw new InvalidOperationException("Verification requires hit[^1].")), "corpse selects native callback");
                    }
                }
                anyHitbox |= boxes.Length != 0;
            }
            if (!anyHitbox)
            {
                object?[] empty = [slot, slot.Position.Whole, new Extent16<RoomPixels>((ushort)100, (ushort)100), true, (ushort)0];
                AssertTrue(!ExtendedHitboxWalker.Find(enemies, empty), "native empty corpse geometry remains non-colliding");
            }
        }
        Console.WriteLine("Crocomire reported E6BC corpse frame resolves empty collision without cartridge reads.");
        Console.WriteLine("All 33 corpse frames match native offsets, rectangles and callbacks; production collision uses no ROM.");
    }

    /// <summary>The Crocomire room's authored scroll storage, read from its retail $8F:A9D7 record.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Crocomire room's authored scroll storage")]
    private static byte[] ReadNativeCrocomireRoomScrolls(ISnesAddressSpace rom) =>
        Enumerable.Range(0, RoomScrollGrid.StorageByteCount).Select(index => rom.ReadCartridgeByte(0x8fa9d7 + index)).ToArray();

    /// <summary>Checks the bridge melt lock at X=1280 and the later skeleton-wall lock at X=512.</summary>
    internal static void VerifyCrocomireCutsceneCamera()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] storage = ReadNativeCrocomireRoomScrolls(rom);
        var scrolls = RoomScrollGrid.LoadCompiled(rom, storage, new Extent16<RoomScreens>(8, 1));
        var camera = new ScrollBoundaryCamera(scrolls);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), CrocomireColors = CrocomireColorCatalog.Load(new MemoryStream(CrocomireColorExtractor.Extract(rom))) }),
        };
        var body = enemies.SlotTable.Slots[0];
        body.Position = body.Position.WithWhole(new(1480, 144));
        var state = new CrocomireEnemyState(body);
        enemies.CrocomireBehavior.Crocomire = state;
        enemies.CrocomireBehavior.CrocomireDeath = new CrocomireDeathState();
        enemies.VerificationServices().SetRoomScrollState = (Action<int, RoomScrollState>)scrolls.SetStorage;
        enemies.Presentation._cgram = new SnesCgram();
        enemies.BindFixtureBus(NewCrocomireTongueNoReadBus());
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 1);
        Action<RoomEnemySlot, SamusState?, ushort> run = enemies.CrocomireBehavior.RunCrocomireMain;
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(1312, 128)) };
        run(body, samus, 1280);
        camera.SetPosition(new Position16<RoomPixels>(1280, 0));
        camera.MoveLeft(1);
        AssertEqual((ushort)1280, camera.PixelPosition.X, "melting cutscene camera cannot show left of screen five");
        AssertEqual(RoomScrollState.RedBoundary, scrolls.ReadNativeState(4), "bridge threshold locks screen four");
        AssertEqual(RoomScrollState.Blue, scrolls.ReadNativeState(5), "screen five remains visible");
        samus.Position = samus.Position.WithWholeX(1311);
        run(body, samus, 1280);
        camera.SetPosition(new Position16<RoomPixels>(1280, 0));
        camera.MoveLeft(1);
        AssertEqual((ushort)1279, camera.PixelPosition.X, "before threshold camera remains free to move left");
        samus.Position = samus.Position.WithWholeX(1312);
        body.Position = body.Position.WithWholeX(1600);
        run(body, samus, 1280);
        AssertEqual(CrocomireDeathPhase.CrumbleBridgeAndSink, state.DeathSequenceIndex, "real bridge collapse begins");
        camera.SetPosition(new Position16<RoomPixels>(1280, 0));
        camera.MoveLeft(1);
        AssertEqual((ushort)1280, camera.PixelPosition.X, "collapse frame retains the native camera lock");
        Console.WriteLine("Crocomire bridge threshold preserves native left camera boundary at X=1280.");

        // Revised interpretation: the later skeleton scene on the left. Begin at its
        // native wait-for-Samus handoff using the retail room's original scrolls.
        scrolls = RoomScrollGrid.LoadCompiled(rom, storage, new Extent16<RoomScreens>(8, 1));
        camera = new ScrollBoundaryCamera(scrolls);
        enemies.VerificationServices().SetRoomScrollState = (Action<int, RoomScrollState>)scrolls.SetStorage;
        state.DeathSequenceIndex = CrocomireDeathPhase.WaitForSamusAtWall;
        body.Position = body.Position.WithWholeX(480);
        samus.Position = samus.Position.WithWholeX(639);
        run(body, samus, 544);
        AssertEqual(CrocomireDeathPhase.RumbleHiddenWall, state.DeathSequenceIndex, "left cutscene starts at native Samus threshold");
        AssertEqual(RoomScrollState.RedBoundary, scrolls.ReadNativeState(1), "left cutscene retains red screen one");
        AssertEqual(RoomScrollState.Blue, scrolls.ReadNativeState(2), "left cutscene retains visible screen two");
        AssertEqual(RoomScrollState.RedBoundary, scrolls.ReadNativeState(3), "left cutscene closes screen three");
        var point = FixedPosition<RoomPixels>.FromWords(639, 0, 128, 0);
        // Runtime currently selects zero here; the cartridge writes six. Check
        // whether that known difference can explain the reported wall overrun.
        foreach (CameraDistanceMode distance in new[] { CameraDistanceMode.NormalTracking, CameraDistanceMode.RightEdge })
        {
            var context = new HorizontalCameraContext(0, SamusMovementType.Standing, 0, 4, distance);
            camera.SetPosition(new Position16<RoomPixels>(544, 0));
            for (int frame = 0; frame < 16; frame++)
                camera.TrackMovedSamusHorizontally(point, point, context);
            AssertEqual((ushort)512, camera.PixelPosition.X, "left cutscene autoscroll settles to native screen two");
            camera.TrackMovedSamusHorizontally(point, point.WithX(point.X.WithWhole(638)), context);
            AssertEqual((ushort)512, camera.PixelPosition.X, "moving left cannot expose the far side of the spike wall");
            camera.TrackMovedSamusHorizontally(point, point.WithX(point.X.WithWhole(640)), context);
            AssertEqual((ushort)512, camera.PixelPosition.X, "moving right cannot expose the far side of the holding wall");
            camera.TrackMovedSamusHorizontally(point.WithX(point.X.WithWhole(750)), point.WithX(point.X.WithWhole(751)), context);
            AssertEqual((ushort)512, camera.PixelPosition.X, "rightward target near holding wall stays within screen two");
        }
        state.DeathSequenceIndex = CrocomireDeathPhase.ClearWallAndOpenScrolls;
        run(body, samus, 512);
        camera.MoveLeft(1);
        AssertEqual((ushort)511, camera.PixelPosition.X, "left camera unlocks at the native completion phase");
        Console.WriteLine("Left skeleton cutscene: existing code locks X=512 for both directions and unlocks at completion; reported overrun not reproduced.");
    }

    /// <summary>Runs the Crocomire death cutscene camera boundary checks.</summary>
    [VerificationCommand("--crocomire-cutscene-camera")]
    internal static int CrocomireCutsceneCameraCommand()
    {
        Suite(nameof(VerifyCrocomireCutsceneCamera), () => VerifyCrocomireCutsceneCamera());
        return 0;
    }

    /// <summary>Runs the Crocomire skeleton collision comparison against retail data.</summary>
    [VerificationCommand("--crocomire-corpse-collision")]
    internal static int CrocomireCorpseCollisionCommand()
    {
        Suite(nameof(VerifyCrocomireCorpseCollision), () => VerifyCrocomireCorpseCollision());
        return 0;
    }
}
