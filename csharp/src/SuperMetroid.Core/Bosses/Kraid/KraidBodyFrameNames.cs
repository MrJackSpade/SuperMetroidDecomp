namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The names of Kraid's frames in his installed body sheet (#1466, #1515, #1540): his body below his head behind the room's
/// sprites or, once he has grown, in front of them, the head his body rises with, and his head at each mouth stage. His
/// head shows over the top rows of his body's area alone, so his body and head are two frames side by side and neither
/// lies over the other. The import names the frames it composes with these, and Kraid shows them by them.
/// </summary>
internal static class KraidBodyFrameNames
{
    /// <summary>The frame of Kraid's body below his head as <paramref name="appearance"/> shows it, or null when it is hidden.</summary>
    /// <exception cref="InvalidDataException"><paramref name="appearance"/> is undefined.</exception>
    internal static string? Of(KraidBodyAppearance appearance) => appearance switch
    {
        KraidBodyAppearance.Hidden => null,
        KraidBodyAppearance.BehindSprites => "Body",
        KraidBodyAppearance.InFrontOfSprites => "BodyInFront",
        _ => throw new InvalidDataException($"Undefined Kraid body appearance {appearance}."),
    };

    /// <summary>
    /// The frame of the head Kraid's body rises with, as <paramref name="appearance"/> shows it, which shows until his head
    /// shows a pose; null when his body is hidden.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="appearance"/> is undefined.</exception>
    internal static string? RestingHead(KraidBodyAppearance appearance) => appearance switch
    {
        KraidBodyAppearance.Hidden => null,
        KraidBodyAppearance.BehindSprites => "RestingHead",
        KraidBodyAppearance.InFrontOfSprites => "RestingHeadInFront",
        _ => throw new InvalidDataException($"Undefined Kraid body appearance {appearance}."),
    };

    /// <summary>The frame of Kraid's head at <paramref name="mouthStage"/>.</summary>
    internal static string Head(int mouthStage) => $"Head{mouthStage}";
}
