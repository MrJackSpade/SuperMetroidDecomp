using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
private static int Main(string[] args)
{
// This is deliberately a plain console executable rather than an xUnit/MSTest project.
// It keeps the reverse-engineering workspace dependency-free and makes every check easy
// to step through in Visual Studio. A failed check throws immediately with concrete state.
// Windows otherwise turns an unhandled CLR assertion into a modal "unknown software
// exception" dialog. That is actively hostile to an automated verifier: the useful stack
// trace belongs in this console and a dialog must never steal focus or stall the process.
if (OperatingSystem.IsWindows())
    NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);

try
{
if (args is ["--lookup-stream-1-atmospheric-attributes"])
{
    string sourceRom = Path.GetFullPath("Super Metroid.smc");
    var rom = CartridgeImportAddressSpace.LoadRetailRom(sourceRom);
    VerifyLookupStream1AtmosphericAttributes(rom);
    VerifySamusAtmosphereArtworkBoundary(sourceRom);
    return 0;
}
if (args is ["--lookup-stream-1-cannon-drawing"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    VerifyLookupStream1CannonDrawingControls(rom);
    VerifyLookupStream1CannonPoses(rom);
    return 0;
}
if (args is ["--lookup-stream-1-cannon-poses"])
{
    VerifyLookupStream1CannonPoses(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream-1-animation-pointers"])
{
    string sourceRom = Path.GetFullPath("Super Metroid.smc");
    var rom = CartridgeImportAddressSpace.LoadRetailRom(sourceRom);
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Animation pointer oracle revision");
    VerifySamusAnimationDelayDefinitions(rom, sourceRom);
    VerifyLookupStream1AnimationAliases(rom);
    return 0;
}
if (args is ["--lookup-stream-1-hud-posture"])
{
    VerifyLookupStream1HudPosture(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-projectile-identity-geometry"])
{
    VerifyLookupStream2ProjectileIdentityGeometry(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-narration-layout"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    var json = IntroNarrationExtractor.Extract(rom);
    VerifyStream3NarrationLayout(rom, json, IntroNarrationPresentation.Load(new MemoryStream(json)));
    Console.WriteLine("Narration layout: 770 native glyph/coordinate records, six calculated pages, one retained hard break and 34 independent edits pass.");
    return 0;
}
if (args is ["--lookup-stream-1-dachora-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Dachora program oracle revision");
    VerifyLookupStream1EscapeDachoraPrograms(rom);
    return 0;
}
if (args is ["--lookup-stream-1-powamp-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Powamp program oracle revision");
    VerifyLookupStream1PowampPrograms(rom);
    return 0;
}
if (args is ["--lookup-intro-narration-registry"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var narrationSources = IntroNarrationDefinitions.Pages.ToArray();
        AssertEqual(6, narrationSources.Length, "narration native registry has six pages");
        byte[] narrationJson = SuperMetroid.AssetExtraction.IntroNarrationExtractor.Extract(rom);
        var narration = IntroNarrationPresentation.Load(new MemoryStream(narrationJson, writable: false));
        for (int page = 0; page < narrationSources.Length; page++)
        {
            IntroNarrationNativePage source = narrationSources[page];
            AssertEqual((IntroNarrationPageId)(page + 1), source.Id, "narration native registry order");
            int address = 0x8c0000 | source.InstructionPointer;
            AssertEqual(source.BeginOpcode, (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8), "narration native begin callback");
            AssertTrue(narration.Compile(source.Id).Length > 0, "native narration extracts and compiles every page through finish callback");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => IntroNarrationDefinitions.NativePage((IntroNarrationPageId)0), "narration registry lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => IntroNarrationDefinitions.NativePage((IntroNarrationPageId)7), "narration registry upper bound");
    Console.WriteLine("Narration registry: six native page boundaries, extraction, compilation, order and bounds pass.");
    return 0;
}
if (args is ["--lookup-stream-1-timer-layout"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Timer layout oracle revision");
    VerifyLookupStream1TimerLayout(rom);
    return 0;
}
if (args is ["--lookup-stream-1-flare-placement"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Flare placement oracle revision");
    VerifyLookupStream1FlarePlacement(rom);
    return 0;
}
if (args is ["--lookup-mochtroid-shake"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    foreach (ushort timer in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 0xfff8, 0xfff9, 0xfffa, 0xfffb, 0xfffc, 0xfffd, 0xfffe, 0xffff })
    {
        int offset = timer & 6;
        short x = unchecked((short)(rom.ReadByte(0xa3a76d + offset) | rom.ReadByte(0xa3a76e + offset) << 8));
        short y = unchecked((short)(rom.ReadByte(0xa3a775 + offset) | rom.ReadByte(0xa3a776 + offset) << 8));
        AssertEqual(((int)x, (int)y), MochtroidShakeDefinitions.Offset(timer), "Native cardinal shake and timer-bit mask");
    }
    VerifyMochtroidInstructionProgramDefinitions(rom);
    Console.WriteLine("Mochtroid shake: sixteen native masked-timer comparisons pass; amplitude remains required.");
    return 0;
}
if (args is ["--lookup-mochtroid-visuals"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mochtroid oracle revision");
    VerifyStream3MochtroidVisuals(rom);
    VerifyMochtroidInstructionProgramDefinitions(rom);
    Console.WriteLine("Mochtroid: six native registrations, eight selectors, address rejection and production instruction checks pass.");
    return 0;
}
if (args is ["--lookup-stream2-environmental-catalogs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Environmental catalog oracle revision");
    VerifyLookupStream2EnvironmentalCatalogs(rom);
    return 0;
}
if (args is ["--lookup-stream2-backdrop-geometry"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Backdrop oracle revision");
    VerifyLookupStream2BackdropGeometry(rom);
    return 0;
}
if (args is ["--lookup-menu-sprite-geometry"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu geometry oracle revision");
    VerifyStream3MenuSpriteGeometry(rom);
    Console.WriteLine("Menu geometry: native compositions, calculated stock storage, independent part edits, reordering and three loader bindings pass.");
    return 0;
}
if (args is ["--lookup-stream2-equipment-base-geometry"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Equipment template oracle revision");
    VerifyLookupStream2EquipmentBaseGeometry(rom);
    return 0;
}
if (args is ["--lookup-stream2-wireframe-mirrors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Wireframe oracle revision");
    VerifyLookupStream2WireframeMirrors(rom);
    return 0;
}
if (args is ["--lookup-intro-caret-registration"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(1, IntroCaretSpriteDefinitions.Frames.Count, "Caret registration count");
    AssertEqual((int)(rom.ReadByte(0x8c8d68) | rom.ReadByte(0x8c8d69) << 8), IntroCaretSpriteDefinitions.Visible.StockPartCount, "Native caret part count");
    AssertEqual(new IntroCaretFrameDefinition(0x8d68, "caret-visible", 1), IntroCaretSpriteDefinitions.Frames.Single(), "Caret identity and enumeration");
    foreach (int invalid in new[] { -1, 1, int.MaxValue })
        AssertThrows<IndexOutOfRangeException>(() => _ = IntroCaretSpriteDefinitions.Frames[invalid], "Caret index bounds");
    Console.WriteLine("Caret registration: native part count, identity, enumeration and bounds pass.");
    return 0;
}
if (args is ["--lookup-enemy-frame-registration"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Enemy registration oracle revision");
    VerifyStream3EnemyFrameRegistration(rom);
    return 0;
}
if (args is ["--lookup-stream-4-spore-fade"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore fade oracle revision");
    VerifyLookupStream4SporeSpriteFade(rom);
    VerifyLookupStream4SporeBackgroundFade(rom);
    VerifyLookupStream4SporeLevelFade(rom);
    VerifyLookupStream4SporeHealthyAlias(rom);
    return 0;
}
if (args is ["--lookup-stream2-reserve-labels"])
{
    VerifyLookupStream2ReserveLabels(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-equipment-labels"])
{
    VerifyLookupStream2EquipmentLabels(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-file-select-patches"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    var data = SuperMetroid.AssetExtraction.FileSelectPresentationExtractor.Extract(rom);
    VerifyStream3FileSelectPatches(rom, data, FileSelectPresentation.Load(new MemoryStream(data)));
    Console.WriteLine("File-select patches: native cells, zero stock fallback, independent edits and actual placement pass.");
    return 0;
}
if (args is ["--lookup-stream2-equipment-blank"])
{
    VerifyLookupStream2EquipmentBlank(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream-4-beam-basis"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Beam basis oracle revision");
    VerifyLookupStream4BeamColorRelations(rom);
    Console.WriteLine("Beam basis: exactly43 required stored colors and149 calculated values; all native outputs, independent edits and CGRAM isolation pass.");
    return 0;
}
if (args is ["--lookup-stream-1-cannon-basis"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Cannon basis oracle revision");
    VerifyLookupStream1ArmCannonTileSources(rom);
    Console.WriteLine("Cannon calculated defaults, zero stock overrides, native selectors, edits and DMA checks passed.");
    return 0;
}
if (args is ["--lookup-stream-4-ending-subtitle"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Subtitle oracle revision");
    VerifyLookupStream4EndingSubtitle(rom);
    return 0;
}
if (args is ["--lookup-stream-4-ending-panel"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Producer panel oracle revision");
    VerifyLookupStream4EndingResultPanel(rom);
    VerifyLookupStream4EndingSubtitle(rom);
    return 0;
}
if (args is ["--lookup-stream2-golden-awakening-layout"])
{
    VerifyGoldenTorizoAwakeningDefinitions(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream-4-tail-rest"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tail rest oracle revision");
    VerifyLookupStream4TailRestGeometry(rom);
    return 0;
}
if (args is ["--lookup-stream-4-baby-phase"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby phase oracle revision");
    VerifyLookupStream4BabyTransferPhase(rom);
    return 0;
}
if (args is ["--lookup-stream4-fly-escape"])
{
    var source = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    VerifyFlyFrameIdentities(source);
    VerifyZebesEscapeExplosionDefinitions(source);
    return 0;
}
if (args is ["--lookup-stream4-hud-auto-cells"])
{
    VerifyLookupStream4HudAutoCells(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream4-hud-anchors"])
{
    VerifyLookupStream4HudAnchors(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream4-hud-digits"])
{
    VerifyLookupStream4HudDigits(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream4-title-identities"])
{
    VerifyTitleSpriteIdentities(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream4-title-card"])
{
    VerifyTitleCardLayout(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream4-sprite-dispatch"])
{
    VerifyRoomSpriteDispatch(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream4-spore-collision"])
{
    VerifyCompiledSporeSpawnCollision(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-dead-torizo-geometry"])
{
    VerifyLookupStream2DeadTorizoGeometry(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-crawler-ramps"])
{
    VerifyLookupStream2CrawlerRamps(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-reserve-arrow-colors"])
{
    VerifyLookupStream2ReserveArrowColors(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-trail-appearance"])
{
    VerifyLookupStream2TrailAppearance(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-lava-jumper-layout"])
{
    VerifyLookupStream2LavaJumperLayout(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-chozo-layout"])
{
    VerifyLookupStream2ChozoLayout(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-torizo-movement"])
{
    VerifyBombTorizoMovementDefinitions(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-zebes-star-fields"])
{
    VerifyLookupStream5ZebesStarFields(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-statue-ramps"])
{
    VerifyLookupStream5StatueRamps(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-crocomire-rumble"])
{
    VerifyCrocomireRumbleDefinitions(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-phantoon-markers"])
{
    VerifyLookupStream5PhantoonMarkers(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-magdollite-pulse"])
{
    VerifyLookupStream5MagdollitePulse(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-zebetite-pulse"])
{
    VerifyLookupStream5ZebetitePulse(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-zebetite-geometry"])
{
    VerifyZebetiteDefinitions(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-xray-room-rules"])
{
    VerifyXrayRoomDisplayRules();
    return 0;
}
if (args is ["--lookup-stream2-torizo-page-dispatch"])
{
    VerifyLookupStream2TorizoPageDispatch(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-kraid-lint-initialization"])
{
    VerifyLookupStream2KraidLintInitialization(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-kago-frame-geometry"])
{
    VerifyLookupStream2KagoFrameGeometry(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-pipe-bug-visual-geometry"])
{
    VerifyLookupStream2PipeBugVisualGeometry(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-horizontal-camera-targets"])
{
    VerifyLookupStream2HorizontalCameraTargets(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-drop-selection"])
{
    VerifyLookupStream2DropSelection(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-pipe-bug-formation"])
{
    VerifyLookupStream2PipeBugFormation(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-yapping-maw-offsets"])
{
    VerifyLookupStream5YappingMawOffsets(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-eye-geometry"])
{
    VerifyLookupStream5EyeGeometry(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-golden-control"])
{
    VerifyLookupStream2GoldenControl(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-trail-programs"])
{
    VerifyLookupStream2TrailPrograms(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-trail-selectors"])
{
    VerifyLookupStream2TrailSelectors(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-tube-ramp"])
{
    VerifyLookupStream2TubeRamp(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-pause-ownership"])
{
    VerifyLookupStream2PauseOwnership(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-dead-torizo-transfers"])
{
    VerifyLookupStream2DeadTorizoTransfers(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream2-pipe-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 pipe-program oracle revision");
    VerifyPipeBugAnimationDefinitions(rom);
    VerifyNorfairPipeBugInstructionProgramDefinitions(rom);
    return 0;
}
if (args is ["--lookup-stream2-body-placements"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 body-placement oracle revision");
    VerifyBombTorizoAttackDefinitions(rom);
    VerifyCompiledStatueWalking(rom);
    return 0;
}
if (args is ["--lookup-stream2-crawler-animations"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 crawler original oracle revision");
    VerifyCrawlerAnimationDefinitions(rom);
    VerifyWaverAnimationDefinitions(rom);
    return 0;
}
if (args is ["--lookup-stream2-gunship-transfers"] or ["--lookup-stream2-reserve-geometry"] or ["--lookup-stream2-ghost-norfair"] or ["--lookup-stream2-palette-mechanics"] or ["--lookup-stream2-yard-directions"] or ["--lookup-stream2-crystal-body"] or ["--lookup-stream2-kraid-ramps"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 original oracle revision");
    if (args[0] == "--lookup-stream2-gunship-transfers")
        VerifyLookupStream2GunshipTransfers(rom);
    else if (args[0] == "--lookup-stream2-reserve-geometry")
        VerifyLookupStream2ReserveGeometry(rom);
    else if (args[0] == "--lookup-stream2-ghost-norfair")
        VerifyLookupStream2GhostAndNorfair(rom);
    else if (args[0] == "--lookup-stream2-palette-mechanics")
        VerifyLookupStream2PaletteMechanics(rom);
    else if (args[0] == "--lookup-stream2-kraid-ramps")
        VerifyLookupStream2KraidRamps(rom);
    else if (args[0] == "--lookup-stream2-crystal-body")
        VerifyLookupStream2CrystalBody(rom);
    else
    {
        VerifyYardDirectionDefinitions(rom);
        VerifyYardTurnDefinitions(rom);
    }
    return 0;
}
if (args is ["--lookup-stream-1"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 1 oracle revision");
    VerifySciserInstructionProgramDefinitions();
    VerifySamusDeathSequence();
    VerifyNuclearWaffleProjectileInstructionProgramDefinitions();
    VerifyNuclearWaffleDefinitions(rom);
    VerifyOwtchInstructionProgramDefinitions();
    VerifyStokeInstructionProgramDefinitions();
    VerifyPuyoInstructionProgramDefinitions();
    VerifyMiscDustProjectileDefinitions(rom);
    VerifyMetroidsClearedStatePlm(new TestAddressSpace());
    VerifyMetroidInstructionProgramDefinitions();
    VerifySamusAtmosphericEffectDefinitions(rom);
    VerifySamusHudDefinitions(rom);
    VerifySamusStoredShineAndShinespark();
    VerifySamusArmCannonDefinitions(rom);
    VerifyPoseDispatchDefinitions(rom);
    VerifyPoseCollisionDefinitions(rom);
    VerifyPoseProjectileOrigin(rom);
    VerifyProjectileOrigins(rom);
    VerifyCompiledSpeedBoosterPlmPrograms();
    VerifyLookupStream1(rom);
    VerifyBrinstarPipeBugInstructionProgramDefinitions();
    VerifyMetroidBehaviorDefinitions(rom);
    VerifyBrinstarBlueSporePaletteFxProgramMechanicsDefinitions(rom);
    VerifyDragonFireballInstructionProgramDefinitions();
    VerifyEscapeEtecoonInstructionProgramDefinitions();
    VerifyDragonInstructionProgramDefinitions();
    VerifyCacatacInstructionProgramDefinitions();
    VerifyBullInstructionProgramDefinitions(rom);
    VerifyChargeFlareDefinitions(rom);
    VerifySamusIndexedSpeeds(rom);
    VerifyLookupStream1ProjectileMotion(rom);
    VerifyBeamCallbackTables(initializeOnly: true);
    VerifyLookupStream1Selection(rom);
    VerifyLookupStream1LaunchRoles(rom);
    VerifyLookupStream1SparkSpikePrograms(rom);
    VerifyPoseInputDefinitions(rom);
    VerifyProjectileDamage(rom);
    VerifyProjectileSoundRoutingDefinitions(rom);
    Console.WriteLine("Stream 1 lookup conversions: focused original-source and domain checks pass.");
    return 0;
}
if (args is ["--lookup-stream-4"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 4 oracle revision");
    VerifyMamaTurtleInstructionProgramDefinitions();
    VerifyEyeDoorProjectileInstructionProgramDefinitions();
    VerifyCompiledDraygonBg2Collision(rom);
    VerifyRidleyCollisionDefinitions();
    VerifyLookupStream4(rom);
    VerifyPowerBombFixedColors();
    VerifyRidleyAttackChoices(rom);
    VerifyRidleyMovementTargets(rom);
    VerifyRidleyClawOffsets(rom);
    VerifySaveRamLayout();
    VerifyLowerNorfairRioInstructionProgramDefinitions();
    VerifyDraygonProjectileInstructionProgramDefinitions();
    VerifyBotwoonHoleRightBounds(rom);
    VerifyBotwoonHoleBottomBounds(rom);
    VerifyBotwoonWallStockMapping(rom);
    VerifyCompiledDraygonOamCollision(rom);
    Console.WriteLine("Stream 4 lookup conversions: focused original-source and domain checks pass.");
    return 0;
}
if (args is ["--lookup-stream5-map-highlight"])
{
    VerifyLookupStream5MapHighlight(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-sidehopper-geometry"])
{
    VerifyLookupStream5SidehopperGeometry(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-phantoon-schedules"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    VerifyCompiledPhantoonCasualFlames(rom);
    VerifyPhantoonSoundDefinitions(rom);
    return 0;
}
if (args is ["--lookup-stream5-phantoon-rain"])
{
    VerifyLookupStream5PhantoonRain(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-ceres-door-ramp"])
{
    VerifyLookupStream5CeresDoorRamp(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--lookup-stream5-corpse-geometry"])
{
    VerifyCorpseMetadataDefinitions(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifyLookupStream5CorpseViews();
    Console.WriteLine("Corpse geometry: all30 rotation offsets,32 sand offsets,32 four-field DMA descriptors and10 terminators match original ROM.");
    return 0;
}
if (args is ["--lookup-stream5-crocomire-order"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire destruction-order oracle revision");
    VerifyCrocomireBridgeFragmentDefinitions(rom);
    return 0;
}
if (args is ["--lookup-stream5-actor-layouts"] or ["--lookup-stream5-initialization"] or ["--lookup-stream5-palette-entries"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 5 original oracle revision");
    if (args[0] == "--lookup-stream5-actor-layouts")
        VerifyLookupStream5ActorLayouts(rom);
    else if (args[0] == "--lookup-stream5-initialization")
        VerifyLookupStream5Initialization(rom);
    else
        VerifyLookupStream5PaletteEntries(rom);
    return 0;
}
if (args is ["--lookup-file-select-names"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "File-select identity fixture revision");
    byte[] source = FileSelectPresentationExtractor.Extract(rom);
    VerifyFileSelectPageNames(source);
    VerifyFileSelectPatchNames(source);
    VerifyFileSelectBorderNames(source);
    VerifyFileSelectDynamicAnchorNames(source);
    VerifyFileSelectSpriteNames(source);
    Console.WriteLine("File-select names: all five original identity domains, ordered cases, bounds and exact loader membership pass.");
    return 0;
}
if (args is ["--lookup-map-sprite-cases"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map sprite case oracle revision");
    VerifyMapSpriteCompositionCases(rom);
    Console.WriteLine("Map sprite cases: all original OAM, independent role edits, required membership and invalid identities pass.");
    return 0;
}
if (args is ["--lookup-map-sprite-names"])
{
    VerifyMapSpriteNameCases();
    Console.WriteLine("Map sprite names: all original identities and order, complete ushort membership and invalid names pass.");
    return 0;
}
if (args is ["--lookup-menu-title-font"] or ["--lookup-menu-large-font"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Large font pixel oracle revision");
    VerifyMenuLargeFontPixels(rom);
    Console.WriteLine("Large font pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-thin-border-pixels"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Thin border pixel oracle revision");
    VerifyMenuThinBorderPixels(rom);
    Console.WriteLine("Thin border pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-beveled-square-pixels"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Beveled square pixel oracle revision");
    VerifyMenuBeveledSquarePixels(rom);
    Console.WriteLine("Beveled square pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-map-arrow-pixels"] or ["--lookup-map-pulse-pixels"] or ["--lookup-defeated-boss-pixels"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map marker pixel oracle revision");
    if (args[0] == "--lookup-map-arrow-pixels") VerifyMapArrowPixels(rom);
    else if (args[0] == "--lookup-map-pulse-pixels") VerifyMapPulsePixels(rom);
    else VerifyDefeatedBossPixels(rom);
    Console.WriteLine($"{args[0]}: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-shoulder-highlight-pixels"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Shoulder highlight pixel oracle revision");
    VerifyMenuShoulderHighlightPixels(rom);
    Console.WriteLine("Shoulder highlight pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-shoulder-button-pixels"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Shoulder button pixel oracle revision");
    VerifyMenuShoulderButtonPixels(rom);
    Console.WriteLine("Shoulder button pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-panel-pixels"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu panel pixel oracle revision");
    VerifyMenuPanelPixels(rom);
    Console.WriteLine("Menu panel pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-elevator-lettering"] or ["--lookup-menu-outlined-lettering"] or ["--lookup-menu-compact-lettering"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevator lettering pixel oracle revision");
    VerifyMenuCompactLetteringPixels(rom);
    Console.WriteLine("Elevator lettering pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-small-font"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Small font pixel oracle revision");
    VerifyMenuSmallFontPixels(rom);
    Console.WriteLine("Small font pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-highlight-tile-pixels"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Highlight pixel oracle revision");
    VerifyHighlightTilePixels(rom);
    Console.WriteLine("Highlight pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-reserve-tile-pixels"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Reserve pixel oracle revision");
    VerifyReserveTilePixels(rom);
    Console.WriteLine("Reserve pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-pause-reserve-frames"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Reserve frame oracle revision");
    VerifyPauseReserveFrameCases(rom);
    Console.WriteLine("Reserve frames: all native role identities and OAM, independent edits, required membership and invalid inputs pass.");
    return 0;
}
if (args is ["--lookup-pause-reserve-anchors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Reserve anchor oracle revision");
    VerifyPauseReserveAnchors(rom);
    Console.WriteLine("Reserve anchors: native coordinate fields, independent edits, actual sprite output and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-reward-arm-art"] or ["--lookup-ending-reward-hair-art"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Reward arm artwork oracle revision");
    ExportEndingRewardGestureArtwork(rom, args[0] == "--lookup-ending-reward-hair-art");
    return 0;
}
if (args is ["--lookup-zebes-planet-art"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Zebes planet artwork oracle revision");
    ExportZebesPlanetArtwork(rom);
    return 0;
}
if (args is ["--lookup-ceres-large-blast-art"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres large blast artwork oracle revision");
    ExportCeresLargeBlastArtwork(rom);
    return 0;
}
if (args is ["--lookup-ceres-asteroid-art"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres asteroid artwork oracle revision");
    ExportCeresAsteroidArtwork(rom);
    return 0;
}
if (args is ["--lookup-ending-explosion-art"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Explosion artwork oracle revision");
    ExportEndingExplosionArtworkEvidence(rom);
    return 0;
}
if (args is ["--lookup-ending-explosion-frame-catalog"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending explosion frame catalog oracle revision");
    VerifyEndingExplosionFrameCatalog(rom);
    Console.WriteLine("Ending explosion frame catalog: all16 native pointers/counts, published keys, enumeration and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-text-regions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending region oracle revision");
    VerifyEndingTextRegions(rom);
    Console.WriteLine("Ending text regions: all six native text spans, positions and styles pass.");
    return 0;
}
if (args is ["--lookup-ending-glyphs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending glyph oracle revision");
    VerifyEndingGlyphMapping(rom);
    Console.WriteLine("Ending glyphs: complete four-style alphabet/digit/blank mappings, both halves and all ushort decoder inputs pass.");
    return 0;
}
if (args is ["--lookup-ending-font-layout"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending font layout oracle revision");
    string output = Path.GetFullPath("csharp/test-temp/1165-ending-font-original.png");
    Directory.CreateDirectory(Path.GetDirectoryName(output)!);
    File.WriteAllBytes(output, SuperMetroid.AssetExtraction.EndingFontAtlasExtractor.Extract(rom));
    Console.WriteLine(output);
    return 0;
}
if (args is ["--lookup-ending-mode7-roles"])
{
    VerifyEndingMode7RoleSelection();
    Console.WriteLine("Ending Mode7 roles: six original sources, filenames, supplied references, identity ordering and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-gunship-art"])
{
    ExportEndingGunshipPaletteEvidence();
    return 0;
}
if (args is ["--lookup-ending-gunship-program"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending gunship program oracle revision");
    VerifyZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions(rom);
    Console.WriteLine("Ending gunship program:35 original control words,16 frame pointers,256 color pointers, complete ownership and384-frame lifetime pass.");
    return 0;
}
if (args is ["--lookup-ending-gunship-colors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending gunship color oracle revision");
    byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
    VerifyExtractedEndingGunshipPaletteFxPresentation(rom, RoomPaletteFxPresentation.Load(new MemoryStream(json)));
    Console.WriteLine("Ending gunship colors: all256 original words,239 calculated colors, complete pointer domain, guarded effect and independent edits pass.");
    return 0;
}
if (args is ["--lookup-ending-logo-glare-program"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Logo glare program oracle revision");
    VerifyPostCreditsIconGlarePaletteFxProgramMechanicsDefinitions(rom);
    Console.WriteLine("Logo glare program: decoded31 control words,14 frame pointers,224 color pointers, complete ownership and lifetime pass.");
    return 0;
}
if (args is ["--lookup-ending-logo-glare"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Logo glare oracle revision");
    byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
    VerifyExtractedLogoGlarePaletteFxPresentation(rom, RoomPaletteFxPresentation.Load(new MemoryStream(json)));
    Console.WriteLine("Logo glare: all224 native colors, full pointer domain, guarded program and independent edits pass.");
    return 0;
}
if (args is ["--lookup-ending-logo-color-art"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo artwork oracle revision");
    ExportEndingLogoPaletteEvidence(rom);
    return 0;
}
if (args is ["--lookup-ending-logo-fade"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo fade oracle revision");
    VerifyEndingLogoPaletteFade(rom);
    Console.WriteLine("Ending logo fade: all512 original colors, computed transfers, independent edits and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-palette-metadata"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending palette metadata oracle revision");
    VerifyEndingPaletteMetadata(rom);
    Console.WriteLine("Ending palettes: six native sources, seven allocation sizes, published filenames and invalid roles pass.");
    return 0;
}
if (args is ["--lookup-ending-palette-roles"])
{
    VerifyEndingPaletteRoleSelection();
    Console.WriteLine("Ending palette roles: all seven supplied references, original identity order and invalid-role behavior pass.");
    return 0;
}
if (args is ["--lookup-ending-fragment-metadata"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending fragment oracle revision");
    VerifyEndingObjectFragmentMetadata(rom);
    Console.WriteLine("Ending fragments: four native compressed sources, published filenames and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-cloud-definitions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending cloud oracle revision");
    VerifyEndingCloudDefinitions(rom);
    Console.WriteLine("Ending clouds: six catalog records,24 original program words, six actor streams and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-reward-actors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending reward actor oracle revision");
    VerifyEndingRewardActorDefinitions(rom);
    return 0;
}
if (args is ["--lookup-ending-reward-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending reward oracle revision");
    VerifyEndingRewardInstructions(rom);
    Console.WriteLine("Ending reward programs: all160 native words,17 actor streams, callback timing, head record layout and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-explosion-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending explosion oracle revision");
    VerifyEndingExplosionPrograms(rom);
    Console.WriteLine("Ending explosion programs: all65 native words, eight actor streams, frame record layout and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-logo-actors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo actor oracle revision");
    VerifyEndingLogoDefinitions(rom);
    return 0;
}
if (args is ["--lookup-ending-logo-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo program oracle revision");
    VerifyEndingLogoPrograms(rom);
    Console.WriteLine("Ending logo programs: all31 native words, four actor streams, callback timing and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-completion-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending completion oracle revision");
    VerifyEndingCompletionTextInstructions(rom);
    Console.WriteLine("Ending completion programs: all164 original words and all14 actor streams pass.");
    return 0;
}
if (args is ["--lookup-ending-logo-tables"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo table oracle revision");
    VerifyEndingLogoPaletteSources(rom);
    VerifyEndingPostShotTransferFields(rom);
    Console.WriteLine("Ending logo tables: all32 original palette sources, six transfer records and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-audio-upload-catalog"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Audio upload oracle revision");
    VerifyAudioUploadCatalog(rom);
    Console.WriteLine("Audio upload catalog: all25 original pointers, names, views and byte rejection domain pass.");
    return 0;
}
if (args is ["--lookup-spc-pan-interpolation"])
{
    VerifySpcPanInterpolation();
    return 0;
}
if (args is ["--lookup-spc-fir-addressing"])
{
    VerifySpcFirAddressing();
    return 0;
}
if (args is ["--lookup-ceres-placement-art"])
{
    ExportCeresPlacementArtwork();
    return 0;
}
if (args is ["--lookup-ceres-initial-metadata"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres destruction metadata oracle revision");
    VerifyCeresInitialActorMetadata(rom);
    Console.WriteLine("Ceres destruction metadata: native spawn identities, asteroid aliases, stationary-vortex overrides and bounds pass.");
    return 0;
}
if (args is ["--lookup-ceres-flight-metadata"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres flight metadata oracle revision");
    VerifyCeresFlightActorMetadata(rom);
    Console.WriteLine("Ceres flight metadata: native spawn identities, initializer branches, motion operands, star aliases and bounds pass.");
    return 0;
}
if (args is ["--lookup-ceres-zebes-metadata"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Zebes metadata oracle revision");
    VerifyCeresZebesActorMetadata(rom);
    Console.WriteLine("Zebes actor metadata: all six native spawn identities, definitions, placements, motion policies and bounds pass.");
    return 0;
}
if (args is ["--lookup-ceres-placement-selectors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres placement oracle revision");
    VerifyCeresRearPlacementSelector(rom);
    VerifyCeresRevealPlacementSelector(rom);
    VerifyCeresInitialPlacementSelector();
    Console.WriteLine("Ceres placement selectors: all 14 original entries, native operands and invalid-index contracts pass.");
    return 0;
}
if (args is ["--lookup-ceres-spawner-schedule"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres spawner oracle revision");
    VerifyCeresSpawnerSchedule(rom);
    Console.WriteLine("Ceres spawner: native list timing, late countdown resets, simultaneous waves and departure boundaries pass.");
    return 0;
}
if (args is ["--lookup-ceres-flight-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres flight program oracle revision");
    VerifyCeresFlightPrograms(rom);
    VerifyCeresFlightFrameCatalog(rom);
    VerifySpaceColonyCaption(rom);
    VerifyCeresStationParts(rom);
    VerifyCeresSmallAsteroidParts(rom);
    VerifyCeresVortexParts(rom);
    VerifyCeresReflectedStarParts(rom);
    VerifyCeresStarPointParts(rom);
    Console.WriteLine("Ceres flight programs: five original streams, byte/word boundaries, shared aliases and interpreter loops pass.");
    return 0;
}
if (args is ["--lookup-ceres-backdrop-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres backdrop program oracle revision");
    VerifyCeresBackdropPrograms(rom);
    VerifyCeresDestructionFrameCatalog(rom);
    VerifyPlanetZebesTitleParts(rom);
    VerifyZebesPlanetBandParts(rom);
    Console.WriteLine("Ceres backdrop programs: seven original streams, word boundaries, loops and title callback timing pass.");
    return 0;
}
if (args is ["--lookup-ceres-explosion-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres explosion program oracle revision");
    VerifyCeresInitialExplosionProgram(rom);
    VerifyCeresRepeatingExplosionProgram(rom);
    VerifyCeresFinalWaveProgram(rom);
    VerifyCeresStationBlastProgram(rom);
    VerifyCeresStationBlastParts(rom);
    VerifyCeresLargeBlastParts(rom);
    Console.WriteLine("Ceres explosion programs: four original streams, byte/word bounds and actual interpreter timing/deletion pass.");
    return 0;
}
if (args is ["--lookup-ceres-burst-layout"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres burst layout oracle revision");
    VerifyCeresBurstLayout(rom);
    Console.WriteLine("Ceres burst layout: all original repeating X/Y and final Y words, common delay and bounds pass.");
    return 0;
}
if (args is ["--lookup-ceres-blast-placement"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres blast oracle revision");
    VerifyCeresInitialBlastX(rom);
    VerifyCeresInitialBlastY(rom);
    VerifyCeresInitialBlastDelay(rom);
    VerifyCeresFinalBlastX(rom);
    VerifyCeresFinalBlastDelay(rom);
    Console.WriteLine("Ceres blast placement: five original geometry/delay mappings and invalid boundaries pass.");
    return 0;
}
if (args is ["--lookup-spc-sound-streams"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC stream oracle revision");
    VerifySpcSoundStream1(rom);
    VerifySpcSoundStream2(rom);
    VerifySpcSoundStream3(rom);
    Console.WriteLine("SPC sound streams: all240 original pointers, counts, invalid inputs and runtime command boundaries pass.");
    return 0;
}
if (args is ["--lookup-spc-sound-policies"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC policy oracle revision");
    VerifySpcSoundPolicy1(rom);
    VerifySpcSoundPolicy2(rom);
    VerifySpcSoundPolicy3(rom);
    Console.WriteLine("SPC sound policies: all240 native dispatches, handler writes, preserved fields, voice counts and invalid inputs pass.");
    return 0;
}
if (args is ["--lookup-spc-allocation-addresses"])
{
    VerifySpcAllocationAddresses();
    Console.WriteLine("SPC allocation layout: all original field bases, channel addresses and rejected indices pass.");
    return 0;
}
if (args is ["--lookup-intro-egg-effect-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro egg-effect oracle revision");
    VerifyIntroEggEffectPrograms(rom);
    VerifyIntroEggEffectFrameCatalog(rom);
    VerifyIntroEggEffectParts(rom);
    Console.WriteLine("Intro egg effects: all76 bytes, eleven frame records/calculated parts, native OAM, independent edits and boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-rinka-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro Rinka oracle revision");
    VerifyIntroRinkaPrograms(rom);
    VerifyIntroRinkaFrameCatalog(rom);
    VerifyIntroRinkaParts(rom);
    Console.WriteLine("Intro Rinka: program/catalog, twelve calculated parts, native OAM, independent edits and bounds pass.");
    return 0;
}
if (args is ["--lookup-intro-baby-discovery-instructions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro discovery oracle revision");
    VerifyIntroBabyDiscoveryInstructions(rom);
    VerifyIntroDiscoveryFrameCatalog(rom);
    VerifyIntroEggRemnantParts(rom);
    VerifyIntroEggRockingParts(rom);
    VerifyIntroEggCrackingParts(rom);
    VerifyIntroConfusedBabyParts(rom);
    VerifyCeresLargeAsteroidParts(rom);
    VerifyIntroBabyDiscoveryInput(rom);
    VerifyIntroDiscoveryCollision(rom);
    Console.WriteLine("Intro discovery programs: all138 bytes, overlapping words, operations and boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-scientist-instructions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro scientist oracle revision");
    VerifyIntroScientistInstructions(rom);
    VerifyIntroScientistFrameCatalog(rom);
    VerifyIntroScientistParts(rom);
    Console.WriteLine("Intro scientist programs/catalog: all142 bytes, overlapping views, ten original frame records, stable asset names and boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-eye-instructions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro eye oracle revision");
    VerifyIntroEyeInstructions(rom);
    Console.WriteLine("Intro eye programs: all74 bytes, overlapping words, loop targets and read boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-collision-art"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro collision art oracle revision");
    ExportIntroCollisionArtwork(rom);
    return 0;
}
if (args is ["--lookup-intro-mother-brain-instructions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain instruction oracle revision");
    VerifyIntroMotherBrainInstructions(rom);
    VerifyIntroMotherBrainFrameCatalog(rom);
    VerifyIntroMotherBrainParts(rom);
    Console.WriteLine("Mother Brain programs/catalog: all46 bytes,45 word views,three frames and boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-mother-brain-input"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain input oracle revision");
    VerifyIntroMotherBrainInputSource(rom);
    Console.WriteLine("Mother Brain input: all112 bytes,110 word views and boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-mother-brain-collision"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain collision oracle revision");
    VerifyIntroMotherBrainCollisionSource(rom);
    Console.WriteLine("Mother Brain collision source: all448 bytes and independent mutable allocations pass.");
    return 0;
}
if (args is ["--lookup-intro-mother-brain-explosion-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro explosion oracle revision");
    VerifyIntroMotherBrainExplosionPrograms(rom);
    VerifyIntroMotherBrainExplosionFrameCatalog(rom);
    VerifyIntroMotherBrainExplosionParts(rom);
    Console.WriteLine("Intro Mother Brain explosions: both native programs, word views, delete, twelve frame identities and bounds pass.");
    return 0;
}
if (args is ["--lookup-intro-caret-instructions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro caret oracle revision");
    VerifyIntroCaretInstructions(rom);
    Console.WriteLine("Intro caret instructions: all20 bytes, overlapping word views and boundary behavior pass.");
    return 0;
}
if (args is ["--lookup-spc-dsp-publication"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC publication oracle revision");
    VerifySpcDspPublication(rom);
    Console.WriteLine("SPC DSP publication: native destination/source maps, echo gates and pending-key reset pass.");
    return 0;
}
if (args is ["--lookup-spc-pan-samples"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC pan sample oracle revision");
    VerifySpcPanSamples(rom);
    Console.WriteLine("SPC pan sample boundary: all21 curve bytes, the adjacent FIR coefficient and rejected indices pass; curve conversion remains pending.");
    return 0;
}
if (args is ["--lookup-pause-wireframe-selection"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause wireframe selection oracle revision");
    VerifyPauseWireframeSelection(rom);
    Console.WriteLine("Pause wireframe selection: native mask and comparison-table operands and all65536 input words pass.");
    return 0;
}
if (args is ["--lookup-pause-equipment-masks"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause equipment mask oracle revision");
    VerifyPauseBeamMasks(rom);
    VerifyPauseSuitMasks(rom);
    VerifyPauseBootMasks(rom);
    Console.WriteLine("Pause equipment masks: all fourteen native flags and category/item rejection contracts pass.");
    return 0;
}
if (args is ["--lookup-pause-selector-anchors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause selector anchor oracle revision");
    VerifyPauseSelectorAnchors(rom);
    Console.WriteLine("Pause selector anchors: original names and coordinates, independent axis edits, zero stock storage and bounds pass.");
    return 0;
}
if (args is ["--lookup-pause-selector-compositions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause selector composition oracle revision");
    VerifyPauseSelectorCompositions(rom);
    Console.WriteLine("Pause selector compositions: native OAM, all groups/phases, independent edits, cyclic indices and capacity pass.");
    return 0;
}
if (args is ["--lookup-pause-selector-durations"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause selector duration oracle revision");
    VerifyPauseSelectorDurations(rom);
    Console.WriteLine("Pause selector durations: native program, sparse edits, custom lengths, cyclic indices and initial timer pass.");
    return 0;
}
if (args is ["--lookup-map-arrow-durations"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map arrow duration oracle revision");
    VerifyMapArrowDurations(rom);
    Console.WriteLine("Map arrow durations: all original phases, sparse edits, custom lengths, bounds and actual timing pass.");
    return 0;
}
if (args is ["--lookup-map-arrow-cases"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map arrow oracle revision");
    VerifyMapArrowCases(rom);
    Console.WriteLine("Map arrow cases: original anchors and shapes, independent direction edits, phase selection, membership and bounds pass.");
    return 0;
}
if (args is ["--lookup-pause-button-spans"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause button span oracle revision");
    VerifyPauseButtonSpanWords(rom);
    VerifyPauseButtonSpanCounts(rom);
    Console.WriteLine("Pause buttons: all six native row destinations and widths, enumeration order and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-pause-categories"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause category oracle revision");
    VerifyPauseCategoryCases(rom);
    Console.WriteLine("Pause categories: all native pointer fields, item counts, dispatched copy lengths, reserve contract and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-map-indicator"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map indicator oracle revision");
    VerifyMapIndicatorSprites(rom);
    VerifyMapIndicatorDelays(rom);
    Console.WriteLine("Map indicator: all native sprite/delay words, input bounds, first-tick order and two loop transitions pass.");
    return 0;
}
if (args is ["--lookup-save-marker-coordinates"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Save-marker coordinate oracle revision");
    VerifyMapSaveMarkerCoordinates(rom);
    Console.WriteLine("Save-marker coordinates: all 68 native components, independent edits, JSON identity, actual marker binding and bounds pass.");
    return 0;
}
if (args is ["--lookup-map-area-cases"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map-area selector oracle revision");
    VerifyFileSelectMapAreaCases(rom);
    Console.WriteLine("Map-area selection: all six native identity cases and rejected input boundaries pass.");
    return 0;
}
if (args is ["--lookup-save-marker-eligibility"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Save-marker eligibility oracle revision");
    VerifySaveMarkerEligibility(rom);
    Console.WriteLine("Save-marker eligibility: all 96 native slots, 34 identities, all used masks and input boundaries pass.");
    return 0;
}
if (args is ["--lookup-map-load-anchors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map-load anchor oracle revision");
    VerifyMapLoadAnchorX(rom);
    VerifyMapLoadAnchorY(rom);
    Console.WriteLine("Map-load anchors: all 34 original X/Y projections, unused stations and invalid input boundaries pass.");
    return 0;
}
if (args is ["--lookup-options-headings"])
{
    VerifyGameOptionsHeadings();
    Console.WriteLine("Options headings: all three original sprite selections and anchor pairs, imported records and enum bounds pass.");
    return 0;
}
if (args is ["--game-options-cursor-phases"])
{
    VerifyGameOptionsCursorPhases();
    return 0;
}
if (args is ["--lookup-options-pages"])
{
    VerifyGameOptionsPageCases();
    Console.WriteLine("Options pages: all five native source pairs, named imported page bytes, descriptions and rejected selectors pass.");
    return 0;
}
if (args is ["--lookup-file-select-slot-fields"])
{
    VerifyFileSelectSlotDestinations();
    Console.WriteLine("File-select slot fields: all 24 original destinations, both slot-label source views, extracted anchors/text and bounds pass.");
    return 0;
}
if (args is ["--lookup-file-select-helmet"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Helmet animation oracle revision");
    VerifyFileSelectHelmetAnimation(rom);
    Console.WriteLine("File-select helmet: all nine original sprite IDs, native cadence/clamp and bounded selectors pass.");
    return 0;
}
if (args is ["--lookup-game-over-baby-animation"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby animation oracle revision");
    VerifyGameOverBabyAnimation(rom);
    Console.WriteLine("Baby animation: original durations, frames, palettes, sound/control handoffs, lazy enumeration and all ushort pointers pass.");
    return 0;
}
if (args is ["--lookup-game-over-baby-colors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby colors oracle revision");
    VerifyGameOverBabyColors(rom);
    Console.WriteLine("Baby colors: all 64 original words, CGRAM application, independent channel edits and bounds pass.");
    return 0;
}
if (args is ["--lookup-game-over-text"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Game-over text oracle revision");
    VerifyGameOverTextSources(rom);
    VerifyGameOverTextDestinations(rom);
    Console.WriteLine("Game-over text: all five original sources and destinations, enumeration order and rejected selectors pass.");
    return 0;
}
if (args is ["--lookup-file-select-navigation"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "File-select navigation oracle revision");
    VerifyFileSelectMainNavigation();
    VerifyFileSelectSourceNavigation(rom);
    VerifyFileSelectDestinationNavigation(rom);
    return 0;
}
if (args is ["--lookup-map-window-motion"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map window motion oracle revision");
    var labels = RetailPresentationFixture().Labels;
    VerifyMapWindowOriginX(rom, labels);
    VerifyMapWindowOriginY(rom, labels);
    VerifyMapWindowLeftVelocities(rom);
    VerifyMapWindowRightVelocities(rom);
    VerifyMapWindowTopVelocities(rom);
    VerifyMapWindowBottomVelocities(rom);
    VerifyMapWindowTimers(rom);
    Console.WriteLine("Map window motion: all 24 original signed16.16 velocities, six timers, 12 named origin fields, independent label edits and rejected areas pass.");
    return 0;
}
if (args is ["--lookup-file-select-geometry"])
{
    VerifyFileSelectGeometryLookups();
    return 0;
}
if (args is ["--lookup-options-toggle-geometry"])
{
    VerifyGameOptionsToggleGeometry();
    return 0;
}
if (args is ["--game-options-language-palettes"])
{
    VerifyGameOptionsLanguagePalettes();
    return 0;
}
if (args is ["--lookup-menu-missile"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu missile oracle revision");
    VerifyMenuMissileSpritemapIds(rom);
    VerifyMenuMissileDurations(rom);
    Console.WriteLine("Menu missile: all four original frame IDs, four duration words, wrap mask and rejected indices pass.");
    return 0;
}
if (args is ["--lookup-options-geometry"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Options geometry oracle revision");
    VerifyOptionsPrimaryCursorY(rom);
    VerifyOptionsControllerCursorY(rom);
    VerifyOptionsSpecialCursorY(rom);
    VerifyOptionsLabelDestinations(rom);
    VerifyOptionsLabelSources(rom);
    Console.WriteLine("Options geometry: all 31 native words across five logical mappings and their bounds pass.");
    return 0;
}
if (args is ["--lookup-controller-buttons"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Controller button oracle revision");
    VerifyAssignableControllerButtons(rom);
    VerifyDefaultControllerButtons(rom);
    Console.WriteLine("Controller buttons: native choices, inverse domain, swaps, rejection and default action mappings pass.");
    return 0;
}
if (args is ["--hyper-beam-fx-colors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hyper Beam FX oracle revision");
    VerifyHyperBeamFxColorArtwork(rom);
    return 0;
}
if (args is ["--lookup-fallback-door-closing"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Fallback closing oracle revision");
    VerifyDoorClosingPlmDefinitions(rom, fallbackOnly: true);
    Console.WriteLine("Fallback closing: all twelve original header/list selections, complete byte domain and production spawning pass.");
    return 0;
}
if (args is ["--lookup-resident-door-closing"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Resident closing oracle revision");
    VerifyResidentDoorClosingDefinitions(rom);
    Console.WriteLine("Resident door closing: eighteen original header fields, complete selector domain and production redirect pass.");
    return 0;
}
if (args is ["--dynamic-collectible-graphics"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Dynamic collectible oracle revision");
    VerifyCompiledDynamicCollectibleGraphics(rom);
    Console.WriteLine("Dynamic collectible graphics: native palette selectors, tiles, pointers, guarded upload and installed artwork pass.");
    return 0;
}
if (args is ["--lookup-tourian-program"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Statue program oracle revision");
    VerifyTourianStatueProgramMappings(rom);
    VerifyTourianStatueSpawnOrder(rom);
    VerifyTourianStatueDescriptorFields(rom);
    VerifyTourianStatueArtworkSources(rom);
    Console.WriteLine("Tourian statue programs: all 36 operand positions, 184 mechanics words and full pointer domains pass.");
    return 0;
}
if (args is ["--lookup-tourian-descriptors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Statue descriptor oracle revision");
    VerifyTourianStatueDescriptorFields(rom);
    VerifyTourianStatueArtworkSources(rom);
    Console.WriteLine("Tourian statue descriptors: all eleven native fields, mechanics aliases, full object domain and artwork sources pass.");
    return 0;
}
if (args is ["--lookup-tourian-artwork"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Statue artwork oracle revision");
    VerifyTourianStatueArtworkSources(rom);
    Console.WriteLine("Tourian statue artwork: all 36 original sources, decoded operand views and complete ushort rejection domains pass.");
    return 0;
}
if (args is ["--lookup-treadmill-mechanics"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Treadmill oracle revision");
    VerifyRetailTreadmillMechanics(rom);
    foreach (var program in OriginalTreadmillPrograms(rom))
        VerifyRetailTreadmillStream(rom, TreadmillDefinition(program.Header).Direction,
            program.Frames.Select(pointer => 0x870000 | ReadVerificationWord(rom, 0x870002 + pointer)).ToArray());
    Console.WriteLine("Treadmill: native header/control fields, calculated cursors, full ushort domains and both guarded loops pass.");
    return 0;
}
if (args is ["--lookup-fx-blends"] )
{
    VerifyRoomFxPaletteBlends();
    return 0;
}
if (args is ["--samus-charge-phases"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Charge phase oracle revision");
    VerifySamusChargeColorPhases(rom);
    return 0;
}
if (args is ["--samus-hyper-beam-colors"])
{
    VerifySamusHyperBeamColors();
    return 0;
}
if (args is ["--lookup-full-body-colors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Full-body color oracle revision");
    VerifyFullBodyPaletteColorData(rom, SamusFullBodyCycleColorExtractor.Extract(rom));
    Console.WriteLine("Full-body palettes:48 native identities, all768 colors, complete pointer domain, edits and CGRAM copies pass.");
    return 0;
}
if (args is ["--normal-suit-catalog-boundary"])
{
    VerifyNormalSuitCatalogBoundary();
    return 0;
}
if (args is ["--lookup-loading-layout"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Loading layout oracle revision");
    VerifySamusLoadingSuitPaletteFxProgramMechanicsDefinitions(rom);
    Console.WriteLine("Suit loading: original decoded group/frame layout, complete pointer ownership and guarded programs pass.");
    return 0;
}
if (args is ["--lookup-loading-colors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Loading colors oracle revision");
    byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
    VerifyExtractedSamusLoadingPaletteFxPresentation(rom, RoomPaletteFxPresentation.Load(new MemoryStream(json)));
    foreach (int editMode in new[] { 0, 1, 2 })
    {
        var document = System.Text.Json.JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(json, MapPresentationFormat.JsonOptions)!;
        int ordinal = 1000;
        foreach (var frames in new[] { document.SamusLoadingPowerSuit, document.SamusLoadingVariaSuit, document.SamusLoadingGravitySuit })
        for (int frame = 0; frame < 9; frame++)
        for (int color = 0; color < 16; color++, ordinal++)
            if (editMode == 0 || (editMode == 1 && frame is 0 or 1) || (editMode == 2 && frame == 7))
                frames[frame][color] = new PaletteRgb5 { Red = ordinal & 31, Green = ordinal >> 5 & 31, Blue = ordinal >> 10 & 31 };
        var edited = RoomPaletteFxPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
        ordinal = 1000;
        foreach (var program in SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
        for (int frame = 0; frame < 9; frame++)
        for (int color = 0; color < 16; color++, ordinal++)
        {
            ushort pointer = program.ColorPointer(frame, color);
            AssertTrue(edited.TryReadColor(pointer, out ushort actual), "Edited loading color remains owned");
            ushort expected = editMode == 0 || (editMode == 1 && frame is 0 or 1) || (editMode == 2 && frame == 7) ? (ushort)ordinal : ReadVerificationWord(rom, 0x8d0000 | pointer);
            AssertEqual(expected, actual, "Independent loading row edits survive alias compilation");
        }
    }
    Console.WriteLine("Loading colors: all432 native colors, full pointer domain,25 stored words plus9 endpoint components, independent edits and guarded consumers pass.");
    return 0;
}
if (args is ["--lookup-heat-colors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Heat color oracle revision");
    byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
    var presentation = RoomPaletteFxPresentation.Load(new MemoryStream(json));
    VerifyExtractedSamusHeatPaletteFxPresentation(rom, presentation);
    var document = System.Text.Json.JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(json,
        MapPresentationFormat.JsonOptions)!;
    int ordinal = 1000;
    foreach (var frames in new[] { document.SamusHeatPowerSuit!, document.SamusHeatVariaSuit!, document.SamusHeatGravitySuit! })
    foreach (var row in frames)
    for (int color = 0; color < row.Length; color++, ordinal++)
        row[color] = new PaletteRgb5 { Red = ordinal & 31, Green = ordinal >> 5 & 31, Blue = ordinal >> 10 & 31 };
    var edited = RoomPaletteFxPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document,
        MapPresentationFormat.JsonOptions)));
    ordinal = 1000;
    foreach (int first in new[] { 0xe468, 0xe694, 0xe8c0 })
    for (int phase = 0; phase < 16; phase++)
    for (int color = 0; color < 15; color++, ordinal++)
    {
        AssertTrue(edited.TryReadColor((ushort)(first + phase * 34 + color * 2), out ushort actual), "Edited heat color owned");
        AssertEqual((ushort)ordinal, actual, "Every separately edited heat color survives alias compilation");
    }
    document = System.Text.Json.JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(json,
        MapPresentationFormat.JsonOptions)!;
    foreach (var frames in new[] { document.SamusLoadingPowerSuit, document.SamusLoadingVariaSuit, document.SamusLoadingGravitySuit })
    for (int color = 0; color < 16; color++)
        frames[0][color] = new PaletteRgb5 { Red = 1, Green = 2, Blue = 3 };
    var loadingEdited = RoomPaletteFxPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document,
        MapPresentationFormat.JsonOptions)));
    foreach (int first in new[] { 0xe468, 0xe694, 0xe8c0 })
    for (int phase = 0; phase < 16; phase++)
    for (int color = 0; color < 15; color++)
    {
        ushort pointer = (ushort)(first + phase * 34 + color * 2);
        AssertTrue(loadingEdited.TryReadColor(pointer, out ushort actual), "Heat color survives independent loading-palette edit");
        AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), actual, "Changing loading art cannot rewrite supplied heat art");
    }
    Console.WriteLine("Heat color aliases: original repeated rows, all pointer boundaries, stock storage, edited frames and guarded consumers pass.");
    return 0;
}
if (args is ["--lookup-heat-selectors"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Heat selector oracle revision");
    VerifyPaletteFxHeatInstructionListDefinitions(rom);
    VerifyPaletteFxHeatProgramMechanicsDefinitions(rom);
    Console.WriteLine("Heat selectors: all48 native words, program layout, bounds and full equipment selection pass.");
    return 0;
}
if (args is ["--lookup-fx-validation"])
{
    VerifyFxValidationBoundaries();
    Console.WriteLine("FX validation: native dispatcher slots, every cartridge byte and every host blend ushort pass.");
    return 0;
}
if (args is ["--lookup-ceres-haze"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres haze oracle revision");
    VerifyCeresHazeNativeRamp(rom);
    VerifyCeresHazeTintScaling(rom);
    VerifyCeresHazePhaseControl(rom);
    Console.WriteLine("Ceres haze: original HDMA bands, all17 native counters, both channels, captured/software views and RGB5 tint scaling pass.");
    return 0;
}
if (args is ["--lookup-animated-frames"])
{
    VerifyRoomFxAnimatedTileMechanicsDefinitions();
    return 0;
}
if (args is ["--area-animated-tile-definitions"])
{
    VerifyAreaAnimatedTileObjectDefinitions();
    return 0;
}
if (args is ["--lookup-palette-fx-areas"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Palette-FX area oracle revision");
    VerifyPaletteFxAreaListPointers(rom);
    VerifyPaletteFxAreaSelections(rom);
    Console.WriteLine("Palette-FX areas: eight original list identities, all64 selections and rejection domains pass.");
    return 0;
}
if (args is ["--lookup-palette-fx-dispatch"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Palette-FX dispatch oracle revision");
    VerifyPaletteFxDispatch(rom);
    Console.WriteLine("Palette-FX dispatch: all 63 native setup/list pairs, full identity domain and guarded spawning pass.");
    return 0;
}
if (args is ["--lookup-sky-chunk-pointers"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Sky chunk oracle revision");
    VerifyLandSkyChunkPointers(rom);
    VerifyOceanSkyChunkPointers(rom);
    Console.WriteLine("Sky chunks: both native mappings, compatibility reads, rejections and all ushort camera inputs pass.");
    return 0;
}
if (args is ["--lookup-sky-sections"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Sky sections oracle revision");
    VerifySkySectionTopPositions(rom);
    VerifySkySectionSubspeeds(rom);
    VerifySkySectionSpeeds(rom);
    VerifySkySectionDataSlots(rom);
    VerifyScrollingSkyState();
    Console.WriteLine("Sky sections: four native fields, bounds, complete world-Y projection and existing integration pass.");
    return 0;
}
if (args is ["--lookup-quake-suppression"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Quake suppression oracle revision");
    VerifyQuakeSoundSuppression(rom);
    Console.WriteLine("Quake suppression: native branches, all 65536 room identities and sound consumer pass.");
    return 0;
}
if (args is ["--lookup-fx-tilemap-sources"])
{
    VerifyRoomFxLayer3Tilemaps();
    return 0;
}
if (args is ["--lookup-quake-sound-selection"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Quake sound oracle revision");
    VerifyQuakeSoundSelection(rom);
    Console.WriteLine("Quake sound selection: all eight original identities, loop marker and production queue requests pass.");
    return 0;
}
if (args is ["--lookup-liquid-wave"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Liquid wave oracle revision");
    VerifyMirroredLiquidWave(rom);
    VerifyHorizontalHeatWave(rom);
    Console.WriteLine("Liquid waves: original mirrored and horizontal pulse samples, bounds, all phases and projection consumers pass.");
    return 0;
}
if (args is ["--lookup-rain-velocity"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Rain velocity oracle revision");
    VerifyRainHorizontalVelocity(rom);
    Console.WriteLine("Rain velocity: four signed 8.8 values, all 65536 RNG selections and invalid bounds pass.");
    return 0;
}
if (args is ["--room-fx-record-definitions", var roomFxDefinitionsRom])
{
    VerifyRoomFxRecordDefinitions(roomFxDefinitionsRom);
    return 0;
}

if (args is ["--load-station-definitions"])
{
    VerifyCompiledLoadStationDefinitions();
    return 0;
}

if (args is ["--door-definitions"])
{
    VerifyCompiledDoorDefinitions();
    return 0;
}

if (args is ["--lookup-retail-door-lists"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Door-list oracle revision");
    VerifyRetailDoorListMapping(rom);
    VerifyCompiledDoorListCollision();
    return 0;
}
if (args is ["--room-header-definitions"])
{
    VerifyCompiledRoomHeaderDefinitions();
    return 0;
}

if (args is ["--room-state-definitions"])
{
    VerifyCompiledRoomStateSelectionDefinitions();
    return 0;
}
if (args is ["--lookup-room-state-settings"])
{
    VerifyCompiledRoomStateDefinitions();
    return 0;
}
if (args is ["--lookup-kraid-arm-hitbox-lists"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Kraid arm list oracle revision");
    VerifyKraidArmHitboxListSelection(rom);
    Console.WriteLine("Kraid arm hitbox selection: all 16 native lists, 24 ordered rectangles, full ushort rejection domain and slice bounds pass.");
    return 0;
}
if (args is ["--room-plm-populations"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Retail population oracle revision");
    VerifyRetailPopulationMappings(rom);
    VerifyCompiledRoomPlmHeaderLoad(rom);
    VerifyPlmPopulationInputBoundary();
    Console.WriteLine("Retail populations: all 284 identities, 941 ordered placements and native terminators, sequential setup and historical state schemas pass.");
    return 0;
}
if (args is ["--lookup-scroll-programs"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Scroll program oracle revision");
    VerifyCompiledRoomScrollPrograms(rom);
    VerifyRoomScrollPlms();
    Console.WriteLine("Scroll programs: all 173 identities, 285 ordered writes and terminators, guarded retail execution and constructed-room behavior pass.");
    return 0;
}
if (args is ["--eye-door-plm-draws"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Eye door oracle revision");
    VerifyEyeDoorPlmDrawDefinitions(rom);
    return 0;
}
if (args is ["--collectible-visuals"])
{
    VerifyCollectibleVisuals();
    return 0;
}
if (args is ["--station-animation-programs"])
{
    VerifyStationAnimationProgramDefinitions();
    return 0;
}
if (args is ["--station-access-plm-definitions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Station access oracle revision");
    VerifyStationAccessPlmDefinitions(rom);
    return 0;
}
if (args is ["--elevator-platform-visuals"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevator platform oracle revision");
    VerifyElevatorPlatformPlmDefinitions(rom);
    VerifyElevatorPlatformVisuals();
    return 0;
}
if (args is ["--chozo-plm-definitions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Chozo program oracle revision");
    VerifyChozoStatuePlmDefinitions(rom);
    return 0;
}
if (args is ["--draygon-cannon-plm-program"])
{
    VerifyDraygonCannonPlmProgram();
    return 0;
}
if (args is ["--mother-brain-glass-instruction-mechanics"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Glass projectile oracle revision");
    VerifyMotherBrainGlassInstructionProgramDefinitions(rom);
    return 0;
}
if (args is ["--mother-brain-glass-shard-definitions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Glass shard oracle revision");
    VerifyMotherBrainGlassShardDefinitions(rom);
    return 0;
}
if (args is ["--mother-brain-glass-plm-draws"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Glass oracle revision");
    VerifyMotherBrainGlassPlmDrawDefinitions(rom);
    return 0;
}
if (args is ["--noob-tube-plm-draws"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tube draw oracle revision");
    VerifyNoobTubePlmDrawDefinitions(rom);
    return 0;
}
if (args is ["--noob-tube-plm-program"])
{
    VerifyNoobTubePlm();
    return 0;
}
if (args is ["--colored-door-plm-draws"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Colored door oracle revision");
    VerifyColoredDoorPlmDrawDefinitions(rom);
    return 0;
}
if (args is ["--blue-door-plm-draws"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Blue door oracle revision");
    VerifyBlueDoorPlmDrawDefinitions(rom);
    return 0;
}
if (args is ["--grey-door-plm-draws"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Grey door oracle revision");
    VerifyGreyDoorPlmDrawDefinitions(rom);
    Console.WriteLine("Grey door definitions and guarded lifecycle pass, including all Bomb Torizo program fields.");
    return 0;
}
if (args is ["--bomb-torizo-hand-plm-program"])
{
    VerifyBombTorizoHandPlm();
    return 0;
}
if (args is ["--bomb-torizo-hand-artwork"])
{
    VerifyBombTorizoHandArtwork();
    return 0;
}
if (args is ["--lookup-escape-gate"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Escape gate oracle revision");
    VerifyMotherBrainEscapeGateCompiledDefinitions(rom);
    VerifyMotherBrainEscapeRoomGate(new TestAddressSpace());
    VerifyEscapeGateVisuals(rom);
    Console.WriteLine("Escape gate: all original draw/program fields and complete domains pass; production door handoff and custom artwork pass.");
    return 0;
}
if (args is ["--lookup-retail-plm-headers"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Retail header oracle revision");
    VerifyRetailPlmHeaderSetups(rom);
    VerifyRetailPlmHeaderInstructions(rom);
    VerifyCompiledRoomPlmHeaderLoad(rom);
    Console.WriteLine("Retail PLM headers: all seventy setup/list cases and full input domain match ROM; guarded population load passes.");
    return 0;
}
if (args is ["--downward-gate-definitions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Downward gate oracle revision");
    VerifyDownwardGateShotBlockDefinitions(rom);
    return 0;
}
if (args is ["--speed-booster-escape-definitions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Speed escape stage oracle revision");
    VerifySpeedBoosterEscapeStageDefinitions(rom);
    return 0;
}
if (args is ["--lookup-speed-escape-program"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Speed escape oracle revision");
    VerifySpeedEscapeProgramControls(rom);
    VerifySpeedEscapeProgramCallbacks(rom);
    VerifySpeedBoosterEscapePlm(new TestAddressSpace());
    Console.WriteLine("Speed escape program: all native fields and complete address domain pass; production handoffs and lava completion pass.");
    return 0;
}
if (args is ["--maridia-elevatube-plm"])
{
    VerifyMaridiaElevatubePlm();
    return 0;
}
if (args is ["--maridia-elevatube-visuals"])
{
    VerifyMaridiaElevatubeVisuals();
    return 0;
}
if (args is ["--mother-brain-fake-death-visuals"])
{
    VerifyMotherBrainFakeDeathVisuals();
    return 0;
}
if (args is ["--mother-brain-fake-death-plms"])
{
    VerifyCompiledMotherBrainFakeDeathPlms();
    return 0;
}
if (args is ["--crocomire-arena-plms"])
{
    VerifyCompiledCrocomireArenaPlms();
    return 0;
}
if (args is ["--crocomire-arena-visuals"])
{
    VerifyCrocomireArenaVisuals();
    return 0;
}
if (args is ["--tourian-access-plms"])
{
    VerifyCompiledTourianAccessPlmPrograms();
    return 0;
}

if (args is ["--tourian-access-visuals"])
{
    VerifyTourianAccessVisuals();
    return 0;
}

if (args is ["--spore-spawn-ceiling-visuals"])
{
    VerifySporeSpawnCeilingVisuals();
    return 0;
}
if (args is ["--spore-spawn-ceiling-plms"])
{
    VerifyCompiledSporeSpawnCeilingPlms();
    return 0;
}
if (args is ["--samus-eater-plm-definitions"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "plant program NTSC J/U oracle");
    VerifySamusEaterPlmDefinitions(rom);
    return 0;
}
if (args is ["--samus-eater-visuals"])
{
    VerifySamusEaterVisuals();
    return 0;
}
if (args is ["--lookup-special-air"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Special-air NTSC J/U v1.0 oracle");
    VerifyQuicksandDefinitions(oracle);
    VerifyQuicksand();
    return 0;
}
if (args is ["--lookup-speed-blocks"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Speed blocks NTSC J/U v1.0 oracle");
    VerifyCompiledSpeedBoosterPlmPrograms();
    return 0;
}
if (args is ["--lookup-grapple-blocks"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Grapple blocks NTSC J/U v1.0 oracle");
    VerifyGrappleBlockPrograms();
    return 0;
}
if (args is ["--lookup-block-reaction-selectors"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Block reactions NTSC J/U v1.0 oracle");
    VerifySharedBreakAnimationSelection(oracle);
    VerifyBombedRevealPhysicalDrawMapping(oracle);
    VerifyBombedRevealControlMapping(oracle);
    VerifyBombedRevealDrawMapping(oracle);
    VerifyContactCrumbleHeaderSelection(oracle);
    VerifyCollisionBombInstructionSelection(oracle);
    VerifyReactionBombInstructionSelection(oracle);
    VerifyCrumbleRevealInstructionSelection(oracle);
    VerifyContactCrumbleInstructionSelection(oracle);
    VerifyBombSpecialInstructionSelection(oracle);
    VerifyBombBlockPrograms();
    VerifyContactCrumblePrograms();
    Console.WriteLine("Block reaction selectors: six complete native mappings and bomb/crumble production fixtures pass.");
    return 0;
}
if (args is ["--lookup-phase-two-rear-leg"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Phase-two rear-leg NTSC J/U v1.0 oracle");
    VerifyStream3PhaseTwoRearLeg(oracle);
    Console.WriteLine("Phase-two rear leg: 15 native colors/destinations and 45 independent RGB edits pass.");
    return 0;
}
if (args is ["--lookup-auxiliary-palettes"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Auxiliary palette NTSC J/U v1.0 oracle");
    VerifyStream3AuxiliaryPalettes(oracle);
    Console.WriteLine("Auxiliary palettes: 393 native colors, 1179 independent edits, identities and bounds pass.");
    return 0;
}
if (args is ["--lookup-stream5-door-quake-decoding"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Door quake NTSC J/U v1.0 oracle");
    VerifyLookupStream5DoorQuakeDecoding(oracle);
    return 0;
}
if (args is ["--lookup-arm-cannon-selectors"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Arm cannon NTSC J/U v1.0 oracle");
    VerifyLookupStream1ArmCannonTileSources(oracle);
    Console.WriteLine("Arm cannon selectors: native directions/frames, full reverse domain, independent edits/hash, DMA pixels and boundaries pass.");
    return 0;
}
if (args is ["--lookup-stream5-ceres-flight-palette"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Ceres flight NTSC J/U v1.0 oracle");
    VerifyLookupStream5CeresFlightPalette(oracle);
    return 0;
}
if (args is ["--lookup-stream-4-oum-layout"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Oum NTSC J/U v1.0 oracle");
    VerifyLookupStream4OumListLayout(oracle);
    return 0;
}
if (args is ["--lookup-stream-4-elevatube"])
{
    VerifyMaridiaElevatubePlm();
    VerifyMaridiaElevatubeVisuals();
    return 0;
}
if (args is ["--lookup-stream-4-breakup-order"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Ridley breakup NTSC J/U v1.0 oracle");
    VerifyRidleyExplosionDefinitions(oracle);
    VerifyLookupStream4BreakupOrder(oracle);
    return 0;
}
if (args is ["--lookup-stream-3"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Stream 3 NTSC J/U v1.0 oracle");
    VerifyLookupStream3(oracle);
    return 0;
}
if (args is ["--lookup-shot-block-draws"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shot-block NTSC J/U v1.0 oracle");
    VerifyShotBlockPlmPrograms();
    VerifyCompiledBotwoonWallPlms();
    return 0;
}
if (args is ["--lookup-botwoon-wall"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon wall NTSC J/U v1.0 oracle");
    VerifyCompiledBotwoonWallPlms();
    VerifyBotwoonWallStockMapping(oracle);
    VerifyBotwoonWallVisualIdMapping();
    VerifyBotwoonWallVisualSeparation(new SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog(
        [new("clear-wall", [0xff,0xff,0xff,0x58,0xff,0xff,0xff,0xff,0xff])]));
    return 0;
}
if (args is ["--lookup-botwoon-hole-bounds"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon hole NTSC J/U v1.0 oracle");
    VerifyBotwoonHoleRightBounds(oracle);
    VerifyBotwoonHoleBottomBounds(oracle);
    Console.WriteLine("Botwoon hole bounds: all native right/bottom edges and production inclusion/exclusion checks pass.");
    return 0;
}
if (args is ["--lookup-botwoon-path-descriptors"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon navigation NTSC J/U v1.0 oracle");
    VerifyBotwoonPathDescriptorMappings(oracle);
    return 0;
}
if (args is ["--lookup-botwoon-projectile-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon projectile NTSC J/U v1.0 oracle");
    VerifyBotwoonProjectileInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-botwoon-program-selection"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon selection NTSC J/U v1.0 oracle");
    VerifyBotwoonInstructionDefinitions(oracle);
    return 0;
}
if (args is ["--lookup-botwoon-head-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon NTSC J/U v1.0 oracle");
    VerifyBotwoonInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-torizo-statue-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Bomb Torizo statue NTSC J/U v1.0 oracle");
    VerifyBombTorizoStatueFragmentDefinitions(oracle);
    VerifyBombTorizoStatueInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-torizo-dormant-and-drool"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Bomb Torizo NTSC J/U v1.0 oracle");
    VerifyBombTorizoDormantDefinitions(oracle);
    VerifyBombTorizoDroolInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-face-block-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Face block NTSC J/U v1.0 oracle");
    VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-beetom-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Beetom NTSC J/U v1.0 oracle");
    VerifyBeetomInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-platform-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Platform NTSC J/U v1.0 oracle");
    VerifyPlatformInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-elevator-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Elevator NTSC J/U v1.0 oracle");
    VerifyElevatorInputDefinitions(oracle);
    VerifyElevatorInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-growing-shutter-definitions"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Growing shutter NTSC J/U v1.0 oracle");
    VerifyCompiledGrowingShutters(oracle);
    return 0;
}
if (args is ["--lookup-shutter-visuals"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shutter NTSC J/U v1.0 oracle");
    VerifyShutterVisualPointerMapping(oracle);
    VerifyGrowingShutterInstructionProgramDefinitions(oracle);
    VerifyHorizontalShutterInstructionProgramDefinitions(oracle);
    VerifyVerticalShutterInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-shutter-pose-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shutter NTSC J/U v1.0 oracle");
    VerifyGrowingShutterInstructionProgramDefinitions(oracle);
    VerifyHorizontalShutterInstructionProgramDefinitions(oracle);
    return 0;
}
if (args is ["--lookup-shutter-initial-functions"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shutter NTSC J/U v1.0 oracle");
    VerifyVerticalShutterInitialFunctionSelection(oracle);
    VerifyHorizontalShutterInitialFunctionSelection(oracle);
    Console.WriteLine("Shutter initial function selection: both native five-entry mappings, state semantics and invalid offsets pass.");
    return 0;
}
if (args is ["--lookup-vertical-shutter-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Vertical shutter NTSC J/U v1.0 oracle");
    VerifyVerticalShutterInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-alcoon-fireball-programs"])
{
    VerifyAlcoonFireballInstructionProgramDefinitions();
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-fune-namihe-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)),
        "Fune/Namihe actor oracle is NTSC J/U v1.0");
    VerifyFuneNamiheDefinitions(oracle);
    VerifyFuneNamiheInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-fune-namihe-fireball-programs"])
{
    var oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)),
        "Fune/Namihe fireball oracle is NTSC J/U v1.0");
    VerifyFuneNamiheFireballInstructionProgramDefinitions(oracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-enemy-fireball-launches"])
{
    var launchOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(launchOracle.Rom)),
        "Enemy fireball launch oracle is NTSC J/U v1.0");
    VerifyCompiledEnemyFireballLaunches(launchOracle);
    return 0;
}
if (args is ["--lookup-boyon-programs"])
{
    var boyonOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(boyonOracle.Rom)),
        "Boyon oracle is NTSC J/U v1.0");
    VerifyBoyonInstructionProgramDefinitions(boyonOracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-boulder-programs"])
{
    var boulderOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(boulderOracle.Rom)),
        "Boulder oracle is NTSC J/U v1.0");
    VerifyBoulderInstructionProgramDefinitions(boulderOracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-alcoon-programs"])
{
    var alcoonOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(alcoonOracle.Rom)),
        "Alcoon oracle is NTSC J/U v1.0");
    VerifyAlcoonMechanicsMapping(alcoonOracle);
    VerifyAlcoonPresentationAddressMapping();
    VerifyAlcoonVisualSelectorMapping(alcoonOracle);
    VerifyCompiledEnemyVisualSelectors();
    Console.WriteLine("Alcoon programs: all 68 native words, 44 visual positions, full ownership domains and bounds pass.");
    return 0;
}
if (args is ["--lookup-atomic-programs"])
{
    var atomicOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(atomicOracle.Rom)),
        "Atomic oracle is NTSC J/U v1.0");
    VerifyAtomicMovementDefinitions(atomicOracle);
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--lookup-draygon-intro-commands"])
{
    var danceOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(danceOracle.Rom)),
        "Draygon intro oracle is NTSC J/U v1.0");
    VerifyDraygonIntroMovementDefinitions(danceOracle);
    Console.WriteLine("Draygon intro: explicit delete selection and all 1104 original X/Y results and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-draws"])
{
    var drawOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(drawOracle.Rom)),
        "Kraid draw oracle is NTSC J/U v1.0");
    VerifyKraidDrawAddresses();
    VerifyKraidDrawOwnerClassification(drawOracle);
    VerifyKraidDrawShapes(drawOracle);
    VerifyKraidDrawWords(drawOracle);
    VerifyKraidDrawVisualIds();
    VerifyKraidRoomVisualSelection();
    Console.WriteLine("Kraid draw definitions: ten identities, eight owners, native shapes, all 45 words, visual IDs and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-room-programs"])
{
    var roomProgramOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(roomProgramOracle.Rom)),
        "Kraid room program oracle is NTSC J/U v1.0");
    VerifyKraidRoomProgramMapping(roomProgramOracle);
    return 0;
}
if (args is ["--lookup-kraid-room-visual-selection"])
{
    VerifyKraidRoomVisualSelection();
    return 0;
}
if (args is ["--lookup-fake-kraid-spike-rows"])
{
    var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Fake Kraid spike-row oracle revision");
    VerifyFakeKraidSpikeRowSelection(rom);
    Console.WriteLine("Fake Kraid spike rows: all three original signed launch positions and invalid selectors pass.");
    return 0;
}
if (args is ["--lookup-fake-kraid-spit-velocities"])
{
    var velocityOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(velocityOracle.Rom)),
        "Fake Kraid velocity oracle is NTSC J/U v1.0");
    VerifyFakeKraidSpitHorizontalVelocity(velocityOracle);
    VerifyFakeKraidSpitVerticalVelocity(velocityOracle);
    Console.WriteLine("Fake Kraid spit: both velocity fields match all four native launches; invalid ordinals pass.");
    return 0;
}
if (args is ["--lookup-fake-kraid-projectile-programs"])
{
    var projectileProgramOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(projectileProgramOracle.Rom)),
        "Fake Kraid projectile program oracle is NTSC J/U v1.0");
    VerifyFakeKraidProjectileMechanicsMapping(projectileProgramOracle);
    VerifyFakeKraidProjectilePresentationAddresses();
    Console.WriteLine("Fake Kraid projectile programs: six native control words, complete bank ownership, three visual operand positions and bounds pass.");
    return 0;
}
if (args is ["--lookup-fake-kraid-programs"])
{
    var programOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(programOracle.Rom)),
        "Fake Kraid program oracle is NTSC J/U v1.0");
    VerifyFakeKraidMechanicsMapping(programOracle);
    VerifyFakeKraidPresentationAddresses();
    Console.WriteLine("Fake Kraid programs: 48 native control words, complete bank ownership, 24 visual operand positions and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-visual-selectors"])
{
    var visualOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(visualOracle.Rom)),
        "Kraid visual oracle is NTSC J/U v1.0");
    VerifyKraidNailVisualSelectors(visualOracle);
    VerifyFakeKraidVisualSelectors(visualOracle);
    VerifyCompiledEnemyVisualSelectors();
    Console.WriteLine("Kraid visual selectors: eight nail operands for both actors,24 Fake Kraid operands, holes and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-spit-speeds"])
{
    var spitOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(spitOracle.Rom)),
        "Kraid spit oracle is NTSC J/U v1.0");
    VerifyKraidRockLaunchDefinitions(spitOracle, definitionsOnly: true);
    return 0;
}
if (args is ["--lookup-kraid-palette-definitions"])
{
    var paletteOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(paletteOracle.Rom)),
        "Kraid palette oracle is NTSC J/U v1.0");
    VerifyKraidPaletteSourceAddresses(paletteOracle);
    VerifyKraidPaletteSourceLengths();
    Console.WriteLine("Kraid palette definitions: five native source operands, complete extents and invalid enums pass.");
    return 0;
}
if (args is ["--lookup-kraid-color-sources"])
{
    VerifyKraidColorSourceSelection();
    return 0;
}
if (args is ["--lookup-kraid-health-thresholds"])
{
    var healthOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(healthOracle.Rom)),
        "Kraid health oracle is NTSC J/U v1.0");
    VerifyKraidHealthEighths(healthOracle);
    VerifyKraidHealthQuarters(healthOracle);
    Console.WriteLine("Kraid health thresholds: both native recurrences across every initial health and ordinal, plus bounds, pass.");
    return 0;
}
if (args is ["--lookup-kraid-ceiling-order"])
{
    var ceilingOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ceilingOracle.Rom)),
        "Kraid ceiling oracle is NTSC J/U v1.0");
    VerifyKraidCeilingRockCoordinates(ceilingOracle);
    VerifyKraidGrowthPlmColumns(ceilingOracle);
    VerifyKraidGrowthPlmRows(ceilingOracle);
    VerifyKraidGrowthPlmHeaders(ceilingOracle);
    Console.WriteLine("Kraid ceiling order: every native byte window and all derived PLM fields match; bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-sink-mappings"])
{
    var sinkOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(sinkOracle.Rom)),
        "Kraid sinking oracle is NTSC J/U v1.0");
    VerifyKraidSinkSchedule(sinkOracle, definitionsOnly: true);
    return 0;
}
if (args is ["--lookup-kraid-defeated-plms"])
{
    var defeatedPlmOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(defeatedPlmOracle.Rom)),
        "Kraid defeated PLM oracle is NTSC J/U v1.0");
    VerifyKraidDefeatedPlmColumns(defeatedPlmOracle);
    VerifyKraidDefeatedPlmRows(defeatedPlmOracle);
    VerifyKraidDefeatedPlmHeaders(defeatedPlmOracle);
    Console.WriteLine("Kraid defeated-room PLMs: both native requests, three fields, order and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-growth-plms"])
{
    var growthPlmOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(growthPlmOracle.Rom)),
        "Kraid growth PLM oracle is NTSC J/U v1.0");
    VerifyKraidGrowthPlmColumns(growthPlmOracle);
    VerifyKraidGrowthPlmRows(growthPlmOracle);
    VerifyKraidGrowthPlmHeaders(growthPlmOracle);
    Console.WriteLine("Kraid growth PLMs: all nine native columns, rows and headers, enumeration and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-movement"])
{
    var movementOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(movementOracle.Rom)),
        "Kraid movement oracle is NTSC J/U v1.0");
    VerifyKraidMovementChoices(movementOracle, definitionsOnly: true);
    return 0;
}
if (args is ["--lookup-kraid-nail-launch"])
{
    var launchOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(launchOracle.Rom)),
        "Kraid launch oracle is NTSC J/U v1.0");
    VerifyKraidNailLaunchXFraction(launchOracle);
    VerifyKraidNailLaunchXWhole(launchOracle);
    VerifyKraidNailLaunchYFraction(launchOracle);
    VerifyKraidNailLaunchYWhole(launchOracle);
    Console.WriteLine("Kraid nail launch: four fields, all sibling words and four RNG choices match native indirect records.");
    return 0;
}
if (args is ["--lookup-kraid-camera"])
{
    var cameraOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(cameraOracle.Rom)),
        "Kraid camera oracle is NTSC J/U v1.0");
    VerifyKraidInitialScrollMapping(cameraOracle);
    VerifyKraidGrownScrollMapping(cameraOracle);
    Console.WriteLine("Kraid camera: both four-screen mappings match native instruction operands; bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-arm-component-selectors"])
{
    var armComponentOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armComponentOracle.Rom)),
        "Kraid arm component oracle is NTSC J/U v1.0");
    VerifyKraidArmCollisionDefinitions(armComponentOracle);
    return 0;
}
if (args is ["--lookup-kraid-arm-callbacks"])
{
    var armCallbackOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armCallbackOracle.Rom)),
        "Kraid arm callback oracle is NTSC J/U v1.0");
    VerifyKraidArmCollisionDefinitions(armCallbackOracle);
    return 0;
}
if (args is ["--lookup-kraid-arm-physical-frames"])
{
    var armFrameOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armFrameOracle.Rom)),
        "Kraid arm physical-frame oracle is NTSC J/U v1.0");
    VerifyKraidArmComponentHitboxes(armFrameOracle);
    VerifyKraidArmPhysicalFramePointers(armFrameOracle);
    _ = VerifyKraidArmPhysicalLayoutSelection(armFrameOracle);
    Console.WriteLine("Kraid arm physical frames: all 22 roots, ordered native layouts, complete pointer domain and ordinal bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-foot-coordinates"])
{
    var footOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(footOracle.Rom)),
        "Kraid foot coordinate oracle is NTSC J/U v1.0");
    VerifyKraidFootFirstX(footOracle);
    VerifyKraidFootFirstY(footOracle);
    VerifyKraidFootSecondX(footOracle);
    VerifyKraidFootSecondY(footOracle);
    VerifyKraidFootSharedHitbox(footOracle);
    Console.WriteLine("Kraid foot coordinates: four fields across 35 frames plus initial alias, shared geometry/callbacks, membership and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-contour-cases"])
{
    var contourOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(contourOracle.Rom)),
        "Kraid contour oracle is NTSC J/U v1.0");
    VerifyKraidBodyContourCases(contourOracle);
    VerifyKraidGrowthResumeCases(contourOracle);
    Console.WriteLine("Kraid body contour and growth resume: full signed-Y and tilemap domains match native records and instruction operands.");
    return 0;
}
if (args is ["--lookup-kraid-nail-contour"])
{
    var nailOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(nailOracle.Rom)),
        "Kraid nail oracle is NTSC J/U v1.0");
    VerifyKraidNailLeftOffsets(nailOracle);
    VerifyKraidNailTopBoundaries(nailOracle);
    Console.WriteLine("Kraid nail contour: all 12 original words, all 65536 wrapped relative-Y selections and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-mouth-mappings"])
{
    var mouthOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(mouthOracle.Rom)),
        "Kraid mouth oracle is NTSC J/U v1.0");
    VerifyKraidMouthShapeCases(mouthOracle);
    VerifyKraidLowHalfBoundaryMapping(mouthOracle);
    Console.WriteLine("Kraid mouth mappings: 32 geometry words, 7 instruction bytes, head/mouth boundary crossings and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-head-programs"])
{
    var headOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(headOracle.Rom)),
        "Kraid head oracle is NTSC J/U v1.0");
    VerifyKraidHeadCommandMapping(headOracle);
    Console.WriteLine("Kraid head programs: all 28 native commands, frame fields, sound IDs, enumeration, exact cursors and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-rock-programs"])
{
    var rockOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rockOracle.Rom)),
        "Kraid rock program oracle is NTSC J/U v1.0");
    VerifyKraidRockMechanicsMapping(rockOracle);
    VerifyKraidRockPresentationMapping();
    Console.WriteLine("Kraid rock programs: 12 mechanics words, 7 presentation offsets, native domains and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-small-programs"])
{
    var smallOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(smallOracle.Rom)),
        "Kraid small-program oracle is NTSC J/U v1.0");
    VerifyKraidLintMechanicsMapping(smallOracle);
    VerifyKraidLintPresentationMapping();
    VerifyKraidNailMechanicsMapping(smallOracle);
    VerifyKraidNailPresentationMapping();
    Console.WriteLine("Kraid lint/nail programs: 14 mechanics words, 10 presentation offsets, native domains and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-arm-programs"])
{
    var armOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armOracle.Rom)),
        "Kraid arm program oracle is NTSC J/U v1.0");
    VerifyKraidArmGeneratedMechanics(armOracle);
    VerifyKraidArmGeneratedPresentation(armOracle);
    Console.WriteLine("Kraid arm programs: 66 mechanics words, 57 presentation positions, native domains and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-foot-programs"])
{
    var programOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(programOracle.Rom)),
        "Kraid foot program oracle is NTSC J/U v1.0");
    VerifyKraidFootGeneratedMechanics(programOracle);
    VerifyKraidFootGeneratedPresentation(programOracle);
    Console.WriteLine("Kraid foot programs: 193 mechanics words, 106 presentation positions, native domains and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-skeleton-frames"])
{
    var skeletonOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(skeletonOracle.Rom)),
        "Skeleton frame oracle is NTSC J/U v1.0");
    VerifyCrocomireSkeletonFrameGeometry(skeletonOracle);
    VerifyExtendedFrameSequence();
    Console.WriteLine("Crocomire skeleton catalog: 33 native roots, geometry, names, enumeration, membership and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-foot-frames"])
{
    var footOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(footOracle.Rom)),
        "Kraid foot oracle is NTSC J/U v1.0");
    VerifyKraidFootFrameGeometry(footOracle);
    VerifyExtendedFrameSequence();
    Console.WriteLine("Kraid foot catalog: 35 native roots, names, geometry, enumeration and bounds pass.");
    return 0;
}
if (args is ["--lookup-pirate-artwork-names"])
{
    VerifyWalkingPirateArtworkNames();
    VerifyWallPirateArtworkNames();
    VerifyExtendedFrameSequence();
    Console.WriteLine("Pirate artwork names: all 55 published keys and bounds pass.");
    return 0;
}
if (args is ["--lookup-boss-oam-roots"])
{
    var rootOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rootOracle.Rom)),
        "Boss OAM root oracle is NTSC J/U v1.0");
    VerifyRidleyOamRootGeometry(rootOracle);
    VerifyDraygonOamRootGeometry(rootOracle);
    VerifySporeSpawnOamRootGeometry(rootOracle);
    VerifyExtendedFrameSequence();
    Console.WriteLine("Boss OAM roots: 71 original identities, native geometry and bounds pass.");
    return 0;
}
if (args is ["--lookup-extended-frame-sequence"])
{
    VerifyExtendedFrameSequence();
    return 0;
}
if (args is ["--lookup-mother-brain-visual-catalogs"])
{
    var catalogOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(catalogOracle.Rom)),
        "Mother Brain catalog oracle is NTSC J/U v1.0");
    VerifyMotherBrainGeneratedOamCatalog(catalogOracle);
    VerifyMotherBrainGeneratedBg2Catalog(catalogOracle);
    Console.WriteLine("Mother Brain catalogs: 17 OAM/16 BG2 identities and names, geometry, membership, exact JSON and loading pass.");
    return 0;
}
if (args is ["--lookup-phantoon-draygon-bg2-catalogs"])
{
    var catalogOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(catalogOracle.Rom)),
        "Boss BG2 catalog oracle is NTSC J/U v1.0");
    VerifyPhantoonGeneratedBg2Catalog(catalogOracle);
    VerifyDraygonGeneratedBg2Catalog(catalogOracle);
    Console.WriteLine("Phantoon/Draygon BG2: 56 original identities/names, geometry, selector domains, exact JSON and loading pass.");
    return 0;
}
if (args is ["--lookup-crocomire-bg2-catalog"])
{
    var catalogOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(catalogOracle.Rom)),
        "Crocomire BG2 catalog oracle is NTSC J/U v1.0");
    VerifyCrocomireBg2GeneratedCatalog(catalogOracle);
    Console.WriteLine("Crocomire BG2 catalog: 42 native identities/names, exact extracted JSON, loading and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-body-frames"])
{
    var bodyOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bodyOracle.Rom)),
        "Crocomire body oracle is NTSC J/U v1.0");
    VerifyCrocomireBodyFrameGeometry(bodyOracle);
    Console.WriteLine("Crocomire body: 50 original frame roots, native geometry, full BG2 membership domain and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-tongue-collision"])
{
    var tongueOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tongueOracle.Rom)),
        "Crocomire tongue collision oracle is NTSC J/U v1.0");
    VerifyCrocomireTongueFramePositions(tongueOracle);
    VerifyCrocomireTongueComponentCases(tongueOracle);
    Console.WriteLine("Crocomire tongue collision: nine native frame identities/components, empty hitbox cases and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-bg2-poses"])
{
    var bg2Oracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bg2Oracle.Rom)),
        "Crocomire BG2 oracle is NTSC J/U v1.0");
    VerifyCrocomireBg2ScrollDefinitions(bg2Oracle);
    Console.WriteLine("Crocomire BG2: all 17 pointers and corrections, full selector domain and wrapped scroll values pass.");
    return 0;
}
if (args is ["--lookup-crocomire-melt-transfers"])
{
    var meltOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(meltOracle.Rom)),
        "Crocomire melt oracle is NTSC J/U v1.0");
    VerifyCrocomireMeltHeaders(meltOracle);
    VerifyCrocomireMeltCopies(meltOracle);
    VerifyCrocomireMeltUploads(meltOracle);
    Console.WriteLine("Crocomire melt: two headers, 13 copies, 13 uploads, sentinels and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-projectile-programs"])
{
    var projectileOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(projectileOracle.Rom)),
        "Crocomire projectile oracle is NTSC J/U v1.0");
    VerifyCrocomireProjectileMechanicsDispatch(projectileOracle);
    VerifyCrocomireProjectilePresentationPositions(projectileOracle);
    Console.WriteLine("Crocomire projectiles: 22 native control words, 13 presentation positions, byte ownership and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-tongue-program"])
{
    var tongueOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tongueOracle.Rom)),
        "Crocomire tongue oracle is NTSC J/U v1.0");
    VerifyCrocomireTongueMechanicsDispatch(tongueOracle);
    VerifyCrocomireTonguePresentationPositions(tongueOracle);
    Console.WriteLine("Crocomire tongue: 14 native control words, nine presentation positions, byte ownership and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-spike-motion"])
{
    var spikeOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(spikeOracle.Rom)),
        "Crocomire spike oracle is NTSC J/U v1.0");
    VerifyCrocomireSpikeAccelerationDelta(spikeOracle);
    VerifyCrocomireSpikeMaximumAcceleration(spikeOracle);
    VerifyCrocomireSpikeMaximumVelocity(spikeOracle);
    Console.WriteLine("Crocomire spike motion: all 54 native slot values and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-combo-sine-alias"])
{
    var comboOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(comboOracle.Rom)),
        "Combo sine oracle is NTSC J/U v1.0");
    VerifyComboSineOffsetAlias(comboOracle);
    Console.WriteLine("Combo sine alias: all 65,536 byte-angle/radius pairs match native split multiplication.");
    return 0;
}
if (args is ["--lookup-zoa-animation"])
{
    var animationOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(animationOracle.Rom)),
        "Zoa animation oracle is NTSC J/U v1.0");
    VerifyZoaAnimationDefinitions(animationOracle, definitionsOnly: true);
    return 0;
}
if (args is ["--lookup-zoa-program"])
{
    var programOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(programOracle.Rom)),
        "Zoa program oracle is NTSC J/U v1.0");
    VerifyZoaMechanicsDispatch(programOracle);
    VerifyZoaPresentationPositions(programOracle);
    Console.WriteLine("Zoa programs: 26 original control words, 12 presentation positions, byte ownership and bounds pass.");
    return 0;
}
if (args is ["--lookup-zoa-speed-cases"])
{
    var zoaOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(zoaOracle.Rom)),
        "Zoa oracle is NTSC J/U v1.0");
    VerifyCompiledZoaSpeeds(zoaOracle, definitionsOnly: true);
    return 0;
}
if (args is ["--lookup-grapple-origin-cases"])
{
    var originOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(originOracle.Rom)),
        "Grapple origin oracle is NTSC J/U v1.0");
    VerifyGrappleOriginXSelection(originOracle);
    VerifyGrappleOriginDefaultYSelection(originOracle);
    VerifyGrappleOriginRunningYSelection(originOracle);
    Console.WriteLine("Grapple origin cases: all 40 original words across three mappings and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-suit-beam-curve"])
{
    var suitOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(suitOracle.Rom)),
        "Suit beam oracle is NTSC J/U v1.0");
    VerifySuitPickupBeamCurveDefinitions(suitOracle);
    return 0;
}
if (args is ["--lookup-absolute-tangent"])
{
    var tangentOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tangentOracle.Rom)),
        "Tangent oracle is NTSC J/U v1.0");
    VerifyCompiledAbsoluteTangent(tangentOracle, definitionsOnly: true);
    Console.WriteLine("Absolute tangent algorithm: all 129 original words, direct caller aliases and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-shaktool-joint-algorithms"])
{
    var jointOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(jointOracle.Rom)),
        "Shaktool joint oracle is NTSC J/U v1.0");
    VerifyShaktoolInitialAngleAlgorithm(jointOracle);
    VerifyShaktoolAngularVelocityAlgorithm(jointOracle);
    Console.WriteLine("Shaktool joint algorithms: all 14 original words, velocity alias and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-power-bomb-base-curves"])
{
    var powerBombOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(powerBombOracle.Rom)),
        "Power Bomb oracle is NTSC J/U v1.0");
    VerifyPowerBombWidthAlgorithm(powerBombOracle);
    VerifyPowerBombTopOffsetAlgorithm(powerBombOracle);
    Console.WriteLine("Power Bomb base curves: all 64 original bytes and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-shaktool-orbit"])
{
    var orbitOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(orbitOracle.Rom)),
        "Shaktool oracle is NTSC J/U v1.0");
    VerifyShaktoolOrbitAlgorithm(orbitOracle);
    Console.WriteLine("Shaktool orbit: all320 words, rounding intervals,256 displacement pairs and bounds pass.");
    return 0;
}
if (args is ["--lookup-signed-sine-review"])
{
    var signedOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(signedOracle.Rom)),
        "Signed sine oracle is NTSC J/U v1.0");
    VerifyCompiledSignedTrigonometry(signedOracle, definitionsOnly: true);
    VerifySignedSixteenBitSineDefinitions(signedOracle);
    VerifyPhantoonWaveMath(signedOracle, definitionsOnly: true);
    Console.WriteLine("Signed sine review: 320 signed words, 256 sixteen-bit words and every Phantoon byte phase pass.");
    return 0;
}
if (args is ["--lookup-half-wave-algorithms"])
{
    var sineOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(sineOracle.Rom)),
        "Sine oracle is NTSC J/U v1.0");
    VerifyEightBitHalfWaveAlgorithm(sineOracle);
    VerifyUnsignedHalfWaveAlgorithm(sineOracle);
    Console.WriteLine("Half-wave algorithms: all 256 original samples, rounding intervals and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-spc-pitch-basis"])
{
    var pitchOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pitchOracle.Rom)),
        "SPC pitch oracle is NTSC J/U v1.0");
    VerifySpcPitchBasisAlgorithm(pitchOracle);
    Console.WriteLine("SPC pitch basis: all13 original words, root bounds and invalid indices pass.");
    return 0;
}
if (args is ["--lookup-dsp-rate-algorithm"])
{
    VerifyDspRateAlgorithm();
    Console.WriteLine("DSP rates: all 32 original hardware periods and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-spc-effect-selection"])
{
    var effectOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(effectOracle.Rom)),
        "SPC effect oracle is NTSC J/U v1.0");
    VerifySpcEffectOperandSelection(effectOracle);
    Console.WriteLine("SPC effects: all 31 original operand counts and invalid index bounds pass.");
    return 0;
}
if (args is ["--lookup-spc-note-percentages"])
{
    var noteOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(noteOracle.Rom)),
        "SPC percentage oracle is NTSC J/U v1.0");
    VerifySpcNoteVolumeAlgorithm(noteOracle);
    VerifySpcNoteGateAlgorithm(noteOracle);
    Console.WriteLine("SPC note percentages: all24 original bytes, every timing-command selector and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-window-curves"])
{
    var curveOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(curveOracle.Rom)),
        "Window curve oracle is NTSC J/U v1.0");
    VerifySuitPickupBeamCurveDefinitions(curveOracle);
    VerifyCompiledAbsoluteTangent(curveOracle, definitionsOnly: true);
    Console.WriteLine("Window curves: 128 suit bytes and129 tangent words, direct readers and bounds match original data.");
    return 0;
}
if (args is ["--lookup-running-cadence-review"])
{
    var cadenceOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(cadenceOracle.Rom)),
        "Cadence oracle is NTSC J/U v1.0");
    VerifyRunningCadence(cadenceOracle, definitionsOnly: true);
    Console.WriteLine("Running cadence: nine logical mappings, all 90 catalog bytes, selector bounds and mutable/wrapped aliases pass.");
    return 0;
}
if (args is ["--lookup-shared-speed-review"])
{
    var speedOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(speedOracle.Rom)),
        "Shared speed oracle is NTSC J/U v1.0");
    VerifyCompiledLinearEnemySpeeds(speedOracle, definitionsOnly: true);
    VerifyCompiledQuadraticEnemySpeeds(speedOracle, definitionsOnly: true);
    Console.WriteLine("Existing speed algorithms: 517 linear pairs, 759 quadratic words in both copies, 757 displacements and bounds match original bytes.");
    return 0;
}
if (args is ["--lookup-grapple-launch-algorithms"])
{
    var launchOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(launchOracle.Rom)),
        "Grapple launch oracle is NTSC J/U v1.0");
    VerifyGrappleLaunchXSelection(launchOracle);
    VerifyGrappleLaunchYSelection(launchOracle);
    VerifyGrappleLaunchAngleAlgorithm(launchOracle);
    Console.WriteLine("Grapple launch: all thirty original words and each field's bounds pass.");
    return 0;
}
if (args is ["--lookup-fireflea-melt-algorithms"])
{
    var effectOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(effectOracle.Rom)),
        "Fireflea/melt oracle is NTSC J/U v1.0");
    VerifyFirefleaFx(includeXrayCapture: false);
    VerifyCrocomireMaskAlgorithm(effectOracle);
    VerifyCrocomireMeltingProductionSequence(effectOracle);
    return 0;
}
if (args is ["--lookup-fireflea-gunship-algorithms"])
{
    var motionOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(motionOracle.Rom)),
        "Fireflea/gunship oracle is NTSC J/U v1.0");
    VerifyFirefleaMovementDefinitions(motionOracle);
    VerifyGunshipMotionDefinitions(motionOracle);
    return 0;
}
if (args is ["--lookup-owtch-shake-algorithms"])
{
    var timingOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(timingOracle.Rom)),
        "Owtch/shake oracle is NTSC J/U v1.0");
    VerifyOwtchMovementDefinitions(timingOracle);
    VerifyRoomShakeDefinitions(timingOracle);
    return 0;
}
if (args is ["--lookup-slope-algorithms"])
{
    var slopeOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(slopeOracle.Rom)),
        "slope algorithm oracle is NTSC J/U v1.0");
    VerifyCompiledSlopeHeights(slopeOracle);
    VerifyCompiledSquareSlopes(slopeOracle);
    return 0;
}
if (args is ["--shaktool-segment-algorithms"])
{
    var segmentOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(segmentOracle.Rom)),
        "Shaktool segment oracle is NTSC J/U v1.0");
    VerifyShaktoolSegmentDefinitions(segmentOracle);
    return 0;
}
if (args is ["--work-robot-initial-selection"])
{
    var robotOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(robotOracle.Rom)),
        "Work Robot selection oracle is NTSC J/U v1.0");
    VerifyWorkRobotInitialSelection(robotOracle);
    return 0;
}
if (args is ["--lookup-phase-algorithms"])
{
    var phaseOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(phaseOracle.Rom)),
        "lookup phase oracle is NTSC J/U v1.0");
    ushort PhaseWord(int address) => (ushort)(phaseOracle.ReadByte(address) | phaseOracle.ReadByte(address + 1) << 8);
    VerifyCompiledBotwoonSpeeds(phaseOracle);
    VerifyComboPowerBombCostAlgorithm(PhaseWord);
    VerifyComboOriginAngleAlgorithm(PhaseWord);
    VerifyDraygonIntroLatencyDefinitions(phaseOracle);
    return 0;
}
if (args is ["--lookup-palette-algorithms"])
{
    var paletteOracle = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(paletteOracle.Rom)),
        "lookup palette oracle is NTSC J/U v1.0");
    VerifyDraygonHealthPaletteDefinitions(paletteOracle);
    VerifyBotwoonHealthPaletteDefinitions(paletteOracle);
    VerifyWorkRobotPaletteTimingDefinitions(paletteOracle);
    return 0;
}
if (args is ["--gunship-landing-compositions", var gunshipAssetRoot, var gunshipRom, var gunshipOutput])
{
    VerifyExtractedGunshipCompositions(gunshipAssetRoot, gunshipRom, gunshipOutput);
    return 0;
}
if (args is ["--gunship-landing-compositions", var gunshipInstallation])
{
    VerifyGunshipLandingCompositions(gunshipInstallation);
    return 0;
}
if (args is ["--ceres-save-repair", var repairedAssetRoot, var originalCopy, var repairedCopy, var installedEnemies])
{
    VerifyCeresSaveRepair(repairedAssetRoot, originalCopy, repairedCopy,
        EnemyTileArtworkFiles.Load(installedEnemies, overrideDirectory: null));
    return 0;
}
if (args is ["--ceres-save-repair", var repairedInstallation, var originalSave, var repairedSave])
{
    VerifyCeresSaveRepair(repairedInstallation, originalSave, repairedSave);
    return 0;
}
if (args is ["--ceres-save-startup", var ceresSaveAssets, var savedEnemyAssets])
{
    VerifyCeresSaveStartup(ceresSaveAssets,
        EnemyTileArtworkFiles.Load(savedEnemyAssets, overrideDirectory: null));
    return 0;
}
if (args is ["--ceres-save-startup", var ceresSaveInstallation])
{
    VerifyCeresSaveStartup(ceresSaveInstallation);
    return 0;
}
if (args is ["--ceres-engine-palette-binding", var ceresPaletteInstallation])
{
    VerifyCeresEnginePaletteBinding(ceresPaletteInstallation);
    return 0;
}
VerifyRuntimeAddressSpaceHasNoCartridgeApi();
VerifyDebuggerVersionCompatibility();
VerifyCpuOperandOpenBus();
if (args is ["--title-artwork-references"])
{
    VerifyTitleArtworkReferences();
    return 0;
}
if (args is ["--room-content-identity"])
{
    VerifyRoomContentIdentity();
    return 0;
}
if (args is ["--projectile-file-contracts", var projectileInstallation])
{
    VerifyProjectileFileContracts(projectileInstallation);
    return 0;
}
if (args is ["--gameplay-content-identity"])
{
    VerifyGameplayContentIdentity();
    return 0;
}
if (args is ["--ending-content-identity"])
{
    VerifyEndingContentIdentity();
    return 0;
}
if (args is ["--intro-content-identity"])
{
    VerifyIntroContentIdentity();
    return 0;
}
if (args is ["--enemy-content-identity"])
{
    VerifyEnemyContentIdentity();
    return 0;
}
if (args is ["--enemy-legacy-overrides"])
{
    VerifyEnemyLegacyOverrides();
    VerifyEnemyExtendedLegacyOverrides();
    VerifyEnemyProjectileLegacyOverrides();
    return 0;
}
if (args is ["--enemy-animation-isolation"])
{
    VerifyEnemyAnimationIsolation();
    return 0;
}
if (args is ["--enemy-effect-resources"])
{
    VerifyEnemyEffectResources();
    return 0;
}
if (args is ["--enemy-effect-isolation"])
{
    VerifyCrocomireEffectIsolation();
    VerifyMotherBrainRotIsolation();
    return 0;
}
if (args is ["--mother-brain-corpse-stock-artwork"])
{
    VerifyMotherBrainCorpseStockArtwork();
    return 0;
}
if (args is ["--crocomire-melt-json"])
{
    VerifyCrocomireMeltJson();
    return 0;
}
if (args is ["--mother-brain-body-presentation"])
{
    VerifyMotherBrainBodyPresentation();
    return 0;
}
if (args is ["--boss-display-bindings"])
{
    VerifyBossDisplayBindings();
    return 0;
}
if (args is ["--boss-display-stock"])
{
    VerifyBossDisplayStock();
    return 0;
}
if (args is ["--tilemap-json-contracts"])
{
    VerifyTilemapJsonContracts();
    return 0;
}
if (args is ["--room-asset-json-contracts", var roomAssetRoot])
{
    VerifyRoomAssetJsonContracts(roomAssetRoot);
    return 0;
}
if (args is ["--palette-json-contracts"])
{
    VerifyPaletteJsonContracts();
    return 0;
}
if (args is ["--kraid-installed-presentation"])
{
    VerifyKraidInstalledPresentation();
    return 0;
}
if (args is ["--mother-brain-body-stock-presentation"])
{
    VerifyMotherBrainBodyStockPresentation();
    return 0;
}
if (args is ["--enemy-animation-stock-parity"])
{
    VerifyEnemyAnimationStockParity();
    return 0;
}
if (args is ["--catalog-identity-state-compatibility"])
{
    VerifyCatalogIdentityStateCompatibility();
    return 0;
}
if (args is ["--plm-content-identity"])
{
    VerifyPlmContentIdentity();
    return 0;
}
if (args is ["--samus-content-identity"])
{
    VerifySamusContentIdentity();
    return 0;
}
if (args is ["--wram-helper-boundary"])
{
    VerifyWramHelperBoundary();
    return 0;
}
if (args is ["--plm-population-input-boundary"])
{
    VerifyPlmPopulationInputBoundary();
    return 0;
}
if (args is ["--mechanics-workram-boundary"])
{
    VerifyMechanicsWorkRamBoundary();
    return 0;
}
if (args is ["--plm-draw-clone"])
{
    VerifyPlmDrawClone();
    return 0;
}
if (args is ["--enemy-definition-boundary", string enemyDefinitionRom])
{
    VerifyEnemyDefinitionBoundary(enemyDefinitionRom);
    return 0;
}
if (args is ["--enemy-gameplay-acceptance"])
{
    VerifyEnemyGameplayAcceptance();
    return 0;
}

if (args is ["--beam-palette-artwork"])
{
    var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    VerifyBeamPaletteArtwork(rom, BeamTileCatalog.Load(BeamTileExtractor.Extract(rom)));
    return 0;
}
if (args is ["--samus-rendering-slice"])
{
    VerifySamusRenderingSlice();
    return 0;
}
if (args is ["--samus-horizontal-speed"])
{
    VerifySamusHorizontalSpeed();
    return 0;
}
if (args is ["--samus-xray"])
{
    VerifySamusXray();
    return 0;
}
if (args is ["--samus-aerial-movement"])
{
    VerifySamusAerialMovement();
    return 0;
}
if (args is ["--samus-atmospheric-effects"])
{
    VerifySamusAtmosphericEffects();
    return 0;
}
if (args is ["--samus-atmosphere-artwork-boundary", var atmosphereRom])
{
    VerifySamusAtmosphereArtworkBoundary(atmosphereRom);
    return 0;
}
if (args is ["--samus-atmospheric-cadence", var atmosphereCadenceRom])
{
    VerifySamusAtmosphericAnimationDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(atmosphereCadenceRom));
    return 0;
}
if (args is ["--samus-aerial-turns-walljump"])
{
    VerifySamusAerialTurnsAndWallJump();
    return 0;
}
if (args is ["--samus-posture-movement"])
{
    VerifySamusPostureMovement();
    return 0;
}
if (args is ["--samus-morph-ball"])
{
    VerifySamusMorphBallMovement();
    return 0;
}
if (args is ["--samus-aimed-aerial"])
{
    VerifySamusAimedAerialMovement();
    return 0;
}
if (args is ["--samus-gun-extended"])
{
    VerifySamusGunExtendedMovement();
    return 0;
}
if (args is ["--samus-grounded-reversal"])
{
    VerifySamusGroundedReversal();
    return 0;
}
if (args is ["--enemy-angle-division"])
{
    VerifyEnemyAngleDivision();
    return 0;
}

if (args is ["--sand-animated-tiles"])
{
    VerifySandAnimatedTiles();
    return 0;
}
if (args is ["--room-fx-animated-tiles"])
{
    VerifyRoomFxAnimatedTileMechanicsDefinitions();
    VerifyRoomFxAnimatedTileArtwork();
    return 0;
}
if (args is ["--tourian-statue-animated-tiles"])
{
    VerifyTourianStatueUnlockDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--treadmill-animated-tiles"])
{
    VerifyAnimatedTileInstructionCodeCatalog();
    return 0;
}
if (args is ["--room-fx-layer3-tilemaps"])
{
    VerifyRoomFxLayer3Tilemaps();
    return 0;
}
if (args is ["--room-fx-palette-blends"])
{
    VerifyRoomFxPaletteBlends();
    return 0;
}
if (args is ["--power-bomb-fixed-colors"])
{
    VerifyPowerBombFixedColors();
    return 0;
}
if (args is ["--samus-visor-colors"])
{
    VerifySamusVisorColors();
    return 0;
}
if (args is ["--samus-hurt-colors"])
{
    VerifySamusHurtColors();
    return 0;
}




if (args is ["--normal-suit-palette-pointers"])
{
    VerifyNormalSuitPalettePointers();
    return 0;
}
if (args is ["--speed-boost-palette-pointers"])
{
    VerifySpeedBoostPalettePointers();
    return 0;
}
if (args is ["--full-body-palette-pointer-lists"])
{
    VerifyFullBodyPalettePointerLists();
    return 0;
}
if (args is ["--spc-sound-library-2-pointers"])
{
    VerifySpcSoundLibrary2Pointers();
    return 0;
}
if (args is ["--room-fx-retail-inventory"])
{
    VerifyRetailRoomFxInventory();
    return 0;
}
if (args is ["--generate-room-fx-records", var roomFxGeneratorRom])
{
    GenerateRoomFxRecordDefinitions(roomFxGeneratorRom);
    return 0;
}
if (args is ["--enemy-projectile-instruction-mechanics"])
{
    VerifyEnemyProjectileInstructionMechanicsDefinitions();
    return 0;
}
if (args is ["--enemy-pickup-instruction-mechanics"])
{
    VerifyEnemyPickupInstructionProgramDefinitions();
    return 0;
}
if (args is ["--enemy-death-instruction-mechanics"])
{
    VerifyEnemyDeathInstructionProgramDefinitions();
    return 0;
}
if (args is ["--shaktool-projectile-instruction-mechanics"])
{
    VerifyShaktoolProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--chozo-tourian-dust-instruction-mechanics"])
{
    VerifyChozoTourianDustInstructionProgramDefinitions();
    return 0;
}
if (args is ["--tourian-statue-projectile-instruction-mechanics"])
{
    VerifyTourianStatueProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--spore-spawn-projectile-instruction-mechanics"])
{
    VerifySporeSpawnProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--botwoon-projectile-instruction-mechanics"])
{
    VerifyBotwoonProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--torizo-landing-dust-instruction-mechanics"])
{
    VerifyTorizoLandingDustInstructionProgramDefinitions();
    return 0;
}
if (args is ["--torizo-explosive-swipe-instruction-mechanics"])
{
    VerifyTorizoExplosiveSwipeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--bomb-torizo-drool-instruction-mechanics"])
{
    VerifyBombTorizoDroolInstructionProgramDefinitions();
    return 0;
}
if (args is ["--torizo-explosion-instruction-mechanics"])
{
    VerifyTorizoExplosionInstructionProgramDefinitions();
    return 0;
}
if (args is ["--torizo-chozo-orb-instruction-mechanics"])
{
    VerifyTorizoChozoOrbInstructionProgramDefinitions();
    return 0;
}
if (args is ["--torizo-sonic-boom-instruction-mechanics"])
{
    VerifyTorizoSonicBoomInstructionProgramDefinitions();
    return 0;
}
if (args is ["--bomb-torizo-statue-instruction-mechanics"])
{
    VerifyBombTorizoStatueInstructionProgramDefinitions();
    return 0;
}
if (args is ["--golden-torizo-egg-instruction-mechanics"])
{
    VerifyGoldenTorizoEggInstructionProgramDefinitions();
    return 0;
}
if (args is ["--golden-torizo-super-missile-instruction-mechanics"])
{
    VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--golden-torizo-eye-beam-instruction-mechanics"])
{
    VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions();
    return 0;
}
if (args is ["--enemy-projectile-instruction-owner-coverage"])
{
    VerifyEnemyProjectileInstructionOwnerCoverage();
    return 0;
}
if (args is ["--enemy-instruction-owner-coverage"])
{
    VerifyEnemyInstructionOwnerCoverage();
    return 0;
}
if (args is ["--mother-brain-body-instruction-mechanics"])
{
    VerifyMotherBrainBodyInstructionPrograms();
    return 0;
}
if (args is ["--mother-brain-room-palette-mechanics"])
{
    VerifyMotherBrainRoomPaletteProgramDefinitions();
    return 0;
}
if (args is ["--gunship-instruction-mechanics"])
{
    VerifyGunshipInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ridley-instruction-mechanics"])
{
    VerifyRidleyInstructionProgramDefinitions();
    return 0;
}
if (args is ["--draygon-instruction-mechanics"])
{
    VerifyDraygonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--walking-space-pirate-instruction-mechanics"])
{
    VerifyWalkingSpacePirateInstructionProgramDefinitions();
    return 0;
}
if (args is ["--wall-space-pirate-instruction-mechanics"])
{
    VerifyWallSpacePirateInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ninja-space-pirate-instruction-mechanics"])
{
    VerifyNinjaSpacePirateInstructionProgramDefinitions();
    return 0;
}
if (args is ["--cacatac-projectile-instruction-mechanics"])
{
    VerifyCacatacProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--falling-spark-instruction-mechanics"])
{
    VerifyFallingSparkInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fune-namihe-fireball-instruction-mechanics"])
{
    VerifyFuneNamiheFireballInstructionProgramDefinitions();
    return 0;
}
if (args is ["--magdollite-lava-instruction-mechanics"])
{
    VerifyMagdolliteLavaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dragon-fireball-instruction-mechanics"])
{
    VerifyDragonFireballInstructionProgramDefinitions();
    return 0;
}
if (args is ["--eye-door-projectile-instruction-mechanics"])
{
    VerifyEyeDoorProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--eye-door-sweat-instruction-mechanics"])
{
    VerifyEyeDoorSweatInstructionProgramDefinitions();
    return 0;
}
if (args is ["--skree-metaree-particle-instruction-mechanics"])
{
    VerifySkreeMetareeParticleInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kraid-rock-projectile-instruction-mechanics"])
{
    VerifyKraidRockProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fake-kraid-projectile-instruction-mechanics"])
{
    VerifyFakeKraidProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--alcoon-fireball-instruction-mechanics"])
{
    VerifyAlcoonFireballInstructionProgramDefinitions();
    return 0;
}
if (args is ["--work-robot-laser-instruction-mechanics"])
{
    VerifyWorkRobotLaserInstructionProgramDefinitions();
    return 0;
}
if (args is ["--powamp-spike-instruction-mechanics"])
{
    VerifyPowampSpikeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--polyp-rock-instruction-mechanics"])
{
    VerifyPolypRockInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kihunter-acid-spit-instruction-mechanics"])
{
    VerifyKiHunterAcidSpitInstructionProgramDefinitions();
    return 0;
}
if (args is ["--stoke-projectile-instruction-mechanics"])
{
    VerifyStokeProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--nuclear-waffle-projectile-instruction-mechanics"])
{
    VerifyNuclearWaffleProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kago-bug-projectile-instruction-mechanics"])
{
    VerifyKagoBugProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--yapping-maw-body-projectile-instruction-mechanics"])
{
    VerifyYappingMawBodyProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--crocomire-projectile-instruction-mechanics"])
{
    VerifyCrocomireProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--phantoon-projectile-instruction-mechanics"])
{
    VerifyPhantoonProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--draygon-projectile-instruction-mechanics"])
{
    VerifyDraygonProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-debris-instruction-mechanics"])
{
    VerifyCeresFallingDebrisInstructionProgramDefinitions();
    return 0;
}
if (args is ["--save-station-electricity-instruction-mechanics"])
{
    VerifySaveStationElectricityInstructionProgramDefinitions();
    return 0;
}
if (args is ["--downward-gate-projectile-instruction-mechanics"])
{
    VerifyDownwardGateProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--noob-tube-projectile-instruction-mechanics"])
{
    VerifyNoobTubeProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mother-brain-top-tube-instruction-mechanics"])
{
    VerifyMotherBrainTopTubeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mother-brain-turret-instruction-mechanics"])
{
    VerifyMotherBrainTurretInstructionProgramDefinitions();
    return 0;
}
if (args is ["--gunship-dust-instruction-mechanics"])
{
    VerifyGunshipDustInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-ridley-projectile-instruction-mechanics"])
{
    VerifyCeresRidleyProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--space-pirate-projectile-instruction-mechanics"])
{
    VerifySpacePirateProjectileInstructionProgramDefinitions();
    return 0;
}
if (args is ["--eye-door-plms"])
{
    VerifyEyeDoorPlms();
    return 0;
}
if (args is ["--cacatac-instruction-mechanics"])
{
    VerifyCacatacInstructionProgramDefinitions();
    return 0;
}
if (args is ["--magdollite-instruction-mechanics"])
{
    VerifyMagdolliteInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kihunter-instruction-mechanics"])
{
    VerifyKiHunterInstructionProgramDefinitions();
    return 0;
}
if (args is ["--owtch-instruction-mechanics"])
{
    VerifyOwtchInstructionProgramDefinitions();
    return 0;
}
if (args is ["--stoke-instruction-mechanics"])
{
    VerifyStokeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ripper-instruction-mechanics"])
{
    VerifyRipperInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kzan-instruction-mechanics"])
{
    VerifyKzanInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fly-instruction-mechanics"])
{
    VerifyFlyInstructionProgramDefinitions();
    return 0;
}
if (args is ["--bull-instruction-mechanics"])
{
    VerifyBullInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kago-instruction-mechanics"])
{
    VerifyKagoInstructionProgramDefinitions();
    return 0;
}
if (args is ["--horizontal-shutter-instruction-mechanics"])
{
    VerifyHorizontalShutterInstructionProgramDefinitions();
    return 0;
}
if (args is ["--growing-shutter-instruction-mechanics"])
{
    VerifyGrowingShutterInstructionProgramDefinitions();
    return 0;
}
if (args is ["--vertical-shutter-instruction-mechanics"])
{
    VerifyVerticalShutterInstructionProgramDefinitions();
    return 0;
}
if (args is ["--choot-instruction-mechanics"])
{
    VerifyChootInstructionProgramDefinitions();
    return 0;
}
if (args is ["--norfair-lava-jumper-instruction-mechanics"])
{
    VerifyNorfairLavaJumperInstructionProgramDefinitions();
    return 0;
}
if (args is ["--beetom-instruction-mechanics"])
{
    VerifyBeetomInstructionProgramDefinitions();
    return 0;
}
if (args is ["--alcoon-instruction-mechanics"])
{
    VerifyAlcoonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--multiviola-instruction-mechanics"])
{
    VerifyMultiviolaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--polyp-instruction-mechanics"])
{
    VerifyPolypInstructionProgramDefinitions();
    return 0;
}
if (args is ["--powamp-instruction-mechanics"])
{
    VerifyPowampInstructionProgramDefinitions();
    return 0;
}
if (args is ["--wrecked-ship-ghost-instruction-mechanics"])
{
    VerifyWreckedShipGhostInstructionProgramDefinitions();
    return 0;
}
if (args is ["--puyo-instruction-mechanics"])
{
    VerifyPuyoInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dead-torizo-instruction-mechanics"])
{
    VerifyDeadTorizoInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dead-sidehopper-instruction-mechanics"])
{
    VerifyDeadSidehopperInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dead-tourian-corpse-instruction-mechanics"])
{
    VerifyDeadTourianCorpseInstructionProgramDefinitions();
    return 0;
}
if (args is ["--shitroid-instruction-mechanics"])
{
    VerifyShitroidInstructionProgramDefinitions();
    return 0;
}
if (args is ["--rio-instruction-mechanics"])
{
    VerifyRioInstructionProgramDefinitions();
    return 0;
}
if (args is ["--spore-spawn-instruction-mechanics"])
{
    VerifySporeSpawnInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-baby-instruction-mechanics"])
{
    VerifyCeresBabyInstructionProgramDefinitions();
    return 0;
}
if (args is ["--rinka-instruction-mechanics"])
{
    VerifyRinkaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fune-namihe-instruction-mechanics"])
{
    VerifyFuneNamiheInstructionProgramDefinitions();
    return 0;
}
if (args is ["--atomic-instruction-mechanics"])
{
    VerifyAtomicMovementDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--sbug-instruction-mechanics"])
{
    VerifySbugMovementDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--spark-instruction-mechanics"])
{
    VerifySparkMovementDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--nuclear-waffle-instruction-mechanics"])
{
    VerifyNuclearWaffleDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--hibashi-instruction-mechanics"])
{
    VerifyHibashiDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--blue-brinstar-face-block-instruction-mechanics"])
{
    VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions();
    return 0;
}
if (args is ["--boulder-instruction-mechanics"])
{
    VerifyBoulderInstructionProgramDefinitions();
    return 0;
}
if (args is ["--boyon-instruction-mechanics"])
{
    VerifyBoyonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--skultera-instruction-mechanics"])
{
    VerifySkulteraInstructionProgramDefinitions();
    return 0;
}
if (args is ["--waver-instruction-mechanics"])
{
    VerifyWaverInstructionProgramDefinitions();
    return 0;
}
if (args is ["--skree-metaree-instruction-mechanics"])
{
    VerifySkreeMetareeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--zoa-instruction-mechanics"])
{
    VerifyZoaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dragon-instruction-mechanics"])
{
    VerifyDragonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--brinstar-pipe-bug-instruction-mechanics"])
{
    VerifyBrinstarPipeBugInstructionProgramDefinitions();
    return 0;
}
if (args is ["--norfair-pipe-bug-instruction-mechanics"])
{
    VerifyNorfairPipeBugInstructionProgramDefinitions();
    return 0;
}
if (args is ["--yellow-pipe-bug-instruction-mechanics"])
{
    VerifyYellowPipeBugInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-steam-instruction-mechanics"])
{
    VerifyCeresSteamInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-door-instruction-mechanics"])
{
    VerifyCeresDoorInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-door-artwork"])
{
    VerifyCeresDoorQuakeDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--ceres-escape-transfers"])
{
    VerifyCeresEscapeVramTransferDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--fake-kraid-instruction-mechanics"])
{
    VerifyFakeKraidInstructionProgramDefinitions();
    return 0;
}
if (args is ["--chozo-statue-instruction-mechanics"])
{
    VerifyChozoStatueInstructionProgramDefinitions();
    return 0;
}
if (args is ["--crocomire-tongue-instruction-mechanics"])
{
    VerifyCrocomireTongueInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mother-brain-baby-instruction-mechanics"])
{
    VerifyMotherBrainBabyInstructionProgramDefinitions();
    return 0;
}
if (args is ["--botwoon-instruction-mechanics"])
{
    VerifyBotwoonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kraid-lint-instruction-mechanics"])
{
    VerifyKraidLintInstructionProgramDefinitions();
    return 0;
}
if (args is ["--ceres-elevator-arrival-definitions"])
{
    VerifyCeresElevatorArrivalGraphicsIndex();
    return 0;
}
if (args is ["--draygon-eye-effects"])
{
    VerifyDraygonEyeEffects();
    return 0;
}
if (args is ["--acid-statue-first-entry"])
{
    VerifyAcidStatueFirstEntry();
    return 0;
}
if (args is ["--demo-input-object"])
{
    VerifyDemoInputObject();
    return 0;
}
if (args is ["--door-alignment"])
{
    VerifyDoorAlignmentParity();
    return 0;
}
if (args is ["--spark-crash-alignment"])
{
    VerifySparkCrashAlignment();
    return 0;
}
if (args is ["--gate-jump-traces"])
{
    VerifyGateJumpTraces();
    return 0;
}
if (args is ["--gate-beam-collision"])
{
    VerifyKronicGateBeamCollision();
    return 0;
}
if (args is ["--right-facing-gate-glitch"])
{
    VerifyRightFacingGateGlitches();
    return 0;
}
if (args is ["--green-hill-gate-glitch"])
{
    VerifyGreenHillGrappleSpeedGateGlitch();
    return 0;
}
if (args is ["--gmode-gate-glitch"])
{
    VerifyGModeGateGlitch();
    return 0;
}
if (args is ["--frozen-gate-glitch"])
{
    VerifyFrozenEnemyGateGlitch();
    return 0;
}
if (args is ["--mochtroid-botwoon-clip"])
{
    VerifyMochtroidBotwoonPipeClip();
    return 0;
}
if (args is ["--red-tower-hero"])
{
    VerifyControlledRedTowerHeroShot();
    return 0;
}
if (args is ["--hero-shot-runtime"])
{
    VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv");
    return 0;
}
if (args is ["--hero-shot-runtime", var nativeHeroRuntimeTrace])
{
    VerifyHeroShotRuntimeCamera(nativeHeroRuntimeTrace);
    return 0;
}
if (args is ["--missile-edge", var missileEdgeTrace])
{
    VerifyMissileImpactCameraEdge(missileEdgeTrace);
    return 0;
}
if (args is ["--wrap-shots", var nativeWrapTrace])
{
    VerifyWrapShotTrace(nativeWrapTrace);
    return 0;
}
if (args is ["--ceiling-wrap", var nativeCeilingTrace])
{
    VerifyCeilingWrapPlmTrace(nativeCeilingTrace);
    return 0;
}
if (args is ["--ceiling-wrap-room"])
{
    VerifyFrogSpeedwayPoolCollision();
    return 0;
}
if (args is ["--ceiling-wrap-runtime", var nativeFrogTrace])
{
    VerifyFrogSpeedwayRuntimeTrace(nativeFrogTrace);
    return 0;
}
if (args is ["--ceiling-wrap-success", var nativeFrogSuccess, var frogDash])
{
    VerifyFrogSpeedwayRuntimeTrace(nativeFrogSuccess, 11, bool.Parse(frogDash));
    return 0;
}
if (args is ["--wrap-shot-rooms"])
{
    VerifyRetailWrapShotDoors();
    return 0;
}
if (args is ["--wrap-shot-enemies"])
{
    VerifyWrapShotEnemySeparation();
    return 0;
}
if (args is ["--wrap-shot-widths", var nativeWrapWidths])
{
    VerifyWrapShotWidths(nativeWrapWidths);
    return 0;
}
if (args is ["--hero-shots"])
{
    VerifyHeroShotCameraLifetime();
    return 0;
}
if (args is ["--hero-shots", var nativeHeroTrace])
{
    VerifyHeroShotCameraLifetime(nativeHeroTrace);
    return 0;
}
if (args is ["--projectile-inheritance-probe"])
{
    ProbeProjectileVelocityInheritance();
    return 0;
}
if (args is ["--samus-physics"])
{
    VerifySamusPhysicsBatch();
    return 0;
}
if (args is ["--samus-projectiles"])
{
    VerifySamusPowerBeamProjectiles();
    VerifyBeamSpeedRows();
    VerifyProjectileCooldowns();
    VerifyHeroShotCameraLifetime("csharp/test-fixtures/movement-release/hero-shot-411.csv");
    VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv");
    VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv");
    VerifyControlledRedTowerHeroShot();
    return 0;
}
if (args is ["--projectile-cooldowns"])
{
    VerifyProjectileCooldowns();
    return 0;
}
if (args is ["--projectile-motion"])
{
    VerifyBeamSpeedRows();
    return 0;
}
if (args is ["--botwoon-plm-identity"])
{
    VerifyBotwoonPlmIdentity();
    return 0;
}
if (args is ["--compiled-enemy-sine"])
{
    VerifyCompiledEnemyTrigonometry();
    return 0;
}
if (args is ["--charge-flare-compositions", var flareRom])
{
    VerifyChargeFlareCompositions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath(flareRom)),
        flareOnly: true, sourceRom: flareRom);
    return 0;
}
if (args is ["--projectile-sprite-compositions", var projectileRom])
{
    var source = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
        Path.GetFullPath(projectileRom));
    VerifyProjectileVisualParts();
    VerifyProjectileCompositions(source, compositionOnly: true,
        sourceRom: projectileRom);
    return 0;
}
if (args is ["--trail-mutable-alias"])
{
    VerifyTrailMutableAlias();
    return 0;
}
if (args is ["--projectile-trail-coordinates"])
{
    VerifyProjectileTrailCoordinates(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--kraid-mouth-hitboxes"])
{
    VerifyKraidMouthHitboxes(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--shaktool-instruction-mechanics"])
{
    VerifyShaktoolInstructionProgramDefinitions();
    return 0;
}
if (args is ["--speed-booster-block-plms"])
{
    VerifySpeedBoosterCollisionBlocks();
    return 0;
}

if (args is ["--speed-booster-visuals"])
{
    VerifySpeedBoosterVisuals();
    return 0;
}

if (args is ["--botwoon-wall-plms"])
{
    VerifyCompiledBotwoonWallPlms();
    return 0;
}
if (args is ["--botwoon-wall-visuals"])
{
    VerifyBotwoonWallVisuals();
    return 0;
}
if (args is ["--kraid-room-plms"])
{
    VerifyCompiledKraidRoomPlms();
    return 0;
}




if (args is ["--kraid-room-visuals"])
{
    VerifyKraidRoomVisuals();
    return 0;
}
if (args is ["--door-closing-definitions"])
{
    VerifyDoorClosingPlmDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--reported-shaft-momentum"])
{
    VerifyReportedShaftMomentum();
    return 0;
}
if (args is ["--shot-block-program-operands"])
{
    VerifyShotBlockProgramOperands();
    return 0;
}
if (args is ["--shot-block-plm-programs"])
{
    VerifyShotBlockPlmPrograms();
    return 0;
}
if (args is ["--grapple-block-programs"])
{
    VerifyGrappleBlockPrograms();
    return 0;
}
if (args is ["--bomb-block-programs"])
{
    VerifyBombBlockPrograms();
    return 0;
}
if (args is ["--contact-crumble-programs"])
{
    VerifyContactCrumblePrograms();
    return 0;
}
if (args is ["--arm-cannon-definitions"])
{
    VerifySamusArmCannonDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--tourian-access-definitions"])
{
    VerifyTourianAccessPlmDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--quicksand-definitions"])
{
    VerifyQuicksandDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifyQuicksand();
    return 0;
}
if (args is ["--save-station-animation-definitions"])
{
    VerifySaveStationAnimationDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--enemy-drop-chance-definitions"])
{
    VerifyEnemyDropChanceDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--enemy-vulnerability-definitions"])
{
    VerifyEnemyVulnerabilityDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--escape-etecoon-definitions"])
{
    VerifyEscapeEtecoonDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--escape-etecoon-instruction-mechanics"])
{
    VerifyEscapeEtecoonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--escape-dachora-instruction-mechanics"])
{
    VerifyEscapeDachoraInstructionProgramDefinitions();
    return 0;
}
if (args is ["--crocomire-instruction-mechanics"])
{
    VerifyCrocomireInstructionProgramDefinitions();
    return 0;
}
if (args is ["--yard-turn-definitions"])
{
    VerifyYardTurnDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--ridley-explosion-definitions"])
{
    VerifyRidleyExplosionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--choot-pattern-definitions"])
{
    VerifyChootPatternDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--enemy-instruction-selectors"])
{
    VerifyEnemyRomTablePointerCatalog();
    return 0;
}
if (args is ["--kraid-head-instruction-definitions"])
{
    VerifyKraidHeadInstructionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--magic-number-audit"])
{
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--save-json-persistence"])
{
    VerifyGameSaveJsonPersistence();
    return 0;
}
if (args is ["--explored-map-packing-definitions"])
{
    VerifyExploredMapPackingDefinitions();
    return 0;
}
if (args is ["--mutable-animation-aliases"])
{
    string sourceRom = Path.GetFullPath("Super Metroid.smc");
    SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(sourceRom);
    VerifySamusAnimationDelayDefinitions(bus, sourceRom);
    VerifyRunningCadence(bus);
    return 0;
}
if (args is ["--enemy-death-explosion-definitions"])
{
    VerifyEnemyDeathExplosionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--ceres-door-initialization-definitions"])
{
    VerifyCeresDoorInitializationDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--botwoon-instruction-definitions"])
{
    VerifyBotwoonInstructionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--botwoon-navigation-definitions"])
{
    VerifyBotwoonNavigationDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--enemy-projectile-definitions"])
{
    VerifyEnemyProjectileDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--room-palette-fx-definitions"])
{
    VerifyRoomPaletteFxDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--ceres-ridley-eye-fade-definitions"])
{
    VerifyCeresRidleyEyeFadeDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--crystal-flash-palette-timing-definitions"])
{
    VerifyCrystalFlashPaletteTimingDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--work-robot-palette-timing-definitions"])
{
    VerifyWorkRobotPaletteTimingDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--samus-death-explosion-timing-definitions"])
{
    VerifySamusDeathExplosionTimingDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifySamusDeathSequence();
    return 0;
}
if (args is ["--hyper-beam-palette-fx-program-definitions"])
{
    VerifyHyperBeamPaletteFxProgramDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifySamusDrainedController();
    return 0;
}
if (args is ["--suit-pickup-beam-curve-definitions"])
{
    VerifySuitPickupBeamCurveDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifyPermanentCollectibles();
    return 0;
}
if (args is ["--palette-fx-instruction-codes"])
{
    VerifyPaletteFxInstructionCodeCatalogs();
    return 0;
}
if (args is ["--enemy-drops"])
{
    VerifyEnemyDrops();
    return 0;
}
if (args is ["--room-sprite-object-definitions"])
{
    VerifyRoomSpriteObjectDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--dead-sidehopper-corpse-definitions"])
{
    VerifyDeadSidehopperCorpseDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--dead-tourian-corpse-definitions"])
{
    VerifyDeadTourianCorpseDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--dead-torizo-corpse-definitions"])
{
    VerifyDeadTorizoCorpseDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--gunship-motion-definitions"])
{
    VerifyGunshipMotionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifyPostCeresGunshipLanding();
    return 0;
}
if (args is ["--remaining-signed-sine-consumers"])
{
    SuperMetroidAddressSpace rom =
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    VerifyBombTorizoDroolSine(rom);
    VerifyMotherBrainNeckSine(rom);
    return 0;
}
if (args is ["--crocomire-bridge-fragment-definitions"])
{
    VerifyCrocomireBridgeFragmentDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--crocomire-melting-definitions"])
{
    VerifyCrocomireMeltingDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--draygon-intro-dance-definitions"])
{
    VerifyDraygonIntroDanceDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--phantoon-sound-definitions"])
{
    VerifyPhantoonSoundDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--baby-metroid-route-definitions"])
{
    VerifyBabyMetroidCutsceneEntrance();
    return 0;
}
if (args is ["--mother-brain-contact-hitboxes"])
{
    VerifyMotherBrainContactHitboxes();
    return 0;
}
if (args is ["--mother-brain-turret-definitions"])
{
    VerifyMotherBrainTurretDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--mama-turtle-shell-contour"])
{
    VerifyMamaTurtleShellContourDefinitions();
    return 0;
}
if (args is ["--maridia-large-snail-instruction-definitions"])
{
    VerifyMaridiaLargeSnailInstructionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--etecoon-instruction-program-definitions"])
{
    VerifyEtecoonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--elevator-instruction-program-definitions"])
{
    VerifyElevatorInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mochtroid-instruction-program-definitions"])
{
    VerifyMochtroidInstructionProgramDefinitions();
    return 0;
}
if (args is ["--platform-instruction-program-definitions"])
{
    VerifyPlatformInstructionProgramDefinitions();
    return 0;
}
if (args is ["--hopper-instruction-program-definitions"])
{
    VerifyHopperAnimationDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    VerifyStream3HopperOperandPositions(CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--hzoomer-instruction-program-definitions"])
{
    VerifyHZoomerInstructionProgramDefinitions();
    return 0;
}
if (args is ["--sciser-instruction-program-definitions"])
{
    VerifySciserInstructionProgramDefinitions();
    return 0;
}
if (args is ["--zero-instruction-program-definitions"])
{
    VerifyZeroInstructionProgramDefinitions();
    return 0;
}
if (args is ["--viola-instruction-program-definitions"])
{
    VerifyViolaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--shared-crawler-instruction-program-definitions"])
{
    VerifySharedCrawlerInstructionProgramDefinitions();
    return 0;
}
if (args is ["--dachora-instruction-program-definitions"])
{
    VerifyDachoraInstructionProgramDefinitions();
    return 0;
}
if (args is ["--fireflea-instruction-program-definitions"])
{
    VerifyFirefleaInstructionProgramDefinitions();
    return 0;
}
if (args is ["--zebetite-instruction-program-definitions"])
{
    VerifyZebetiteInstructionProgramDefinitions();
    return 0;
}
if (args is ["--evir-instruction-program-definitions"])
{
    VerifyEvirInstructionProgramDefinitions();
    return 0;
}
if (args is ["--morph-ball-eye-instruction-program-definitions"])
{
    VerifyMorphBallEyeInstructionProgramDefinitions();
    return 0;
}
if (args is ["--yapping-maw-instruction-program-definitions"])
{
    VerifyYappingMawInstructionProgramDefinitions();
    return 0;
}
if (args is ["--metroid-instruction-program-definitions"])
{
    VerifyMetroidInstructionProgramDefinitions();
    return 0;
}
if (args is ["--norfair-rio-instruction-program-definitions"])
{
    VerifyNorfairRioInstructionProgramDefinitions();
    return 0;
}
if (args is ["--lower-norfair-rio-instruction-program-definitions"])
{
    VerifyLowerNorfairRioInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mama-turtle-instruction-program-definitions"])
{
    VerifyMamaTurtleInstructionProgramDefinitions();
    return 0;
}
if (args is ["--tourian-entrance-statue-instruction-program-definitions"])
{
    VerifyTourianEntranceStatueInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kraid-nail-instruction-program-definitions"])
{
    VerifyKraidNailInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kraid-arm-instruction-program-definitions"])
{
    VerifyKraidArmInstructionProgramDefinitions();
    return 0;
}
if (args is ["--kraid-foot-instruction-program-definitions"])
{
    VerifyKraidFootInstructionProgramDefinitions();
    return 0;
}
if (args is ["--phantoon-instruction-program-definitions"])
{
    VerifyPhantoonInstructionProgramDefinitions();
    return 0;
}
if (args is ["--work-robot-instruction-program-definitions"])
{
    VerifyWorkRobotInstructionProgramDefinitions();
    return 0;
}
if (args is ["--yard-instruction-program-definitions"])
{
    VerifyYardInstructionProgramDefinitions();
    return 0;
}
if (args is ["--mama-turtle-enemy-definitions"])
{
    VerifyMamaTurtleEnemyDefinitions();
    return 0;
}
if (args is ["--pose-dispatch-definitions"])
{
    var poseRom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    VerifyPoseDispatchDefinitions(poseRom);
    VerifyPoseCollisionDefinitions(poseRom);
    VerifyPoseProjectileOrigin(poseRom);
    VerifySamusHudDefinitions(poseRom);
    return 0;
}
if (args is ["--pose-input-definitions"])
{
    VerifyPoseInputDefinitions(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grapple-rope-geometry"])
{
    VerifyGrappleRopeGeometry(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grapple-sprite-artwork"])
{
    VerifyGrappleSpriteArtwork(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grapple-tile-artwork"])
{
    VerifyGrappleTileArtwork(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grapple-flare-placement"])
{
    VerifyGrappleFlarePlacement(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grapple-swing-frames"])
{
    VerifyGrappleBodyPlacement(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--stock-attract-scenes"])
{
    VerifyStockAttractScenes();
    return 0;
}
if (args is ["--native-boss-markers", var bossTrace])
{
    VerifyNativeBossMarkers(bossTrace);
    return 0;
}
if (args is ["--pause-boss-markers"])
{
    VerifyPauseBossMarkers();
    return 0;
}
if (args is ["--crystal-palette-native", var paletteRom, var paletteTrace])
{
    VerifyCrystalPaletteNative(paletteRom, paletteTrace);
    return 0;
}
if (args is ["--crystal-window-native", var windowRom, var windowTrace])
{
    VerifyCrystalWindowNative(windowRom, windowTrace);
    return 0;
}
if (args is ["--crystal-flash-contact-native", var contactRom, var contactTrace])
{
    VerifyCrystalFlashContactNative(contactRom, contactTrace);
    return 0;
}
if (args is ["--crystal-flash-runtime"])
{
    VerifyCrystalFlashRuntime();
    return 0;
}
if (args is ["--crystal-flash-lifetime", var lifetimeRom, var lifetimeTrace])
{
    VerifyCrystalFlashLifetime(lifetimeRom, lifetimeTrace);
    return 0;
}
if (args is ["--crystal-flash-native", var crystalRom, var crystalTrace])
{
    VerifyCrystalFlashCleanup(crystalRom, crystalTrace);
    return 0;
}
if (args is ["--crystal-flash"])
{
    VerifySamusCrystalFlash();
    return 0;
}
if (args is ["--plasma-penetration"])
{
    VerifyPlasmaEnemyPenetration();
    return 0;
}
if (args is ["--pcm-loop-entry"])
{
    VerifyManagedDspUsesIndependentLoopEntry();
    return 0;
}
if (args is ["--audio-bank-transition", var audioDirectory])
{
    VerifyAudioBankTransition(audioDirectory);
    return 0;
}
if (args is ["--wall-spread-trace", var wallTrace])
{
    VerifyAerialSpreadTransitions(outputPath: wallTrace, wallRoute: true);
    return 0;
}
if (args is ["--wall-spread-transition", var wallNative])
{
    VerifyAerialSpreadTransitions(tracePath: wallNative, wallRoute: true);
    return 0;
}
if (args is ["--wall-spread-transition"])
{
    VerifyAerialSpreadTransitions(wallRoute: true);
    return 0;
}
if (args is ["--aerial-spread-trace", var aerialTrace])
{
    VerifyAerialSpreadTransitions(outputPath: aerialTrace);
    return 0;
}
if (args is ["--aerial-spread-transition"])
{
    VerifyAerialSpreadTransitions();
    return 0;
}
if (args is ["--aerial-spread-transition", var nativeAerialTrace])
{
    VerifyAerialSpreadTransitions(nativeAerialTrace);
    return 0;
}
if (args is ["--grounded-spread-transition"])
{
    VerifyGroundedSpreadTransition();
    return 0;
}
if (args is ["--grounded-spread-transition", var transitionTrace])
{
    VerifyGroundedSpreadTransition(transitionTrace);
    return 0;
}
if (args is ["--grounded-bomb-spread-native", var spreadTrace])
{
    VerifyGroundedBombSpreadNative(spreadTrace);
    return 0;
}
if (args is ["--grounded-bomb-spread"])
{
    VerifyGroundedBombSpread();
    return 0;
}
if (args is ["--map-installation", var installationRom])
{
    VerifyMapInstallation(installationRom);
    return 0;
}
if (args is ["--map-presentation"])
{
    VerifyMapPresentation();
    return 0;
}
if (args is ["--escape-timer-pointer-definitions", var timerRom])
{
    VerifyEscapeTimerPointerDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath(timerRom)));
    return 0;
}
if (args is ["--cartridge-room-state-selection"])
{
    VerifyCartridgeRoomStateSelection();
    return 0;
}
if (args is ["--room-tileset-definitions"])
{
    VerifyRoomAssetRomData();
    return 0;
}
if (args is ["--room-character-atlases"])
{
    VerifyRoomCharacterAtlases();
    return 0;
}
if (args is ["--library-background-loader"])
{
    VerifyLibraryBackgroundLoader();
    return 0;
}
if (args is ["--library-background-artwork-boundary", var libraryArtworkRom])
{
    using var directory = new MapCatalogTestDirectory();
    var installation = GameAssetInstaller.Install(libraryArtworkRom, directory.Root);
    VerifyLibraryBackgroundLoader();
    VerifyLibraryBackgroundInstalledParity(libraryArtworkRom, installation);
    return 0;
}
if (args is ["--oam-source-routing"])
{
    VerifyOamSpritemapPacking();
    return 0;
}
if (args is ["--enemy-tile-artwork"])
{
    VerifyEnemyTileArtwork();
    VerifyEnemyMappedSourceRouting();
    VerifyRoomEnemyLoading();
    return 0;
}
if (args is ["--enemy-source-routing"])
{
    VerifyEnemyMappedSourceRouting();
    return 0;
}
if (args is ["--enemy-visual-selector-inventory"])
{
    InspectEnemyVisualSelectors();
    return 0;
}
if (args is ["--generate-enemy-visual-selectors"])
{
    InspectEnemyVisualSelectors(generateCatalog: true);
    return 0;
}
if (args is ["--verify-enemy-visual-selectors"])
{
    VerifyCompiledEnemyVisualSelectors();
    return 0;
}
if (args is ["--generate-space-pirate-collision"])
{
    GenerateSpacePirateCollisionDefinitions();
    return 0;
}
if (args is ["--generate-room-level-stream-corpus"])
{
    GenerateRoomLevelStreamCorpus();
    return 0;
}
if (args is ["--verify-space-pirate-collision"])
{
    VerifySpacePirateCollisionDefinitions();
    return 0;
}
if (args is ["--verify-ridley-collision"])
{
    VerifyRidleyCollisionDefinitions();
    return 0;
}
if (args is ["--verify-ceres-steam-collision"])
{
    VerifyCeresSteamCollisionDefinitions();
    return 0;
}
if (args is ["--verify-oum-collision"])
{
    VerifyMaridiaLargeSnailCollisionDefinitions();
    return 0;
}
if (args is ["--verify-crocomire-tongue-collision"])
{
    VerifyCrocomireTongueCollisionDefinitions();
    return 0;
}
if (args is ["--verify-crocomire-body-collision"])
{
    VerifyCrocomireBodyCollisionDefinitions();
    return 0;
}
if (args is ["--kraid-foot-collision"])
{
    VerifyKraidFootCollisionDefinitions();
    return 0;
}
if (args is ["--projectile-frame-bindings"])
{
    VerifyProjectileFrameBindings(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--intro-cinematic-artwork", var introBackgroundRom])
{
    VerifyIntroCinematicArtwork(introBackgroundRom);
    return 0;
}
if (args is ["--samus-body-artwork", var samusBodyRom])
{
    VerifyIntroCinematicArtwork(samusBodyRom, samusBodyOnly: true);
    return 0;
}
if (args is ["--generic-sprite-artwork", var genericSpriteRom])
{
    VerifyGenericSpriteArtwork(genericSpriteRom);
    return 0;
}
if (args is ["--enemy-sprite-artwork-boundary", var enemySpriteRom])
{
    VerifyEnemySpriteArtworkBoundary(enemySpriteRom);
    return 0;
}
if (args is ["--golden-torizo-rom-free", var goldenTorizoRom])
{
    VerifyFrontendRomFreeGoldenTorizo(goldenTorizoRom, frameCount: 500);
    return 0;
}
if (args is ["--golden-torizo-rom-free", var goldenTorizoExtendedRom,
        var goldenTorizoFrameText])
{
    if (!int.TryParse(goldenTorizoFrameText, out int goldenTorizoFrames) ||
        goldenTorizoFrames is < 1 or > 5000)
        throw new ArgumentOutOfRangeException(nameof(goldenTorizoFrameText),
            "Golden Torizo room comparison requires 1 through 5000 frames.");
    VerifyFrontendRomFreeGoldenTorizo(goldenTorizoExtendedRom,
        goldenTorizoFrames);
    return 0;
}
if (args is ["--golden-torizo-rom-free", var goldenTorizoInputRom,
        var goldenTorizoInputFrameText, var goldenTorizoHeldInputText])
{
    if (!int.TryParse(goldenTorizoInputFrameText, out int goldenTorizoInputFrames) ||
        goldenTorizoInputFrames is < 1 or > 5000)
        throw new ArgumentOutOfRangeException(nameof(goldenTorizoInputFrameText),
            "Golden Torizo room comparison requires 1 through 5000 frames.");
    if (!ushort.TryParse(goldenTorizoHeldInputText.TrimStart('$'),
            System.Globalization.NumberStyles.HexNumber,
            System.Globalization.CultureInfo.InvariantCulture,
            out ushort goldenTorizoHeldInput))
        throw new ArgumentOutOfRangeException(nameof(goldenTorizoHeldInputText),
            "Held SNES input must be a hexadecimal 16-bit word.");
    VerifyFrontendRomFreeGoldenTorizo(goldenTorizoInputRom,
        goldenTorizoInputFrames, goldenTorizoHeldInput);
    return 0;
}
if (args is ["--rom-free-room-census", var censusRom, var censusSnapshotDirectory])
{
    VerifyFrontendRomFreeRoomCensusFromSnapshots(censusRom, censusSnapshotDirectory);
    return 0;
}
if (args is ["--rom-free-direct-room", var directRoomRom,
        var directRoomPointerText, var directRoomFrameText])
{
    VerifyFrontendRomFreeDirectRoom(directRoomRom, directRoomPointerText,
        directRoomFrameText);
    return 0;
}
if (args is ["--rom-free-direct-room", var inputRoomRom,
        var inputRoomPointerText, var inputRoomFrameText, var heldInputText])
{
    VerifyFrontendRomFreeDirectRoom(inputRoomRom, inputRoomPointerText,
        inputRoomFrameText, heldInputText);
    return 0;
}
if (args is ["--gameplay-base-palettes", var gameplayPaletteRom])
{
    VerifyGameplayBasePalettes(gameplayPaletteRom);
    return 0;
}
if (args is ["--standard-object-artwork", var standardObjectRom])
{
    VerifyStandardObjectArtwork(standardObjectRom);
    return 0;
}
if (args is ["--room-character-installation", var roomCharacterRom])
{
    VerifyRoomArtworkInstallation(roomCharacterRom);
    return 0;
}
if (args is ["--room-artwork-installation", var roomArtworkRom])
{
    VerifyRoomArtworkInstallation(roomArtworkRom);
    return 0;
}
if (args is ["--room-static-palettes"])
{
    VerifyRoomStaticPaletteExtraction();
    return 0;
}
if (args is ["--room-metatiles"])
{
    VerifyRoomMetatileExtraction();
    return 0;
}
if (args is ["--library-background-inventory"])
{
    VerifyLibraryBackgroundSourceInventory();
    return 0;
}
if (args is ["--room-background-tilemaps"])
{
    VerifyRoomBackgroundTilemapExtraction();
    return 0;
}
if (args is ["--room-sky-tilemaps"])
{
    VerifyScrollingSkyState();
    VerifyRoomSkyTilemaps();
    return 0;
}
if (args is ["--room-state-payloads"])
{
    VerifyCompiledRoomStateDefinitions();
    return 0;
}
if (args is ["--enemy-definitions"])
{
    VerifyCompiledEnemyDefinitions();
    return 0;
}
if (args is ["--enemy-room-lists"])
{
    VerifyCompiledEnemyRoomLists();
    return 0;
}
if (args is ["--room-scroll-definitions"])
{
    VerifyCompiledRoomScrollDefinitions();
    return 0;
}
if (args is ["--room-callback-definitions"])
{
    VerifyCompiledRoomCallbackDefinitions();
    return 0;
}
if (args is ["--room-definition-integration"])
{
    VerifyCompiledRoomDefinitionIntegration();
    return 0;
}
if (args is ["--gameplay-message-titles", var messageTitleRom])
{
    VerifyGameplayMessageTitles(messageTitleRom);
    return 0;
}
if (args is ["--gameplay-message-panels", var messagePanelRom])
{
    VerifyGameplayMessagePanels(messagePanelRom);
    return 0;
}
if (args is ["--gameplay-message-notices", var messageNoticeRom])
{
    VerifyGameplayMessageNotices(messageNoticeRom);
    return 0;
}
if (args is ["--gameplay-message-definitions"])
{
    VerifyGameplayMessageDefinitions();
    return 0;
}
if (args is ["--escape-typewriter-presentation", var escapeTextRom])
{
    VerifyEscapeTypewriterPresentation(escapeTextRom);
    return 0;
}
if (args is ["--intro-narration-presentation", var narrationRom])
{
    VerifyIntroNarrationPresentation(narrationRom);
    return 0;
}
if (args is ["--ending-text-presentation", var endingTextRom])
{
    VerifyEndingTextPresentation(endingTextRom);
    return 0;
}
if (args is ["--credits-presentation", var creditsRom])
{
    VerifyCreditsPresentation(creditsRom);
    return 0;
}
if (args is ["--pause-reserve-hud"])
{
    VerifyPauseReserveHud();
    return 0;
}
if (args is ["--reserve-auto-frontend"])
{
    VerifyReserveAutoFrontend();
    return 0;
}
if (args is ["--health-warning"])
{
    VerifyHealthWarning();
    return 0;
}
if (args is ["--health-warning-native", var healthWarningTrace])
{
    VerifyHealthWarningNative(healthWarningTrace);
    return 0;
}
if (args is ["--reserve-native-trace", var reserveTrace])
{
    VerifyReserveNativeTrace(reserveTrace);
    return 0;
}
if (args is ["--reserve-mode", var reserveModeTrace])
{
    VerifyReserveMode(reserveModeTrace);
    return 0;
}
if (args is ["--cinematic-flash", var cinematicFlashTrace])
{
    VerifyCinematicCrystalFlash(cinematicFlashTrace);
    return 0;
}
if (args is ["--pause-reserve-manual"])
{
    VerifyPauseReserveManual();
    return 0;
}
if (args is ["--pause-reserve-arrow"])
{
    VerifyPauseReserveArrow();
    return 0;
}
if (args is ["--pause-reserve-native"])
{
    VerifyPauseReserveNativePixels();
    return 0;
}
if (args is ["--pause-reserve-tanks"])
{
    VerifyPauseReserveTanks();
    return 0;
}
if (args is ["--draygon-tilemap-production"])
{
    VerifyDraygonTilemapProduction();
    return 0;
}
if (args is ["--projectile-runtime-phase"])
{
    VerifyProjectileRuntimePhase();
    return 0;
}
if (args is ["--projectile-contact-phase"] or ["--projectile-contact-damage"])
{
    VerifyProjectileContactPhase(args[0] == "--projectile-contact-phase");
    return 0;
}
if (args is ["--enemy-contact-phase"])
{
    VerifyRipperEnemy(verifyDeferredContact: true);
    return 0;
}
if (args is ["--ripper-enemy"])
{
    VerifyRipperEnemy();
    return 0;
}
if (args is ["--ceres-door-boss"])
{
    VerifyCeresDoorBossBranch();
    return 0;
}
if (args is ["--x-plasma-timers"])
{
    VerifyRipperEnemy(verifyXrayTimers: true);
    return 0;
}
if (args is ["--native-x-plasma-timers", var timerTrace])
{
    VerifyRipperEnemy(verifyXrayTimers: true, nativeXrayTimerTrace: timerTrace);
    return 0;
}
if (args is ["--window-pixels"])
{
    VerifyWindowPixels();
    VerifyGameplaySnapshots();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--hardware-windows"])
{
    VerifyHardwareWindows();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--dma-source-routing"])
{
    VerifyVramWriteQueue();
    VerifyDmaSourceRouting();
    VerifyQueuedVramAssets();
    return 0;
}
if (args is ["--dma-artwork-boundary", var dmaArtworkRom])
{
    VerifyDmaArtworkBoundary(dmaArtworkRom);
    return 0;
}
if (args is ["--file-select-map-entry"])
{
    VerifyFileSelectMapEntry();
    return 0;
}
if (args is ["--intro-artwork-post-slices", var postSliceRom])
{
    VerifyIntroArtworkPostSlices(postSliceRom);
    return 0;
}
if (args is ["--power-bomb-fuse"])
{
    VerifyPowerBombFuse();
    VerifyPowerBombBoundary();
    VerifyPowerBombRuntimeRendererIntegration();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--beam-callback-tables"])
{
    VerifyBeamCallbackTables();
    return 0;
}
if (args is ["--beam-speed-rows"])
{
    VerifyBeamSpeedRows();
    VerifySamusPowerBeamProjectiles();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--chainsaw-firing"])
{
    VerifyChainsawFiring();
    return 0;
}
if (args is ["--spacetime-beam"])
{
    VerifySpacetimeBeam();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--murder-beam"])
{
    VerifyMurderBeam();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--invalid-beam-graphics"])
{
    VerifyInvalidBeamGraphics();
    VerifyProductionMagicNumberAudit();
    return 0;
}
if (args is ["--invalid-beam-selection"])
{
    VerifyPauseMenuEquipmentInteraction();
    VerifyInvalidBeamSelection();
    return 0;
}
if (args is ["--spin-entry-audio"])
{
    VerifySamusSpaceJumpAndScrewAttack();
    VerifySamusAtmosphericEffects();
    return 0;
}
if (args is ["--boost-floor-scroll"])
{
    VerifyBoostFloorScroll();
    return 0;
}
if (args is ["--xray-controls"])
{
    VerifyXrayControls();
    return 0;
}
if (args is ["--elevatube-scrolling"])
{
    VerifyElevatubeScrolling();
    return 0;
}
if (args is ["--maridia-puyo-pile"])
{
    VerifyMaridiaPuyoPile();
    return 0;
}
if (args is ["--moat-first-entry"])
{
    VerifyMoatFirstEntry();
    return 0;
}
if (args is ["--pillar-first-entry"])
{
    VerifyPillarFirstEntry();
    return 0;
}
if (args is ["--ridley-acid"])
{
    VerifyRidleyAcid();
    return 0;
}
if (args is ["--treadmill-visual"])
{
    VerifyTreadmillVisual();
    return 0;
}
if (args is ["--metroid-bomb-placement"])
{
    VerifyMetroidBombPlacement();
    return 0;
}
if (args is ["--fireflea-eye"])
{
    VerifyFirefleaEye();
    return 0;
}
if (args is ["--yard-trajectories"])
{
    VerifyYardTrajectories();
    return 0;
}
if (args is ["--statue-splash"])
{
    VerifyStatueSplash();
    return 0;
}
if (args is ["--projectile-quake"])
{
    VerifyProjectileQuake();
    return 0;
}
if (args is ["--pause-reserve-labels"])
{
    VerifyPauseReserveLabels();
    return 0;
}
if (args is ["--draygon-goop-drops"])
{
    VerifyDraygonGoopDrops();
    return 0;
}
if (args is ["--ending-takeoff-wrap"] or ["--ending-takeoff-math"])
{
    VerifyEndingTakeoffColorMath(args[0] == "--ending-takeoff-wrap");
    return 0;
}
if (args is ["--ending-planet-boundary"])
{
    VerifyEndingPlanetBoundary();
    return 0;
}
if (args is ["--ending-native-ppu"] or ["--ending-native-offset-check"])
{
    VerifyEndingNativePpu(args[0] == "--ending-native-offset-check");
    return 0;
}
if (args is ["--ending-native-finale"])
{
    for (int frame = 512; frame <= 800; frame += 16) VerifyEndingNativePpu(frame: frame);
    return 0;
}
if (args is ["--ending-native-burst"])
{
    for (int frame = 400; frame <= 464; frame += 16) VerifyEndingNativePpu(frame: frame);
    return 0;
}
if (args is ["--ending-reward-definitions"])
{
    VerifyEndingRewardGesture();
    return 0;
}
if (args is ["--intro-mother-brain-definitions"])
{
    VerifyIntroMotherBrainDefinitions();
    return 0;
}
if (args is ["--intro-rinka-definitions"])
{
    VerifyIntroRinkaDefinitions();
    return 0;
}
if (args is ["--intro-baby-actor-definitions"])
{
    VerifyIntroBabyActorDefinitions();
    return 0;
}
if (args is ["--intro-egg-effect-definitions"])
{
    VerifyIntroEggEffectDefinitions();
    return 0;
}
if (args is ["--ceres-explosion-definitions"])
{
    VerifyCeresExplosionDefinitions();
    return 0;
}
if (args is ["--ceres-flight-actor-definitions"])
{
    VerifyCeresFlightActorDefinitions();
    return 0;
}
if (args is ["--ceres-destruction-actor-definitions"])
{
    VerifyCeresDestructionActorDefinitions();
    return 0;
}
if (args.Contains("--ending-dma"))
{
    VerifyEndingDma();
    VerifyEndingRenderSnapshots();
    return 0;
}
if (args.Length == 2 && args[0] == "--mother-brain-health")
{
    VerifyMotherBrainHealthPalette(args[1]);
    return 0;
}
if (args.Length == 2 && args[0] == "--escape-animals")
{
    VerifyEscapeAnimalBlocks(args[1]);
    VerifyEscapeRoomEffects(args[1]);
    return 0;
}
if (args.Contains("--escape-timer"))
{
    VerifyEscapeTimerBcd();
    VerifyEscapeTimerStateMachine();
    VerifyEscapeTimerFloor();
    VerifyControllerInputRecording();
    return 0;
}
VerifyAndroidHostPolicies();
VerifyBoostFloorScroll();
VerifyXrayControls();
VerifyIntroPoseHistory();
VerifyCinematicTextGlow();
VerifyProjectileContactPhase(verifyPhase: true);
VerifyProjectileRuntimePhase();
VerifyMorphedSpikeRelease();
VerifySpikeShinesparkSuit();
VerifyReserveMode("csharp/test-fixtures/movement-release/reserve-mode-433.csv");
VerifyCinematicCrystalFlash("csharp/test-fixtures/movement-release/cinematic-flash-432.csv");
VerifyElevatubeScrolling();
VerifyIniEditing();
VerifyBackgroundSampler();
if (args is ["--ini-edit"]) return 0;
if (args is ["--ceres-ridley-room-entry"])
{
    VerifyCeresRidleyRoomEntry();
    return 0;
}
if (args is ["--mother-brain"])
{
    VerifyMotherBrainHandBeamBodyInstructionDefinitions();
    VerifyMotherBrainFallingTubeInstructionDefinitions();
    VerifyMotherBrainHeadInstructionProgramDefinitions();
    VerifyMotherBrainBeamWindow();
    VerifyMotherBrainDeathHandoff();
    VerifySamusDrainedController();
    VerifyMotherBrainRainbowBeamSamusMovement();
    VerifyMotherBrainRainbowBeamAttackSequence();
    VerifyEnemyProjectileInstructionMechanicsDefinitions();
    VerifyMotherBrainBombProjectiles();
    VerifyMotherBrainProjectileRendering();
    VerifyMiscDustProjectiles();
    VerifyMotherBrainEscapeDoorParticles();
    VerifyBabyMetroidCutsceneEntrance();
    return 0;
}
if (args is ["--mother-brain-transfer-sources"])
{
    VerifyMotherBrainSpriteTransferSources();
    return 0;
}
if (args is ["--grapple-movement"])
{
    VerifySamusGrappleRomData();
    VerifySamusGrappleSwingAndRelease();
    return 0;
}
if (args is ["--shinespark"])
{
    VerifySamusStoredShineAndShinespark();
    Console.WriteLine("PASS shinespark: native movement, energy cutoff, invincibility, and crash lifecycle.");
    return 0;
}
if (args is ["--file-select-sound"])
{
    VerifyFileSelectSound();
    return 0;
}
if (args is ["--enemy-contact-death"])
{
    VerifyContactDeathStopsEnemyDispatch();
    return 0;
}
if (args is ["--android-host"])
    return 0;
if (args is ["--audio-queues"])
{
    VerifyCartridgeAudioQueues();
    return 0;
}
if (args is ["--audio-instruments"])
{
    VerifyEditableAudioInstruments();
    return 0;
}
if (args is ["--audio-overrides"])
{
    VerifyPersistentAudioOverrides();
    return 0;
}
if (args is ["--audio-sfx-programs"])
{
    VerifyEditableSoundEffectPrograms();
    return 0;
}
if (args is ["--audio-music-programs"])
{
    VerifyEditableMusicPrograms();
    return 0;
}
if (args is ["--shutter-native-arc"])
{
    AuditMorphShutterApproaches(reproduceOnly: true, exportNativeArc: true);
    return 0;
}
if (args is ["--bomb-wall"])
{
    VerifyBombJumpWallContact();
    return 0;
}
if (args is ["--shutter-morph-repro"])
{
    AuditMorphShutterApproaches(reproduceOnly: true);
    return 0;
}
if (args is ["--shutter-morph-approaches"])
{
    AuditMorphShutterApproaches();
    return 0;
}
if (args is ["--shutter-repeat"])
{
    AuditRepeatedShutterBombs();
    return 0;
}
if (args is ["--boost-floor-audit"])
{
    AuditBoostFloor();
    return 0;
}
if (args is ["--ocean-sky"])
{
    VerifyOceanSky();
    return 0;
}
if (args is ["--shutter-riding"])
{
    VerifyShutterRiding();
    return 0;
}
if (args is ["--shutter-embedding"])
{
    VerifyShutterEmbedding();
    return 0;
}
if (args is ["--xray-input"])
{
    VerifyXrayInput();
    return 0;
}
if (args is ["--mutable-memory-boundary"])
{
    VerifySuperMetroidAddressSpace();
    return 0;
}
if (args is ["--xray-setup"])
{
    VerifyXraySetupBuffers();
    return 0;
}
if (args is ["--fireflea-fx"])
{
    VerifyFirefleaFx();
    return 0;
}
if (args is ["--xray-window-geometry"])
{
    VerifyXrayWindowGeometry();
    return 0;
}
if (args is ["--xray-reveal"])
{
    VerifyXrayRoomDisplayRules();
    VerifyXrayRevealTable();
    VerifyXrayExtensions();
    VerifyXrayTilemap();
    VerifyXrayOverlays();
    return 0;
}
if (args is ["--grapple-enemy-death"])
{
    VerifyGrappleEnemyDeath();
    return 0;
}
if (args is ["--samus-grapple"])
{
    VerifySamusGrappleSwingAndRelease();
    return 0;
}
if (args is ["--samus-xray"])
{
    VerifySamusXray();
    return 0;
}
if (args is ["--grapple-resident-trigger"])
{
    VerifyNoobTubePlm();
    VerifyPermanentCollectibles();
    return 0;
}
if (args is ["--lower-norfair-hand"])
{
    VerifyLowerNorfairHand();
    return 0;
}
if (args is ["--draygon-defeated-room"])
{
    VerifyDraygonDefeatedRoom();
    return 0;
}
if (args is ["--grapple-gates"])
{
    VerifyGrappleGreenGateVisibility();
    return 0;
}
if (args is ["--grapple-spin"])
{
    VerifyGrappleSpinInput();
    return 0;
}
if (args is ["--grapple-sounds"])
{
    VerifyGrappleSounds();
    return 0;
}
if (args is ["--grapple-doors"])
{
    VerifyGrappleBlueDoors();
    VerifyGrapplePoseRefire();
    return 0;
}
if (args is ["--phantoon-position"])
{
    VerifyPhantoonPosition();
    return 0;
}
if (args is ["--title-instruction-definitions"])
{
    VerifyTitleGradientTables(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc")));
    VerifyTitleSequenceRomData();
    return 0;
}
if (args.Length > 1 || (args.Length == 1 && args[0] != "--render-contract"))
    throw new ArgumentException("Usage: SuperMetroid.Verification [--render-contract | --title-instruction-definitions | --phantoon-position | --samus-grapple | --samus-xray | --grapple-doors | --grapple-sounds | --grapple-spin | --grapple-gates | --grapple-enemy-death | --shutter-riding | --shutter-embedding]");
Console.WriteLine("Verifying translated Super Metroid routines...");
VerifyPlmDrawClone();
VerifyGameConfigurationIni();
VerifyViewportTileRowParity();
VerifyPpuMemorySnapshotOwnership();
VerifyTitleRenderSnapshots();
VerifyPauseRenderSnapshots();
VerifyPauseBossMarkers();
VerifyFileMenuRenderSnapshots();
VerifyRenderFrameHandoff();
VerifyRenderPresentationGate();
VerifyRenderPacketCodec();
VerifyFrontendRenderCapture();
VerifyCinematicRenderSnapshots();
VerifyGameplaySnapshots();
VerifyWindowPixels();
VerifyColorWindowSnapshots();
VerifyMessageSnapshots();
VerifyGameplayMessageDefinitions();
VerifyEyeWindowSnapshots();
VerifyRoomFxSnapshots();
VerifyGameplayCaptureIntegration();
VerifyMode7GameplaySnapshots();
VerifyEndingRenderSnapshots();
VerifyFileMapSnapshots();
VerifyAttractCapture();

// This portable gate retains every snapshot, codec, publication, scene-capture and
// software parity check above. It deliberately excludes unrelated gameplay audits,
// and never loads the Windows desktop or a graphics backend.
if (args.Length == 1)
{
    Console.WriteLine($"Portable render contract passed on {RuntimeInformation.OSDescription}; {RuntimeInformation.FrameworkDescription}.");
    return 0;
}

VerifyRandomNumberGeneratorExhaustively();
SaveLoadRandomAudit.Run(Path.GetFullPath("Super Metroid.smc"));
        VerifySandAnimatedTiles();
        VerifyQuicksand();
        VerifyTreadmillPhysics();
        VerifyPhantoonPosition();
        VerifyPausePaletteSound();
VerifyKnownRandomSequence();
VerifyTimedHeldInputTimeline();
VerifyEventBitfield();
VerifyBossBitfield();
VerifyMultiplicationExhaustively();
VerifySmCompressionFormat();
VerifyVramWriteQueue();
VerifyEscapeTimerBcd();
VerifyEscapeTimerStateMachine();
VerifyEscapeTimerFloor();
VerifyControllerInputLatch();
VerifyGameOptionsRomDataCatalog();
VerifyControllerBindingsAndOptionsSubmenus();
VerifyGameOverRomData();
VerifyTitleSequenceRomData();
VerifyStrictFailureBoundaries();
VerifyReserveAutoRecovery();
VerifyHealthWarning();
VerifyReserveAutoFrontend();
VerifyPauseReserveManual();
VerifyPauseReserveArrow();
VerifyPauseReserveTanks();
VerifyPauseReserveHud();
VerifyDoorAlignmentParity();
VerifyDoorOpeningTrajectories();
VerifyCreditsObjectInterpreter();
VerifyEndingCreditsState();
VerifyGenericGamepadInput();
VerifyDemoInputObject();
VerifyAttractDemoScene();
VerifyAttractDemoControllerOverride();
VerifyGrappleDemoTrajectory();
VerifyFrameRuntime();
VerifyGameTimeState();
VerifySuperMetroidAddressSpace();
VerifyCartridgeAudioQueues();
VerifyManagedSnesDsp();
VerifyTypedNativeWords();
VerifyProductionMagicNumberAudit();
VerifyPhantoonWaveLifecycle();
VerifyBackgroundMosaicSampling();
VerifyOamSpritemapPacking();
VerifyCeresElevatorArrivalGraphicsIndex();
VerifySamusRenderingSlice();
VerifySamusMovementRomData();
VerifySamusRenderingRomData();
VerifySamusPaletteRomData();
VerifySamusSpecialSequenceRomData();
VerifySamusArmCannon();
VerifySamusProjectileRomData();
VerifySamusGrappleRomData();
VerifySamusXrayRomData();
VerifySamusHudSelection();
VerifySamusVisorPalette();
VerifySamusHurtFlashPalette();
VerifySamusPoseTransitionMatching();
VerifySamusHorizontalSpeed();
VerifySamusExtraDisplacement();
VerifySamusStoredShineAndShinespark();
VerifySamusCrystalFlash();
VerifyCrystalFlashRuntime();
VerifySamusXray();
VerifySamusDeathSequence();
VerifyMotherBrainBeamWindow();
VerifyMotherBrainDeathHandoff();
VerifySamusDrainedController();
VerifySamusGrabbedByDraygon();
VerifyMotherBrainRainbowBeamSamusMovement();
VerifyMotherBrainHandBeamBodyInstructionDefinitions();
VerifyMotherBrainFallingTubeInstructionDefinitions();
VerifyMotherBrainHeadInstructionProgramDefinitions();
VerifyMotherBrainRainbowBeamAttackSequence();
VerifyEnemyProjectileInstructionMechanicsDefinitions();
VerifyMotherBrainBombProjectiles();
VerifyMotherBrainProjectileRendering();
VerifyMiscDustProjectiles();
VerifyMotherBrainEscapeDoorParticles();
VerifyBabyMetroidCutsceneEntrance();
VerifySamusSolidEnemyCollision();
VerifySamusAerialMovement();
VerifyZeroDistanceJumpContact();
VerifyShinesparkEnemyStop();
VerifyKnockbackHorizontalStop();
VerifyRetailFallingSpeedRecurrence();
VerifyCrampedAerialLandingPoseCollision();
VerifySamusSpaceJumpAndScrewAttack();
VerifySamusLiquidPhysics();
VerifySamusAtmosphericEffects();
VerifySamusAerialTurnsAndWallJump();
VerifySamusPoseHistory();
VerifyWallJumpDust();
VerifyCeresHazeLifecycle();
VerifyCeresRidleyWallImpact();
VerifySamusKnockbackAndDamageBoost();
VerifySamusGrappleSwingAndRelease();
VerifyGrappleBlueDoors();
VerifyGrappleSounds();
VerifyGrapplePoseRefire();
VerifyGrappleSpinInput();
VerifyGrappleGreenGateVisibility();
VerifyGrappleEnemyDeath();
VerifyShutterRiding();
VerifyXrayInput();
VerifyXrayWindowGeometry();
VerifyXraySetupBuffers();
VerifyFirefleaFx();
VerifyBombJumpWallContact();
VerifyXrayRoomDisplayRules();
VerifyXrayRevealTable();
VerifyXrayExtensions();
VerifyXrayTilemap();
VerifyXrayOverlays();
VerifyBreakableGrapplePlms();
VerifyBombBlockPrograms();
VerifyContactCrumblePrograms();
VerifyStationAnimationProgramDefinitions();
VerifyPermanentCollectibles();
VerifyCollectibleVisuals();
VerifyEnemyDrops();
VerifySamusPostureMovement();
VerifySamusPowerBeamProjectiles();
ProbeProjectileVelocityInheritance();
VerifyProjectileCooldowns();
VerifyWrapShotTrace("csharp/test-fixtures/movement-release/wrap-shot-409.csv");
VerifyRetailWrapShotDoors();
VerifyWrapShotEnemySeparation();
VerifyWrapShotWidths("csharp/test-fixtures/movement-release/wrap-width-409.csv");
VerifyCeilingWrapPlmTrace("csharp/test-fixtures/movement-release/ceiling-plm-410.csv");
VerifyKronicGateBeamCollision();
VerifyGateJumpTraces();
VerifyRightFacingGateGlitches();
VerifyGreenHillGrappleSpeedGateGlitch();
VerifyGModeGateGlitch();
VerifyFrozenEnemyGateGlitch();
VerifyMochtroidBotwoonPipeClip();
VerifySparkCrashAlignment();
VerifyEnemyAngleDivision();
VerifyDraygonEyeEffects();
VerifyDraygonTilemapProduction();
VerifyFrogSpeedwayPoolCollision();
VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv");
VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv", 9);
VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-run-410.csv", 11);
VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-walk-410.csv", 11, false);
VerifyHeroShotCameraLifetime("csharp/test-fixtures/movement-release/hero-shot-411.csv");
VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv");
VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv");
VerifyControlledRedTowerHeroShot();
VerifyBeamSpeedRows();
VerifyBeamCallbackTables();
VerifySamusMorphBallMovement();
VerifyShotBlockPlmPrograms();
VerifyCompactWalkOffCollision();
VerifyPauseMomentumReconciliation();
VerifyBombChargeRejection();
VerifyGroundedBombSpread();
VerifyGroundedSpreadTransition();
VerifyAerialSpreadTransitions();
VerifyAerialSpreadTransitions(wallRoute: true);
VerifySamusStandingAimMovement();
VerifySamusAimedAerialMovement();
VerifySamusGunExtendedMovement();
VerifySamusSlopePhysics();
VerifySamusBlockCollision();
VerifySpeedBoosterCollisionBlocks();
VerifyMaridiaElevatubePlm();
VerifyCompiledTourianAccessPlmPrograms();
VerifyTourianAccessVisuals();
VerifySpeedBoosterVisuals();
VerifyMaridiaElevatubeVisuals();
VerifyCompiledSporeSpawnCeilingPlms();
VerifyCompiledBotwoonWallPlms();
VerifyBotwoonWallVisuals();
VerifyCompiledKraidRoomPlms();
VerifyCompiledCrocomireArenaPlms();
VerifyCompiledMotherBrainFakeDeathPlms();
VerifyMotherBrainFakeDeathVisuals();
VerifyCrocomireArenaVisuals();
VerifyKraidRoomVisuals();
VerifySporeSpawnCeilingVisuals();
VerifySamusEaterVisuals();
VerifySamusGroundedMovement();
VerifySamusGroundedReversal();
VerifySamusMoonwalking();
VerifySamusRanIntoWall();
VerifyObjRendering();
VerifyHudStateAndBg3Rendering();
VerifyDebugRoomCamera();
VerifyRoomScrollGridAndBoundaryCamera();
VerifyRoomScrollPlms();
VerifyRoomPlmHeaderCatalog();
VerifyRoomPlmInstructionListCatalog();
VerifySequentialRoomPlmPopulationLoader();
VerifyMotherBrainEscapeGateCompiledDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyEscapeGateVisuals(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyCompiledRoomPlmPopulationDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyNoobTubePlm();
VerifyDownwardGatePlms();
VerifyEyeDoorPlms();
VerifyDraygonCannonPlms();
VerifyBombTorizoHandPlm();
VerifyPauseMenuEquipmentInteraction();
VerifyInvalidBeamSelection();
VerifyInvalidBeamGraphics();
VerifyMovedSamusCameraTracking();
VerifyBackgroundScrollState();
VerifyLevelBlockTilemapExpansion();
VerifyRoomLevelData();
VerifyCartridgeRoomStateSelection();
VerifyCompiledRoomHeaderDefinitions();
VerifyCompiledRoomStateDefinitions();
VerifyCompiledRoomStateSelectionDefinitions();
VerifyCompiledLoadStationDefinitions();
VerifyCompiledDoorDefinitions();
VerifyCompiledEnemyDefinitions();
VerifyCompiledEnemyRoomLists();
VerifyCompiledRoomScrollDefinitions();
VerifyCompiledRoomCallbackDefinitions();
VerifyCompiledRoomDefinitionIntegration();
VerifyRoomMainCodeCatalog();
VerifyRoomSetupCodeCatalog();
VerifyRoomAssetRomData();
VerifyAreaMapAssets();
VerifyMapPresentation();
VerifyBackgroundTilemapStreamer();
VerifyFourBitBackgroundRendering();
VerifyLoRomCrossBankCompressedData();
VerifyMode7Rendering();
VerifyLayerCompositorBackdrop();
VerifyBgPriorityPlaneRendering();
VerifyLibraryBackgroundLoader();
VerifyControllerInputRecording();
VerifySaveRamLayout();
VerifyExploredMapPackingDefinitions();
VerifyGameSaveJsonPersistence();
VerifyFileSelectFreshSaveTilemap();
VerifyFileSelectMapWindow();
VerifySavedGameLoadAppearance();
VerifyIntroCinematicRomData();
VerifyIntroCinematicArtwork(Path.GetFullPath("Super Metroid.smc"));
VerifyIntroGameplayFlashbackVerticalScroll();
VerifyIntroMotherBrainDefinitions();
VerifyIntroRinkaDefinitions();
VerifyIntroBabyActorDefinitions();
VerifyIntroEggEffectDefinitions();
VerifyCeresExplosionDefinitions();
VerifyCeresFlightActorDefinitions();
VerifyCeresDestructionActorDefinitions();
VerifyCinematicPaletteFader();
VerifyHostRoomViewportAlignment();
VerifyPowerBombColorMathWindow();
VerifyHardwareWindows();
VerifyChainsawFiring();
VerifySpacetimeBeam();
VerifyMurderBeam();
VerifyPowerBombRuntimeRendererIntegration();
VerifyPowerBombFuse();
VerifyPowerBombBoundary();
VerifyRoomFxRomData();
VerifyPowerBombFixedColors();
VerifySamusVisorColors();
VerifySamusHurtColors();
VerifySamusHyperBeamColors();
VerifySpcSoundLibrary2Pointers();
VerifyScrollingSkyState();
VerifyOceanSky();
AuditBoostFloor();
VerifyEnemyAiCodePointerCatalog();
VerifyEnemyInstructionCodePointerCatalogs();
VerifyEnemyRomTablePointerCatalog();
VerifyMotherBrainContactHitboxes();
VerifyMamaTurtleShellContourDefinitions();
VerifyMamaTurtleEnemyDefinitions();
VerifyPaletteFxInstructionCodeCatalogs();
VerifyAnimatedTileInstructionCodeCatalog();
VerifyEnemyProjectileCodePointerCatalog();
VerifyEnemyMappedSourceRouting();
VerifyRoomEnemyLoading();
VerifyEnemyTileArtwork();
VerifyCompiledEnemyVisualSelectors();
VerifySpacePirateCollisionDefinitions();
VerifyRidleyCollisionDefinitions();
VerifyCeresSteamCollisionDefinitions();
VerifyMaridiaLargeSnailCollisionDefinitions();
VerifyCrocomireTongueCollisionDefinitions();
VerifyCrocomireBodyCollisionDefinitions();
VerifyBotwoonPlmIdentity();
VerifyCompiledEnemyTrigonometry();
VerifyRidleyExplosionDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyCrocomireMeltingDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyDraygonIntroDanceDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyEnemyProjectileCollisionLifecycle();
VerifyRipperEnemy();
VerifyRipperEnemy(verifyXrayTimers: true);
VerifyPostCeresGunshipLanding();
VerifyCeresElevatorPlatformAnimation();
VerifyCeresDoorBossBranch();
VerifyCeresRidleyRoomEntry();
VerifyCeresEscapeVramTransferDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
VerifyCeresEscapeHandoff();
VerifyCeresDestructionCinematic();

Console.WriteLine("All bank $80 verification checks passed.");
return 0;
}
catch (Exception exception)
{
    // This is deliberately handled here, at the process boundary. Assertions still stop the
    // verifier immediately, but Windows never receives an unhandled CLR exception that it can
    // turn into a focus-stealing dialog. ToString() retains the type, message, inner exception,
    // and complete stack trace in the terminal where the failure is actually actionable.
    Console.Error.WriteLine(exception);
    return 1;
}

}

}
