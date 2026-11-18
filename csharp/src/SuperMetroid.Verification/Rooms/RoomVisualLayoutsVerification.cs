using SuperMetroid.AssetExtraction;
using SuperMetroid.Tooling;
using System.Buffers.Binary;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Checks the installed room terrain and appearances against the cartridge's level words, decoded by the import's codec.</summary>
internal static class RoomVisualLayoutsVerification
{
    /// <summary>
    /// Checks every installed level's terrain, behaviours, background and terrain beyond the room, and every stock
    /// appearance, against the cartridge's level words decoded by the import's codec. The installed levels are the shared
    /// installation's catalogs, which the room loads below read too, so they are read once (#1680).
    /// </summary>
    internal static void VerifyRoomVisualLayouts(GameInstallation installed,
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        RoomLevelArtworks stock = InstalledFixtures.RoomAssets.VisualLayouts;
        AssertEqual(245, RoomVisualLayoutFiles.RetailLevels.Count,
            "distinct pinned room levels");
        InstalledRoomLevelStreams levelStreams = InstalledFixtures.RoomAssets.LevelStreams;
        AssertEqual(RoomVisualLayoutFiles.RetailLevels.Count, levelStreams.Count,
            "installed room-level count");
        VerifyRoomLevelStreamRejections(installed);
        try
        {
            _ = levelStreams.Get(new RoomLevelId("UnknownLevel"));
            throw new InvalidOperationException("Unknown installed room level was accepted.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains("lacks level", StringComparison.Ordinal),
                "unknown installed room level fails without ROM fallback");
        }
        // Each level's cartridge words are decompressed once, for its own check and for every state that loads it.
        var decompressed = new Dictionary<int, byte[]>();
        byte[] NativeLevel(int source)
        {
            if (!decompressed.TryGetValue(source, out byte[]? native))
                decompressed[source] = native = RomDataReader.Decompress(bus, source);
            return native;
        }
        foreach ((RoomLevelId level, int width) in RoomVisualLayoutFiles.RetailLevels)
        {
            RoomLevelArtwork layout = stock.Get(level);
            InstalledRoomLevel terrain = levelStreams.Get(level);
            byte[] native = NativeLevel(RoomLevelSourceDefinitions.Addresses[level]);
            int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(native);
            int count = layerBytes / 2;
            int backgroundOffset = 2 + layerBytes + count;
            int backgroundBytes = Math.Min(layerBytes, native.Length - backgroundOffset);
            AssertEqual(width, layout.Size.X, $"room level {level} visual stride");
            AssertEqual(count / width, layout.Size.Y,
                $"room level {level} visual allocation height");
            AssertEqual(layout.Size, terrain.Size, $"room level {level} terrain grid size");
            string levelName = $"room level {level}";
            for (int index = 0; index < count; index++)
            {
                ushort foreground = BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(2 + index * 2));
                RoomBlockAppearance background = RoomLevelWordCodec.DecodeAppearance(index * 2 < backgroundBytes
                    ? BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(backgroundOffset + index * 2))
                    : (ushort)0);
                AssertBlockEqual(RoomLevelWordCodec.Decode(foreground), terrain.Foreground[index], levelName, "installed terrain", index);
                AssertInstalledBehavior(terrain.Foreground[index].Collision, native[2 + layerBytes + index], terrain.Behaviors[index],
                    RoomVisualLayoutFiles.RetailLevelAreas[level], levelName, index);
                AssertBlockEqual(background, terrain.Background[index], levelName, "installed background", index);
                AssertBlockEqual(RoomLevelWordCodec.DecodeAppearance(foreground), layout.Foreground.Span[index],
                    levelName, "foreground appearance", index);
                AssertBlockEqual(background, layout.Background.Span[index], levelName, "background appearance", index);
            }

            // Past the grid: every level word the decompressed level reaches, the last possibly half the fill,
            // then the fill to the end of the cleared level space; the fill decodes to the game's edge-solid block.
            int payloadWords = (native.Length - 2 + 1) / 2;
            AssertEqual(Math.Max(0, payloadWords - count), terrain.Following.Length,
                $"room level {level} data following the grid");
            AssertEqual(Math.Max(LevelSpaceWords, payloadWords) - count, terrain.BeyondCount,
                $"room level {level} terrain beyond the grid");
            for (int index = 0; index < terrain.Following.Length; index++)
            {
                int offset = 2 + (count + index) * 2;
                ushort word = offset + 1 < native.Length
                    ? BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(offset))
                    : (ushort)(native[offset] | (FillWord & 0xff00));
                AssertEqual(RoomLevelWordCodec.Decode(word), terrain.Following[index],
                    $"room level {level} block {count + index} past the grid");
            }
            AssertEqual(RoomTerrainBlocks.EdgeSolid.Collision, RoomLevelWordCodec.Decode(FillWord).Collision,
                "the level-space fill collides as the game's edge-solid block");
        }

        // Exercise the real room loader for every selectable state, not merely
        // representative addresses. Each state's compressed level source is
        // forbidden, so an accidental ROM fallback fails at its first read.
        int stateCount = 0;
        int slopeBlocks = 0;
        int spikeBlocks = 0;
        foreach (RoomDefinition header in RetailRooms.All)
        {
            SelectedRoom room = SelectedRoom.LoadUsingCompiledSelection(header, new RoomStateSelectionContext(new PersistentProgress().Events, DefeatedBosses.None, false, false));
            ushort headerPointer = header.Pointer;
            foreach (ushort statePointer in RoomStateSelection.GetStatePointers(header))
            {
                string stateName = $"room ${headerPointer:X4} state ${statePointer:X4}";
                room = room with { State = RoomStateDefinitions.Get(statePointer) };
                int source = RoomLevelSourceDefinitions.Addresses[room.State.Level];
                byte[] native = NativeLevel(source);
                int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(native);
                int blockCount = layerBytes / 2;
                int bg2Offset = 2 + layerBytes + blockCount;
                int availableBg2 = Math.Min(layerBytes, native.Length - bg2Offset);
                RoomLevelData loaded = LoadFixtureRoomAssets(room).LevelData;
                AssertEqual(blockCount, loaded.Size.X * loaded.Size.Y, $"{stateName} physical allocation");
                // The plane's shown blocks are built once per state, not once per block read.
                RoomBlockAppearance[] shownBackground = loaded.BackgroundPlane.Blocks;
                for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
                {
                    RoomCollisionBlock block = loaded.GetCollisionBlockByIndex(blockIndex);
                    ushort nativeWord = BinaryPrimitives.ReadUInt16LittleEndian(
                        native.AsSpan(2 + blockIndex * 2));
                    byte nativeBts = native[2 + layerBytes + blockIndex];
                    ushort nativeBg2Word = blockIndex * 2 < availableBg2
                        ? BinaryPrimitives.ReadUInt16LittleEndian(
                            native.AsSpan(bg2Offset + blockIndex * 2))
                        : FillWord;
                    AssertBlockEqual(RoomLevelWordCodec.Decode(nativeWord), block.Terrain, stateName, "terrain", blockIndex);
                    if (block.CollisionType == RoomCollisionType.DoorBlock)
                        AssertDoorBlock(loaded, block, header, nativeBts, stateName, blockIndex);
                    else
                        AssertLoadedBehavior(block, nativeBts, header.Area, stateName, blockIndex);
                    AssertBlockEqual(RoomLevelWordCodec.DecodeAppearance(nativeBg2Word), shownBackground[blockIndex],
                        stateName, "background", blockIndex);
                    if (block.CollisionType == RoomCollisionType.Slope) slopeBlocks++;
                    if (block.CollisionType is RoomCollisionType.SpikeAir or
                        RoomCollisionType.SpikeBlock) spikeBlocks++;
                }
                stateCount++;
            }
        }
        AssertEqual(RoomStateDefinitions.RetailStateCount, stateCount,
            "all retail room-state loads use the guarded installed level streams");
        AssertTrue(slopeBlocks > 0 && spikeBlocks > 0,
            "guarded installed-stream audit includes physical slope and spike-hazard blocks");
        Console.WriteLine($"  Room layouts: {RoomVisualLayoutFiles.RetailLevels.Count} levels decode to the installed terrain and " +
            $"{stateCount} guarded state loads keep it (including slopes and spikes), and the installed appearances are the native ones.");
    }

    /// <summary>
    /// Asserts block <paramref name="index"/>'s <paramref name="property"/> in <paramref name="where"/> is
    /// <paramref name="expected"/>, building the failure's context only for a mismatch, since every block of every level
    /// and state is compared.
    /// </summary>
    private static void AssertBlockEqual<T>(T expected, T actual, string where, string property, int index)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            AssertEqual(expected, actual, $"{where} {property} {index}");
    }

    /// <summary>
    /// Asserts a level block's installed behaviour against its native byte: a door block holds none, its door being its
    /// room's own (#2090); a reacting block reacts as its byte does in every area of the rooms using the level, holding the
    /// first behaviour of its reaction (#1805); any other block holds its byte's behaviour, unresolved.
    /// </summary>
    private static void AssertInstalledBehavior(RoomCollisionType kind, byte native, RoomBlockBehavior installed,
        IReadOnlyList<AreaId> areas, string where, int index)
    {
        if (kind == RoomCollisionType.DoorBlock)
        {
            AssertBlockEqual(RoomBlockBehavior.None, installed, where, "installed door behaviour", index);
            return;
        }
        if (BlockReactionFamilies.Of(kind) is not { } family)
        {
            AssertBlockEqual(RoomBlockBehaviorCodec.Decode(native), installed, where, "installed behaviour", index);
            return;
        }
        foreach (AreaId area in areas)
            AssertBlockEqual(RoomBlockBehaviorCodec.Decode(area, native).ReactionIn(family), installed.In(area).ReactionIn(family),
                where, $"installed {area} reaction", index);
    }

    /// <summary>
    /// Asserts a loaded block's behaviour, which its room's area resolves, against its native byte: a reacting block reacts
    /// as its byte does in the area, and any other block holds its byte's behaviour in the area.
    /// </summary>
    private static void AssertLoadedBehavior(RoomCollisionBlock block, byte native, AreaId area, string where, int index)
    {
        if (BlockReactionFamilies.Of(block.CollisionType) is { } family)
            AssertBlockEqual(RoomBlockBehaviorCodec.Decode(area, native).ReactionIn(family), block.Behavior.ReactionIn(family),
                where, "reaction", index);
        else
            AssertBlockEqual(RoomBlockBehaviorCodec.Decode(area, native), block.Behavior, where, "behaviour", index);
    }

    /// <summary>
    /// Asserts a door block holds no behaviour and leads where its native byte's position in its room's door list does
    /// (#2090); a position past the list leads nowhere, as no door of the room is there.
    /// </summary>
    private static void AssertDoorBlock(RoomLevelData loaded, RoomCollisionBlock block, RoomDefinition room, byte nativeBts,
        string where, int index)
    {
        AssertBlockEqual(RoomBlockBehavior.None, block.Behavior, where, "door behaviour", index);
        IReadOnlyList<Core.Rooms.Doors.DoorBlockExit> list = RoomDoorListDefinitions.ByRoom[room];
        int link = RoomBlockBehaviorCodec.DoorLinkOf(nativeBts);
        if (link < list.Count)
            AssertBlockEqual(list[link], loaded.ResolveDoorCollision(block, SamusCollisionPose.StandingNormal, publishDoorSideEffects: false),
                where, "door exit", index);
        else
            AssertThrows<InvalidDataException>(
                () => loaded.ResolveDoorCollision(block, SamusCollisionPose.StandingNormal, publishDoorSideEffects: false),
                $"{where} door block {index} past its room's door list leads nowhere");
    }

    /// <summary>The cartridge's level-space fill word, which the import decodes as the game's edge-solid block.</summary>
    private const ushort FillWord = 0x8000;

    /// <summary>The level words the cartridge fills before decompressing a room.</summary>
    private const int LevelSpaceWords = 0x3200;

    /// <summary>
    /// Confirms the stock-only level-stream loader rejects each corrupted installation with a
    /// path-specific error: a resized stream, a missing file and an unsupported collision-kind
    /// document version; and that the catalog rejects door blocks that leave out a room's use of a level, the completeness
    /// the resource audit's closed contract for <see cref="InstalledRoomLevelStreams.DoorBlocks"/> relies on (#2435).
    /// </summary>
    private static void VerifyRoomLevelStreamRejections(GameInstallation installed)
    {
        using var scratch = new TestScratchDirectory("room-level-stream-rejections");
        string firstName = RoomLevelStreamFiles.LevelFileName(
            RoomVisualLayoutFiles.RetailLevels.Keys.OrderBy(candidate => candidate.Name, StringComparer.Ordinal).First());

        void Expect(string label, Func<string, string> expectedText, Action<string> corrupt)
        {
            string directory = Path.Combine(scratch.Root, label);
            Directory.CreateDirectory(directory);
            // The copy is the installation's files, which the shared catalogs loaded, so only the corruption can reject it.
            foreach (string file in Directory.GetFiles(installed.RoomLevelStreamDirectory))
                File.Copy(file, Path.Combine(directory, Path.GetFileName(file)));
            corrupt(directory);
            try
            {
                _ = RoomLevelStreamFiles.Load(directory);
                throw new InvalidOperationException($"Room-level streams accepted a {label} installation.");
            }
            catch (InvalidDataException error)
            {
                AssertTrue(error.Message.Contains(expectedText(directory), StringComparison.Ordinal),
                    $"room-level stream {label} is rejected naming {expectedText(directory)}");
            }
        }

        static void EditKinds(string directory, Action<JsonObject> edit)
        {
            string path = Path.Combine(directory, RoomLevelStreamFiles.CollisionKindsFileName);
            JsonObject kinds = RequiredJson.Parse(File.ReadAllText(path)).AsObject();
            edit(kinds);
            File.WriteAllText(path, kinds.ToJsonString());
        }

        Expect("length-mismatch", directory => Path.Combine(directory, firstName), directory =>
        {
            using var file = new FileStream(Path.Combine(directory, firstName), FileMode.Append, FileAccess.Write);
            file.WriteByte(0);
        });
        Expect("missing-file", directory => Path.Combine(directory, firstName), directory => File.Delete(Path.Combine(directory, firstName)));
        Expect("wrong-version", directory => Path.Combine(directory, RoomLevelStreamFiles.CollisionKindsFileName),
            directory => EditKinds(directory, kinds => kinds["version"] = 1));
        Expect("missing-door-block-use", _ => "Installed door blocks lack", directory =>
        {
            string path = Path.Combine(directory, RoomDoorBlockFiles.FileName);
            JsonNode document = RequiredJson.Parse(File.ReadAllText(path));
            JsonObject room = document.Required("rooms").AsObject().First().Value?.AsObject()
                ?? throw new InvalidOperationException("The installed door blocks' first room names no levels.");
            room.Remove(room.First().Key);
            File.WriteAllText(path, document.ToJsonString());
        });
    }

}
