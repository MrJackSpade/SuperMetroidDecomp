namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The state of an Evir's reusable spit: held at the body's mouth, aimed at Samus, or growing back.</summary>
public sealed class EvirSpitState : EvirPartState
{
    /// <summary>Starts the state of a spit in <paramref name="action"/> whose body, two places before it, is <paramref name="body"/>.</summary>
    /// <param name="action">What the spit starts doing.</param>
    /// <param name="body">The body's place.</param>
    /// <param name="bodyState">The body's state as the spit found it.</param>
    internal EvirSpitState(EvirSpitAction action, RoomEnemySlot body, EvirBodyState bodyState)
    {
        Action = action;
        Body = Ensure.NotNull(body);
        BodyState = Ensure.NotNull(bodyState);
    }

    /// <summary>The place of the spit's body, which its room lists two places before it.</summary>
    internal RoomEnemySlot Body { get; }

    /// <summary>
    /// The body's state as the spit found it when it was placed. The spit keeps reaching it after the body has left its
    /// place (#1543), as the arms do (<see cref="EvirArmsState.BodyState"/>).
    /// </summary>
    internal EvirBodyState BodyState { get; }

    /// <summary>What the spit does each update.</summary>
    public EvirSpitAction Action { get; internal set; }

    /// <summary>
    /// The regenerating spit's horizontal offset from the body's mouth, in room pixels: eight pixels away from the
    /// body's installed facing when regeneration starts, then one pixel back towards the mouth per animation step.
    /// </summary>
    public Offset16<RoomPixels> RegenerationOffset { get; internal set; }

    /// <summary>The spit's velocity per AI update, calculated from its launch angle.</summary>
    public FixedVector<RoomPixels> ProjectileVelocity { get; internal set; }

    /// <summary>Whether the spit is in flight; flight selects the normal projectile animation.</summary>
    public bool InFlight { get; internal set; }

    /// <summary>Whether the spit is growing back at the mouth: set when distant flight ends and cleared by the regeneration animation's last step.</summary>
    public bool Regenerating { get; internal set; }
}
