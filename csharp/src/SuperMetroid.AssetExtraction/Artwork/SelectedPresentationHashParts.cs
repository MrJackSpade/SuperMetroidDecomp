using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Hashes sprite part compositions into an installed content identity (#1463): only the import pipeline holds parts.</summary>
internal static class SelectedPresentationHashParts
{
    /// <summary>The part framing a content identity hashes.</summary>
    extension(SelectedPresentationHash self)
    {
        /// <summary>Preserves part order, offsets, size, flips, priority, palette and character selection.</summary>
        public void AppendEnemyParts(ReadOnlySpan<SpritePart> parts)
        {
            self.Append("parts", parts.Length);
            foreach (SpritePart part in parts)
                part.AppendIdentity(self);
        }

        /// <summary>Hashes calculated parts with the identical ordering and framing as stored spans.</summary>
        public void AppendEnemyParts(IReadOnlyList<SpritePart> parts)
        {
            self.Append("parts", parts.Count);
            for (int index = 0; index < parts.Count; index++)
                parts[index].AppendIdentity(self);
        }
    }
}
