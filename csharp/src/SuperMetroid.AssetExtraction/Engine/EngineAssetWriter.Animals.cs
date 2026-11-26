using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Spatial;
using SoundRequestPriority = SuperMetroid.Core.Compatibility.SoundRequestPriority;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The animals: the Dachora and the Etecoons that teach Samus what they can do, and the ones she finds trapped on
/// her way out. They are harmless creatures of the Godot game, shown by timelines made of the old animations.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The data file of a creature that was one of the old game's enemies: its pictures, where they lie, and its body.</summary>
    private EngineObjectFile Creature(string file, string type, EnemyDefinitionId id, out EngineSheetPictures pictures)
    {
        pictures = EnemyPictures(file[..file.LastIndexOf('.')], EnemyDefinitionIdKeys.Installed.Of(id));
        Extent16<RoomPixels> radius = RoomEnemyDefinitionCatalog.ById[id].Radius;
        return new EngineObjectFile(_tree, file, type)
            .PictureOffset(pictures)
            .Collider("Body", -radius.X, -radius.Y, 2 * radius.X, 2 * radius.Y);
    }

    /// <summary>Writes the animals.</summary>
    private void WriteAnimals()
    {
        WriteDachora();
        WriteEtecoon();
        WriteEscapeDachora();
        WriteEscapeEtecoon();
    }

    private void WriteDachora()
    {
        EngineObjectFile dachora = Creature("objects/dachora.json", "Dachora", EnemyDefinitionId.Dachora, out EngineSheetPictures pictures);
        JsonObject Look(DachoraSegment segment) => EngineTimelines.Of(
            DachoraAnimations.Segments, DachoraAnimations.StartingWith(segment), frame => PictureOf(pictures, frame), static (NoAnimationCue _) => null);

        dachora
            .Timeline("idle_left", Look(DachoraSegment.IdleLeft))
            .Timeline("idle_right", Look(DachoraSegment.IdleRight))
            .Timeline("blink_left", Look(DachoraSegment.BlinkLeft))
            .Timeline("blink_right", Look(DachoraSegment.BlinkRight))
            .Timeline("running_left", Look(DachoraSegment.RunningLeft))
            .Timeline("running_right", Look(DachoraSegment.RunningRight))
            .Timeline("running_fast_left", Look(DachoraSegment.RunningLeftFast))
            .Timeline("running_fast_right", Look(DachoraSegment.RunningRightFast))
            .Timeline("running_very_fast_left", Look(DachoraSegment.RunningLeftVeryFast))
            .Timeline("running_very_fast_right", Look(DachoraSegment.RunningRightVeryFast))
            // It only ever crouches to shoot up at the end of a run to the right, and comes down facing left.
            .Timeline("charge_right", Look(DachoraSegment.ChargeRight))
            .Timeline("shinespark_left", Look(DachoraSegment.EchoLeft))
            .Timeline("shinespark_right", Look(DachoraSegment.EchoRight))
            .Timeline("falling_left", Look(DachoraSegment.FallingLeft))
            .Timeline("falling_right", Look(DachoraSegment.FallingRight));

        // The echoes it leaves behind as it shoots up: its own stretched picture, there for a moment.
        const string echo = "objects/dachora_echo.json";
        _tree.WriteData(echo, new JsonObject
        {
            ["steps"] = new JsonArray(new JsonObject
            {
                ["picture"] = PictureOf(pictures, EnemyFrameId.DachoraStretchedUpFacingRight),
                ["updates"] = DachoraTimingDefinitions.EchoLifetime,
            }),
            ["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY },
        });

        // Its glows: each frame of its speed's and its shine's colours, as changes of the colours of its pictures.
        string sheet = _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.Dachora));
        JsonArray Glow(DachoraColourPhase phase)
        {
            var frames = new JsonArray();
            for (int frame = 0; frame < DachoraColorDefinitions.AnimatedFrameCount; frame++)
                frames.Add(EnemySheetSwaps(sheet, DachoraShades.Of(phase, frame).Key));
            return frames;
        }

        dachora
            .Group("glow_colours", new JsonObject { ["speed"] = Glow(DachoraColourPhase.Speed), ["shine"] = Glow(DachoraColourPhase.Shine) })
            .Value("echo_effect", echo)
            .Value("activation_sound", EngineNames.SoundOrStop(EnemySoundEffectDefinitions.DachoraActivationSound))
            .Value("speed_sound", EngineNames.SoundOrStop(EnemySoundEffectDefinitions.DachoraSpeedBoosterSound))
            .Value("speed_stop_sound", EngineNames.SoundOrStop(EnemySoundEffectDefinitions.DachoraSpeedBoosterSoundStop))
            .Value("charge_sound", EngineNames.SoundOrStop(EnemySoundEffectDefinitions.DachoraChargeSound))
            .Value("launch_sound", EngineNames.SoundOrStop(EnemySoundEffectDefinitions.DachoraLaunchSound))
            .Value("ceiling_sound", EngineNames.SoundOrStop(EnemySoundEffectDefinitions.DachoraCeilingImpactSound))
            .Write();
    }

    private void WriteEtecoon()
    {
        EngineObjectFile etecoon = Creature("objects/etecoon.json", "Etecoon", EnemyDefinitionId.Etecoon, out EngineSheetPictures pictures);
        string Picture(EnemyFrameId frame) => PictureOf(pictures, frame);
        JsonObject Of(Animation<EtecoonSegment> animation) =>
            EngineTimelines.Of(EtecoonAnimations.Segments, animation, Picture, static (NoAnimationCue _) => null);
        JsonObject Look(EtecoonSegment segment) => Of(EtecoonAnimations.StartingWith(segment));

        // The hop it begins from the top of a lesson's jump: the rest of a hop from where its arms are flung out.
        IReadOnlyList<AnimationBeat<EnemyFrameId, NoAnimationCue>> hop = EtecoonAnimations.Segments.Beats(EtecoonSegment.ContinueHoppingFacingLeft);
        var fromCrest = hop.SkipWhile(beat => beat.ShownFrame != EnemyFrameId.EtecoonHopArmsFlungFacingLeft).ToList<IAnimationBeat<EnemyFrameId, NoAnimationCue>>();
        if (fromCrest.Count == 0)
            throw new InvalidDataException("An Etecoon's hop never shows its arms flung out.");

        etecoon
            .Timeline("starting_crouch", Look(EtecoonSegment.StartingCrouch))
            .Timeline("flexing", Of(EtecoonAnimations.Flexing))
            .Timeline("hopping_left", Look(EtecoonSegment.HoppingFacingLeft))
            .Timeline("hopping_right", Look(EtecoonSegment.HoppingFacingRight))
            .Timeline("continue_hopping_left", Look(EtecoonSegment.ContinueHoppingFacingLeft))
            .Timeline("continue_hopping_right", Look(EtecoonSegment.ContinueHoppingFacingRight))
            .Timeline("hop_from_crest", EngineTimelines.Of(fromCrest, null, Picture, static (NoAnimationCue _) => null))
            .Timeline("look_right_then_run_left", Look(EtecoonSegment.LookRightAtSamusAndRunLeft))
            .Timeline("look_left_then_run_right", Look(EtecoonSegment.LookLeftAtSamusAndRunRight))
            .Timeline("begin_running_left", Look(EtecoonSegment.BeginRunningLeft))
            .Timeline("begin_running_right", Look(EtecoonSegment.BeginRunningRight))
            .Timeline("running_left", Look(EtecoonSegment.RunningLeft))
            .Timeline("running_right", Look(EtecoonSegment.RunningRight))
            .Timeline("jumping_right", Look(EtecoonSegment.JumpingRight))
            // Against the wall it was heading for it faces back the way it will leap.
            .Timeline("clinging_to_right_wall", Look(EtecoonSegment.WallJumpLeftEligible))
            .Timeline("clinging_to_left_wall", Look(EtecoonSegment.WallJumpRightEligible))
            .Timeline("wall_jump_left", Look(EtecoonSegment.WallJumpLeft))
            .Timeline("wall_jump_right", Look(EtecoonSegment.WallJumpRight))
            // Nothing turns its wake away.
            .Value("wake_sound", EngineNames.Sound(EnemySoundEffectDefinitions.EtecoonWakeSound, SoundRequestPriority.Essential))
            .Value("jump_sound", EngineNames.SoundOrStop(EnemySoundEffectDefinitions.EtecoonJumpSound))
            .Value("wall_sound", EngineNames.SoundOrStop(EnemySoundEffectDefinitions.EtecoonWallSound))
            .Write();
    }

    /// <summary>
    /// The trapped Dachora runs laps: so many to the left, as many to the right. When the acid has risen it runs them
    /// faster, going on from the lap it had reached; so each lap of the slow laps ends by naming itself, and there is
    /// a timeline of the fast laps to go on with for every lap it could have reached.
    /// </summary>
    private void WriteEscapeDachora()
    {
        EngineObjectFile dachora = Creature("objects/escape_dachora.json", "EscapeDachora", EnemyDefinitionId.DachoraEscape, out EngineSheetPictures pictures);
        string Picture(EnemyFrameId frame) => PictureOf(pictures, frame);
        static string Cue(EscapeDachoraCue cue) => cue switch
        {
            EscapeDachoraCue.Move move => $"move:{move.Offset}",
            EscapeDachoraCue.LowTideLapEnds lap => $"low_tide_lap_ends:{EngineNames.Side(lap.Lap)}",
            EscapeDachoraCue.LapEnds lap => $"lap_ends:{EngineNames.Side(lap.Lap)}",
            _ => throw new InvalidDataException($"The trapped Dachora's animation raises {cue}, which this does not know."),
        };

        AnimationPhase<EscapeDachoraSegment> lapsLeft = EscapeDachoraAnimations.RunningAroundHighTide.Phases[0];
        AnimationPhase<EscapeDachoraSegment> lapsRight = EscapeDachoraAnimations.RunningAroundHighTide.Phases[1];
        if (EscapeDachoraAnimations.RunningAroundHighTide.Phases.Count != 2 || lapsLeft.Segment != EscapeDachoraSegment.RunningAroundHighTideLeft
            || lapsRight.Segment != EscapeDachoraSegment.RunningAroundHighTideRight || lapsLeft.Plays != lapsRight.Plays)
            throw new InvalidDataException("The trapped Dachora's fast laps are not as many to the left and then as many to the right.");
        int laps = lapsLeft.Plays;

        // Each slow lap's end names which lap of its side it was, counted from one.
        JsonObject lowTide = EngineTimelines.Of(EscapeDachoraAnimations.Segments, EscapeDachoraAnimations.RunningAroundLowTide, Picture, Cue);
        var ended = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (JsonNode? step in lowTide["steps"]!.AsArray())
        {
            if (step!["cues"] is not JsonArray cues)
                continue;
            for (int index = 0; index < cues.Count; index++)
            {
                string cue = cues[index]!.GetValue<string>();
                if (!cue.StartsWith("low_tide_lap_ends:", StringComparison.Ordinal))
                    continue;
                int lap = ended.GetValueOrDefault(cue) % laps + 1;
                ended[cue] = lap;
                cues[index] = $"{cue}_{lap}";
            }
        }

        dachora.Timeline("running_around_low_tide", lowTide);

        (List<IAnimationBeat<EnemyFrameId, EscapeDachoraCue>> _, List<IAnimationBeat<EnemyFrameId, EscapeDachoraCue>>? fastLaps) =
            EngineTimelines.Played(EscapeDachoraAnimations.Segments, EscapeDachoraAnimations.RunningAroundHighTide);
        if (fastLaps is null)
            throw new InvalidDataException("The trapped Dachora's fast laps do not go round and round.");
        int beatsInLap = EscapeDachoraAnimations.Segments.Beats(EscapeDachoraSegment.RunningAroundHighTideLeft).Count;
        if (EscapeDachoraAnimations.Segments.Beats(EscapeDachoraSegment.RunningAroundHighTideRight).Count != beatsInLap || fastLaps.Count != 2 * laps * beatsInLap)
            throw new InvalidDataException("The trapped Dachora's fast laps are not all as long as one another.");
        foreach ((string side, int lapsBefore) in new[] { ("left", 0), ("right", laps) })
        {
            for (int lap = 1; lap <= laps; lap++)
            {
                dachora.Timeline($"high_tide_after_{side}_{lap}_lap", EngineTimelines.Of(
                    fastLaps.Skip((lapsBefore + lap) * beatsInLap), fastLaps, Picture, Cue));
            }
        }

        // Its way out, once the wall is open: after a lap to the left it stops and turns first.
        dachora
            .Timeline("escape_after_left_lap", EngineTimelines.Of(EscapeDachoraAnimations.Segments, EscapeDachoraAnimations.RunningForEscape, Picture, Cue))
            .Timeline("escape_after_right_lap", EngineTimelines.Of(EscapeDachoraAnimations.Segments, EscapeDachoraAnimations.RunningForEscapeAccelerating, Picture, Cue))
            .Write();
    }

    private void WriteEscapeEtecoon()
    {
        EngineObjectFile etecoon = Creature("objects/escape_etecoon.json", "EscapeEtecoon", EnemyDefinitionId.EtecoonEscape, out EngineSheetPictures pictures);
        // It stands solid to Samus, its body's size, as the old one is set up to whatever its room places it with.
        Extent16<RoomPixels> radius = RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.EtecoonEscape].Radius;
        etecoon.Collider("Solid", -radius.X, -radius.Y, 2 * radius.X, 2 * radius.Y);
        static string Cue(EscapeEtecoonCue cue) => cue switch
        {
            EscapeEtecoonCue.LowTideRunStarts run => $"low_tide_run:{EngineNames.Side(run.Running)}",
            EscapeEtecoonCue.Move move => $"move:{move.Offset}",
            EscapeEtecoonCue.Signal { Kind: EscapeEtecoonCueKind.ExpressesGratitude } => "gratitude",
            EscapeEtecoonCue.Signal { Kind: EscapeEtecoonCueKind.Escapes } => "escape",
            _ => throw new InvalidDataException($"A trapped Etecoon's animation raises {cue}, which this does not know."),
        };
        JsonObject Look(EscapeEtecoonSegment segment) => EngineTimelines.Of(
            EscapeEtecoonAnimations.Segments, EscapeEtecoonAnimations.StartingWith(segment), frame => PictureOf(pictures, frame), Cue);

        etecoon
            .Timeline("stationary", Look(EscapeEtecoonSegment.Stationary))
            .Timeline("running_left_low_tide", Look(EscapeEtecoonSegment.RunningLeftLowTide))
            .Timeline("running_right_low_tide", Look(EscapeEtecoonSegment.RunningRightLowTide))
            .Timeline("running_left_high_tide", Look(EscapeEtecoonSegment.RunningLeftHighTide))
            .Timeline("running_right_high_tide", Look(EscapeEtecoonSegment.RunningRightHighTide))
            .Timeline("running_for_escape", Look(EscapeEtecoonSegment.RunningForEscape))
            .Timeline("gratitude_then_escape", Look(EscapeEtecoonSegment.ExpressGratitudeThenEscape))
            .Write();
    }
}
