using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's glass PLM ($D6DE): its setup, shatter projectiles and the room argument it keeps after deletion.</summary>
[RoomLifetime]
internal sealed class MotherBrainGlassPlms
{
    /// <summary>The area's boss-defeat flags.</summary>
    private readonly IAreaBossFlagReader? _bosses;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>Physical PLM slots used to retain the glass argument across deletion.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Room sound queue receiving the glass-shatter effect.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Binds glass instruction handling to the room's PLM slots, event services, and sound queue.</summary>
    internal MotherBrainGlassPlms(IAreaBossFlagReader? bosses, IGlobalEventFlags? events, PlmSlotTable slotTable, PlmSoundQueue sounds)
    {
        _bosses = bosses;
        _events = events;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>Only pre-instruction accepted from the installed Mother Brain glass header.</summary>
    /// <summary>Four shard spawns emitted by the most recent shatter instruction.</summary>
    private readonly List<MotherBrainGlassProjectileRequest>
        _motherBrainGlassProjectileRequests = new();
    /// <summary>The slot of the live glass PLM, or null after deletion.</summary>
    private PlmSlot? _motherBrainGlassSlot;
    /// <summary>Whether the current room loaded the glass PLM before head damage can update it.</summary>
    internal bool MotherBrainGlassWasLoaded { get; private set; }
    /// <summary>
    /// Glass hit count retained after the glass PLM's slot has been deleted. Only glass-phase head hits
    /// add to it, so it stays far below the slot word's range; nothing in play reads it back.
    /// </summary>
    internal int MotherBrainGlassLastRoomArgument { get; private set; }

    /// <summary>Shard actors requested by the most recent PLM handler pass.</summary>
    public IReadOnlyList<MotherBrainGlassProjectileRequest> MotherBrainGlassProjectileRequests =>
        _motherBrainGlassProjectileRequests;

    /// <summary>Runs the glass setup against the slot room population gave it, the first in processing order.</summary>
    internal void SetupMotherBrainGlassSlot(
        RoomLevelData level,
        RoomPlmPopulationRecord record,
        PlmSlot slot)
    {
        bool firstSpawned = ReferenceEquals(slot, _slotTable.FirstInProcessingOrder);
        if (record.RecordIndex != 0 || !firstSpawned ||
            record.Block != new Position16<RoomBlocks>(9, 5))
        {
            throw new InvalidDataException(
                $"Mother Brain glass population diverged: record={record.RecordIndex}, " +
                $"first spawned={firstSpawned}, block=({record.Block.X},{record.Block.Y}), " +
                $"argument={record.Argument}.");
        }

        slot.StartAnimation(new MotherBrainGlassPlmAnimation());
        slot.HitCounter = 0;
        RoomCollisionBlock original = level.GetCollisionBlockByIndex(slot.BlockIndex);
        level.SetTerrain(slot.BlockIndex, original.Terrain.WithCollision(RoomCollisionType.SolidBlock));
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);

        _motherBrainGlassSlot = slot;
        _motherBrainGlassRoomWidth = level.Size.X;
        MotherBrainGlassWasLoaded = true;
        MotherBrainGlassLastRoomArgument = 0;
    }

    /// <summary>
    /// Implements Mother Brain head shot AI's direct <c>INC $1DC7</c>. This is intentionally
    /// separate from the block-hit pre-instruction: a missile can overlap the brain record
    /// before its terrain probe reaches the glass block, and native code counts that hit
    /// immediately in the enemy collision pass.
    /// </summary>
    public void IncrementMotherBrainGlassRoomArgument()
    {
        if (!MotherBrainGlassWasLoaded)
        {
            throw new InvalidOperationException(
                "Mother Brain head damage requires loaded room PLM $D6DE.");
        }

        if (TryGetMotherBrainGlassSlot(out PlmSlot? slot))
            slot.HitCounter = WrappingCounter.IncrementWord(slot.HitCounter);
        else
            MotherBrainGlassLastRoomArgument++;
    }

    /// <summary>Clears shard spawn requests before processing the next room frame.</summary>
    internal void BeginMotherBrainGlassFrame() => _motherBrainGlassProjectileRequests.Clear();

    /// <summary>Counts missile or super-missile block hits through the installed glass pre-instruction.</summary>
    internal static void RunMotherBrainGlassPreInstruction(PlmSlot slot)
    {
        if (slot.ProjectileHit?.Family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
            slot.HitCounter = WrappingCounter.IncrementWord(slot.HitCounter);
        slot.ProjectileHit = null;
    }
    /// <summary>Whether Mother Brain, the area boss, is dead.</summary>
    internal bool IsBossDead() => _bosses?.IsDefeated(BossBits.AreaBoss) == true;

    /// <summary>Whether the glass was destroyed on an earlier visit.</summary>
    internal bool IsGlassDestroyed() => _events?.Has(EventNumber.MotherBrainGlassDestroyed) == true;

    /// <summary>
    /// <c>$84:D30B</c>: queues the shatter sound and spawns a burst of four shard projectiles at the glass's block.
    /// </summary>
    internal void SpawnShards(PlmSlot slot, IReadOnlyList<MotherBrainGlassShard> shards)
    {
        _sounds.Queue(Library3Sound.MotherBrainGlassShatter, MaximumQueued: ShatterSoundMaximumQueued);
        // The native fragment spawn carries both block coordinates as bytes.
        Position16<RoomBlocks> glassBlock = new(checked((byte)(slot.BlockIndex % _motherBrainGlassRoomWidth)),
            checked((byte)(slot.BlockIndex / _motherBrainGlassRoomWidth)));
        foreach (MotherBrainGlassShard shard in shards)
            _motherBrainGlassProjectileRequests.Add(new MotherBrainGlassProjectileRequest(
                RoomEnemyProjectileKind.MotherBrainGlassShard, shard, glassBlock));
    }

    /// <summary>Sets the glass-destroyed event.</summary>
    internal void SetGlassDestroyed() =>
        (_events ?? throw new InvalidOperationException("Mother Brain glass has no event writer.")).Set(EventNumber.MotherBrainGlassDestroyed);

    /// <summary>The shatter sound's native queue cap.</summary>
    private static readonly SoundQueueLimit ShatterSoundMaximumQueued = SoundQueueLimit.Max15;

    /// <summary>Room width in blocks used to turn the glass block index into shard spawn coordinates.</summary>
    private int _motherBrainGlassRoomWidth;

    /// <summary>Finds the original live glass slot without accepting a later PLM that reused it.</summary>
    internal bool TryGetMotherBrainGlassSlot([NotNullWhen(true)] out PlmSlot? slot)
    {
        if (_motherBrainGlassSlot is { Active: true, Header: PlmHeaderId.MotherBrainGlass } candidate)
        {
            slot = candidate;
            return true;
        }
        slot = null;
        return false;
    }

    /// <summary>Retains the glass hit count and clears the deleted slot's glass-only instruction identity.</summary>
    internal void OnPlmDeleted(PlmSlot slot)
    {
        if (slot.Header != PlmHeaderId.MotherBrainGlass)
            return;
        MotherBrainGlassLastRoomArgument = slot.HitCounter;
        _motherBrainGlassSlot = null;
        // Deleted slots are reusable. Clear the family discriminator now so a
        // later ordinary PLM cannot accidentally inherit glass-only instruction dispatch.
        slot.Header = 0;
        slot.PreInstruction = PlmPreInstructions.Inert;
    }
}
