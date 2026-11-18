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
/// colour data. #1794 adds the original's routine slots for Samus, the timer/hack handler and gamma, read in every type,
/// and, outside hardware owners, a compound name a comment sets as code instead of citing it. #1797 adds, outside
/// hardware owners, the machine's display transfers, staging and NTSC and PAL video standards, the common room elements,
/// and the original's selector tables, record tables and input and pose records. #1812 adds, outside hardware owners, the
/// original's indirect dispatch, the machine's bus, regionalized values and the original's enemy entries named as records.
/// #1828 adds the original's routine handlers, read in every type: a handler call, and a music, sound, colour-effect,
/// timer, sprite or state handler. #1853 adds, outside hardware owners, the original's trigonometry tables: an angle
/// counted in table units, angles or steps, and a sine, cosine or trigonometry table. #1855 widens the routine handlers,
/// read in every type, to a handler named by the word before it, a handler pair and a dispatch point. #1856 adds, outside
/// hardware owners, the original's numbering of Samus's movement types, modes, collision results and statuses. #1892
/// adds, outside hardware owners, the palette in any word form. #1893 adds, outside hardware owners, the original's
/// animation interpreter's sleep, stream and bytecode. #1971, #1998 and #2018 add the review trail, read in every type:
/// review and investigation citations, proofs, pinned reference data, release pins and commit hashes; and, outside
/// hardware owners, the original's table, entry, slot, parameter and program numbering, its callers and calls, its
/// dispatch, the sound chip's voices and key-off, and words run on to a number. #2080 adds, outside hardware owners, the
/// original's parameter tables, tail calls, short multiplies, extension arrays, rotting tables and arithmetic modulo 256,
/// and #2089 its fixed colour, main-screen designation and per-line effects, a type by number and, in every type, a
/// review cited under its ticket. #2105 adds to the review trail, in every type, the review inventory and fix history,
/// and, outside hardware owners, a check of the repository cited by its kind or its name.
/// #2113 adds, outside hardware owners, a controller button's letter used as the name of a gameplay action.
/// #2133 adds to the review trail, in every type, relocation history: a ticket followed by what a thing was, a
/// parenthetical telling what it was, and where a thing moved from.
/// #2123 adds to the review trail, in every type, anything verified, and, outside hardware owners, the host's
/// desktop, the original's assembly loops, branches and labels, and a value stored as a number for a meaning or cited
/// as a literal number.
/// #2139 adds to the review trail, in every type, reviewed or approved wording and a port's history told against its
/// present ("X used to ... now", "used to be"); and, outside hardware owners, the port's narration of itself (managed
/// code), the original's steps and programs, its backing up and restoring of graphics, and a story event named by a
/// number or as a bit.
/// #2152 adds, outside hardware owners, a slot as an enemy's, a projectile's or a bomb's place, a frame named as a map,
/// a subtraction's borrow, the repository's verification, verifiers, fixtures and movies, and the original's tables,
/// indexes and bugs.
/// #2172 adds, outside hardware owners, the machine's 16.16 arithmetic named by its sixty-five-thousandths (a
/// 1/65536-pixel addition, helper or form), paired additions, and running or calling a helper or controller.
/// #2192 adds to the review trail, in every type, fix history told by an adverb of error, a fall-through, an earlier
/// order or what an alternative would strand, and debug narration: a diagnostic's path, debug and non-debug, and the
/// port's C# construction; and, outside hardware owners, the Japanese and North American release named with no
/// difference stated, a speed row, a cited variable label, a sort order, a scroll table, colours loaded into a row, a
/// state extension or extra values, and a literal write.
/// #2222 adds, outside hardware owners, a background object, a palette's ranges, a colour write or a fade's component
/// steps, a script's record, the dropped drawing order (once interleaved, a presentation quirk), an opcode mnemonic in
/// markup, a register reloaded, a field of the C reimplementation, a packed value, a disassembly label, an encounter's
/// numbered event, stage, form, state, place or region, and the retail population or initializer.
/// #2248 and #2265 narrow debug narration to the repository's tooling (the original game's debug mode is the game's), add
/// to the review trail, in every type, fix history told inside a clause, and add, outside hardware owners, the
/// original's enemies told as physical ones, its routines as walkers and helpers, bit masks, negative branches and
/// unsigned values, its program flow (installing steps, jumps into animations, fall-throughs), data told as
/// engine-owned, default rows, colour zero and numbered parameters, literal writes with words between and a pace per
/// any call.
/// #2271 adds, outside hardware owners, the machine's storage told for a value: a value shifted by a number of places,
/// state cleared as the room loads, a table's physical order, both halves of one random number, a value kept in another
/// quantity, and a vector or fixed-point helper.
/// #2292 adds, outside hardware owners, the original's routines told as wrappers, modified helpers and controllers, a
/// merge path, equipment and state bits, a stage by number outside the Speed Booster's, updates counted as calls in prose
/// and in declared names, a value compared or read as signed, a deferred write or a suit's colour copy, odd frames that
/// return, hardcoded data, an offset written with a plus sign, the retail game, a cursor through the original's lists, a
/// count of bits, position and policy rows and records, and arithmetic written as a summary.
/// #2345 adds, outside hardware owners, a value's arithmetic told as a division, remainder or modulo by a number, the
/// original's program structure (if/else chains, a leading call, a call not made, a no-op, a routine's prologue) and
/// storage told as left over or never read.
/// #2352 adds, outside hardware owners, a position's overflow, a range named by its 1/N-pixel storage, scan, target,
/// collision and solid counters, state lying below a clear, allocations that clear and routes that allocate, a check that
/// runs to confirm a value, and the original's code branches told in prose.
/// #2362 adds, outside hardware owners, the frontend's, effects', HUD's, escape's, progression's, input's, saves' and
/// animation's forms of the original's calls, storage and program: updates counted as calls (calls elapsed or made,
/// later or remaining calls, a thing's call, a call that completes something, calls of the same update or owner, a call
/// that saves where it resumes), screen values and counters told by their storage width and wrap, the original's object
/// handling, places and actor order, initialization parameters, a named storage bit, the save memory's order, a
/// projectile-backed thing, a coroutine, a chosen handler, a backward scan, a checksum, an
/// emulator, what the original reads and engine definitions.
/// #2372 adds, outside hardware owners, enemy prose's wrapped, wraparound or modular distances and differences, a
/// coordinate's wraparound and a move wrapping within the coordinate range, installing or loading art, the game storing,
/// checking or subtracting a value, completion and draining calls, table multipliers and a value of 0 through 255.
/// #2377 adds, outside hardware owners, an enemy's presence, contact or processing told as a stored property, a post- or
/// pre-decrement, a main-AI call or AI invocation, running the code's loop, the handler's guard, a signed wrapped
/// comparison and a strict modular range.
/// #2386 adds, outside hardware owners, an allocator, a renderer, a streamer or streamed view, a saved state, a matcher,
/// a translated thing and progression, defeat, controller, button or input bits.
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
    /// #2123 adds the assembly's loops, branches, labels, versions and implementation and an ascending or descending
    /// assembly; assembling a value from its parts stays English.
    /// </summary>
    private static readonly Regex ProgramProse = new(
        @"\b(Address|Pointer|Opcode|PLM)(s|es|ed|ing)?\b|\b(?:[AXY]|index|accumulator|status|processor|CPU|hardware|display|data|bank)\s+registers?\b|\bpre-?instructions?\b|\binstruction[-\s]+lists?\b" +
        @"|\b(?:eight|sixteen|8|16)-bit\b|\bextension\s+words?\b|\baccumulator\s+mode\b" +
        @"|\bassembly\s+(?:code|language|listing|routine|source|loop|branch|label|version|implementation)(?:s|es)?\b" +
        @"|\bthe\s+(?:original\s+)?assembly\b(?!\s+line)|\b(?:ascending|descending)\s+assembly\b" +
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
    /// host-configured) are not read, nor is a ghost. #2123 adds the desktop in any form (the desktop runtime, a
    /// desktop-held button, desktop composition): Core runs the same game whatever application runs it.
    /// </summary>
    private static readonly Regex HostProvenanceProse = new(
        @"\bhost[-\s]+(?:side|only|readable|visible|friendly|authored|facing)\b" +
        @"|\bhost\s+(?:slots?|exceptions?|numbers?|notions?|enums?|integers?|flags?|timers?|RNG|random|records?|fields?" +
        @"|latch(?:es)?|guards?|masks?|names|equivalents?|storage|states?|lists?|locks?|projections?|tables?|constants?" +
        @"|sprites?|animations?|readings?|physics|ordering|results?|no-ops?|errors?|loops?|vectors?|poses?|phases?" +
        @"|interactions?|signed|movement|gravity|jumps?|knockback|palettes?|handlers?|death|schedulers?|allocations?|bombs?" +
        @"|areas?|aiming|smoothing|floating-point)\b" +
        @"|\bthis\s+(?:gameplay\s+)?host\b|\bdesktops?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The display's memory and its line tables in prose (#1632): video, display or graphics memory, and a per-line or
    /// scanline table. Core publishes pictures, layers and sampled waves; it owns no display memory and fills no table
    /// the display reads line by line. A per-line scroll or window is the effect itself and stays legal; a per-line effect
    /// is <see cref="DisplayEffectProse"/>'s.
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
    /// The original program's routine slots for Samus in prose (#1794), read in every Core type: the timer/hack handler
    /// the original called after her movement (a hack handler, hack-handler, hack handling or hack owner, the timer hack, a
    /// slot written after a slash: movement/hack handlers, owner/hack handling) and gamma, the name a reading of the
    /// original gave that slot's routines (grab gamma, the gamma initialization). Core names the ability that holds her
    /// (<c>SamusMovementOwner</c>) and the check that runs after her movement (Draygon's escape, the drained get-up). English
    /// stays legal: a hack that is not that slot (a neutral-jump hack, a visual hack) and a colour's gamma correction,
    /// curve, ramp, space or value.
    /// </summary>
    private static readonly Regex RoutineSlotProse = new(
        @"/\s*hack\b|\btimer[-\s]+hacks?\b|\bhack[-\s]+(?:handl(?:ers?|ing)|owners?)\b" +
        @"|\bgamma\b(?![-\s]+(?:correct\w*|curves?|ramps?|space|encod\w*|decod\w*|values?)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original program's routine handlers in prose (#1828, #1855), read in every Core type, spaced or hyphenated: a
    /// handler call (the extra sound-handler call, two handler calls), a handler named by the word before it (the music
    /// handler's fetch, the colour effect handler, Samus's timer handler, the movement handler, the room-object handler,
    /// the unable-to-stand handler, AI handlers, a sub-handler), a handler pair and a dispatch point (the ordinary locked
    /// handler pair, the palette update's dispatch point). The original ran Samus's input and movement, each enemy's AI and
    /// the room objects through routines it installed in slots and called; Core names the movement owner, the input mode,
    /// the phase, the check or the queue's pass that runs (<c>SamusMovementOwner</c>, <c>SamusInputMode</c>,
    /// <c>MusicQueuePasses</c>, <c>ColourEffectsRun</c>). English stays legal: a handler alone or after an article, a
    /// determiner, a pronoun, a number or a possessive (a handler, each handler, its handlers, the grapple beam's handler),
    /// an exception or event handler, calling a handler, a call, a point and a dispatch alone.
    /// </summary>
    private static readonly Regex RoutineHandlerProse = new(
        @"\bhandler[-\s]+(?:calls?|pairs?)\b|\bdispatch[-\s]+points?\b" +
        @"|\b(?:music|sound(?:[-\s]+effects?)?|colou?r[-\s]+effects?|timer|sprite|state)[-\s]+handlers?\b" +
        @"|(?<![\w'’-])[A-Za-z]\w*(?:-[A-Za-z]\w*)*[-\s]+handlers?\b" +
        @"(?<!(?<![\w'’-])(?:" + HandlerDeterminers + "|" + NumberWordsOnly + @")[-\s]+handlers?)" +
        @"(?<!\b(?:exception|event)[-\s]+handlers?)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The words before a handler that do not name it (#1855): articles, determiners, pronouns, quantifiers, conjunctions,
    /// prepositions and the verbs of being.
    /// </summary>
    private const string HandlerDeterminers =
        @"a|an|the|this|that|these|those|its|it|their|his|her|our|your|my|each|every|any|no|not|some|all|both|either|neither" +
        @"|which|whose|what|another|other|same|next|own|and|or|nor|of|to|for|by|with|in|on|at|from|as|into|than|is|are|was" +
        @"|were|be|been|being";

    /// <summary>
    /// The original's numbering of Samus's movement in prose (#1856), outside hardware owners, spaced or hyphenated, in
    /// digits or words: a movement type by number (movement type zero, movement-type 6), a type by number hyphenated (the
    /// ordinary type-6 fall, #2089), a mode by number (mode one, modes
    /// one and two, acceleration mode 2), a collision result by number (collision result one) and a status by number (the
    /// elevator's status from one to two, status 0). The original numbered each pose's movement type, Samus's acceleration
    /// modes, her collision results and the elevator's status; Core names each one and cites it with <c>see cref</c>.
    /// English stays legal: a number that counts the noun after it (a mode two frames long), and a mode, a type, a result
    /// or a status alone.
    /// </summary>
    private static readonly Regex MovementNumberProse = new(
        @"\bmovement[-\s]+types?[-\s]+" + NumberWord + @"\b" + CountedNoun +
        @"|\btypes?-" + NumberWord + @"\b" +
        @"|\bmodes?[-\s]+" + NumberWord + @"\b" + CountedNoun +
        @"|\bcollision[-\s]+results?[-\s]+" + NumberWord + @"\b" + CountedNoun +
        @"|\bstatus(?:es)?[-\s]+(?:from[-\s]+)?" + NumberWord + @"\b" + CountedNoun,
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A documentation code element holding one name (#1794): <c>&lt;c&gt;MoveSamusWithDraygon&lt;/c&gt;</c>,
    /// <c>&lt;c&gt;Power.UpRight&lt;/c&gt;</c>, <c>&lt;c&gt;CalculateAngleFromXY()&lt;/c&gt;</c>. Core cites its own code
    /// with <c>see cref</c>, <c>paramref</c> or <c>typeparamref</c>, which the compiler resolves, so a compound name set as
    /// code is the original program's routine label, even where a Core member that ported the routine still spells it. An
    /// expression or several words set as code (<c>InitialAngle - 32</c>), a quoted string such as an installed file's key
    /// (<c>"Power.UpRight"</c>) and a name of one word (<c>Boyon</c>) are not read.
    /// </summary>
    private static readonly Regex CodeElementName = new(
        @"<c>\s*(?<name>[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)(?:\(\))?\s*</c>", RegexOptions.CultureInvariant);

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
    /// The original's trigonometry tables in prose (#1853), outside hardware owners, spaced or hyphenated: an angle counted
    /// in the table's units, angles or steps (eight table units, 0 through 63 table angles, an eighth of a table step) and
    /// a sine, cosine or trigonometry table (the shared sine table, a sine-table angle, the negative-cosine table, the
    /// trigonometry tables, a trig-table scale). The original looked its sines up in a 256-entry table and counted angles
    /// by their place in it; Core counts angles in steps, 256ths of a turn (<c>TrigonometryAngle</c>, <c>RotationAngle</c>),
    /// and computes the wave (<c>Trigonometry.CoarseHalfWave</c>). An encoding owner of the angle names the encoding it
    /// owns, and a compatibility service may describe the table its reproduced behaviour reads. English stays legal: a sine,
    /// a cosine, a table and a unit alone, a turn's steps and a speed table.
    /// </summary>
    private static readonly Regex TrigonometryTableProse = new(
        @"\btables?[-\s]+(?:units?|angles?|steps?)\b" +
        @"|\b(?:co)?sine[-\s]+tables?\b|\btrig(?:onometry|onometric)?[-\s]+tables?\b",
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

    /// <summary>
    /// The machine's display transfers, staging and video standard and the original's tables and records in prose (#1797),
    /// outside hardware owners: a frame, artwork, graphics, sprite, palette, picture, layer, display, queued or general
    /// transfer and a transfer queue, buffer, slot, size, length, source, destination or entry; a staging buffer, copy,
    /// area, list or queue, a foreground, background, scroll, layer, picture, artwork, graphics or sprite staging, staging
    /// one of those (stages the foreground) and one of those or a position staged (the scroll is being staged); the NTSC and
    /// PAL video standards, read case-sensitively; the common room elements, spaced or hyphenated; a selector table or row;
    /// and a record table, an input-table record, an input record and a pose record. Core publishes whole frames, places
    /// the views where they stand, names the release (the Japanese and North American release, the European release),
    /// names the shared door, item and block pictures every room carries, and names the choice, the entry, the input
    /// transition or the pose. English stays legal: the reserve tanks' transfer (a manual, reserve or energy transfer),
    /// transferring a value or a position, a stage of an animation or an ending, a staged fight or a staging point, a
    /// lower-case pal, a common room, a selector alone and a record alone or of something (a record of kills).
    /// </summary>
    private static readonly Regex MachineTableProse = new(
        @"\b(?:frame|artwork|graphics|sprite|palette|picture|layer|display|queued|general)[-\s]+transfers?\b" +
        @"|\btransfers?[-\s]+(?:queues?|buffers?|slots?|sizes?|lengths?|sources?|destinations?|entries)\b" +
        @"|\bstaging[-\s]+(?:buffers?|copy|copies|areas?|lists?|queues?)\b" +
        @"|\b(?:foreground|background|scroll|layer|picture|artwork|graphics|sprite)[-\s]+staging\b" +
        @"|\bstag(?:e|es|ed|ing)\s+(?:the\s+|its\s+|a\s+)?(?:foreground|background|scroll|layers?|pictures?|artwork|graphics|sprites?)\b" +
        @"|\b(?:foreground|background|scroll|layers?|pictures?|artwork|graphics|sprites?|position)\s+(?:(?:is|was|were|are|being|been)\s+)*staged\b" +
        @"|(?-i:\bNTSC\b|\bPAL\b)" +
        @"|\bcommon[-\s]+room[-\s]+elements?\b" +
        @"|\bselector[-\s]+(?:tables?|rows?)\b" +
        @"|\brecord[-\s]+tables?\b|\b(?:input[-\s]+table|input|pose)[-\s]+records?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's indirect dispatch, the machine's bus, regionalized values and the original's enemy entries named as
    /// records in prose (#1812), outside hardware owners: "indirect" before a jump, target, entry, call, table, selection or
    /// dispatch with up to two words between (its indirect jump, all four indirect targets, the indirect main-AI entry, its
    /// indirect state table, the indirect movement-table selection); a bus in any number (the room bus); "regionalized" in
    /// any word form; and a record that is an enemy's entry in the room's enemy list: a population, spawn, definition or
    /// boss record, a part's record (a body, wing, arms, head, brain, tongue, tube, statue, graphics or cosmetic record), a
    /// parent, follower, partner, physical or composite record, a record by its place (the first, second, third or fourth
    /// record), an empty, emptied, deleted or cleared record, and an actor of so many records (a three-record actor). Core
    /// names the action, phase or movement that runs, the room binding that supplies what an enemy calls, the release whose
    /// value differs, and the enemy, its part, its partner or its place in the room's list. English stays legal: indirect
    /// G-Mode and anything indirect that is not dispatch, the verb (the death records the drop origin), a record of
    /// something (the first record of the kills), an enemy's record (#1765) and a record alone.
    /// </summary>
    private static readonly Regex MachineEntryProse = new(
        @"\bindirect(?:ly)?[-\s]+(?:[\w-]+\s+){0,2}?(?:jumps?|targets?|entr(?:y|ies)|calls?|tables?|selections?|dispatch\w*)\b" +
        @"|\bbus(?:es|ses)?\b" +
        @"|\bregionali[sz]\w*" +
        @"|\b(?:population|spawn|definition|boss|body|wing|arms?|head|brain|tongue|tube|statue|graphics|cosmetic|parent" +
        @"|follower|partner|physical|composite)[-\s]+records?\b" +
        @"|\b(?:first|second|third|fourth|empty|emptied|deleted|cleared)\s+records?\b(?!\s+of\b)" +
        @"|\b" + NumberWordsOnly + @"-records?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The palette in prose (#1892), outside hardware owners, in any word form and outside markup: a palette, palettes, a
    /// palette's colours, a palette-cycle glow. The original coloured its pictures from palette rows and animated colours by
    /// rewriting them; Core shows pictures in their shades, owns colours and colour effects, and fades or steps them. A type
    /// named for a palette is cited by <c>see cref</c>, whose markup is not read, and a run-on symbol
    /// (<c>palette_change_num</c>) or stored value (<c>palette0800</c>) is another rule's.
    /// </summary>
    private static readonly Regex PaletteProse = new(@"\bpalettes?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's animation interpreter in prose (#1893), outside hardware owners and outside markup: its sleep in any
    /// word form (a script's sleep, a sleeping animation, sleeps on its frame, asleep, slept), its bytecode, and its
    /// streams (the head stream, a cue stream, a delay stream, the authored stream of ovals). Core's animations hold their
    /// last frame, rest or are hidden, and its cues and frames say what they do. English stays legal: an event, audio,
    /// sound, file, data, level, random, input, output or memory stream, and the verb of streaming in, into, through,
    /// from or out.
    /// </summary>
    private static readonly Regex InterpreterRestProse = new(
        @"\b(?:a?sleep\w*|slept)\b|\bbyte-?codes?\b" +
        @"|(?<!\b(?:event|audio|sound|file|data|level|random|input|output|memory)[-\s]+)\bstream(?:s|'s)?\b" +
        @"(?!\s+(?:in|into|through|from|out)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The review trail in prose (#1971, #1998, #2018), read in every Core type: a review, check, verification, audit or
    /// investigation cited by its ticket (reviewed for #1165, reviewed under 1165 (#2089), #1165 independently confirms,
    /// the #1165 research, Issue #1039:), anything independently reviewed, checked, verified, audited or confirmed, a
    /// pair of ticket citations (#625 / #643), an investigation in any word form, a proof, data pinned for reference (the pinned retail data, the
    /// pinned cartridge's sine words, its pinned room states), a release pinned by its version (<c>v1.0</c>) and a
    /// commit hash (<c>db3aa8b3bb</c>). #2105 adds the review inventory (see the review inventory's inputs) and fix
    /// history: an earlier implementation choice told by what it broke (Hardcoding the branch stranded the actor, Omitting
    /// this queue made the pad flash, Treating the room as sufficient discarded the view). Core states what the game
    /// does; how a value was reviewed, which ticket checked it, what an earlier version of the port got wrong and which
    /// commit fixed it are the repository's history, not the game's. English stays legal: a ticket cited for the change it
    /// names (<c>(#1989)</c>), a thing pinned in the game (Samus pinned by the beam, the ring pinned to the mouth), a check
    /// the game itself makes, treating a value as another with no breakage told, an inventory alone, a frozen enemy, and a
    /// decimal number. #2123 adds anything verified (the verified animations, a transition that is not verified, verified
    /// against captured frames): what the game does is stated, not that the port's author verified it; a check the game or
    /// the installation makes is checked.
    /// </summary>
    private static readonly Regex ReviewTrailProse = new(
        @"\breview\s+inventor(?:y|ies)\b" +
        @"|\b(?:omitting|hard-?coding|treating|ignoring|skipping|dropping|special-?casing)\b[^.;]*?" +
        @"\b(?:stranded|discarded|broke|broken|regressed|desync\w*|clipped)\b" +
        @"|\b(?:omitting|hard-?coding|ignoring|skipping|dropping|special-?casing)\s+(?:this|that|these|those)\b[^.;]*?" +
        @"\b(?:made|left|caused)\b" +
        @"|\b(?:reviewed|checked|verified|audited|confirmed|investigated)\s+(?:(?:for|in|by)\s+#|under\s+#?)[0-9]+" +
        @"|\bindependently\s+(?:re-?)?(?:review|check|verif|audit|confirm)\w*" +
        @"|#[0-9]+\s+(?:research|independently|review\w*|audit\w*|investigation)\b|\bissue\s+#[0-9]+" +
        @"|#[0-9]+\s*/\s*#[0-9]+" +
        @"|\binvestigat\w*|\bproofs?\b|\bverified\b" +
        @"|\bpinned\s+(?:[\w'-]+\s+){0,2}?(?:sources?|data|references?|cartridge|retail|release|game|room\s+states?|values?)\b" +
        @"|(?<![\w.])v[0-9]+\.[0-9]+(?![\w.])" +
        @"|(?-i:(?<![\w.#-])(?=[0-9a-f]{0,39}[a-f])(?=[0-9a-f]{0,39}[0-9])[0-9a-f]{7,40}(?![\w-]))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Relocation history in prose (#2133), part of the review trail and read in every Core type, hardware owners
    /// included, outside markup: a ticket followed by what a thing was or where it moved ((#1620, was <c>Old.Name</c>),
    /// (#1620, was in Bosses, unmarked), (#1416: moved from the deleted catalog)), a parenthetical opening with what a
    /// thing was, formerly was or where it moved ((was in Bosses), (moved from Samus's movement, #1621)), and where a thing
    /// moved from when its clause names no destination (moved here from the ending catalog). Which member a summary's
    /// subject was before a ticket renamed or moved it, and which module it lived in, are the repository's history, not
    /// the game's. English stays legal: a thing that moved from one state or place to another (the elevator moved from
    /// its arrival to its return), a ticket cited for the change it names, and a parenthetical of what a thing is.
    /// </summary>
    private static readonly Regex RelocationHistoryProse = new(
        @"#[0-9]+\s*[,:;]\s*(?:was|were|moved|formerly)\b" +
        @"|\(\s*(?:was|were|formerly|moved)\b" +
        @"|\bmoved\s+(?:here\s+)?from\b(?![^.;:()]*\bto\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// More of the review trail in prose (#2139), read in every Core type: anything reviewed or approved (the reviewed
    /// wording, a reviewed margin, the approved launch bodies) and the port's history told against its present: what
    /// something used to do before what it does now (replacing it with defaults used to hide bad data; the loader now
    /// gets the bindings) and what used to be (the update used to be gated). Core states what the game does; which
    /// wording a review approved and what the port did before are the repository's history, not the game's. English
    /// stays legal: a thing used to do something (the counter used to pace the dust) with no present told against it.
    /// </summary>
    private static readonly Regex ApprovalAndHistoryProse = new(
        @"\b(?:reviewed|approved)\b|\bused\s+to\s+be\b|\bused\s+to\b[^.;]*?(?:\.\s+[^.;]*?)?\bnow\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The port's narration of itself and of the original program in prose (#2139), outside hardware owners: managed code
    /// set against the original (the managed menu, the managed input order), the original's steps, programs and code (an
    /// otherwise unused original step) and its backing up and restoring of graphics (backing up gameplay graphics,
    /// restores gameplay graphics). Core is the game: it names what the menu, the input or the pause does, never that it
    /// is the managed version of something, which step of the original it is, or how the original kept its pictures.
    /// English stays legal: original alone (the original position), restoring a value or a colour, and graphics alone.
    /// </summary>
    private static readonly Regex PortNarrationProse = new(
        @"\bmanaged\b|\boriginal\s+(?:steps?|programs?|code)\b" +
        @"|\b(?:back(?:s|ed|ing)?[-\s]+up|restor(?:e|es|ed|ing))\s+(?:[\w'-]+\s+){0,2}?graphics\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A story event named by a number or as a bit in prose (#2139), outside hardware owners, markup included: an event
    /// number, spaced, hyphenated or run together (the event number, <c>eventNumber</c>), and an event or story-event bit
    /// (global event bits, a persistent event bit, story-event bits). The original numbered its story events and kept
    /// them as bits; Core names each <c>StoryEvent</c> and says whether it has happened. English stays legal: an event,
    /// a story event, a number and a bit alone.
    /// </summary>
    private static readonly Regex StoryEventNumberProse = new(
        @"\bevent[-\s]?numbers?\b|\b(?:story[-\s]+)?events?[-\s]+bits?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A check of the repository cited in prose (#2105), outside hardware owners: a check, test or suite named by its kind
    /// (the Tourian animation lookup check, the eye-fade verification checks every shade, a parity check, the regression
    /// test, the Ridley full-movie check) and a check's name set as code (<c>&lt;c&gt;BeamCallbackTablesVerification&lt;/c&gt;</c>,
    /// <c>&lt;c&gt;VerifyRunningCadence&lt;/c&gt;</c>). Core states what the game does; which of the repository's checks
    /// confirms it is the repository's, not the game's. A compatibility service's or an encoding owner's documentation may
    /// name the check that confirms the behaviour it reproduces. English stays legal: a check the game itself makes (the
    /// awakening facing check), the condition a replay tests, and a test or a check alone.
    /// </summary>
    private static readonly Regex CitedCheckProse = new(
        @"\b(?:lookup|verification|regression|oracle|parity|full-movie|movie)[-\s]+(?:checks?|suites?)\b" +
        @"|\b(?:lookup|verification|regression|oracle|parity)[-\s]+tests?\b" +
        @"|<c>\s*(?:\w+Verification|Verify\w+)\s*</c>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's numbering of its tables, entries, slots, parameters and programs in prose (#1971), outside hardware
    /// owners, in digits or words: a table or list by its count of entries or rows (a sixteen-entry shot-block table, the
    /// eight-entry list, a two-row table), an entry by number (entry 23, entries 40..43), a pool by its count of slots (the
    /// eighteen-slot pool, all eighteen projectile slots, all forty slots), a slot by number (slot zero, random slots 0 and
    /// 3), a parameter by number (animation parameter nine, the parameter-nine dust) and a program (a Torizo program, an
    /// animation program, the gunship's programs). The original numbered the entries of its lookup tables, the slots of its
    /// fixed pools and the parameters its routines took, and ran each animation as a program; Core names the choice, the
    /// step, the place or the animation. English stays legal: a number that counts the noun after it (an entry two frames
    /// later), a save slot by number, the rows of a screen, a menu or a panel (the two-row Yes/No prompt, the nine-row
    /// result panel), a slot or an entry alone, and a program joined into a name or a key (the sound-program credit, a
    /// generate-*-programs script).
    /// </summary>
    private static readonly Regex OriginalNumberingProse = new(
        @"\b" + NumberWord + @"-(?:entry|entries)\b|\b" + NumberWord + @"-rows?\s+(?:[\w-]+\s+)?tables?\b" +
        @"|\bentr(?:y|ies)[-\s]+" + NumberWord + @"\b" + CountedNoun +
        @"|\b" + NumberWord + @"-slots?\b|\b" + NumberWord + @"[-\s]+(?:enemy|projectile|sprite|object)[-\s]+slots?\b" +
        @"|\ball\s+" + NumberWord + @"\s+slots\b" +
        @"|(?<!\bsave[-\s]+)\bslots?[-\s]+" + NumberWord + @"\b" + CountedNoun +
        @"|\bparameters?[-\s]+" + NumberWord + @"\b" + CountedNoun +
        @"|(?<![-\w])programs?(?![-\w])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's call structure in prose (#1998, #2018), outside hardware owners: a caller in any number and form (the
    /// caller reads first, its callers, the caller's guard, caller-owned), a pace per call, with one word between
    /// (#2248: four pixels per call, one setup stage per AI call), and its
    /// dispatch in any word form (the main AI dispatches, dispatches the body's behaviours, before dispatch, the
    /// redispatch). The original's routines were called from others and jumped through tables of routines; Core says what
    /// runs, when and how often in the game (per update, per frame) and who owns a value. English stays legal: sending a
    /// request, a sound, music, a message, an event or audio (the queue dispatches the request, a dispatched request,
    /// audio dispatch), a call and calling.
    /// </summary>
    private static readonly Regex CallerProse = new(
        @"\bcallers?(?:'s|s')?\b|\bper\s+(?:[\w-]+\s+)?call\b" +
        @"|(?<!\b" + SentThings + @"[-\s]+)\b(?:re-?)?dispatch(?:es|ed|ing)?\b(?!\s+(?:[\w'-]+\s+){0,3}?" + SentThings + @"\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>What dispatching may send in English: a request, a sound, music, a message, an event or audio.</summary>
    private const string SentThings = @"(?:requests?|sounds?|music|messages?|events?|audio)";

    /// <summary>
    /// The sound chip's vocabulary in prose (#2018), outside hardware owners: a voice, the noun in any number (a voice
    /// queue, the looping voice, environmental voices, a sound voice) and a key-off, spaced or hyphenated (a key-off
    /// release). The original played its sounds on the chip's eight voices and released a note by keying it off; Core
    /// names the sound library and its queue, the looping sound and the clip's fade. English stays legal: the verb before
    /// its object (the particle voices the attack's sound) and a voice that keys on, which
    /// <see cref="MachineArithmeticProse"/> reads.
    /// </summary>
    private static readonly Regex SoundChipProse = new(
        @"\bvoice[sd]?\b(?!\s+(?:the|its|their|a|an|each|every)\b)|\bkey[-\s]?offs?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original program's structure and machine arithmetic in prose (#2080), outside hardware owners, spaced or
    /// hyphenated: a parameter table (calls through its parameter table), a tail call in any word form (tail-calls the
    /// initializer, a tail call into the setup, tail-calling the mover), a short multiply or multiplication, an extension
    /// array (the enemy extension arrays), a rotting table (the corpse's rotting table, a rotting-table row) and a value
    /// taken modulo 256 or mod 256 (Y modulo 256 reaches 240, the sample modulo 256). The original read its routines'
    /// settings from parameter tables, jumped on into another routine, multiplied by a byte, kept species state in
    /// arrays beside each enemy, rotted corpses row by row from a table and kept a value's low byte; Core says what
    /// runs next, names the circle product, the state, the rot schedule and the roll or place within a screen. A
    /// compatibility service describes the arithmetic it reproduces. English stays legal: a table of settings, a tail
    /// (Ridley's tail whip) or a call alone, a multiplication, a rotting corpse, and a value taken modulo another number.
    /// </summary>
    private static readonly Regex ProgramStructureProse = new(
        @"\bparameter[-\s]+tables?\b|\btail[-\s]?call\w*|\bshort[-\s]+multipl\w*|\bextension[-\s]+arrays?\b" +
        @"|\brotting[-\s]+tables?\b|\bmod(?:ulo)?[-\s]+256\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The display's colour math and screen designation in prose (#2089), outside hardware owners: a fixed colour, spaced
    /// or hyphenated, in either spelling (the fixed-colour flash, fades fixed color), the main-screen designation
    /// <c>TM</c>, read case-sensitively (switches TM back to text), and a per-line or per-scanline effect (the update's
    /// per-line effects, the rainbow beam's per-line effect). The original added a fixed colour to the picture, chose the
    /// main screen's layers by a register and drew its beams, hazes and waves by rewriting the display line by line;
    /// Core names the flash, the wash, the endpoint colour, the page shown and the effect itself (the beam's cone, the
    /// haze, the sky's bands). English stays legal: a colour that is fixed (a fixed set of colours), a lower-case tm, and
    /// a per-line scroll or window.
    /// </summary>
    private static readonly Regex DisplayEffectProse = new(
        @"\bfixed[-\s]+colou?rs?\b|(?-i:\bTM\b)|\bper[-\s]+(?:scan)?line\s+effects?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A value cited by the number the original stored or compared in prose (#2123), outside hardware owners: a value
    /// stored as a number for a meaning (stores the direction as zero for left and one for right) and a literal number
    /// (any value other than literal four, the literal five, the literal 190/66/2 branches). Core names the meaning: the
    /// direction is left or right, a pose faces left. English stays legal: a count of what is stored (stores these as four
    /// separate flags) and a literal alone.
    /// </summary>
    private static readonly Regex StoredValueProse = new(
        @"\bstor(?:e|es|ed|ing)\b[^.;]*?\bas\s+" + NumberWord + @"\s+(?:for|when|if|meaning)\b|\bliteral\s+" + NumberWord + @"\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A slot as an enemy's, a projectile's or a bomb's place in prose (#2152), outside hardware owners and outside markup:
    /// a slot in any number and word form, alone or named for what holds it (a slot holds an occupant, the enemy occupies
    /// its slot, the brain slot, the projectile slots, a bomb slot, an enemy-slot index, the highest-free-slot search). The
    /// original kept its enemies, projectiles and bombs in fixed slots; Core names the enemy's place in the room's list,
    /// the projectile's or the bomb's place in its pool, or the enemy or projectile itself. The game's save slots stay its
    /// own: a slot by its letter (slot C, slots A and B), a slot in a clause that names a save or a file (a save slot, the
    /// file slot, selecting an empty slot starts a new file), and every slot in a type named for saves or files (see
    /// <see cref="SaveTypeName"/>).
    /// </summary>
    private static readonly Regex EnemyPlaceSlotProse = new(
        @"(?<!\b" + SaveWords + @"\b[^.;:!?]*)\bslots?\b(?!\s+(?-i:[A-C])\b)(?![^.;:!?]*\b" + SaveWords + @"\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>The words of a save in prose: a save, saved, saves or saving, and a file.</summary>
    private const string SaveWords = @"(?:save[sd]?|saving|files?)";

    /// <summary>
    /// A type named for saves or files, whose slots are the game's save slots (#2152): a save or a file, read as a whole
    /// camel-case word (<c>SaveSlotStore</c>, <c>GameSaveJsonCodec</c>, <c>FileSelectMenuState</c>; not <c>Saved</c>).
    /// </summary>
    private static readonly Regex SaveTypeName = new(@"(?:Save|File)s?(?![a-z])", RegexOptions.CultureInvariant);

    /// <summary>
    /// A frame named as a map in prose (#2152), outside hardware owners and outside markup: the original's word for the
    /// sprite map a frame was built from, after what the frame shows (the one-frame hitbox map, the neutral map, a frozen
    /// map, the extended body maps, the displayed map, the last animation map, the tongue's or head's map) and after a
    /// count of frames (the four-map spin). Core shows frames and pictures. English stays legal: the map the player reads
    /// (the area map, the map's area labels), a map station and its access arm (an extended map access arm), and the verb
    /// (every pose maps to it).
    /// </summary>
    private static readonly Regex FrameMapProse = new(
        @"\b(?:hitbox|neutral|frozen|extended|displayed|animation|body|tongue|head)[-\s]+maps?\b(?!\s+(?:to|onto|into|access)\b)" +
        @"|\b" + NumberWord + @"-maps?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A subtraction's borrow in prose (#2152), outside hardware owners: borrow in any word form, the processor's carry
    /// out of a subtraction (a subtract/borrow formulation, the game's borrow chain, borrows from the frame, borrowing one
    /// whole pixel). Core says what the value did: it passes zero, drops a whole pixel, counts down past the frame.
    /// English stays legal: borrowing a thing in a clause that names it, ammunition or a round, a unit, a fuse or a path
    /// (a borrowed ammo unit, the Chainsaw beam's borrowed fuse, drawing borrows this frame path).
    /// </summary>
    private static readonly Regex ArithmeticBorrowProse = new(
        @"(?<!\b" + LentThings + @"\b[^.;:!?]*)\bborrow(?:s|ed|ing)?\b(?![^.;:!?]*\b" + LentThings + @"\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>What the game lends in English: ammunition or a round, a unit, a fuse or a path.</summary>
    private const string LentThings = @"(?:ammo|ammunition|rounds?|units?|fuses?|paths?)";

    /// <summary>
    /// The repository's checks in prose (#2152), outside hardware owners: verification and a verifier in any number and
    /// as a possessive (verification's catalog relates each member, the reusable head verifier, a constructed verifier
    /// room), a fixture (a synthetic room fixture, standalone enemy fixtures) and a movie (the recorded Ridley movie). Core
    /// states what the game does; which check confirms it, what a test builds and which recording a replay compares are
    /// the repository's. A compatibility service may cite the recording that shows the behaviour it reproduces. English
    /// stays legal: an assembly's name (<c>SuperMetroid.Verification</c>, <c>SuperMetroid.RenderVerification</c>) and a
    /// demo's recording.
    /// </summary>
    private static readonly Regex RepositoryCheckProse = new(
        @"(?<![.\w])verif(?:ications?|iers?)\b|\bfixtures?\b|\bmovies?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's tables, indexes and bugs in prose (#2152), outside hardware owners, spaced or hyphenated: a retail or
    /// original bug, with up to one word between (that retail bug, its original wall bug, the retail arm-pump bug); a retail
    /// order (the retail-ordered places); a random, angle, place or linear-speed table (the random-table selections, the
    /// angle table's displacement, the place table, the common linear-speed table); a table's convention, entries, samples
    /// or selections (in the table convention, pose-table entries, a water/lava table selection); identical entries; a
    /// collision or same index; the alpha pass; definition bits; and the typed representation. Core says what the game does:
    /// the places a room lists, the random choices, the enemy places, the speeds, the frame shown and the update that
    /// follows. English stays legal: a table alone (a drop-chance table), a speed table, an index alone and a bug that is not
    /// the original's.
    /// </summary>
    private static readonly Regex TableNarrationProse = new(
        @"\b(?:retail|original)[-\s]+(?:[\w-]+\s+)?bugs?\b|\bretail[-\s]+order(?:ed)?\b" +
        @"|\b(?:random|angle|place|linear-speed)[-\s]+tables?\b" +
        @"|\btables?[-\s]+(?:conventions?|entr(?:y|ies)|samples?|selections?)\b|\bidentical\s+entries\b" +
        @"|\b(?:collision|same)\s+index\b|\balpha\s+pass(?:es)?\b|\bdefinition\s+bits\b|\btyped\s+representations?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The machine's 16.16 arithmetic named by its sixty-five-thousandths in prose (#2172), outside hardware owners and
    /// outside markup: a 1/65536-pixel (or 1/65536th) addition, add, sum, helper, form, space, constant or arithmetic, with
    /// up to two words between (the game's 1/65536-pixel additions, one 1/65536-pixel add, the 1/65536-pixel collision
    /// helper, the ordinary 1/65536-pixel movement helpers, in signed 1/65536-pixel form, in 1/65536-pixel space). The
    /// original kept a position or speed as a whole word and a fraction word and added them as a pair; Core says what the
    /// value does: the speed moves the position, a fraction past a whole pixel carries into the whole pixels, the whole
    /// pixels wrap around the room. English stays legal: a unit of measure (a speed in 1/65536 pixels, a 1/65536-pixel
    /// velocity, displacement or position).
    /// </summary>
    private static readonly Regex SixteenSixteenArithmeticProse = new(
        @"(?<![\w/])1/65536(?:ths?)?(?:[-\s]pixels?)?(?:\s+[\w'-]+){0,2}?\s+" +
        @"(?:helpers?|additions?|adds?|sums?|form|space|constants?|arithmetic)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Paired arithmetic in prose (#2172), outside hardware owners, spaced or hyphenated: paired additions, adds,
    /// subtractions or sums (the game's paired additions), the original's add of a fraction word and then a whole word.
    /// Core says what the value does: a fraction past a whole pixel carries into the whole pixels. English stays legal: a
    /// pair of things the game pairs (paired doors, paired enemies) and an addition alone.
    /// </summary>
    private static readonly Regex PairedArithmeticProse = new(
        @"\bpaired[-\s]+(?:additions?|adds?|subtractions?|sums?)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A routine run or called as a helper or controller in prose (#2172), outside hardware owners and outside markup: run,
    /// runs, running, ran, call, calls, calling or called, then an article, determiner or possessive, up to six words and a
    /// helper or controller (Runs the linked-segment placement helper, runs the last segment's collision/reversal
    /// controller, calls the previous-pose helper, call the sound helper). The original split an enemy's or Samus's step
    /// into routines it called; Core says what the step does in the game: it places the segment, reverses the chain, takes
    /// the previous pose. English stays legal: the player's controller and its input, buttons or port (runs the controller
    /// input), a helper or controller named without running or calling it, and a test's helper outside Core.
    /// </summary>
    private static readonly Regex RoutineHelperProse = new(
        @"\b(?:run|runs|running|ran|call|calls|calling|called)\s+(?:the|its|their|a|an|this|that|each|both)\s+" +
        @"(?:[\w'/-]+\s+){0,6}?(?:helpers?|controllers?)\b(?![-\s]+(?:inputs?|buttons?|ports?)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The Japanese and North American release named for what the game does (#2192), outside hardware owners, where the
    /// paragraph states no difference from the European release (The Japanese and North American release measures
    /// angles in 256ths of a turn; the Japanese and North American release reloads the burrow duration). That release is
    /// the game Core plays; naming it says nothing a statement of the behaviour does not, unless the paragraph says what
    /// the European release does instead (see <see cref="ReleaseNarrationFinding"/>). English stays legal: a paragraph
    /// that sets the two releases side by side (the Japanese and North American release takes 80; the European release
    /// takes 99), across the lines of one comment too.
    /// </summary>
    private static readonly Regex ReleaseNarrationProse = new(
        @"\bJapanese\s+and\s+North\s+American\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>The European release, whose naming in a paragraph states a difference from the other release.</summary>
    private static readonly Regex EuropeanRelease = new(@"\bEuropean\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A speed named as a row of the original's speed table (#2192), outside hardware owners, spaced or hyphenated: a
    /// speed row or a speed-table row (the population's linear speed row, whose speed row is read). Core names the speed
    /// and the way it moves. English stays legal: a speed, a row of blocks and a speed table alone.
    /// </summary>
    private static readonly Regex SpeedRowProse = new(
        @"\bspeed[-\s]+(?:table[-\s]+)?rows?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A variable label cited as code in a comment (#2192), outside hardware owners: a lower-case identifier set in
    /// backticks (Testing only <c>`ranActorAi`</c> here, the family of <c>`shade`</c>). Core says what the value means in
    /// the game, and cites its own members with <c>see cref</c> or <c>paramref</c>, which the compiler resolves. English
    /// stays legal: an expression in backticks and a member's name (<c>`StepMovement`</c>).
    /// </summary>
    private static readonly Regex CitedVariableProse = new(@"`\s*[a-z_][A-Za-z0-9_]*\s*`", RegexOptions.CultureInvariant);

    /// <summary>
    /// A value's place in the original's numbering told as its sort order (#2192), outside hardware owners: a thing that
    /// sorts, sorted or sorting after, before, below or above another (a normal bomb's family sorts after the Power Bomb
    /// family). Core says what the game does with the two: which reaction ignores which. English stays legal: a sort
    /// alone and a list sorted by a named key.
    /// </summary>
    private static readonly Regex SortOrderProse = new(
        @"\bsort(?:s|ed|ing)?\s+(?:after|before|below|above)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's per-line scroll table (#2192), outside hardware owners, spaced or hyphenated: a scroll table, a
    /// background or room-specific scroll table (resets the background scroll table). Core names the distortion, the
    /// wave or the camera's limits. English stays legal: a scroll, a scroll's cells and a per-line scroll.
    /// </summary>
    private static readonly Regex ScrollTableProse = new(
        @"\bscroll[-\s]+tables?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Colours loaded into a row of the original's palette (#2192), outside hardware owners: loads, loaded or loading,
    /// up to four words, into, in or to, and a row, with up to two words before it (which his fight loads into the
    /// projectiles' row). Core says which colours a picture shows. English stays legal: loading a room, a file or a row
    /// that no load puts colours into (a room loads, the first row of blocks).
    /// </summary>
    private static readonly Regex LoadedRowProse = new(
        @"\bload(?:s|ed|ing)?\s+(?:[\w'-]+\s+){0,4}?(?:into|in|to)\s+(?:the\s+|its\s+|a\s+|his\s+|her\s+)?(?:[\w'-]+\s+){0,2}?rows?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// State told as the original's extension of an enemy's record (#2192), outside hardware owners, spaced or
    /// hyphenated: a state, boss, encounter, death, enemy, species, family, typed or record extension (the shared state
    /// extension, the typed encounter extension, Crocomire death extension), extended state and extra values (the ghost's
    /// extra values). Core names the state for what it holds: the fight, the death, the encounter. English stays legal:
    /// a block's extension, a neck's, tail's, leg's or rope's extension, and a value alone.
    /// </summary>
    private static readonly Regex StateExtensionProse = new(
        @"\b(?:state|boss|encounter|death|enemy|species|family|typed|record)[-\s]+extensions?\b|\bextended[-\s]+states?\b" +
        @"|\bextra\s+values?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A value told as a literal write (#2192), outside hardware owners: a literal write, store or assignment, with up to
    /// two words between (#2248: these radii are literal writes, preserve the literal write, literal inventory writes,
    /// literal observable writes). Core says what the value is and why the game sets it. English stays legal: a literal
    /// alone.
    /// </summary>
    private static readonly Regex LiteralWriteProse = new(
        @"\bliteral\s+(?:[\w-]+\s+){0,2}?(?:writes?|stores?|assignments?)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's cinematic objects named as background objects (#2222), outside hardware owners, spaced or
    /// hyphenated (five cinematic background-object updates, the eye's background object). Core names the actor, the
    /// animation or the update. English stays legal: a background alone and an object in the background.
    /// </summary>
    private static readonly Regex BackgroundObjectProse = new(
        @"\bbackground[-\s]+objects?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A palette's slices named as the ranges they colour (#2222), outside hardware owners and markup: a palette, colour,
    /// text, portrait, projectile, sprite or gameplay range, joined by slashes, commas or a conjunction (text/portrait
    /// ranges, gameplay and projectile ranges, page-three text and portrait ranges). Core names what fades or shows. English
    /// stays legal: a range alone, a shot's range and a range of rows.
    /// </summary>
    private static readonly Regex PaletteRangeProse = new(
        @"\b(?:palette|colou?r|text|portrait|projectile|sprite|gameplay)s?(?:\s*[/,]\s*|\s+(?:and|or)\s+|[-\s]+)" +
        @"(?:(?:palette|colou?r|text|portrait|projectile|sprite|gameplay)s?(?:\s*[/,]\s*|\s+(?:and|or)\s+|[-\s]+))*ranges?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A colour told as a write to the palette or a fade's channel step (#2222), outside hardware owners and markup: a
    /// colour write, spaced or hyphenated, overwriting up to four words before a colour (explicitly overwrites the two
    /// glyph colours), and a component update or step (thirty-two component steps). Core says which colours a picture or
    /// the screen shows and how a fade steps. English stays legal: a colour alone, a colour step, writing a picture's
    /// colours into a buffer and writing a message.
    /// </summary>
    private static readonly Regex ColourWriteProse = new(
        @"\bcolou?r[-\s]+writes?\b|\boverwrit(?:es|e|ing|ten)\s+(?:[\w'-]+\s+){0,4}?colou?rs?\b" +
        @"|\bcomponent[-\s]+(?:updates?|steps?)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A script's step named as the original's record (#2222), outside hardware owners, spaced or hyphenated: a character,
    /// caption, glyph, letter, text, script, instruction, wait, delay or one-frame record (between character records, the
    /// katakana caption record, installed glyph records). Core names what the step types, shows or waits. English stays
    /// legal: a record of something, the verb and a demo's recording.
    /// </summary>
    private static readonly Regex ScriptRecordProse = new(
        @"\b(?:character|caption|glyph|letter|text|script|instruction|wait|delay|one-frame)[-\s]+records?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The port's narration of the hardware's drawing order it dropped (#2222), outside hardware owners: tiers that once
    /// interleaved and a presentation quirk the model does not keep (the priority tiers that once interleaved them are a
    /// presentation quirk). Core states the order the layers are drawn in. English stays legal: interleaving alone and a
    /// quirk of the game.
    /// </summary>
    private static readonly Regex DroppedQuirkProse = new(
        @"\bonce\s+interleaved\b|\bpresentation\s+quirks?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A CPU opcode's mnemonic set as code in markup (#2222), outside hardware owners, read case-sensitively: the
    /// increments, decrements, carry and flag instructions and register transfers <see cref="CpuProse"/>'s list leaves out
    /// (<c>&lt;c&gt;INC&lt;/c&gt;</c> permits more than one ring, <c>&lt;c&gt;DEC&lt;/c&gt;</c>, <c>&lt;c&gt;CLC&lt;/c&gt;</c>), which
    /// read as words outside markup. Core says what the value does: a count goes up. English stays legal: a name in capitals
    /// that is no mnemonic (<c>&lt;c&gt;HUD&lt;/c&gt;</c>).
    /// </summary>
    private static readonly Regex MarkupMnemonicProse = new(
        @"<c>\s*(?:INC|DEC|INX|INY|DEX|DEY|CLC|SEC|CLI|SEI|CLV|BIT|TXY|TYX|TCD|TDC|TCS|TSC|XCE)\b[^<]*</c>",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// A CPU register reloaded (#2222), outside hardware owners: reload, reloads, reloaded or reloading the X, Y or A
    /// register, the accumulator or an index register (does not reload Y), the letter read case-sensitively. Core says what
    /// value the step uses. English stays legal: reloading a timer, a room or a weapon.
    /// </summary>
    private static readonly Regex RegisterReloadProse = new(
        @"\b[Rr]eload(?:s|ed|ing)?\s+(?:the\s+)?(?:[XYA]\b|accumulator\b|index\s+registers?\b)", RegexOptions.CultureInvariant);

    /// <summary>
    /// A field of the original's C reimplementation cited in prose (#2222), outside hardware owners and markup: a
    /// capitalised structure name, a dot and a lower-case field (the Baby place's <c>Enemy.health</c>,
    /// <c>Samus.x_pos</c>). Core names the value it means. English stays legal: a file's name (<c>Program.cs</c>) and a
    /// member Core cites with <c>see cref</c>.
    /// </summary>
    private static readonly Regex CFieldProse = new(
        @"\b[A-Z][A-Za-z]*\.[a-z][a-z0-9]*(?:_[a-z0-9]+)*\b(?<!\.(?:cs|json|png|md|smc|sfc|txt|ps1|py|xml|csproj|slnx|dll|exe))",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// A value told as stored packed (#2222), outside hardware owners: packed or unpacked, then up to four words and a
    /// value, field, word, byte, pair, bit, radius, coordinate, position, decimal or digit (the projectile's packed X and
    /// Y collision radii, a packed decimal pair). Core names the values it holds. English stays legal: a pack of enemies,
    /// packing pictures onto a sheet and a backend's packing.
    /// </summary>
    private static readonly Regex PackedProse = new(
        @"\b(?:un)?packed\s+(?:[\w-]+\s+){0,4}?(?:values?|fields?|words?|bytes?|pairs?|bits?|radii|radius|coordinates?" +
        @"|positions?|decimals?|digits?)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original disassembly's label set as a summary (#2222), outside hardware owners: an enemy projectile or enemy
    /// proj animation label (Enemy projectile animation: Mother brains drool) and the original's Japanese text label
    /// (time Bomb set japan text). Core says what the animation shows. English stays legal: an enemy's animation and
    /// the Japanese text.
    /// </summary>
    private static readonly Regex DisassemblyLabelProse = new(
        @"\bEnemy\s+(?:proj(?:ectile)?\s+)?animation\s*:|\bjapan\s+text\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's numbering of its encounters' states (#2222), outside hardware owners and markup: an event, form,
    /// place or region by a number in digits, with an optional to (changes body form to 4, scroll region 1, lands in
    /// place 17), and a corpse, combat, final-combat, body or story event, form or state by a number in digits or words
    /// (corpse state one, final-combat state 2, story event two). Core names the story event, the form, the state, the
    /// scroll or the place. English stays legal: a number that counts the noun after it (a form 2 frames long), a number
    /// word after a verb or as a determiner (places two pickups, the four definitions that form one encounter), a room's
    /// or the beam's state by number (#1769), and a stage by number, which names the game's own Speed Booster stages and
    /// the stages its pictures show.
    /// </summary>
    private static readonly Regex NumberedStateProse = new(
        @"(?:\b(?:events?|forms?|places?|regions?)\s+(?:to\s+)?\d+\b" +
        @"|\b(?:corpse|combat|final-combat|body|story)[-\s]+(?:events?|forms?|states?)\s+(?:to\s+)?(?:\d+|zero|one|two|three|four" +
        @"|five|six|seven|eight|nine|ten|eleven|twelve)\b)" +
        @"(?!\s+(?:of|frames?|pixels?|updates?|steps?|times?|blocks?|seconds?|hits?|calls?)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The cartridge's retail data named for its storage (#2222), outside hardware owners: retail then up to two words and
    /// a population or an initializer (Mother Brain's retail population, retail part init). Core names the enemies in the
    /// room and what their setup does. English stays legal: a retail cartridge alone.
    /// </summary>
    private static readonly Regex RetailPopulationProse = new(
        @"\bretail\s+(?:[\w'-]+\s+){0,2}?(?:populations?|init(?:iali[sz](?:ation|ers?|ed))?)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's enemies behind an encounter's parts told as the physical ones (#2265), outside hardware owners:
    /// physical, then up to two words and a part, pool, enemy, actor, tube, projectile, explosion, bomb, body, Baby or
    /// place (the physical projectile pool, Draygon's physical body place, the physical Baby, physical tube enemies), and
    /// a part's enemy by number (tube enemy 1). Core names the part, the falling tube, the projectile or the enemy itself.
    /// English stays legal: physical contact and a physical component or motion, which are geometry.
    /// </summary>
    private static readonly Regex PhysicalCopyProse = new(
        @"\bphysical\s+(?:[\w-]+\s+){0,2}?(?:parts?|pools?|enem(?:y|ies)|actors?|tubes?|projectiles?|explosions?|bombs?" +
        @"|bod(?:y|ies)|Baby|places?)\b" +
        @"|\b(?:tube|part|body|head)\s+enem(?:y|ies)\s+(?:[0-9]+|one|two|three|four|five)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's routines named as walkers and helpers (#2265), outside hardware owners and markup: a collision,
    /// hitbox, frame-hitbox, extended, shot, touch, contact, bomb, rectangle, caret or actor walker and the walker (the
    /// extended-hitbox walker marks the projectile, the collision walkers test), and a damage, standing, posture,
    /// collision, movement, shared, common, ordinary, walking or AI helper and the helper (the damage helper, the
    /// standing helper reports her standing, the helper's hard floor). #2271 adds the vector and fixed-point helpers (the
    /// signed fixed-point vector helper). Core names the collision test, the check or the step and says what it does.
    /// English stays legal: an enemy that walks (a floor walker, the left walker's role), a helper alone and the helper
    /// text of a screen.
    /// </summary>
    private static readonly Regex RoutineWalkerProse = new(
        @"\b(?:collision|hitbox|frame-hitbox|extended(?:-hitbox)?|shot|touch|contact|bomb|normal-bomb|power-bomb|rectangle" +
        @"|caret|actor|cinematic-actor)\s+walkers?\b|\bthe\s+walkers?\b" +
        @"|\b(?:damage|standing|posture|collision|room-collision|movement|shared|common|ordinary|walking|AI|vector|fixed-point)\s+helpers?\b" +
        @"|\bthe\s+helpers?\b(?![-\s]+(?:texts?|messages?|labels?|lines?)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The machine's bit arithmetic in prose (#2265), outside hardware owners: a mask written as an and with a number
    /// (<c>RNG&amp;7</c>, <c>a &amp; 127</c>) and a masked random number, bit, sample or counter (masked RNG or frame
    /// bits); a branch or wrap to negative (a decrement of zero branches negative, it expires when it wraps negative);
    /// and a value told as unsigned (the addition and comparison are unsigned here, the speed read unsigned). Core says
    /// which choice a random number makes, that a count runs out or passes zero, and which way a value lies. English
    /// stays legal: an and between words (Samus &amp; Ridley) and an unsigned value alone.
    /// </summary>
    private static readonly Regex BitArithmeticProse = new(
        @"(?:&amp;|(?<!&)&(?!&))\s*[0-9]+\b|\bmasked\s+(?:[\w-]+\s+)?(?:RNG|random|bits?|samples?|counters?)\b" +
        @"|\b(?:branch(?:es|ed|ing)?|wraps?|wrapped|wrapping)\s+(?:to\s+)?negative\b" +
        @"|\bunsigned\s+here\b|\b(?:is|are|was|were|read|reads)\s+unsigned\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's program flow in prose (#2248, #2265), outside hardware owners and markup: the code installing,
    /// writing, storing, loading, branching, jumping, calling, running, falling or reading (the code installs the typing
    /// step); installing a phase, step, setup, behaviour, handler, routine, reaction, wait, action, delay or countdown,
    /// with up to two words between (installs the wait phase, installs its moving frame behaviour); a jump into an
    /// animation, routine, code, branch, handler, script, label or list (the shot animation's jump into the common
    /// deletion animation); and a fall-through: the noun, and falling into a phase, step, state, setup, branch, case,
    /// call, code or successor, or falling through into or to anything but a place (the delay falls through into its
    /// first step, the neck step falls directly into its successor). Core says what the game does next and in which
    /// update: the step starts, it takes its first step in the same update. English stays legal: installed content
    /// (the installed pictures), Samus jumping to a ledge and anything falling through into a room, a floor, the water or
    /// a gap.
    /// </summary>
    private static readonly Regex ProgramFlowProse = new(
        @"\bthe\s+code\s+(?:installs?|writes?|stores?|loads?|branches|jumps?|calls?|runs?|falls?|reads?)\b" +
        @"|\binstall(?:s|ing)?\s+(?:the\s+|its\s+|a\s+|an\s+|their\s+|his\s+|her\s+)?(?:[\w-]+\s+){0,2}?(?:phases?|steps?" +
        @"|setups?|behaviou?rs?|handlers?|routines?|reactions?|waits?|actions?|delays?|countdowns?)\b" +
        @"|\bjump(?:s|ed|ing)?\s+(?:in)?to\s+(?:the\s+|its\s+|a\s+|an\s+|another\s+)?(?:[\w-]+\s+){0,2}?(?:animations?" +
        @"|routines?|code|branch(?:es)?|handlers?|scripts?|labels?|lists?)\b" +
        @"|\bfall-?through\b" +
        @"|\bfall(?:s|ing)?\s+(?:straight\s+|directly\s+|immediately\s+)?(?:in)?to\s+(?:the|its|that|this)\s+(?:[\w-]+\s+){0,2}?" +
        @"(?:phases?|steps?|states?|setups?|branch(?:es)?|cases?|calls?|code|successors?)\b" +
        @"|\bfall(?:s|ing)?\s+(?:straight\s+|directly\s+)?through\s+(?:in)?to\b(?!\s+(?:the\s+|a\s+)?(?:[\w-]+\s+)?(?:rooms?" +
        @"|floors?|platforms?|water|lava|acid|ground|pits?|holes?|gaps?|below)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The port's narration of its own data and the original's tables (#2248), outside hardware owners: data told as
    /// engine-owned, spaced or hyphenated (Ridley's engine-owned hitboxes); a default or combination row (the default
    /// row for every other area, the uncharged combination row); a colour by number in words (transparent colour zero);
    /// and an actor's or projectile's parameter and a parameter by its place (the actor parameter's rightward flag, its
    /// first parameter). Core names what the data is for: the hitboxes the game defines, the damage everywhere else, the
    /// colour no picture draws, which way an egg moves. English stays legal: an engine alone, a row of blocks and a
    /// method's parameter.
    /// </summary>
    private static readonly Regex PortDataProse = new(
        @"\bengine[-\s]+owned\b|\b(?:default|combination)\s+rows?\b|\bcolou?rs?[-\s]+zero\b" +
        @"|\b(?:actor|projectile)[-\s]+parameters?\b|\b(?:first|second|third|fourth)\s+parameter\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The machine's storage told for a value (#2271), outside hardware owners: a value shifted right or left by a number
    /// of places (health shifted right one, each axis shifted left eight bits, a right-shift); state cleared as the room
    /// loads (clears on room load, cleared as the room's enemies load, the room's enemy setup clears); a table's
    /// physical order; both halves of one random number; and a value kept in another quantity (the game kept it in the
    /// projectile's horizontal velocity). Core states the value's meaning: half his starting health, 256 pixels a screen,
    /// a ring that starts at the room's origin, the frames in the order named, the random number's choices, the phase the
    /// segment is in. English stays legal: a shift by a counted noun (the camera shifts left two pixels), a room that
    /// clears when Samus leaves, physical contact, halves of a picture and a value kept for an update.
    /// </summary>
    private static readonly Regex MachineStorageNarrationProse = new(
        @"\bshift(?:s|ed|ing)?\s+(?:right|left)\s+(?:by\s+)?" + NumberWord + @"\b" + CountedNoun +
        @"|\b(?:right|left)-shift(?:s|ed)?\b" +
        @"|\bclear(?:s|ed|ing)?\s+(?:on|at)\s+(?:the\s+)?room(?:'s)?\s+loads?\b" +
        @"|\bcleared\s+(?:as|when)\s+the\s+room's\s+enem(?:y|ies)\s+loads?\b|\benemy\s+setup\s+clears\b" +
        @"|\bphysical\s+order\b" +
        @"|\bboth\s+halves\s+of\s+(?:one|the|a|each|its)\s+(?:[\w-]+\s+){0,2}?(?:RNG|random)\w*" +
        @"|\b(?:kept|keeps|stored|stores)\s+(?:it|them)\s+in\s+(?:the\s+|its\s+|a\s+)?(?:[\w'-]+\s+){0,3}?" +
        @"(?:velocit(?:y|ies)|timers?|counters?)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's routines told as wrappers, helpers and controllers (#2292), outside hardware owners and markup: a
    /// wrapper in any number (the turning wrapper, the movement wrapper skips block detection); a helper after any word but
    /// an article (the rising helper, the bottom-boundary helper, this helper, each helper); a scripted, drained, grabbed,
    /// tail, pogo, flight, fake-wall, formation or movement controller, its or their controllers, an owner's controllers
    /// (Mother Brain's controllers install it) and a controller paired with a pose. Core names the step and says what it
    /// does. English stays legal: a helper alone, the helper text of a screen, the player's controller and its input, a
    /// scripted controller's input, and Shaktool's room controller, the room object the game places.
    /// </summary>
    private static readonly Regex RoutineWrapperProse = new(
        @"\bwrappers?\b" +
        @"|\b(?!(?:a|an)\b)[\w'-]+\s+helpers?\b(?![-\s]+(?:texts?|messages?|labels?|lines?)\b)" +
        @"|\b(?:scripted|drained(?:-\w+)?|crouching-or-falling|grabbed|tail|pogo|flight|fake-wall|formation|movement)\s+controllers?\b(?![-\s]+(?:inputs?|buttons?|ports?)\b)" +
        @"|\b(?:its|their)\s+controllers?\b|\b\w+'s\s+controllers\b(?<!\bplayer's\s+controllers)|\bcontroller/pose\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's storage of Samus's and the game's state in prose (#2292), outside hardware owners and markup: a merge
    /// path; an equipment, state, suit, item, beam, facing, release, turning or liquid bit and an item's own bit (the Screw
    /// Attack bit, both suit bits are equipped); both bits; and this, that, the, its or which bit (Grapple physics reads this
    /// bit). Core names the item equipped, the flag or the state. English stays legal: a bit more, a bit of the way, and a
    /// hardware owner. #2321 adds the sound-effect and vulnerability flags or bits that escaped it (the shared SFX flag,
    /// the rainbow-beam SFX-playing flag, the vulnerability bit): Core says whether the sound plays or the shot blocks.
    /// </summary>
    private static readonly Regex StateBitProse = new(
        @"\b(?:SFX(?:-playing)?|sound-playing|vulnerability)[-\s]+(?:flags?|bits?)\b" +
        @"|\bmerge[-\s]+paths?\b" +
        @"|\b(?:equipment|state|suit|item|beam|boss|event|status|collected|enabled|upgrade|pickup|facing|release|turning" +
        @"|liquid|Varia|Gravity|Spazer|Plasma|Morph|Attack|Jump|Draygon-facing)[-\s]+bits?\b" +
        @"|\bboth\s+bits\b|\b(?:this|that|the|its|which)\s+bit\b(?!\s+(?:of|more|less|further|longer)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A step of a sequence told by the original's stage number (#2292), outside hardware owners and markup: a stage by
    /// number in digits or words (stage three equips the suit, stages three and four, stage-two edge pair) and a numbered
    /// stage before a noun (the third-stage handoff), unless its clause names the Speed Booster's run (a boost, booster,
    /// run, running or speed), whose stages the game itself counts. Core names what the step does: the beam narrows, the
    /// suit is equipped. English stays legal: a stage that counts the noun after it (a stage two frames long) and the
    /// Speed Booster's stages (boost stage four, a stage-four run).
    /// </summary>
    private static readonly Regex NumberedStageProse = new(
        @"(?<!\b(?:boost\w*|booster|runs?|running|speed)\b[^.;:!?]*)" +
        @"(?:\bstages?[-\s]+" + NumberWord + @"\b(?![-\s]+(?:frames?|updates?|pixels?|times?)\b)" +
        @"|\b(?:first|second|third|fourth|fifth|sixth|seventh)-stage\b)" +
        @"(?![^.;:!?]*\b(?:boost\w*|booster|runs?|running|speed)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Updates counted as the original's calls in prose (#2292), outside hardware owners and markup: a number of calls
    /// (sixteen calls precede the flash, ten approach calls, how many calls it lasts), a numbered call joined to its noun
    /// (the nine-call cadence), a call picked out by each, every, this, that, the next, previous, first or last and the
    /// like (runs one call of the destruction, on the destruction's first call), calls left, remaining or apart, a call
    /// count, order or cadence, and counted, flashing or approach calls. Core counts updates, frames or steps. English
    /// stays legal: a call into another owner (a call to the queue), one call per room, one host call, an input built for
    /// one call, and calling.
    /// <para>
    /// #2321 closes the forms that escaped: a call picked out with up to four words before it, hyphenated or joined by a
    /// slash (the next body AI call, the first post-hit enemy call, another beam/movement call, one Mother Brain
    /// active-rainbow body-phase call); a same-call step; rot calls; and the call as the update's own work, on the call
    /// or the call that shows, gave, applied, ran, runs, asks, completes, reaches or takes something, the call after
    /// another and the call's result. English stays legal: the call alone and a call that returns to its caller.
    /// </para>
    /// </summary>
    private static readonly Regex UpdateCallProse = new(
        @"\b(?:" + NumberWord + @"|how\s+many|many|several|few|more|fewer)\s+(?:[\w-]+\s+)?calls\b" +
        @"|\b(?:" + NumberWord + @"|same)-calls?\b" +
        @"|(?<!\b(?:for|in)\s)\b(?:each|every|per|this|that|next|previous|following|same|first|last|second|third|final|one|another)\s+" +
        @"(?:(?!the\b|host\b)[\w/-]+\s+)?call\b(?!\s+to\b)(?!\s+per\s+(?!(?:update|frame)s?\b))" +
        @"|(?<!\b(?:for|in)\s)\b(?:each|every|this|that|next|previous|following|same|first|last|second|third|final|one|another)\s+" +
        @"(?:(?!(?:the|a|an|host|is|it|its|and|or|of|to|that|which|does|then|per|for|in|on|at|as|by)\b)[\w/-]+\s+){2,4}call\b" +
        @"(?!\s+(?:to|into|per)\b)" +
        @"|\brot\s+calls?\b" +
        @"|\b(?:on|until)\s+the\s+call\b(?!\s+(?:to|into)\b)" +
        @"|\bthe\s+call(?:'s|’s)?\s+(?:after|that|which|shows|showed|gave|gives|applied|applies|ran|runs|asks|asked|last" +
        @"|completes|reaches|takes|result)\b" +
        @"|\bcalls?\s+(?:left|remaining|apart|later|earlier|longer|precedes?)\b|\bcall\s+(?:counts?|counters?|order|cadences?)\b" +
        @"|\b(?:counted|flashing|approach)\s+calls\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The machine's arithmetic and the original's data told for a value (#2292), outside hardware owners and markup: a
    /// value compared, read, treated or taken as signed; a deferred, pending or delayed copy or write and a suit's,
    /// shade's or normal colour's copy (perform the suit colours copy, flush that deferred write); odd or even frames that
    /// return; hardcoded data; an offset written with a plus sign, glued to a name or before a noun (destination+31, the
    /// +31 allowance, align at +0); a count of bits (wrapping at sixteen bits, a three-bit jitter); a position, policy,
    /// launch, nail or search row, record or table, a search order and a row by its number (the 352 row); and arithmetic
    /// written as a summary (the left edge (-24) minus two, 256 + 108 * 96 = 10624). Core states what the value means:
    /// which way a distance lies, where the camera stops, the room's wrap, the destination. English stays legal: a signed
    /// value, a sum written with spaces (n + 1 updates), a signed list of deltas (+1 for frames 0..5) and a hardware owner.
    /// #2321 adds a counter told by the range its storage holds (the frame counter of 0 through 65535), which escaped it:
    /// Core says which updates a cadence picks.
    /// </summary>
    private static readonly Regex MachineReadingProse = new(
        @"\bcounters?\s+of\s+0\s+through\s+(?:255|65535)\b" +
        @"|\b(?:compared|compares|read|reads|treated|taken|interpreted|tested)\s+(?:as\s+)?signed\b" +
        @"|\b(?:deferred|pending|delayed)\s+(?:copy|copies|writes?)\b|\b(?:suit|shade|normal-colou?r)\s+(?:colou?rs?\s+)?(?:copy|copies)\b" +
        @"|\b(?:odd|even)\s+(?:frames?|updates?)\s+return\b|\breturns?\s+(?:early\s+)?on\s+(?:odd|even)\s+(?:frames?|updates?)\b" +
        @"|\bhard-?coded\b" +
        @"|\b[A-Za-z]{2,}\w*\+[0-9]+\b|(?<![\w)\]])\+[0-9]+\s+(?!(?:or|and|for|to|through)\b)[a-z]+|\b(?:at|use|uses|using)\s+\+[0-9]+\b" +
        @"|\b" + NumberWord + @"[-\s]+bits?\b" +
        @"|\b(?:movement-policy|policy|position|launch|nail|search)[-\s]+(?:rows?|records?|tables?)\b|\bsearch\s+order\b" +
        @"|\bthe\s+[0-9]+\s+rows?\b" +
        @"|\(\s*[-+]?[0-9]+\s*\)\s+(?:plus|minus|times)\b|\b[0-9]+(?:\s*[-+*×/]\s*[0-9]+)+\s*=\s*[0-9]+\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The retail game and the original's cursors in prose (#2292), outside hardware owners and markup: retail in any use
    /// (retail emits a rock, exactly as retail, the retail game, every retail door), and a cursor through the original's
    /// lists: a history, movement, animation, line, character, shared, aimed, path, table, list or sequence cursor, a
    /// cursor advanced or stepped (the falling sequence advances the cursor), a cursor that crosses, passes or resumes, one
    /// that stays at its end, and a selection by a cursor. Core is the game: it names what the game shows or does next.
    /// English stays legal: a menu's cursor (the cursor stays on the transfer), the cursor's picture and a hardware owner.
    /// </summary>
    private static readonly Regex RetailCursorProse = new(
        @"\bretail\b" +
        @"|\b(?:history|movement|animation|line|character|next-character|shared|aimed|delayed|path|table|list|read|write|frame" +
        @"|segment|sequence|ring|rock|fragment)\s+cursors?\b" +
        @"|(?<!\b(?:the|a|an|each|every)\s)\b(?:advances?|advanced|advancing|steps?|stepped|stepping)\s+(?:the|its|a|their)\s+" +
        @"(?:[\w-]+\s+)?cursors?\b" +
        @"|\bcursor\s+(?:crosses|passes|resumes)\b|\bcursor\s+stays\s+at\s+(?:the|its)\s+end\b|\bby\s+a\s+cursor\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A camel-case word of a declared name that counts the original's calls (#2292): Call or Calls at the start of a name,
    /// after an underscore or at a word boundary, and not the start of a longer word (<c>FlashCalls</c>,
    /// <c>ImpactSoundCall</c>, <c>DelayCallsPerRow</c>, <c>_holdCalls</c>; not <c>Callback</c>, <c>Called</c> or
    /// <c>Caller</c>). Core counts updates, frames or steps and names them so.
    /// </summary>
    /// <summary>
    /// Samus's pose data narrated as the original's stored numbers and tables in prose (#2322), outside hardware owners
    /// and outside markup: a direction stated as a number (the pose X direction is deliberately eight, the pose-X
    /// direction is zero, the shot direction is two, a direction that stores six), a table's entry or route (the bomb-jump movement table's entry, the landing
    /// table's route), and a transition, input or distance table (a transition-table route, an input-table transition, the
    /// posture change's distance table). The original stored a pose's facing and aim as numbers and chose her next pose,
    /// her movement's response and her posture's distance by reading tables; Core names the facing and the aim and names
    /// the input transitions, the movement's definition and the distances by their definitions. English stays legal: a
    /// number that counts the noun after it (the direction is two pixels away), a direction a number measures from
    /// (the direction one turn on, the direction of one link), a numbered direction of a type that numbers its own
    /// (direction zero of the rope's turn), a direction alone, a table alone and a table's own words (a table of speeds).
    /// </summary>
    private static readonly Regex PoseDataNarrationProse = new(
        @"\bdirections?(?:[-\s]|/{2,3})+(?:is|was|stores?|=)(?:[-\s]|/{2,3})+(?:deliberately(?:[-\s]|/{2,3})+)?" +
        NumberWord + @"\b" + CountedNoun +
        @"|\btable(?:'|\u2019)s(?:[-\s]|/{2,3})+(?:entr(?:y|ies)|routes?)\b" +
        @"|\b(?:transition|input|distance)(?:[-\s]|/{2,3})+tables?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's arithmetic, program structure and leftover storage in prose (#2345), outside hardware owners and
    /// markup: a value told as divided by, modulo or the remainder of a number (the sample divided by 256, modulo 4; the
    /// random number modulo 64; the result's remainder by eight); an if/else or else-if chain, a call chain, a leading
    /// call, a call the code does not or never makes, a no-op and a routine's prologue (the bug's update is an if/else-if
    /// chain, the leading call in the game's reaction, custom shot AI does not call normal shot AI, the prologue requires
    /// its parent); and storage told as left over or never read (nothing a previous Kzan left reaches a new one, a height
    /// it republishes, which nothing reads). Core says which of its choices a random number makes and what the game does.
    /// English stays legal: dividing a picture, an instruction not to call a method, Ceres, the prologue station, values
    /// nothing reads because they are identities, and what a previous room hands the next.
    /// </summary>
    private static readonly Regex ArithmeticAndStorageSummaryProse = new(
        @"\bdivided\s+by\s+" + NumberWord + @"\b|\bmodulo\s+" + NumberWord + @"\b|\bremainder\s+(?:by|of|after)\s+" + NumberWord + @"\b" +
        @"|\bif\s*/\s*else(?:[-\s]if)?\b|\belse[-\s]if\b|\b(?:if|condition|branch|call)\s+chains?\b|\bleading\s+calls?\b" +
        @"|\b(?:does\s+not|doesn't|never)\s+call\b|\bno-op\b|(?<!\bstation\s)\bprologue\b(?!\s+station\b)" +
        @"|\bnothing\s+(?:a|the)\s+previous\b|\b(?:which|that|but)\s+nothing\s+reads\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex CallNameWord = new(@"(?:^|_|(?<=[a-z0-9])(?=[A-Z]))[Cc]alls?(?![a-z])", RegexOptions.CultureInvariant);

    /// <summary>
    /// The original's routines and storage told in boss and enemy prose (#2336), outside hardware owners and markup: an
    /// initializer (the shared initializer, the spin-jump initializer's priority), a writer (the common writer emits his
    /// body), an execution counter, ordinary projectile places and places scanned in ascending or descending order, a
    /// value wrapped into the coordinate range, a branch on a value (the shot reaction branches on area index), a signed
    /// compare, comparison or branch, and the roaring or damaged flag. Core says what the game does: the enemy's or the
    /// pose's setup, the common enemy drawing, the update count, Samus's shots, a position off the room's edge, what a
    /// reaction depends on, which way a distance lies, the roar ending. English stays legal: a C# object, collection,
    /// field, property, array, static or module initializer, a branch of a projectile, and a hardware owner.
    /// </summary>
    private static readonly Regex RoutineNarrationProse = new(
        @"\binitiali[sz]ers?\b(?<!\b(?:object|collection|field|property|array|static|module)\s+initiali[sz]ers?)" +
        @"|\bwriters?\b|\bexecution[-\s]+counters?\b" +
        @"|\bordinary\s+(?:projectile\s+|shot\s+)?places\b|\bplaces\s+in\s+(?:ascending|descending)\s+order\b" +
        @"|\bwrapped\s+into\s+(?:the|its)\s+[\w-]+\s+range\b" +
        @"|\bbranch(?:es|ed|ing)?\s+on\b|\bsigned[-\s]+(?:compares?|comparisons?|branch(?:es)?)\b" +
        @"|\b(?:roaring|damaged)[-\s]+flags?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The machine's storage and the repository's checks told for a value in prose (#2352), outside hardware owners and
    /// markup: a position, velocity, speed, coordinate, fraction or sub-pixel overflow (the 1/65536-pixel position
    /// overflow); a value's range named by its 1/N-pixel storage (far inside the 1/256-pixel range, wrapping within its
    /// signed 1/256-pixel range); a scan's, target, collision or solid counter (a hit clears both scan counters, the scan's
    /// solid counter completes); state that lies below what a clear reaches (the inventory, which lie below the state the
    /// setup clears); an allocation that clears, installs, calls, writes or sets something, and routes that allocate
    /// (Allocation clears the step, the collision routes allocate the trigger); and a check that runs to confirm a value
    /// (the radius check runs every update to confirm it). Core says what the game does: a shot's speed stays within its
    /// bound, a block stops the shot, what Samus keeps, an effect starts at its first shade, a block notifies its room
    /// object. English stays legal: a sum that overflows, a range in pixels (the 32-pixel range of the attack), a counter
    /// alone, lying below a ledge, an allocation alone and a check the game makes.
    /// </summary>
    private static readonly Regex StorageAndCheckNarrationProse = new(
        @"\b(?:position|velocity|speed|coordinate|fraction|sub-?pixel)\s+overflows?\b" +
        @"|\b(?:the|its|their|a)\s+(?:signed\s+)?1/[0-9]+-pixel\s+range\b" +
        @"|\bscan(?:'s)?\s+(?:[\w-]+\s+)?counters?\b|\b(?:target|collision|solid)\s+counters?\b" +
        @"|\blies?\s+below\b[^.;:!?]*?\bclears?\b" +
        @"|\ballocation\s+(?:clears?|installs?|calls?|writes?|sets?)\b|\broutes?\s+allocates?\b" +
        @"|\bchecks?\s+runs?\b[^.;:!?]*?\bto\s+(?:confirm|verify|prove)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A branch of the original's code told in prose (#2352), outside hardware owners and markup: this, that, either,
    /// neither, each, every, both or which branch (that branch restores the pose, both branches of the draw); a branch of
    /// a step, check or calculation (the deceleration branch of the speed calculation), except a room's state choice, a
    /// projectile or an afterburn; a branch taken, entered, selected, chosen, reached, bypassed, suppressed, covered,
    /// kept, preserved, modelled, consumed, run, executed, completed or used, with up to four words between (takes the dud
    /// branch, runs the first-form branch); a branch that returns, reads, writes, stores, sets, clears, restores,
    /// publishes, applies, bypasses, skips, leaves, performs, ends, accepts, moves, loads, sets up, samples, repeats,
    /// does, can or cannot, never, always or still, matters or keeps something (the landing branch moves the game); a
    /// branch order; and branching straight or directly to something. Core says what the game does in that case: a hit
    /// with no damage is a dud, the camera scrolls slowly, the game moves to Crateria. English stays legal: a room's state
    /// choice's branches, a projectile's or an afterburn's branches, an animation that branches to another (which
    /// branches to its release) and a branch alone.
    /// </summary>
    private static readonly Regex BranchNarrationProse = new(
        @"\b(?:this|that|either|neither|each|every|both|which|whichever)\s+branch(?:es)?\b(?![-\s]+(?:to|into|out|off|back)\b)" +
        @"|\bbranch(?:es)?\s+of\b(?!\s+(?:a|an|the|its)\s+(?:[\w'-]+\s+){0,2}?(?:state\s+choice|projectiles?|afterburns?)\b)" +
        @"|\b(?:takes?|took|taking|taken|enters?|entered|entering|selects?|selected|selecting|choos(?:e|es|ing)|chose|chosen" +
        @"|reach(?:es|ed|ing)?|bypass(?:es|ed|ing)?|suppress(?:es|ed|ing)?|covers?|retains?|keeps?|preserv(?:e|es|ed|ing)" +
        @"|models?|consumes?|runs?|executes?|completes?|uses?)\s+(?:the|its|a|an|their|his|her|that|this)\s+" +
        @"(?:[\w/'""“”-]+\s+){0,4}?branch(?:es)?\b" +
        @"|\bbranch(?:es)?\s+(?:returns?|reads?|writes?|stores?|sets?|clears?|restores?|publishes|applies|bypasses|skips" +
        @"|leaves|performs|ends|accepts|moves|loads|initiali[sz]es|samples|repeats|does|cannot|can|never|always|still" +
        @"|matters|keeps)\b" +
        @"|\bbranch\s+order\b|\bbranch(?:es)?\s+(?:directly|straight)\s+to\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Updates counted as the original's calls in frontend, effect, HUD, escape, progression, input, save and animation prose
    /// (#2362), outside hardware owners and markup: calls elapsed, made, still to skip or so far (Calls elapsed for the
    /// spawner, cinematic calls made so far); later, subsequent, remaining, earlier, repeated, consecutive, separate,
    /// eligible, typing, spawn, gameplay or audio calls (on its fourth, eighth and later calls, during the gameplay call);
    /// a list of calls run on (the fourth, eighth, ... calls); a thing's calls (the final character's call, a stage's
    /// calls); calls a thing lasts or is reloaded in (each frame lasting a period's calls, a reload in calls); a call that
    /// does something (the separate call that completes the fade, a call that decrements it); the calls a thing moves for;
    /// a call of the same, complete or shared update, owner, routine or step (calls the same update, call the same
    /// earthquake sound owner); the call that consumed, decremented, performed or completed something; and a call that saves
    /// where it resumes. Core counts updates and says what each does. English stays legal: a call to or into something, a
    /// call that returns to its caller, calls of a thing, and a later call's subject doing something (the producer first
    /// calls the setup, only the spawn calls it).
    /// </summary>
    private static readonly Regex CountedCallProse = new(
        @"\bcalls?\s+(?:elapsed|made|still|so\s+far)\b" +
        @"|\b(?:later|subsequent|remaining|earlier|repeated|consecutive|separate|eligible|typing|spawn|gameplay|audio)\s+calls?\b" +
        CallObjectFollows +
        @"|(?:\.\.\.|…)\s+calls\b" +
        @"|\b[\w-]+(?:'|’)s\s+calls?\b" + CallObjectFollows +
        @"|\blasting\s+(?:\S+\s+)?calls\b|\bin\s+calls\b" +
        @"|\b(?:a|the|each|every)\s+(?:[\w-]+\s+)?call\s+(?:that|which)\s+(?!returns?\b)\w+" +
        @"|\bthe\s+calls\s+(?!(?:to|into|of)\b)\w+" +
        @"|\bcalls?\s+the\s+(?:same|complete|shared)\s+(?:[\w-]+\s+){0,3}?(?:update|owner|routine|step|handler|coroutine)s?\b" +
        @"|\bcalls?\s+the\s+(?:[\w-]+\s+){0,3}?owner\b" +
        @"|\bthe\s+call\s+(?:consumed|consumes|decrements|decremented|performs|performed|makes|made|completes|completed|ends|ended)\b" +
        @"|\bcalls?\s+saves?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// What follows a word that may be the verb calls (#2362): an object, a preposition or a quantifier, in which case the
    /// word is the verb and not a counted call.
    /// </summary>
    private const string CallObjectFollows =
        @"(?!\s+(?:to|into|it|them|its|the|a|an|this|that|his|her|their|each|every|any|one|on)\b)";

    /// <summary>
    /// The machine's storage told for a value in frontend, enemy and progression prose (#2362), outside hardware owners and
    /// markup: a screen value's 0 through 255 range told with its wrap (its screen Y, 0 through 255, can wrap); the game's or
    /// the original's byte, word or wrapping counter, and a count that would have wrapped; the original's object handling
    /// and free object places; initialization parameters; an actor, object, enemy, projectile or place order told as
    /// ascending or descending; an actor being processed when another is allocated; a named storage bit (the
    /// process-off-screen bit, the reversed-once bit); the save memory's order and saved order; and a thing told as
    /// projectile-backed or parameter-selected. Core says what the game does: an object leaves before it slides off the
    /// screen, a list's later place updates after an earlier one, a flag is set. English stays legal: a range alone, a room
    /// object's place, a bit more and a hardware owner.
    /// </summary>
    private static readonly Regex StoredLayoutProse = new(
        @"\b0\s+through\s+255\b[^.;:!?]*\bwrap|\bwrap\w*\b[^.;:!?]*\b0\s+through\s+255\b" +
        @"|\bthe\s+(?:game|original)(?:'|’)s\s+(?:wrapping\s+)?(?:bytes?|words?|wrapping\s+counters?)\b" +
        @"|\bwould\s+have\s+wrapped\b" +
        @"|\bobject\s+handling\b|\bfree\s+object\s+places?\b" +
        @"|\binit(?:ialization)?\s+parameters?\b" +
        @"|\b(?:ascending|descending)\s+(?:actor|object|enemy|projectile|place)\s+order\b" +
        @"|\bbeing\s+processed\b" +
        @"|\b(?:\w+-)+\w+\s+bits?\b(?!\s+(?:of|more|less)\b)" +
        @"|\bsave(?:d)?\s+(?:memory(?:(?:'|’)s)?\s+)?order\b|\bsave\s+memory\b" +
        @"|\bprojectile-backed\b|\bparameter-selected\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original program and its host in frontend, input and save prose (#2362), outside hardware owners and markup: a
    /// coroutine (the main-gameplay coroutine, the save-pod coroutine); a handler chosen or selected and the handler of
    /// something (the handler of the category selected); a scan backward (the menu scans this list backward); a
    /// checksum; an emulator; the original reading, writing,
    /// storing, keeping, testing or checking something (as the original reads the current area); a match with the game's
    /// table; and engine definitions. Core says what the game does: a whole gameplay update runs, the category selected
    /// toggles the item, the latest button in a list wins, a save reads back whole, the room's area decides. English stays
    /// legal: a handler alone, scanning a room, an original picture and a hardware owner.
    /// </summary>
    private static readonly Regex OriginalProgramHostProse = new(
        @"\bcoroutines?\b" +
        @"|\bhandlers?\s+(?:is|are|was)\s+(?:chosen|selected|picked)\b|\bhandlers?\s+of\s+the\b" +
        @"|\bscans?\s+(?:[\w-]+\s+){0,3}?backward\b|\bbackward\s+scans?\b" +
        @"|\bchecksums?\b|\bemulators?\b" +
        @"|\bthe\s+original\s+(?:reads|read|writes|stores|keeps|tests|checks)\b" +
        @"|\bmatch(?:es)?\s+the\s+game(?:'|’)s\s+table\b" +
        @"|\bengine\s+definitions?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The machine's sixteen-bit coordinate arithmetic told for a distance in enemy prose (#2372), outside hardware owners
    /// and markup: a wrapped, wraparound or modular distance, difference, subtraction, displacement, position, proximity
    /// or comparison, with up to three words between (strict wrapped horizontal distance, the wrapped signed difference,
    /// the modular absolute difference, modular Y proximity, wrapped signed positions); a coordinate's wraparound
    /// (the spawn Y minus the radius with room-coordinate wraparound); and a move wrapping within the coordinate range.
    /// Core says how far apart two things lie and which way, and cites <c>CoordinateAxis</c>, the encoding owner that
    /// states the wrap once. English stays legal: a thing wrapped up or around, a turn's wrap, a distance alone and the
    /// encoding owner itself.
    /// </summary>
    private static readonly Regex WrappedDistanceProse = new(
        @"\b(?:wrapped|wraparound|modular)\s+(?:[\w-]+\s+){0,3}?(?:distances?|differences?|subtractions?|displacements?|positions?|proximity|comparisons?)\b" +
        @"|\b(?:room-)?coordinate\s+wraparound\b" +
        @"|\bwrapping\s+within\s+the\s+(?:[\w-]+\s+)?range\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original program's storage and calls told in enemy prose (#2372), outside hardware owners and markup: installing
    /// or loading art, with up to three words between (installs crawling art, installs hop art for its heading), where
    /// the program wrote an animation's pictures into a record; the game storing, checking or subtracting a value (the
    /// game subtracts one mark and checks whether the result is negative); a completion or draining call (including on
    /// the completion call, the draining calls each stage lasts); table multipliers; and a value of 0 through 255. Core
    /// says which animation starts, what the game does with the value, which update it happens on, and the value's
    /// meaning. English stays legal: installed art and loading installed art, which are the installation's content, a
    /// table, a multiplier and a range alone.
    /// </summary>
    private static readonly Regex ArtAndStorageNarrationProse = new(
        @"\b(?:installs?|installing|loads?|loading)\s+(?!installed\b)(?:[\w-]+\s+){0,3}?art\b" +
        @"|\bthe\s+game\s+(?:stores|stored|checks|checked|subtracts|subtracted)\b" +
        @"|\b(?:completion|draining)\s+calls?\b" +
        @"|\btable\s+multipliers?\b" +
        @"|\bof\s+0\s+through\s+255\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// An enemy's presence, contact or processing told as one of the original's stored properties in enemy, boss and
    /// world prose (#2377), outside hardware owners and markup: an intangible, invisible, hidden, deleted, hide-drawing,
    /// animating, off-screen, ignore-Samus, shot-, plasma- or beam-blocking, persists-on-contact, respawn, contact or solid
    /// property, alone or in a list (the intangibility property, the animating, intangibility and invisibility properties,
    /// both of his contact properties, the plasma-blocking enemy property). Core says what the enemy does: it is hidden or
    /// gone, it ignores Samus, it stops the Plasma Beam, it animates, it keeps updating off screen. English stays legal: a
    /// property of a thing, an enemy's other properties, a projectile's property set or mask, and a hardware owner.
    /// </summary>
    private static readonly Regex FlagPropertyProse = new(
        @"\b(?:intangib(?:le|ility)|tangib(?:le|ility)|invisib(?:le|ility)|visib(?:le|ility)|hidden|deleted|hide-drawing|animating" +
        @"|process-off-screen|off-screen|ignore-Samus(?:-collision)?|shot-blocking|plasma-blocking|beam-blocking|persists-on-contact" +
        @"|respawn|contact|solid)(?:\s+and\s+[\w-]+)?(?:,\s+[\w-]+)*(?:\s+enemy)?\s+propert(?:y|ies)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original program's steps and arithmetic told in enemy prose (#2377), outside hardware owners and markup: a
    /// post- or pre-decrement or increment (post-decrement timer semantics); a main-AI call or an AI invocation (the
    /// preceding main-AI call, once per relevant main-AI invocation); running a loop of the code (Runs the projectile-link
    /// loop); the handler's guard or check (retain the handler's literal guard); a signed wrapped comparison or value; and
    /// a strict modular range or distance. Core says which update a count runs out on, what each update does, what a step
    /// places and how far apart two things lie (<c>CoordinateAxis</c>). English stays legal: a handler alone, an
    /// animation's loop, a decrement or a call alone, and a hardware owner.
    /// </summary>
    private static readonly Regex ProgramStepNarrationProse = new(
        @"\b(?:post|pre)-?(?:de|in)crement\w*\b" +
        @"|\bmain-AI\s+(?:calls?|invocations?)\b|\bAI\s+invocations?\b" +
        @"|\bruns\s+the\s+(?:[\w-]+\s+){0,3}?loop\b" +
        @"|\bthe\s+handler(?:'|’)s\b" +
        @"|\bsigned\s+wrapped\b|\bstrict\s+modular\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The original program's and the port's machinery told in world, runtime and Samus prose (#2386), outside hardware
    /// owners and markup: an allocator (the room-object allocator, the projectile allocator's order); a renderer other
    /// than the host's (the death-state renderer, renderers draw the plane); a streamer, a thing streamed and a
    /// background, graphics, viewport, level, layer or plane streaming or streaming one of those (the background
    /// streamer, the viewport is streamed, background streaming, a streaming background plane); a saved state or saved
    /// game state; a matcher or a table match (the table matcher, this matcher); a thing translated (has no translated
    /// mover); and progression, defeat, controller, button or input bits. Core says what the game does: a room's
    /// free room-object places or enemy-projectile places, what the display draws, a door's scroll bringing the
    /// destination in, the game's saves, an input transition chosen, the movement a pose runs, the progression recorded,
    /// the bosses defeated, the buttons pressed. English stays legal: the host's renderer, the application's own (#1632),
    /// hair or jets streaming out and a thing streamed out, a position translated by an offset or to an origin, a save and a hardware owner.
    /// </summary>
    private static readonly Regex MachineryNarrationProse = new(
        @"\ballocators?\b|(?<!\bhosts?(?:'s|')?\s+)\brenderers?\b" +
        @"|\bstreamers?\b|\bstreamed\b(?!\s+(?:in|into|through|from|out|past|by|down|across|over|along)\b)" +
        @"|\b(?:background|graphics|artwork|viewport|tiles?|tilemap|level|layers?|planes?)[-\s]+streaming\b" +
        @"|\bstreaming[-\s]+(?:background|graphics|artwork|viewport|tiles?|tilemap|level|layers?|planes?)\b" +
        @"|\bsaved\s+(?:game\s+)?states?\b" +
        @"|\bmatchers?\b|\btable[-\s]+match(?:es|ing)?\b" +
        @"|\btranslated\b(?!\s+(?:to|by|into|onto|along|across|toward|towards)\b)" +
        @"|\b(?:progression|defeat|controller|button|input)[-\s]+bits?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A camel-case word of a declared name that names the original's execution counter (#2336): ExecutionCounter at
    /// any word boundary (<c>MainEnemyExecutionCounter</c>, <c>enemyMainExecutionCounter</c>). Core names the update
    /// count it means.
    /// </summary>
    private static readonly Regex ExecutionCounterNameWord = new(@"(?:^|_|(?<=[a-z0-9])(?=[A-Z]))[Ee]xecutionCounters?(?![a-z])", RegexOptions.CultureInvariant);

    /// <summary>
    /// Fix history told by an adverb of error (#2192), part of the review trail and read in every Core type: incorrectly,
    /// wrongly, erroneously or mistakenly (Testing it here incorrectly advanced the animation, Requiring a collision
    /// wrongly skipped the check). Core states what the game does; what an earlier version of the port got wrong is the
    /// repository's history. English stays legal: a correct, right or wrong thing the game shows (the wrong answer).
    /// </summary>
    private static readonly Regex ErrorAdverbProse = new(
        @"\b(?:incorrectly|wrongly|erroneously|mistakenly)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Fix history told as a fall-through of code (#2192), part of the review trail and read in every Core type: falls,
    /// falling or fall through, then ran, runs, made, left, caused, counted, skipped, let or reached (Falling through ran
    /// Samus movement). Core states where the update ends. English stays legal: Samus or a block falling through a
    /// platform, the floor or the room.
    /// </summary>
    private static readonly Regex FallThroughProse = new(
        @"\bfall(?:s|ing)?\s+through\s+(?:ran|runs?|made|left|caused|counted|skipped|let|reached)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Fix history told by an order an earlier version chose (#2192), part of the review trail and read in every Core
    /// type: a step done first, earlier, later, early, late, twice or again, then what it made, caused, let, left, broke,
    /// lost or produced (publishing first made the minimum read two). Core states the order the game keeps. English
    /// stays legal: an order alone (the HUD publishes first) and a consequence the game shows (the hit first makes the
    /// enemy flash).
    /// </summary>
    private static readonly Regex EarlierOrderProse = new(
        @"\b\w+ing\s+(?:it\s+|them\s+|this\s+|that\s+)?(?:first|earlier|later|early|late|twice|again)\s+" +
        @"(?:made|caused|let|left|broke|lost|produced)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Fix history told by what an alternative would strand (#2192), part of the review trail and read in every Core type:
    /// would, will, could or might strand, with otherwise between (omitting it would strand Metroid in custom hurt). Core
    /// states what the game does. English stays legal: a stranded thing the game has (a stranded X-ray beam).
    /// </summary>
    private static readonly Regex StrandingProse = new(
        @"\b(?:would|will|could|might)\s+(?:otherwise\s+)?strand\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A gameplay path told as a diagnostic's (#2192), read in every Core type: a diagnostic seam, load, step, use,
    /// request, path, run, magnitude, value, room, entry or branch, with up to one word between (the old diagnostic seam,
    /// direct diagnostic loads, its diagnostic pre-impact magnitude), a diagnostic of the room, frame, runtime or replay
    /// (the room diagnostic), and a slash joining one (direct/diagnostic step use). Core is the game: every path it holds
    /// is a gameplay path with a gameplay reason. English stays legal: a name, text or identity for diagnostics, which
    /// describes an error message or a host's display.
    /// </summary>
    private static readonly Regex DiagnosticPathProse = new(
        @"\bdiagnostic[-\s]+(?:[\w-]+\s+)?(?:seams?|loads?|steps?|uses?|requests?|paths?|runs?|magnitudes?|values?|rooms?" +
        @"|entr(?:y|ies)|branch(?:es)?)\b|\b(?:room|frame|runtime|replay)\s+diagnostics?\b|/diagnostic\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Debug narration of the repository's own tooling (#2192, narrowed by the owner's ruling under #2248 and #2265), read
    /// in every Core type: debug or non-debug before a session, script, tool, runner, build, host, harness, overlay,
    /// console, log, output, window, run, helper, seam, launcher, configuration, snapshot, capture or probe (fresh debug
    /// sessions, debug scripts, debug tools, a non-debug build), and constructing, building, creating, making, loading or
    /// spawning a debug room. Core is the game: it holds no path for a debug session and narrates none; the debugger in
    /// any word form is <see cref="HardwareProse"/>'s. English stays legal: the original game's own developer debug mode
    /// (the game's debug flag, debug-only save icons, the area map changes areas with debug enabled, its non-debug input)
    /// and a name that contains debug, which another rule reads.
    /// </summary>
    private static readonly Regex DebugProse = new(
        @"\b(?:non-?)?debug[-\s]+(?:sessions?|scripts?|tools?|tooling|runners?|builds?|hosts?|harness(?:es)?|overlays?" +
        @"|consoles?|logs?|logging|output|windows?|runs?|helpers?|seams?|launchers?|configurations?|snapshots?|captures?" +
        @"|probes?)\b" +
        @"|\b(?:construct|build|built|creat|mak|load|spawn)\w*\s+(?:a|an|the|its|their)\s+(?:[\w-]+\s+)?debug[-\s]+rooms?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Fix history told inside a clause (#2248, #2265), part of the review trail and read in every Core type: leaving
    /// this, that or it out (leaving this assignment out produces a silent attack), reducing, combining, collapsing,
    /// simplifying or merging something that then lost, broke or regressed something (reducing those branches to
    /// three/two loses one point), and doing only one thing that then made, caused, broke or lost something (reading only
    /// the uncharged row made Wave Shield disappear). Core states what the game does; what an earlier version of the port
    /// got wrong is the repository's history. English stays legal: a consequence told in the present (moving only left
    /// turns her) and leaving a room.
    /// </summary>
    private static readonly Regex ClauseFixHistoryProse = new(
        @"\bleaving\s+(?:this|that|these|those|it|them)\b[^.;]*?\bout\b" +
        @"|\b(?:reducing|combining|collapsing|simplifying|merging)\b[^.;]*?\b(?:loses|lost|broke|breaks|regressed)\b" +
        @"|\b\w+ing\s+only\b[^.;]*?\b(?:made|caused|broke|lost|regressed)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The port's narration of its C# construction (#2192), read in every Core type: C# then up to three words and a
    /// constructor, class, code, port, type, method, implementation, version, record, struct, field, property or loop
    /// (the C# room constructor, the C# class that happens to hold it). Core states what the game does, not how its code
    /// is laid out against the original. English stays legal: C# alone.
    /// </summary>
    private static readonly Regex CSharpConstructProse = new(
        @"(?<![\w#])C#\s+(?:[\w-]+\s+){0,3}?(?:constructors?|class(?:es)?|code|ports?|types?|methods?|implementations?" +
        @"|versions?|records?|structs?|fields?|propert(?:y|ies)|loops?)\b",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// A controller button's letter used as the name of a gameplay action in prose (#2113), outside hardware owners: a
    /// letter that is an action (A is Jump, X here is the configured Shoot action, X is the default shoot binding), an
    /// action that is a letter (Jump is A), a letter before an action's button, binding or action (the B run button) and a
    /// letter in brackets after an action (Shoot (X)). The letters are read case-sensitively. Core reads typed gameplay
    /// actions; which button the player binds to each is the host's, so Core names the action. English stays legal: a
    /// coordinate (X is the sample, Y is the tangent), an article (A held animation, A binding names a frame) and a letter
    /// alone.
    /// </summary>
    private static readonly Regex ButtonLetterActionProse = new(
        ButtonLetter + @"(?:\s+here)?\s+(?:is|means|stands\s+for)\s+(?:the\s+)?(?:(?:default|configured|normalized|usual)\s+)?" +
        ActionWord + @"\b|\b" + ActionWord + @"\s+is\s+(?:the\s+)?" + ButtonLetter +
        "|" + ButtonLetter + @"\s+" + ActionWord + @"[-\s]+(?:buttons?|bindings?|actions?)\b|\b" + ActionWord + @"\s*\(\s*" +
        ButtonLetter + @"\s*\)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>A controller button's letter, case-sensitive, standing alone: not a word's first letter, X-ray or a possessive.</summary>
    private const string ButtonLetter = @"(?-i:\b[ABXYLR]\b)(?![-'])";

    /// <summary>A gameplay action named in prose: jump, run, dash, shoot, fire, item select or cancel, aim or angle up or down.</summary>
    private const string ActionWord =
        @"(?:jump|run|dash|shoot|fire|item[-\s]+select|item[-\s]+cancel|aim[-\s]+(?:up|down)|angle[-\s]+(?:up|down))";

    /// <summary>
    /// A word run on to a number in a comment (#2018): a lower-case word of two or more letters followed directly by digits
    /// (<c>counter16</c>, <c>hold3</c>, <c>contains100</c>). It is a garbled numeral or a label of the original; Core
    /// spaces its numbers (a counter of sixteen, a hold of three frames). English stays legal: a file extension
    /// (<c>.ps1</c>) and a name Core's code spells, which is the code being described.
    /// </summary>
    private static readonly Regex GluedNumeral = new(@"(?<![.\w])[a-z]{2,}[0-9]+\b", RegexOptions.CultureInvariant);

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
    /// identities (<c>draygon_goop_spiky</c> is <c>DraygonGoopSpiky</c>), so a name whose Pascal-case form Core's code spells
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
            start.RegisterSyntaxTreeAction(tree => AnalyzeTree(tree, tree.IsGeneratedCode ? null : codeNames, tree.IsGeneratedCode));
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
    /// <remarks>
    /// A generated file's comments are the generator's, so they are not read for routine labels, and its text is the
    /// build's, so it is not read for the review trail: the SDK stamps the target framework's version and the commit into
    /// the assembly's generated attributes.
    /// </remarks>
    private static void AnalyzeTree(SyntaxTreeAnalysisContext context, Lazy<ImmutableHashSet<string>>? codeNames, bool generated)
    {
        SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);
        foreach (SyntaxTrivia trivia in root.DescendantTrivia(descendIntoTrivia: false))
        {
            if (!trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && !trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) &&
                !trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) &&
                !trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                continue;
            Report(context, trivia.ToFullString(), trivia.GetLocation(), trivia.Token.Parent, codeNames, generated, trivia);
        }
        foreach (SyntaxToken token in root.DescendantTokens())
        {
            if (token.IsKind(SyntaxKind.StringLiteralToken) || token.IsKind(SyntaxKind.InterpolatedStringTextToken) ||
                token.IsKind(SyntaxKind.Utf8StringLiteralToken) || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken) ||
                token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken))
                Report(context, token.ValueText, token.GetLocation(), token.Parent, codeNames: null, generated, comment: null);
        }
        if (generated)
            return;
        foreach (SyntaxNode node in root.DescendantNodes())
        {
            if (DeclaredName(node) is { } name &&
                (CallNameWord.IsMatch(name.ValueText) || ExecutionCounterNameWord.IsMatch(name.ValueText)) && !IsInHardwareOwner(node))
                context.ReportDiagnostic(Diagnostic.Create(Rule, name.GetLocation(), name.ValueText));
        }
    }

    /// <summary>
    /// The name <paramref name="node"/> declares (#2292): a type's, delegate's, method's, property's, event's, variable's,
    /// parameter's, enum member's, local function's, designation's, loop variable's or type parameter's; null for any
    /// other node.
    /// </summary>
    private static SyntaxToken? DeclaredName(SyntaxNode node) => node switch
    {
        BaseTypeDeclarationSyntax type => type.Identifier,
        DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier,
        MethodDeclarationSyntax method => method.Identifier,
        PropertyDeclarationSyntax property => property.Identifier,
        EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier,
        VariableDeclaratorSyntax variable => variable.Identifier,
        ParameterSyntax parameter => parameter.Identifier,
        EnumMemberDeclarationSyntax member => member.Identifier,
        LocalFunctionStatementSyntax localFunction => localFunction.Identifier,
        SingleVariableDesignationSyntax designation => designation.Identifier,
        ForEachStatementSyntax loop => loop.Identifier,
        TypeParameterSyntax typeParameter => typeParameter.Identifier,
        _ => null,
    };

    /// <summary>
    /// The paragraph a comment belongs to: a line comment with the line comments directly above and below it, up to a
    /// blank line or anything that is not a comment; any other comment alone.
    /// </summary>
    private static string CommentParagraph(SyntaxTrivia comment)
    {
        if (!comment.IsKind(SyntaxKind.SingleLineCommentTrivia))
            return comment.ToFullString();
        SyntaxTriviaList list = comment.Token.LeadingTrivia;
        int index = list.IndexOf(comment);
        if (index < 0)
        {
            list = comment.Token.TrailingTrivia;
            index = list.IndexOf(comment);
        }
        int first = ParagraphEdge(list, index, step: -1);
        int last = ParagraphEdge(list, index, step: 1);
        return string.Join(" ", list
            .Skip(first)
            .Take(last - first + 1)
            .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            .Select(trivia => trivia.ToFullString()));
    }

    /// <summary>
    /// The farthest line comment from <paramref name="index"/> in direction <paramref name="step"/> that joins its
    /// paragraph: one reached over whitespace and single line ends, never a blank line.
    /// </summary>
    private static int ParagraphEdge(SyntaxTriviaList list, int index, int step)
    {
        int edge = index;
        int lineEnds = 0;
        for (int i = index + step; i >= 0 && i < list.Count; i += step)
        {
            SyntaxTrivia neighbour = list[i];
            if (neighbour.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                edge = i;
                lineEnds = 0;
            }
            else if (neighbour.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                lineEnds++;
                if (lineEnds > 1)
                    break;
            }
            else if (!neighbour.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                break;
            }
        }
        return edge;
    }

    /// <summary>
    /// The Japanese and North American release <paramref name="text"/> names (<see cref="ReleaseNarrationProse"/>) when
    /// its paragraph, the comment's (<see cref="CommentParagraph"/>) or the literal itself, names no European release;
    /// null otherwise.
    /// </summary>
    private static string? ReleaseNarrationFinding(string text, SyntaxTrivia? comment) =>
        ReleaseNarrationProse.Match(text) is { Success: true } release &&
        !EuropeanRelease.IsMatch(comment is { } trivia ? CommentParagraph(trivia) : text)
            ? release.Value
            : null;

    /// <summary>
    /// Reports <paramref name="text"/> once when it cites the machine's provenance (<see cref="MachineProvenanceProse"/>), the
    /// original's call structure (<see cref="CallStructureProse"/>), its routine slots (<see cref="RoutineSlotProse"/>), its routine
    /// handlers (<see cref="RoutineHandlerProse"/>) or the C reimplementation
    /// (<see cref="ReimplementationProse"/>), in any type, or names the hardware outside a hardware-owning type. A comment carries
    /// <paramref name="codeNames"/>, against which its compound names are read as routine labels; a string literal does not,
    /// and is not read for labels or hexadecimal literals. <paramref name="generated"/> text is not read for the review
    /// trail (<see cref="ReviewTrailProse"/>, <see cref="RelocationHistoryProse"/>), fix history or debug narration.
    /// <paramref name="comment"/> is the comment the text is, whose paragraph says whether a release is set against the
    /// other (<see cref="ReleaseNarrationFinding"/>); null for a string literal.
    /// </summary>
    private static void Report(SyntaxTreeAnalysisContext context, string text, Location location, SyntaxNode? owner,
        Lazy<ImmutableHashSet<string>>? codeNames, bool generated, SyntaxTrivia? comment)
    {
        text = AllowanceMarker.Replace(text, string.Empty);
        string? word = MachineProvenanceProse.Match(text) is { Success: true } provenance ? provenance.Value
            : CallStructureProse.Match(text) is { Success: true } call ? call.Value
            : RoutineSlotProse.Match(text) is { Success: true } slot ? slot.Value
            : RoutineHandlerProse.Match(text) is { Success: true } handler ? handler.Value
            : ReimplementationProse.Match(text) is { Success: true } reimplementation ? reimplementation.Value
            : NumberedLabelProse.Match(text) is { Success: true } numberedLabel ? numberedLabel.Value
            : !generated && ReviewTrailProse.Match(text) is { Success: true } reviewTrail ? reviewTrail.Value
            : !generated && RelocationHistoryProse.Match(Markup.Replace(text, " ")) is { Success: true } relocation ? relocation.Value
            : !generated && ApprovalAndHistoryProse.Match(text) is { Success: true } approvalOrHistory ? approvalOrHistory.Value
            : !generated && ErrorAdverbProse.Match(text) is { Success: true } errorAdverb ? errorAdverb.Value
            : !generated && FallThroughProse.Match(text) is { Success: true } fallThrough ? fallThrough.Value
            : !generated && EarlierOrderProse.Match(text) is { Success: true } earlierOrder ? earlierOrder.Value
            : !generated && StrandingProse.Match(text) is { Success: true } stranding ? stranding.Value
            : !generated && ClauseFixHistoryProse.Match(text) is { Success: true } clauseFixHistory ? clauseFixHistory.Value
            : !generated && DiagnosticPathProse.Match(text) is { Success: true } diagnosticPath ? diagnosticPath.Value
            : !generated && DebugProse.Match(text) is { Success: true } debug ? debug.Value
            : !generated && CSharpConstructProse.Match(text) is { Success: true } cSharpConstruct ? cSharpConstruct.Value
            : IsInHardwareOwner(owner) ? null
            : CitedCheckProse.Match(text) is { Success: true } citedCheck ? citedCheck.Value
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
            : TrigonometryTableProse.Match(text) is { Success: true } trigonometryTable ? trigonometryTable.Value
            : GameNumberProse.Match(text) is { Success: true } gameNumber ? gameNumber.Value
            : PoseNumberProse.Match(text) is { Success: true } poseNumber ? poseNumber.Value
            : QueueLimitProse.Match(text) is { Success: true } queueLimit ? queueLimit.Value
            : SpritePartProse.Match(text) is { Success: true } spritePart ? spritePart.Value
            : MachineTableProse.Match(text) is { Success: true } machineTable ? machineTable.Value
            : MachineEntryProse.Match(text) is { Success: true } machineEntry ? machineEntry.Value
            : MovementNumberProse.Match(text) is { Success: true } movementNumber ? movementNumber.Value
            : PaletteProse.Match(Markup.Replace(text, " ")) is { Success: true } palette ? palette.Value
            : InterpreterRestProse.Match(Markup.Replace(text, " ")) is { Success: true } rest ? rest.Value
            : OriginalNumberingProse.Match(Markup.Replace(text, " ")) is { Success: true } originalNumbering ? originalNumbering.Value
            : CallerProse.Match(Markup.Replace(text, " ")) is { Success: true } caller ? caller.Value
            : SoundChipProse.Match(Markup.Replace(text, " ")) is { Success: true } soundChip ? soundChip.Value
            : ProgramStructureProse.Match(Markup.Replace(text, " ")) is { Success: true } programStructure ? programStructure.Value
            : DisplayEffectProse.Match(Markup.Replace(text, " ")) is { Success: true } displayEffect ? displayEffect.Value
            : ButtonLetterActionProse.Match(Markup.Replace(text, " ")) is { Success: true } buttonLetter ? buttonLetter.Value
            : StoredValueProse.Match(Markup.Replace(text, " ")) is { Success: true } storedValue ? storedValue.Value
            : PortNarrationProse.Match(text) is { Success: true } portNarration ? portNarration.Value
            : StoryEventNumberProse.Match(text) is { Success: true } storyEventNumber ? storyEventNumber.Value
            : EnemyPlaceSlotFinding(text, owner) is { } enemyPlaceSlot ? enemyPlaceSlot
            : FrameMapProse.Match(Markup.Replace(text, " ")) is { Success: true } frameMap ? frameMap.Value
            : ArithmeticBorrowProse.Match(Markup.Replace(text, " ")) is { Success: true } borrow ? borrow.Value
            : RepositoryCheckProse.Match(Markup.Replace(text, " ")) is { Success: true } repositoryCheck ? repositoryCheck.Value
            : TableNarrationProse.Match(Markup.Replace(text, " ")) is { Success: true } tableNarration ? tableNarration.Value
            : SixteenSixteenArithmeticProse.Match(Markup.Replace(text, " ")) is { Success: true } sixteenSixteen ? sixteenSixteen.Value
            : PairedArithmeticProse.Match(Markup.Replace(text, " ")) is { Success: true } paired ? paired.Value
            : RoutineHelperProse.Match(Markup.Replace(text, " ")) is { Success: true } routineHelper ? routineHelper.Value
            : ReleaseNarrationFinding(text, comment) is { } release ? release
            : SpeedRowProse.Match(text) is { Success: true } speedRow ? speedRow.Value
            : CitedVariableProse.Match(text) is { Success: true } citedVariable ? citedVariable.Value
            : SortOrderProse.Match(text) is { Success: true } sortOrder ? sortOrder.Value
            : ScrollTableProse.Match(text) is { Success: true } scrollTable ? scrollTable.Value
            : LoadedRowProse.Match(Markup.Replace(text, " ")) is { Success: true } loadedRow ? loadedRow.Value
            : StateExtensionProse.Match(text) is { Success: true } stateExtension ? stateExtension.Value
            : LiteralWriteProse.Match(text) is { Success: true } literalWrite ? literalWrite.Value
            : BackgroundObjectProse.Match(text) is { Success: true } backgroundObject ? backgroundObject.Value
            : PaletteRangeProse.Match(Markup.Replace(text, " ")) is { Success: true } paletteRange ? paletteRange.Value
            : ColourWriteProse.Match(Markup.Replace(text, " ")) is { Success: true } colourWrite ? colourWrite.Value
            : ScriptRecordProse.Match(text) is { Success: true } scriptRecord ? scriptRecord.Value
            : DroppedQuirkProse.Match(text) is { Success: true } droppedQuirk ? droppedQuirk.Value
            : MarkupMnemonicProse.Match(text) is { Success: true } markupMnemonic ? markupMnemonic.Value
            : RegisterReloadProse.Match(text) is { Success: true } registerReload ? registerReload.Value
            : CFieldProse.Match(Markup.Replace(text, " ")) is { Success: true } cField ? cField.Value
            : PackedProse.Match(text) is { Success: true } packed ? packed.Value
            : DisassemblyLabelProse.Match(text) is { Success: true } disassemblyLabel ? disassemblyLabel.Value
            : NumberedStateProse.Match(Markup.Replace(text, " ")) is { Success: true } numberedState ? numberedState.Value
            : RetailPopulationProse.Match(text) is { Success: true } retailPopulation ? retailPopulation.Value
            : PhysicalCopyProse.Match(text) is { Success: true } physicalCopy ? physicalCopy.Value
            : RoutineWalkerProse.Match(Markup.Replace(text, " ")) is { Success: true } routineWalker ? routineWalker.Value
            : BitArithmeticProse.Match(text) is { Success: true } bitArithmetic ? bitArithmetic.Value
            : ProgramFlowProse.Match(Markup.Replace(text, " ")) is { Success: true } programFlow ? programFlow.Value
            : PortDataProse.Match(text) is { Success: true } portData ? portData.Value
            : MachineStorageNarrationProse.Match(text) is { Success: true } machineStorage ? machineStorage.Value
            : RoutineWrapperProse.Match(Markup.Replace(text, " ")) is { Success: true } routineWrapper ? routineWrapper.Value
            : StateBitProse.Match(Markup.Replace(text, " ")) is { Success: true } stateBit ? stateBit.Value
            : NumberedStageProse.Match(Markup.Replace(text, " ")) is { Success: true } numberedStage ? numberedStage.Value
            : UpdateCallProse.Match(Markup.Replace(text, " ")) is { Success: true } updateCall ? updateCall.Value
            : MachineReadingProse.Match(Markup.Replace(text, " ")) is { Success: true } machineReading ? machineReading.Value
            : RetailCursorProse.Match(Markup.Replace(text, " ")) is { Success: true } retailCursor ? retailCursor.Value
            : PoseDataNarrationProse.Match(Markup.Replace(text, " ")) is { Success: true } poseData ? poseData.Value
            : RoutineNarrationProse.Match(Markup.Replace(text, " ")) is { Success: true } routineNarration ? routineNarration.Value
            : ArithmeticAndStorageSummaryProse.Match(Markup.Replace(text, " ")) is { Success: true } summary ? summary.Value
            : StorageAndCheckNarrationProse.Match(Markup.Replace(text, " ")) is { Success: true } storageAndCheck ? storageAndCheck.Value
            : BranchNarrationProse.Match(Markup.Replace(text, " ")) is { Success: true } branchNarration ? branchNarration.Value
            : CountedCallProse.Match(Markup.Replace(text, " ")) is { Success: true } countedCall ? countedCall.Value
            : StoredLayoutProse.Match(Markup.Replace(text, " ")) is { Success: true } storedLayout ? storedLayout.Value
            : OriginalProgramHostProse.Match(Markup.Replace(text, " ")) is { Success: true } originalProgram ? originalProgram.Value
            : WrappedDistanceProse.Match(Markup.Replace(text, " ")) is { Success: true } wrappedDistance ? wrappedDistance.Value
            : ArtAndStorageNarrationProse.Match(Markup.Replace(text, " ")) is { Success: true } artAndStorage ? artAndStorage.Value
            : FlagPropertyProse.Match(Markup.Replace(text, " ")) is { Success: true } flagProperty ? flagProperty.Value
            : ProgramStepNarrationProse.Match(Markup.Replace(text, " ")) is { Success: true } programStep ? programStep.Value
            : MachineryNarrationProse.Match(Markup.Replace(text, " ")) is { Success: true } machinery ? machinery.Value
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
        TileCharacter.Match(Markup.Replace(text, " ")) is { Success: true } character && !IsInTypeNamed(owner, TextTypeName)
            ? character.Value
            : null;

    /// <summary>
    /// The slot <paramref name="text"/>'s prose names as an enemy's, a projectile's or a bomb's place, outside its markup
    /// (<see cref="EnemyPlaceSlotProse"/>); null when it names none or lies in a type named for saves or files, whose slots
    /// are the game's save slots.
    /// </summary>
    private static string? EnemyPlaceSlotFinding(string text, SyntaxNode? owner) =>
        EnemyPlaceSlotProse.Match(Markup.Replace(text, " ")) is { Success: true } slot && !IsInTypeNamed(owner, SaveTypeName)
            ? slot.Value
            : null;

    /// <summary>Whether a node lies in a type whose name <paramref name="name"/> matches.</summary>
    private static bool IsInTypeNamed(SyntaxNode? node, Regex name)
    {
        for (SyntaxNode? current = node; current is not null; current = current.Parent)
        {
            if (current is BaseTypeDeclarationSyntax declaration && name.IsMatch(declaration.Identifier.ValueText))
                return true;
        }
        return false;
    }

    /// <summary>
    /// A comment's routine label, underscore-joined, set as code (<see cref="CodeElementName"/>) or compound, its memory
    /// symbol, or its hexadecimal literal; null when it has none.
    /// </summary>
    private static string? CommentOnlyFinding(string text, Lazy<ImmutableHashSet<string>> codeNames)
    {
        if (RoutineLabel.Match(text) is { Success: true } label)
            return label.Value;
        if (HexLiteral.Match(text) is { Success: true } literal)
            return literal.Value;
        foreach (Match code in CodeElementName.Matches(text))
        {
            string name = code.Groups["name"].Value;
            if (CompoundName.IsMatch(name))
                return name;
        }
        string prose = Markup.Replace(text, " ");
        foreach (Match glued in GluedNumeral.Matches(prose))
        {
            if (!codeNames.Value.Contains(glued.Value))
                return glued.Value;
        }
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
