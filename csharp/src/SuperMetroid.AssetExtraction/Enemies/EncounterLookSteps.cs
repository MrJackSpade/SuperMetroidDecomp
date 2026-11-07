namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One encounter's looks and what each copies over the sprite characters to show it (#1538): a look shows every copy of
/// the looks up to it, in order, over the room's load.
/// </summary>
/// <param name="Encounter">The encounter.</param>
/// <param name="Looks">What each look copies, by look in the order the encounter shows them; nothing for the room's load.</param>
internal sealed record EncounterLookSteps(ComposedEncounter Encounter, IReadOnlyList<IReadOnlyList<EncounterArtworkUpload>> Looks);
