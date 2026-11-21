namespace SuperMetroid.Core;

/// <summary>
/// Selects the generated constructor that restores a type from a game snapshot (#2602): it reads every field the type
/// holds from the snapshot instead of building it from its arguments.
/// </summary>
public readonly partial struct StateGraphRestore;
