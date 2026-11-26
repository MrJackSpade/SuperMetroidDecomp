using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Phantoon and its flames.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The scene of one of Phantoon's flames.</summary>
    private const string PhantoonFlameScene = "res://game/bosses/phantoon/phantoon_flame.tscn";

    /// <summary>Writes Phantoon: its body's and parts' timelines, where it can be hit, its path, its rains, its sounds and its flames.</summary>
    private void WritePhantoon()
    {
        const string file = "bosses/phantoon.json";
        BossBody body = CutBossBody("Phantoon", file[..file.LastIndexOf('.')]);
        EngineEnemy phantoon = BossWithBody(file, "Phantoon", EnemyDefinitionId.PhantoonBody, body, EnemyFrameId.PhantoonBody);
        (int X, int Y) origin = body.Anchor(EnemyFrameId.PhantoonBody);

        static string? Cue(PhantoonPartCue cue) => cue switch
        {
            PhantoonPartCue.PlayMaterializationSound => "appear_sound",
            PhantoonPartCue.BeginEyeTracking => "begin_tracking",
            PhantoonPartCue.PickNewPattern => "pick_pattern",
            PhantoonPartCue.SpawnCasualFlame => "flame",
            _ => throw new InvalidDataException($"Phantoon's animation raises {cue}, which this does not know."),
        };

        void Part(string name, PhantoonPartSegment segment) =>
            phantoon.Timeline(name, BossBodyTimeline(body, PhantoonAnimations.Segments, PhantoonAnimations.StartingWith(segment), origin, Cue));

        Part("body", PhantoonPartSegment.Body);
        Part("eye_open", PhantoonPartSegment.EyeOpen);
        Part("eye_close", PhantoonPartSegment.EyeClose);
        Part("eye_close_and_pick", PhantoonPartSegment.EyeCloseAndPickNewPattern);
        Part("eyeball_centred", PhantoonPartSegment.EyeballCentered);
        Part("mouth_idle", PhantoonPartSegment.MouthInitial);
        Part("mouth_spawn_flame", PhantoonPartSegment.MouthSpawnFlame);
        Part("tentacles", PhantoonPartSegment.Tentacles);
        foreach (KeyValuePair<CompassDirection, PhantoonPartSegment> gaze in PhantoonPatternDefinitions.EyeGazeAnimations)
            Part("eye_looking_" + EngineSheets.FileName(gaze.Key.ToString()), gaze.Value);

        // Where it can be touched and shot: its body, its eye and its three tentacles, each from its place. The old
        // probe meets a shot and Samus's touch on different edges of a box, a pixel apart, so each is written twice.
        void Sized(string name, JsonObject rect) => phantoon.Collider(
            name, rect["left"]!.GetValue<int>(), rect["top"]!.GetValue<int>(), rect["width"]!.GetValue<int>(), rect["height"]!.GetValue<int>());
        void Area(string name, EnemyCollisionHitbox box)
        {
            Sized(name, EngineEnemy.ShotRect(box));
            Sized(name == "EyeHurt" ? "EyeTouch" : name + "Touch", EngineEnemy.TouchRect(box));
        }

        Area("Body", PhantoonCollisionDefinitions.Body);
        Area("EyeHurt", PhantoonCollisionDefinitions.Eye);
        Area("LeftTentacle", PhantoonCollisionDefinitions.LeftTentacle);
        Area("RightTentacle", PhantoonCollisionDefinitions.RightTentacle);
        Area("CentreTentacle", PhantoonCollisionDefinitions.CenterTentacle);

        // The point at its middle, which alone takes shots on the update its body turns solid: a box of no size in
        // the old game, which a shot meets as it meets any box's left and top edge.
        Sized("Centre", EngineEnemy.ShotRect(
            PhantoonSolidifyingCentre.Centre.X, PhantoonSolidifyingCentre.Centre.Y, PhantoonSolidifyingCentre.Centre.X, PhantoonSolidifyingCentre.Centre.Y));

        // Its path: a digit a step, as on a number pad, which is how the Godot Phantoon reads it.
        (char Digit, int X, int Y)[] pad = [('1', -1, 1), ('2', 0, 1), ('3', 1, 1), ('4', -1, 0), ('6', 1, 0), ('7', -1, -1), ('8', 0, -1), ('9', 1, -1)];
        if (PhantoonPathDefinitions.Steps.Count != pad.Length
            || pad.Any(key => !PhantoonPathDefinitions.Steps.TryGetValue(key.Digit, out Offset16<RoomPixels> step) || step.X != key.X || step.Y != key.Y))
            throw new InvalidDataException("Phantoon's path is not written a digit a step as on a number pad.");

        // Its rains, in the order a number rolled picks them; the first rain falls from the first or the third.
        IReadOnlyList<PhantoonRainPattern> rains = [.. EngineNames.Each(PhantoonRainPatternDefinitions.LaterRain)];
        if (rains.Count != 8 || rains[0] != PhantoonRainPatternDefinitions.FirstRainFromLeftHalf || rains[2] != PhantoonRainPatternDefinitions.FirstRainFromRightHalf)
            throw new InvalidDataException("Phantoon's rains are not eight, with its first rain's two places first and third, as the Godot Phantoon takes them.");

        const string family = GameInstallationLayout.BossBodySheetDirectoryName;
        string sheet = _sheets.SheetNamed(family, BossBodySheetPrefix + body.Boss);
        // Its pictures of each band of its health, as that band colours them.
        IReadOnlyList<PhantoonHealthBand> bands = PhantoonHealthBandDefinitions.HighestFirst;
        for (int band = 0; band < bands.Count; band++)
        {
            _sheets.CutShade(_tree, family, file[..file.LastIndexOf('.')] + "_health_" + band.ToString(System.Globalization.CultureInfo.InvariantCulture), sheet,
                PhantoonShades.Body(PhantoonBodyLook.Health(bands[band])).Key);
        }

        static JsonObject Field(RandomSampleField field) => new() { ["from"] = field.Shift, ["digits"] = field.Width };

        if (PhantoonSoundDefinitions.StartingFlame != PhantoonSoundDefinitions.CasualFlame)
            throw new InvalidDataException("Phantoon's opening flames and the flames of its mouth sound different, where the Godot Phantoon has one sound for both.");
        phantoon
            .Value("fade_steps", PhantoonColorDefinitions.BodyFadeSteps)
            .Value("round_damage", PhantoonPatternDefinitions.RoundDamageThreshold)
            .Value("death_drops", PhantoonArenaDefinitions.DeathDropCount)
            .Value("big_explosion_effect", DustFile(DustExplosionAnimation.BigExplosion))
            .Value("small_explosion_effect", DustFile(DustExplosionAnimation.SmallExplosion))
            .Value("flame_sound", EngineNames.Sound(PhantoonSoundDefinitions.CasualFlame))
            .Value("shot_cry", EngineNames.Sound(PhantoonSoundDefinitions.ShotCry))
            .Value("dying_cry", EngineNames.Sound(PhantoonSoundDefinitions.DyingCry))
            .Value("rage_sound", EngineNames.Sound(PhantoonRageDefinitions.WaveSound))
            .Value("big_explosion_sound", EngineNames.Sound(PhantoonSoundDefinitions.DeathExplosionSounds[DustExplosionAnimation.BigExplosion]))
            .Value("small_explosion_sound", EngineNames.Sound(PhantoonSoundDefinitions.DeathExplosionSound))
            // His door's changes: the top block of its column is held solid from his turn (PhantoonDoorObjects), and
            // the door is drawn, for the fight or as it was, on the change's first update, as the room's objects take theirs.
            .Group("fight_door", new JsonObject
            {
                ["held"] = FightDoorHeldPatch,
                ["closes"] = new JsonObject { ["steps"] = new JsonArray(new JsonObject { ["reverts"] = FightDoorHeldPatch, ["patch"] = FightDoorPatch }) },
                ["opens"] = new JsonObject { ["steps"] = new JsonArray(new JsonObject { ["reverts"] = FightDoorPatch }) },
            })
            .Group("path", PhantoonPathDefinitions.Directions)
            .Group("rain_patterns", EngineNames.List(rains.Select(rain => (JsonNode)new JsonObject
            {
                ["path_place"] = rain.PathPlace,
                ["x"] = (int)rain.BodyPosition.X,
                ["y"] = (int)rain.BodyPosition.Y,
            })))
            .Group("appear_sounds", new JsonArray([.. PhantoonSoundDefinitions.MaterializationSounds.Select(sound => (JsonNode)EngineNames.SoundOrStop(sound))]))
            // Its colours change as it is worn down: its pictures are kept once for each band of its health.
            .Group("health_bands", EngineNames.List(bands.Select(band => band.MinimumHealth)))
            .PictureSets("{health}")
            .Group("scattered_drops", Drops(phantoon.Definition.Drops) ?? throw new InvalidDataException("Phantoon scatters nothing when it is gone."))

            // The ship's power coming back over its room once it is gone: the room's own pictures at each step of
            // the way to their powered colours, the last of which stays.
            .Group("power_on", EngineNames.List(Enumerable.Range(1, PhantoonColorDefinitions.PowerOnSteps).Select(step => (JsonNode)ChangePartWay(
                ShadeChange(PhantoonShades.Powered.Key, RoomPlacesOf(EnemyDefinitionId.PhantoonBody)),
                new SuperMetroid.Core.Assets.FadeLevel(step, PhantoonColorDefinitions.PowerOnSteps)))))
            .Group("drop_scatter", new JsonObject
            {
                ["x"] = (int)PhantoonArenaDefinitions.DeathDropOrigin.X,
                ["y"] = (int)PhantoonArenaDefinitions.DeathDropOrigin.Y,
                ["across"] = Field(EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter),
                ["down"] = Field(EnemyDropScatterDefinitions.ArenaBandVerticalScatter),
            });
        // It leaves nothing of its own as it goes: what it scatters is all.
        phantoon.Data.Remove("drops");
        phantoon.Data.Remove("death");

        // Its flames. Each is a projectile of a class of its own, which Phantoon tells what to do.
        static string? FlameCue(PhantoonFlameCue cue) => cue switch
        {
            PhantoonFlameCue.Delete => "end",
            // What a flame put out leaves is in its shot.
            PhantoonFlameCue.SpawnDrop => null,
            _ => throw new InvalidDataException($"A flame of Phantoon's raises {cue}, which this does not know."),
        };
        string Flame(string name, PhantoonFlameSegment segment) =>
            phantoon.SpawnedTimeline(name, PhantoonFlameAnimations.Segments, PhantoonFlameAnimations.StartingWith(segment), FlameCue);

        string burning = Flame("flame_burning", PhantoonFlameSegment.Burning);
        phantoon.ProjectileData("bosses/phantoon_flame.json", "PhantoonFlame",
            ("gravity", EngineNames.Number(PhantoonFlameMotionDefinitions.FlameGravity)),
            ("ring_radius", PhantoonFlameMotionDefinitions.IntroCircleRadius),
            ("ring_hold_updates", PhantoonFlameMotionDefinitions.IntroCircleHoldUpdates),
            ("landing_drop", PhantoonArenaDefinitions.FlameLandingDrop),
            // The old game keeps the pause in a flame's own state, which starts it at its whole length.
            ("landing_pause", new PhantoonCasualFlameState().LandingPause.UpdatesLeft),
            ("leap_speed", (int)-PhantoonArenaDefinitions.CasualRiseSpeed.Pixels),
            ("drift_speed", EngineNames.Number(PhantoonArenaDefinitions.CasualDriftSpeed)),
            ("falling_timeline", Flame("flame_falling", PhantoonFlameSegment.CasualFallingKindling)),
            ("landing_timeline", Flame("flame_landing", PhantoonFlameSegment.CasualHitGround)),
            ("burning_timeline", burning),
            ("resting_timeline", Flame("flame_resting", PhantoonFlameSegment.CasualResting)),
            ("rain_impact_timeline", Flame("flame_rain_impact", PhantoonFlameSegment.RainImpact)),
            ("rain_sound", EngineNames.Sound(PhantoonFlameMotionDefinitions.RainFallSound)));
        // The Godot flame leaps a pixel slower at each of its two bounces after the first.
        if (PhantoonArenaDefinitions.CasualFirstBounceSpeed != PhantoonArenaDefinitions.CasualRiseSpeed + SignedEightEight.FromPixels(1)
            || PhantoonArenaDefinitions.CasualLaterBounceSpeed != PhantoonArenaDefinitions.CasualRiseSpeed + SignedEightEight.FromPixels(2))
            throw new InvalidDataException("A flame of Phantoon's does not bounce a pixel slower each time, as the Godot flame does.");

        // A flame Samus's shot puts out dies down where it is, still a flame to the touch, and may leave something once
        // it has died down.
        (List<IAnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>> dying, List<IAnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>>? dyingCycle) =
            EngineTimelines.Played(PhantoonFlameAnimations.Segments, PhantoonFlameAnimations.StartingWith(PhantoonFlameSegment.Dying));
        string putOut = phantoon.SpawnedTimelineAt($"{phantoon.Folder}/flame_put_out.json", dying, dyingCycle, EnemySheetDefinitions.ProjectileFrameName,
            static (PhantoonFlameCue _) => null);
        phantoon.Shot("flame", RoomEnemyProjectileKind.PhantoonDestroyableFlame, (_, shot, projectile) =>
            {
                shot["scene"] = PhantoonFlameScene;
                projectile["timeline"] = burning;
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
                projectile["shot_timeline"] = putOut;
                if (Drops(EnemyDrops.Rolling(PhantoonFlameSpawnDefinitions.ShotFlameDropChances)) is { } drops)
                    projectile["drops"] = drops;
            })
            .Shot("starting_flame", RoomEnemyProjectileKind.PhantoonStartingFlame, (flames, shot, projectile) =>
            {
                shot["scene"] = PhantoonFlameScene;
                projectile["timeline"] = flames.SpawnedTimeline("flame_starting", PhantoonFlameAnimations.Segments,
                    PhantoonFlameAnimations.StartingWith(PhantoonFlameSegment.StartingFlame), FlameCue);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
            })
            .Write();
    }
}
