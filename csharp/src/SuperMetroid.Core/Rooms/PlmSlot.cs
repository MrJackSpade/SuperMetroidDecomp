using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>One of the forty native PLM slots: header, block, instruction state and the family state it carries.</summary>
internal sealed class PlmSlot
{
    /// <summary>Samus position saved by the Brinstar plant setup (native <c>PLM_Vars</c> X, <c>PLMExtra_Vars</c> Y).</summary>
    public Position16<RoomPixels> PlantHeldPosition { get; set; }
    public bool Active { get; set; }
    public PlmHeaderId HeaderPointer { get; set; }
    public int BlockIndex { get; set; }
    /// <summary>
    /// Native <c>PLM_Vars</c>. Grapple setup saves the original word; collision-bomb
    /// setup synthesizes the dimension-parent restoration word <c>$x058</c> instead.
    /// </summary>
    public ushort RestoreLevelWord { get; set; }
    public ushort InstructionPointer { get; set; }
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
}
