namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Initial main-function and animation selection for one Ceres-door actor.</summary>
internal readonly record struct CeresDoorInitializationDefinition(
    CeresDoorFunction MainFunction,
    CeresDoorSegment Animation);
