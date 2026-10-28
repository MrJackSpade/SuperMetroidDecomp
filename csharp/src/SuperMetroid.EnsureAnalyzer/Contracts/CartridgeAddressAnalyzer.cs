using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// The ROM is known only to the import pipeline (SME6447, #1411): in <c>SuperMetroid.Core</c>, no integral constant,
/// field or property initializer, enum member value, attribute argument or switch label holds a plausible cartridge or
/// work-RAM address. A value is plausible when it is a 24-bit address in a mapped bank (<see cref="MappedBank"/>), or a
/// 16-bit value at or above <c>$8000</c> declared in a type whose name or namespace names a cartridge structure
/// (<see cref="StructureWords"/>), in a file the encoding scanner names as a catalog
/// (<see cref="CartridgeEncodingScanner.IsCatalogFile"/>), or in an address-valued enum (<see cref="IsAddressValuedEnum"/>,
/// #1452) wherever it is declared. The address moves to the AssetExtraction codec that reads it
/// once, which produces the installed definition or the semantic identity. See <c>docs/cartridge-addresses.md</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CartridgeAddressAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core value is a plausible cartridge or work-RAM address.</summary>
    public const string CartridgeAddressId = "SME6447";

    /// <summary>
    /// Camel-case words naming a cartridge structure in a type or namespace name, matched whole, without regard to case
    /// and with a plural ending: <c>SamusXrayRomData</c>, <c>SamusProjectileProgramAddresses</c>, <c>Hardware.Snes</c>.
    /// </summary>
    internal static readonly ImmutableArray<string> StructureWords =
        ["Rom", "Cartridge", "Bank", "Pointer", "Address", "Wram", "Sram", "Snes", "Native"];

    /// <summary>Endings that keep a structure word the same word: <c>Pointers</c>, <c>Addresses</c>.</summary>
    private static readonly string[] Endings = ["", "s", "es"];

    /// <summary>The lowest 16-bit value in the cartridge's half of a bank.</summary>
    private const long BankUpperHalf = 0x8000;

    /// <summary>The highest 16-bit value.</summary>
    private const long WordMaximum = 0xffff;

    /// <summary>The highest 24-bit value.</summary>
    private const long LongAddressMaximum = 0xffffff;

    private static readonly DiagnosticDescriptor Rule = new(
        CartridgeAddressId,
        "Core holds a cartridge address",
        "{0} is {1}; the address belongs to the AssetExtraction codec that reads it, which produces the definition or identity",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The ROM is known only to the import and extraction pipeline. A cartridge address, bank, work-RAM offset " +
            "or pointer inside the simulation is scaffolding on the way out: the AssetExtraction codec reads it once and " +
            "produces the installed definition or the semantic identity, and Core keeps definition data under domain names " +
            "(#1411, docs/cartridge-addresses.md).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            start.RegisterSyntaxNodeAction(AnalyzeValueSite,
                SyntaxKind.VariableDeclarator, SyntaxKind.PropertyDeclaration, SyntaxKind.EnumMemberDeclaration,
                SyntaxKind.AttributeArgument, SyntaxKind.CaseSwitchLabel, SyntaxKind.CasePatternSwitchLabel,
                SyntaxKind.SwitchExpressionArm);
        });
    }

    /// <summary>
    /// Why <paramref name="value"/> is a plausible address, or null: a 24-bit address in a mapped bank anywhere, or a
    /// 16-bit value in the cartridge's half of a bank when <paramref name="cartridgeContext"/> names where it was declared.
    /// </summary>
    internal static string? Plausibility(long value, string? cartridgeContext)
    {
        if (value > WordMaximum && value <= LongAddressMaximum && MappedBank(value) is { } bank)
            return $"a 24-bit address in {bank}";
        if (value >= BankUpperHalf && value <= WordMaximum && cartridgeContext is not null)
            return $"a 16-bit address in {cartridgeContext}";
        return null;
    }

    /// <summary>
    /// The memory a 24-bit value addresses, or null when its bank is not mapped: the cartridge's ROM in banks
    /// <c>$80</c>–<c>$DF</c> (upper half), save RAM in <c>$70</c>–<c>$7D</c> (lower half), or work RAM in <c>$7E</c>–<c>$7F</c>.
    /// </summary>
    internal static string? MappedBank(long value)
    {
        long bank = value >> 16;
        long offset = value & WordMaximum;
        string number = bank.ToString("X2", CultureInfo.InvariantCulture);
        return bank switch
        {
            >= 0x80 and <= 0xdf when offset >= BankUpperHalf => $"cartridge ROM bank ${number}",
            >= 0x70 and <= 0x7d when offset < BankUpperHalf => $"save RAM bank ${number}",
            0x7e or 0x7f => $"work RAM bank ${number}",
            _ => null,
        };
    }

    /// <summary>Whether a type or namespace name contains one of <see cref="StructureWords"/> as a whole camel-case word.</summary>
    internal static bool NamesCartridgeStructure(string name) =>
        CoreVocabularyAnalyzer.Words(name).Any(word => StructureWords.Any(structure => Endings.Any(ending =>
            word.Length == structure.Length + ending.Length &&
            word.StartsWith(structure, StringComparison.OrdinalIgnoreCase) &&
            word.EndsWith(ending, StringComparison.OrdinalIgnoreCase))));

    /// <summary>Checks the values a constant, initializer, enum member, attribute argument or switch label declares.</summary>
    private static void AnalyzeValueSite(SyntaxNodeAnalysisContext context)
    {
        IEnumerable<ExpressionSyntax> roots = context.Node switch
        {
            VariableDeclaratorSyntax { Initializer.Value: { } value } declarator when IsCheckedDeclarator(declarator) => new[] { value },
            PropertyDeclarationSyntax { Initializer.Value: { } value } => new[] { value },
            EnumMemberDeclarationSyntax { EqualsValue.Value: { } value } => new[] { value },
            AttributeArgumentSyntax argument => new[] { argument.Expression },
            CaseSwitchLabelSyntax label => new[] { label.Value },
            CasePatternSwitchLabelSyntax label => ConstantPatterns(label.Pattern),
            SwitchExpressionArmSyntax arm => ConstantPatterns(arm.Pattern),
            _ => Array.Empty<ExpressionSyntax>(),
        };
        string? cartridgeContext = null;
        bool contextResolved = false;
        foreach (ExpressionSyntax root in roots)
        {
            foreach (ExpressionSyntax literal in LiteralValues(root, context.SemanticModel, context.CancellationToken))
            {
                if (context.SemanticModel.GetConstantValue(literal, context.CancellationToken) is not { HasValue: true, Value: { } boxed } ||
                    Integral(boxed) is not { } value)
                {
                    continue;
                }
                if (!contextResolved)
                {
                    cartridgeContext = CartridgeContext(context.Node, context.SemanticModel, context.CancellationToken);
                    contextResolved = true;
                }
                if (Plausibility(value, cartridgeContext) is { } why)
                {
                    string shown = "$" + value.ToString("X", CultureInfo.InvariantCulture);
                    context.ReportDiagnostic(Diagnostic.Create(Rule, literal.GetLocation(), shown, why));
                }
            }
        }
    }

    /// <summary>Whether a declarator is a field's (any field with an initializer) or a constant local's.</summary>
    private static bool IsCheckedDeclarator(VariableDeclaratorSyntax declarator) => declarator.Parent?.Parent switch
    {
        FieldDeclarationSyntax => true,
        LocalDeclarationStatementSyntax local => local.IsConst,
        _ => false,
    };

    /// <summary>The constant values a switch pattern compares against.</summary>
    private static IEnumerable<ExpressionSyntax> ConstantPatterns(PatternSyntax pattern) =>
        pattern.DescendantNodesAndSelf().OfType<ConstantPatternSyntax>().Select(constant => constant.Expression);

    /// <summary>
    /// The outermost integral constant expressions under <paramref name="root"/> that are written as numbers: a literal,
    /// or arithmetic, casts and parentheses over literals only. A reference to another constant is checked where that
    /// constant is declared.
    /// </summary>
    private static IEnumerable<ExpressionSyntax> LiteralValues(ExpressionSyntax root, SemanticModel model, CancellationToken cancellationToken)
    {
        var pending = new Stack<ExpressionSyntax>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            ExpressionSyntax expression = pending.Pop();
            if (IsWrittenNumber(expression) && model.GetConstantValue(expression, cancellationToken).HasValue)
            {
                yield return expression;
                continue;
            }
            foreach (ExpressionSyntax child in expression.ChildNodes().OfType<ExpressionSyntax>().Reverse())
                pending.Push(child);
            foreach (ArgumentSyntax argument in expression.ChildNodes().OfType<ArgumentListSyntax>().SelectMany(list => list.Arguments).Reverse())
                pending.Push(argument.Expression);
            foreach (ArgumentSyntax argument in expression.ChildNodes().OfType<ArgumentSyntax>().Reverse())
                pending.Push(argument.Expression);
            foreach (ExpressionSyntax element in expression.ChildNodes().OfType<InitializerExpressionSyntax>().SelectMany(list => list.Expressions).Reverse())
                pending.Push(element);
            foreach (ExpressionSyntax element in expression.ChildNodes().OfType<CollectionElementSyntax>().OfType<ExpressionElementSyntax>()
                .Select(item => item.Expression).Reverse())
            {
                pending.Push(element);
            }
        }
    }

    /// <summary>Whether an expression is a number written out: a literal, or arithmetic, casts and parentheses over literals only.</summary>
    private static bool IsWrittenNumber(ExpressionSyntax expression) => expression switch
    {
        LiteralExpressionSyntax literal => literal.IsKind(SyntaxKind.NumericLiteralExpression),
        ParenthesizedExpressionSyntax parenthesized => IsWrittenNumber(parenthesized.Expression),
        CastExpressionSyntax cast => IsWrittenNumber(cast.Expression),
        PrefixUnaryExpressionSyntax unary => IsWrittenNumber(unary.Operand),
        BinaryExpressionSyntax binary => IsWrittenNumber(binary.Left) && IsWrittenNumber(binary.Right),
        CheckedExpressionSyntax checkedExpression => IsWrittenNumber(checkedExpression.Expression),
        _ => false,
    };

    /// <summary>A boxed integral constant as a number, or null for any other constant.</summary>
    private static long? Integral(object boxed) => boxed switch
    {
        byte value => value,
        sbyte value => value,
        short value => value,
        ushort value => value,
        int value => value,
        uint value => value,
        long value => value,
        ulong value when value <= long.MaxValue => (long)value,
        _ => null,
    };

    /// <summary>
    /// Where a value was declared, when that place names a cartridge structure: a catalog-named file, or an enclosing type
    /// or namespace named for one. Null otherwise.
    /// </summary>
    private static string? CartridgeContext(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
    {
        if (CartridgeEncodingScanner.IsCatalogFile(node.SyntaxTree.FilePath))
            return $"the catalog file '{Path.GetFileName(node.SyntaxTree.FilePath)}'";
        if (node.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>() is not { } declaration ||
            model.GetDeclaredSymbol(declaration, cancellationToken) is not { } type)
        {
            return null;
        }
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (NamesCartridgeStructure(current.Name))
                return $"the cartridge structure '{current.Name}'";
        }
        for (INamespaceSymbol? space = type.ContainingNamespace; space is { IsGlobalNamespace: false }; space = space.ContainingNamespace)
        {
            if (NamesCartridgeStructure(space.Name))
                return $"the cartridge namespace '{space.ToDisplayString()}'";
        }
        if (IsAddressValuedEnum(type))
            return $"the address-valued enum '{type.Name}'";
        return null;
    }

    /// <summary>
    /// Whether <paramref name="type"/> is an enum whose members are cartridge words (#1452): it is not <c>[Flags]</c> and a
    /// member's value lies in the cartridge's half of a bank below the all-ones word. Identities are ordinal names with
    /// arbitrary values, so such a member is a routine or record address wherever the enum is declared. A <c>[Flags]</c>
    /// enum's high member is a bit, and the all-ones word is the native "none" sentinel that no routine or record starts
    /// at, so an ordinal enum whose only high member is that sentinel is not address-valued.
    /// </summary>
    internal static bool IsAddressValuedEnum(INamedTypeSymbol type) =>
        type.TypeKind == TypeKind.Enum &&
        !type.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.FlagsAttribute") &&
        type.GetMembers().OfType<IFieldSymbol>().Any(member =>
            member is { HasConstantValue: true, ConstantValue: { } boxed } &&
            Integral(boxed) is >= BankUpperHalf and < WordMaximum);
}
