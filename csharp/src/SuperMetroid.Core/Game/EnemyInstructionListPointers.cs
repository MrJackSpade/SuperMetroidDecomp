namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A3 animation-list pointers used by small translated enemy families.</summary>
internal static class OrdinaryEnemyInstructionLists
{
    /// <summary><c>kWaver_Ilist_86A7</c> at $A3:86A7.</summary>
    public const ushort WaverInitial = WaverInstructionProgramDefinitions.SteadyFacingLeft;
    /// <summary><c>kMetalee_Ilist_8910</c> at $A3:8910.</summary>
    public const ushort MetareeInitial = 0x8910;
    /// <summary><c>kFireflea_Ilist_8C2F</c> at $A3:8C2F.</summary>
    public const ushort FirefleaInitial = FirefleaInstructionProgramDefinitions.Loop;
    /// <summary><c>kSkree_Ilist_C65E</c> at $A3:C65E.</summary>
    public const ushort SkreeInitial = 0xc65e;
}

/// <summary>Named bank-$A2 instruction opcodes used by Rio.</summary>
internal static class RioInstructionCodes
{
    /// <summary>Marks Rio's current animation complete at $A2:BBC3.</summary>
    public const ushort SetAnimationFinished = 0xbbc3;
}
