using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks SME6454: Core comments and string literals name no hardware and cite no address (#1482).</summary>
internal static class CoreDocumentationVocabularyAnalyzerTests
{
    /// <summary>Each kind of text the rule reads, the owners it exempts, and the assemblies it skips.</summary>
    internal static void VerifyCoreDocumentationVocabularyAnalyzer()
    {
        var analyzer = new CoreDocumentationVocabularyAnalyzer();
        void Expect(string source, string? message = null) =>
            VerifySnippet(source, message is null ? null : CoreDocumentationVocabularyAnalyzer.DocumentationHardwareId, message, analyzer,
                assemblyName: CoreLayerAnalyzer.CoreAssembly);

        // Addresses and hardware words, in documentation, line comments, block comments and string literals.
        Expect("/// <summary>WRAM $07E1.</summary>\nclass P { }", "names the hardware or the debugger ('$07E1')");
        Expect("class P { /// <summary>$AA:C95E, the shared no-op movement step.</summary>\nvoid Step() { } }", "('$AA:C95E')");
        Expect("class P { void Run() { // copies the tiles to VRAM\n } }", "('VRAM')");
        Expect("class P { /* runs during the NMI */ }", "('NMI')");
        Expect("class P { string Name => \"OAM capacity\"; }", "('OAM')");
        Expect("class P { string Name => $\"{1} native words\"; }", "('native words')");
        Expect("class P { /// <summary>Reads the cartridge's banks.</summary>\nvoid Read() { } }", "('cartridge')");
        Expect("class P { // matches the 65C816 routine in the disassembly\n }", "('65C816')");
        // The processor's direct page, spaced or hyphenated, and the debugger in any word form (#1501).
        Expect("class P { /// <summary>The frame's direct page scratch word.</summary>\nint W => 0; }", "('direct page')");
        Expect("class P { void Run() { // keeps the direct-page copy\n } }", "('direct-page')");
        Expect("/// <summary>Excluded from debugger-state serialization.</summary>\nclass P { }", "('debugger')");
        Expect("class P { /* exposed so frame-by-frame debugging is useful */ }", "('debugging')");
        Expect("class P { string Name => \"Debugger watch\"; }", "('Debugger')");
        // Save RAM, RAM, the sprite and background layers, unsigned and byte citations, disassembly files, routine labels and
        // natively (#1528).
        Expect("class P { /// <summary>Copied into SRAM at save.</summary>\nvoid Save() { } }", "('SRAM')");
        Expect("class P { // the word lives in RAM\n }", "('RAM')");
        Expect("/// <summary>Draws the OBJ layer.</summary>\nclass P { }", "('OBJ')");
        Expect("class P { /* scrolls BG2 with the floor */ }", "('BG2')");
        Expect("class P { /// <summary>The routine at 88:DE43.</summary>\nvoid Run() { } }", "('88:DE43')");
        Expect("class P { // the timer starts at $1E\n }", "('$1E')");
        Expect("class P { /// <summary>Matches bank_87.asm.</summary>\nvoid Run() { } }", "('bank_87.asm')");
        Expect("class P { // as Handle_MapScrollArrows does\n }", "('Handle_MapScrollArrows')");
        Expect("class P { /* the game natively wraps it */ }", "('natively')");
        // A bank run on to its number, three-digit and bare hexadecimal values, and comment hex literals (#1558).
        Expect("class P { // the copy at native8C:D629\n }", "('native8C')");
        Expect("class P { /// <summary>Matches bank87's table.</summary>\nvoid Run() { } }", "('bank87')");
        Expect("class P { // the speed starts at $1A0\n }", "('$1A0')");
        Expect("class P { // F32B disables the layers\n }", "('F32B')");
        Expect("class P { // keeps 0x4000 set\n }", "('0x4000')");
        // The program's vocabulary, word widths, mnemonics, registers, palettes and priorities (#1558).
        Expect("class P { /// <summary>The handler address it jumps to.</summary>\nvoid Run() { } }", "('address')");
        Expect("class P { // the instruction pointer moves on\n }", "('pointer')");
        Expect("class P { /* the opcode table */ }", "('opcode')");
        Expect("class P { // read from the X register\n }", "('X register')");
        Expect("class P { /// <summary>Registers the spike's touch response, registered once.</summary>\nvoid Run() { } }");
        Expect("class P { // the 32 kHz clock\n }");
        Expect("class P { // the Assembly Line room's conveyor\n }");
        Expect("class P { int M(int a) => a; // cartridge-encoding: allow(HostPixelFormat) - the host's pixel layout\n }");
        Expect("class P { /// <summary>The PLM that spawns it.</summary>\nvoid Run() { } }", "('PLM')");
        Expect("class P { // the pre-instruction runs first\n }", "('pre-instruction')");
        Expect("class P { // in eight-bit accumulator mode\n }", "('eight-bit')");
        Expect("class P { // two extension words follow\n }", "('extension words')");
        Expect("class P { // shifted LSR ×4\n }", "('LSR')");
        Expect("class P { // passed through A\n }", "('through A')");
        Expect("class P { /// <summary>Drawn in palette seven.</summary>\nint F => 0; }", "('palette seven')");
        Expect("class P { // drawn at priority three\n }", "('priority three')");
        // The interpreter vocabulary of the original's streams (#1624): operands, instructions, numbered commands, and
        // Samus and animation commands, in documentation, line comments and string literals.
        Expect("class P { /// <summary>One cue of the Alcoon's animations, with its typed operands.</summary>\nint F => 0; }", "('operands')");
        Expect("class P { // a frame on a cue's operand reads the step\n }", "('operand')");
        Expect("class P { // the walking instructions store the delta\n }", "('instructions')");
        Expect("class P { // the instruction stream rewinds\n }", "('instruction')");
        Expect("class P { string Name => \"Samus animation instruction {0} selected no delay.\"; }", "('instruction')");
        Expect("class P { /// <summary>Samus command zero: installs the locked pair.</summary>\nvoid Run() { } }", "('Samus command')");
        Expect("class P { string Name => \"Samus commands test it.\"; }", "('Samus commands')");
        Expect("class P { // emitted by the spike-release animation command\n }", "('animation command')");
        Expect("class P { // special prospective command 9/10 snaps Samus to the rope\n }", "('command 9')");
        Expect("class P { // Command-six's empty beta skips animation\n }", "('Command-six')");
        Expect("class P { // controller commands 0/1/2/4 report themselves handled\n }", "('commands 0')");
        Expect("class P { // the connection pose commands 9 and 10 share one tail\n }", "('commands 9')");
        Expect("class P { /// <summary>Command twenty-four leaves her unable to stand.</summary>\nvoid Run() { } }", "('Command twenty')");
        // Arithmetic operands, the player's instructions, unnumbered commands and a count of the noun after it are English.
        Expect("class P { /// <summary>Compares the subtraction's operands; the operands of the comparison are signed.</summary>\nint F => 0; }");
        Expect("class P { // shows the player's instructions; instructions for the player follow the title\n }");
        Expect("class P { /// <summary>A data-management command row; the menu's commands run one at a time.</summary>\nint F => 0; }");
        Expect("class P { // the player's command one frame later restores the pair; the player commands Samus\n }");
        Expect("class P { /// <summary>The cue carries the values it acts on; the control setup restores Samus's control.</summary>\nint F => 0; }");
        Expect("[System.AttributeUsage(System.AttributeTargets.All)] sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; }\n" +
            "/// <summary>Evidence: the walking instructions store the delta; command seven reads it.</summary>\n[CompatibilityService(\"walk\")] class Walk { }");
        // Every other command of the original's streams and dispatchers, and numbered functions and selectors (#1624).
        Expect("class P { /// <summary>Latches the restore from the rainbow-stop command.</summary>\nint F => 0; }", "('command')");
        Expect("class P { // the unpause command restores the normal pair\n }", "('command')");
        Expect("class P { string Name => \"Chozo control command requires an active Samus actor.\"; }", "('command')");
        Expect("class P { // waits for the opening music commands\n }", "('commands')");
        Expect("class P { /// <summary>Named for the command list the game builds it with.</summary>\nint F => 0; }", "('command')");
        Expect("class P { // the special-air table ends with a copy-one command\n }", "('command')");
        Expect("class P { // Command-to-event conversion\n }", "('Command')");
        Expect("class P { /// <summary>Runs controller function zero.</summary>\nvoid Run() { } }", "('function zero')");
        Expect("class P { // Function 26 releases function 25 into the exit\n }", "('Function 26')");
        Expect("class P { // runs the main AI and functions one through six\n }", "('functions one')");
        Expect("class P { /// <summary>The population's direction selector two.</summary>\nint F => 0; }", "('selector two')");
        Expect("class P { /// <summary>Selector 0..3, indexing the pattern table.</summary>\nint F => 0; }", "('Selector 0')");
        // A menu's, a user's or a person's command, the verb after a person, and a count of the noun after a number are English.
        Expect("class P { /// <summary>A menu command; a file-select command row reads as slot C; a user's command.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>Handle controller-binding rows and commands.</summary>\nint F => 0; }");
        Expect("class P { // the player who commands Samus; the options commands\n }");
        Expect("class P { /// <summary>Checked by the HUD selector one layer above this; a function of the speed.</summary>\nint F => 0; }");
        // The original's dispatch named as a function without a number (#1624), in documentation, line comments and
        // string literals.
        Expect("class P { string Name => \"A Ceres door has no main function.\"; }", "('function')");
        Expect("class P { string Name => $\"Kraid body function {1} has ended.\"; }", "('function')");
        Expect("class P { /// <summary>Sets the eye's function when the fight starts.</summary>\nint F => 0; }", "('function')");
        Expect("class P { // the body functions the rainbow sequence runs\n }", "('functions')");
        Expect("class P { /* the record names the swinging function */ }", "('function')");
        // A mathematical function, C#'s own and writing one are English.
        Expect("class P { /// <summary>A function of each colour; a pure function of the velocity; thresholds and functions are copied from the game's arithmetic.</summary>\nint F => 0; }");
        Expect("class P { // inventing a function for them would only re-encode the ordering; sine without platform math functions\n }");
        Expect("class P { /// <summary>Runs a local function, a lambda function and a function delegate; defining a function is the code's.</summary>\nint F => 0; }");
        // Storage and machine vocabulary #1558 left out (#1611).
        Expect("class P { /// <summary>Kept in variable E.</summary>\nint F => 0; }", "('variable E')");
        Expect("class P { // extended word 08 holds it\n }", "('extended word')");
        Expect("class P { // init1 packs two unrelated bytes\n }", "('init1')");
        Expect("class P { /// <summary>The level words of the room.</summary>\nint F => 0; }", "('level words')");
        Expect("class P { // the six-byte input record\n }", "('six-byte')");
        Expect("class P { // keep the high bit raw\n }", "('high bit')");
        Expect("class P { // the random-number word and the carry\n }", "('random-number word')");
        Expect("class P { // paints colors 1-15 white\n }", "('colors 1-15')");
        Expect("class P { // palette handlers one and six run here\n }", "('palette handlers')");
        Expect("class P { // Handler seven owns them\n }", "('Handler seven')");
        Expect("class P { // the animation tile transfers run\n }", "('tile transfers')");
        Expect("class P { // sprite character 0\n }", "('sprite character')");
        Expect("class P { /// <summary>Width of each page in tile cells.</summary>\nint F => 0; }", "('tile cells')");
        Expect("class P { // a 32-by-32 page\n }", "('32-by-32 page')");
        Expect("class P { // the register shadows hold it\n }", "('register shadows')");
        Expect("class P { // the liquid registers move\n }", "('liquid registers')");
        Expect("class P { // kept in work memory\n }", "('work memory')");
        Expect("class P { // stops at scanline 224\n }", "('scanline 224')");
        Expect("class P { // the wait branches on the carry it returns\n }", "('on the carry')");
        Expect("class P { // leaves the carry flag set\n }", "('carry flag')");
        Expect("class P { /// <summary>Draygon's carry spiral; the carry orbit grows as Ridley's carry ends.</summary>\nint F => 0; }");
        // The same terms hyphenated (#1632): a corpse schedule in the "work-memory heap" was read as game prose.
        Expect("class P { /// <summary>The game keeps every schedule in one shared work-memory heap.</summary>\nint F => 0; }", "('work-memory')");
        foreach (string term in new[] { "unrelated-bytes", "level-word", "speed-word", "high-byte",
            "low-bit", "carry-flag", "colour-15", "palette-handler", "handler-seven", "visor-colour", "tile-transfer",
            "sprite-character", "transfer-list", "tile-cell", "tilemap-page", "32-by-32-page", "register-shadow",
            "shadow-register", "staging-surface", "transfer-residue", "scanline-224" })
            Expect($"class P {{ // the {term} stays as it was\n }}", $"('{term}')");
        // Game prose using the same words is not storage vocabulary.
        Expect("class P { /// <summary>Samus carries a variable speed; the carry over to the next room keeps it.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>The Speed Booster residue stays; the eight colours fade.</summary>\nint F => 0; }");
        // The processor's operations and the program's record vocabulary #1611 left out (#1632).
        Expect("class P { // Carry is set only for count two\n }", "('Carry is set')");
        Expect("class P { /// <summary>Reports carry when the walk ends.</summary>\nint F => 0; }", "('Reports carry')");
        Expect("class P { // the wait returns the carry when it ends\n }", "('returns the carry')");
        Expect("class P { // adds the speed with carry\n }", "('with carry')");
        Expect("class P { // moves bit7 to carry\n }", "('bit7')");
        Expect("class P { /// <summary>Whether Draygon swoops: the sample's bit zero.</summary>\nint F => 0; }", "('bit zero')");
        Expect("class P { // bits eight to thirteen place it\n }", "('bits eight')");
        Expect("class P { // handler bit 2 stays set\n }", "('bit 2')");
        Expect("class P { /// <summary>Extra word two holds the speed.</summary>\nint F => 0; }", "('Extra word')");
        Expect("class P { // Family-extra variable zero counts the hops\n }", "('Family-extra variable')");
        Expect("class P { // the extended variable zero\n }", "('extended variable')");
        Expect("class P { // Extension variable four\n }", "('Extension variable')");
        Expect("class P { // Extra enemy word four\n }", "('Extra enemy word')");
        Expect("class P { /// <summary>The slot's enemy words.</summary>\nint F => 0; }", "('enemy words')");
        Expect("class P { // kept in variables 0C/0D\n }", "('variables 0C')");
        Expect("class P { // Variable 10 counts down\n }", "('Variable 10')");
        Expect("class P { // the family-extras are cleared\n }", "('family-extras')");
        Expect("class P { /// <summary>The walk routine.</summary>\nvoid Walk() { } }", "('routine')");
        Expect("class P { string Name => \"The demo calls no routine.\"; }", "('routine')");
        Expect("class P { // rejects a zero sprite map\n }", "('sprite map')");
        Expect("class P { /// <summary>The empty projectile spritemap.</summary>\nint F => 0; }", "('spritemap')");
        Expect("class P { // the species header lists it\n }", "('header')");
        Expect("class P { // translated from the original\n }", "('translated from')");
        Expect("class P { // not another untranslated arm\n }", "('untranslated')");
        Expect("class P { // a direct translation keeps it\n }", "('direct translation')");
        Expect("class P { // toriz_var_00 holds the step\n }", "('toriz_var_00')");
        Expect("class P { /// <summary>Set while palette_change_num counts.</summary>\nint F => 0; }", "('palette_change_num')");
        // The game's own carrying, a little bit, records' game prose, heads and geometric translation are the game's.
        Expect("class P { /// <summary>Draygon carries Samus; the carry spiral grows and Ridley keeps the carry anchor.</summary>\nint F => 0; }");
        Expect("class P { // the conveyor carries Samus; the platform's carry measures the step\n }");
        Expect("class P { // the grab starts the carry on the grabbing update\n }");
        Expect("class P { // the adds carry the fractions into whole steps\n }");
        Expect("class P { /// <summary>Samus slides a bit; the shot drifts a bit two pixels lower.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>The extra missiles; an enemy's word of warning; a variable amount.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>Heading left, the head turns ahead; the headroom stays; Phantoon routinely waits.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>The turning floor's translation; translates the whole pixels by an offset.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>The scene is already translated to its origin; translational motion stops.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>Samus's energy transfer; the sand heap registers the hit at the original height.</summary>\nint F => 0; }");
        // The program's working memory and the port's runtime set against it (#1632): physical slots, host slots,
        // exceptions, numbers and notions, memset and scratch values.
        Expect("class P { // the wings occupy the next physical slot\n }", "('physical slot')");
        Expect("class P { /// <summary>Owns physical enemy slots.</summary>\nint F => 0; }", "('physical enemy slots')");
        Expect("class P { // the boss writes physical projectile slots 14 to 17\n }", "('physical projectile slots')");
        Expect("class P { /// <summary>Runs the physical-slot follow-up.</summary>\nvoid Run() { } }", "('physical-slot')");
        Expect("class P { string Name => \"The ghost requires physical slot zero.\"; }", "('physical slot')");
        Expect("class P { // request zero lands in host slot 17\n }", "('host slot')");
        Expect("class P { /// <summary>Turns the fatal shot into a host exception.</summary>\nvoid Run() { } }", "('host exception')");
        Expect("class P { // a negative host number loses the underflow\n }", "('host number')");
        Expect("class P { // a friendlier host notion of submerged\n }", "('host notion')");
        Expect("class P { // the post-memset marker keeps it\n }", "('memset')");
        Expect("class P { /// <summary>Provides scratch state and the room's scroll.</summary>\nint F => 0; }", "('scratch')");
        Expect("class P { // resets the scratch timer afterwards\n }", "('scratch')");
        Expect("class P { // instead of treating queue scratch as a link\n }", "('scratch')");
        // Physical geometry, the host that presents the game and its numbered frames, and starting over are not working memory.
        Expect("class P { /// <summary>The body's physical components and physical motion; the place is free.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>The host's save slots; the host persists them and reports an exception it catches.</summary>\nint F => 0; }");
        Expect("class P { // the first host frame on which the wait expires; the hosts' save list\n }");
        Expect("class P { /// <summary>Queues the room's music from scratch; the claw leaves scratch marks.</summary>\nint F => 0; }");
        // The port's host set against the original (#1632): the simulation's state and arithmetic named as the host's, the
        // provenance compounds, and "this host", in comments and string literals.
        foreach (string noun in new[] { "enum", "integer", "flags", "timer", "RNG", "random", "record", "field", "latch",
            "guard", "mask", "names", "equivalents", "storage", "state", "list", "lock", "projection", "table", "constant",
            "sprite", "animation", "reading", "physics", "ordering", "result", "no-ops", "error", "loop", "vector", "pose",
            "phase", "interaction", "signed", "movement", "gravity", "jump", "knockback", "palette", "handler", "death",
            "scheduler", "allocation", "bomb", "area", "aiming", "smoothing", "floating-point" })
            Expect($"class P {{ // the shot keeps the host {noun} as it was\n }}", $"('host {noun}')");
        foreach (string compound in new[] { "host-side", "host-only", "host-readable", "host-visible", "host-friendly",
            "host-authored", "host-facing" })
            Expect($"class P {{ /// <summary>No {compound} copy survives.</summary>\nint F => 0; }}", $"('{compound}')");
        Expect("class P { /// <summary>Host-readable form of the close-collision flag.</summary>\nbool F => false; }", "('Host-readable')");
        Expect("class P { /// <summary>Host latch preventing repeat entry.</summary>\nbool F => false; }", "('Host latch')");
        Expect("class P { // In this host those mean animation processing\n }", "('this host')");
        Expect("class P { // the outer owner is not part of this gameplay host\n }", "('this gameplay host')");
        Expect("class P { string Name => \"The host enum rejects it.\"; }", "('host enum')");
        // The application that runs the game is a host: as an actor, possessive or plural, with its presentation, services
        // and ownership compounds, and a ghost is not a host.
        Expect("class P { /// <summary>The host binds the artwork; a host plays audio, and the host's state is its own.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>Host-owned, host-installed, host-bound, host-supplied and host-configured content.</summary>\nint F => 0; }");
        Expect("class P { // host PCM at the host rate, host pixels, host artwork, host options, host elapsed time, host diagnostics\n }");
        Expect("class P { // the host renderer, a host compositor, one host call and the hosts' import format\n }");
        Expect("class P { /// <summary>The ghost flag and the ghost's state.</summary>\nint F => 0; }");
        // The display path's hardware vocabulary (#1632): tile characters, video and display memory, and per-line tables, in
        // documentation, line comments and string literals.
        Expect("class P { /// <summary>The turning floor's map; its characters are the room tileset's.</summary>\nint F => 0; }", "('characters')");
        Expect("class P { // the game's crumble also loads the debris character before the fragments\n }", "('character')");
        Expect("class P { /// <summary>Shows nothing rather than pause character zero, the visible glyph.</summary>\nint F => 0; }", "('character zero')");
        Expect("class P { string Name => \"The HUD characters are missing.\"; }", "('characters')");
        Expect("class P { /// <summary>The narration types its text; the HUD characters stay.</summary>\nint F => 0; }", "('characters')");
        Expect("class TextureCache { // its characters are loaded first\n }", "('characters')");
        Expect("class P { // copies the picture into video memory\n }", "('video memory')");
        Expect("class P { /// <summary>Pause owns no display memory.</summary>\nint F => 0; }", "('display memory')");
        Expect("class P { string Name => \"Out of graphics memory.\"; }", "('graphics memory')");
        Expect("class P { /// <summary>The phase of the lava background per-line table.</summary>\nint F => 0; }", "('per-line table')");
        Expect("class P { // the haze's per-line fixed-colour table\n }", "('per-line fixed-colour table')");
        Expect("class P { // fills the scanline table\n }", "('scanline table')");
        // Text characters, a type of text, people, markup, a per-line window or scroll and other memory are not the display's.
        Expect("class P { /// <summary>Types the warning one character at a time; the typed characters and the message's characters wait.</summary>\nint F => 0; }");
        Expect("class P { // a two-update character delay, the character cadence and the next character\n }");
        Expect("class P { /// <summary>Width of one text character; the letters' characters and the narration characters.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>Shows <paramref name=\"character\"/> at its place.</summary>\nvoid Show(int character) { } }");
        Expect("class EndingTypewriter { /// <summary>The installed characters.</summary>\nint F => 0; }");
        Expect("class IntroTextCell { // the cell's characters\n }");
        Expect("class P { // the game's characters and every playable character; characters who speak\n }");
        // A per-line effect is #2089's (DisplayEffectProse); a per-line scroll or window stays the effect itself.
        Expect("class P { /// <summary>The room's per-line window and per-line scroll run first; the credits table and the memory of the room.</summary>\nint F => 0; }");
        // The processor's arithmetic and the original's display and sound records (#1726): underflows, sign tests and sign
        // bits, testing and comparing by a sign, tile attributes and values, per-line effect objects and voices keyed on, in
        // documentation, line comments and string literals.
        Expect("class P { /// <summary>Waits for signed timer underflow, then locks Samus.</summary>\nint F => 0; }", "('underflow')");
        Expect("class P { // the timer seeded at 64 underflows on call 65\n }", "('underflows')");
        Expect("class P { string Name => \"The bounce counter underflowed.\"; }", "('underflowed')");
        Expect("class P { // The sign test accepts zero and expires only after the count passes it\n }", "('sign test')");
        Expect("class P { // Both boundaries are signed tests of the difference\n }", "('signed tests')");
        Expect("class P { // both stay far below the word's sign bit\n }", "('sign bit')");
        Expect("class P { /// <summary>Kept as the wrapped-sign test, rather than an ordered comparison.</summary>\nint F => 0; }", "('sign test')");
        Expect("class P { // the sign-check keeps it\n }", "('sign-check')");
        Expect("class P { /// <summary>A step that leaves the first row (the game tests its sign) restarts.</summary>\nint F => 0; }", "('tests its sign')");
        Expect("class P { // The death explosions decrement before testing the sign\n }", "('testing the sign')");
        Expect("class P { // The comparison tests only the sign of the wrapped difference\n }", "('tests only the sign')");
        Expect("class P { /// <summary>The patrol compares it by the sign of the wrapped difference.</summary>\nint F => 0; }", "('compares it by the sign')");
        Expect("class P { // The game caps the speed by the sign of its difference from the cap\n }", "('caps the speed by the sign')");
        Expect("class P { // stores the moved-back place and branches on its sign\n }", "('branches on its sign')");
        Expect("class P { /// <summary>Supported font layouts and tile attributes for ending text.</summary>\nint F => 0; }", "('tile attributes')");
        Expect("class P { // the frames the shooting stars' tile values give attributes for\n }", "('tile values')");
        Expect("class P { // each tile's attributes stay\n }", "('tile's attributes')");
        Expect("class P { string Name => \"Tile attribute out of range.\"; }", "('Tile attribute')");
        Expect("class P { // Both spawned per-line effect objects run their first step\n }", "('per-line effect objects')");
        Expect("class P { /// <summary>X-ray per-line display effect object phases.</summary>\nint F => 0; }", "('per-line display effect object')");
        Expect("class P { // a fresh key-on restarts the note\n }", "('key-on')");
        Expect("class P { // each tick keys on the voice it starts\n }", "('keys on the voice')");
        Expect("class P { string Name => \"The voice stays keyed on.\"; }", "('voice stays keyed on')");
        // A value's sign as its direction, a key on disk, keying a binding or a table on a name, a per-line scroll and the
        // game's blocks and pictures are English (a per-line effect is #2089's).
        Expect("class P { /// <summary>The speed's sign selects the animation; the sign of the difference picks the side; a sign-inverted mirror.</summary>\nint F => 0; }");
        Expect("class P { // the percentage's digits and percent sign; the turn relies on the sign of the speed\n }");
        Expect("class P { /// <summary>Counts down until the countdown passes zero, then reads as below zero.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>Renaming a frame never changes a key on disk; installed frame bindings key on, so identities stay.</summary>\nint F => 0; }");
        Expect("class P { // the table is keyed on the name; bindings key on the identity\n }");
        Expect("class P { /// <summary>The room's heat wave; the effect's per-line scroll; the falling tile's picture.</summary>\nint F => 0; }");
        Expect("[System.AttributeUsage(System.AttributeTargets.All)] sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; }\n" +
            "/// <summary>Evidence: the countdown's signed underflow; the sign test of the wrapped difference.</summary>\n[CompatibilityService(\"countdown\")] class Countdown { }");
        // A compound name in a comment is a routine label unless Core's code spells it (#1558).
        Expect("class P { // as EnemyMain does\n }", "('EnemyMain')");
        Expect("class P { // set by InitAI\n }", "('InitAI')");
        Expect("class P { int SamusPose => 0; // SamusPose holds the pose\n }");
        Expect("class P { int Pose(int frameCount) => frameCount; // frameCount counts frames\n }");
        Expect("class P { /// <summary>See <see cref=\"Pose\"/>.</summary>\nint Pose => 0; }");
        // A compound name set as code is a routine label even when Core spells it, since Core cites its own code with see
        // cref (#1794); an expression, a one-word name, a cited member and a hardware owner's code element are not read.
        Expect("class P { int MoveSamusWithDraygon() => 0; /// <summary>Runs <c>MoveSamusWithDraygon</c> after enemy AI.</summary>\nint F => 0; }",
            "('MoveSamusWithDraygon')");
        Expect("class P { int CalculateAngleFromXY() => 0; /// <summary>Integer octant form of <c>CalculateAngleFromXY()</c>.</summary>\nint F => 0; }",
            "('CalculateAngleFromXY')");
        Expect("enum Power { UpRight }\nclass P { /// <summary>Drawn as <c>Power.UpRight</c>.</summary>\nint F => 0; }", "('Power.UpRight')");
        Expect("class P { int InitialAngle => 0; /// <summary>Turns to <c>InitialAngle - 32</c>; the <c>Boyon</c> enemy; <see cref=\"InitialAngle\"/>.</summary>\nint F => 0; }");
        Expect("enum Power { UpRight }\nclass P { /// <summary>Installed as <c>\"Power.UpRight\"</c>.</summary>\nint F => 0; }");
        // Hex literals and compound names are read only in comments.
        Expect("class P { string Name => \"0x10 SpeedEcho\"; }");
        // Routine labels are read only in comments: a string literal's underscore-joined frame name is a presentation key.
        Expect("class P { string Frame => \"mother_brain_body_initial\"; }");
        // A lower-case frame name whose Pascal-case form is a Core identity is a presentation key, not a symbol (#1632).
        Expect("enum Frame { DraygonFrame12 }\nclass P { /// <summary>The draygon_frame_12 picture.</summary>\nint F => 0; }");
        // Identifiers are not prose, and a word that only begins the same is not the term.
        Expect("class P { /// <summary>The ramp, the object and the background fade up; time is 12:30.</summary>\nint Ram => 0; }");
        Expect("/// <summary>The addressee opens the pointed door; 1200 frames pass.</summary>\nclass P { }");
        Expect("class P { bool Debugger => false; int DirectPage => 0; }");
        // Debug itself is debug narration (#2192), read below.
        Expect("/// <summary>The cursor moves directly to the next page; the grapple selection stays.</summary>\nclass P { }");
        // Outside Core the rule reads nothing.
        VerifySnippet("/// <summary>A debugger-visible direct page word at WRAM $07E1.</summary>\nclass P { }", analyzer: analyzer);
        // An allowance marker is enforcement syntax; its own words are not read.
        Expect("class P { int M(int a) => a; // cartridge-encoding: allow(Wrap) - a signed delta word\n }");
        // Game prose is not hardware: whole words only.
        Expect("/// <summary>Samus jumps from the riverbank toward Kraid's chamber.</summary>\nclass P { }");
        Expect("class P { string Name => \"Brinstar\"; }");
        // Compatibility services and encoding owners may describe the hardware they reproduce.
        Expect("[System.AttributeUsage(System.AttributeTargets.All)] sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; }\n" +
            "/// <summary>Reproduces the NMI frame counter at $05B5.</summary>\n[CompatibilityService(\"frame counter\")] class Clock { }");
        Expect("[System.AttributeUsage(System.AttributeTargets.All)] sealed class EncodingOwnerAttribute(string format) : System.Attribute { public string Format { get; } = format; }\n" +
            "[EncodingOwner(\"pose numbers\")] static class Numbers { /// <summary>The native pose number.</summary>\nstatic int N => 0; }");
        // The machine's provenance is read in every type, hardware owners included: native words and bit patterns, the
        // cartridge's routines and hexadecimal fixed-point values (#1743).
        const string EncodingOwner =
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class EncodingOwnerAttribute(string format) : System.Attribute { public string Format { get; } = format; }\n";
        const string CompatibilityService =
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; }\n";
        // A hardware owner may set a routine's name as code (#1794); only types outside the owners cite Core's code by cref.
        Expect(CompatibilityService + "/// <summary>Evidence: <c>GenerateRandomNumber</c> advances it.</summary>\n[CompatibilityService(\"random\")] class Random { }");
        Expect(EncodingOwner + "[EncodingOwner(\"fixed point\")] readonly struct F { /// <summary>Gets the signed native word of the value.</summary>\nint Raw => 0; }",
            "('native word')");
        Expect(EncodingOwner + "[EncodingOwner(\"fixed point\")] readonly struct F { /// <summary>The whole pixels as their unsigned native bit pattern.</summary>\nint W => 0; }",
            "('native bit pattern')");
        Expect(CompatibilityService + "/// <summary>Replaces one half, as cartridge routines that store one half do.</summary>\n[CompatibilityService(\"halves\")] class Halves { }",
            "('cartridge routines')");
        Expect(CompatibilityService + "[CompatibilityService(\"dash\")] class Dash { // the dash adds exactly 0.1000 per frame\n }", "('0.1000')");
        Expect("class P { /// <summary>The boost holds the extra speed at 7.0000.</summary>\nint F => 0; }", "('7.0000')");
        Expect("class P { // the cap reads $1.8C00\n }", "('$1.8C00')");
        Expect("class P { string Name => \"Extra speed passed 2.0000.\"; }", "('2.0000')");
        // A decimal without a trailing zero, a version, a native thing that is no word and a routine that is not the
        // cartridge's are English.
        Expect("class P { /// <summary>Gravity adds 0.125, the speed decays by 0.0625 and sinks 0.1875 pixels.</summary>\nint F => 0; }");
        Expect("class P { // a signed displacement beside a speed, in version 0.9.0\n }");
        Expect(EncodingOwner + "[EncodingOwner(\"words\")] static class W { /// <summary>The native pose number; the game's clamping routine; a word.</summary>\nstatic int N => 0; }");
        // The original program's call structure is read in every type, hardware owners included (#1754): dispatchers, a
        // dispatcher index, callables and a routine's labels, in documentation, line comments and string literals.
        Expect("class P { /// <summary>Runs the elevator's main behaviour and its four-entry dispatcher.</summary>\nvoid Run() { } }", "('dispatcher')");
        Expect("class P { /// <summary>Dispatcher index zero, selected without a rider.</summary>\nint F => 0; }", "('Dispatcher')");
        Expect("class P { // the outer dispatchers repeat it\n }", "('dispatchers')");
        Expect("class P { string Name => \"Draygon part dispatcher received another family.\"; }", "('dispatcher')");
        Expect(CompatibilityService + "/// <summary>Evidence: the original bombed-special dispatcher indexes its table.</summary>\n[CompatibilityService(\"bomb\")] class Bomb { }",
            "('dispatcher')");
        Expect("class P { /// <summary>Torizo animation: special callable, blow up the gut.</summary>\nint F => 0; }", "('callable')");
        Expect("class P { /// <summary>Despite the inverted label, starts the leftward hop.</summary>\nint F => 0; }", "('inverted label')");
        Expect("class P { /// <summary>Always clears the still-art flag at the merge label.</summary>\nvoid Run() { } }", "('merge label')");
        Expect("class P { // Same merge-label detail as the moving helper\n }", "('merge-label')");
        Expect("class P { // The source label's branch name is reversed\n }", "('source label')");
        Expect(EncodingOwner + "[EncodingOwner(\"reactions\")] static class R { /// <summary>Read at the entry label.</summary>\nstatic int N => 0; }",
            "('entry label')");
        // The C reimplementation is read in every type (#1754), case-sensitively for the letter.
        Expect("class P { // Preserve the game's branch, not the upstream C rewrite's bugfix.\n }", "('upstream C')");
        Expect("class P { /// <summary>Do not use upstream C's category guard.</summary>\nint F => 0; }", "('upstream C')");
        Expect("class P { // the C port clears it first\n }", "('the C port')");
        Expect(CompatibilityService + "/// <summary>Kept as upstream C keeps it.</summary>\n[CompatibilityService(\"guard\")] class Guard { }", "('upstream C')");
        // A dispatched request, a callback, the labels the game shows and the verb are English, as are C# and a C that is
        // not that program.
        Expect("class P { /// <summary>Dispatches the sound request; the dispatch sends the event; the callback runs.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>The ENERGY label and the Exit label lit; the map's area labels; zero labels for reserves.</summary>\nint F => 0; }");
        Expect("class P { // the select map labels the area; the label shows; each label in its colours\n }");
        // The port's C# construction is another rule's (#2192).
        Expect("class P { // written in C#, it keeps it; slot C and button C; the upstream river\n }");
        // The original's data and number encodings outside hardware owners (#1754): decompression, colour indexes, two's
        // complement, unsigned compares and the 8.8 and 16.16 format names.
        Expect("class P { /// <summary>Active room's decompressed foreground.</summary>\nint F => 0; }", "('decompressed')");
        Expect("class P { // loaded by the initial artwork decompression\n }", "('decompression')");
        Expect("class P { string Name => \"Decompressing the room failed.\"; }", "('Decompressing')");
        Expect("class P { /// <summary>Installs the grey colours at the statue's grey colour index.</summary>\nint F => 0; }", "('colour index')");
        Expect("class P { // The writer receives color index 241\n }", "('color index')");
        Expect("class P { // the colour-indices stay\n }", "('colour-indices')");
        Expect("class P { /// <summary>Leftward speed is the exact two's complement of the speed.</summary>\nint F => 0; }", "('two's complement')");
        Expect("class P { // keeps the two's-complement form\n }", "('two's-complement')");
        Expect("class P { // and performs an unsigned compare\n }", "('unsigned compare')");
        Expect("class P { /// <summary>Branches on an unsigned comparison.</summary>\nint F => 0; }", "('unsigned comparison')");
        Expect("class P { /// <summary>The tube's signed 8.8 vertical velocity.</summary>\nint F => 0; }", "('8.8')");
        Expect("class P { // a signed 16.16 displacement beside an 8.8 speed, in version 0.9.0\n }", "('16.16')");
        Expect("class P { // the whole pixels promoted to 16.16.\n }", "('16.16')");
        Expect("class P { string Name => \"The product does not fit a 16.16 speed.\"; }", "('16.16')");
        // Hardware owners describe their encodings and the arithmetic they reproduce; an uncompressed file, a colour and its
        // index, a decimal, a version, an unsigned value and another complement are English.
        Expect(EncodingOwner + "[EncodingOwner(\"8.8 fixed-point number\")] static class E { /// <summary>An 8.8 value; the int overloads keep two's-complement sign.</summary>\nstatic int N => 0; }");
        Expect(EncodingOwner + "[EncodingOwner(\"palette\")] static class C { /// <summary>The colour index and the unsigned compare.</summary>\nstatic int N => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the game filled the level before decompressing the room.</summary>\n[CompatibilityService(\"terrain\")] class Terrain { }");
        Expect("class P { /// <summary>An uncompressed stereo WAV; the colour and its index; an unsigned value; the set's complement.</summary>\nint F => 0; }");
        Expect("class P { // gravity adds 8.25 pixels, the 18.8 seconds pass, in version 0.8.8 and release 2.16.16\n }");
        // The original's table positions and enemy record outside hardware owners (#1765): a menu index, a table index and
        // an enemy record, spaced or hyphenated, in documentation, line comments and string literals.
        Expect("class P { /// <summary>File-copy menu index 13: the destination receives the source's save.</summary>\nint F => 0; }",
            "('menu index')");
        Expect("class P { // Performs menu indices zero through two\n }", "('menu indices')");
        Expect("class P { /// <summary>Gets the facing-table index: zero for left.</summary>\nint F => 0; }", "('table index')");
        Expect("class P { // treating them as ordinary table indices lost the door\n }", "('table indices')");
        Expect("class P { /// <summary>The departure angles at two table indexes.</summary>\nint F => 0; }", "('table indexes')");
        Expect("class P { /// <summary>Copies the health into the live enemy record.</summary>\nint F => 0; }", "('enemy record')");
        Expect("class P { // These are four separate enemy records\n }", "('enemy records')");
        Expect("class P { string Name => \"The enemy-record copy failed.\"; }", "('enemy-record')");
        // A compatibility service may describe the enemy record its behaviour reads; a table indexed by a value, the menu's
        // index, an enemy's record, and a menu, table, index, enemy or record alone are English.
        Expect(CompatibilityService + "/// <summary>Evidence: the launch reads the sibling's enemy record.</summary>\n[CompatibilityService(\"sibling\")] class Sibling { }");
        Expect("class P { /// <summary>The drop-chance table indexed by the definition; the menu's index; an enemy's record of kills.</summary>\nint F => 0; }");
        Expect("class P { // the table and its index; the menu, the enemy and the record\n }");
        // The original's numbered routine and variable labels are read in every type (#1769), in any case.
        Expect("class P { // Func126 clears the colour effects\n }", "('Func126')");
        Expect("class P { /// <summary>Func139–142: rotate the shot.</summary>\nint F => 0; }", "('Func139')");
        Expect("class P { /// <summary>Func115 seeds var4 to 63.</summary>\nint F => 0; }", "('Func115')");
        Expect("class P { // the countdown seeds VAR4\n }", "('VAR4')");
        Expect("class P { string Name => \"func12 finished.\"; }", "('func12')");
        Expect(CompatibilityService + "/// <summary>Evidence: Func120 clears the flash.</summary>\n[CompatibilityService(\"flash\")] class Flash { }",
            "('Func120')");
        // The original's routine slots for Samus are read in every type (#1794): the timer/hack handler and gamma.
        Expect("class P { /// <summary>True while the escape handler is installed as Samus's timer/hack handler.</summary>\nint F => 0; }",
            "('/hack')");
        Expect("class P { // The movement handler calls this installed hack handler after movement\n }", "('hack handler')");
        Expect("class P { // before the timer hack, liquid animation and velocity cleanup\n }", "('timer hack')");
        Expect("class P { string Name => \"Draygon owner/hack handling requires the grab first.\"; }", "('/hack')");
        Expect("class P { /// <summary>Preserves grab gamma while a newer movement behaviour takes over.</summary>\nint F => 0; }", "('gamma')");
        Expect("class P { // The gamma initialization owns this shift\n }", "('gamma')");
        Expect(CompatibilityService + "/// <summary>Evidence: the shove's gamma runs first.</summary>\n[CompatibilityService(\"shove\")] class Shove { }",
            "('gamma')");
        // Another hack and a colour's gamma correction, curve or value are English.
        Expect("class P { /// <summary>Neutral-jump hacks; not a visual alpha hack; the gamma correction, gamma curve and gamma value.</summary>\nint F => 0; }");
        // The original's routine handlers are read in every type (#1828): a handler call and a music, sound, colour-effect,
        // timer, sprite or state handler, spaced or hyphenated.
        Expect("class P { /// <summary>Times this update processes the queue, including its handler call.</summary>\nint F => 0; }",
            "('handler call')");
        Expect("class P { // The box's last update makes two sound-handler calls\n }", "('sound-handler')");
        Expect("class P { /// <summary>The music requests waiting for the music handler.</summary>\nint F => 0; }", "('music handler')");
        Expect("class P { // Main gameplay runs the colour effect handler before Samus\n }", "('colour effect handler')");
        Expect("class P { string Name => \"The color-effect handler is disabled.\"; }", "('color-effect handler')");
        Expect("class P { /// <summary>Expiry installs Samus's drawing-only timer handler.</summary>\nint F => 0; }", "('timer handler')");
        Expect("class P { // drawn by the generic sprite handler\n }", "('sprite handler')");
        Expect("class P { /// <summary>The separately run state handlers.</summary>\nint F => 0; }", "('state handlers')");
        Expect("class P { // one sound effect handler pass\n }", "('sound effect handler')");
        Expect(CompatibilityService + "/// <summary>Evidence: the sound handler accepts one request.</summary>\n[CompatibilityService(\"sound\")] class Sound { }",
            "('sound handler')");
        // A handler named by the word before it, a handler pair and a dispatch point are read in every type too (#1855),
        // spaced or hyphenated, in documentation, line comments and string literals.
        Expect("class P { /// <summary>The movement handler runs after the input handler.</summary>\nint F => 0; }", "('movement handler')");
        Expect("class P { // the unable-to-stand handler keeps her crouched\n }", "('unable-to-stand handler')");
        Expect("class P { /// <summary>One Alcoon occupant: its own AI handlers.</summary>\nint F => 0; }", "('AI handlers')");
        Expect("class P { string Name => \"The room-object handler requires an active room.\"; }", "('room-object handler')");
        Expect("class P { // the switched-to handler accepted the item\n }", "('switched-to handler')");
        Expect("class P { // the knockback sub-handler runs first\n }", "('sub-handler')");
        Expect("class P { /// <summary>Runs Beetom's custom touch handler.</summary>\nint F => 0; }", "('touch handler')");
        Expect("class P { // restores the handler pair\n }", "('handler pair')");
        Expect("class P { /// <summary>Runs at the same palette-update dispatch point.</summary>\nint F => 0; }", "('dispatch point')");
        Expect(CompatibilityService + "/// <summary>Evidence: the HUD handler runs before the projectile handler.</summary>\n[CompatibilityService(\"hud\")] class Hud { }",
            "('HUD handler')");
        // A handler alone or of another kind, calling a handler and a call alone are English, as are a handler after an
        // article, determiner, pronoun, number or possessive, an unhandled-exception handler and a point; a dispatch alone is #2018's.
        Expect("class P { /// <summary>An exception handler; the event handlers; the grapple beam's handler; calling a handler; a call.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>Each handler; its handlers; two handlers; Kraid's handlers; an unhandled-exception handler; the next point.</summary>\nint F => 0; }");
        // C#'s Func delegate and a var declaration carry no number.
        Expect("class P { /// <summary>Takes a Func of the speed; var declarations stay; the variance holds.</summary>\nint F => 0; }");
        // The original's game state, load station and area numbers outside hardware owners (#1769), spaced or hyphenated,
        // in digits or words, in documentation, line comments and string literals.
        Expect("class P { // so the game state remains, with game state eight\n }", "('game state eight')");
        Expect("class P { // in this very same game-state-17 call\n }", "('game-state-17')");
        Expect("class P { /// <summary>Changes load station 18 to the gunship.</summary>\nint F => 0; }", "('load station 18')");
        Expect("class P { // selects Crateria station zero\n }", "('station zero')");
        Expect("class P { // That is why the station-eighteen gunship can carry Samus\n }", "('station-eighteen')");
        Expect("class P { /// <summary>Publishes area six for the new game.</summary>\nint F => 0; }", "('area six')");
        Expect("class P { // Bomb Torizo's area-zero orb\n }", "('area-zero')");
        Expect("class P { string Name => \"Areas 2 and 3 share it.\"; }", "('Areas 2')");
        Expect("class P { /// <summary>Norfair (area two) uses 300 frames.</summary>\nint F => 0; }", "('area two')");
        // A number that counts the noun after it, an area number past the game's eight areas, a room's or the beam's state by
        // number, a game state, station or area alone, and a hardware owner's description are English.
        Expect("class P { /// <summary>A save station two rooms away; an area two tiles wide; the area 450 to the right.</summary>\nint F => 0; }");
        Expect("class P { // the room's state 2; the beam's state two; the game state and the station; the areas\n }");
        Expect(CompatibilityService + "/// <summary>Evidence: the game state eight check.</summary>\n[CompatibilityService(\"pause\")] class Pause { }");
        // A pose by the original's number outside hardware owners (#1769): in hexadecimal, as a word, and a transition
        // between two hexadecimal numbers.
        Expect("class P { /// <summary>The fixed component turns at pose A/B.</summary>\nint F => 0; }", "('pose A')");
        Expect("class P { // poses 0A and 0B share it\n }", "('poses 0A')");
        Expect("class P { // the flashback locks Samus in pose two\n }", "('pose two')");
        Expect("class P { /// <summary>Pose zero's transitions overlap.</summary>\nint F => 0; }", "('Pose zero')");
        Expect("class P { // completes landing art's A5 -> 02 transition\n }", "('A5 -> 02')");
        Expect("class P { string Name => \"Samus took pose number 1C.\"; }", "('pose number 1C')");
        // A family's own frames by decimal ordinal, a count of the noun after a number, a pose alone, decimal arrows and an
        // encoding owner of pose numbers are English.
        Expect("class P { /// <summary>Pose 3 of the wave trail; poses 4 and 5 lower the arm; the pose two frames later.</summary>\nint F => 0; }");
        Expect("class P { // the pose holds; 10 -> 20 pixels; a pose a frame\n }");
        Expect(EncodingOwner + "[EncodingOwner(\"pose numbers\")] static class Poses { /// <summary>Pose A5 is the left landing.</summary>\nstatic int N => 0; }");
        // A sound queue named for the original's per-call limit outside hardware owners (#1769).
        Expect("class P { /// <summary>Queues the shot through the max-fifteen sound queue.</summary>\nint F => 0; }", "('max-fifteen')");
        Expect("class P { // the roar's queue (maximum six queued)\n }", "('maximum six queued')");
        Expect("class P { /// <summary>The cries' policy: at most six queued.</summary>\nint F => 0; }", "('at most six queued')");
        Expect("class P { /// <summary>Queued with a six-entry admission limit.</summary>\nint F => 0; }", "('six-entry admission limit')");
        Expect("class P { // queues the cursor sound with the six-request limit\n }", "('six-request limit')");
        Expect("class P { string Name => \"Over the 15-entry queue limit.\"; }", "('15-entry queue limit')");
        // A maximum or limit that names no queue position, a queue alone, and the compatibility service owning the
        // priorities are English.
        Expect("class P { /// <summary>The maximum speed; at most three steps; the queue and its limit; maximum-speed threshold.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the max-six queue admits at most six queued.</summary>\n[CompatibilityService(\"queue\")] class Queue { }");
        // A sprite part's stored placement and colour data outside hardware owners (#1769).
        Expect("class P { /// <summary>Its initializer colours it with palette0800.</summary>\nint F => 0; }", "('palette0800')");
        Expect("class P { // the spawn indexes interleaved (X-10h,Y-3Bh) pairs\n }", "('10h')");
        Expect("class P { /// <summary>Each record has one part, at X=508, Y=252.</summary>\nint F => 0; }", "('part, at X=5')");
        Expect("class P { // Part j=0..3 uses X=0 for even j\n }", "('Part j=0..3 uses X=0')");
        Expect("class P { /// <summary>Its motion masks X to nine bits.</summary>\nint F => 0; }", "('nine bits')");
        Expect("class P { // with nine-bit X wrap\n }", "('nine-bit')");
        // A position no part is placed by, a palette or a part alone, a word ending in h and a hardware owner are English.
        Expect("class P { /// <summary>Starts the goop path at X=-80, Y=384; the ship parts slow below Y=768.</summary>\nint F => 0; }");
        Expect("class P { // the shade and its part; each part's picture; the depth of 8 high; the 256th; the height\n }");
        Expect(EncodingOwner + "[EncodingOwner(\"sprite parts\")] static class Parts { /// <summary>One part, at X=508, in palette0800.</summary>\nstatic int N => 0; }");
        // The machine's display transfers and staging outside hardware owners (#1797).
        Expect("class P { /// <summary>Publishes the frame transfers before the latch.</summary>\nint F => 0; }", "('frame transfers')");
        Expect("class P { // the transfer queue holds the artwork\n }", "('transfer queue')");
        Expect("class P { string Name => \"The queued transfers overflowed.\"; }", "('queued transfers')");
        Expect("class P { // the general artwork follows the general transfer\n }", "('general transfer')");
        Expect("class P { // the foreground staging offset\n }", "('foreground staging')");
        Expect("class P { /// <summary>Copies the sprite list into the staging buffer.</summary>\nint F => 0; }", "('staging buffer')");
        Expect("class P { // an upward door stages the foreground five pixels lower\n }", "('stages the foreground')");
        Expect("class P { /// <summary>While its room scroll is being staged.</summary>\nint F => 0; }", "('scroll is being staged')");
        Expect("class P { // from the foreground position staged before the first call\n }", "('position staged')");
        // The video standards, read case-sensitively (#1797).
        Expect("class P { /// <summary>The NTSC wall-turret shot speed.</summary>\nint F => 0; }", "('NTSC')");
        Expect("class P { // only the PAL build uses the fraction\n }", "('PAL')");
        Expect("class P { string Name => \"NTSC delay exceeded.\"; }", "('NTSC')");
        // The common room elements, spaced or hyphenated, and the original's selector and record tables and its input and
        // pose records (#1797).
        Expect("class P { /// <summary>The definition's common-room-element flag.</summary>\nint F => 0; }", "('common-room-element')");
        Expect("class P { // drops the common room elements' colours\n }", "('common room elements')");
        Expect("class P { /// <summary>A trail selector table picks the family.</summary>\nint F => 0; }", "('selector table')");
        Expect("class P { // The family each selector row names\n }", "('selector row')");
        Expect("class P { /// <summary>Finish after three trips through the end of the record table.</summary>\nint F => 0; }", "('record table')");
        Expect("class P { string Name => \"The entry exceeds its six-record table.\"; }", "('record table')");
        Expect("class P { // the active retail input-table records that start the jump\n }", "('input-table records')");
        Expect("class P { // Reads the previous pose record before the turn\n }", "('pose record')");
        Expect("class P { /// <summary>These are ordinary held input records.</summary>\nint F => 0; }", "('input records')");
        // The reserve tanks' transfer, transferring a value, an animation's or ending's stage, a staged fight and a staging
        // point, a lower-case pal, a common room, a selector alone, a record of something and a hardware owner are English.
        Expect("class P { /// <summary>The reserve tank's manual transfer; transfers the camera position back; a reserve-transfer sound; the foot-transfer advance.</summary>\nint F => 0; }");
        Expect("class P { // the logo stage; the ending's stages; a staged fight; the staging point Ridley retreats to; attack staging positions\n }");
        Expect("class P { /// <summary>Samus's pal; a common room; the room's elements; the selector picks a row; a record of the kills; the input's record.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the shots index the selector table and read the pose record in NTSC timing.</summary>\n[CompatibilityService(\"shots\")] class Shots { }");
        // The original's indirect dispatch, the machine's bus and regionalized values outside hardware owners (#1812).
        Expect("class P { /// <summary>Dispatches the Dachora's main AI through its indirect jump.</summary>\nint F => 0; }", "('indirect jump')");
        Expect("class P { // runs the main AI, including all four indirect targets\n }", "('indirect targets')");
        Expect("class P { /// <summary>The indirect main-AI entry, independent for each half.</summary>\nint F => 0; }", "('indirect main-AI entry')");
        Expect("class P { // runs the Owtch's main AI and its indirect state table\n }", "('indirect state table')");
        Expect("class P { /// <summary>Provides the active room bus used when departure sets up Samus.</summary>\nint F => 0; }", "('bus')");
        Expect("class P { string Name => \"The room's bus is not bound.\"; }", "('bus')");
        Expect("class P { // the regionalized displacement is exactly one pixel\n }", "('regionalized')");
        // Indirect G-Mode, anything indirect that is not dispatch and a hardware owner are English.
        Expect("class P { /// <summary>Indirect G-Mode deliberately carries the flag; the direct/indirect G-Mode state; an indirect hit.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the routine reads the room bus through its indirect jump.</summary>\n[CompatibilityService(\"jump\")] class Jump { }");
        // The original's enemy entries named as records outside hardware owners (#1812).
        Expect("class P { /// <summary>Reinitializes a respawned enemy from its saved population record.</summary>\nint F => 0; }", "('population record')");
        Expect("class P { // reads facing and position from the immediately prior body record\n }", "('body record')");
        Expect("class P { string Name => \"A follower must follow a parent record of the same definition.\"; }", "('parent record')");
        Expect("class P { // while all four following physical records are still at the start\n }", "('physical records')");
        Expect("class P { /// <summary>The second record is not a second creature.</summary>\nint F => 0; }", "('second record')");
        Expect("class P { /// <summary>Binds slot and collision owners for the three-record Evir actor.</summary>\nint F => 0; }", "('three-record')");
        Expect("class P { // the body still holds the empty record the room load left\n }", "('empty record')");
        // The verb, a record of something, an enemy's record, a record alone and a hardware owner are English.
        Expect("class P { /// <summary>Records the desired animation; the death records the drop origin; the first record of the kills; the enemy's record; a record.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the arms read the facing in the body record, which the kill leaves an empty record.</summary>\n[CompatibilityService(\"evir\")] class Evir { }");
        // The original's trigonometry tables outside hardware owners (#1853): an angle in table units, angles or steps, and
        // a sine, cosine or trigonometry table, spaced or hyphenated, in documentation, line comments and string literals.
        Expect("class P { /// <summary>The spiral's step: eight table units, slowed each frame.</summary>\nint F => 0; }", "('table units')");
        Expect("class P { /// <summary>A goop's angle within its quarter turn, 0 through 63 table angles.</summary>\nint F => 0; }", "('table angles')");
        Expect("class P { // an eighth of a table step per frame\n }", "('table step')");
        Expect("class P { // reads signed values from the shared sine table\n }", "('sine table')");
        Expect("class P { /// <summary>Moves the body along a sine-table angle.</summary>\nint F => 0; }", "('sine-table')");
        Expect("class P { // uses the negative cosine-table Y component\n }", "('cosine-table')");
        Expect("class P { string Name => \"The trigonometry tables are not installed.\"; }", "('trigonometry tables')");
        Expect("class P { // hardcoded clamps and trig-table scaling\n }", "('trig-table')");
        // An encoding owner of the angle names its encoding, and a compatibility service may describe the table it reads.
        Expect(EncodingOwner + "[EncodingOwner(\"sine-table byte angle\")] readonly struct A { /// <summary>The angle in table units.</summary>\nint Raw => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the product reads the shared sine table.</summary>\n[CompatibilityService(\"circle\")] class Circle { }");
        // A sine, a cosine, a table and a unit alone, a turn's steps and a speed table are English.
        Expect("class P { /// <summary>The sine and cosine of the angle in steps of a turn; the speed table's next entry; a unit; the table.</summary>\nint F => 0; }");
        // The original's numbering of Samus's movement outside hardware owners (#1856): a movement type, a mode, a
        // collision result and a status by number, spaced or hyphenated, in documentation, line comments and string literals.
        Expect("class P { /// <summary>Morph-ball movement (movement type zero) bounces.</summary>\nint F => 0; }", "('movement type zero')");
        Expect("class P { string Name => \"Spin-jump movement requires movement-type 3.\"; }", "('movement-type 3')");
        Expect("class P { // acceleration modes one and two slow her; mode zero keeps the speed\n }", "('modes one')");
        Expect("class P { /// <summary>Clears acceleration mode-2.</summary>\nint F => 0; }", "('mode-2')");
        Expect("class P { // stops on collision result one\n }", "('collision result one')");
        Expect("class P { /// <summary>Moves the elevator's status from one to two.</summary>\nint F => 0; }", "('status from one')");
        Expect("class P { string Name => \"The elevator left status 0.\"; }", "('status 0')");
        // A hardware owner may cite the numbering it reproduces.
        Expect(CompatibilityService + "/// <summary>Evidence: movement type 6 reads collision result one.</summary>\n[CompatibilityService(\"type\")] class Type { }");
        // A number that counts the noun after it, and a mode, type, result or status alone, are English.
        Expect("class P { /// <summary>A mode two frames long; the movement type; one result; the elevator's status; status one or more.</summary>\nint F => 0; }");
        // The palette outside hardware owners (#1892), in any word form, in documentation, line comments and string literals.
        Expect("class P { /// <summary>Restores the eye palette after its glow.</summary>\nint F => 0; }", "('palette')");
        Expect("class P { // steps the palette-cycle glow\n }", "('palette')");
        Expect("class P { /* both palettes fade */ }", "('palettes')");
        Expect("class P { string Name => \"Palette's colours are not installed.\"; }", "('Palette')");
        // A type named for a palette cited by markup, a run-on symbol and a hardware owner are not this rule's.
        Expect("class P { /// <summary>Shows <see cref=\"Palettes\"/> and <see cref=\"SamusPalette\"/>.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the routine copies the palette.</summary>\n[CompatibilityService(\"copy\")] class Copy { }");
        // The animation interpreter's sleep, stream and bytecode outside hardware owners (#1893).
        Expect("class P { /// <summary>Held as the script's sleep holds it.</summary>\nint F => 0; }", "('sleep')");
        Expect("class P { // the sleeping animation resumes\n }", "('sleeping')");
        Expect("class P { string Name => \"The animation does not sleep.\"; }", "('sleep')");
        Expect("class P { /// <summary>Bomb Torizo is asleep.</summary>\nint F => 0; }", "('asleep')");
        Expect("class P { string Name => \"Mother Brain body bytecode ran without its state.\"; }", "('bytecode')");
        Expect("class P { /// <summary>Runs the open-mouth head stream.</summary>\nint F => 0; }", "('stream')");
        Expect("class P { // advances the death pose's delay streams\n }", "('streams')");
        // An event, audio or level stream, the verb streaming in, into or through, and a hardware owner are English.
        Expect("class P { /// <summary>Identical event streams; the installed level stream; the room streams in through a door; nothing streams into it.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the script's sleep keeps the stream.</summary>\n[CompatibilityService(\"sleep\")] class Sleep { }");
        // The review trail, read in every type (#1971, #1998, #2018): review citations, independent checks, ticket pairs,
        // investigations, proofs, pinned reference data, release pins and commit hashes.
        Expect("class P { /// <summary>Reviewed for #1165; the single owner of the collision space.</summary>\nint F => 0; }", "('Reviewed for #1165')");
        Expect("class P { // Independently checked for #1165 against all 128 of the game's values.\n }", "('Independently checked')");
        Expect("class P { /// <remarks>#1165 independently confirms the spawn order.</remarks>\nint F => 0; }", "('#1165 independently')");
        Expect("class P { /// <summary>The #1165 research evaluator used bounded decimal arithmetic.</summary>\nint F => 0; }", "('#1165 research')");
        Expect("class P { // Issue #1027: the air reveal covers every behaviour.\n }", "('Issue #1027')");
        Expect("class P { /// <summary>#625 / #643 exact: each of the 23 authored samples.</summary>\nint F => 0; }", "('#625 / #643')");
        Expect("class P { /// <remarks>Ten steps of ten updates. Investigation: the pulse.</remarks>\nint F => 0; }", "('Investigation')");
        Expect("class P { // the authored-table proof checks both bounds\n }", "('proof')");
        Expect("class P { /// <summary>Matches the pinned retail data.</summary>\nint F => 0; }", "('pinned retail')");
        Expect("class P { /// <summary>The Japanese and North American v1.0 release's copies of the curve.</summary>\nint F => 0; }", "('v1.0')");
        Expect("class P { // the movie diverges without the carried fractions (ef4b79943a)\n }", "('ef4b79943a')");
        Expect(CompatibilityService + "/// <summary>Evidence: the replay diverges without it (db3aa8b3bb), against the pinned cartridge's sine words.</summary>\n[CompatibilityService(\"carry\")] class Carry { }", "('db3aa8b3bb')");
        Expect(EncodingOwner + "[EncodingOwner(\"angle\")] readonly struct A { /// <summary>Independently verified for #1165.</summary>\nint Raw => 0; }", "('Independently verified')");
        // A generated file's text is the build's: the SDK stamps the framework version and the commit into its attributes.
        Expect("// <auto-generated/>\nclass P { string Version => \"1.0.0+bda16268d85d25bd9d63f37496f3200bf75bb8f9\"; string Framework => \".NETCoreApp,Version=v10.0\"; }");
        // A ticket cited for its change, a thing pinned in the game, a decimal, a word of digits and a hex word are English.
        Expect("class P { /// <summary>Samus, pinned by the beam (#1989); the ring stays pinned to the mouth; 0.125; 8686886868; the facade.</summary>\nint F => 0; }");
        // The original's table, entry, slot, parameter and program numbering outside hardware owners (#1971).
        Expect("class P { // bypasses the normal 16-entry shot-block table\n }", "('16-entry')");
        Expect("class P { /// <summary>The delay entry, 0 through 7 of the eight-entry list.</summary>\nint F => 0; }", "('eight-entry')");
        Expect("class P { // reads the row of a two-row table\n }", "('two-row table')");
        Expect("class P { /// <summary>The nominal overread at entry 23.</summary>\nint F => 0; }", "('entry 23')");
        Expect("class P { // The shot-block table's entries 40..43 select the four doors.\n }", "('entries 40')");
        Expect("class P { /// <summary>Owns the eighteen-slot enemy projectile pool.</summary>\nint F => 0; }", "('eighteen-slot')");
        Expect("class P { // Spawning fails when all eighteen projectile slots are live.\n }", "('eighteen projectile slots')");
        Expect("class P { // lets Samus pass when all forty slots are taken\n }", "('all forty slots')");
        Expect("class P { /// <summary>The first cry, selected by random slots 0 and 3.</summary>\nint F => 0; }", "('slots 0')");
        Expect("class P { // passes animation parameter nine\n }", "('parameter nine')");
        Expect("class P { string Name => \"Enemy projectile is not running a Torizo program.\"; }", "('program')");
        Expect("class P { string Name => $\"Morph-Ball transition {1} selected different animation programs.\"; }", "('programs')");
        // A number that counts the noun after it, a save slot by number, the rows of a menu or panel, a save's slot, an entry
        // alone, a program joined into a key and a hardware owner are English (a slot alone is #2152's).
        Expect("class P { /// <summary>An entry two frames later; save slot 2; the two-row Yes/No prompt; the nine-row result panel; the save's slot; each entry.</summary>\nint F => 0; }");
        Expect("class P { string Name => \"sound-program-heading\"; }");
        Expect("class P { /// <summary>Generated by the generate-library-background-programs script.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the sound program reads entry 4 of the eight-entry table.</summary>\n[CompatibilityService(\"sound\")] class Sound { }");
        // The original's callers, calls and dispatch outside hardware owners (#1998, #2018).
        Expect("class P { /// <summary>The caller reads first, then substitutes the last sample.</summary>\nint F => 0; }", "('caller')");
        Expect("class P { // breaks its caller's running-phase guard\n }", "('caller's')");
        Expect("class P { /// <remarks>The six-frame timer and phase wrap are caller-owned.</remarks>\nint F => 0; }", "('caller')");
        Expect("class P { /// <summary>Above this Y the ship moves 4.5 pixels per call.</summary>\nint F => 0; }", "('per call')");
        Expect("class P { /// <summary>The six behaviours Boulder's main AI dispatches.</summary>\nint F => 0; }", "('dispatches')");
        Expect("class P { // Dispatches the body's main behaviours.\n }", "('Dispatches')");
        Expect("class P { string Name => \"Normal-bomb Torizo dispatch requires a Torizo body.\"; }", "('dispatch')");
        Expect("class P { // every collision kind left after extension redispatch\n }", "('redispatch')");
        Expect("class P { /// <summary>The queue holds each with its delay and dispatches it when the delay expires.</summary>\nint F => 0; }", "('dispatches')");
        // Sending a request, a sound, music, a message, an event or audio, a call, calling, a parameter's name in markup
        // and a hardware owner are English.
        Expect("class P { /// <summary>The queue dispatches the request; a dispatched request; selected audio dispatch owners; the call; calling it.</summary>\nint F => 0; }");
        Expect("class P { /// <param name=\"caller\">The occupant.</param>\nvoid M(int caller) { } }");
        Expect(CompatibilityService + "/// <summary>Evidence: the routine's caller dispatches it per call.</summary>\n[CompatibilityService(\"call\")] class Call { }");
        // The sound chip's voices and key-off outside hardware owners (#2018).
        Expect("class P { /// <summary>The library whose voice queue plays the sound.</summary>\nint F => 0; }", "('voice')");
        Expect("class P { // stops the same looping voice\n }", "('voice')");
        Expect("class P { /// <summary>A cut clip fading out over one key-off release.</summary>\nint F => 0; }", "('key-off')");
        // The verb before its object and a hardware owner are English.
        Expect("class P { /// <summary>The first particle voices the convergence; the last voices each pulse.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the chip keys the voice off.</summary>\n[CompatibilityService(\"voice\")] class Voice { }");
        // A word run on to a number in a comment outside hardware owners (#2018).
        Expect("class P { /// <summary>Uniform footstep hold3, as authored.</summary>\nint F => 0; }", "('hold3')");
        Expect("class P { // advances counters0 through 15\n }", "('counters0')");
        // A file extension, a name Core's code spells, a string literal and a hardware owner are not read.
        Expect("class P { int level1 = 0; /// <summary>Generated by the generate script into level1.</summary>\nint F => level1; }");
        Expect("class P { string Name => \"hold3\"; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the original's hold3.</summary>\n[CompatibilityService(\"hold\")] class Hold { }");
        // The original program's structure and machine arithmetic outside hardware owners (#2080): parameter tables, tail
        // calls, short multiplies, extension arrays, rotting tables and values modulo 256.
        Expect("class P { // The initial action calls through its parameter table during this very update.\n }", "('parameter table')");
        Expect("class P { /// <summary>Yard deliberately tail-calls the common crawler initializer.</summary>\nint F => 0; }", "('tail-calls')");
        Expect("class P { // executes the zero-health tail call into the beam's setup\n }", "('tail call')");
        Expect("class P { /// <summary>Samples the signed short multiply path.</summary>\nint F => 0; }", "('short multiply')");
        Expect("class P { // Enemy extension arrays are cleared with the room.\n }", "('extension arrays')");
        Expect("class P { /// <summary>Does nothing: the rotting table is exhausted.</summary>\nint F => 0; }", "('rotting table')");
        Expect("class P { string Name => \"Emits dust when a rotting-table row finishes.\"; }", "('rotting-table')");
        Expect("class P { /// <summary>Continues the descent until Y modulo 256 reaches 240.</summary>\nint F => 0; }", "('modulo 256')");
        // A table of settings, a tail whip, a call, a multiplication, a rotting corpse and a hardware owner are English
        // (any other modulus is arithmetic told as a summary, #2345).
        Expect("class P { /// <summary>A table of settings; Ridley's tail whip; the call; a multiplication; the rotting corpse.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the short multiply keeps the radius modulo 256.</summary>\n[CompatibilityService(\"product\")] class Product { }");
        // The display's colour math, screen designation and per-line effects outside hardware owners (#2089).
        Expect("class P { /// <summary>The rear-view station grows while its fixed-color flash fades.</summary>\nint F => 0; }", "('fixed-color')");
        Expect("class P { // The flash adds the fixed colour to the rear view.\n }", "('fixed colour')");
        Expect("class P { // keeps showing the actors until its final decrement switches TM back to text\n }", "('TM')");
        Expect("class P { /// <summary>The update's per-line effects run before the cinematic.</summary>\nint F => 0; }", "('per-line effects')");
        Expect("class P { string Name => \"The rainbow beam's per-line effect.\"; }", "('per-line effect')");
        // A type by number, hyphenated, outside hardware owners (#2089).
        Expect("class P { // the following frames execute the ordinary type-6 fall\n }", "('type-6')");
        // A review cited under its ticket, with or without its number sign, in every type (#2089).
        Expect("class P { /// <summary>Authored collision-mask data reviewed under 1165.</summary>\nint F => 0; }", "('reviewed under 1165')");
        Expect(CompatibilityService + "/// <summary>Evidence: reviewed under #1165.</summary>\n[CompatibilityService(\"mask\")] class Mask { }", "('reviewed under #1165')");
        // A fixed set of colours, a lower-case tm, a per-line scroll or window, a type alone, a check under a condition and a
        // hardware owner are English.
        Expect("class P { /// <summary>A fixed set of colours; 5 tm; the per-line scroll; a per-line window; the type; checked under water.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the colour math adds the fixed colour to the TM layers, a per-line effect of type-2.</summary>\n[CompatibilityService(\"math\")] class ColourMath { }");
        // The review inventory and fix history, in every type (#2105).
        Expect("class P { /// <remarks>The shares are authored tuning (see the review inventory's residual scalar inputs).</remarks>\nint F => 0; }", "('review inventory')");
        Expect("class P { // Hardcoding the early-game branch stranded the invisible actor after the getaway cutscene.\n }",
            "('Hardcoding the early-game branch stranded')");
        Expect("class P { void Run() { // Omitting this queue made the moving pad flash, then left the platform frozen.\n } }", "('Omitting this queue made')");
        Expect("class P { /// <remarks>Treating the station's room as sufficient discarded the elevator's view.</remarks>\nint F => 0; }",
            "('Treating the station's room as sufficient discarded')");
        Expect(CompatibilityService + "/// <summary>Evidence: hard-coding the step broke the carry.</summary>\n[CompatibilityService(\"carry\")] class Carry { }", "('hard-coding the step broke')");
        // Relocation history, in every type, hardware owners included (#2133): a ticket followed by what a thing was or
        // where it moved, a parenthetical opening with what a thing was or where it moved, and where a thing moved from.
        Expect("class P { /// <summary>Where Ridley aims (#1620, was <c>CeresRidleyFlight.JitteredFireballTarget</c>): a jitter.</summary>\nint F => 0; }", "('#1620, was')");
        Expect("class P { /// <summary>Partwise reversals (#1620, were <c>A.B</c> and <c>A.C</c>).</summary>\nint F => 0; }", "('#1620, were')");
        Expect(CompatibilityService + "/// <summary>One axis of a seeking boss's velocity (#1620, was in Bosses, unmarked). Evidence: the run.</summary>\n[CompatibilityService(\"run\")] class Run { }", "('#1620, was')");
        Expect("class P { /// <summary>The ending's actors (#1416: moved from the deleted ending catalog).</summary>\nint F => 0; }", "('#1416: moved')");
        Expect(CompatibilityService + "/// <summary>How Samus's speeds behave (moved from Samus's movement, #1621). Evidence: the speed.</summary>\n[CompatibilityService(\"speed\")] class Speed { }", "('(moved')");
        Expect("class P { // The rule (was in Bosses) reverses the speed.\n }", "('(was')");
        Expect("class P { string Name => \"The table, moved here from the ending catalog.\"; }", "('moved here from')");
        Expect(EncodingOwner + "[EncodingOwner(\"angle\")] readonly struct A { /// <summary>The angle (formerly <c>Old.Angle</c>).</summary>\nint Raw => 0; }", "('(formerly')");
        // A thing that moved from one state or place to another, a ticket cited for its change, a parenthetical of what a
        // thing is and a hardware owner's description of its behaviour are English.
        Expect("class P { /// <summary>The arriving elevator moved from <see cref=\"P.F\"/> to <see cref=\"P.F\"/>; the shot moved from its muzzle to the wall (#1989); a shell (the one that was kicked) stops.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Where it moved to is what it leaves in its place; Samus was in the air. Evidence: the move (#1620).</summary>\n[CompatibilityService(\"move\")] class Move { }");
        // A check of the repository cited by its kind or its name, outside hardware owners (#2105).
        Expect("class P { /// <summary>The Tourian animation lookup check matches each statue's segments.</summary>\nint F => 0; }", "('lookup check')");
        Expect("class P { // The eye-fade verification checks every shade.\n }", "('verification checks')");
        Expect("class P { /// <summary>A drain as a parity check states one.</summary>\nint F => 0; }", "('parity check')");
        Expect("class P { /// <remarks>Evidence: the Ridley full-movie check compares the random number.</remarks>\nint F => 0; }", "('full-movie check')");
        Expect("class P { /// <remarks>The running cadence check (<c>VerifyRunningCadence</c>) replays every update.</remarks>\nint F => 0; }", "('<c>VerifyRunningCadence</c>')");
        Expect("class P { /// <remarks>Compared by <c>BeamCallbackTablesVerification</c>.</remarks>\nint F => 0; }", "('<c>BeamCallbackTablesVerification</c>')");
        // An inventory alone, treating a value as another with no breakage told, a frozen enemy, a check the game makes, the
        // condition a replay tests, a test alone and a hardware owner's cited check are English.
        Expect("class P { /// <summary>Samus's inventory; treating the left-behind value as gameplay; skipping a frozen enemy; the awakening facing check compares the facing; the condition the replay tests; the test.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the parity check (<c>VerifyRunningCadence</c>) replays every update.</summary>\n[CompatibilityService(\"cadence\")] class Cadence { }");
        // A controller button's letter used as the name of a gameplay action, outside hardware owners (#2113).
        Expect("class P { // The runtime supplies normalized controls, so X here is the configured Shoot action.\n }", "('X here is the configured Shoot')");
        Expect("class P { /// <summary>Runs the swing. X is the default shoot binding used by this runtime.</summary>\nint F => 0; }", "('X is the default shoot')");
        Expect("class P { // Inputs here are normalized: A is Jump, B is Run.\n }", "('A is Jump')");
        Expect("class P { /// <summary>Starts the turn once Jump is A.</summary>\nint F => 0; }", "('Jump is A')");
        Expect("class P { string Name => \"Hold the B run button.\"; }", "('B run button')");
        Expect("class P { // A fresh press of Shoot (X) fires.\n }", "('Shoot (X)')");
        // A coordinate, an article, X-ray, a lower-case letter, a letter alone and a hardware owner are English.
        Expect("class P { /// <summary>X is the sample; Y is the tangent; A held animation stays held; A binding names a frame.</summary>\nint F => 0; }");
        Expect("class P { // X-ray is shoot-free; a is jump; the X button; Shoot alone.\n }");
        Expect(CompatibilityService + "/// <summary>Evidence: the game reads B as Run and A is Jump.</summary>\n[CompatibilityService(\"buttons\")] class Buttons { }");
        // Anything verified, in every type, as the review trail (#2123).
        Expect("class P { /// <summary>Generated from the verified animations.</summary>\nint F => 0; }", "('verified')");
        Expect("class P { string Name => \"Spin direction transition {0} is not verified.\"; }", "('verified')");
        Expect(CompatibilityService + "/// <summary>Evidence: verified against the captured frames.</summary>\n[CompatibilityService(\"frames\")] class Frames { }", "('verified')");
        // The desktop, the original's assembly loops and branches, a value stored as a number for a meaning and a literal
        // number, outside hardware owners (#2123).
        Expect("class P { // The desktop runtime performs shade writes later in the frame.\n }", "('desktop')");
        Expect("class P { /// <summary>Its own cooldown keeps a desktop-held button from firing faster.</summary>\nint F => 0; }", "('desktop')");
        Expect("class P { // Later bombs overwrite earlier ones exactly as the ascending assembly loop does.\n }", "('ascending assembly')");
        Expect("class P { /// <summary>An assembly branch skips the turn.</summary>\nint F => 0; }", "('assembly branch')");
        Expect("class P { /// <summary>Samus still stores the direction as zero for left and one for right.</summary>\nint F => 0; }",
            "('stores the direction as zero for')");
        Expect("class P { // Any value other than literal four follows the right-facing path.\n }", "('literal four')");
        Expect("class P { // These are the literal 190/66/2 and 192/64/4 branches.\n }", "('literal 190')");
        // Assembling a value, Wrecked Ship's Assembly Line, a count of what is stored, a literal alone, checking, the
        // unverified and a hardware owner are English.
        Expect("class P { /// <summary>Whole and fraction assembly; the Assembly Line room; it stores these as four separate flags; a string literal; checked when it opens; unverified.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the desktop-held button; the ascending assembly loop; stores the direction as zero for left; any value but literal four.</summary>\n[CompatibilityService(\"loop\")] class Loop { }");
        // Reviewed or approved wording and the port's history told against its present, in every type (#2139).
        Expect("class P { /// <summary>Reviewed three-line grouping of the Ceres warning.</summary>\nint F => 0; }", "('Reviewed')");
        Expect("class P { /// <summary>The approved wording of each warning line.</summary>\nint F => 0; }", "('approved')");
        Expect("class P { /// <remarks>The omitted term is small; the reviewed 1e-19 margin covers rounding.</remarks>\nint F => 0; }", "('reviewed')");
        Expect("class P { /// <remarks>Replacing a corrupt permutation with defaults used to hide bad save data. The loader now gets the bindings.</remarks>\nint F => 0; }",
            "('used to hide bad save data. The loader now')");
        Expect("class P { // The ordinary update below used to be gated on movement.\n }", "('used to be')");
        Expect(CompatibilityService + "/// <summary>Evidence: the reviewed cadence holds.</summary>\n[CompatibilityService(\"cadence\")] class Cadence { }", "('reviewed')");
        // A thing used to do something with no present told against it is English.
        Expect("class P { /// <summary>Supplies the enemy counter used to pace dust sounds.</summary>\nint F => 0; }");
        // The port's narration, the original's steps and graphics backups, and numbered story events, outside hardware owners (#2139).
        Expect("class P { // Preserve the managed input order on idle frames.\n }", "('managed')");
        Expect("class P { /// <summary>Forgets the bosses, as an otherwise unused original step does.</summary>\nint F => 0; }", "('original step')");
        Expect("class P { /// <summary>Prepares the pause screen, backing up gameplay graphics.</summary>\nint F => 0; }", "('backing up gameplay graphics')");
        Expect("class P { /// <summary>Restores gameplay graphics on a blank display.</summary>\nint F => 0; }", "('Restores gameplay graphics')");
        Expect("class P { /// <summary>Global story-event bits.</summary>\nint F => 0; }", "('story-event bits')");
        Expect("class P { /// <summary>Checks a persistent event bit.</summary>\nint F => 0; }", "('event bit')");
        Expect("class P { /// <summary>Whether <paramref name=\"eventNumber\"/> is set.</summary>\nbool Has(int eventNumber) => false; }", "('eventNumber')");
        Expect("class P { // the event number the door reads\n }", "('event number')");
        // Original alone, restoring a value, graphics alone, a story event, a number and a bit, and a hardware owner are English.
        Expect("class P { /// <summary>Returns to the original position, restoring the colour; the graphics; the story event; a number; a bit.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the managed copy restores gameplay graphics as an original step; event bit 3.</summary>\n[CompatibilityService(\"pause\")] class Pause { }");
        // A slot as an enemy's, a projectile's or a bomb's place, outside hardware owners and markup (#2152), in
        // documentation, line comments and string literals, alone, named for what holds it and hyphenated.
        Expect("/// <summary>Which Kraid occupant a slot holds, one per definition the species installs.</summary>\nclass P { }", "('slot')");
        Expect("class P { /// <summary>Whether an enemy is drawn, whether it still occupies its slot.</summary>\nint F => 0; }", "('slot')");
        Expect("class P { // The brain slot independently advances the neck raises\n }", "('slot')");
        Expect("class P { /// <summary>Clears a bomb slot when its lifetime ends.</summary>\nint F => 0; }", "('slot')");
        Expect("class P { string Name => \"Botwoon requires thirteen free enemy-projectile slots during room load.\"; }", "('slots')");
        Expect("class P { // The door explosion invokes the highest-free-slot search eight times\n }", "('slot')");
        Expect("class P { /// <summary>What a projectile keeps beyond the slot's shared position.</summary>\nint F => 0; }", "('slot')");
        Expect("class P { /// <summary>A cleared or inactive slot.</summary>\nint F => 0; }", "('slot')");
        // A save slot: by its letter, in a clause that names a save or a file, and in a type named for saves or files, and a
        // hardware owner's slot are the game's own.
        Expect("class P { /// <summary>Copies slot A to slots B and C; a save slot; selecting an empty slot starts a new file; the slot holds no save.</summary>\nint F => 0; }");
        Expect("class FileSelectMenuState { /// <summary>Chooses an occupied source or clear slot, then opens destination or confirmation.</summary>\nint F => 0; }");
        Expect("class SaveSlotStore { // the menu's selected slot\n }");
        Expect("class GameSaveJsonCodec { string Path => \"slots[0].checkpoint\"; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the spawn takes the first free projectile slot.</summary>\n[CompatibilityService(\"spawn\")] class Spawn { }");
        // A frame named as a map, after what it shows or a count of frames, outside hardware owners and markup (#2152).
        Expect("class P { // Every part starts with the one-frame hitbox map\n }", "('hitbox map')");
        Expect("class P { /// <summary>Preserves the direction-specific frozen map after bomb reactions.</summary>\nint F => 0; }", "('frozen map')");
        Expect("class P { /// <summary>Extended body maps for both Ridley encounters.</summary>\nint F => 0; }", "('body maps')");
        Expect("class P { // the tongue follows the body through its animation and extended map\n }", "('extended map')");
        Expect("class P { /// <summary>Switches to its four-map splash at world Y 215.</summary>\nint F => 0; }", "('four-map')");
        Expect("class P { string Name => \"The neutral map is missing.\"; }", "('neutral map')");
        // The map the player reads, a map station's access arm, the verb and a hardware owner are English.
        Expect("class P { /// <summary>The area map; the map's area labels; an extended map access arm; every pose maps to it; the head maps onto the body.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the frozen map follows the facing.</summary>\n[CompatibilityService(\"frozen\")] class Frozen { }");
        // A subtraction's borrow in any word form, outside hardware owners (#2152).
        Expect("class P { // The overlap test's subtract/borrow formulation is strict overlap\n }", "('borrow')");
        Expect("class P { /// <summary>Counted from a cleared cycle, it borrows from the frame.</summary>\nint F => 0; }", "('borrows')");
        Expect("class P { /// <summary>The negated speed, borrowing one whole pixel from the whole pixels.</summary>\nint F => 0; }", "('borrowing')");
        Expect("class P { string Name => \"The stage borrowed past zero.\"; }", "('borrowed')");
        // Borrowing a thing the clause names, ammunition, a round, a unit, a fuse or a path, and a hardware owner are English.
        Expect("class P { /// <summary>Returns a borrowed ammo unit; the Chainsaw beam's borrowed fuse starts it; drawing borrows this frame path.</summary>\nint F => 0; }");
        Expect("class P { // an update that throws after borrowing the last round; the ammunition the update borrowed\n }");
        Expect(CompatibilityService + "/// <summary>Evidence: an empty countdown borrows from the stage.</summary>\n[CompatibilityService(\"boost\")] class Boost { }");
        // The repository's verification, verifiers, fixtures and movies, outside hardware owners and markup (#2152).
        Expect("enum P { /// <summary>Its values are arbitrary; verification's catalog relates each member to its source.</summary>\nA }", "('verification')");
        Expect("class P { /// <summary>The reusable head verifier advances its private animation.</summary>\nint F => 0; }", "('verifier')");
        Expect("class P { // A synthetic room fixture may omit Samus\n }", "('fixture')");
        Expect("class P { /// <summary>Null when no camera owns them, as in standalone enemy fixtures.</summary>\nint F => 0; }", "('fixtures')");
        Expect("class P { /// <summary>The recorded Ridley movie's random number matches.</summary>\nint F => 0; }", "('movie')");
        Expect("[Attr(\"the one other method taking it is Verification's EnemySystemFixtures.New\")] class P { }\nclass AttrAttribute(string reason) : System.Attribute { public string Reason { get; } = reason; }", "('Verification')");
        // An assembly's name, a demo's recording, the unverified and a hardware owner's cited recording are English.
        Expect("class P { string Name => \"SuperMetroid.Verification\"; string Other => \"SuperMetroid.RenderVerification\"; }");
        Expect("class P { /// <summary>The demo's recording ends; the transition is unverified.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the 100% movie loses the Baby's fractions.</summary>\n[CompatibilityService(\"baby\")] class Baby { }");
        // The original's tables, indexes and bugs, outside hardware owners and markup (#2152).
        Expect("class P { // preserving that retail bug matters to the velocity math\n }", "('retail bug')");
        Expect("class P { /// <summary>Runs the hover and its original wall bug.</summary>\nint F => 0; }", "('original wall bug')");
        Expect("class P { /// <summary>A Magdollite's three retail-ordered places.</summary>\nint F => 0; }", "('retail-ordered')");
        Expect("class P { /// <summary>Runs the three independent random-table selections.</summary>\nint F => 0; }", "('random-table')");
        Expect("class P { /// <summary>Downward velocity from the common linear-speed table.</summary>\nint F => 0; }", "('linear-speed table')");
        Expect("class P { /// <summary>The aim angle in the table convention.</summary>\nint F => 0; }", "('table convention')");
        Expect("class P { /// <summary>Place table.</summary>\nint F => 0; }", "('Place table')");
        Expect("class P { // The table contains three identical entries per side\n }", "('identical entries')");
        Expect("class P { // The first step of that animation republishes the same index\n }", "('same index')");
        Expect("class P { // where the collision pass has already accepted the collision index\n }", "('collision index')");
        Expect("class P { // the marker is consumed on the next alpha pass\n }", "('alpha pass')");
        Expect("class P { // retains the other definition bits; the typed representation stores the flags\n }", "('definition bits')");
        Expect("class P { /// <summary>True for all five running pose-table entries.</summary>\nint F => 0; }", "('table entries')");
        // A table alone, a speed table, an index alone, a bug that is not the original's and a hardware owner are English.
        Expect("class P { /// <summary>The drop-chance table; Samus's speed table; the frame's index; a bug in the pipe.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the retail bug reads the angle table's entries.</summary>\n[CompatibilityService(\"angle\")] class Angle { }");
        // The machine's 16.16 arithmetic named by its sixty-five-thousandths, outside hardware owners and markup (#2172), in
        // documentation, line comments and string literals, with up to two words before the arithmetic.
        Expect("class P { /// <summary>Moves the Owtch with the game's 1/65536-pixel additions.</summary>\nint F => 0; }", "('1/65536-pixel additions')");
        Expect("class P { /// <summary>Signed 1/256-pixel velocity; the horizontal component feeds the 1/65536-pixel collision helper.</summary>\nint F => 0; }",
            "('1/65536-pixel collision helper')");
        Expect("class P { // One 1/65536-pixel add: the fraction's overflow reaches the whole pixels.\n }", "('1/65536-pixel add')");
        Expect("class P { // The stores produce delta/4 in signed 1/65536-pixel form.\n }", "('1/65536-pixel form')");
        Expect("class P { /// <summary>Both are one 1/65536-pixel sum; floating point or the ordinary 1/65536-pixel movement helpers.</summary>\nint F => 0; }",
            "('1/65536-pixel sum')");
        Expect("class P { string Name => \"Moves the projectile in 1/65536-pixel space.\"; }", "('1/65536-pixel space')");
        Expect("class P { // The 1/65536th constant is exactly -1.0\n }", "('1/65536th constant')");
        // A unit of measure and a hardware owner are English.
        Expect("class P { /// <summary>A speed in 1/65536 pixels; the signed 1/65536-pixel velocity; a 1/65536-pixel displacement into the slope move; its 1/65536-pixel position.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the game's 1/65536-pixel additions carry the fraction.</summary>\n[CompatibilityService(\"carry\")] class Carry { }");
        // Paired arithmetic, outside hardware owners, spaced or hyphenated (#2172).
        Expect("class P { // The fraction's overflow and the whole pixels' wraparound match the game's paired additions.\n }", "('paired additions')");
        Expect("class P { /// <summary>Its paired-subtraction step.</summary>\nint F => 0; }", "('paired-subtraction')");
        Expect("class P { string Name => \"The paired adds wrapped.\"; }", "('paired adds')");
        // Things the game pairs, an addition alone and a hardware owner are English.
        Expect("class P { /// <summary>The paired doors open together; an addition to the count; the paired enemies.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the paired additions carry the fraction.</summary>\n[CompatibilityService(\"pair\")] class Pair { }");
        // A routine run or called as a helper or controller, outside hardware owners and markup (#2172).
        Expect("class P { /// <summary>Runs the linked-segment placement helper.</summary>\nint F => 0; }", "('Runs the linked-segment placement helper')");
        Expect("class P { /// <summary>Runs the last segment's collision/reversal controller.</summary>\nint F => 0; }",
            "('Runs the last segment's collision/reversal controller')");
        Expect("class P { // Shared rainbow setup calls the previous-pose helper before lock.\n }", "('calls the previous-pose helper')");
        Expect("class P { string Name => \"Both outcomes call the sound helper.\"; }", "('call the sound helper')");
        Expect("class P { /// <summary>Runs the white-flash ramp and its flicker helper.</summary>\nint F => 0; }", "('Runs the white-flash ramp and its flicker helper')");
        // The player's controller and its input, buttons or port, a helper or controller not run or called, a reference
        // in markup and a hardware owner are English.
        Expect("class P { /// <summary>Runs the controller input check; reads the player's controller; the helper text; calls the elevator; runs <see cref=\"Helper\"/>.</summary>\nint F => 0; int Helper => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: runs the placement helper.</summary>\n[CompatibilityService(\"helper\")] class Helper { }");
        // The Japanese and North American release named with no difference from the European release stated in its
        // paragraph, outside hardware owners (#2192), in line comments across their lines, documentation and literals.
        Expect("class P { void Run() { // The Japanese and North American release measures angles in 256ths of a turn.\n } }",
            "('Japanese and North American')");
        Expect("class P { void Run() {\n // The Japanese and North American release reloads the burrow duration every airborne frame.\n // It becomes the whole burrow duration only on the landing frame.\n } }",
            "('Japanese and North American')");
        Expect("class P { /// <summary>The Japanese and North American release rises by half a pixel.</summary>\nint F => 0; }",
            "('Japanese and North American')");
        Expect("class P { string Name => \"The Japanese and North American release moves it.\"; }", "('Japanese and North American')");
        // A paragraph ends at a blank line.
        Expect("class P { void Run() {\n // The Japanese and North American release moves it.\n\n // The European release uses the fraction.\n } }",
            "('Japanese and North American')");
        // A paragraph that sets the two releases side by side, across a comment's lines too, the European release alone and
        // a hardware owner are English.
        Expect("class P { void Run() { // The Japanese and North American release takes 80; the European release takes 99.\n } }");
        Expect("class P { void Run() {\n // The speed is authored with a fraction, but the Japanese and North American release moves it by whole pixels;\n // only the European release uses the fraction.\n } }");
        Expect("class P { /// <summary>The first flame waits 120 frames; the European release waits 96.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the Japanese and North American release measures angles.</summary>\n[CompatibilityService(\"angle\")] class Angle { }");
        // A speed row, spaced or hyphenated, outside hardware owners (#2192).
        Expect("class P { /// <summary>Displacement to the right, from the positive half of the population's linear speed row.</summary>\nint F => 0; }",
            "('speed row')");
        Expect("class P { void Run() { // the reversal reuses the speed-table row\n } }", "('speed-table row')");
        Expect("class P { string Name => \"The speed rows ran out.\"; }", "('speed rows')");
        // A speed, a row of blocks, a speed table alone and a hardware owner are English.
        Expect("class P { /// <summary>The speed; the first row of blocks; Samus's speed table.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the speed row holds both signs.</summary>\n[CompatibilityService(\"row\")] class Row { }");
        // A variable label cited in backticks, outside hardware owners (#2192).
        Expect("class P { void Run() { // Testing only `ranActorAi` here leaves the animation running.\n } }", "('`ranActorAi`')");
        Expect("class P { void Run() { // Whether the sprites of `layer` are placed.\n } }", "('`layer`')");
        // An expression in backticks, a member's name in backticks and a hardware owner are English.
        Expect("class P { void StepMovement() { } void Run() { // rejects `abs(delta) >= sum`; the frame `StepMovement` returns\n } }");
        Expect(CompatibilityService + "/// <summary>Evidence: the copy reads `probe`.</summary>\n[CompatibilityService(\"probe\")] class Probe { }");
        // A sort order, outside hardware owners (#2192).
        Expect("class P { /// <summary>A normal bomb's family sorts after the Power Bomb family.</summary>\nint F => 0; }", "('sorts after')");
        Expect("class P { void Run() { // the shot sorted before the bomb\n } }", "('sorted before')");
        // A sort alone, a list sorted by a key and a hardware owner are English.
        Expect("class P { /// <summary>The sort of the list; the list is sorted by distance.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the bomb's family sorts after the Power Bomb's.</summary>\n[CompatibilityService(\"sort\")] class Sort { }");
        // A scroll table, spaced or hyphenated, outside hardware owners (#2192).
        Expect("class P { /// <summary>Begins the melt: resets the background scroll table and the distortion.</summary>\nint F => 0; }",
            "('scroll table')");
        Expect("class P { void Run() { // until a room-specific scroll-table is loaded\n } }", "('scroll-table')");
        // A scroll, a per-line scroll, a scroll's cells and a hardware owner are English.
        Expect("class P { /// <summary>The scroll; the melt's per-line scroll; the scroll's cells.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: an explicit scroll table's load keeps the cells.</summary>\n[CompatibilityService(\"scroll\")] class Scroll { }");
        // Colours loaded into a row, outside hardware owners and markup (#2192).
        Expect("class P { /// <summary>His projectiles' colours, which his fight loads into the projectiles' row.</summary>\nint F => 0; }",
            "('loads into the projectiles' row')");
        Expect("class P { void Run() { // the glow is loaded in its own row\n } }", "('loaded in its own row')");
        // Loading a room or a file, a row of blocks and a hardware owner are English.
        Expect("class P { /// <summary>The room loads; the first row of blocks falls; loads the file.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the fight loads them into the projectiles' row.</summary>\n[CompatibilityService(\"load\")] class Load { }");
        // A state extension, extended state and extra values, outside hardware owners (#2192).
        Expect("class P { /// <summary>Typed extended state used by the bridge.</summary>\nint F => 0; }", "('extended state')");
        Expect("class P { string Name => \"Crocomire death extension is not initialized for the current room.\"; }", "('death extension')");
        Expect("class P { /// <summary>Requires the body and its shared state extension.</summary>\nint F => 0; }", "('state extension')");
        Expect("class P { /// <summary>The typed encounter extension while the population is loaded.</summary>\nint F => 0; }",
            "('encounter extension')");
        Expect("class P { /// <summary>The state of the ghost, with its extra values.</summary>\nint F => 0; }", "('extra values')");
        // A block's, neck's, tail's or rope's extension, a value alone and a hardware owner are English.
        Expect("class P { /// <summary>The block's vertical extension; the neck extension; the tail's extension speed; a value.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the boss extension holds the speed.</summary>\n[CompatibilityService(\"extension\")] class Extension { }");
        // A literal write, store or assignment, outside hardware owners (#2192).
        Expect("class P { void Run() { // the radii and coordinates are literal writes\n } }", "('literal writes')");
        Expect("class P { /// <summary>Preserves the literal store.</summary>\nint F => 0; }", "('literal store')");
        // A literal alone and a hardware owner are English.
        Expect("class P { /// <summary>The literal text the message shows.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: these are literal writes.</summary>\n[CompatibilityService(\"literal\")] class Literal { }");
        // Fix history told by an adverb of error, a fall-through, an earlier order or a stranding, in every type, hardware
        // owners included (#2192).
        Expect("class P { void Run() { // Testing it here incorrectly advanced a Metroid's body animation.\n } }", "('incorrectly')");
        Expect("class P { /// <summary>Requiring a vertical collision here wrongly skipped the run check.</summary>\nint F => 0; }", "('wrongly')");
        Expect("class P { void Run() { // This update ends here. Falling through ran Samus movement and counted three frames.\n } }",
            "('Falling through ran')");
        Expect("class P { void Run() { // publishing first made the on-screen minimum read two\n } }", "('publishing first made')");
        Expect("class P { /// <summary>Clears the hurt state; omitting it would strand Metroid in custom hurt forever.</summary>\nint F => 0; }",
            "('would strand')");
        Expect(CompatibilityService + "/// <summary>Evidence: rounding first made the step short.</summary>\n[CompatibilityService(\"round\")] class Round { }",
            "('rounding first made')");
        // A thing the game shows wrong, Samus falling through a platform, an order alone, a stranded beam and a generated
        // file are English.
        Expect("class P { /// <summary>The wrong answer shows; Samus falling through the platform; the HUD publishes first; the stranded X-ray beam.</summary>\nint F => 0; }");
        Expect("// <auto-generated/>\nclass P { // a step wrongly skipped\n }");
        // A gameplay path told as a diagnostic's, debug and non-debug, and the port's C# construction, in every type,
        // hardware owners included (#2192).
        Expect("class P { void Run() { // repeat it here to preserve the old diagnostic seam\n } }", "('diagnostic seam')");
        Expect("class P { void Run() { // the loader also serves direct diagnostic loads\n } }", "('diagnostic loads')");
        Expect("class P { void Run() { // do not restore its diagnostic pre-impact magnitude for effects\n } }",
            "('diagnostic pre-impact magnitude')");
        Expect("class P { void Run() { // the same sequence the room diagnostic uses\n } }", "('room diagnostic')");
        Expect("class P { void Run() { // it stays in the pose movement for direct/diagnostic step use\n } }", "('/diagnostic')");
        Expect("class P { void Run() { // Fresh debug sessions begin without an area map.\n } }", "('debug sessions')");
        Expect("class P { void Run() { // Debug scripts that grant Hyper Beam get the same order.\n } }", "('Debug scripts')");
        Expect("class P { /// <summary>The input a non-debug build reads for the map.</summary>\nint F => 0; }", "('non-debug build')");
        Expect("class P { void Run() { // the debug tools construct a debug room for it\n } }", "('debug tools')");
        Expect("class P { void Run() { // the check constructs a debug room for it\n } }", "('constructs a debug room')");
        Expect("class P { void Run() { // The C# room constructor groups those owners differently.\n } }", "('C# room constructor')");
        Expect("class P { /// <summary>Grouped by subsystem rather than by the C# class that holds them.</summary>\nint F => 0; }",
            "('C# class')");
        Expect(CompatibilityService + "/// <summary>Evidence: a debug session shows it.</summary>\n[CompatibilityService(\"session\")] class Session { }",
            "('debug session')");
        // The original game's own developer debug mode is the game's (the owner's ruling under #2248 and #2265): its debug
        // flag, its debug-only save icons, an area change with debug enabled and its non-debug input.
        Expect("class P { /// <summary>Without the game's debug flag the map draws no debug-only save icons; with debug enabled the area map changes areas, and its non-debug input discards the direction.</summary>\nint F => 0; }");
        Expect("class P { void Run() { // Normal (non-debug) input for the map; the game's debug mode\n } }");
        // A name, text or identity for diagnostics, C# alone and a generated file are English.
        Expect("class P { /// <summary>The reaction's name, for diagnostics; a stable diagnostic identity; the diagnostic text; written in C#.</summary>\nint F => 0; }");
        Expect("// <auto-generated/>\nclass P { // a debug session\n }");
        // A background object, spaced or hyphenated, outside hardware owners (#2222).
        Expect("class P { /// <summary>Five cinematic background-object updates between letters.</summary>\nint F => 0; }",
            "('background-object')");
        Expect("class P { void Run() { // the eye's background object animates\n } }", "('background object')");
        // A background alone, an object in the background and a hardware owner are English.
        Expect("class P { /// <summary>The background; the object in the background; the room's objects.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the background object's timer.</summary>\n[CompatibilityService(\"object\")] class BackgroundObject { }");
        // A palette's ranges, outside hardware owners and markup (#2222).
        Expect("class P { void Run() { // text/portrait ranges fade in\n } }", "('text/portrait ranges')");
        Expect("class P { /// <summary>Fades gameplay and projectile ranges out.</summary>\nint F => 0; }", "('gameplay and projectile ranges')");
        Expect("class P { /// <summary>The colour ranges the shade changes.</summary>\nint F => 0; }", "('colour ranges')");
        Expect("class P { string Name => \"the sprite range\"; }", "('sprite range')");
        // A range alone, a shot's range, a range of rows and a hardware owner are English.
        Expect("class P { /// <summary>The range of the shot; within range; the shot's range; a range of rows.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the sprite range fades.</summary>\n[CompatibilityService(\"range\")] class Range { }");
        // A colour write, overwriting colours and a fade's component steps, outside hardware owners and markup (#2222).
        Expect("class P { /// <summary>Whether the crossfade still owns the colour writes.</summary>\nint F => 0; }", "('colour writes')");
        Expect("class P { void Run() { // this primes an immediate color write\n } }", "('color write')");
        Expect("class P { void Run() { // the skip explicitly overwrites the two glyph colours\n } }", "('overwrites the two glyph colours')");
        Expect("class P { /// <summary>Thirty-two component steps.</summary>\nint F => 0; }", "('component steps')");
        Expect("class P { /// <summary>Advances 32 component updates.</summary>\nint F => 0; }", "('component updates')");
        // A colour alone, a colour step, writing a picture's colours into a buffer and a hardware owner are English.
        Expect("class P { /// <summary>The colour; a colour step; writes each colour the picture shows into the buffer; writes the message.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the later colour write wins.</summary>\n[CompatibilityService(\"write\")] class Write { }");
        // A script's record, spaced or hyphenated, outside hardware owners (#2222).
        Expect("class P { /// <summary>Five updates between character records.</summary>\nint F => 0; }", "('character records')");
        Expect("class P { void Run() { // the optional one-frame katakana caption record\n } }", "('caption record')");
        Expect("class P { /// <summary>The narration written from installed glyph records.</summary>\nint F => 0; }", "('glyph records')");
        Expect("class P { /// <summary>One start-delay record for an explosion.</summary>\nint F => 0; }", "('delay record')");
        // A record of something, the verb, a demo's recording and a hardware owner are English.
        Expect("class P { /// <summary>A record of the hits; the game records the kill; the demo's recording.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the caption record shows.</summary>\n[CompatibilityService(\"record\")] class Record { }");
        // The dropped drawing order, outside hardware owners (#2222).
        Expect("class P { void Run() { // the priority tiers that once interleaved them\n } }", "('once interleaved')");
        Expect("class P { /// <summary>The flicker is a presentation quirk the model does not keep.</summary>\nint F => 0; }",
            "('presentation quirk')");
        // Interleaving alone, a quirk of the game and a hardware owner are English.
        Expect("class P { /// <summary>Interleaving the two lists; the jump's quirk.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: a presentation quirk.</summary>\n[CompatibilityService(\"quirk\")] class Quirk { }");
        // An opcode mnemonic set as code in markup, outside hardware owners (#2222).
        Expect("class P { /// <summary>A count: <c>INC</c> permits more than one ring.</summary>\nint F => 0; }", "('<c>INC</c>')");
        Expect("class P { /// <summary>The <c>DEC</c> that follows.</summary>\nint F => 0; }", "('<c>DEC</c>')");
        // A capitalised name that is no mnemonic and a hardware owner are English.
        Expect("class P { /// <summary>The <c>HUD</c> and the <c>ENERGY</c> label.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: <c>INC</c> adds one.</summary>\n[CompatibilityService(\"count\")] class Count { }");
        // A register reloaded, outside hardware owners (#2222).
        Expect("class P { void Run() { // The walk step does not reload Y before the walk.\n } }", "('reload Y')");
        Expect("class P { /// <summary>Reloads X before the step.</summary>\nint F => 0; }", "('Reloads X')");
        // Reloading a timer, a room or a weapon and a hardware owner are English.
        Expect("class P { /// <summary>Reloads its timer; reloads the room; a reload of the weapon; reloads a missile.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: it does not reload X.</summary>\n[CompatibilityService(\"reload\")] class Reload { }");
        // A field of the C reimplementation, outside hardware owners and markup (#2222).
        Expect("class P { void Run() { // The ring writes the Baby place's Enemy.health from this pass.\n } }", "('Enemy.health')");
        Expect("class P { /// <summary>The <c>Samus.speed</c> it reads.</summary>\nint F => 0; }", "('Samus.speed')");
        // A file's name, a member cited by cref and a hardware owner are English.
        Expect("class P { /// <summary>The Room.cs file and <see cref=\"P.F\"/>; the room.json file.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: Enemy.health is read.</summary>\n[CompatibilityService(\"health\")] class Health { }");
        // A packed value, outside hardware owners (#2222).
        Expect("class P { /// <summary>Sets the projectile's packed X and Y collision radii.</summary>\nint F => 0; }",
            "('packed X and Y collision radii')");
        Expect("class P { /// <summary>The digit, not a packed decimal pair.</summary>\nint F => 0; }", "('packed decimal')");
        // A pack of enemies, packing pictures onto a sheet, a backend's packing and a hardware owner are English.
        Expect("class P { /// <summary>A pack of enemies; packing the pictures onto a sheet; the backend's packing; a packed room.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the packed radii.</summary>\n[CompatibilityService(\"radius\")] class Radius { }");
        // A disassembly label as a summary and the Japanese text's label, outside hardware owners (#2222).
        Expect("class P { /// <summary>Enemy projectile animation: Mother brains drool.</summary>\nint F => 0; }", "('Enemy projectile animation:')");
        Expect("class P { /// <summary>Enemy proj animation: the escape door particle.</summary>\nint F => 0; }", "('Enemy proj animation:')");
        Expect("class P { /// <summary>The time Bomb set japan text.</summary>\nint F => 0; }", "('japan text')");
        // An enemy's animation, the Japanese text and a hardware owner are English.
        Expect("class P { /// <summary>The enemy's animation: it flies; the Japanese text.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: Enemy projectile animation: drool.</summary>\n[CompatibilityService(\"drool\")] class Drool { }");
        // An encounter's numbered event, form, state, place or region, outside hardware owners and markup (#2222).
        Expect("class P { /// <summary>Changes the form to 4.</summary>\nint F => 0; }", "('form to 4')");
        Expect("class P { /// <summary>Changes body form to 4.</summary>\nint F => 0; }", "('body form to 4')");
        Expect("class P { void Run() { // until the corpse-state value reaches final-combat state 2\n } }", "('final-combat state 2')");
        Expect("class P { /// <summary>Closes scroll region 1.</summary>\nint F => 0; }", "('region 1')");
        Expect("class P { void Run() { // the first turret lands in place 17\n } }", "('place 17')");
        Expect("class P { /// <summary>Rewritten when corpse state one is published.</summary>\nint F => 0; }", "('corpse state one')");
        Expect("class P { void Run() { // the body form two is clear\n } }", "('body form two')");
        // A number that counts the noun after it, a number word after a verb or as a determiner, a stage by number and a
        // hardware owner are English.
        Expect("class P { /// <summary>A form 2 frames long; places two pickups; the definitions that form one encounter; the room's state 2; Speed Booster stage four.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the turret lands in place 17.</summary>\n[CompatibilityService(\"turret\")] class Turret { }");
        // The retail population or initializer, outside hardware owners (#2222).
        Expect("class P { /// <summary>While Mother Brain's retail population is loaded.</summary>\nint F => 0; }", "('retail population')");
        Expect("class P { void Run() { // Retail part init clears that write.\n } }", "('Retail part init')");
        Expect("class P { /// <summary>The kinds retail room-object populations place.</summary>\nint F => 0; }",
            "('retail room-object populations')");
        // The game alone, a population alone and a hardware owner are English.
        Expect("class P { /// <summary>The game; the room's population; initializes the room.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the retail population lists it.</summary>\n[CompatibilityService(\"population\")] class Population { }");
        // Fix history told inside a clause, in every type, hardware owners included (#2248, #2265).
        Expect("class P { void Run() { // leaving this assignment out produces a conspicuously silent attack\n } }",
            "('leaving this assignment out')");
        Expect("class P { void Run() { // reducing those branches to three/two loses one point on both paths\n } }",
            "('reducing those branches to three/two loses')");
        Expect("class P { /// <summary>Reading only the uncharged combination row made Wave Shield disappear.</summary>\nint F => 0; }",
            "('Reading only the uncharged combination row made')");
        Expect(CompatibilityService + "/// <summary>Evidence: leaving it out stops the shake.</summary>\n[CompatibilityService(\"shake\")] class Shake { }",
            "('leaving it out')");
        // A consequence told in the present, leaving a room and a generated file are English.
        Expect("class P { /// <summary>Moving only left turns her; leaving the room ends the fight.</summary>\nint F => 0; }");
        Expect("// <auto-generated/>\nclass P { // leaving this out made it silent\n }");
        // The original's enemies behind an encounter's parts told as the physical ones, outside hardware owners (#2265).
        Expect("class P { /// <summary>Allocates the eight flames from the physical projectile pool.</summary>\nint F => 0; }",
            "('physical projectile')");
        Expect("class P { /// <summary>Copies the body's colours into a physical part.</summary>\nint F => 0; }", "('physical part')");
        Expect("class P { /// <summary>Creates the physical Baby.</summary>\nint F => 0; }", "('physical Baby')");
        Expect("class P { void Run() { // requests tube enemy 2\n } }", "('tube enemy 2')");
        // Physical contact, a physical component or motion and a hardware owner are English.
        Expect("class P { /// <summary>Physical contact; the body's physical components; physical motion.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the physical tube enemies fall.</summary>\n[CompatibilityService(\"tube\")] class Tube { }");
        // The original's routines named as walkers and helpers, outside hardware owners and markup (#2265).
        Expect("class P { void Run() { // The extended-hitbox walker marks the projectile first.\n } }", "('extended-hitbox walker')");
        Expect("class P { /// <summary>The collision walkers test the frame's hitboxes.</summary>\nint F => 0; }", "('collision walkers')");
        Expect("class P { void Run() { // the walker has marked every overlapping family\n } }", "('the walker')");
        Expect("class P { void Run() { // That reaction's damage helper does not convert it.\n } }", "('damage helper')");
        Expect("class P { /// <summary>The standing helper reports her standing.</summary>\nint F => 0; }", "('standing helper')");
        Expect("class P { void Run() { // the helper's hard floor reports arrival\n } }", "('the helper')");
        // An enemy that walks, a helper alone, the helper text, markup and a hardware owner are English.
        Expect("class P { /// <summary>A floor walker; the left walker's role; a helper; the helper text; <see cref=\"Walker\"/>.</summary>\nint F => 0; int Walker => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the collision walker marks it.</summary>\n[CompatibilityService(\"walker\")] class Walker { }");
        // Bit masks, negative branches and unsigned values, outside hardware owners (#2265).
        Expect("class P { /// <summary>RNG&amp;7 selects the vulnerable window.</summary>\nint F => 0; }", "('&amp;7')");
        Expect("class P { void Run() { // sin((a & 127)*pi/128)\n } }", "('& 127')");
        Expect("class P { /// <param name=\"index\">Zero through seven, selected by masked RNG or frame bits.</param>\nvoid M(int index) { } }",
            "('masked RNG')");
        Expect("class P { void Run() { // A decrement of zero branches negative without storing -1.\n } }", "('branches negative')");
        Expect("class P { /// <summary>Expires only when it wraps negative.</summary>\nint F => 0; }", "('wraps negative')");
        Expect("class P { void Run() { // The addition and comparison are unsigned here.\n } }", "('are unsigned')");
        Expect("class P { /// <param name=\"speed\">The beam's speed, read unsigned.</param>\nvoid M(int speed) { } }", "('read unsigned')");
        // An and between words or conditions, a negative value, an unsigned value alone and a hardware owner are English.
        Expect("class P { /// <summary>Samus &amp; Ridley; a &amp;&amp; b; a negative speed; an unsigned value.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: RNG&amp;7 picks it; the sum is unsigned here.</summary>\n[CompatibilityService(\"mask\")] class Mask { }");
        // The original's program flow, outside hardware owners and markup (#2248, #2265).
        Expect("class P { void Run() { // When it runs out the code installs the typing step.\n } }", "('the code installs')");
        Expect("class P { /// <summary>Starts the fireball's flight: installs its moving frame behaviour.</summary>\nint F => 0; }",
            "('installs its moving frame behaviour')");
        Expect("class P { void Run() { // the fall step installs the wait phase\n } }", "('installs the wait')");
        Expect("class P { /// <summary>Deletes it, as the shot animation's jump into the common deletion animation does.</summary>\nint F => 0; }",
            "('jump into the common deletion animation')");
        Expect("class P { void Run() { // The fallthrough from the rise applies both changes.\n } }", "('fallthrough')");
        Expect("class P { void Run() { // the delay falls through into its first step\n } }", "('falls through into')");
        Expect("class P { void Run() { // The neck step falls directly into its successor.\n } }", "('falls directly into its successor')");
        Expect("class P { void Run() { // the grab falls through into the carry on the grabbing update\n } }", "('falls through into')");
        // Installed content, Samus jumping or falling through a room, a floor or the water, and a hardware owner are English.
        Expect("class P { /// <summary>The installed steps and pictures; Samus jumps to the ledge; she falls through into the room below; the block falls through to the floor.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the setup falls through into its first step.</summary>\n[CompatibilityService(\"flow\")] class Flow { }");
        // Data told as engine-owned, default rows, colour zero and numbered parameters, outside hardware owners (#2248).
        Expect("class P { /// <summary>Answers Ridley's engine-owned hitboxes.</summary>\nint F => 0; }", "('engine-owned')");
        Expect("class P { /// <summary>Selects Norfair, Tourian or the default row for every other area.</summary>\nint F => 0; }",
            "('default row')");
        Expect("class P { void Run() { // the colours leave transparent colour zero as it is\n } }", "('colour zero')");
        Expect("class P { /// <summary>The actor parameter's rightward flag selects it.</summary>\nint F => 0; }", "('actor parameter')");
        Expect("class P { void Run() { // Mother Brain's head authors no slow laser in its first parameter.\n } }", "('first parameter')");
        // An engine alone, a row of blocks, a colour, a method's parameter and a hardware owner are English.
        Expect("class P { /// <summary>The engine; the first row of blocks; the zero colour step; the parameter is the sample.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: colour zero fades too.</summary>\n[CompatibilityService(\"zero\")] class Zero { }");
        // A literal write with words between and a pace per any call, outside hardware owners (#2248).
        Expect("class P { /// <summary>Literal inventory writes of Golden Torizo's code.</summary>\nint F => 0; }", "('Literal inventory writes')");
        Expect("class P { /// <summary>Advances one setup stage per AI call.</summary>\nint F => 0; }", "('per AI call')");
        // A literal alone, a call alone and a hardware owner are English.
        Expect("class P { /// <summary>The literal text the inventory shows; one call per room.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: literal inventory writes, one per AI call.</summary>\n[CompatibilityService(\"inventory\")] class Inventory { }");
        // The machine's storage told for a value, outside hardware owners (#2271).
        Expect("class P { /// <summary>Initial maximum health shifted right one, the threshold for his faster speed.</summary>\nint F => 0; }",
            "('shifted right one')");
        Expect("class P { /// <summary>The origin of a screen: each axis shifted left eight bits.</summary>\nint F => 0; }",
            "('shifted left eight')");
        Expect("class P { string S = \"right-shift applied to horizontal speed\"; }", "('right-shift')");
        Expect("class P { void Run() { // the trail lies in the state the room's enemy setup clears on room load\n } }",
            "('enemy setup clears')");
        Expect("class P { void Run() { // its history clears on room load\n } }", "('clears on room load')");
        Expect("class P { /// <summary>The head's last positions, cleared as the room's enemies load.</summary>\nint F => 0; }",
            "('cleared as the room's enemies load')");
        Expect("class P { /// <summary>The frames, named in the physical order of the table.</summary>\nint F => 0; }", "('physical order')");
        Expect("class P { void Run() { // Each position uses both halves of one RNG result.\n } }", "('both halves of one RNG')");
        Expect("class P { /// <summary>The segment's phase; the game kept it in the projectile's horizontal velocity.</summary>\nint F => 0; }",
            "('kept it in the projectile's horizontal velocity')");
        Expect("class P { /// <summary>The movement angle, consumed by the signed fixed-point vector helper.</summary>\nint F => 0; }",
            "('vector helper')");
        // A shift by a counted noun, a room that clears, physical contact, halves of a picture, a value kept for an update
        // and a hardware owner are English.
        Expect("class P { /// <summary>The camera shifts left two pixels; the room clears when Samus leaves; physical contact; the halves of a picture; a value kept for an update.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the ring is cleared as the room's enemies load, in its physical order.</summary>\n[CompatibilityService(\"ring\")] class Ring { }");
        // The original's routines told as wrappers, helpers and controllers, outside hardware owners and markup (#2292).
        Expect("class P { void Run() { // The turning wrapper enters the shared mover.\n } }", "('wrapper')");
        Expect("class P { void Run() { // The rising helper restores input while speed is fractional.\n } }", "('rising helper')");
        Expect("class P { /// <summary>Unlike the bottom-boundary helper, this does not subtract one.</summary>\nint F => 0; }",
            "('bottom-boundary helper')");
        Expect("class P { void Run() { // This helper models one pose family.\n } }", "('This helper')");
        Expect("class P { /// <summary>Restarts the animation after a scripted controller writes the pose.</summary>\nint F => 0; }",
            "('scripted controller')");
        Expect("class P { /// <summary>The four bodies Mother Brain's controllers install.</summary>\nint F => 0; }",
            "('Brain's controllers')");
        Expect("class P { void Run() { // only the selected tail controller moves it\n } }", "('tail controller')");
        Expect("class P { /// <summary>Changes the drained Samus controller/pose.</summary>\nint F => 0; }", "('controller/pose')");
        // A helper alone, the helper text, the player's controller and its input, Shaktool's room controller and a hardware
        // owner are English.
        Expect("class P { /// <summary>A helper; the helper text; the player's controller and its input; Shaktool's room controller; the flashback's scripted controller input.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the movement wrapper and the rising helper.</summary>\n[CompatibilityService(\"wrap\")] class Wrap { }");
        // A merge path, equipment and state bits, outside hardware owners and markup (#2292).
        Expect("class P { void Run() { // The merge path executes even for an unexpected pose.\n } }", "('merge path')");
        Expect("class P { /// <summary>The item room object grants the equipment bit.</summary>\nint F => 0; }", "('equipment bit')");
        Expect("class P { void Run() { // the first pose tests the other suit bit\n } }", "('suit bit')");
        Expect("class P { /// <summary>The Screw Attack bit has priority.</summary>\nint F => 0; }", "('Attack bit')");
        Expect("class P { void Run() { // a save with both bits equipped\n } }", "('both bits')");
        Expect("class P { /// <summary>Grapple physics reads this bit during a phase.</summary>\nint F => 0; }", "('this bit')");
        // A bit more, a bit of the way and a hardware owner are English.
        Expect("class P { /// <summary>It rises a bit more, a bit of the way.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the equipment bit and the merge path.</summary>\n[CompatibilityService(\"bit\")] class Bit { }");
        // A stage by number outside the Speed Booster's, outside hardware owners and markup (#2292).
        Expect("class P { /// <summary>The Varia pickup: stage three equips the suit.</summary>\nint F => 0; }", "('stage three')");
        Expect("class P { /// <summary>Stages three and four: the closing column.</summary>\nint F => 0; }", "('Stages three')");
        Expect("class P { /// <summary>Stage-two initial edge pair.</summary>\nint F => 0; }", "('Stage-two')");
        Expect("class P { /// <summary>Installs the suit at the third-stage handoff.</summary>\nint F => 0; }", "('third-stage')");
        // The Speed Booster's stages, a stage that counts the noun after it and a hardware owner are English.
        Expect("class P { /// <summary>Speed Booster stage one; boost stage four; a stage-four run slides; stage two frames long.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: stage three equips the suit.</summary>\n[CompatibilityService(\"stage\")] class Stage { }");
        // Updates counted as calls in prose and declared names, outside hardware owners and markup (#2292).
        Expect("class P { /// <summary>Sixteen calls precede the alternating flash.</summary>\nint F => 0; }", "('Sixteen calls')");
        Expect("class P { /// <summary>How many calls the destruction lasts.</summary>\nint F => 0; }", "('How many calls')");
        Expect("class P { /// <summary>Its count of nine lasts ten approach calls.</summary>\nint F => 0; }", "('ten approach calls')");
        Expect("class P { /// <summary>The timer producing the nine-call cadence.</summary>\nint F => 0; }", "('nine-call')");
        Expect("class P { /// <summary>Runs one call of the destruction.</summary>\nint F => 0; }", "('one call')");
        Expect("class P { void Run() { // The first explosion bursts on the destruction's first call.\n } }", "('first call')");
        Expect("class P { /// <summary>Calls left before the next explosion.</summary>\nint F => 0; }", "('Calls left')");
        Expect("class P { /// <summary>The objects it spawns, in call order.</summary>\nint F => 0; }", "('call order')");
        Expect("class P { int FlashCalls { get; set; } }", "('FlashCalls')");
        Expect("class P { const int ImpactSoundCall = 2; }", "('ImpactSoundCall')");
        Expect("class P { void CountPreRotDelayCall() { } }", "('CountPreRotDelayCall')");
        Expect("class P { int _holdCalls; }", "('_holdCalls')");
        Expect("class P { void M(int delayCalls) { } }", "('delayCalls')");
        // A call into another owner, one call per room, one host call, calling, a callback, a caller-free name and a
        // hardware owner are English.
        Expect("class P { /// <summary>A call to the queue; one call per room; one host call; calling it.</summary>\nint F => 0; }");
        Expect("class P { string S = \"A per-call input, built for one call and never kept.\"; }");
        Expect("class P { System.Action Callback = () => { }; bool Called; void Calling() { } }");
        Expect(CompatibilityService + "/// <summary>Evidence: sixteen calls precede it.</summary>\n[CompatibilityService(\"calls\")] class Calls { int HoldCalls; }");
        // The update-call forms that escaped #2292 (#2321): a call picked out with up to four words before it, hyphenated
        // or joined by a slash; a same-call step; rot calls; and the call as the update's own work.
        Expect("class P { /// <summary>Choosing an attack waits until the next body AI call.</summary>\nint F => 0; }",
            "('next body AI call')");
        Expect("class P { void Run() { // the first post-hit enemy call sees sixteen\n } }", "('first post-hit enemy call')");
        Expect("class P { /// <summary>Runs another beam/movement call.</summary>\nint F => 0; }", "('another beam/movement call')");
        Expect("class P { /// <summary>What one Mother Brain active-rainbow body-phase call asks of the room.</summary>\nint F => 0; }",
            "('one Mother Brain active-rainbow body-phase call')");
        Expect("class P { /// <summary>Spawns the glow with its same-call pin.</summary>\nint F => 0; }", "('same-call')");
        Expect("class P { /// <summary>The rows as its rot calls have left them.</summary>\nint F => 0; }", "('rot calls')");
        Expect("class P { /// <summary>Shows the last image; true on the call after it.</summary>\nint F => 0; }", "('on the call')");
        Expect("class P { /// <summary>The image the call shows, if it shows one.</summary>\nint F => 0; }", "('the call shows')");
        Expect("class P { void Run() { // the call that takes it to zero ends the destruction\n } }", "('the call that')");
        Expect("class P { /// <summary>The call's result.</summary>\nint F => 0; }", "('The call's result')");
        // The call alone, a call that returns to its caller, a call into another owner with words before it and a
        // hardware owner are English.
        Expect("class P { /// <summary>Each row falls before the call returns; the call; the first host call to the queue.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the first post-hit enemy call; its rot calls.</summary>\n[CompatibilityService(\"rot\")] class Rot { }");
        // Sound-effect and vulnerability flags or bits (#2321).
        Expect("class P { /// <summary>Rainbow-beam SFX-playing flag.</summary>\nint F => 0; }", "('SFX-playing flag')");
        Expect("class P { void Run() { // The Baby's drain writes the shared SFX flag.\n } }", "('SFX flag')");
        Expect("class P { void Run() { // The vulnerability bit is toggled every update.\n } }", "('vulnerability bit')");
        // A counter told by the range its storage holds (#2321).
        Expect("class P { /// <summary>Every second update of the frame counter of 0 through 65535.</summary>\nint F => 0; }",
            "('counter of 0 through 65535')");
        // Whether a sound plays, a counter's cadence and a hardware owner are English.
        Expect("class P { /// <summary>Whether her beam's sound plays; every second update of the frame counter.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the SFX flag; the frame counter of 0 through 255.</summary>\n[CompatibilityService(\"sfx\")] class Sfx { }");
        // The machine's arithmetic and the original's data told for a value, outside hardware owners and markup (#2292).
        Expect("class P { /// <summary>Gravity applies while the whole pixels, compared as signed, stay below the cap.</summary>\nint F => 0; }",
            "('compared as signed')");
        Expect("class P { /// <summary>The smaller of two velocities, read signed.</summary>\nint F => 0; }", "('read signed')");
        Expect("class P { void Run() { // Flush that deferred write first.\n } }", "('deferred write')");
        Expect("class P { void Run() { // perform the suit colours copy immediately\n } }", "('suit colours copy')");
        Expect("class P { void Run() { // Odd frames return immediately.\n } }", "('Odd frames return')");
        Expect("class P { /// <summary>The hardcoded room objects enemy AI requested.</summary>\nint F => 0; }", "('hardcoded')");
        Expect("class P { void Run() { // Up calculates the background from destination+31.\n } }", "('destination+31')");
        Expect("class P { void Run() { // the room-wide conservative +31 allowance\n } }", "('+31 allowance')");
        Expect("class P { void Run() { // top-aligned screens align at +0\n } }", "('at +0')");
        Expect("class P { /// <summary>The first pixel of the following screen, wrapping at sixteen bits.</summary>\nint F => 0; }",
            "('sixteen bits')");
        Expect("class P { /// <summary>A foot dust puff with an additional three-bit jitter.</summary>\nint F => 0; }", "('three-bit')");
        Expect("class P { /// <summary>The initial position and default movement-policy row.</summary>\nint F => 0; }",
            "('movement-policy row')");
        Expect("class P { /// <summary>The position records the movement search runs over, in search order.</summary>\nint F => 0; }",
            "('position records')");
        Expect("class P { /// <summary>Unlisted positions select the 352 row.</summary>\nint F => 0; }", "('the 352 row')");
        Expect("class P { /// <summary>The lint hitbox's left edge (-24) minus two.</summary>\nint F => 0; }", "('(-24) minus')");
        Expect("class P { /// <summary>The speed stays below 256 + 108 * 96 = 10624.</summary>\nint F => 0; }", "('256 + 108 * 96 = 10624')");
        // A signed value, a sum written with spaces, a signed list of deltas and a hardware owner are English.
        Expect("class P { /// <summary>A signed speed; a wait of n lasts n + 1 updates; +1 for frames 0..5, -1 after.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: compared as signed at destination+31, wrapping at sixteen bits.</summary>\n[CompatibilityService(\"sum\")] class Sum { }");
        // The retail game and the original's cursors, outside hardware owners and markup (#2292).
        Expect("class P { void Run() { // Retail emits a rock on the open mouth.\n } }", "('Retail')");
        Expect("class P { /// <summary>Every retail door setup.</summary>\nint F => 0; }", "('retail')");
        Expect("class P { /// <summary>Changes collision when its delayed history cursor crosses a hole.</summary>\nint F => 0; }",
            "('history cursor')");
        Expect("class P { /// <summary>The falling sequence advances the cursor.</summary>\nint F => 0; }", "('advances the cursor')");
        Expect("class P { /// <summary>The animation has ended; its cursor stays at the end.</summary>\nint F => 0; }",
            "('cursor stays at the end')");
        Expect("class P { /// <summary>The game selects each rock by a cursor.</summary>\nint F => 0; }", "('by a cursor')");
        // A menu's cursor, the step a cursor names and a hardware owner are English.
        Expect("class P { /// <summary>The cursor stays on the transfer; the options page's cursor; the step the cursor names.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: retail Evirs bob; the movement cursor crosses rows.</summary>\n[CompatibilityService(\"retail\")] class Retail { }");
        // Samus's pose data narrated as the original's numbers and tables, outside hardware owners and markup (#2322): a
        // direction read by number, on one line or across a documentation comment's lines, a table's entry or route, and
        // a transition, input or distance table.
        Expect("class P { /// <summary>Its pose X direction is deliberately eight.</summary>\nint F => 0; }", "('direction is deliberately eight')");
        Expect("class P { /// <summary>Its pose X direction is deliberately\n/// eight: movement follows travel.</summary>\nint F => 0; }",
            "('direction is deliberately\n/// eight')");
        Expect("class P { /// <summary>Their pose-X direction is zero.</summary>\nint F => 0; }", "('direction is zero')");
        Expect("class P { void Run() { // the current shot direction is two\n } }", "('direction is two')");
        Expect("class P { /// <summary>Its stored direction stores 6 when it turns.</summary>\nint F => 0; }", "('direction stores 6')");
        Expect("class P { /// <summary>Executes the bomb-jump movement table's entry for her movement.</summary>\nint F => 0; }",
            "('table's entry')");
        Expect("class P { /// <summary>Follows the landing table\u2019s routes.</summary>\nint F => 0; }", "('table\u2019s routes')");
        Expect("class P { string S = \"Crouch jump is not a transition-table route.\"; }", "('transition-table')");
        Expect("class P { void Run() { // An input-table transition out of the turn starts the jump.\n } }", "('input-table')");
        Expect("class P { void Run() { // The posture change reads five from its distance table.\n } }", "('distance table')");
        Expect("class P { /// <summary>The transition tables of the spin.</summary>\nint F => 0; }", "('transition tables')");
        // A number that counts the noun after it, a direction alone, a table alone, a table of something, the input
        // transitions and a hardware owner are English.
        Expect("class P { /// <summary>Moves two pixels in her direction; the direction is two pixels away; a speed table; a table of distances; the input transitions.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>The direction one turn on; the direction of one link; directions one full turn holds; direction zero of the rope's turn.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the shot direction is two in the transition table.</summary>\n[CompatibilityService(\"direction\")] class Direction { }");
        // The original's routines and storage told in boss and enemy prose, outside hardware owners and markup (#2336): an
        // initializer, a writer, an execution counter, ordinary places and places in ascending order, a value wrapped into
        // the coordinate range, a branch on a value, a signed compare and the roaring or damaged flag, in prose; and an
        // execution counter in a declared name.
        Expect("class P { /// <summary>Runs the Norfair branch of the shared initializer.</summary>\nint F => 0; }", "('initializer')");
        Expect("class P { void Run() { // The spin-jump initializers cancel the boost.\n } }", "('initializers')");
        Expect("class P { string S = \"N00b-tube initialiser lost its piece.\"; }", "('initialiser')");
        Expect("class P { /// <summary>Draws the wings before the common writer emits his body.</summary>\nint F => 0; }", "('writer')");
        Expect("class P { /// <summary>The body uses the current execution counter.</summary>\nint F => 0; }", "('execution counter')");
        Expect("class P { /// <summary>Checks Samus's five ordinary projectile places.</summary>\nint F => 0; }",
            "('ordinary projectile places')");
        Expect("class P { void Run() { // The check scans the shots' places in ascending order.\n } }", "('places in ascending order')");
        Expect("class P { /// <summary>The corner, retaining negative limits wrapped into the coordinate range.</summary>\nint F => 0; }",
            "('wrapped into the coordinate range')");
        Expect("class P { void Run() { // Ridley's shot reaction branches on area index.\n } }", "('branches on')");
        Expect("class P { void Run() { // The rainbow step uses a signed compare after adding sixteen.\n } }", "('signed compare')");
        Expect("class P { /// <summary>Clears the roaring flag.</summary>\nint F => 0; }", "('roaring flag')");
        Expect("class P { /// <summary>Clears the damaged flag.</summary>\nint F => 0; }", "('damaged flag')");
        Expect("class P { int MainEnemyExecutionCounter { get; init; } }", "('MainEnemyExecutionCounter')");
        Expect("class P { void M(int enemyMainExecutionCounter) { } }", "('enemyMainExecutionCounter')");
        // A C# construct's initializer, a projectile's branch, the update count, the shot places, setup, a roar ending and a
        // hardware owner are English.
        Expect("class P { /// <summary>Set before property initializers run; an object initializer; the afterburn's branches; the enemy's setup.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>The body's update count; Samus's five shot places; his roar ends.</summary>\nint F => 0; int UpdateCount; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the initializer branches on the execution counter.</summary>\n[CompatibilityService(\"init\")] class Init { int MainEnemyExecutionCounter; }");
        // The original's arithmetic told as a summary, its program structure and its leftover storage, outside hardware
        // owners and markup (#2345).
        Expect("class P { /// <summary>Adds a step: the sample divided by 256, modulo 4.</summary>\nint F => 0; }", "('divided by 256')");
        Expect("class P { /// <summary>Picks a wait: the sample modulo 8.</summary>\nint F => 0; }", "('modulo 8')");
        Expect("class P { void Run() { // the result's remainder by eight chooses a cry\n } }", "('remainder by eight')");
        Expect("class P { void Run() { // The bug's update is an if/else-if chain.\n } }", "('if/else-if')");
        Expect("class P { void Run() { // An else-if picks the wall.\n } }", "('else-if')");
        Expect("class P { /// <summary>Its call chain runs the shot.</summary>\nint F => 0; }", "('call chain')");
        Expect("class P { /// <summary>Exactly matching the leading call in the game's reaction.</summary>\nint F => 0; }", "('leading call')");
        Expect("class P { void Run() { // Custom shot AI does not call normal shot AI.\n } }", "('does not call')");
        Expect("class P { void Run() { // The collision marks the shot before calling the no-op.\n } }", "('no-op')");
        Expect("class P { void Run() { // The prologue requires the baby's parent.\n } }", "('prologue')");
        Expect("class P { /// <summary>Nothing a previous Kzan left reaches a new one.</summary>\nint F => 0; }", "('Nothing a previous')");
        Expect("class P { void Run() { // It republishes a height, which nothing reads.\n } }", "('which nothing reads')");
        Expect("class P { void Run() { // A hit draws a number the game keeps but nothing reads.\n } }", "('but nothing reads')");
        // Dividing a picture, an instruction not to call a method, Ceres the prologue station, identities nothing reads,
        // what a previous room hands the next and a hardware owner are English.
        Expect("class P { /// <summary>The picture divided into halves; do not call it twice; Ceres, the prologue station; the research station prologue.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>Its values are arbitrary and nothing reads them; the countdown the previous room's main left.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the sample divided by 256, modulo 4, in an if/else chain.</summary>\n[CompatibilityService(\"modulo\")] class Modulo { }");
        // The machine's storage and the repository's checks told for a value, outside hardware owners and markup (#2352): a
        // position's overflow, a range named by its 1/N-pixel storage, scan, target, collision and solid counters, state
        // lying below a clear, allocations that clear and routes that allocate, and a check that runs to confirm a value.
        Expect("class P { /// <summary>Runs the homing particle, keeping its 1/256-pixel acceleration and position overflow.</summary>\nint F => 0; }",
            "('position overflow')");
        Expect("class P { /// <summary>The speed stays far inside the 1/256-pixel range.</summary>\nint F => 0; }", "('the 1/256-pixel range')");
        Expect("class P { /// <summary>Negates the speed, wrapping within its signed 1/256-pixel range.</summary>\nint F => 0; }",
            "('its signed 1/256-pixel range')");
        Expect("class P { void Run() { // A surface hit clears both scan counters.\n } }", "('scan counters')");
        Expect("class P { /// <summary>Reports a collision only when the scan's solid counter completes.</summary>\nint F => 0; }",
            "('scan's solid counter')");
        Expect("class P { void Run() { // The span decrements its target counter for each stopping block.\n } }", "('target counter')");
        Expect("class P { void Run() { // Gate setup forces the collision counter.\n } }", "('collision counter')");
        Expect("class P { /// <summary>Copies the inventory, which lie below the state her setup clears.</summary>\nint F => 0; }",
            "('lie below the state her setup clears')");
        Expect("class P { void Run() { // Allocation clears the effect's colour step.\n } }", "('Allocation clears')");
        Expect("class P { void Run() { // Both collision routes allocate the shot trigger.\n } }", "('routes allocate')");
        Expect("class P { /// <remarks>The radius check runs every update of a blast against the arithmetic to confirm it.</remarks>\nint F => 0; }",
            "('check runs every update of a blast against the arithmetic to confirm')");
        // A sum that overflows, a range in pixels, a counter alone, lying below a ledge, an allocation alone, a check the game
        // makes and a hardware owner are English.
        Expect("class P { /// <summary>A sum that overflows; the 32-pixel range of the attack; the counter; the floor lies below a ledge; an allocation; the game's check runs every update.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the scan counters and the 1/65536-pixel position overflow.</summary>\n[CompatibilityService(\"scan\")] class Scan { }");
        // The original's code branches told in prose, outside hardware owners and markup (#2352): this, that, both or every
        // branch, a branch of a step, a branch taken, entered or run, a branch that does something, a branch order and
        // branching straight to something.
        Expect("class P { void Run() { // Morph Ball's radius is smaller, and that branch restores the previous pose.\n } }", "('that branch')");
        Expect("class P { /// <summary>Preserves both branches of the echo draw.</summary>\nint F => 0; }", "('both branches')");
        Expect("class P { /// <summary>Runs the deceleration branch of the speed calculation.</summary>\nint F => 0; }",
            "('Runs the deceleration branch')");
        Expect("class P { /// <summary>The hidden-wall branch of the room's shaking.</summary>\nint F => 0; }", "('branch of')");
        Expect("class P { void Run() { // A zero-damage hit takes the dud branch.\n } }", "('takes the dud branch')");
        Expect("class P { void Run() { // a fast swing takes the alternating boost/normal branch\n } }",
            "('takes the alternating boost/normal branch')");
        Expect("class P { void Run() { // The landing branch moves the game to Crateria.\n } }", "('branch moves')");
        Expect("class P { string S = \"The thaw branch leaves the clock at its remainder.\"; }", "('branch leaves')");
        Expect("class P { /// <summary>Picks the suit in the game's branch order.</summary>\nint F => 0; }", "('branch order')");
        Expect("class P { void Run() { // The step branches straight to the shots.\n } }", "('branches straight to')");
        // A room's state choice's branches, a projectile's branches, an afterburn branch, an animation that branches to
        // another, a branch alone and a hardware owner are English.
        Expect("class P { /// <summary>One branch of a room's state choice; the branches of the projectile; an afterburn branch; the swing, which branches to its release; a branch.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the wall branch stores the speed; it takes the retreating branch.</summary>\n[CompatibilityService(\"branch\")] class Branch { }");
        // Updates counted as the original's calls in frontend, escape, effect and animation prose, outside hardware owners and
        // markup (#2362): calls elapsed or made, later, remaining, gameplay or audio calls, a run-on list of calls, a thing's
        // call, calls a thing lasts or is reloaded in, a call that does something, the calls a thing moves for, a call of the
        // same update or owner, the call that consumed something and a call that saves where it resumes.
        Expect("class P { /// <summary>Calls elapsed for the explosion spawner.</summary>\nint F => 0; }", "('Calls elapsed')");
        Expect("class P { /// <summary>Cinematic calls made so far.</summary>\nint F => 0; }", "('calls made')");
        Expect("class P { /// <summary>The fade steps on its fourth, eighth and later calls.</summary>\nint F => 0; }", "('later calls')");
        Expect("class P { /// <summary>Remaining calls until the next character.</summary>\nint F => 0; }", "('Remaining calls')");
        Expect("class P { void Run() { // A door hit changes the game state during the gameplay call.\n } }", "('gameplay call')");
        Expect("class P { /// <summary>Alternates typewriter audio calls while letters appear.</summary>\nint F => 0; }", "('audio calls')");
        Expect("class P { void Run() { // Both halves step on the fourth, eighth, ... calls.\n } }", "('... calls')");
        Expect("class P { /// <summary>False until the final character's call.</summary>\nint F => 0; }", "('character's call')");
        Expect("class P { /// <summary>Each frame lasting <paramref name=\"period\"/> calls.</summary>\nint F => 0; }", "('lasting   calls')");
        Expect("class P { /// <summary>The countdown reload in <see cref=\"Step\"/> calls.</summary>\nint F => 0; }", "('in   calls')");
        Expect("class P { /// <summary>Includes the separate call that completes the fade.</summary>\nint F => 0; }",
            "('the separate call that completes')");
        Expect("class P { /// <summary>The calls the text moves down the screen for.</summary>\nint F => 0; }", "('The calls the')");
        Expect("class P { /// <summary>The fade-in state calls the same update before fading in.</summary>\nint F => 0; }",
            "('calls the same update')");
        Expect("class P { /// <summary>The statue's steps call the same earthquake sound owner.</summary>\nint F => 0; }",
            "('call the same earthquake sound owner')");
        Expect("class P { /// <summary>True while held: the call consumed one frame of the hold.</summary>\nint F => 0; }", "('the call consumed')");
        Expect("class P { /// <summary>Where the play stands, as an animation's call saves where it resumes.</summary>\nint F => 0; }",
            "('animation's call')");
        Expect("class P { /// <summary>The place a call saves to return to.</summary>\nint F => 0; }", "('call saves')");
        // A call to something, a call that returns to its caller, calls of a thing, and the verb after a subject (the producer
        // first calls the setup, only the spawn calls it) are English, and a hardware owner may say it.
        Expect("class P { /// <summary>A later call to the queue; a call that returns; the calls of the menu; the producer first calls the setup; only the spawn calls it.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the second call that completes the warm-up.</summary>\n[CompatibilityService(\"call\")] class Calls { }");
        // The machine's storage told for a value in frontend, enemy and progression prose, outside hardware owners and markup
        // (#2362): a screen value's 0 through 255 wrap, the game's byte or wrapping counter, a count that would have wrapped,
        // object handling and free object places, initialization parameters, a descending actor order, an actor being
        // processed, a named storage bit, the saved order and a projectile-backed or parameter-selected thing.
        Expect("class P { void Run() { // its screen Y, 0 through 255, can wrap below the screen\n } }", "('0 through 255, can wrap')");
        Expect("class P { /// <summary>A growing count reproduces the game's wrapping counter.</summary>\nint F => 0; }",
            "('the game's wrapping counter')");
        Expect("class P { /// <summary>Never near the 256 at which the game's byte wraps.</summary>\nint F => 0; }", "('the game's byte')");
        Expect("class P { /// <summary>The count never reaches the point where it would have wrapped.</summary>\nint F => 0; }",
            "('would have wrapped')");
        Expect("class P { void Run() { // the cinematic's object handling reaches it next\n } }", "('object handling')");
        Expect("class P { void Run() { // the subtitle takes the next free object place\n } }", "('free object place')");
        Expect("class P { void Run() { // Four init parameters select four arcs.\n } }", "('init parameters')");
        Expect("class P { void Run() { // The baby follows the egg in the descending actor order.\n } }", "('descending actor order')");
        Expect("class P { void Run() { // allocated while the first actor is already being processed\n } }", "('being processed')");
        Expect("class P { void Run() { // the enemy still receives the process-off-screen bit\n } }", "('process-off-screen bit')");
        Expect("class P { /// <summary>Members keep the saved order.</summary>\nint F => 0; }", "('saved order')");
        Expect("class P { /// <summary>The copy runs in the save memory's order.</summary>\nint F => 0; }", "('save memory's order')");
        Expect("class P { /// <summary>The Maw's projectile-backed body.</summary>\nint F => 0; }", "('projectile-backed')");
        Expect("class P { /// <summary>A parameter-selected dust cloud.</summary>\nint F => 0; }", "('parameter-selected')");
        // A range alone, a room object's place, a bit more, a three-word flag and a hardware owner are English.
        Expect("class P { /// <summary>A column 0 through 255; the room object's place; a bit more of the way; the order a save lists the items.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the quarter-turn bits wrap at 0 through 255.</summary>\n[CompatibilityService(\"bits\")] class Bits { }");
        // The original program and its host in frontend, input and save prose, outside hardware owners and markup (#2362): a
        // coroutine, a chosen handler and the handler of something, a backward scan (a numbered entry list is read already), a
        // checksum, an
        // emulator, what the original reads, a match with the game's table and engine definitions.
        Expect("class P { void Run() { // The escape calls the complete main-gameplay coroutine.\n } }", "('calls the complete main-gameplay coroutine')");
        Expect("class P { /// <summary>The save-pod coroutine's handshake.</summary>\nint F => 0; }", "('coroutine')");
        Expect("class P { void Run() { // The handler is chosen before movement.\n } }", "('handler is chosen')");
        Expect("class P { void Run() { // the handler of the category selected toggles the item\n } }", "('handler of the')");
        Expect("class P { /// <summary>The menu's nine-entry list.</summary>\nint F => 0; }", "('nine-entry')");
        Expect("class P { /// <summary>The options menu scans this list backward.</summary>\nint F => 0; }", "('scans this list backward')");
        Expect("class P { /// <summary>A valid save checksum proves only that the slot was written.</summary>\nint F => 0; }", "('checksum')");
        Expect("class P { /// <summary>Extension replacing the emulator-oriented host file.</summary>\nint F => 0; }", "('emulator')");
        Expect("class P { /// <summary>Frozen in a Norfair room, as the original reads the current area.</summary>\nint F => 0; }",
            "('the original reads')");
        Expect("class P { /// <summary>All 256 values match the game's table exactly.</summary>\nint F => 0; }", "('match the game's table')");
        Expect("class P { /// <summary>These are engine definitions, never art data.</summary>\nint F => 0; }", "('engine definitions')");
        // A handler alone, scanning a room, an original picture and a hardware owner are English.
        Expect("class P { /// <summary>A handler; Samus scans the room; the original picture is kept.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the original reads the coroutine's checksum.</summary>\n[CompatibilityService(\"original\")] class Original { }");
        // The machine's sixteen-bit coordinate arithmetic told for a distance in enemy prose, outside hardware owners and
        // markup (#2372): a wrapped, wraparound or modular distance, difference, subtraction, displacement, position or
        // proximity, a coordinate's wraparound and a move wrapping within the coordinate range.
        Expect("class P { /// <summary>Checks strict wrapped horizontal distance from an Etecoon to Samus.</summary>\nint F => 0; }",
            "('wrapped horizontal distance')");
        Expect("class P { /// <summary>Positions compare with it by the wrapped difference.</summary>\nint F => 0; }", "('wrapped difference')");
        Expect("class P { void Run() { // The row test takes the modular absolute difference.\n } }", "('modular absolute difference')");
        Expect("class P { /// <summary>Starts the spit after modular Y proximity.</summary>\nint F => 0; }", "('modular Y proximity')");
        Expect("class P { void Run() { // The bounds compare by whether the wrapped subtraction is negative.\n } }", "('wrapped subtraction')");
        Expect("class P { /// <summary>Gets the Baby's wrapped room position.</summary>\nint F => 0; }", "('wrapped room position')");
        Expect("class P { /// <summary>The spawn Y minus the radius with room-coordinate wraparound.</summary>\nint F => 0; }",
            "('room-coordinate wraparound')");
        Expect("class P { /// <summary>Moves the actor by <paramref name=\"d\"/>, wrapping within the coordinate range.</summary>\nint F(int d) => 0; }",
            "('wrapping within the coordinate range')");
        // A thing wrapped around, a turn's wrap, a distance alone and a hardware owner are English.
        Expect("class P { /// <summary>The scarf wrapped around the statue; the angle wraps around the turn; the distance to Samus.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the wrapped difference reads as signed.</summary>\n[CompatibilityService(\"wrap\")] class Wrap { }");
        // The original program's storage and calls told in enemy prose, outside hardware owners and markup (#2372): installing
        // or loading art, the game storing, checking or subtracting a value, a completion or draining call, table
        // multipliers and a value of 0 through 255.
        Expect("class P { /// <summary>Installs crawling art for its heading.</summary>\nint F => 0; }", "('Installs crawling art')");
        Expect("class P { /// <summary>Loads the long-hop place and installs hop art.</summary>\nint F => 0; }", "('installs hop art')");
        Expect("class P { void Run() { // The game subtracts one mark from the clock.\n } }", "('The game subtracts')");
        Expect("class P { void Run() { // The game stores the increment first.\n } }", "('The game stores')");
        Expect("class P { void Run() { // the game checks whether it is rising\n } }", "('the game checks')");
        Expect("class P { /// <summary>Shows the rotted rows, including on the completion call.</summary>\nint F => 0; }", "('completion call')");
        Expect("class P { /// <summary>The draining calls each stage lasts.</summary>\nint F => 0; }", "('draining calls')");
        Expect("class P { void Run() { // the angle feeds the buggy table multipliers\n } }", "('table multipliers')");
        Expect("class P { /// <summary>The shard launches at a random angle of 0 through 255.</summary>\nint F => 0; }", "('of 0 through 255')");
        // Installed art and loading installed art, a table, a multiplier, a range alone and a hardware owner are English.
        Expect("class P { /// <summary>Draws it with installed sprite art; loads installed flight art; a table; the speed multiplier; a column 0 through 255.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the game stores the count in a value of 0 through 255.</summary>\n[CompatibilityService(\"store\")] class Store { }");
        // An enemy's presence, contact or processing told as a stored property, outside hardware owners and markup (#2377):
        // an intangible, invisible, hide-drawing, animating, off-screen, shot- or plasma-blocking, contact or listed property.
        Expect("class P { void Run() { // The intangible property removes the dead body from the list.\n } }", "('intangible property')");
        Expect("class P { void Run() { // The invisibility property controls rendering only.\n } }", "('invisibility property')");
        Expect("class P { void Run() { // preserve the hide-drawing property\n } }", "('hide-drawing property')");
        Expect("class P { void Run() { // The setup sets the animating, intangibility and invisibility properties.\n } }",
            "('intangibility and invisibility properties')");
        Expect("class P { void Run() { // He starts with both of his contact properties.\n } }", "('contact properties')");
        Expect("class P { void Run() { // sets their process-off-screen property below\n } }", "('process-off-screen property')");
        Expect("class P { void Run() { // The plasma-blocking enemy property also forces the mark.\n } }", "('plasma-blocking enemy property')");
        // A property of a thing, an enemy's other properties, a projectile's property set or mask and a hardware owner are
        // English.
        Expect("class P { /// <summary>A property of the frame; its other properties are kept; the animation's property set and property mask.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the death leaves the dying enemy's respawn property set.</summary>\n[CompatibilityService(\"respawn\")] class Respawn { }");
        // The original program's steps and arithmetic told in enemy prose, outside hardware owners and markup (#2377): a
        // post- or pre-decrement, a main-AI call or AI invocation, running the code's loop, the handler's guard, a signed
        // wrapped comparison and a strict modular range.
        Expect("class P { /// <summary>Runs the escape, including post-decrement timer semantics.</summary>\nint F => 0; }", "('post-decrement')");
        Expect("class P { void Run() { // the pixelation reads this pre-increment value\n } }", "('pre-increment')");
        Expect("class P { void Run() { // The flag was produced at the end of the preceding main-AI call.\n } }", "('main-AI call')");
        Expect("class P { /// <summary>Spends one AI invocation choosing its movement.</summary>\nint F => 0; }", "('AI invocation')");
        Expect("class P { /// <summary>Runs the projectile-link loop.</summary>\nint F => 0; }", "('Runs the projectile-link loop')");
        Expect("class P { void Run() { // but retain the handler's literal guard\n } }", "('the handler's')");
        Expect("class P { /// <summary>Compares only the whole pixels as signed wrapped values.</summary>\nint F => 0; }", "('signed wrapped')");
        Expect("class P { /// <summary>Opens only inside strict modular X and Y ranges.</summary>\nint F => 0; }", "('strict modular')");
        // A handler alone, an animation's loop, a decrement or a call alone and a hardware owner are English.
        Expect("class P { /// <summary>A handler; the idle animation's loop; each decrement; a call to the queue.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the handler's post-decrement test runs the main loop.</summary>\n[CompatibilityService(\"loop\")] class Loop { }");
        // The original program's and the port's machinery told in world, runtime and Samus prose, outside hardware owners
        // and markup (#2386): an allocator, a renderer, a streamer, a streamed view, background streaming or a streaming
        // plane, a saved game state, a matcher, a translated thing and progression or controller bits.
        Expect("class P { void Run() { // preserve the room-object allocator's false result\n } }", "('allocator')");
        Expect("class P { /// <summary>Binds the death-state renderer.</summary>\nint F => 0; }", "('renderer')");
        Expect("class P { /// <summary>Holds the camera and background streamer.</summary>\nint F => 0; }", "('streamer')");
        Expect("class P { void Run() { // the first frame would already have streamed a viewport\n } }", "('streamed')");
        Expect("class P { /// <summary>Runs the main scrolling and background streaming.</summary>\nint F => 0; }", "('background streaming')");
        Expect("class P { void Run() { // not over a streaming background plane\n } }", "('streaming background')");
        Expect("class P { /// <summary>Rebound when a saved game state is restored.</summary>\nint F => 0; }", "('saved game state')");
        Expect("class P { void Run() { // The table matcher clears the request.\n } }", "('matcher')");
        Expect("class P { string M => \"Cached movement has no translated movement-type mover.\"; }", "('translated')");
        Expect("class P { /// <summary>Marks the saves in area progression bits.</summary>\nint F => 0; }", "('progression bits')");
        Expect("class P { /// <summary>Supplies newly pressed controller bits.</summary>\nint F => 0; }", "('controller bits')");
        // The host's renderer, hair or jets streaming out, a thing streamed out, a position translated by an offset or to
        // an origin, a save and a hardware owner are English.
        Expect("class P { /// <summary>The host's renderer; her hair streaming out; the jets streamed out; the scene is not translated to this origin; a position translated by an offset; the game's saves.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the allocator's own order and the renderer.</summary>\n[CompatibilityService(\"order\")] class Order { }");
        // Updates counted as calls by number in Samus prose, outside hardware owners and markup (#2390): a call by its
        // number, calls by number in a list or a range, odd or even calls, and the super-special name.
        Expect("class P { void Run() { // The flash plays the impact sound exactly on call two.\n } }", "('on call two')");
        Expect("class P { /// <summary>Selected on hurt-counter calls one, three, and five.</summary>\nint F => 0; }",
            "('calls one, three, and five')");
        Expect("class P { void Run() { // Only calls one through six write colours.\n } }", "('calls one through six')");
        Expect("class P { void Run() { // Odd calls select the fixed hurt shade.\n } }", "('Odd calls')");
        Expect("class P { /// <summary>Applied once the frame's super-special transition owners decline it.</summary>\nint F => 0; }",
            "('super-special')");
        // A call that counts the noun after it, a call to something, updates counted by their order and a hardware owner
        // are English.
        Expect("class P { /// <summary>The producer calls two methods; a call to the queue; the flash's first and third updates; a special transition.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the flash's colours change on calls one through six.</summary>\n[CompatibilityService(\"flash\")] class Flash { }");
        // The port's narration of itself in Samus and world prose, outside hardware owners and markup (#2398): anything
        // inspectable and patches told as fixes to the game's code.
        Expect("class P { /// <summary>Flash presentation is kept as inspectable state.</summary>\nint F => 0; }", "('inspectable')");
        Expect("class P { void Run() { // the directly inspectable frame below\n } }", "('inspectable')");
        Expect("class P { /// <summary>The check handles them without per-input patches.</summary>\nint F => 0; }",
            "('without per-input patches')");
        Expect("class P { void Run() { // shared behaviour, not room-name patches\n } }", "('room-name patches')");
        // A picture's patch and a hardware owner are English.
        Expect("class P { /// <summary>The text patches a save slot's fields draw over a page; the page's name patch.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the inspectable latch.</summary>\n[CompatibilityService(\"latch\")] class Latch { }");
        // A table told as the original's storage, outside hardware owners and markup (#2398): the game's or the
        // original's tables, a direction or lookup table, a table's height and a table-selected pose.
        Expect("class P { void Run() { // indexing the game table by the mirrored point\n } }", "('game table')");
        Expect("class P { /// <summary>Restores a save through the game's area/load-station tables.</summary>\nint F => 0; }",
            "('game's area/load-station tables')");
        Expect("class P { void Run() { // The swinging poses bypass the direction tables.\n } }", "('direction tables')");
        Expect("class P { /// <summary>Expresses the mapping without a lookup table.</summary>\nint F => 0; }", "('lookup table')");
        Expect("class P { void Run() { // A table height at or above the point is solid.\n } }", "('table height')");
        Expect("class P { void Run() { // Only the six table-selected poses launch.\n } }", "('table-selected')");
        // A table alone or named for its data and a hardware owner are English.
        Expect("class P { /// <summary>A drop-chance table; the speed table; the vulnerability table; a table.</summary>\nint F => 0; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the shot indexes the game's tables.</summary>\n[CompatibilityService(\"table\")] class Table { }");
        // A repository path in prose, outside hardware owners and generated text (#2398): a path under the repository's
        // top folders and a documentation or script file.
        Expect("class P { /// <summary>Nothing else may read it (#1329, <c>docs/room-scope.md</c>).</summary>\nint F => 0; }",
            "('docs/room-scope.md')");
        Expect("class P { void Run() { // regenerate with tools/generate-placements.ps1\n } }", "('tools/generate-placements.ps1')");
        Expect("class P { /// <summary>See architecture-audit.md for the kinds.</summary>\nint F => 0; }", "('architecture-audit.md')");
        // An installed file's name, a generated file's header and a hardware owner are English.
        Expect("class P { /// <summary>Reads manifest.json from the installation.</summary>\nint F => 0; }");
        Expect("// <auto-generated/>\n/// <summary>Regenerate with tools/generate-placements.ps1.</summary>\nclass P { }");
        Expect(CompatibilityService + "/// <summary>Evidence: <c>csharp/test-fixtures/movement-release/angles.csv</c>.</summary>\n[CompatibilityService(\"slope\")] class Slope { }");
        // Translated in a declared name, outside hardware owners (#2398).
        Expect("class P { int CaptureTranslatedRenderSnapshot() => 0; }", "('CaptureTranslatedRenderSnapshot')");
        Expect("class P { void M() { bool hasTranslatedBodyChange = true; } }", "('hasTranslatedBodyChange')");
        Expect("class P { int _untranslatedPages; }", "('_untranslatedPages')");
        // A name that holds no Translated word and a hardware owner are English.
        Expect("class P { int TranslateBy() => 0; int Translation; }");
        Expect(CompatibilityService + "/// <summary>Evidence: the turn.</summary>\n[CompatibilityService(\"turn\")] class Turn { bool isTranslatedTurnPose; }");
    }
}
