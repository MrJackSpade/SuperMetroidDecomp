namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A shinespark crash echo's flight out from Samus along its departure angle (#1923), until it leaves the screen.
/// </summary>
/// <param name="state">The state the shot's kind keeps.</param>
internal sealed class ShinesparkEchoFlight(ShinesparkEchoShotState state) : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override SamusShotKindState KindState => state;

    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Shinespark.StepShinesparkEcho(update.Samus, update.Shot, state, update.Camera);
}
