namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>The state of one Fune, which spits each time its cooldown runs out.</summary>
public sealed class FuneEnemyState : FuneNamiheEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal FuneEnemyState(FuneAction action, FuneSpawnSettings settings)
    {
        Action = action;
        Settings = Ensure.NotNull(settings);
    }

    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal FuneSpawnSettings Settings { get; }

    /// <inheritdoc/>
    internal override FuneNamiheSpawnSettings SharedSettings => Settings;

    /// <summary>The waiting or spitting behaviour.</summary>
    public FuneAction Action { get; internal set; }

    /// <summary>
    /// The wait between spits, counted once each waiting update and started again when it ends: it lasts its spawn
    /// settings' cooldown time (#2045, #2476, <see cref="FuneNamiheBehavior.CooldownOf"/>).
    /// </summary>
    public UpdateCountdown Cooldown { get; internal set; } = UpdateCountdown.Ended();

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
