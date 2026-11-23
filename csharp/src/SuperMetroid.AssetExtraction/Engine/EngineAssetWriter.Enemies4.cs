using SuperMetroid.Core.Enemies;
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
    /// <summary>Writes the Ki-Hunters, the Rio, the Squeept, the Oum, the turtles and the Spark.</summary>
    private void WriteMoreCuedEnemies()
    {
        // The Ki-Hunters: three colours, each a body and its wings, which are sheets of their own.
        foreach (string colour in new[] { "Green", "Red", "Yellow" })
        {
            EnemyDefinitionId id = Enum.Parse<EnemyDefinitionId>("Kihunter" + colour);
            Enemy($"enemies/ki_hunter_{colour.ToLowerInvariant()}.json", "KiHunter", id, $"Kihunter{colour}", $"Kihunter{colour}Wings")
                .BodyColliders(Ordinary)
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
            .Value("dive_sound", EngineNames.Sound(EnemySoundEffectDefinitions.RioDiveSound))
            .Looks(RioAnimations.Segments, RioAnimations.StartingWith, static (RioCue _) => "finished",
                ("idle", RioSegment.TuckedWingbeat), ("swoop_start", RioSegment.SwoopOpening), ("swoop", RioSegment.SwoopLoop),
                ("land", RioSegment.SwoopCooldown))
            .Timeline("hover", RioAnimations.Segments, RioAnimations.PostSwoopHover, static (RioCue _) => "finished")
            .Write();

        // The Squeept: its flame is the old game's second enemy, which only follows the first.
        Enemy("enemies/squeept.json", "Squeept", EnemyDefinitionId.Squeept).BodyColliders("Hurtbox")
            .Value("jump_sound", EngineNames.Sound(EnemySoundEffectDefinitions.SqueeptJumpSound))
            .Looks(NorfairLavaJumperAnimations.Segments, NorfairLavaJumperAnimations.StartingWith, static (NorfairLavaJumperCue _) => "flipped",
                ("hidden", NorfairLavaJumperSegment.Hidden), ("jump", NorfairLavaJumperSegment.Jump), ("flame", NorfairLavaJumperSegment.Follower))
            .Write();

        Enemy("enemies/oum.json", "Oum", EnemyDefinitionId.Oum).BodyColliders("Body", "Solid", "Hurtbox", "Hitbox")
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
            .Write();

        Enemy("enemies/mama_turtle.json", "MamaTurtle", EnemyDefinitionId.MamaTurtle).BodyColliders("Solid", "Hitbox")
            .Value("wall_sound", EngineNames.Sound(EnemySoundEffectDefinitions.MamaTurtleWallSound))
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
            .Looks(SbugAnimations.Segments, SbugAnimations.StartingWith,
                ("facing_right", CompassDirection.Right), ("facing_up_right", CompassDirection.UpRight), ("facing_up", CompassDirection.Up),
                ("facing_up_left", CompassDirection.UpLeft), ("facing_left", CompassDirection.Left),
                ("facing_down_left", CompassDirection.DownLeft), ("facing_down", CompassDirection.Down),
                ("facing_down_right", CompassDirection.DownRight))
            .Write();

        // The Tripper, and the suspensor platform, which is the same thing with one look whether it sinks or not.
        Enemy("enemies/tripper.json", "Tripper", EnemyDefinitionId.Tripper).BodyColliders("Body", "Solid", "Hurtbox")
            .Looks(PlatformAnimations.Segments, PlatformAnimations.StartingWith, static (PlatformCue _) => null,
                ("moving_left", PlatformSegment.TripperMovingLeft), ("moving_right", PlatformSegment.TripperMovingRight),
                ("still_left", PlatformSegment.TripperStillMovingLeft), ("still_right", PlatformSegment.TripperStillMovingRight))
            .Write();
        Enemy("enemies/suspensor_platform.json", "Tripper", EnemyDefinitionId.SuspensorPlatform).BodyColliders("Body", "Solid", "Hurtbox")
            .Looks(PlatformAnimations.Segments, PlatformAnimations.StartingWith, static (PlatformCue _) => null,
                ("moving_left", PlatformSegment.SuspensorPlatformHeadingLeft), ("moving_right", PlatformSegment.SuspensorPlatformHeadingRight),
                ("still_left", PlatformSegment.SuspensorPlatformHeadingLeft), ("still_right", PlatformSegment.SuspensorPlatformHeadingRight))
            .Write();

        Enemy("enemies/spark.json", "Spark", EnemyDefinitionId.Spark).BodyColliders(Ordinary)
            .Looks(SparkAnimations.Segments, SparkAnimations.StartingWith,
                static (SparkCue cue) => cue.Kind == SparkCueKind.BecomeTangible ? "tangible" : "intangible",
                ("active", SparkSegment.Active), ("flicker_on", SparkSegment.FlickerOn), ("flicker_out", SparkSegment.FlickerOut),
                ("emitter", SparkSegment.Emitter))
            .Write();
    }
}
