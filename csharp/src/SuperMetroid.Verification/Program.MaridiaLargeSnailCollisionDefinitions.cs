using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledOumVisualSelectors(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo selector = typeof(RoomEnemySystem).GetMethod(
            "ReadEnemyVisualSelector", flags)!;
        var denied = new OumNoReadBus();
        var enemies = new RoomEnemySystem { TileArtwork = stock };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, denied);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.MaridiaLargeSnailDefinition;
        slot.Definition = default(RoomEnemyDefinition) with
        { Bank = MaridiaLargeSnailCollisionDefinitions.Bank };
        var seen = new HashSet<ushort>();
        for (int index = 0;
             index < MaridiaLargeSnailInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = MaridiaLargeSnailInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = (ushort)(rom.ReadByte(0xa20000 | operand) |
                rom.ReadByte(0xa20000 | unchecked((ushort)(operand + 1))) << 8);
            ushort selected = (ushort)selector.Invoke(enemies, [slot, operand])!;
            AssertEqual(native, selected,
                $"Oum visual selector $A2:{operand:X4} matches native");
            AssertTrue(stock.ExtendedFrames!.TryGetDisplay(
                    MaridiaLargeSnailCollisionDefinitions.Bank, selected, out _),
                $"Oum visual selector $A2:{operand:X4} has installed art");
            seen.Add(selected);
        }
        AssertTrue(seen.SetEquals(MaridiaLargeSnailCollisionDefinitions.FramePointers.ToArray()),
            "Oum's 60 operands select exactly the 30 extracted physical frames");
        AssertEqual(0, denied.ReadAttempts,
            "installed Oum selector path makes no ROM reads");
        Console.WriteLine("Installed Oum: 60 native visual selectors cover 30 editable " +
            "frames with no ROM reads.");
    }

    private static void VerifyMaridiaLargeSnailCollisionDefinitions()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = new OumNoReadBus();
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
            { Bank = MaridiaLargeSnailCollisionDefinitions.Bank };
        nativeSlot.EnemyDefinitionPointer = 0xffff;
        compiledSlot.EnemyDefinitionPointer = RoomEnemySystem.MaridiaLargeSnailDefinition;
        AssertEqual(30, MaridiaLargeSnailCollisionDefinitions.FramePointers.Length,
            "all Oum extended frames have compiled collision");
        AssertEqual(30, MaridiaLargeSnailCollisionDefinitions.HitboxPointers.Count(),
            "all Oum hitbox lists have compiled collision");

        var seenLists = new HashSet<ushort>();
        int boxesChecked = 0;
        int probes = 0;
        foreach (ushort frame in MaridiaLargeSnailCollisionDefinitions.FramePointers)
        {
            AssertEqual((ushort)1, ReadWord(0xa20000 | frame),
                $"Oum $A2:{frame:X4} native one-component header");
            AssertEqual((ushort)0, ReadWord(0xa20000 | frame + 2),
                "Oum native component X is zero");
            AssertEqual((ushort)0, ReadWord(0xa20000 | frame + 4),
                "Oum native component Y is zero");
            ushort list = MaridiaLargeSnailCollisionDefinitions.HitboxListAt(frame);
            AssertEqual(ReadWord(0xa20000 | frame + 8), list,
                "Oum native frame hitbox-list pointer");
            seenLists.Add(list);
            ReadOnlySpan<MaridiaLargeSnailCollisionHitbox> boxes =
                MaridiaLargeSnailCollisionDefinitions.HitboxesAt(list);
            AssertEqual(ReadWord(0xa20000 | list), (ushort)boxes.Length,
                $"Oum $A2:{list:X4} native hitbox count");
            for (int index = 0; index < boxes.Length; index++)
            {
                MaridiaLargeSnailCollisionHitbox box = boxes[index];
                int record = 0xa20000 | unchecked((ushort)(list + 2 + index * 12));
                AssertEqual(unchecked((short)ReadWord(record)), box.Left, "Oum left");
                AssertEqual(unchecked((short)ReadWord(record + 2)), box.Top, "Oum top");
                AssertEqual(unchecked((short)ReadWord(record + 4)), box.Right, "Oum right");
                AssertEqual(unchecked((short)ReadWord(record + 6)), box.Bottom, "Oum bottom");
                AssertEqual(ReadWord(record + 8), box.TouchAi, "Oum touch callback");
                AssertEqual(ReadWord(record + 10), box.ShotAi, "Oum shot callback");
                boxesChecked++;
            }

            nativeSlot.SpritemapPointer = compiledSlot.SpritemapPointer = frame;
            foreach ((ushort originX, ushort originY) in
                     new (ushort, ushort)[]
                     {
                         (0x0100, 0x0100), (0x0004, 0x0006), (0xfffc, 0xfffa),
                     })
            {
                nativeSlot.XPosition = compiledSlot.XPosition = originX;
                nativeSlot.YPosition = compiledSlot.YPosition = originY;
                foreach (MaridiaLargeSnailCollisionHitbox box in boxes)
                {
                    ushort[] xs = BoundaryPoints(originX, box.Left, box.Right);
                    ushort[] ys = BoundaryPoints(originY, box.Top, box.Bottom);
                    foreach (ushort x in xs)
                    foreach (ushort y in ys)
                    for (int shot = 0; shot <= 1; shot++)
                    {
                        object?[] nativeArguments =
                            [nativeSlot, x, y, (ushort)0, (ushort)0, shot != 0, (ushort)0];
                        object?[] compiledArguments =
                            [compiledSlot, x, y, (ushort)0, (ushort)0, shot != 0, (ushort)0];
                        bool nativeHit = (bool)walker.Invoke(native, nativeArguments)!;
                        bool compiledHit = (bool)walker.Invoke(compiled, compiledArguments)!;
                        AssertEqual(nativeHit, compiledHit,
                            $"Oum $A2:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                        AssertEqual((ushort)nativeArguments[^1]!,
                            (ushort)compiledArguments[^1]!,
                            $"Oum $A2:{frame:X4} callback {x:X4},{y:X4}, shot={shot}");
                        probes++;
                    }
                }
            }
        }
        AssertTrue(seenLists.SetEquals(MaridiaLargeSnailCollisionDefinitions.HitboxPointers),
            "every compiled Oum list belongs to a selected frame");
        AssertEqual(66, boxesChecked, "all Oum physical rectangles were verified");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailCollisionDefinitions.HitboxListAt(0x8000),
            "unknown Oum frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailCollisionDefinitions.HitboxesAt(0x8000),
            "unknown Oum hitbox list fails loudly");
        AssertEqual(0, denied.ReadAttempts,
            "installed Oum touch and shot walkers never read ROM bytes");
        Console.WriteLine($"Oum collision: 30 frames, 66 rectangles, {probes} " +
            "native-equivalent touch/shot boundary probes with no ROM reads.");

        ushort ReadWord(int address) => (ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        static ushort[] BoundaryPoints(ushort origin, short low, short high) =>
        [
            unchecked((ushort)(origin + low - 1)),
            unchecked((ushort)(origin + low)),
            unchecked((ushort)(origin + low + 1)),
            unchecked((ushort)(origin + high - 1)),
            unchecked((ushort)(origin + high)),
            unchecked((ushort)(origin + high + 1)),
        ];
    }

    private sealed class OumNoReadBus : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            ReadAttempts++;
            throw new InvalidOperationException(
                $"Installed Oum frame or collision read ROM byte ${address:X6}.");
        }

        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Installed Oum frame or collision wrote ROM byte ${address:X6}.");
    }
}
