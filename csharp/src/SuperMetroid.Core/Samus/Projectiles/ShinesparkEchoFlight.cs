namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A shinespark crash echo's flight out from Samus along its departure angle (#1923), until it leaves the screen.
/// </summary>
internal sealed class ShinesparkEchoFlight : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Shinespark.StepShinesparkEcho(update.Samus, update.Shot, update.Camera);
}
