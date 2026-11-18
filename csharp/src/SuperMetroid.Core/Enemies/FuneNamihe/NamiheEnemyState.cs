namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>The state of one Namihe, which spits each time Samus comes level with it.</summary>
public sealed class NamiheEnemyState : FuneNamiheEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal NamiheEnemyState(NamiheAction action) => Action = action;

    /// <summary>The waiting or spitting behaviour.</summary>
    public NamiheAction Action { get; internal set; }

    /// <summary>Strict vertical wake distance in pixels, from the spawn settings.</summary>
    public int YProximity { get; internal set; }

    /// <inheritdoc/>
    /// <remarks>A Namihe's mouth is four pixels lower than a Fune's.</remarks>
    internal override int MouthDrop => 4;

    /// <inheritdoc/>
    internal override RoomEnemyProjectileKind Fireball => RoomEnemyProjectileKind.NamiheFireball;

    /// <inheritdoc/>
    internal override bool IsSpitting => Action == NamiheAction.Spitting;

    /// <inheritdoc/>
    internal override FuneNamiheSegment Animation => IsSpitting
        ? Facing == HorizontalDirection.Right ? FuneNamiheSegment.NamiheActiveRight : FuneNamiheSegment.NamiheActiveLeft
        : Facing == HorizontalDirection.Right ? FuneNamiheSegment.NamiheIdleRight : FuneNamiheSegment.NamiheIdleLeft;

    /// <inheritdoc/>
    internal override void FinishSpit()
    {
        Action = NamiheAction.WaitForSamus;
    }
}
