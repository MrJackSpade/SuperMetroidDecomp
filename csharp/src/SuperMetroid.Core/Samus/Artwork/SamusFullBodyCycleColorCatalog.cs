using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Authored Samus full-body colors keyed by the already-compiled native palette pointers.
/// The catalog never chooses a suit, cycle phase, or animation delay.
/// </summary>
/// <remarks>Stock inputs are21 opaque base inks, four transparent payloads,
/// and15 endpoint components. The opaque inks are Power1..15,Varia2/10/11,
/// Gravity2/10/11. Original $91:DD5B copies these categorical sprite inks
/// into fixed OBJ slots; their indices are not brightness/time coordinates.
/// They match normal suit artwork; Gravity1/12 instead derive from Power's
/// dim endpoint channels. No physical lighting/material input specifies these
/// remaining paint choices. Fitting ink index to RGB would encode the painting,
/// the #1165 nonsense exception, not replace a functional lookup relationship.
/// The four slot-zero payloads0000/2003/3800/14E0 are copied by the native
/// loader but ignored by OBJ rendering, which skips pixel index0 before RGB
/// lookup. Their distinct unused RGB values have no rendering-derived formula.
/// Endpoint choices are Speed Power dim1 R14/G6,dim2 B13,bright9 B21,
/// dim12 R22/G16; Varia bright10 B29,dim12 R13; Gravity dim2 B27;
/// active Power middle2 B5,bright2 B8,dim8 B17; Screw Power bright10 R16,
/// bright15 G31 and Varia bright2 R20. These are chosen ink tint targets,
/// with no quantitative input deriving the targets. Shared channels and shade
/// operations are calculated separately, including Varia active bright2's
/// equality to Power middle2. Retaining these exact remaining components is
/// the same painting exception, not a performance/size/complexity exemption.
/// All supplied custom differences remain independently owned overrides.
/// This disposition covers only these full-body inputs, not other palettes.</remarks>
public sealed class SamusFullBodyCycleColorCatalog
{
    /// <summary>Authored colors remaining after canonical shade relationships and independent endpoint channels are removed.</summary>
    internal readonly Dictionary<int, Bgr555> colors;
    /// <summary>Independent red and green input for Power's first dim Speed Booster ink.</summary>
    internal readonly LoadingPaletteInputView.Channels powerDim1;
    /// <summary>Independent blue input for Power's second dim Speed Booster ink.</summary>
    internal readonly LoadingPaletteInputView.Channels powerDim2;
    /// <summary>Independent blue input for Power's ninth bright Speed Booster ink.</summary>
    internal readonly LoadingPaletteInputView.Channels powerBright9;
    /// <summary>Independent red and green input for Power's twelfth dim Speed Booster ink.</summary>
    internal readonly LoadingPaletteInputView.Channels powerDim12;
    /// <summary>Independent blue input for Varia's tenth bright Speed Booster ink.</summary>
    internal readonly LoadingPaletteInputView.Channels variaBright10;
    /// <summary>Independent red input for Varia's twelfth dim Speed Booster ink.</summary>
    internal readonly LoadingPaletteInputView.Channels variaDim12;
    /// <summary>Independent blue input for Gravity's second dim Speed Booster ink.</summary>
    internal readonly LoadingPaletteInputView.Channels gravityDim2;
    /// <summary>Active-shinespark blue input at <c>$9C64</c>, also shared by Varia at <c>$9E84</c>.</summary>
    internal readonly LoadingPaletteInputView.Channels activePowerMiddle2;
    /// <summary>Active-shinespark blue input at <c>$9C84</c>.</summary>
    internal readonly LoadingPaletteInputView.Channels activePowerBright2;
    /// <summary>Active-shinespark blue input at <c>$9C50</c>.</summary>
    internal readonly LoadingPaletteInputView.Channels activePowerDim8;
    /// <summary>Screw Attack red input for Power's tenth bright ink at <c>$9D14</c>.</summary>
    internal readonly LoadingPaletteInputView.Channels screwPowerBright10;
    /// <summary>Screw Attack green input for Power's fifteenth bright ink at <c>$9D1E</c>.</summary>
    internal readonly LoadingPaletteInputView.Channels screwPowerBright15;
    /// <summary>Screw Attack red input for Varia's second bright ink at <c>$9F04</c>.</summary>
    internal readonly LoadingPaletteInputView.Channels screwVariaBright2;

    /// <summary>Separates supplied colors into categorical ink overrides and endpoint channels with computable shade bases.</summary>
    private SamusFullBodyCycleColorCatalog(Bgr555[][] palettes)
    {
        colors = new Dictionary<int, Bgr555>();
        for (int palette = 0; palette < palettes.Length; palette++)
        for (int color = 0; color < SamusFullBodyCycleColorFormat.ColorsPerPalette; color++)
        {
            int index = palette * SamusFullBodyCycleColorFormat.ColorsPerPalette + color;
            int source = SamusFullBodyCycleColorFormat.CanonicalColorIndex(palette, color);
            Bgr555 value = palettes[palette][color];
            if (source != index && value == palettes[source / 16][source % 16]) continue;
            if (source == index && SamusFullBodyCycleColorFormat.IsStoredShineShade(palette, color) &&
                value == SamusFullBodyCycleColorFormat.StoredShineColor(palettes[palette / 16 * 16 + 4][color], palette % 4)) continue;
            if (source == index && SamusFullBodyCycleColorFormat.TrySpeedBoosterTint(palette, color,
                palettes[palette / 16 * 16][color], out Bgr555 tinted) && value == tinted) continue;
            if (source == index && SamusFullBodyCycleColorFormat.TrySpeedBoosterBrightening(palette, color,
                palettes[palette / 16 * 16 + 1][color], out Bgr555 brightened) && value == brightened) continue;
            int channelSource = SamusFullBodyCycleColorFormat.SpeedSharedChannelSource(palette, color);
            if (source == index && channelSource >= 0 && value == SamusFullBodyCycleColorFormat.SpeedSharedChannelColor(
                palette, color, palettes[palette / 16 * 16][color], palettes[channelSource / 16][channelSource % 16])) continue;
            if (source == index && SamusFullBodyCycleColorFormat.IsActiveShineTint(palette, color) &&
                value == SamusFullBodyCycleColorFormat.ActiveShineTint(palettes[palette / 16 * 16 + 8][color], palette % 4)) continue;
            if (source == index && SamusFullBodyCycleColorFormat.TryActiveGoldRamp(palette, color,
                palettes[palette / 16 * 16 + 8][color], out Bgr555 gold) && value == gold) continue;
            if (source == index && SamusFullBodyCycleColorFormat.TryScrewAttackTint(palette, color,
                palettes[palette / 16 * 16 + 12][color], out Bgr555 screw) && value == screw) continue;
            if (source == index && SamusFullBodyCycleColorFormat.TryScrewPowerInk(palette, color,
                palettes[12][color], palettes[1][12], out Bgr555 ink) && value == ink) continue;
            if (source == index && SamusFullBodyCycleColorFormat.TryVariaScrewInk(palette, color,
                palettes[28][color], palettes[31][color], out Bgr555 variaInk) && value == variaInk) continue;
            if (palette == 32 && color is 1 or 12 && value ==
                SamusFullBodyCycleColorFormat.GravitySharedBase(palettes[0][color], palettes[1][color])) continue;
            colors.Add(index, value);
        }
        powerDim1 = Capture(1, 1, false, 2);
        powerDim2 = Capture(1, 2, false, 2);
        powerBright9 = Capture(3, 9, false, 0);
        powerDim12 = Capture(1, 12, false, 2);
        variaBright10 = Capture(19, 10, true, 0);
        variaDim12 = Capture(17, 12, true, 2);
        gravityDim2 = Capture(33, 2, false, 2);
        activePowerMiddle2 = CaptureActive(10, 2);
        activePowerBright2 = CaptureActive(11, 2);
        activePowerDim8 = CaptureActive(9, 8);
        screwPowerBright10 = CaptureScrew(15, 10);
        screwPowerBright15 = CaptureScrew(15, 15);
        screwVariaBright2 = CaptureScrew(31, 2);

        LoadingPaletteInputView.Channels CaptureScrew(int palette, int color)
        {
            Bgr555 basis = palettes[palette / 16 * 16 + 12][color];
            Bgr555 expected;
            if (palette == 31) _ = SamusFullBodyCycleColorFormat.TryVariaScrewInk(palette, color, basis, basis, out expected);
            else _ = SamusFullBodyCycleColorFormat.TryScrewAttackTint(palette, color, basis, out expected);
            colors.Remove(palette * 16 + color);
            return new(palettes[palette][color], expected);
        }

        LoadingPaletteInputView.Channels CaptureActive(int palette, int color)
        {
            Bgr555 basis = palettes[palette / 16 * 16 + 8][color];
            Bgr555 expected;
            if (color == 8) expected = SamusFullBodyCycleColorFormat.ActiveShineTint(basis, 1);
            else _ = SamusFullBodyCycleColorFormat.TryActiveGoldRamp(palette, color, basis, out expected);
            colors.Remove(palette * 16 + color);
            return new(palettes[palette][color], expected);
        }

        LoadingPaletteInputView.Channels Capture(int palette, int color, bool varia, int shade)
        {
            Bgr555 basis = palettes[palette / 16 * 16][color];
            Bgr555 expected = varia ? LoadingPaletteColorDefinitions.VariaTintColor(basis, shade) :
                LoadingPaletteColorDefinitions.TintColor(basis, shade);
            colors.Remove(palette * 16 + color);
            return new(palettes[palette][color], expected);
        }
    }

    /// <summary>Returns one BGR555 color at a compiled bank-$9B palette pointer.</summary>
    public Bgr555 Resolve(ushort pointer, int colorIndex)
    {
        int palette = SamusFullBodyCycleColorFormat.PaletteIndex(pointer);
        if ((uint)colorIndex >= SamusFullBodyCycleColorFormat.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));
        int index = palette * SamusFullBodyCycleColorFormat.ColorsPerPalette + colorIndex;
        return ResolveIndex(index);
    }

    /// <summary>Resolves an indexed ink through endpoint channels, categorical overrides, and shared shade formulas.</summary>
    private Bgr555 ResolveIndex(int index)
    {
        switch (index)
        {
            case 17: return powerDim1.Apply(LoadingPaletteColorDefinitions.TintColor(ResolveIndex(1), 2));
            case 18: return powerDim2.Apply(LoadingPaletteColorDefinitions.TintColor(ResolveIndex(2), 2));
            case 57: return powerBright9.Apply(LoadingPaletteColorDefinitions.TintColor(ResolveIndex(9), 0));
            case 28: return powerDim12.Apply(LoadingPaletteColorDefinitions.TintColor(ResolveIndex(12), 2));
            case 314: return variaBright10.Apply(LoadingPaletteColorDefinitions.VariaTintColor(ResolveIndex(266), 0));
            case 284: return variaDim12.Apply(LoadingPaletteColorDefinitions.VariaTintColor(ResolveIndex(268), 2));
            case 530: return gravityDim2.Apply(LoadingPaletteColorDefinitions.TintColor(ResolveIndex(514), 2));
            case 162: return activePowerMiddle2.Apply(ActiveGoldExpected(10));
            case 178: return activePowerBright2.Apply(ActiveGoldExpected(11));
            case 152: return activePowerDim8.Apply(SamusFullBodyCycleColorFormat.ActiveShineTint(ResolveIndex(136), 1));
            case 250: return screwPowerBright10.Apply(ScrewExpected(15, 10));
            case 255: return screwPowerBright15.Apply(ScrewExpected(15, 15));
            case 498: return screwVariaBright2.Apply(ScrewExpected(31, 2));
            default:
                break;
        }
        if (colors.TryGetValue(index, out Bgr555 value)) return value;
        if (index is 513 or 524)
            return SamusFullBodyCycleColorFormat.GravitySharedBase(ResolveIndex(index % 16), ResolveIndex(16 + index % 16));
        int palette = index / 16, color = index % 16;
        int source = SamusFullBodyCycleColorFormat.CanonicalColorIndex(palette, color);
        if (source != index) return ResolveIndex(source);
        if (palette is 29 or 30 && SamusFullBodyCycleColorFormat.TryVariaScrewInk(palette, color,
            ResolveIndex(28 * 16 + color), ResolveIndex(498), out Bgr555 variaInk)) return variaInk;
        if (palette is >= 13 and <= 15 && SamusFullBodyCycleColorFormat.TryScrewPowerInk(palette, color,
            ResolveIndex(12 * 16 + color), ResolveIndex(16 + 12), out Bgr555 ink)) return ink;
        if (palette % 16 >= 13 && SamusFullBodyCycleColorFormat.TryScrewAttackTint(palette, color,
            ResolveIndex((palette / 16 * 16 + 12) * 16 + color), out Bgr555 screw)) return screw;
        if (SamusFullBodyCycleColorFormat.IsActiveShineTint(palette, color))
            return SamusFullBodyCycleColorFormat.ActiveShineTint(ResolveIndex((palette / 16 * 16 + 8) * 16 + color), palette % 4);
        if (palette % 16 is >= 9 and <= 11 && SamusFullBodyCycleColorFormat.TryActiveGoldRamp(palette, color,
            ResolveIndex((palette / 16 * 16 + 8) * 16 + color), out Bgr555 gold)) return gold;
        int channelSource = SamusFullBodyCycleColorFormat.SpeedSharedChannelSource(palette, color);
        if (channelSource >= 0) return SamusFullBodyCycleColorFormat.SpeedSharedChannelColor(
            palette, color, ResolveIndex(palette / 16 * 256 + color), ResolveIndex(channelSource));
        if (palette % 16 is 2 or 3 && SamusFullBodyCycleColorFormat.TrySpeedBoosterBrightening(palette, color,
            ResolveIndex((palette / 16 * 16 + 1) * 16 + color), out Bgr555 brightened)) return brightened;
        if (SamusFullBodyCycleColorFormat.TrySpeedBoosterTint(palette, color,
            ResolveIndex(palette / 16 * 256 + color), out Bgr555 tinted)) return tinted;
        return SamusFullBodyCycleColorFormat.StoredShineColor(
            ResolveIndex((palette / 16 * 16 + 4) * 16 + color), palette % 4);
    }

    /// <summary>Computes an active-shinespark gold-ramp basis for a palette's second ink.</summary>
    private Bgr555 ActiveGoldExpected(int palette)
    {
        _ = SamusFullBodyCycleColorFormat.TryActiveGoldRamp(palette, 2,
            ResolveIndex((palette / 16 * 16 + 8) * 16 + 2), out Bgr555 value);
        return value;
    }

    /// <summary>Computes a Screw Attack tint basis, including Varia's separate second-ink formula.</summary>
    private Bgr555 ScrewExpected(int palette, int color)
    {
        Bgr555 basis = ResolveIndex((palette / 16 * 16 + 12) * 16 + color);
        Bgr555 value;
        if (palette == 31) _ = SamusFullBodyCycleColorFormat.TryVariaScrewInk(palette, color, basis, basis, out value);
        else _ = SamusFullBodyCycleColorFormat.TryScrewAttackTint(palette, color, basis, out value);
        return value;
    }

    /// <summary>Copies sixteen display colors to Samus OBJ palette four.</summary>
    public void Apply(IPaletteColors cgram, ushort pointer)
    {
        Ensure.NotNull(cgram);
        for (int index = 0; index < SamusFullBodyCycleColorFormat.ColorsPerPalette; index++)
            new SamusPalettes(cgram).Suit.Write(index,
                Resolve(pointer, index));
    }

    /// <summary>Loads and validates all 48 full-body cycle palettes for three suits and four families.</summary>
    public static SamusFullBodyCycleColorCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        SamusFullBodyCycleColorDocument document;
        document = JsonAssetDocument.Read<SamusFullBodyCycleColorDocument>(json, ArtworkJson.Options, "Samus full-body cycle color");
        if (document.Version != SamusFullBodyCycleColorFormat.Version)
            throw new InvalidDataException("Samus full-body cycle colors require the supported version.");

        var palettes = new Bgr555[SamusFullBodyCycleColorFormat.PaletteCount][];
        AddFamily(palettes, SamusFullBodyCycleFamily.SpeedBooster, document.SpeedBooster);
        AddFamily(palettes, SamusFullBodyCycleFamily.ScrewAttack, document.ScrewAttack);
        AddFamily(palettes, SamusFullBodyCycleFamily.StoredShine, document.StoredShine);
        AddFamily(palettes, SamusFullBodyCycleFamily.ActiveShinespark, document.ActiveShinespark);
        return new(palettes);
    }

    /// <summary>Validates and serializes a full-body cycle color document as JSON.</summary>
    public static byte[] Write(SamusFullBodyCycleColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Load(json));

    /// <summary>Validates a family's three suits and four shades, then places RGB5 colors at compiled native pointers.</summary>
    private static void AddFamily(Bgr555[][] destination,
        SamusFullBodyCycleFamily family, PaletteRgb5[][][]? source)
    {
        if (source is null || source.Length != SamusFullBodyCycleColorFormat.SuitCount)
            throw new InvalidDataException($"{family} requires three suit palettes.");
        for (int suit = 0; suit < source.Length; suit++)
        {
            PaletteRgb5[][]? shades = source[suit];
            if (shades is null || shades.Length != SamusFullBodyCycleColorFormat.ShadesPerSuit)
                throw new InvalidDataException($"{family} suit {suit} requires four shades.");
            for (int shade = 0; shade < shades.Length; shade++)
            {
                PaletteRgb5[]? color = shades[shade];
                if (color is null || color.Length != SamusFullBodyCycleColorFormat.ColorsPerPalette)
                    throw new InvalidDataException($"{family} suit {suit}, shade {shade} requires sixteen colors.");
                var words = new Bgr555[color.Length];
                for (int index = 0; index < color.Length; index++)
                {
                    PaletteRgb5? rgb = color[index];
                    if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                        (uint)rgb.Blue > 31)
                        throw new InvalidDataException(
                            $"{family} suit {suit}, shade {shade}, color {index} requires RGB5 channels 0..31.");
                    words[index] = rgb.ToBgr555();
                }
                ushort pointer = SamusFullBodyCycleColorFormat.Pointer(family, suit, shade);
                int paletteIndex = SamusFullBodyCycleColorFormat.PaletteIndex(pointer);
                if (destination[paletteIndex] is not null)
                    throw new InvalidDataException($"Duplicate full-body palette pointer ${pointer:X4}.");
                destination[paletteIndex] = words;
            }
        }
    }

}
