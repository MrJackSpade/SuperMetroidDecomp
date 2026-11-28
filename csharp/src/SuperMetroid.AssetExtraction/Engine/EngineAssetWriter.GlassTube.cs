using System.Globalization;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The glass tube of Maridia. The tube itself is its room's own blocks, so breaking it is a patch of the room's
/// layouts; its crack is an effect, and its shards and the bubbles let out are its own, shown by their timelines.
/// </summary>
public sealed partial class EngineAssetWriter
{
    private EngineLayouts? _layouts;

    /// <summary>The room layouts, to be given patches; there once they have been built.</summary>
    private EngineLayouts Layouts => _layouts ?? throw new InvalidOperationException("The layouts are given a patch before they are built.");

    /// <summary>Every placement of one kind of the old game's room objects, with the room and the state of it the placement is in.</summary>
    private static IEnumerable<(RoomDefinition Room, RoomStateSettings State, RoomObjectPlacement Placement)> Placed<TSetup>()
        where TSetup : RoomObjectPlacementSetup
    {
        foreach ((RoomDefinition room, RoomStateSettings state) in EngineLayouts.States())
        {
            foreach (RoomObjectPlacement placement in state.ObjectPopulation.Placements.ToArray())
            {
                if (placement.Setup is TSetup)
                    yield return (room, state, placement);
            }
        }
    }

    /// <summary>The name of the patch that is the glass tube broken open.</summary>
    private const string TubeOpenPatch = "tube_open";

    /// <summary>Writes the glass tube: the patch that opens it, its effects and its data.</summary>
    private void WriteGlassTube()
    {
        const string directory = GameInstallationLayout.RoomObjectNoobTubeVisualDirectoryName, folder = "objects/glass_tube";
        static string Frame(NoobTubeDraw draw) => EngineSheets.FileName(draw.ToString()).Replace('_', '-');
        static (string, RoomBlockShape) Drawn(NoobTubeDraw draw) => (Frame(draw), NoobTubeObjectDraws.Family.Shape(draw));

        // Broken, the tube's upper rows and then its lower rows are cleared, and last the block the old game placed
        // it on shows as its rim. Every state of the room that has the tube has the patch: one the tube is already
        // broken in is given it too, and there it changes nothing.
        foreach ((RoomDefinition room, RoomStateSettings state, RoomObjectPlacement placement) in Placed<NoobTubeSetup>())
        {
            Layouts.Patch(room, state, TubeOpenPatch, placement.Block, Layouts.PatchCells(
                directory, Drawn(NoobTubeDraw.UpperRowsOpen), Drawn(NoobTubeDraw.LowerRowsOpen), Drawn(NoobTubeDraw.Opened)));
        }

        EngineSheetPictures pictures = _sheets.CutSpawned(
            _tree, folder, EngineSheets.SharedSpawner, NoobTubeProjectileAnimations.ShownFrames.Select(EnemySheetDefinitions.ProjectileFrameName));
        string Picture(EnemyProjectileFrameId frame) => pictures.Of(EnemySheetDefinitions.ProjectileFrameName(frame));
        JsonObject Placed(JsonArray steps) => new()
        {
            ["steps"] = steps,
            ["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY },
        };
        static string Drift(SignedEightEight across, SignedEightEight down) =>
            $"drift:{across.Pixels.ToString(CultureInfo.InvariantCulture)}:{down.Pixels.ToString(CultureInfo.InvariantCulture)}";

        // The crack spreads; then the whole pane flickers as it bursts, and falls.
        string crack = $"{folder}/crack.json";
        {
            (List<IAnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>> beats, List<IAnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>>? cycle) =
                EngineTimelines.Played(NoobTubeProjectileAnimations.Segments, NoobTubeProjectileAnimations.StartingAt[NoobTubeProjectileSegment.Crack]);
            if (cycle is not null)
                throw new InvalidDataException("The glass tube's crack goes round and round, which an effect that ends cannot.");
            var steps = new JsonArray();
            bool flickers = false;
            string? cue = null;
            void Add(string picture, int updates)
            {
                var step = new JsonObject { ["picture"] = picture, ["updates"] = updates };
                if (cue is not null)
                    step["cues"] = new JsonArray(cue);
                cue = null;
                steps.Add(step);
            }

            foreach (IAnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> beat in beats)
            {
                switch (beat)
                {
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame frame when flickers:
                        // Shown and hidden by turns, an update each.
                        for (int update = 0; update < frame.Duration; update++)
                            Add((steps.Count & 1) == 0 ? Picture(frame.Shown.Shown) : "", 1);
                        break;
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame frame:
                        Add(Picture(frame.Shown.Shown), frame.Duration);
                        break;
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event { Raised.Kind: NoobTubeProjectileCueKind.StartCrackFlicker }:
                        flickers = true;
                        break;
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event { Raised.Kind: NoobTubeProjectileCueKind.StartCrackFall }:
                        flickers = false;
                        cue = Drift(SignedEightEight.Zero, NoobTubeDebrisDefinitions.CrackFallVelocity);
                        break;
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event { Raised.Kind: NoobTubeProjectileCueKind.Delete }:
                        break;
                    default:
                        throw new InvalidDataException($"The glass tube's crack has a beat this does not know: {beat}.");
                }
            }

            _tree.WriteData(crack, Placed(steps));
        }

        // The shards and the bubbles are the tube's own, which it moves and shows by their timelines' cues: a shard
        // shown at its mirror image too, when a piece sets off, when it begins to sway, and when it is gone. Each frame
        // is a step, with the cues raised before it; cues raised after the last frame come on a step of their own. A
        // shard the old game hides on alternate updates is shown on every one, the owner's ruling (#3743, #2639).
        static string? PieceCue(NoobTubeProjectileCue cue) => cue.Kind switch
        {
            NoobTubeProjectileCueKind.FlickerReflected => "reflected",
            NoobTubeProjectileCueKind.FlickerHidden => null,
            NoobTubeProjectileCueKind.StartBubbleRise => "rise",
            NoobTubeProjectileCueKind.AssignShardFallingAngle or NoobTubeProjectileCueKind.AssignBubbleFallingAngle => "sway",
            NoobTubeProjectileCueKind.StartShardFall or NoobTubeProjectileCueKind.StartBubbleFall => "fall",
            NoobTubeProjectileCueKind.Delete => "end",
            _ => throw new InvalidDataException($"A piece of the glass tube raises {cue.Kind}, which this does not know."),
        };
        JsonObject Piece(Animation<NoobTubeProjectileSegment> animation)
        {
            (List<IAnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>> beats, List<IAnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>>? cycle) =
                EngineTimelines.Played(NoobTubeProjectileAnimations.Segments, animation);
            if (cycle is not null)
                throw new InvalidDataException("A piece of the glass tube goes round and round, where the tube's pieces are each gone in the end.");
            var steps = new JsonArray();
            var cues = new JsonArray();
            void Add(string picture, int updates)
            {
                var step = new JsonObject { ["picture"] = picture, ["updates"] = updates };
                if (cues.Count > 0)
                {
                    step["cues"] = cues;
                    cues = new JsonArray();
                }

                steps.Add(step);
            }

            foreach (IAnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> beat in beats)
            {
                switch (beat)
                {
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame frame:
                        Add(Picture(frame.Shown.Shown), frame.Duration);
                        break;
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event raised:
                        if (PieceCue(raised.Raised) is { } cue)
                            cues.Add(cue);
                        break;
                    default:
                        throw new InvalidDataException($"A piece of the glass tube has a beat this does not know: {beat}.");
                }
            }

            if (cues.Count > 0)
                Add("", 1);
            return Placed(steps);
        }

        // Each shard flies out from the crack and then sinks. The old game draws one, by turns, at its own column and
        // as its mirror image across the tube; the Godot game draws both at once (#3743). The mirror images' pictures
        // are given beside its own.
        var shards = new JsonArray();
        for (int index = 0; index < NoobTubeDebrisDefinitions.Shards.Length; index++)
        {
            NoobTubeShard shard = NoobTubeDebrisDefinitions.Shards[index].Start is NoobTubePieceStart.ShardFlight flight
                ? flight.Shard
                : throw new InvalidDataException("A shard of the glass tube does not start as a shard in flight.");
            string file = $"{folder}/shard_{index + 1}.json";
            _tree.WriteData(file, Piece(NoobTubeProjectileAnimations.ShardAnimations[shard]));
            shards.Add(file);
        }

        var mirrors = new JsonObject();
        foreach (NoobTubeProjectileFrame frame in NoobTubeProjectileAnimations.Segments.Frames)
        {
            if (frame.Mirrored is not { } mirrored)
                continue;
            string own = Picture(frame.Shown), image = Picture(mirrored);
            if (mirrors[own]?.GetValue<string>() is { } known && known != image)
                throw new InvalidDataException($"The glass tube's shard picture {own} is mirrored by two pictures.");
            mirrors[own] = image;
        }

        // The bubbles let out rise the whole of their short lives, and sway as they go.
        string bubble = $"{folder}/bubble.json";
        _tree.WriteData(bubble, Piece(NoobTubeProjectileAnimations.StartingAt[NoobTubeProjectileSegment.ReleasedAirBubble]));

        const int block = EngineRoomObjects.Block;
        new EngineObjectFile(_tree, "objects/glass_tube.json", "GlassTube")
            // The tube shows as the room's own blocks do; only what breaking it leaves is the tube's own to show.
            .Value("intact", "")
            .Value("cracking", "")
            .Value("shattered", "")
            .Value("crack", crack)
            .Value("bubble", bubble)
            .Value("open_patch", TubeOpenPatch)
            .Value("break_sound", EngineNames.SoundOrStop(NoobTubeObjectDefinitions.BreakSound))
            .Value("dud_sound", EngineNames.SoundOrStop(NoobTubeObjectDefinitions.IneffectiveShotSound))
            .Group("shards", shards)
            .Group("mirrors", mirrors)
            // The one block a power bomb's blast must reach.
            .Collider("Hurtbox", 0, 0, block, block)
            .Write();
    }
}
