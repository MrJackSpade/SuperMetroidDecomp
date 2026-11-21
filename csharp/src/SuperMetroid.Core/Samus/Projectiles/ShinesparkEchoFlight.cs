namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A shinespark crash echo's flight out from Samus along its departure angle (#1923), until it leaves the screen.
/// </summary>
internal sealed class ShinesparkEchoFlight : SamusShotBehavior<ShinesparkEchoShotState>
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update, ShinesparkEchoShotState state) =>
        update.Shots.Shinespark.StepShinesparkEcho(update.Samus, update.Held, state, update.Camera);
}
