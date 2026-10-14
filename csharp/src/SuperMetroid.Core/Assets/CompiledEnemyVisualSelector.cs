namespace SuperMetroid.Core.Assets;

/// <summary>One immutable cartridge visual-pointer operand and its selected target.</summary>
internal readonly record struct CompiledEnemyVisualSelector(int Address, ushort Pointer);
