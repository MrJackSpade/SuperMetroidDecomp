namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may replace its main AI with its hurt behaviour while hurt.</summary>
internal interface IEnemyHurtReaction : IEnemyEntity
{
    /// <summary>
    /// Runs the enemy's hurt behaviour for this enemy frame and returns true, or returns false without running anything
    /// when the enemy has none, so the step falls through to its frozen or main AI.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    bool RunHurt(RoomEnemyFrame frame);
}
