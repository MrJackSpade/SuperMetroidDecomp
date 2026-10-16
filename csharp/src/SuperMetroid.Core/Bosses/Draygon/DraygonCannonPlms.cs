using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's wall-turret PLMs: header identification and the cannon state Draygon's grab fight drives.</summary>
internal sealed class DraygonCannonPlms : IPlmFamilySource
{
    /// <summary>Registers the DraygonCannon family, which has no step of its own: its slots take the shared path.</summary>
    public void RegisterPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Register(PlmFamily.DraygonCannon, static (_, _) => PlmStepOutcome.Shared);
    }

    /// <summary>Enemy control-word writer notified when a cannon is destroyed.</summary>
    private readonly PlmRoomBindings _bindings;

    /// <summary>Connects cannon PLM instruction handling to the active room's enemy services.</summary>
    internal DraygonCannonPlms(PlmRoomBindings bindings)
    {
        _bindings = bindings;
    }

    /// <summary>Recognizes the active right, pre-destroyed right, and left retail cannon headers.</summary>
    internal static bool IsDraygonCannonHeader(PlmHeaderId header) => header is
        PlmHeaderId.DraygonCannonFacingRight or
        PlmHeaderId.DraygonCannonFacingRightDestroyed or
        PlmHeaderId.DraygonCannonFacingLeft;

    /// <summary>Captures cannon orientation and its enemy control word, installing shootable terrain unless the cannon starts destroyed.</summary>
    internal static void SetupDraygonCannonSlot(RoomLevelData level, PlmSlot slot)
    {
        DraygonCannonOrientation orientation = slot.HeaderPointer switch
        {
            PlmHeaderId.DraygonCannonFacingRight or
                PlmHeaderId.DraygonCannonFacingRightDestroyed =>
                    DraygonCannonOrientation.Right,
            PlmHeaderId.DraygonCannonFacingLeft => DraygonCannonOrientation.Left,
            _ => throw new ArgumentOutOfRangeException(
                nameof(slot), slot.HeaderPointer, "Not a retail Draygon cannon header."),
        };

        ushort variablePointer = slot.RoomArgument;
        if (!DraygonCannonWorkRamAddresses.TryDecode(variablePointer, out _))
        {
            throw new InvalidDataException(
                $"Draygon cannon header $84:{(int)slot.HeaderPointer:X4} has invalid control " +
                $"word ${variablePointer:X4}.");
        }

        slot.Family = new DraygonCannonPlmState(variablePointer);
        if (slot.HeaderPointer == PlmHeaderId.DraygonCannonFacingRightDestroyed)
        {
            // Setup $DF4C only moves the original argument to PLM_Variable and seeds three.
            // Its list begins at the damage instruction, which owns the terrain mutation.
            slot.RoomArgument = DraygonCannonRomData.HitsToDestroy;
            slot.StartAnimation(new DraygonCannonPlmAnimation(DraygonCannonPlmProgramDefinitions.FirstProgram(orientation, destroyed: true)));
            return;
        }

        slot.RoomArgument = 0;
        slot.StartAnimation(new DraygonCannonPlmAnimation(DraygonCannonPlmProgramDefinitions.FirstProgram(orientation, destroyed: false)));
        WriteDraygonCannonTypeAndBts(
            level,
            slot.BlockIndex,
            DraygonCannonRomData.CannonCollisionWord);
        WriteDraygonCannonTypeAndBts(
            level,
            checked(slot.BlockIndex + level.Size.X),
            DraygonCannonRomData.CannonExtensionWord);
    }

    /// <summary>Accepts pending missile hits, treating a Super Missile as enough damage to reach the destruction threshold.</summary>
    internal static void RunDraygonCannonPreInstruction(PlmSlot slot)
    {
        DraygonCannonPlmState? state = slot.Family as DraygonCannonPlmState;
        if (state is null || slot.PreInstruction == 0)
            return;
        if (slot.PreInstruction != DraygonCannonRomData.MissileHitPreInstruction)
        {
            throw new InvalidDataException(
                $"Draygon cannon reached untranslated pre-instruction $84:{slot.PreInstruction:X4}.");
        }
        if (!state.HasPendingHit)
            return;

        SamusProjectileFamily family = new SamusProjectileTypeWord(slot.LoopTimer).Family;
        if (family == SamusProjectileFamily.SuperMissile)
            slot.RoomArgument = DraygonCannonRomData.SuperMissileHitCounterSeed;
        else if (family != SamusProjectileFamily.Missile)
            return;

        slot.LoopTimer = 0;
        state.HasPendingHit = false;
        slot.WakeAtLink();
    }

    /// <summary>
    /// <c>$84:8ACD</c> on the cannon's hit counter: counts a hit and reports whether it destroys the cannon. The
    /// destroying hit marks the counter spent and removes the missile wake.
    /// </summary>
    internal static bool CountHit(PlmSlot slot)
    {
        byte next = unchecked((byte)(slot.RoomArgument + 1));
        if (next < DraygonCannonRomData.HitsToDestroy)
        {
            slot.RoomArgument = next;
            return false;
        }
        slot.RoomArgument = ushort.MaxValue;
        slot.PreInstruction = 0;
        return true;
    }

    /// <summary>Disables the matching bank-$A5 cannon firing word and changes both room blocks to destroyed terrain.</summary>
    /// <summary>Destroys the cannon: disables its bank-$A5 firing word and writes its destroyed terrain.</summary>
    internal void DamageDraygonCannon(
        RoomLevelData level,
        PlmSlot slot)
    {
        DraygonCannonPlmState state = (slot.Family as DraygonCannonPlmState)
            ?? throw new InvalidOperationException("Draygon cannon damage reached a slot without cannon state.");
        (_bindings.Enemies ?? throw new InvalidOperationException(
            "Draygon cannon bytecode has no enemy-control-word writer.")).DisableDraygonCannon(
                DraygonCannonWorkRamAddresses.Decode(state.VariablePointer));
        WriteDraygonCannonTypeAndBts(
            level,
            slot.BlockIndex,
            DraygonCannonRomData.DestroyedCannonWord);
        WriteDraygonCannonTypeAndBts(
            level,
            checked(slot.BlockIndex + level.Size.X),
            DraygonCannonRomData.DestroyedCannonWord);
        state.Destroyed = true;
    }

    /// <summary>Changes a cannon block's collision type and behavior while preserving its visual tile bits.</summary>
    private static void WriteDraygonCannonTypeAndBts(
        RoomLevelData level,
        int blockIndex,
        ushort typeAndBts)
    {
        ushort original = level.GetPlmCollisionBlockByIndex(blockIndex).LevelWord;
        level.SetPlmForegroundEntry(
            blockIndex,
            new RoomLevelWord(original).WithCollisionType(new RoomLevelWord(typeAndBts).CollisionType).Raw);
        level.SetPlmBehavior(blockIndex, unchecked((byte)typeAndBts));
    }

    /// <summary>Per-slot cannon enemy control-word identity, destruction state, and queued hit flag.</summary>
    internal sealed class DraygonCannonPlmState(
        ushort variablePointer) : PlmFamilyState
    {
        /// <inheritdoc/>
        internal override PlmFamily Family => PlmFamily.DraygonCannon;

        /// <summary>Bank-$A5 word written when this physical cannon stops firing.</summary>
        public ushort VariablePointer { get; } = variablePointer;
        /// <summary>Marks that this cannon's terrain and firing control have been disabled.</summary>
        public bool Destroyed { get; set; }
        /// <summary>Indicates a projectile collision awaiting missile-family filtering.</summary>
        public bool HasPendingHit { get; set; }
    }
}
