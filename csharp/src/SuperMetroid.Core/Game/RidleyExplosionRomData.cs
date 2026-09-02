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

    /// <summary>First tail-fragment instruction list at $A6:C70F.</summary>
    public const ushort Tail0InstructionList = 0xc70f;
    /// <summary>Second tail-fragment instruction list at $A6:C727.</summary>
    public const ushort Tail1InstructionList = 0xc727;
    /// <summary>Third tail-fragment instruction list at $A6:C73F.</summary>
    public const ushort Tail2InstructionList = 0xc73f;
    /// <summary>Fourth tail-fragment instruction list at $A6:C757.</summary>
    public const ushort Tail3InstructionList = 0xc757;
    /// <summary>Fifth tail-fragment instruction list at $A6:C76F.</summary>
    public const ushort Tail4InstructionList = 0xc76f;
    /// <summary>Sixth tail-fragment instruction list at $A6:C787.</summary>
    public const ushort Tail5InstructionList = 0xc787;
}
