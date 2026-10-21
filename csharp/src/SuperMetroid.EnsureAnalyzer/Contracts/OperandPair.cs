using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>An operand of a comparison and the other operand.</summary>
/// <param name="Subject">The subject.</param>
/// <param name="Other">The other.</param>
internal readonly record struct OperandPair(IOperation Subject, IOperation Other);
