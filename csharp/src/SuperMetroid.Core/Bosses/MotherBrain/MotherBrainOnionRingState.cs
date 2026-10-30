using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one of Mother Brain's onion rings keeps: how long it stays at her mouth before it flies.</summary>
/// <param name="attachDelay">Updates the ring stays at the mouth.</param>
internal sealed class MotherBrainOnionRingState(int attachDelay) : EnemyProjectileState
{
    /// <summary>Updates the ring stays pinned to the mouth; it flies once this has run out.</summary>
    internal int AttachDelay
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    } = Ensure.AtLeastZero(attachDelay);
}
