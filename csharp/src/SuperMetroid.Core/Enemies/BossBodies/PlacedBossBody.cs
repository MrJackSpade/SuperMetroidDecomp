namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss's body of parts as its room shows it (#2194): the frames its parts show (<see cref="BossBodyParts"/>), placed
/// and hidden as its boss says. The boss's setup attaches it to the room's display; the body's parts and the boss each
/// keep what is theirs, and neither holds the other.
/// </summary>
/// <param name="parts">The frames the boss's parts show on its body.</param>
/// <param name="showing">Where the boss places the body and whether it shows nothing.</param>
internal sealed partial class PlacedBossBody(BossBodyParts parts, IBossBodyShowing showing) : IBossBodyDisplay
{
    /// <summary>Where the boss places the body and whether it shows nothing.</summary>
    private readonly IBossBodyShowing _showing = Ensure.NotNull(showing);

    /// <summary>The frames the boss's parts show on its body.</summary>
    internal BossBodyParts Parts { get; } = Ensure.NotNull(parts);

    /// <inheritdoc />
    BossBodyShowing? IBossBodyDisplay.Showing => Parts.IsShown ? new BossBodyShowing(Parts.Publish(), _showing.BodyPlacement) : null;

    /// <inheritdoc />
    bool IBossBodyDisplay.HidesBodyLayer => _showing.BodyHidden;

    /// <inheritdoc/>
    bool IBossBodyDisplay.HidesBackgroundLayer => _showing.HidesBackgroundLayer;
}
