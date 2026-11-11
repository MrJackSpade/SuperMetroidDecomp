namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The names of Kraid's frames in his installed body sheet (#1466, #1515, #1540): his body below his head, the head his
/// body rises with, and his head at each mouth stage. His head shows over the top rows of his body's area alone, so his body
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

    /// <summary>The frame of Kraid's head at <paramref name="mouthStage"/>.</summary>
    internal static string Head(int mouthStage) => $"Head{mouthStage}";
}
