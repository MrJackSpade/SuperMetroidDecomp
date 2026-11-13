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
/// may, so there is no identifier allowance. An identifier that spells a cartridge value in hexadecimal is the machine's
/// numbering too (#1554, <see cref="HexCode"/>): a capital code after a word (<c>Unused0B</c>, <c>PoseFD</c>,
/// <c>Backdrop2A</c>), an address run into a name (<c>Projectile86E314Frame</c>, <c>Projectile86C76eFrame</c>), or a
/// family of a type's members numbered in hexadecimal (<c>Row9</c> beside <c>RowA</c>). See <c>docs/core-vocabulary.md</c>.
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
        // #1481: routine labels name the CPU's return and jump instructions.
        "Rts", "Rtl", "Jsr", "Jsl", "Jmp", "Jml",
        // #1558: the machine's storage widths, its save memory, opcodes, and the program's PLM and enemy-projectile labels.
        "Word", "Byte", "Sram", "Opcode", "Plm", "Eproj",
        // #1685: the cartridge's record headers, which named rooms, room objects, effects, projectiles and enemies by the
        // structure the program stored them in.
        "Header",
    ];

    /// <summary>Endings that keep a word the same hardware word: <c>Banks</c>, <c>Addresses</c>, <c>Banked</c>, <c>Addressing</c>.</summary>
    private static readonly string[] Endings = ["", "s", "es", "ed", "ing"];

    private static readonly DiagnosticDescriptor Rule = new(
        HardwareWordId,
        "Core identifier names the hardware or spells a cartridge value",
        "'{0}' {1}; name it for what it is in the game",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Core is a C# game. Its identifiers say what things are in the game, never which chip, memory or " +
            "cartridge structure the original stored them in, and never number them as the cartridge did in hexadecimal; " +
            "hardware behaviour the game needs lives in a compatibility service named for the game (#1400, #1554, #1685, " +
            "docs/core-vocabulary.md).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            // A record's positional parameter is also its property; each location is reported once.
            var reported = new ConcurrentDictionary<Location, byte>();
            void Report(Action<Diagnostic> report, string name, Location location, string finding)
            {
                if (reported.TryAdd(location, 0))
                    report(Diagnostic.Create(Rule, location, name, finding));
            }
            void Check(Action<Diagnostic> report, string name, Location location)
            {
                if (Finding(name) is { } finding)
                    Report(report, name, location, finding);
            }
            start.RegisterSymbolAction(symbolContext => AnalyzeSymbol(symbolContext, Check),
                SymbolKind.NamedType, SymbolKind.Field, SymbolKind.Property, SymbolKind.Method, SymbolKind.Event);
            start.RegisterSymbolAction(symbolContext => AnalyzeFamilies(symbolContext, Report), SymbolKind.NamedType);
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

    /// <summary>
    /// Reports every member of a type's hexadecimal-numbered family (#1554): members whose names share a stem and end in
    /// capital codes of one length, where one code holds a hexadecimal letter and one a decimal digit, as <c>Row9</c> and
    /// <c>RowA</c>, or <c>Backdrop28</c> and <c>Backdrop2A</c>. Letters alone (<c>BossA</c>, <c>BossB</c>) and digits alone
    /// (<c>Frame09</c>, <c>Frame10</c>) are ordinary names and numbers.
    /// </summary>
    private static void AnalyzeFamilies(SymbolAnalysisContext context, Action<Action<Diagnostic>, string, Location, string> report)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        var families = new Dictionary<string, List<ISymbol>>(StringComparer.Ordinal);
        foreach (ISymbol member in type.GetMembers())
        {
            if (member.IsImplicitlyDeclared || member is IMethodSymbol { MethodKind: not MethodKind.Ordinary } ||
                member.IsOverride || ImplementsExternal(member) || CapitalHexSuffixLength(member.Name) is not { } length)
            {
                continue;
            }
            string key = member.Name.Substring(0, member.Name.Length - length) + "#" + length;
            if (!families.TryGetValue(key, out List<ISymbol>? family))
                families[key] = family = [];
            family.Add(member);
        }
        foreach (List<ISymbol> family in families.Values)
        {
            int length = CapitalHexSuffixLength(family[0].Name) ??
                throw new InvalidOperationException($"Capital-code family member {family[0].Name} has no capital code.");
            string Suffix(ISymbol member) => member.Name.Substring(member.Name.Length - length);
            if (family.Count < 2 || !family.Any(member => Suffix(member).Any(IsHexLetter)) ||
                !family.Any(member => Suffix(member).Any(IsDecimalDigit)))
            {
                continue;
            }
            foreach (ISymbol member in family)
            {
                if (member.Locations.FirstOrDefault(location => location.IsInSource) is { } location)
                    report(context.ReportDiagnostic, member.Name, location, $"numbers its family in hexadecimal ('{Suffix(member)}')");
            }
        }
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

    /// <summary>What a declared name says that Core may not, as the diagnostic states it, or null when it says nothing of the machine.</summary>
    internal static string? Finding(string identifier)
    {
        if (HardwareWord(identifier) is { } word)
            return $"names the hardware ('{word}')";
        return HexCode(identifier) is { } code ? $"spells the cartridge value '{code}' in hexadecimal" : null;
    }

    /// <summary>
    /// The hexadecimal code <paramref name="identifier"/> spells for a cartridge value, or null (#1554). A code follows a
    /// word and ends its own word (the end of the name, an underscore, or a capitalised word): a run of two or more capital
    /// hexadecimal characters after a lower-case letter holding at least one hexadecimal letter (<c>Unused0B</c>,
    /// <c>PoseFD</c>, <c>Backdrop2A</c>, <c>Projectile86E314Frame</c>), or a run of four or more hexadecimal characters of
    /// either case that starts with a decimal digit after a letter and holds a letter (<c>Projectile86C76eFrame</c>).
    /// Ordinary words are not codes: a capital that starts a lower-case word begins that word (<c>Add</c>, <c>Face</c>,
    /// <c>Dead</c>, <c>Feed</c>, <c>Bad</c>; <c>Shift8Add</c> is <c>8</c> then <c>Add</c>), a run of capitals that continues
    /// into other capitals is an acronym, a lone letter is a word or an ordinal (<c>BossA</c>), and decimal numbers,
    /// zero-padded or not (<c>Frame12</c>, <c>Frame05</c>), are numbers.
    /// </summary>
    internal static string? HexCode(string identifier)
    {
        foreach (string part in identifier.Split('_'))
        {
            foreach ((int start, int end) in CapitalHexRuns(part))
            {
                if (end - start >= 2 && HasHexLetter(part, start, end))
                    return part.Substring(start, end - start);
            }
            if (MixedHexCode(part) is { } code)
                return code;
        }
        return null;
    }

    /// <summary>
    /// The runs of capital hexadecimal characters in a name part that follow a lower-case letter and end their word, as
    /// start and end offsets. A final capital letter followed by lower case starts the next word and is not part of the
    /// run (<c>Row2Dead</c> is <c>Row</c>, <c>2</c>, <c>Dead</c>); a run followed by another capital that is not a word's
    /// start is part of an acronym and is not a run.
    /// </summary>
    private static IEnumerable<(int Start, int End)> CapitalHexRuns(string part)
    {
        int index = 1;
        while (index < part.Length)
        {
            if (!IsCapitalHex(part[index]) || !char.IsLower(part[index - 1]))
            {
                index++;
                continue;
            }
            int end = index;
            while (end < part.Length && IsCapitalHex(part[end]))
                end++;
            int stop = end;
            if (stop < part.Length && char.IsLower(part[stop]) && IsHexLetter(part[stop - 1]))
                stop--;
            bool endsWord = stop == part.Length ||
                char.IsUpper(part[stop]) && stop + 1 < part.Length && char.IsLower(part[stop + 1]);
            if (endsWord && stop > index)
                yield return (index, stop);
            index = end;
        }
    }

    /// <summary>
    /// A run of four or more hexadecimal characters of either case that starts with a decimal digit after a letter, holds a
    /// letter and ends its word, or null. A capitalised segment of the run with no decimal digit is the next word
    /// (<c>86C76eFrame</c> ends before <c>Frame</c>, <c>8AddMagnitude</c> is <c>8</c>), while one with a digit continues
    /// the code (<c>86Caa4Frame</c> is <c>86Caa4</c>).
    /// </summary>
    private static string? MixedHexCode(string part)
    {
        int index = 1;
        while (index < part.Length)
        {
            if (!IsDecimalDigit(part[index]) || !char.IsLetter(part[index - 1]))
            {
                index++;
                continue;
            }
            int end = index;
            while (end < part.Length && Uri.IsHexDigit(part[end]))
                end++;
            int stop = end;
            for (int at = index + 1; at < end; at++)
            {
                if (!char.IsUpper(part[at]) || at + 1 >= part.Length || !char.IsLower(part[at + 1]))
                    continue;
                int next = at + 1;
                while (next < end && !char.IsUpper(part[next]))
                    next++;
                if (!part.Substring(at, next - at).Any(IsDecimalDigit))
                {
                    stop = at;
                    break;
                }
            }
            if (stop < part.Length && char.IsLower(part[stop]))
                stop = index;
            if (stop - index >= 4 && part.Substring(index, stop - index).Any(char.IsLetter))
                return part.Substring(index, stop - index);
            index = end;
        }
        return null;
    }

    /// <summary>
    /// The length of the capital hexadecimal code that ends <paramref name="identifier"/> after a lower-case letter, one
    /// character or more, or null: the suffix a hexadecimal-numbered family varies.
    /// </summary>
    private static int? CapitalHexSuffixLength(string identifier)
    {
        string part = identifier.Substring(identifier.LastIndexOf('_') + 1);
        foreach ((int start, int end) in CapitalHexRuns(part))
        {
            if (end == part.Length)
                return end - start;
        }
        return null;
    }

    private static bool HasHexLetter(string part, int start, int end)
    {
        for (int index = start; index < end; index++)
        {
            if (IsHexLetter(part[index]))
                return true;
        }
        return false;
    }

    private static bool IsCapitalHex(char character) => IsDecimalDigit(character) || IsHexLetter(character);

    private static bool IsHexLetter(char character) => character is >= 'A' and <= 'F';

    private static bool IsDecimalDigit(char character) => character is >= '0' and <= '9';

    /// <summary>The hardware word <paramref name="identifier"/> contains as a whole camel-case word, or null.</summary>
    internal static string? HardwareWord(string identifier)
    {
        // An address in a label is its own underscore-separated part; camel-case splitting would break it at each letter.
        foreach (string part in identifier.Split('_'))
        {
            if (IsAddressWord(part))
                return part;
        }
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
    /// Whether a word is a cartridge address spelled into a name (#1481): four or six hexadecimal digits with at least one
    /// decimal digit among them, as in <c>RTS_86BB30</c> or <c>Instr_A0B3</c>. Requiring a digit keeps words such as
    /// <c>Face</c> or <c>Bead</c>.
    /// </summary>
    internal static bool IsAddressWord(string word) =>
        word.Length is 4 or 6 && word.All(Uri.IsHexDigit) && word.Any(char.IsDigit);

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
