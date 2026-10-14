namespace SuperMetroid.Core.Game;

/// <summary>One native <c>(signed Y offset, timer)</c> Mother Brain corpse-rotting table record.</summary>
internal readonly record struct MotherBrainCorpseRotEntry(short YOffset, ushort Timer);
