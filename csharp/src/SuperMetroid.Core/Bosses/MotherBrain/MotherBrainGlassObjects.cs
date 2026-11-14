using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's glass: its setup, its shatter projectiles and the hit count it keeps after the glass is gone.</summary>
[RoomLifetime]
internal sealed class MotherBrainGlassObjects
{
    /// <summary>The area's boss-defeat flags.</summary>
    private readonly IAreaBossFlagReader? _bosses;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>Room sound queue receiving the glass-shatter effect.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds the glass animation's handling to the room's room-object slots, event services, and sound queue.</summary>
    internal MotherBrainGlassObjects(IAreaBossFlagReader? bosses, IGlobalEventFlags? events, RoomObjectSoundQueue sounds)
    {
        _bosses = bosses;
        _events = events;
        _sounds = sounds;
    }
    /// <summary>Four shard spawns emitted by the most recent shatter.</summary>
    private readonly List<MotherBrainGlassProjectileRequest>
        _motherBrainGlassProjectileRequests = new();
    /// <summary>The slot of the live glass room object, or null after deletion.</summary>
    private RoomObjectSlot? _motherBrainGlassSlot;
    /// <summary>Whether the current room loaded the glass room object before head damage can update it.</summary>
    internal bool MotherBrainGlassWasLoaded { get; private set; }

    /// <summary>Shard actors requested by the most recent room-object pass.</summary>
    public IReadOnlyList<MotherBrainGlassProjectileRequest> MotherBrainGlassProjectileRequests =>
        _motherBrainGlassProjectileRequests;

    /// <summary>Runs the glass setup against the slot room population gave it.</summary>
    internal void SetupMotherBrainGlassSlot(
        RoomLevelData level,
        PlacedRoomObject placed,
        RoomObjectSlot slot)
    {
        if (placed.PlacementIndex != 0 ||
            placed.Block != new Position16<RoomBlocks>(9, 5))
        {
            throw new InvalidDataException(
                $"Mother Brain glass population diverged: placement={placed.PlacementIndex}, " +
                $"block=({placed.Block.X},{placed.Block.Y}), argument={placed.Argument}.");
        }

        slot.Become(new MotherBrainGlassObject(this));
        RoomCollisionBlock original = level.GetCollisionBlock(slot.Block);
        level.SetTerrain(slot.Block, original.Terrain.WithCollision(RoomCollisionType.SolidBlock));
        level.SetBehavior(slot.Block, RoomBlockBehavior.Reacting(ShootableReaction.ResidentObjectTrigger));

        _motherBrainGlassSlot = slot;
        MotherBrainGlassWasLoaded = true;
    }

    /// <summary>
    /// Mother Brain's head shot counts a hit on the glass at once. This is separate from the glass's own count of strikes
    /// on its block: a missile can overlap the brain before its terrain probe reaches the glass block, and the head shot
    /// counts that hit immediately in the enemy collision pass. A hit once the glass has left the room changes nothing.
    /// </summary>
    public void CountHeadShotOnGlass()
    {
        if (!MotherBrainGlassWasLoaded)
        {
            throw new InvalidOperationException(
                "Mother Brain head damage requires Mother Brain's glass room object to be loaded.");
        }

        if (TryGetMotherBrainGlass(out MotherBrainGlassObject? glass))
            glass.CountHeadShot();
    }

    /// <summary>Clears shard spawn requests before processing the next room frame.</summary>
    internal void BeginMotherBrainGlassFrame() => _motherBrainGlassProjectileRequests.Clear();

    /// <summary>Whether Mother Brain, the area boss, is dead.</summary>
    internal bool IsBossDead() => _bosses?.IsDefeated(DefeatedBosses.AreaBoss) == true;

    /// <summary>Whether the glass was destroyed on an earlier visit.</summary>
    internal bool IsGlassDestroyed() => _events?.Has(StoryEvent.MotherBrainGlassDestroyed) == true;

    /// <summary>
    /// Queues the shatter sound and spawns a burst of four shard projectiles at the glass's block.
    /// </summary>
    internal void SpawnShards(RoomObjectSlot slot, IReadOnlyList<MotherBrainGlassShard> shards)
    {
        _sounds.Queue(RoomSound.MotherBrainGlassShatter, Priority: ShatterSoundPriority);
        // The fragment spawn carries both block coordinates, each below 256.
        Position16<RoomBlocks> glassBlock = slot.Block;
        foreach (MotherBrainGlassShard shard in shards)
            _motherBrainGlassProjectileRequests.Add(new MotherBrainGlassProjectileRequest(
                RoomEnemyProjectileKind.MotherBrainGlassShard, shard, glassBlock));
    }

    /// <summary>Sets the glass-destroyed event.</summary>
    internal void SetGlassDestroyed() =>
        (_events ?? throw new InvalidOperationException("Mother Brain glass has no event writer.")).Set(StoryEvent.MotherBrainGlassDestroyed);

    /// <summary>The shatter sound's request priority.</summary>
    private static readonly SoundRequestPriority ShatterSoundPriority = SoundRequestPriority.Essential;


    /// <summary>Finds the live glass, without accepting a later object that took its place.</summary>
    internal bool TryGetMotherBrainGlass([NotNullWhen(true)] out MotherBrainGlassObject? glass)
    {
        if (_motherBrainGlassSlot is { Active: true, Behaviour: MotherBrainGlassObject live })
        {
            glass = live;
            return true;
        }
        glass = null;
        return false;
    }

    /// <summary>Forgets the glass as it leaves the room, before its place is freed.</summary>
    internal void OnObjectDeleted(RoomObjectSlot slot)
    {
        Ensure.NotNull(slot);
        if (ReferenceEquals(slot, _motherBrainGlassSlot))
            _motherBrainGlassSlot = null;
    }
}
