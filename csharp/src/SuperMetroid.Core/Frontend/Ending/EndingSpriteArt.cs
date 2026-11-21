using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What the ending's actors are drawn from this frame (#2195): each family's pictures, and the typed texts' letters.</summary>
/// <param name="Pictures">The presentation of each sprite family's pictures, in the ending's colours.</param>
/// <param name="TextSheet">The sheet that pictures the typed texts' letters.</param>
/// <param name="Shading">The shading the screen shows a draw named in a colouring's drawn shade in.</param>
internal sealed record EndingSpriteArt(Func<EndingSpriteFamily, IIntroCinematicSpritePresentation> Pictures, SpriteSheet TextSheet,
    Func<PictureShade, PictureShading> Shading);
