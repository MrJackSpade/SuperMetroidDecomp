namespace SuperMetroid.Core.Assets;

/// <summary>
/// The characters of one animation frame drawn over a region of the OBJ characters (#1406). An encounter's catalogued
/// characters name a frame this way, and a frame drawn over the region shows the picture of the frame its animation
/// selected. <paramref name="Characters"/> are the bytes asset extraction pictures the frame from.
/// </summary>
/// <param name="Frame">The frame.</param>
/// <param name="Characters">The installed characters the frame shows.</param>
public sealed record ArtworkFrameCharacters(ArtworkFrame Frame, ObjCharacterSource Characters) : ObjCharacterSource
{
    /// <inheritdoc />
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver) => Characters.Resolve(resolver);

    /// <summary>The frame's name, which the region's pictures are keyed by.</summary>
    public override string ToString() => Frame.ToString();
}
