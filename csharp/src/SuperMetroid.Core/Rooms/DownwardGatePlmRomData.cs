namespace SuperMetroid.Core.Rooms;

/// <summary>Named cartridge constants for bank-$84 downward gates.</summary>
internal static class DownwardGatePlmRomData
{
    public const byte GateHeightInBlocks = 5;
    public const byte ClosedGateBts = 0x10;
    public const byte RejectedShotSound = 0x57;
    public const byte MovementSound = 0x0e;
}

/// <summary>Temporary gate-trigger PLM headers selected by shootable BTS $46-$4D.</summary>
internal static class DownwardGateTriggerPlmHeaders
{
    public const ushort GreenLeft = 0xc806;
    public const ushort GreenRight = 0xc80a;
    public const ushort RedLeft = 0xc80e;
    public const ushort RedRight = 0xc812;
    public const ushort BlueLeft = 0xc816;
    public const ushort BlueRight = 0xc81a;
    public const ushort YellowLeft = 0xc81e;
    public const ushort YellowRight = 0xc822;
}

/// <summary>Bank-$84 pre-instruction callbacks used by the resident gate coroutine.</summary>
internal static class DownwardGatePreInstructionCodes
{
    public const ushort WakeIfTriggered = 0xbb52;
    public const ushort WakeIfTriggeredOrSamusBelow = 0xbb6b;
    public const ushort Inert = 0xbb6a;
}

/// <summary>The exact projectile action published by a gate instruction to bank $86.</summary>
/// <param name="Operation">Whether to allocate a gate actor or wake an already associated actor; applying the request belongs to the runtime's projectile owner.</param>
/// <param name="DefinitionPointer">Bank-$86 moving/initially closed gate header for Spawn; unused and published as zero for Wake.</param>
/// <param name="PlmBlockIndex">Nonnegative row-major level-block ordinal for the gate's top cell, not a pixel coordinate or native byte offset; each block is sixteen pixels and the native association stores twice this index.</param>
public readonly record struct DownwardGateProjectileRequest(
    DownwardGateProjectileOperation Operation,
    ushort DefinitionPointer,
    int PlmBlockIndex);

/// <summary>Mutually exclusive work transferred from a downward-gate PLM to the bank-$86 projectile pool.</summary>
public enum DownwardGateProjectileOperation
{
    /// <summary>$84:BBE1, Instruction_PLM_SpawnEnemyProjectileY, or closed-gate setup: allocates the selected gate actor and associates it with the PLM block; a full pool leaves no new actor.</summary>
    Spawn,
    /// <summary>$84:BBF0, Instruction_PLM_WakeEnemyProjectileAtPLMsPosition: finds the associated actor, advances its instruction pointer two bytes past sleep, and sets its instruction timer to one without allocating another actor.</summary>
    Wake,
}
