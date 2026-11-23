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

        Enemy("enemies/choot.json", "Choot", EnemyDefinitionId.Choot).BodyColliders(Ordinary)
            .Looks(ChootAnimations.Segments, ChootAnimations.StartingWith,
                static (ChootCue cue) => cue.Kind == ChootCueKind.ProcessOffScreen ? EngineNames.AlwaysActive : EngineNames.NotAlwaysActive,
                ("idle", ChootSegment.Idle), ("jumping", ChootSegment.Jumping), ("falling", ChootSegment.Falling))
            .Write();

        // The Dragon's attack tells it when it has spat its last.
        Enemy("enemies/dragon.json", "Dragon", EnemyDefinitionId.Dragon).BodyColliders(Ordinary)
            .Value("fireball_rise", EngineNames.Number(-DragonFireballDefinitions.LaunchRise))
            .Value("muzzle_across", DragonFireballDefinitions.MuzzleFacingRight.X)
            .Value("muzzle_up", -DragonFireballDefinitions.MuzzleFacingRight.Y)
            // Its fireball rises and falls through everything, shown by other pictures once it falls.
            .Shot("fireball", RoomEnemyProjectileKind.DragonFireball, static (dragon, shot, projectile) =>
            {
                shot["speed"] = EngineNames.Number(DragonFireballDefinitions.SidewaysSpeed);
                shot["sound"] = EngineNames.Sound(EnemySoundEffectDefinitions.DragonFireballSound);
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
            .Looks(WaverAnimations.Segments, WaverAnimations.StartingWith, static (WaverCue _) => null,
                ("steady_left", WaverSegment.SteadyFacingLeft), ("steady_right", WaverSegment.SteadyFacingRight),
                ("spin_left", WaverSegment.SpinningFacingLeft), ("spin_right", WaverSegment.SpinningFacingRight))
            .Write();

        // A Zoa's animations say how fast it goes from then on.
        Enemy("enemies/zoa.json", "Zoa", EnemyDefinitionId.Zoa).BodyColliders(Ordinary)
            .Looks(ZoaAnimations.Segments, ZoaAnimations.StartingWith, static (ZoaCue cue) => "speed:" + EngineNames.InCue(cue.LaunchSpeed),
                ("shooting_left", ZoaSegment.FacingLeftShooting), ("shooting_right", ZoaSegment.FacingRightShooting),
                ("rising_left", ZoaSegment.FacingLeftRising), ("rising_right", ZoaSegment.FacingRightRising))
            .Write();

        // An Owtch's walk says which way it walks.
        Enemy("enemies/owtch.json", "Owtch", EnemyDefinitionId.Owtch).BodyColliders("Hurtbox")
            .Looks(OwtchAnimations.Segments, OwtchAnimations.StartingWith, static (OwtchCue cue) => "move:" + EngineNames.Side(cue.Direction),
                ("moving_left", OwtchSegment.MovingLeft), ("moving_right", OwtchSegment.MovingRight))
            .Write();

        // The divers: the Skree and the Metaree, alike but for their pictures.
        foreach ((string file, EnemyDefinitionId id, SkreeMetareeSegment idle, SkreeMetareeSegment windup, SkreeMetareeSegment dive, SkreeMetareeParticleSegment debris) in new[]
                 {
                     ("skree", EnemyDefinitionId.Skree, SkreeMetareeSegment.SkreeIdling, SkreeMetareeSegment.SkreePreparingAttack, SkreeMetareeSegment.SkreeDiving,
                         SkreeMetareeParticleSegment.Skree),
                     ("metaree", EnemyDefinitionId.Metaree, SkreeMetareeSegment.MetareeIdling, SkreeMetareeSegment.MetareePreparingAttack, SkreeMetareeSegment.MetareeDiving,
                         SkreeMetareeParticleSegment.Metaree),
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
