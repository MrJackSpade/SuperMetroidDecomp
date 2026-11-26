using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The two Chozo statues that come to life, and what each does to its room's blocks.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The patch that is the plug below the Lower Norfair statue gone.</summary>
    private const string ChozoPlugGonePatch = "chozo_plug_gone";

    /// <summary>The patch that opens the Wrecked Ship statue's way down through the spikes.</summary>
    private const string ChozoWayOpenPatch = "chozo_way_open";

    /// <summary>The patch that shuts the Wrecked Ship statue's way, as the old game sets the room up.</summary>
    private const string ChozoWayShutPatch = "chozo_way_shut";

    /// <summary>What the patches of the blocks the Wrecked Ship statue's feet break through are named with, before each block's column and row.</summary>
    private const string ChozoPlugPatches = "chozo_plug";

    /// <summary>Writes each statue's data file and gives its room's layouts the patches it names.</summary>
    private void WriteChozoStatues()
    {
        const string directory = GameInstallationLayout.RoomObjectChozoStatueVisualDirectoryName;
        static (string, RoomBlockShape) Drawn(ChozoStatueDraw draw) =>
            (RoomObjectChozoStatueVisualFiles.VisualId(ChozoStatueDraws.Family.Of(draw)), ChozoStatueDraws.Family.Shape(draw));

        static string? Cue(ChozoStatueCue cue)
        {
            switch (cue)
            {
                case ChozoStatueCue.Move move:
                {
                    // How far it steps, toward the side it carries to, and where it holds Samus on that side.
                    ChozoCarryStepDefinition motion = ChozoCarry.Read(move.Step);
                    decimal stride = motion.Velocity.Pixels;
                    if (stride != decimal.Truncate(stride))
                        throw new InvalidDataException($"A Chozo statue strides {stride} pixels, which is not whole.");
                    return $"move:{(int)stride}:{motion.SamusOffset.X}:{motion.SamusOffset.Y}";
                }

                case ChozoStatueCue.Footstep footstep:
                    return $"footstep:{footstep.OffsetX}";
                case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.AwaitHand }:
                    return "await_hand";
                case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.StartLoweringAcid }:
                    return "lower_liquid";
                case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.UnlockSamus }:
                    return "unlock";
                case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.ReleaseSamus }:
                    return "release";
                case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.PlayGrabSound }:
                    return EngineNames.SoundCue(EnemySoundEffectDefinitions.ChozoGrabsSamusSound);
                case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.PlayFootstepSound }:
                    return EngineNames.SoundCue(EnemySoundEffectDefinitions.ChozoFootstepsSound);
                // The room's liquid is put where it lies lowered at once, however far it has yet to fall.
                case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.SetLoweredAcidPosition }:
                    return "settle_liquid";
                default:
                    throw new InvalidDataException($"A Chozo statue's animation raises {cue}, which this does not know.");
            }
        }

        // One whose work was done before sits as it does idle, and no longer waits for anything.
        static string? Done(ChozoStatueCue cue) => cue is ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.AwaitHand }
            ? null
            : throw new InvalidDataException($"A Chozo statue's idle animation raises {cue}, which this does not know.");

        foreach ((ChozoStatueSite site, ChozoStatueSiteDefinition definition) in ChozoStatueSiteDefinitions.Sites)
        {
            // Where the old game stands the statue, and the rooms it stands in.
            var stands = EngineLayouts.States()
                .SelectMany(placed => placed.State.EnemyPopulation.Placements.ToArray()
                    .Where(placement => placement.DefinitionId == EnemyDefinitionId.Chozo && placement.SpawnSettings is ChozoStatueSpawnSettings settings && settings.Site == site)
                    .Select(placement => (placed.Room, Placing: placement.Placing)))
                .ToList();
            Position16<RoomPixels> at = stands.Select(stood => stood.Placing is EnemyPlacedAt placedAt
                    ? placedAt.Position
                    : throw new InvalidDataException($"The Chozo statue of {site} is placed by its setup, not at a place."))
                .Distinct().Single();
            var rooms = stands.Select(stood => stood.Room).Distinct().ToList();

            // Its hand: the block Samus comes down onto. The Lower Norfair statue's is the block she must first break
            // open; the block the old game calls its hand's is the plug it pulls.
            Position16<RoomBlocks> handBlock = site == ChozoStatueSite.LowerNorfair ? ChozoStatueRoomDefinitions.LowerNorfairTriggerBlock : definition.HandBlock;
            Position16<RoomPixels> hand = handBlock.ToRoomPixels();

            string file = $"enemies/chozo_statue_{EngineSheets.FileName(site.ToString())}.json";
            // Its hurtbox is there for Samus's shots to be spent on, as the cartridge's are on its size: its reactions
            // harm it by none.
            EngineEnemy statue = Enemy(file, "ChozoStatue", EnemyDefinitionId.Chozo).BodyColliders("Body", "Solid", "Hurtbox")
                .Collider("Hand", hand.X - at.X, hand.Y - at.Y, EngineRoomObjects.Block, EngineRoomObjects.Block)
                .Value("footstep_depth", ChozoStatueRoomDefinitions.FootstepDepth)
                .Value("lowering_delay", ChozoStatueFxData.LoweringDelay)
                // It shows its own colours, which no other enemy does.
                .PictureSets("_own")
                .ShadedSet("_own", definition.Shade.Key)
                .Timeline("idle", ChozoStatueAnimations.Segments, ChozoStatueAnimations.StartingWith(definition.Idle), Cue)
                .Timeline("activated", ChozoStatueAnimations.Segments, ChozoStatueAnimations.StartingWith(definition.Activated), Cue)
                .Timeline("lowered", ChozoStatueAnimations.Segments, ChozoStatueAnimations.StartingWith(definition.Idle), Done);

            // A plug's breaking up, which the old game draws over its block from the update the block is made empty:
            // the shot block's own pictures, each for as long as the old frame stood. Its last frame, the block gone,
            // shows nothing, as the block itself now does.
            TerrainChange crumble = ChozoStatueObjectDefinitions.CrumblePlug;
            if (crumble.FirstFrameDelay != 1)
                throw new InvalidDataException($"A Chozo statue's plug first breaks up {crumble.FirstFrameDelay} updates after it is made, where its effect shows from the update it is left.");
            var crumbling = new JsonArray();
            for (int index = 0; index < crumble.Frames.Count; index++)
            {
                TerrainChangeFrame frame = crumble.Frames[index];
                if (frame.SoundBefore is not null)
                    throw new InvalidDataException("A Chozo statue's plug breaks up with a sound, which its effect does not ask for.");
                if (Layouts.BreakupPicture(frame.Draw) is { } picture)
                    crumbling.Add(new JsonObject { ["picture"] = picture, ["updates"] = frame.Updates });
                else if (index != crumble.Frames.Count - 1)
                    throw new InvalidDataException("A Chozo statue's plug is gone before its last frame of breaking up.");
            }

            string crumbleFile = $"{statue.Folder}/plug_crumble.json";
            _tree.WriteData(crumbleFile, new JsonObject { ["steps"] = crumbling });
            statue.Value("plug_crumble", crumbleFile);

            IEnumerable<(RoomDefinition Room, RoomStateSettings State)> everyState = EngineLayouts.States().Where(placed => rooms.Contains(placed.Room));
            switch (site)
            {
                case ChozoStatueSite.LowerNorfair:
                {
                    foreach ((RoomDefinition room, RoomStateSettings state) in everyState)
                        Layouts.Patch(room, state, ChozoPlugGonePatch, definition.HandBlock, Layouts.PatchCells(directory, Drawn(ChozoStatueDraw.LowerNorfairClearedHand)));
                    // The plug its hand pulls, which breaks up where it was.
                    Position16<RoomPixels> plug = definition.HandBlock.ToRoomPixels();
                    statue.Value("hand_patch", ChozoPlugGonePatch)
                        .Group("hand_plug", new JsonObject { ["x"] = plug.X, ["y"] = plug.Y });
                    break;
                }

                case ChozoStatueSite.WreckedShip:
                {
                    // Its way: the old game clears a run of blocks and then makes two blocks the slope down, writing
                    // each its slope; shutting it, it fills the run in and makes the two spike blocks with no
                    // behaviour of their own, which a spike block reads as a heavy spike. Both keep each block's picture.
                    Position16<RoomBlocks> origin = ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock;
                    Position16<RoomBlocks> first = ChozoStatueRoomDefinitions.FirstSlopeBlock, second = first + Offset16<RoomBlocks>.OneRight;
                    foreach ((RoomDefinition room, RoomStateSettings state) in everyState)
                    {
                        EngineLayouts.PatchCell Written(Position16<RoomBlocks> block, RoomCollisionType kind, RoomBlockBehavior does) =>
                            Layouts.Kept(state, origin, block.X - origin.X, block.Y - origin.Y, kind) with { Behavior = does };
                        List<EngineLayouts.PatchCell> open =
                        [
                            .. Layouts.PatchCells(directory, Drawn(ChozoStatueDraw.ClearSlopeAccess)),
                            Written(first, RoomCollisionType.Slope, ChozoStatueRoomDefinitions.FirstSlopeBehavior),
                            Written(second, RoomCollisionType.Slope, ChozoStatueRoomDefinitions.SecondSlopeBehavior),
                        ];
                        Layouts.Patch(room, state, ChozoWayOpenPatch, origin, open);
                        Layouts.Patch(room, state, ChozoWayShutPatch, origin,
                        [
                            .. Layouts.PatchCells(directory, Drawn(ChozoStatueDraw.BlockSlopeAccess)),
                            // Heavy spikes: they hurt once the ship has power, and are plain solid blocks before.
                            Written(first, RoomCollisionType.SpikeBlock, RoomBlockBehavior.None),
                            Written(second, RoomCollisionType.SpikeBlock, RoomBlockBehavior.None),
                        ]);

                        // What its feet break through: the block a foot comes down in, wherever the block below it
                        // is a spike while the way is open. It is left empty and shows nothing.
                        var opened = open.Where(cell => cell.Collision != RoomCollisionType.SpikeBlock)
                            .Select(cell => new Position16<RoomBlocks>(origin.X + cell.Across, origin.Y + cell.Down)).ToHashSet();
                        var spikes = Layouts.Blocks(state).Where(block => block.Terrain.Collision == RoomCollisionType.SpikeBlock && !opened.Contains(block.At))
                            .Select(block => block.At).ToHashSet();
                        foreach (Position16<RoomBlocks> spike in spikes)
                        {
                            if (spike.Y == 0)
                                continue;
                            Position16<RoomBlocks> plug = spike + Offset16<RoomBlocks>.OneUp;
                            Layouts.Patch(room, state, $"{ChozoPlugPatches}_{plug.X}_{plug.Y}", plug,
                                [new EngineLayouts.PatchCell(0, 0, RoomCollisionType.Air, null, BlockMirroring.None)]);
                        }
                    }

                    // The dust of a footstep: the old game throws it a chance distance as it appears, which the Godot
                    // statue does as it leaves it.
                    (List<IAnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>> dust, List<IAnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>>? dustCycle) =
                        EngineTimelines.Played(ChozoTourianDustAnimations.Segments, ChozoTourianDustAnimations.StartingAt[ChozoTourianDustSegment.Footsteps]);
                    EnemyProjectileScatter thrown = dust.OfType<AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event>()
                        .Select(raised => raised.Raised).OfType<EnemyProjectileCommonCue.Scatter>().Single().Spread;
                    string dustFile = $"{statue.Folder}/footstep.json";
                    _tree.WriteData(dustFile, SharedTimeline(dustFile, dust, dustCycle, EnemySheetDefinitions.ProjectileFrameName,
                        static (EnemyProjectileCommonCue cue) => cue is EnemyProjectileCommonCue.Scatter or EnemyProjectileCommonCue.Signal { Kind: EnemyProjectileCommonCueKind.Delete }
                            ? null
                            : throw new InvalidDataException($"The dust of a Chozo statue's footstep raises {cue}, which this does not know.")));
                    statue.Value("way_patch", ChozoWayOpenPatch)
                        .Value("shut_patch", ChozoWayShutPatch)
                        .Value("plug_patches", ChozoPlugPatches)
                        .Value("footstep_effect", dustFile)
                        .Group("footstep_scatter", new JsonObject
                        {
                            ["width"] = thrown.Span.X,
                            ["height"] = thrown.Span.Y,
                            ["middle_x"] = thrown.Center.X,
                            ["middle_y"] = thrown.Center.Y,
                        });
                    break;
                }

                default:
                    throw new InvalidDataException($"There is a Chozo statue of {site}, which this does not know.");
            }

            statue.Write();
        }
    }
}
