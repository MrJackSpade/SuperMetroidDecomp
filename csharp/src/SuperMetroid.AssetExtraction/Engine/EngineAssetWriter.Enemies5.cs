using System.Text.Json.Nodes;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The enemies that are more than one picture at once: a body and its flames, a body and what crackles round it.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes the Geruta, the Holtz and the Metroid.</summary>
    private void WritePartedEnemies()
    {
        // The Geruta: its animations say where its flames lie and which way they blow.
        Enemy("enemies/geruta.json", "Swooper", EnemyDefinitionId.Geruta).BodyColliders(Ordinary)
            .Value("reach", RioLaunchDefinitions.NorfairTriggerDistance)
            .Value("across_speed", EngineNames.Number(RioLaunchDefinitions.NorfairXVelocity))
            .Value("dive_speed", EngineNames.Number(RioLaunchDefinitions.NorfairFastYVelocity))
            .Value("slow_dive_speed", EngineNames.Number(RioLaunchDefinitions.NorfairSlowYVelocity))
            .Value("lift", EngineNames.Number(RioLaunchDefinitions.NorfairGravity))
            .Value("return_speed", EngineNames.Number(-RioLaunchDefinitions.ReturnYVelocity))
            .Value("dive_sound", EngineNames.Sound(EnemySoundEffectDefinitions.GerutaDiveSound))
            .Looks(GerutaAnimations.Segments, GerutaAnimations.StartingWith,
                static (GerutaCue cue) => cue switch
                {
                    GerutaCue.PlaceFlames { Flames: GerutaFlames.Descending } place => $"flames_down:{place.Below}",
                    GerutaCue.PlaceFlames { Flames: GerutaFlames.Ascending } place => $"flames_up:{place.Below}",
                    GerutaCue.SwoopStartFinished => "finished",
                    _ => throw new InvalidDataException($"A Geruta's animation raises {cue}, which this does not know."),
                },
                ("idle", GerutaSegment.Idle), ("takeoff", GerutaSegment.StartDescending), ("dive", GerutaSegment.Descending),
                ("climb_start", GerutaSegment.StartAscending), ("climb", GerutaSegment.Ascending),
                ("flames_down", GerutaSegment.FlamesDescending), ("flames_up", GerutaSegment.FlamesAscending))
            .Write();

        // The Holtz: it has one swoop, takes a moment to land, and its flames only show or hide, always as far below it.
        // Unlike the Geruta, it shows its idle on every update it stays perched.
        Enemy("enemies/holtz.json", "Swooper", EnemyDefinitionId.Holtz).BodyColliders(Ordinary)
            .Value("idles_whenever_perched", true)
            .Value("reach", RioLaunchDefinitions.LowerNorfairTriggerDistance)
            .Value("across_speed", EngineNames.Number(RioLaunchDefinitions.LowerNorfairXVelocity))
            .Value("dive_speed", EngineNames.Number(RioLaunchDefinitions.LowerNorfairYVelocity))
            .Value("slow_dive_speed", EngineNames.Number(RioLaunchDefinitions.LowerNorfairYVelocity))
            .Value("lift", EngineNames.Number(RioLaunchDefinitions.LowerNorfairGravity))
            .Value("return_speed", EngineNames.Number(-RioLaunchDefinitions.ReturnYVelocity))
            .Value("return_sound", EngineNames.Sound(EnemySoundEffectDefinitions.HoltzReturnSound))
            .Looks(HoltzAnimations.Segments, HoltzAnimations.StartingWith,
                static (HoltzCue cue) => cue.Kind switch
                {
                    HoltzCueKind.AnimationFinished => "finished",
                    HoltzCueKind.HideFlames => "flames_off",
                    HoltzCueKind.ShowFlames => $"flames_up:{RioLaunchDefinitions.LowerNorfairFlameDrop}",
                    _ => throw new InvalidDataException($"A Holtz's animation raises {cue.Kind}, which this does not know."),
                },
                ("idle", HoltzSegment.Idle), ("takeoff", HoltzSegment.PrepareToSwoop), ("dive", HoltzSegment.Descending),
                ("climb_start", HoltzSegment.AscentOpening), ("climb", HoltzSegment.Ascending), ("land", HoltzSegment.Cooldown),
                ("flames_up", HoltzSegment.Flames))
            .Write();

        // The Metroid: the lightning inside it and its shell are drawn over its body, from the sheets of what it spawns.
        Enemy("enemies/metroid.json", "Metroid", EnemyDefinitionId.Metroid).BodyColliders(Ordinary)
            .Value("aim_above", MetroidBehaviorDefinitions.AimAboveSamus)
            .Value("latch_distance", MetroidBehaviorDefinitions.LatchDistance)
            .Value("fastest_chase", MetroidBehaviorDefinitions.FastestChase)
            .Value("frozen_updates", MetroidBehaviorDefinitions.FrozenDuration)
            .Value("feeding_sound_least_energy", MetroidBehaviorDefinitions.DrainSoundLeastHealth)
            // What it drains from Samus in her first suit; her other suits lessen it.
            .Value("drain_hundredths", Hundredths(MetroidBehaviorDefinitions.PowerSuitDrainFraction))
            .Value("feeding_sound_every", MetroidCadenceDefinitions.DrainSound.Period)
            .Value("drop_count", MetroidBehaviorDefinitions.FrozenDeathDropCount)
            .Value("feeding_sound", EngineNames.Sound(RoomSound.IncrementalEnergy))
            .Value("recoil_sound", EngineNames.Sound(EnemySoundEffectDefinitions.MetroidRecoilSoundEffect))
            // The eight cries its cry is picked from by a number drawn.
            .Group("cries", EngineNames.List(EngineNames.Each(MetroidBehaviorDefinitions.Cry).Select(cry => (JsonNode)JsonValue.Create(
                EngineNames.Sound(cry) ?? throw new InvalidDataException("One of a Metroid's cries only stops a sound.")))))
            .Looks(MetroidAnimations.Segments, MetroidAnimations.StartingWith,
                static (MetroidCue cue) => cue.Kind switch
                {
                    MetroidCueKind.PlayRandomCry => "cry",
                    MetroidCueKind.PlayDrainingSound => EngineNames.SoundCue(EnemySoundEffectDefinitions.MetroidAnimationSoundEffect),
                    _ => throw new InvalidDataException($"A Metroid's animation raises {cue.Kind}, which this does not know."),
                },
                ("chasing", MetroidSegment.ChasingSamus), ("draining", MetroidSegment.DrainingSamus))
            .SpawnedLook("electricity", RoomEffectActorKind.MetroidElectricity)
            .SpawnedLook("shell", RoomEffectActorKind.MetroidShell)
            .Write();

        // The Yapping Maw: a look for each way it bites, and its root, its neck and its drawing back for each of the
        // two ways it can be set: striking upward or downward.
        static string Way(YappingMawAim aim) => EngineSheets.FileName(aim.ToString());
        static string? MawCue(YappingMawCue cue) => cue switch
        {
            YappingMawCue.HoldSamus hold => "hold:" + Way(hold.Aim),
            YappingMawCue.Signal { Kind: YappingMawCueKind.PlayBiteSound } => "bite",
            _ => throw new InvalidDataException($"A Yapping Maw's animation raises {cue}, which this does not know."),
        };
        var heldOffsets = new JsonObject();
        EngineEnemy maw = Enemy("enemies/yapping_maw.json", "YappingMaw", EnemyDefinitionId.YappingMaw).BodyColliders("Hurtbox")
            .Value("safety_distance", YappingMawAttackDefinitions.GrabSafetyDistance)
            .Value("grab_cooldown", YappingMawAttackDefinitions.GrabCooldown)
            .Value("retracted_hold", YappingMawAttackDefinitions.RetractedHold)
            .Value("root_distance", YappingMawAttackDefinitions.RootDistance)
            .Value("longest_reach", YappingMawAttackGeometry.MaximumTargetDistance)
            .Value("bite_sound", EngineNames.Sound(EnemySoundEffectDefinitions.YappingMawAttackSound))
            .Group("held_offsets", heldOffsets);
        foreach (YappingMawAim aim in Enum.GetValues<YappingMawAim>())
        {
            Offset16<RoomPixels> held = YappingMawHoldDefinitions.HeldSamusOffsets[aim];
            heldOffsets[Way(aim)] = new JsonObject { ["x"] = held.X, ["y"] = held.Y };
            maw.Timeline("attack_" + Way(aim), YappingMawAnimations.Segments, YappingMawAnimations.StartingWith(YappingMawAnimationDefinitions.Attacks[aim]), MawCue);
        }

        foreach ((string after, YappingMawReach reach) in new[] { ("", YappingMawReach.Up), ("_down", YappingMawReach.Down) })
        {
            maw.Timeline("cooldown_straight" + after, YappingMawAnimations.Segments, YappingMawAnimations.StartingWith(reach.StraightCooldown), MawCue)
                .Timeline("cooldown_right" + after, YappingMawAnimations.Segments, YappingMawAnimations.StartingWith(reach.RightCooldown), MawCue)
                .Timeline("cooldown_left" + after, YappingMawAnimations.Segments, YappingMawAnimations.StartingWith(reach.LeftCooldown), MawCue)
                .SpawnedLook("root" + after, reach.Root)
                .SpawnedLook("link" + after, YappingMawBodyProjectileAnimations.Segments, YappingMawBodyProjectileAnimations.StartingAt[reach.Body], EngineEnemy.EndsOnly);
        }

        maw.Write();

        // The Powamp: a body and the balloon over it. The balloon's timelines say how full it is at each step, which is
        // how high it sits.
        static string Pose(PowampBalloonPose pose) => EngineSheets.FileName(pose.ToString());
        var balloonHeights = new JsonObject();
        foreach (PowampBalloonPose pose in Enum.GetValues<PowampBalloonPose>())
            balloonHeights[Pose(pose)] = PowampMotionDefinitions.BalloonHeights[pose];
        EngineEnemy powamp = Enemy("enemies/powamp.json", "Powamp", EnemyDefinitionId.Powamp).BodyColliders("Body", "Hurtbox", "BalloonHurtbox")
            .Value("rise_distance", PowampMotionDefinitions.UngrappledRise)
            .Value("rest_updates", PowampMotionDefinitions.RestDuration)
            .Value("inflation_updates", PowampMotionDefinitions.InflationDuration)
            .Value("death_delay", PowampMotionDefinitions.DeathDelay)
            // Its delayed death, by a shot, a bomb or a power bomb, takes its explosion from the place its burst's last
            // spike took among the enemies' shots (LeftoverNumberDeathExplosion): eight spikes into places given from
            // the last down end at the eleventh or lower, which names the small explosion, as does a burst the places
            // ran out for. Only with eight to ten places already held does the last spike take the second or third
            // place and name the normal or the big one; that case is not written.
            .Group("death", Death(EnemyDeathExplosion.Small, raisedBySuperMissile: false))
            .Value("wiggle_step_updates", PowampMotionDefinitions.WiggleStepDuration)
            .Value("rise_speed", EngineNames.Number(-PowampMotionDefinitions.RiseVelocity))
            .Value("sink_speed", EngineNames.Number(PowampMotionDefinitions.SinkVelocity))
            .Value("spike_quickening", EngineNames.Number(PowampMotionDefinitions.SpikeAccelerations[CompassDirection.Right].X))
            .Group("balloon_heights", balloonHeights)
            .Group("wiggle", EngineNames.List(PowampMotionDefinitions.WiggleOffsets))
            .Timeline("body_slow", PowampAnimations.BodySegments, PowampAnimations.BodyStartingAt[PowampBodySegment.BodySlow])
            .Timeline("body_fast", PowampAnimations.BodySegments, PowampAnimations.BodyStartingAt[PowampBodySegment.BodyFast])
            // Its eight spikes gather speed the way each is thrown, and end on what they meet.
            .Shot("spike", RoomEnemyProjectileKind.PowampSpike, static (thrower, _, projectile) =>
            {
                projectile["timeline"] = thrower.SpawnedTimeline("spike", PowampSpikeAnimations.Segments, PowampSpikeAnimations.StartingAt[PowampSpikeSegment.Flying], EngineEnemy.EndsOnly);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["motion"] = "accelerating";
                projectile["acceleration"] = EngineNames.Number(PowampMotionDefinitions.SpikeAccelerations[CompassDirection.Right].X);
            });
        foreach ((string name, PowampBalloonSegment segment) in new[]
                 {
                     ("deflated", PowampBalloonSegment.Deflated), ("inflating", PowampBalloonSegment.InflatingFromDeflated),
                     ("inflating_from_halfway", PowampBalloonSegment.InflatingFromHalfway), ("sinking", PowampBalloonSegment.StartSinking),
                 })
        {
            (List<SuperMetroid.Core.Animation.IAnimationBeat<PowampBalloonPose, SuperMetroid.Core.Animation.NoAnimationCue>> lead,
                List<SuperMetroid.Core.Animation.IAnimationBeat<PowampBalloonPose, SuperMetroid.Core.Animation.NoAnimationCue>>? cycle) =
                EngineTimelines.Played(PowampAnimations.BalloonSegments, PowampAnimations.BalloonStartingAt[segment]);
            JsonObject timeline = EngineTimelines.Of(lead, cycle, pose => powamp.Picture(PowampAnimations.BalloonFrames[pose]),
                static (SuperMetroid.Core.Animation.NoAnimationCue _) => null);
            // Each step says which pose its picture is, for the balloon's height.
            foreach (JsonNode? step in timeline["steps"]!.AsArray())
            {
                string shown = step!["picture"]!.GetValue<string>();
                PowampBalloonPose pose = Enum.GetValues<PowampBalloonPose>().Single(posed => powamp.Picture(PowampAnimations.BalloonFrames[posed]) == shown);
                step["cues"] = new JsonArray("pose:" + Pose(pose));
            }

            powamp.Timeline(name, timeline);
        }

        powamp.Write();

        // The work robot: all it does is written in its animations, so each of its timelines is named for the old
        // segment it plays, and its cues name the timelines they lead to. The robot without power is the same thing
        // standing in one of three poses, whose pictures are on a sheet of their own.
        static string RobotLook(WorkRobotSegment segment) => segment switch
        {
            WorkRobotSegment.StandingFacingLeft => "standing_left",
            WorkRobotSegment.FacingLeftShotSamusAhead => "shot_left_ahead",
            WorkRobotSegment.FacingLeftShotSamusBehind => "shot_left_behind",
            WorkRobotSegment.FacingRightShotSamusAhead => "shot_right_ahead",
            WorkRobotSegment.FacingRightShotSamusBehind => "shot_right_behind",
            _ => EngineSheets.FileName(segment.ToString()),
        };
        static string? RobotCue(WorkRobotCue cue) => cue switch
        {
            WorkRobotCue.Step { Move: var move } => $"step:{move.HorizontalPixels}:{EngineNames.Side(move.Facing)}:{RobotLook(move.WallAnimation)}" +
                (move.Ledge is { } ledge ? $":{EngineNames.Side(ledge.ProbeSide)}:{RobotLook(ledge.FallAnimation)}:{EngineNames.Side(ledge.FallFacing)}" : ""),
            WorkRobotCue.TryLaser fire => "laser:" + fire.Laser switch
            {
                WorkRobotLaser.Upward => "up",
                WorkRobotLaser.Horizontal => "level",
                WorkRobotLaser.Downward => "down",
                _ => throw new InvalidDataException($"A work robot fires a laser {fire.Laser}, which this does not know."),
            } + ":" + RobotLook(fire.Firing),
            WorkRobotCue.Signal { Kind: WorkRobotCueKind.Footstep } => "footstep",
            WorkRobotCue.Signal { Kind: WorkRobotCueKind.CountDownLaserCooldown } => "cool",
            _ => throw new InvalidDataException($"A work robot's animation raises {cue}, which this does not know."),
        };
        static void RobotLaser(EngineEnemy robot, JsonObject shot, JsonObject projectile)
        {
            shot["speed"] = EngineNames.Number(WorkRobotLaserDefinitions.Speed);
            projectile["timeline"] = robot.SpawnedTimeline("laser", WorkRobotLaserAnimations.Segments,
                WorkRobotLaserAnimations.StartingAt[WorkRobotLaserSegment.Flare], EngineEnemy.EndsOnly);
            projectile["picture_offset"] = EngineEnemy.NoOffset();
        }

        EngineEnemy robot = Enemy("enemies/work_robot.json", "WorkRobot", EnemyDefinitionId.WorkRobot, "WorkRobot", "UnpoweredWorkRobot")
            .BodyColliders("Body", "Solid", "Hurtbox")
            .Value("gravity", EngineNames.Number(WorkRobotMotionDefinitions.Gravity))
            .Value("laser_cooldown", WorkRobotLaserDefinitions.CooldownBase)
            .Value("turn_delay", WorkRobotLaserDefinitions.TurnDelay)
            .Value("recoil_delay", WorkRobotLaserDefinitions.RecoilDelay)
            .Value("muzzle_ahead", WorkRobotLaserDefinitions.MuzzleAhead)
            .Value("muzzle_above", WorkRobotLaserDefinitions.MuzzleAbove)
            .Value("laser_slant_speed", EngineNames.Number(WorkRobotLaserDefinitions.DiagonalVerticalSpeed))
            .Value("footstep_sound", EngineNames.Sound(EnemySoundEffectDefinitions.WorkRobotFootstepSound))
            .Value("laser_sound", EngineNames.Sound(EnemySoundEffectDefinitions.WorkRobotLaserSound))
            .Shot("laser_level", RoomEnemyProjectileKind.WorkRobotLaserHorizontal, RobotLaser)
            .Shot("laser_diagonal", RoomEnemyProjectileKind.WorkRobotLaserDiagonal, RobotLaser);
        foreach ((WorkRobotSegment segment, SuperMetroid.Core.Animation.Animation<WorkRobotSegment> animation) in WorkRobotAnimations.StartingAt)
            robot.Timeline(RobotLook(segment), WorkRobotAnimations.Segments, animation, RobotCue);
        robot.Write();
    }

    /// <summary>An amount of energy as a whole number of hundredths of a point.</summary>
    /// <exception cref="InvalidDataException">The amount is no whole number of hundredths.</exception>
    private static int Hundredths(SuperMetroid.Core.Compatibility.EnergyAmount amount)
    {
        const int partsOfPoint = 1 << 16;
        int parts = amount.FractionParts * 100;
        return parts % partsOfPoint == 0
            ? amount.WholeEnergy * 100 + parts / partsOfPoint
            : throw new InvalidDataException("An amount of energy is no whole number of hundredths of a point.");
    }
}
