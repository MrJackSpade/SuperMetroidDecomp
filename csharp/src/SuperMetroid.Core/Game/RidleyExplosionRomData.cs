namespace SuperMetroid.Core.Game;

/// <summary>Parameters that select one authored component of Ridley's death breakup.</summary>
internal static class RidleyExplosionParts
{
    /// <summary>First tail segment.</summary>
    public const ushort Tail0 = 0x0000;
    /// <summary>Second tail segment.</summary>
    public const ushort Tail1 = 0x0002;
    /// <summary>Third tail segment.</summary>
    public const ushort Tail2 = 0x0004;
    /// <summary>Fourth tail segment.</summary>
    public const ushort Tail3 = 0x0006;
    /// <summary>Fifth tail segment.</summary>
    public const ushort Tail4 = 0x0008;
    /// <summary>Sixth tail segment.</summary>
    public const ushort Tail5 = 0x000a;
    /// <summary>Wing fragment.</summary>
    public const ushort Wings = 0x000e;
    /// <summary>Leg fragment.</summary>
    public const ushort Legs = 0x0010;
    /// <summary>Open-head and neck fragment.</summary>
    public const ushort OpenHeadAndNeck = 0x0012;
    /// <summary>Torso fragment.</summary>
    public const ushort Torso = 0x0014;
    /// <summary>Claw fragment.</summary>
    public const ushort Claw = 0x0016;
}

/// <summary>Bank-$A6 tables and lists used by Ridley's death-breakup actors.</summary>
internal static class RidleyExplosionRomData
{
    /// <summary>Tail-fragment initial velocity table at $A6:C6CE.</summary>
    public const int TailVelocityTable = 0xa6c6ce;
    /// <summary>Tail-angle instruction-list pointer table at $A6:C7BA.</summary>
    public const int TailAngleInstructionListTable = 0xa6c7ba;
    /// <summary>Wing instruction-list pointer table at $A6:C808.</summary>
    public const int WingInstructionListTable = 0xa6c808;
    /// <summary>Leg X-offset table at $A6:C836.</summary>
    public const int LegXOffsetTable = 0xa6c836;
    /// <summary>Leg instruction-list pointer table at $A6:C83A.</summary>
    public const int LegInstructionListTable = 0xa6c83a;
    /// <summary>Open-head X-offset table at $A6:C868.</summary>
    public const int OpenHeadXOffsetTable = 0xa6c868;
    /// <summary>Open-head instruction-list pointer table at $A6:C86C.</summary>
    public const int OpenHeadInstructionListTable = 0xa6c86c;
    /// <summary>Torso X-offset table at $A6:C89A.</summary>
    public const int TorsoXOffsetTable = 0xa6c89a;
    /// <summary>Torso instruction-list pointer table at $A6:C89E.</summary>
    public const int TorsoInstructionListTable = 0xa6c89e;
    /// <summary>Claw X-offset table at $A6:C8CC.</summary>
    public const int ClawXOffsetTable = 0xa6c8cc;
    /// <summary>Claw instruction-list pointer table at $A6:C8D0.</summary>
    public const int ClawInstructionListTable = 0xa6c8d0;

    /// <summary>$A6:CA47, InstList_RidleyTail_Large, loaded by initializers C70F/C727 for the first two segments.</summary>
    public const ushort LargeTailInstructionList = 0xca47;
    /// <summary>$A6:CA4D, InstList_RidleyTail_Medium, loaded by initializers C73F/C757 for the middle segments.</summary>
    public const ushort MediumTailInstructionList = 0xca4d;
    /// <summary>$A6:CA53, InstList_RidleyTail_Small, loaded by initializers C76F/C787 for the last two ordinary segments.</summary>
    public const ushort SmallTailInstructionList = 0xca53;
}
