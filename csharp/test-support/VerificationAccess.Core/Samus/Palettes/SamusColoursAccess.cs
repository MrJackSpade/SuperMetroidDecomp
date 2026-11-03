using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Verification access to <see cref="SamusColours"/> (#1512): the colours her sheets' inks show under the shading she
/// publishes, resolved through the shades the import pre-colours from <see cref="SamusShadeSources"/>. A check compares
/// these with the colours the cartridge shows, so it confirms both the shade her state names and the import's colours
/// for it.
/// </summary>
internal static class SamusColoursAccess
{
    /// <summary>The inks of one of her sheets: eight sprite colour groups.</summary>
    private const int SheetInks = 8 * SamusSheetInks.GroupColours;

    /// <summary>The import's shades by key, computed once per set of colour tables.</summary>
    private static readonly ConditionalWeakTable<SamusShadeSources, Dictionary<string, SheetShade>> shadesBySources = new();

    /// <summary>Reads Samus's colours as a check compares them.</summary>
    extension(SamusColours self)
    {
        /// <summary>
        /// The colour each of her sheets' inks shows under her shading, the shades resolved from
        /// <paramref name="sources"/>; inks no shade covers show black.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Her shading names a shade the import does not make.</exception>
        internal RgbColor[] ShownInks(SamusShadeSources sources)
        {
            Dictionary<string, SheetShade> shades = shadesBySources.GetValue(sources,
                key => SamusSheetShades.Of(key).ToDictionary(shade => shade.Shade.Key, StringComparer.Ordinal));
            var inks = new RgbColor[SheetInks];
            foreach (PictureShade shade in self.Shading.Shades)
            {
                SheetShade shown = shades[shade.Key];
                for (int index = 0; index < shown.Inks.Length; index++)
                    inks[shown.Inks[index]] = shown.Colours[index];
            }
            return inks;
        }

        /// <summary>The sixteen colours her suit shows.</summary>
        internal RgbColor[] ShownSuit(SamusShadeSources sources) =>
            self.ShownInks(sources)[SamusSheetInks.Suit..(SamusSheetInks.Suit + SamusColourDefinitions.SuitColours)];

        /// <summary>The sixteen colours her beams show.</summary>
        internal RgbColor[] ShownBeams(SamusShadeSources sources) =>
            self.ShownInks(sources)[SamusSheetInks.Beams..(SamusSheetInks.Beams + SamusColourDefinitions.SuitColours)];

        /// <summary>The sixteen colours her body without the suit shows.</summary>
        internal RgbColor[] ShownSuitless(SamusShadeSources sources) =>
            self.ShownInks(sources)[SamusSheetInks.Suitless..(SamusSheetInks.Suitless + SamusColourDefinitions.SuitColours)];

        /// <summary>The colour her visor shows.</summary>
        internal RgbColor ShownVisor(SamusShadeSources sources) => self.ShownInks(sources)[SamusSheetInks.Suit + SamusSheetInks.VisorColour];

        /// <summary>The visor shade laid over her suit, or null while the suit's own visor colour shows.</summary>
        internal PictureShade? VisorShade => self.SuitOverlays.LastOrDefault(shade =>
            shade.Effect == SamusShades.Visor(0).Effect || shade == SamusShades.DoorCompletionVisor) is { Effect: not null } shade
                ? shade
                : null;

        /// <summary>The colour her grapple flare shows.</summary>
        internal RgbColor ShownGrappleFlare(SamusShadeSources sources) => self.ShownInks(sources)[SamusSheetInks.GrappleFlare];
        /// <summary>The shades laid over the suit's, oldest first.</summary>
        public IReadOnlyList<PictureShade> SuitOverlays => self.suitOverlays;
        /// <summary>The shades laid over the beams', oldest first.</summary>
        public IReadOnlyList<PictureShade> BeamOverlays => self.beamOverlays;
    }
}
