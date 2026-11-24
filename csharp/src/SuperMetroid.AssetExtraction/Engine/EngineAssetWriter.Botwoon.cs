using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Botwoon: its head and the segments of its body, its holes and paths, its spit, and the wall that falls with it.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The name of the set of Botwoon's pictures that shows it as hurt as one of its bands of health is.</summary>
    private static string BotwoonHealthSet(BotwoonHealthBand band) => "_" + EngineSheets.FileName(band.Key);

    /// <summary>Writes Botwoon and the patches of its wall.</summary>
    private void WriteBotwoon()
    {
        EngineEnemy botwoon = Enemy("bosses/botwoon.json", "Botwoon", EnemyDefinitionId.Botwoon);
        // The eight ways it points, clockwise from up, as the Godot Botwoon names them.
        CompassDirection[] ways =
        [
            CompassDirection.Up, CompassDirection.UpRight, CompassDirection.Right, CompassDirection.DownRight,
            CompassDirection.Down, CompassDirection.DownLeft, CompassDirection.Left, CompassDirection.UpLeft,
        ];
        static string Way(CompassDirection way) => EngineSheets.FileName(way.ToString());

        // Its wall: it crumbles a row at a time once its hold is over, and a room where it has been beaten begins
        // with it gone.
        var states = StatesHolding(EnemyDefinitionId.Botwoon);
        static (string Directory, string Frame, RoomBlockShape Shape) WallDrawn(RoomObjectDraw draw) => draw switch
        {
            RoomObjectFrameDraw<ShotBlockObjectDraws.BreakupDraw> breaking => (
                GameInstallationLayout.RoomObjectShotBlockVisualDirectoryName,
                $"{BlockArrangementKeys.Installed.Of(breaking.Frame.Layout)}-{BlockBreakupStageKeys.Installed.Of(breaking.Frame.Stage)}",
                ShotBlockObjectDraws.BreakupFamily.Shape(breaking.Frame)),
            RoomObjectFrameDraw<BotwoonWallDraw> wall => (
                GameInstallationLayout.RoomObjectBotwoonWallVisualDirectoryName, RoomObjectBotwoonWallVisualFiles.VisualId(draw),
                BotwoonWallObjectDraws.Family.Shape(wall.Frame)),
            _ => throw new InvalidDataException("Botwoon's wall draws something that is not one of its own frames or a block breaking up."),
        };
        JsonObject wallCrumble = TerrainChangeAt(states, "wall_crumbles", BotwoonWallMutationDefinitions.Block, BotwoonWallMutationDefinitions.Crumble, WallDrawn);
        TerrainChangeAt(states, "wall_down", BotwoonWallMutationDefinitions.Block, BotwoonWallMutationDefinitions.Clear, WallDrawn, leaves: "wall_down");

        // Its head: a cue sizes it for the way it points, and its spitting gives the cue that lets the volley go.
        static string? HeadCue(BotwoonHeadCue cue) => cue switch
        {
            BotwoonHeadCue.ApplyFacingRadius radius => $"radius:{BotwoonAnimations.HeadRadius(radius.Facing).X}:{BotwoonAnimations.HeadRadius(radius.Facing).Y}",
            BotwoonHeadCue.Signal { Kind: BotwoonHeadCueKind.QueueSpitSound } => EngineNames.SoundCue(BotwoonSoundDefinitions.Spit),
            BotwoonHeadCue.Signal { Kind: BotwoonHeadCueKind.ReachSpitFrame } => "spit_frame",
            _ => throw new InvalidDataException($"Botwoon's head raises {cue}, which this does not know."),
        };
        string HeadPicture(BotwoonHeadFrame frame) => botwoon.Picture(BotwoonVisualDefinitions.HeadFrames.Of(frame));
        JsonObject Head(BotwoonHeadAnimationSegment segment) =>
            EngineTimelines.Of(BotwoonAnimations.HeadSegments, BotwoonAnimations.HeadStartingWith(segment), HeadPicture, HeadCue);
        botwoon.Timeline("hidden", Head(BotwoonHeadAnimationSegment.Hidden));
        foreach (CompassDirection way in ways)
        {
            botwoon.Timeline($"moving_{Way(way)}", Head(BotwoonHeadAnimationSegment.Moving(way)))
                .Timeline($"spitting_{Way(way)}", Head(BotwoonHeadAnimationSegment.Spitting(way)));
        }

        // The segments of its body and its tail, which the old game drew as things it launched, in each of its colours.
        static string? NoCue(NoAnimationCue cue) => null;
        var bodyLooks = new List<(string Name, IReadOnlyList<EnemyProjectileFrameId> Frames)>();
        foreach (CompassDirection way in ways)
        {
            foreach ((string part, BotwoonBodyAnimationSegment segment) in new[]
                     {
                         ("body", BotwoonBodyAnimationSegment.Link(way)), ("tail", BotwoonBodyAnimationSegment.TailCap(way)),
                     })
            {
                string name = $"{part}_{Way(way)}";
                Animation<BotwoonBodyAnimationSegment> look = BotwoonAnimations.BodyStartingWith(segment);
                botwoon.SpawnedLook(name, BotwoonAnimations.BodySegments, look, NoCue);
                (List<IAnimationBeat<EnemyProjectileFrameId, NoAnimationCue>> lead, List<IAnimationBeat<EnemyProjectileFrameId, NoAnimationCue>>? cycle) =
                    EngineTimelines.Played(BotwoonAnimations.BodySegments, look);
                bodyLooks.Add((name, [.. lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).OfType<EnemyProjectileFrameId>()]));
            }
        }

        foreach (BotwoonHealthBand band in BotwoonHealthBandDefinitions.InOrderShown)
        {
            string set = BotwoonHealthSet(band), shade = BotwoonShades.Health(band).Key;
            botwoon.ShadedSet(set, shade);
            foreach ((string name, IReadOnlyList<EnemyProjectileFrameId> frames) in bodyLooks)
                botwoon.ShadedSpawnedSet(set, name, frames, shade);
        }

        // Its spit flies through everything until it is out of view.
        botwoon.Shot("spit", RoomEnemyProjectileKind.BotwoonSpit, static (boss, _, projectile) =>
        {
            projectile["timeline"] = boss.SpawnedTimeline("spit", BotwoonAnimations.SpitSegments, BotwoonAnimations.SpitFlight, NoCue);
            projectile["picture_offset"] = EngineEnemy.NoOffset();
            projectile["meeting_terrain"] = "passes_through";
            EngineEnemy.GoneOutOfView(projectile);
        });

        // A segment of its body is as big as the old thing it was.
        EnemyProjectileDefinition segmentOf = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.BotwoonBody];
        for (int segment = 1; segment <= BotwoonFightDefinitions.BodySegments; segment++)
            botwoon.Collider($"BodyHit{segment}", -segmentOf.Radius.X, -segmentOf.Radius.Y, segmentOf.Radius.X * 2, segmentOf.Radius.Y * 2);

        IReadOnlyList<BotwoonHoleDefinition> holes = BotwoonNavigationDefinitions.Holes;
        int HoleNumber(BotwoonHole hole) => holes.Select((definition, index) => (definition, index)).Single(listed => listed.definition.Hole == hole).index;
        botwoon.BodyColliders("Hurtbox")
            .Value("opening_delay", BotwoonFightDefinitions.OpeningDelay)
            .Value("floor_y", BotwoonFightDefinitions.FloorY)
            .Value("pre_fall_wait", BotwoonDeathDefinitions.PreFallWaitUpdates)
            .Value("wall_updates", BotwoonDeathDefinitions.WallExplosionDuration)
            .Value("wall_drops", BotwoonDeathDefinitions.WallDropCount)
            .Value("wall_x", BotwoonDeathDefinitions.LargeExplosionLeft)
            .Value("explosion_effect", DustFile(DustExplosionAnimation.BigExplosion))
            .Value("wall_explosion_effect", botwoon.SpawnedEffect("wall_explosion", RoomEffectActorKind.BigExplosion))
            .Value("smoke_effect", EffectFile(RoomEffectActorKind.SmokePuff))
            .Value("explosion_sound", EngineNames.Sound(BotwoonSoundDefinitions.Explosion))
            .Group("holes", EngineNames.List(holes.Select(hole => (JsonNode)new JsonObject { ["x"] = hole.Target.X, ["y"] = hole.Target.Y })))
            .Group("paths", EngineNames.List(BotwoonPathDefinitions.Paths.Select(path => (JsonNode)new JsonObject
            {
                ["from"] = HoleNumber(path.From),
                ["to"] = HoleNumber(path.To),
                ["inside_wall"] = path.Route == BotwoonPathRoute.InsideWall,
                // A step is the number of the way it goes, of the eight, clockwise from up.
                ["steps"] = string.Concat(path.Steps.Select(step => Array.IndexOf(ways, StepWay(step)))),
            })))
            .Group("health_shades", EngineNames.List(BotwoonHealthBandDefinitions.InOrderShown.Select(band => (JsonNode)new JsonObject
            {
                ["below"] = band.HealthBelow,
                ["shade"] = BotwoonHealthSet(band),
            })))
            .Group("wall_crumble", wallCrumble)
            .PictureSets("{shade}")
            .Write();
    }

    /// <summary>The way one step of a path of Botwoon's goes.</summary>
    private static CompassDirection StepWay(BotwoonPathStep step) => step switch
    {
        BotwoonPathStep.Up => CompassDirection.Up,
        BotwoonPathStep.UpRight => CompassDirection.UpRight,
        BotwoonPathStep.Right => CompassDirection.Right,
        BotwoonPathStep.DownRight => CompassDirection.DownRight,
        BotwoonPathStep.Down => CompassDirection.Down,
        BotwoonPathStep.DownLeft => CompassDirection.DownLeft,
        BotwoonPathStep.Left => CompassDirection.Left,
        BotwoonPathStep.UpLeft => CompassDirection.UpLeft,
        _ => throw new InvalidDataException($"A path of Botwoon's steps {step}, which this does not know."),
    };
}
