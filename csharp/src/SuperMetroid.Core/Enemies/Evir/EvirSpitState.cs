namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The state of an Evir's reusable spit: held at the body's mouth, aimed at Samus, or growing back.</summary>
public sealed class EvirSpitState : EvirPartState
{
    /// <summary>Starts the state of a spit in <paramref name="action"/> whose body, two places before it, is <paramref name="body"/>.</summary>
    /// <param name="action">What the spit starts doing.</param>
    /// <param name="body">The body's place.</param>
    internal EvirSpitState(EvirSpitAction action, RoomEnemyPlace body)
    {
        Action = action;
        Body = Ensure.NotNull(body);
    }

    /// <summary>
    /// The place of the spit's body, which its room lists two places before it. The spit reads the body there after it has
    /// left its place, as the arms do (<see cref="EvirArmsState.Body"/>).
    /// </summary>
    internal RoomEnemyPlace Body { get; }

    /// <summary>
    /// What the spit does each update: held at the mouth, in flight, or growing back from distant flight until the
    /// regeneration animation's last step.
    /// </summary>
    public EvirSpitAction Action { get; internal set; }

    /// <summary>
    /// The regenerating spit's horizontal offset from the body's mouth, in room pixels: <see cref="EvirMotionDefinitions.RegenerationStartDistance"/> away from the
    /// body's installed facing when regeneration starts, then one pixel back towards the mouth per animation step.
    /// </summary>
    public Offset16<RoomPixels> RegenerationOffset { get; internal set; }

    /// <summary>The spit's velocity per AI update, calculated from its launch angle.</summary>
    public FixedVector<RoomPixels> ProjectileVelocity { get; internal set; }
}
