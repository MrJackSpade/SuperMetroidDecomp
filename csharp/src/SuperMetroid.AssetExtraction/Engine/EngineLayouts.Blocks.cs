using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SoundRequestPriority = SuperMetroid.Core.Compatibility.SoundRequestPriority;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// How the blocks that react break, come back and show what they are, from the old game's own changes to them: the
/// pictures of a block breaking up and of it growing back, how long each shows, how long the block stays gone, the
/// sound of it, and the picture a bomb leaves on one it does not break.
/// </summary>
internal sealed partial class EngineLayouts
{
    /// <summary>The animations of blocks that the room whose layout is being built sets going.</summary>
    private IReadOnlyList<BlockAnimation> _moving = [];

    /// <summary>The animations of blocks that the effects of one state of a room set going.</summary>
    private static List<BlockAnimation> MovedIn(RoomStateSettings state) =>
        [.. (state.Fx?.Effects ?? []).SelectMany(effect => effect.BlockAnimations).Distinct()];

    /// <summary>What an animation of blocks is called in the names of the pictures that change with it.</summary>
    private static string InputOf(BlockAnimation animation) => $"animation {BlockAnimationFrames.Frame(animation, "idle 0").Animation.Name}";

    /// <summary>The pixels of a named picture of a set of graphics, a block in size and mirrored as asked: its part behind with its part in front over it.</summary>
    private RgbaPixel[]? NamedBlockPixels(string tileset, string frame, BlockMirroring mirroring)
    {
        const string family = GameInstallationLayout.RoomBlockSheetDirectoryName;
        RgbaPixel[]? behind = _sheets.FramePixels(family, tileset, frame, Block, Block);
        RgbaPixel[]? front = _sheets.FramePixels(family, tileset, $"{frame}^front", Block, Block);
        if (behind is null && front is null)
            return null;
        bool across = mirroring.HasFlag(BlockMirroring.Horizontal), down = mirroring.HasFlag(BlockMirroring.Vertical);
        var pixels = new RgbaPixel[Block * Block];
        for (int y = 0; y < Block; y++)
        {
            for (int x = 0; x < Block; x++)
            {
                int from = (down ? Block - 1 - y : y) * Block + (across ? Block - 1 - x : x);
                pixels[y * Block + x] = front is not null && front[from].A != 0 ? front[from] : behind is not null ? behind[from] : default;
            }
        }

        return pixels;
    }

    /// <summary>
    /// The pictures of a block that the old game animates, as the room whose layout is being built shows it: one for
    /// each frame of the animations the room sets going, and the one it rests on where the room sets none of them
    /// going. None for a block the set of graphics has no such pictures of.
    /// </summary>
    /// <exception cref="InvalidDataException">The animations that move it do not keep time with one another, or a picture is missing.</exception>
    private (JsonArray Pictures, int Updates)? MovingPictures(string tileset, int block, BlockMirroring mirroring)
    {
        const string family = GameInstallationLayout.RoomBlockSheetDirectoryName;
        string subject = $"block {block}";
        string? named = _sheets.FramesOf(family, tileset).FirstOrDefault(frame => frame.StartsWith(subject + "|", StringComparison.Ordinal));
        if (named is null)
            return null;
        if (named.EndsWith("^front", StringComparison.Ordinal))
            named = named[..^"^front".Length];
        string[] inputs = [.. named.Split('|').Skip(1).Select(part => part[..part.IndexOf('=', StringComparison.Ordinal)])];

        // A block whose picture changes with something other than the animations a room's effect sets going is shown by
        // whatever changes it, and the layout draws nothing for it.
        string[] animations = [.. Enum.GetValues<BlockAnimation>().Select(InputOf)];
        if (inputs.Any(input => !animations.Contains(input, StringComparer.Ordinal)))
            return null;
        List<BlockAnimation> going = [.. _moving.Where(animation => inputs.Contains(InputOf(animation), StringComparer.Ordinal))];

        // The frames every animation that moves it steps through, which must keep time for the block to have one picture at a time.
        var steps = new List<(string Shown, int Updates)>();
        foreach (BlockAnimation animation in going)
        {
            JsonArray played = EngineTimelines.Of<ArtworkFrame, NoAnimationCue>(
                [], BlockAnimationLoopDefinitions.IdleLoops[animation].Select(IAnimationBeat<ArtworkFrame, NoAnimationCue> (beat) => beat),
                frame => frame.Name, _ => null)["steps"]!.AsArray();
            List<(string Shown, int Updates)> own = [.. played.Select(step => (step!["picture"]!.GetValue<string>(), step["updates"]!.GetValue<int>()))];
            if (steps.Count == 0)
                steps = own;
            else if (!steps.SequenceEqual(own))
                throw new InvalidDataException($"The block {block} of the graphics {tileset} is moved by animations that do not keep time.");
        }

        if (steps.Count == 0)
            steps.Add(("-", 1));
        if (steps.Select(step => step.Updates).Distinct().Count() != 1)
            throw new InvalidDataException($"The block {block} of the graphics {tileset} shows its pictures for different lengths of time, which a tile cannot.");
        string mirrored = $"{(mirroring.HasFlag(BlockMirroring.Horizontal) ? "_h" : "")}{(mirroring.HasFlag(BlockMirroring.Vertical) ? "_v" : "")}";
        string moved = going.Count == 0 ? "still" : string.Join('_', going.Select(animation => EngineSheets.FileName(animation.ToString())));
        var pictures = new JsonArray();
        foreach ((string shown, int _) in steps)
        {
            string frame = subject + string.Concat(inputs.Select(input =>
                $"|{input}={(going.Any(animation => InputOf(animation) == input) ? shown : "-")}"));
            string path = $"tiles/{EngineSheets.FileName(tileset)}/{block}{mirrored}_{moved}_{EngineSheets.FileName(shown == "-" ? "rest" : shown)}.png";
            if (!_tree.Holds(path))
            {
                RgbaPixel[] pixels = NamedBlockPixels(tileset, frame, mirroring)
                    ?? throw new InvalidDataException($"The graphics {tileset} have no picture named {frame}.");
                _tree.WritePicture(path, Block, Block, pixels);
                if (EngineRoomObjects.NamedBlockRegions(_sheets, tileset, frame,
                        mirroring.HasFlag(BlockMirroring.Horizontal), mirroring.HasFlag(BlockMirroring.Vertical)) is { } regions)
                    _tree.WriteRegionMap(path, Block, Block, regions);
                WriteFront(path, tileset, frame, mirroring);
            }

            pictures.Add(path);
        }

        return (pictures, steps[0].Updates);
    }

    /// <summary>The path of the picture of what a level draws behind itself, with the set of graphics it is drawn in left for the room to put in.</summary>
    internal static string LevelBackgroundPath(RoomStateSettings state, string graphics) =>
        $"backdrops/{graphics}/level_{EngineSheets.FileName(state.Level.Name)}.png";

    private readonly HashSet<string> _levelBackgrounds = new(StringComparer.Ordinal);

    /// <summary>
    /// Writes what a level draws behind itself as one picture, for a state whose room has no picture of its own behind
    /// it: every block of the level's background, where it lies. A level that draws nothing there gets a picture of
    /// one clear block, which shows as nothing. The part of those blocks that lies in front of what stands in the
    /// room is written beside the picture as a tile's is, where any block has one: the cartridge draws that part of
    /// its background over the things in the room and over the part of the room's own blocks that lies behind them.
    /// </summary>
    private void WriteLevelBackground(RoomStateSettings state, InstalledRoomLevel level, string tileset)
    {
        if (state.Background is not null)
            return;
        string path = LevelBackgroundPath(state, EngineSheets.FileName(tileset));
        if (!_levelBackgrounds.Add(path))
            return;
        int width = level.Size.X * Block, height = level.Size.Y * Block;
        var canvas = new RgbaPixel[width * height];

        // Where the set's sheet tells regions apart, the picture has its map of them, put together as it is.
        int[]? canvasRegions = _sheets.HasRegions(GameInstallationLayout.RoomBlockSheetDirectoryName, tileset) ? new int[width * height] : null;
        var front = new RgbaPixel[width * height];
        int[]? frontRegions = canvasRegions is null ? null : new int[width * height];
        bool any = false, anyInFront = false;
        for (int cell = 0; cell < level.Background.Length; cell++)
        {
            RoomBlockAppearance shown = level.Background[cell];
            if (shown.Shows != RoomBlockShown.Block)
                continue;
            RgbaPixel[]? pixels = EngineRoomObjects.BlockPixels(_sheets, tileset, shown.Block,
                shown.Mirroring.HasFlag(BlockMirroring.Horizontal), shown.Mirroring.HasFlag(BlockMirroring.Vertical));
            if (pixels is null)
                continue;
            any = true;
            int left = cell % level.Size.X * Block, top = cell / level.Size.X * Block;
            for (int y = 0; y < Block; y++)
                pixels.AsSpan(y * Block, Block).CopyTo(canvas.AsSpan((top + y) * width + left, Block));
            if (EngineRoomObjects.NamedBlockFrontPixels(_sheets, tileset, EngineRoomObjects.BlockFrame(shown.Block),
                    shown.Mirroring.HasFlag(BlockMirroring.Horizontal), shown.Mirroring.HasFlag(BlockMirroring.Vertical)) is { } part)
            {
                anyInFront = true;
                for (int y = 0; y < Block; y++)
                    part.AsSpan(y * Block, Block).CopyTo(front.AsSpan((top + y) * width + left, Block));
                if (frontRegions is not null)
                {
                    int[] partRegions = EngineRoomObjects.NamedBlockFrontRegions(_sheets, tileset, EngineRoomObjects.BlockFrame(shown.Block),
                            shown.Mirroring.HasFlag(BlockMirroring.Horizontal), shown.Mirroring.HasFlag(BlockMirroring.Vertical))
                        ?? throw new InvalidDataException($"The block {shown.Block} of the graphics {tileset} has a part in front and no regions for it.");
                    for (int y = 0; y < Block; y++)
                        partRegions.AsSpan(y * Block, Block).CopyTo(frontRegions.AsSpan((top + y) * width + left, Block));
                }
            }

            if (canvasRegions is null)
                continue;
            int[] regions = EngineRoomObjects.NamedBlockRegions(_sheets, tileset, EngineRoomObjects.BlockFrame(shown.Block),
                    shown.Mirroring.HasFlag(BlockMirroring.Horizontal), shown.Mirroring.HasFlag(BlockMirroring.Vertical))
                ?? throw new InvalidDataException($"The block {shown.Block} of the graphics {tileset} has a picture and no regions.");
            for (int y = 0; y < Block; y++)
                regions.AsSpan(y * Block, Block).CopyTo(canvasRegions.AsSpan((top + y) * width + left, Block));
        }

        if (any)
        {
            _tree.WritePicture(path, width, height, canvas);
            if (canvasRegions is not null)
                _tree.WriteRegionMap(path, width, height, canvasRegions);
            if (anyInFront)
            {
                string frontPath = EngineAssetTree.FrontPath(path);
                _tree.WritePicture(frontPath, width, height, front);
                if (frontRegions is not null)
                    _tree.WriteRegionMap(frontPath, width, height, frontRegions);
            }
        }
        else
        {
            _tree.WritePicture(path, Block, Block, new RgbaPixel[Block * Block]);
            if (canvasRegions is not null)
                _tree.WriteRegionMap(path, Block, Block, new int[Block * Block]);
        }
    }

    /// <summary>
    /// The pictures of the block an item hides in as one part of the old game's reveal shows them, as a timeline of a
    /// block type; the blocks whose pictures they are are added to <paramref name="shown"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">The part shows something that is not a frame of the items' own, or one that draws nothing.</exception>
    private JsonObject HiddenItemBlockLooks(CollectibleSequence part, List<int> shown)
    {
        string Look(CollectibleImage image)
        {
            CollectibleDraw draw = image.What is CollectibleImage.DrawnImage { Draw: RoomObjectFrameDraw<CollectibleDraw> framed }
                ? framed.Frame
                : throw new InvalidDataException("The block an item hides in shows something that is not a frame of the items' own.");
            int block = FrameBlock(GameInstallationLayout.RoomObjectCollectibleVisualDirectoryName, EngineSheets.FileName(draw.ToString()).Replace('_', '-'), 0, 0)?.Block
                ?? throw new InvalidDataException($"The frame {draw} of the block an item hides in draws nothing.");
            shown.Add(block);
            return SharedPicture(block);
        }

        JsonObject played = EngineTimelines.Of(CollectibleSequenceAnimations.Segments, part, false, Look, static (CollectiblePhase _) => null);
        return new JsonObject { ["steps"] = played["steps"]!.DeepClone() };
    }

    /// <summary>The old game's plant that a kind of air is, or null for air that is no plant.</summary>
    internal static SamusEaterObjectDefinition? PlantOf(SpecialAirReaction air) => SpecialAirReactions.InsideOf(air).Plant;

    /// <summary>The name a plant's look goes by in the names of its patches and in its data.</summary>
    internal static string PlantLook(SamusEaterDraw look) => EngineSheets.FileName(look.ToString());

    /// <summary>The data file of the block a plant of one kind is, as the layouts name it.</summary>
    internal static string PlantBlockFile(SpecialAirReaction air) => $"blocks/air_{EngineSheets.FileName(air.ToString())}.json";

    /// <summary>The looks of a plant other than the one the level has it in: those it chews with, and the one it is let go with.</summary>
    internal static IEnumerable<SamusEaterDraw> PlantLooks(SamusEaterObjectDefinition plant) =>
        plant.Chew.Select(frame => frame.Image).Append(plant.Released).Where(look => look != plant.Idle).Distinct();

    /// <summary>
    /// Gives a layout the patches of one plant: for each of its looks but the one the level has it in, the blocks the
    /// old game draws over it, with what kind of block each then is. They are named for where the plant's block is.
    /// </summary>
    private void PatchPlant(RoomDefinition room, RoomStateSettings state, int x, int y, SpecialAirReaction kind)
    {
        SamusEaterObjectDefinition plant = PlantOf(kind) ?? throw new InvalidDataException($"The air {kind} is no plant.");
        foreach (SamusEaterDraw look in PlantLooks(plant))
        {
            Patch(room, state, $"plant_{x}_{y}_{PlantLook(look)}", new Position16<RoomBlocks>(x, y),
                PatchCells(GameInstallationLayout.RoomObjectSamusEaterVisualDirectoryName,
                    (RoomObjectSamusEaterVisualFiles.VisualId(look), SamusEaterObjectDraws.Family.Shape(look))));
        }
    }

    /// <summary>
    /// Gives the layout of one state of a room the patch that is its treadmill running one way, as the old game's
    /// entrance treadmill makes it once the area has power: the row of blocks it runs along, which show nothing and
    /// carry what is on foot that way, and every block of the room the belt's animation moves, shown moving that
    /// way. The state's own effect does not set that animation going; the door come in by does.
    /// </summary>
    /// <exception cref="InvalidDataException">The layout has not been built.</exception>
    internal void PatchTreadmill(RoomDefinition room, RoomStateSettings state, string name, WreckedShipTreadmillDirection direction)
    {
        string path = PathOf(room, state);
        Layout layout = _layouts.TryGetValue(path, out Layout? built)
            ? built
            : throw new InvalidDataException($"The layout {path} has not been built, and is given the treadmill {name}.");
        BlockAnimation belt = BlockAnimationLoopDefinitions.TreadmillAnimations[direction];
        var origin = new Position16<RoomBlocks>(
            SuperMetroid.Core.Enemies.WreckedShip.WreckedShipTreadmillBlockDefinitions.BlockX,
            SuperMetroid.Core.Enemies.WreckedShip.WreckedShipTreadmillBlockDefinitions.BlockY);
        var cells = new List<PatchCell>();

        // The blocks the belt's animation moves, each as the level has it.
        _moving = [belt];
        for (int y = 0; y < layout.Level.Size.Y; y++)
        {
            for (int x = 0; x < layout.Level.Size.X; x++)
            {
                RoomTerrainBlock block = layout.Level.Foreground[y * layout.Level.Size.X + x];
                if (block.Appearance.Shows == RoomBlockShown.Block
                    && MovingPictures(layout.Tileset, block.Appearance.Block, block.Appearance.Mirroring) is { } moving && moving.Pictures.Count > 1)
                {
                    cells.Add(new PatchCell(x - origin.X, y - origin.Y, block.Collision, block.Appearance.Block, block.Appearance.Mirroring));
                }
            }
        }

        // The row it runs along, over whatever else is there.
        RoomBlockBehavior carries = direction == WreckedShipTreadmillDirection.Rightwards
            ? SuperMetroid.Core.Enemies.WreckedShip.WreckedShipTreadmillBlockDefinitions.RightwardsBehavior
            : SuperMetroid.Core.Enemies.WreckedShip.WreckedShipTreadmillBlockDefinitions.LeftwardsBehavior;
        for (int across = 0; across < SuperMetroid.Core.Enemies.WreckedShip.WreckedShipTreadmillBlockDefinitions.BlockCount; across++)
        {
            cells.Add(new PatchCell(
                across, 0, SuperMetroid.Core.Enemies.WreckedShip.WreckedShipTreadmillBlockDefinitions.RunningCollision, null, BlockMirroring.None, carries));
        }

        Patch(room, state, name, origin, cells, alsoMoving: belt);
    }

    /// <summary>
    /// The rules of the surface of quicksand, from the old game's own: one whose feet are in it stands there and is
    /// drawn down, faster while rising or falling; it lets her rise only so fast and takes her momentum; and the
    /// Gravity Suit eases it.
    /// </summary>
    private static JsonObject QuicksandSurface()
    {
        QuicksandSurfacePhysics plain = QuicksandDefinitions.WithoutGravitySuit, eased = QuicksandDefinitions.WithGravitySuit;
        return new JsonObject
        {
            ["solid"] = false,
            ["contact"] = "pulls_down",
            ["contact_speed"] = EngineNames.Number(plain.StationaryDisplacement),
            ["moving_contact_speed"] = EngineNames.Number(plain.MovingDisplacement),
            ["rise_limit"] = EngineNames.Number(plain.UpwardSpeedLimit),
            ["holds_up"] = true,
            ["feet_only"] = true,
            ["stops_momentum"] = true,
            // Standing on it she sinks only this far an update, the downward probe of a standing Samus being held to it.
            ["standing_limit"] = EngineNames.Number(QuicksandDefinitions.StationarySurfaceProbeLimit),
            ["eased"] = new JsonObject
            {
                ["by"] = new JsonArray("gravity_suit"),
                ["contact_speed"] = EngineNames.Number(eased.StationaryDisplacement),
                ["moving_contact_speed"] = EngineNames.Number(eased.MovingDisplacement),
                ["rise_limit"] = EngineNames.Number(eased.UpwardSpeedLimit),
            },
        };
    }

    /// <summary>
    /// The rules of a conveyor's block, from the old game's own: which way and how fast it carries, and whether only
    /// what is on foot. One that carries only where there is power is a conveyor in the rooms whose effect sets its
    /// belt going, and plain air in the others: null.
    /// </summary>
    private JsonObject? Conveyor(SpecialAirReaction air)
    {
        ConveyorBlock conveyor = ConveyorBlockDefinitions.Conveyors[air];
        bool rightward = conveyor.Direction == HorizontalDirection.Right;
        var rules = new JsonObject
        {
            ["solid"] = false,
            ["contact"] = "carries",
            ["contact_speed"] = EngineNames.Number(rightward ? ConveyorBlockDefinitions.RightwardCarry : ConveyorBlockDefinitions.LeftwardCarry),
        };
        switch (conveyor.Condition)
        {
            case ConveyorCarryCondition.Always:
                return rules;
            case ConveyorCarryCondition.OnFootWithPower:
                BlockAnimation belt = BlockAnimationLoopDefinitions.TreadmillAnimations[
                    rightward ? WreckedShipTreadmillDirection.Rightwards : WreckedShipTreadmillDirection.Leftwards];
                if (!_moving.Contains(belt))
                    return null;
                rules["carries_only_level"] = true;
                return rules;
            default:
                throw new InvalidDataException($"A conveyor carries on a condition this does not know: {conveyor.Condition}.");
        }
    }

    /// <summary>The hit kinds that show a hidden block for what it is without breaking it: a bomb's blast.</summary>
    private static JsonArray Revealers() => ["bomb"];

    /// <summary>The picture of one of the blocks every set of graphics has, as a block type names it: the room puts its set of graphics in.</summary>
    private static string SharedPicture(int block) => $"tiles/{{graphics}}/{block}.png";

    /// <summary>
    /// Adds to a block's rules what one of the old game's changes to a block makes of it: the pictures of its breaking
    /// up, those of its growing back, how long from its breaking to its return, and the sound. Gives the blocks whose
    /// pictures those are, which every set of graphics that shows the block must have.
    /// </summary>
    /// <exception cref="InvalidDataException">The change shows something other than a block breaking up and coming back.</exception>
    private static List<int> AddBreakup(JsonObject rules, IEnumerable<(BreakupFrame Shown, int Updates, SoundEffectId? Sound, SoundRequestPriority Priority)> frames)
    {
        var breaking = new JsonArray();
        var regrowing = new JsonArray();
        var blocks = new List<int>();
        int updatesInAll = 0;
        bool gone = false, back = false;
        string? sound = null;
        foreach ((BreakupFrame shown, int updates, SoundEffectId? heard, SoundRequestPriority priority) in frames)
        {
            // The sound is named with the priority the change asks for it with.
            if (heard is { } effect && EngineNames.Sound(effect) is not null)
                sound ??= EngineNames.Sound(effect, priority);
            if (shown.Whole)
            {
                back = true;
                continue;
            }

            if (back)
                throw new InvalidDataException("A block breaks up again after it has come back, which a block type cannot say.");
            updatesInAll += updates;
            if (shown.Block is not { } block)
            {
                gone = true;
                continue;
            }

            blocks.Add(block);
            (gone ? regrowing : breaking).Add(new JsonObject { ["picture"] = SharedPicture(block), ["updates"] = updates });
        }

        if (!gone)
            throw new InvalidDataException("A block breaks up and is never gone.");
        if (breaking.Count > 0)
            rules["breaking"] = new JsonObject { ["steps"] = breaking };
        if (back)
        {
            if (regrowing.Count > 0)
                rules["regrowing"] = new JsonObject { ["steps"] = regrowing };
            rules["regrows_after"] = updatesInAll;
        }
        else if (regrowing.Count > 0)
        {
            throw new InvalidDataException("A block grows back and is never whole again.");
        }

        if (sound is not null)
            rules["break_sound"] = sound;
        return blocks;
    }

    /// <summary>
    /// The sound of a way of breaking a block that differs from another way only in its sound, named with the priority
    /// it is asked for with: a block type says the one breaking and a sound for each kind of hit.
    /// </summary>
    /// <exception cref="InvalidDataException">The two ways differ in more than their sound, or this one has none.</exception>
    private string BreakSoundOf(BlockReactionChange plain, BlockReactionChange heard)
    {
        var plainFrames = Frames(plain).ToList();
        var heardFrames = Frames(heard).ToList();
        if (plain.FirstFrameDelay != heard.FirstFrameDelay
            || !plainFrames.Select(frame => (frame.Shown, frame.Updates)).SequenceEqual(heardFrames.Select(frame => (frame.Shown, frame.Updates))))
        {
            throw new InvalidDataException("Two ways of breaking one block differ in more than their sound, which a block type cannot say.");
        }

        foreach ((BreakupFrame _, int _, SoundEffectId? sound, SoundRequestPriority priority) in heardFrames)
        {
            if (sound is { } effect && EngineNames.Sound(effect) is not null)
                return EngineNames.Sound(effect, priority);
        }

        throw new InvalidDataException("A way of breaking a block that is told from another by its sound has none.");
    }

    /// <summary>
    /// What one frame of a block's breaking up shows: the block whole again, or the block whose picture stands for it
    /// in pieces, or nothing while it is gone.
    /// </summary>
    private readonly partial record struct BreakupFrame(bool Whole, int? Block);

    /// <summary>What one picture of the old game's changes to a breaking block shows.</summary>
    private BreakupFrame Shown(BlockReactionImage image)
    {
        // Anything but a stage of the breaking is the block as it was, or as its kind shows it once it has come back.
        if (image is not BlockReactionImage.Drawn { Draw: RoomObjectFrameDraw<ShotBlockObjectDraws.BreakupDraw> draw })
            return new BreakupFrame(true, null);
        if (draw.Frame.Stage == BlockBreakupStage.Gone)
            return new BreakupFrame(false, null);
        return new BreakupFrame(false, BreakupBlock(draw));
    }

    /// <summary>
    /// The picture one of the old game's frames of a block breaking up shows, as a block type names it; none for the
    /// frame where the block is gone, which shows nothing.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame is not a stage of a block breaking up, or draws nothing.</exception>
    internal string? BreakupPicture(RoomObjectDraw draw)
    {
        if (draw is not RoomObjectFrameDraw<ShotBlockObjectDraws.BreakupDraw> breakup)
            throw new InvalidDataException($"The frame {draw} is not a stage of a block breaking up.");
        return breakup.Frame.Stage == BlockBreakupStage.Gone ? null : SharedPicture(BreakupBlock(breakup));
    }

    /// <summary>The block one stage of a block breaking up, short of its being gone, draws.</summary>
    /// <exception cref="InvalidDataException">The stage draws nothing.</exception>
    private int BreakupBlock(RoomObjectFrameDraw<ShotBlockObjectDraws.BreakupDraw> draw) =>
        FrameBlock(GameInstallationLayout.RoomObjectShotBlockVisualDirectoryName, RoomObjectShotBlockVisualFiles.VisualId(draw), 0, 0)?.Block
            ?? throw new InvalidDataException($"The breaking block's frame {draw.Frame.Stage} draws nothing.");

    /// <summary>The blocks the X-ray scope shows a reacting block as, by the kind of block and what it does: one for each of its tiles, row by row.</summary>
    private Dictionary<(string Collision, string Reaction), int[]>? _sight;

    /// <summary>
    /// Adds to a block's rules that the X-ray scope shows it for what it is, with the pictures the installation keeps
    /// for the scope, and what it looks like when it comes back from breaking: a block of one tile is as the level
    /// has it again, and a larger one shows what it is. Gives the blocks whose pictures those are; none for a kind
    /// of block the scope shows nothing for.
    /// </summary>
    /// <exception cref="InvalidDataException">The scope's pictures are not one for each tile, or differ from what a bomb shows.</exception>
    private List<int> AddSight(JsonObject rules, RoomCollisionType collision, string reaction, int width, int height)
    {
        if (!Sight().TryGetValue((collision.ToString(), reaction), out int[]? parts))
            return [];
        if (parts.Length != width * height)
            throw new InvalidDataException($"The X-ray scope shows the {collision} block {reaction} with {parts.Length} pictures, and it has {width * height} tiles.");
        rules["revealed_by_sight"] = true;
        JsonObject pictures = rules["pictures"]?.AsObject() ?? [];
        if (pictures["intact"]?.GetValue<string>() is { } bombed && parts.Any(part => SharedPicture(part) != bombed))
            throw new InvalidDataException($"The X-ray scope and a bomb show the {collision} block {reaction} differently.");
        pictures.Remove("intact");
        if (parts.Distinct().Count() == 1)
            pictures["intact"] = SharedPicture(parts[0]);
        else
            pictures["intact_parts"] = new JsonArray([.. parts.Select(part => (JsonNode)SharedPicture(part))]);
        if (rules["pictures"] is null)
            rules["pictures"] = pictures;
        if (parts.Length == 1)
            rules["hides_again"] = true;
        return [.. parts];
    }

    /// <summary>
    /// What the X-ray scope shows each kind of block as, read once from the installation. A kind the old game lists
    /// with no reaction is kept under an empty one.
    /// </summary>
    private Dictionary<(string Collision, string Reaction), int[]> Sight()
    {
        if (_sight is not null)
            return _sight;
        _sight = [];
        string file = Path.Combine(_content, GameInstallationLayout.XrayRevealVisualDirectoryName, "reveals.json");
        JsonArray entries = JsonNode.Parse(File.ReadAllText(file))?["entries"]?.AsArray()
            ?? throw new InvalidDataException($"The file {file} lists nothing the X-ray scope shows.");
        foreach (JsonNode? entry in entries)
        {
            JsonObject blocks = entry!["blocks"]!.AsObject();

            // The parts of a block are named for where they lie; these are the names in the order of its tiles.
            string[] places = ["block", "left", "top", "topLeft", "right", "topRight", "bottom", "bottomLeft", "bottomRight"];
            int[] shown = [.. places.Where(blocks.ContainsKey).Select(place => blocks[place]!.GetValue<int>())];
            if (shown.Length != blocks.Count)
                throw new InvalidDataException($"The file {file} names a part of a block this does not know.");
            string collision = entry["collisionType"]!.GetValue<string>();
            JsonArray reactions = entry["reactions"]!.AsArray();
            if (reactions.Count == 0)
                _sight[(collision, "")] = shown;
            foreach (JsonNode? named in reactions)
                _sight[(collision, named!.GetValue<string>())] = shown;
        }

        return _sight;
    }

    /// <summary>
    /// The picture a tile that is no reacting block shows to the X-ray scope in place of its own: nothing for a wall
    /// that is air, the old game's own picture for the few solid kinds it marks. Null for a tile the scope shows as
    /// it always is.
    /// </summary>
    private string? SeenAs(RoomTerrainBlock terrain, RoomBlockBehavior behavior, string tileset, string? own)
    {
        string reaction = terrain.Collision switch
        {
            RoomCollisionType.ShootableBlock or RoomCollisionType.ShootableAir => $"{behavior.Shootable}",
            RoomCollisionType.BombableBlock or RoomCollisionType.BombableAir => $"{behavior.Bombable}",
            RoomCollisionType.SpecialBlock => $"{behavior.Special}",
            RoomCollisionType.SpecialAir => $"{behavior.SpecialAir}",
            RoomCollisionType.SpikeBlock or RoomCollisionType.SpikeAir => $"{behavior.Spike}",
            RoomCollisionType.GrappleBlock => $"{behavior.Grapple}",
            _ => "",
        };
        if (!Sight().TryGetValue((terrain.Collision.ToString(), reaction), out int[]? parts) || parts.Length != 1)
            return null;
        string seen = Picture(tileset, parts[0], BlockMirroring.None) ?? "";
        return seen == (own ?? "") ? null : seen;
    }

    /// <summary>The frames of a change to a block that undoes itself.</summary>
    private IEnumerable<(BreakupFrame Shown, int Updates, SoundEffectId? Sound, SoundRequestPriority Priority)> Frames(BlockReactionChange change) =>
        change.Frames.Select(frame => (Shown(frame.Image), frame.Updates, frame.SoundBefore, frame.SoundPriority));

    /// <summary>The frames of a change to the terrain that is a single block breaking up.</summary>
    private IEnumerable<(BreakupFrame Shown, int Updates, SoundEffectId? Sound, SoundRequestPriority Priority)> Frames(TerrainChange change) =>
        change.Frames.Select(frame => (Shown(new BlockReactionImage.Drawn(frame.Draw)), frame.Updates, frame.SoundBefore, frame.SoundPriority));

    /// <summary>The frames of a change to a block that lasts.</summary>
    private IEnumerable<(BreakupFrame Shown, int Updates, SoundEffectId? Sound, SoundRequestPriority Priority)> Frames(LastingBlockReactionChange change) =>
        change.Frames.Select(frame => (Shown(frame.Image), frame.Updates, frame.SoundBefore, frame.SoundPriority));

    /// <summary>
    /// The frames of a block the grapple beam breaks, from when it starts to break: before that the old game shows it
    /// whole for as long as it holds, which is not part of its breaking.
    /// </summary>
    /// <exception cref="InvalidDataException">The change shows something that is not a picture of a grapple block.</exception>
    private IEnumerable<(BreakupFrame Shown, int Updates, SoundEffectId? Sound, SoundRequestPriority Priority)> GrappleFrames(BlockReactionChange change) =>
        GrappleFrames(change, held => { });

    /// <summary>How many updates the old game shows a block the grapple beam breaks whole, once the beam has it, before it breaks.</summary>
    /// <exception cref="InvalidDataException">The change does not begin with the block shown whole.</exception>
    private int GrappleHeldUpdates(BlockReactionChange change)
    {
        int held = 0;
        foreach ((BreakupFrame _, int _, SoundEffectId? _, SoundRequestPriority _) in GrappleFrames(change, updates => held += updates))
        {
        }

        return held > 0 ? held : throw new InvalidDataException("A grapple block that breaks is never shown whole before it does.");
    }

    private IEnumerable<(BreakupFrame Shown, int Updates, SoundEffectId? Sound, SoundRequestPriority Priority)> GrappleFrames(BlockReactionChange change, Action<int> heldFor)
    {
        const string directory = GameInstallationLayout.RoomObjectGrappleBlockVisualDirectoryName;
        bool breaking = false;
        foreach (BlockReactionFrame frame in change.Frames)
        {
            if (frame.Image is not BlockReactionImage.Drawn { Draw: RoomObjectFrameDraw<GrappleBlockDraw> draw })
            {
                yield return (new BreakupFrame(true, null), frame.Updates, frame.SoundBefore, frame.SoundPriority);
                continue;
            }

            if (draw.Frame == GrappleBlockDraw.Intact)
            {
                if (breaking)
                    throw new InvalidDataException("A grapple block is shown whole in the middle of its breaking.");
                heldFor(frame.Updates);
                continue;
            }

            breaking = true;
            int? block = FrameBlock(directory, RoomObjectGrappleBlockVisualFiles.VisualId(draw.Frame), 0, 0)?.Block;
            yield return (new BreakupFrame(false, block), frame.Updates, frame.SoundBefore, frame.SoundPriority);
        }
    }

    /// <summary>
    /// Adds to a block's rules that a bomb shows it for what it is, with the picture the old game's change draws over
    /// it. Gives the block whose picture that is.
    /// </summary>
    /// <exception cref="InvalidDataException">The change draws other than one picture, or different pictures over the block's parts.</exception>
    private int AddReveal(JsonObject rules, LastingBlockReactionChange reveal, int width, int height)
    {
        if (reveal.Frames.Count != 1)
            throw new InvalidDataException($"A bomb's showing of a block draws {reveal.Frames.Count} pictures, where one was expected.");
        RoomObjectDraw draw = reveal.Frames[0].Draw;
        (string directory, string frame) = draw switch
        {
            _ when draw == SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw =>
                (GameInstallationLayout.RoomObjectSpeedBoosterVisualDirectoryName, RoomObjectSpeedBoosterVisualFiles.VisualId(draw)),
            RoomObjectFrameDraw<ContactCrumbleRestoreDraw> or RoomObjectFrameDraw<BombBlockRestoreDraw> =>
                (GameInstallationLayout.RoomObjectLinkedRestoreVisualDirectoryName, RoomObjectLinkedRestoreVisualFiles.VisualId(draw)),
            _ => (GameInstallationLayout.RoomObjectRevealAndWallVisualDirectoryName, RoomObjectRevealAndWallVisualFiles.VisualId(draw)),
        };
        int? shown = null;
        for (int down = 0; down < height; down++)
        {
            for (int across = 0; across < width; across++)
            {
                int block = FrameBlock(directory, frame, across, down)?.Block
                    ?? throw new InvalidDataException($"The frame {frame} that shows a block for what it is draws nothing at one of its parts.");
                if ((shown ??= block) != block)
                    throw new InvalidDataException($"The frame {frame} shows a block for what it is with different pictures at its parts, which a block type cannot say.");
            }
        }

        rules["hidden_until"] = Revealers();
        rules["pictures"] = new JsonObject { ["intact"] = SharedPicture(shown!.Value) };
        return shown.Value;
    }
}
