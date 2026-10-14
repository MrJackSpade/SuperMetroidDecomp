using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.ResourceAudit.EnemyVisualPrograms;

/// <summary>Finite adapters for reviewed custom layouts; no gameplay interpreter is executed.</summary>
internal static class EnemyVisualProgramSpecializations
{
    /// <summary>Program owners containing only control words, with no visual selectors to resolve.</summary>
    private static readonly HashSet<string> ControlOnly =
    [
        nameof(CommonEnemyProjectileInstructionProgramDefinitions),
        nameof(TourianEntranceStatueInstructionProgramDefinitions),
    ];

    /// <summary>Identifies reviewed instruction owners with no presentation operands.</summary>
    internal static bool IsControlOnly(Type type) => ControlOnly.Contains(type.Name);
}
