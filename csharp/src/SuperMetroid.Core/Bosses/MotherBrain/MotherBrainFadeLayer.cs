namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Which colouring of Mother Brain's encounter a fade's images show on (#2553): an image replaces the last image its own
/// fade's layer showed, so a layer can only show images of the fades that colour it.
/// </summary>
public enum MotherBrainFadeLayer
{
    /// <summary>Her brain's fake-death fade to grey and its revival.</summary>
    Brain,

    /// <summary>Her whole body's drain to grey by the Baby.</summary>
    BodyDrain,

    /// <summary>Her whole body's revival from the drain's grey.</summary>
    BodyRevival,

    /// <summary>Her whole body's death fade.</summary>
    BodyDeath,

    /// <summary>Her detached brain's corpse fade.</summary>
    Corpse,

    /// <summary>Her room's lights recovering after the Baby's death.</summary>
    RoomLights,

    /// <summary>The Baby's fade to black.</summary>
    Baby,
}
