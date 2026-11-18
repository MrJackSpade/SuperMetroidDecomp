namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>The state of one Fune, which spits each time its cooldown runs out.</summary>
public sealed class FuneEnemyState : FuneNamiheEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal FuneEnemyState(FuneAction action) => Action = action;

    /// <summary>The waiting or spitting behaviour.</summary>
    public FuneAction Action { get; internal set; }

    /// <summary>
    /// The count of waiting updates, reset to zero when the wait ends: it counts up to its spawn settings' cooldown time
    /// (#2045, #2476).
    /// </summary>
    public int CooldownTimer
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }

    /// <inheritdoc/>
    internal override int MouthDrop => 0;

    /// <inheritdoc/>
    internal override RoomEnemyProjectileKind Fireball => RoomEnemyProjectileKind.FuneFireball;

    /// <inheritdoc/>
    internal override bool IsSpitting => Action == FuneAction.Spitting;

    /// <inheritdoc/>
    internal override FuneNamiheSegment Animation(HorizontalDirection facing) => IsSpitting
        ? facing == HorizontalDirection.Right ? FuneNamiheSegment.FuneActiveRight : FuneNamiheSegment.FuneActiveLeft
        : facing == HorizontalDirection.Right ? FuneNamiheSegment.FuneIdleRight : FuneNamiheSegment.FuneIdleLeft;

    /// <inheritdoc/>
    internal override void FinishSpit()
    {
        Action = FuneAction.WaitForCooldown;
    }
}
