using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The Magdollite: the head in the lava, the pillar it raises and the hand that throws.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The two ways a Magdollite's lava flies, as the Godot Magdollite names them, with the old lava's animation each way.</summary>
    private static readonly (string Side, MagdolliteLavaSegment Segment)[] MagdolliteLavaSides =
        [("left", MagdolliteLavaSegment.Left), ("right", MagdolliteLavaSegment.Right)];

    /// <summary>Writes the Magdollite: its head's, pillar's and hand's timelines, its glow and the lava it throws.</summary>
    private void WriteMagdollite()
    {
        static string? HeadCue(MagdolliteHeadCue cue) => cue switch
        {
            MagdolliteHeadCue.Move { Offset.X: 0 } move => $"move:{move.Offset.Y}",
            MagdolliteHeadCue.MoveWithPillar { Offset.X: 0 } move => $"pillar:{move.Offset.Y}",
            MagdolliteHeadCue.Signal { Kind: MagdolliteHeadCueKind.SinkAndShowParts } => "sink_and_show",
            MagdolliteHeadCue.Signal { Kind: MagdolliteHeadCueKind.FinishSubmerge } => "submerged",
            MagdolliteHeadCue.Signal { Kind: MagdolliteHeadCueKind.RestoreOrigins } => "restore",
            MagdolliteHeadCue.Signal { Kind: MagdolliteHeadCueKind.SurfaceAndHideParts } => "surface_and_hide",
            MagdolliteHeadCue.Signal { Kind: MagdolliteHeadCueKind.FinishEmerge } => "surfaced",
            _ => throw new InvalidDataException($"The Magdollite's head raises {cue}, which this does not know."),
        };

        static string? HandCue(MagdolliteOverlayCue cue) => cue switch
        {
            // The one sound its hand asks for is the sound of its throw, which its data names.
            MagdolliteOverlayCue.PlaySoundIfOnScreen sound when sound.Sound == MagdolliteSoundDefinitions.Throw => "throw_sound",
            MagdolliteOverlayCue.ShiftFromThrowOrigin shift => $"shift:{shift.Offset.X}:{shift.Offset.Y}",
            MagdolliteOverlayCue.Signal { Kind: MagdolliteOverlayCueKind.StartCooldown } => "cooldown",
            MagdolliteOverlayCue.Signal { Kind: MagdolliteOverlayCueKind.SpawnLava } => "lava",
            MagdolliteOverlayCue.Signal { Kind: MagdolliteOverlayCueKind.FinishThrow } => "thrown",
            _ => throw new InvalidDataException($"The Magdollite's hand raises {cue}, which this does not know."),
        };

        // The Godot Magdollite works its pillar by one length: a length more for every two lengths risen, and its hand
        // half a length above the top. The old table must say the same of every stage.
        const int length = MagdollitePhaseDefinitions.BodyShiftPerPhase;
        var pillars = new List<MagdolliteBodySegment>();
        for (MagdolliteRiseStage stage = MagdolliteRiseStage.OneSegment; ; stage = stage.Higher)
        {
            MagdollitePhaseDefinition phase = stage.Phase;
            int lengths = pillars.Count + 1;
            if (phase.DistanceThreshold != lengths * length * 2 || phase.OverlayYOffset != length / 2 + length * lengths)
                throw new InvalidDataException($"The Magdollite's pillar of {lengths} lengths is not measured by its length as the Godot Magdollite measures it.");
            pillars.Add(phase.BodyAnimation);
            if (stage == Enum.GetValues<MagdolliteRiseStage>().Max())
                break;
        }

        MagdolliteRiseStage setup = MagdolliteRiseStage.Setup;
        if (setup.Phase.BodyAnimation != pillars[0] || setup.Phase.OverlayYOffset != MagdolliteRiseStage.OneSegment.Phase.OverlayYOffset)
            throw new InvalidDataException("The Magdollite's pillar does not wait as its shortest length.");

        MagdolliteApexGrowth growth = _installation.LoadEnemyOverreads().MagdolliteApexGrowth;
        EngineEnemy magdollite = Enemy("enemies/magdollite.json", "Magdollite", EnemyDefinitionId.Magdollite).BodyColliders("Hurtbox")
            .Value("hidden_depth", MagdolliteMotionDefinitions.HiddenDepth)
            .Value("sunk_depth", MagdolliteMotionDefinitions.SunkDepth)
            .Value("surfaced_depth", MagdolliteMotionDefinitions.SurfacedDepth)
            .Value("maximum_rise", MagdollitePhaseDefinitions.MaximumRise)
            .Value("pillar_length", length)
            .Value("most_lengths", pillars.Count)
            .Value("attack_cooldown", MagdolliteMotionDefinitions.AttackCooldown)
            .Value("attack_poll_updates", MagdolliteMotionDefinitions.AttackPollWait)
            .Value("smallest_reach", MagdolliteMotionDefinitions.SmallestHeadRadius)
            .Value("reach_past_hand", MagdolliteMotionDefinitions.HeadReachPastOverlay)
            .Value("lava_drop", MagdolliteMotionDefinitions.LavaDrop)
            .Value("glow_updates", MagdolliteMotionDefinitions.GlowFrameDuration)
            .Value("throw_sound", EngineNames.Sound(MagdolliteSoundDefinitions.Throw))
            // Whether a pillar at its full height grows a last length, by where the old game lists it among a room's enemies.
            .Group("grows_at_full_height", new JsonArray([.. Enumerable.Range(0, EnemyPlaceOrder.Capacity).Select(place => (JsonNode)JsonValue.Create(growth.GrowsFinalSegment(place)))]))
            .Looks(MagdolliteHeadAnimations.Segments, segment => MagdolliteHeadAnimations.StartingAt[segment], HeadCue,
                ("idle_left", MagdolliteHeadSegment.LeftIdle), ("idle_right", MagdolliteHeadSegment.RightIdle),
                ("submerge_left", MagdolliteHeadSegment.LeftSubmerge), ("submerge_right", MagdolliteHeadSegment.RightSubmerge),
                ("emerge_left", MagdolliteHeadSegment.LeftEmerge), ("emerge_right", MagdolliteHeadSegment.RightEmerge))
            .Looks(MagdolliteOverlayAnimations.Segments, segment => MagdolliteOverlayAnimations.StartingAt[segment], HandCue,
                ("cap", MagdolliteOverlaySegment.PillarCap), ("throw_left", MagdolliteOverlaySegment.LeftThrow),
                ("throw_right", MagdolliteOverlaySegment.RightThrow));
        for (int lengths = 1; lengths <= pillars.Count; lengths++)
            magdollite.Timeline($"pillar_{lengths}", MagdolliteBodyAnimations.Segments, MagdolliteBodyAnimations.StartingAt[pillars[lengths - 1]]);

        // Its glow: each step a change of the colours of all of it, and of the lava it has thrown, which the old game
        // draws in its thrower's colours. The lava's pictures lie in folders of their own under its own.
        const string family = Installation.GameInstallationLayout.EnemySheetDirectoryName;
        string spawner = EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.Magdollite);
        var bodyPlaces = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [magdollite.Folder] = [_sheets.EnemySheet(spawner)],
        };
        var lavaPlaces = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach ((string side, MagdolliteLavaSegment segment) in MagdolliteLavaSides)
        {
            EnemyProjectileFrameId frame = MagdolliteLavaAnimations.Segments.Beats(segment).Select(beat => beat.ShownFrame).Single(shown => shown.HasValue)!.Value;
            lavaPlaces[$"{magdollite.Folder}/lava_{side}"] = [_sheets.SheetHolding(spawner, EnemySheetDefinitions.ProjectileFrameName(frame))];
        }

        magdollite
            .Group("glow", EngineNames.List(MagdolliteGlowAnimations.Cycle.Select(step =>
                (JsonNode)ShadeChange(MagdolliteShades.Glow(step).Key, family, bodyPlaces))))
            .Group("lava_glow", EngineNames.List(MagdolliteGlowAnimations.Cycle.Select(step =>
                (JsonNode)ShadeChange(MagdolliteShades.Glow(step).Key, family, lavaPlaces))));

        magdollite
            // Its lava flies level through everything until it is out of view; a shot destroys it, and it leaves what a Magdollite leaves.
            .Shot("lava", RoomEnemyProjectileKind.LavaThrownByMagdollite, static (thrower, shot, projectile) =>
            {
                shot["speed"] = EngineNames.Number(MagdolliteMotionDefinitions.LavaThrowSpeed);
                // Shot, it holds still as it was, and on its next update leaves its drop and is gone: the old shot
                // animation shows no picture of its own and raises its drop and its end on its first update. That is
                // what something shot down with no end of its own to play does, so it is given none.
                (List<IAnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>> shotBeats, List<IAnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>>? shotCycle) =
                    EngineTimelines.Played(MagdolliteLavaAnimations.Segments, MagdolliteLavaAnimations.StartingAt[MagdolliteLavaSegment.Shot]);
                if (shotCycle is not null || shotBeats.Any(beat => beat.ShownFrame.HasValue)
                    || !shotBeats.Any(beat => beat is AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>.Event { Raised.Kind: MagdolliteLavaCueKind.SpawnDrops }))
                    throw new InvalidDataException("A Magdollite's shot lava shows a picture of its own, never ends or leaves no drop, where the Godot lava shows as it was for the one update before it leaves its drop and is gone.");
                foreach ((string side, MagdolliteLavaSegment segment) in MagdolliteLavaSides)
                {
                    EnemyProjectileFrameId frame = MagdolliteLavaAnimations.Segments.Beats(segment).Select(beat => beat.ShownFrame).Single(shown => shown.HasValue)!.Value;
                    _ = thrower.SpawnedPicture($"lava_{side}", frame);
                }

                projectile["timeline"] = $"{thrower.Folder}/lava_{{side}}.json";
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
                EngineEnemy.GoneOutOfView(projectile);
                if (Drops(thrower.Definition.Drops) is { } drops)
                    projectile["drops"] = drops;
            })
            .Write();
    }
}
