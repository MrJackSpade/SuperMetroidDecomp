using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What stands in Mother Brain's room besides her: the turrets in its ceiling and the glass of her tank.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The name the installation keeps the pieces of her tank's glass under.</summary>
    private const string TankGlassSpawner = "MotherBrainTankGlass";

    /// <summary>
    /// Starts the data file of something the old game drew as a thing one of its enemies launched and the Godot game
    /// keeps as an enemy of its own. It has no pictures of its own: what it shows is cut from the sheets of what the
    /// old enemy spawns.
    /// </summary>
    private EngineEnemy PartOf(string file, string type, EnemyDefinitionId owner) =>
        new(_tree, _sheets, file, type, owner, new EngineSheetPictures(0, 0, 1, 1, new Dictionary<string, string>(StringComparer.Ordinal)), EngineEnemy.NoPlaces);

    /// <summary>The name of the patch that is one of the pictures her tank's glass draws into the room.</summary>
    private static string GlassPatch(MotherBrainGlassDraw draw) => "glass_" + EngineSheets.FileName(draw.ToString());

    /// <summary>Writes the turrets of Mother Brain's room.</summary>
    private void WriteMotherBrainTurret()
    {
        EngineEnemy turret = PartOf("bosses/mother_brain_turret.json", "MotherBrainTurret", EnemyDefinitionId.MotherBrainBody);
        // The ways it points, in the order it turns through them, which is the order the Godot turret numbers them in.
        IReadOnlyList<MotherBrainTurretDirection> ways = MotherBrainTurretDefinitions.Ring.Members;
        static string? Cue(MotherBrainTurretCue cue) => cue switch
        {
            // A turret and a flying bullet hold their one picture, and raise neither.
            MotherBrainTurretCue.StopMoving or MotherBrainTurretCue.Delete => null,
            _ => throw new InvalidDataException($"A turret of Mother Brain's raises {cue}, which this does not know."),
        };
        // A bullet that is shot, or that touches Samus, becomes its smoke as itself: it holds still, still harms Samus,
        // and takes itself away as the smoke ends.
        static string? SmokeCue(MotherBrainTurretCue cue) => cue switch
        {
            MotherBrainTurretCue.StopMoving => null,
            MotherBrainTurretCue.Delete => "end",
            _ => throw new InvalidDataException($"A turret's bullet that is shot raises {cue}, which this does not know."),
        };
        turret.ProjectileData("bosses/mother_brain_turret_bullet.json", "MotherBrainTurretBullet",
            ("smoke_timeline", turret.SpawnedTimeline("bullet_smoke", MotherBrainTurretAnimations.Segments,
                MotherBrainTurretAnimations.StartingAt[MotherBrainTurretPose.BulletImpact], SmokeCue)));
        var muzzles = new JsonArray();
        for (int way = 0; way < ways.Count; way++)
        {
            MotherBrainTurretDirection direction = ways[way];
            MotherBrainTurretDirectionDefinition launch = MotherBrainTurrets.ForDirection(direction);
            turret.SpawnedLook($"pointing_{way}", MotherBrainTurretAnimations.Segments,
                MotherBrainTurretAnimations.StartingAt[MotherBrainTurretPose.Turret(direction)], Cue);
            muzzles.Add(new JsonObject { ["x"] = launch.BulletOffset.X, ["y"] = launch.BulletOffset.Y });
            string name = $"bullet_{way}";
            turret.Shot(name, RoomEnemyProjectileKind.MotherBrainRoomTurretBullet, (owner, shot, projectile) =>
            {
                // The Godot turret gives a bullet this speed along each of the ways across and down it goes.
                decimal across = Math.Abs(launch.BulletVelocity.X.Pixels), down = Math.Abs(launch.BulletVelocity.Y.Pixels);
                if (across != 0 && down != 0 && across != down)
                    throw new InvalidDataException($"A turret's bullet flying {direction} goes at one speed across and another down.");
                shot["speed"] = JsonValue.Create(Math.Max(across, down));
                // It moves itself and is gone silently where its middle meets a block; it stops shots every other update,
                // and shot, or touching Samus, it becomes its smoke. It lives on after touching her, as the old one does.
                shot["scene"] = "res://game/bosses/mother_brain/mother_brain_turret_bullet.tscn";
                projectile["timeline"] = owner.SpawnedTimeline(name, MotherBrainTurretAnimations.Segments,
                    MotherBrainTurretAnimations.StartingAt[MotherBrainTurretPose.Bullet(direction)], Cue);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
                projectile["met_by_shot"] = "destroyed";
            });
        }

        turret.Value("least_turn_wait", MotherBrainTurretDefinitions.MinimumRotationDelay)
            .Value("least_fire_wait", MotherBrainTurretDefinitions.MinimumFiringCooldown)
            .Value("smoke_effect", DustFile(DustExplosionAnimation.Smoke))
            .Value("view_across", MotherBrainTurretDefinitions.ViewOffset.X)
            .Value("view_down", MotherBrainTurretDefinitions.ViewOffset.Y)
            .Value("view_width", MotherBrainTurretDefinitions.ViewWidth)
            .Value("view_height", EnemyView.Extent)
            .Group("muzzles", muzzles)
            .Write();
    }

    /// <summary>The block the glass of her tank is placed at in one state of her room.</summary>
    /// <exception cref="InvalidDataException">The state does not place it just once.</exception>
    private static Position16<RoomBlocks> MotherBrainGlassBlock(RoomStateSettings state)
    {
        RoomObjectPlacement[] placed = [.. state.ObjectPopulation.Placements.ToArray().Where(placement => placement.Setup is MotherBrainGlassSetup)];
        return placed.Length == 1
            ? placed[0].Block
            : throw new InvalidDataException($"The state {state.Name} of Mother Brain's room places the glass of her tank {placed.Length} times.");
    }

    /// <summary>
    /// Writes the glass of Mother Brain's tank, and gives the layouts of her room a patch for each picture the glass
    /// draws into it as it breaks.
    /// </summary>
    private void WriteMotherBrainGlass(IReadOnlyList<(RoomDefinition Room, RoomStateSettings State)> states)
    {
        EngineEnemy glass = PartOf("bosses/mother_brain_glass.json", "MotherBrainGlass", EnemyDefinitionId.MotherBrainBody);

        // What it draws into the room. The block it stands at is the glass itself, which the Godot glass is while
        // it lasts, so the room has nothing there; a block that carries on from the one beside or above it is as
        // solid as the glass it carries on from.
        const string directory = GameInstallationLayout.RoomObjectMotherBrainGlassVisualDirectoryName;
        foreach ((RoomDefinition room, RoomStateSettings state) in states)
        {
            Position16<RoomBlocks> block = MotherBrainGlassBlock(state);
            foreach (MotherBrainGlassDraw draw in Enum.GetValues<MotherBrainGlassDraw>())
            {
                IEnumerable<EngineLayouts.PatchCell> cells = Layouts
                    .PatchCells(directory, (MotherBrainGlassDrawKeys.Installed.Of(draw), MotherBrainGlassObjectDrawDefinitions.Shapes[draw]))
                    .Select(cell => cell with
                    {
                        Collision = cell.Collision switch
                        {
                            RoomCollisionType.ShootableBlock when cell is { Across: 0, Down: 0 } => RoomCollisionType.Air,
                            RoomCollisionType.VerticalExtension or RoomCollisionType.HorizontalExtension => RoomCollisionType.SolidBlock,
                            RoomCollisionType.SolidBlock or RoomCollisionType.Air => cell.Collision,
                            var other => throw new InvalidDataException($"The glass of Mother Brain's tank draws a block that is {other}, which this does not know."),
                        },
                    });
                Layouts.Patch(room, state, GlassPatch(draw), block, cells);
            }
        }

        // Its stages, each with the count of hits it gives way at and the shards it throws as it does.
        var stages = new JsonArray();
        for (MotherBrainGlassStage? stage = MotherBrainGlassStageDefinitions.First; stage is not null;)
        {
            var listed = new JsonObject { ["patch"] = GlassPatch(stage.Image), ["hits"] = stage.BreaksAtHits };
            if (stage is MotherBrainGlassStage.Bursts bursts)
            {
                static JsonArray Places(IReadOnlyList<MotherBrainGlassShard> shards) =>
                    EngineNames.List(shards.Select(shard => MotherBrainGlassShardDefinitions.EmitterXs[shard]));
                listed["first"] = Places(bursts.Burst.First);
                listed["second"] = Places(bursts.Burst.Second);
            }

            stages.Add(listed);
            stage = stage switch
            {
                MotherBrainGlassStage.Cracks cracks => cracks.Next,
                MotherBrainGlassStage.Bursts.Into into => into.Next,
                MotherBrainGlassStage.Bursts.Last => null,
                _ => throw new InvalidDataException("The glass of Mother Brain's tank has a stage this does not know."),
            };
        }

        // Its shards: one shot for each sixteenth of the turn a shard may be thrown into, shown by the spin the old
        // game gives a shard thrown that way.
        static string? ShardCue(MotherBrainGlassCue cue) =>
            throw new InvalidDataException($"A shard of Mother Brain's glass raises {cue}, which this does not know.");
        var spins = new Dictionary<MotherBrainGlassSegment, string>();
        const int arcs = 16;
        for (int arc = 0; arc < arcs; arc++)
        {
            MotherBrainGlassSegment spin = MotherBrainGlassAnimations.ShardAnimations.At(new TrigonometryAngle(arc * (256 / arcs)));
            if (!spins.TryGetValue(spin, out string? timeline))
            {
                spins[spin] = timeline = glass.SpawnedTimelineFrom(TankGlassSpawner, EngineSheets.FileName(spin.ToString()),
                    MotherBrainGlassAnimations.Segments, MotherBrainGlassAnimations.StartingAt[spin], ShardCue);
            }

            glass.Shot($"shard_{arc}", RoomEnemyProjectileKind.MotherBrainGlassShard, (_, shot, projectile) =>
            {
                // It moves and falls itself, is gone once it has fallen out of the room's top screen, wherever the view
                // is, and sheds sparkles as it goes.
                shot["scene"] = "res://game/bosses/mother_brain/mother_brain_glass_shard.tscn";
                projectile["timeline"] = timeline;
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
                projectile["view_margin"] = -1;
            });
        }

        // The sparkles a shard sheds, which play out where they appear.
        static string? SparkleCue(MotherBrainGlassCue cue) => cue == MotherBrainGlassCue.Delete
            ? null
            : throw new InvalidDataException($"A sparkle of Mother Brain's glass raises {cue}, which this does not know.");
        string sparkle = glass.SpawnedTimelineFrom(TankGlassSpawner, "sparkle", MotherBrainGlassAnimations.Segments,
            MotherBrainGlassAnimations.StartingAt[MotherBrainGlassSegment.Sparkle], SparkleCue);
        if (MotherBrainGlassShardDefinitions.SparkleOffset.Shift != 0)
            throw new InvalidDataException("A sparkle's place is read from digits of the number that do not start at its lowest, which its data cannot say.");
        glass.ProjectileData("bosses/mother_brain_glass_shard.json", "MotherBrainGlassShard",
            ("gravity", EngineNames.Number(MotherBrainGlassShardDefinitions.ShardGravity)),
            ("bottom", MotherBrainGlassShardDefinitions.TopScreenBottomY),
            ("sparkle_effect", sparkle),
            ("sparkle_spread", MotherBrainGlassShardDefinitions.SparkleOffset.Outcomes),
            ("sparkle_centre", MotherBrainGlassShardDefinitions.SparkleOffsetCentre));

        glass.Collider("Hurtbox", 0, 0, RoomGridConversions.BlockPixels, RoomGridConversions.BlockPixels)
            .Collider("Solid", 0, 0, RoomGridConversions.BlockPixels, RoomGridConversions.BlockPixels)
            .Value("shard_drop", MotherBrainGlassShardDefinitions.EmitterY)
            .Value("shatter_sound", EngineNames.Sound(RoomSound.MotherBrainGlassShatter, MotherBrainGlassObjects.ShatterSoundPriority))
            .Value("burst_updates", MotherBrainGlassStageDefinitions.BurstFrameDuration)
            .Value("shatter_updates", MotherBrainGlassStageDefinitions.FinalShatterDuration)
            .Value("shattered_patch", GlassPatch(MotherBrainGlassStageDefinitions.ShatteredImage))
            .Value("shard_arcs", arcs)
            .Group("stages", stages)
            .Write();
    }
}
