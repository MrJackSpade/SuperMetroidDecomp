
namespace SuperMetroid.Core.Fx;

/// <summary>An FX definition and its index.</summary>
/// <param name="Fx">The fx.</param>
/// <param name="Index">The index.</param>
internal readonly record struct IndexedFx(RoomFxDefinition Fx, int Index);
