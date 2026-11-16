using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What a cinematic actor shows this frame and where (#2195): the frame, its screen place and its colouring.</summary>
/// <typeparam name="TFrame">What the actor's family shows.</typeparam>
/// <param name="Shown">The frame the actor shows.</param>
/// <param name="Place">Where on the screen the frame's origin lies; left of or above the screen is negative.</param>
/// <param name="Colouring">The drawn shade of the colouring the frame's recolourable parts draw in (#1512).</param>
internal readonly record struct CinematicActorDrawing<TFrame>(TFrame Shown, Offset16<ScreenPixels> Place, PictureShade Colouring)
    where TFrame : struct;
