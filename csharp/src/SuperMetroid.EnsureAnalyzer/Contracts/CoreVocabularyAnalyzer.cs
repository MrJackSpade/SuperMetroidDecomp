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
/// plural or verb ending, is reported, as is a run of words that is one of <see cref="HardwarePhrases"/>. Whole words only: <c>Object</c> is not <c>Obj</c>, <c>Roman</c> is not <c>Rom</c>.
/// Compatibility services may describe the hardware behaviour they reproduce in their documentation; identifiers never
/// may, so there is no identifier allowance. An identifier that spells a cartridge value in hexadecimal is the machine's
/// numbering too (#1554, <see cref="HexCode"/>): a capital code after a word (<c>Unused0B</c>, <c>PoseFD</c>,
/// <c>Backdrop2A</c>), an address run into a name (<c>Projectile86E314Frame</c>, <c>Projectile86C76eFrame</c>), or a
/// family of a type's members numbered in hexadecimal (<c>Row9</c> beside <c>RowA</c>). A name that says <c>Callback</c>
/// names the original's function-pointer dispatch (#1624, <see cref="DispatchWords"/>) unless what it declares holds a C#
/// delegate the port passes (<see cref="HoldsDelegate"/>); a name that says <c>Dispatch</c> names the original's jump
/// through a table of routines (#1624, <see cref="RoutineDispatchWords"/>) under the same allowance, unless it says what it
/// sends (<see cref="SentThingWords"/>). A name that says <c>Function</c> names the enemy record's AI
/// function field (#1701, <see cref="BehaviourFieldWords"/>) under the same delegate allowance. An area's name followed
/// by a number and another word numbers the area by the cartridge's tileset (#1720, <see cref="TilesetNumber"/>):
/// <c>Brinstar6DarkPattern</c>. A name that says <c>AlphaHandler</c>, <c>BetaHandler</c> or <c>SetupCode</c> names the
/// original's routine (#1721, <see cref="RoutinePhrases"/>). A name that says <c>Tile</c> or <c>Metatile</c> names the
/// hardware's tile (#1726, <see cref="TileWords"/>) unless it is one of the game's own tiles: the area map's, a room layer
/// named as the destination engine names it, Ceres station's falling ceiling tiles (<see cref="GameTilePhrases"/>), or a
/// value whose type is named for the map (<see cref="NamedForMap"/>); and a name that says <c>Underflow</c> names the CPU's
/// arithmetic (#1726, <see cref="MachineArithmeticWords"/>). See <c>docs/core-vocabulary.md</c>.
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
        // #1624: the interpreter's vocabulary for the original's streams, which named a cue, a frame, a reveal shape or a
        // momentum response by the instruction, operand or numbered handler command that carried it.
        "Command", "Operand", "Instruction",
        // #1685: the cartridge's record headers, which named rooms, room objects, effects, projectiles and enemies by the
        // structure the program stored them in.
        "Header",
        // #1694: the cartridge's frame built from hardware sprite pieces, the display's raster line, and the cartridge's
        // one-word background tilemap. A room layer named as the destination engine names it (TileMap, TileMapLayer) is
        // two words and stays the game's own.
        "Spritemap", "Scanline", "Tilemap",
    ];

    /// <summary>
    /// The machine's two-word names, matched as consecutive whole camel-case words without regard to case, the last with
    /// the endings of <see cref="Endings"/> (#1701): the enemy record's AI function field, which named an enemy's or a
    /// boss's behaviour state by the routine pointer the program stored (<c>AiFunction</c>, <c>EnemyFunction</c>,
    /// <c>FightFunction</c>). Each word alone stays the game's own: an enemy, a fight, a function.
    /// </summary>
    internal static readonly ImmutableArray<ImmutableArray<string>> HardwarePhrases =
    [
        ["Ai", "Function"], ["Enemy", "Function"], ["Fight", "Function"],
    ];

    /// <summary>
    /// The original's dispatch vocabulary (#1624), matched as whole camel-case words like <see cref="HardwareWords"/>: the
    /// cartridge stored a room's setup and main behaviour, a beam's frame behaviour or a sinking step as a function pointer
    /// it called back, and a Core name that says <c>Callback</c> names that pointer instead of the behaviour. A declaration
    /// that holds a C# delegate the port passes (a delegate type, or a field, property, event, parameter or local whose type
    /// holds one) is a genuine callback and keeps the word.
    /// </summary>
    internal static readonly ImmutableArray<string> DispatchWords = ["Callback"];

    /// <summary>
    /// The original's routine dispatch (#1624), matched as whole camel-case words like <see cref="DispatchWords"/>: the
    /// cartridge ran a game state, an enemy's AI, a pose's movement or a boss's state by jumping through a table of routine
    /// pointers, and a Core name that says <c>Dispatch</c> (a <c>DispatchMainGameplay</c> method, an <c>IEnemyDispatch</c>
    /// service, a <c>SamusPoseDispatch</c> table, a <c>Dispatcher</c> index, a <c>dispatched</c> local, a
    /// <c>RedispatchBeta</c> step) names that jump instead of the phase or step it runs. A name that also says what it
    /// sends (<see cref="SentThingWords"/>: a music request's <c>PendingMusicDispatch</c>) uses the English verb of sending,
    /// and a declaration that holds a C# delegate the port passes dispatches through that delegate; both keep the word.
    /// </summary>
    internal static readonly ImmutableArray<string> RoutineDispatchWords = ["Dispatch", "Dispatcher", "Redispatch"];

    /// <summary>
    /// The things a name may say it dispatches, sending them (#1624): music, a sound, audio, a request, a message or an
    /// event. A <see cref="RoutineDispatchWords"/> word beside one of them is the English verb, not the original's jump.
    /// </summary>
    internal static readonly ImmutableArray<string> SentThingWords = ["Music", "Sound", "Audio", "Request", "Message", "Event"];

    /// <summary>
    /// The original's routine names (#1721), two-word names matched like <see cref="HardwarePhrases"/>: Samus's frame ran an
    /// alpha and a beta handler the control setups installed (<c>SamusAlphaHandler</c>, <c>SamusBetaHandler</c>), and a door
    /// ran the setup code its record pointed at, which an interpreter replayed (<c>DoorSetupCode</c>,
    /// <c>DoorSetupCodeInterpreter</c>). Core names the phase or the setup instead: Samus's input and movement modes, a door's
    /// setup. Each word alone stays the game's own: an alpha, a beta, a handler, a setup, a code.
    /// </summary>
    internal static readonly ImmutableArray<ImmutableArray<string>> RoutinePhrases =
    [
        ["Alpha", "Handler"], ["Beta", "Handler"], ["Setup", "Code"],
    ];

    /// <summary>
    /// The enemy record's AI function field (#1701), matched as whole camel-case words like <see cref="DispatchWords"/>: the
    /// cartridge stored an enemy's or a boss's behaviour state, a part's wait and the state a cue installs as the routine
    /// pointer its record's function field held, and a Core name that says <c>Function</c> (a <c>TorizoFunction</c> enum, a
    /// <c>Function</c>, <c>SetFunction</c> or <c>FunctionTimer</c> member, a <c>function</c> local) names that field instead of
    /// the phase, action or movement it selects. A declaration that holds a C# delegate the port passes (a <c>Func&lt;int&gt;</c>
    /// field or parameter, a delegate type) is a genuine function and keeps the word, and a word that only shares its letters
    /// (<c>Functional</c>, <c>Func</c>, <c>Functor</c>) is another word.
    /// </summary>
    internal static readonly ImmutableArray<string> BehaviourFieldWords = ["Function"];

    /// <summary>
    /// The hardware's tile (#1726), matched as whole camel-case words like <see cref="HardwareWords"/>: the eight-pixel
    /// picture the machine drew its backgrounds and sprites from (<c>AnimatedTileObject</c>, <c>TileFlip</c>,
    /// <c>BehindFrontTiles</c>, a <c>tiles</c> local), and the block named as four of them (<c>TankMetatile</c>). Core names
    /// the block, the picture, the block animation or the eight-pixel cell it means. The game's own tiles keep the word
    /// (<see cref="GameTilePhrases"/>, <see cref="NamedForMap"/>).
    /// </summary>
    internal static readonly ImmutableArray<string> TileWords = ["Tile", "Metatile"];

    /// <summary>
    /// The game's own tiles (#1726), each a <c>Tile</c> word of <see cref="TileWords"/> beside a word, matched without
    /// regard to case, the neighbour with the endings of <see cref="Endings"/>: the area map's tiles, <c>Map</c> before it
    /// (<c>MapTiles</c>, <c>IsMapTileExplored</c>); a room layer named as the destination engine names it, <c>Map</c> after
    /// it (<c>TileMap</c>, <c>TileMapLayer</c>); and the ceiling tiles that fall in Ceres station and the room named for
    /// them, <c>Falling</c> before it or <c>Fall</c> after it (<c>FallingTile</c>, <c>CeresTileFall</c>). A phrase is written
    /// before, the tile word, after.
    /// </summary>
    internal static readonly ImmutableArray<(string? Before, string? After)> GameTilePhrases =
    [
        ("Map", null), (null, "Map"), ("Falling", null), (null, "Fall"),
    ];

    /// <summary>
    /// The original's arithmetic (#1726), matched as whole camel-case words like <see cref="HardwareWords"/>: a value that
    /// underflowed was the CPU's subtraction borrowing past zero (<c>speedUnderflowed</c>). Core says what the value did in
    /// the game: it passed zero, ran out, turned round.
    /// </summary>
    internal static readonly ImmutableArray<string> MachineArithmeticWords = ["Underflow"];

    /// <summary>
    /// The game's areas, as the camel-case words that name them (#1720): an area's name followed by a number is the
    /// cartridge's graphics-set number for one of its tilesets (<see cref="TilesetNumber"/>).
    /// </summary>
    internal static readonly ImmutableArray<ImmutableArray<string>> AreaNames =
    [
        ["Crateria"], ["Brinstar"], ["Norfair"], ["Maridia"], ["Tourian"], ["Ceres"], ["Wrecked", "Ship"],
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
            "cartridge structure the original stored them in, never number them as the cartridge did in hexadecimal or by its " +
            "tilesets, and " +
            "never call a behaviour the callback the original dispatched it through, the routine table it jumped through, " +
            "the function field its record held or the handler or setup code routine it ran, " +
            "and never name the hardware's tile where they mean a block, a picture, a block animation or a cell, or the CPU's " +
            "arithmetic; hardware behaviour the game needs lives in " +
            "a compatibility service named for the game (#1400, #1554, #1624, #1685, #1694, #1701, #1720, #1721, #1726, " +
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
            void Check(Action<Diagnostic> report, string name, Location location, Func<bool> holdsDelegate, Func<bool> typedForMap)
            {
                if (Finding(name, holdsDelegate, typedForMap) is { } finding)
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
                // A file is named for its type, so a file named for a delegate type the file declares is that delegate's;
                // a file holds no value, so its name is never typed for the map.
                Check(treeContext.ReportDiagnostic, file, Location.Create(treeContext.Tree, new TextSpan(0, 0)),
                    () => treeContext.Tree.GetRoot(treeContext.CancellationToken).DescendantNodes()
                        .OfType<DelegateDeclarationSyntax>().Any(declaration => declaration.Identifier.ValueText == file),
                    static () => false);
            });
        });
    }

    /// <summary>Reports a declared type or member; overrides and implementations of external members keep their names.</summary>
    private static void AnalyzeSymbol(SymbolAnalysisContext context,
        Action<Action<Diagnostic>, string, Location, Func<bool>, Func<bool>> check)
    {
        ISymbol symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared || symbol.Locations.FirstOrDefault(location => location.IsInSource) is not { } location)
            return;
        if (symbol is IMethodSymbol { MethodKind: not (MethodKind.Ordinary or MethodKind.UserDefinedOperator or MethodKind.Conversion) })
            return;
        if (symbol.IsOverride || ImplementsExternal(symbol))
            return;
        check(context.ReportDiagnostic, symbol.Name, location, () => DeclaresDelegate(symbol), () => NamedForMap(HeldType(symbol)));
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
    private static void AnalyzeNode(SyntaxNodeAnalysisContext context,
        Action<Action<Diagnostic>, string, Location, Func<bool>, Func<bool>> check)
    {
        // Whether the local or parameter the node declares holds a delegate; asked only of a name that says a dispatch word.
        bool HoldsDeclaredDelegate() =>
            context.SemanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken) is { } declared && DeclaresDelegate(declared);
        static bool DeclaresNoDelegate() => false;
        // Whether the local or parameter the node declares holds a value typed for the map; asked only of a name that
        // says a tile word.
        bool DeclaredForMap() =>
            context.SemanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken) is { } declared &&
            NamedForMap(HeldType(declared));
        static bool NotTypedForMap() => false;
        switch (context.Node)
        {
            case VariableDeclaratorSyntax declarator when declarator.Parent?.Parent is not (FieldDeclarationSyntax or EventFieldDeclarationSyntax):
                check(context.ReportDiagnostic, declarator.Identifier.ValueText, declarator.Identifier.GetLocation(), HoldsDeclaredDelegate, DeclaredForMap);
                break;
            case SingleVariableDesignationSyntax designation:
                check(context.ReportDiagnostic, designation.Identifier.ValueText, designation.Identifier.GetLocation(), HoldsDeclaredDelegate, DeclaredForMap);
                break;
            case ForEachStatementSyntax loop:
                check(context.ReportDiagnostic, loop.Identifier.ValueText, loop.Identifier.GetLocation(), HoldsDeclaredDelegate, DeclaredForMap);
                break;
            case CatchDeclarationSyntax { Identifier.ValueText.Length: > 0 } caught:
                check(context.ReportDiagnostic, caught.Identifier.ValueText, caught.Identifier.GetLocation(), DeclaresNoDelegate, NotTypedForMap);
                break;
            case LocalFunctionStatementSyntax function:
                check(context.ReportDiagnostic, function.Identifier.ValueText, function.Identifier.GetLocation(), DeclaresNoDelegate, NotTypedForMap);
                break;
            case ParameterSyntax parameter when IsOwnParameter(parameter, context.SemanticModel, context.CancellationToken):
                check(context.ReportDiagnostic, parameter.Identifier.ValueText, parameter.Identifier.GetLocation(), HoldsDeclaredDelegate, DeclaredForMap);
                break;
            case TypeParameterSyntax typeParameter:
                check(context.ReportDiagnostic, typeParameter.Identifier.ValueText, typeParameter.Identifier.GetLocation(), DeclaresNoDelegate, NotTypedForMap);
                break;
            case BaseNamespaceDeclarationSyntax space:
                foreach (IdentifierNameSyntax segment in space.Name.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
                    check(context.ReportDiagnostic, segment.Identifier.ValueText, segment.GetLocation(), DeclaresNoDelegate, NotTypedForMap);
                break;
            case FromClauseSyntax from:
                check(context.ReportDiagnostic, from.Identifier.ValueText, from.Identifier.GetLocation(), DeclaresNoDelegate, NotTypedForMap);
                break;
            case LetClauseSyntax let:
                check(context.ReportDiagnostic, let.Identifier.ValueText, let.Identifier.GetLocation(), DeclaresNoDelegate, NotTypedForMap);
                break;
            case JoinClauseSyntax join:
                check(context.ReportDiagnostic, join.Identifier.ValueText, join.Identifier.GetLocation(), DeclaresNoDelegate, NotTypedForMap);
                break;
            case QueryContinuationSyntax continuation:
                check(context.ReportDiagnostic, continuation.Identifier.ValueText, continuation.Identifier.GetLocation(), DeclaresNoDelegate, NotTypedForMap);
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

    /// <summary>
    /// What a declared name says that Core may not, as the diagnostic states it, or null when it says nothing of the machine.
    /// <paramref name="holdsDelegate"/> says whether the declaration holds a C# delegate; it is asked only of a name that
    /// says a dispatch word or a behaviour-field word. <paramref name="typedForMap"/> says whether the declaration holds a
    /// value whose type is named for the map (<see cref="NamedForMap"/>); it is asked only of a name that says a tile word.
    /// </summary>
    internal static string? Finding(string identifier, Func<bool> holdsDelegate, Func<bool> typedForMap)
    {
        if (HardwareWord(identifier) is { } word)
            return $"names the hardware ('{word}')";
        if (HardwareTile(identifier) is { } tile && !typedForMap())
            return $"names the hardware's tile ('{tile}')";
        if (ContainedWord(identifier, MachineArithmeticWords) is { } arithmetic)
            return $"names the CPU's arithmetic ('{arithmetic}')";
        if (ContainedWord(identifier, DispatchWords) is { } dispatch && !holdsDelegate())
            return $"names the original's dispatch ('{dispatch}') where it declares no delegate";
        if (ContainedWord(identifier, RoutineDispatchWords) is { } routine && ContainedWord(identifier, SentThingWords) is null &&
            !holdsDelegate())
        {
            return $"names the original's routine dispatch ('{routine}') where it sends nothing and declares no delegate";
        }
        if (ContainedPhrase(identifier, RoutinePhrases) is { } routineName)
            return $"names the original's routine ('{routineName}')";
        if (ContainedWord(identifier, BehaviourFieldWords) is { } field && !holdsDelegate())
            return $"names the enemy record's AI function field ('{field}') where it declares no delegate";
        if (HexCode(identifier) is { } code)
            return $"spells the cartridge value '{code}' in hexadecimal";
        return TilesetNumber(identifier) is { } tileset ? $"numbers its area by the cartridge's tileset ('{tileset}')" : null;
    }

    /// <summary>
    /// The area and tileset number <paramref name="identifier"/> spells, joined, or null (#1720): an area's name
    /// (<see cref="AreaNames"/>, without regard to case) carrying a decimal number, run into its last word or as its own
    /// underscore-separated word, that another word follows, as in <c>Brinstar6DarkPattern</c>, <c>Crateria23Mechanical</c>,
    /// <c>Norfair9And10CavernVertical</c>, <c>WreckedShip45EntranceHall</c> or <c>Crateria_2_BrickRoom</c>. A number that
    /// ends the name is an ordinal, as a room's doors are numbered (<c>ElevToBlueBrinstar0</c>), and a number after any
    /// other word is the game's own (<c>Metroids2Default</c>). An area followed by a lone capital letter is not read as a
    /// tileset letter: <c>MaridiaBBrownWall</c> and <c>BrinstarETank</c>, an energy tank, have one shape.
    /// </summary>
    internal static string? TilesetNumber(string identifier)
    {
        string[] words = Words(identifier).ToArray();
        foreach (ImmutableArray<string> area in AreaNames)
        {
            for (int start = 0; start + area.Length <= words.Length; start++)
            {
                bool matches = true;
                for (int offset = 0; offset < area.Length - 1 && matches; offset++)
                    matches = string.Equals(words[start + offset], area[offset], StringComparison.OrdinalIgnoreCase);
                if (!matches)
                    continue;
                int last = start + area.Length - 1;
                string word = words[last];
                string name = area[area.Length - 1];
                if (!word.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                    continue;
                string number = word.Substring(name.Length);
                int next = last + 1;
                if (number.Length == 0 && next < words.Length && words[next].All(IsDecimalDigit))
                    number = words[next++];
                if (number.Length > 0 && number.All(IsDecimalDigit) && next < words.Length)
                    return string.Concat(area) + number;
            }
        }
        return null;
    }

    /// <summary>
    /// The tile word of <see cref="TileWords"/> that <paramref name="identifier"/> contains as a whole camel-case word, with
    /// the endings of <see cref="Endings"/>, where it is not one of the game's own tiles of <see cref="GameTilePhrases"/>,
    /// or null (#1726). Each occurrence is judged by its own neighbours: <c>MapTileFlip</c> keeps its tile, while
    /// <c>FallingTileFrameTiles</c> names the hardware's in its second.
    /// </summary>
    internal static string? HardwareTile(string identifier)
    {
        string[] words = Words(identifier).ToArray();
        for (int index = 0; index < words.Length; index++)
        {
            foreach (string listed in TileWords)
            {
                if (!IsWordWithEnding(words[index], listed))
                    continue;
                bool gameTile = false;
                foreach ((string? before, string? after) in GameTilePhrases)
                {
                    gameTile |= (before is not null && index > 0 && IsWordWithEnding(words[index - 1], before)) ||
                        (after is not null && index + 1 < words.Length && IsWordWithEnding(words[index + 1], after));
                }
                if (!gameTile)
                    return listed;
            }
        }
        return null;
    }

    /// <summary>
    /// Whether a type is named for the map (#1726): its name has the camel-case word <c>Map</c>, or it is an array or
    /// constructed generic type of such a type (<c>MapCoordinateJsonDocument[]</c>, <c>Position16&lt;MapTiles&gt;</c>). A
    /// value of it named <c>tile</c> is one of the area map's tiles.
    /// </summary>
    internal static bool NamedForMap(ITypeSymbol? type) => type switch
    {
        null => false,
        IArrayTypeSymbol array => NamedForMap(array.ElementType),
        INamedTypeSymbol named => Words(named.Name).Any(word => IsWordWithEnding(word, "Map")) ||
            named.IsGenericType && named.TypeArguments.Any(NamedForMap),
        _ => false,
    };

    /// <summary>The type of the value a declared field, property, parameter or local holds, or null for any other symbol.</summary>
    private static ITypeSymbol? HeldType(ISymbol symbol) => symbol switch
    {
        IFieldSymbol field => field.Type,
        IPropertySymbol property => property.Type,
        IParameterSymbol parameter => parameter.Type,
        ILocalSymbol local => local.Type,
        _ => null,
    };

    /// <summary>
    /// Whether a declared symbol holds a C# delegate (#1624): a delegate type, an event, or a field, property, parameter or
    /// local whose type holds one.
    /// </summary>
    private static bool DeclaresDelegate(ISymbol symbol) => symbol switch
    {
        INamedTypeSymbol type => type.TypeKind == TypeKind.Delegate,
        IEventSymbol => true,
        IFieldSymbol field => HoldsDelegate(field.Type),
        IPropertySymbol property => HoldsDelegate(property.Type),
        IParameterSymbol parameter => HoldsDelegate(parameter.Type),
        ILocalSymbol local => HoldsDelegate(local.Type),
        _ => false,
    };

    /// <summary>
    /// Whether a type holds a C# delegate: is one (<c>Action</c>, <c>Func&lt;int&gt;</c>), or is an array or constructed
    /// generic type of one (<c>Action[]</c>, <c>List&lt;Func&lt;int&gt;&gt;</c>).
    /// </summary>
    internal static bool HoldsDelegate(ITypeSymbol? type) => type switch
    {
        null => false,
        { TypeKind: TypeKind.Delegate } => true,
        IArrayTypeSymbol array => HoldsDelegate(array.ElementType),
        INamedTypeSymbol { IsGenericType: true } named => named.TypeArguments.Any(HoldsDelegate),
        _ => false,
    };

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
        return ContainedWord(identifier, HardwareWords) ?? ContainedPhrase(identifier, HardwarePhrases);
    }

    /// <summary>
    /// The word of <paramref name="vocabulary"/> that <paramref name="identifier"/> contains as a whole camel-case word,
    /// ignoring case and allowing the plural and verb <see cref="Endings"/>, or null.
    /// </summary>
    private static string? ContainedWord(string identifier, ImmutableArray<string> vocabulary)
    {
        foreach (string word in Words(identifier))
        {
            foreach (string listed in vocabulary)
            {
                if (IsWordWithEnding(word, listed))
                    return listed;
            }
        }
        return null;
    }

    /// <summary>
    /// The phrase of <paramref name="phrases"/> that <paramref name="identifier"/> contains as consecutive whole camel-case
    /// words, ignoring case and allowing the <see cref="Endings"/> on its last word, joined, or null.
    /// </summary>
    private static string? ContainedPhrase(string identifier, ImmutableArray<ImmutableArray<string>> phrases)
    {
        string[] words = Words(identifier).ToArray();
        foreach (ImmutableArray<string> phrase in phrases)
        {
            for (int start = 0; start + phrase.Length <= words.Length; start++)
            {
                bool matches = true;
                for (int offset = 0; offset < phrase.Length && matches; offset++)
                {
                    string word = words[start + offset];
                    matches = offset == phrase.Length - 1
                        ? IsWordWithEnding(word, phrase[offset])
                        : string.Equals(word, phrase[offset], StringComparison.OrdinalIgnoreCase);
                }
                if (matches)
                    return string.Concat(phrase);
            }
        }
        return null;
    }

    /// <summary>Whether <paramref name="word"/> is <paramref name="hardware"/>, ignoring case, with one of <see cref="Endings"/>.</summary>
    private static bool IsWordWithEnding(string word, string hardware)
    {
        foreach (string ending in Endings)
        {
            if (word.Length == hardware.Length + ending.Length &&
                word.StartsWith(hardware, StringComparison.OrdinalIgnoreCase) &&
                word.EndsWith(ending, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
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
