using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The enemies whose old cues carry something with them: a way to throw, a place to reach.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>
    /// What a Cacatac's spike is: it flies straight through everything and is gone once out of view, and it is shown
    /// by a picture for the way it was thrown. One thrown to a side is shown as <paramref name="left"/> or
    /// <paramref name="right"/>, which differ by whether the Cacatac is on the floor or the ceiling.
    /// </summary>
    private static void CacatacSpike(EngineEnemy cacatac, System.Text.Json.Nodes.JsonObject projectile, string name, CacatacSpikeDirection left, CacatacSpikeDirection right)
    {
        (string Direction, CacatacSpikeDirection Spike)[] thrown =
        [
            ("left", left), ("right", right), ("up_left", CacatacSpikeDirection.UpLeft), ("up", CacatacSpikeDirection.Up),
            ("up_right", CacatacSpikeDirection.UpRight), ("down_left", CacatacSpikeDirection.DownLeft), ("down", CacatacSpikeDirection.Down),
            ("down_right", CacatacSpikeDirection.DownRight),
        ];
        foreach ((string direction, CacatacSpikeDirection spike) in thrown)
            cacatac.SpawnedTimeline($"{name}_{direction}", CacatacProjectileAnimations.Segments, CacatacProjectileAnimations.StartingAt[spike], EngineEnemy.EndsOnly);
        projectile["timeline"] = $"{cacatac.Folder}/{name}_{{direction}}.json";
        projectile["picture_offset"] = EngineEnemy.NoOffset();
        projectile["meeting_terrain"] = "passes_through";
        EngineEnemy.GoneOutOfView(projectile);
    }

    /// <summary>
    /// What a Fune's or a Namihe's fireball is: it flies level through everything and is gone once out of view, shown
    /// by its pictures for the side it flies to.
    /// </summary>
    private static void FuneFireball(EngineEnemy fune, System.Text.Json.Nodes.JsonObject projectile)
    {
        fune.SpawnedTimeline("fireball_left", FuneNamiheFireballAnimations.Segments, FuneNamiheFireballAnimations.StartingAt[FuneNamiheFireballSegment.Left], EngineEnemy.EndsOnly);
        fune.SpawnedTimeline("fireball_right", FuneNamiheFireballAnimations.Segments, FuneNamiheFireballAnimations.StartingAt[FuneNamiheFireballSegment.Right], EngineEnemy.EndsOnly);
        projectile["timeline"] = $"{fune.Folder}/fireball_{{side}}.json";
        projectile["picture_offset"] = EngineEnemy.NoOffset();
        projectile["meeting_terrain"] = "passes_through";
        EngineEnemy.GoneOutOfView(projectile);
    }

    /// <summary>Writes the enemies that spit, throw and erupt by their animations.</summary>
    private void WriteSpittingEnemies()
    {
        // The Alcoon: its leap out looks up, and coming down it looks ahead. Its fire is thrown up, level or down.
        Enemy("enemies/alcoon.json", "Alcoon", EnemyDefinitionId.Alcoon).BodyColliders(Ordinary)
            .Value("emerge_across_distance", AlcoonMotionDefinitions.EmergeXDistance)
            .Value("emerge_down_distance", AlcoonMotionDefinitions.EmergeYDistance)
            .Value("hide_distance", AlcoonMotionDefinitions.HideXDistance)
            .Value("emerge_jump_speed", EngineNames.Number(-AlcoonMotionDefinitions.EmergeJumpVelocity))
            .Value("hide_jump_speed", EngineNames.Number(-AlcoonMotionDefinitions.HideJumpVelocity))
            .Value("gravity", EngineNames.Number(AlcoonMotionDefinitions.Gravity))
            .Value("stride", Whole(AlcoonWalkDefinitions.StrideDistance))
            .Value("muzzle_across", AlcoonFireballLaunchDefinitions.MuzzleFacingRight.X)
            .Value("muzzle_up", -AlcoonFireballLaunchDefinitions.MuzzleFacingRight.Y)
            .Value("emerge_sound", EngineNames.Sound(EnemySoundEffectDefinitions.AlcoonEmergeSound))
            // Each spit is heard whether or not the room has a place for its fireball.
            .Value("fire_sound", EngineNames.Sound(EnemySoundEffectDefinitions.AlcoonFireSound))
            .Shot("fireball", RoomEnemyProjectileKind.AlcoonFireball, static (alcoon, shot, projectile) =>
            {
                shot["speed"] = EngineNames.Number(AlcoonFireballLaunchDefinitions.LaunchSpeed);
                shot["scene"] = "res://game/enemies/alcoon/alcoon_fireball.tscn";
                projectile["timeline"] = alcoon.SpawnedTimeline("fireball", AlcoonFireballAnimations.Segments,
                    AlcoonFireballAnimations.StartingAt[AlcoonFireballSegment.Spinning], EngineEnemy.EndsOnly);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
            })
            .ProjectileData("enemies/alcoon_fireball.json", "AlcoonFireball",
                ("slowing", EngineNames.Number(AlcoonFireballLaunchDefinitions.Deceleration)),
                ("steady_speed", EngineNames.Number(AlcoonFireballLaunchDefinitions.TerminalSpeed)))
            .Looks(AlcoonAnimations.Segments, AlcoonAnimations.StartingWith,
                static (AlcoonCue cue) => cue switch
                {
                    AlcoonCue.Fire fire => fire.Launch.YVelocity.Pixels switch { < 0 => "fire:up", > 0 => "fire:down", _ => "fire:level" },
                    AlcoonCue.Signal { Kind: AlcoonCueKind.Move } => "move",
                    AlcoonCue.Signal { Kind: AlcoonCueKind.StartWalking } => "start_walking",
                    AlcoonCue.Signal { Kind: AlcoonCueKind.CompleteWalkCycle } => "walk_cycle",
                    _ => throw new InvalidDataException($"An Alcoon's animation raises {cue}, which this does not know."),
                },
                ("walking_left", AlcoonSegment.WalkingLeft), ("walking_right", AlcoonSegment.WalkingRight),
                ("fire_left", AlcoonSegment.FireLeft), ("fire_right", AlcoonSegment.FireRight),
                ("leap_left", AlcoonSegment.AirborneLeftLookingUp), ("leap_right", AlcoonSegment.AirborneRightLookingUp),
                ("airborne_left", AlcoonSegment.AirborneLeftLookingForward), ("airborne_right", AlcoonSegment.AirborneRightLookingForward))
            .Write();

        // The Cacatac: each spike of its attack is thrown one way.
        Enemy("enemies/cacatac.json", "Cacatac", EnemyDefinitionId.Cacatac).BodyColliders(Ordinary)
            .Value("attack_chances", CacatacAttackDefinitions.Attacks.TakingValues)
            .Value("spike_speed", EngineNames.Number(CacatacProjectileDefinitions.CardinalSpeed))
            .Value("diagonal_spike_speed", EngineNames.Number(CacatacProjectileDefinitions.DiagonalSpeed))
            .Shot("spike", RoomEnemyProjectileKind.CacatacSpike, static (cacatac, _, projectile) =>
                CacatacSpike(cacatac, projectile, "spike", CacatacSpikeDirection.LeftFacingUp, CacatacSpikeDirection.RightFacingUp))
            .Shot("spike_from_ceiling", RoomEnemyProjectileKind.CacatacSpike, static (cacatac, _, projectile) =>
                CacatacSpike(cacatac, projectile, "spike_from_ceiling", CacatacSpikeDirection.LeftFacingDown, CacatacSpikeDirection.RightFacingDown))
            .Looks(CacatacAnimations.Segments, CacatacAnimations.StartingWith,
                static (CacatacCue cue) => cue switch
                {
                    CacatacCue.SpawnSpike spike => "spike:" + spike.Direction switch
                    {
                        CacatacSpikeDirection.LeftFacingUp or CacatacSpikeDirection.LeftFacingDown => "left",
                        CacatacSpikeDirection.RightFacingUp or CacatacSpikeDirection.RightFacingDown => "right",
                        CacatacSpikeDirection.UpLeft => "up_left",
                        CacatacSpikeDirection.Up => "up",
                        CacatacSpikeDirection.UpRight => "up_right",
                        CacatacSpikeDirection.DownLeft => "down_left",
                        CacatacSpikeDirection.Down => "down",
                        CacatacSpikeDirection.DownRight => "down_right",
                        _ => throw new InvalidDataException($"A Cacatac throws a spike {spike.Direction}, which this does not know."),
                    },
                    CacatacCue.Signal { Kind: CacatacCueKind.PlaySpikeSound } => EngineNames.SoundCue(EnemySoundEffectDefinitions.CacatacSpikeSound),
                    CacatacCue.Signal { Kind: CacatacCueKind.RestorePatrol } => "patrol",
                    _ => throw new InvalidDataException($"A Cacatac's animation raises {cue}, which this does not know."),
                },
                ("idle", CacatacSegment.OnFloorIdle), ("attack", CacatacSegment.OnFloorAttack),
                ("idle_ceiling", CacatacSegment.OnCeilingIdle), ("attack_ceiling", CacatacSegment.OnCeilingAttack))
            .Write();

        // The Fune and the Namihe: one Godot type, each with its own pictures.
        static string? FuneCue(FuneNamiheCue cue) => cue switch
        {
            FuneNamiheCue.Fire => "fire",
            FuneNamiheCue.Signal { Kind: FuneNamiheCueKind.FinishActivity } => "done",
            FuneNamiheCue.Signal { Kind: FuneNamiheCueKind.SpitSound } => EngineNames.SoundCue(FuneNamiheDefinitions.SpitSoundEffect),
            _ => throw new InvalidDataException($"A Fune's animation raises {cue}, which this does not know."),
        };
        // Every room places a Fune and a Namihe solid to Samus: a head in the wall she is stopped by and can stand on.
        Enemy("enemies/fune.json", "Fune", EnemyDefinitionId.Fune).BodyColliders("Body", "Hurtbox", "Solid")
            .Shot("fireball", RoomEnemyProjectileKind.FuneFireball, static (fune, _, projectile) => FuneFireball(fune, projectile))
            .Looks(FuneNamiheAnimations.Segments, FuneNamiheAnimations.StartingWith, FuneCue,
                ("idle_left", FuneNamiheSegment.FuneIdleLeft), ("idle_right", FuneNamiheSegment.FuneIdleRight),
                ("active_left", FuneNamiheSegment.FuneActiveLeft), ("active_right", FuneNamiheSegment.FuneActiveRight))
            .Write();
        Enemy("enemies/namihe.json", "Fune", EnemyDefinitionId.Namihe).BodyColliders("Body", "Hurtbox", "Solid")
            // The Namihe's mouth is lower in its pictures than the Fune's.
            .Value("mouth_drop", 4)
            .Shot("fireball", RoomEnemyProjectileKind.NamiheFireball, static (namihe, _, projectile) => FuneFireball(namihe, projectile))
            .Looks(FuneNamiheAnimations.Segments, FuneNamiheAnimations.StartingWith, FuneCue,
                ("idle_left", FuneNamiheSegment.NamiheIdleLeft), ("idle_right", FuneNamiheSegment.NamiheIdleRight),
                ("active_left", FuneNamiheSegment.NamiheActiveLeft), ("active_right", FuneNamiheSegment.NamiheActiveRight))
            .Write();

        // The Hibashi: each step of its eruption says how high the part of the flame that burns has climbed.
        Enemy("enemies/hibashi.json", "Hibashi", EnemyDefinitionId.Hibashi)
            .Value("first_wait", HibashiDefinitions.FirstWait)
            .Value("half_width", HibashiDefinitions.RestoredHalfWidth)
            .Looks(HibashiAnimations.Segments, HibashiAnimations.StartingWith,
                static (HibashiCue cue) => cue switch
                {
                    HibashiCue.EruptionStep step => $"reach:{step.Step.YOffset}:{step.Step.YRadius}",
                    HibashiCue.Signal { Kind: HibashiCueKind.FinishActivity } => "done",
                    HibashiCue.Signal { Kind: HibashiCueKind.PlayEruptionSound } => EngineNames.SoundCue(HibashiDefinitions.EruptionSoundEffect),
                    _ => throw new InvalidDataException($"A Hibashi's animation raises {cue}, which this does not know."),
                },
                ("flame", HibashiSegment.Flame))
            .Write();

        // The Evir, and the spine ball it holds, which is an enemy of its own.
        Enemy("enemies/evir.json", "Evir", EnemyDefinitionId.Evir).BodyColliders(Ordinary)
            .Value("arms_across", EvirMotionDefinitions.ArmsFacingRight.X)
            .Value("arms_down", EvirMotionDefinitions.ArmsFacingRight.Y)
            .Value("grip_across", EvirMotionDefinitions.IdleSpitFacingRight.X)
            .Value("grip_down", EvirMotionDefinitions.IdleSpitFacingRight.Y)
            .Looks(EvirAnimations.Segments, EvirAnimations.StartingWith,
                static (EvirCue cue) => cue.Kind == EvirCueKind.PlaySpitSound ? EngineNames.SoundCue(EnemySoundEffectDefinitions.EvirSpitSound) : null,
                ("body_left", EvirSegment.BodyFacingLeft), ("body_right", EvirSegment.BodyFacingRight),
                ("arms_left", EvirSegment.ArmsFacingLeft), ("arms_right", EvirSegment.ArmsFacingRight))
            .Write();
        Enemy("enemies/evir_spit.json", "EvirSpit", EnemyDefinitionId.EvirProjectile).BodyColliders(Ordinary)
            .Value("throw_distance", EvirMotionDefinitions.SpitActivationDistance)
            .Value("speed", EvirMotionDefinitions.SpitSpeed)
            .Value("regrow_margin", EvirMotionDefinitions.SpitRegenerationMargin)
            .Value("regrow_distance", EvirMotionDefinitions.RegenerationStartDistance)
            .Looks(EvirAnimations.Segments, EvirAnimations.StartingWith,
                static (EvirCue cue) => cue.Kind switch
                {
                    EvirCueKind.SetInitialRegenerationOffset => "regrow_start",
                    EvirCueKind.AdvanceRegenerationOffset => "regrow_step",
                    EvirCueKind.FinishRegeneration => "regrown",
                    EvirCueKind.PlaySpitSound => EngineNames.SoundCue(EnemySoundEffectDefinitions.EvirSpitSound),
                    _ => throw new InvalidDataException($"An Evir's animation raises {cue.Kind}, which this does not know."),
                },
                ("idle", EvirSegment.ProjectileIdle), ("regrowing", EvirSegment.ProjectileRegenerating))
            .Write();

        // The Kzan's top is what Samus stands on and its underside what hurts; both are sized as the old game sizes it.
        Enemy("enemies/kzan.json", "Kzan", EnemyDefinitionId.KzanTop).BodyColliders("Solid", "Hitbox", "Hurtbox")
            .Value("rise_wait", KzanMotionDefinitions.RiseWait)
            .Value("rise_speed", EngineNames.Number(-KzanMotionDefinitions.RisingStep))
            .Value("fall_quickening", EngineNames.Number(KzanMotionDefinitions.FallAcceleration))
            .Value("fastest_fall", EngineNames.Number(KzanMotionDefinitions.FastestFall))
            .Value("landing_sound", EngineNames.Sound(EnemySoundEffectDefinitions.KzanLandingSoundEffect))
            .Looks(KzanAnimations.Segments, KzanAnimations.StartingWith, ("idle", KzanSegment.Idle))
            .Write();
        Enemy("enemies/polyp.json", "Polyp", EnemyDefinitionId.Polyp)
            .Value("first_launch_mark", EngineNames.Marks(PolypLaunchDefinitions.FirstLaunchPlace))
            .Value("reach", PolypLaunchDefinitions.LaunchReach)
            // The sixteen speeds across a rock may be lobbed with and the eight waits between rocks, each picked by a number drawn.
            .Group("rock_drifts", EngineNames.List(EngineNames.Each(PolypLaunchDefinitions.XVelocity).Select(EngineNames.Number)))
            .Group("cooldowns", EngineNames.List(EngineNames.Each(PolypLaunchDefinitions.Cooldown)))
            .Shot("rock", RoomEnemyProjectileKind.PolypRock, static (polyp, shot, projectile) =>
            {
                shot["scene"] = "res://game/enemies/polyp/polyp_rock.tscn";
                projectile["timeline"] = polyp.SpawnedTimeline("rock", PolypRockAnimations.Segments, PolypRockAnimations.StartingAt[PolypRockSegment.LavaRock], EngineEnemy.EndsOnly);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
                EngineEnemy.GoneOutOfView(projectile);
            })
            .ProjectileData("enemies/polyp_rock.json", "PolypRock",
                ("curve_marks", EngineNames.Marks(PolypLaunchDefinitions.RockFallStep)),
                ("fastest_fall_mark", EngineNames.Marks(PolypLaunchDefinitions.RockFastestFallPlace)))
            // It has no look: the cartridge places every Polyp hidden, and only its rocks are seen (#3709).
            .Write();
    }
}
