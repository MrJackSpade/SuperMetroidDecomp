namespace SuperMetroid.Core.Runtime;

/// <summary>A door loader's deferred write of Samus's whole position.</summary>
internal readonly record struct LoaderSamusPlacement(int EnemySlot, Position16<RoomPixels> Position);
