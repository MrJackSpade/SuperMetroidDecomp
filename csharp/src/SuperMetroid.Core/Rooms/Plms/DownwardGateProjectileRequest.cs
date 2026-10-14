namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The exact projectile action published by a gate instruction to bank $86.</summary>
/// <param name="Operation">Whether to allocate a gate actor or wake an already associated actor; applying the request belongs to the runtime's projectile owner.</param>
/// <param name="DefinitionPointer">Bank-$86 moving/initially closed gate header for Spawn; unused and published as zero for Wake.</param>
/// <param name="PlmBlockIndex">Nonnegative row-major level-block ordinal for the gate's top cell, not a pixel coordinate or native byte offset; each block is sixteen pixels and the native association stores twice this index.</param>
public readonly record struct DownwardGateProjectileRequest(
    DownwardGateProjectileOperation Operation,
    ushort DefinitionPointer,
    int PlmBlockIndex);
