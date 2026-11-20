using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>A copy from one save slot to another (#2553).</summary>
/// <param name="Source">The slot copied from.</param>
/// <param name="Destination">The slot copied onto.</param>
internal readonly record struct FileCopyRoute(SaveSlot Source, SaveSlot Destination);
