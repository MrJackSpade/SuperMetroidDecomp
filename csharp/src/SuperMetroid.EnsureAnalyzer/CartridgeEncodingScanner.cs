using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>What a cartridge-encoding occurrence is, for the ratchet and the #641 inventory.</summary>
internal enum CartridgeEncodingRule
{
    /// <summary>A hexadecimal literal in functional (non-catalog) code: ratcheted by SME6340.</summary>
    RawHexLiteral,
    /// <summary>
    /// A primitive compared with, offset from or switched over an address-valued code or instruction
    /// constant: ratcheted by SME6341.
    /// </summary>
    PointerDispatch,
    /// <summary>An integral array or collection literal holding hexadecimal words: ratcheted by SME6342.</summary>
    OpaqueWordArray,
    /// <summary>A hexadecimal literal owned by a named definition catalog. Inventoried, not ratcheted.</summary>
    CatalogHexLiteral,
    /// <summary>A functional hexadecimal literal carrying a reviewed same-line allowance. Inventoried, not ratcheted.</summary>
    JustifiedHexLiteral,
    /// <summary>
    /// A raw storage-width or layout operation in functional code (#223): a shift by a byte, word or bank width,
    /// or a byte/word/bank mask. Ratcheted by SME6345.
    /// </summary>
    RawWidthOperation,
    /// <summary>A width operation inside a catalog or a <c>[SnesEncodingOwner]</c> type that documents it. Inventoried.</summary>
    OwnedWidthOperation,
    /// <summary>A functional width operation carrying a reviewed same-line allowance. Inventoried, not ratcheted.</summary>
    JustifiedWidthOperation,
}

/// <summary>The six #641 audit classes. The scanner proposes one per occurrence and names its basis.</summary>
internal enum CartridgeEncodingClass
{
    /// <summary>A count, distance, duration, velocity or other ordinary quantity written in hexadecimal.</summary>
    NumericQuantity,
    /// <summary>A member of a mutually exclusive domain: a case label, comparison constant or plain enum member.</summary>
    DomainIdentity,
    /// <summary>A bit, mask or shift: composable bits when the cartridge treats them so.</summary>
    ComposableBits,
    /// <summary>Encoded instruction or definition words held in an opaque integral array.</summary>
    StructuredInstruction,
    /// <summary>A cartridge address, code pointer or address-based dispatch.</summary>
    CartridgeAddress,
    /// <summary>No rule establishes the meaning; it stays at the boundary until one does.</summary>
    Unknown,
}

/// <summary>One occurrence: its rule, proposed class, the basis for that class, and where it is.</summary>
internal readonly record struct CartridgeEncodingOccurrence(
    CartridgeEncodingRule Rule,
    CartridgeEncodingClass Class,
    string Basis,
    Location Location);

/// <summary>
/// Finds cartridge encoding in production C# (#641): hexadecimal literals, address-valued pointer
/// dispatch and opaque word arrays. The build analyzer ratchets the counts per file; the
/// cartridge-encoding audit tool writes the baseline and the inventory from the same scan.
/// </summary>
internal static class CartridgeEncodingScanner
{
    /// <summary>The ratchet baseline's file name, passed to audited projects as an additional file.</summary>
    internal const string BaselineFileName = "cartridge-encoding-baseline.txt";

    /// <summary>
    /// File-name suffixes of named definition catalogs, shared with the production magic-number audit:
    /// raw values there are catalog-owned data, inventoried but not ratcheted as functional literals.
    /// </summary>
    internal static readonly string[] CatalogSuffixes =
    [
        "Addresses.cs", "Catalog.cs", "CodePointers.cs", "Codes.cs", "Constants.cs", "Definitions.cs", "Flags.cs",
        "InstructionLists.cs", "Layout.cs", "Masks.cs", "Pointers.cs", "RomData.cs", "PlmHeaderId.cs",
        "RoomPlmInstruction.cs", "SamusPoseId.cs", "Tables.cs", "Values.cs", "Words.cs",
    ];

    /// <summary>The ratcheted diagnostic ids, in rule order.</summary>
    internal static readonly string[] RatchetedRuleIds = ["SME6340", "SME6341", "SME6342", "SME6345"];

    /// <summary>
    /// The attribute (#223) marking a type that owns and documents an encoding: its literals and width
    /// operations are definitions, not functional code.
    /// </summary>
    internal const string EncodingOwnerAttribute = "SnesEncodingOwnerAttribute";

    /// <summary>Shift counts that move a value by a byte, a word or a bank.</summary>
    private static readonly long[] StorageWidthShifts = [8, 16, 24];

    /// <summary>Masks that keep a byte lane, a word, a bank byte or a 24-bit address.</summary>
    private static readonly long[] StorageWidthMasks = [0xff, 0xff00, 0xffff, 0xff0000, 0xffffff, 0xffff0000];

    /// <summary>The diagnostic id that ratchets <paramref name="rule"/>, or null when the rule is inventory-only.</summary>
    internal static string? RuleId(CartridgeEncodingRule rule) => rule switch
    {
        CartridgeEncodingRule.RawHexLiteral => "SME6340",
        CartridgeEncodingRule.PointerDispatch => "SME6341",
        CartridgeEncodingRule.OpaqueWordArray => "SME6342",
        CartridgeEncodingRule.RawWidthOperation => "SME6345",
        CartridgeEncodingRule.CatalogHexLiteral or CartridgeEncodingRule.JustifiedHexLiteral or
            CartridgeEncodingRule.OwnedWidthOperation or CartridgeEncodingRule.JustifiedWidthOperation => null,
        _ => throw new InvalidOperationException($"Undefined {nameof(CartridgeEncodingRule)} {rule}."),
    };

    private static readonly string[] AllowanceMarkers = ["cartridge-encoding: allow(", "magic-number-audit: allow("];

    private static readonly string[] CodeCatalogNameFragments =
        ["Instruction", "Program", "CodePointer", "Pointer", "Address", "Routine", "Code"];

    private static readonly string[] PointerContextFragments =
        ["pointer", "instruction", "address", "routine", "function", "code", "bank", "instlist", "list"];

    /// <summary>True when <paramref name="path"/> names a definition catalog by its file-name suffix.</summary>
    internal static bool IsCatalogFile(string path)
    {
        string name = Path.GetFileName(path);
        if (name.EndsWith(".Generated.cs", StringComparison.OrdinalIgnoreCase))
            name = name.Substring(0, name.Length - ".Generated.cs".Length) + ".cs";
        return CatalogSuffixes.Any(suffix => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>True for compiler- or build-generated sources, which are not authored code.</summary>
    internal static bool IsBuildGenerated(string path)
    {
        string normalized = path.Replace('\\', '/');
        return normalized.Contains("/obj/") || normalized.Contains("/bin/") ||
            normalized.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Returns <paramref name="path"/> relative to <paramref name="repositoryRoot"/> with '/' separators, or null outside it.</summary>
    internal static string? RepositoryRelative(string repositoryRoot, string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;
        string root = Path.GetFullPath(repositoryRoot).TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
        string full = Path.GetFullPath(path);
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            return null;
        return full.Substring(root.Length).Replace('\\', '/');
    }

    /// <summary>Scans one authored tree.</summary>
    internal static List<CartridgeEncodingOccurrence> Scan(SemanticModel model, CancellationToken cancellationToken)
    {
        SyntaxTree tree = model.SyntaxTree;
        SyntaxNode root = tree.GetRoot(cancellationToken);
        bool catalog = IsCatalogFile(tree.FilePath);
        var occurrences = new List<CartridgeEncodingOccurrence>();
        var text = tree.GetText(cancellationToken);

        foreach (SyntaxToken token in root.DescendantTokens())
        {
            if (!token.IsKind(SyntaxKind.NumericLiteralToken) || !IsHex(token.Text))
                continue;
            string line = text.Lines.GetLineFromPosition(token.SpanStart).ToString();
            CartridgeEncodingRule rule = catalog || IsInEncodingOwner(token.Parent, model, cancellationToken)
                ? CartridgeEncodingRule.CatalogHexLiteral
                : AllowanceMarkers.Any(marker => HasAllowance(line, marker)) ? CartridgeEncodingRule.JustifiedHexLiteral
                : CartridgeEncodingRule.RawHexLiteral;
            (CartridgeEncodingClass cls, string basis) = ClassifyHex(token);
            occurrences.Add(new(rule, cls, basis, token.GetLocation()));
        }

        foreach (SyntaxNode node in root.DescendantNodes())
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (node)
            {
                case BinaryExpressionSyntax width when IsWidthOperation(width, model, cancellationToken) &&
                    !IsInsideWidthOperation(width, model, cancellationToken):
                {
                    string line = text.Lines.GetLineFromPosition(width.SpanStart).ToString();
                    CartridgeEncodingRule rule = catalog || IsInEncodingOwner(width, model, cancellationToken)
                        ? CartridgeEncodingRule.OwnedWidthOperation
                        : AllowanceMarkers.Any(marker => HasAllowance(line, marker)) ? CartridgeEncodingRule.JustifiedWidthOperation
                        : CartridgeEncodingRule.RawWidthOperation;
                    occurrences.Add(new(rule, CartridgeEncodingClass.ComposableBits, WidthBasis(width), width.OperatorToken.GetLocation()));
                    break;
                }
                case BinaryExpressionSyntax binary when IsDispatchOperator(binary.Kind()):
                    if (IsPointerDispatch(binary.Left, binary.Right, model, cancellationToken) ||
                        IsPointerDispatch(binary.Right, binary.Left, model, cancellationToken))
                    {
                        occurrences.Add(new(CartridgeEncodingRule.PointerDispatch, CartridgeEncodingClass.CartridgeAddress,
                            "compared with or offset from an address-valued code constant", binary.OperatorToken.GetLocation()));
                    }
                    break;
                case BinaryExpressionSyntax isExpression when isExpression.IsKind(SyntaxKind.IsExpression) &&
                    IsIntegralPrimitive(model.GetTypeInfo(isExpression.Left, cancellationToken).Type) &&
                    ReferencesCodeConstant(isExpression.Right, model, cancellationToken):
                    // 'p is Catalog.Constant' parses as a type test but binds as a constant pattern.
                    occurrences.Add(new(CartridgeEncodingRule.PointerDispatch, CartridgeEncodingClass.CartridgeAddress,
                        "pattern over an address-valued code constant", isExpression.Right.GetLocation()));
                    break;
                case CaseSwitchLabelSyntax label when IsPrimitiveSwitchArm(label, model, cancellationToken) &&
                    ReferencesCodeConstant(label.Value, model, cancellationToken):
                    occurrences.Add(new(CartridgeEncodingRule.PointerDispatch, CartridgeEncodingClass.CartridgeAddress,
                        "switch arm over an address-valued code constant", label.Value.GetLocation()));
                    break;
                case ConstantPatternSyntax pattern when IsPrimitivePatternSubject(pattern, model, cancellationToken) &&
                    ReferencesCodeConstant(pattern.Expression, model, cancellationToken):
                    occurrences.Add(new(CartridgeEncodingRule.PointerDispatch, CartridgeEncodingClass.CartridgeAddress,
                        "pattern over an address-valued code constant", pattern.GetLocation()));
                    break;
                case InitializerExpressionSyntax initializer when initializer.IsKind(SyntaxKind.ArrayInitializerExpression):
                    if (IsOpaqueWordArray(initializer.Expressions, InitializerElementType(initializer, model, cancellationToken)))
                    {
                        occurrences.Add(new(CartridgeEncodingRule.OpaqueWordArray, CartridgeEncodingClass.StructuredInstruction,
                            "integral array literal holding hexadecimal words", initializer.OpenBraceToken.GetLocation()));
                    }
                    break;
                case CollectionExpressionSyntax collection:
                    if (IsOpaqueWordArray(
                            collection.Elements.OfType<ExpressionElementSyntax>().Select(element => element.Expression).ToList(),
                            CollectionElementType(model.GetTypeInfo(collection, cancellationToken).ConvertedType)))
                    {
                        occurrences.Add(new(CartridgeEncodingRule.OpaqueWordArray, CartridgeEncodingClass.StructuredInstruction,
                            "integral collection literal holding hexadecimal words", collection.OpenBracketToken.GetLocation()));
                    }
                    break;
            }
        }
        return occurrences;
    }

    /// <summary>
    /// A shift by a byte, word or bank width, or a byte/word/bank mask, that the compiler does not fold to a
    /// constant: the operation encodes a storage width or a layout whose meaning the call site must name.
    /// </summary>
    internal static bool IsWidthOperation(BinaryExpressionSyntax binary, SemanticModel model, CancellationToken cancellationToken)
    {
        bool mask = binary.IsKind(SyntaxKind.BitwiseAndExpression);
        if (!mask && !binary.IsKind(SyntaxKind.LeftShiftExpression) && !binary.IsKind(SyntaxKind.RightShiftExpression) &&
            !binary.IsKind(SyntaxKind.UnsignedRightShiftExpression))
            return false;
        if (model.GetConstantValue(binary, cancellationToken).HasValue)
            return false;
        object? value = model.GetConstantValue(binary.Right, cancellationToken).Value ??
            (mask ? model.GetConstantValue(binary.Left, cancellationToken).Value : null);
        if (value is null or bool or string or char)
            return false;
        long constant = Convert.ToInt64(value, CultureInfo.InvariantCulture);
        return mask ? StorageWidthMasks.Contains(constant) : StorageWidthShifts.Contains(constant);
    }

    // (x >> 8) & 0xff is one operation: only the outermost of a nested width chain is counted.
    private static bool IsInsideWidthOperation(BinaryExpressionSyntax binary, SemanticModel model, CancellationToken cancellationToken)
    {
        SyntaxNode? parent = binary.Parent;
        while (parent is ParenthesizedExpressionSyntax or CastExpressionSyntax or CheckedExpressionSyntax)
            parent = parent.Parent;
        return parent is BinaryExpressionSyntax outer && IsWidthOperation(outer, model, cancellationToken);
    }

    private static string WidthBasis(BinaryExpressionSyntax binary) => binary.Kind() switch
    {
        SyntaxKind.BitwiseAndExpression => $"'{binary}' keeps a byte, word or bank lane",
        SyntaxKind.LeftShiftExpression => $"'{binary}' moves a value up by a byte, word or bank width",
        _ => $"'{binary}' moves a value down by a byte, word or bank width",
    };

    private static bool IsInEncodingOwner(SyntaxNode? node, SemanticModel model, CancellationToken cancellationToken)
    {
        for (SyntaxNode? current = node; current is not null; current = current.Parent)
        {
            if (current is BaseTypeDeclarationSyntax declaration &&
                model.GetDeclaredSymbol(declaration, cancellationToken) is { } type &&
                type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == EncodingOwnerAttribute))
                return true;
        }
        return false;
    }

    private static bool IsHex(string text) =>
        text.Length > 2 && text[0] == '0' && (text[1] == 'x' || text[1] == 'X');

    private static bool HasAllowance(string line, string marker)
    {
        int start = line.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            return false;
        int close = line.IndexOf(')', start + marker.Length);
        return close > start + marker.Length && line.IndexOf(" - ", close, StringComparison.Ordinal) == close + 1;
    }

    private static ulong HexValue(string text)
    {
        string digits = text.Substring(2).TrimEnd('u', 'U', 'l', 'L').Replace("_", string.Empty);
        return ulong.Parse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
    }

    private static (CartridgeEncodingClass Class, string Basis) ClassifyHex(SyntaxToken token)
    {
        ulong value = HexValue(token.Text);
        SyntaxNode context = Climb(token.Parent!);
        SyntaxNode? parent = context.Parent;

        if (parent is InitializerExpressionSyntax { RawKind: (int)SyntaxKind.ArrayInitializerExpression } or
            ExpressionElementSyntax)
        {
            return (CartridgeEncodingClass.StructuredInstruction, "array element");
        }
        if (value is >= 0x808000 and <= 0xffffff)
            return (CartridgeEncodingClass.CartridgeAddress, "long cartridge address range");
        if (parent is EqualsValueClauseSyntax { Parent: EnumMemberDeclarationSyntax member })
        {
            bool flags = member.Parent is EnumDeclarationSyntax enumDeclaration &&
                enumDeclaration.AttributeLists.SelectMany(list => list.Attributes)
                    .Any(attribute => attribute.Name.ToString() is "Flags" or "FlagsAttribute" or "System.Flags");
            if (flags)
                return (CartridgeEncodingClass.ComposableBits, "[Flags] enum member");
            return value is >= 0x8000 and <= 0xffff
                ? (CartridgeEncodingClass.CartridgeAddress, "address-valued enum member")
                : (CartridgeEncodingClass.DomainIdentity, "enum member");
        }
        if (parent is CaseSwitchLabelSyntax or ConstantPatternSyntax or SwitchExpressionArmSyntax)
            return (CartridgeEncodingClass.DomainIdentity, "switch arm or pattern constant");
        if (IsBitwise(parent))
            return (CartridgeEncodingClass.ComposableBits, "bitwise operand");
        if (value is >= 0x8000 and <= 0xffff && NamesPointer(context))
            return (CartridgeEncodingClass.CartridgeAddress, "pointer-named context");
        if (parent is BinaryExpressionSyntax binary &&
            binary.Kind() is SyntaxKind.EqualsExpression or SyntaxKind.NotEqualsExpression)
        {
            return (CartridgeEncodingClass.DomainIdentity, "equality constant");
        }
        if (parent is BinaryExpressionSyntax or AssignmentExpressionSyntax or ArgumentSyntax or EqualsValueClauseSyntax or
            ReturnStatementSyntax or ArrowExpressionClauseSyntax)
        {
            return (CartridgeEncodingClass.NumericQuantity, "arithmetic, comparison or assigned value");
        }
        return (CartridgeEncodingClass.Unknown, "no classifying context");
    }

    private static SyntaxNode Climb(SyntaxNode node)
    {
        while (node.Parent is ParenthesizedExpressionSyntax or CastExpressionSyntax or CheckedExpressionSyntax or
               PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.UnaryMinusExpression or (int)SyntaxKind.UnaryPlusExpression })
        {
            node = node.Parent;
        }
        return node;
    }

    private static bool IsBitwise(SyntaxNode? parent) => parent switch
    {
        BinaryExpressionSyntax binary => binary.Kind() is SyntaxKind.BitwiseAndExpression or SyntaxKind.BitwiseOrExpression or
            SyntaxKind.ExclusiveOrExpression or SyntaxKind.LeftShiftExpression or SyntaxKind.RightShiftExpression or
            SyntaxKind.UnsignedRightShiftExpression,
        AssignmentExpressionSyntax assignment => assignment.Kind() is SyntaxKind.AndAssignmentExpression or
            SyntaxKind.OrAssignmentExpression or SyntaxKind.ExclusiveOrAssignmentExpression or
            SyntaxKind.LeftShiftAssignmentExpression or SyntaxKind.RightShiftAssignmentExpression,
        PrefixUnaryExpressionSyntax unary => unary.IsKind(SyntaxKind.BitwiseNotExpression),
        _ => false,
    };

    private static bool NamesPointer(SyntaxNode context)
    {
        SyntaxNode scope = context.Ancestors().FirstOrDefault(ancestor =>
            ancestor is StatementSyntax or MemberDeclarationSyntax or SwitchExpressionArmSyntax) ?? context;
        string lower = scope.ToString().ToLowerInvariant();
        return PointerContextFragments.Any(fragment => lower.Contains(fragment));
    }

    private static bool IsDispatchOperator(SyntaxKind kind) => kind is SyntaxKind.EqualsExpression or
        SyntaxKind.NotEqualsExpression or SyntaxKind.AddExpression or SyntaxKind.SubtractExpression or
        SyntaxKind.LessThanExpression or SyntaxKind.LessThanOrEqualExpression or SyntaxKind.GreaterThanExpression or
        SyntaxKind.GreaterThanOrEqualExpression;

    private static bool IsPointerDispatch(
        ExpressionSyntax constantSide,
        ExpressionSyntax variableSide,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        if (!constantSide.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any())
            return false;
        if (!model.GetConstantValue(constantSide, cancellationToken).HasValue ||
            model.GetConstantValue(variableSide, cancellationToken).HasValue)
        {
            return false;
        }
        return IsIntegralPrimitive(model.GetTypeInfo(variableSide, cancellationToken).Type) &&
            ReferencesCodeConstant(constantSide, model, cancellationToken);
    }

    private static bool IsPrimitiveSwitchArm(CaseSwitchLabelSyntax label, SemanticModel model, CancellationToken cancellationToken) =>
        label.Parent?.Parent is SwitchStatementSyntax switchStatement &&
        IsIntegralPrimitive(model.GetTypeInfo(switchStatement.Expression, cancellationToken).Type);

    private static bool IsPrimitivePatternSubject(ConstantPatternSyntax pattern, SemanticModel model, CancellationToken cancellationToken)
    {
        ExpressionSyntax? subject = pattern.Ancestors().Select(ancestor => ancestor switch
        {
            IsPatternExpressionSyntax isPattern => isPattern.Expression,
            SwitchExpressionSyntax switchExpression => switchExpression.GoverningExpression,
            SwitchStatementSyntax switchStatement => switchStatement.Expression,
            _ => null,
        }).FirstOrDefault(expression => expression is not null);
        return subject is not null && IsIntegralPrimitive(model.GetTypeInfo(subject, cancellationToken).Type);
    }

    private static bool ReferencesCodeConstant(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken) =>
        expression.DescendantNodesAndSelf().OfType<SimpleNameSyntax>().Any(identifier =>
            model.GetSymbolInfo(identifier, cancellationToken).Symbol is IFieldSymbol { HasConstantValue: true } field &&
            IsCodeConstant(field));

    private static bool IsCodeConstant(IFieldSymbol field)
    {
        ulong value;
        try
        {
            value = Convert.ToUInt64(field.ConstantValue, CultureInfo.InvariantCulture);
        }
        catch (OverflowException)
        {
            return false;
        }
        catch (InvalidCastException)
        {
            return false;
        }
        if (value is not (>= 0x8000 and <= 0xffff or >= 0x808000 and <= 0xffffff))
            return false;
        string owner = field.ContainingType.Name;
        return CodeCatalogNameFragments.Any(fragment => owner.Contains(fragment));
    }

    private static bool IsIntegralPrimitive(ITypeSymbol? type) => type?.SpecialType is SpecialType.System_Byte or
        SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or
        SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64;

    private static ITypeSymbol? InitializerElementType(
        InitializerExpressionSyntax initializer,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        ITypeSymbol? arrayType = initializer.Parent switch
        {
            ArrayCreationExpressionSyntax creation => model.GetTypeInfo(creation, cancellationToken).Type,
            ImplicitArrayCreationExpressionSyntax creation => model.GetTypeInfo(creation, cancellationToken).Type,
            StackAllocArrayCreationExpressionSyntax creation => model.GetTypeInfo(creation, cancellationToken).ConvertedType,
            EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator } =>
                model.GetDeclaredSymbol(declarator, cancellationToken) switch
                {
                    IFieldSymbol field => field.Type,
                    ILocalSymbol local => local.Type,
                    _ => null,
                },
            _ => null,
        };
        return CollectionElementType(arrayType);
    }

    private static ITypeSymbol? CollectionElementType(ITypeSymbol? collection) => collection switch
    {
        IArrayTypeSymbol array => array.ElementType,
        IPointerTypeSymbol pointer => pointer.PointedAtType,
        INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } named => named.TypeArguments[0],
        _ => null,
    };

    private static bool IsOpaqueWordArray(IReadOnlyList<ExpressionSyntax> elements, ITypeSymbol? elementType) =>
        elements.Count >= 4 && IsIntegralPrimitive(elementType) &&
        elements.Any(element => element.DescendantTokens().Any(token =>
            token.IsKind(SyntaxKind.NumericLiteralToken) && IsHex(token.Text)));
}
