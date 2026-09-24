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
            "walking/wall Pirate distinct extended-frame count");
        AssertEqual(EnemyExtendedFrameDefinitions.WallFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("wall_pirate_", StringComparison.Ordinal)),
            "all wall-Pirate visual frame identities are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.NinjaFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("ninja_pirate_", StringComparison.Ordinal)),
            "all ninja-Pirate visual frame identities are installed");
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
        AssertEqual(GetTouchCallback(stock, guard, editedPointer),
            GetTouchCallback(edited, guard, editedPointer),
            "editable walking Pirate component does not move the compiled hitbox");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .ExtendedFrames!.TryGet(EnemyExtendedFrameDefinitions.Bank,
                    editedPointer, out _),
            "extended composition override survives catalog reload");

        // Existing v1 overrides were authored before wall-Pirate frames were
        // extracted. Preserve their validated walking edits and fill only the
        // new identities from the hash-checked current stock catalog.
        var legacyDocument = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.FirstVersion,
            Frames = document.Frames.Where(entry => entry.Key.StartsWith(
                "walking_pirate_", StringComparison.Ordinal)).ToDictionary(
                    entry => entry.Key, entry => entry.Value,
                    StringComparer.Ordinal),
        };
        byte[] legacyJson = JsonSerializer.SerializeToUtf8Bytes(legacyDocument,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyExtendedFrameCatalog.Load(
                new MemoryStream(legacyJson, writable: false)),
            "legacy extended override needs complete verified stock to merge");
        File.WriteAllBytes(overridePath, legacyJson);
        EnemyTileArtworkCatalog legacy = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer legacyWalking = DrawExtended(legacy, guard,
            editedPointer, 0x0040, 0x0080);
        AssertTrue(legacyWalking.LowTable.SequenceEqual(editedOam.LowTable) &&
                   legacyWalking.HighTable.SequenceEqual(editedOam.HighTable),
            "v1 walking-Pirate artwork edit survives the v2 catalog update");

        const string wallName = "wall_pirate_climb_left_0";
        ushort wallPointer = EnemyExtendedFrameDefinitions.Frames.ToArray()
            .Single(frame => frame.Name == wallName).Pointer;
        OamBuffer stockWall = DrawExtended(stock, guard,
            wallPointer, 0x0040, 0x0080);
        OamBuffer legacyWall = DrawExtended(legacy, guard,
            wallPointer, 0x0040, 0x0080);
        AssertTrue(stockWall.LowTable.SequenceEqual(legacyWall.LowTable) &&
                   stockWall.HighTable.SequenceEqual(legacyWall.HighTable),
            "v1 override inherits stock wall-Pirate frames");

        EnemyExtendedVisualComponent wallFirst = document.Frames[wallName][0];
        document.Frames[wallName][0] = wallFirst with
        {
            OffsetX = wallFirst.OffsetX + 1,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        EnemyTileArtworkCatalog editedWall = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer movedWall = DrawExtended(editedWall, guard,
            wallPointer, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockWall.LowTable[0] + 1)),
            movedWall.LowTable[0],
            "editable wall-Pirate component moves live OAM by one pixel");
        AssertEqual(stockWall.LowTable[1], movedWall.LowTable[1],
            "wall-Pirate X edit does not move visual Y");
        AssertEqual(GetTouchCallback(stock, guard, wallPointer),
            GetTouchCallback(editedWall, guard, wallPointer),
            "editable wall-Pirate component does not move its compiled hitbox");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .ExtendedFrames!.TryGet(EnemyExtendedFrameDefinitions.Bank,
                    wallPointer, out _),
            "edited wall-Pirate composition survives catalog reload");

        // The second published schema includes edited walking and wall frames,
        // but no Ninja identities. It must inherit only those new frames from
        // current stock without dropping either family's prior edit.
        var versionTwoDocument = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreviousVersion,
            Frames = document.Frames.Where(entry => !entry.Key.StartsWith(
                "ninja_pirate_", StringComparison.Ordinal)).ToDictionary(
                    entry => entry.Key, entry => entry.Value,
                    StringComparer.Ordinal),
        };
        byte[] versionTwoJson = JsonSerializer.SerializeToUtf8Bytes(versionTwoDocument,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyExtendedFrameCatalog.Load(
                new MemoryStream(versionTwoJson, writable: false)),
            "v2 extended override requires complete verified v3 stock");
        File.WriteAllBytes(overridePath, versionTwoJson);
        EnemyTileArtworkCatalog versionTwo = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer versionTwoWalking = DrawExtended(versionTwo, guard,
            editedPointer, 0x0040, 0x0080);
        OamBuffer versionTwoWall = DrawExtended(versionTwo, guard,
            wallPointer, 0x0040, 0x0080);
        AssertTrue(versionTwoWalking.LowTable.SequenceEqual(editedOam.LowTable) &&
                   versionTwoWall.LowTable.SequenceEqual(movedWall.LowTable),
            "v2 override preserves both walking and wall-Pirate art edits");

        EnemyExtendedFrameDefinition ninjaFrame = EnemyExtendedFrameDefinitions
            .Frames.ToArray().First(frame => frame.Name.StartsWith(
                "ninja_pirate_", StringComparison.Ordinal));
        string ninjaName = ninjaFrame.Name;
        ushort ninjaPointer = ninjaFrame.Pointer;
        OamBuffer stockNinja = DrawExtended(stock, guard,
            ninjaPointer, 0x0040, 0x0080);
        OamBuffer versionTwoNinja = DrawExtended(versionTwo, guard,
            ninjaPointer, 0x0040, 0x0080);
        OamBuffer versionOneNinja = DrawExtended(legacy, guard,
            ninjaPointer, 0x0040, 0x0080);
        AssertTrue(stockNinja.LowTable.SequenceEqual(versionTwoNinja.LowTable) &&
                   stockNinja.HighTable.SequenceEqual(versionTwoNinja.HighTable) &&
                   stockNinja.LowTable.SequenceEqual(versionOneNinja.LowTable) &&
                   stockNinja.HighTable.SequenceEqual(versionOneNinja.HighTable),
            "v1 and v2 overrides inherit verified stock Ninja frames");

        EnemyExtendedVisualComponent ninjaFirst = document.Frames[ninjaName][0];
        document.Frames[ninjaName][0] = ninjaFirst with
        {
            OffsetX = ninjaFirst.OffsetX + 1,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        EnemyTileArtworkCatalog editedNinja = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer movedNinja = DrawExtended(editedNinja, guard,
            ninjaPointer, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockNinja.LowTable[0] + 1)),
            movedNinja.LowTable[0],
            "editable Ninja component moves live OAM by one pixel");
        AssertEqual(stockNinja.LowTable[1], movedNinja.LowTable[1],
            "Ninja X edit does not move visual Y");
        AssertEqual(GetTouchCallback(stock, guard, ninjaPointer),
            GetTouchCallback(editedNinja, guard, ninjaPointer),
            "editable Ninja component does not move its compiled hitbox");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .ExtendedFrames!.TryGet(EnemyExtendedFrameDefinitions.Bank,
                    ninjaPointer, out _),
            "edited Ninja composition survives catalog reload");

        EnemyExtendedVisualComponent[] editedNinjaComponents = document.Frames[ninjaName];
        document.Frames.Remove(ninjaName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "v3 override missing a Ninja frame fails loudly");
        document.Frames.Add(ninjaName, editedNinjaComponents);

        EnemyExtendedVisualComponent[] editedWallComponents = document.Frames[wallName];
        document.Frames.Remove(wallName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "v3 override missing a wall-Pirate frame fails loudly");
        document.Frames.Add(wallName, editedWallComponents);

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
            "Space Pirate extended art: 131 walking/wall/ninja frames match native OAM " +
            "at three origins with visual ROM reads forbidden; three-family edits, " +
            "compiled-hitbox isolation, v1/v2 override migration, reload, stock hash " +
            "and invalid-resource checks pass.");
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
        slot.EnemyDefinitionPointer = PirateDefinitionForFrame(pointer);
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
        slot.EnemyDefinitionPointer = PirateDefinitionForFrame(pointer);
        slot.Definition = default(RoomEnemyDefinition) with
        {
            Bank = EnemyExtendedFrameDefinitions.Bank,
        };
        slot.SpritemapPointer = pointer;
        slot.XPosition = 0x0040;
        slot.YPosition = 0x0080;
        SpacePirateCollisionComponent component =
            SpacePirateCollisionDefinitions.ComponentsAt(pointer)[0];
        SpacePirateCollisionHitbox hitbox =
            SpacePirateCollisionDefinitions.HitboxesAt(component.HitboxPointer)[0];
        ushort sampleX = unchecked((ushort)(slot.XPosition + component.X +
            (hitbox.Left + hitbox.Right) / 2));
        ushort sampleY = unchecked((ushort)(slot.YPosition + component.Y +
            (hitbox.Top + hitbox.Bottom) / 2));
        MethodInfo collision = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        object?[] arguments =
            [slot, sampleX, sampleY, (ushort)0, (ushort)0,
                false, (ushort)0];
        AssertTrue((bool)collision.Invoke(enemies, arguments)!,
            "Space Pirate stock hitbox contains its sampled point");
        return (ushort)arguments[6]!;
    }

    private static ushort PirateDefinitionForFrame(ushort pointer)
    {
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            if (frame.Pointer != pointer)
                continue;
            if (frame.Name.StartsWith("ninja_pirate_", StringComparison.Ordinal))
                return RoomEnemySystem.GreyNinjaSpacePirateDefinition;
            if (frame.Name.StartsWith("wall_pirate_", StringComparison.Ordinal))
                return RoomEnemySystem.GreyWallSpacePirateDefinition;
            return RoomEnemySystem.GreyWalkingSpacePirateDefinition;
        }
        if (pointer == EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap)
            return RoomEnemySystem.GreyWalkingSpacePirateDefinition;
        throw new InvalidDataException(
            $"Extended Space Pirate frame $B2:{pointer:X4} is not installed.");
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
