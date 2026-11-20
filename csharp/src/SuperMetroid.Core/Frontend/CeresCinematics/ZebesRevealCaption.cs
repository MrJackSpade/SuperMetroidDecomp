using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// One caption of the Zebes reveal (#1733, #2489): the PLANET ZEBES title or its Japanese subtitle, which hold still and
/// are deleted before the scene slides away.
/// </summary>
/// <param name="sprite">The cinematic sprite showing the caption: its position and animation.</param>
internal sealed class ZebesRevealCaption(CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> sprite)
{
    /// <summary>The cinematic sprite showing the caption: its position and animation.</summary>
    internal CinematicActor<CeresAnimation, CinematicSpritePose, CeresCue> Sprite { get; } = Ensure.NotNull(sprite);
}
