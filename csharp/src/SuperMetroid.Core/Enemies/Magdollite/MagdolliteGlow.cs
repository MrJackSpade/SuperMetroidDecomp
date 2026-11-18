namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// One frame of the room's lava glow (#2343), which every Magdollite and the lava they throw show at once, named for the
/// frame it shows (#2474): where the glow's brightest colour stands among the four lava colours the Magdollite is drawn
/// with, ranked from the brightest as drawn. The glow carries its brightest colour from the brightest drawn colour to the
/// darkest and back up through the others; it plays its frames in that order, the last followed by the first
/// (<see cref="MagdolliteGlowAnimations"/>, #2439). Each frame's colours are the installed shade the import pre-colours
/// for it (<see cref="Artwork.MagdolliteShades"/>). Its values are arbitrary.
/// </summary>
public enum MagdolliteGlow
{
    /// <summary>The lava as drawn: its brightest colour brightest, each darker one darker.</summary>
    OnBrightestColour,

    /// <summary>The glow's brightest colour on the lava's darkest drawn colour, each brighter one a step darker.</summary>
    OnDarkestColour,

    /// <summary>The glow's brightest colour on the lava's second darkest drawn colour.</summary>
    OnSecondDarkestColour,

    /// <summary>The glow's brightest colour on the lava's second brightest drawn colour.</summary>
    OnSecondBrightestColour,
}
