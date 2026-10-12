using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>How the high byte's address follows the low byte's in a reconstructed word.</summary>
public enum WordReadProgression
{
    /// <summary>The next 24-bit bus address: <c>address + 1</c>.</summary>
    Linear,
    /// <summary>The next address within the bank: <c>AddWithinBank(address, 1)</c> or a 16-bit wrapped offset.</summary>
    FixedBank,
    /// <summary>A bank OR'd with an unwrapped <c>offset + 1</c>: the boundary behaviour cannot be inferred.</summary>
    Ambiguous,
}

/// <summary>One little-endian word rebuilt from two adjacent byte reads of a canonical memory source.</summary>
/// <param name="Expression">The whole expression to replace, including a covering cast or <c>unchecked</c>.</param>
/// <param name="Receiver">The byte source, as written.</param>
/// <param name="ByteMethod">The byte read: ReadCartridgeByte, ReadWorkRamByte or ReadSaveRamByte.</param>
/// <param name="Address">The low byte's address.</param>
/// <param name="Progression">How the high byte's address follows.</param>
/// <param name="OtherOperands">Further operands OR'd into the same expression, such as a bank.</param>
/// <param name="ViaAddressSpace">True when the bytes came through the address space's import capability, which the
/// canonical read names explicitly with <c>CartridgeImportSource.Require</c>.</param>
/// <param name="Long">True for a 24-bit value: a canonical word and the byte after it.</param>
public sealed record WordReadMatch(
    ExpressionSyntax Expression,
    ExpressionSyntax Receiver,
    string ByteMethod,
    ExpressionSyntax Address,
    WordReadProgression Progression,
    ImmutableArray<ExpressionSyntax> OtherOperands,
    bool ViaAddressSpace,
    bool Long = false)
{
    /// <summary>The receiver of the canonical read as source text.</summary>
    public string CanonicalReceiver => ViaAddressSpace ? $"CartridgeImportSource.Require({Receiver})" : Receiver.ToString();

    /// <summary>
    /// The canonical word operation for this byte source and progression, or null when the progression is
    /// ambiguous or no caller has needed that operation yet (the SRAM word). The linear cartridge long lives in
    /// test support, since only verification oracles read longs on the linear bus.
    /// </summary>
    public string? CanonicalMethod => (ByteMethod, Progression, Long) switch
    {
        ("ReadCartridgeByte", WordReadProgression.Linear, false) => "ReadCartridgeWord",
        ("ReadCartridgeByte", WordReadProgression.FixedBank, false) => "ReadCartridgeWordFixedBank",
        ("ReadCartridgeByte", WordReadProgression.Linear, true) => "ReadCartridgeLong",
        ("ReadCartridgeByte", WordReadProgression.FixedBank, true) => "ReadCartridgeLongFixedBank",
        ("ReadWorkRamByte", WordReadProgression.Linear, false) => "ReadWorkRamWord",
        ("ReadWorkRamByte", WordReadProgression.FixedBank, false) => "ReadWorkRamWordFixedBank",
        _ => null,
    };
}

/// <summary>Recognizes adjacent byte reads combined into a little-endian word (#223).</summary>
public static class WordReadMatcher
{
    private static readonly string[] ByteMethods = ["ReadCartridgeByte", "ReadWorkRamByte", "ReadSaveRamByte"];
    private static readonly string[] Sources =
        ["SuperMetroid.Core.Hardware.IImportCartridgeSource", "SuperMetroid.Core.Hardware.ISnesMutableMemory"];

    /// <summary>The canonical implementations, which compose the bytes themselves.</summary>
    public static readonly string[] CanonicalTypes =
        ["SuperMetroid.Core.Hardware.SnesWordReads", "SuperMetroid.Core.Hardware.CartridgeWordReads", "SuperMetroid.Core.Hardware.CartridgeLongReads"];

    /// <summary>Matches the top of an OR (or plus) chain that rebuilds a word.</summary>
    public static WordReadMatch? Match(BinaryExpressionSyntax root, SemanticModel model)
    {
        if (!IsCombine(root) || root.Parent is BinaryExpressionSyntax parent && IsCombine(parent) && parent.Kind() == root.Kind())
            return null;
        var operands = new List<ExpressionSyntax>();
        Flatten(root, root.Kind(), operands);
        for (int highIndex = 0; highIndex < operands.Count; highIndex++)
        {
            if (Unwrap(operands[highIndex]) is not BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LeftShiftExpression } shift ||
                shift.Right is not LiteralExpressionSyntax { Token.ValueText: "8" or "16" } shiftCount ||
                ByteRead(Unwrap(shift.Left), model) is not { } high)
            {
                continue;
            }
            bool isLong = shiftCount.Token.ValueText == "16";
            for (int lowIndex = 0; lowIndex < operands.Count; lowIndex++)
            {
                if (lowIndex == highIndex)
                    continue;
                ByteReadCall? low = isLong ? CanonicalWordRead(Unwrap(operands[lowIndex]), model) : ByteRead(Unwrap(operands[lowIndex]), model);
                if (low is null || low.Method != high.Method || !Same(low.Receiver, high.Receiver) ||
                    low.ViaAddressSpace != high.ViaAddressSpace)
                {
                    continue;
                }
                if (Progression(low.Address, high.Address, isLong ? 2 : 1, model) is not { } progression ||
                    isLong && progression != low.WordProgression)
                {
                    continue;
                }
                ImmutableArray<ExpressionSyntax> others = [.. operands.Where((_, index) => index != lowIndex && index != highIndex)];
                // A covering cast belongs to the replacement only when the value is the whole expression.
                return new(others.IsEmpty && !isLong ? Covering(root) : root, low.Receiver, low.Method, low.Address, progression,
                    others, low.ViaAddressSpace, isLong);
            }
        }
        return null;
    }

    // The low word of a 24-bit value: an already canonical cartridge word read.
    private static ByteReadCall? CanonicalWordRead(ExpressionSyntax expression, SemanticModel model)
    {
        if (expression is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "ReadCartridgeWord" or "ReadCartridgeWordFixedBank" } member,
                ArgumentList.Arguments.Count: 1,
            } invocation ||
            model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol)
        {
            return null;
        }
        ExpressionSyntax receiver = member.Expression;
        bool viaAddressSpace = false;
        if (receiver is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Require" },
                ArgumentList.Arguments.Count: 1,
            } require)
        {
            receiver = require.ArgumentList.Arguments[0].Expression;
            viaAddressSpace = true;
        }
        WordReadProgression progression = member.Name.Identifier.ValueText == "ReadCartridgeWord"
            ? WordReadProgression.Linear : WordReadProgression.FixedBank;
        return new(receiver, "ReadCartridgeByte", invocation.ArgumentList.Arguments[0].Expression, viaAddressSpace, progression);
    }

    private static bool IsCombine(BinaryExpressionSyntax expression) =>
        expression.IsKind(SyntaxKind.BitwiseOrExpression) || expression.IsKind(SyntaxKind.AddExpression);

    private static void Flatten(ExpressionSyntax expression, SyntaxKind kind, List<ExpressionSyntax> operands)
    {
        if (expression is BinaryExpressionSyntax binary && binary.IsKind(kind))
        {
            Flatten(binary.Left, kind, operands);
            Flatten(binary.Right, kind, operands);
        }
        else
        {
            operands.Add(expression);
        }
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (true)
        {
            switch (expression)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    expression = parenthesized.Expression;
                    break;
                case CastExpressionSyntax cast:
                    expression = cast.Expression;
                    break;
                case CheckedExpressionSyntax checkedExpression:
                    expression = checkedExpression.Expression;
                    break;
                default:
                    return expression;
            }
        }
    }

    // A covering (ushort) cast or unchecked(...) around the whole combination belongs to the replacement.
    private static ExpressionSyntax Covering(ExpressionSyntax expression)
    {
        while (expression.Parent is ParenthesizedExpressionSyntax or CheckedExpressionSyntax or
               CastExpressionSyntax { Type: PredefinedTypeSyntax { Keyword.ValueText: "ushort" } })
        {
            expression = (ExpressionSyntax)expression.Parent;
        }
        return expression;
    }

    private sealed record ByteReadCall(ExpressionSyntax Receiver, string Method, ExpressionSyntax Address, bool ViaAddressSpace,
        WordReadProgression? WordProgression = null);

    private static ByteReadCall? ByteRead(ExpressionSyntax expression, SemanticModel model)
    {
        if (expression is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax member,
                ArgumentList.Arguments.Count: 1,
            } invocation ||
            !ByteMethods.Contains(member.Name.Identifier.ValueText) ||
            model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method ||
            method.ReturnType.SpecialType != SpecialType.System_Byte)
        {
            return null;
        }
        string name = member.Name.Identifier.ValueText;
        ExpressionSyntax address = invocation.ArgumentList.Arguments[0].Expression;
        if (method.ReducedFrom is { ContainingType: { } extensionOwner } &&
            extensionOwner.ToDisplayString() == AddressSpaceImportExtensions && name == "ReadCartridgeByte")
        {
            return new(member.Expression, name, address, ViaAddressSpace: true);
        }
        return OwnsByteRead(method) ? new(member.Expression, name, address, ViaAddressSpace: false) : null;
    }

    private const string AddressSpaceImportExtensions = "SuperMetroid.Core.Hardware.SnesCartridgeImportExtensions";

    // The byte read must be the source interface's own member or an implementation of it.
    private static bool OwnsByteRead(IMethodSymbol method)
    {
        INamedTypeSymbol owner = method.ContainingType;
        foreach (INamedTypeSymbol source in owner.AllInterfaces.Prepend(owner).Where(type => Sources.Contains(type.ToDisplayString())))
        {
            foreach (IMethodSymbol member in source.GetMembers(method.Name).OfType<IMethodSymbol>())
            {
                if (SymbolEqualityComparer.Default.Equals(member, method) ||
                    SymbolEqualityComparer.Default.Equals(owner.FindImplementationForInterfaceMember(member), method))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static WordReadProgression? Progression(ExpressionSyntax low, ExpressionSyntax high, int distance, SemanticModel model)
    {
        low = StripParentheses(low);
        high = StripParentheses(high);
        if (Distance(low, high, model) == distance)
            return WordReadProgression.Linear;
        if (high is InvocationExpressionSyntax { ArgumentList.Arguments.Count: 2 } within &&
            model.GetSymbolInfo(within).Symbol is IMethodSymbol { Name: "AddWithinBank" } &&
            Same(StripParentheses(within.ArgumentList.Arguments[0].Expression), low) &&
            model.GetConstantValue(within.ArgumentList.Arguments[1].Expression) is { HasValue: true, Value: int step } && step == distance)
        {
            return WordReadProgression.FixedBank;
        }
        // (a & bank mask) | ((a + d) & offset mask): the same bank, a wrapped offset.
        if (high is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.BitwiseOrExpression } split &&
            StripParentheses(split.Left) is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.BitwiseAndExpression } bankPart &&
            Same(StripParentheses(bankPart.Left), low) && Wrapped16(split.Right, model) is { } wrappedOffset &&
            Distance(low, wrappedOffset, model) == distance)
        {
            return WordReadProgression.FixedBank;
        }
        if (BankRelative(low, model) is { } lowBank && BankRelative(high, model) is { } highBank && Same(lowBank.Bank, highBank.Bank) &&
            Distance(lowBank.Offset, highBank.Offset, model) == distance)
        {
            return highBank.Wrapped ? WordReadProgression.FixedBank : WordReadProgression.Ambiguous;
        }
        return null;
    }

    private sealed record BankOffset(ExpressionSyntax Bank, ExpressionSyntax Offset, bool Wrapped);

    // bank | offset, or CartridgeBankWindow.At(window, offset); the offset may be wrapped to 16 bits.
    private static BankOffset? BankRelative(ExpressionSyntax address, SemanticModel model)
    {
        address = StripParentheses(address);
        (ExpressionSyntax Bank, ExpressionSyntax Offset)? parts = address switch
        {
            BinaryExpressionSyntax { RawKind: (int)SyntaxKind.BitwiseOrExpression } or => (StripParentheses(or.Left), StripParentheses(or.Right)),
            InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "At" }, ArgumentList.Arguments.Count: 2 } at =>
                (at.ArgumentList.Arguments[0].Expression, StripParentheses(at.ArgumentList.Arguments[1].Expression)),
            _ => null,
        };
        if (parts is not { } found)
            return null;
        return Wrapped16(found.Offset, model) is { } inner ? new(found.Bank, inner, true) : new(found.Bank, found.Offset, false);
    }

    // The constant distance from one address to another when both share their non-constant terms.
    private static long? Distance(ExpressionSyntax from, ExpressionSyntax to, SemanticModel model) =>
        Normalize(from, model) is { } a && Normalize(to, model) is { } b && a.Terms == b.Terms ? b.Constant - a.Constant : null;

    private static (string Terms, long Constant)? Normalize(ExpressionSyntax expression, SemanticModel model)
    {
        var terms = new List<string>();
        long constant = 0;
        void Add(ExpressionSyntax node, int sign)
        {
            node = StripParentheses(node);
            if (model.GetConstantValue(node) is { HasValue: true, Value: { } value } && IsIntegral(value))
            {
                constant += sign * Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
                return;
            }
            if (node is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } add)
            {
                Add(add.Left, sign);
                Add(add.Right, sign);
                return;
            }
            if (node is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.SubtractExpression } subtract)
            {
                Add(subtract.Left, sign);
                Add(subtract.Right, -sign);
                return;
            }
            terms.Add((sign < 0 ? "-" : "+") + node.WithoutTrivia().NormalizeWhitespace().ToFullString());
        }
        Add(expression, 1);
        terms.Sort(StringComparer.Ordinal);
        return (string.Join(" ", terms), constant);
    }

    private static bool IsIntegral(object value) => value is byte or sbyte or short or ushort or int or uint or long;

    // Structural equality that ignores trivia: `a` and the `a ` of `a + 1` are the same address.
    private static bool Same(SyntaxNode left, SyntaxNode right) => SyntaxFactory.AreEquivalent(left, right, topLevel: false);

    private static ExpressionSyntax StripParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;
        return expression;
    }

    // unchecked((ushort)(x)), (ushort)(x), or x & 0xffff: a 16-bit wrapped offset.
    private static ExpressionSyntax? Wrapped16(ExpressionSyntax expression, SemanticModel model)
    {
        expression = StripParentheses(expression);
        if (expression is CheckedExpressionSyntax { RawKind: (int)SyntaxKind.UncheckedExpression } wrap)
            expression = StripParentheses(wrap.Expression);
        if (expression is CastExpressionSyntax { Type: PredefinedTypeSyntax { Keyword.ValueText: "ushort" } } cast)
            return StripParentheses(cast.Expression);
        if (expression is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.BitwiseAndExpression } mask &&
            model.GetConstantValue(mask.Right) is { HasValue: true, Value: int bits } && bits == 0xffff)
        {
            return StripParentheses(mask.Left);
        }
        return null;
    }
}

/// <summary>
/// Reports little-endian words rebuilt from adjacent byte reads of WRAM, SRAM or the cartridge (#223):
/// the canonical <c>Read…Word</c>/<c>Read…WordFixedBank</c> operations own the byte order and the bank
/// progression. Their implementations are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WordReadAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A word rebuilt from two byte reads.</summary>
    public const string ReconstructedWordId = "SME6350";

    private static readonly DiagnosticDescriptor ReconstructedWordRule = new(
        ReconstructedWordId,
        "Read the word through its canonical operation",
        "{0}",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Little-endian words are read with SnesWordReads/CartridgeWordReads, which own the byte order and " +
            "state whether the high byte's address wraps within the bank.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ReconstructedWordRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.BitwiseOrExpression, SyntaxKind.AddExpression);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        for (INamedTypeSymbol? owner = context.ContainingSymbol?.ContainingType; owner is not null; owner = owner.ContainingType)
        {
            if (WordReadMatcher.CanonicalTypes.Contains(owner.ToDisplayString()))
                return;
        }
        if (WordReadMatcher.Match((BinaryExpressionSyntax)context.Node, context.SemanticModel) is not { } match)
            return;
        string message = match.CanonicalMethod is { } method
            ? $"Use {match.CanonicalReceiver}.{method}({match.Address}) instead of composing two {match.ByteMethod} calls"
            : match.Progression != WordReadProgression.Ambiguous
            ? $"Add the {match.Progression} {(match.Long ? "long" : "word")} operation for {match.ByteMethod} to the canonical " +
              "word reads and use it instead of composing bytes"
            : $"Use a canonical {match.ByteMethod.Replace("Byte", "Word")} operation; '{match.Address}' OR'd with an unwrapped " +
              "offset + 1 does not say whether the high byte stays in the bank, so choose Linear or FixedBank deliberately";
        context.ReportDiagnostic(Diagnostic.Create(ReconstructedWordRule, match.Expression.GetLocation(), message));
    }
}
