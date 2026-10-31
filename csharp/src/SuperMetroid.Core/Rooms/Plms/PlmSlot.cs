using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One of the room's PLM slots: header, block, instruction state and the family state it carries.</summary>
internal sealed class PlmSlot
{
    /// <summary>Samus position saved by the Brinstar plant setup.</summary>
    public Position16<RoomPixels> PlantHeldPosition { get; set; }
    /// <summary>
    /// Whether this slot holds a PLM and participates in the room update. Only <see cref="PlmSlotTable"/>'s
    /// spawn operations claim a slot; <see cref="Delete"/> and <see cref="Clear"/> free it.
    /// </summary>
    public bool Active { get; private set; }
    /// <summary>The PLM the slot holds, selecting its setup and behavior.</summary>
    public PlmHeaderId Header { get; set; }
    /// <summary>Index of the room block owned by this PLM.</summary>
    public int BlockIndex { get; set; }
    /// <summary>
    /// The terrain the slot restores at its block. Grapple setup saves the original block; collision-bomb setup
    /// saves the dimension parent instead.
    /// </summary>
    public RoomTerrainBlock RestoreTerrain { get; set; } = PlmSlotDefinitions.ClearedRestoreTerrain;
    /// <summary>The typed program the slot runs (#1296), or null for a slot a family step drives or a free slot.</summary>
    internal PlmAnimation? Animation { get; private set; }
    /// <summary>Countdown until the slot's next instruction-list step.</summary>
    public WrappingCountdown InstructionTimer { get; set; }
    /// <summary>The pre-instruction run before the timer/list pass.</summary>
    internal IPlmPreInstruction PreInstruction { get; set; } = PlmPreInstructions.Inert;
    /// <summary>What the slot's PLM was placed with, or the state its setup or opening left there; null for none.</summary>
    public PlmRoomArgument? Argument { get; set; }
    /// <summary>The hits a Draygon cannon or Mother Brain's glass has taken.</summary>
    public int HitCounter { get; set; }
    /// <summary>
    /// The slot's loop countdown, distinct from the instruction countdown: a counted program loop repeats until it
    /// reaches zero. A downward gate keeps its trigger in it (nonzero once shot), and the Speed Booster escape keeps the
    /// lavaquake stage it has reached, each stepped by its own owner.
    /// </summary>
    public WrappingCountdown LoopTimer { get; set; }
    /// <summary>
    /// The projectile hit on the slot's trigger block that its pre-instruction has not yet consumed, or null for none.
    /// The cartridge keeps the hit in the loop counter's storage; no PLM that reads hits runs a counted loop while its
    /// block still triggers (the eye door turns its block into a blue door before its counted opening flashes), so the
    /// two are kept apart.
    /// </summary>
    public ResidentPlmProjectileHit? ProjectileHit { get; set; }
    /// <summary>
    /// The semantic state of the family this slot belongs to, or null while the slot runs only its shared state under the
    /// shared pre-instruction, timer and instruction interpreter.
    /// </summary>
    internal PlmFamilyState? Family { get; set; }

    /// <summary>
    /// Clears every field and the family reference, so a recycled slot cannot run its former family. Claiming a free slot
    /// replaces every prior discriminator; every population and runtime spawn passes through this reset before setting its
    /// own state.
    /// </summary>
    internal void Clear()
    {
        Active = false;
        PlantHeldPosition = Position16<RoomPixels>.Zero;
        Header = 0;
        BlockIndex = 0;
        RestoreTerrain = PlmSlotDefinitions.ClearedRestoreTerrain;
        Animation = null;
        InstructionTimer = WrappingCountdown.Start(0);
        PreInstruction = PlmPreInstructions.Inert;
        Argument = null;
        HitCounter = 0;
        LoopTimer = WrappingCountdown.Start(0);
        ProjectileHit = null;
        Family = null;
    }

    /// <summary>
    /// Claims this free slot for a new PLM: every field is reset and the slot is active. Only <see cref="PlmSlotTable"/>'s
    /// spawn operations call it, which analyzer SME6303 enforces.
    /// </summary>
    internal void Claim()
    {
        if (Active)
            throw new InvalidOperationException("An occupied PLM slot cannot be claimed.");
        Clear();
        Active = true;
    }

    /// <summary>
    /// Deletes the PLM, as the delete instruction does: only occupancy is cleared, and the other fields stay until the
    /// slot is claimed again.
    /// </summary>
    internal void Delete() => Active = false;

    /// <summary>Runs <paramref name="animation"/>; the caller keeps the slot's timer.</summary>
    internal void StartAnimation(PlmAnimation animation) => Animation = Ensure.NotNull(animation);

    /// <summary>A pre-instruction's wake: the typed program resumes at the program it linked, on the next pass.</summary>
    internal void WakeAtLink()
    {
        (Animation ?? throw new InvalidOperationException("A PLM woke at its link without a typed program.")).WakeAtLink();
        InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>A pre-instruction's wake past the sleep the slot rests on: the typed program steps over its hold, on the next pass.</summary>
    internal void WakePastSleep()
    {
        (Animation ?? throw new InvalidOperationException("A PLM woke past its sleep without a typed program.")).WakePastSleep();
        InstructionTimer = WrappingCountdown.Start(1);
    }
}
