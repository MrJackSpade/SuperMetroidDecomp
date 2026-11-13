using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1400 Core vocabulary rule, SME6438.</summary>
internal static class CoreVocabularyAnalyzerTests
{
    /// <summary>One positive per hardware word, the near misses that share its letters, and the documentation allowance.</summary>
    internal static void VerifyCoreVocabularyAnalyzer()
    {
        var analyzer = new CoreVocabularyAnalyzer();
        void Expect(string source, string? message = null) =>
            VerifySnippet(source, message is null ? null : CoreVocabularyAnalyzer.HardwareWordId, message, analyzer,
                assemblyName: CoreLayerAnalyzer.CoreAssembly);

        // Each word, in each kind of declaration the rule reads.
        Expect("class SnesColor { }", "'SnesColor' names the hardware ('Snes')");
        Expect("class P { int vramOffset; }", "'vramOffset' names the hardware ('Vram')");
        Expect("class P { int CgramIndex { get; set; } }", "'CgramIndex' names the hardware ('Cgram')");
        Expect("class P { void ClearOam() { } }", "'ClearOam' names the hardware ('Oam')");
        Expect("class P { void Draw(int objSize) { } }", "'objSize' names the hardware ('Obj')");
        Expect("class P { void Run() { int dmaLength = 0; _ = dmaLength; } }", "'dmaLength' names the hardware ('Dma')");
        Expect("enum Effect { None, HdmaGradient }", "'HdmaGradient' names the hardware ('Hdma')");
        Expect("class P { event System.Action? NmiStarted; }", "'NmiStarted' names the hardware ('Nmi')");
        Expect("class Queue<TApuPort> { }", "'TApuPort' names the hardware ('Apu')");
        Expect("namespace Audio.Spc { class P { } }", "'Spc' names the hardware ('Spc')");
        Expect("class P { int Run() { int Echo(int dspValue) => dspValue; return Echo(0); } }", "'dspValue' names the hardware ('Dsp')");
        Expect("class P { int Run() { int WramWord() => 0; return WramWord(); } }", "'WramWord' names the hardware ('Wram')");
        Expect("class P { void Run(int[] values) { foreach (int ppuLine in values) { } } }", "'ppuLine' names the hardware ('Ppu')");
        Expect("class P { void Run(object value) { if (value is int nativeWord) { } } }", "'nativeWord' names the hardware ('Native')");
        Expect("record RomTable(int First);", "'RomTable' names the hardware ('Rom')");
        // Storage widths, save memory, opcodes and the program's labels (#1558).
        Expect("class P { int PresentationWordCount => 0; }", "'PresentationWordCount' names the hardware ('Word')");
        Expect("class P { void Run() { int frameCounterByte = 0; } }", "'frameCounterByte' names the hardware ('Byte')");
        Expect("class P { void SaveToSram() { } }", "'SaveToSram' names the hardware ('Sram')");
        Expect("class P { void SpawnEprojInner() { } }", "'SpawnEprojInner' names the hardware ('Eproj')");
        // The cartridge's record headers, in each kind of declaration a renamed identity used them in (#1685).
        Expect("sealed class RoomHeaderDefinition { }", "'RoomHeaderDefinition' names the hardware ('Header')");
        Expect("static class SamusProjectileHeaders { }", "'SamusProjectileHeaders' names the hardware ('Header')");
        Expect("class P { int Header { get; } }", "'Header' names the hardware ('Header')");
        Expect("class P { void SpawnEnemyDropFromEnemyHeader() { } }", "'SpawnEnemyDropFromEnemyHeader' names the hardware ('Header')");
        Expect("class P { void Spawn(int roomObjectHeader) { } }", "'roomObjectHeader' names the hardware ('Header')");
        Expect("class P { void Run() { int header = 0; _ = header; } }", "'header' names the hardware ('Header')");
        // Words that begin with the same letters are the game's own: a facing, a head, a heading.
        Expect("class P { int Heading; int Head; int Headroom; int Headbutt; int HeadedLeft; }");
        // The cartridge's sprite pieces, the display's raster lines and its background tilemaps, in each kind of
        // declaration a renamed Core identifier used them in (#1694).
        Expect("static class CrocomireBodyFrameSpritemaps { }", "'CrocomireBodyFrameSpritemaps' names the hardware ('Spritemap')");
        Expect("class P { void DrawRidleyWorldSpritemap() { } }", "'DrawRidleyWorldSpritemap' names the hardware ('Spritemap')");
        Expect("class P { bool SpritemapFlashing { get; set; } }", "'SpritemapFlashing' names the hardware ('Spritemap')");
        Expect("class P { void Run() { int spritemapX = 0; _ = spritemapX; } }", "'spritemapX' names the hardware ('Spritemap')");
        Expect("class P { int _lightBeamScanlines; }", "'_lightBeamScanlines' names the hardware ('Scanline')");
        Expect("class P { P(int firstScanline) { } }", "'firstScanline' names the hardware ('Scanline')");
        Expect("class P { void LoadPauseMapTilemap() { } }", "'LoadPauseMapTilemap' names the hardware ('Tilemap')");
        Expect("class P { static int ScrollingSkyTilemapsLandingSite { get; } }", "'ScrollingSkyTilemapsLandingSite' names the hardware ('Tilemap')");
        Expect("class P { string Name(int tilemap) => \"\"; }", "'tilemap' names the hardware ('Tilemap')");
        // A sprite, a map sprite, a scan, a screen line, a tile and a room layer named as the destination engine names it
        // (two words: TileMap) are the game's own.
        Expect("class P { int Sprite; int MapSpriteId; int SpriteSheets; int ScanLeftToRight; int Scanned; int Scanner; int FirstLine; int HudLines; }");
        Expect("class P { int Tile; int Tiles; int Metatile; int MapTiles; int TileMap; int TileMapLayer; }");
        // The enemy record's AI function field, in each kind of declaration a renamed behaviour state used it in (#1701).
        Expect("public enum KraidAiFunction { None }", "'KraidAiFunction' names the hardware ('AiFunction')");
        Expect("public enum BeetomEnemyFunction { DecideAction = 1 }", "'BeetomEnemyFunction' names the hardware ('EnemyFunction')");
        Expect("class P { int FightFunction { get; set; } }", "'FightFunction' names the hardware ('FightFunction')");
        Expect("class P { int SavedFightFunction { get; set; } }", "'SavedFightFunction' names the hardware ('FightFunction')");
        Expect("static class PipeBugCodes { internal static readonly int[] PipeBugEnemyFunctions = []; }",
            "'PipeBugEnemyFunctions' names the hardware ('EnemyFunction')");
        Expect("class P { void Run(int aiFunction) { } }", "'aiFunction' names the hardware ('AiFunction')");
        Expect("class P { void Run() { int enemyFunctionTimer = 0; _ = enemyFunctionTimer; } }",
            "'enemyFunctionTimer' names the hardware ('EnemyFunction')");
        Expect("class P { int AIFunction; }", "'AIFunction' names the hardware ('AiFunction')");
        // The words apart or in another order, and words that only share the field's letters, are the game's own: a phase,
        // an action, a fight, an enemy, a functional part.
        Expect("class P { int EnemyState; int FightPhase; int AiState; int FunctionalEnemy; int EnemyFunctional; int Functor; int Func; int Functionality; }");
        // The record's function field alone, in each kind of declaration a renamed behaviour state, wait or cue used it in
        // (#1701): an enum, a state's property, a cue record and its member, a timer, a method, an enum member, a plural
        // table, a parameter and a local, none of which holds a delegate.
        Expect("public enum TorizoFunction { Idle = 1 }",
            "'TorizoFunction' names the enemy record's AI function field ('Function') where it declares no delegate");
        Expect("enum Phase { Idle } class P { Phase? Function { get; set; } }",
            "'Function' names the enemy record's AI function field ('Function') where it declares no delegate");
        Expect("enum Phase { Idle } abstract record Cue { internal sealed record SetFunction(Phase Phase) : Cue; }",
            "'SetFunction' names the enemy record's AI function field ('Function') where it declares no delegate");
        Expect("enum Phase { Idle } record SetPhase(Phase Function);",
            "'Function' names the enemy record's AI function field ('Function') where it declares no delegate");
        Expect("class P { int FunctionTimer { get; set; } }",
            "'FunctionTimer' names the enemy record's AI function field ('Function') where it declares no delegate");
        Expect("class P { void TickKraidFunctionTimer() { } }",
            "'TickKraidFunctionTimer' names the enemy record's AI function field ('Function') where it declares no delegate");
        Expect("enum KraidPhase { HandleFunctionTimer }",
            "'HandleFunctionTimer' names the enemy record's AI function field ('Function') where it declares no delegate");
        Expect("static class Tables { internal static readonly int[] SegmentFunctions = []; }",
            "'SegmentFunctions' names the enemy record's AI function field ('Function') where it declares no delegate");
        Expect("enum Phase { Idle } class P { void Run(Phase function) { } }",
            "'function' names the enemy record's AI function field ('Function') where it declares no delegate");
        Expect("enum Phase { Idle } class P { void Run() { Phase nextFunction = Phase.Idle; _ = nextFunction; } }",
            "'nextFunction' names the enemy record's AI function field ('Function') where it declares no delegate");
        // A method's name is its own even when it returns a delegate.
        Expect("class P { System.Func<int> ReadFunction() => () => 0; }",
            "'ReadFunction' names the enemy record's AI function field ('Function') where it declares no delegate");
        // A C# delegate the port passes is a genuine function: a delegate type, and a field, property, parameter or local
        // whose type is a delegate or a collection of them.
        Expect("delegate int ScoreFunction(); class P { System.Func<int>? Function; System.Func<int, int> HashFunction { get; } = x => x; }");
        Expect("class P { int Run(System.Func<int> function) { System.Func<int>[] functions = [function]; return functions[0](); } }");
        // A file is named for its type: a delegate type's file keeps the word, an enum's file does not.
        ExpectFile("delegate int ScoreFunction();", "ScoreFunction.cs");
        ExpectFile("public enum TorizoFunction { Idle = 1 }", "TorizoFunction.cs",
            "'TorizoFunction' names the enemy record's AI function field ('Function')",
            "'TorizoFunction' names the enemy record's AI function field ('Function')");
        // The original's function-pointer dispatch, in each kind of declaration a renamed Core identifier used it in (#1624):
        // an enum of room behaviours and its catalog, a record's member, a plural catalog, a method, a constant, a
        // parameter and a local, none of which holds a delegate.
        Expect("enum RoomMainCallback { None }", "'RoomMainCallback' names the original's dispatch ('Callback')");
        Expect("static class RoomCallbackDefinitions { }", "'RoomCallbackDefinitions' names the original's dispatch ('Callback')");
        Expect("enum Choice { NoWave } record struct Definition(Choice Callback);", "'Callback' names the original's dispatch ('Callback')");
        Expect("static class SamusBeamCallbacks { }", "'SamusBeamCallbacks' names the original's dispatch ('Callback')");
        Expect("class P { bool HasDeadTourianCorpseShotCallback() => true; }",
            "'HasDeadTourianCorpseShotCallback' names the original's dispatch ('Callback')");
        Expect("class P { const int EscapeDachoraPixelsPerCallback = 6; }",
            "'EscapeDachoraPixelsPerCallback' names the original's dispatch ('Callback')");
        Expect("enum Step { None } class P { bool Run(Step callback) => callback == Step.None; }",
            "'callback' names the original's dispatch ('Callback')");
        Expect("class P { void Run() { bool validMainCallback = true; _ = validMainCallback; } }",
            "'validMainCallback' names the original's dispatch ('Callback')");
        // A method's name is its own even when it returns a delegate.
        Expect("class P { System.Action ReadCallback() => () => { }; }", "'ReadCallback' names the original's dispatch ('Callback')");
        // A C# delegate the port passes is a genuine callback: a delegate type, an event, and a field, property, parameter,
        // local or pattern variable whose type is a delegate or a collection of them.
        Expect("delegate void FrameCallback(); class P { event System.Action? DoneCallback; }");
        Expect("class P { System.Action? AudioCallback; System.Func<int>? ReadCallback { get; } System.Action[] Callbacks = []; }");
        Expect("class P { System.Collections.Generic.List<System.Func<int>> PendingCallbacks = []; }");
        Expect("class P { int Run(System.Func<int> callback) { System.Func<int> nextCallback = callback; return nextCallback(); } }");
        Expect("class P { void Run(object value) { if (value is System.Action callback) callback(); } }");
        Expect("record Hook(System.Action Callback);");
        // Words that share its letters are the game's own.
        Expect("class P { int Call; int Called; int Recall; int Back; int Fallback; }");
        // A file is named for its type: a delegate type's file keeps the word, another type's file does not.
        ExpectFile("delegate void FrameCallback();", "FrameCallback.cs");
        ExpectFile("enum RoomMainCallback { None }", "RoomMainCallback.cs",
            "'RoomMainCallback' names the original's dispatch ('Callback')", "'RoomMainCallback' names the original's dispatch ('Callback')");
        Expect("class P { int Banked; }", "'Banked' names the hardware ('Bank')");
        Expect("class P { int[] Pointers = []; }", "'Pointers' names the hardware ('Pointer')");
        Expect("class P { void Run() { try { } catch (System.Exception addressing) { _ = addressing; } } }", "'addressing' names the hardware ('Address')");
        Expect("interface ICartridgeSource { }", "'ICartridgeSource' names the hardware ('Cartridge')");
        Expect("class Mode7Floor { }", "'Mode7Floor' names the hardware ('Mode7')");
        // #1481: routine labels name the CPU's return and jump instructions, and an address spelled into a name is the hardware.
        Expect("enum FrameBehavior { RTS_86A3B0 }", "'RTS_86A3B0' names the hardware ('86A3B0')");
        Expect("enum FrameBehavior { Return_86A3B0 }", "'Return_86A3B0' names the hardware ('86A3B0')");
        Expect("enum FrameBehavior { Rts }", "'Rts' names the hardware ('Rts')");
        Expect("class P { void JslTarget() { } }", "'JslTarget' names the hardware ('Jsl')");
        Expect("class P { int Instr_A0B3; }", "'Instr_A0B3' names the hardware ('A0B3')");
        // Hexadecimal-looking words without a digit, and short or long digit runs, are ordinary words and numbers.
        Expect("class P { int FaceDead; int Bead; int Frame12; int Value123; int Step12345; }");
        // #1554: a cartridge value spelled in hexadecimal after a word, in any declaration the rule reads.
        Expect("enum SamusMovement { Standing, Unused0B }", "'Unused0B' spells the cartridge value '0B' in hexadecimal");
        Expect("enum SamusPose { AdjacentCodePoseFD }", "'AdjacentCodePoseFD' spells the cartridge value 'FD' in hexadecimal");
        Expect("class P { int VisorBackdrop2A; }", "'VisorBackdrop2A' spells the cartridge value '2A' in hexadecimal");
        Expect("class P { void Draw() { int specialType1BBottom = 0; _ = specialType1BBottom; } }",
            "'specialType1BBottom' spells the cartridge value '1B' in hexadecimal");
        Expect("class P { void Draw(int frame9FA0) { } }", "'frame9FA0' spells the cartridge value '9FA0' in hexadecimal");
        // An address run into a name is a code, whatever the case of its letters, up to the next word.
        Expect("enum Frame { Projectile86E314Frame00 }", "'Projectile86E314Frame00' spells the cartridge value '86E314' in hexadecimal");
        Expect("enum Frame { Projectile86C76eFrame00 }", "'Projectile86C76eFrame00' spells the cartridge value '86C76e' in hexadecimal");
        Expect("enum Frame { Projectile86Caa4Frame00 }", "'Projectile86Caa4Frame00' spells the cartridge value '86Caa4' in hexadecimal");
        Expect("class SmallEnergyDrop0CE1E { }", "'SmallEnergyDrop0CE1E' spells the cartridge value '0CE1E' in hexadecimal");
        // A type's members numbered in hexadecimal are one family: each member is reported, decimal-looking ones included.
        VerifySnippetIds("enum Row { Row8, Row9, RowA, RowB }", analyzer, CoreLayerAnalyzer.CoreAssembly,
            CoreVocabularyAnalyzer.HardwareWordId, CoreVocabularyAnalyzer.HardwareWordId, CoreVocabularyAnalyzer.HardwareWordId,
            CoreVocabularyAnalyzer.HardwareWordId);
        VerifySnippetIds("class P { int Backdrop28; int Backdrop2A; }", analyzer, CoreLayerAnalyzer.CoreAssembly,
            CoreVocabularyAnalyzer.HardwareWordId, CoreVocabularyAnalyzer.HardwareWordId);
        // Words that read as hexadecimal, lone letters, acronyms and decimal ordinals, padded or not, are the game's own.
        Expect("class P { int Add; int Bad; int Face; int Dead; int Feed; int Fade; int BossA; int BossB; int Shift8AddMagnitude; }");
        Expect("class P { int Frame05; int Frame09; int Frame10; int Mode0; int Rgb565; int ColorABGR; int Row2Dead; int Vector3dEdge; }");
        Expect("enum Boss { BossA, BossB, BossC, BossD }");
        Expect("enum Frame { Dachora00, Dachora09, Dachora10, Dachora19 }");
        // A checked-in generator's output is Core source like any other: its marker does not hide it (#1452).
        Expect("// <auto-generated/>\nclass SnesColor { }", "'SnesColor' names the hardware ('Snes')");
        Expect("using System.Linq; class P { int Run(int[] values) => (from register in values select register).Sum(); }",
            "'register' names the hardware ('Register')");
        // An acronym written in capitals is the same word.
        Expect("class OAMBuffer { }", "'OAMBuffer' names the hardware ('Oam')");

        // Words that merely share letters with a hardware word are the game's own.
        Expect("class Objective { int Roman; int Banker; int Pointed; int Addressee; int Nativity; int Registry; int Dsplay; }");
        Expect("class P { object Mode; int Mode8; }");
        // A record's positional parameter and its property are one declaration, reported once.
        Expect("record Span(int VramStart);", "'VramStart' names the hardware ('Vram')");
        // A compatibility service may say in its documentation which hardware behaviour it reproduces.
        Expect("""
            sealed class CompatibilityServiceAttribute : System.Attribute { }
            /// <summary>Reproduces when the sound program acknowledges a request, as the SPC port handshake did.</summary>
            [CompatibilityService]
            sealed class SoundAcceptanceClock { }
            """);
        // Outside Core the import pipeline may name the ROM.
        VerifySnippet("class RomReader { }", analyzer: analyzer, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>
    /// Compiles one Core source at <paramref name="path"/> and checks that SME6438 reports exactly
    /// <paramref name="messages"/>, each diagnostic containing its message, in source order (the file name's first).
    /// </summary>
    private static void ExpectFile(string source, string path, params string[] messages)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: path);
        var compilation = CSharpCompilation.Create(CoreLayerAnalyzer.CoreAssembly, [tree], References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers([new CoreVocabularyAnalyzer()]).GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult()
            .OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        if (diagnostics.Length != messages.Length ||
            diagnostics.Where((diagnostic, index) => diagnostic.Id != CoreVocabularyAnalyzer.HardwareWordId ||
                diagnostic.Severity != DiagnosticSeverity.Error ||
                !diagnostic.GetMessage().Contains(messages[index], StringComparison.Ordinal)).Any())
        {
            throw new InvalidOperationException(
                $"Expected [{string.Join("; ", messages)}] for {path}, got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in {source}");
        }
    }
}
