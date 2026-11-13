namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The main action and animation one Ceres-door actor starts with.</summary>
internal readonly record struct CeresDoorInitializationDefinition(
    CeresDoorAction MainAction,
    CeresDoorSegment Animation);
