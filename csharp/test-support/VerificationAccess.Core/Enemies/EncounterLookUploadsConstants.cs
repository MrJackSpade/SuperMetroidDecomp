using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EncounterLookUploads"/> that only verification reads.</summary>
internal static class EncounterLookUploadsConstants
{
    /// <summary>The place among <paramref name="encounter"/>'s looks of the look gameplay names as <paramref name="look"/>.</summary>
    /// <exception cref="InvalidDataException">The look is none of the encounter's.</exception>
    internal static int IndexOf(ComposedEncounter encounter, EncounterLook look) => EncounterLookUploads.IndexOf(encounter, ComposedLook.Of(encounter, look));
}
