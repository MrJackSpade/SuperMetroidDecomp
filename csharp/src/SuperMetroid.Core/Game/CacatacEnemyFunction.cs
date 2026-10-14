namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A2 function pointers stored at Cacatac's <c>$0FB2,x</c>.
/// Native <c>InitAI_Cacatac</c> reads the three-word table at $A2:9F42 + 2*i
/// for parameter-one low-byte selector i = 0..2: $9FBA moves left, $9FEC
/// moves right, and $A01B is the stopped RTS while an attack animation owns
/// the actor. Other selectors are outside the authored table.
/// </summary>
public enum CacatacEnemyFunction : ushort
{
    /// <summary>$A2:9FBA, <c>Function_Cacatac_MovingLeft</c>: advances the split 16.16 left velocity, turns right below the minimum X bound, then evaluates the random attack gate.</summary>
    MovingLeft = 0x9fba,
    /// <summary>$A2:9FEC, <c>Function_Cacatac_MovingRight</c>: advances the split 16.16 right velocity, turns left at or beyond the maximum X bound, then evaluates the random attack gate.</summary>
    MovingRight = 0x9fec,
    /// <summary>$A2:A01B, <c>RTS_A2A01B</c>: performs no patrol update while attack animation commands own the actor; also the initial target for population direction selector two.</summary>
    Stopped = 0xa01b,
}
