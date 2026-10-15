namespace SuperMetroid.Core.Enemies;

/// <summary>One native enemy-owned <c>QueueMusic_Delayed*</c> publication.</summary>
/// <param name="Command">The lossless bank-$80 command published by the original caller.</param>
/// <param name="Delay">The validated delay before bank $80 handles the command.</param>
public readonly record struct EnemyMusicRequest(
    MusicCommand Command,
    MusicCommandDelay Delay);
