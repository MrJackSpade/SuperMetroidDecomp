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
    /// Writes the bug a Kago lets out, which the old game made a projectile of the Kago: it has no health to speak of,
    /// hurts as the projectile did, and any shot ends it in a burst that leaves what a Kago leaves.
    /// </summary>
    private void WriteKagoBug()
    {
        const string file = "enemies/kago_bug.json";
        string folder = file[..file.LastIndexOf('.')];
        string spawner = EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.Kago);
        EnemyProjectileDefinition old = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.KagoBug];
        if (old.Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact))
            throw new InvalidDataException("The Kago's bug outlives touching Samus, which the Godot bug does not.");
        if (KagoDefinitions.JumpHeightStepPixels != 1)
            throw new InvalidDataException("The Kago's bug's jumps differ by other than a pixel an update, which the Godot bug's do.");

        static string? Cue(KagoBugProjectileCue cue) => cue.Kind switch
        {
            KagoBugProjectileCueKind.StartJump => "jump",
            KagoBugProjectileCueKind.StartIdle => "wait",
            KagoBugProjectileCueKind.SpawnDrop => "drop",
            // It ends by taking itself away, which a timeline does by ending.
            KagoBugProjectileCueKind.Delete => null,
            _ => throw new InvalidDataException($"The Kago's bug raises {cue.Kind}, which this does not know."),
        };

        (string Name, KagoBugProjectileSegment Segment)[] looks =
        [
            ("waiting", KagoBugProjectileSegment.Waiting), ("jump_start", KagoBugProjectileSegment.JumpStart),
            ("falling", KagoBugProjectileSegment.Falling), ("landed", KagoBugProjectileSegment.Landed),
        ];
        var played = looks.Select(look => (look.Name, Beats: EngineTimelines.Played(KagoBugProjectileAnimations.Segments, KagoBugProjectileAnimations.StartingAt[look.Segment]))).ToList();
        EngineSheetPictures pictures = _sheets.CutSpawned(_tree, folder, spawner, played
            .SelectMany(look => look.Beats.Lead.Concat(look.Beats.Cycle ?? []))
            .Select(beat => beat.ShownFrame).Where(frame => frame.HasValue).Select(frame => EnemySheetDefinitions.ProjectileFrameName(frame!.Value)));

        // Its end: the burst a shot makes of it, which leaves the drop.
        (List<IAnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>> burst, List<IAnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>>? burstCycle) =
            EngineTimelines.Played(KagoBugProjectileAnimations.Segments, KagoBugProjectileAnimations.StartingAt[KagoBugProjectileSegment.Shot]);
        if (burstCycle is not null)
            throw new InvalidDataException("The burst of a Kago's bug never ends.");
        string death = $"{folder}/shot.json";
        EngineSheetPictures burstPictures = _sheets.CutSpawned(_tree, death[..death.LastIndexOf('.')], spawner,
            burst.Select(beat => beat.ShownFrame).Where(frame => frame.HasValue).Select(frame => EnemySheetDefinitions.ProjectileFrameName(frame!.Value)));
        JsonObject deathTimeline = EngineTimelines.Of(burst, null, frame => burstPictures.Of(EnemySheetDefinitions.ProjectileFrameName(frame)), Cue);
        deathTimeline["offset"] = new JsonObject { ["x"] = burstPictures.OffsetX, ["y"] = burstPictures.OffsetY };
        _tree.WriteData(death, deathTimeline);

        // Samus's own body does a projectile no harm; everything she fires ends it.
        var reactions = new JsonArray();
        foreach (string kind in new[] { "screw_attack", "speed_boost", "shinespark", "charged_spin", "grapple" })
            reactions.Add(new JsonObject { ["kinds"] = new JsonArray(kind), ["damage"] = "none" });

        // The old bug is a shot drawn behind Samus, and so behind every enemy that stands behind her, the Kago it
        // comes out of among them.
        if (old.Depth != EnemyProjectileDepth.BehindSamus)
            throw new InvalidDataException("The Kago's bug is not drawn behind Samus, which the Godot bug's standing says it is.");
        EngineObjectFile bug = new EngineObjectFile(_tree, file, "KagoBug").PictureOffset(pictures)
            .Group("standing", "rearmost")
            .Value("health", 1)
            .Value("contact_damage", old.Damage)
            .Value("jump_speed", KagoDefinitions.BaseJumpSpeedPixels)
            .Value("across_speed", EngineNames.Number(KagoDefinitions.BugHorizontalSpeed))
            .Value("gravity", EngineNames.Number(KagoDefinitions.BugGravity))
            .Value("fall_start_speed", EngineNames.Number(KagoDefinitions.BugFallStartSpeed))
            .Value("shortest_wait", KagoDefinitions.ShortestGroundWait)
            .Value("homing_distance", KagoDefinitions.BugHomingDistance)
            .Value("clear_distance", KagoDefinitions.BugBlocksShotsDistance)
            .Value("sound", EngineNames.Sound(EnemySoundEffectDefinitions.KagoBugSoundEffect))
            .Value("first_sound_after", KagoDefinitions.FirstSoundPastFirstHop)
            .Group("reactions", reactions)
            .Group("death", new JsonObject { ["effect"] = death })
            .Collider("Body", -old.Radius.X, -old.Radius.Y, old.Radius.X * 2, old.Radius.Y * 2)
            .Collider("Hurtbox", -old.Radius.X, -old.Radius.Y, old.Radius.X * 2, old.Radius.Y * 2);
        if (Drops(RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.Kago].Drops) is { } drops)
            bug.Group("drops", drops);
        foreach ((string name, (List<IAnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>> lead, List<IAnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>>? cycle)) in played)
            bug.Timeline(name, EngineTimelines.Of(lead, cycle, frame => pictures.Of(EnemySheetDefinitions.ProjectileFrameName(frame)), Cue));
        bug.Write();
    }
}
