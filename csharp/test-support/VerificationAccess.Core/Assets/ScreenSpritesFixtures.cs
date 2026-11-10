using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Frontend sprite entries as tests compare them (#1358): a blank screen to place parts into, and the OAM records a
/// finished frame's one-part draws stand for, so native spritemap comparisons keep their byte form.
/// </summary>
internal static class ScreenSpritesFixtures
{
    /// <summary>Empty sprite entries to place frames into.</summary>
    internal static ScreenSprites Blank() => new();

    /// <summary>One finished frame of the entries <paramref name="draw"/> places.</summary>
    internal static ScreenSprites Frame(Action<ScreenSprites> draw)
    {
        ScreenSprites sprites = Blank();
        sprites.BeginFrame();
        draw(sprites);
        sprites.FinishFrame();
        return sprites;
    }

    /// <summary>
    /// The OAM records <paramref name="sprites"/>'s finished draws stand for, finalized: each frame's composition parts,
    /// placed from its origin by the spritemap loader that origin selects, so the OAM matches the cartridge's byte for byte.
    /// A draw's origin is its signed screen position (#1587). Only an origin on the screen's band of
    /// <see cref="OnScreenLoaderBand"/> lines is read by the on-screen loader; one above the screen, or at or below the
    /// band's last line, is read by the off-screen loader, as the cartridge's cinematic sprite draw dispatches it: it chooses
    /// the loader by the origin's sixteen-bit Y and passes only its low byte. An origin below the band is therefore never
    /// read by the on-screen loader, whose low-byte arithmetic would bring it round to the top of the screen.
    /// </summary>
    internal static OamBuffer ToOam(ScreenSprites sprites)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        foreach (ImageSpriteDraw draw in sprites.Draws.Cast<ImageSpriteDraw>())
        {
            CharacterSpriteDraw character = SuperMetroid.TestSupport.RepositoryInstallation.Verification.SheetFrameSources.CharacterDraw(draw);
            ScreenObjectSizes sizes = SuperMetroid.TestSupport.RepositoryInstallation.Verification.SheetFrameSources.ObjectSizes(draw);
            var origin = new Position16<SpritePixels>(unchecked((ushort)(character.Origin.X & 0x1ff)), unchecked((ushort)(character.Origin.Y & 0xff)));
            bool onScreenLoader = (short)character.Origin.Y is >= 0 and < OnScreenLoaderBand;
            foreach (SpriteFramePart part in character.Frame.Parts)
            {
                var attributes = SnesObjAttributeWord.Create(part.Character, part.Palette ?? character.Palette, part.Priority ?? character.Priority, part.Flip);
                SnesSpritemapXWord x = SnesSpritemapXWord.Create(part.Offset.X, part.Size == sizes.Large);
                SnesSpritemapYByte y = SnesSpritemapYByte.Create(part.Offset.Y);
                if (onScreenLoader)
                    oam.AddOnScreenSpritePart(x, y, attributes, origin);
                else
                    oam.AddOffScreenSpritePart(x, y, attributes, origin);
            }
        }
        oam.FinalizeFrame();
        return oam;
    }

    /// <summary>The lines from the top of the screen whose origins the on-screen loader reads: those its low Y byte names.</summary>
    private const int OnScreenLoaderBand = 256;

    /// <summary>One-part frames already composed, by spritemap words.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<PartKey, SpriteSheetFrame> partFrames = new();

    /// <summary>
    /// A sheet frame of one spritemap part, at the menus' object sizes over a blank page, registered so its draws expand
    /// to the OAM record the loader writes for that part.
    /// </summary>
    internal static SpriteSheetFrame PartFrame(SnesSpritemapXWord x, SnesSpritemapYByte y, SnesObjAttributeWord attributes) =>
        partFrames.GetOrAdd(new PartKey(x.Raw, y.Raw, attributes.Raw), key =>
        {
            string name = $"Part.{key.X:X4}.{key.Y:X2}.{key.Attributes:X4}";
            SpriteFrame frame = new SpriteComposition([new CompiledSpritePart(new SpritemapPart(new SnesSpritemapXWord(key.X),
                new SnesSpritemapYByte(key.Y), new SnesObjAttributeWord(key.Attributes)).ToPart(), InheritPalette: false)]).ToFrame(name, 0);
            var page = new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.FrontendObjects), new byte[2 * 256 * SpriteArtwork.CharacterByteCount]);
            var sources = new SuperMetroid.AssetExtraction.Artwork.SpriteSheetSources(
                [new SuperMetroid.AssetExtraction.Artwork.SheetFrameSource(name, frame, page, 0)], new RgbColor[8 * 16], SpritePartFrames.StandardSizes,
                PartsKeepOwnRows: true);
            return SuperMetroid.TestSupport.RepositoryInstallation.Verification.CinematicSheetFixtures.Load(sources, name).Frame(name);
        });

    /// <summary>A one-part frame's spritemap words.</summary>
    /// <param name="X">The X word: offset and size.</param>
    /// <param name="Y">The Y offset byte.</param>
    /// <param name="Attributes">The attribute word.</param>
    private readonly record struct PartKey(ushort X, byte Y, ushort Attributes);

    /// <summary>Reads a finished frame's sprite entries as the OAM tables they stand for.</summary>
    extension(ScreenSprites self)
    {
        /// <summary>The low OAM table of the finished frame.</summary>
        internal ReadOnlySpan<byte> LowTable => ToOam(self).LowTable;

        /// <summary>The high OAM table of the finished frame.</summary>
        internal ReadOnlySpan<byte> HighTable => ToOam(self).HighTable;

        /// <summary>The objects the finished frame placed.</summary>
        internal int LastFinalizedSpriteCount => ToOam(self).LastFinalizedSpriteCount;

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

    }

    /// <summary>Compares OAM records with a finished frame's sprite entries.</summary>
    extension(OamBuffer self)
    {
        /// <summary>Whether <paramref name="sprites"/>'s finished frame stands for exactly these records.</summary>
        internal bool ImageEquals(ScreenSprites sprites) => SuperMetroid.Core.Rendering.PpuImageAccess.ImageEquals(self, ToOam(sprites));

        /// <summary>The first object whose record differs from <paramref name="sprites"/>'s finished frame, for a failure message.</summary>
        internal string FirstDifference(ScreenSprites sprites) => SuperMetroid.Core.Rendering.PpuImageAccess.FirstDifference(self, ToOam(sprites));
    }
}
