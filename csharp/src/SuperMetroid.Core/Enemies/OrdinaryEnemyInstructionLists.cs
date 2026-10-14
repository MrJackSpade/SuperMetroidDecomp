using SuperMetroid.Core.Enemies.Waver;

namespace SuperMetroid.Core.Enemies;

/// <summary>Named bank-$A3 animation-list pointers used by small translated enemy families.</summary>
internal static class OrdinaryEnemyInstructionLists
{
    /// <summary><c>kWaver_Ilist_86A7</c> at $A3:86A7.</summary>
    public const ushort WaverInitial = WaverInstructionProgramDefinitions.SteadyFacingLeft;
    /// <summary><c>kMetalee_Ilist_8910</c> at $A3:8910.</summary>
    public const ushort MetareeInitial = 0x8910;
    /// <summary><c>kSkree_Ilist_C65E</c> at $A3:C65E.</summary>
    public const ushort SkreeInitial = 0xc65e;
}
