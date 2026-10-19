using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Development-tool instance members of <see cref="EnemyTileArtworkCatalog"/>.</summary>
internal static class EnemyTileArtworkCatalogToolingExtensions
{
    /// <summary>Development-tool instance members of <see cref="EnemyTileArtworkCatalog"/>.</summary>
    extension(EnemyTileArtworkCatalog self)
    {
        /// <summary>Uploads the complete sheet selected by a room graphics-set record.</summary>
        public void LoadTo(EnemyDefinitionId definitionPointer, int byteCount, VramRegion destination, int byteOffset)
        {
            if (!self.sheets.TryGetValue(definitionPointer, out RoomCharacterAtlas? atlas))
                throw new InvalidDataException($"Enemy ${(int)definitionPointer:X4} has no installed tile sheet.");
            if (atlas.Transfer.Length != byteCount)
                throw new InvalidDataException(
                    $"Enemy ${(int)definitionPointer:X4} requires {byteCount} tile bytes, installed sheet has {atlas.Transfer.Length}.");
            destination.Load(atlas.Transfer.Span, byteOffset);
        }
    }
}
