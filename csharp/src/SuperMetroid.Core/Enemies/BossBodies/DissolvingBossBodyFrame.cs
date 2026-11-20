using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body frame that dissolves (#1652, #2554): the frame, the picture it shows and the dissolve that hides that
/// picture's pixels band by band, as Crocomire's melt images do.
/// </summary>
/// <param name="Frame">The frame as installed.</param>
/// <param name="Picture">The picture the frame shows.</param>
/// <param name="Dissolve">How the picture's pixels dissolve.</param>
public sealed record DissolvingBossBodyFrame(BossBodyFrame Frame, SpriteSheetFrame Picture, PictureDissolve Dissolve)
{
    /// <summary>The frame with each band's pixels hidden as far as <paramref name="bandSteps"/> has dissolved it.</summary>
    /// <param name="bandSteps">How many steps each band has dissolved.</param>
    public BossBodyFrame Dissolved(ReadOnlySpan<int> bandSteps) => Frame.Hiding(Picture, Dissolve.Hidden(bandSteps));
}
