using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Enemies.Sparks;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>More enemies whose animations tell them what to do next.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>
    /// The name of the timeline of a Ki-Hunter's acid splashing on the floor, within the Ki-Hunter's own folder: the
    /// acid is of its Ki-Hunter's colours, so each has a splash of its own beside its acid's flight.
    /// </summary>
    private const string AcidSplashName = "acid_splash";

    /// <summary>What an event of a Ki-Hunter's acid becomes: it is launched, it stops, it is gone.</summary>
    private static string? AcidCue(KiHunterAcidSpitCue cue) => cue switch
    {
        KiHunterAcidSpitCue.Launch => "launch",
        KiHunterAcidSpitCue.Signal { Kind: KiHunterAcidSpitCueKind.StopMoving } => "stop",
        KiHunterAcidSpitCue.Signal { Kind: KiHunterAcidSpitCueKind.Delete } => "end",
        _ => throw new InvalidDataException($"A Ki-Hunter's acid raises {cue}, which this does not know."),
    };

    /// <summary>What a box of the Oum does with a shot, as the Godot Oum names it: its shell or its soft part.</summary>
    private static string OumShotBox(EnemyReaction reaction) =>
        reaction == EnemyReaction.Ignore ? "shell"
        : reaction == OumReactions.Shot ? "soft"
        : throw new InvalidDataException($"A box of the Oum meets a shot with {reaction}, which this does not know.");

    /// <summary>What a box of the Oum does to Samus's touch, as the Godot Oum names it: hurts her, or only shoves her.</summary>
    private static string OumTouchBox(EnemyReaction reaction) =>
        reaction == OumReactions.DamagingTouch ? "hurts"
        : reaction == OumReactions.NonDamagingTouch ? "shoves"
        : throw new InvalidDataException($"A box of the Oum meets Samus's touch with {reaction}, which this does not know.");

    /// <summary>Writes the Ki-Hunters, the Rio, the Squeept, the Oum, the turtles and the Spark.</summary>
    private void WriteMoreCuedEnemies()
    {
        // The Ki-Hunters: three colours, each a body and its wings, which are sheets of their own.
        foreach (string colour in new[] { "Green", "Red", "Yellow" })
        {
            EnemyDefinitionId id = Enum.Parse<EnemyDefinitionId>("Kihunter" + colour);
            // Its loose wings meet the ground by the size the old game gives the wings.
            RoomEnemyDefinition wings = RoomEnemyDefinitionCatalog.ById[Enum.Parse<EnemyDefinitionId>($"Kihunter{colour}Wings")];
            Enemy($"enemies/ki_hunter_{colour.ToLowerInvariant()}.json", "KiHunter", id, $"Kihunter{colour}", $"Kihunter{colour}Wings")
                .BodyColliders(Ordinary)
                .Value("swoop_distance", KiHunterMotionDefinitions.SwoopTriggerDistance)
                .Value("swoop_least_depth", KiHunterMotionDefinitions.SwoopLeastDepth)
                .Value("patrol_speed", KiHunterMotionDefinitions.PatrolSpeed)
                .Value("patrol_rise", KiHunterMotionDefinitions.PatrolRise)
                .Value("patrol_depth", KiHunterMotionDefinitions.PatrolDepth)
                .Value("swoop_quickening", KiHunterMotionDefinitions.SwoopAcceleration.FractionsOfStep)
                .Value("swoop_fastest_turn", KiHunterMotionDefinitions.SwoopMaximumAngularVelocity.WholeSteps)
                .Value("strike_after_steps",
                    KiHunterMotionDefinitions.RightSwoopAnimationChangeAngle.TurnFrom(KiHunterMotionDefinitions.RightSwoopStartAngle) / RotationAngle.FractionsPerStep)
                .Value("gravity", EngineNames.Number(KiHunterMotionDefinitions.Gravity))
                .Value("hop_across_speed", KiHunterMotionDefinitions.GroundJumpHorizontalSpeed)
                .Value("hop_up_speed", KiHunterMotionDefinitions.GroundJumpUpwardSpeed)
                .Value("landing_wait", KiHunterMotionDefinitions.LandingWait)
                .Value("spit_wait", KiHunterMotionDefinitions.SpitWait)
                .Value("spit_range", KiHunterMotionDefinitions.SpitRange)
                .Value("wing_arc_radius", KiHunterMotionDefinitions.DetachedWingRadius)
                .Value("wing_arc_climb", KiHunterMotionDefinitions.DetachedWingClimb.Height.WholePixels)
                .Value("wing_arc_step", KiHunterMotionDefinitions.DetachedWingClimb.StepPerPass.InTwoHundredFiftySixthsOfMark)
                .Value("wing_half_width", wings.Radius.X)
                .Value("wing_half_height", wings.Radius.Y)
                .Value("spit_reach", KiHunterMotionDefinitions.AcidSpitReach)
                .Value("spit_height", KiHunterMotionDefinitions.AcidSpitHeight)
                // Its acid gathers at its mouth, shoots forward and falls; where it meets the floor it splashes.
                .Shot("acid", RoomEnemyProjectileKind.KiHunterAcidSpit, static (hunter, shot, projectile) =>
                {
                    shot["speed"] = EngineNames.Number(KiHunterMotionDefinitions.AcidHorizontalSpeed);
                    shot["sound"] = EngineNames.Sound(EnemySound.AcidSpit);
                    shot["scene"] = "res://game/enemies/ki_hunter/ki_hunter_acid.tscn";
                    hunter.SpawnedTimeline("acid_left", KiHunterAcidSpitAnimations.Segments, KiHunterAcidSpitAnimations.StartingAt[KiHunterAcidSpitSegment.Left], AcidCue);
                    hunter.SpawnedTimeline("acid_right", KiHunterAcidSpitAnimations.Segments, KiHunterAcidSpitAnimations.StartingAt[KiHunterAcidSpitSegment.Right], AcidCue);
                    // Every Ki-Hunter's acid splashes alike, in its own colours; the one data file of the acid names
                    // the splash's file, which lies beside the acid's flight.
                    (List<SuperMetroid.Core.Animation.IAnimationBeat<EnemyProjectileFrameId, KiHunterAcidSpitCue>> splash, _) =
                        EngineTimelines.Played(KiHunterAcidSpitAnimations.Segments, KiHunterAcidSpitAnimations.StartingAt[KiHunterAcidSpitSegment.HitFloor]);
                    hunter.SpawnedTimeline(AcidSplashName, splash, null, AcidCue);
                    projectile["timeline"] = $"{hunter.Folder}/acid_{{side}}.json";
                    projectile["picture_offset"] = EngineEnemy.NoOffset();
                    projectile["meeting_terrain"] = "passes_through";
                })
                .ProjectileData("enemies/ki_hunter_acid.json", "KiHunterAcid",
                    ("launch_shift", KiHunterMotionDefinitions.AcidLaunchShift),
                    ("gravity", EngineNames.Number(KiHunterMotionDefinitions.AcidGravity)),
                    ("fastest_fall", EngineNames.Number(KiHunterMotionDefinitions.AcidFastestFall)),
                    ("splash", AcidSplashName + ".json"))
                .Looks(KiHunterAnimations.Segments, KiHunterAnimations.StartingWith,
                    static (KiHunterCue cue) => cue switch
                    {
                        KiHunterCue.Spit => "spit",
                        KiHunterCue.Signal { Kind: KiHunterCueKind.ReturnToFlying } => "return_to_flying",
                        KiHunterCue.Signal { Kind: KiHunterCueKind.StartHop } => "hop",
                        KiHunterCue.Signal { Kind: KiHunterCueKind.StartWinglessThinking } => "think",
                        _ => throw new InvalidDataException($"A Ki-Hunter's animation raises {cue}, which this does not know."),
                    },
                    ("flying_left", KiHunterSegment.FlyingLeft), ("flying_right", KiHunterSegment.FlyingRight),
                    ("swoop_left", KiHunterSegment.SwoopLeft), ("swoop_right", KiHunterSegment.SwoopRight),
                    ("jump_left", KiHunterSegment.JumpLeft), ("jump_right", KiHunterSegment.JumpRight),
                    ("land_left", KiHunterSegment.LandLeft), ("land_right", KiHunterSegment.LandRight),
                    ("spit_left", KiHunterSegment.SpitLeft), ("spit_right", KiHunterSegment.SpitRight),
                    ("wings_left", KiHunterSegment.WingsLeft), ("wings_right", KiHunterSegment.WingsRight),
                    ("detached_wings", KiHunterSegment.DetachedWings))
                .Write();
        }

        // The Rio: its swoop begins and ends with animations that say when they are done.
        Enemy("enemies/rio.json", "Rio", EnemyDefinitionId.Rio).BodyColliders(Ordinary)
            .Value("reach", RioLaunchDefinitions.RioTriggerDistance)
            .Value("across_speed", EngineNames.Number(RioLaunchDefinitions.RioXVelocity))
            .Value("dive_speed", EngineNames.Number(RioLaunchDefinitions.RioYVelocity))
            .Value("lift", EngineNames.Number(RioLaunchDefinitions.RioGravity))
            .Value("return_speed", EngineNames.Number(-RioLaunchDefinitions.ReturnYVelocity))
            .Value("dive_sound", EngineNames.Sound(EnemySoundEffectDefinitions.RioDiveSound))
            .Looks(RioAnimations.Segments, RioAnimations.StartingWith, static (RioCue _) => "finished",
                ("idle", RioSegment.TuckedWingbeat), ("swoop_start", RioSegment.SwoopOpening), ("swoop", RioSegment.SwoopLoop),
                ("land", RioSegment.SwoopCooldown))
            .Timeline("hover", RioAnimations.Segments, RioAnimations.PostSwoopHover, static (RioCue _) => "finished")
            .Write();

        // The Squeept: its flame is the old game's second enemy, which only follows the first.
        Enemy("enemies/squeept.json", "Squeept", EnemyDefinitionId.Squeept).BodyColliders("Hurtbox")
            .Value("gravity", EngineNames.Number(NorfairLavaJumpDefinitions.Gravity))
            .Value("flip_speed", EngineNames.Number(-NorfairLavaJumpDefinitions.AnimationSwitchVelocity))
            // It is back in the lava once it is in the lowest row of blocks of any screen of the room.
            .Value("screen_height", RoomGridConversions.BlocksPerScreen * RoomGridConversions.BlockPixels)
            .Value("block_height", RoomGridConversions.BlockPixels)
            // The four upward speeds a leap may start with, picked by a number drawn.
            .Group("launch_speeds", EngineNames.List(EngineNames.Each(NorfairLavaJumpDefinitions.Launch).Select(speed => EngineNames.Number(-speed))))
            .Value("jump_sound", EngineNames.Sound(EnemySoundEffectDefinitions.SqueeptJumpSound))
            .Looks(NorfairLavaJumperAnimations.Segments, NorfairLavaJumperAnimations.StartingWith, static (NorfairLavaJumperCue _) => "flipped",
                ("hidden", NorfairLavaJumperSegment.Hidden), ("jump", NorfairLavaJumperSegment.Jump), ("flame", NorfairLavaJumperSegment.Follower))
            .Write();

        Enemy("enemies/oum.json", "Oum", EnemyDefinitionId.Oum).BodyColliders("Body", "Solid")
            .Value("pushing_reach_across", OumMotionDefinitions.PushingReachX)
            .Value("pushing_reach_down", OumMotionDefinitions.PushingReachY)
            .Value("attack_reach", OumMotionDefinitions.AttackReach)
            .Value("attack_cooldown", OumMotionDefinitions.InitialAttackCooldown)
            .Value("settling_bounces", OumMotionDefinitions.SettlingBounces)
            .Value("bounce_step", OumMotionDefinitions.BounceAcceleration.InTwoHundredFiftySixthsOfMark)
            .Value("fastest_fall_mark", EngineNames.Marks(OumMotionDefinitions.FastestFallPlace))
            .Value("impact_loss_marks", EngineNames.Marks(OumMotionDefinitions.ImpactCurveLoss))
            .Value("roll_speed", OumMotionDefinitions.RollSpeed.WholePixels)
            .Value("push_reach", OumMotionDefinitions.PushReach.WholePixels)
            .Value("shove_distance", OumMotionDefinitions.ShoveDistance)
            .Looks(OumAnimations.Segments, OumAnimations.StartingWith,
                static (OumCue cue) => cue.Kind switch
                {
                    OumCueKind.AllowAttackRotation => "can_attack",
                    OumCueKind.DisallowAttackRotation => "cannot_attack",
                    OumCueKind.FinishAttack => "attack_done",
                    OumCueKind.PlaySplash => EngineNames.SoundCue(EnemySoundEffectDefinitions.OumSplashSound),
                    _ => throw new InvalidDataException($"An Oum's animation raises {cue.Kind}, which this does not know."),
                },
                ("idle_left", OumSegment.FacingLeftIdle), ("idle_right", OumSegment.FacingRightIdle),
                ("attack_left", OumSegment.FacingLeftAttacking), ("attack_right", OumSegment.FacingRightAttacking),
                ("roll_forward_left", OumSegment.FacingLeftRollingForwards), ("roll_forward_right", OumSegment.FacingRightRollingForwards),
                ("roll_backward_left", OumSegment.FacingLeftRollingBackwards), ("roll_backward_right", OumSegment.FacingRightRollingBackwards))
            // Shots and Samus meet the boxes of the picture it shows: its shell swallows a shot, its soft part takes one,
            // and only the boxes of its jet hurt her.
            .PosedByFrame(OumCollisionDefinitions.Frames, OumShotBox, OumTouchBox)
            .Write();

        Enemy("enemies/mama_turtle.json", "MamaTurtle", EnemyDefinitionId.MamaTurtle).BodyColliders("Solid", "Hitbox", "Hurtbox")
            // How high her shell stands at each pixel out from her middle, to the left and to the right.
            .Group("shell_left", EngineNames.List(MamaTurtleShellContourDefinitions.LeftOfOrigin))
            .Group("shell_right", EngineNames.List(MamaTurtleShellContourDefinitions.RightOfOrigin))
            .Value("wall_sound", EngineNames.Sound(EnemySoundEffectDefinitions.MamaTurtleWallSound))
            .Value("awake_half_height", MamaTurtleMotionDefinitions.AwakeHalfHeight)
            .Value("rise_updates", MamaTurtleMotionDefinitions.RiseToHoverDuration)
            .Value("hover_quickening", EngineNames.Number(MamaTurtleMotionDefinitions.HoverAcceleration))
            .Value("fastest_hover", MamaTurtleMotionDefinitions.FastestHover.WholePixels)
            .Value("ridden_peak", MamaTurtleMotionDefinitions.RiddenAscentPeak)
            .Value("ridden_climb_speed", MamaTurtleMotionDefinitions.RiddenAscentSpeed)
            .Value("peak_pause", MamaTurtleMotionDefinitions.PeakPause)
            .Value("fall_quickening", EngineNames.Number(MamaTurtleMotionDefinitions.FallAcceleration))
            .Value("fastest_fall", MamaTurtleMotionDefinitions.FastestFall.WholePixels)
            .Value("wall_shake_strength", (int)MamaTurtleMotionDefinitions.WallQuake.Strength - (int)SuperMetroid.Core.Earthquakes.EarthquakeStrength.Light + 1)
            .Value("wall_shake_updates", MamaTurtleMotionDefinitions.WallQuakeDuration)
            .Looks(MamaTurtleAnimations.Segments, MamaTurtleAnimations.StartingWith,
                static (MamaTurtleCue cue) => cue switch
                {
                    MamaTurtleCue.RiseToHover rise => "rise:" + EngineNames.Side(rise.Direction),
                    MamaTurtleCue.Signal { Kind: MamaTurtleCueKind.EnterShell } => "enter_shell",
                    MamaTurtleCue.Signal { Kind: MamaTurtleCueKind.PlaySpinningSound } => EngineNames.SoundCue(EnemySoundEffectDefinitions.MamaTurtleSpinSound),
                    _ => throw new InvalidDataException($"The Mama Turtle's animation raises {cue}, which this does not know."),
                },
                ("asleep", MamaTurtleSegment.Asleep), ("spinning", MamaTurtleSegment.Spinning),
                ("leave_shell_left", MamaTurtleSegment.LeaveShellLeft), ("leave_shell_right", MamaTurtleSegment.LeaveShellRight),
                ("enter_shell_left", MamaTurtleSegment.EnterShellLeft), ("enter_shell_right", MamaTurtleSegment.EnterShellRight))
            .Write();

        Enemy("enemies/baby_turtle.json", "BabyTurtle", EnemyDefinitionId.BabyTurtle).BodyColliders("Body", "Solid", "Hurtbox")
            .Value("crawl_speed", MamaTurtleMotionDefinitions.BabyCrawlSpeed)
            .Value("travel_distance", MamaTurtleMotionDefinitions.BabyTravelDistance)
            .Value("rider_left_delay", MamaTurtleMotionDefinitions.BabyRiderLeftDelay)
            .Value("hide_updates", MamaTurtleMotionDefinitions.BabyHideDuration)
            .Value("spin_speed", (int)MamaTurtleMotionDefinitions.BabySpinVelocity.X)
            .Value("spin_fall_speed", (int)MamaTurtleMotionDefinitions.BabySpinVelocity.Y)
            .Looks(BabyTurtleAnimations.Segments, BabyTurtleAnimations.StartingWith,
                static (BabyTurtleCue cue) => cue.Kind switch
                {
                    BabyTurtleCueKind.Crawl => "crawl",
                    BabyTurtleCueKind.LoopOrTurnAround => "loop",
                    BabyTurtleCueKind.LeaveShell => "come_out",
                    BabyTurtleCueKind.LeftShell => "out",
                    BabyTurtleCueKind.SetSpinningStoppable => "stoppable",
                    BabyTurtleCueKind.PlaySpinningSound => EngineNames.SoundCue(EnemySoundEffectDefinitions.MamaTurtleSpinSound),
                    _ => throw new InvalidDataException($"A Baby Turtle's animation raises {cue.Kind}, which this does not know."),
                },
                ("crawling_left", BabyTurtleSegment.CrawlingLeft), ("crawling_right", BabyTurtleSegment.CrawlingRight),
                ("hiding_left", BabyTurtleSegment.HidingLeft), ("hiding_right", BabyTurtleSegment.HidingRight),
                ("leave_shell", BabyTurtleSegment.LeaveShell), ("spinning", BabyTurtleSegment.Spinning))
            .Write();

        // The Rinka: it forms unseen, and then comes out to take aim.
        Enemy("enemies/rinka.json", "Rinka", EnemyDefinitionId.Rinka).BodyColliders(Ordinary)
            .Value("puff_effect", DustFile(DustExplosionAnimation.SmallExplosion))
            .Value("aim_updates", RinkaLifeDefinitions.AimDelayUpdates)
            .Value("speed", EngineNames.Number(RinkaLifeDefinitions.Speed))
            .Value("view_margin", RinkaLifeDefinitions.FlightMargin)
            .Looks(RinkaAnimations.Segments, RinkaAnimations.StartingWith,
                static (RinkaCue cue) => cue.Kind switch
                {
                    RinkaCueKind.Fire => "fire",
                    RinkaCueKind.Hide => "hide",
                    RinkaCueKind.HideAndProcessOffScreen => $"hide{EngineTimelines.And}{EngineNames.AlwaysActive}",
                    _ => throw new InvalidDataException($"A Rinka's animation raises {cue.Kind}, which this does not know."),
                },
                ("life", RinkaSegment.Ordinary))
            .Write();

        // The Sbug: a look for each of the eight ways it can point.
        Enemy("enemies/sbug.json", "Sbug", EnemyDefinitionId.Sbug).BodyColliders("Hurtbox")
            .Value("dash_updates", SbugMovementDefinitions.LegDuration)
            .Value("zigzag_leg_updates", SbugDefinitions.ZigZagSecondLeg.CountsEachWay)
            .Looks(SbugAnimations.Segments, SbugAnimations.StartingWith,
                ("facing_right", CompassDirection.Right), ("facing_up_right", CompassDirection.UpRight), ("facing_up", CompassDirection.Up),
                ("facing_up_left", CompassDirection.UpLeft), ("facing_left", CompassDirection.Left),
                ("facing_down_left", CompassDirection.DownLeft), ("facing_down", CompassDirection.Down),
                ("facing_down_right", CompassDirection.DownRight))
            .Write();

        // The Tripper, and the suspensor platform, which is the same thing with one look whether it sinks or not.
        Enemy("enemies/tripper.json", "Tripper", EnemyDefinitionId.Tripper).BodyColliders("Body", "Solid", "Hurtbox")
            .Value("resting_depth", PlatformMotionDefinitions.RestingDepthBelowPlacement)
            .Looks(PlatformAnimations.Segments, PlatformAnimations.StartingWith, static (PlatformCue _) => null,
                ("moving_left", PlatformSegment.TripperMovingLeft), ("moving_right", PlatformSegment.TripperMovingRight),
                ("still_left", PlatformSegment.TripperStillMovingLeft), ("still_right", PlatformSegment.TripperStillMovingRight))
            .Write();
        Enemy("enemies/suspensor_platform.json", "Tripper", EnemyDefinitionId.SuspensorPlatform).BodyColliders("Body", "Solid", "Hurtbox")
            .Value("resting_depth", PlatformMotionDefinitions.RestingDepthBelowPlacement)
            .Looks(PlatformAnimations.Segments, PlatformAnimations.StartingWith, static (PlatformCue _) => null,
                ("moving_left", PlatformSegment.SuspensorPlatformHeadingLeft), ("moving_right", PlatformSegment.SuspensorPlatformHeadingRight),
                ("still_left", PlatformSegment.SuspensorPlatformHeadingLeft), ("still_right", PlatformSegment.SuspensorPlatformHeadingRight))
            .Write();

        SuperMetroid.Core.Compatibility.FallingSparkUnauthoredLaunch pastTheList = _installation.LoadEnemyOverreads().FallingSparkUnauthoredLaunch;
        Enemy("enemies/spark.json", "Spark", EnemyDefinitionId.Spark).BodyColliders(Ordinary)
            .Value("drip_below", FallingSparkMotionDefinitions.SpawnBelowSource)
            .Value("extra_off_updates", SparkTimingDefinitions.ExtraTimeOut)
            // The eight speeds across a falling spark may drip with; the old game lists seven and reads the eighth from past its list.
            .Group("spark_drifts", EngineNames.List(EngineNames.Each(FallingSparkLaunchDefinitions.Launch).Select(launch => EngineNames.Number(launch.DriftWith(() => pastTheList)))))
            .Shot("falling_spark", RoomEnemyProjectileKind.FallingSpark, static (spark, shot, projectile) =>
            {
                shot["scene"] = "res://game/enemies/spark/falling_spark.tscn";
                projectile["timeline"] = spark.SpawnedTimeline("falling_spark", FallingSparkAnimations.Segments,
                    FallingSparkAnimations.StartingAt[FallingSparkSegment.Falling], EngineEnemy.EndsOnly);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
                spark.ProjectileData("enemies/falling_spark.json", "FallingSpark",
                    ("gravity", EngineNames.Number(FallingSparkMotionDefinitions.Gravity)),
                    ("fall_speed_limit", FallingSparkMotionDefinitions.TerminalWholeSpeed),
                    ("skitter_times", FallingSparkMotionDefinitions.ImpactDriftScale),
                    ("skip_speed", EngineNames.Number(-FallingSparkMotionDefinitions.ImpactBounce)),
                    ("skip_lift", FallingSparkMotionDefinitions.ImpactLift),
                    // The Godot spark leaves its trail on the first update of each period.
                    ("trail_every", FallingSparkCadenceDefinitions.SparkTrail.Phase == 0
                        ? FallingSparkCadenceDefinitions.SparkTrail.Period
                        : throw new InvalidDataException("A falling spark's trail falls on a place of its period other than the first.")),
                    ("skitter_timeline", spark.SpawnedTimeline("falling_spark_skitter", FallingSparkAnimations.Segments,
                        FallingSparkAnimations.StartingAt[FallingSparkSegment.HitFloor], EngineEnemy.EndsOnly)),
                    ("trail_effect", spark.SpawnedEffect("falling_spark_trail", SuperMetroid.Core.Enemies.EffectActors.RoomEffectActorKind.FallingSparkTrail)));
            })
            .Looks(SparkAnimations.Segments, SparkAnimations.StartingWith,
                static (SparkCue cue) => cue.Kind == SparkCueKind.BecomeTangible ? "tangible" : "intangible",
                ("active", SparkSegment.Active), ("flicker_on", SparkSegment.FlickerOn), ("flicker_out", SparkSegment.FlickerOut),
                ("emitter", SparkSegment.Emitter))
            .Write();
    }
}
