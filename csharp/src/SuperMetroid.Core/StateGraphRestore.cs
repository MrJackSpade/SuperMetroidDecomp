namespace SuperMetroid.Core;

/// <summary>
/// Selects the constructor the compiler writes for restoring a type from a debugger state (#2602): it reads every field
/// the type holds from the state instead of building it from its arguments.
/// </summary>
public readonly partial struct StateGraphRestore;
