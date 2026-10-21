using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// The v0.9.0 remnant detector (#1400, SME6438): in <c>SuperMetroid.Core</c>, no declared identifier (type, type parameter,
/// member, parameter, local, local function, namespace segment or file name) names the machine the game was ported from.
/// An identifier is split into camel-case words; a word that is one of <see cref="HardwareWords"/>, or one of them with a
/// plural or verb ending, is reported. Whole words only: <c>Object</c> is not <c>Obj</c>, <c>Roman</c> is not <c>Rom</c>.
/// Compatibility services may describe the hardware behaviour they reproduce in their documentation; identifiers never
/// may, so there is no identifier allowance. See <c>docs/core-vocabulary.md</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CoreVocabularyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core identifier names the hardware.</summary>
    public const string HardwareWordId = "SME6438";

    /// <summary>
    /// The machine's vocabulary, matched as whole camel-case words without regard to case. Each entry is a word a C#
    /// remake written by someone who had never seen the SNES would not use for its own state (v0.9.0 manifesto, section 5).
    /// </summary>
    internal static readonly ImmutableArray<string> HardwareWords =
    [
        "Snes", "Vram", "Cgram", "Oam", "Obj", "Dma", "Hdma", "Nmi", "Apu", "Spc", "Dsp", "Wram", "Ppu",
        "Native", "Rom", "Bank", "Pointer", "Address", "Cartridge", "Mode7", "Register",
    ];

    /// <summary>Endings that keep a word the same hardware word: <c>Banks</c>, <c>Addresses</c>, <c>Banked</c>, <c>Addressing</c>.</summary>
    private static readonly string[] Endings = ["", "s", "es", "ed", "ing"];

    private static readonly DiagnosticDescriptor Rule = new(
        HardwareWordId,
        "Core identifier names the hardware",
        "'{0}' names the hardware ('{1}'); name it for what it is in the game",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Core is a C# game. Its identifiers say what things are in the game, never which chip, memory or " +
            "cartridge structure the original stored them in; hardware behaviour the game needs lives in a compatibility " +
            "service named for the game (#1400, docs/core-vocabulary.md).");

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
            // A record's positional parameter is also its property; each location is reported once.
            var reported = new ConcurrentDictionary<Location, byte>();
            void Check(Action<Diagnostic> report, string name, Location location)
            {
                if (HardwareWord(name) is { } word && reported.TryAdd(location, 0))
                    report(Diagnostic.Create(Rule, location, name, word));
            }
            start.RegisterSymbolAction(symbolContext => AnalyzeSymbol(symbolContext, Check),
                SymbolKind.NamedType, SymbolKind.Field, SymbolKind.Property, SymbolKind.Method, SymbolKind.Event);
            start.RegisterSyntaxNodeAction(nodeContext => AnalyzeNode(nodeContext, Check),
                SyntaxKind.VariableDeclarator, SyntaxKind.SingleVariableDesignation, SyntaxKind.ForEachStatement,
                SyntaxKind.CatchDeclaration, SyntaxKind.LocalFunctionStatement, SyntaxKind.Parameter, SyntaxKind.TypeParameter,
                SyntaxKind.NamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration, SyntaxKind.FromClause,
                SyntaxKind.LetClause, SyntaxKind.JoinClause, SyntaxKind.QueryContinuation);
            start.RegisterSyntaxTreeAction(treeContext =>
            {
                string file = Path.GetFileNameWithoutExtension(treeContext.Tree.FilePath);
                Check(treeContext.ReportDiagnostic, file, Location.Create(treeContext.Tree, new TextSpan(0, 0)));
            });
        });
    }

    /// <summary>Reports a declared type or member; overrides and implementations of external members keep their names.</summary>
    private static void AnalyzeSymbol(SymbolAnalysisContext context, Action<Action<Diagnostic>, string, Location> check)
    {
        ISymbol symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared || symbol.Locations.FirstOrDefault(location => location.IsInSource) is not { } location)
            return;
        if (symbol is IMethodSymbol { MethodKind: not (MethodKind.Ordinary or MethodKind.UserDefinedOperator or MethodKind.Conversion) })
            return;
        if (symbol.IsOverride || ImplementsExternal(symbol))
            return;
        check(context.ReportDiagnostic, symbol.Name, location);
    }

    /// <summary>Reports a declared local, parameter, type parameter, local function, range variable or namespace segment.</summary>
    private static void AnalyzeNode(SyntaxNodeAnalysisContext context, Action<Action<Diagnostic>, string, Location> check)
    {
        switch (context.Node)
        {
            case VariableDeclaratorSyntax declarator when declarator.Parent?.Parent is not (FieldDeclarationSyntax or EventFieldDeclarationSyntax):
                check(context.ReportDiagnostic, declarator.Identifier.ValueText, declarator.Identifier.GetLocation());
                break;
            case SingleVariableDesignationSyntax designation:
                check(context.ReportDiagnostic, designation.Identifier.ValueText, designation.Identifier.GetLocation());
                break;
            case ForEachStatementSyntax loop:
                check(context.ReportDiagnostic, loop.Identifier.ValueText, loop.Identifier.GetLocation());
                break;
            case CatchDeclarationSyntax { Identifier.ValueText.Length: > 0 } caught:
                check(context.ReportDiagnostic, caught.Identifier.ValueText, caught.Identifier.GetLocation());
                break;
            case LocalFunctionStatementSyntax function:
                check(context.ReportDiagnostic, function.Identifier.ValueText, function.Identifier.GetLocation());
                break;
            case ParameterSyntax parameter when IsOwnParameter(parameter, context.SemanticModel, context.CancellationToken):
                check(context.ReportDiagnostic, parameter.Identifier.ValueText, parameter.Identifier.GetLocation());
                break;
            case TypeParameterSyntax typeParameter:
                check(context.ReportDiagnostic, typeParameter.Identifier.ValueText, typeParameter.Identifier.GetLocation());
                break;
            case BaseNamespaceDeclarationSyntax space:
                foreach (IdentifierNameSyntax segment in space.Name.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
                    check(context.ReportDiagnostic, segment.Identifier.ValueText, segment.GetLocation());
                break;
            case FromClauseSyntax from:
                check(context.ReportDiagnostic, from.Identifier.ValueText, from.Identifier.GetLocation());
                break;
            case LetClauseSyntax let:
                check(context.ReportDiagnostic, let.Identifier.ValueText, let.Identifier.GetLocation());
                break;
            case JoinClauseSyntax join:
                check(context.ReportDiagnostic, join.Identifier.ValueText, join.Identifier.GetLocation());
                break;
            case QueryContinuationSyntax continuation:
                check(context.ReportDiagnostic, continuation.Identifier.ValueText, continuation.Identifier.GetLocation());
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Whether a parameter's name is Core's to choose: not one that overrides or implements an external member, whose
    /// name the base declares.
    /// </summary>
    private static bool IsOwnParameter(ParameterSyntax parameter, SemanticModel model, CancellationToken cancellationToken)
    {
        if (model.GetDeclaredSymbol(parameter, cancellationToken) is not { ContainingSymbol: IMethodSymbol method })
            return true;
        return !method.IsOverride && !ImplementsExternal(method);
    }

    /// <summary>Whether a member implements an interface member declared outside this compilation.</summary>
    private static bool ImplementsExternal(ISymbol symbol)
    {
        if (symbol.ContainingType is not { } type)
            return false;
        foreach (INamedTypeSymbol contract in type.AllInterfaces)
        {
            if (contract.Locations.Any(location => location.IsInSource))
                continue;
            foreach (ISymbol member in contract.GetMembers())
            {
                if (SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(member), symbol))
                    return true;
            }
        }
        return false;
    }

    /// <summary>The hardware word <paramref name="identifier"/> contains as a whole camel-case word, or null.</summary>
    internal static string? HardwareWord(string identifier)
    {
        foreach (string word in Words(identifier))
        {
            foreach (string hardware in HardwareWords)
            {
                foreach (string ending in Endings)
                {
                    if (word.Length == hardware.Length + ending.Length &&
                        word.StartsWith(hardware, StringComparison.OrdinalIgnoreCase) &&
                        word.EndsWith(ending, StringComparison.OrdinalIgnoreCase))
                    {
                        return hardware;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// The camel-case words of an identifier: a word starts at an upper-case letter that follows a lower-case letter or
    /// digit, or that begins a capitalised word after an acronym (<c>OAMBuffer</c> is <c>OAM</c>, <c>Buffer</c>); digits
    /// stay with the word they follow (<c>Mode7</c>); underscores separate.
    /// </summary>
    internal static IEnumerable<string> Words(string identifier)
    {
        int start = 0;
        for (int index = 1; index <= identifier.Length; index++)
        {
            bool boundary = index == identifier.Length || identifier[index] == '_' || identifier[index - 1] == '_' ||
                char.IsUpper(identifier[index]) && (char.IsLower(identifier[index - 1]) || char.IsDigit(identifier[index - 1]) ||
                    index + 1 < identifier.Length && char.IsUpper(identifier[index - 1]) && char.IsLower(identifier[index + 1]));
            if (!boundary)
                continue;
            if (index > start && identifier[start] != '_')
                yield return identifier.Substring(start, index - start);
            start = index;
        }
    }
}
