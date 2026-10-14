namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Literal bank-$A2 behavior-function words stored in Oum's common variable A. Keeping the
/// cartridge addresses in the enum makes debugger state directly comparable with $7E:0F92.
/// </summary>
public enum MaridiaLargeSnailEnemyFunction : ushort
{
    /// <summary>Function_Oum_Idle, $A2:CDE6: completes settling bounces, then selects an idle facing and enters rolling when Samus is strictly within 24 horizontal pixels.</summary>
    Idle = 0xcde6,
    /// <summary>Function_Oum_Rolling, $A2:CE2B: applies bounce/floor and horizontal collision movement; the preceding update's stop flag also suppresses the signed-underflow attack countdown.</summary>
    Rolling = 0xce2b,
    /// <summary>Function_Oum_Attacking, $A2:CF40: waits for instruction $CCB3's animation-finished flag, consumes it, and installs the corresponding rolling list without moving during the wait.</summary>
    Attacking = 0xcf40,
}
