namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The names of Kraid's frames in his installed body sheet (#1466): his upper and lower body, behind the room's sprites
/// or, once he has grown, in front of them, and his head at each mouth stage. The import names the frames it composes
/// with these, and Kraid draws them by them.
/// </summary>
internal static class KraidBodyFrameNames
{
    /// <summary>The frame of <paramref name="half"/> of Kraid's body as <paramref name="appearance"/> shows it, or null when it is hidden.</summary>
    /// <exception cref="InvalidDataException"><paramref name="appearance"/> or <paramref name="half"/> is undefined.</exception>
    internal static string? Of(KraidBodyAppearance appearance, KraidBodyHalf half)
    {
        string halfName = half switch
        {
            KraidBodyHalf.Upper => "Upper",
            KraidBodyHalf.Lower => "Lower",
            _ => throw new InvalidDataException($"Undefined Kraid body half {half}."),
        };
        return appearance switch
        {
            KraidBodyAppearance.Hidden => null,
            KraidBodyAppearance.BehindSprites => halfName,
            KraidBodyAppearance.InFrontOfSprites => halfName + "InFront",
            _ => throw new InvalidDataException($"Undefined Kraid body appearance {appearance}."),
        };
    }

    /// <summary>The frame of Kraid's head at <paramref name="mouthStage"/>.</summary>
    internal static string Head(int mouthStage) => $"Head{mouthStage}";
}
