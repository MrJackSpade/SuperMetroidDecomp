namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose animation is a typed animation the shared enemy animation yields to.</summary>
internal interface IEnemyTypedAnimation : IEnemyEntity
{
    /// <summary>Advances the occupant's typed animation for this enemy frame.</summary>
    /// <param name="context">The enemy frame's Samus, room, camera and input.</param>
    void AdvanceTypedAnimation(EnemyAnimationCueContext context);
}
