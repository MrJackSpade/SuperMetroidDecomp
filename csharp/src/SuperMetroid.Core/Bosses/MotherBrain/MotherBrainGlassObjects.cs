using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's glass: its setup, its shatter projectiles and the hit count it keeps after the glass is gone.</summary>
internal sealed partial class MotherBrainGlassObjects
{
    /// <summary>The area's boss-defeat flags.</summary>
    private readonly IDefeatedBossesReader _bosses;
    /// <summary>The game's event flags.</summary>
    private readonly IStoryEvents _events;
    /// <summary>Room sound queue receiving the glass-shatter effect.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds the glass animation's handling to the room's event services and sound queue.</summary>
    internal MotherBrainGlassObjects(IDefeatedBossesReader bosses, IStoryEvents events, RoomObjectSoundQueue sounds)
    {
        _bosses = bosses;
        _events = events;
        _sounds = sounds;
    }
    /// <summary>The place of the live glass room object, or null after deletion.</summary>
    private RoomObjectPlace? _motherBrainGlassPlace;
    /// <summary>Whether the current room loaded the glass room object before head damage can update it.</summary>
    internal bool MotherBrainGlassWasLoaded { get; set; }

    /// <summary>
    /// Sets the glass up at its place in the room, throwing its shards among <paramref name="shards"/>: the room's enemy
    /// projectiles, which the room's enemies built before its room objects are set up.
    /// </summary>
    internal void SetupMotherBrainGlass(RoomLevelData level, RoomObjectPlace place, IMotherBrainGlassShardSpawner shards)
    {
        place.Become(new MotherBrainGlassObject(this, shards));
        // The setup makes the glass's block solid and shootable and leaves its picture as it is (#2502).
        level.SetCollisionAndBehavior(place.Block, RoomCollisionType.SolidBlock, RoomBlockBehavior.Reacting(ShootableReaction.ResidentObjectTrigger));

        _motherBrainGlassPlace = place;
        MotherBrainGlassWasLoaded = true;
    }

    /// <summary>
    /// Mother Brain's head shot counts a hit on the glass at once. This is separate from the glass's own count of strikes
    /// on its block: a missile can overlap the brain before its terrain probe reaches the glass block, and the head shot
    /// counts that hit immediately in the enemy collision pass. A hit once the glass has left the room changes nothing.
    /// </summary>
    public void CountHeadShotOnGlass()
    {
        if (TryGetMotherBrainGlass(out MotherBrainGlassObject? glass))
            glass.CountHeadShot();
    }

    /// <summary>
    /// Confirms, as Mother Brain's body sets up, that her room set her glass up, which her head shots count on: the room's
    /// objects are set up before its enemies, so her room's data is checked once, as she spawns.
    /// </summary>
    /// <exception cref="InvalidDataException">Her room set up no glass.</exception>
    public void ConfirmGlassLoaded()
    {
        if (!MotherBrainGlassWasLoaded)
            throw new InvalidDataException("Mother Brain's room sets her glass up before she spawns; her head shots count on it.");
    }

    /// <summary>Whether Mother Brain, the area boss, is dead.</summary>
    internal bool IsBossDead() => _bosses.IsDefeated(DefeatedBosses.AreaBoss);

    /// <summary>Whether the glass was destroyed on an earlier visit.</summary>
    internal bool IsGlassDestroyed() => _events.Has(StoryEvent.MotherBrainGlassDestroyed);

    /// <summary>
    /// Queues the shatter sound and throws a burst of shard projectiles from the glass's block into the step's
    /// <paramref name="outcome"/>, each spawning among <paramref name="spawner"/>.
    /// </summary>
    internal void ThrowShards(RoomObjectPlace place, IReadOnlyList<MotherBrainGlassShard> shards, RoomObjectStepOutcome outcome,
        IMotherBrainGlassShardSpawner spawner)
    {
        Ensure.NotNull(place);
        Ensure.NotNull(shards);
        Ensure.NotNull(outcome);
        _sounds.Queue(RoomSound.MotherBrainGlassShatter, Priority: ShatterSoundPriority);
        // The shards spawn from the glass's block.
        Position16<RoomBlocks> glassBlock = place.Block;
        foreach (MotherBrainGlassShard shard in shards)
            outcome.Threw(new MotherBrainGlassProjectileRequest(shard, glassBlock, spawner));
    }

    /// <summary>Sets the glass-destroyed event.</summary>
    internal void SetGlassDestroyed() => _events.Set(StoryEvent.MotherBrainGlassDestroyed);

    /// <summary>The shatter sound's request priority.</summary>
    private static readonly SoundRequestPriority ShatterSoundPriority = SoundRequestPriority.Essential;


    /// <summary>Finds the live glass, without accepting a later object that took its place.</summary>
    internal bool TryGetMotherBrainGlass([NotNullWhen(true)] out MotherBrainGlassObject? glass)
    {
        if (_motherBrainGlassPlace is { Active: true, Behaviour: MotherBrainGlassObject live })
        {
            glass = live;
            return true;
        }
        glass = null;
        return false;
    }

    /// <summary>Forgets the glass at <paramref name="place"/> as the glass leaves the room, before its place is freed.</summary>
    internal void ForgetGlass(RoomObjectPlace place)
    {
        Ensure.NotNull(place);
        if (ReferenceEquals(place, _motherBrainGlassPlace))
            _motherBrainGlassPlace = null;
    }
}
