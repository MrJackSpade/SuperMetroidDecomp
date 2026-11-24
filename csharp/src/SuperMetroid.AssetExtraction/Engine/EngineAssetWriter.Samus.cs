using SuperMetroid.Core.Audio;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using M = SuperMetroid.Core.Samus.Movement.SamusMovementType;
using P = SuperMetroid.Core.Samus.Poses.SamusPoseId;
using Seg = SuperMetroid.Core.Samus.Poses.SamusAnimationSegmentName;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Samus's data file: her body's sizes, how she moves in air and in each liquid, her timings, her body's powers, where
/// her shots leave her, and everything it names.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The mediums the Godot Samus moves in, by name, with the old medium each is. A liquid's type gives one of the names.</summary>
    private static readonly (string Name, SamusLiquidMedium Medium)[] SamusMediums =
    [
        ("air", SamusLiquidMedium.Air), ("water", SamusLiquidMedium.Water), ("lava", SamusLiquidMedium.LavaOrAcid), ("acid", SamusLiquidMedium.LavaOrAcid),
    ];

    /// <summary>The move values the Godot game's states ask a medium for, by name, with the old movement each is.</summary>
    private static readonly (string State, M Movement)[] SamusMoves =
    [
        ("standing", M.Standing), ("running", M.Running), ("turning", M.TurningOnGround), ("crouching", M.Crouching), ("jumping", M.NormalJumping),
        ("falling", M.Falling), ("wall_jumping", M.WallJumping), ("crawling", M.MorphBallGround), ("crawling_in_air", M.MorphBallFalling),
        ("knocked_back", M.Knockback), ("spin_jumping", M.SpinJumping), ("moonwalking", M.Moonwalking), ("damage_boost", M.DamageBoost),
        ("crawling_jumping", M.SpringBallInAir),
    ];

    /// <summary>The ways she aims, as her data names them, with the old direction of each facing right and facing left.</summary>
    private static readonly (string Aim, SamusProjectileDirection Right, SamusProjectileDirection Left)[] SamusAims =
    [
        ("forward", SamusProjectileDirection.Right, SamusProjectileDirection.Left),
        ("diagonal_up", SamusProjectileDirection.UpRight, SamusProjectileDirection.UpLeft),
        ("up", SamusProjectileDirection.UpFacingRight, SamusProjectileDirection.UpFacingLeft),
        ("diagonal_down", SamusProjectileDirection.DownRight, SamusProjectileDirection.DownLeft),
        ("down", SamusProjectileDirection.DownFacingRight, SamusProjectileDirection.DownFacingLeft),
    ];

    /// <summary>How long one segment of a pose's animation lasts.</summary>
    private static int SamusSegmentUpdates(P pose, Seg segment) => SamusPoseTraits.Of(pose).Animation.Segment(segment).Delays.Sum();

    /// <summary>How long one segment of a pose's animation lasts where each of its pictures is held <paramref name="longer"/> updates more, as in a liquid.</summary>
    private static int SamusSegmentUpdates(P pose, Seg segment, int longer) =>
        SamusSegmentUpdates(pose, segment) + SamusPoseTraits.Of(pose).Animation.Segment(segment).Delays.Count * longer;

    /// <summary>The timed changes of hers that an animation times, by the name of the value that gives each one's length, with the old pose and segment.</summary>
    private static readonly (string Value, P Pose, Seg Segment)[] SamusTimedChanges =
    [
        ("turn_updates", P.TurningRightToLeftPose, Seg.Main), ("crouch_updates", P.CrouchingTransitionRightPose, Seg.Main),
        ("stand_updates", P.StandingTransitionRightPose, Seg.Main), ("crawl_enter_updates", P.MorphingTransitionRightPose, Seg.Main),
        ("crawl_leave_updates", P.UnmorphingTransitionRightPose, Seg.Main), ("jump_start_updates", P.NeutralJumpTransitionRightPose, Seg.Main),
        ("wall_jump_ready_updates", P.SpinJumpRightPose, Seg.WallJumpReady), ("landing_updates", P.NormalLandingRightPose, Seg.Main),
        ("spin_landing_updates", P.SpinLandingRightPose, Seg.Main),
    ];

    /// <summary>
    /// The most pixels the surface of any of the old game's sloped shapes (not its square ones, which are steps and
    /// walls) rises or falls between one pixel column and the next, within the block.
    /// </summary>
    private static int SteepestSlopeStep()
    {
        int steepest = 0;
        foreach (SlopeShape shape in SlopeShapeDefinitions.All.Where(shape => !shape.IsSquare))
        {
            for (int column = 1; column < SlopeShape.Columns; column++)
            {
                int here = Math.Min(shape.HeightAt(column), SlopeShape.Columns), before = Math.Min(shape.HeightAt(column - 1), SlopeShape.Columns);
                steepest = Math.Max(steepest, Math.Abs(here - before));
            }
        }

        return steepest;
    }

    /// <summary>A speed the old game keeps unsigned, as a plain number of pixels.</summary>
    private static double Number(UnsignedSixteenSixteen speed) => speed.RawValue / (double)UnsignedSixteenSixteen.FromWholePixels(1).RawValue;

    /// <summary>A turning speed as a plain number of steps of a turn.</summary>
    private static double Steps(AngularSpeed speed) => speed.FractionsOfStep / (double)AngularSpeed.FromSteps(1).FractionsOfStep;

    /// <summary>An angle as a plain number of steps of a turn.</summary>
    private static double Steps(RotationAngle angle) => angle.RawValue / (double)RotationAngle.FromSteps(1).RawValue;

    /// <summary>An angle that is a whole number of steps of a turn.</summary>
    /// <exception cref="InvalidDataException">The angle lies between two steps.</exception>
    private static int WholeSteps(RotationAngle angle) => angle == angle.WholeStep
        ? angle.RawValue / RotationAngle.FromSteps(1).RawValue
        : throw new InvalidDataException("An angle lies between two steps of a turn, where a whole step was wanted.");

    /// <summary>One set of move values.</summary>
    private static JsonObject Moves(SamusHorizontalSpeeds speeds) => new()
    {
        ["acceleration"] = Number(speeds.Acceleration),
        ["top_speed"] = Number(speeds.MaximumSpeed),
        ["deceleration"] = Number(speeds.Deceleration),
    };

    /// <summary>Where shots leave her in one stance, by the way she faces and her aim.</summary>
    private static JsonObject Origins(SamusProjectileDirectionMap<Offset16<RoomPixels>> places)
    {
        var right = new JsonObject();
        var left = new JsonObject();
        foreach ((string aim, SamusProjectileDirection toRight, SamusProjectileDirection toLeft) in SamusAims)
        {
            right[aim] = new JsonObject { ["x"] = places.For(toRight).X, ["y"] = places.For(toRight).Y };
            left[aim] = new JsonObject { ["x"] = places.For(toLeft).X, ["y"] = places.For(toLeft).Y };
        }

        return new JsonObject { ["right"] = right, ["left"] = left };
    }

    /// <summary>How she moves in each medium.</summary>
    private static JsonObject SamusMediumsWritten()
    {
        var mediums = new JsonObject();
        foreach ((string name, SamusLiquidMedium medium) in SamusMediums)
        {
            IReadOnlyDictionary<M, SamusHorizontalSpeeds> speeds = SamusHorizontalSpeedDefinitions.ByMedium[medium];
            var moves = new JsonObject();
            foreach ((string state, M movement) in SamusMoves)
                moves[state] = Moves(speeds[movement]);

            // A bomb jump thrown to one side goes by speeds of its own, the same in every medium.
            moves["bomb_jumping"] = Moves(SamusHorizontalSpeedDefinitions.DiagonalBombJump);
            bool liquid = medium != SamusLiquidMedium.Air;
            int share = liquid ? GrappleBeamDefinitions.Physics.SubmergedShare : 1;

            // In a liquid every picture of her animations is held longer, so the changes they time last longer.
            var updates = new JsonObject();
            if (liquid)
            {
                int longer = medium == SamusLiquidMedium.Water
                    ? SamusAnimationDelayDefinitions.WaterAnimationDelay
                    : SamusAnimationDelayDefinitions.LavaOrAcidAnimationDelay;
                foreach ((string value, P pose, Seg segment) in SamusTimedChanges)
                    updates[value] = SamusSegmentUpdates(pose, segment, longer);
            }
            mediums[name] = new JsonObject
            {
                ["gravity"] = Number(SamusVerticalMotionDefinitions.Gravities[medium]),
                ["fall_speed_limit"] = SamusVerticalMotionDefinitions.TerminalFallWholeSpeed,
                ["jump_speed"] = Number(SamusVerticalMotionDefinitions.UnassistedLaunchSpeeds[medium][SamusLaunchKind.Jump]),
                ["boosted_jump_speed"] = Number(SamusVerticalMotionDefinitions.HighJumpLaunchSpeeds[medium][SamusLaunchKind.Jump]),
                ["wall_jump_speed"] = Number(SamusVerticalMotionDefinitions.UnassistedLaunchSpeeds[medium][SamusLaunchKind.WallJump]),
                ["boosted_wall_jump_speed"] = Number(SamusVerticalMotionDefinitions.HighJumpLaunchSpeeds[medium][SamusLaunchKind.WallJump]),
                ["knockback_up_speed"] = Number(SamusVerticalMotionDefinitions.KnockbackSpeeds[medium]),
                ["moves"] = moves,
                ["updates"] = updates,
                ["speeds"] = new JsonObject
                {
                    ["bomb_jump"] = Number(SamusVerticalMotionDefinitions.BombJumpSpeeds[medium]),
                    ["space_jump_least_fall"] = Number(liquid
                        ? SamusVerticalMotionDefinitions.SpaceJumpLiquidMinimumFallingSpeed
                        : SamusVerticalMotionDefinitions.SpaceJumpAirMinimumFallingSpeed),
                    ["space_jump_most_fall"] = Number(SamusVerticalMotionDefinitions.SpaceJumpFallingSpeedLimit),

                    // The swing's pull and push are in steps of a turn an update; under a liquid she takes a share of each.
                    ["grapple_swing_pull"] = Steps(GrappleBeamDefinitions.Physics.GravityMagnitude) / share,
                    ["grapple_swing_push"] = Steps(GrappleBeamDefinitions.Physics.DirectionInputMagnitude) / share,
                    ["grapple_reel_speed"] = GrappleBeamDefinitions.Swing.RopeLengthStep,
                    ["grapple_shortest_line"] = GrappleBeamDefinitions.Swing.ShortestRope,
                    ["grapple_longest_line"] = GrappleBeamDefinitions.Swing.LongestLetOutRope,
                },
            };
        }

        return mediums;
    }

    /// <summary>Writes Samus and all that is hers.</summary>
    private void WriteSamus()
    {
        WriteSamusParticles();
        WriteSamusBeam();
        WriteSamusMissile(super: false);
        WriteSamusMissile(super: true);
        WriteSamusBombs();
        WriteSamusSpecialShots();

        var weapons = new JsonArray();
        foreach (string weapon in SamusWeaponFiles)
            weapons.Add(weapon);

        // The speed booster's boost is counted in strides of her run. Each stage has its own pace for the stride's
        // pictures and its own count of strides.
        var boostStrides = new List<int>();
        var boostCounts = new List<int>();
        for (int stage = 0; stage < SamusSpeedBoostCounter.AuthoredStages; stage++)
        {
            SamusRunningCadenceTiming timing = SamusRunningCadenceDefinitions.Timings.Values.Single(cadence => cadence.Boost?.Stage == stage);
            boostStrides.AddRange(timing.Delays);
            boostCounts.Add(timing.Boost!.CountdownAfterAdvance);
        }

        var file = new EngineObjectFile(_tree, "samus/samus.json", "Samus");
        file.Value("half_width", SamusPoseCollisionDefinitions.HorizontalRadius)
            .Value("knockback_updates", EnemyCollisionDefinitions.SamusTouchKnockbackUpdates)
            .Value("invincible_updates", EnemyCollisionDefinitions.SamusTouchInvincibilityUpdates)
            .Value("hazard_knockback_updates", SpikeHazardDefinitions.KnockbackFrames)
            .Value("hazard_invincible_updates", SpikeHazardDefinitions.InvincibilityFrames)

            // The old game sounds heat and a liquid's harm alike: the same sound, on the same updates, above the same energy.
            .Value("room_damage_sound", EngineNames.Sound(RoomSound.IncrementalEnergy))
            .Value("room_damage_sound_every", SamusCadenceDefinitions.LiquidDamageSound.Period)
            .Value("room_damage_sound_least_energy", SamusLiquidDamageDefinitions.DamageSoundLeastEnergy)
            .Value("turn_updates", SamusSegmentUpdates(P.TurningRightToLeftPose, Seg.Main))
            .Value("dying_updates", SamusDyingUpdates())
            .Value("land_effect", SamusParticleFile(SamusParticleKind.Dust))

            // She climbs as steeply as the old game's sloped shapes step from one pixel column to the next.
            .Value("climb_per_pixel", SteepestSlopeStep())
            .Value("starting_energy", SamusEnergyDefinitions.StartingEnergy)

            // What she can do, and the equipment each thing needs: the Godot game's own names.
            .Value("can_crouch", true)
            .Value("crouch_while_running", true)
            .Value("can_crawl", true)
            .Value("crawl_needs", "morph_ball")
            .Value("can_jump_while_crawling", true)
            .Value("jump_while_crawling_needs", "spring_ball")
            .Value("can_wall_jump", true)
            .Value("wall_jump_from_any_jump", false)
            .Value("wall_jump_needs_steering_away", true)
            .Value("jump_boost_needs", "hi_jump_boots")
            .Value("dash_action", "run")
            .Value("dash_boost_needs", "speed_booster")

            // A wall jump: she leaves the wall with no speed given her across and gains it by the wall jump's own
            // values for as long as the jump is held. The spin is ready to jump off a wall it has found for as long as
            // its ready picture is shown.
            .Value("wall_jump_reach", SamusMovementDefinitions.WallJumpReachPixels)
            .Value("wall_jump_steered", true)
            .Value("wall_jump_across_speed", 0)
            .Value("wall_jump_ready_updates", SamusSegmentUpdates(P.SpinJumpRightPose, Seg.WallJumpReady))

            // Crouching, standing up, curling up and uncurling each last as long as the pose between is shown, and a
            // jump from a plain crouch starts lifted.
            .Value("crouch_updates", SamusSegmentUpdates(P.CrouchingTransitionRightPose, Seg.Main))
            .Value("stand_updates", SamusSegmentUpdates(P.StandingTransitionRightPose, Seg.Main))
            .Value("crawl_enter_updates", SamusSegmentUpdates(P.MorphingTransitionRightPose, Seg.Main))
            .Value("crawl_leave_updates", SamusSegmentUpdates(P.UnmorphingTransitionRightPose, Seg.Main))
            .Value("crouch_jump_lift", SamusPoseCollisionDefinitions.CrouchJumpLift)

            // A jump from standing is gathered for as long as its first pose is shown. A jump held for fewer updates
            // than the old limit as she lands or finishes a turn counts as pressed afresh.
            .Value("jump_start_updates", SamusSegmentUpdates(P.NeutralJumpTransitionRightPose, Seg.Main))
            .Value("jump_buffer_updates", SamusAutoJumpDefinitions.TimerComparisonLimit)
            .Value("landing_updates", SamusSegmentUpdates(P.NormalLandingRightPose, Seg.Main))
            .Value("spin_landing_updates", SamusSegmentUpdates(P.SpinLandingRightPose, Seg.Main))

            // Letting go of the jump while she rises stops her rise at once.
            .Value("jump_cut_speed", Number(SamusVerticalVelocity.FallFromRest.Speed))
            .Value("crawl_bounce_speed", Number(SamusVerticalMotionDefinitions.BallBounceSpeed))
            .Value("crawl_second_bounce_speed", Number(SamusVerticalMotionDefinitions.SecondBallBounceSpeed))
            .Value("crawl_bounce_fall_speed", SamusMovementDefinitions.FirstBallBounceMinimumSpeed)
            .Value("dash_acceleration", Number(SamusMovementDefinitions.HorizontalMotion.ExtraRunAcceleration))
            .Value("dash_cap", Number(SamusHorizontalSpeedDefinitions.UnboostedExtraRunSpeedCap))
            .Value("boosted_dash_cap", Number(SamusHorizontalSpeedDefinitions.BoostedExtraRunSpeedCap))
            .Value("weapons", weapons)
            .Value("varia_damage_over", SamusSuitDamageDefinitions.VariaDivisor)
            .Value("gravity_damage_over", SamusSuitDamageDefinitions.GravityDivisor)
            .Value("low_energy", SamusEnergyDefinitions.HealthyEnergy - 1)
            .Value("run_stride_updates", EngineNames.List(SamusPoseTraits.Of(P.MovingRightNormalPose).Animation.Segment(Seg.Run).Delays))
            .Value("dash_stride_updates", EngineNames.List(SamusRunningCadenceDefinitions.Timings[SamusRunningCadence.Ordinary].Delays))
            .Value("boost_stride_updates", EngineNames.List(boostStrides))
            .Value("boost_stride_counts", EngineNames.List(boostCounts))
            .Value("stored_shine_updates", ShinesparkDefinitions.StoredShineFrames)
            .Value("shinespark_windup_updates", ShinesparkDefinitions.WindupFrames)
            .Value("shinespark_speed", Number(ShinesparkDefinitions.WindupRunSpeed))
            .Value("shinespark_top_speed", Number(ShinesparkDefinitions.MostRunSpeed))
            .Value("shinespark_rise_gain", ShinesparkDefinitions.WindupInitialVerticalAcceleration.Pixels)
            .Value("shinespark_most_rise", ShinesparkDefinitions.MostRiseStep.Whole)
            .Value("shinespark_least_energy", ShinesparkDefinitions.MinimumSustainingEnergy)
            .Value("shinespark_crash_updates", ShinesparkDefinitions.CrashHoldFrames)
            .Value("shinespark_echo_shot", ShinesparkEchoFile)
            .Value("special_attack_updates", BeamChargeDefinitions.SpecialAttackCounter)
            .Value("charge_sound_updates", BeamChargeDefinitions.ChargeSoundStartCounter)
            .Value("bomb_jump_fuse_left", MorphBallBombDefinitions.BombJumpExplosionCount)
            .Value("hard_landing_speed", SamusMovementDefinitions.HardLandingSpeed.Whole)
            .Value("crystal_flash_energy_below", CrystalFlashDefinitions.AdmittingEnergyBelow)
            .Value("crystal_flash_ammunition", CrystalFlashDefinitions.AmmunitionPerKind)
            .Value("crystal_flash_energy_each", CrystalFlashDefinitions.EnergyPerAmmunition)
            .Value("crystal_flash_drain_every", SamusCadenceDefinitions.CrystalFlashDrain.Period)
            .Value("charged_spin_updates", BeamChargeDefinitions.FullyChargedCounter)
            .Value("xray_widening_step", Steps(XrayScopeDefinitions.Cone.WideningStep))
            .Value("xray_widest_half_width", WholeSteps(XrayScopeDefinitions.Cone.WidestHalfWidth))
            .Value("xray_aim_bands", EngineNames.List(
            [
                WholeSteps(XrayScopeDefinitions.AnimationAngles.RightRaisedFrom), WholeSteps(XrayScopeDefinitions.AnimationAngles.RightLevelFrom),
                WholeSteps(XrayScopeDefinitions.AnimationAngles.RightLoweredFrom), WholeSteps(XrayScopeDefinitions.AnimationAngles.RightDownFrom),
            ]));

        // Her body's height in each form: the old poses' own.
        file.Group("forms", new JsonObject
        {
            ["standing"] = SamusPoseTraits.Of(P.FacingRightNormalPose).VerticalRadius,
            ["crouching"] = SamusPoseTraits.Of(P.CrouchingRightPose).VerticalRadius,
            ["crawling"] = SamusPoseTraits.Of(P.MorphBallGroundRightPose).VerticalRadius,
            ["jumping"] = SamusPoseTraits.Of(P.NeutralJumpRightPose).VerticalRadius,
            ["falling"] = SamusPoseTraits.Of(P.FallingRightPose).VerticalRadius,
            ["aiming_down"] = SamusPoseTraits.Of(P.NormalJumpAimDownRightPose).VerticalRadius,
            ["spinning"] = SamusPoseTraits.Of(P.SpinJumpRightPose).VerticalRadius,
        });
        file.Group("mediums", SamusMediumsWritten());

        // Heat is the one harm of a room itself her suits spare her; a liquid's type says what spares her its own.
        file.Group("ongoing_damage_cancelled_by", new JsonObject { ["heat"] = new JsonArray("varia_suit", "gravity_suit") });
        file.Group("body_powers", new JsonObject
        {
            ["screw_attack"] = new JsonObject { ["damage"] = EnemyCollisionDefinitions.ScrewAttackContactDamage, ["kinds"] = new JsonArray("screw_attack") },
            ["speed_boost"] = new JsonObject { ["damage"] = EnemyCollisionDefinitions.SpeedBoosterContactDamage, ["kinds"] = new JsonArray("speed_boost") },
            ["shinespark"] = new JsonObject { ["damage"] = EnemyCollisionDefinitions.ShinesparkContactDamage, ["kinds"] = new JsonArray("shinespark") },
            ["charged_spin"] = new JsonObject { ["damage"] = EnemyCollisionDefinitions.PseudoScrewContactDamage, ["kinds"] = new JsonArray("charged_spin") },
        });
        file.Group("muzzles", new JsonObject
        {
            ["standing"] = Origins(SamusMuzzleOriginDefinitions.Standing),
            ["running"] = Origins(SamusMuzzleOriginDefinitions.Running),
        });
        file.Group("grapple_origins", new JsonObject
        {
            ["standing"] = Origins(GrappleFiringDefinitions.StandingOrigins),
            ["running"] = Origins(GrappleFiringDefinitions.RunningOrigins),
        });

        // The grapple beam's hit is of one point: what it destroys, it destroys by the reaction written for it.
        file.Group("grapple", new JsonObject
        {
            ["extend_speed"] = GrappleFiringDefinitions.CardinalSpeed.Pixels,
            ["extend_diagonal_speed"] = GrappleFiringDefinitions.DiagonalSpeed.Pixels,
            ["reach"] = GrappleBeamDefinitions.Physics.FiringCancelLength,
            ["damage"] = 1,
        });
        var specialAttacks = new JsonObject();
        foreach ((SamusBeamFlags beam, SamusComboKind kind) in SamusComboDefinitions.KindsOfSingleBeams)
            specialAttacks[SamusBeams.Single(one => one.Beam == beam).Equipment] = SpecialAttackFile(kind);
        file.Group("special_attacks", specialAttacks);

        var timelines = new JsonObject();
        var heights = new JsonObject();
        WriteSamusLooks(timelines, heights);
        file.Group("shot_heights", heights);
        foreach ((string name, JsonNode? timeline) in timelines.ToList())
        {
            timelines.Remove(name);
            file.Timeline(name, timeline!.AsObject());
        }

        file.Write();
    }
}
