using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledCrocomireTongueVisualSelectors(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo selector = typeof(RoomEnemySystem).GetMethod(
            "ReadEnemyVisualSelector", flags)!;
        var denied = new CrocomireTongueNoReadBus();
        var enemies = new RoomEnemySystem { TileArtwork = stock };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, denied);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.CrocomireTongueDefinition;
        slot.Definition = default(RoomEnemyDefinition) with
        { Bank = CrocomireTongueCollisionDefinitions.Bank };
        var seen = new HashSet<ushort>();
        for (int index = 0;
             index < CrocomireTongueInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = CrocomireTongueInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = (ushort)(rom.ReadByte(0xa40000 | operand) |
                rom.ReadByte(0xa40000 | unchecked((ushort)(operand + 1))) << 8);
            ushort selected = (ushort)selector.Invoke(enemies, [slot, operand])!;
            AssertEqual(native, selected,
                $"Crocomire tongue selector $A4:{operand:X4} matches native");
            AssertTrue(stock.ExtendedFrames!.TryGetDisplay(
                    CrocomireTongueCollisionDefinitions.Bank, selected, out _),
                $"Crocomire tongue selector $A4:{operand:X4} has installed art");
            seen.Add(selected);
        }
        AssertTrue(seen.SetEquals(CrocomireTongueCollisionDefinitions.FramePointers.ToArray()),
            "Crocomire tongue's nine operands select the nine extracted frames");
        AssertEqual(0, denied.ReadAttempts,
            "installed Crocomire tongue selectors never read ROM bytes");
        Console.WriteLine("Installed Crocomire tongue: nine native visual selectors and " +
            "editable frames, no ROM reads.");
    }

    private static void VerifyCrocomireTongueCollisionDefinitions()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = new CrocomireTongueNoReadBus();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        var native = new RoomEnemySystem();
        var compiled = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(native, rom);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(compiled, denied);
        RoomEnemySlot nativeSlot = native.Slots[0];
        RoomEnemySlot compiledSlot = compiled.Slots[0];
        nativeSlot.Definition = compiledSlot.Definition =
            default(RoomEnemyDefinition) with
            { Bank = CrocomireTongueCollisionDefinitions.Bank };
        nativeSlot.EnemyDefinitionPointer = 0xffff;
        compiledSlot.EnemyDefinitionPointer = RoomEnemySystem.CrocomireTongueDefinition;
        AssertEqual(9, CrocomireTongueCollisionDefinitions.FramePointers.Length,
            "all selected Crocomire tongue frames have compiled collision");

        var seenLists = new HashSet<ushort>();
        int probes = 0;
        foreach (ushort frame in CrocomireTongueCollisionDefinitions.FramePointers)
        {
            CrocomireTongueCollisionComponent component =
                CrocomireTongueCollisionDefinitions.ComponentAt(frame);
            AssertEqual((ushort)1, ReadWord(0xa40000 | frame),
                $"Crocomire tongue $A4:{frame:X4} native one-component header");
            AssertEqual(unchecked((short)ReadWord(0xa40000 | frame + 2)), component.X,
                "Crocomire tongue native component X");
            AssertEqual(unchecked((short)ReadWord(0xa40000 | frame + 4)), component.Y,
                "Crocomire tongue native component Y");
            AssertEqual(ReadWord(0xa40000 | frame + 8), component.HitboxPointer,
                "Crocomire tongue native hitbox-list pointer");
            seenLists.Add(component.HitboxPointer);
            nativeSlot.SpritemapPointer = compiledSlot.SpritemapPointer = frame;
            foreach ((ushort x, ushort y) in new (ushort, ushort)[]
                     {
                         (0x0100, 0x0100), (0x0004, 0x0006),
                         (0xfffc, 0xfffa),
                     })
            for (int shot = 0; shot <= 1; shot++)
            {
                nativeSlot.XPosition = compiledSlot.XPosition = x;
                nativeSlot.YPosition = compiledSlot.YPosition = y;
                object?[] nativeArguments =
                    [nativeSlot, x, y, (ushort)0, (ushort)0, shot != 0, (ushort)0];
                object?[] compiledArguments =
                    [compiledSlot, x, y, (ushort)0, (ushort)0, shot != 0, (ushort)0];
                bool nativeHit = (bool)walker.Invoke(native, nativeArguments)!;
                bool compiledHit = (bool)walker.Invoke(compiled, compiledArguments)!;
                AssertEqual(nativeHit, compiledHit,
                    $"Crocomire tongue $A4:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                AssertEqual((ushort)nativeArguments[^1]!,
                    (ushort)compiledArguments[^1]!,
                    "Crocomire tongue native/compiled callback parity");
                AssertTrue(!compiledHit,
                    "empty native Crocomire tongue hitbox list cannot hit Samus or shots");
                probes++;
            }
        }
        AssertTrue(seenLists.SetEquals(new ushort[] { 0xcbb3, 0xcc3b }),
            "Crocomire tongue selected frames refer to exactly two native hitbox lists");
        foreach (ushort list in seenLists)
        {
            AssertEqual((ushort)0, ReadWord(0xa40000 | list),
                $"Crocomire tongue $A4:{list:X4} native hitbox list is empty");
            AssertEqual(0, CrocomireTongueCollisionDefinitions.HitboxCountAt(list),
                "Crocomire tongue compiled hitbox list is empty");
        }
        AssertThrows<InvalidDataException>(
            () => CrocomireTongueCollisionDefinitions.ComponentAt(0x8000),
            "unknown Crocomire tongue frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => CrocomireTongueCollisionDefinitions.HitboxCountAt(0x8000),
            "unknown Crocomire tongue hitbox list fails loudly");
        AssertEqual(0, denied.ReadAttempts,
            "installed Crocomire tongue collision never reads ROM bytes");
        Console.WriteLine($"Crocomire tongue collision: nine frames, two empty lists, " +
            $"{probes} native-equivalent touch/shot probes; installed path reads no ROM.");

        ushort ReadWord(int address) => (ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }

    private sealed class CrocomireTongueNoReadBus : ISnesAddressSpace
    {
        internal int ReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            ReadAttempts++;
            throw new InvalidOperationException(
                $"Installed Crocomire tongue read ROM byte ${address:X6}.");
        }

        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Installed Crocomire tongue wrote ROM byte ${address:X6}.");
    }
}
