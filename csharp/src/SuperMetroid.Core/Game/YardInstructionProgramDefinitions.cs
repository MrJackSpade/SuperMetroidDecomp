namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled simulation control for Yard's bank-$A3 crawling, turn, hiding, and airborne
/// instruction programs. Interleaved spritemap operands remain live cartridge presentation.
/// </summary>
internal static class YardInstructionProgramDefinitions
{
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideRight_MovingUp</c> at $A3:C8C6.</summary>
    public const ushort OutsideTurnUpsideRightMovingUp = 0xc8c6;
    /// <summary><c>InstList_Yard_Crawling_UpsideUp_MovingLeft</c> at $A3:C8E0.</summary>
    public const ushort CrawlingUpsideUpMovingLeft = 0xc8e0;
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideUp_MovingLeft</c> at $A3:C8FC.</summary>
    public const ushort OutsideTurnUpsideUpMovingLeft = 0xc8fc;
    /// <summary><c>InstList_Yard_Crawling_UpsideLeft_MovingDown</c> at $A3:C916.</summary>
    public const ushort CrawlingUpsideLeftMovingDown = 0xc916;
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideLeft_MovingDown</c> at $A3:C932.</summary>
    public const ushort OutsideTurnUpsideLeftMovingDown = 0xc932;
    /// <summary><c>InstList_Yard_Crawling_UpsideDown_MovingRight</c> at $A3:C94C.</summary>
    public const ushort CrawlingUpsideDownMovingRight = 0xc94c;
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideDown_MovingRight</c> at $A3:C968.</summary>
    public const ushort OutsideTurnUpsideDownMovingRight = 0xc968;
    /// <summary><c>InstList_Yard_Crawling_UpsideRight_MovingUp</c> at $A3:C982.</summary>
    public const ushort CrawlingUpsideRightMovingUp = 0xc982;
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideLeft_MovingUp</c> at $A3:C99E.</summary>
    public const ushort OutsideTurnUpsideLeftMovingUp = 0xc99e;
    /// <summary><c>InstList_Yard_Crawling_UpsideUp_MovingRight</c> at $A3:C9B8.</summary>
    public const ushort CrawlingUpsideUpMovingRight = 0xc9b8;
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideUp_MovingRight</c> at $A3:C9D4.</summary>
    public const ushort OutsideTurnUpsideUpMovingRight = 0xc9d4;
    /// <summary><c>InstList_Yard_Crawling_UpsideRight_MovingDown</c> at $A3:C9EE.</summary>
    public const ushort CrawlingUpsideRightMovingDown = 0xc9ee;
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideRight_MovingDown</c> at $A3:CA0A.</summary>
    public const ushort OutsideTurnUpsideRightMovingDown = 0xca0a;
    /// <summary><c>InstList_Yard_Crawling_UpsideDown_MovingLeft</c> at $A3:CA24.</summary>
    public const ushort CrawlingUpsideDownMovingLeft = 0xca24;
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideDown_MovingLeft</c> at $A3:CA40.</summary>
    public const ushort OutsideTurnUpsideDownMovingLeft = 0xca40;
    /// <summary><c>InstList_Yard_Crawling_UpsideLeft_MovingUp</c> at $A3:CA5A.</summary>
    public const ushort CrawlingUpsideLeftMovingUp = 0xca5a;
    /// <summary><c>InstList_Yard_InsideTurn_UpsideUp_MovingLeft</c> at $A3:CA76.</summary>
    public const ushort InsideTurnUpsideUpMovingLeft = 0xca76;
    /// <summary><c>InstList_Yard_InsideTurn_UpsideRight_MovingUp</c> at $A3:CA8E.</summary>
    public const ushort InsideTurnUpsideRightMovingUp = 0xca8e;
    /// <summary><c>InstList_Yard_InsideTurn_UpsideDown_MovingRight</c> at $A3:CAA6.</summary>
    public const ushort InsideTurnUpsideDownMovingRight = 0xcaa6;
    /// <summary><c>InstList_Yard_InsideTurn_UpsideLeft_MovingDown</c> at $A3:CABE.</summary>
    public const ushort InsideTurnUpsideLeftMovingDown = 0xcabe;
    /// <summary><c>InstList_Yard_InsideTurn_UpsideUp_MovingRight</c> at $A3:CAD6.</summary>
    public const ushort InsideTurnUpsideUpMovingRight = 0xcad6;
    /// <summary><c>InstList_Yard_InsideTurn_UpsideLeft_MovingUp</c> at $A3:CAEE.</summary>
    public const ushort InsideTurnUpsideLeftMovingUp = 0xcaee;
    /// <summary><c>InstList_Yard_InsideTurn_UpsideDown_MovingLeft</c> at $A3:CB06.</summary>
    public const ushort InsideTurnUpsideDownMovingLeft = 0xcb06;
    /// <summary><c>InstList_Yard_InsideTurn_UpsideRight_MovingDown</c> at $A3:CB1E.</summary>
    public const ushort InsideTurnUpsideRightMovingDown = 0xcb1e;
    /// <summary><c>InstList_Yard_Hiding_UpsideUp_MovingLeft</c> at $A3:CB36.</summary>
    public const ushort HidingUpsideUpMovingLeft = 0xcb36;
    /// <summary><c>InstList_Yard_Hidden_UpsideUp_MovingLeft</c> at $A3:CB44.</summary>
    public const ushort HiddenUpsideUpMovingLeft = 0xcb44;
    /// <summary><c>InstList_Yard_Hiding_UpsideDown_MovingLeft</c> at $A3:CB50.</summary>
    public const ushort HidingUpsideDownMovingLeft = 0xcb50;
    /// <summary><c>InstList_Yard_Hiding_UpsideDown_MovingRight</c> at $A3:CB6A.</summary>
    public const ushort HidingUpsideDownMovingRight = 0xcb6a;
    /// <summary><c>InstList_Yard_Hiding_UpsideUp_MovingRight</c> at $A3:CB84.</summary>
    public const ushort HidingUpsideUpMovingRight = 0xcb84;
    /// <summary><c>InstList_Yard_Hidden_UpsideUp_MovingRight</c> at $A3:CB92.</summary>
    public const ushort HiddenUpsideUpMovingRight = 0xcb92;
    /// <summary><c>InstList_Yard_Hiding_UpsideRight_MovingUp</c> at $A3:CB9E.</summary>
    public const ushort HidingUpsideRightMovingUp = 0xcb9e;
    /// <summary><c>InstList_Yard_Hiding_UpsideLeft_MovingUp</c> at $A3:CBB8.</summary>
    public const ushort HidingUpsideLeftMovingUp = 0xcbb8;
    /// <summary><c>InstList_Yard_Hiding_UpsideLeft_MovingDown</c> at $A3:CBD2.</summary>
    public const ushort HidingUpsideLeftMovingDown = 0xcbd2;
    /// <summary><c>InstList_Yard_Hiding_UpsideRight_MovingDown</c> at $A3:CBEC.</summary>
    public const ushort HidingUpsideRightMovingDown = 0xcbec;
    /// <summary><c>InstList_Yard_Airborne_FacingLeft_0</c> at $A3:CC06.</summary>
    public const ushort AirborneFacingLeft = 0xcc06;
    /// <summary><c>InstList_Yard_Airborne_FacingLeft_1</c> loop entry at $A3:CC0E.</summary>
    public const ushort AirborneFacingLeftLoop = 0xcc0e;
    /// <summary><c>InstList_Yard_Airborne_FacingRight_0</c> at $A3:CC1E.</summary>
    public const ushort AirborneFacingRight = 0xcc1e;
    /// <summary><c>InstList_Yard_Airborne_FacingRight_1</c> loop entry at $A3:CC26.</summary>
    public const ushort AirborneFacingRightLoop = 0xcc26;

    private const int PresentationOperand = -1;
    private const ushort FirstWordAddress = OutsideTurnUpsideRightMovingUp;
    private const ushort EndAddress = 0xcc36;

    private static readonly int[] Words =
    [
        0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0xcc5f, 0xfffc,
        0xfff8, 0xcc36, 0xcfa6, 0xcc3f, 0xcb36, 0xcc48, 0x0006, 0x0009, PresentationOperand, 0x000d, PresentationOperand, 0x0009,
        PresentationOperand, 0x80ed, 0xc8e0, 0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007,
        PresentationOperand, 0xcc5f, 0xfff8, 0x0004, 0xcc36, 0xcfb7, 0xcc3f, 0xcbd2, 0xcc48, 0x0003, 0x0009, PresentationOperand,
        0x000d, PresentationOperand, 0x0009, PresentationOperand, 0x80ed, 0xc916, 0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand,
        0x0004, PresentationOperand, 0x0007, PresentationOperand, 0xcc5f, 0x0004, 0x0008, 0xcc36, 0xcfbd, 0xcc3f, 0xcb6a, 0xcc48,
        0x0005, 0x0009, PresentationOperand, 0x000d, PresentationOperand, 0x0009, PresentationOperand, 0x80ed, 0xc94c, 0xcc36, 0xcf5f, 0xcc3f,
        0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0xcc5f, 0x0008, 0xfffc, 0xcc36, 0xcfce,
        0xcc3f, 0xcb9e, 0xcc48, 0x0000, 0x0009, PresentationOperand, 0x000d, PresentationOperand, 0x0009, PresentationOperand, 0x80ed, 0xc982,
        0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0xcc5f, 0x0004,
        0xfff8, 0xcc36, 0xcfd4, 0xcc3f, 0xcb84, 0xcc48, 0x0007, 0x0009, PresentationOperand, 0x000d, PresentationOperand, 0x0009,
        PresentationOperand, 0x80ed, 0xc9b8, 0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007,
        PresentationOperand, 0xcc5f, 0x0008, 0x0004, 0xcc36, 0xcfe5, 0xcc3f, 0xcbec, 0xcc48, 0x0001, 0x0009, PresentationOperand,
        0x000d, PresentationOperand, 0x0009, PresentationOperand, 0x80ed, 0xc9ee, 0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand,
        0x0004, PresentationOperand, 0x0007, PresentationOperand, 0xcc5f, 0xfffc, 0x0008, 0xcc36, 0xcfeb, 0xcc3f, 0xcb50, 0xcc48,
        0x0004, 0x0009, PresentationOperand, 0x000d, PresentationOperand, 0x0009, PresentationOperand, 0x80ed, 0xca24, 0xcc36, 0xcf5f, 0xcc3f,
        0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0xcc5f, 0xfff8, 0xfffc, 0xcc36, 0xcffc,
        0xcc3f, 0xcbb8, 0xcc48, 0x0002, 0x0009, PresentationOperand, 0x000d, PresentationOperand, 0x0009, PresentationOperand, 0x80ed, 0xca5a,
        0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0x80ed, 0xc982,
        0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0x80ed, 0xc94c,
        0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0x80ed, 0xc916,
        0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0x80ed, 0xc8e0,
        0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0x80ed, 0xca5a,
        0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0x80ed, 0xca24,
        0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0x80ed, 0xc9ee,
        0xcc36, 0xcf5f, 0xcc3f, 0xcf5f, 0x0007, PresentationOperand, 0x0004, PresentationOperand, 0x0007, PresentationOperand, 0x80ed, 0xc9b8,
        0xcc36, 0xcf60, 0x0005, PresentationOperand, 0x0001, PresentationOperand, 0xcc78, 0x0030, PresentationOperand, 0x0010, PresentationOperand, 0x80ed,
        0xc8e0, 0xcc36, 0xcf60, 0x0005, PresentationOperand, 0x0001, PresentationOperand, 0xcc78, 0x0030, PresentationOperand, 0x0010, PresentationOperand,
        0x80ed, 0xca24, 0xcc36, 0xcf60, 0x0005, PresentationOperand, 0x0001, PresentationOperand, 0xcc78, 0x0030, PresentationOperand, 0x0010,
        PresentationOperand, 0x80ed, 0xc94c, 0xcc36, 0xcf60, 0x0005, PresentationOperand, 0x0001, PresentationOperand, 0xcc78, 0x0030, PresentationOperand,
        0x0010, PresentationOperand, 0x80ed, 0xc9b8, 0xcc36, 0xcf60, 0x0005, PresentationOperand, 0x0001, PresentationOperand, 0xcc78, 0x0030,
        PresentationOperand, 0x0010, PresentationOperand, 0x80ed, 0xc982, 0xcc36, 0xcf60, 0x0005, PresentationOperand, 0x0001, PresentationOperand, 0xcc78,
        0x0030, PresentationOperand, 0x0010, PresentationOperand, 0x80ed, 0xca5a, 0xcc36, 0xcf60, 0x0005, PresentationOperand, 0x0001, PresentationOperand,
        0xcc78, 0x0030, PresentationOperand, 0x0010, PresentationOperand, 0x80ed, 0xc916, 0xcc36, 0xcf60, 0x0005, PresentationOperand, 0x0001,
        PresentationOperand, 0xcc78, 0x0030, PresentationOperand, 0x0010, PresentationOperand, 0x80ed, 0xc9ee, 0xcc36, 0xd1b3, 0x0003, PresentationOperand,
        0x0003, PresentationOperand, 0x0003, PresentationOperand, 0x0003, PresentationOperand, 0x80ed, 0xcc0e, 0xcc36, 0xd1b3, 0x0003, PresentationOperand,
        0x0003, PresentationOperand, 0x0003, PresentationOperand, 0x0003, PresentationOperand, 0x80ed, 0xcc26,
    ];

    internal const int MechanicsWordCount = 328;
    internal const int PresentationWordCount = 112;

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
        throw new InvalidOperationException("Yard presentation-word index is inconsistent.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        int byteOffset = bankAddress - FirstWordAddress;
        if (byteOffset < 0 || bankAddress >= EndAddress)
            return false;
        return Words[byteOffset >> 1] != PresentationOperand;
    }

    private static InvalidDataException NotCompiled(ushort address) =>
        new($"Yard instruction mechanics pointer $A3:{address:X4} is not compiled.");
}
