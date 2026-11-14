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
/// and routine labels written without an underscore: a compound name in a comment that names nothing in Core's code. #1632
/// adds the carry flag, bits by number, record variables and extra words, routines, spritemaps, headers, the port's
/// translation and lower-case memory symbols, then physical slots, memset and scratch values, and the port's host set
/// against the original (host state, enums, integers, flags, timers and the like, host-side and host-only, this host).
/// Core's documentation states game behaviour in game
/// terms; provenance belongs to the import pipeline or a check's native catalog. Only a
/// <c>[CompatibilityService]</c> type, which reproduces hardware behaviour, or an <c>[EncodingOwner]</c> codec, which owns
/// an encoding, may describe the hardware. One diagnostic per comment or literal; there is no allowance list.
/// #1624 adds the interpreter vocabulary of the original's streams: operands, instructions and numbered, Samus and
/// animation commands, then every other command of the original's streams and dispatchers and numbered functions and
/// selectors, then the original's dispatch named as a function without a number. #1632 adds the display path's hardware
/// vocabulary: the tile characters pictures were assembled
/// from, video and display memory, and per-line tables. #1726 adds the processor's arithmetic and the original's display
/// and sound records: underflows, sign tests and sign bits, tile attributes and values, per-line effect objects and
/// voices keyed on. #1743 adds the original machine's provenance, which no type may describe, hardware owners included:
/// its native words and bit patterns, the cartridge's routines, and a value written as the machine's hexadecimal fixed-point
/// (<c>0.1000</c>, <c>7.0000</c>). #1754 adds the original program's call structure and the C reimplementation, read in
/// every type too: dispatchers, callables and a routine's labels, and the upstream C rewrite; and, outside hardware
/// owners, the original's data and number encodings: decompression, colour indexes, two's complement, unsigned compares and
/// the 8.8 and 16.16 format names. #1765 adds the original's menu and table indexes and its enemy record. #1769 adds the
/// original's numbered routine and variable labels, read in every type, and, outside hardware owners, its game state,
/// load station, area and pose numbers, its sound queues named for their limits and its sprite parts' stored placement and
/// colour data.
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
    /// The program's own vocabulary in prose (#1558): its addresses, pointers, opcodes, the processor's registers (the noun;
    /// registering a handler is the game's own code), instruction lists and pre-instructions, PLMs, word widths and extension words, its assembly code (not Wrecked Ship's Assembly Line room), and numbered palettes and priorities.
    /// </summary>
    private static readonly Regex ProgramProse = new(
        @"\b(Address|Pointer|Opcode|PLM)(s|es|ed|ing)?\b|\b(?:[AXY]|index|accumulator|status|processor|CPU|hardware|display|data|bank)\s+registers?\b|\bpre-?instructions?\b|\binstruction[-\s]+lists?\b" +
        @"|\b(?:eight|sixteen|8|16)-bit\b|\bextension\s+words?\b|\baccumulator\s+mode\b" +
        @"|\bassembly\s+(?:code|language|listing|routine|source)s?\b|\bthe\s+(?:original\s+)?assembly\b(?!\s+line)" +
        @"|\bpalette\s+(?:zero|one|two|three|four|five|six|seven|[0-7])\b|\bpriority\s+(?:zero|one|two|three|[0-3])\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The interpreter vocabulary of the original's streams in prose (#1624): an operand, an instruction, a command by
    /// number (<c>command 9/10</c>, <c>command-six</c>, <c>commands 0/1/2/4</c>) and a Samus or animation command. A Core cue
    /// says what it does and carries the values it acts on, a frame carries its gameplay, and a script's change of who
    /// controls Samus is a control setup or lock. English stays legal: an arithmetic operation's operands (the
    /// subtraction's operands, the operands of a comparison), the player's instructions and instructions for the player, a
    /// command that is not numbered (a menu's commands, a data-management command) and a number that counts the noun after
    /// it (a command one frame later).
    /// </summary>
    private static readonly Regex InterpreterProse = new(
        @"(?<!\b(?:sum|difference|product|quotient|addition|subtraction|multiplication|division|comparison)'s\s+)\boperands?\b" +
        @"(?!\s+of\s+(?:the|an?|its)\s+(?:sum|difference|product|quotient|addition|subtraction|multiplication|division|comparison)\b)" +
        @"|(?<!\b(?:player's|players'|on-screen|written|follows?|followed|following|gives?|gave|given)\s+(?:the\s+|its\s+|their\s+)?)" +
        @"\binstructions?\b(?!\s+(?:to|for)\s+(?:the\s+)?(?:players?|users?|readers?)\b)(?!\s+(?:manual|booklet)s?\b)" +
        @"|\b(?:Samus|animation)[-\s]+commands?\b" +
        @"|\bcommands?[-\s]+(?:[0-9]+|zero|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|fourteen" +
        @"|fifteen|sixteen|seventeen|eighteen|nineteen|twenty|thirty)\b" +
        @"(?![-\s]+(?:frames?|updates?|ticks?|times?|steps?|pixels?|seconds?|beats?|more|fewer|less|of)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's stream and dispatch commands in prose, numbered or not (#1624): a command of any kind (the
    /// rainbow-stop command, the unpause command, music commands, a copy-one command, a command list), and a function or
    /// selector by number (controller function zero, functions one through six, selector two, <c>Selector 0..3</c>). A Core
    /// cue, sound or music request, control setup or lock, reveal shape, action or phase says what it is. English stays
    /// legal: a command of the player, the user or a menu (the player's command, a menu command, the menu's commands, a
    /// data-management or file-select command, an options page's rows and commands, a command row), the verb after a person
    /// (the player commands Samus, who commands it), and a number that counts the noun after it (the selector one layer
    /// above).
    /// </summary>
    private static readonly Regex DispatchProse = new(
        @"(?<!\b(?:player's|players'|players?|user's|users'|users?|who|menu's|menus'|menus?|data-management|file-select" +
        @"|options?|rows?\s+and)[-\s]+)\bcommands?\b(?![-\s]+rows?\b)" +
        @"|\b(?:functions?|selectors?)[-\s]+(?:[0-9]+|zero|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve" +
        @"|thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|twenty|thirty)\b" +
        @"(?![-\s]+(?:frames?|updates?|ticks?|times?|steps?|pixels?|seconds?|beats?|layers?|levels?|rows?|columns?|more|fewer" +
        @"|less|of)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's dispatch named as a function without a number (#1624): the routine an enemy, part, boss or phase
    /// runs (a door's main function, the eye's function, a body, death, growth, movement, corpse or crawl function, a
    /// tube that has no function, a handler that requires a function). A Core action, phase, movement or connection says
    /// what it is. English stays legal: a mathematical function (a function of each colour, a pure function of the
    /// velocity, math or trigonometric functions, functions in a clause about arithmetic or mathematics), C#'s own (a
    /// local, lambda or anonymous function, a function delegate) and writing one (inventing, writing, defining or
    /// declaring a function).
    /// </summary>
    private static readonly Regex FunctionProse = new(
        @"(?<!\b(?:pure|math|mathematical|trigonometric|arithmetic|local|lambda|anonymous|invent(?:s|ed|ing)?|writ(?:e|es|ing)" +
        @"|wrote|written|defin(?:e|es|ed|ing)|declar(?:e|es|ed|ing))\s+(?:(?:an?|the|new)\s+)?)" +
        @"\bfunctions?\b(?!\s+(?:of|delegates?)\b)(?![^.;:!?]*\b(?:arithmetic|mathematics|maths?)\b)",
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
    private static readonly Regex CompoundName = new(@"\b(?:[A-Z][a-z0-9]+|[a-z][a-z0-9]+)(?:[A-Z][A-Za-z0-9]*)+\b", RegexOptions.CultureInvariant);

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
    /// The program's storage and machine vocabulary #1558 left out (#1611): a record's generic variables, words and bytes
    /// named by where they are stored, palette colours and handlers by number, tile transfers and characters, tilemap cells
    /// and pages, register shadows and work memory, scanline numbers and raw bits. Each term is read spaced or hyphenated
    /// ("work memory", "work-memory heap"); the clauses about the carry and what uploads leave are read as written.
    /// </summary>
    private static readonly Regex StorageProse = new(
        @"\b[Vv]ariable[-\s][A-F]\b|\bextended[-\s]words?\b|\binit[12]\b|\bunrelated[-\s]bytes\b" +
        @"|\b(?:level|draw|controller|random-number|RNG|scratch|speed|whole|attribute|pose-definition|population|parameter|property)[-\s]+(?:words?|bytes?)\b" +
        @"|\b(?:one|two|three|four|six|eight|sixteen|[0-9]+)-byte\b|\bodd-byte\b|\b(?:high|low)[-\s](?:bit|byte|word)s?\b|\bcarry[-\s](?:flags?|bits?|set|clear|returned|out)\b|\b(?:on|with|without|by) the carry\b|\bthe carry returned\b" +
        @"|\bcolou?rs?[-\s][0-9]+(?:\s*(?:-|through|to)\s*[0-9]+)?\b|\bpalette[-\s]handlers?\b|\bhandler[-\s](?:one|two|three|four|five|six|seven|eight|nine|ten|[0-9]+)\b|\bvisor[-\s]colou?r\b" +
        @"|\btile[-\s](?:transfers?|uploaders?|uploads?)\b|\b(?:sheet|sprite)[-\s]characters?\b|\buploads? leave\b|\btransfer[-\s]lists?\b|\btile[-\s]cells?\b|\btilemap[-\s]pages?\b|\b[0-9]+-by-[0-9]+[-\s]pages?\b" +
        @"|\bregister[-\s]shadows?\b|\b(?:liquid|shadow|hardware)[-\s]registers?\b|\bhardware[-\s]timing\b|\bwork[-\s]memory\b|\bstaging[-\s]surfaces?\b|\b(?:transfer|turning-floor)[-\s]residue\b|\bscanline[-\s][0-9]+\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The processor's operations and the program's record vocabulary #1611 left out (#1632): the carry flag as an operation
    /// sets, clears, reports, returns or adds it (Draygon, Ridley, a Chozo statue, a conveyor or a platform carrying Samus,
    /// and a carry spiral, orbit, anchor or phase, are the game's), a bit by number (not "a bit"), a record's extra,
    /// extended, extension, family-extra, enemy or numbered variables and words, the program's routines, spritemaps and
    /// headers, and the port's translation of the original (a position, layer or motion translated by an offset is
    /// geometry).
    /// </summary>
    private static readonly Regex OperationProse = new(
        @"\bcarry\s+(?:is|was|stays|remains|becomes|left)\s+(?:set|clear(?:ed)?)\b" +
        @"|\b(?:sets?|setting|clears?|clearing|cleared|reports?|reported|reporting|returns?|returning|tests?|tested|testing|keeps?|kept|chains?|chained|passe[sd]|propagates?" +
        @"|adds?|added|adding|subtracts?|subtracted|subtracting|with)\s+(?:the\s+|its\s+|a\s+|that\s+)?(?:set\s+|clear\s+)?carry\b" +
        @"(?!\s+(?:spiral|orbit|anchor|phase|state|setup|release|step|poses?|the|a|an|its|it|them|their|her|him|Samus)\b)" +
        @"|(?<![-\w])(?<!\ba\s)bits?\s*(?:[0-9]+|zero|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|fourteen|fifteen)\b" +
        @"|\b(?:extra|extended|extension|family[-\s]extra|enemy|record|spare|generic|species)[-\s]+(?:enemy[-\s]+)?(?:variables?|words?)\b|\bfamily[-\s]extras?\b" +
        @"|\bvariables?[-\s]+(?:(?-i:[0-9A-F]{1,2})|zero|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|fourteen|fifteen)\b" +
        @"|\broutines?\b|\bsprite[-\s]?maps?\b|\bheaders?\b" +
        @"|\buntranslated\b|\bretranslat\w*|\btranslat(?:ed|ion|ions|es|ing|e)\s+from\b" +
        @"|\btranslated\s+(?:routines?|code|logic|behaviou?rs?|AI|functions?|programs?|ports?|versions?|sources?|bosses|boss|enem(?:y|ies)|famil(?:y|ies)|paths?|branch(?:es)?)\b" +
        @"|\b(?:direct|literal|mechanical|faithful|line-by-line|original|cartridge|program's|port's)\s+translations?\b" +
        @"|\btranslations?\s+of\s+(?:the\s+)?(?:original|routines?|code|program|assembly|game|retail|cartridge)\b" +
        @"|\b(?:not\s+yet|yet\s+to\s+be|already|faithfully|literally|directly|mechanically|been|be)\s+translated\b(?!\s+(?:to|by|into|onto|along|across|toward|towards)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The program's working memory (#1632): a physical slot, enemy slot or projectile slot (an enemy's place in the room's
    /// listing is the game's; a physical component or motion is geometry), a memset, and scratch values (starting from
    /// scratch and scratch marks are English).
    /// </summary>
    private static readonly Regex WorkingMemoryProse = new(
        @"\bphysical[-\s]+(?:(?:enemy|projectile)[-\s]+)?slots?\b" +
        @"|\bmemset\b" +
        @"|(?<!\bfrom\s+)\bscratch\b(?!\s+marks?\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The port's runtime set against the original program (#1632): "host" directly followed by the simulation's own state,
    /// arithmetic or behaviour (a host slot, exception, number, notion, enum, integer, flag, timer, RNG, record, field,
    /// latch, list, constant, sprite, animation, physics, ordering ...), a provenance compound (host-side, host-only,
    /// host-readable, host-visible, host-friendly, host-authored, host-facing), and "this host" or "this gameplay host".
    /// Core is never a host, so these describe the game as a translation. The application that runs the game stays legal:
    /// the host as an actor (the host binds, persists or plays), possessive (the host's save slots), plural (the hosts'
    /// import format), its presentation and services (host frames, PCM, pixels, artwork, options, content, elapsed time,
    /// diagnostics, renderer, calls) and its ownership compounds (host-owned, host-installed, host-bound, host-supplied,
    /// host-configured) are not read, nor is a ghost.
    /// </summary>
    private static readonly Regex HostProvenanceProse = new(
        @"\bhost[-\s]+(?:side|only|readable|visible|friendly|authored|facing)\b" +
        @"|\bhost\s+(?:slots?|exceptions?|numbers?|notions?|enums?|integers?|flags?|timers?|RNG|random|records?|fields?" +
        @"|latch(?:es)?|guards?|masks?|names|equivalents?|storage|states?|lists?|locks?|projections?|tables?|constants?" +
        @"|sprites?|animations?|readings?|physics|ordering|results?|no-ops?|errors?|loops?|vectors?|poses?|phases?" +
        @"|interactions?|signed|movement|gravity|jumps?|knockback|palettes?|handlers?|death|schedulers?|allocations?|bombs?" +
        @"|areas?|aiming|smoothing|floating-point)\b" +
        @"|\bthis\s+(?:gameplay\s+)?host\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The display's memory and its line tables in prose (#1632): video, display or graphics memory, and a per-line or
    /// scanline table. Core publishes pictures, layers and sampled waves; it owns no display memory and fills no table
    /// the display reads line by line. A per-line effect, scroll or window is the effect itself and stays legal.
    /// </summary>
    private static readonly Regex DisplayMemoryProse = new(
        @"\b(?:video|display|graphics)[-\s]+memor(?:y|ies)\b" +
        @"|\bper[-\s]+(?:scan)?line\s+(?:[\w-]+\s+){0,2}?tables?\b|\bscanline\s+tables?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The processor's arithmetic and the original's display and sound records in prose (#1726): an underflow in any word
    /// form; a sign test, sign bit, sign check or sign flag (spaced, hyphenated or signed: a signed test); testing a value's
    /// sign (tests its sign, testing the sign); a comparison, cap, clamp or branch made by or on a value's sign within its
    /// clause (compares it by the sign of the wrapped difference, branches on its sign); a tile's attributes or values; a
    /// per-line effect object, with up to two words between (a per-line display effect object); and a voice, note or
    /// channel keyed on (a key-on, the tick keys on the voice, a voice keyed on). A Core countdown passes zero, a
    /// comparison says which side of the other a value lies, a picture is named, and a liquid's or effect's motion names
    /// its steps. English stays legal: a value's sign as its direction (the sign of the speed, the sign of the difference,
    /// a sign-inverted mirror, the percent sign) when nothing tests or compares by it; a key on disk; and keying a table
    /// or a binding on a name (installed bindings key on the identity, a table keyed on the name).
    /// </summary>
    private static readonly Regex MachineArithmeticProse = new(
        @"\bunderflow\w*" +
        @"|\bsign(?:ed)?[-\s]+(?:tests?|testing|tested|bits?|checks?|flags?)\b" +
        @"|\btest(?:s|ed|ing)?\s+(?:only\s+)?(?:the|its|their|that)\s+(?:[\w-]+(?:'s)?\s+){0,2}?" + SignWord +
        @"|\b(?:compar(?:e|es|ed|ing)|caps?|capped|capping|clamps?|clamped|clamping|branch(?:es|ed|ing)?|judged)\b[^.;:!?]{0,60}?" +
        @"\b(?:by|on)\s+(?:the|its|their)\s+(?:[\w-]+(?:'s)?\s+){0,2}?" + SignWord +
        @"|\btiles?(?:'s|s')?[-\s]+(?:attributes?|values?)\b" +
        @"|\bper[-\s]+line\s+(?:[\w-]+\s+){0,2}?objects?\b" +
        @"|\bkey[-]?ons?\b" +
        @"|\bkey(?:s|ed|ing)?\s+on\s+(?:the\s+|a\s+|its\s+|each\s+|every\s+|this\s+)?(?:voices?|notes?|channels?|samples?|instruments?)\b" +
        @"|\b(?:voices?|notes?|channels?)\s+(?:(?:is|are|was|were|stays?|stayed|remains?|remained)\s+)?keyed\s+on\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original machine's provenance in prose (#1743), read in every Core type: a native word or bit pattern (spaced or
    /// hyphenated, with up to two words between: a signed native word), a cartridge routine or the cartridge's routines, and a
    /// value written as the machine's hexadecimal fixed-point, a whole part starting with a digit and a four-digit fraction
    /// that ends in zero or holds a hexadecimal letter (<c>0.1000</c>, <c>7.0000</c>, <c>1.8C00</c>). A compatibility service
    /// says which behaviour it reproduces and an encoding owner describes its encoding, but neither cites the machine's words,
    /// its routines or its spelling of a value. English stays legal: a native pose number or any other native thing that is
    /// not a word or bit pattern, a routine that is not the cartridge's, and a decimal with no trailing zero (<c>0.0625</c>,
    /// <c>0.1875</c>, <c>0.125</c>). A format's name (<c>16.16</c>, <c>8.8</c>) is not a value; <see cref="MachineNumberProse"/>
    /// reads it outside hardware owners.
    /// </summary>
    private static readonly Regex MachineProvenanceProse = new(
        @"\bnative[-\s]+(?:[\w-]+\s+){0,2}?(?:words?|bit[-\s]+patterns?)\b" +
        @"|\bcartridge(?:'s)?[-\s]+routines?\b" +
        @"|(?<!\w|\w\.)\$?[0-9][0-9A-Fa-f]{0,3}\.(?:[0-9A-Fa-f]{3}0|(?=[0-9]{0,3}[A-Fa-f])[0-9A-Fa-f]{4})(?!\w|\.\w)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original program's call structure in prose (#1754), read in every Core type: a dispatcher, in any number and as
    /// a dispatcher index (the movement dispatcher, the outer dispatcher, dispatcher index one), a callable (a special
    /// callable), and a routine's label, the place its code merged, branched or was entered (a merge, inverted, reversed,
    /// source, branch, jump, entry, routine, code, program, assembly, local, loop or return label). Core names the phase,
    /// step, check, response or game state that runs. A compatibility service says which behaviour it reproduces and an
    /// encoding owner describes its encoding; neither is a routine, so neither describes the original's dispatch through
    /// one. English stays legal: dispatching a request, a sound or an event (the verb), a callback, and every label the
    /// game shows, and the verb (the ENERGY label, the Exit label, the map's area labels, zero labels for reserves, the
    /// select map labels the area).
    /// </summary>
    private static readonly Regex CallStructureProse = new(
        @"\bdispatchers?\b|\bcallables?\b" +
        @"|\b(?:merge|inverted|reversed|source|branch|jump|entry|routine|code|program|assembly|local|loop|return)[-\s]+labels?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The C reimplementation of the original in prose (#1754), read in every Core type and case-sensitively for the
    /// language's letter: the upstream C rewrite and its bugfixes (upstream C, upstream C's), and the C port, rewrite,
    /// reimplementation, version, code, source or decompilation. Core is the game; another program's reading of the
    /// cartridge is not its provenance. C# is not C, and a C that is not that program (a grade, a button, slot C) is not read.
    /// </summary>
    private static readonly Regex ReimplementationProse = new(
        @"\b[Uu]pstream\s+C\b(?!#)" +
        @"|\b(?:[Tt]he|[Aa])\s+C\s+(?:re-?implementations?|rewrites?|ports?|versions?|code|sources?|decompilations?)\b",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's data encodings in prose (#1754), outside hardware owners: decompression in any word form (the
    /// decompressed foreground, the artwork decompression), and a colour index, spaced or hyphenated (the statue's grey colour
    /// index, colour indices). Core loads installed pictures, levels and owned colours; what the cartridge compressed and
    /// which palette entry held a colour are the import's. An encoding owner describes its encoding and a compatibility
    /// service the hardware behaviour it reproduces. English stays legal: an uncompressed file, a colour, an index.
    /// </summary>
    private static readonly Regex DataEncodingProse = new(
        @"\bdecompress\w*|\bcolou?r[-\s]+ind(?:ex|exes|exed|ices)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The machine's number formats in prose (#1754), outside hardware owners: two's complement, spaced or hyphenated
    /// (the exact two's complement, two's-complement form), an unsigned compare or comparison, and the 8.8 and 16.16
    /// fixed-point format names (an 8.8 velocity, a 16.16 position, promoted to 16.16). Core states a value in the game's
    /// units: a speed in 1/256 pixels, a position in 1/65536 pixels, a value negated, a comparison by which side lies
    /// higher. An encoding owner such as <c>EightEight</c> owns its format and names it, and a compatibility service
    /// describes the arithmetic it reproduces. English stays legal: a decimal (<c>0.125</c>, <c>8.25</c>), a version
    /// (<c>0.8.8</c>), an unsigned value, a complement that is not two's.
    /// </summary>
    private static readonly Regex MachineNumberProse = new(
        @"\btwo(?:'s|s|’s)?[-\s]+complement\b" +
        @"|\bunsigned[-\s]+compar(?:e|es|ed|ing|isons?)\b" +
        @"|(?<![\w.])(?:8\.8|16\.16)(?!\w|\.\w)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's table positions and enemy record in prose (#1765), outside hardware owners, spaced or hyphenated: a
    /// menu index (menu index 13, menu indices zero through two), a table index, alone or naming its table (a facing-table or
    /// speed-table index, ordinary table indices, two table indexes), and an enemy record (the live enemy record, four
    /// enemy records). The original numbered its menus' steps, looked values up by their place in its tables and kept each
    /// enemy in a fixed-size record; Core names the menu's step, an angle's whole step, a speed curve's place or the enemy
    /// itself. A compatibility service may describe the enemy record its reproduced behaviour reads. English stays legal:
    /// a table indexed by a value (the verb), the menu's index, an enemy's record, and a menu, table, index, enemy or record
    /// alone.
    /// </summary>
    private static readonly Regex TablePositionProse = new(
        @"\b(?:menu|table)[-\s]+ind(?:ex|exes|ices)\b|\benemy[-\s]+records?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A numbered routine or variable label of the original program in prose (#1769), read in every Core type and in any
    /// case: <c>Func126</c>, <c>Func139–142</c>, <c>var4</c>. Core names the step, phase or countdown that runs; a
    /// compatibility service or encoding owner names the behaviour or encoding, never the original's label for it. English
    /// stays legal: C#'s <c>Func</c> delegate and a <c>var</c> declaration, which carry no number.
    /// </summary>
    private static readonly Regex NumberedLabelProse = new(@"\b(?:Func|var)[0-9]+\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's numbered game states, load stations and areas in prose (#1769), outside hardware owners, spaced or
    /// hyphenated, in digits or words: a game state by number (game state eight, game-state-17), a load station by number
    /// (station 18, load-station zero, station-eighteen) and an area by number (area six, area-zero, area 2). The original
    /// numbered its game states, each area's load stations and its areas; Core names the game state
    /// (<c>SuperMetroidGameState</c>), the load station (the gunship, the Zebes arrival) and the area (Crateria, Ceres). A menu
    /// index is <see cref="TablePositionProse"/>'s. English stays legal: a number that counts the noun after it (a station
    /// two rooms away, an area two tiles wide), an area number past the game's eight areas (the area 450 to the right), and a
    /// game state, station or area, or a room's or the beam's state by number.
    /// </summary>
    private static readonly Regex GameNumberProse = new(
        @"\bgame[-\s]+states?[-\s]+" + NumberWord + @"\b" +
        @"|\b(?:load[-\s]+)?stations?[-\s]+" + NumberWord + @"\b" + CountedNoun +
        @"|\bareas?[-\s]+(?:zero|one|two|three|four|five|six|seven|[0-7])\b" + CountedNoun,
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A pose by the original's number in prose (#1769), outside hardware owners: a hexadecimal pose number, holding a
    /// capital hexadecimal letter or a leading zero (pose A/B, poses 0A, pose number 02), a pose number written as a word
    /// (pose two, Pose zero's transitions), and a transition between two such numbers (<c>A5 -> 02</c>). Core names the pose
    /// (<c>FacingLeftNormalPose</c>, her standing pose); an encoding owner of pose numbers describes them. English stays
    /// legal: a decimal ordinal of a family's own animation frames in digits (Pose 3 of the wave trail, poses 4 and 5 lower
    /// the arm), a number that counts the noun after it (the pose two frames later), and a pose alone.
    /// </summary>
    private static readonly Regex PoseNumberProse = new(
        @"\bposes?[-\s]+(?:numbers?[-\s]+)?(?-i:(?=[0-9]?[A-F])[0-9A-F]{1,2}|0[0-9])\b" +
        @"|\bposes?[-\s]+(?:numbers?[-\s]+)?" + NumberWordsOnly + @"\b" + CountedNoun +
        @"|(?-i:\b(?:[A-F][0-9A-F]|[0-9][A-F]|0[0-9])\s*(?:->|→)\s*(?:[A-F][0-9A-F]|[0-9][A-F]|0[0-9])\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A sound queue named for the original's per-call limit in prose (#1769), outside hardware owners: a "max-N" queue or
    /// entry point (the max-fifteen sound queue), a limit of queued sounds (maximum six queued, at most six queued) and an
    /// N-entry or N-request limit (a six-entry admission limit, the six-request limit, a six-entry queue limit). Core states
    /// the request's <c>SoundRequestPriority</c> (normal, essential); the compatibility service that owns those priorities
    /// describes the limits they reproduce. English stays legal: a maximum or a limit that names no queue position (the
    /// maximum speed, at most three steps), and a queue alone.
    /// </summary>
    private static readonly Regex QueueLimitProse = new(
        @"\bmax(?:imum)?-" + NumberWord + @"\b" +
        @"|\b(?:max(?:imum)?|at\s+most)\s+" + NumberWord + @"\s+queued\b" +
        @"|\b" + NumberWord + @"[-\s]+(?:requests?|entry|entries|sounds?)[-\s]+(?:(?:queue|admission)[-\s]+)?limits?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A sprite part's placement and colour data in prose (#1769), outside hardware owners: a palette run on to its stored
    /// attribute value (<c>palette0800</c>), a hexadecimal value written with an <c>h</c> suffix (<c>X-10h</c>,
    /// <c>Y-3Bh</c>), a part placed by its stored X or Y (one part, at X=508; Part j uses X=0) and the nine-bit X the
    /// original stored a part's horizontal place in (masks X to nine bits, nine-bit X wrap). Core shows whole pictures at
    /// places in pixels, in named colours. English stays legal: a position by X or Y that no part is placed by (the goop path
    /// at X=-80, the ship parts slowing below Y=768), a palette or a part alone, and a word ending in h.
    /// </summary>
    private static readonly Regex SpritePartProse = new(
        @"(?-i:\bpalette[0-9][0-9A-F]{1,3}\b)" +
        @"|(?-i:\b[0-9][0-9A-F]*h\b)" +
        @"|\bpart\b(?:[^.;!?]|\.\.+){0,80}?\b[XY]\s*=\s*-?[0-9]" +
        @"|\b(?:nine|9)[-\s]+bits?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>The numbers a game state, station or queue is cited by: digits, or a word from zero through ninety.</summary>
    private const string NumberWord = @"(?:[0-9]+|" + NumberWordsOnly + ")";

    /// <summary>The numbers zero through twenty and the tens through ninety, as words.</summary>
    private const string NumberWordsOnly =
        @"(?:zero|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|fourteen|fifteen|sixteen|seventeen" +
        @"|eighteen|nineteen|twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety)";

    /// <summary>A number that counts the noun after it (two frames, six tiles, one or more), not one that names a thing.</summary>
    private const string CountedNoun =
        @"(?![-\s]+(?:frames?|updates?|ticks?|times?|steps?|pixels?|seconds?|beats?|calls?|tiles?|blocks?|rooms?|screens?" +
        @"|units?|more|fewer|less|of|or\s+more|percent)\b)";

    /// <summary>A value's sign as a word, not a percent, plus, minus, equals or dollar sign.</summary>
    private const string SignWord = @"(?<!\b(?:percent|plus|minus|equals?|dollar)[-\s]+)sign\b";

    /// <summary>The words of text in prose: what is typed, written or spelled, and the text, message, letter or glyph it is.</summary>
    private const string TextWords =
        @"(?:typ(?:es|ed|ing)|typewriters?|texts?|narrations?|messages?|captions?|letters?|glyphs?|alphabets?" +
        @"|spell(?:s|ed|ing)?|strings?|chars?|fonts?)";

    /// <summary>
    /// The hardware's tile characters in prose (#1632): the eight-pixel squares the original assembled its pictures from,
    /// loaded, copied or shown as characters. Core shows whole pictures; what a picture was composed from is the import's.
    /// A character by number always reports (<c>character zero</c>). Any other character is read as the hardware's unless
    /// it is text or a person: text names itself in the same clause (up to a full stop, semicolon or colon) by a word of
    /// text (typed, typewriter, text, narration, message, caption, letter, glyph, alphabet, spelled, string, font), by the
    /// typing it measures (a character at a time, a character's delay, cadence, cursor, call, records, placements,
    /// processing, work, glow or count, per-character, the next character), or by its type, one named for text (see
    /// <see cref="TextTypeName"/>); a person is a game's, story's, playable, player or non-player character, or a
    /// character who acts. Markup is not prose, so a parameter named <c>character</c> is not read.
    /// </summary>
    private static readonly Regex TileCharacter = new(
        @"\bcharacters?[-\s]+(?:zero|number\s+\w+|[0-9]+)\b" +
        @"|(?<!\b(?:per|next)[-\s])(?<!\b(?:game's|games'|story's|playable|player|non-player)\s+)" +
        @"(?<!\b" + TextWords + @"\b[^.;:!?]*)\bcharacters?\b" +
        @"(?!(?:'s?)?[-\s]+(?:at\s+a\s+time|delays?|cadences?|cursors?|calls?|records?|placements?|processing|work|glows?|counts?)\b)" +
        @"(?![^.;:!?]*\b(?:" + TextWords + @"|who|whom)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A type named for text, whose characters are the text's (#1632): a typewriter, text, narration, caption, glyph,
    /// letter, font or message, read as a whole camel-case word (<c>EscapeTypewriterState</c>, <c>EndingTextCharacter</c>;
    /// not <c>Texture</c>).
    /// </summary>
    private static readonly Regex TextTypeName = new(
        @"(?:Typewriter|Text|Narration|Caption|Glyph|Letter|Font|Message)s?(?![a-z])", RegexOptions.CultureInvariant);

    /// <summary>
    /// A memory symbol of the original program in a comment: a lower-case name joined by underscores (<c>toriz_var_00</c>,
    /// <c>palette_change_num</c>) (#1632). The installed artwork's frame names are the snake case of Core's own frame
    /// identities (<c>draygon_goop_first</c> is <c>DraygonGoopFirst</c>), so a name whose Pascal-case form Core's code spells
    /// is a presentation key, not a symbol.
    /// </summary>
    private static readonly Regex MemorySymbol = new(@"\b[a-z][a-z0-9]*(?:_[a-z0-9]+)+\b", RegexOptions.CultureInvariant);

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

    /// <summary>The analyzers' same-line allowance markers with their category: enforcement syntax, not prose, so their words are not read.</summary>
    private static readonly Regex AllowanceMarker = new(@"(cartridge-encoding|magic-number-audit): allow\([^)]*\)", RegexOptions.CultureInvariant);

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
            start.RegisterSyntaxTreeAction(tree => AnalyzeTree(tree, tree.IsGeneratedCode ? null : codeNames));
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
    /// <remarks>A generated file's comments are the generator's, so they are not read for routine labels.</remarks>
    private static void AnalyzeTree(SyntaxTreeAnalysisContext context, Lazy<ImmutableHashSet<string>>? codeNames)
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
    /// Reports <paramref name="text"/> once when it cites the machine's provenance (<see cref="MachineProvenanceProse"/>), the
    /// original's call structure (<see cref="CallStructureProse"/>) or the C reimplementation
    /// (<see cref="ReimplementationProse"/>), in any type, or names the hardware outside a hardware-owning type. A comment carries
    /// <paramref name="codeNames"/>, against which its compound names are read as routine labels; a string literal does not,
    /// and is not read for labels or hexadecimal literals.
    /// </summary>
    private static void Report(SyntaxTreeAnalysisContext context, string text, Location location, SyntaxNode? owner,
        Lazy<ImmutableHashSet<string>>? codeNames)
    {
        text = AllowanceMarker.Replace(text, string.Empty);
        string? word = MachineProvenanceProse.Match(text) is { Success: true } provenance ? provenance.Value
            : CallStructureProse.Match(text) is { Success: true } call ? call.Value
            : ReimplementationProse.Match(text) is { Success: true } reimplementation ? reimplementation.Value
            : NumberedLabelProse.Match(text) is { Success: true } numberedLabel ? numberedLabel.Value
            : IsInHardwareOwner(owner) ? null
            : Address.Match(text) is { Success: true } address ? address.Value
            : BankedValue.Match(text) is { Success: true } banked ? banked.Value
            : HardwareProse.Match(text) is { Success: true } prose ? prose.Value
            : HardwareTerms.Match(text) is { Success: true } term ? term.Value
            : ProgramProse.Match(text) is { Success: true } program ? program.Value
            : InterpreterProse.Match(text) is { Success: true } interpreter ? interpreter.Value
            : DispatchProse.Match(text) is { Success: true } dispatch ? dispatch.Value
            : FunctionProse.Match(text) is { Success: true } function ? function.Value
            : CpuProse.Match(text) is { Success: true } cpu ? cpu.Value
            : StorageProse.Match(text) is { Success: true } storage ? storage.Value
            : OperationProse.Match(text) is { Success: true } operation ? operation.Value
            : WorkingMemoryProse.Match(text) is { Success: true } memory ? memory.Value
            : HostProvenanceProse.Match(text) is { Success: true } host ? host.Value
            : DisplayMemoryProse.Match(text) is { Success: true } display ? display.Value
            : MachineArithmeticProse.Match(text) is { Success: true } arithmetic ? arithmetic.Value
            : DataEncodingProse.Match(text) is { Success: true } encoding ? encoding.Value
            : MachineNumberProse.Match(text) is { Success: true } number ? number.Value
            : TablePositionProse.Match(text) is { Success: true } tablePosition ? tablePosition.Value
            : GameNumberProse.Match(text) is { Success: true } gameNumber ? gameNumber.Value
            : PoseNumberProse.Match(text) is { Success: true } poseNumber ? poseNumber.Value
            : QueueLimitProse.Match(text) is { Success: true } queueLimit ? queueLimit.Value
            : SpritePartProse.Match(text) is { Success: true } spritePart ? spritePart.Value
            : TileCharacterFinding(text, owner) is { } character ? character
            : codeNames is not null ? CommentOnlyFinding(text, codeNames)
            : null;
        if (word is null)
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, word));
    }

    /// <summary>
    /// The hardware's tile character <paramref name="text"/>'s prose names, outside its markup; null when it names none or
    /// lies in a type named for text, whose characters are the text's.
    /// </summary>
    private static string? TileCharacterFinding(string text, SyntaxNode? owner) =>
        TileCharacter.Match(Markup.Replace(text, " ")) is { Success: true } character && !IsInTextType(owner)
            ? character.Value
            : null;

    /// <summary>Whether a node lies in a type named for text (<see cref="TextTypeName"/>).</summary>
    private static bool IsInTextType(SyntaxNode? node)
    {
        for (SyntaxNode? current = node; current is not null; current = current.Parent)
        {
            if (current is BaseTypeDeclarationSyntax declaration && TextTypeName.IsMatch(declaration.Identifier.ValueText))
                return true;
        }
        return false;
    }

    /// <summary>
    /// A comment's routine label, underscore-joined or compound, its memory symbol, or its hexadecimal literal; null when it
    /// has none.
    /// </summary>
    private static string? CommentOnlyFinding(string text, Lazy<ImmutableHashSet<string>> codeNames)
    {
        if (RoutineLabel.Match(text) is { Success: true } label)
            return label.Value;
        if (HexLiteral.Match(text) is { Success: true } literal)
            return literal.Value;
        string prose = Markup.Replace(text, " ");
        foreach (Match symbol in MemorySymbol.Matches(prose))
        {
            if (!codeNames.Value.Contains(PascalCase(symbol.Value)))
                return symbol.Value;
        }
        foreach (Match compound in CompoundName.Matches(prose))
        {
            if (!codeNames.Value.Contains(compound.Value))
                return compound.Value;
        }
        return null;
    }

    /// <summary>The Pascal-case form of a lower-case underscore-joined name: each part capitalized, the underscores dropped.</summary>
    private static string PascalCase(string snakeCase) =>
        string.Concat(snakeCase.Split('_').Select(part => part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part.Substring(1)));

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
