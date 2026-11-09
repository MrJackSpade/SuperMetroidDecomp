using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// The documentation counterpart of SME6438 (#1482, SME6454): in <c>SuperMetroid.Core</c>, no comment or string literal
/// cites a cartridge address or value (<c>$BB:AAAA</c>, <c>BB:AAAA</c>, <c>$AAAA</c>, <c>$XX</c>) or names the machine
/// (WRAM, VRAM, CGRAM, OAM, SRAM, RAM, OBJ, BG1-BG4, DMA, HDMA, NMI, SNES, APU, SPC, DSP, PPU, ROM, native, bank,
/// cartridge, the processor's direct page, a jump mnemonic or a disassembly file), no comment names a routine label of the
/// original program (#1528), and none describes state by what a debugger sees, edits or stores (#1501). #1558 adds a bank
/// prefixed to an address, three-digit and bare hexadecimal values, comment hex literals, the program's addresses,
/// pointers, opcodes, registers, instruction lists and PLMs, word widths, CPU mnemonics, numbered palettes and priorities,
/// and routine labels written without an underscore: a compound name in a comment that names nothing in Core's code. Core's documentation states game behaviour in game
/// terms; provenance belongs to the import pipeline or a check's native catalog. Only a
/// <c>[CompatibilityService]</c> type, which reproduces hardware behaviour, or an <c>[EncodingOwner]</c> codec, which owns
/// an encoding, may describe the hardware. One diagnostic per comment or literal; there is no allowance list.
/// See <c>docs/core-vocabulary.md</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CoreDocumentationVocabularyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core comment or string literal names the hardware or the debugger, or cites an address.</summary>
    public const string DocumentationHardwareId = "SME6454";

    /// <summary>
    /// A cartridge or memory address or value: a bank and offset with or without its sign (<c>$BB:AAAA</c>, <c>BB:AAAA</c>),
    /// a bare four- to six-digit hexadecimal address, or a two-digit hexadecimal byte (<c>$1E</c>) (#1528).
    /// </summary>
    private static readonly Regex Address = new(
        @"\$?\b[0-9A-Fa-f]{2}:[0-9A-Fa-f]{4}\b|\$[0-9A-Fa-f]{3,6}\b|\$[0-9A-Fa-f]{2}\b", RegexOptions.CultureInvariant);

    /// <summary>
    /// A bank named with its number run on or joined (<c>native8C:D629</c>, <c>Native82</c>, <c>bank87</c>, <c>bank-$8F</c>),
    /// and a bare four-digit capital hexadecimal value holding both a letter and a digit (<c>F32B</c>, <c>E504</c>) (#1558).
    /// </summary>
    private static readonly Regex BankedValue = new(
        @"\b(?:native|bank)[-\s$]*[0-9A-Fa-f]{2}\b|\b(?=[0-9A-F]{0,3}[A-F])(?=[0-9A-F]{0,3}[0-9])[0-9A-F]{4}\b",
        RegexOptions.CultureInvariant);

    /// <summary>A hexadecimal literal written in a comment (<c>0x0100</c>) (#1558): a value spelled as the machine stores it.</summary>
    private static readonly Regex HexLiteral = new(@"\b0x[0-9A-Fa-f]+\b", RegexOptions.CultureInvariant);

    /// <summary>
    /// The program's own vocabulary in prose (#1558): its addresses, pointers, opcodes, registers, instruction lists and
    /// pre-instructions, PLMs, word widths and extension words, its assembly code, and numbered palettes and priorities.
    /// </summary>
    private static readonly Regex ProgramProse = new(
        @"\b(Address|Pointer|Opcode|Register|PLM)(s|es|ed|ing)?\b|\bpre-?instructions?\b|\binstruction[-\s]+lists?\b" +
        @"|\b(?:eight|sixteen|8|16)-bit\b|\bextension\s+words?\b|\baccumulator\s+mode\b" +
        @"|\bassembly\s+(?:code|language|listing|routine|source)s?\b|\bthe\s+(?:original\s+)?assembly\b" +
        @"|\bpalette\s+(?:zero|one|two|three|four|five|six|seven|[0-7])\b|\bpriority\s+(?:zero|one|two|three|[0-3])\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The processor's instructions and registers in prose, read case-sensitively (#1558): a capital mnemonic
    /// (<c>LSR ×4</c>) and a value carried through a register (<c>through A</c>).
    /// </summary>
    private static readonly Regex CpuProse = new(
        @"\b(?:LSR|ASL|ROL|ROR|LDA|STA|STZ|LDX|STX|LDY|STY|ADC|SBC|CMP|CPX|CPY|ORA|EOR|TAX|TAY|TXA|TYA|XBA|REP|SEP|PHA|PLA|PHX|PLX" +
        @"|PHY|PLY|PHB|PLB|PHP|PLP|BRA|BRL|BEQ|BNE|BCC|BCS|BMI|BPL|BVC|BVS|TSB|TRB|MVN|MVP|WAI)\b|\bthrough\s+[AXY]\b",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// A compound name in a comment (<c>EnemyMain</c>, <c>InitAI</c>, <c>frameCounter</c>) (#1558). It is a routine label of the
    /// original program unless Core's own code spells it: a name of Core's types, members, locals or the libraries Core
    /// calls is the code being described.
    /// </summary>
    private static readonly Regex CompoundName = new(@"\b(?:[A-Z][a-z0-9]+|[a-z][a-z0-9]*)(?:[A-Z][A-Za-z0-9]*)+\b", RegexOptions.CultureInvariant);

    /// <summary>A documentation comment's markup, whose cross-references the compiler checks itself.</summary>
    private static readonly Regex Markup = new(@"<[^>]*>", RegexOptions.CultureInvariant);

    /// <summary>
    /// The machine's vocabulary in prose, as whole words with a plural or verb ending; the processor's direct page, spaced
    /// or hyphenated; the debugger in any word form (debugger, debugging, debuggable); and a disassembly file (<c>.asm</c>).
    /// </summary>
    private static readonly Regex HardwareProse = new(
        @"\b(WRAM|VRAM|CGRAM|OAM|DMA|HDMA|NMI|SNES|APU|SPC|DSP|PPU|ROM|Native|Bank|Cartridge|RTS|RTL|JSR|JSL|JMP|JML)(s|es|ed|ing|ly)?\b|\b65C?816\b|\bdisassembl\w*" +
        @"|\bdirect[-\s]+pages?\b|\bdebugg\w*|\b\w+\.asm\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The machine's memories and layers, read case-sensitively so ordinary words are not matched: save RAM, RAM, the
    /// sprite layer's OBJ and the four background layers (#1528).
    /// </summary>
    private static readonly Regex HardwareTerms = new(@"\b(SRAM|RAM|OBJ|BG[1-4])s?\b", RegexOptions.CultureInvariant);

    /// <summary>
    /// A routine label of the original program in a comment: an identifier joined by underscores that has a capital letter
    /// (<c>Handle_MapScrollArrows</c>, <c>kRoomPlms_</c>) (#1528). The installed artwork's frame names are lower-case
    /// underscore-joined presentation keys (<c>draygon_frame_12</c>), so they are not labels; string literals are not read
    /// for labels at all.
    /// </summary>
    private static readonly Regex RoutineLabel = new(@"\b(?=[A-Za-z0-9_]*[A-Z])[A-Za-z][A-Za-z0-9]*_[A-Za-z0-9_]*", RegexOptions.CultureInvariant);

    /// <summary>The analyzers' same-line allowance markers: enforcement syntax, not prose, so their words are not read.</summary>
    private static readonly Regex AllowanceMarker = new(@"(cartridge-encoding|magic-number-audit): allow\(", RegexOptions.CultureInvariant);

    /// <summary>The attributes whose types may describe the hardware.</summary>
    private static readonly ImmutableHashSet<string> HardwareOwners =
        ["CompatibilityService", "CompatibilityServiceAttribute", "EncodingOwner", "EncodingOwnerAttribute"];

    private static readonly DiagnosticDescriptor Rule = new(
        DocumentationHardwareId,
        "Core documentation names the hardware or the debugger",
        "Core text names the hardware or the debugger ('{0}'); describe the game behaviour, and keep provenance in the import pipeline or a check's native catalog",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Core is a C# game: its comments and text say what things do in the game, never which address, chip or " +
            "memory the original used, and never what a debugger sees or stores. Only compatibility services and encoding " +
            "owners describe the hardware they reproduce (#1482, #1501, docs/core-vocabulary.md).");

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
            Compilation compilation = start.Compilation;
            var codeNames = new Lazy<ImmutableHashSet<string>>(() => CodeNames(compilation, start.CancellationToken));
            start.RegisterSyntaxTreeAction(tree => AnalyzeTree(tree, codeNames));
        });
    }

    /// <summary>Every name Core's code spells: each identifier of every source file, declared or used.</summary>
    private static ImmutableHashSet<string> CodeNames(Compilation compilation, CancellationToken cancellationToken) =>
        compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot(cancellationToken).DescendantTokens())
            .Where(token => token.IsKind(SyntaxKind.IdentifierToken))
            .Select(token => token.ValueText)
            .ToImmutableHashSet(StringComparer.Ordinal);

    /// <summary>Reports each comment and string literal of a tree that names the hardware.</summary>
    private static void AnalyzeTree(SyntaxTreeAnalysisContext context, Lazy<ImmutableHashSet<string>> codeNames)
    {
        SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);
        foreach (SyntaxTrivia trivia in root.DescendantTrivia(descendIntoTrivia: false))
        {
            if (!trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && !trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) &&
                !trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) &&
                !trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                continue;
            Report(context, trivia.ToFullString(), trivia.GetLocation(), trivia.Token.Parent, codeNames);
        }
        foreach (SyntaxToken token in root.DescendantTokens())
        {
            if (token.IsKind(SyntaxKind.StringLiteralToken) || token.IsKind(SyntaxKind.InterpolatedStringTextToken) ||
                token.IsKind(SyntaxKind.Utf8StringLiteralToken) || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken) ||
                token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken))
                Report(context, token.ValueText, token.GetLocation(), token.Parent, codeNames: null);
        }
    }

    /// <summary>
    /// Reports <paramref name="text"/> once when it names the hardware outside a hardware-owning type. A comment carries
    /// <paramref name="codeNames"/>, against which its compound names are read as routine labels; a string literal does not,
    /// and is not read for labels or hexadecimal literals.
    /// </summary>
    private static void Report(SyntaxTreeAnalysisContext context, string text, Location location, SyntaxNode? owner,
        Lazy<ImmutableHashSet<string>>? codeNames)
    {
        text = AllowanceMarker.Replace(text, string.Empty);
        string? word = Address.Match(text) is { Success: true } address ? address.Value
            : BankedValue.Match(text) is { Success: true } banked ? banked.Value
            : HardwareProse.Match(text) is { Success: true } prose ? prose.Value
            : HardwareTerms.Match(text) is { Success: true } term ? term.Value
            : ProgramProse.Match(text) is { Success: true } program ? program.Value
            : CpuProse.Match(text) is { Success: true } cpu ? cpu.Value
            : codeNames is not null ? CommentOnlyFinding(text, codeNames)
            : null;
        if (word is null || IsInHardwareOwner(owner))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, word));
    }

    /// <summary>A comment's routine label, underscore-joined or compound, or its hexadecimal literal; null when it has none.</summary>
    private static string? CommentOnlyFinding(string text, Lazy<ImmutableHashSet<string>> codeNames)
    {
        if (RoutineLabel.Match(text) is { Success: true } label)
            return label.Value;
        if (HexLiteral.Match(text) is { Success: true } literal)
            return literal.Value;
        string prose = Markup.Replace(text, " ");
        foreach (Match compound in CompoundName.Matches(prose))
        {
            if (!codeNames.Value.Contains(compound.Value))
                return compound.Value;
        }
        return null;
    }

    /// <summary>Whether a node lies in a type marked as a compatibility service or encoding owner.</summary>
    private static bool IsInHardwareOwner(SyntaxNode? node)
    {
        for (SyntaxNode? current = node; current is not null; current = current.Parent)
        {
            if (current is BaseTypeDeclarationSyntax declaration && declaration.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .Any(attribute => HardwareOwners.Contains(attribute.Name switch
                    {
                        QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
                        SimpleNameSyntax simple => simple.Identifier.ValueText,
                        _ => attribute.Name.ToString(),
                    })))
                return true;
        }
        return false;
    }
}
