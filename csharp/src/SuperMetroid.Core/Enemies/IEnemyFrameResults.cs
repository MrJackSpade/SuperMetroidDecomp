namespace SuperMetroid.Core.Enemies;

/// <summary>The last enemy pass's results, for readers after the frame.</summary>
internal interface IEnemyFrameResults
{
    /// <summary>The results of the last pass, or of none since the room loaded.</summary>
    EnemyFrameResult LastEnemyFrame { get; }
}
