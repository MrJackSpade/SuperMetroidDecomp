using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>The beam character sheets <c>BeamTilesPointers</c> at $90:C3B1 can select, valued by long address.</summary>
public enum BeamTileSource
{
    /// <summary>$9A:F200, Tiles_PowerBeam; native power-beam character source.</summary>
    Power = 0x9af200,
    /// <summary>$9A:F400, Tiles_IceBeam; native ice-beam character source.</summary>
    Ice = 0x9af400,
    /// <summary>$9A:F600, Tiles_WaveBeam; native wave and ice/wave character source.</summary>
    Wave = 0x9af600,
    /// <summary>$9A:F800, Tiles_PlasmaBeam; shared native plasma-combination character source.</summary>
    Plasma = 0x9af800,
    /// <summary>$9A:FA00, Tiles_Spazer; shared native Spazer-combination character source.</summary>
    Spazer = 0x9afa00,
    /// <summary>The bounded Chainsaw sheet read from adjacent native data.</summary>
    Chainsaw = Game.ChainsawBeamGraphicsDefinitions.TileSource,
    /// <summary>The bounded SpaceTime sheet read from adjacent native data.</summary>
    Spacetime = Game.SpacetimeBeamGraphicsDefinitions.TileSource,
}

/// <summary>Native $90:AC8D beam-character upload geometry, separate from projectile mechanics.</summary>
public static class BeamTileAtlasDefinitions
{
    /// <summary>Indexed PNG width in pixels, arranging eight 8-pixel beam/impact characters left to right in native upload order.</summary>
    public const int Width = 64;
    /// <summary>Indexed PNG height in pixels: one row of 8-by-8 characters, not a projectile collision height.</summary>
    public const int Height = 8;
    /// <summary>Native $90:AC8D upload length, $0100 bytes: eight four-bit planar characters at 32 bytes each.</summary>
    public const int ByteCount = 256;
    /// <summary>$90:AC8D writes eight 4-bpp tiles to VRAM word $6300.</summary>
    public const ushort DestinationWord = 0x6300;
    /// <summary>$90:C3B1 contains twelve legal beam-combination pointers before palette data.</summary>
    public const int SelectionCount = 12;
    /// <summary>Twelve ordinary sheets plus the bounded Chainsaw and SpaceTime adjacent-table uploads.</summary>
    public const int ArtworkCount = SelectionCount + 2;
    /// <summary>Maps the complete artwork-catalog ordinal to its independently editable beam identity: ordinary selections $00..$0B, then bounded Chainsaw $0D and SpaceTime $0E.</summary>
    /// <param name="index">Zero-based artwork ordinal 0..13; the last two ordinals are not their returned beam-selection values.</param>
    /// <returns>The beam combination used to name and install the tile sheet; $0C and $0F have no artwork.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The artwork ordinal is outside 0..13.</exception>
    public static SamusBeamCombination SelectionAt(int index) => index == SelectionCount ? Game.ChainsawBeamGraphicsDefinitions.Selection :
        index == SelectionCount + 1 ? Game.SpacetimeBeamGraphicsDefinitions.Selection : (uint)index < SelectionCount
            ? SamusBeamCombinations.FromTableIndex(index) : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>The artwork-catalog ordinal of a combination's sheet: the inverse of <see cref="SelectionAt"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no artwork.</exception>
    public static int ArtworkOrdinal(SamusBeamCombination selection) => selection switch
    {
        Game.ChainsawBeamGraphicsDefinitions.Selection => SelectionCount,
        Game.SpacetimeBeamGraphicsDefinitions.Selection => SelectionCount + 1,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaIceWave =>
            throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam combination has no artwork."),
        SamusBeamCombination.Power or SamusBeamCombination.Wave or SamusBeamCombination.Ice or
            SamusBeamCombination.IceWave or SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or
            SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.Plasma or
            SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or
            SamusBeamCombination.PlasmaIceWave => selection.TableIndex,
        _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination."),
    };
    /// <summary>$90:C3B1, BeamTilesPointers; resolves a legacy source to its base selection.
    /// Native shared sheets do not encode the independently editable combination identity.</summary>
    /// <returns>The base selection, or null when the address is not a beam tile source.</returns>
    public static SamusBeamCombination? LegacySelectionFor(int sourceAddress)
    {
        var source = (BeamTileSource)sourceAddress;
        if (!Enum.IsDefined(source))
            return null;
        return source switch
        {
            BeamTileSource.Chainsaw => Game.ChainsawBeamGraphicsDefinitions.Selection,
            BeamTileSource.Spacetime => Game.SpacetimeBeamGraphicsDefinitions.Selection,
            BeamTileSource.Power => SamusBeamCombination.Power,
            BeamTileSource.Wave => SamusBeamCombination.Wave,
            BeamTileSource.Ice => SamusBeamCombination.Ice,
            BeamTileSource.Spazer => SamusBeamCombination.Spazer,
            BeamTileSource.Plasma => SamusBeamCombination.Plasma,
            _ => throw new InvalidOperationException($"Undefined BeamTileSource {source}."),
        };
    }

    /// <summary>Creates a selection-keyed PNG filename, retaining separately editable combination identities even where native combinations share the same source sheet.</summary>
    /// <param name="selection">Native beam identity $00..$0B, Chainsaw $0D, or SpaceTime $0E; not an artwork-catalog ordinal.</param>
    /// <returns><c>beam-XX-tiles.png</c> with two uppercase hexadecimal selection digits.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The selection is outside the supported ordinary/bounded domain.</exception>
    public static string FileName(SamusBeamCombination selection)
    {
        _ = ArtworkOrdinal(selection);
        return $"beam-{selection.TableIndex:X2}-tiles.png";
    }

    /// <summary>The bounded glitch sheets read adjacent native data; none of the retail pixel relationships apply.</summary>
    private static bool IsAdjacentDataArtwork(SamusBeamCombination selection)
    {
        if (selection is Game.ChainsawBeamGraphicsDefinitions.Selection or Game.SpacetimeBeamGraphicsDefinitions.Selection)
            return true;
        if (!selection.IsRetail)
            throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam combination has no artwork.");
        return false;
    }
    /// <summary>$9A:F240..F25F is the transposed power-beam tile0 at F200..F21F.</summary>
    private const int PowerTransposedTile = 2;
    /// <summary>$9A:F860..F87F and FA60..FA7F quarter-turn each family's tile0.</summary>
    private const int LongBeamVerticalTile = 3;
    /// <summary>$9A:F2A0..F2FF and F4A0..F4FF are three transparent upload tiles in the power/ice sheets.</summary>
    private const int TransparentTailFirstTile = 5;

    /// <summary>$9A:F6A0..F6BF: Wave impact tile5 has half-turn symmetry.</summary>
    private const int WaveLargeImpactTile = 5;
    /// <summary>$9A:F6C0..F6DF: Wave impact tile6 reflects across both central axes.</summary>
    private const int WaveCenteredImpactTile = 6;
    /// <summary>$9A:F6E0..F6FF: Wave impact tile7 has half-turn symmetry.</summary>
    private const int WaveSmallImpactTile = 7;
    /// <summary>$9A:FA20..FA5F: the two adjacent Spazer diagonal upload tiles, used together by $93:D10E/D25A spritemaps.</summary>
    private const int SpazerDiagonalFirstTile = 1;
    /// <summary>$9A:FA06/FA08 and FA16/FA18: selected horizontal Spazer ribbon occupies two rows; this base artwork thickness remains REQUIRED.</summary>
    internal const int SpazerRibbonThickness = 2;
    /// <summary>$9A:F880..F89F and FA80..FA9F: the wider horizontal ribbon occupies upload tile4.</summary>
    private const int LongBeamWideRibbonTile = 4;
    /// <summary>$9A:F882/F892: required wide-ribbon profile begins its uniform border at row1; row7 shares that border ink.</summary>
    private const int WideRibbonBorderRow = 1;
    /// <summary>$9A:F884/F886/F88A/F88C and second planes: required wide-ribbon side rows repeat a four-pixel pattern.</summary>
    private const int WideRibbonSidePeriod = 4;
    /// <summary>$9A:F88A/F88C and F89A/F89C: required opposite wide-ribbon edges shift their upper partners by two pixels.</summary>
    private const int WideRibbonOppositePhase = 2;
    /// <summary>$9A:F888/F898: required wide-ribbon center repeats two selected inks.</summary>
    private const int WideRibbonCenterPeriod = 2;
    /// <summary>$9A:F8E0..F8FF and FAE0..FAFF: final Plasma/Spazer upload tile shares repeated rows.</summary>
    private const int LongBeamImpactTile = 7;
    /// <summary>$9A:F8E0..F8FF and FAE0..FAFF: required long-beam impact pattern repeats its odd rows every four rows; even rows share row0.</summary>
    private const int LongBeamImpactRowPeriod = 4;
    /// <summary>$9A:F806/F80A and F816/F81A: required Plasma ribbon edge profile, one row either side of the center.</summary>
    private const int PlasmaEdgeDistance = 1;
    /// <summary>$9A:F808/F818: required Plasma center ink pattern repeats every two pixels.</summary>
    private const int PlasmaCenterPeriod = 2;
    /// <summary>$9A:F806/F816: required Plasma edge ink pattern repeats every four pixels.</summary>
    private const int PlasmaEdgePeriod = 4;
    /// <summary>$9A:F80A/F81A: required opposite Plasma edge phase is two pixels.</summary>
    private const int PlasmaOppositeEdgePhase = 2;
    /// <summary>$9A:FA08/FA18: required lower Spazer ribbon row shifts its FA06/FA16 upper row by one pixel.</summary>
    private const int SpazerLowerRowShift = 1;
    /// <summary>$9A:FA06/FA16: required horizontal highlight phase places its central white pen at column2.</summary>
    private const int SpazerHighlightPhase = 2;
    /// <summary>$9A:FA20..FA5F: required diagonal texture pulse mirrors its first six rows, followed by a repeated tail.</summary>
    private const int SpazerDiagonalPulseRows = 6;

    /// <summary>$9A:F806/07/16/17, tile0 row3 x0: categorical dark-green edge base, palette pen4.</summary>
    private const byte PlasmaEdgeBasePen = 4;
    /// <summary>$9A:F806/07/16/17, tile0 row3 x1: categorical green edge highlight, palette pen2.</summary>
    private const byte PlasmaEdgeHighlightPen = 2;
    /// <summary>$9A:F806/07/16/17, tile0 row3 x3: categorical darkest edge shadow, palette pen15.</summary>
    private const byte PlasmaEdgeShadowPen = 15;
    /// <summary>$9A:F808/09/18/19, tile0 row4 x0: categorical pale-green center highlight, palette pen5.</summary>
    private const byte PlasmaCenterHighlightPen = 5;
    /// <summary>$9A:F808/09/18/19, tile0 row4 x1: categorical middle-green center ink, palette pen3.</summary>
    private const byte PlasmaCenterMidPen = 3;

    /// <summary>Five independently reviewed categorical pen assignments in the first Plasma ribbon tile.</summary>
    /// <remarks>Issue1165 narrow nonsense retention: these are distinct painted pixel classes,
    /// not samples of a hue/intensity magnitude determining their pen numbers. Native rows
    /// are424F424F /53535353 /4F424F42, with selected dark/highlight/shadow roles. Arithmetic
    /// fitting of those categorical assignments would re-encode the painting. Only the five
    /// original seed positions are covered; profile width/period/phase, RGB palettes and all
    /// other tile/pixel choices remain required. Independently supplied edits override each pen.</remarks>
    internal static bool TryStockPlasmaInk(SamusBeamCombination selection, int pixel, out byte ink)
    {
        if (IsAdjacentDataArtwork(selection)) { ink = 0; return false; }
        if ((uint)pixel >= Width * Height) throw new ArgumentOutOfRangeException(nameof(pixel));
        if (!selection.HasPlasma)
        {
            ink = 0;
            return false;
        }
        const int edge = (Height / 2 - PlasmaEdgeDistance) * Width;
        const int center = Height / 2 * Width;
        int selected = pixel switch
        {
            edge => PlasmaEdgeBasePen,
            edge + 1 => PlasmaEdgeHighlightPen,
            edge + 3 => PlasmaEdgeShadowPen,
            center => PlasmaCenterHighlightPen,
            center + 1 => PlasmaCenterMidPen,
            _ => -1,
        };
        ink = selected >= 0 ? (byte)selected : (byte)0;
        return selected >= 0;
    }
    /// <summary>
    /// Calculate only proven within-sheet orientation/transparent relationships. All
    /// other pixel sites remain independently required artwork. Source -1 is transparent;
    /// derived sources lie in the first tile or earlier rows/columns of their own impact tile. These rules form acyclic dependencies on independently supplied basis pixels.
    /// </summary>
    internal static bool TryDerivedPixelSource(SamusBeamCombination selection, int pixel, out int source)
    {
        if (IsAdjacentDataArtwork(selection)) { source = 0; return false; }
        if ((uint)pixel >= Width * Height) throw new ArgumentOutOfRangeException(nameof(pixel));
        int tile = pixel % Width / Height;
        int x = pixel % Height;
        int y = pixel / Width;
        // $9A:F400..F41F is horizontally symmetric about the first Ice tile's center.
        if (selection == SamusBeamCombination.Ice && tile == 0 && x >= Height / 2)
        {
            source = y * Width + Height - 1 - x;
            return true;
        }
        // Rotate the same physical ribbon through45 degrees and sample pixel centers.
        // Native composition origins remain owned/required in the shared catalog;
        // the base horizontal thickness remains required here. Neither is inferred
        // from the diagonal mask that this projection produces.
        if (selection.HasSpazer && tile >= SpazerDiagonalFirstTile && tile <= SpazerDiagonalFirstTile + 1)
        {
            int bandX = (tile - SpazerDiagonalFirstTile) * Height + x;
            double distance = (SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginX
                + SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginY
                + SpazerCompositionGeometryDefinitions.DiagonalStripWidth - bandX + y) / Math.Sqrt(2);
            bool outside = Math.Abs(distance) >= SpazerRibbonThickness / 2.0;
            if (outside)
            {
                source = -1;
                return true;
            }
            int row = y < SpazerDiagonalPulseRows
                ? Math.Min(y, SpazerDiagonalPulseRows - 1 - y) : SpazerDiagonalPulseRows;
            int center = SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginX
                + SpazerCompositionGeometryDefinitions.DiagonalFirstPairOriginY
                + SpazerCompositionGeometryDefinitions.DiagonalStripWidth;
            int relativeX = bandX - y;
            int sourceX;
            if (row == SpazerDiagonalPulseRows / 2 - 1)
                sourceX = row + Math.Min(relativeX, 2 * center - relativeX);
            else
                sourceX = (int)Math.Floor(row + center - SpazerRibbonThickness / 2.0 * Math.Sqrt(2)) + 1;
            source = row * Width + SpazerDiagonalFirstTile * Height + sourceX;
            return source != pixel;
        }
        // Native Wave impact sheets are centered shapes: two half-turn pairs and
        // one shape reflected across both central axes. Keep their source quadrants required.
        if (CanonicalSelection(selection) == SamusBeamCombination.Wave)
        {
            if ((tile == WaveLargeImpactTile || tile == WaveSmallImpactTile) && y >= Height / 2)
            {
                source = (Height - 1 - y) * Width + tile * Height + Height - 1 - x;
                return true;
            }
            if (tile == WaveCenteredImpactTile && (x >= Height / 2 || y >= Height / 2))
            {
                source = Math.Min(y, Height - 1 - y) * Width + tile * Height + Math.Min(x, Height - 1 - x);
                return true;
            }
        }
        // These exact wide-ribbon repetitions do not dispose of their selected
        // profile rows, pattern periods, or independent source inks.
        if ((selection.HasSpazer || selection.HasPlasma) && tile == LongBeamWideRibbonTile)
        {
            if (y is WideRibbonBorderRow or (Height - 1))
                source = WideRibbonBorderRow * Width + tile * Height;
            else if (y == Height / 2)
                source = y * Width + tile * Height + x % WideRibbonCenterPeriod;
            else if (y > WideRibbonBorderRow)
            {
                int row = y > Height / 2 ? Height - y : y;
                int phase = y > Height / 2 ? WideRibbonOppositePhase : 0;
                source = row * Width + tile * Height + (x + phase) % WideRibbonSidePeriod;
            }
            else
            {
                source = 0;
                return false;
            }
            return source != pixel;
        }
        if ((selection.HasSpazer || selection.HasPlasma) && tile == LongBeamImpactTile)
        {
            int row = (y & 1) == 0 ? 0 : y % LongBeamImpactRowPeriod;
            source = row * Width + tile * Height + x;
            return source != pixel;
        }
        // These native ribbon profiles remain required artwork choices; calculate only
        // their exact row reflection/repetition and preserve independently selected inks.
        if (selection == SamusBeamCombination.Power && tile == 0 && y >= Height / 2)
        {
            source = (Height - 1 - y) * Width + x;
            return true;
        }
        if (selection.HasPlasma && tile == 0)
        {
            int center = Height / 2;
            if (y < center - PlasmaEdgeDistance || y > center + PlasmaEdgeDistance)
            {
                source = -1;
                return true;
            }
            if (y == center) source = center * Width + x % PlasmaCenterPeriod;
            else
            {
                int phase = (x + (y > center ? PlasmaOppositeEdgePhase : 0)) % PlasmaEdgePeriod;
                source = (center - PlasmaEdgeDistance) * Width + ((phase & 1) == 0 ? 0 : phase);
            }
            return source != pixel;
        }
        if (selection.HasSpazer && tile == 0)
        {
            double distance = SpazerCompositionGeometryDefinitions.HorizontalStripOriginY + y + 0.5;
            if (Math.Abs(distance) >= SpazerRibbonThickness / 2.0)
            {
                source = -1;
                return true;
            }
            int upper = -SpazerCompositionGeometryDefinitions.HorizontalStripOriginY - SpazerRibbonThickness / 2;
            if (y == upper)
            {
                source = upper * Width + Math.Min(x, (2 * SpazerHighlightPhase - x + Height) % Height);
                return source != pixel;
            }
            source = upper * Width + (x + Height - SpazerLowerRowShift * (y - upper)) % Height;
            return true;
        }
        if (selection is SamusBeamCombination.Power or SamusBeamCombination.Ice && tile >= TransparentTailFirstTile)
        {
            source = -1;
            return true;
        }
        if (selection == SamusBeamCombination.Power && tile == PowerTransposedTile)
        {
            source = x * Width + y;
            return true;
        }
        if ((selection.HasSpazer || selection.HasPlasma) && tile == LongBeamVerticalTile)
        {
            source = (Height - 1 - x) * Width + y;
            return true;
        }
        source = 0;
        return false;
    }

    /// <summary>$90:C3B1..C3C7: Plasma, then Spazer, then Wave, then Ice selects the shared native sheet; secondary equipped effects do not change its pixels.</summary>
    internal static SamusBeamCombination CanonicalSelection(SamusBeamCombination selection) => selection switch
    {
        SamusBeamCombination.Power => SamusBeamCombination.Power,
        SamusBeamCombination.Ice => SamusBeamCombination.Ice,
        SamusBeamCombination.Wave or SamusBeamCombination.IceWave => SamusBeamCombination.Wave,
        SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or SamusBeamCombination.SpazerIce or
            SamusBeamCombination.SpazerIceWave => SamusBeamCombination.Spazer,
        SamusBeamCombination.Plasma or SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or
            SamusBeamCombination.PlasmaIceWave => SamusBeamCombination.Plasma,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
            SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
            throw new ArgumentOutOfRangeException(nameof(selection), selection, "Only retail combinations share a native sheet."),
        _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination."),
    };

    /// <summary>$9A:F660..F69F repeats PowerF260..F29F; FA80..FAFF repeats PlasmaF880..F8FF. Other families provide their own required pixels.</summary>
    /// <returns>The sheet whose shared tiles this canonical sheet repeats, or null when it repeats none.</returns>
    internal static SamusBeamCombination? SharedTileSourceSelection(SamusBeamCombination selection) => CanonicalSelection(selection) switch
    {
        SamusBeamCombination.Wave => SamusBeamCombination.Power,
        SamusBeamCombination.Spazer => SamusBeamCombination.Plasma,
        SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Plasma => null,
        var canonical => throw new InvalidOperationException($"{canonical} is not a canonical beam sheet."),
    };

    /// <summary>Wave tiles3/4 share Power's same slots; Spazer's last four upload tiles share Plasma's same slots.</summary>
    internal static bool IsSharedTilePixel(SamusBeamCombination selection, int pixel)
    {
        if ((uint)pixel >= Width * Height) throw new ArgumentOutOfRangeException(nameof(pixel));
        int tile = pixel % Width / Height;
        return CanonicalSelection(selection) switch
        {
            SamusBeamCombination.Wave => tile is 3 or 4,
            SamusBeamCombination.Spazer => tile >= 4,
            SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Plasma => false,
            var canonical => throw new InvalidOperationException($"{canonical} is not a canonical beam sheet."),
        };
    }
}
