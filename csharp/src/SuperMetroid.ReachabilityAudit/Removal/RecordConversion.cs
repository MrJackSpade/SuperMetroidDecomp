namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// A positional record whose every generated property is dead while some of its parameters are still read by its base
/// call or its members' initializers: it becomes a record without a parameter list whose constructor takes the parameters
/// still read, passes them to its base and gives its members their initial values (#2599, #2611).
/// </summary>
/// <param name="Record">The record's name.</param>
/// <param name="Parameters">The names of all its positional parameters, in order, which find it in its file.</param>
/// <param name="Kept">The names of the parameters its base call or initializers read, which the constructor keeps.</param>
/// <param name="MovesInitializers">Whether an initializer reads a kept parameter, so every instance initializer of the
/// record moves into the constructor's body, in order.</param>
internal sealed record RecordConversion(string Record, IReadOnlyList<string> Parameters, IReadOnlySet<string> Kept, bool MovesInitializers);
