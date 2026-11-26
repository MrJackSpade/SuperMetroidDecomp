using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;
using SoundRequestPriority = SuperMetroid.Core.Compatibility.SoundRequestPriority;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Spore Spawn: the pod, its stalk, the places its spores come from, the spores, and the ceiling it brings down.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The name of the patch that is Spore Spawn's ceiling open.</summary>
    private const string SporeSpawnCeilingOpenPatch = "ceiling_open";

    /// <summary>Writes Spore Spawn, its spore and the patches of its ceiling.</summary>
    private void WriteSporeSpawn()
    {
        const string sporeScene = "res://game/bosses/spore_spawn/spore_spawn_spore.tscn", sporeFile = "bosses/spore_spawn_spore.json";
        EngineEnemy boss = Enemy("bosses/spore_spawn.json", "SporeSpawn", EnemyDefinitionId.SporeSpawn);
        SortedDictionary<string, string> roomPlaces = RoomPlacesOf(EnemyDefinitionId.SporeSpawn);

        // Its other colours, how hurt it is and each step of its hardening, are changes of the colours of its body's
        // pictures and of its stalk's, each cut from sheets of its own.
        const string enemyFamily = GameInstallationLayout.EnemySheetDirectoryName;
        string sporeSpawnKey = EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.SporeSpawn);
        IReadOnlyList<string> bodySheets = [_sheets.EnemySheet(sporeSpawnKey)];
        IReadOnlyList<string> stalkSheets = [_sheets.SheetHolding(sporeSpawnKey, EnemySheetDefinitions.ProjectileFrameName(EnemyProjectileFrameId.SporeSpawnStalk))];
        JsonObject BodyColours(string shade) =>
            ShadeChange(shade, enemyFamily, new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [boss.Folder] = bodySheets });
        JsonObject StalkColours(string shade) =>
            ShadeChange(shade, enemyFamily, new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [$"{boss.Folder}/stalk"] = stalkSheets });

        // The ceiling crumbles through each frame of the old change and is left as its last, the patch a room where
        // it has been beaten begins with.
        JsonObject ceiling = TerrainChangeAt(
            StatesHolding(EnemyDefinitionId.SporeSpawn), "ceiling_crumbles", SporeSpawnCeilingMutationDefinitions.Block,
            SporeSpawnCeilingMutationDefinitions.Changes[SporeSpawnCeilingMutation.Crumble],
            GameInstallationLayout.RoomObjectSporeSpawnCeilingVisualDirectoryName,
            static draw => draw is RoomObjectFrameDraw<SporeSpawnCeilingDraw> framed
                ? (SporeSpawnCeilingDrawKeys.Installed.Of(framed.Frame), SporeSpawnCeilingDraws.Family.Shape(framed.Frame))
                : throw new InvalidDataException("Spore Spawn's ceiling shows something that is not one of its own frames."),
            leaves: SporeSpawnCeilingOpenPatch);

        // The spores: where each emitter is from where Spore Spawn hangs, and how far below it a spore is gone.
        int[] emitterHeights = [.. SporeSpawnProjectileDefinitions.Emitters.Select(emitter => emitter.Place.Y).Distinct()];
        if (emitterHeights.Length != 1)
            throw new InvalidDataException("Spore Spawn's emitters are at different heights, which the one fall its spores are given cannot say.");

        static string Movement(SporeSpawnMovement movement) => movement switch
        {
            SporeSpawnMovement.Idle => "idle",
            SporeSpawnMovement.Descending => "descending",
            SporeSpawnMovement.Moving => "moving",
            SporeSpawnMovement.SetupDeath => "setup_death",
            SporeSpawnMovement.Dying => "dying",
            _ => throw new InvalidDataException($"Spore Spawn moves as {movement}, which this does not know."),
        };
        static string? Cue(SporeSpawnCue cue) => cue switch
        {
            SporeSpawnCue.ShowDeathShade shown => "shade:" + HardeningSet(shown.DeathShadeStep),
            SporeSpawnCue.QueueSound queued => EngineNames.SoundCue(queued.Sound),
            SporeSpawnCue.SetMovement set => "movement:" + Movement(set.Movement),
            // A swing the old game turns counterclockwise goes forward through its angles.
            SporeSpawnCue.SetSway sway => $"sway:{sway.Radius.Pixels}:{SwayTurn(sway.Step)}",
            SporeSpawnCue.SetSporeGeneration spores => spores.SporeGenerationSuppressed ? "spores:off" : "spores:on",
            SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.WidenSway } => "widen",
            SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.ReopenToHits } => "reopen",
            SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.Harden } => "harden",
            SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.RequestDeathDrops } => "drops",
            SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.SpawnHardeningDust } => "hardening_dust",
            SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.SpawnDyingExplosion } => "dying_explosion",
            _ => throw new InvalidDataException($"Spore Spawn's animation raises {cue}, which this does not know."),
        };

        // The core takes no beam that is not charged, whatever the table says of it; a charged one does the charged damage.
        EnemyBeamFamilyVulnerability noBeam = new(EnemyWeaponVulnerability.NoDamage, EnemyWeaponVulnerability.NoDamage, EnemyWeaponVulnerability.NoDamage, EnemyWeaponVulnerability.NoDamage);
        RoomEnemyDefinition core = boss.Definition with
        {
            Vulnerability = boss.Definition.Vulnerability with { Power = noBeam, Spazer = noBeam, Plasma = noBeam },
        };

        boss.BodyColliders("Solid")
            .Group("reactions", EngineReactions.Of(core))
            .Value("start_rise", SporeSpawnArenaDefinitions.FightStartRise)
            .Value("descent_speed", SporeSpawnArenaDefinitions.DescentSpeed)
            .Value("ceiling_above", SporeSpawnArenaDefinitions.CentreY - SporeSpawnArenaDefinitions.CeilingY)
            .Value("stalk_anchor_above", -SporeSpawnArenaDefinitions.StalkAnchorAboveBody.Y)
            .Value("stalk_body_gap", SporeSpawnArenaDefinitions.StalkBodyGap)
            .Value("vertical_sway_narrowing", SporeSpawnArenaDefinitions.VerticalSwayNarrowing)
            .Value("widened_sway_limit", SporeSpawnArenaDefinitions.WidenedSwayLimit)
            .Value("sway_widening", SporeSpawnArenaDefinitions.SwayWidening)
            .Value("fast_swing_below", SporeSpawnHealthBandDefinitions.FastSwingBelow)
            .Value("fast_sway_step", SporeSpawnArenaDefinitions.FastSwayStep.ToTrigonometryAngle().RawValue)
            .Value("death_drops", SporeSpawnArenaDefinitions.DeathDrops)
            .Value("dust_effect", DustFile(DustExplosionAnimation.BigDustCloud))
            .Value("ceiling_dust_effect", EffectFile(RoomEffectActorKind.BigDustCloud))
            .Value("explosion_effect", EffectFile(RoomEffectActorKind.SmallExplosion))
            .Value("hardening_sound", EngineNames.Sound(SporeSpawnSoundDefinitions.HardeningDust))
            .Value("explosion_sound", EngineNames.Sound(SporeSpawnSoundDefinitions.DyingExplosion))
            .Group("ceiling_crumble", ceiling)
            .Group("emitters", EngineNames.List(SporeSpawnProjectileDefinitions.Emitters.Select(emitter => (JsonNode)new JsonObject
            {
                ["x"] = emitter.Place.X - SporeSpawnArenaDefinitions.CentreX,
                ["y"] = emitter.Place.Y - SporeSpawnArenaDefinitions.CentreY,
                ["drift"] = EngineNames.Side(emitter.Drift),
            })))
            .Group("poses", SporeSpawnPoses())
            .Group("health_shades", EngineNames.List(SporeSpawnHealthBandDefinitions.HighestFirst.Select(band => (JsonNode)new JsonObject
            {
                ["health"] = band.MinimumHealth,
                ["body"] = BodyColours(SporeSpawnShades.Health(band).Key),
                ["stalk"] = StalkColours(SporeSpawnShades.Health(band).Key),
            })))
            .Group("hardening_colours", EngineNames.List(SporeSpawnDeathFadeStep.All.Select(step => (JsonNode)new JsonObject
            {
                ["shade"] = HardeningSet(step),
                ["body"] = BodyColours(SporeSpawnShades.DeathSprite(step).Key),
                ["stalk"] = StalkColours(SporeSpawnShades.DeathSprite(step).Key),
                // Each step of its hardening lays the room's level and then its background in the husk's colours.
                ["room"] = ShadeChange($"{SporeSpawnShades.DeathLevel(step).Key}+{SporeSpawnShades.DeathBackground(step).Key}", roomPlaces),
            })));

        foreach ((string name, SporeSpawnBodySegment segment) in new[]
                 {
                     ("husk", SporeSpawnBodySegment.Husk), ("awaiting_descent", SporeSpawnBodySegment.AwaitingDescent),
                     ("fight_started", SporeSpawnBodySegment.FightStarted), ("open_and_stop", SporeSpawnBodySegment.OpenAndStop),
                     ("close_and_move", SporeSpawnBodySegment.CloseAndMove), ("death", SporeSpawnBodySegment.Death),
                 })
        {
            boss.Timeline(name, EngineEnemy.WithPoseCues(
                EngineTimelines.Of(SporeSpawnAnimations.Segments, SporeSpawnAnimations.StartingWith(segment), boss.Picture, Cue)));
        }

        // The stalk and the emitters, which the old game drew as things Spore Spawn launched and kept by it.
        static string? Release(SporeSpawnCeilingCue cue) => cue == SporeSpawnCeilingCue.SpawnSpore
            ? "spore"
            : throw new InvalidDataException($"Spore Spawn's ceiling raises {cue}, which this does not know.");
        boss.SpawnedLook("stalk", SporeSpawnProjectileAnimations.CeilingSegments, SporeSpawnProjectileAnimations.StartingWith(SporeSpawnCeilingSegment.Stalk), Release)
            .SpawnedLook("emitter_closed", SporeSpawnProjectileAnimations.CeilingSegments, SporeSpawnProjectileAnimations.StartingWith(SporeSpawnCeilingSegment.SpawnerClosed), Release)
            .SpawnedLook("emitter_release", SporeSpawnProjectileAnimations.CeilingSegments, SporeSpawnProjectileAnimations.StartingWith(SporeSpawnCeilingSegment.SpawnerRelease), Release);

        // A spore drifts through everything and is taken away at the floor; shot, it bursts and may leave something.
        static string? Burst(SporeSpawnSporeCue cue) => cue switch
        {
            SporeSpawnSporeCue.SetImpactProperties => "harmless",
            // The cry of a death, which joins only an empty queue.
            SporeSpawnSporeCue.QueueKilledSound => EngineNames.SoundCue(EnemyDeathsAndDrops.EnemyKilledSound, SoundRequestPriority.WhenIdle),
            SporeSpawnSporeCue.RequestDrop => "drop",
            // It takes itself away, which a timeline does by ending.
            SporeSpawnSporeCue.Delete => null,
            _ => throw new InvalidDataException($"A spore's animation raises {cue}, which this does not know."),
        };
        string burst = boss.SpawnedTimeline("spore_burst", SporeSpawnProjectileAnimations.SporeSegments,
            SporeSpawnProjectileAnimations.StartingWith(SporeSpawnSporeSegment.SporeShot), Burst);
        boss.Shot("spore", RoomEnemyProjectileKind.SporeSpawnSpore, static (spawn, shot, projectile) =>
            {
                shot["scene"] = sporeScene;
                projectile["timeline"] = spawn.SpawnedTimeline("spore", SporeSpawnProjectileAnimations.SporeSegments,
                    SporeSpawnProjectileAnimations.StartingWith(SporeSpawnSporeSegment.Spore), Burst);
                projectile["picture_offset"] = EngineEnemy.NoOffset();
                projectile["meeting_terrain"] = "passes_through";
                projectile["drops"] = Drops(EnemyDrops.Rolling(SporeSpawnProjectileDefinitions.SporeDropChances))
                    ?? throw new InvalidDataException("A spore drops nothing.");
            })
            .Write();

        var spore = new JsonObject();
        EngineDefaults.Fill(spore, "SporeSpawnSpore");
        if (!spore.ContainsKey("fall_distance") || !spore.ContainsKey("burst_timeline"))
            throw new InvalidDataException($"The data file {sporeFile} is given values its type does not read by those names.");
        spore["fall_distance"] = SporeSpawnArenaDefinitions.SporeFloorY - emitterHeights[0];
        spore["burst_timeline"] = burst;
        spore["drift"] = EngineNames.List(SporeSpawnProjectileDefinitions.Drift.Select(step => (JsonNode)new JsonObject { ["x"] = step.X, ["y"] = step.Y }));
        _tree.WriteData(sporeFile, spore);
    }

    /// <summary>The name of one step of Spore Spawn's hardening.</summary>
    private static string HardeningSet(SporeSpawnDeathFadeStep step) => $"hardening_{step.Step}";

    /// <summary>How many angle steps a swing of Spore Spawn's turns each update: forward for one the old game turns counterclockwise.</summary>
    private static int SwayTurn(TurnStep step)
    {
        int steps = step.PerUpdate.ToTrigonometryAngle().RawValue;
        return step.Direction == RotationDirection.Counterclockwise ? steps : -steps;
    }

    /// <summary>
    /// Where Spore Spawn's parts are for each of its frames. The old game strikes the first of a frame's hitboxes a
    /// shot overlaps, so one that lies wholly inside an earlier one is never struck and is left out. What is left is
    /// the shell's pieces, which turn shots away, and at most one piece that takes them: the core.
    /// </summary>
    private static JsonObject SporeSpawnPoses()
    {
        var poses = new JsonObject();
        foreach ((EnemyFrameId frame, EnemyCollisionComponent[] components) in SporeSpawnCollisionDefinitions.FrameComponents)
        {
            var struck = new List<EnemyCollisionHitbox>();
            foreach (EnemyCollisionComponent component in components)
            {
                if (component.Offset != Offset16<RoomPixels>.Zero)
                    throw new InvalidDataException($"A piece of Spore Spawn's frame {frame} lies away from its place, which its poses do not say.");
                foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
                {
                    if (!struck.Any(over => over.Left <= hitbox.Left && over.Top <= hitbox.Top && over.Right >= hitbox.Right && over.Bottom >= hitbox.Bottom))
                        struck.Add(hitbox);
                }
            }

            // In the order a shot meets them: the shell above and below, the core, the crown.
            List<EnemyCollisionHitbox> shell = [.. struck.Where(hitbox => hitbox.Shot == SporeSpawnCollisionDefinitions.Dud)];
            List<EnemyCollisionHitbox> cores = [.. struck.Where(hitbox => hitbox.Shot == SporeSpawnCollisionDefinitions.Shot)];
            if (shell.Count + cores.Count != struck.Count || shell.Count is < 2 or > 3 || cores.Count > 1)
                throw new InvalidDataException($"Spore Spawn's frame {frame} has pieces its poses cannot say.");
            string[] names = shell.Count == 2 ? ["UpperShell", "Crown"] : ["UpperShell", "LowerShell", "Crown"];
            var ordered = new List<(string Part, EnemyCollisionHitbox Hitbox)>();
            for (int piece = 0; piece < shell.Count - 1; piece++)
                ordered.Add((names[piece], shell[piece]));
            if (cores.Count == 1)
                ordered.Add(("Core", cores[0]));
            ordered.Add((names[^1], shell[^1]));
            if (!ordered.Select(piece => piece.Hitbox).SequenceEqual(struck))
                throw new InvalidDataException($"A shot meets the pieces of Spore Spawn's frame {frame} in an order its parts do not have.");

            // Each piece is a hurtbox for a shot and a hitbox for Samus's touch, which the old probe meets on
            // different edges of its box.
            var colliders = new JsonObject();
            foreach ((string part, EnemyCollisionHitbox hitbox) in ordered)
            {
                colliders[part] = EngineEnemy.ShotRect(hitbox);
                colliders[part + "Touch"] = EngineEnemy.TouchRect(hitbox);
            }
            poses[EngineEnemy.PoseName(frame)] = new JsonObject { ["core_takes_shots"] = cores.Count == 1, ["colliders"] = colliders };
        }

        return poses;
    }
}
