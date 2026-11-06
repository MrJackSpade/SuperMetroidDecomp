namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The names of Kraid's frames in his installed body sheet (#1466, #1515): his whole body behind the room's sprites or,
/// once he has grown, in front of them, and his head at each mouth stage. The import names the frames it composes with
/// these, and Kraid shows them by them.
/// </summary>
internal static class KraidBodyFrameNames
{
    /// <summary>The frame of Kraid's whole body as <paramref name="appearance"/> shows it, or null when it is hidden.</summary>
    /// <exception cref="InvalidDataException"><paramref name="appearance"/> is undefined.</exception>
    internal static string? Of(KraidBodyAppearance appearance) => appearance switch
    {
        KraidBodyAppearance.Hidden => null,
        KraidBodyAppearance.BehindSprites => "Body",
        KraidBodyAppearance.InFrontOfSprites => "BodyInFront",
        _ => throw new InvalidDataException($"Undefined Kraid body appearance {appearance}."),
    };

    /// <summary>The frame of Kraid's head at <paramref name="mouthStage"/>.</summary>
    internal static string Head(int mouthStage) => $"Head{mouthStage}";
}
