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
        Expect("class P { /* the NMI handler */ }", "('NMI')");
        Expect("class P { string Name => \"OAM capacity\"; }", "('OAM')");
        Expect("class P { string Name => $\"{1} native words\"; }", "('native')");
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
        Expect("class P { /// <summary>Registers the spike's touch handler, registered once.</summary>\nvoid Run() { } }");
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
        Expect("class P { /// <summary>A function of each colour; a pure function of the velocity; thresholds and functions are copied from retail arithmetic.</summary>\nint F => 0; }");
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
        Expect("class P { // the grab falls through into the carry on the grabbing update\n }");
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
        Expect("class P { /// <summary>The body's physical components and physical motion; the place's slot is free.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>The host's save slots; the host persists them and reports an exception it catches.</summary>\nint F => 0; }");
        Expect("class P { // the first host frame on which the wait expires; the hosts' slot list\n }");
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
        // Text characters, a type of text, people, markup, per-line effects and other memory are not the display's.
        Expect("class P { /// <summary>Types the warning one character at a time; the typed characters and the message's characters wait.</summary>\nint F => 0; }");
        Expect("class P { // a two-call character delay, the character cadence and the next character\n }");
        Expect("class P { /// <summary>Width of one text character; the letters' characters and the narration characters.</summary>\nint F => 0; }");
        Expect("class P { /// <summary>Shows <paramref name=\"character\"/> at its place.</summary>\nvoid Show(int character) { } }");
        Expect("class EndingTypewriter { /// <summary>The installed characters.</summary>\nint F => 0; }");
        Expect("class IntroTextCell { // the cell's characters\n }");
        Expect("class P { // the game's characters and every playable character; characters who speak\n }");
        Expect("class P { /// <summary>The room's per-line effects and per-line scroll run first; the credits table and the memory of the room.</summary>\nint F => 0; }");
        // A compound name in a comment is a routine label unless Core's code spells it (#1558).
        Expect("class P { // as EnemyMain does\n }", "('EnemyMain')");
        Expect("class P { // set by InitAI\n }", "('InitAI')");
        Expect("class P { int SamusPose => 0; // SamusPose holds the pose\n }");
        Expect("class P { int Pose(int frameCount) => frameCount; // frameCount counts frames\n }");
        Expect("class P { /// <summary>See <see cref=\"Pose\"/>.</summary>\nint Pose => 0; }");
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
        Expect("/// <summary>The cursor moves directly to the next page; the debug grapple selection stays.</summary>\nclass P { }");
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
    }
}
