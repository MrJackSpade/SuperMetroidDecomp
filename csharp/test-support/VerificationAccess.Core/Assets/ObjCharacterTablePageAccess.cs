using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="ObjCharacterTablePage"/> members production does not use.</summary>
internal static class ObjCharacterTablePageAccess
{
    /// <summary>Exposes <see cref="ObjCharacterTablePage"/> state that only verification reads.</summary>
    extension(ObjCharacterTablePage self)
    {
        /// <summary>The page <paramref name="uploads"/> show with their artwork as <paramref name="resolver"/> resolves it now.</summary>
        /// <param name="uploads">The OBJ character table's uploads.</param>
        /// <param name="resolver">Resolves installed artwork identities to their current bytes.</param>
        /// <exception cref="InvalidDataException">An upload's source is shorter than the range it shows.</exception>
        public SpriteArtwork Page(SpriteArtworkPlacements uploads, IInstalledArtworkContent resolver)
        {
            Ensure.NotNull(uploads);
            Ensure.NotNull(resolver);
            SpriteArtworkPlacement[] current = [.. uploads.Uploads];
            var resolved = new ReadOnlyMemory<byte>[current.Length];
            for (int index = 0; index < current.Length; index++)
                resolved[index] = current[index].Source.Resolve(resolver);
            if (self.page is not null && self.composedFrom is { } previous &&
                previous.Uploads.SequenceEqual(current) && ObjCharacterTablePageConstants.SameSources(previous.Sources, resolved))
                return self.page;

            var table = new byte[SpriteArtworkPlacements.TableByteCount];
            for (int index = 0; index < current.Length; index++)
            {
                SpriteArtworkPlacement upload = current[index];
                ReadOnlyMemory<byte> bytes = resolved[index];
                if (upload.ByteCount > bytes.Length)
                    throw new InvalidDataException($"{upload.Source} has {bytes.Length} bytes; it shows {upload.ByteCount}.");
                bytes.Span[..upload.ByteCount].CopyTo(table.AsSpan(upload.FirstByte));
            }
            self.page = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), table);
            self.composedFrom = new ObjCharacterComposition(current, resolved);
            return self.page;
        }

        /// <summary>The page the shown <paramref name="looks"/> leave, each placed where the cartridge shows it, with their artwork as <paramref name="resolver"/> resolves it now.</summary>
        /// <exception cref="InvalidDataException">A look's artwork is shorter than the range it shows.</exception>
        public SpriteArtwork Page(ShownSpriteArtwork looks, IInstalledArtworkContent resolver) => self.Page(SpriteArtworkPlacements.Of(looks), resolver);
    }
}
