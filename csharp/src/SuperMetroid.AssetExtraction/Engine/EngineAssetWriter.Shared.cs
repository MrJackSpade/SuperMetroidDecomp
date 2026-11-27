using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What many things share: the curve of speeds, the lesser effects, the deaths of enemies and what they drop.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The data file of the curve of speeds that hops, lunges and swings follow.</summary>
    internal const string SpeedCurveFile = "curves/quadratic.json";

    /// <summary>The lesser effects that are drawn alike in every room, which a death or a dud shot leaves.</summary>
    private static readonly RoomEffectActorKind[] SharedEffects =
    [
        RoomEffectActorKind.SmallExplosion, RoomEffectActorKind.Smoke, RoomEffectActorKind.DudShot, RoomEffectActorKind.SmokePuff,
        RoomEffectActorKind.ShortBigDustCloud, RoomEffectActorKind.BigDustCloud,
    ];

    /// <summary>The data file of the timeline of one of the old game's lesser effects.</summary>
    internal static string EffectFile(RoomEffectActorKind kind) => $"effects/{EngineSheets.FileName(kind.ToString())}.json";

    /// <summary>The data file of the timeline of one of the old game's clouds of dust and bursts, which many things leave.</summary>
    internal static string DustFile(DustExplosionAnimation animation) => $"effects/dust_{EngineSheets.FileName(animation.ToString())}.json";

    /// <summary>The data file of the timeline of one of the old game's deaths of an enemy.</summary>
    internal static string DeathFile(EnemyDeathExplosion explosion) => $"effects/enemy_death_{EngineSheets.FileName(explosion.ToString())}.json";

    /// <summary>The data file of one of the old game's pickups.</summary>
    internal static string PickupFile(EnemyPickupKind kind) => $"pickups/{EngineSheets.FileName(kind.ToString())}.json";

    /// <summary>Writes what many things share.</summary>
    private void WriteShared()
    {
        var speeds = new JsonArray();
        foreach (QuadraticCurveSpeed mark in QuadraticSpeedCurveDefinitions.Speeds)
            speeds.Add(EngineNames.Number(mark.Speed));
        _tree.WriteData(SpeedCurveFile, new JsonObject { ["speeds"] = speeds });

        foreach (RoomEffectActorKind kind in SharedEffects)
        {
            (List<IAnimationBeat<RoomEffectActorPicture, NoAnimationCue>> lead, List<IAnimationBeat<RoomEffectActorPicture, NoAnimationCue>>? cycle) =
                EngineTimelines.Played(RoomEffectActorAnimations.Segments, RoomEffectActorAnimations.StartingWith(kind));
            if (cycle is not null)
                throw new InvalidDataException($"The lesser effect {kind} never ends, which an effect left in a room must.");
            string file = EffectFile(kind);
            _tree.WriteData(file, SharedTimeline(file, lead, null, EngineEnemy.EffectFrame, static (NoAnimationCue _) => null));
        }

        foreach (DustExplosionAnimation animation in Enum.GetValues<DustExplosionAnimation>())
        {
            (List<IAnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>> lead, List<IAnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>>? cycle) =
                EngineTimelines.Played(DustExplosionProjectileAnimations.Segments, DustExplosionProjectileAnimations.StartingAt[animation]);
            string file = DustFile(animation);
            // Each ends by taking itself away, which a timeline does by ending.
            _tree.WriteData(file, SharedTimeline(file, lead, cycle, EnemySheetDefinitions.ProjectileFrameName,
                static (EnemyProjectileCommonCue cue) => cue is EnemyProjectileCommonCue.Signal { Kind: EnemyProjectileCommonCueKind.Delete }
                    ? null
                    : throw new InvalidDataException($"A cloud of dust raises {cue}, which this does not know.")));
        }

        foreach (EnemyDeathExplosion explosion in Enum.GetValues<EnemyDeathExplosion>())
        {
            (List<IAnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>> lead, List<IAnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>>? cycle) =
                EngineTimelines.Played(EnemyDeathAndDropAnimations.Segments, EnemyDeathAndDropAnimations.Explosions[explosion]);
            if (cycle is not null)
                throw new InvalidDataException($"The death {explosion} never ends.");
            string file = DeathFile(explosion);
            _tree.WriteData(file, SharedTimeline(file, lead, null, EnemySheetDefinitions.ProjectileFrameName, DeathCue));
        }

        WritePickup(EnemyPickupKind.SmallEnergy, "small energy", "energy", "reserve_energy", EnemyPickupDefinitions.SmallEnergyAmount, EnemySound.SmallEnergyPickup);
        WritePickup(EnemyPickupKind.BigEnergy, "big energy", "energy", "reserve_energy", EnemyPickupDefinitions.BigEnergyAmount, EnemySound.BigEnergyPickup);
        WritePickup(EnemyPickupKind.Missile, "missiles", "missiles", null, EnemyPickupDefinitions.MissileAmount, EnemySound.AmmoPickup);
        WritePickup(EnemyPickupKind.SuperMissile, "super missile", "super_missiles", null, EnemyPickupDefinitions.SuperMissileAmount, EnemySound.AmmoPickup);
        WritePickup(EnemyPickupKind.PowerBomb, "power bomb", "power_bombs", null, EnemyPickupDefinitions.PowerBombAmount, EnemySound.AmmoPickup);
    }

    /// <summary>What an event of a death becomes in the timeline its remains play. A death's sound joins only an empty queue.</summary>
    private static string? DeathCue(EnemyDeathAndDropCue cue) => cue switch
    {
        EnemyDeathAndDropCue.SpawnEffectActor spawn => Scatter(spawn),
        EnemyDeathAndDropCue.Signal { Kind: EnemyDeathAndDropCueKind.BecomePickup } => "drop",
        EnemyDeathAndDropCue.Signal { Kind: EnemyDeathAndDropCueKind.PlayEnemyKilledSound } =>
            EngineNames.SoundCue(EnemyDeathsAndDrops.EnemyKilledSound, SoundRequestPriority.WhenIdle),
        EnemyDeathAndDropCue.Signal { Kind: EnemyDeathAndDropCueKind.PlaySmallExplosionSound } =>
            EngineNames.SoundCue(EnemySoundEffectDefinitions.DeathSmallExplosionSound, SoundRequestPriority.WhenIdle),
        EnemyDeathAndDropCue.Signal { Kind: EnemyDeathAndDropCueKind.PlayContactKilledSound } =>
            EngineNames.SoundCue(EnemySoundEffectDefinitions.DeathContactKilledSound, SoundRequestPriority.WhenIdle),
        _ => throw new InvalidDataException($"A death raises {cue}, which this does not know."),
    };

    /// <summary>
    /// The cue that scatters a lesser effect about a death. The Godot game settles the place as the old one does: across
    /// by the lowest digits of the number drawn and down by its second eight, over a span twice the reach.
    /// </summary>
    private static string Scatter(EnemyDeathAndDropCue.SpawnEffectActor spawn)
    {
        int reach = spawn.Spread.Radius;
        if (!SharedEffects.Contains(spawn.EffectActor))
            throw new InvalidDataException($"A death scatters {spawn.EffectActor}, which is not written as an effect every room shares.");
        if (spawn.Spread.HorizontalScatter != RandomSampleField.OneOf(reach * 2) || spawn.Spread.VerticalScatter != RandomSampleField.SecondOf(reach * 2))
            throw new InvalidDataException($"A death scatters {spawn.EffectActor} over a span the Godot game's cue cannot say.");
        return $"scatter:{EffectFile(spawn.EffectActor)}:{reach}";
    }

    /// <summary>
    /// A timeline of pictures every room shares, cut into a folder named as the timeline's file is, with where those
    /// pictures lie from the place of what they show, and against the room's tiles.
    /// </summary>
    private JsonObject SharedTimeline<TFrame, TEvent>(
        string file, IReadOnlyList<IAnimationBeat<TFrame, TEvent>> lead, IReadOnlyList<IAnimationBeat<TFrame, TEvent>>? cycle, Func<TFrame, string> frameName,
        Func<TEvent, string?> cue)
        where TFrame : struct
        where TEvent : notnull
    {
        IEnumerable<string> frames = lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).Where(frame => frame.HasValue).Select(frame => frameName(frame!.Value));
        EngineSheetPictures pictures = _sheets.CutSpawned(_tree, file[..file.LastIndexOf('.')], EngineSheets.SharedSpawner, frames);
        JsonObject timeline = EngineTimelines.Of(lead, cycle, frame => pictures.Of(frameName(frame)), cue);
        timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
        return pictures.Lay(timeline, file);
    }

    /// <summary>Writes one of the pickups an enemy drops, and the timeline that shows it.</summary>
    private void WritePickup(EnemyPickupKind kind, string name, string addsTo, string? thenTo, int quantity, SuperMetroid.Core.Audio.SoundEffectId sound)
    {
        string file = PickupFile(kind);
        string timelineFile = file[..file.LastIndexOf('.')] + "_timeline.json";
        (List<IAnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>> lead, List<IAnimationBeat<EnemyProjectileFrameId, EnemyDeathAndDropCue>>? cycle) =
            EngineTimelines.Played(EnemyDeathAndDropAnimations.Segments, EnemyDeathAndDropAnimations.StartingAt[EnemyPickupDefinitions.Animations[kind].Animation]);
        _tree.WriteData(timelineFile, SharedTimeline(timelineFile, lead, cycle, EnemySheetDefinitions.ProjectileFrameName, DeathCue));

        // A pickup is touched as far from its middle each way as the old game's drop is.
        EnemyProjectileDefinition drop = EnemyProjectileDefinitionCatalog.Definitions[RoomEnemyProjectileKind.EnemyDeathPickup];
        if (drop.Radius.X != drop.Radius.Y)
            throw new InvalidDataException("The old game's drop is not as wide as it is tall, which a pickup's one size cannot say.");
        var data = new JsonObject
        {
            ["name"] = name,
            ["adds_to"] = addsTo,
            ["quantity"] = quantity,
            ["lasts_updates"] = EnemyPickupDefinitions.Lifetime,
            ["size"] = drop.Radius.X * 2,
            ["timeline"] = timelineFile,
            // A pickup's sound joins only an empty queue.
            ["sound"] = EngineNames.Sound(sound, SoundRequestPriority.WhenIdle),
        };
        if (thenTo is not null)
            data["then_to"] = thenTo;
        _tree.WriteData(file, data);
    }

    /// <summary>What an enemy drops, as its data file says it; null for one that drops nothing.</summary>
    internal static JsonObject? Drops(EnemyDrops drops)
    {
        if (drops is not EnemyDrops.Rolled { Chances: var chances })
            return null;
        static JsonObject Chance(EnemyPickupKind kind, int chance) => new() { ["pickup"] = PickupFile(kind), ["chance"] = chance };
        return new JsonObject
        {
            ["out_of"] = EnemyDropSelectionDefinitions.FullWeightBudget,
            ["nothing"] = chances.Nothing,
            ["minor"] = new JsonArray(
                Chance(EnemyPickupKind.SmallEnergy, chances.SmallEnergy), Chance(EnemyPickupKind.BigEnergy, chances.BigEnergy),
                Chance(EnemyPickupKind.Missile, chances.Missile)),
            ["major"] = new JsonArray(Chance(EnemyPickupKind.SuperMissile, chances.SuperMissile), Chance(EnemyPickupKind.PowerBomb, chances.PowerBomb)),
            // While Samus is short of energy, only energy is offered.
            ["need"] = new JsonObject
            {
                ["name"] = "energy",
                ["amounts"] = new JsonArray("energy", "reserve_energy"),
                ["below"] = EnemyDropSelectionDefinitions.CriticalEnergy,
                ["until"] = EnemyDropSelectionDefinitions.RecoveredEnergy,
                ["only"] = new JsonArray(PickupFile(EnemyPickupKind.SmallEnergy), PickupFile(EnemyPickupKind.BigEnergy)),
            },
        };
    }

    /// <summary>
    /// How an enemy dies, as its data file says it: the death the old game gives it when a shot destroys it, and the
    /// others the old game plays whatever the enemy: the small one for a power bomb or the grapple beam, and the one for
    /// Samus's own body. A Super Missile's kill raises the smaller explosions to the normal one
    /// (<see cref="EnemyDeathExplosions.RaisedBySuperMissile"/>), for an enemy that dies by the common shot death.
    /// </summary>
    /// <param name="explosion">The explosion a shot's kill gives it.</param>
    /// <param name="raisedBySuperMissile">Whether a Super Missile's kill raises it: not so for one whose death names its own explosion whatever killed it.</param>
    internal static JsonObject Death(EnemyDeathExplosion explosion, bool raisedBySuperMissile = true)
    {
        var byKind = new JsonObject
        {
            ["power_bomb"] = DeathFile(EnemyDeathExplosion.Small),
            ["grapple"] = DeathFile(EnemyDeathExplosion.Small),
            ["screw_attack"] = DeathFile(EnemyDeathExplosion.SamusContact),
            ["speed_boost"] = DeathFile(EnemyDeathExplosion.SamusContact),
            ["shinespark"] = DeathFile(EnemyDeathExplosion.SamusContact),
            ["charged_spin"] = DeathFile(EnemyDeathExplosion.SamusContact),
        };
        if (raisedBySuperMissile)
            byKind["super_missile"] = DeathFile(EnemyDeathExplosions.RaisedBySuperMissile(explosion));
        return new JsonObject { ["effect"] = DeathFile(explosion), ["by_kind"] = byKind };
    }
}
