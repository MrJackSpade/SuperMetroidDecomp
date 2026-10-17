using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Translates the pose a cinematic sprite shows back to the cartridge spritemap its native list would have selected,
/// so the ROM oracles that walk the native lists can compare an actor's frame with theirs.
/// </summary>
internal static class CinematicSpritePoseSpritemaps
{
    /// <summary>
    /// The spritemap the native list selects for <paramref name="pose"/>; zero, as the native actor's spritemap word
    /// holds, when the actor has shown no pose yet or has been deleted.
    /// </summary>
    internal static ushort Of(CinematicSpritePose? pose) =>
        pose is { } shown ? CinematicSpritePoseDefinitions.Spritemap(shown) : (ushort)0;
}
