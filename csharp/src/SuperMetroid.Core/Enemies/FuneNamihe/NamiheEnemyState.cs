namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The state of one Namihe, which spits each time Samus comes within its spawn settings' vertical wake distance, which it
/// reads where it tests it (#2476).
/// </summary>
public sealed class NamiheEnemyState : FuneNamiheEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal NamiheEnemyState(NamiheAction action) => Action = action;

    /// <summary>The waiting or spitting behaviour.</summary>
    public NamiheAction Action { get; internal set; }

    /// <inheritdoc/>
    /// <remarks>A Namihe's mouth is four pixels lower than a Fune's.</remarks>
    internal override int MouthDrop => 4;

    /// <inheritdoc/>
    internal override RoomEnemyProjectileKind Fireball => RoomEnemyProjectileKind.NamiheFireball;

    /// <inheritdoc/>
    internal override bool IsSpitting => Action == NamiheAction.Spitting;

    /// <inheritdoc/>
    internal override FuneNamiheSegment Animation(HorizontalDirection facing) => IsSpitting
        ? facing == HorizontalDirection.Right ? FuneNamiheSegment.NamiheActiveRight : FuneNamiheSegment.NamiheActiveLeft
        : facing == HorizontalDirection.Right ? FuneNamiheSegment.NamiheIdleRight : FuneNamiheSegment.NamiheIdleLeft;

    /// <inheritdoc/>
    internal override void FinishSpit()
    {
        Action = NamiheAction.WaitForSamus;
    }
}
