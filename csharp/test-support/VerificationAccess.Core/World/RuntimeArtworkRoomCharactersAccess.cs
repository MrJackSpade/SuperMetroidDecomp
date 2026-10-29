using System.Runtime.CompilerServices;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room character catalog verification composes a runtime's VRAM image from (#1466): gameplay reads no characters, so
/// a runtime's image shows the catalog a test installed for it, or the repository installation's stock catalog, which the
/// installed block sheets were composed from.
/// </summary>
internal static class RuntimeArtworkRoomCharactersAccess
{
    /// <summary>The catalog each test installed for a runtime's images.</summary>
    private static readonly ConditionalWeakTable<RuntimeArtwork, RoomCharacterAtlases> Installed = new();

    /// <summary>The room characters a runtime's VRAM image shows.</summary>
    extension(RuntimeArtwork self)
    {
        /// <summary>The catalog a test installed for the runtime's images, or the repository installation's stock catalog.</summary>
        internal RoomCharacterAtlases RoomCharacterArt
        {
            get => Installed.TryGetValue(self, out RoomCharacterAtlases? characters) ? characters : InstalledFixtures.RoomCharacters;
            set => Installed.AddOrUpdate(self, Ensure.NotNull(value));
        }
    }
}
