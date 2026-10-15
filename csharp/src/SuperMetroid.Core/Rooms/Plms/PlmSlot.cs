using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One of the forty native PLM slots: header, block, instruction state and the family state it carries.</summary>
internal sealed class PlmSlot
{
    /// <summary>Samus position saved by the Brinstar plant setup (native <c>PLM_Vars</c> X, <c>PLMExtra_Vars</c> Y).</summary>
    public Position16<RoomPixels> PlantHeldPosition { get; set; }
    /// <summary>Marks whether this physical PLM slot participates in the room update.</summary>
    public bool Active { get; set; }
    /// <summary>Bank-$84 header selecting the resident PLM's setup and behavior.</summary>
    public PlmHeaderId HeaderPointer { get; set; }
    /// <summary>Index of the room block owned by this PLM.</summary>
    public int BlockIndex { get; set; }
    /// <summary>
    /// Native <c>PLM_Vars</c>. Grapple setup saves the original word; collision-bomb
    /// setup synthesizes the dimension-parent restoration word <c>$x058</c> instead.
    /// </summary>
    public ushort RestoreLevelWord { get; set; }
    /// <summary>
    /// Current bank-$84 instruction-list position for the slot's generic interpreter; zero while a typed program runs.
    /// Storing a list pointer ends any typed program, as a native list store replaces the running program.
    /// </summary>
    public ushort InstructionPointer
    {
        get;
        set
        {
            field = value;
            Animation = null;
        }
    }
    /// <summary>
    /// The typed program replacing this slot's native instruction list (#1296), or null when the bank-$84 interpreter
    /// runs <see cref="InstructionPointer"/>.
    /// </summary>
    internal PlmAnimation? Animation { get; private set; }
    /// <summary>Countdown until the slot's next instruction-list step.</summary>
    public NativeCountdown InstructionTimer { get; set; }
    /// <summary>Native bank-$84 pre-instruction pointer run before the timer/list pass.</summary>
    public ushort PreInstruction { get; set; }
    /// <summary>Native room argument word owned independently by every physical PLM slot.</summary>
    public ushort RoomArgument { get; set; }
    /// <summary>Native <c>PLM_Timers</c>, distinct from the instruction countdown.</summary>
    public ushort LoopTimer { get; set; }
    /// <summary>Native extra PLM link-instruction word used by sleeping coroutines.</summary>
    public ushort LinkInstruction { get; set; }
    /// <summary>
    /// Semantic state for one of bank $84's 63 permanent-item headers. A null value
    /// leaves this physical slot under the ordinary instruction interpreter.
    /// </summary>
    public CollectiblePlms.CollectiblePlmState? Item { get; set; }
    /// <summary>Semantic state for resident bank-$84 scroll trigger header $B703.</summary>
    public ScrollPlms.ScrollPlmState? Scroll { get; set; }
    /// <summary>Semantic state for resident yellow/green/red door-cap PLMs.</summary>
    public ColoredDoorPlmState? ColoredDoor { get; set; }
    /// <summary>Semantic state for condition-gated grey door-cap PLMs.</summary>
    public GreyDoorPlmState? GreyDoor { get; set; }
    /// <summary>Semantic owner for map/resource/save station instruction families.</summary>
    public StationPlms.StationPlmState? Station { get; set; }
    /// <summary>Marks header $B70B while its animation remains generic list execution.</summary>
    public bool IsElevatorPlatform { get; set; }
    /// <summary>Marks the resident five-block downward gate coroutine.</summary>
    public GatePlms.DownwardGatePlmState? Gate { get; set; }
    /// <summary>Semantic owner for one component of a mirrored eye-door assembly.</summary>
    public EyeDoorPlms.EyeDoorPlmState? EyeDoor { get; set; }
    /// <summary>Semantic owner for one of Draygon's three retail cannon headers.</summary>
    public DraygonCannonPlms.DraygonCannonPlmState? DraygonCannon { get; set; }
    /// <summary>Semantic owner for door-spawned Wrecked Ship treadmill PLMs.</summary>
    public WreckedShipTreadmillPlmState? Treadmill { get; set; }

    /// <summary>Runs <paramref name="animation"/> in place of any native list; the caller keeps the slot's timer.</summary>
    internal void StartAnimation(PlmAnimation animation)
    {
        InstructionPointer = 0;
        Animation = Ensure.NotNull(animation);
    }

    /// <summary>
    /// A pre-instruction's wake: the native list resumes at <see cref="LinkInstruction"/>, a typed program at the
    /// program it linked, on the next pass.
    /// </summary>
    internal void WakeAtLink()
    {
        if (Animation is { } animation)
            animation.WakeAtLink();
        else
            InstructionPointer = LinkInstruction;
        InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>
    /// A pre-instruction's wake past the sleep the slot rests on: the native list steps over its sleep instruction, a
    /// typed program over its hold, on the next pass.
    /// </summary>
    internal void WakePastSleep()
    {
        if (Animation is { } animation)
            animation.WakePastSleep();
        else
            InstructionPointer = unchecked((ushort)(InstructionPointer + 2));
        InstructionTimer = NativeCountdown.Start(1);
    }
}
