namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>The destination word and tiles of one crocomire body visuals native.</summary>
/// <param name="DestinationWord">The destination word.</param>
/// <param name="Tiles">The tiles.</param>
internal readonly record struct CrocomireBodyVisualsNative(ushort DestinationWord, ushort[] Tiles);
