namespace SuperMetroid.Core.Game;

/// <summary>One cropped tile row copied from the corpse sheet to its rotting surface.</summary>
internal readonly record struct DeadTorizoGraphicsCopy(int SourceOffset, int DestinationOffset, int Length);
