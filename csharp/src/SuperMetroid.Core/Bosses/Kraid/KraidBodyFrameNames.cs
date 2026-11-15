namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The names of Kraid's frames in his installed body sheet (#1466, #1515, #1540): his body below his head, the head his
/// body rises with, and his head at each mouth stage. His head shows over his body's top rows alone, so his body
/// and head are two frames side by side and neither lies over the other. His body and the head it rises with are one
/// picture each, drawn behind the sprites or, once he has grown, in front of them (#1634). The import names the frames it
/// composes with these, and Kraid shows them by them.
/// </summary>
internal static class KraidBodyFrameNames
{
    /// <summary>The frame of Kraid's body below his head.</summary>
    internal const string Body = "Body";

    /// <summary>The frame of the head Kraid's body rises with, which shows until his head shows a pose.</summary>
    internal const string RestingHead = "RestingHead";

    /// <summary>The frame of Kraid's head with his mouth closed.</summary>
    internal const string HeadMouthClosed = "HeadMouthClosed";

    /// <summary>The frame of Kraid's head as his mouth starts to open.</summary>
    internal const string HeadMouthOpening = "HeadMouthOpening";

    /// <summary>The frame of Kraid's head with his mouth opened wider.</summary>
    internal const string HeadMouthWider = "HeadMouthWider";

    /// <summary>The frame of Kraid's head with his mouth fully open.</summary>
    internal const string HeadMouthOpen = "HeadMouthOpen";

    /// <summary>The frame of Kraid's head at <paramref name="mouthStage"/>, named for how far his mouth is open (#1885).</summary>
    /// <param name="mouthStage">How far the mouth is open, from zero (closed) to <see cref="KraidHeadFrame.FullyOpenMouthStage"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mouthStage"/> is no mouth stage of his head.</exception>
    internal static string Head(int mouthStage) => mouthStage switch
    {
        0 => HeadMouthClosed,
        1 => HeadMouthOpening,
        2 => HeadMouthWider,
        KraidHeadFrame.FullyOpenMouthStage => HeadMouthOpen,
        _ => throw new ArgumentOutOfRangeException(nameof(mouthStage), mouthStage, "Kraid's head has no such mouth stage."),
    };
}
