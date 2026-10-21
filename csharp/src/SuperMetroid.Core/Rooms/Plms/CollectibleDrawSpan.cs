namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A run of collectible draw words.</summary>
/// <param name="First">The first.</param>
/// <param name="Count">The count.</param>
internal readonly record struct CollectibleDrawSpan(int First, int Count);
