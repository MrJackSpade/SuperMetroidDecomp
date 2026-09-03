namespace SuperMetroid.Core.Rooms;

/// <summary>Named cartridge identities and table locations for bank-$84 downward gates.</summary>
internal static class DownwardGatePlmRomData
{
    public const ushort ShotBlockInstructionListTable = 0xc70a;
    public const ushort LeftShotBlockWordTable = 0xc71a;
    public const ushort RightShotBlockWordTable = 0xc72a;

    public const byte GateHeightInBlocks = 5;
    public const byte LastShotBlockTableByteOffset = 14;
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
public readonly record struct DownwardGateProjectileRequest(
    DownwardGateProjectileOperation Operation,
    ushort DefinitionPointer,
    int PlmBlockIndex);

public enum DownwardGateProjectileOperation
{
    Spawn,
    Wake,
}
