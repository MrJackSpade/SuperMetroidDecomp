namespace SuperMetroid.Core.Game;

/// <summary>Constants of <c>InitAI_Evir</c> ($A8:87E0).</summary>
public static class EvirInitDefinitions
{
    /// <summary>
    /// <c>LDA.W Enemy.init1+1,Y</c> at $A8:8811: slot zero's init1 high byte, $0FB7, indexed by
    /// the speed-table byte offset in Y instead of the enemy index.
    /// </summary>
    public const int AliasedTimerAddress = 0x0fb7;
}
