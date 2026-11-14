namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with a main AI, run when no AI state of its own is selected.</summary>
internal interface IEnemyMainAi : IEnemyEntity
{
    /// <summary>Runs the behaviour of the enemy in the slot the entity is bound to for this enemy frame.</summary>
    /// <param name="frame">The enemy frame's camera, Samus, input, room and clock.</param>
    void RunMainAi(IEnemyAiFrame frame);
}
