using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The Space Pirates: those that walk, those that climb walls and the ninjas, each in its colours.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes every Space Pirate.</summary>
    private void WritePirates()
    {
        foreach (string colour in new[] { "Gold", "Green", "Grey", "Magenta", "Red", "Silver" })
        {
            Enemy($"enemies/walking_pirate_{colour.ToLowerInvariant()}.json", "WalkingPirate", Enum.Parse<EnemyDefinitionId>($"Pirate{colour}Walking"))
                .BodyColliders("Body")
                .Value("walk_speed_left", EngineNames.Number(-SpacePirateMotionDefinitions.WalkingLeftStep))
                .Value("walk_speed_right", EngineNames.Number(SpacePirateMotionDefinitions.WalkingRightStep))
                .Value("firing_band", SpacePirateMotionDefinitions.WalkingFiringBand)
                .Value("flinch_reach", SpacePirateMotionDefinitions.WalkingFlinchReach)
                .Value("ledge_reach_left", SpacePirateMotionDefinitions.WalkingLeftLedgeProbe)
                .Value("ledge_reach_right", SpacePirateMotionDefinitions.WalkingRightLedgeProbe)
                .Value("muzzle_ahead", SpacePirateMotionDefinitions.WalkingLaserMuzzleAhead)
                .Looks(WalkingSpacePirateAnimations.Segments, WalkingSpacePirateAnimations.StartingWith,
                    static (WalkingSpacePirateCue cue) => cue switch
                    {
                        WalkingSpacePirateCue.Begin { Action: WalkingSpacePirateAction.StandingStill } => "still",
                        WalkingSpacePirateCue.BeginHeading { Action: WalkingSpacePirateAction.Walking } heading => "walk:" + EngineNames.Side(heading.Heading),
                        WalkingSpacePirateCue.FireLaser fire => $"fire:{EngineNames.Side(fire.Direction)}:{fire.LaserHeight}",
                        WalkingSpacePirateCue.Signal => "choose",
                        _ => throw new InvalidDataException($"A walking Space Pirate's animation raises {cue}, which this does not know."),
                    },
                    ("walking_left", WalkingSpacePirateSegment.WalkingLeft), ("walking_right", WalkingSpacePirateSegment.WalkingRight),
                    ("looking_left", WalkingSpacePirateSegment.LookingFacingLeft), ("looking_right", WalkingSpacePirateSegment.LookingFacingRight),
                    ("fire_lasers_left", WalkingSpacePirateSegment.FireLasersLeft), ("fire_lasers_right", WalkingSpacePirateSegment.FireLasersRight),
                    ("flinch_left", WalkingSpacePirateSegment.FlinchFacingLeft), ("flinch_right", WalkingSpacePirateSegment.FlinchFacingRight))
                .PosedByFrame(SpacePirateCollisionDefinitions.Frames)
                .Shot("laser", RoomEnemyProjectileKind.PirateMotherBrainLaser, PirateLaser)
                .Write();
        }

        foreach (string colour in new[] { "Gold", "Grey", "Magenta", "Silver" })
        {
            Enemy($"enemies/wall_pirate_{colour.ToLowerInvariant()}.json", "WallPirate", Enum.Parse<EnemyDefinitionId>($"Pirate{colour}Wall"))
                .BodyColliders("Body")
                .Value("detection_band", SpacePirateMotionDefinitions.WallDetectionBand)
                .Value("muzzle_above", SpacePirateMotionDefinitions.WallLaserMuzzleAbove)
                .Value("muzzle_ahead", SpacePirateMotionDefinitions.WalkingLaserMuzzleAhead)
                // Its two leaps: how fast each turns, and how far short of the far wall the slow one is aimed.
                .Value("fast_jump_turn", (int)SpacePirateMotionDefinitions.FastWallJump.TurnPerUpdate.ToTrigonometryAngle().RawValue)
                .Value("slow_jump_turn", (int)SpacePirateMotionDefinitions.SlowWallJump.TurnPerUpdate.ToTrigonometryAngle().RawValue)
                .Value("slow_jump_short_by", SpacePirateMotionDefinitions.FastWallJump.RightTarget.ToTrigonometryAngle()
                    .StepsFrom(SpacePirateMotionDefinitions.SlowWallJump.RightTarget.ToTrigonometryAngle()))
                .Value("block_size", SuperMetroid.Core.Spatial.RoomGridConversions.BlockPixels)
                .Looks(WallSpacePirateAnimations.Segments, WallSpacePirateAnimations.StartingWith,
                    static (WallSpacePirateCue cue) => cue switch
                    {
                        WallSpacePirateCue.BeginOnWall { Action: WallSpacePirateAction.Climbing } on => "on_wall:" + EngineNames.Side(on.Wall),
                        WallSpacePirateCue.BeginOnWall { Action: WallSpacePirateAction.WallJumping } to => "jump:" + EngineNames.Side(to.Wall),
                        WallSpacePirateCue.Begin { Action: WallSpacePirateAction.Attacking } => "attacking",
                        WallSpacePirateCue.Climb climb => $"climb:{climb.ClimbPixels}",
                        WallSpacePirateCue.ChooseClimb => "choose_climb",
                        WallSpacePirateCue.FireLaser fire => "fire:" + EngineNames.Side(fire.Direction),
                        WallSpacePirateCue.PrepareJump prepare => "prepare_jump:" + EngineNames.Side(prepare.Direction),
                        // Its one plain signal is the yell of its leap.
                        WallSpacePirateCue.Signal => EngineNames.SoundCue(EnemySoundEffectDefinitions.WallPirateJumpSound),
                        _ => throw new InvalidDataException($"A wall Space Pirate's animation raises {cue}, which this does not know."),
                    },
                    ("moving_up_left_wall", WallSpacePirateSegment.MovingUpLeftWall), ("moving_down_left_wall", WallSpacePirateSegment.MovingDownLeftWall),
                    ("moving_up_right_wall", WallSpacePirateSegment.MovingUpRightWall), ("moving_down_right_wall", WallSpacePirateSegment.MovingDownRightWall),
                    ("fire_and_jump_left", WallSpacePirateSegment.FireAndJumpLeft), ("fire_and_jump_right", WallSpacePirateSegment.FireAndJumpRight),
                    ("landed_on_left_wall", WallSpacePirateSegment.LandedOnLeftWall), ("landed_on_right_wall", WallSpacePirateSegment.LandedOnRightWall))
                .PosedByFrame(SpacePirateCollisionDefinitions.Frames)
                .Shot("laser", RoomEnemyProjectileKind.PirateMotherBrainLaser, PirateLaser)
                .Write();
        }

        foreach (string colour in new[] { "Gold", "Silver" })
        {
            Enemy($"enemies/ninja_pirate_{colour.ToLowerInvariant()}.json", "NinjaPirate", Enum.Parse<EnemyDefinitionId>($"Pirate{colour}Ninja"))
                .BodyColliders("Body")
                .Value("activation_distance", SpacePirateMotionDefinitions.NinjaActivationDistance)
                .Value("flinch_distance", SpacePirateMotionDefinitions.NinjaFlinchDistance)
                .Value("midpoint_distance", SpacePirateMotionDefinitions.NinjaMidpointTriggerDistance)
                .Value("kick_distance", SpacePirateMotionDefinitions.NinjaKickDistance)
                .Value("spin_speed_change", EngineNames.Number(SpacePirateMotionDefinitions.NinjaSpinSpeedChange))
                .Value("spin_climb_step", SpacePirateMotionDefinitions.NinjaSpinClimbStep)
                .Value("dive_start_speed", (int)SpacePirateMotionDefinitions.NinjaDiveStartSpeed.Pixels)
                .Value("dive_speed_change", EngineNames.Number(SpacePirateMotionDefinitions.NinjaDiveSpeedChange))
                .Value("dive_step", SpacePirateMotionDefinitions.NinjaDiveStep)
                .Value("return_step", SpacePirateMotionDefinitions.NinjaReturnStep)
                .Value("landing_dust_spread", SpacePirateMotionDefinitions.NinjaLandingDustSpread)
                .Value("landing_dust_drop", SpacePirateMotionDefinitions.NinjaLandingDustDrop)
                .Value("claw_attack_every", NinjaSpacePirateCadenceDefinitions.ClawAttack.Period)
                // The gold one scatters drops about itself as a shot destroys it.
                .Value("scattered_drops", colour == "Gold" ? GoldNinjaSpacePirateDefinitions.DropScatterCount : 0)
                .Looks(NinjaSpacePirateAnimations.Segments, NinjaSpacePirateAnimations.StartingWith,
                    static (NinjaSpacePirateCue cue) => cue switch
                    {
                        NinjaSpacePirateCue.Begin begin => "state:" + begin.Action switch
                        {
                            NinjaSpacePirateAction.StandingStill => "still",
                            NinjaSpacePirateAction.AwaitingSamus => "awaiting",
                            NinjaSpacePirateAction.Active => "active",
                            NinjaSpacePirateAction.ReadyToDivekick => "ready",
                            _ => throw new InvalidDataException($"A ninja Space Pirate's animation begins {begin.Action} with no heading, which this does not know."),
                        },
                        NinjaSpacePirateCue.BeginHeading heading => "state:" + heading.Action switch
                        {
                            NinjaSpacePirateAction.SpinJumpRising => "spin_rising",
                            NinjaSpacePirateAction.DivekickJump => "dive_jump",
                            NinjaSpacePirateAction.DivekickDive => "dive",
                            NinjaSpacePirateAction.DivekickWalkToPost => "walk_to_post",
                            _ => throw new InvalidDataException($"A ninja Space Pirate's animation begins {heading.Action} with a heading, which this does not know."),
                        } + ":" + EngineNames.Side(heading.Heading),
                        NinjaSpacePirateCue.Signal { Kind: NinjaSpacePirateCueKind.StartActive } => "start_active",
                        NinjaSpacePirateCue.Signal { Kind: NinjaSpacePirateCueKind.ResetSpinSpeed } => "reset_spin",
                        NinjaSpacePirateCue.Signal { Kind: NinjaSpacePirateCueKind.StartDivekick } => "start_dive",
                        NinjaSpacePirateCue.ThrowClaw thrown => $"claw:{EngineNames.Side(thrown.Throw)}:{thrown.Offset.X}:{thrown.Offset.Y}",
                        NinjaSpacePirateCue.PlaySound sound => EngineNames.SoundCue(sound.Sound),
                        // The flash of its colours as it dives is not something the Godot game's data says.
                        NinjaSpacePirateCue.SetShading => null,
                        _ => throw new InvalidDataException($"A ninja Space Pirate's animation raises {cue}, which this does not know."),
                    },
                    ("awaiting_left", NinjaSpacePirateSegment.AwaitingSamusFacingLeft), ("awaiting_right", NinjaSpacePirateSegment.AwaitingSamusFacingRight),
                    ("active_left", NinjaSpacePirateSegment.ActiveFacingLeft), ("active_right", NinjaSpacePirateSegment.ActiveFacingRight),
                    ("flinch_left", NinjaSpacePirateSegment.FlinchFacingLeft), ("flinch_right", NinjaSpacePirateSegment.FlinchFacingRight),
                    ("kick_left", NinjaSpacePirateSegment.KickFacingLeft), ("kick_right", NinjaSpacePirateSegment.KickFacingRight),
                    ("claw_attack_left", NinjaSpacePirateSegment.ClawAttackLeft), ("claw_attack_right", NinjaSpacePirateSegment.ClawAttackRight),
                    ("spin_jump_left", NinjaSpacePirateSegment.SpinJumpLeft), ("spin_jump_right", NinjaSpacePirateSegment.SpinJumpRight),
                    ("divekick_jump_left", NinjaSpacePirateSegment.DivekickLeftJump), ("divekick_jump_right", NinjaSpacePirateSegment.DivekickRightJump),
                    ("divekick_dive_left", NinjaSpacePirateSegment.DivekickLeftDive), ("divekick_dive_right", NinjaSpacePirateSegment.DivekickRightDive),
                    ("walk_to_post_left", NinjaSpacePirateSegment.WalkToLeftPost), ("walk_to_post_right", NinjaSpacePirateSegment.WalkToRightPost),
                    ("land_left", NinjaSpacePirateSegment.LandFacingLeft), ("land_right", NinjaSpacePirateSegment.LandFacingRight))
                // Its claw flies out, slows, and comes back through everything until it is out of view.
                .Shot("claw", RoomEnemyProjectileKind.PirateClaw, static (ninja, shot, projectile) =>
                {
                    shot["speed"] = SpacePirateMotionDefinitions.NinjaClawLaunchSpeed.WholePixels;
                    shot["scene"] = "res://game/enemies/space_pirate/pirate_claw.tscn";
                    ninja.SpawnedTimeline("claw_left", SpacePirateProjectileAnimations.Segments,
                        SpacePirateProjectileAnimations.StartingAt[SpacePirateProjectileSegment.ClawLeft], static (SpacePirateProjectileCue _) => null);
                    ninja.SpawnedTimeline("claw_right", SpacePirateProjectileAnimations.Segments,
                        SpacePirateProjectileAnimations.StartingAt[SpacePirateProjectileSegment.ClawRight], static (SpacePirateProjectileCue _) => null);
                    projectile["timeline"] = $"{ninja.Folder}/claw_{{side}}.json";
                    projectile["picture_offset"] = EngineEnemy.NoOffset();
                    projectile["meeting_terrain"] = "passes_through";
                    EngineEnemy.GoneOutOfView(projectile);
                    // The dust of its landing, as its rooms draw it.
                    ninja.Value("landing_dust_effect", ninja.SpawnedEffect("landing_dust", RoomEffectActorKind.RisingDustPuff));
                })
                .ProjectileData("enemies/pirate_claw.json", "PirateClaw")
                .PosedByFrame(SpacePirateCollisionDefinitions.Frames)
                .Write();
        }
    }

    /// <summary>
    /// What a Space Pirate's laser is: it gathers at the muzzle for as long as the old one's animation takes to fire
    /// it, then flies level through everything until it is out of view, and it hurts as much as the Pirate itself.
    /// </summary>
    private static void PirateLaser(EngineEnemy pirate, JsonObject shot, JsonObject projectile)
    {
        shot["damage"] = pirate.Definition.Damage;
        shot["sound"] = EngineNames.Sound(EnemySoundEffectDefinitions.PirateMotherBrainLaserSound);
        pirate.SpawnedTimeline("laser_left", SpacePirateProjectileAnimations.Segments,
            SpacePirateProjectileAnimations.StartingAt[SpacePirateProjectileSegment.LaserLeft], static (SpacePirateProjectileCue _) => null);
        pirate.SpawnedTimeline("laser_right", SpacePirateProjectileAnimations.Segments,
            SpacePirateProjectileAnimations.StartingAt[SpacePirateProjectileSegment.LaserRight], static (SpacePirateProjectileCue _) => null);
        projectile["timeline"] = $"{pirate.Folder}/laser_{{side}}.json";
        projectile["picture_offset"] = EngineEnemy.NoOffset();
        projectile["meeting_terrain"] = "passes_through";
        projectile["meeting_target"] = "ends";
        projectile["met_by_shot"] = "ignored";
        projectile["launch_delay_updates"] = UpdatesBefore(
            SpacePirateProjectileAnimations.Segments.Beats(SpacePirateProjectileSegment.LaserLeft),
            static (SpacePirateProjectileCue cue) => cue.Kind == SpacePirateProjectileCueKind.FireLaser);
        EngineEnemy.GoneOutOfView(projectile);
    }

    /// <summary>How many updates of frames an animation shows before it first raises an event that <paramref name="awaited"/> picks out.</summary>
    /// <exception cref="InvalidDataException">The animation never raises such an event.</exception>
    private static int UpdatesBefore<TFrame, TEvent>(IEnumerable<IAnimationBeat<TFrame, TEvent>> beats, Func<TEvent, bool> awaited)
        where TFrame : struct
        where TEvent : notnull
    {
        int updates = 0;
        foreach (IAnimationBeat<TFrame, TEvent> beat in beats)
        {
            switch (beat)
            {
                case AnimationBeat<TFrame, TEvent>.Frame frame:
                    updates += frame.Duration;
                    break;
                case AnimationBeat<TFrame, TEvent>.Event raised when awaited(raised.Raised):
                    return updates;
            }
        }

        throw new InvalidDataException("An animation never raises the event its launch waits for.");
    }
}
