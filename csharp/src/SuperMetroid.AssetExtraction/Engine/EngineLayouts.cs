using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Writes the Godot game's room layouts. A layout is a grid of letters, one a tile, with what each letter stands
/// for: nothing, something solid, a slope with its height at each pixel across, or a block that reacts. Each of the
/// old game's levels is written once for each area that uses it and each set of graphics it is drawn with, since a
/// block's reaction depends on the area and its picture on the graphics.
/// </summary>
/// <remarks>
/// The layouts are built first and written last, so that whatever writes a thing that changes its room's blocks
/// during play can give the room's layouts the patch that is that change.
/// </remarks>
internal sealed partial class EngineLayouts
{
    /// <summary>The width and height of a block, in pixels.</summary>
    private const int Block = 16;

    /// <summary>The first letter given to a tile; letters go on from here, one for each different tile of a layout.</summary>
    private const char FirstLetter = 'À';

    private readonly EngineAssetTree _tree;
    private readonly EngineSheets _sheets;
    private readonly InstalledRoomLevelStreams _levels;
    private readonly HashSet<string> _blockTypes = new(StringComparer.Ordinal);
    private readonly SortedSet<string> _graphicsSets = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Layout> _layouts = new(StringComparer.Ordinal);

    /// <summary>A layout as it is being built.</summary>
    private sealed partial class Layout(string tileset, InstalledRoomLevel level, string placed)
    {
        internal string Tileset { get; } = tileset;
        internal InstalledRoomLevel Level { get; } = level;

        /// <summary>What the room's own things put into it, as text: every use of the layout must put the same.</summary>
        internal string Placed { get; } = placed;
        internal JsonObject Legend { get; } = [];
        internal Dictionary<string, char> Letters { get; } = new(StringComparer.Ordinal);
        internal List<string> Rows { get; } = [];
        internal JsonObject Patches { get; } = [];
    }

    /// <summary>Every set of graphics a layout written so far is drawn with, by the old game's name for it.</summary>
    internal IReadOnlyList<string> GraphicsSets => [.. _graphicsSets];

    private readonly string _content;

    /// <summary>The layouts of the installation whose levels are <paramref name="levels"/> and whose files lie under <paramref name="content"/>.</summary>
    internal EngineLayouts(EngineAssetTree tree, EngineSheets sheets, InstalledRoomLevelStreams levels, string content)
    {
        _tree = tree;
        _sheets = sheets;
        _levels = levels;
        _content = content;
    }

    /// <summary>What a heavy spike does in the layout being written; null where the layout is shown both with its area's power and without.</summary>
    private int? _heavySpikesHurt = SpikeHazardDefinitions.HeavySpikeDamage;

    /// <summary>
    /// What a solid heavy spike does in one state of a room. The cartridge's heavy spikes do nothing in an area whose
    /// machinery has no power until its boss is beaten, which is the room's state until the one chosen once that boss
    /// is dead. A room of such an area with no state of its own for the boss's death is shown both ways by the one
    /// layout, which cannot say either: null.
    /// </summary>
    private static int? HeavySpikeDamageIn(RoomDefinition room, RoomStateSettings state)
    {
        if (!AreaIds.Of(room.Area).PoweredFromBossDefeat)
            return SpikeHazardDefinitions.HeavySpikeDamage;
        RoomStateChoice choice = RoomStateSelectionDefinitions.Choices[room];
        RoomStateCondition powered = RoomStateCondition.BossDead(DefeatedBosses.AreaBoss);
        if (!choice.Branches.Any(branch => branch.Condition == powered))
            return null;
        return choice.Branches.Any(branch => branch.Condition == powered && ReferenceEquals(branch.State, state)) ? SpikeHazardDefinitions.HeavySpikeDamage : 0;
    }

    /// <summary>Every state of every room, with its room.</summary>
    internal static IEnumerable<(RoomDefinition Room, RoomStateSettings State)> States()
    {
        foreach ((RoomDefinition room, RoomStateChoice choice) in RoomStateSelectionDefinitions.Choices)
        {
            foreach (RoomStateSettings state in choice.Branches.Select(branch => branch.State).Append(choice.Default))
                yield return (room, state);
        }
    }

    /// <summary>Builds the layout of every state of every room.</summary>
    internal void BuildAll()
    {
        foreach ((RoomDefinition room, RoomStateSettings state) in States())
            Build(room, state);
    }

    /// <summary>Writes every layout built, with the patches it has been given.</summary>
    internal void WriteAll()
    {
        foreach ((string path, Layout layout) in _layouts)
        {
            var rows = new JsonArray();
            foreach (string row in layout.Rows)
                rows.Add(row);
            var data = new JsonObject
            {
                ["tile_size"] = Block,
                // What stands in the room and looks as its graphics do is drawn with this set.
                ["graphics"] = EngineRoomObjects.GraphicsName(layout.Tileset),
                ["tiles"] = layout.Legend,
                ["rows"] = rows,
            };
            // Where nothing of the room is drawn the cartridge shows the first colour of the set's palette.
            if (_sheets.BackdropOf(GameInstallationLayout.RoomBlockSheetDirectoryName, layout.Tileset) is { } backdrop)
                data["backdrop"] = backdrop;
            if (layout.Patches.Count > 0)
                data["patches"] = layout.Patches;
            _tree.WriteData(path, data);
        }
    }

    /// <summary>The letter of a tile in a layout, given the first time the layout has that tile.</summary>
    private static char Letter(Layout layout, JsonObject tile, string path)
    {
        string key = tile.ToJsonString();
        if (!layout.Letters.TryGetValue(key, out char letter))
        {
            letter = (char)(FirstLetter + layout.Letters.Count);
            if (char.IsSurrogate(letter))
                throw new InvalidDataException($"The layout {path} has more different tiles than there are letters for.");
            layout.Letters[key] = letter;
            layout.Legend[letter.ToString()] = tile;
        }

        return letter;
    }

    /// <summary>One block of a patch: where it lies from the patch's origin, and what the block there becomes.</summary>
    /// <param name="Across">How many blocks right of the origin.</param>
    /// <param name="Down">How many blocks below it.</param>
    /// <param name="Collision">What kind of block it becomes.</param>
    /// <param name="Block">The block of the room's graphics it shows; none for one that shows nothing.</param>
    /// <param name="Mirroring">How that block is mirrored.</param>
    /// <param name="Behavior">
    /// What the block does from then on, where the change writes that with its kind; none for a change that gives
    /// the block its kind and leaves what the level says it does.
    /// </param>
    internal readonly partial record struct PatchCell(
        int Across, int Down, RoomCollisionType Collision, int? Block, BlockMirroring Mirroring, RoomBlockBehavior? Behavior = null);

    /// <summary>
    /// Gives the layout of one state of a room a named patch: the blocks a change to the room leaves, over the
    /// rectangle they span. A block of that rectangle the change does not touch stays as the layout has it. The same
    /// patch given again to a layout several states share is the one patch.
    /// </summary>
    /// <exception cref="InvalidDataException">The patch has no blocks, lies outside the level, or is given twice with different blocks.</exception>
    /// <param name="alsoMoving">
    /// An animation of blocks that is going where the patch is applied though the state's own effect does not set it
    /// going: something else of the room starts it, as a door's setup starts a belt.
    /// </param>
    internal void Patch(
        RoomDefinition room, RoomStateSettings state, string name, Position16<RoomBlocks> origin, IEnumerable<PatchCell> cells, BlockAnimation? alsoMoving = null)
    {
        string path = PathOf(room, state);
        Layout layout = _layouts.TryGetValue(path, out Layout? built)
            ? built
            : throw new InvalidDataException($"The layout {path} has not been built, and is given the patch {name}.");
        _moving = alsoMoving is { } more ? [.. MovedIn(state), more] : MovedIn(state);
        _heavySpikesHurt = HeavySpikeDamageIn(room, state);
        // Later blocks of a change lie over earlier ones.
        var last = new Dictionary<(int Across, int Down), PatchCell>();
        foreach (PatchCell cell in cells)
            last[(cell.Across, cell.Down)] = cell;
        if (last.Count == 0)
            throw new InvalidDataException($"The patch {name} of the layout {path} has no blocks.");
        int left = origin.X + last.Keys.Min(at => at.Across), top = origin.Y + last.Keys.Min(at => at.Down);
        int right = origin.X + last.Keys.Max(at => at.Across), bottom = origin.Y + last.Keys.Max(at => at.Down);
        if (left < 0 || top < 0 || right >= layout.Level.Size.X || bottom >= layout.Level.Size.Y)
            throw new InvalidDataException($"The patch {name} of the layout {path} does not lie inside its level.");
        var rows = new JsonArray();
        var row = new StringBuilder(right - left + 1);
        for (int y = top; y <= bottom; y++)
        {
            row.Clear();
            for (int x = left; x <= right; x++)
            {
                if (!last.TryGetValue((x - origin.X, y - origin.Y), out PatchCell cell))
                {
                    row.Append(layout.Rows[y][x]);
                    continue;
                }

                RoomBlockAppearance appearance = cell.Block is int block ? RoomBlockAppearance.Of(block).WithMirroring(cell.Mirroring) : RoomBlockAppearance.Nothing;
                // As in the old game, a change gives a block its kind and its picture and leaves what the level says
                // the block there does, its slope, its reaction, unless it writes that too.
                RoomBlockBehavior behavior = cell.Behavior ?? layout.Level.Behaviors[y * layout.Level.Size.X + x].In(room.Area);
                row.Append(Letter(layout, Tile(new RoomTerrainBlock(appearance, cell.Collision), behavior, underElevator: false, layout.Tileset), path));
            }

            rows.Add(row.ToString());
        }

        var patch = new JsonObject { ["column"] = left, ["row"] = top, ["rows"] = rows };
        if (layout.Patches[name] is { } given)
        {
            if (given.ToJsonString() != patch.ToJsonString())
                throw new InvalidDataException($"The layout {path} is given the patch {name} twice, with different blocks.");
            return;
        }

        layout.Patches[name] = patch;
    }

    /// <summary>
    /// How many updates the block an item is hidden in stays gone once shot: the old game's block crumbling away, the
    /// item showing, and the block coming back over it.
    /// </summary>
    private static int HiddenItemBlockUpdates()
    {
        int Updates(Animation<CollectibleSequence> animation)
        {
            (List<IAnimationBeat<CollectibleImage, CollectiblePhase>> lead, List<IAnimationBeat<CollectibleImage, CollectiblePhase>>? cycle) =
                EngineTimelines.Played(CollectibleSequenceAnimations.Segments, animation);
            if (cycle is not null)
                throw new InvalidDataException("An item's reveal goes round and round, so it has no length.");
            return lead.OfType<AnimationBeat<CollectibleImage, CollectiblePhase>.Frame>().Sum(frame => frame.Duration);
        }

        return Updates(CollectibleSequenceAnimations.ShotBlockReveal) + Updates(CollectibleSequenceAnimations.Revealed);
    }

    /// <summary>
    /// The tiles the things placed in one state of a room put over its level's own, by the cell each lies on: the
    /// block an item is hidden in, the blank air where an item lies in the open or in an orb, and an elevator's
    /// platform.
    /// </summary>
    private Dictionary<int, JsonObject> PlacedTiles(RoomStateSettings state, InstalledRoomLevel level, string tileset, string path)
    {
        var tiles = new Dictionary<int, JsonObject>();
        foreach (RoomObjectPlacement placement in state.ObjectPopulation.Placements.ToArray())
        {
            int cell = placement.Block.Y * level.Size.X + placement.Block.X;
            switch (placement.Setup)
            {
                case CollectibleSetup { Item.Presentation: CollectiblePresentation.ShotBlock }:
                {
                    // The block looks as the level has it there, and any weapon breaks it for as long as the old
                    // game shows the item.
                    // It crumbles away and comes back over the item with the old game's own pictures.
                    var crumbling = new List<int>();
                    var block = new ReactingBlock("hidden_item", new JsonObject
                    {
                        ["solid"] = true,
                        ["broken_by"] = AnyWeapon(),
                        ["regrows_after"] = HiddenItemBlockUpdates(),
                        ["break_sound"] = EngineNames.Sound(
                            SuperMetroid.Core.Audio.EnemySound.BlockCrumble, SuperMetroid.Core.Compatibility.SoundRequestPriority.Normal),

                        // The pictures of its crumbling and of its coming back are solid blocks on the cartridge.
                        ["blocks_while_crumbling"] = true,
                        ["breaking"] = HiddenItemBlockLooks(CollectibleSequence.ShotBlockReveal, crumbling),
                        ["regrowing"] = HiddenItemBlockLooks(CollectibleSequence.ShotBlockConceal, crumbling),
                    }, Pictures: crumbling);
                    foreach (int shared in crumbling)
                    {
                        if (Picture(tileset, shared, BlockMirroring.None) is null)
                            throw new InvalidDataException($"The graphics {tileset} draw nothing for the block {shared}, which the block an item hides in shows.");
                    }

                    var tile = new JsonObject { ["kind"] = "block", ["block"] = BlockType(block) };
                    RoomBlockAppearance shown = level.Foreground[cell].Appearance;
                    if (shown.Shows == RoomBlockShown.Block && Picture(tileset, shown.Block, shown.Mirroring) is { } picture)
                        tile["picture"] = picture;
                    tiles[cell] = tile;
                    break;
                }

                case CollectibleSetup { Item.Presentation: CollectiblePresentation.Exposed or CollectiblePresentation.ChozoOrb }:
                    // An item that lies in the open or in an orb has its own block where the level's was from the
                    // room's loading on, and leaves blank air there once it is opened or taken, or when the room is
                    // come to with it already taken: whatever the level has at that block is never seen nor met.
                    // Where the level has blank air there already nothing is put, so that a state of the room that
                    // places no item keeps the same layout.
                    RoomTerrainBlock own = level.Foreground[cell];
                    bool shows = own.Appearance.Shows == RoomBlockShown.Block && Picture(tileset, own.Appearance.Block, own.Appearance.Mirroring) is not null;
                    if (shows || own.Collision != RoomCollisionType.Air)
                        tiles[cell] = new JsonObject { ["kind"] = "empty" };
                    break;

                case ElevatorPlatformSetup:
                {
                    // The platform an elevator comes to rest on: solid blocks that glow by turns.
                    const string directory = GameInstallationLayout.RoomObjectElevatorPlatformVisualDirectoryName;
                    IReadOnlyList<ElevatorPlatformFrame> cycle = ElevatorPlatformObjectDefinitions.Cycle;
                    int updates = cycle[0].Updates;
                    if (cycle.Any(frame => frame.Updates != updates))
                        throw new InvalidDataException("An elevator platform's pictures do not all show for as long as one another.");
                    var draws = cycle.Select(frame => frame.Draw is RoomObjectFrameDraw<ElevatorPlatformDraw> drawn
                        ? drawn.Frame
                        : throw new InvalidDataException("An elevator platform shows something that is not a frame of its own.")).ToList();
                    foreach (RoomBlockShapeCell part in ElevatorPlatformDraws.Family.Shape(draws[0]).Cells)
                    {
                        if (part.Collision != RoomCollisionType.SolidBlock)
                            throw new InvalidDataException($"An elevator platform has a block that is {part.Collision}, not solid.");
                        var pictures = new JsonArray();
                        foreach (ElevatorPlatformDraw draw in draws)
                        {
                            (int block, BlockMirroring mirroring) = FrameBlock(directory, EngineSheets.FileName(draw.ToString()).Replace('_', '-'), part.Cell.X, part.Cell.Y)
                                ?? throw new InvalidDataException($"The elevator platform's frame {draw} draws nothing at one of its blocks.");
                            pictures.Add(Picture(tileset, block, mirroring)
                                ?? throw new InvalidDataException($"The graphics {tileset} draw nothing for a block of an elevator platform."));
                        }

                        int at = (placement.Block.Y + part.Cell.Y) * level.Size.X + placement.Block.X + part.Cell.X;
                        if (placement.Block.X + part.Cell.X >= level.Size.X || placement.Block.Y + part.Cell.Y >= level.Size.Y)
                            throw new InvalidDataException($"An elevator platform of the layout {path} does not lie inside its level.");
                        tiles[at] = new JsonObject { ["kind"] = "solid", ["pictures"] = pictures, ["updates"] = updates };
                    }

                    break;
                }

                case MotherBrainEscapeRoomGateSetup:
                {
                    // The gate that shuts behind Samus in the first room of the escape. Placed, the old gate is drawn
                    // closed on its first update, and arriving by its door closes it through pictures that are solid
                    // every one: its blocks are solid from the room's start, and are shown closed.
                    const string directory = GameInstallationLayout.RoomObjectEscapeGateVisualDirectoryName;
                    string closed = SuperMetroid.AssetExtraction.Rooms.MotherBrainEscapeGateDrawKeys.Installed.Of(MotherBrainEscapeGateDraw.Closed);
                    foreach (RoomBlockShapeCell part in MotherBrainEscapeGateDraws.Family.Shape(MotherBrainEscapeGateDraw.Closed).Cells)
                    {
                        if (part.Collision != RoomCollisionType.SolidBlock)
                            throw new InvalidDataException($"The escape room's gate has a block that is {part.Collision}, not solid.");
                        int x = placement.Block.X + part.Cell.X, y = placement.Block.Y + part.Cell.Y;
                        if (x >= level.Size.X || y >= level.Size.Y)
                            throw new InvalidDataException($"The escape room's gate of the layout {path} does not lie inside its level.");
                        var shut = new JsonObject { ["kind"] = "solid" };
                        if (FrameBlock(directory, closed, part.Cell.X, part.Cell.Y) is { } drawn && Picture(tileset, drawn.Block, drawn.Mirroring) is { } picture)
                            shut["picture"] = picture;
                        tiles[y * level.Size.X + x] = shut;
                    }

                    break;
                }

                default:
                    break;
            }
        }

        // The Ceres elevator shaft once Ridley is gone: its setup turns the blocks of the shaft's door solid and
        // leaves their pictures as they were, so the door looks the same and stops Samus instead of leading out.
        if (ReferenceEquals(state.SetupBehavior, RoomSetupBehavior.SealCeresElevatorDoorAndSpawnHaze))
        {
            if (SuperMetroid.Core.Areas.Ceres.CeresElevatorDoorSealDefinitions.SealedCollision != RoomCollisionType.SolidBlock)
                throw new InvalidDataException("The seal of the Ceres elevator shaft's door is not solid blocks.");
            Position16<RoomBlocks> top = SuperMetroid.Core.Areas.Ceres.CeresElevatorDoorSealDefinitions.DoorTop;
            for (int row = 0; row < SuperMetroid.Core.Areas.Ceres.CeresElevatorDoorSealDefinitions.HeightInBlocks; row++)
            {
                int x = top.X, y = top.Y + row;
                if (x >= level.Size.X || y >= level.Size.Y)
                    throw new InvalidDataException($"The seal of the door of the layout {path} does not lie inside its level.");
                int cell = y * level.Size.X + x;
                var sealedBlock = new JsonObject { ["kind"] = "solid" };
                RoomBlockAppearance shown = level.Foreground[cell].Appearance;
                if (shown.Shows == RoomBlockShown.Block && Picture(tileset, shown.Block, shown.Mirroring) is { } picture)
                    sealedBlock["picture"] = picture;
                tiles[cell] = sealedBlock;
            }
        }

        return tiles;
    }

    private readonly Dictionary<string, Dictionary<string, JsonArray>> _frames = new(StringComparer.Ordinal);

    /// <summary>The cells of a frame the installation keeps under <paramref name="directory"/>.</summary>
    internal JsonArray FrameCells(string directory, string frame)
    {
        if (!_frames.TryGetValue(directory, out Dictionary<string, JsonArray>? frames))
        {
            frames = new Dictionary<string, JsonArray>(StringComparer.Ordinal);
            foreach (string file in Directory.EnumerateFiles(Path.Combine(_content, directory), "*.json").Order(StringComparer.Ordinal))
            {
                if (JsonNode.Parse(File.ReadAllText(file))?["frames"] is not JsonArray listed)
                    continue;
                foreach (JsonNode? one in listed)
                    frames[one!["id"]!.GetValue<string>()] = one["cells"]!.AsArray();
            }

            _frames[directory] = frames;
        }

        return frames.TryGetValue(frame, out JsonArray? cells)
            ? cells
            : throw new InvalidDataException($"The installation keeps no room object frame {frame} under {directory}.");
    }

    /// <summary>The block a frame draws at one of its cells and how it is mirrored; none where it draws nothing there.</summary>
    /// <exception cref="InvalidDataException">The frame has no such cell.</exception>
    internal (int Block, BlockMirroring Mirroring)? FrameBlock(string directory, string frame, int across, int down)
    {
        foreach (JsonNode? cell in FrameCells(directory, frame))
        {
            if (cell!["x"]!.GetValue<int>() != across || cell["y"]!.GetValue<int>() != down)
                continue;
            if (cell["block"]?.GetValue<int>() is not int block)
                return null;
            return (block, cell["mirroring"]!.GetValue<string>() switch
            {
                "None" => BlockMirroring.None,
                "Horizontal" => BlockMirroring.Horizontal,
                "Vertical" => BlockMirroring.Vertical,
                "Both" => BlockMirroring.Horizontal | BlockMirroring.Vertical,
                var other => throw new InvalidDataException($"The room object frame {frame} mirrors a block {other}, which this does not know."),
            });
        }

        throw new InvalidDataException($"The room object frame {frame} has no cell {across} across and {down} down.");
    }

    /// <summary>
    /// The blocks a patch is, from frames the old game draws one after another, each with the kinds of block it makes.
    /// A later frame's blocks lie over an earlier one's.
    /// </summary>
    internal IEnumerable<PatchCell> PatchCells(string directory, params ReadOnlySpan<(string Frame, RoomBlockShape Shape)> frames)
    {
        var cells = new List<PatchCell>();
        foreach ((string frame, RoomBlockShape shape) in frames)
        {
            foreach (RoomBlockShapeCell cell in shape.Cells)
            {
                (int Block, BlockMirroring Mirroring)? drawn = FrameBlock(directory, frame, cell.Cell.X, cell.Cell.Y);
                cells.Add(new PatchCell(cell.Cell.X, cell.Cell.Y, cell.Collision, drawn?.Block, drawn?.Mirroring ?? BlockMirroring.None));
            }
        }

        return cells;
    }

    /// <summary>The path of the layout of one state of a room, as the Godot project's room scenes name it.</summary>
    internal static string PathOf(RoomDefinition room, RoomStateSettings state) =>
        $"rooms/{EngineSheets.FileName(room.Area.ToString())}/{EngineSheets.FileName(state.Level.Name)}_in_{EngineSheets.FileName(state.GraphicsSet.ToString())}.json";

    /// <summary>How many blocks a door's cap is.</summary>
    internal const int DoorCapBlocks = 4;

    /// <summary>Builds the layout of one state of a room, unless another state has already built the same one.</summary>
    /// <exception cref="InvalidDataException">Another state built the same layout with other things placed in it.</exception>
    private void Build(RoomDefinition room, RoomStateSettings state)
    {
        string path = PathOf(room, state);
        InstalledRoomLevel level = _levels.Get(state.Level);
        string tileset = state.GraphicsSet.ToString();
        _moving = MovedIn(state);
        _heavySpikesHurt = HeavySpikeDamageIn(room, state);
        WriteLevelBackground(state, level, tileset);
        Dictionary<int, JsonObject> placed = PlacedTiles(state, level, tileset, path);
        string placedAs = string.Join('|', placed.OrderBy(tile => tile.Key).Select(tile => $"{tile.Key}:{tile.Value.ToJsonString()}"));
        if (_layouts.TryGetValue(path, out Layout? built))
        {
            if (built.Placed != placedAs)
                throw new InvalidDataException($"Two states share the layout {path} and place different things in it.");
            return;
        }

        var layout = new Layout(tileset, level, placedAs);
        _layouts[path] = layout;
        var elevatorBlocks = new HashSet<Position16<RoomBlocks>>(
            _levels.DoorBlocks(room, state.Level).Where(door => door.Exit.IsElevatorRide).Select(door => door.Block));

        // A door's cap is four blocks of the level, a column or a row from the block that answers a shot. The Godot
        // game's cap is a barrier placed in the room's scene, which draws itself shut and opening; so the layout
        // leaves those blocks open. What it draws there is the doorway with nothing in it, as the cartridge leaves
        // the blocks once the cap has gone: tiles like any others, with the strip of them that lies in front of
        // what passes through.
        var capCells = new Dictionary<int, (int Block, BlockMirroring Mirroring)>();
        for (int y = 0; y < level.Size.Y; y++)
        {
            for (int x = 0; x < level.Size.X; x++)
            {
                int cell = y * level.Size.X + x;
                if (level.Foreground[cell].Collision != RoomCollisionType.ShootableBlock)
                    continue;
                (int across, int down, string side) = level.Behaviors[cell].In(room.Area).Shootable switch
                {
                    ShootableReaction.BlueDoorCapLeft => (0, 1, "left"),
                    ShootableReaction.BlueDoorCapRight => (0, 1, "right"),
                    ShootableReaction.BlueDoorCapUp => (1, 0, "up"),
                    ShootableReaction.BlueDoorCapDown => (1, 0, "down"),
                    _ => (0, 0, ""),
                };
                for (int part = 0; part < DoorCapBlocks && (across, down) != (0, 0); part++)
                {
                    int capX = x + across * part, capY = y + down * part;
                    if (capX < level.Size.X && capY < level.Size.Y)
                    {
                        capCells[capY * level.Size.X + capX] =
                            FrameBlock(GameInstallationLayout.RoomObjectGreyDoorVisualDirectoryName, $"clear-{side}", across * part, down * part)
                            ?? throw new InvalidDataException($"The open doorway of a door on the {side} draws nothing {across * part} across and {down * part} down.");
                    }
                }
            }
        }

        var plants = new List<(int X, int Y, SpecialAirReaction Kind)>();
        var row = new StringBuilder(level.Size.X);
        for (int y = 0; y < level.Size.Y; y++)
        {
            row.Clear();
            for (int x = 0; x < level.Size.X; x++)
            {
                int cell = y * level.Size.X + x;
                RoomTerrainBlock terrain = level.Foreground[cell];
                RoomBlockBehavior behavior = level.Behaviors[cell].In(room.Area);
                if (terrain.Collision == RoomCollisionType.SpecialAir && behavior.SpecialAir is { } air && PlantOf(air) is not null)
                    plants.Add((x, y, air));
                JsonObject tile = capCells.TryGetValue(cell, out (int Block, BlockMirroring Mirroring) doorway)
                    ? EmptyShowing(Picture(tileset, doorway.Block, doorway.Mirroring))
                    : placed.TryGetValue(cell, out JsonObject? put)
                        ? put
                        : Tile(terrain, behavior, elevatorBlocks.Contains(new Position16<RoomBlocks>(x, y)), tileset);
                if (terrain.Collision is RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension && !capCells.ContainsKey(cell) && !placed.ContainsKey(cell))
                    RequireNoHoleAt(level, room, x, y, path);
                row.Append(Letter(layout, tile, path));
            }

            layout.Rows.Add(row.ToString());
        }

        // A few of the cartridge's levels run on below their room (Double Chamber's and the Bowling Alley's each hold
        // a screen more of rows than the room has). The layout is the room's own rows: its screens are the room's.
        int roomRows = room.Size.Y * (256 / Block);
        if (layout.Rows.Count > roomRows)
            layout.Rows.RemoveRange(roomRows, layout.Rows.Count - roomRows);

        _graphicsSets.Add(tileset);
        foreach ((int x, int y, SpecialAirReaction kind) in plants)
            PatchPlant(room, state, x, y, kind);
    }

    /// <summary>
    /// Refuses a cell that continues another block and is written as open space where the block it stands for is
    /// something there. On the cartridge such a cell collides and reacts as the block its links lead to
    /// (ContinuedBlocks); here it is open, which is right in three cases only: its links end in nothing or lead to
    /// open air; they lead to a reacting block of several cells that covers it by its size; or they lead to a plant,
    /// whose mouth the plants themselves reach across. Anything else would be a hole in the room.
    /// </summary>
    /// <exception cref="InvalidDataException">The cell stands for a block it is not covered by, or its links leave the level or never end.</exception>
    private void RequireNoHoleAt(InstalledRoomLevel level, RoomDefinition room, int x, int y, string path)
    {
        int width = level.Size.X, cells = width * level.Size.Y;
        int at = y * width + x;
        for (int hops = 0; hops < cells; hops++)
        {
            RoomTerrainBlock here = level.Foreground[at];
            RoomBlockBehavior does = level.Behaviors[at].In(room.Area);
            if (here.Collision is not (RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension))
            {
                int column = at % width, row = at / width;
                if (here.Collision == RoomCollisionType.Air)
                    return;
                if (here.Collision == RoomCollisionType.SpecialAir && does.SpecialAir is { } air && PlantOf(air) is not null)
                    return;
                if (Reacting(here.Collision, does) is { } whole && x >= column && x < column + whole.Width && y >= row && y < row + whole.Height)
                    return;
                throw new InvalidDataException(
                    $"The block {x} across and {y} down of the layout {path} continues the block {column} across and {row} down, a {here.Collision} that does not cover it: it would be a hole.");
            }

            if (does.EndsExtension)
                return;

            // A link along a row counts blocks in the room's reading order; one along a column counts rows.
            at += here.Collision == RoomCollisionType.HorizontalExtension ? does.ExtensionOffset : does.ExtensionOffset * width;
            if (at < 0 || at >= cells)
                throw new InvalidDataException($"The block {x} across and {y} down of the layout {path} continues a block outside its level.");
        }

        throw new InvalidDataException($"The block {x} across and {y} down of the layout {path} continues blocks that lead back to themselves.");
    }

    /// <summary>What one block of a level is as a tile of a layout.</summary>
    private JsonObject Tile(RoomTerrainBlock terrain, RoomBlockBehavior behavior, bool underElevator, string tileset)
    {
        var tile = new JsonObject();
        switch (terrain.Collision)
        {
            case RoomCollisionType.SolidBlock:
                tile["kind"] = "solid";
                break;
            case RoomCollisionType.DoorBlock:
                // A door's blocks are open, for the room's exit to lie over; those under an elevator are what it rests on.
                tile["kind"] = underElevator ? "solid" : "empty";
                break;
            case RoomCollisionType.Slope:
                tile["kind"] = "slope";
                var solid = new JsonArray();
                for (int column = 0; column < SlopeShape.Columns; column++)
                {
                    // The old height of a column is how far its surface lies from the block's far edge, so the rest of
                    // the column is what is solid.
                    int across = behavior.Slope.FlipsHorizontally ? SlopeShape.Columns - 1 - column : column;
                    solid.Add(Block - Math.Clamp(behavior.Slope.Shape.HeightAt(across), 0, Block));
                }

                tile["solid"] = solid;
                if (behavior.Slope.FlipsVertically)
                    tile["from"] = "top";

                // The old sloped shapes hold Samus up by her middle column alone; the square ones are steps and walls.
                if (behavior.Slope.IsNonSquare)
                    tile["holds"] = "middle";
                else
                    tile["step"] = Block / 2;

                // A slope read as a height profile lets only a share of a move along it through, counted in 256ths:
                // of Samus's on the ground where it is a floor's, and of a crawler's on a floor's or a ceiling's. The
                // old shapes keep the two apart and give every shape the same for both.
                int share = behavior.Slope.Shape.GroundedSpeedShare.TwoHundredFiftySixths;
                if (behavior.Slope.Shape.CrawlerTangentShare.TwoHundredFiftySixths != share)
                    throw new InvalidDataException($"The slope shape {behavior.Slope.Shape} lets a crawler through by another share than Samus, and a tile keeps one share.");
                if (behavior.Slope.IsNonSquare && share != SpeedShare.Whole.TwoHundredFiftySixths)
                    tile["share"] = share;
                break;
            case RoomCollisionType.HorizontalExtension:
            case RoomCollisionType.VerticalExtension:
                // A cell that continues the block beside or above it: that block's own tile covers it.
                tile["kind"] = "empty";
                break;
            default:
                if (Reacting(terrain.Collision, behavior) is { } block)
                {
                    tile["kind"] = "block";
                    tile["block"] = BlockType(block);

                    // What it shows as it breaks, and for what it is, every set of graphics that has it must hold.
                    foreach (int shared in block.Pictures ?? [])
                    {
                        if (Picture(tileset, shared, BlockMirroring.None) is null)
                            throw new InvalidDataException($"The graphics {tileset} draw nothing for the block {shared}, which the block {block.Name} shows.");
                    }
                    if (block.Width > 1)
                        tile["width"] = block.Width;
                    if (block.Height > 1)
                        tile["height"] = block.Height;
                }
                else
                {
                    tile["kind"] = terrain.Collision.ToString().EndsWith("Block", StringComparison.Ordinal) ? "solid" : "empty";
                }

                break;
        }

        if (terrain.Appearance.Shows == RoomBlockShown.Block)
        {
            if (Picture(tileset, terrain.Appearance.Block, terrain.Appearance.Mirroring) is { } picture)
            {
                tile["picture"] = picture;
            }
            else if (MovingPictures(tileset, terrain.Appearance.Block, terrain.Appearance.Mirroring) is { } moving)
            {
                // A block the room's effect animates: its pictures in turn, or the one it rests on where nothing moves it.
                if (moving.Pictures.Count == 1)
                {
                    tile["picture"] = moving.Pictures[0]!.GetValue<string>();
                }
                else
                {
                    tile["pictures"] = moving.Pictures;
                    tile["updates"] = moving.Updates;
                }
            }
        }

        // What the X-ray scope shows of a tile that is no reacting block, where that is not what it always shows.
        if (tile["kind"]!.GetValue<string>() != "block" && SeenAs(terrain, behavior, tileset, tile["picture"]?.GetValue<string>()) is { } seen)
            tile["seen"] = seen;
        return tile;
    }

    /// <summary>A block that reacts: the rules it follows, by a name, and how many tiles it covers.</summary>
    private sealed partial record ReactingBlock(string Name, JsonObject Rules, int Width = 1, int Height = 1, IReadOnlyList<int>? Pictures = null);

    /// <summary>The hit kind of the Godot game by which an enemy cuts through the blocks an enemy breaks, as its Shaktool's saws do.</summary>
    private const string EnemyCutKind = "saw";

    /// <summary>
    /// The hit kinds of the Godot game that are "any weapon" to a block: every shot and bomb, and the grapple's touch,
    /// which strikes a block the cartridge's shots meet as a beam does (ShootableTable, BlockStrike.GrappleTouch).
    /// </summary>
    internal static JsonArray AnyWeapon() => ["beam", "missile", "super_missile", "bomb", "power_bomb", "grapple"];

    /// <summary>Every shot and bomb without the grapple's touch, which the wall the animals escape through turns away.</summary>
    internal static JsonArray AnyShot() => ["beam", "missile", "super_missile", "bomb", "power_bomb"];

    /// <summary>What a block of a kind that reacts is, or null for one that does nothing a plain tile does not.</summary>
    private ReactingBlock? Reacting(RoomCollisionType collision, RoomBlockBehavior behavior)
    {
        bool solid = collision.ToString().EndsWith("Block", StringComparison.Ordinal);
        string body = solid ? "block" : "air";
        switch (collision)
        {
            case RoomCollisionType.ShootableBlock or RoomCollisionType.ShootableAir:
            {
                string reaction = behavior.Shootable.ToString();
                (int width, int height) = Span(reaction);
                bool comesBack = reaction.StartsWith("Respawning", StringComparison.Ordinal);
                JsonArray? brokenBy = reaction switch
                {
                    _ when reaction.Contains("Break", StringComparison.Ordinal) => AnyWeapon(),
                    _ when reaction.Contains("PowerBomb", StringComparison.Ordinal) => ["power_bomb"],
                    _ when reaction.Contains("SuperMissile", StringComparison.Ordinal) => ["super_missile"],
                    _ => null,
                };
                if (brokenBy is null)
                    return null;
                var rules = new JsonObject { ["solid"] = solid, ["broken_by"] = brokenBy };
                var pictures = new List<int>();
                if (reaction.Contains("Break", StringComparison.Ordinal))
                {
                    pictures.AddRange(AddBreakup(rules, comesBack
                        ? Frames(BlockReactionChanges.RespawningShot(behavior.Shootable))
                        : Frames(BlockReactionChanges.PermanentShot(behavior.Shootable))));
                }
                else
                {
                    // A block only a power bomb or a super missile breaks: a bomb shows it for what it is.
                    WeaponShotBlock weapon = WeaponBlockReactionDefinitions.WeaponShotBlocks[ShotBlockReactionDefinitions.Handled[behavior.Shootable]];
                    pictures.Add(AddReveal(rules, weapon.Reveal, width, height));
                    pictures.AddRange(AddBreakup(rules, Frames(weapon.Break)));
                }

                pictures.AddRange(AddSight(rules, collision, reaction, width, height));
                return new ReactingBlock($"shot_{body}_{EngineSheets.FileName(reaction)}", rules, width, height, pictures);
            }

            case RoomCollisionType.BombableBlock or RoomCollisionType.BombableAir:
            {
                if (behavior.Bombable is not { } bombable || !bombable.ToString().Contains("Break", StringComparison.Ordinal))
                    return null;
                string reaction = bombable.ToString();
                (int width, int height) = Span(reaction);
                // A bomb, a power bomb, and Samus breaking through at speed or with the screw attack.
                var rules = new JsonObject
                {
                    ["solid"] = solid,
                    ["broken_by"] = new JsonArray("bomb", "power_bomb", "screw_attack", "speed_boost", "shinespark"),
                };
                // The breaking a bomb sets off, which is silent under the bomb's own blast.
                BlockReactionChange bombed = BlockReactionChangeDefinitions.BombBreak[bombable];
                List<int> pictures = AddBreakup(rules, Frames(bombed));
                // A power bomb and Samus's body break it the same way, each with a sound of its own.
                string byPowerBomb = BreakSoundOf(bombed, BlockReactionChangeDefinitions.ReactionBomb[bombable]);
                string byBody = BreakSoundOf(bombed, BlockReactionChangeDefinitions.CollisionBomb[bombable]);
                rules["break_sound_by_kind"] = new JsonObject
                {
                    ["power_bomb"] = byPowerBomb,
                    ["screw_attack"] = byBody,
                    ["speed_boost"] = byBody,
                    ["shinespark"] = byBody,
                };
                pictures.AddRange(AddSight(rules, collision, reaction, width, height));
                return new ReactingBlock($"bomb_{body}_{EngineSheets.FileName(reaction)}", rules, width, height, pictures);
            }

            case RoomCollisionType.SpecialBlock:
            {
                if (behavior.Special is not { } special)
                    return null;
                string reaction = special.ToString();
                (int width, int height) = Span(reaction);
                var rules = new JsonObject { ["solid"] = true };
                var pictures = new List<int>();
                if (BlockReactionDefinitions.ContactCrumbles.TryGetMeaning(special, out ContactCrumbleBlock crumble))
                {
                    // It gives way a moment after it is stood on.
                    BlockReactionChange change = BlockReactionChanges.ContactCrumble(crumble);
                    rules["breaks_when_stood_on"] = true;
                    rules["stood_on_updates"] = change.FirstFrameDelay;
                    pictures.AddRange(AddBreakup(rules, Frames(change)));
                }
                else if (SpeedBoosterBlockObjectDefinitions.CommonBlocks.TryGetValue(special, out BlockReactionChange? common)
                    || SpeedBoosterBlockObjectDefinitions.BrinstarBlocks.TryGetValue(special, out common))
                {
                    rules["broken_by"] = new JsonArray("speed_boost", "shinespark");
                    pictures.AddRange(AddBreakup(rules, Frames(common)));
                }
                else if (special != SpecialBlockReaction.BombRevealsSpeedBlock)
                {
                    return null;
                }

                // A bomb shows each of these for what it is; one of them only looks like a block that speed breaks.
                pictures.Add(AddReveal(rules, BombSpecialBlockReactions.ChangeFor(special), width, height));
                pictures.AddRange(AddSight(rules, collision, reaction, width, height));
                return new ReactingBlock($"special_{EngineSheets.FileName(reaction)}", rules, width, height, pictures);
            }

            case RoomCollisionType.SpikeBlock or RoomCollisionType.SpikeAir:
            {
                if (behavior.Spike is not { } spike)
                    return null;

                // A block an enemy breaks: nothing of Samus's does, and an enemy that cuts its way through, by a hit
                // of the saw's kind, clears it at once. It breaks up as a single block does, with the crumble's sound.
                if (SuperMetroid.Core.Enemies.EnemyBreakableTerrain.IsEnemyBreakable(spike))
                {
                    var cut = new JsonObject { ["solid"] = solid, ["broken_by"] = new JsonArray(EnemyCutKind) };
                    List<int> shown = AddBreakup(cut, Frames(ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak));
                    return new ReactingBlock($"spike_{body}_{EngineSheets.FileName(spike.ToString())}", cut, Pictures: shown);
                }

                int damage = spike switch
                {
                    // An area whose machinery has no power until its boss is beaten has harmless heavy spikes until
                    // then, as the Wrecked Ship's are before Phantoon: they are plain blocks of their kind.
                    SpikeReaction.HeavySpike when solid => _heavySpikesHurt
                        ?? throw new InvalidDataException("A heavy spike lies in a layout that is shown both with its area's power and without it."),
                    SpikeReaction.LightSpike or SpikeReaction.TurretSpike when solid => SpikeHazardDefinitions.LightSpikeDamage,

                    // Air hurts her only with the damaging spike air's reaction: with a heavy or a light spike's it
                    // does nothing, as the cartridge's test of what she is inside has it.
                    SpikeReaction.DamagingSpikeAir when !solid => SpikeHazardDefinitions.LightSpikeDamage,
                    _ => 0,
                };
                if (damage == 0)
                    return null;
                return new ReactingBlock($"spike_{body}_{EngineSheets.FileName(spike.ToString())}",
                    new JsonObject { ["solid"] = solid, ["contact"] = "hurts", ["contact_hit"] = damage });
            }

            case RoomCollisionType.GrappleBlock:
            {
                if (behavior.Grapple is not { } grapple || grapple == GrappleReaction.Rejected)
                    return null;
                bool breaks = grapple is GrappleReaction.RespawningBreakableAnchor or GrappleReaction.PermanentBreakableAnchor;
                var rules = new JsonObject { ["solid"] = true, ["contact"] = breaks ? "latch_once" : "latch" };
                var pictures = new List<int>();
                if (breaks)
                {
                    // It holds for so long once the beam has it, and then breaks whether or not the beam still does.
                    BlockReactionChange change = grapple == GrappleReaction.RespawningBreakableAnchor
                        ? BlockReactionChangeDefinitions.RespawningBreakableGrappleBlock
                        : BlockReactionChangeDefinitions.PermanentBreakableGrappleBlock;
                    rules["latched_updates"] = GrappleHeldUpdates(change);
                    pictures.AddRange(AddBreakup(rules, GrappleFrames(change)));
                }

                pictures.AddRange(AddSight(rules, collision, grapple.ToString(), 1, 1));
                return new ReactingBlock($"grapple_{EngineSheets.FileName(grapple.ToString())}", rules, Pictures: pictures);
            }

            case RoomCollisionType.SpecialAir:
            {
                if (behavior.SpecialAir is not { } air)
                    return null;
                JsonObject? rules = air switch
                {
                    SpecialAirReaction.RightwardConveyor or SpecialAirReaction.RightwardConveyorOnFoot
                        or SpecialAirReaction.LeftwardConveyor or SpecialAirReaction.LeftwardConveyorOnFoot => Conveyor(air),
                    SpecialAirReaction.SlowSandfall =>
                        new JsonObject { ["solid"] = false, ["contact"] = "pulls_down", ["contact_speed"] = EngineNames.Number(QuicksandDefinitions.SlowSandfallDisplacement) },
                    SpecialAirReaction.FastSandfall =>
                        new JsonObject { ["solid"] = false, ["contact"] = "pulls_down", ["contact_speed"] = EngineNames.Number(QuicksandDefinitions.FastSandfallDisplacement) },
                    SpecialAirReaction.QuicksandSurface => QuicksandSurface(),

                    // A plant that seizes Samus: a block that does nothing of itself, which the room's plants are found by.
                    SpecialAirReaction.FloorPlant or SpecialAirReaction.CeilingPlant => new JsonObject { ["solid"] = false },

                    // Sand she sinks through: it ends her fall's speed and takes her down at its own pace, and holds
                    // nothing up: she is in the air in it, as the cartridge's move passes through it.
                    SpecialAirReaction.SubmergingQuicksand => new JsonObject
                    {
                        ["solid"] = false,
                        ["contact"] = "pulls_down",
                        ["contact_speed"] = EngineNames.Number(QuicksandDefinitions.SubmergingSinkDisplacement),
                        ["stops_fall"] = SpecialAirReactions.CollisionOf(air).StopsFall,
                    },
                    _ => null,
                };
                return rules is null ? null : new ReactingBlock($"air_{EngineSheets.FileName(air.ToString())}", rules);
            }

            default:
                return null;
        }
    }

    /// <summary>How many tiles across and down a reaction named for its size covers.</summary>
    private static (int Width, int Height) Span(string reaction) =>
        reaction.EndsWith("2x2", StringComparison.Ordinal) ? (2, 2)
        : reaction.EndsWith("2x1", StringComparison.Ordinal) ? (2, 1)
        : reaction.EndsWith("1x2", StringComparison.Ordinal) ? (1, 2)
        : (1, 1);

    /// <summary>The data file of a reacting block's rules, written the first time a layout names it.</summary>
    private string BlockType(ReactingBlock block)
    {
        string path = $"blocks/{block.Name}.json";
        if (_blockTypes.Add(path))
            _tree.WriteData(path, block.Rules);
        return path;
    }

    /// <summary>A tile that is open and shows a picture, or nothing where there is none.</summary>
    private static JsonObject EmptyShowing(string? picture)
    {
        var tile = new JsonObject { ["kind"] = "empty" };
        if (picture is not null)
            tile["picture"] = picture;
        return tile;
    }

    /// <summary>
    /// The picture of one block of a set of graphics, mirrored as the level shows it: the part of it behind the
    /// room's things and the part in front, as one picture. None for a block that shows nothing.
    /// </summary>
    private string? Picture(string tileset, int block, BlockMirroring mirroring)
    {
        string path = $"tiles/{EngineSheets.FileName(tileset)}/{block}{(mirroring.HasFlag(BlockMirroring.Horizontal) ? "_h" : "")}{(mirroring.HasFlag(BlockMirroring.Vertical) ? "_v" : "")}.png";
        if (_tree.Holds(path))
            return path;
        if (EngineRoomObjects.BlockPixels(_sheets, tileset, block, mirroring.HasFlag(BlockMirroring.Horizontal), mirroring.HasFlag(BlockMirroring.Vertical)) is not { } pixels)
            return null;
        _tree.WritePicture(path, Block, Block, pixels);
        if (EngineRoomObjects.NamedBlockRegions(_sheets, tileset, EngineRoomObjects.BlockFrame(block),
                mirroring.HasFlag(BlockMirroring.Horizontal), mirroring.HasFlag(BlockMirroring.Vertical)) is { } regions)
            _tree.WriteRegionMap(path, Block, Block, regions);
        WriteFront(path, tileset, EngineRoomObjects.BlockFrame(block), mirroring);
        return path;
    }

    /// <summary>
    /// Writes, beside a tile's picture, the part of it that lies in front of the things standing in a room, where it
    /// has one, with that part's map of regions where the set's sheet tells regions apart. The tile's own picture
    /// stays whole: the part is drawn again over what stands in the room.
    /// </summary>
    private void WriteFront(string path, string tileset, string frame, BlockMirroring mirroring)
    {
        bool across = mirroring.HasFlag(BlockMirroring.Horizontal), down = mirroring.HasFlag(BlockMirroring.Vertical);
        if (EngineRoomObjects.NamedBlockFrontPixels(_sheets, tileset, frame, across, down) is not { } pixels)
            return;
        string front = EngineAssetTree.FrontPath(path);
        _tree.WritePicture(front, Block, Block, pixels);
        if (EngineRoomObjects.NamedBlockFrontRegions(_sheets, tileset, frame, across, down) is { } regions)
            _tree.WriteRegionMap(front, Block, Block, regions);
    }
}
