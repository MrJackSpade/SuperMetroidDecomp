using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Writes the Godot game's room layouts. A layout is a grid of letters, one a tile, with what each letter stands
/// for: nothing, something solid, a slope with its height at each pixel across, or a block that reacts. Each of the
/// old game's levels is written once for each area that uses it and each set of graphics it is drawn with, since a
/// block's reaction depends on the area and its picture on the graphics.
/// </summary>
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

    /// <summary>Every set of graphics a layout written so far is drawn with, by the old game's name for it.</summary>
    internal IReadOnlyList<string> GraphicsSets => [.. _graphicsSets];

    internal EngineLayouts(EngineAssetTree tree, EngineSheets sheets, InstalledRoomLevelStreams levels)
    {
        _tree = tree;
        _sheets = sheets;
        _levels = levels;
    }

    /// <summary>Writes the layout of every state of every room.</summary>
    internal void WriteAll()
    {
        foreach ((RoomDefinition room, RoomStateChoice choice) in RoomStateSelectionDefinitions.Choices)
        {
            foreach (RoomStateSettings state in choice.Branches.Select(branch => branch.State).Append(choice.Default))
                Write(room, state);
        }
    }

    /// <summary>The path of the layout of one state of a room, as the Godot project's room scenes name it.</summary>
    internal static string PathOf(RoomDefinition room, RoomStateSettings state) =>
        $"rooms/{EngineSheets.FileName(room.Area.ToString())}/{EngineSheets.FileName(state.Level.Name)}_in_{EngineSheets.FileName(state.GraphicsSet.ToString())}.json";

    /// <summary>How many blocks a door's cap is.</summary>
    internal const int DoorCapBlocks = 4;

    /// <summary>Writes the layout of one state of a room, unless another state has already written the same one.</summary>
    private void Write(RoomDefinition room, RoomStateSettings state)
    {
        string path = PathOf(room, state);
        if (_tree.Holds(path))
            return;
        InstalledRoomLevel level = _levels.Get(state.Level);
        string tileset = state.GraphicsSet.ToString();
        var elevatorBlocks = new HashSet<Position16<RoomBlocks>>(
            _levels.DoorBlocks(room, state.Level).Where(door => door.Exit.IsElevatorRide).Select(door => door.Block));

        // A door's cap is four blocks of the level, a column or a row from the block that answers a shot. The Godot
        // game's cap is a barrier placed in the room's scene, which draws itself shut, opening and open; so the
        // layout leaves those blocks open and draws nothing there.
        var capCells = new HashSet<int>();
        for (int y = 0; y < level.Size.Y; y++)
        {
            for (int x = 0; x < level.Size.X; x++)
            {
                int cell = y * level.Size.X + x;
                if (level.Foreground[cell].Collision != RoomCollisionType.ShootableBlock)
                    continue;
                (int across, int down) = level.Behaviors[cell].In(room.Area).Shootable switch
                {
                    ShootableReaction.BlueDoorCapLeft or ShootableReaction.BlueDoorCapRight => (0, 1),
                    ShootableReaction.BlueDoorCapUp or ShootableReaction.BlueDoorCapDown => (1, 0),
                    _ => (0, 0),
                };
                for (int part = 0; part < DoorCapBlocks && (across, down) != (0, 0); part++)
                {
                    int capX = x + across * part, capY = y + down * part;
                    if (capX < level.Size.X && capY < level.Size.Y)
                        capCells.Add(capY * level.Size.X + capX);
                }
            }
        }

        var legend = new JsonObject();
        var letters = new Dictionary<string, char>(StringComparer.Ordinal);
        var rows = new JsonArray();
        var row = new StringBuilder(level.Size.X);
        for (int y = 0; y < level.Size.Y; y++)
        {
            row.Clear();
            for (int x = 0; x < level.Size.X; x++)
            {
                int cell = y * level.Size.X + x;
                RoomTerrainBlock terrain = level.Foreground[cell];
                RoomBlockBehavior behavior = level.Behaviors[cell].In(room.Area);
                JsonObject tile = capCells.Contains(cell)
                    ? new JsonObject { ["kind"] = "empty" }
                    : Tile(terrain, behavior, elevatorBlocks.Contains(new Position16<RoomBlocks>(x, y)), tileset);
                string key = tile.ToJsonString();
                if (!letters.TryGetValue(key, out char letter))
                {
                    letter = (char)(FirstLetter + letters.Count);
                    if (char.IsSurrogate(letter))
                        throw new InvalidDataException($"The layout {path} has more different tiles than there are letters for.");
                    letters[key] = letter;
                    legend[letter.ToString()] = tile;
                }

                row.Append(letter);
            }

            rows.Add(row.ToString());
        }

        _graphicsSets.Add(tileset);
        _tree.WriteData(path, new JsonObject
        {
            ["tile_size"] = Block,
            // What stands in the room and looks as its graphics do is drawn with this set.
            ["graphics"] = EngineRoomObjects.GraphicsName(tileset),
            ["tiles"] = legend,
            ["rows"] = rows,
        });
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
                    int across = behavior.Slope.FlipsHorizontally ? SlopeShape.Columns - 1 - column : column;
                    solid.Add(Math.Clamp(behavior.Slope.Shape.HeightAt(across), 0, Block));
                }

                tile["solid"] = solid;
                if (behavior.Slope.FlipsVertically)
                    tile["from"] = "top";
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

        if (terrain.Appearance.Shows == RoomBlockShown.Block && Picture(tileset, terrain.Appearance.Block, terrain.Appearance.Mirroring) is { } picture)
            tile["picture"] = picture;
        return tile;
    }

    /// <summary>A block that reacts: the rules it follows, by a name, and how many tiles it covers.</summary>
    private sealed partial record ReactingBlock(string Name, JsonObject Rules, int Width = 1, int Height = 1);

    /// <summary>The hit kinds of the Godot game that are "any weapon" to a block.</summary>
    internal static JsonArray AnyWeapon() => ["beam", "missile", "super_missile", "bomb", "power_bomb"];

    /// <summary>What a block of a kind that reacts is, or null for one that does nothing a plain tile does not.</summary>
    private static ReactingBlock? Reacting(RoomCollisionType collision, RoomBlockBehavior behavior)
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
                if (comesBack)
                    rules["regrows_after"] = RegrowUpdates;
                return new ReactingBlock($"shot_{body}_{EngineSheets.FileName(reaction)}", rules, width, height);
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
                if (reaction.StartsWith("Respawning", StringComparison.Ordinal))
                    rules["regrows_after"] = RegrowUpdates;
                return new ReactingBlock($"bomb_{body}_{EngineSheets.FileName(reaction)}", rules, width, height);
            }

            case RoomCollisionType.SpecialBlock:
            {
                if (behavior.Special is not { } special)
                    return null;
                string reaction = special.ToString();
                (int width, int height) = Span(reaction);
                bool comesBack = reaction.StartsWith("Respawning", StringComparison.Ordinal);
                JsonObject? rules = reaction switch
                {
                    _ when reaction.Contains("Crumble", StringComparison.Ordinal) => new JsonObject { ["solid"] = true, ["breaks_when_stood_on"] = true },
                    _ when reaction.Contains("SpeedBlock", StringComparison.Ordinal) =>
                        new JsonObject { ["solid"] = true, ["broken_by"] = new JsonArray("speed_boost", "shinespark") },
                    _ => null,
                };
                if (rules is null)
                    return null;
                if (comesBack)
                    rules["regrows_after"] = RegrowUpdates;
                return new ReactingBlock($"special_{EngineSheets.FileName(reaction)}", rules, width, height);
            }

            case RoomCollisionType.SpikeBlock or RoomCollisionType.SpikeAir:
            {
                if (behavior.Spike is not { } spike)
                    return null;
                int damage = spike switch
                {
                    SpikeReaction.HeavySpike => SpikeHazardDefinitions.HeavySpikeDamage,
                    SpikeReaction.LightSpike or SpikeReaction.TurretSpike => SpikeHazardDefinitions.LightSpikeDamage,
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
                if (grapple == GrappleReaction.RespawningBreakableAnchor)
                    rules["regrows_after"] = RegrowUpdates;
                return new ReactingBlock($"grapple_{EngineSheets.FileName(grapple.ToString())}", rules);
            }

            case RoomCollisionType.SpecialAir:
            {
                if (behavior.SpecialAir is not { } air)
                    return null;
                JsonObject? rules = air switch
                {
                    SpecialAirReaction.RightwardConveyor or SpecialAirReaction.RightwardConveyorOnFoot =>
                        new JsonObject { ["solid"] = false, ["contact"] = "carries", ["contact_speed"] = ConveyorSpeed },
                    SpecialAirReaction.LeftwardConveyor or SpecialAirReaction.LeftwardConveyorOnFoot =>
                        new JsonObject { ["solid"] = false, ["contact"] = "carries", ["contact_speed"] = -ConveyorSpeed },
                    SpecialAirReaction.SlowSandfall =>
                        new JsonObject { ["solid"] = false, ["contact"] = "pulls_down", ["contact_speed"] = EngineNames.Number(QuicksandDefinitions.SlowSandfallDisplacement) },
                    SpecialAirReaction.FastSandfall =>
                        new JsonObject { ["solid"] = false, ["contact"] = "pulls_down", ["contact_speed"] = EngineNames.Number(QuicksandDefinitions.FastSandfallDisplacement) },
                    SpecialAirReaction.QuicksandSurface or SpecialAirReaction.SubmergingQuicksand =>
                        new JsonObject { ["solid"] = false, ["contact"] = "pulls_down", ["contact_speed"] = QuicksandSpeed },
                    _ => null,
                };
                return rules is null ? null : new ReactingBlock($"air_{EngineSheets.FileName(air.ToString())}", rules);
            }

            default:
                return null;
        }
    }

    /// <summary>
    /// How many updates a block that grows back stays gone. This number is not yet taken from the old game's block
    /// objects, which time a block's return by their animations.
    /// </summary>
    private const int RegrowUpdates = 300;

    /// <summary>
    /// How fast a conveyor carries and quicksand pulls, in pixels an update. These numbers are not yet taken from the
    /// old game, which moves Samus on a conveyor and in quicksand by rules of its own.
    /// </summary>
    private const double ConveyorSpeed = 1, QuicksandSpeed = 1;

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
        return path;
    }
}
