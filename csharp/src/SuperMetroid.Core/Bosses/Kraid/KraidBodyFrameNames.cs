namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The names of Kraid's frames in his installed body sheet (#1466): his upper and lower body, behind the room's sprites
/// or, once he has grown, in front of them, and his head at each mouth stage. The import names the frames it composes
/// with these, and Kraid draws them by them.
/// </summary>
internal static class KraidBodyFrameNames
{
    /// <summary>The frame of <paramref name="half"/> of Kraid's body as <paramref name="page"/> shows it, or null for a blank page.</summary>
    /// <exception cref="InvalidDataException"><paramref name="page"/> or <paramref name="half"/> is undefined.</exception>
    internal static string? Of(KraidBodyPage page, KraidBodyHalf half)
    {
        string halfName = half switch
        {
            KraidBodyHalf.Upper => "Upper",
            KraidBodyHalf.Lower => "Lower",
            _ => throw new InvalidDataException($"Undefined Kraid body half {half}."),
        };
        return page switch
        {
            KraidBodyPage.Blank => null,
            KraidBodyPage.Body => halfName,
            KraidBodyPage.PriorityBody => halfName + "InFront",
            _ => throw new InvalidDataException($"Undefined Kraid body page {page}."),
        };
    }

    /// <summary>The frame of Kraid's head at <paramref name="mouthStage"/>.</summary>
    internal static string Head(int mouthStage) => $"Head{mouthStage}";
}
