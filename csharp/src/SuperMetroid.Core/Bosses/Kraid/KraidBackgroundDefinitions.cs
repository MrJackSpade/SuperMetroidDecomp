namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What Kraid shows behind the room (#1413, #1466): how far the room backdrop's fades move its colours. The layouts his
/// body sheet is composed from belong to the asset import.
/// </summary>
public static partial class KraidBackgroundDefinitions
{


    /// <summary>
    /// The shared fade counter's last value at which a step of the room backdrop's fades moves its colours (#1512): the
    /// step after it completes the fade. A fade's colours move in this many parts, one for each of its own steps after its
    /// first (#2302).
    /// </summary>
    public const int BackdropFadeLastStep = 13;
}
