using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCrocomireCorpseCollision()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new CrocomireTongueNoReadBus());
        var slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.CrocomireDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa4 };
        slot.SpritemapPointer = 0xe6bc;
        var walker = typeof(RoomEnemySystem).GetMethod("TryFindExtendedHitboxCallback", flags)!;
        object?[] arguments = [slot, (ushort)0, (ushort)0, (ushort)8, (ushort)8, false, (ushort)0];
        AssertTrue(!(bool)walker.Invoke(enemies, arguments)!, "river corpse has native empty collision");
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int address) => (ushort)(rom.ReadByte(0xa40000 | address) | rom.ReadByte(0xa40000 | (address + 1)) << 8);
        foreach (var frame in CrocomireSkeletonVisualDefinitions.Frames)
        {
            var components = CrocomireBodyCollisionDefinitions.ComponentsAt(frame.Pointer);
            AssertEqual((int)Word(frame.Pointer), components.Length, "corpse native component count");
            slot.SpritemapPointer = frame.Pointer;
            slot.XPosition = slot.YPosition = 4096;
            bool anyHitbox = false;
            for (int index = 0; index < components.Length; index++)
            {
                var component = components[index];
                int address = frame.Pointer + 2 + 8 * index;
                AssertEqual(unchecked((short)Word(address)), component.X, "corpse native component X");
                AssertEqual(unchecked((short)Word(address + 2)), component.Y, "corpse native component Y");
                AssertEqual(Word(address + 6), component.HitboxPointer, "corpse native hitbox pointer");
                var boxes = CrocomireBodyCollisionDefinitions.HitboxesAt(component.HitboxPointer);
                AssertEqual((int)Word(component.HitboxPointer), boxes.Length, "corpse native hitbox count");
                for (int boxIndex = 0; boxIndex < boxes.Length; boxIndex++)
                {
                    var box = boxes[boxIndex];
                    int boxAddress = component.HitboxPointer + 2 + 12 * boxIndex;
                    ushort[] words = [unchecked((ushort)box.Left), unchecked((ushort)box.Top), unchecked((ushort)box.Right), unchecked((ushort)box.Bottom), box.TouchAi, box.ShotAi];
                    for (int word = 0; word < words.Length; word++)
                        AssertEqual(Word(boxAddress + word * 2), words[word], "corpse native rectangle/callback");
                }
                if (!anyHitbox && boxes.Length != 0)
                {
                    var box = boxes[0];
                    ushort x = (ushort)(4096 + component.X + (box.Left + box.Right) / 2);
                    ushort y = (ushort)(4096 + component.Y + (box.Top + box.Bottom) / 2);
                    foreach (bool shot in new[] { false, true })
                    {
                        object?[] hit = [slot, x, y, (ushort)0, (ushort)0, shot, (ushort)0];
                        AssertTrue((bool)walker.Invoke(enemies, hit)!, "corpse physical component is visited");
                        AssertEqual(shot ? box.ShotAi : box.TouchAi, (ushort)hit[^1]!, "corpse selects native callback");
                    }
                }
                anyHitbox |= boxes.Length != 0;
            }
            if (!anyHitbox)
            {
                object?[] empty = [slot, slot.XPosition, slot.YPosition, (ushort)100, (ushort)100, true, (ushort)0];
                AssertTrue(!(bool)walker.Invoke(enemies, empty)!, "native empty corpse geometry remains non-colliding");
            }
        }
        Console.WriteLine("Crocomire reported E6BC corpse frame resolves empty collision without cartridge reads.");
        Console.WriteLine("All 33 corpse frames match native offsets, rectangles and callbacks; production collision uses no ROM.");
    }

    private static void VerifyCrocomireCutsceneCamera()
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] storage = Enumerable.Range(0, RoomScrollGrid.StorageByteCount).Select(index => rom.ReadByte(0x8fa9d7 + index)).ToArray();
        var scrolls = RoomScrollGrid.LoadCompiled(rom, storage, 8, 1);
        var camera = new ScrollBoundaryCamera(scrolls);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(),
                crocomireColors: CrocomireColorCatalog.Load(new MemoryStream(CrocomireColorExtractor.Extract(rom)))),
        };
        var body = enemies.Slots[0];
        body.XPosition = 1480;
        body.YPosition = 144;
        var state = new CrocomireEnemyState(body);
        typeof(RoomEnemySystem).GetField("_crocomire", flags)!.SetValue(enemies, state);
        typeof(RoomEnemySystem).GetField("_crocomireDeath", flags)!.SetValue(enemies, new CrocomireDeathState());
        typeof(RoomEnemySystem).GetField("_setRoomScrollState", flags)!.SetValue(enemies, (Action<int, RoomScrollState>)scrolls.SetStorage);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new CrocomireTongueNoReadBus());
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => 1));
        var run = typeof(RoomEnemySystem).GetMethod("RunCrocomireMain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState?, ushort, RoomLevelData?, ushort>>(enemies);
        var samus = new SamusState { XPosition = 1312, YPosition = 128 };
        run(body, samus, 0, null, 1280);
        camera.SetPosition(1280, 0);
        camera.MoveLeft(1);
        AssertEqual((ushort)1280, camera.XPosition, "melting cutscene camera cannot show left of screen five");
        AssertEqual(RoomScrollState.RedBoundary, scrolls.ReadNativeState(4), "bridge threshold locks screen four");
        AssertEqual(RoomScrollState.Blue, scrolls.ReadNativeState(5), "screen five remains visible");
        samus.XPosition = 1311;
        run(body, samus, 0, null, 1280);
        camera.SetPosition(1280, 0);
        camera.MoveLeft(1);
        AssertEqual((ushort)1279, camera.XPosition, "before threshold camera remains free to move left");
        samus.XPosition = 1312;
        body.XPosition = 1600;
        run(body, samus, 0, null, 1280);
        AssertEqual(CrocomireDeathPhases.CrumbleBridgeAndSink, state.DeathSequenceIndex, "real bridge collapse begins");
        camera.SetPosition(1280, 0);
        camera.MoveLeft(1);
        AssertEqual((ushort)1280, camera.XPosition, "collapse frame retains the native camera lock");
        Console.WriteLine("Crocomire bridge threshold preserves native left camera boundary at X=1280.");
    }
}
