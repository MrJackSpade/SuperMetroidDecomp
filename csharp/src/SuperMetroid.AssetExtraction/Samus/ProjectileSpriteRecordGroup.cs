using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// One group of composition records the cartridge stores together (#2565): the compositions its records hold, in storage
/// order, and where each of its records lies, from where the group starts as its layout of record sizes says.
/// </summary>
/// <typeparam name="TAddress">How the cartridge's place of a record is named.</typeparam>
/// <param name="Compositions">The composition each of the group's records holds, in storage order.</param>
/// <param name="RecordAt">Where the group's record at a place in its storage order lies.</param>
internal sealed record ProjectileSpriteRecordGroup<TAddress>(IReadOnlyList<ProjectileSpriteId> Compositions, Func<int, TAddress> RecordAt)
{
    /// <summary>Each of the group's records, where it lies, with the composition it holds, in storage order.</summary>
    internal IEnumerable<KeyValuePair<TAddress, ProjectileSpriteId>> Records() =>
        Compositions.Select((composition, place) => KeyValuePair.Create(RecordAt(place), composition));
}
