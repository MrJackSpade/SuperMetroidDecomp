using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's glass PLM ($D6DE): its setup, shatter projectiles and the room argument it keeps after deletion.</summary>
internal sealed class MotherBrainGlassPlms
{
    /// <summary>Boss and event services used by the glass PLM's conditional instructions.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Physical PLM slots used to retain the glass argument across deletion.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Room sound queue receiving the glass-shatter effect.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Binds glass instruction handling to the room's PLM slots, event services, and sound queue.</summary>
    internal MotherBrainGlassPlms(PlmRoomBindings bindings, PlmSlotTable slotTable, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>Only pre-instruction accepted from the installed Mother Brain glass header.</summary>
    /// <summary>Four shard spawns emitted by the most recent shatter instruction.</summary>
    private readonly List<MotherBrainGlassProjectileRequest>
        _motherBrainGlassProjectileRequests = new();
    /// <summary>Physical slot of the live glass PLM, or minus one after deletion.</summary>
    private int _motherBrainGlassSlotIndex = -1;
    /// <summary>Whether the current room loaded the glass PLM before head damage can update it.</summary>
    internal bool _motherBrainGlassWasLoaded;
    /// <summary>Last shot count retained when the glass PLM's physical slot has been deleted.</summary>
    internal ushort _motherBrainGlassLastRoomArgument;

    /// <summary>Shard actors requested by the most recent PLM handler pass.</summary>
    public IReadOnlyList<MotherBrainGlassProjectileRequest> MotherBrainGlassProjectileRequests =>
        _motherBrainGlassProjectileRequests;

    /// <summary>Runs setup $D5F6 against the already allocated highest room slot.</summary>
    internal void SetupMotherBrainGlassSlot(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        RoomPlmPopulationRecord record,
        PlmSlot slot)
    {
        int physicalSlot = Array.IndexOf(_slotTable._slots, slot);
        if (record.RecordIndex != 0 || physicalSlot != _slotTable._slots.Length - 1 ||
            record.Block != new Position16<RoomBlocks>(9, 5) || record.RoomArgument != MotherBrainGlassBlockDefinitions.GlassRoomArgument)
        {
            throw new InvalidDataException(
                $"Mother Brain glass population diverged: record={record.RecordIndex}, " +
                $"slot={physicalSlot}, block=({record.Block.X},{record.Block.Y}), " +
                $"argument=${record.RoomArgument:X4}.");
        }

        slot.StartAnimation(new MotherBrainGlassPlmAnimation());
        slot.RoomArgument = 0;
        RoomCollisionBlock original = level.GetCollisionBlockByIndex(slot.BlockIndex);
        ushort glassLevelWord = unchecked((ushort)((original.LevelWord & MotherBrainGlassBlockDefinitions.TileNumberBits) | MotherBrainGlassBlockDefinitions.GlassBlockType));
        level.SetForegroundEntry(slot.BlockIndex, glassLevelWord);
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);
        streamer.SetLevelEntry(slot.BlockIndex, glassLevelWord);

        _motherBrainGlassSlotIndex = physicalSlot;
        _motherBrainGlassRoomWidth = level.Size.X;
        _motherBrainGlassWasLoaded = true;
        _motherBrainGlassLastRoomArgument = 0;
    }

    /// <summary>
    /// Implements Mother Brain head shot AI's direct <c>INC $1DC7</c>. This is intentionally
    /// separate from the block-hit pre-instruction: a missile can overlap the brain record
    /// before its terrain probe reaches the glass block, and native code counts that hit
    /// immediately in the enemy collision pass.
    /// </summary>
    public void IncrementMotherBrainGlassRoomArgument()
    {
        if (!_motherBrainGlassWasLoaded)
        {
            throw new InvalidOperationException(
                "Mother Brain head damage requires loaded room PLM $D6DE.");
        }

        if (TryGetMotherBrainGlassSlot(out PlmSlot? slot))
            slot.RoomArgument = unchecked((ushort)(slot.RoomArgument + 1));
        else
            _motherBrainGlassLastRoomArgument = unchecked((ushort)(
                _motherBrainGlassLastRoomArgument + 1));
    }

    /// <summary>Clears shard spawn requests before processing the next room frame.</summary>
    internal void BeginMotherBrainGlassFrame() => _motherBrainGlassProjectileRequests.Clear();

    /// <summary>Discards cached glass slot, shot count, room dimensions, and pending shard requests.</summary>
    internal void ResetMotherBrainGlassState()
    {
        _motherBrainGlassProjectileRequests.Clear();
        _motherBrainGlassSlotIndex = -1;
        _motherBrainGlassWasLoaded = false;
        _motherBrainGlassLastRoomArgument = 0;
        _motherBrainGlassRoomWidth = 0;
    }

    /// <summary>Counts missile or super-missile block hits through the installed glass pre-instruction.</summary>
    internal static void RunMotherBrainGlassPreInstruction(PlmSlot slot)
    {
        SamusProjectileFamily family = new SamusProjectileTypeWord(slot.LoopTimer).Family;
        if (family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
            slot.RoomArgument = unchecked((ushort)(slot.RoomArgument + 1));
        slot.LoopTimer = 0;
    }
    /// <summary>Whether Mother Brain, the area boss, is dead.</summary>
    internal bool IsBossDead() => _bindings.Bosses?.IsDefeated(BossBits.AreaBoss) == true;

    /// <summary>Whether the glass was destroyed on an earlier visit.</summary>
    internal bool IsGlassDestroyed() => _bindings.Events?.Has(EventNumber.MotherBrainGlassDestroyed) == true;

    /// <summary>
    /// <c>$84:D30B</c>: queues the shatter sound and spawns a burst of four shard projectiles at the glass's block.
    /// </summary>
    internal void SpawnShards(PlmSlot slot, IReadOnlyList<MotherBrainGlassShard> shards)
    {
        _sounds._soundRequests.Add(_sounds.CreateSoundRequest(RoomPlmSounds.MotherBrainGlassShattering, MaximumQueued: ShatterSoundMaximumQueued));
        // The native fragment spawn carries both block coordinates as bytes.
        Position16<RoomBlocks> glassBlock = new(checked((byte)(slot.BlockIndex % _motherBrainGlassRoomWidth)),
            checked((byte)(slot.BlockIndex / _motherBrainGlassRoomWidth)));
        foreach (MotherBrainGlassShard shard in shards)
            _motherBrainGlassProjectileRequests.Add(new MotherBrainGlassProjectileRequest(
                (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard, (ushort)shard, glassBlock));
    }

    /// <summary>Sets the glass-destroyed event.</summary>
    internal void SetGlassDestroyed() =>
        (_bindings.Events ?? throw new InvalidOperationException("Mother Brain glass has no event writer.")).Set(EventNumber.MotherBrainGlassDestroyed);

    /// <summary>The shatter sound's native queue cap.</summary>
    private const int ShatterSoundMaximumQueued = 15;

    /// <summary>Room width in blocks used to turn the glass block index into shard spawn coordinates.</summary>
    private int _motherBrainGlassRoomWidth;

    /// <summary>Finds the original live glass slot without accepting a later PLM that reused its index.</summary>
    internal bool TryGetMotherBrainGlassSlot([NotNullWhen(true)] out PlmSlot? slot)
    {
        if ((uint)_motherBrainGlassSlotIndex < (uint)_slotTable._slots.Length)
        {
            PlmSlot candidate = _slotTable._slots[_motherBrainGlassSlotIndex];
            if (candidate.Active && candidate.HeaderPointer == PlmHeaderId.MotherBrainGlass)
            {
                slot = candidate;
                return true;
            }
        }
        slot = null;
        return false;
    }

    /// <summary>Retains the glass hit count and clears the deleted slot's glass-only instruction identity.</summary>
    internal void OnPlmDeleted(PlmSlot slot)
    {
        if (slot.HeaderPointer != PlmHeaderId.MotherBrainGlass)
            return;
        _motherBrainGlassLastRoomArgument = slot.RoomArgument;
        _motherBrainGlassSlotIndex = -1;
        // Deleted physical slots are reusable. Clear the family discriminator now so a
        // later ordinary PLM cannot accidentally inherit glass-only instruction dispatch.
        slot.HeaderPointer = 0;
        slot.PreInstruction = PlmPreInstructions.Inert;
    }
}
