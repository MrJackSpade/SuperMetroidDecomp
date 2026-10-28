using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.EnsureAnalyzer.CartridgeEncoding;

/// <summary>
/// Finds cartridge encoding in repository C# (#641): hexadecimal literals, address-valued pointer dispatch, opaque
/// word arrays, raw storage-width operations, hand-written countdowns, [Flags] primitive casts and narrow storage.
/// The build analyzer reports every occurrence; catalog files and reviewed same-line allowances are classified apart.
/// </summary>
internal static class CartridgeEncodingScanner
{
    /// <summary>
    /// File-name suffixes of named definition catalogs, shared with the production magic-number audit:
    /// raw values there are catalog-owned data, inventoried but not ratcheted as functional literals. A name that
    /// says the file holds addresses, pointers, ROM data or tables does not exempt it (#1411): an address belongs to
    /// the import pipeline, not to a catalog in the simulation.
    /// </summary>
    internal static readonly string[] CatalogSuffixes =
    [
        "Catalog.cs", "Codes.cs", "Constants.cs", "Definition.cs", "Definitions.cs", "Flags.cs", "InstructionLists.cs",
        "Layout.cs", "Masks.cs", "PlmHeaderId.cs", "RoomPlmInstruction.cs", "SamusPoseId.cs", "Values.cs", "Words.cs",
    ];

    /// <summary>
    /// Name fragments that mark a primitive as a countdown for SME6348 when they appear in the name's final
    /// camel-case word, its head noun (<c>FunctionTimer</c>, <c>DelayFrames</c>, but not
    /// <c>TargetCounterClockwiseAngle</c>). A sign test is a timer idiom when its operand is named as one; signed
    /// comparisons of quantities, velocities, angles and packed flags are other rules' work.
    /// </summary>
    private static readonly string[] CountdownNameFragments = ["timer", "countdown", "counter", "delay", "cooldown", "ticks", "frames"];

    /// <summary>A word's sign bit, bit 15.</summary>
    private const long WordSignBit = 0x8000;

    /// <summary>A byte's sign bit, bit 7.</summary>
    private const long ByteSignBit = 0x80;

    /// <summary>Both sign bits, for an operand wide enough to hold either width.</summary>
    private static readonly long[] SignBits = [WordSignBit, ByteSignBit];

    /// <summary>The sign bit of a byte operand.</summary>
    private static readonly long[] ByteSignBits = [ByteSignBit];

    /// <summary>The sign bit of a word operand.</summary>
    private static readonly long[] WordSignBits = [WordSignBit];

    /// <summary>
    /// The attribute (#223) marking a type that owns and documents an encoding: its literals and width
    /// operations are definitions, not functional code.
    /// </summary>
    internal const string EncodingOwnerAttribute = "EncodingOwnerAttribute";

    /// <summary>Shift counts that move a value by a byte, a word or a bank.</summary>
    private static readonly long[] StorageWidthShifts = [8, 16, 24];

    /// <summary>Masks that keep a byte lane, a word, a bank byte or a 24-bit address.</summary>
    private static readonly long[] StorageWidthMasks = [0xff, 0xff00, 0xffff, 0xff0000, 0xffffff, 0xffff0000];

    /// <summary>The diagnostic id that reports <paramref name="rule"/>, or null when the occurrence is classified as allowed.</summary>
    internal static string? RuleId(CartridgeEncodingRule rule) => rule switch
    {
        CartridgeEncodingRule.RawHexLiteral => "SME6340",
        CartridgeEncodingRule.PointerDispatch => "SME6341",
        CartridgeEncodingRule.OpaqueWordArray => "SME6342",
        CartridgeEncodingRule.RawWidthOperation => "SME6345",
        CartridgeEncodingRule.CountdownSignTest => "SME6348",
        CartridgeEncodingRule.CountdownSelfDecrement => "SME6349",
        CartridgeEncodingRule.FlagsPrimitiveCast => "SME6346",
        CartridgeEncodingRule.NarrowStorageMember => "SME6347",
        CartridgeEncodingRule.CatalogHexLiteral or CartridgeEncodingRule.JustifiedHexLiteral or
            CartridgeEncodingRule.OwnedWidthOperation or CartridgeEncodingRule.JustifiedWidthOperation or
            CartridgeEncodingRule.OwnedFlagsPrimitiveCast or CartridgeEncodingRule.OwnedNarrowStorageMember => null,
        _ => throw new InvalidOperationException($"Undefined {nameof(CartridgeEncodingRule)} {rule}."),
    };

    /// <summary>Reviewed same-line exemption prefixes accepted by the encoding inventory.</summary>
    private static readonly string[] AllowanceMarkers = ["cartridge-encoding: allow(", "magic-number-audit: allow("];

    /// <summary>Definition-owner name fragments used to identify address-valued code constants.</summary>
    private static readonly string[] CodeCatalogNameFragments =
        ["Instruction", "Program", "CodePointer", "Pointer", "Address", "Routine", "Code"];

    /// <summary>Source-context cues for a 16-bit literal that may denote a cartridge pointer.</summary>
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
            CartridgeEncodingRule rule = catalog || IsEnumMemberValue(token) || IsInEncodingOwner(token.Parent, model, cancellationToken)
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
                // Sign tests precede pointer dispatch: '(short)timer < 0' is a relational expression too.
                case BinaryExpressionSyntax sign when !catalog && IsCountdownSignTest(sign, model, cancellationToken) &&
                    !IsInEncodingOwner(sign, model, cancellationToken):
                    occurrences.Add(new(CartridgeEncodingRule.CountdownSignTest, CartridgeEncodingClass.ComposableBits,
                        $"'{sign}' tests a countdown's sign bit by hand", sign.OperatorToken.GetLocation()));
                    break;
                case AssignmentExpressionSyntax assignment when !catalog && IsSelfDecrement(assignment, model, cancellationToken) &&
                    !IsInEncodingOwner(assignment, model, cancellationToken):
                    occurrences.Add(new(CartridgeEncodingRule.CountdownSelfDecrement, CartridgeEncodingClass.NumericQuantity,
                        $"'{assignment}' decrements narrow storage with native wraparound", assignment.OperatorToken.GetLocation()));
                    break;
                case PostfixUnaryExpressionSyntax postfix when !catalog && postfix.IsKind(SyntaxKind.PostDecrementExpression) &&
                    IsNarrowStorage(postfix.Operand, model, cancellationToken) && !IsInEncodingOwner(postfix, model, cancellationToken):
                    occurrences.Add(new(CartridgeEncodingRule.CountdownSelfDecrement, CartridgeEncodingClass.NumericQuantity,
                        $"'{postfix}' decrements narrow storage with native wraparound", postfix.OperatorToken.GetLocation()));
                    break;
                case PrefixUnaryExpressionSyntax prefix when !catalog && prefix.IsKind(SyntaxKind.PreDecrementExpression) &&
                    IsNarrowStorage(prefix.Operand, model, cancellationToken) && !IsInEncodingOwner(prefix, model, cancellationToken):
                    occurrences.Add(new(CartridgeEncodingRule.CountdownSelfDecrement, CartridgeEncodingClass.NumericQuantity,
                        $"'{prefix}' decrements narrow storage with native wraparound", prefix.OperatorToken.GetLocation()));
                    break;
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
                case CastExpressionSyntax cast when FlagsEnumCastSource(cast, model, cancellationToken) is { } flags:
                {
                    bool owned = catalog || IsInEncodingOwner(cast, model, cancellationToken) || HasAnyAllowance(text, cast.SpanStart);
                    occurrences.Add(new(owned ? CartridgeEncodingRule.OwnedFlagsPrimitiveCast : CartridgeEncodingRule.FlagsPrimitiveCast,
                        CartridgeEncodingClass.ComposableBits, $"'{cast}' casts [Flags] {flags.Name} to {cast.Type}", cast.OpenParenToken.GetLocation()));
                    break;
                }
                case FieldDeclarationSyntax field when IsInstanceMember(field.Modifiers) && IsNarrowType(field.Declaration.Type):
                    foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                        occurrences.Add(NarrowMember(variable.Identifier, field.Declaration.Type.ToString(), "instance state", field, model, cancellationToken));
                    break;
                case PropertyDeclarationSyntax property when IsInstanceMember(property.Modifiers) && IsNarrowType(property.Type):
                    occurrences.Add(NarrowMember(property.Identifier, property.Type.ToString(), "instance state", property, model, cancellationToken));
                    break;
                case ParameterSyntax { Type: { } parameterType } parameter when IsNarrowType(parameterType):
                    occurrences.Add(NarrowMember(parameter.Identifier, parameterType.ToString(),
                        parameter.Parent?.Parent is RecordDeclarationSyntax ? "instance state" : "as a parameter", parameter, model, cancellationToken));
                    break;
                case VariableDeclaratorSyntax { Parent.Parent: LocalDeclarationStatementSyntax or ForStatementSyntax or UsingStatementSyntax } local
                    when NarrowLocalType(local, model, cancellationToken) is { } localType:
                    occurrences.Add(NarrowMember(local.Identifier, localType, "as a local", local, model, cancellationToken));
                    break;
                // #1452: an enum's explicit narrow underlying type holds its identities at a register width.
                case EnumDeclarationSyntax enumeration when NarrowUnderlyingType(enumeration, model, cancellationToken) is { } underlying:
                    occurrences.Add(NarrowMember(enumeration.Identifier, underlying, "as its underlying type", enumeration, model, cancellationToken));
                    break;
                case ForEachStatementSyntax loop when NarrowLocalType(loop, model, cancellationToken) is { } loopType:
                    occurrences.Add(NarrowMember(loop.Identifier, loopType, "as a local", loop, model, cancellationToken));
                    break;
                case SingleVariableDesignationSyntax designation when NarrowLocalType(designation, model, cancellationToken) is { } designatedType:
                    occurrences.Add(NarrowMember(designation.Identifier, designatedType, "as a local", designation, model, cancellationToken));
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
                default:
                    break;
            }
        }
        return occurrences;
    }

    /// <summary>The <c>[Flags]</c> enum a cast converts to an integral primitive, or null when the cast is anything else.</summary>
    private static INamedTypeSymbol? FlagsEnumCastSource(CastExpressionSyntax cast, SemanticModel model, CancellationToken cancellationToken)
    {
        if (!IsIntegralPrimitive(model.GetTypeInfo(cast.Type, cancellationToken).Type))
            return null;
        return model.GetTypeInfo(cast.Expression, cancellationToken).Type is INamedTypeSymbol { TypeKind: TypeKind.Enum } source &&
            source.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.FlagsAttribute")
            ? source
            : null;
    }

    /// <summary>Whether a member's modifiers make it per-instance storage: neither <c>static</c> nor <c>const</c>.</summary>
    private static bool IsInstanceMember(SyntaxTokenList modifiers) =>
        !modifiers.Any(SyntaxKind.StaticKeyword) && !modifiers.Any(SyntaxKind.ConstKeyword);

    /// <summary>A <c>byte</c>, <c>sbyte</c>, <c>short</c> or <c>ushort</c> type, nullable or not.</summary>
    private static bool IsNarrowType(TypeSyntax type) => (type is NullableTypeSyntax nullable ? nullable.ElementType : type) is
        PredefinedTypeSyntax { Keyword.RawKind: (int)SyntaxKind.UShortKeyword or (int)SyntaxKind.ShortKeyword or
            (int)SyntaxKind.ByteKeyword or (int)SyntaxKind.SByteKeyword };

    /// <summary>Whether a resolved type is <c>byte</c>, <c>sbyte</c>, <c>short</c> or <c>ushort</c>, nullable or not.</summary>
    private static bool IsNarrowType(ITypeSymbol? type) =>
        (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable ? nullable.TypeArguments[0] : type)?.SpecialType is
            SpecialType.System_UInt16 or SpecialType.System_Int16 or SpecialType.System_Byte or SpecialType.System_SByte;

    /// <summary>
    /// The display name of an enum's explicitly written narrow underlying type (<c>: byte</c>, <c>: ushort</c>, ...), or
    /// null when the enum declares none or a wider one.
    /// </summary>
    private static string? NarrowUnderlyingType(EnumDeclarationSyntax enumeration, SemanticModel model, CancellationToken cancellationToken) =>
        enumeration.BaseList is not null &&
        model.GetDeclaredSymbol(enumeration, cancellationToken) is { EnumUnderlyingType: { } underlying } && IsNarrowType(underlying)
            ? underlying.ToDisplayString()
            : null;

    /// <summary>The display name of a local's narrow type, written or inferred, or null when the local is not narrow.</summary>
    private static string? NarrowLocalType(SyntaxNode declaration, SemanticModel model, CancellationToken cancellationToken) =>
        model.GetDeclaredSymbol(declaration, cancellationToken) is ILocalSymbol local && IsNarrowType(local.Type)
            ? local.Type.ToDisplayString()
            : null;

    /// <summary>
    /// The occurrence for one narrow declaration (#1419). Only the wrapping stubs of the Compatibility module and a type
    /// that owns an encoding (<c>[EncodingOwner]</c>) may hold gameplay state at a narrow width; a catalog file or a
    /// same-line comment does not excuse it.
    /// </summary>
    private static CartridgeEncodingOccurrence NarrowMember(SyntaxToken identifier, string type, string role, SyntaxNode declaration,
        SemanticModel model, CancellationToken cancellationToken)
    {
        bool owned = IsInEncodingOwner(declaration, model, cancellationToken) || IsInCompatibilityModule(declaration, model, cancellationToken);
        string what = role == "instance state" ? $"stores {type} instance state" : $"holds {type} {role}";
        return new(owned ? CartridgeEncodingRule.OwnedNarrowStorageMember : CartridgeEncodingRule.NarrowStorageMember,
            CartridgeEncodingClass.NumericQuantity, $"'{identifier.ValueText}' {what}", identifier.GetLocation());
    }

    /// <summary>The namespace of Core's compatibility module, whose wrapping stubs own the widths they reproduce.</summary>
    private const string CompatibilityNamespace = "SuperMetroid.Core.Compatibility";

    /// <summary>Whether a declaration lies in a type of the Compatibility module.</summary>
    private static bool IsInCompatibilityModule(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
    {
        for (SyntaxNode? current = node; current is not null; current = current.Parent)
        {
            if (current is BaseTypeDeclarationSyntax declaration && model.GetDeclaredSymbol(declaration, cancellationToken) is { } type)
            {
                string? space = type.ContainingNamespace?.ToDisplayString();
                return space is not null && (space == CompatibilityNamespace || space.StartsWith(CompatibilityNamespace + ".", StringComparison.Ordinal));
            }
        }
        return false;
    }

    /// <summary>
    /// Whether <paramref name="comment"/> is exactly one reviewed same-line allowance: a <c>//</c> comment that starts
    /// with an allowance marker and carries its category and reason. Its text is what this inventory requires.
    /// </summary>
    internal static bool IsAllowanceComment(string comment) =>
        AllowanceMarkers.Any(marker => comment.StartsWith("// " + marker, StringComparison.Ordinal) && HasAllowance(comment, marker));

    /// <summary>Whether the line holding <paramref name="position"/> carries a reviewed same-line allowance.</summary>
    private static bool HasAnyAllowance(SourceText text, int position)
    {
        string line = text.Lines.GetLineFromPosition(position).ToString();
        return AllowanceMarkers.Any(marker => HasAllowance(line, marker));
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
        // A named constant already states the width's meaning; only literal widths are raw.
        ExpressionSyntax? width = IsRawWidth(binary.Right, model, cancellationToken) ? binary.Right
            : mask && IsRawWidth(binary.Left, model, cancellationToken) ? binary.Left : null;
        if (width is null)
            return false;
        object? value = model.GetConstantValue(width, cancellationToken).Value;
        if (value is null or bool or string or char)
            return false;
        long constant = Convert.ToInt64(value, CultureInfo.InvariantCulture);
        return mask ? StorageWidthMasks.Contains(constant) : StorageWidthShifts.Contains(constant);
    }

    /// <summary>
    /// A hand-written sign-bit test whose operand is a single countdown-named member or local (#1324):
    /// <c>t &amp; 0x8000</c>, <c>t &amp; 0x80</c>, <c>t &gt;= 0x8000</c>, <c>t &lt; 0x80</c>, <c>(short)t &lt; 0</c>,
    /// <c>(sbyte)t &gt;= 0</c> and their mirrored forms. A difference such as <c>(short)(a - b) &lt; 0</c> is a
    /// signed comparison of quantities, not a timer idiom.
    /// </summary>
    internal static bool IsCountdownSignTest(BinaryExpressionSyntax binary, SemanticModel model, CancellationToken cancellationToken)
    {
        if (model.GetConstantValue(binary, cancellationToken).HasValue)
            return false;
        if (binary.IsKind(SyntaxKind.BitwiseAndExpression))
            return IsSignBitConstant(binary.Right, binary.Left, relational: false, model, cancellationToken) && IsCountdownOperand(binary.Left, model, cancellationToken) ||
                IsSignBitConstant(binary.Left, binary.Right, relational: false, model, cancellationToken) && IsCountdownOperand(binary.Right, model, cancellationToken);
        if (binary.IsKind(SyntaxKind.LessThanExpression) || binary.IsKind(SyntaxKind.GreaterThanOrEqualExpression))
            return IsSignBitConstant(binary.Right, binary.Left, relational: true, model, cancellationToken) && IsCountdownOperand(binary.Left, model, cancellationToken) ||
                IsZero(binary.Right, model, cancellationToken) && IsSignedReinterpretation(binary.Left, model, cancellationToken);
        if (binary.IsKind(SyntaxKind.GreaterThanExpression) || binary.IsKind(SyntaxKind.LessThanOrEqualExpression))
            return IsSignBitConstant(binary.Left, binary.Right, relational: true, model, cancellationToken) && IsCountdownOperand(binary.Right, model, cancellationToken) ||
                IsZero(binary.Left, model, cancellationToken) && IsSignedReinterpretation(binary.Right, model, cancellationToken);
        return false;
    }

    /// <summary>
    /// <c>x = unchecked((ushort)(x - 1))</c> or its <c>byte</c> form, or <c>x -= 1</c> on narrow storage: the
    /// 65C816 <c>DEC</c> written by hand.
    /// </summary>
    internal static bool IsSelfDecrement(AssignmentExpressionSyntax assignment, SemanticModel model, CancellationToken cancellationToken)
    {
        if (assignment.IsKind(SyntaxKind.SubtractAssignmentExpression))
            return IsOne(assignment.Right, model, cancellationToken) && IsNarrowStorage(assignment.Left, model, cancellationToken);
        if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return false;
        if (StripWrappers(assignment.Right, stripCasts: false) is not CastExpressionSyntax
            {
                Type: PredefinedTypeSyntax { Keyword.RawKind: (int)SyntaxKind.UShortKeyword or (int)SyntaxKind.ByteKeyword },
            } cast)
        {
            return false;
        }
        return StripWrappers(cast.Expression, stripCasts: false) is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.SubtractExpression } difference &&
            IsOne(difference.Right, model, cancellationToken) &&
            SyntaxFactory.AreEquivalent(StripWrappers(difference.Left, stripCasts: false), StripWrappers(assignment.Left, stripCasts: false));
    }

    /// <summary>A ushort or byte field, property or element: storage the decrement persists into, not a local or parameter.</summary>
    private static bool IsNarrowStorage(ExpressionSyntax operand, SemanticModel model, CancellationToken cancellationToken)
    {
        if (model.GetTypeInfo(operand, cancellationToken).Type?.SpecialType is not (SpecialType.System_UInt16 or SpecialType.System_Byte))
            return false;
        return operand is ElementAccessExpressionSyntax ||
            model.GetSymbolInfo(operand, cancellationToken).Symbol is IFieldSymbol or IPropertySymbol;
    }

    /// <summary><c>(short)t</c> or <c>(sbyte)t</c> over a single countdown-named operand, compared with zero.</summary>
    private static bool IsSignedReinterpretation(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken) =>
        StripWrappers(expression, stripCasts: false) is CastExpressionSyntax
        {
            Type: PredefinedTypeSyntax { Keyword.RawKind: (int)SyntaxKind.ShortKeyword or (int)SyntaxKind.SByteKeyword },
        } cast && IsCountdownOperand(cast.Expression, model, cancellationToken);

    /// <summary>A single non-enum integral member, element or local whose name marks it as a countdown.</summary>
    private static bool IsCountdownOperand(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken)
    {
        ExpressionSyntax operand = StripWrappers(expression, stripCasts: true);
        if (operand is not (IdentifierNameSyntax or MemberAccessExpressionSyntax or ElementAccessExpressionSyntax))
            return false;
        ITypeSymbol? type = model.GetTypeInfo(operand, cancellationToken).Type;
        if (type is null || type.TypeKind == TypeKind.Enum || !IsIntegralPrimitive(type))
            return false;
        string headNoun = HeadNoun(operand);
        return CountdownNameFragments.Any(fragment => headNoun.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>The final camel-case word of the operand's own name (an element access names its array).</summary>
    private static string HeadNoun(ExpressionSyntax operand)
    {
        ExpressionSyntax named = operand is ElementAccessExpressionSyntax element ? StripWrappers(element.Expression, stripCasts: true) : operand;
        string name = named switch
        {
            MemberAccessExpressionSyntax access => access.Name.Identifier.ValueText,
            SimpleNameSyntax simple => simple.Identifier.ValueText,
            _ => named.ToString(),
        };
        int start = 0;
        for (int index = 1; index < name.Length; index++)
        {
            if (char.IsUpper(name[index]) && !char.IsUpper(name[index - 1]))
                start = index;
        }
        return name.Substring(start).TrimStart('_');
    }

    /// <summary>
    /// A constant equal to the sign bit of <paramref name="operand"/>'s width: bit 7 of a byte, bit 15 of a word.
    /// A wider operand may hold either width, so a mask may name either bit; compared by magnitude, a wider
    /// value is a word's sign only at bit 15, since a byte held in it is already a byte-width comparison.
    /// </summary>
    private static bool IsSignBitConstant(ExpressionSyntax expression, ExpressionSyntax operand, bool relational, SemanticModel model, CancellationToken cancellationToken)
    {
        if (IntegralConstant(expression, model, cancellationToken) is not { } value)
            return false;
        long[] operandSignBits = model.GetTypeInfo(StripWrappers(operand, stripCasts: false), cancellationToken).Type?.SpecialType switch
        {
            SpecialType.System_Byte or SpecialType.System_SByte => ByteSignBits,
            SpecialType.System_UInt16 or SpecialType.System_Int16 => WordSignBits,
            SpecialType.None or
                SpecialType.System_Object or
                SpecialType.System_Enum or
                SpecialType.System_MulticastDelegate or
                SpecialType.System_Delegate or
                SpecialType.System_ValueType or
                SpecialType.System_Void or
                SpecialType.System_Boolean or
                SpecialType.System_Char or
                SpecialType.System_Int32 or
                SpecialType.System_UInt32 or
                SpecialType.System_Int64 or
                SpecialType.System_UInt64 or
                SpecialType.System_Decimal or
                SpecialType.System_Single or
                SpecialType.System_Double or
                SpecialType.System_String or
                SpecialType.System_IntPtr or
                SpecialType.System_UIntPtr or
                SpecialType.System_Array or
                SpecialType.System_Collections_IEnumerable or
                SpecialType.System_Collections_Generic_IEnumerable_T or
                SpecialType.System_Collections_Generic_IList_T or
                SpecialType.System_Collections_Generic_ICollection_T or
                SpecialType.System_Collections_IEnumerator or
                SpecialType.System_Collections_Generic_IEnumerator_T or
                SpecialType.System_Collections_Generic_IReadOnlyList_T or
                SpecialType.System_Collections_Generic_IReadOnlyCollection_T or
                SpecialType.System_Nullable_T or
                SpecialType.System_DateTime or
                SpecialType.System_Runtime_CompilerServices_IsVolatile or
                SpecialType.System_IDisposable or
                SpecialType.System_TypedReference or
                SpecialType.System_ArgIterator or
                SpecialType.System_RuntimeArgumentHandle or
                SpecialType.System_RuntimeFieldHandle or
                SpecialType.System_RuntimeMethodHandle or
                SpecialType.System_RuntimeTypeHandle or
                SpecialType.System_IAsyncResult or
                SpecialType.System_AsyncCallback or
                SpecialType.System_Runtime_CompilerServices_RuntimeFeature or
                SpecialType.System_Runtime_CompilerServices_PreserveBaseOverridesAttribute or
                SpecialType.System_Runtime_CompilerServices_InlineArrayAttribute => relational ? WordSignBits : SignBits,
            _ => relational ? WordSignBits : SignBits,
        };
        return operandSignBits.Contains(value);
    }

    /// <summary>A constant equal to zero.</summary>
    private static bool IsZero(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken) =>
        IntegralConstant(expression, model, cancellationToken) == 0;

    /// <summary>A constant equal to one.</summary>
    private static bool IsOne(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken) =>
        IntegralConstant(expression, model, cancellationToken) == 1;

    /// <summary>The expression's integral constant value, or null when it is not an integral constant.</summary>
    private static long? IntegralConstant(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken)
    {
        object? value = model.GetConstantValue(expression, cancellationToken).Value;
        return value is null or bool or string or char or float or double or decimal or ulong and > long.MaxValue
            ? null
            : Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    /// <summary>Removes parentheses, <c>checked</c>/<c>unchecked</c> and, when <paramref name="stripCasts"/> is set, casts.</summary>
    private static ExpressionSyntax StripWrappers(ExpressionSyntax expression, bool stripCasts)
    {
        while (true)
        {
            switch (expression)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    expression = parenthesized.Expression;
                    break;
                case CheckedExpressionSyntax wrapped:
                    expression = wrapped.Expression;
                    break;
                case CastExpressionSyntax cast when stripCasts:
                    expression = cast.Expression;
                    break;
                default:
                    return expression;
            }
        }
    }

    /// <summary>Recognizes a numeric literal or primitive MaxValue used as a raw storage-width operand.</summary>
    private static bool IsRawWidth(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken)
    {
        while (expression is ParenthesizedExpressionSyntax or CastExpressionSyntax or CheckedExpressionSyntax)
        {
            expression = expression switch
            {
                ParenthesizedExpressionSyntax parenthesized => parenthesized.Expression,
                CastExpressionSyntax cast => cast.Expression,
                CheckedExpressionSyntax wrapped => wrapped.Expression,
                _ => expression,
            };
        }
        return expression switch
        {
            LiteralExpressionSyntax literal => literal.IsKind(SyntaxKind.NumericLiteralExpression),
            MemberAccessExpressionSyntax { Name.Identifier.ValueText: "MaxValue" } access =>
                model.GetTypeInfo(access.Expression, cancellationToken).Type?.SpecialType is SpecialType.System_Byte or
                    SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32,
            _ => false,
        };
    }

    /// <summary>Counts a nested shift-and-mask chain once, at its outermost width operation.</summary>
    private static bool IsInsideWidthOperation(BinaryExpressionSyntax binary, SemanticModel model, CancellationToken cancellationToken)
    {
        SyntaxNode? parent = binary.Parent;
        while (parent is ParenthesizedExpressionSyntax or CastExpressionSyntax or CheckedExpressionSyntax)
            parent = parent.Parent;
        return parent is BinaryExpressionSyntax outer && IsWidthOperation(outer, model, cancellationToken);
    }

    /// <summary>Explains which byte, word, or bank lane a raw mask or shift manipulates.</summary>
    private static string WidthBasis(BinaryExpressionSyntax binary) =>
        binary.IsKind(SyntaxKind.BitwiseAndExpression) ? $"'{binary}' keeps a byte, word or bank lane"
        : binary.IsKind(SyntaxKind.LeftShiftExpression) ? $"'{binary}' moves a value up by a byte, word or bank width"
        : $"'{binary}' moves a value down by a byte, word or bank width";

    /// <summary>
    /// Whether the literal is an enum member's value: an enum is a named definition set wherever its file lives, so
    /// the value it assigns is a catalog definition rather than an encoding in functional code.
    /// </summary>
    private static bool IsEnumMemberValue(SyntaxToken token) =>
        token.Parent?.FirstAncestorOrSelf<EqualsValueClauseSyntax>() is { Parent: EnumMemberDeclarationSyntax };

    /// <summary>Finds a containing type explicitly marked as the owner of its SNES encoding.</summary>
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

    /// <summary>Distinguishes hexadecimal numeric tokens from other C# numeric literals.</summary>
    private static bool IsHex(string text) =>
        text.Length > 2 && text[0] == '0' && (text[1] == 'x' || text[1] == 'X');

    /// <summary>Requires a nonempty category and a reason after the same-line allowance marker.</summary>
    private static bool HasAllowance(string line, string marker)
    {
        int start = line.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            return false;
        int close = line.IndexOf(')', start + marker.Length);
        return close > start + marker.Length && line.IndexOf(" - ", close, StringComparison.Ordinal) == close + 1;
    }

    /// <summary>Parses a C# hexadecimal token after removing digit separators and numeric suffixes.</summary>
    private static ulong HexValue(string text)
    {
        string digits = text.Substring(2).TrimEnd('u', 'U', 'l', 'L').Replace("_", string.Empty);
        return ulong.Parse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
    }

    /// <summary>Proposes a domain class for a hex token from its numeric range and syntax context.</summary>
    private static HexClassification ClassifyHex(SyntaxToken token)
    {
        ulong value = HexValue(token.Text);
        SyntaxNode context = Climb(token.Parent ??
            throw new InvalidOperationException("A hex literal token always belongs to an expression."));
        SyntaxNode? parent = context.Parent;

        if (parent is InitializerExpressionSyntax { RawKind: (int)SyntaxKind.ArrayInitializerExpression } or
            ExpressionElementSyntax)
        {
            return new HexClassification(CartridgeEncodingClass.StructuredInstruction, "array element");
        }
        if (value is >= 0x808000 and <= 0xffffff)
            return new HexClassification(CartridgeEncodingClass.CartridgeAddress, "long cartridge address range");
        if (parent is EqualsValueClauseSyntax { Parent: EnumMemberDeclarationSyntax member })
        {
            bool flags = member.Parent is EnumDeclarationSyntax enumDeclaration &&
                enumDeclaration.AttributeLists.SelectMany(list => list.Attributes)
                    .Any(attribute => attribute.Name.ToString() is "Flags" or "FlagsAttribute" or "System.Flags");
            if (flags)
                return new HexClassification(CartridgeEncodingClass.ComposableBits, "[Flags] enum member");
            return value is >= 0x8000 and <= 0xffff
                ? new HexClassification(CartridgeEncodingClass.CartridgeAddress, "address-valued enum member")
                : new HexClassification(CartridgeEncodingClass.DomainIdentity, "enum member");
        }
        if (parent is CaseSwitchLabelSyntax or ConstantPatternSyntax or SwitchExpressionArmSyntax)
            return new HexClassification(CartridgeEncodingClass.DomainIdentity, "switch arm or pattern constant");
        if (IsBitwise(parent))
            return new HexClassification(CartridgeEncodingClass.ComposableBits, "bitwise operand");
        if (value is >= 0x8000 and <= 0xffff && NamesPointer(context))
            return new HexClassification(CartridgeEncodingClass.CartridgeAddress, "pointer-named context");
        if (parent is BinaryExpressionSyntax binary &&
            binary.Kind() is SyntaxKind.EqualsExpression or SyntaxKind.NotEqualsExpression)
        {
            return new HexClassification(CartridgeEncodingClass.DomainIdentity, "equality constant");
        }
        if (parent is BinaryExpressionSyntax or AssignmentExpressionSyntax or ArgumentSyntax or EqualsValueClauseSyntax or
            ReturnStatementSyntax or ArrowExpressionClauseSyntax)
        {
            return new HexClassification(CartridgeEncodingClass.NumericQuantity, "arithmetic, comparison or assigned value");
        }
        return new HexClassification(CartridgeEncodingClass.Unknown, "no classifying context");
    }

    /// <summary>Moves past wrappers and unary signs to the expression whose parent determines a literal's use.</summary>
    private static SyntaxNode Climb(SyntaxNode node)
    {
        while (node.Parent is ParenthesizedExpressionSyntax or CastExpressionSyntax or CheckedExpressionSyntax or
               PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.UnaryMinusExpression or (int)SyntaxKind.UnaryPlusExpression })
        {
            node = node.Parent;
        }
        return node;
    }

    /// <summary>Detects masks, shifts, and bitwise updates around a hexadecimal operand.</summary>
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

    /// <summary>Looks for pointer-related names in the nearest containing statement or member.</summary>
    private static bool NamesPointer(SyntaxNode context)
    {
        SyntaxNode scope = context.Ancestors().FirstOrDefault(ancestor =>
            ancestor is StatementSyntax or MemberDeclarationSyntax or SwitchExpressionArmSyntax) ?? context;
        string lower = scope.ToString().ToLowerInvariant();
        return PointerContextFragments.Any(fragment => lower.Contains(fragment));
    }

    /// <summary>Limits pointer-dispatch checks to comparisons and address offsets.</summary>
    private static bool IsDispatchOperator(SyntaxKind kind) => kind is SyntaxKind.EqualsExpression or
        SyntaxKind.NotEqualsExpression or SyntaxKind.AddExpression or SyntaxKind.SubtractExpression or
        SyntaxKind.LessThanExpression or SyntaxKind.LessThanOrEqualExpression or SyntaxKind.GreaterThanExpression or
        SyntaxKind.GreaterThanOrEqualExpression;

    /// <summary>Finds a primitive value compared with or offset from a named address-valued code constant.</summary>
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

    /// <summary>Checks whether a case label dispatches on a primitive integral switch subject.</summary>
    private static bool IsPrimitiveSwitchArm(CaseSwitchLabelSyntax label, SemanticModel model, CancellationToken cancellationToken) =>
        label.Parent?.Parent is SwitchStatementSyntax switchStatement &&
        IsIntegralPrimitive(model.GetTypeInfo(switchStatement.Expression, cancellationToken).Type);

    /// <summary>Finds the enclosing is-pattern or switch subject and checks its primitive integral type.</summary>
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

    /// <summary>Checks whether an expression names a constant owned by a code or address catalog.</summary>
    private static bool ReferencesCodeConstant(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken) =>
        expression.DescendantNodesAndSelf().OfType<SimpleNameSyntax>().Any(identifier =>
            model.GetSymbolInfo(identifier, cancellationToken).Symbol is IFieldSymbol { HasConstantValue: true } field &&
            IsCodeConstant(field));

    /// <summary>Recognizes bank-local or long cartridge address constants in code-named owner types.</summary>
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

    /// <summary>Restricts raw dispatch and opaque-array findings to built-in integral storage types.</summary>
    private static bool IsIntegralPrimitive(ITypeSymbol? type) => type?.SpecialType is SpecialType.System_Byte or
        SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or
        SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64;

    /// <summary>Resolves an array initializer's element type from its creation or declared destination.</summary>
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

    /// <summary>Extracts an element type from an array, pointer, or single-argument generic collection.</summary>
    private static ITypeSymbol? CollectionElementType(ITypeSymbol? collection) => collection switch
    {
        IArrayTypeSymbol array => array.ElementType,
        IPointerTypeSymbol pointer => pointer.PointedAtType,
        INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } named => named.TypeArguments[0],
        _ => null,
    };

    /// <summary>Flags an integral collection with at least four entries and a hexadecimal word among them.</summary>
    private static bool IsOpaqueWordArray(IReadOnlyList<ExpressionSyntax> elements, ITypeSymbol? elementType) =>
        elements.Count >= 4 && IsIntegralPrimitive(elementType) &&
        elements.Any(element => element.DescendantTokens().Any(token =>
            token.IsKind(SyntaxKind.NumericLiteralToken) && IsHex(token.Text)));
}
