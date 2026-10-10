using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled simulation control for Yard's bank-$A3 crawling, turn, hiding, and airborne
/// instruction programs. Interleaved spritemap operands remain live cartridge presentation.
/// </summary>
/// <remarks>
/// Independently reviewed for #1165 against pinned bank_A3 InstList_Yard_* and every native
/// word. Programs are written as their instructions; word addresses follow from layout order.
/// Frame durations are Yard's authored animation cadence and the movement-function and
/// direction operands name the routines they select.
/// </remarks>
internal abstract class YardInstructionProgramDefinitions
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

    /// <summary><c>RTL_A3CF5F</c> at $A3:CF5F.</summary>
    private const ushort EmptyLongRoutineCF5F = 0xcf5f;
    /// <summary><c>Function_Yard_Movement_Hiding</c> at $A3:CF60.</summary>
    private const ushort MovementHidingFunction = 0xcf60;
    /// <summary><c>Function_Yard_Movement_Crawling_UpsideUp_MovingLeft</c> at $A3:CFA6.</summary>
    private const ushort MovementCrawlingUpsideUpMovingLeftFunction = 0xcfa6;
    /// <summary><c>Function_Yard_Movement_Crawling_UpsideLeft_MovingDown</c> at $A3:CFB7.</summary>
    private const ushort MovementCrawlingUpsideLeftMovingDownFunction = 0xcfb7;
    /// <summary><c>Function_Yard_Movement_Crawling_UpsideDown_MovingRight</c> at $A3:CFBD.</summary>
    private const ushort MovementCrawlingUpsideDownMovingRightFunction = 0xcfbd;
    /// <summary><c>Function_Yard_Movement_Crawling_UpsideRight_MovingUp</c> at $A3:CFCE.</summary>
    private const ushort MovementCrawlingUpsideRightMovingUpFunction = 0xcfce;
    /// <summary><c>Function_Yard_Movement_Crawling_UpsideUp_MovingRight</c> at $A3:CFD4.</summary>
    private const ushort MovementCrawlingUpsideUpMovingRightFunction = 0xcfd4;
    /// <summary><c>Function_Yard_Movement_Crawling_UpsideRight_MovingDown</c> at $A3:CFE5.</summary>
    private const ushort MovementCrawlingUpsideRightMovingDownFunction = 0xcfe5;
    /// <summary><c>Function_Yard_Movement_Crawling_UpsideDown_MovingLeft</c> at $A3:CFEB.</summary>
    private const ushort MovementCrawlingUpsideDownMovingLeftFunction = 0xcfeb;
    /// <summary><c>Function_Yard_Movement_Crawling_UpsideLeft_MovingUp</c> at $A3:CFFC.</summary>
    private const ushort MovementCrawlingUpsideLeftMovingUpFunction = 0xcffc;
    /// <summary><c>Function_Yard_Movement_Airborne</c> at $A3:D1B3.</summary>
    private const ushort MovementAirborneFunction = 0xd1b3;

    /// <summary>Native program bank $A3.</summary>
    internal const byte Bank = 0xa3;

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xc8c6),
        Entry(OutsideTurnUpsideRightMovingUp),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)YardInstruction.MoveByPixelsInY, 0xfffc, 0xfff8),
        Entry(CrawlingUpsideUpMovingLeft),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementCrawlingUpsideUpMovingLeftFunction),
        Op((ushort)YardInstruction.HidingInstListInY, HidingUpsideUpMovingLeft),
        Op((ushort)YardInstruction.DirectionInY, 0x0006),
        Frame(9),
        Frame(13),
        Frame(9),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideUpMovingLeft),
        Entry(OutsideTurnUpsideUpMovingLeft),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)YardInstruction.MoveByPixelsInY, 0xfff8, 0x0004),
        Entry(CrawlingUpsideLeftMovingDown),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementCrawlingUpsideLeftMovingDownFunction),
        Op((ushort)YardInstruction.HidingInstListInY, HidingUpsideLeftMovingDown),
        Op((ushort)YardInstruction.DirectionInY, 0x0003),
        Frame(9),
        Frame(13),
        Frame(9),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideLeftMovingDown),
        Entry(OutsideTurnUpsideLeftMovingDown),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)YardInstruction.MoveByPixelsInY, 0x0004, 0x0008),
        Entry(CrawlingUpsideDownMovingRight),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementCrawlingUpsideDownMovingRightFunction),
        Op((ushort)YardInstruction.HidingInstListInY, HidingUpsideDownMovingRight),
        Op((ushort)YardInstruction.DirectionInY, 0x0005),
        Frame(9),
        Frame(13),
        Frame(9),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideDownMovingRight),
        Entry(OutsideTurnUpsideDownMovingRight),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)YardInstruction.MoveByPixelsInY, 0x0008, 0xfffc),
        Entry(CrawlingUpsideRightMovingUp),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementCrawlingUpsideRightMovingUpFunction),
        Op((ushort)YardInstruction.HidingInstListInY, HidingUpsideRightMovingUp),
        Op((ushort)YardInstruction.DirectionInY, 0x0000),
        Frame(9),
        Frame(13),
        Frame(9),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideRightMovingUp),
        Entry(OutsideTurnUpsideLeftMovingUp),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)YardInstruction.MoveByPixelsInY, 0x0004, 0xfff8),
        Entry(CrawlingUpsideUpMovingRight),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementCrawlingUpsideUpMovingRightFunction),
        Op((ushort)YardInstruction.HidingInstListInY, HidingUpsideUpMovingRight),
        Op((ushort)YardInstruction.DirectionInY, 0x0007),
        Frame(9),
        Frame(13),
        Frame(9),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideUpMovingRight),
        Entry(OutsideTurnUpsideUpMovingRight),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)YardInstruction.MoveByPixelsInY, 0x0008, 0x0004),
        Entry(CrawlingUpsideRightMovingDown),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementCrawlingUpsideRightMovingDownFunction),
        Op((ushort)YardInstruction.HidingInstListInY, HidingUpsideRightMovingDown),
        Op((ushort)YardInstruction.DirectionInY, 0x0001),
        Frame(9),
        Frame(13),
        Frame(9),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideRightMovingDown),
        Entry(OutsideTurnUpsideRightMovingDown),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)YardInstruction.MoveByPixelsInY, 0xfffc, 0x0008),
        Entry(CrawlingUpsideDownMovingLeft),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementCrawlingUpsideDownMovingLeftFunction),
        Op((ushort)YardInstruction.HidingInstListInY, HidingUpsideDownMovingLeft),
        Op((ushort)YardInstruction.DirectionInY, 0x0004),
        Frame(9),
        Frame(13),
        Frame(9),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideDownMovingLeft),
        Entry(OutsideTurnUpsideDownMovingLeft),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)YardInstruction.MoveByPixelsInY, 0xfff8, 0xfffc),
        Entry(CrawlingUpsideLeftMovingUp),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementCrawlingUpsideLeftMovingUpFunction),
        Op((ushort)YardInstruction.HidingInstListInY, HidingUpsideLeftMovingUp),
        Op((ushort)YardInstruction.DirectionInY, 0x0002),
        Frame(9),
        Frame(13),
        Frame(9),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideLeftMovingUp),
        Entry(InsideTurnUpsideUpMovingLeft),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideRightMovingUp),
        Entry(InsideTurnUpsideRightMovingUp),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideDownMovingRight),
        Entry(InsideTurnUpsideDownMovingRight),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideLeftMovingDown),
        Entry(InsideTurnUpsideLeftMovingDown),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideUpMovingLeft),
        Entry(InsideTurnUpsideUpMovingRight),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideLeftMovingUp),
        Entry(InsideTurnUpsideLeftMovingUp),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideDownMovingLeft),
        Entry(InsideTurnUpsideDownMovingLeft),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideRightMovingDown),
        Entry(InsideTurnUpsideRightMovingDown),
        Op((ushort)YardInstruction.MovementFunctionInY, EmptyLongRoutineCF5F),
        Op((ushort)YardInstruction.HidingInstListInY, EmptyLongRoutineCF5F),
        Frame(7),
        Frame(4),
        Frame(7),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideUpMovingRight),
        Entry(HidingUpsideUpMovingLeft),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementHidingFunction),
        Frame(5),
        Frame(1),
        Op((ushort)YardInstruction.GoBack4BytesIfHidingOr50PercentChance),
        Entry(HiddenUpsideUpMovingLeft),
        Frame(48),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideUpMovingLeft),
        Entry(HidingUpsideDownMovingLeft),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementHidingFunction),
        Frame(5),
        Frame(1),
        Op((ushort)YardInstruction.GoBack4BytesIfHidingOr50PercentChance),
        Frame(48),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideDownMovingLeft),
        Entry(HidingUpsideDownMovingRight),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementHidingFunction),
        Frame(5),
        Frame(1),
        Op((ushort)YardInstruction.GoBack4BytesIfHidingOr50PercentChance),
        Frame(48),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideDownMovingRight),
        Entry(HidingUpsideUpMovingRight),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementHidingFunction),
        Frame(5),
        Frame(1),
        Op((ushort)YardInstruction.GoBack4BytesIfHidingOr50PercentChance),
        Entry(HiddenUpsideUpMovingRight),
        Frame(48),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideUpMovingRight),
        Entry(HidingUpsideRightMovingUp),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementHidingFunction),
        Frame(5),
        Frame(1),
        Op((ushort)YardInstruction.GoBack4BytesIfHidingOr50PercentChance),
        Frame(48),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideRightMovingUp),
        Entry(HidingUpsideLeftMovingUp),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementHidingFunction),
        Frame(5),
        Frame(1),
        Op((ushort)YardInstruction.GoBack4BytesIfHidingOr50PercentChance),
        Frame(48),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideLeftMovingUp),
        Entry(HidingUpsideLeftMovingDown),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementHidingFunction),
        Frame(5),
        Frame(1),
        Op((ushort)YardInstruction.GoBack4BytesIfHidingOr50PercentChance),
        Frame(48),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideLeftMovingDown),
        Entry(HidingUpsideRightMovingDown),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementHidingFunction),
        Frame(5),
        Frame(1),
        Op((ushort)YardInstruction.GoBack4BytesIfHidingOr50PercentChance),
        Frame(48),
        Frame(16),
        Op((ushort)CommonEnemyInstruction.Goto, CrawlingUpsideRightMovingDown),
        Entry(AirborneFacingLeft),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementAirborneFunction),
        Frame(3),
        Entry(AirborneFacingLeftLoop),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)CommonEnemyInstruction.Goto, AirborneFacingLeftLoop),
        Entry(AirborneFacingRight),
        Op((ushort)YardInstruction.MovementFunctionInY, MovementAirborneFunction),
        Frame(3),
        Entry(AirborneFacingRightLoop),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)CommonEnemyInstruction.Goto, AirborneFacingRightLoop));
    public static int PresentationWordCount => Layout.PresentationSlotCount;

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value : throw NotCompiled(address);

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return Layout.PresentationSlotAddress(index);
    }

    /// <summary>Tests one native operand for an installed presentation slot.</summary>
    internal static bool IsPresentationWordAddress(ushort address) => Layout.IsPresentationWord(address);

    private static InvalidDataException NotCompiled(ushort address) =>
        new($"Yard instruction mechanics pointer $A3:{address:X4} is not compiled.");
}
