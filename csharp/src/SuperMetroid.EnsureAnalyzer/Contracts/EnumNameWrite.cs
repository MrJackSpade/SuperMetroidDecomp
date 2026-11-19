using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A place where source writes an enum member's name into text (SME6460).</summary>
/// <param name="Text">The operation whose value carries the name: the hole, the call or the concatenation.</param>
/// <param name="Enum">The enum whose member's name it writes.</param>
internal readonly record struct EnumNameWrite(IOperation Text, ITypeSymbol Enum);
