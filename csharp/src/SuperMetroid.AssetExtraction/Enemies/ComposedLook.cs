using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One look of an encounter the import composes (#1538, #2548): the encounter and the look's name, the key its finished
/// pictures are installed under, written out once where the look is declared. A drawing lists the looks it is drawn in by
/// this identity, and an encounter finds a look's place among its looks by it, never by searching its names.
/// </summary>
/// <param name="Encounter">The encounter whose look it is.</param>
/// <param name="Name">The look's installed name.</param>
internal readonly record struct ComposedLook(ComposedEncounter Encounter, [InstalledKey] string Name)
{
    /// <summary>The look <paramref name="encounter"/> shows as its room loads, until it shows another.</summary>
    internal static ComposedLook Loaded(ComposedEncounter encounter) => new(encounter, EncounterLook.LoadedName);

    /// <summary>The look of <paramref name="encounter"/> gameplay names as <paramref name="look"/>.</summary>
    internal static ComposedLook Of(ComposedEncounter encounter, EncounterLook look) => new(encounter, look.Name);

    /// <summary>Names the look for diagnostics.</summary>
    public override string ToString() => $"{Encounter} {Name}";
}
