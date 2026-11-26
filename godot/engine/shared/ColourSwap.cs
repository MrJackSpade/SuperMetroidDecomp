using Godot;

namespace Engine.Shared;

/// <summary>
/// One colour of a picture shown as another: every pixel of exactly the first is drawn in the second. The second
/// may be see-through, wholly or in part: the pixels are then drawn so, which is how a thing fades in or out a colour
/// at a time. Where a picture has two parts of one colour that are changed differently, the change is of one region
/// of it, which the picture's map of regions tells apart (<see cref="Engine.Content.ContentLibrary.LoadTexture"/>).
/// </summary>
/// <param name="From">The colour as the picture has it; how see-through it is plays no part.</param>
/// <param name="To">The colour it is shown as, and how opaque.</param>
/// <param name="Region">The region of the picture the change holds in, as its map of regions numbers it; <see cref="Everywhere"/> for wherever the colour is drawn.</param>
public readonly record struct ColourSwap(Color From, Color To, int Region = ColourSwap.Everywhere)
{
    /// <summary>The region of a change that holds wherever its colour is drawn.</summary>
    public const int Everywhere = -1;
}
