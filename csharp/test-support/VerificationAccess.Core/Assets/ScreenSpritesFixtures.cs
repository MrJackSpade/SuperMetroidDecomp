using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Frontend sprite entries as tests compare them (#1358): a blank screen to place parts into, and the OAM records a
/// finished frame's one-part draws stand for, so native spritemap comparisons keep their byte form.
/// </summary>
internal static class ScreenSpritesFixtures
{
    /// <summary>Sprite entries over a blank 512-character OBJ page, for checks of placement only.</summary>
    internal static ScreenSprites Blank() =>
        new(new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.FrontendObjects), new byte[2 * 256 * SpriteArtwork.CharacterByteCount]));

    /// <summary>The OAM records <paramref name="sprites"/>'s finished draws stand for, finalized.</summary>
    internal static OamBuffer ToOam(ScreenSprites sprites)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        for (int index = 0; index < sprites.Draws.Length; index++)
        {
            SpriteDraw draw = sprites.Draws[index];
            SpriteFramePart part = draw.Frame.Parts.Single();
            int x = draw.Origin.X & 0x1ff;
            bool large = part.Size.X > SpriteArtwork.CharacterPixels;
            var attributes = SnesObjAttributeWord.Create(part.Character, part.Palette ?? draw.Palette, part.Priority ?? draw.Priority, part.Flip);
            oam.LowTableBytes[index * 4] = unchecked((byte)x);
            oam.LowTableBytes[index * 4 + 1] = unchecked((byte)draw.Origin.Y);
            oam.WriteAttributes(index * 4, attributes);
            oam.SetHighTablePair(index, SnesOamHighTablePair.FromSprite(unchecked((ushort)x), large));
        }
        oam.NextByteOffset = sprites.Draws.Length * 4;
        oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Reads a finished frame's sprite entries as the OAM tables they stand for.</summary>
    extension(ScreenSprites self)
    {
        /// <summary>The low OAM table of the finished frame.</summary>
        internal ReadOnlySpan<byte> LowTable => ToOam(self).LowTable;

        /// <summary>The high OAM table of the finished frame.</summary>
        internal ReadOnlySpan<byte> HighTable => ToOam(self).HighTable;

        /// <summary>The objects the finished frame placed.</summary>
        internal int LastFinalizedSpriteCount => self.Draws.Length;

        /// <summary>One object of the finished frame.</summary>
        internal OamEntry GetEntry(int index) => ToOam(self).GetEntry(index);

        /// <summary>The bytes of OAM the finished frame's objects fill.</summary>
        internal int NextByteOffset => self.Draws.Length * 4;

        /// <summary>Whether the finished frame stands for exactly <paramref name="records"/>.</summary>
        internal bool ImageEquals(OamBuffer records) => SuperMetroid.Core.Rendering.PpuImageAccess.ImageEquals(ToOam(self), records);

        /// <summary>Whether the finished frame stands for the same OAM records as <paramref name="other"/>'s.</summary>
        internal bool ImageEquals(ScreenSprites other) =>
            SuperMetroid.Core.Rendering.PpuImageAccess.ImageEquals(ToOam(self), ToOam(other));

        /// <summary>Finishes the frame, under OAM's name for it.</summary>
        internal void FinalizeFrame() => self.FinishFrame();

        /// <summary>Occupies one object with a small on-screen spritemap part, as capacity checks fill the frame.</summary>
        internal void AddOccupyingPart(Position16<OamPixels> position, SnesObjAttributeWord attributes) =>
            self.AddPart(SnesSpritemapXWord.Create(0, false), SnesSpritemapYByte.Create(0), attributes, position);

    }

    /// <summary>Compares OAM records with a finished frame's sprite entries.</summary>
    extension(OamBuffer self)
    {
        /// <summary>Whether <paramref name="sprites"/>'s finished frame stands for exactly these records.</summary>
        internal bool ImageEquals(ScreenSprites sprites) => SuperMetroid.Core.Rendering.PpuImageAccess.ImageEquals(self, ToOam(sprites));

        /// <summary>
        /// Occupies one object with a small on-screen spritemap part, as capacity checks fill the frame: unlike a raw
        /// store, the spritemap loader stops when OAM is full.
        /// </summary>
        internal void AddOccupyingPart(Position16<OamPixels> position, SnesObjAttributeWord attributes) =>
            self.AddOnScreenSpritePart(SnesSpritemapXWord.Create(0, false), SnesSpritemapYByte.Create(0), attributes, position);
    }
}
