using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Native dispatch-table indexes stored in ordinary Metroid variable F. These are indexes,
/// not bank-$A3 function addresses: main AI uses the word directly to select one of four
/// movement routines at <c>$A3:EC09</c>.
/// </summary>
public enum MetroidAiFunction : ushort
{
    /// <summary>Index 0 selects $A3:EC11, Function_Metroid_0_ChaseSamus: proportional 16.16 attraction toward Samus's X and eight pixels above her Y, capped at three pixels per update.</summary>
    Homing = 0,
    /// <summary>Index 1 selects $A3:ECDC, Function_Metroid_1_LatchOntoSamus: byte-oriented close-range seeking entered on touch before the attachment threshold is reached.</summary>
    ClosingOnSamus = 1,
    /// <summary>Index 2 selects $A3:ED8F, Function_Metroid_2_LatchedOntoSamus: hard-lock position to Samus's X and Y minus eight pixels, clearing movement velocities.</summary>
    AttachedToSamus = 2,
    /// <summary>Index 3 selects $A3:EDAB, Function_Metroid_3_BombedOffSamus: perform four displacement steps after bomb collision detaches the Metroid, then resume homing.</summary>
    PowerBombEscape = 3,
}

/// <summary>
/// One requested drop from <c>Enemy_ItemDrop_Metroid</c> at <c>$A0:B968</c>. Actual pickup
/// actors are spawned into the shared enemy-projectile pool. This empty marker counts
/// each of the five requests; it does not retain the pickup's RNG-scattered coordinates.
/// </summary>
public readonly record struct MetroidDropRequest();

/// <summary>
/// Typed view of ordinary Metroid's six common enemy variables plus its three parallel
/// extension words. The two bank-$B4 sprite-object references are the managed equivalent
/// of native indexes <c>metroid_var_00/01</c>; the drain accumulator is <c>metroid_var_02</c>.
/// </summary>
public sealed class MetroidEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal MetroidEnemyState(
        RoomEnemySlot slot,
        RoomSpriteObjectSlot outerBodyA,
        RoomSpriteObjectSlot outerBodyB)
    {
        _slot = slot;
        OuterBodyA = outerBodyA;
        OuterBodyB = outerBodyB;
    }

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
    public ushort EscapeTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
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
    /// Unsigned fractional health-drain accumulator. A borrow from subtracting the suit-
    /// selected rate removes exactly one energy from Samus.
    /// </summary>
    public ushort DrainAccumulator { get; internal set; }
}
