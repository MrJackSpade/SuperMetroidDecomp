namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The item a permanent-item PLM holds and how it is presented.</summary>
/// <param name="Kind">The item.</param>
/// <param name="Presentation">Whether the item is exposed, in a Chozo orb or in a concealed shot block.</param>
internal readonly record struct PermanentCollectiblePlm(InWorldCollectibleKind Kind, CollectiblePresentation Presentation);
