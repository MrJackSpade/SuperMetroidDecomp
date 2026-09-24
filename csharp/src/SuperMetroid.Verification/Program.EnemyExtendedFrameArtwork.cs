using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledEnemyExtendedFrames(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.ExtendedFrames is not null,
            "installed enemy catalog contains extended visual compositions");
        var guard = new ExtendedVisualReadGuard(rom);
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            guard.BlockFrame(frame.Bank, frame.Pointer);
            AssertTrue(stock.ExtendedFrames!.TryGet(frame.Bank, frame.Pointer, out _),
                $"installed extended frame {frame.Name} exists");
        }
        ushort emptyPointer = EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap;
        guard.BlockFrame(EnemyExtendedFrameDefinitions.Bank, emptyPointer);
        AssertTrue(stock.ExtendedFrames!.TryGet(EnemyExtendedFrameDefinitions.Bank,
                emptyPointer, out ReadOnlyMemory<EnemyExtendedDrawComponent> empty) &&
                   empty.IsEmpty,
            "walking Pirate initial empty frame is a compiled draw identity");
        OamBuffer nativeEmpty = DrawExtended(null, rom, emptyPointer,
            0x0040, 0x0080);
        OamBuffer installedEmpty = DrawExtended(stock, guard, emptyPointer,
            0x0040, 0x0080);
        AssertTrue(nativeEmpty.LowTable.SequenceEqual(installedEmpty.LowTable) &&
                   nativeEmpty.HighTable.SequenceEqual(installedEmpty.HighTable) &&
                   nativeEmpty.NextByteOffset == installedEmpty.NextByteOffset,
            "walking Pirate common empty frame draws without ROM reads");
        AssertEqual(EnemyExtendedFrameDefinitions.ExpectedFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Length,
            "walking Pirate distinct extended-frame count");
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            foreach ((ushort x, ushort y) in new (ushort, ushort)[]
                     {
                         (0x0040, 0x0080),
                         (0x01f8, 0x00fc),
                         (0x0000, 0x0000),
                     })
            {
                OamBuffer native = DrawExtended(null, rom, frame.Pointer, x, y);
                OamBuffer installed = DrawExtended(stock, guard,
                    frame.Pointer, x, y);
                AssertTrue(native.LowTable.SequenceEqual(installed.LowTable) &&
                           native.HighTable.SequenceEqual(installed.HighTable) &&
                           native.NextByteOffset == installed.NextByteOffset,
                    $"installed extended {frame.Name} matches native OAM at {x:X4},{y:X4}");
            }
        }

        string fileName = EnemyExtendedFrameDefinitions.FileName;
        string stockPath = Path.Combine(stockDirectory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "stock extended enemy compositions are manifest-hash checked");
        File.WriteAllBytes(stockPath, original);

        EnemyExtendedFrameDocument document =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string editedName = "walking_pirate_walk_left_0";
        EnemyExtendedVisualComponent first = document.Frames[editedName][0];
        document.Frames[editedName][0] = first with
        {
            OffsetX = first.OffsetX + 1,
        };
        string overrideDirectory = Path.Combine(stockDirectory,
            "extended-composition-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort editedPointer = EnemyExtendedFrameDefinitions.Frames.ToArray()
            .Single(frame => frame.Name == editedName).Pointer;
        OamBuffer stockOam = DrawExtended(stock, guard,
            editedPointer, 0x0040, 0x0080);
        OamBuffer editedOam = DrawExtended(edited, guard,
            editedPointer, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockOam.LowTable[0] + 1)),
            editedOam.LowTable[0],
            "editable walking Pirate component moves live OAM by one pixel");
        AssertEqual(stockOam.LowTable[1], editedOam.LowTable[1],
            "walking Pirate X edit does not move visual Y");
        AssertEqual(GetTouchCallback(stock, rom, editedPointer),
            GetTouchCallback(edited, rom, editedPointer),
            "editable walking Pirate component does not move the native hitbox");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .ExtendedFrames!.TryGet(EnemyExtendedFrameDefinitions.Bank,
                    editedPointer, out _),
            "extended composition override survives catalog reload");

        document.Frames.Remove(editedName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "missing extended frame fails loudly");
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "malformed extended composition override fails loudly");

        Console.WriteLine(
            "Walking Pirate extended art: 37 frames/75 components match native OAM " +
            "at three origins with visual ROM reads forbidden; edits, hitbox isolation, " +
            "reload, stock hash and invalid-resource checks pass.");
    }

    private static OamBuffer DrawExtended(EnemyTileArtworkCatalog? art,
        ISnesAddressSpace bus, ushort pointer, ushort x, ushort y)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = art };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        var queues = (List<ushort>[])typeof(RoomEnemySystem)
            .GetField("_drawQueues", flags)!.GetValue(enemies)!;
        queues[0].Add(0);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.GreyWalkingSpacePirateDefinition;
        slot.Definition = default(RoomEnemyDefinition) with
        {
            Bank = EnemyExtendedFrameDefinitions.Bank,
        };
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        slot.SpritemapPointer = pointer;
        slot.XPosition = x;
        slot.YPosition = y;
        var oam = new OamBuffer();
        enemies.DrawLayers(oam, 0, 0, 0, 0);
        return oam;
    }

    private static ushort GetTouchCallback(EnemyTileArtworkCatalog art,
        ISnesAddressSpace bus, ushort pointer)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = art };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.Definition = default(RoomEnemyDefinition) with
        {
            Bank = EnemyExtendedFrameDefinitions.Bank,
        };
        slot.SpritemapPointer = pointer;
        slot.XPosition = 0x0040;
        slot.YPosition = 0x0080;
        MethodInfo collision = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        object?[] arguments =
            [slot, (ushort)0x003b, (ushort)0x0078, (ushort)0, (ushort)0,
                false, (ushort)0];
        AssertTrue((bool)collision.Invoke(enemies, arguments)!,
            "walking Pirate stock hitbox contains its sampled point");
        return (ushort)arguments[6]!;
    }

    private sealed class ExtendedVisualReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        private readonly HashSet<int> blocked = [];

        internal void BlockFrame(byte bank, ushort pointer)
        {
            int root = (bank << 16) | pointer;
            int count = source.ReadByte(root);
            Block(bank, pointer, 2 + count * 8);
            for (int index = 0; index < count; index++)
            {
                ushort component = unchecked((ushort)(pointer + 2 + index * 8));
                ushort sprite = (ushort)(source.ReadByte((bank << 16) |
                        unchecked((ushort)(component + 4))) |
                    source.ReadByte((bank << 16) |
                        unchecked((ushort)(component + 5))) << 8);
                int spriteAddress = (bank << 16) | sprite;
                int parts = source.ReadByte(spriteAddress) |
                    source.ReadByte((bank << 16) |
                        unchecked((ushort)(sprite + 1))) << 8;
                Block(bank, sprite, 2 + parts * 5);
            }
        }

        private void Block(byte bank, ushort pointer, int length)
        {
            for (int index = 0; index < length; index++)
                blocked.Add((bank << 16) | unchecked((ushort)(pointer + index)));
        }

        public byte ReadByte(int address)
        {
            if (blocked.Contains(address))
                throw new InvalidOperationException(
                    $"Installed extended enemy draw read visual ROM byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
