using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>The bank and offset expressions of an address.</summary>
/// <param name="Bank">The bank.</param>
/// <param name="Offset">The offset.</param>
internal readonly record struct BankOffsetParts(ExpressionSyntax Bank, ExpressionSyntax Offset);
