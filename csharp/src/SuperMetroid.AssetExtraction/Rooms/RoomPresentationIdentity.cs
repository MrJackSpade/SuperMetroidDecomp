using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Named identities of the room catalogs actually bound by a playable host.</summary>
public static class RoomPresentationIdentity
{
    /// <summary>
    /// Captures the named identity of the room presentation catalog actually selected by a host. How a room's blocks and
    /// backgrounds look is the block sheets' identity (#1431, #1466), which the host records with them; the room characters,
    /// colours, library background and sky tilemaps those pictures are composed from are the import pipeline's input, which
    /// no host binds (#1544).
    /// </summary>
    /// <param name="layouts">Non-null installed initial foreground/background room visual layouts.</param>
    /// <returns>A new ordinal-keyed dictionary of decoded-content identities, using installation directory names.</returns>
    /// <remarks>Includes in-place edits of the installed files, reads no files, and retains no catalog references. These component identities remain separate from cartridge/engine provenance, room collision, and active mutable room state.</remarks>
    public static IReadOnlyDictionary<string, string> Create(RoomLevelArtworks layouts) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.RoomVisualLayoutDirectoryName] = layouts.ContentIdentity,
        };
}
