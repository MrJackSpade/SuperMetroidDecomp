using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// A place where source writes a source name into text (SME6460, #2548): an enum member's, or, through <c>nameof</c>, any
/// member's or type's.
/// </summary>
/// <param name="Text">The operation whose value carries the name: the hole, the call, the concatenation or the <c>nameof</c>.</param>
/// <param name="Enum">The enum whose member's name it writes, the type whose member's name it writes, or the type it names.</param>
/// <param name="NamesType">Whether the name is the type's own rather than one of its members'.</param>
internal readonly record struct EnumNameWrite(IOperation Text, ITypeSymbol Enum, bool NamesType = false)
{
    /// <summary>What the name is, as a message says it: <c>a 'Glow' member</c> or <c>the type 'Glow'</c>.</summary>
    internal string Named => NamesType ? "the type '" + Enum.Name + "'" : "a '" + Enum.Name + "' member";
}
