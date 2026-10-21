namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The two shared bank-$A0 no-op routines an enemy header can name for its AI.</summary>
internal enum EnemyNoOpRoutine : ushort
{
    /// <summary><c>RTS_A0804B</c> at $A0:804B: returns from a short call.</summary>
    ShortReturn = 0x804b,

    /// <summary><c>RTL_A0804C</c> at $A0:804C: returns from a long call.</summary>
    LongReturn = 0x804c,
}
