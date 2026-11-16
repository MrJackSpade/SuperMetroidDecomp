using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Translates the pose a cinematic sprite shows back to the cartridge spritemap its native list would have selected,
/// so the ROM oracles that walk the native lists can compare an actor's frame with theirs.
/// </summary>
internal static class CinematicSpritePoseSpritemaps
{
    /// <summary>
    /// The spritemap the native list selects for <paramref name="pose"/>; zero, as the native actor's spritemap word
    /// holds, for the blank pose or when the actor has shown no pose yet or has been deleted.
    /// </summary>
    internal static ushort Of(CinematicSpritePose? pose) =>
        pose is { } shown && shown != CinematicSpritePose.Blank ? CinematicSpritemapSources.Of(shown) : (ushort)0;

    /// <summary>
    /// Whether the native list's spritemap word <paramref name="native"/> draws <paramref name="pose"/>: zero for the blank
    /// pose or no pose, otherwise the pose's own spritemap or one that draws the same picture (the escape pod's long trail).
    /// </summary>
    internal static bool Shows(CinematicSpritePose? pose, ushort native) =>
        pose is { } shown && shown != CinematicSpritePose.Blank ? CinematicSpritemapSourcesConstants.IsDrawnBy(shown, native) : native == 0;
}
