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
    /// <summary>The pre-instruction run before the timer/list pass (native bank-$84 pointer <c>$1CD7,x</c>).</summary>
    internal IPlmPreInstruction PreInstruction { get; set; } = PlmPreInstructions.Inert;
    /// <summary>Native room argument word owned independently by every physical PLM slot.</summary>
    public ushort RoomArgument { get; set; }
    /// <summary>Native <c>PLM_Timers</c>, distinct from the instruction countdown.</summary>
    public int LoopTimer
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>
    /// The semantic state of the family this slot belongs to, or null while the slot runs only its native words under the
    /// shared pre-instruction, timer and instruction interpreter.
    /// </summary>
    internal PlmFamilyState? Family { get; set; }

    /// <summary>
    /// Clears every native word and the family reference, so a recycled slot cannot run its former family. Native PLM
    /// arrays hold only scalar words, so allocating an inactive slot replaces every prior discriminator; every population
    /// and runtime spawn passes through this reset before writing its own words.
    /// </summary>
    internal void Clear()
    {
        Active = false;
        PlantHeldPosition = Position16<RoomPixels>.Zero;
        HeaderPointer = 0;
        BlockIndex = 0;
        RestoreLevelWord = 0;
        InstructionPointer = 0;
        InstructionTimer = NativeCountdown.Start(0);
        PreInstruction = PlmPreInstructions.Inert;
        RoomArgument = 0;
        LoopTimer = 0;
        Family = null;
    }

    /// <summary>Runs <paramref name="animation"/> in place of any native list; the caller keeps the slot's timer.</summary>
    internal void StartAnimation(PlmAnimation animation)
    {
        InstructionPointer = 0;
        Animation = Ensure.NotNull(animation);
    }

    /// <summary>
    /// A pre-instruction's wake: a typed program resumes at the program it linked, on the next pass. Only typed programs
    /// record a link, so a slot running a native list has none and resumes at the cleared list word, as the cartridge's
    /// zero link word directs.
    /// </summary>
    internal void WakeAtLink()
    {
        if (Animation is { } animation)
            animation.WakeAtLink();
        else
            InstructionPointer = 0;
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
