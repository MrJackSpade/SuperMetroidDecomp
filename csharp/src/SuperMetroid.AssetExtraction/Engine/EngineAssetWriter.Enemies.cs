using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Shutters;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The enemies' data files: for each, the Godot type that reads it, the old enemy it is, and the old animation that
/// each timeline the type plays is.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The colliders an ordinary enemy's scene has: what moves it through the room, and what a hit lands on.</summary>
    private static readonly string[] Ordinary = ["Body", "Hurtbox"];

    /// <summary>Writes every enemy's data file.</summary>
    private void WriteEnemies()
    {
        WriteAtomic();
        WriteCrawlers();
        WriteSimpleEnemies();
        WritePipeBugs();
        WriteShutters();
        WriteCuedEnemies();
        WriteSpittingEnemies();
        WriteMoreCuedEnemies();
        WritePartedEnemies();
        WritePirates();
        WriteCorpses();
        WriteGlowingEnemies();
        WriteBarriersAndFakeKraid();
    }

    /// <summary>The Atomic: its four ways of spinning.</summary>
    private void WriteAtomic() =>
        Enemy("enemies/atomic.json", "Atomic", EnemyDefinitionId.Atomic).BodyColliders(Ordinary)
            .Looks(AtomicAnimations.Segments, AtomicAnimations.StartingWith,
                ("up_right_turning", AtomicSegment.UpRightTurning), ("up_left_turning", AtomicSegment.UpLeftTurning),
                ("up_right_turning_back", AtomicSegment.UpRightTurningBack), ("up_left_turning_back", AtomicSegment.UpLeftTurningBack))
            .Write();

    /// <summary>The crawlers: one Godot type, each kind with its own pictures of itself on each surface.</summary>
    private void WriteCrawlers()
    {
        (string File, EnemyDefinitionId Id, CrawlerAnimationFamily Family)[] kinds =
        [
            ("zoomer", EnemyDefinitionId.Zoomer, CrawlerAnimationFamily.Shared),
            ("armored_zoomer", EnemyDefinitionId.ArmoredZoomer, CrawlerAnimationFamily.Shared),
            ("orange_zoomer", EnemyDefinitionId.OrangeZoomer, CrawlerAnimationFamily.Shared),
            ("zeela", EnemyDefinitionId.Zeela, CrawlerAnimationFamily.Shared),
            ("sova", EnemyDefinitionId.Sova, CrawlerAnimationFamily.Shared),
            ("sciser", EnemyDefinitionId.Sciser, CrawlerAnimationFamily.Sciser),
            ("zero", EnemyDefinitionId.Zero, CrawlerAnimationFamily.Zero),
            ("viola", EnemyDefinitionId.Viola, CrawlerAnimationFamily.Viola),
        ];
        foreach ((string file, EnemyDefinitionId id, CrawlerAnimationFamily family) in kinds)
        {
            EngineEnemy crawler = Enemy($"enemies/{file}.json", "Crawler", id).BodyColliders(Ordinary);

            // It lets go of its surface as the shake of a super missile striking a wall begins, and at no other.
            crawler.Data["falls_with_shake"] = Shake(SuperMissileDefinitions.WallImpactEarthquake, SuperMissileDefinitions.EarthquakeDuration);
            crawler
                .Value("landing_speed", EngineNames.Number(CrawlerMotionDefinitions.DefaultCrawlSpeed.X))
                // The orange Zoomer is the one that turns toward Samus.
                .Value("pursues", id == EnemyDefinitionId.OrangeZoomer)
                .Looks(CrawlerAnimations.Segments, CrawlerAnimations.StartingWith,
                    ("floor", CrawlerSurfaceAnimations.AnimationFor(family, CrawlerSurfaceOrientation.OnFloor)),
                    ("ceiling", CrawlerSurfaceAnimations.AnimationFor(family, CrawlerSurfaceOrientation.OnCeiling)),
                    ("left_wall", CrawlerSurfaceAnimations.AnimationFor(family, CrawlerSurfaceOrientation.OnLeftWall)),
                    ("right_wall", CrawlerSurfaceAnimations.AnimationFor(family, CrawlerSurfaceOrientation.OnRightWall)))
                .Write();
        }
    }

    /// <summary>The enemies whose animations raise nothing the Godot game hears, and whose values are their types' own.</summary>
    private void WriteSimpleEnemies()
    {
        Enemy("enemies/boulder.json", "Boulder", EnemyDefinitionId.Boulder).BodyColliders(Ordinary)
            .Value("first_rebound_mark", EngineNames.Marks(BoulderMotionDefinitions.FirstReboundBounceSpeed))
            .Value("fastest_bouncing_mark", EngineNames.Marks(BoulderMotionDefinitions.MaximumBouncingSpeed))
            .Value("bouncing_travel_step", MarkParts(BoulderMotionDefinitions.BouncingTravelSpeedStep))
            .Value("rolling_travel_step", MarkParts(BoulderMotionDefinitions.RollingTravelSpeedStep))
            .Value("fastest_rolling_mark", EngineNames.Marks(BoulderMotionDefinitions.MaximumRollingTravelSpeed))
            .Value("ground_reach", BoulderMotionDefinitions.RollingGroundReach)
            .Value("break_effect", DustFile(BoulderMotionDefinitions.BreakDust))
            .Value("break_sound", EngineNames.Sound(EnemySoundEffectDefinitions.BoulderBreakSound))
            .Value("impact_sound", EngineNames.Sound(EnemySoundEffectDefinitions.BoulderImpactSound))
            // Where on the curve each of its rebounds starts, one for each time it lands; after the last it rolls.
            .Group("rebound_marks", EngineNames.List(Enumerable.Range(0, BoulderBounces.ImpactCount).Select(landed => EngineNames.Marks(BoulderBounces.RestartPlace(landed)))))
            .Looks(BoulderAnimations.Segments, BoulderAnimations.StartingWith, ("left", BoulderSegment.Left), ("right", BoulderSegment.Right))
            .Write();
        Enemy("enemies/bull.json", "Bull", EnemyDefinitionId.Bull).BodyColliders(Ordinary)
            .Value("acceleration_step", EngineNames.Number(BullMovementDefinitions.AccelerationStep))
            .Value("kick_acceleration", EngineNames.Number(BullMovementDefinitions.ShotKickAcceleration))
            .Value("kick_speed", EngineNames.Number(BullMovementDefinitions.ShotKickSpeed))
            .Value("kick_guard_updates", BullMovementDefinitions.ShotReactionGuardDuration)
            .Value("off_target_turn", (int)BullMovementDefinitions.OffTargetTurn.RawValue)
            .Value("wait_updates", BullMovementDefinitions.MovementDelay)
            .Looks(BullAnimations.Segments, BullAnimations.StartingWith, ("idle", BullSegment.Idle))
            .Timeline("shot", BullAnimations.Segments, BullAnimations.Shot)
            .Write();
        Enemy("enemies/multiviola.json", "Multiviola", EnemyDefinitionId.Multiviola).BodyColliders(Ordinary)
            .Looks(MultiviolaAnimations.Segments, MultiviolaAnimations.StartingWith, ("flying", MultiviolaSegment.Flying))
            .Write();
        Enemy("enemies/fireflea.json", "Fireflea", EnemyDefinitionId.Fireflea).BodyColliders(Ordinary)
            .Looks(FirefleaAnimations.Segments, FirefleaAnimations.StartingWith, ("flying", FirefleaSegment.BrightestWingbeat))
            .Write();
        foreach ((string file, EnemyDefinitionId id) in new[]
                 {
                     ("mellow", EnemyDefinitionId.Mellow), ("mella", EnemyDefinitionId.Mella), ("maridia_fly", EnemyDefinitionId.MaridiaFly),
                 })
        {
            Enemy($"enemies/{file}.json", "Fly", id).BodyColliders(Ordinary)
                .Value("reach", FlyMotionDefinitions.AttackHorizontalRange)
                .Value("wheel_step", FlyMotionDefinitions.CircleTurnSteps)
                .Value("rest_updates", FlyMotionDefinitions.AttackCooldown)
                .Value("dive_across_speed", FlyMotionDefinitions.DiveAcrossSpeed)
                .Value("dive_down_speed", FlyMotionDefinitions.DiveDownSpeed)
                .Looks(FlyAnimations.Segments, FlyAnimations.StartingWith, ("flying", FlySegment.Flight))
                .Write();
        }

        Enemy("enemies/kago.json", "Kago", EnemyDefinitionId.Kago).BodyColliders(Ordinary)
            .Value("shake_strength", ShakeStrength(KagoDefinitions.ShotQuake))
            .Value("shake_updates", KagoDefinitions.ShotQuakeDuration)
            .Value("shake_way", ShakeWayName(KagoDefinitions.ShotQuake))
            .Looks(KagoAnimations.Segments, KagoAnimations.StartingWith, ("slow", KagoSegment.Slow), ("fast", KagoSegment.Fast))
            .Write();
        Enemy("enemies/mochtroid.json", "Mochtroid", EnemyDefinitionId.Mochtroid).BodyColliders(Ordinary)
            .Value("attached_speed", Whole(MochtroidBehaviorDefinitions.AttachedSpeed))
            .Value("bite_every", MochtroidBehaviorDefinitions.AttachedDrainPeriod)
            .Value("fastest_flight", Whole(MochtroidBehaviorDefinitions.FastestFlight))
            .Value("feeding_sound_every", MetroidCadenceDefinitions.DrainSound.Period)
            .Value("feeding_sound_least_energy", MetroidBehaviorDefinitions.DrainSoundLeastHealth)
            .Value("feeding_sound", EngineNames.Sound(RoomSound.IncrementalEnergy))
            .Looks(MochtroidAnimations.Segments, MochtroidAnimations.StartingWith,
                ("free", MochtroidSegment.FreeFlight), ("attached", MochtroidSegment.Attached))
            .Write();
        var hops = new System.Text.Json.Nodes.JsonObject();
        foreach ((string name, PuyoHopType type) in new[]
                 {
                     ("small", PuyoHopType.NormalSmall), ("big", PuyoHopType.NormalBig), ("long", PuyoHopType.NormalLong), ("giant", PuyoHopType.Giant),
                     ("dropping", PuyoHopType.Dropping), ("dropped_small", PuyoHopType.DroppedSmall), ("dropped_big", PuyoHopType.DroppedBig),
                 })
        {
            PuyoHopDefinition hop = PuyoHopDefinitions.Hops[type];
            hops[name] = new System.Text.Json.Nodes.JsonObject
            {
                ["height"] = hop.JumpHeight,
                ["across"] = hop.XPixels,
                // Both steps are in the parts of a mark the Godot game's curve counts by, which are the old game's.
                ["search_step"] = hop.LaunchSearchStep.InTwoHundredFiftySixthsOfMark,
                ["curve_step"] = hop.CurveStep.InTwoHundredFiftySixthsOfMark,
                ["drop_speed"] = EngineNames.Number(hop.DropSpeed),
            };
        }

        Enemy("enemies/puyo.json", "Puyo", EnemyDefinitionId.Puyo).BodyColliders(Ordinary)
            .Group("hops", hops)
            .Looks(PuyoAnimations.Segments, PuyoAnimations.StartingWith,
                ("grounded_slow", PuyoSegment.GroundedSlow), ("grounded_medium", PuyoSegment.GroundedMedium),
                ("grounded_fast", PuyoSegment.GroundedFast), ("upright", PuyoSegment.Upright),
                ("tilted_left", PuyoSegment.TiltedLeft), ("tilted_right", PuyoSegment.TiltedRight),
                ("tilted_far_left", PuyoSegment.TiltedFarLeft), ("tilted_far_right", PuyoSegment.TiltedFarRight))
            .Write();
        Enemy("enemies/dead_torizo.json", "DeadTorizo", EnemyDefinitionId.CorpseTorizo).BodyColliders("Body", "Hurtbox", "Solid")

            .Value("rows", DeadTorizoCorpseDefinitions.RowCount)
            .Value("sand_rows", DeadTorizoCorpseDefinitions.SandRowCount)
            .Value("sand_lines", DeadTorizoCorpseDefinitions.SandLines)
            .Value("sand_line_updates", DeadTorizoCorpseDefinitions.RotUpdatesPerSandLine)
            .Value("dust_effect", DustFile(DustExplosionAnimation.CorpseDustCloud))
            .Value("dust_sound", EngineNames.Sound(EnemySound.LiquidDamage))
            .Group("touch_areas", EngineNames.List(DeadTorizoTouchDefinitions.TouchHitboxes.ToArray().Select(area => (System.Text.Json.Nodes.JsonNode)new System.Text.Json.Nodes.JsonObject
            {
                ["left"] = area.Left, ["top"] = area.Top, ["right"] = area.Right, ["bottom"] = area.Bottom,
            })))
            .Looks(DeadTorizoAnimations.Segments, DeadTorizoAnimations.StartingWith, ("corpse", DeadTorizoSegment.Stationary))
            .Write();

        // The Rippers: the jet one shows the grapple one's flying, as the cartridge has it; the plain one has its own.
        foreach ((string file, EnemyDefinitionId id, RipperSegment left, RipperSegment right) in new[]
                 {
                     ("ripper", EnemyDefinitionId.Ripper, RipperSegment.RipperMovingLeft, RipperSegment.RipperMovingRight),
                     ("jet_ripper", EnemyDefinitionId.JetRipper, RipperSegment.GrappleRipperMovingLeft, RipperSegment.GrappleRipperMovingRight),
                     ("grapple_ripper", EnemyDefinitionId.GrappleRipper, RipperSegment.GrappleRipperMovingLeft, RipperSegment.GrappleRipperMovingRight),
                 })
        {
            Enemy($"enemies/{file}.json", "Ripper", id).BodyColliders(Ordinary)
                .Value("can_be_latched", id == EnemyDefinitionId.GrappleRipper)
                .Looks(RipperAnimations.Segments, RipperAnimations.StartingWith, ("left", left), ("right", right))
                .Write();
        }
    }

    /// <summary>The bugs that live in pipes: four kinds over three Godot types.</summary>
    private void WritePipeBugs()
    {
        Enemy("enemies/zeb.json", "PipeBug", EnemyDefinitionId.Zeb).BodyColliders("Hurtbox")
            .Value("emergence_height", PipeBugDefinitions.BrinstarEmergenceHeight)
            .Value("flight_step", PipeBugDefinitions.BrinstarFlightStep)
            .Value("respawn_wait", PipeBugDefinitions.BrinstarRespawnWait)
            .Value("trigger_top", PipeBugDefinitions.BrinstarTriggerTop)
            .Value("trigger_width", PipeBugDefinitions.BrinstarTriggerWidth)
            .Looks(PipeBugAnimations.Segments, PipeBugAnimations.StartingWith,
                ("rising_left", PipeBugSegment.ZebRisingLeft), ("rising_right", PipeBugSegment.ZebRisingRight),
                ("shooting_left", PipeBugSegment.ZebShootingLeft), ("shooting_right", PipeBugSegment.ZebShootingRight))
            .Write();
        Enemy("enemies/zebbo.json", "PipeBug", EnemyDefinitionId.Zebbo).BodyColliders("Hurtbox")
            .Value("emergence_height", PipeBugDefinitions.BrinstarEmergenceHeight)
            .Value("flight_step", PipeBugDefinitions.BrinstarFlightStep)
            .Value("respawn_wait", PipeBugDefinitions.BrinstarRespawnWait)
            .Value("trigger_top", PipeBugDefinitions.BrinstarTriggerTop)
            .Value("trigger_width", PipeBugDefinitions.BrinstarTriggerWidth)
            .Looks(PipeBugAnimations.Segments, PipeBugAnimations.StartingWith,
                ("rising_left", PipeBugSegment.ZebboRisingLeft), ("rising_right", PipeBugSegment.ZebboRisingRight),
                ("shooting_left", PipeBugSegment.ZebboShootingLeft), ("shooting_right", PipeBugSegment.ZebboShootingRight))
            .Write();
        Enemy("enemies/gamet.json", "Gamet", EnemyDefinitionId.Gamet).BodyColliders("Hurtbox")
            .Value("rise_speed", Whole(PipeBugDefinitions.NorfairRiseSpeed))
            .Looks(PipeBugAnimations.Segments, PipeBugAnimations.StartingWith,
                ("rising_left", PipeBugSegment.GametRisingLeft), ("rising_right", PipeBugSegment.GametRisingRight),
                ("flying_left", PipeBugSegment.GametFlyingLeft), ("flying_right", PipeBugSegment.GametFlyingRight))
            .Write();
        Enemy("enemies/geega.json", "Geega", EnemyDefinitionId.Geega).BodyColliders("Hurtbox")
            .Value("emergence_delay", PipeBugDefinitions.YellowEmergenceDelay)
            .Value("swoop_distance", PipeBugDefinitions.YellowArcTriggerDistance)
            .Value("swoop_start_mark", EngineNames.Marks(PipeBugDefinitions.YellowArcStart))
            .Value("trigger_distance", PipeBugDefinitions.YellowTriggerDistance)
            .Value("trigger_height", PipeBugDefinitions.YellowTriggerHeight)
            .Looks(PipeBugAnimations.Segments, PipeBugAnimations.StartingWith,
                ("flying_left", PipeBugSegment.GeegaFlyingLeft), ("flying_right", PipeBugSegment.GeegaFlyingRight),
                ("arcing_left", PipeBugSegment.GeegaArcingLeft), ("arcing_right", PipeBugSegment.GeegaArcingRight))
            .Write();
    }

    /// <summary>The shutters: three that slide, each one picture, and the one that grows a section at a time.</summary>
    private void WriteShutters()
    {
        foreach ((string file, EnemyDefinitionId id, ShutterSegment look) in new[]
                 {
                     ("shutter_shootable", EnemyDefinitionId.ShutterShootable, ShutterSegment.FullyGrown),
                     ("shutter_destroyable", EnemyDefinitionId.ShutterDestroyable, ShutterSegment.FullyGrown),
                     ("kamer", EnemyDefinitionId.Kamer, ShutterSegment.KamerPlatform),
                 })
        {
            Enemy($"enemies/{file}.json", "Shutter", id).BodyColliders("Body", "Hurtbox", "Solid")
                .Value("activation_sound", EngineNames.Sound(EnemySoundEffectDefinitions.ShutterActivationSound))
                .Looks(ShutterAnimations.Segments, ShutterAnimations.StartingWith, ("idle", look))
                .Write();
        }

        Enemy("enemies/shutter_growing.json", "GrowingShutter", EnemyDefinitionId.ShutterGrowing).BodyColliders("Body", "Hurtbox", "Solid")
            .Value("activation_sound", EngineNames.Sound(EnemySoundEffectDefinitions.ShutterActivationSound))
            .Looks(ShutterAnimations.Segments, ShutterAnimations.StartingWith,
                ("quarter", ShutterSegment.QuarterGrown), ("half", ShutterSegment.HalfGrown),
                ("three_quarters", ShutterSegment.ThreeQuartersGrown), ("full", ShutterSegment.FullyGrown))
            .Write();
    }
}
