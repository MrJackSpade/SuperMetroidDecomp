namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A sheet frame and the colourings the game draws it in (#1513), its standard colouring first: the frame's
/// palette-inheriting parts are pictured in the standard colouring's sprite set, and a draw in another colouring shows the
/// picture through that colouring's drawn shade.
/// </summary>
/// <typeparam name="TColouring">The screen's colouring domain.</typeparam>
/// <param name="Frame">The frame, its palette row the standard colouring's sprite set.</param>
/// <param name="Colourings">The colourings it is drawn in, the standard one first, each once.</param>
internal sealed record ColouredFrame<TColouring>(SheetFrameSource Frame, TColouring[] Colourings)
    where TColouring : struct, Enum;
