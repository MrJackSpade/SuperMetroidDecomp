namespace SuperMetroid.Core.Game;

internal static class WorkRobotInstructionProgramDefinitions
{
    /// <summary><c>InstList_RobotNoPower_Neutral</c> at $A8:C6D3.</summary>
    public const ushort NoPowerNeutral = 0xc6d3;
    /// <summary><c>InstList_RobotNoPower_LeaningLeft</c> at $A8:C6D9.</summary>
    public const ushort NoPowerLeaningLeft = 0xc6d9;
    /// <summary><c>InstList_RobotNoPower_LeaningRight</c> at $A8:C6DF.</summary>
    public const ushort NoPowerLeaningRight = 0xc6df;
    /// <summary><c>InstList_Robot_Initial</c> at $A8:C6E5.</summary>
    public const ushort Initial = 0xc6e5;
    /// <summary><c>InstList_Robot_FacingLeft_WalkingForwards</c> at $A8:C6E9.</summary>
    public const ushort FacingLeftWalkingForwards = 0xc6e9;
    /// <summary><c>InstList_Robot_FacingLeft_HitWallMovingForwards</c> at $A8:C73F.</summary>
    public const ushort FacingLeftHitWallMovingForwards = 0xc73f;
    /// <summary><c>InstList_Robot_FacingLeft_Shot_SamusAhead</c> at $A8:C7BB.</summary>
    public const ushort FacingLeftShotSamusAhead = 0xc7bb;
    /// <summary><c>InstList_Robot_FacingLeft_Shot_SamusBehind</c> at $A8:C833.</summary>
    public const ushort FacingLeftShotSamusBehind = 0xc833;
    /// <summary><c>InstList_Robot_FacingLeft_ShotLaserDownLeft</c> at $A8:C8B1.</summary>
    public const ushort FacingLeftShotLaserDownLeft = 0xc8b1;
    /// <summary><c>InstList_Robot_FacingLeft_ShotLaserLeft</c> at $A8:C8BD.</summary>
    public const ushort FacingLeftShotLaserLeft = 0xc8bd;
    /// <summary><c>InstList_Robot_FacingLeft_ShotLaserUpLeft</c> at $A8:C8D1.</summary>
    public const ushort FacingLeftShotLaserUpLeft = 0xc8d1;
    /// <summary><c>InstList_Robot_FacingLeft_LaserShotRecoil</c> at $A8:C8E9.</summary>
    public const ushort FacingLeftLaserShotRecoil = 0xc8e9;
    /// <summary><c>InstList_Robot_ApproachingFallRight</c> at $A8:C91B.</summary>
    public const ushort ApproachingFallRight = 0xc91b;
    /// <summary><c>InstList_Robot_FacingRight_WalkingForwards</c> at $A8:C92D.</summary>
    public const ushort FacingRightWalkingForwards = 0xc92d;
    /// <summary><c>InstList_Robot_FacingRight_HitWallMovingForwards</c> at $A8:C985.</summary>
    public const ushort FacingRightHitWallMovingForwards = 0xc985;
    /// <summary><c>InstList_Robot_FacingRight_Shot_SamusAhead</c> at $A8:CA01.</summary>
    public const ushort FacingRightShotSamusAhead = 0xca01;
    /// <summary><c>InstList_Robot_FacingRight_Shot_SamusBehind</c> at $A8:CA7D.</summary>
    public const ushort FacingRightShotSamusBehind = 0xca7d;
    /// <summary><c>InstList_Robot_FacingRight_ShotLaserDownRight</c> at $A8:CAFD.</summary>
    public const ushort FacingRightShotLaserDownRight = 0xcafd;
    /// <summary><c>InstList_Robot_FacingRight_ShotLaserRight</c> at $A8:CB09.</summary>
    public const ushort FacingRightShotLaserRight = 0xcb09;
    /// <summary><c>InstList_Robot_FacingRight_ShotLaserUpRight</c> at $A8:CB1D.</summary>
    public const ushort FacingRightShotLaserUpRight = 0xcb1d;
    /// <summary><c>InstList_Robot_FacingRight_LaserShotRecoil</c> at $A8:CB35.</summary>
    public const ushort FacingRightLaserShotRecoil = 0xcb35;
    /// <summary><c>InstList_Robot_ApproachingFallLeft</c> at $A8:CB65.</summary>
    public const ushort ApproachingFallLeft = 0xcb65;

    private const int PresentationOperand = -1;
    private const ushort FirstWordAddress = NoPowerNeutral;
    private const ushort EndAddress = 0xcb77;

    // This dense map covers the complete contiguous authored instruction region. Mechanics
    // words retain their native values; presentation entries are sentinels so spritemap
    // pointer values remain cartridge-owned and replaceable independently.
    private static readonly int[] Words =
    [
        0x7fff, PresentationOperand, 0x812f, 0x7fff, PresentationOperand, 0x812f, 0x7fff, PresentationOperand, 0x812f, 0x0020, PresentationOperand, 0x0001,
        PresentationOperand, 0x000a, PresentationOperand, 0x0001, PresentationOperand, 0xd107, 0x0009, PresentationOperand, 0x0001, PresentationOperand, 0xd0d2, 0x0009,
        PresentationOperand, 0x000a, PresentationOperand, 0xd091, 0xcd09, 0x000a, PresentationOperand, 0xcd09, 0x000a, PresentationOperand, 0x000a, PresentationOperand,
        0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0xcd09, 0xd091, 0x000a, PresentationOperand, 0xcd09, 0x0001,
        PresentationOperand, 0xd13d, 0x0009, PresentationOperand, 0x80ed, 0xc6ed, 0x0001, PresentationOperand, 0x000a, PresentationOperand, 0xd091, 0xcdea,
        0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0xd091, 0xcdea,
        0x000a, PresentationOperand, 0xcdea, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a,
        PresentationOperand, 0xcdea, 0xd091, 0x000a, PresentationOperand, 0xcdea, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand,
        0x000a, PresentationOperand, 0x000a, PresentationOperand, 0xd091, 0xcdea, 0x000a, PresentationOperand, 0xcdea, 0x000a, PresentationOperand, 0x000a,
        PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0xcecb, 0x0005, PresentationOperand, 0xd091, 0xce85,
        0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xd091, 0xce85,
        0x0005, PresentationOperand, 0xce85, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005,
        PresentationOperand, 0xce85, 0xd091, 0x0005, PresentationOperand, 0xce85, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand,
        0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xd091, 0xce85, 0x0005, PresentationOperand, 0xce85, 0x0005, PresentationOperand, 0x0005,
        PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xcecb, 0xd091, 0xcda4, 0x0005, PresentationOperand,
        0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xd091, 0xcda4, 0x0005, PresentationOperand, 0xcda4, 0x0005,
        PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xcda4, 0xd091, 0x0005,
        PresentationOperand, 0xcda4, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand,
        0xd091, 0xcda4, 0x0005, PresentationOperand, 0xcda4, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005,
        PresentationOperand, 0x0005, PresentationOperand, 0xcda4, 0xd091, 0x0005, PresentationOperand, 0xcda4, 0x0005, PresentationOperand, 0xd0c2, 0x0005,
        PresentationOperand, 0x0002, PresentationOperand, 0x80ed, 0xc8e9, 0x0005, PresentationOperand, 0x0002, PresentationOperand, 0xcdea, 0xd091, 0x000a,
        PresentationOperand, 0x80ed, 0xc8e9, 0x0005, PresentationOperand, 0x0002, PresentationOperand, 0x0002, PresentationOperand, 0x0004, PresentationOperand, 0xcdea,
        0xd091, 0x0004, PresentationOperand, 0xcdea, 0x0010, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand,
        0x0005, PresentationOperand, 0xd091, 0xcdea, 0x000a, PresentationOperand, 0xcdea, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x0060,
        PresentationOperand, 0xd16b, 0x80ed, 0xc6e9, 0x0080, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0xd16b, 0x80ed,
        0xc6e9, 0x0001, PresentationOperand, 0x0001, PresentationOperand, 0xd131, 0x0009, PresentationOperand, 0xd16b, 0x000a, PresentationOperand, 0x0001,
        PresentationOperand, 0xd100, 0x0009, PresentationOperand, 0x0001, PresentationOperand, 0xd0c6, 0x0009, PresentationOperand, 0x000a, PresentationOperand, 0xd091,
        0xcecf, 0x000a, PresentationOperand, 0xcecf, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand,
        0x000a, PresentationOperand, 0xcecf, 0xd091, 0x000a, PresentationOperand, 0xcecf, 0x80ed, 0xc931, 0x0001, PresentationOperand, 0x000a,
        PresentationOperand, 0xd091, 0xcfb0, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a,
        PresentationOperand, 0xd091, 0xcfb0, 0x000a, PresentationOperand, 0xcfb0, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand,
        0x000a, PresentationOperand, 0x000a, PresentationOperand, 0xd091, 0xcfb0, 0x000a, PresentationOperand, 0xcfb0, 0x000a, PresentationOperand, 0x000a,
        PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0xd091, 0xcfb0, 0x000a, PresentationOperand, 0xcfb0,
        0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0xd0c2, 0x0001,
        PresentationOperand, 0x0005, PresentationOperand, 0xd091, 0xd04b, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005,
        PresentationOperand, 0x0005, PresentationOperand, 0xd091, 0xd04b, 0x0005, PresentationOperand, 0xd04b, 0x0005, PresentationOperand, 0x0005, PresentationOperand,
        0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xd04b, 0xd091, 0x0005, PresentationOperand, 0xd04b, 0x0005,
        PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xd091, 0xd04b, 0x000a,
        PresentationOperand, 0xd04b, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand,
        0xd0c2, 0xcf6a, 0xd091, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005,
        PresentationOperand, 0xd091, 0xcf6a, 0x0005, PresentationOperand, 0xcf6a, 0xd091, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005,
        PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xcf6a, 0xd091, 0x0005, PresentationOperand, 0xcf6a, 0x0005, PresentationOperand,
        0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xd091, 0xcf6a, 0x0005, PresentationOperand,
        0xcf6a, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xcf6a,
        0xd091, 0x0005, PresentationOperand, 0xcf6a, 0xcecb, 0x0005, PresentationOperand, 0x0002, PresentationOperand, 0x80ed, 0xcb35, 0x0005,
        PresentationOperand, 0x0002, PresentationOperand, 0xd091, 0xcfb0, 0x000a, PresentationOperand, 0x80ed, 0xcb35, 0x0005, PresentationOperand, 0x0002,
        PresentationOperand, 0x0002, PresentationOperand, 0x0004, PresentationOperand, 0xd091, 0xcfb0, 0x0004, PresentationOperand, 0xcfb0, 0x0010, PresentationOperand,
        0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0x0005, PresentationOperand, 0xd091, 0xcfb0, 0x000a, PresentationOperand,
        0xcfb0, 0x000a, PresentationOperand, 0x000a, PresentationOperand, 0x0060, PresentationOperand, 0x80ed, 0xc92d, 0x0080, PresentationOperand, 0x000a,
        PresentationOperand, 0x000a, PresentationOperand, 0xd16b, 0x80ed, 0xc92d,
    ];

    internal const int MechanicsWordCount = 367;
    internal const int PresentationWordCount = 227;

    /// <summary>Returns compiled Work Robot control data or rejects presentation/data pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int byteOffset = address - FirstWordAddress;
        if ((byteOffset & 1) != 0 || byteOffset < 0 || address >= EndAddress)
            throw NotCompiled(address);

        int value = Words[byteOffset >> 1];
        if (value == PresentationOperand)
            throw NotCompiled(address);
        return unchecked((ushort)value);
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        for (int wordIndex = 0; wordIndex < Words.Length; wordIndex++)
        {
            if (Words[wordIndex] != PresentationOperand)
                continue;
            if (index-- == 0)
                return unchecked((ushort)(FirstWordAddress + wordIndex * 2));
        }

        throw new InvalidOperationException("Work Robot presentation-word index is inconsistent.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        int byteOffset = bankAddress - FirstWordAddress;
        if (byteOffset < 0 || bankAddress >= EndAddress)
            return false;

        int wordIndex = byteOffset >> 1;
        return Words[wordIndex] != PresentationOperand;
    }

    private static InvalidDataException NotCompiled(ushort address) =>
        new($"Work Robot instruction mechanics pointer $A8:{address:X4} is not compiled.");
}

