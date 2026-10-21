namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>A pose a cinematic sprite program shows; each cinematic sprite family's artwork is keyed by the poses it draws.</summary>
/// <param name="Index">The pose's index.</param>
public readonly record struct CinematicSpritePose(int Index)
{
    /// <summary>The pose that draws nothing.</summary>
    public static CinematicSpritePose Blank { get; } = new(26);
}
