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
/// layouts; its crack, its shards and the bubbles let out are effects.
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
        var origins = new HashSet<Position16<RoomBlocks>>();
        foreach ((RoomDefinition room, RoomStateSettings state, RoomObjectPlacement placement) in Placed<NoobTubeSetup>())
        {
            Layouts.Patch(room, state, TubeOpenPatch, placement.Block, Layouts.PatchCells(
                directory, Drawn(NoobTubeDraw.UpperRowsOpen), Drawn(NoobTubeDraw.LowerRowsOpen), Drawn(NoobTubeDraw.Opened)));
            origins.Add(placement.Block);
        }

        if (origins.Count != 1)
            throw new InvalidDataException($"The glass tube stands on {origins.Count} different blocks; its shards are written for one.");
        int tubeX = origins.Single().X * EngineRoomObjects.Block;

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

        // Each shard flies out from the crack and then sinks. The old game draws each as two, by turns: itself, and
        // its mirror image as far from the room's other side, which here is the same effect's picture set across.
        var shards = new JsonArray();
        for (int index = 0; index < NoobTubeDebrisDefinitions.Shards.Length; index++)
        {
            NoobTubePiece piece = NoobTubeDebrisDefinitions.Shards[index];
            NoobTubeShard shard = piece.Start is NoobTubePieceStart.ShardFlight flight
                ? flight.Shard
                : throw new InvalidDataException("A shard of the glass tube does not start as a shard in flight.");
            (List<IAnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>> beats, List<IAnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>>? cycle) =
                EngineTimelines.Played(NoobTubeProjectileAnimations.Segments, NoobTubeProjectileAnimations.ShardAnimations[shard]);
            if (cycle is not null)
                throw new InvalidDataException("A shard of the glass tube goes round and round, which an effect that ends cannot.");
            var steps = new JsonArray();
            decimal column = tubeX + piece.Offset.X;
            bool falling = false, reflected = false;
            string? cue = null;
            foreach (IAnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> beat in beats)
            {
                switch (beat)
                {
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame frame:
                        for (int update = 0; update < frame.Duration; update++)
                        {
                            bool own = (steps.Count & 1) == 0;
                            var step = new JsonObject { ["updates"] = 1 };
                            if (own)
                            {
                                step["picture"] = Picture(frame.Shown.Shown);
                            }
                            else if (reflected && frame.Shown.Mirrored is { } mirrored)
                            {
                                step["picture"] = Picture(mirrored);
                                step["offset"] = new JsonObject { ["x"] = NoobTubeDebrisDefinitions.ShardMirrorSpan - 2 * (int)Math.Floor(column), ["y"] = 0 };
                            }
                            else
                            {
                                step["picture"] = "";
                            }

                            if (cue is not null)
                                step["cues"] = new JsonArray(cue);
                            cue = null;
                            steps.Add(step);
                            if (!falling)
                                column += piece.Velocity.X.Pixels;
                        }

                        break;
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event { Raised.Kind: NoobTubeProjectileCueKind.FlickerReflected }:
                        reflected = true;
                        break;
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event { Raised.Kind: NoobTubeProjectileCueKind.FlickerHidden }:
                        reflected = false;
                        break;
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event { Raised.Kind: NoobTubeProjectileCueKind.StartShardFall }:
                        falling = true;
                        cue = Drift(SignedEightEight.Zero, NoobTubeDebrisDefinitions.ShardFallYVelocity);
                        break;
                    case AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event { Raised.Kind: NoobTubeProjectileCueKind.AssignShardFallingAngle or NoobTubeProjectileCueKind.Delete }:
                        break;
                    default:
                        throw new InvalidDataException($"A shard of the glass tube has a beat this does not know: {beat}.");
                }
            }

            string file = $"{folder}/shard_{index + 1}.json";
            _tree.WriteData(file, Placed(steps));
            shards.Add(file);
        }

        // The bubbles let out rise the whole of their short lives.
        string bubble = $"{folder}/bubble.json";
        JsonObject bubbles = EngineTimelines.Of(
            NoobTubeProjectileAnimations.Segments, NoobTubeProjectileAnimations.StartingAt[NoobTubeProjectileSegment.ReleasedAirBubble],
            frame => Picture(frame.Shown), static (NoobTubeProjectileCue _) => null);
        bubbles["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
        _tree.WriteData(bubble, bubbles);

        const int block = EngineRoomObjects.Block;
        new EngineObjectFile(_tree, "objects/glass_tube.json", "GlassTube")
            // The tube shows as the room's own blocks do; only what breaking it leaves is the tube's own to show.
            .Value("intact", "")
            .Value("cracking", "")
            .Value("shattered", "")
            .Value("crack", crack)
            .Value("bubble", bubble)
            .Value("open_patch", TubeOpenPatch)
            .Value("break_sound", EngineNames.SoundOrSilence(NoobTubeObjectDefinitions.BreakSound))
            .Value("dud_sound", EngineNames.SoundOrSilence(NoobTubeObjectDefinitions.IneffectiveShotSound))
            .Group("shards", shards)
            // The one block a power bomb's blast must reach.
            .Collider("Hurtbox", 0, 0, block, block)
            .Write();
    }
}
