namespace SuperMetroid.Core.Enemies;

/// <summary>Where an enemy program's loop counter lives.</summary>
internal enum EnemyProgramCounter
{
    /// <summary>The program loads no loop counter; counter steps would count a discarded local.</summary>
    None,
    /// <summary>The counter is the enemy's timer word, as the native timer-load and countdown commands use.</summary>
    SlotTimer,
}
