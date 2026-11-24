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
    /// one clear block, which shows as nothing.
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
        bool any = false;
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
        }

        if (any)
            _tree.WritePicture(path, width, height, canvas);
        else
            _tree.WritePicture(path, Block, Block, new RgbaPixel[Block * Block]);
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
    private static List<int> AddBreakup(JsonObject rules, IEnumerable<(BreakupFrame Shown, int Updates, SoundEffectId? Sound)> frames)
    {
        var breaking = new JsonArray();
        var regrowing = new JsonArray();
        var blocks = new List<int>();
        int updatesInAll = 0;
        bool gone = false, back = false;
        string? sound = null;
        foreach ((BreakupFrame shown, int updates, SoundEffectId? heard) in frames)
        {
            if (heard is { } effect)
                sound ??= EngineNames.Sound(effect);
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
        int block = FrameBlock(GameInstallationLayout.RoomObjectShotBlockVisualDirectoryName, RoomObjectShotBlockVisualFiles.VisualId(draw), 0, 0)?.Block
            ?? throw new InvalidDataException($"The breaking block's frame {draw.Frame.Stage} draws nothing.");
        return new BreakupFrame(false, block);
    }

    /// <summary>The frames of a change to a block that undoes itself.</summary>
    private IEnumerable<(BreakupFrame Shown, int Updates, SoundEffectId? Sound)> Frames(BlockReactionChange change) =>
        change.Frames.Select(frame => (Shown(frame.Image), frame.Updates, frame.SoundBefore));

    /// <summary>The frames of a change to a block that lasts.</summary>
    private IEnumerable<(BreakupFrame Shown, int Updates, SoundEffectId? Sound)> Frames(LastingBlockReactionChange change) =>
        change.Frames.Select(frame => (Shown(frame.Image), frame.Updates, frame.SoundBefore));

    /// <summary>
    /// The frames of a block the grapple beam breaks, from when it starts to break: before that the old game shows it
    /// whole for as long as it holds, which is not part of its breaking.
    /// </summary>
    /// <exception cref="InvalidDataException">The change shows something that is not a picture of a grapple block.</exception>
    private IEnumerable<(BreakupFrame Shown, int Updates, SoundEffectId? Sound)> GrappleFrames(BlockReactionChange change)
    {
        const string directory = GameInstallationLayout.RoomObjectGrappleBlockVisualDirectoryName;
        bool breaking = false;
        foreach (BlockReactionFrame frame in change.Frames)
        {
            if (frame.Image is not BlockReactionImage.Drawn { Draw: RoomObjectFrameDraw<GrappleBlockDraw> draw })
            {
                yield return (new BreakupFrame(true, null), frame.Updates, frame.SoundBefore);
                continue;
            }

            if (draw.Frame == GrappleBlockDraw.Intact)
            {
                if (breaking)
                    throw new InvalidDataException("A grapple block is shown whole in the middle of its breaking.");
                continue;
            }

            breaking = true;
            int? block = FrameBlock(directory, RoomObjectGrappleBlockVisualFiles.VisualId(draw.Frame), 0, 0)?.Block;
            yield return (new BreakupFrame(false, block), frame.Updates, frame.SoundBefore);
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
