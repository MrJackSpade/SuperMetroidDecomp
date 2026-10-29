using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Development-tool instance members of <see cref="EnemyTileSourceArtwork"/>.</summary>
internal static class EnemyTileArtworkCatalogToolingExtensions
{
    /// <summary>Development-tool instance members of <see cref="EnemyTileSourceArtwork"/>.</summary>
    extension(EnemyTileSourceArtwork self)
    {
        /// <summary>Uploads the complete sheet selected by a room graphics-set record.</summary>
        public void LoadTo(EnemyDefinitionId definitionPointer, int byteCount, VramRegion destination, int byteOffset)
        {
            if (!self.TryGetSheet(definitionPointer, out RoomCharacterAtlas? atlas))
                throw new InvalidDataException($"Enemy {definitionPointer} has no installed tile sheet.");
            if (atlas.Characters.Length != byteCount)
                throw new InvalidDataException(
                    $"Enemy {definitionPointer} requires {byteCount} tile bytes, installed sheet has {atlas.Characters.Length}.");
            destination.Load(atlas.Characters.Span, byteOffset);
        }
    }
}
