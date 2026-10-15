using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Typed view of the six common enemy variables reused by both halves of a Powamp. The ROM
/// gives the words different meanings according to population parameter one: a nonzero
/// parameter is the balloon, while zero is the immediately following moving body.
/// </summary>
public sealed class PowampEnemyState
{
    /// <summary>Shared enemy slot holding either the balloon's or the body's reused variable words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects the live Powamp slot into its typed variable view.</summary>
    internal PowampEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<PowampProgram> Program { get; set; }

    /// <summary>Whether population parameter one is nonzero, selecting the balloon interpretation of shared variables; zero selects the immediately following moving body.</summary>
    public bool IsBalloon => _slot.Parameter1 != 0;

    // Body meanings for variables A/B. Together these are a signed 16.16 displacement.
    /// <summary>Body variable A, the signed high word of vertical pixels-per-AI-update displacement; negative values move upward, and this word aliases <see cref="BalloonSpawn"/>'s X on balloon slots.</summary>
    public ushort YVelocity { get => _slot.VariableA; internal set => _slot.VariableA = value; }
    /// <summary>Body variable B, the low word of vertical displacement in 1/65536-pixel units, combined with <see cref="YVelocity"/> as signed 16.16; aliases <see cref="BalloonSpawn"/>'s Y on balloon slots.</summary>
    public ushort YSubvelocity { get => _slot.VariableB; internal set => _slot.VariableB = value; }

    // Balloon meanings for the same physical words.
    /// <summary>Balloon variables A/B, the original room position: X is both halves' horizontal wiggle center and Y feeds the body's resting and rise-target calculations; aliases body <see cref="YVelocity"/> and <see cref="YSubvelocity"/>.</summary>
    public Position16<RoomPixels> BalloonSpawn
    {
        get => new(_slot.VariableA, _slot.VariableB);
        internal set
        {
            _slot.VariableA = value.X;
            _slot.VariableB = value.Y;
        }
    }

    /// <summary>Body variable C, the zero-based index 0-11 into the signed three-pixel wiggle wave; phases zero and six are centered.</summary>
    public ushort WiggleIndex { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    /// <summary>Body variable D, an AI-update countdown reset to five for each horizontal wiggle offset; aliases balloon <see cref="BalloonGrappleTravelDistance"/>.</summary>
    public NativeCountdown WiggleTimer { get => NativeCountdown.FromRaw(_slot.VariableD); internal set => _slot.VariableD = value.RawValue; }
    /// <summary>Balloon variable D, copied from population parameter two; room-pixel distance subtracted from the balloon's spawn Y to obtain the grappled body target height.</summary>
    public ushort BalloonGrappleTravelDistance { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    /// <summary>Body variable E, decremented once per relevant main-AI invocation; initialized to 60 for rest, ten for inflation/deflation, or 32 for death, and expires at zero or signed underflow.</summary>
    public NativeCountdown FunctionTimer { get => NativeCountdown.FromRaw(_slot.VariableE); internal set => _slot.VariableE = value.RawValue; }
    /// <summary>Variable F at first-slot WRAM $0FB2, interpreted as the bank-$A8 main-AI function pointer; balloon slots retain the no-op entry.</summary>
    public PowampEnemyFunction Function
    {
        get => (PowampEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
