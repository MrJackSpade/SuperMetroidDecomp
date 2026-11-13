namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Which stage of a figure-eight's speed change Phantoon's body is in. Each pass runs <see cref="EasingIn"/>, then
/// <see cref="Surging"/>, then <see cref="Settling"/>, and back to <see cref="EasingIn"/>; the returning half mirrors the
/// forward half's speed changes.
/// </summary>
public enum PhantoonFigureEightStage
{
    /// <summary>Changing speed by the slow step until the slow cap, where the surge begins.</summary>
    EasingIn,

    /// <summary>Changing speed by the fast step until the fast cap, where the body starts settling.</summary>
    Surging,

    /// <summary>Changing speed back by the fast step until the limit, where the pass eases in again.</summary>
    Settling,
}
