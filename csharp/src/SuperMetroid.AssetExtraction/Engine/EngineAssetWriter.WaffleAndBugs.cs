using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The Nuclear Waffle, and the bugs a Kago lets out.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>
    /// Writes the Nuclear Waffle: its head, the links that burn, which the old game made of projectiles, and the links
    /// between them, which it made of lesser effects; its sweep, and what it throws up as it breaks the lava.
    /// </summary>
    private void WriteNuclearWaffle()
    {
        NuclearWaffleSweepDefinition forward = NuclearWaffleDefinitions.ForwardSweep, backward = NuclearWaffleDefinitions.BackwardSweep;
        // The Godot waffle is told the middle of its sweep and how far it reaches to either side; the sweep backward
        // is the sweep forward the other way round.
        static int Steps(NuclearWaffleSweepAngle angle, string what) => angle.FractionsOfStep % RotationSteps == 0
            ? angle.FractionsOfStep / RotationSteps
            : throw new InvalidDataException($"The Nuclear Waffle's {what} is not a whole number of angle steps.");
        int start = Steps(forward.StartAngle, "start"), end = Steps(forward.EndAngle, "end");
        int rises = Steps(forward.RisingTurnThreshold, "rising turn"), falls = Steps(forward.FallingTurnThreshold, "falling turn");
        if ((start + end) % 2 != 0 || start + end != rises + falls)
            throw new InvalidDataException("The Nuclear Waffle's sweep and the lava's surface do not share a middle.");
        if (backward.StartAngle != forward.EndAngle || backward.EndAngle != forward.StartAngle || backward.RisingTurnThreshold != forward.FallingTurnThreshold
            || backward.FallingTurnThreshold != forward.RisingTurnThreshold || backward.SegmentSpacing != forward.SegmentSpacing
            || backward.InterleavedSegmentOffset != forward.InterleavedSegmentOffset)
            throw new InvalidDataException("The Nuclear Waffle's sweep backward is not its sweep forward the other way round.");
        if (forward.InterleavedSegmentOffset * 2 != forward.SegmentSpacing)
            throw new InvalidDataException("The Nuclear Waffle's burning links do not lie half a spacing from the links between them.");
        if (NuclearWaffleDefinitions.ProjectileLinks != 4 || NuclearWaffleDefinitions.SpriteLinks != 3)
            throw new InvalidDataException("The Nuclear Waffle has other than four links that burn and three between them, which its Godot scene has.");

        EnemyProjectileDefinition link = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.NuclearWaffleBody];
        if (link.Radius.X != link.Radius.Y)
            throw new InvalidDataException("A link of the Nuclear Waffle is not as wide as it is tall, which the Godot waffle's one size cannot say.");
        EngineEnemy waffle = Enemy("enemies/nuclear_waffle.json", "NuclearWaffle", EnemyDefinitionId.NuclearWaffle).BodyColliders("Head");
        waffle
            .Value("sweep_middle", (start + end) / 2)
            .Value("sweep_reach", (end - start) / 2)
            .Value("surface_reach", (falls - rises) / 2)
            .Value("link_spacing", forward.SegmentSpacing)
            .Value("link_half_size", link.Radius.X)
            .Value("link_damage", link.Damage)
            .Value("splash_effect", waffle.SpawnedEffect("splash", RoomEffectActorKind.NuclearWaffleSplash))
            .Value("left_burst_effect", waffle.SpawnedEffect("left_burst", RoomEffectActorKind.NuclearWaffleLeftExplosion))
            .Value("right_burst_effect", waffle.SpawnedEffect("right_burst", RoomEffectActorKind.NuclearWaffleRightExplosion))
            .Value("surface_sound", EngineNames.Sound(NuclearWaffleDefinitions.TurnSoundEffect))
            .Looks(NuclearWaffleAnimations.Segments, NuclearWaffleAnimations.StartingWith, ("head", NuclearWaffleSegment.BodyLoop))
            .SpawnedLook("link", NuclearWaffleProjectileAnimations.Segments, NuclearWaffleProjectileAnimations.StartingAt[NuclearWaffleProjectileSegment.Churning],
                EngineEnemy.EndsOnly)
            .SpawnedLook("trail", RoomEffectActorKind.NuclearWaffleBody)
            .Write();
    }

    /// <summary>How many parts of an angle step the old game's sweeps count in.</summary>
    private const int RotationSteps = 256;

    /// <summary>
    /// Gives a Kago the bug it lets out, which is one of its shots as the old game's is: it hurts as the old
    /// projectile does, any shot of Samus's that meets it shoots it down into a burst where it is, which still hurts
    /// to the touch, and as the burst ends it leaves what a Kago leaves. Writes the bug's own data file too.
    /// </summary>
    private EngineEnemy WithKagoBug(EngineEnemy kago) => kago.Shot("bug", RoomEnemyProjectileKind.KagoBug, (_, shot, projectile) =>
    {
        EnemyProjectileDefinition old = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.KagoBug];
        if (old.Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact))
            throw new InvalidDataException("The Kago's bug outlives touching Samus, which the Godot bug does not.");
        if (KagoDefinitions.JumpHeightStepPixels != 1)
            throw new InvalidDataException("The Kago's bug's jumps differ by other than a pixel an update, which the Godot bug's do.");

        Func<KagoBugProjectileCue, string?> cue = static raised => raised.Kind switch
        {
            KagoBugProjectileCueKind.StartJump => "jump",
            KagoBugProjectileCueKind.StartIdle => "wait",
            // Shot down, the Godot bug leaves its drop as its burst ends and is gone with it: its settings say what
            // it leaves, and a timeline ends by ending.
            KagoBugProjectileCueKind.SpawnDrop or KagoBugProjectileCueKind.Delete => null,
            _ => throw new InvalidDataException($"The Kago's bug raises {raised.Kind}, which this does not know."),
        };

        string Look(string name, KagoBugProjectileSegment segment)
        {
            (List<IAnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>> lead, List<IAnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>>? cycle) =
                EngineTimelines.Played(KagoBugProjectileAnimations.Segments, KagoBugProjectileAnimations.StartingAt[segment]);
            return kago.SpawnedTimeline($"bug_{name}", lead, cycle, cue);
        }

        // Its end: the burst a shot makes of it where it is, which leaves the drop and is gone at once after it.
        (List<IAnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>> burst, List<IAnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>>? burstCycle) =
            EngineTimelines.Played(KagoBugProjectileAnimations.Segments, KagoBugProjectileAnimations.StartingAt[KagoBugProjectileSegment.Shot]);
        if (burstCycle is not null)
            throw new InvalidDataException("The burst of a Kago's bug never ends.");
        if (burst.Count < 2
            || burst[^2] is not AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>.Event { Raised.Kind: KagoBugProjectileCueKind.SpawnDrop }
            || burst[^1] is not AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>.Event { Raised.Kind: KagoBugProjectileCueKind.Delete })
            throw new InvalidDataException("The burst of a Kago's bug does not end by leaving its drop and then taking itself away.");

        shot["scene"] = "res://game/enemies/kago/kago_bug.tscn";
        projectile["timeline"] = Look("waiting", KagoBugProjectileSegment.Waiting);
        // Her shots meet it only once it is clear of its hive, which its own class says; met, it is shot down.
        projectile["met_by_shot"] = "destroyed";
        projectile["shot_timeline"] = kago.SpawnedTimeline("bug_shot", burst, null, cue);
        projectile["picture_offset"] = EngineEnemy.NoOffset();
        // Its own class moves it against the blocks.
        projectile["meeting_terrain"] = "passes_through";
        if (Drops(kago.Definition.Drops) is { } drops)
            projectile["drops"] = drops;

        var data = new JsonObject();
        EngineDefaults.Fill(data, "KagoBug");
        void Set(string name, JsonNode? value)
        {
            if (!data.ContainsKey(name))
                throw new InvalidDataException($"The Kago's bug is given the value {name}, which its type does not read by that name.");
            data[name] = value;
        }

        Set("jump_speed", KagoDefinitions.BaseJumpSpeedPixels);
        Set("across_speed", EngineNames.Number(KagoDefinitions.BugHorizontalSpeed));
        Set("gravity", EngineNames.Number(KagoDefinitions.BugGravity));
        Set("fall_start_speed", EngineNames.Number(KagoDefinitions.BugFallStartSpeed));
        Set("shortest_wait", KagoDefinitions.ShortestGroundWait);
        Set("homing_distance", KagoDefinitions.BugHomingDistance);
        Set("clear_distance", KagoDefinitions.BugBlocksShotsDistance);
        Set("sound", EngineNames.Sound(EnemySoundEffectDefinitions.KagoBugSoundEffect));
        Set("first_sound_after", KagoDefinitions.FirstSoundPastFirstHop);
        Set("jump_start_timeline", Look("jump_start", KagoBugProjectileSegment.JumpStart));
        Set("falling_timeline", Look("falling", KagoBugProjectileSegment.Falling));
        Set("landed_timeline", Look("landed", KagoBugProjectileSegment.Landed));
        _tree.WriteData("enemies/kago_bug.json", data);
    });
}
