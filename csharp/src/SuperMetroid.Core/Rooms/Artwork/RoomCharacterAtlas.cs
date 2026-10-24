using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One room-character stream the import pipeline compiled from editable, palette-indexed 8x8 artwork (#1409). The room's
/// palette and metatile definitions assign the visible colors and placement. Its characters remain bytes here because
/// the OBJ character sources, the standard OBJ page and the Ceres Mode 7 map still read them.
/// </summary>
public sealed class RoomCharacterAtlas
{
    /// <summary>Owned planar 4-bpp transfer bytes for the authored room characters.</summary>
    internal readonly byte[] planar;

    /// <summary>Retains newly compiled planar bytes without exposing a mutable document buffer.</summary>
    /// <param name="planar">Whole 4-bpp characters the import pipeline compiled.</param>
    internal RoomCharacterAtlas(byte[] planar) => this.planar = planar;

    /// <summary>Read-only view of the compiled, row-major 8x8 SNES 4-bpp characters, excluding unused PNG cells.</summary>
    public ReadOnlyMemory<byte> Characters => planar;

    /// <summary>Identity of selected decoded pixels; PNG metadata and display padding do not participate.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomCharacterAtlas),
        content => content.Append("characters", planar));
}
