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
                self.AppendPart(part);
        }

        /// <summary>Hashes calculated parts with the identical ordering and framing as stored spans.</summary>
        public void AppendEnemyParts(IReadOnlyList<SpritePart> parts)
        {
            self.Append("parts", parts.Count);
            for (int index = 0; index < parts.Count; index++)
                self.AppendPart(parts[index]);
        }

        /// <summary>Hashes one piece's visual fields in a fixed order (#2604: test code, as the import keeps no part identity).</summary>
        public void AppendPart(SpritePart part)
        {
            self.Append("x", part.Offset.X);
            self.Append("y", part.Offset.Y);
            self.Append("large", part.Large ? 1 : 0);
            self.Append("character", part.Character);
            self.Append("flip-horizontal", part.FlipsHorizontally ? 1 : 0);
            self.Append("flip-vertical", part.FlipsVertically ? 1 : 0);
            self.Append("palette", part.Palette);
            self.AppendName("depth", ActorDepthKeys.Installed.Of(part.Depth));
        }

        /// <summary>
        /// Hashes the frame <paramref name="composition"/> draws (#2604), its parts in draw order: each part's offset, size,
        /// character, mirroring, own palette row or none for a part that takes the draw's, and depth.
        /// </summary>
        public void AppendComposition(SpriteComposition composition)
        {
            SpriteFrame frame = composition.ToFrame(name: nameof(SpriteComposition), firstCharacter: 0);
            self.Append("parts", frame.Parts.Length);
            foreach (SpriteFramePart part in frame.Parts)
            {
                self.Append("x", part.Offset.X);
                self.Append("y", part.Offset.Y);
                self.Append("width", part.Size.X);
                self.Append("height", part.Size.Y);
                self.Append("character", part.Character);
                self.Append("flip-horizontal", (part.Flip & PictureFlip.Horizontal) != PictureFlip.None ? 1 : 0);
                self.Append("flip-vertical", (part.Flip & PictureFlip.Vertical) != PictureFlip.None ? 1 : 0);
                self.Append("inherit-palette", part.Palette is null ? 1 : 0);
                self.Append("palette", part.Palette ?? 0);
                self.AppendName("depth", part.Depth is { } depth ? ActorDepthKeys.Installed.Of(depth) : string.Empty);
            }
        }
    }
}
