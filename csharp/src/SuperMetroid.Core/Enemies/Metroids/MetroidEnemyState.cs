using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Typed view of ordinary Metroid's six common enemy variables plus its three parallel
/// extension words. The two bank-$B4 sprite-object references are the managed equivalent
/// of native indexes <c>metroid_var_00/01</c>; the drain accumulator is <c>metroid_var_02</c>.
/// </summary>
public sealed class MetroidEnemyState
{
    /// <summary>Physical Metroid slot backing velocity, escape timer, and movement function.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Connects the Metroid's common slot to its two live outer-body sprite objects.</summary>
    internal MetroidEnemyState(
        RoomEnemySlot slot,
        RoomSpriteObjectSlot outerBodyA,
        RoomSpriteObjectSlot outerBodyB)
    {
        _slot = slot;
        OuterBodyA = outerBodyA;
        OuterBodyB = outerBodyB;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<MetroidProgram> Program { get; set; }

    /// <summary>
    /// Signed 16.16 velocity in pixels per movement update, positive right and down: variables
    /// B:A (native $0FAA:$0FA8) hold the horizontal whole:fraction words and D:C ($0FAE:$0FAC)
    /// the vertical ones.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => FixedVector<RoomPixels>.FromWords(_slot.VariableB, _slot.VariableA, _slot.VariableD, _slot.VariableC);
        internal set
        {
            _slot.VariableA = value.X.Fraction;
            _slot.VariableB = value.X.WholeWord;
            _slot.VariableC = value.Y.Fraction;
            _slot.VariableD = value.Y.WholeWord;
        }
    }

    /// <summary>Variable E; four-frame escape timer and frozen-shake countdown.</summary>
    public NativeCountdown EscapeTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Variable F at native $0FB2: zero-based movement dispatcher index 0..3 into $A3:EC09, not an AI routine pointer.</summary>
    public MetroidAiFunction Function
    {
        get => (MetroidAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Live shared-pool electricity sprite object, native selector $32 and reference $7E:7800; the Metroid owner repositions, freezes and terminates it alongside the central body.</summary>
    public RoomSpriteObjectSlot OuterBodyA { get; }
    /// <summary>Live shared-pool shell sprite object, native selector $34 and reference $7E:7802; distinct from both the electricity layer and central enemy spritemap.</summary>
    public RoomSpriteObjectSlot OuterBodyB { get; }

    /// <summary>
    /// Fractional health-drain accumulator, 0 through 65535 in 1/65536ths of one energy. A borrow from subtracting the suit-selected rate removes exactly one energy from Samus
    /// (<see cref="DrainBorrows"/>).
    /// </summary>
    public int DrainAccumulator { get; internal set; }

    /// <summary>The accumulator's range: one energy in fractional steps.</summary>
    private const int DrainFractionSteps = ushort.MaxValue + 1;

    /// <summary>
    /// Subtracts one update's drain <paramref name="rate"/> from the accumulator, wrapping below zero back into its
    /// range as the cartridge's word subtraction does, and reports whether it borrowed: a borrow drains one energy.
    /// </summary>
    /// <param name="rate">The suit-selected drain per update, in the accumulator's fractional steps.</param>
    internal bool DrainBorrows(int rate)
    {
        int remaining = DrainAccumulator - rate;
        bool borrowed = remaining < 0;
        DrainAccumulator = borrowed ? remaining + DrainFractionSteps : remaining;
        return borrowed;
    }
}
