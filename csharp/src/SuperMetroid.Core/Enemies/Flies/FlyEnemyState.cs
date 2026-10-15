using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// Typed debugger view of the six private words shared by Mellow, Mella, and Memu. Every
/// property remains backed by the common enemy slot so debugger state matches WRAM A-F.
/// </summary>
public sealed class FlyEnemyState
{
    /// <summary>Physical fly-family enemy slot holding movement, target, and AI words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Mellow-family private words from the occupied enemy slot.</summary>
    internal FlyEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the fly's typed flight loop (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<FlyProgram> Program { get; set; }

    /// <summary>Native <c>Flies.retreatTimer</c> in variable A at <c>$0FA8,x</c>; counts up per attack update, down through signed underflow during retreat, and then down from 24 to gate another idle attack.</summary>
    public NativeCountdown RetreatTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableA);
        internal set => _slot.VariableA = value.RawValue;
    }

    /// <summary>
    /// Raw signed 8.8 attack/retreat velocity in room pixels per actor update: X in variable B
    /// at <c>$0FAA,x</c>, sampled from Samus's attack-start angle and retained during retreat;
    /// Y in variable C at <c>$0FAC,x</c>, whose sign reverses when the saved target Y is crossed.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get => new(new SnesSignedEightEight(_slot.VariableB), new SnesSignedEightEight(_slot.VariableC));
        internal set
        {
            _slot.VariableB = value.X.RawValue;
            _slot.VariableC = value.Y.RawValue;
        }
    }

    /// <summary>Samus's whole room-pixel Y coordinate captured when the attack begins, in variable D at <c>$0FAE,x</c>; determines the turnaround rather than tracking Samus's later movement.</summary>
    public ushort TargetYPosition
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>
    /// Even byte offset into the 256-entry sine/cosine table. The idle routines retain
    /// the native nine-bit range because they add/subtract $20 before masking with $1FF.
    /// </summary>
    public ushort Angle
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Current indirect bank-$A2 AI address, native <c>Flies.function</c> in variable F at <c>$0FB2,x</c>; reads and writes the owning enemy slot directly.</summary>
    public FlyEnemyFunction Function
    {
        get => (FlyEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
