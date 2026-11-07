using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// How one encounter's looks change a frame (#1538): the encounter, and the names of its looks the frame is drawn in, of
/// each of which the sheet holds a finished picture.
/// </summary>
/// <param name="Encounter">The encounter whose looks change the frame.</param>
/// <param name="Looks">The names of the encounter's looks the frame is drawn in, in the order the encounter shows them.</param>
internal sealed record FrameLookChange(EncounterArtwork Encounter, IReadOnlyList<string> Looks);
