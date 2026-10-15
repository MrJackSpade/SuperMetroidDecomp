using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>Recognizes adjacent byte reads combined into a little-endian word (#223).</summary>
public static class WordReadMatcher
{
    /// <summary>Byte operations eligible for replacement by an owned word read.</summary>
    private static readonly string[] ByteMethods = ["ReadCartridgeByte", "ReadWorkRamByte", "ReadSaveRamByte"];
    /// <summary>Interfaces that identify a read as coming from supported memory rather than an unrelated helper.</summary>
    private static readonly string[] Sources =
        ["SuperMetroid.AssetExtraction.Cartridge.IImportCartridgeSource", "SuperMetroid.Core.Hardware.ISnesMutableMemory"];

    /// <summary>The canonical implementations, which compose the bytes themselves.</summary>
    public static readonly string[] CanonicalTypes =
        ["SuperMetroid.Core.Hardware.SnesWordReads", "SuperMetroid.AssetExtraction.Cartridge.CartridgeWordReads", "SuperMetroid.TestSupport.VerificationImporters.CartridgeLongReads"];

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

    /// <summary>Recognizes an already canonical cartridge word as the low part of a 24-bit read.</summary>
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

    /// <summary>Accepts OR and addition, the two supported ways to combine shifted bytes.</summary>
    private static bool IsCombine(BinaryExpressionSyntax expression) =>
        expression.IsKind(SyntaxKind.BitwiseOrExpression) || expression.IsKind(SyntaxKind.AddExpression);

    /// <summary>Collects all operands of one same-kind combination without crossing other operators.</summary>
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

    /// <summary>Exposes the byte read beneath parentheses, casts, and checked expressions.</summary>
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

    /// <summary>Includes a covering ushort cast or checked expression in the suggested replacement span.</summary>
    private static ExpressionSyntax Covering(ExpressionSyntax expression)
    {
        while (expression.Parent is ParenthesizedExpressionSyntax or CheckedExpressionSyntax or
               CastExpressionSyntax { Type: PredefinedTypeSyntax { Keyword.ValueText: "ushort" } })
        {
            expression = (ExpressionSyntax)expression.Parent;
        }
        return expression;
    }

    /// <summary>Captures a byte operation and whether it came through the cartridge import capability.</summary>
    private sealed record ByteReadCall(ExpressionSyntax Receiver, string Method, ExpressionSyntax Address, bool ViaAddressSpace,
        WordReadProgression? WordProgression = null);

    /// <summary>Accepts only a supported byte method owned by a recognized memory source.</summary>
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

    /// <summary>The import extension that provides cartridge reads through an address space.</summary>
    private const string AddressSpaceImportExtensions = "SuperMetroid.AssetExtraction.Cartridge.SnesCartridgeImportExtensions";

    /// <summary>Checks whether the byte method implements a recognized source interface member.</summary>
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

    /// <summary>Classifies the high byte's address as linear, bank-wrapped, ambiguous, or unrelated.</summary>
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

    /// <summary>Separates a bank expression from its offset and records explicit 16-bit wrapping.</summary>
    private sealed record BankOffset(ExpressionSyntax Bank, ExpressionSyntax Offset, bool Wrapped);

    /// <summary>Recognizes bank OR offset and cartridge-window addresses, including wrapped offsets.</summary>
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

    /// <summary>Finds the constant step between addresses that share all nonconstant terms.</summary>
    private static long? Distance(ExpressionSyntax from, ExpressionSyntax to, SemanticModel model) =>
        Normalize(from, model) is { } a && Normalize(to, model) is { } b && a.Terms == b.Terms ? b.Constant - a.Constant : null;

    /// <summary>Sorts symbolic terms and folds integral constants so equivalent address sums compare equal.</summary>
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

    /// <summary>Limits constant folding to integer address components.</summary>
    private static bool IsIntegral(object value) => value is byte or sbyte or short or ushort or int or uint or long;

    /// <summary>Compares address syntax while ignoring formatting trivia.</summary>
    private static bool Same(SyntaxNode left, SyntaxNode right) => SyntaxFactory.AreEquivalent(left, right, topLevel: false);

    /// <summary>Removes only parentheses when an address pattern needs to retain cast and wrapping semantics.</summary>
    private static ExpressionSyntax StripParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;
        return expression;
    }

    /// <summary>Recognizes a 16-bit wrapped offset through a ushort cast or an FFFF mask.</summary>
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
