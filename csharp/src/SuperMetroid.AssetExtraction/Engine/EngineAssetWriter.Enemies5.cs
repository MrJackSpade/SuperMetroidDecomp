using System.Text.Json.Nodes;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Rios;
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
        Enemy("enemies/holtz.json", "Swooper", EnemyDefinitionId.Holtz).BodyColliders(Ordinary)
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
    }
}
