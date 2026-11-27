using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.Zoa;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The enemies whose animations raise cues: what each of the old game's cues is to the Godot game.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes the enemies whose old cues are a kind and nothing more.</summary>
    private void WriteCuedEnemies()
    {
        Enemy("enemies/beetom.json", "Beetom", EnemyDefinitionId.Beetom).BodyColliders(Ordinary)
            .Value("bite_every", BeetomCadenceDefinitions.DrainDamage.Period)
            .Value("feeding_sound_every", BeetomCadenceDefinitions.DrainSound.Period)
            .Value("feeding_sound_least_energy", BeetomCadenceDefinitions.DrainSoundLeastHealth)
            .Value("cling_above", BeetomMotionDefinitions.DrainHoldAbove)
            .Value("crawl_speed", EngineNames.Number(BeetomMotionDefinitions.CrawlSpeed))
            .Value("crawl_updates", BeetomMotionDefinitions.CrawlDuration)
            .Value("idle_updates", BeetomMotionDefinitions.IdleDuration)
            .Value("ledge_reach", BeetomMotionDefinitions.LedgeProbeReach)
            .Value("drop_speed", EngineNames.Number(BeetomMotionDefinitions.DropSpeed))
            .Value("fastest_mark", EngineNames.Marks(BeetomMotionDefinitions.MaximumYSpeedCurvePlace))
            .Value("flung_marks", BeetomMotionDefinitions.FlungFallingMarks)
            .Value("flung_speed", EngineNames.Number(BeetomMotionDefinitions.FlungSpeed))
            .Value("thrown_distance", BeetomMotionDefinitions.EscapeThrow)
            .Value("shake_off_count", BeetomMotionDefinitions.MashCount)
            .Value("hop_speed", EngineNames.Number(BeetomMotionDefinitions.HopSpeed))
            .Value("short_hop_height", Whole(BeetomLeapDefinitions.ShortLeap.Height))
            .Value("short_hop_marks", BeetomMotionDefinitions.ShortHopMarks)
            .Value("long_hop_height", Whole(BeetomLeapDefinitions.LongLeap.Height))
            .Value("long_hop_marks", BeetomMotionDefinitions.LongHopMarks)
            .Value("lunge_distance", BeetomMotionDefinitions.ProximityDistance)
            .Value("lunge_height", Whole(BeetomLeapDefinitions.Lunge.Height))
            .Value("lunge_aim_marks", EngineNames.Marks(BeetomLeapDefinitions.Lunge.StepPerPass))
            .Value("lunge_rising_marks", BeetomMotionDefinitions.LungeRisingMarks)
            .Value("lunge_falling_marks", BeetomMotionDefinitions.LungeFallingMarks)
            .Value("lunge_speed", EngineNames.Number(BeetomMotionDefinitions.LungeSpeed))
            .Value("feeding_sound", EngineNames.Sound(RoomSound.IncrementalEnergy))
            .Looks(BeetomAnimations.Segments, BeetomAnimations.StartingWith,
                static (BeetomCue cue) => cue.Kind == BeetomCueKind.ProcessOffScreen ? EngineNames.AlwaysActive : EngineNames.NotAlwaysActive,
                ("crawling_left", BeetomSegment.CrawlingLeft), ("crawling_right", BeetomSegment.CrawlingRight),
                ("hop_left", BeetomSegment.HopLeft), ("hop_right", BeetomSegment.HopRight),
                ("draining_left", BeetomSegment.DrainingLeft), ("draining_right", BeetomSegment.DrainingRight))
            .Write();

        Enemy("enemies/boyon.json", "Boyon", EnemyDefinitionId.Boyon).BodyColliders(Ordinary)
            .Looks(BoyonAnimations.Segments, BoyonAnimations.StartingWith,
                static (BoyonCue cue) => cue.Kind switch
                {
                    BoyonCueKind.ProcessOffScreen => EngineNames.AlwaysActive,
                    BoyonCueKind.StopProcessingOffScreen => EngineNames.NotAlwaysActive,
                    BoyonCueKind.PlayBounceSound => EngineNames.SoundCue(EnemySoundEffectDefinitions.BoyonBounceSound),
                    _ => throw new InvalidDataException($"A Boyon's animation raises {cue.Kind}, which this does not know."),
                },
                ("idle", BoyonSegment.Idle), ("bouncing", BoyonSegment.Bouncing))
            .Write();

        var fallingPaths = new System.Text.Json.Nodes.JsonObject();
        foreach (ChootFallingPath path in Enum.GetValues<ChootFallingPath>())
        {
            ChootFallingPathDefinition fall = ChootFallingPathDefinitions.Paths[path];
            fallingPaths[EngineSheets.FileName(path.ToString())] = new System.Text.Json.Nodes.JsonObject
            {
                ["loop_fall"] = fall.LoopYDistance,
                ["samples"] = EngineNames.List(fall.Samples.Select(sample => (System.Text.Json.Nodes.JsonNode)new System.Text.Json.Nodes.JsonObject
                {
                    ["x"] = sample.Offset.X, ["y"] = sample.Offset.Y, ["updates"] = sample.Updates,
                })),
            };
        }

        Enemy("enemies/choot.json", "Choot", EnemyDefinitionId.Choot).BodyColliders(Ordinary)
            .Value("reach", ChootMotionDefinitions.ActivationDistance)
            .Value("ascent_step", MarkParts(ChootMotionDefinitions.AscentStep))
            .Group("falling_paths", fallingPaths)
            .Looks(ChootAnimations.Segments, ChootAnimations.StartingWith,
                static (ChootCue cue) => cue.Kind == ChootCueKind.ProcessOffScreen ? EngineNames.AlwaysActive : EngineNames.NotAlwaysActive,
                ("idle", ChootSegment.Idle), ("jumping", ChootSegment.Jumping), ("falling", ChootSegment.Falling))
            .Write();

        // The Dragon's attack tells it when it has spat its last.
        Enemy("enemies/dragon.json", "Dragon", EnemyDefinitionId.Dragon).BodyColliders(Ordinary)
            .Value("rise_updates", DragonTimingDefinitions.RiseOrSinkDuration)
            .Value("shots_per_attack", DragonTimingDefinitions.ShotsPerAttack)
            .Value("wait_before_sinking", DragonTimingDefinitions.WaitBeforeSink)
            .Value("wait_before_rising", DragonTimingDefinitions.WaitBeforeRise)
            .Value("fireball_rise", EngineNames.Number(-DragonFireballDefinitions.LaunchRise))
            .Value("muzzle_across", DragonFireballDefinitions.MuzzleFacingRight.X)
            .Value("muzzle_up", -DragonFireballDefinitions.MuzzleFacingRight.Y)
            // Each spit is heard whether or not the room has a place for its fireball.
            .Value("spit_sound", EngineNames.Sound(EnemySoundEffectDefinitions.DragonFireballSound))
            // Its fireball rises and falls through everything, shown by other pictures once it falls.
            .Shot("fireball", RoomEnemyProjectileKind.DragonFireball, static (dragon, shot, projectile) =>
            {
                shot["speed"] = EngineNames.Number(DragonFireballDefinitions.SidewaysSpeed);
                foreach ((string name, DragonFireballSegment segment) in new[]
                         {
                             ("fireball_rising_left", DragonFireballSegment.RisingLeft), ("fireball_rising_right", DragonFireballSegment.RisingRight),
                             ("fireball_falling_left", DragonFireballSegment.FallingLeft), ("fireball_falling_right", DragonFireballSegment.FallingRight),
                         })
                {
                    dragon.SpawnedTimeline(name, DragonFireballAnimations.Segments, DragonFireballAnimations.StartingAt[segment], EngineEnemy.EndsOnly);
                }

                projectile["timeline"] = $"{dragon.Folder}/fireball_rising_{{side}}.json";
                projectile["falling_timeline"] = $"{dragon.Folder}/fireball_falling_{{side}}.json";
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["motion"] = "falling";
                projectile["acceleration"] = EngineNames.Number(DragonFireballDefinitions.Gravity);
                projectile["meeting_terrain"] = "passes_through";
                EngineEnemy.GoneFarBelowView(projectile);
            })
            .Looks(DragonAnimations.Segments, DragonAnimations.StartingWith, static (DragonCue _) => "spit",
                ("idle_left", DragonSegment.IdleFacingLeft), ("idle_right", DragonSegment.IdleFacingRight),
                ("wings_left", DragonSegment.WingsFacingLeft), ("wings_right", DragonSegment.WingsFacingRight),
                ("attack_left", DragonSegment.AttackingFacingLeft), ("attack_right", DragonSegment.AttackingFacingRight))
            .Write();

        Enemy("enemies/skultera.json", "Skultera", EnemyDefinitionId.Skultera).BodyColliders(Ordinary)
            .Looks(SkulteraAnimations.Segments, SkulteraAnimations.StartingWith, static (SkulteraCue _) => "turned",
                ("swimming_left", SkulteraSegment.SwimmingLeft), ("swimming_right", SkulteraSegment.SwimmingRight),
                ("turning_left", SkulteraSegment.TurningLeft), ("turning_right", SkulteraSegment.TurningRight))
            .Write();

        // The Waver's roll is over when its timeline is; the Godot game asks the timeline, not a cue.
        Enemy("enemies/waver.json", "Waver", EnemyDefinitionId.Waver).BodyColliders(Ordinary)
            .Value("speed", EngineNames.Number(WaverMotionDefinitions.HorizontalSpeed))
            .Value("wave_height", WaverMotionDefinitions.WaveHeight)
            .Value("wave_step", WholeSteps(WaverMotionDefinitions.WaveStep))
            .Value("spins_at", (int)WaverMotionDefinitions.SpinPlaceInHalfTurn.RawValue)
            .Looks(WaverAnimations.Segments, WaverAnimations.StartingWith, static (WaverCue _) => null,
                ("steady_left", WaverSegment.SteadyFacingLeft), ("steady_right", WaverSegment.SteadyFacingRight),
                ("spin_left", WaverSegment.SpinningFacingLeft), ("spin_right", WaverSegment.SpinningFacingRight))
            .Write();

        // A Zoa's animations say how fast it goes from then on.
        Enemy("enemies/zoa.json", "Zoa", EnemyDefinitionId.Zoa).BodyColliders(Ordinary)
            .Value("reach", ZoaSpeedDefinitions.ActivationDistance)
            .Value("rise_speed", EngineNames.Number(ZoaSpeedDefinitions.RisingSpeed))
            .Looks(ZoaAnimations.Segments, ZoaAnimations.StartingWith, static (ZoaCue cue) => "speed:" + EngineNames.InCue(cue.LaunchSpeed),
                ("shooting_left", ZoaSegment.FacingLeftShooting), ("shooting_right", ZoaSegment.FacingRightShooting),
                ("rising_left", ZoaSegment.FacingLeftRising), ("rising_right", ZoaSegment.FacingRightRising))
            .Write();

        // An Owtch's walk says which way it walks.
        Enemy("enemies/owtch.json", "Owtch", EnemyDefinitionId.Owtch).BodyColliders("Hurtbox")
            .Value("burial_depth", OwtchMovementDefinitions.MaximumBurialDepth)
            .Value("sink_chance", OwtchMovementDefinitions.Sinks.TakingValues)
            .Looks(OwtchAnimations.Segments, OwtchAnimations.StartingWith, static (OwtchCue cue) => "move:" + EngineNames.Side(cue.Direction),
                ("moving_left", OwtchSegment.MovingLeft), ("moving_right", OwtchSegment.MovingRight))
            .Write();

        // The divers: the Skree and the Metaree. The Metaree is set off from further away, steers harder, and dives the
        // faster the further below it Samus is: its slowest dive, and how much further down she must be for each pixel more.
        int slowestMetareeDive = SuperMetroid.Core.Compatibility.MetareeDiveSpeed.Of(0, 0);
        int distanceForFasterDive = Enumerable.Range(1, 4096).First(below => SuperMetroid.Core.Compatibility.MetareeDiveSpeed.Of(0, below) > slowestMetareeDive);
        foreach ((string file, EnemyDefinitionId id, SkreeMetareeSegment idle, SkreeMetareeSegment windup, SkreeMetareeSegment dive, SkreeMetareeParticleSegment debris,
                     int reach, int diveSpeed, int distancePerSpeed, int drift, int burrowUpdates, int debrisAt) in new[]
                 {
                     ("skree", EnemyDefinitionId.Skree, SkreeMetareeSegment.SkreeIdling, SkreeMetareeSegment.SkreePreparingAttack, SkreeMetareeSegment.SkreeDiving,
                         SkreeMetareeParticleSegment.Skree,
                         SkreeMotionDefinitions.WindUpDistance, SkreeMotionDefinitions.DiveStep, 0, SkreeMotionDefinitions.DiveDrift,
                         SkreeMotionDefinitions.BurrowDuration, SkreeMotionDefinitions.DebrisBurstAt),
                     ("metaree", EnemyDefinitionId.Metaree, SkreeMetareeSegment.MetareeIdling, SkreeMetareeSegment.MetareePreparingAttack, SkreeMetareeSegment.MetareeDiving,
                         SkreeMetareeParticleSegment.Metaree,
                         SuperMetroid.Core.Enemies.Metaree.MetareeMotionDefinitions.ActivationDistance, slowestMetareeDive, distanceForFasterDive,
                         SuperMetroid.Core.Enemies.Metaree.MetareeMotionDefinitions.DiveSteer,
                         SuperMetroid.Core.Enemies.Metaree.MetareeMotionDefinitions.BurrowLifetime, SuperMetroid.Core.Enemies.Metaree.MetareeMotionDefinitions.BurstCountdown),
                 })
        {
            // The debris it throws up: two pieces to each side, an outer and an inner, which fall through everything.
            var pieces = new System.Text.Json.Nodes.JsonArray();
            foreach (PixelVelocity<RoomPixels> thrown in new[] { MetalParticleBurstDefinitions.OuterRightVelocity, MetalParticleBurstDefinitions.InnerRightVelocity })
            {
                pieces.Add(new System.Text.Json.Nodes.JsonObject
                {
                    ["side"] = MetalParticleBurstDefinitions.SideOffset,
                    ["across"] = EngineNames.Number(thrown.X),
                    ["up"] = EngineNames.Number(-thrown.Y),
                });
            }

            Enemy($"enemies/{file}.json", "Diver", id).BodyColliders(Ordinary)
                .Value("reach", reach)
                .Value("dive_speed", diveSpeed)
                .Value("distance_per_speed", distancePerSpeed)
                .Value("drift", drift)
                .Value("burrow_updates", burrowUpdates)
                .Value("debris_at", debrisAt)
                .Group("debris", pieces)
                .Shot("debris", RoomEnemyProjectileKind.SkreeMetareeParticle, (diver, _, projectile) =>
                {
                    projectile["timeline"] = diver.SpawnedTimeline("debris", SkreeMetareeParticleAnimations.Segments,
                        SkreeMetareeParticleAnimations.StartingAt[debris], EngineEnemy.EndsOnly);
                    projectile["picture_offset"] = EngineEnemy.NoOffset();
                    projectile["motion"] = "falling";
                    projectile["acceleration"] = EngineNames.Number(MetalParticleBurstDefinitions.Gravity);
                    projectile["meeting_terrain"] = "passes_through";
                    EngineEnemy.GoneOutOfView(projectile);
                })
                .Value("dive_sound", EngineNames.Sound(EnemySoundEffectDefinitions.DiveSound))
                .Value("burrow_sound", EngineNames.Sound(EnemySoundEffectDefinitions.BurrowSound))
                .Looks(SkreeMetareeAnimations.Segments, SkreeMetareeAnimations.StartingWith,
                    static (SkreeMetareeCue cue) => cue.Kind == SkreeMetareeCueKind.SetAttackReady ? "ready" : EngineNames.AlwaysActive,
                    ("idle", idle), ("windup", windup), ("dive", dive))
                .Write();
        }

        // The hoppers: five kinds, each with its own pictures on a floor and on a ceiling.
        foreach ((string file, EnemyDefinitionId id) in new[]
                 {
                     ("sidehopper", EnemyDefinitionId.Sidehopper), ("sidehopper_large", EnemyDefinitionId.SidehopperLarge),
                     ("sidehopper_tourian", EnemyDefinitionId.SidehopperTourian), ("dessgeega", EnemyDefinitionId.Dessgeega),
                     ("dessgeega_large", EnemyDefinitionId.DessgeegaLarge),
                 })
        {
            HopperKind kind = HopperKindDefinitions.Kinds[id];
            HopperSegment Of(HopperSurface surface, HopperPhase phase) => HopperAnimationDefinitions.Animations[new(kind, surface, phase)];
            Enemy($"enemies/{file}.json", "Hopper", id).BodyColliders(Ordinary)
                .Value("across_speed", HopperHopDefinitions.HorizontalSpeed)
                .Value("small_hop_height", Whole(HopperHopDefinitions.SmallHop.Height))
                .Value("small_hop_step", MarkParts(HopperHopDefinitions.SmallHop.StepPerPass))
                .Value("big_hop_height", Whole(HopperHopDefinitions.BigHop.Height))
                .Value("big_hop_step", MarkParts(HopperHopDefinitions.BigHop.StepPerPass))
                .Value("fastest_mark", EngineNames.Marks(HopperHopDefinitions.FastestReturnPlace))
                .Looks(HopperAnimations.Segments, HopperAnimations.StartingWith,
                    static (HopperCue cue) => cue switch
                    {
                        HopperCue.PlaySound sound => EngineNames.SoundCue(sound.Sound),
                        HopperCue.Signal { Kind: HopperCueKind.ReadyToHop } => "ready",
                        HopperCue.Signal { Kind: HopperCueKind.ProcessOffScreen } => EngineNames.AlwaysActive,
                        HopperCue.Signal { Kind: HopperCueKind.StopProcessingOffScreen } => EngineNames.NotAlwaysActive,
                        _ => throw new InvalidDataException($"A hopper's animation raises {cue}, which this does not know."),
                    },
                    ("landed", Of(HopperSurface.Floor, HopperPhase.Landed)), ("jumping", Of(HopperSurface.Floor, HopperPhase.Jumping)),
                    ("landed_ceiling", Of(HopperSurface.Ceiling, HopperPhase.Landed)), ("jumping_ceiling", Of(HopperSurface.Ceiling, HopperPhase.Jumping)))
                .Write();
        }
    }
}
