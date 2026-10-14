namespace SuperMetroid.Core.Game;

/// <summary>One shared native <c>(signed Y offset, timer)</c> corpse-rotting record.</summary>
internal readonly record struct CorpseRottingTableEntry(short YOffset, ushort Timer);
