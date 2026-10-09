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
// to step through in Visual Studio. A failed check throws immediately with concrete state;
// its outermost suite records it and the run continues with the next suite.
// Windows otherwise turns an unhandled CLR assertion into a modal "unknown software
// exception" dialog. That is actively hostile to an automated verifier: the useful stack
// trace belongs in this console and a dialog must never steal focus or stall the process.
if (OperatingSystem.IsWindows())
    NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);

try
{
    int exit = RunFlags(args);
    if (failedSuites.Count == 0)
        return exit;
    Console.Error.WriteLine($"{failedSuites.Count} suite(s) failed: {string.Join(", ", failedSuites)}");
    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}
}

private static int RunFlags(string[] args)
{
var flagWatch = System.Diagnostics.Stopwatch.StartNew();
try
{
if (args is ["--suite", var suiteName])
    return RunNamedSuite(suiteName);
if (args is ["--lookup-stream-4-hud-icons"])
{
    var rom=CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),"HUD icon source revision");
    Suite(nameof(VerifyLookupStream4HudIcons), () => VerifyLookupStream4HudIcons(rom));
    return 0;
}
if (args is ["--lookup-stream-4-hud-template"])
{
    var rom=CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),"HUD template source revision");
    Suite(nameof(VerifyLookupStream4HudTemplate), () => VerifyLookupStream4HudTemplate(rom));
    return 0;
}
if (args is ["--lookup-stream-4-hud-top-row"])
{
    var rom=CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),"HUD top-row source revision");
    Suite(nameof(VerifyLookupStream4HudTopRow), () => VerifyLookupStream4HudTopRow(rom));
    return 0;
}
if (args is ["--lookup-stream-4-hud-auto"])
{
    var rom=CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),"AUTO source revision");
    Suite(nameof(VerifyLookupStream4HudAutoComplete), () => VerifyLookupStream4HudAutoComplete(rom));
    return 0;
}
if (args is ["--lookup-stream-4-draygon-layout"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Draygon layout oracle revision");
    Suite(nameof(VerifyLookupStream4DraygonPresentationLayout), () => VerifyLookupStream4DraygonPresentationLayout(rom));
    return 0;
}
if (args is ["--lookup-stream-4-draygon-intro"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Draygon intro oracle revision");
    Suite(nameof(VerifyLookupStream4DraygonIntro), () => VerifyLookupStream4DraygonIntro(rom));
    return 0;
}
if (args is ["--lookup-stream-4-draygon-sprite"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Draygon sprite oracle revision");
    Suite(nameof(VerifyLookupStream4DraygonSprite), () => VerifyLookupStream4DraygonSprite(rom));
    return 0;
}
if (args is ["--lookup-stream-4-draygon-background"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Draygon background oracle revision");
    Suite(nameof(VerifyLookupStream4DraygonBackground), () => VerifyLookupStream4DraygonBackground(rom));
    Suite(nameof(VerifyLookupStream4DraygonHealth), () => VerifyLookupStream4DraygonHealth(rom));
    return 0;
}
if (args is ["--lookup-stream-4-draygon-health"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Draygon health oracle revision");
    Suite(nameof(VerifyLookupStream4DraygonHealth), () => VerifyLookupStream4DraygonHealth(rom));
    return 0;
}
if (args is ["--lookup-stream-4-norfair-reveal"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Norfair reveal oracle revision");
    Suite(nameof(VerifyLookupStream4NorfairReveal), () => VerifyLookupStream4NorfairReveal(rom));
    return 0;
}
if (args is ["--lookup-stream5-ceres-door-materials"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres door material oracle revision");
    Suite(nameof(VerifyLookupStream5CeresNormalPaint), () => VerifyLookupStream5CeresNormalPaint(rom));
    Suite(nameof(VerifyLookupStream5CeresEscapePaint), () => VerifyLookupStream5CeresEscapePaint(rom));
    return 0;
}
if (args is ["--lookup-stream5-ceres-normal-paint"])
{
    Suite(nameof(VerifyLookupStream5CeresNormalPaint), () => VerifyLookupStream5CeresNormalPaint(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-ceres-escape-paint"])
{
    Suite(nameof(VerifyLookupStream5CeresEscapePaint), () => VerifyLookupStream5CeresEscapePaint(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-ceres-beacon-paint"])
{
    Suite(nameof(VerifyLookupStream5CeresBeaconPaint), () => VerifyLookupStream5CeresBeaconPaint(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-ceres-rumble"])
{
    Suite(nameof(VerifyLookupStream5CeresRumble), () => VerifyLookupStream5CeresRumble(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream3-exploded-door-paints"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Exploded door palette oracle revision");
    Suite(nameof(VerifyStream3MotherBrainFades), () => VerifyStream3MotherBrainFades(rom));
    Console.WriteLine("Exploded door:14 native colors,42 independent channel edits,no stock array,hashes,bounds and instance isolation pass; other death fades preserved.");
    return 0;
}
if (args is ["--lookup-stream3-auxiliary-complete"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Auxiliary palette oracle revision");
    Suite(nameof(VerifyStream3AuxiliaryPalettes), () => VerifyStream3AuxiliaryPalettes(rom));
    Suite(nameof(VerifyStream3ShitroidPulse), () => VerifyStream3ShitroidPulse(rom));
    Console.WriteLine("Auxiliary palettes:393 native colors,1179 independent RGB edits,zero stock endpoint/corpse arrays,identities,bounds and standalone target isolation pass.");
    return 0;
}
if (args is ["--lookup-stream5-ceres-platform"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres platform oracle revision");
    Suite(nameof(VerifyLookupStream5CeresPlatform), () => VerifyLookupStream5CeresPlatform(rom));
    return 0;
}
if (args is ["--lookup-stream5-ceres-overlay-complete"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres overlay native revision");
    Suite(nameof(VerifyLookupStream5CeresSourcePages), () => VerifyLookupStream5CeresSourcePages());
    Suite(nameof(VerifyLookupStream5CeresOverlayWords), () => VerifyLookupStream5CeresOverlayWords(rom));
    Suite(nameof(VerifyCeresEscapeVramTransferDefinitions), () => VerifyCeresEscapeVramTransferDefinitions(rom));
    return 0;
}
if (args is ["--lookup-stream-1-projectile-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Projectile program oracle revision");
    Suite(nameof(VerifyLookupStream1ProjectilePrograms), () => VerifyLookupStream1ProjectilePrograms(rom));
    return 0;
}
if (args is ["--lookup-stream-1-projectile-radii"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Projectile radius oracle revision");
    Suite(nameof(VerifyLookupStream1ProjectileRadiusLayout), () => VerifyLookupStream1ProjectileRadiusLayout(rom));
    return 0;
}
if (args is ["--lookup-stream-1-world-foreground"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "World foreground oracle revision");
    Suite(nameof(VerifyLookupStream1WorldForeground), () => VerifyLookupStream1WorldForeground(rom));
    return 0;
}
if (args is ["--lookup-stream-1-world-background"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "World BG3 oracle revision");
    Suite(nameof(VerifyLookupStream1WorldBackground), () => VerifyLookupStream1WorldBackground(rom));

    return 0;
}
if (args is ["--lookup-stream-1-death-pixels"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Death pixel oracle revision");
    Suite(nameof(VerifyLookupStream1DeathPixels), () => VerifyLookupStream1DeathPixels(rom));

    return 0;
}
if (args is ["--lookup-stream-4-foreground-cadence"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Foreground cadence oracle revision");
    Suite(nameof(VerifyLookupStream4ForegroundCadence), () => VerifyLookupStream4ForegroundCadence(rom));
    return 0;
}
if (args is ["--lookup-stream-4-title-ambient-complete"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Title ambient oracle revision");
    Suite(nameof(VerifyLookupStream4TitleAmbientMechanics), () => VerifyLookupStream4TitleAmbientMechanics(rom));
    Suite(nameof(VerifyLookupStream4TitleAmbientComplete), () => VerifyLookupStream4TitleAmbientComplete(rom));
    return 0;
}
if (args is ["--lookup-stream3-baby-normal-target"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby normal/target oracle revision");
    Suite(nameof(VerifyStream3ShitroidPulse), () => VerifyStream3ShitroidPulse(rom));
    Suite(nameof(VerifyStream3BabyInitialPaints), () => VerifyStream3BabyInitialPaints(rom));
    Console.WriteLine("Baby normal/target: native outputs, independent pulse/target/initial edits, bidirectional isolation, identities and bounds pass.");
    return 0;
}
if (args is ["--lookup-stream-1-drained-geometry"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Drained geometry oracle revision");
    Suite(nameof(VerifyLookupStream1DrainedGeometry), () => VerifyLookupStream1DrainedGeometry(rom));
    return 0;
}
if (args is ["--lookup-stream3-baby-initial-fade"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby palette oracle revision");
    Suite(nameof(VerifyStream3BabyInitialPaints), () => VerifyStream3BabyInitialPaints(rom));
    Suite(nameof(VerifyStream3BabyFade), () => VerifyStream3BabyFade(rom));
    return 0;
}
if (args is ["--lookup-stream-1-posture-geometry"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Posture geometry oracle revision");
    Suite(nameof(VerifyLookupStream1PostureGeometry), () => VerifyLookupStream1PostureGeometry(rom));
    return 0;
}
if (args is ["--lookup-stream-1-landing-placement"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Landing placement oracle revision");
    Suite(nameof(VerifyLookupStream1BodyFacingOffsets), () => VerifyLookupStream1BodyFacingOffsets(rom));
    return 0;
}
if (args is ["--mother-brain-room-palette-mechanics"])
{
    Suite(nameof(VerifyMotherBrainRoomPaletteProgramDefinitions), () => VerifyMotherBrainRoomPaletteProgramDefinitions());
    return 0;
}
if (args is ["--lookup-stream3-room-flash"])
{
    Suite(nameof(VerifyStream3RoomFlash), () => VerifyStream3RoomFlash(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Console.WriteLine("Room flash:336native colors/mirrors,1008independent edits,zero stock arrays/paint scalars pass.");
    return 0;
}
if (args is ["--lookup-stream3-room-entry-palettes"])
{
    Suite(nameof(VerifyStream3RoomEntryPalettes), () => VerifyStream3RoomEntryPalettes(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Console.WriteLine("Glass/tube:30native colors,90edits,room/recovery independence and legacy inheritance pass.");
    return 0;
}
if (args is ["--lookup-stream3-recovery-lights"])
{
    Suite(nameof(VerifyStream3RecoveryLights), () => VerifyStream3RecoveryLights(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Console.WriteLine("Recovery lights:196native colors,588channel edits,72room edits,routing and legacy inheritance pass.");
    return 0;
}
if (args is ["--lookup-stream3-final-room-paints"])
{
    var native = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyStream3FinalRoomPaints), () => VerifyStream3FinalRoomPaints(native));
    Console.WriteLine("Final room:24native colors,72independent channel edits and dependent-palette isolation pass.");
    return 0;
}
if (args is ["--lookup-stream2-golden-torizo-strides"])
{
    Suite(nameof(VerifyLookupStream2GoldenTorizoFootGeometry), () => VerifyLookupStream2GoldenTorizoFootGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream-1-escape-dachora-cadence"])
{
    Suite(nameof(VerifyLookupStream1EscapeDachoraCadence), () => VerifyLookupStream1EscapeDachoraCadence(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream-1-powamp-cadence"] )
{
    Suite(nameof(VerifyLookupStream1PowampCadence), () => VerifyLookupStream1PowampCadence(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream3-attack-palette"])
{
    Suite(nameof(VerifyStream3MotherBrainAttackPalette), () => VerifyStream3MotherBrainAttackPalette(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Console.WriteLine("Mother Brain attack palette:15 native colors,45 edits,recovery independence and paired rear routing pass.");
    return 0;
}
if (args is ["--lookup-stream2-chozo-strides"])
{
    Suite(nameof(VerifyLookupStream2ChozoFootGeometry), () => VerifyLookupStream2ChozoFootGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream-1-metroid-pulse"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Metroid pulse oracle revision");
    Suite(nameof(VerifyLookupStream1MetroidPulse), () => VerifyLookupStream1MetroidPulse(rom));
    return 0;
}

if (args is ["--lookup-stream-1-atmospheric-cadence"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Atmospheric cadence oracle revision");
    Suite(nameof(VerifyLookupStream1AtmosphericCadence), () => VerifyLookupStream1AtmosphericCadence(rom));
    Console.WriteLine("Atmospheric cadence:37 native holds, seven domains,74 actual expiry/delayed-start cases and rejection contracts pass.");
    return 0;
}

if (args is ["--lookup-stream3-rainbow-materials"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Rainbow palette oracle revision");
    Suite(nameof(VerifyStream3DrainFades), () => VerifyStream3DrainFades(oracle));
    Console.WriteLine("Rainbow material palettes: native colors, routing, independent edits and reduced material basis pass.");
    return 0;
}
if (args is ["--lookup-stream2-tourian-accent-cadence"])
{
    Suite(nameof(VerifyLookupStream2TourianAccentCadence), () => VerifyLookupStream2TourianAccentCadence(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-ghost-palette"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ghost palette oracle revision");
    Suite(nameof(VerifyLookupStream2GhostPalette), () => VerifyLookupStream2GhostPalette(rom));
    return 0;
}
if (args is ["--lookup-stream-1-timer-cadence"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Timer cadence oracle revision");
    Suite(nameof(VerifyLookupStream1TimerCadence), () => VerifyLookupStream1TimerCadence(rom));
    return 0;
}
if (args is ["--lookup-stream-1-timer-glyphs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Timer glyph oracle revision");
    Suite(nameof(VerifyLookupStream1TimerGlyphs), () => VerifyLookupStream1TimerGlyphs(rom));
    return 0;
}
if (args is ["--lookup-stream2-ninja-program-layout"])
{
    Suite(nameof(VerifyLookupStream2NinjaProgramLayout), () => VerifyLookupStream2NinjaProgramLayout());
    return 0;
}
if (args is ["--lookup-stream5-slope-speeds"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Slope speed oracle revision");
    Suite(nameof(VerifyCompiledSlopeSpeeds), () => VerifyCompiledSlopeSpeeds(rom));
    AssertThrows<ArgumentOutOfRangeException>(() => SlopeSpeedDefinitions.HorizontalMultiplier(-1), "Slope family lower bound");
    AssertThrows<ArgumentOutOfRangeException>(() => SlopeSpeedDefinitions.HorizontalMultiplier(32), "Slope family upper bound");
    return 0;
}
if (args is ["--lookup-stream5-crocomire-paint"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire palette oracle revision");
    Suite(nameof(VerifyLookupStream5CrocomireSharedPaint), () => VerifyLookupStream5CrocomireSharedPaint(rom));
    return 0;
}
if (args is ["--lookup-stream3-falling-tube-population-layout"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Falling tube oracle revision");
    Suite(nameof(VerifyStream3FallingTubePopulationLayout), () => VerifyStream3FallingTubePopulationLayout(rom));
    return 0;
}
if (args is ["--lookup-stream3-menu-sprites"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyStream3MenuSpriteGeometry), () => VerifyStream3MenuSpriteGeometry(oracle));
    Console.WriteLine("Menu sprites: 15 native parts, 135 independent field edits, reordered/expanded compositions and shared cursor loaders pass.");
    return 0;
}
if (args is ["--lookup-stream3-file-select-sprites"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyStream3FileSelectSprites), () => VerifyStream3FileSelectSprites(oracle));
    return 0;
}
if (args is ["--lookup-stream3-options-sprites"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyStream3OptionsBorders), () => VerifyStream3OptionsBorders(oracle, SuperMetroid.AssetExtraction.GameOptionsPresentationExtractor.Extract(oracle)));
    Console.WriteLine("Options headings: 144 native parts, 576 independent edits, reordered/expanded compositions and actual OAM draw pass.");
    return 0;
}
if (args is ["--lookup-stream3-intro-font"]){ VerifyStream3IntroFont(); return 0; }
if (args is ["--lookup-stream3-background-maps"]){ VerifyStream3BackgroundMaps(); return 0; }
if (args is ["--lookup-stream3-work-robot-registry"]) { VerifyStream3WorkRobotRegistry(); return 0; }
if (args is ["--lookup-stream3-shaktool-registry"]) { VerifyStream3ShaktoolRegistry(); return 0; }
if (args is ["--lookup-stream3-nintendo-fade-entries"]) { VerifyStream3NintendoFadeEntries(); return 0; }
if (args is ["--lookup-stream2-yard-groups"]) { VerifyLookupStream2YardGroups(); return 0; }
if (args is ["--lookup-stream3-portrait-map"])
{
    Suite(nameof(VerifyStream3PortraitMap), () => VerifyStream3PortraitMap());
    return 0;
}
if (args is ["--lookup-stream3-initial-narration"])
{
    Suite(nameof(VerifyStream3InitialNarrationMap), () => VerifyStream3InitialNarrationMap());
    return 0;
}
if (args is ["--lookup-stream3-intro-palette-rows"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro palette oracle revision");
    Suite(nameof(VerifyStream3IntroPaletteRows), () => VerifyStream3IntroPaletteRows(rom));
    return 0;
}
if (args is ["--lookup-stream3-intro-layouts"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro layout oracle revision");
    Suite(nameof(VerifyStream3IntroEyeRectangles), () => VerifyStream3IntroEyeRectangles(rom));
    Suite(nameof(VerifyStream3IntroDivider), () => VerifyStream3IntroDivider(rom));
    return 0;
}
if (args is ["--lookup-stream5-ceres-source-pages"])
{
    var ceresPageOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ceresPageOracle.Rom)), "Ceres source-page oracle revision");
    Suite(nameof(VerifyLookupStream5CeresSourcePages), () => VerifyLookupStream5CeresSourcePages());
    Suite(nameof(VerifyCeresEscapeVramTransferDefinitions), () => VerifyCeresEscapeVramTransferDefinitions(ceresPageOracle));
    return 0;
}
if (args is ["--lookup-stream-1-body-frame-basis"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Body frame oracle revision");
    Suite(nameof(VerifyLookupStream1XrayBodyFrames), () => VerifyLookupStream1XrayBodyFrames(rom, true));
    return 0;
}
if (args is ["--lookup-stream-1-body-frame-final"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Body frame oracle revision");
    Suite(nameof(VerifyLookupStream1XrayBodyFrames), () => VerifyLookupStream1XrayBodyFrames(rom, false, true));
    return 0;
}
if (args is ["--lookup-stream-1-body-facing"] or ["--lookup-stream-1-xray-frames"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Body oracle revision");
    Suite(nameof(VerifyLookupStream1BodyFacingOffsets), () => VerifyLookupStream1BodyFacingOffsets(rom));
    Suite(nameof(VerifyLookupStream1BodyGraphicsOrigins), () => VerifyLookupStream1BodyGraphicsOrigins(rom));
    Suite(nameof(VerifyLookupStream1BodySetPointers), () => VerifyLookupStream1BodySetPointers(rom));
    Suite(nameof(VerifyLookupStream1BodyPosePointers), () => VerifyLookupStream1BodyPosePointers(rom));
    Suite(nameof(VerifyLookupStream1XrayBodyFrames), () => VerifyLookupStream1XrayBodyFrames(rom));
    return 0;
}
if (args is ["--morph-ball-pickup-collision"])
{
    Suite(nameof(VerifyMorphBallPickupCollision), () => VerifyMorphBallPickupCollision());
    return 0;
}
if (args is ["--save-load-rng-fixture"]) { return SaveLoadRandomAudit.Run(Path.GetFullPath("Super Metroid.smc"), RepositoryInstallation.BindGame); }
if (args is ["--frontend-fixtures"]) { VerifyIntroPoseHistory(); VerifyFrontendRenderCapture(); VerifyGameplayCaptureIntegration(); VerifyAttractCapture(); return 0; }
if (args is ["--menu-fixtures"]) { VerifyFileMenuRenderSnapshots(); VerifyFileMapSnapshots(); VerifyControllerBindingsAndOptionsSubmenus(); VerifyMessageSnapshots(); return 0; }
if (args is ["--cinematic-fixtures"]) { VerifyCinematicRenderSnapshots(); VerifyEndingRenderSnapshots(); VerifyEndingCreditsState(); return 0; }
if (args is ["--projectile-fixtures"]) {
Suite(nameof(VerifyWrapShotTrace), () => VerifyWrapShotTrace("csharp/test-fixtures/movement-release/wrap-shot-409.csv"));
Suite(nameof(VerifyWrapShotEnemySeparation), () => VerifyWrapShotEnemySeparation());
Suite(nameof(VerifyWrapShotWidths), () => VerifyWrapShotWidths("csharp/test-fixtures/movement-release/wrap-width-409.csv"));
Suite(nameof(VerifyKronicGateBeamCollision), () => VerifyKronicGateBeamCollision());
Suite(nameof(VerifyRightFacingGateGlitches), () => VerifyRightFacingGateGlitches());
Suite(nameof(VerifyGModeGateGlitch), () => VerifyGModeGateGlitch());
Suite(nameof(VerifyFrogSpeedwayPoolCollision), () => VerifyFrogSpeedwayPoolCollision());
Suite(nameof(VerifyHeroShotCameraLifetime), () => VerifyHeroShotCameraLifetime("csharp/test-fixtures/movement-release/hero-shot-411.csv"));
Suite(nameof(VerifyMissileImpactCameraEdge), () => VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv"));
Suite(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows());
Suite(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables());
Suite(nameof(VerifySamusMorphBallMovement), () => VerifySamusMorphBallMovement());
Suite(nameof(VerifyBombChargeRejection), () => VerifyBombChargeRejection());
Suite(nameof(VerifyGroundedBombSpread), () => VerifyGroundedBombSpread());
return 0; }
if (args is ["--file-map-fixtures"]) { VerifyFileSelectMapWindow(); return 0; }
if (args is ["--map-icons-fixture"]) { VerifyFileSelectMapIcons(); return 0; }
if (args is ["--map-animation-fixture"]) { VerifyFileSelectMapAnimations(); return 0; }
if (args is ["--saved-map-fixture"]) { VerifySavedGameMapFrontend(); return 0; }
if (args is ["--map-cancel-fixture"]) { VerifyMapCancelPresentation(); return 0; }
if (args is ["--station-marker-fixture"]) { VerifyFileSelectStationMarker(); return 0; }
if (args is ["--inventory-fixture"]) { VerifyInvalidBeamSelection(); return 0; }
if (args is ["--space-screw-fixture"]) { VerifySamusSpaceJumpAndScrewAttack(); return 0; }
if (args is ["--arm-cannon-fixture"]) { VerifySamusArmCannon(); return 0; }
if (args is ["--game-over-fixture"]) { VerifyGameOverRomData(); return 0; }
if (args is ["--enemy-pointer-fixture"]) { VerifyEnemyRomTablePointerCatalog(); return 0; }
if (args is ["--draygon-prebattle-xray"]) { VerifyDraygonPrebattleXray(); return 0; }
if (args is ["--kihunter-spit-audio"]) { VerifyKiHunterSpitAudio(); return 0; }
if (args is ["--samus-liquid-physics"]) { VerifySamusLiquidPhysics(); return 0; }
if (args is ["--shallow-water-jump"]) { VerifyShallowWaterJump(); return 0; }
if (args is ["--yapping-maw-grapple-release"]) { VerifyYappingMawGrappleRelease(); return 0; }
if (args is ["--ridley-acceleration-carry"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyCompiledRidleyInertia), () => VerifyCompiledRidleyInertia(rom));
    Suite(nameof(VerifyRidleyDeathAcceleration), () => VerifyRidleyDeathAcceleration(rom));
    return 0;
}
if (args is ["--ridley-fireball-damage"]) { VerifyRidleyFireballDamage(); return 0; }
if (args is ["--ridley-spin-fireball"]) { VerifyRidleySpinFireball(); return 0; }
if (args is ["--ridley-contact-ordering"]) { VerifyRidleyContactOrdering(); return 0; }
if (args is ["--ridley-swoop-timer"]) { VerifyRidleySwoopTimer(); return 0; }
if (args is ["--ridley-death-finish"]) { VerifyRidleyDeathFinish(); return 0; }
if (args is ["--ridley-grab-entry"]) { VerifyRidleyGrabEntry(); return 0; }
if (args is ["--ridley-map-initialization"]) { VerifyRidleyMapInitialization(); return 0; }
if (args is ["--samus-retained-horizontal-speed"]) { VerifyRetainedHorizontalSpeed(); return 0; }
if (args is ["--pause-dispatch-rng"]) { VerifyPauseDispatcherRandom(); return 0; }
if (args is ["--spin-fallback-history"]) { VerifySpinFallbackHistory(); return 0; }
if (args is ["--morph-camera-checkpoint"]) { VerifyMorphCameraCheckpoint(); return 0; }
if (args is ["--aim-up-landing-animation"]) { VerifyAimUpLandingAnimation(); return 0; }
if (args is ["--ridley-fireball-square-slope"]) { VerifyRidleyFireballSquareSlope(); return 0; }
if (args is ["--ridley-tail-impact"]) { VerifyRidleyTailImpact(); return 0; }
if (args is ["--ridley-screen-gate"]) { VerifyRidleyScreenGate(); return 0; }
if (args is ["--spring-ball-release"]) { VerifySpringBallRelease(); return 0; }
if (args is ["--ridley-tail-offsets"]) { VerifyRidleyTailOffsets(); return 0; }
if (args is ["--ridley-center-facing"]) { VerifyRidleyCenterFacing(); return 0; }
if (args is ["--ridley-pause-page-timing"]) { VerifyRidleyPausePageTiming(); return 0; }
if (args is ["--ridley-palette-selection"]) { VerifyRidleyPaletteSelection(); return 0; }
if (args is ["--ridley-door-entry"]) { VerifyRidleyDoorEntry(); return 0; }
if (args is ["--ridley-full-movie"]) { VerifyRidleyFullMovie(Path.GetFullPath("native-captures/issue-1266-ridley")); return 0; }
if (args is ["--lsmv-playthrough-movie", var lowTraceDirectory]) { VerifyLowPercentPlaythroughMovie(lowTraceDirectory); return 0; }
if (args is ["--lsmv-playthrough-movie", var lowTracedDirectory, "--trace-from", var lowTraceFrom]) { VerifyLowPercentPlaythroughMovie(lowTracedDirectory, int.Parse(lowTraceFrom)); return 0; }
if (args is ["--full-playthrough-movie", var fullTraceDirectory]) { VerifyFullPlaythroughMovie(fullTraceDirectory); return 0; }
if (args is ["--full-playthrough-movie", var tracedDirectory, "--trace-from", var traceFrom]) { VerifyFullPlaythroughMovie(tracedDirectory, int.Parse(traceFrom)); return 0; }
if (args is ["--ridley-player-opening"]) { VerifyRidleyPlayerOpening(); return 0; }
if (args is ["--door-autosave"]) { VerifyDoorTransitionAutosave(); return 0; }
if (args is ["--door-music-timing"]) { VerifyDoorMusicTiming(); return 0; }
if (args is ["--collectible-message-timing"]) { VerifyCollectibleMessageTiming(); return 0; }
if (args is ["--permanent-collectibles-fixture"]) { VerifyPermanentCollectibles(); return 0; }
if (args is ["--room-plm-population"]) { VerifySequentialRoomPlmPopulationLoader(); return 0; }
if (args is ["--enemy-art-fixtures"]) { VerifyGrappleGreenGateVisibility(); VerifyGrappleEnemyDeath(); VerifyDraygonTilemapProduction(); return 0; }
if (args is ["--xray-overlay-fixtures"]) { VerifyXrayOverlays(); VerifyXraySetupBuffers(); return 0; }
if (args is ["--grapple-sound-refire-fixtures"]) { VerifyGrappleSounds(); VerifyGrapplePoseRefire(); return 0; }
if (args is ["--attract-input-fixtures"]) { VerifyAttractDemoScene(); VerifyGrappleDemoTrajectory(); return 0; }
if (args is ["--mother-brain-beam-fixture"]) { VerifyMotherBrainBeamWindow(); return 0; }
if (args is ["--death-sequence-fixture"]) { VerifySamusDeathSequence(); return 0; }
if (args is ["--special-palette-fixtures"]) { VerifySamusCrystalFlash(); VerifySamusXray(); return 0; }
if (args is ["--samus-palette-fixtures"]) { VerifySamusVisorPalette(); VerifySamusHurtFlashPalette(); VerifySamusVisorColors(); return 0; }
if (args is ["--power-bomb-fixtures"]) { VerifyPowerBombFuse(); VerifyPowerBombColorMathWindow(); return 0; }
if (args is ["--horizontal-speed-fixture"]) { VerifySamusHorizontalSpeed(); return 0; }
if (args is ["--demo-input-fixture"]) { VerifyDemoInputObject(); return 0; }
if (args is ["--cacatac-fixture"]) { VerifyCacatacProjectileInstructionProgramDefinitions(); return 0; }
if (args is ["--color-window-fixture"]) { VerifyColorWindowSnapshots(); return 0; }
if (args is ["--title-fixtures"]) { VerifyTitleRenderSnapshots(); VerifyTitleSequenceRomData(); return 0; }
if (args is ["--mutable-memory-boundary"]) { VerifySuperMetroidAddressSpace(); return 0; }
if (args is ["--pause-fixtures"])
{
    Suite(nameof(VerifyPauseRenderSnapshots), () => VerifyPauseRenderSnapshots());
    Suite(nameof(VerifyPauseBossMarkers), () => VerifyPauseBossMarkers());
    Suite(nameof(VerifyPausePaletteSound), () => VerifyPausePaletteSound());
    Suite(nameof(VerifyPauseReserveManual), () => VerifyPauseReserveManual());
    Suite(nameof(VerifyPauseReserveArrow), () => VerifyPauseReserveArrow());
    Suite(nameof(VerifyPauseReserveArrowRebindKeepsLatch), () => VerifyPauseReserveArrowRebindKeepsLatch());
    Suite(nameof(VerifyPauseReserveTanks), () => VerifyPauseReserveTanks());
    Suite(nameof(VerifyPauseReserveHud), () => VerifyPauseReserveHud());
    return 0;
}
if (args is ["--chozo-power-bomb"]) { VerifyChozoGrabAnimation(powerBomb: true); return 0; }
if (args is ["--golden-torizo-code-entry"]) { VerifyGoldenTorizoCodeEntry(); return 0; }
if (args is ["--mockball-boost-contact"]) { VerifyMockballBoostContact(); return 0; }
if (args is ["--game-save-json"])
{
    Suite(nameof(VerifyGameSaveJsonPersistence), () => VerifyGameSaveJsonPersistence());
    Suite(nameof(VerifySaveSchemaTranslation), () => VerifySaveSchemaTranslation());
    return 0;
}
if (args is ["--torizo-palette-shake"]) return VerifyTorizoPaletteShake();
if (args is ["--gunship-escape-timer"]) return VerifyGunshipEscapeTimer();
if (args is ["--baby-metroid-theme"]) return VerifyBabyMetroidTheme();
if (args is ["--baby-metroid-death-cry"]) return VerifyBabyMetroidDeathCry();
if (args is ["--mother-brain-plasma-impact"]) return VerifyMotherBrainPlasmaImpact();
if (args is ["--mother-brain-later-contact"]) return VerifyMotherBrainLaterContact();
if (args is ["--mother-brain-ascent-capture"]) return VerifyMotherBrainTankBackground(ascentMaskOnly: true);
if (args is ["--mother-brain-tube-descent"]) return VerifyMotherBrainTubeDescent();
if (args is ["--mother-brain-tank-background"]) return VerifyMotherBrainTankBackground();
if (args is ["--dead-torizo-collision"]) return VerifyDeadTorizoCollision();
if (args is ["--frozen-metroid-shell"]) return VerifyFrozenMetroidShell();
if (args is ["--tourian-statue-water"]) return VerifyTourianStatueWater();
if (args is ["--crocomire-spike-animation"]) return VerifyCrocomirePresentation(true);
if (args is ["--crocomire-comeback-rumble"]) return VerifyCrocomirePresentation(false);
if (args is ["--kraid-reported-collisions"]) return VerifyKraidSuperMissileDamage(collisionReport: true);
if (args is ["--kraid-super-missile-damage"])
{
    return VerifyKraidSuperMissileDamage();
}
if (args is ["--phantoon-intro-flame-sound"])
{
    Suite(nameof(VerifyPhantoonIntroFlameSound), () => VerifyPhantoonIntroFlameSound());
    return 0;
}
if (args is ["--phantoon-death-wave-initialization"])
{
    Suite(nameof(VerifyPhantoonDeathWaveInitialization), () => VerifyPhantoonDeathWaveInitialization());
    return 0;
}
if (args is ["--speed-booster-echo-stop"])
{
    Suite(nameof(VerifySpeedBoosterEchoStop), () => VerifySpeedBoosterEchoStop());
    return 0;
}
if (args is ["--grapple-hurt-feedback"])
{
    Suite(nameof(VerifyGrappleHurtFeedback), () => VerifyGrappleHurtFeedback());
    return 0;
}
if (args is ["--pause-door-shinespark"])
{
    Suite(nameof(VerifyPauseDoorShinespark), () => VerifyPauseDoorShinespark());
    return 0;
}
if (args is ["--overlapping-enemy-shots"])
{
    Suite(nameof(VerifyOverlappingEnemyShots), () => VerifyOverlappingEnemyShots());
    return 0;
}
if (args is ["--gameplay-grapple-palette"])
{
    Suite(nameof(VerifyGameplayGrapplePalette), () => VerifyGameplayGrapplePalette());
    return 0;
}
if (args is ["--xray-no-fx-darkening"])
{
    Suite(nameof(VerifyXrayNoFxDarkening), () => VerifyXrayNoFxDarkening());
    return 0;
}
if (args is ["--chozo-grab-animation"])
{
    Suite(nameof(VerifyChozoGrabAnimation), () => VerifyChozoGrabAnimation());
    return 0;
}
if (args is ["--etecoon-door-fanfare"])
{
    Suite(nameof(VerifyEtecoonFanfareAfterDoor), () => VerifyEtecoonFanfareAfterDoor());
    return 0;
}
if (args is ["--waterfall-rooms"])
{
    Suite(nameof(VerifyWaterfallRooms), () => VerifyWaterfallRooms());
    return 0;
}
if (args is ["--gunship-entry-sound"])
{
    Suite(nameof(VerifyPostCeresGunshipLanding), () => VerifyPostCeresGunshipLanding(entrySoundOnly: true));
    return 0;
}
if (args is ["--speed-booster-pickup"])
{
    Suite(nameof(VerifySpeedBoosterPickupContinuation), () => VerifySpeedBoosterPickupContinuation());
    return 0;
}
if (args is ["--escape-running-footsteps"])
{
    Suite(nameof(VerifyEscapeRunningFootsteps), () => VerifyEscapeRunningFootsteps());
    return 0;
}
if (args is ["--tourian-elevator-doors"])
{
    Suite(nameof(VerifyTourianElevatorDoors), () => VerifyTourianElevatorDoors());
    return 0;
}
if (args is ["--phantoon-flame-sound"])
{
    Suite(nameof(VerifyPhantoonFlameSound), () => VerifyPhantoonFlameSound());
    return 0;
}
if (args is ["--evir-death-frame"])
{
    Suite(nameof(VerifyEvirDeathFrame), () => VerifyEvirDeathFrame());
    return 0;
}
if (args is ["--recharge-station-admission"])
{
    Suite(nameof(VerifyRechargeStationAdmission), () => VerifyRechargeStationAdmission());
    return 0;
}
if (args is ["--ridley-missed-lunge"])
{
    Suite(nameof(VerifyRidleyMissedLunge), () => VerifyRidleyMissedLunge());
    return 0;
}
if (args is ["--crocomire-cutscene-camera"])
{
    Suite(nameof(VerifyCrocomireCutsceneCamera), () => VerifyCrocomireCutsceneCamera());
    return 0;
}
if (args is ["--crocomire-corpse-collision"])
{
    Suite(nameof(VerifyCrocomireCorpseCollision), () => VerifyCrocomireCorpseCollision());
    return 0;
}
if (args is ["--ridley-flight-tail"])
{
    Suite(nameof(VerifyRidleyFlightTail), () => VerifyRidleyFlightTail());
    return 0;
}
if (args is ["--ridley-breakup-programs"])
{
    Suite(nameof(VerifyRidleyBreakupPrograms), () => VerifyRidleyBreakupPrograms());
    return 0;
}
if (args is ["--attract-runtime-bindings"])
{
    Suite(nameof(VerifyAttractRuntimeBindings), () => VerifyAttractRuntimeBindings());
    return 0;
}
if (args is ["--golden-torizo-code"])
{
    Suite(nameof(VerifyGoldenTorizoCode), () => VerifyGoldenTorizoCode());
    return 0;
}
if (args is ["--kraid-growth-command-cursor"])
{
    Suite(nameof(VerifyKraidGrowthCommandCursor), () => VerifyKraidGrowthCommandCursor());
    return 0;
}
if (args is ["--power-bomb-death-drawing"])
{
    Suite(nameof(VerifyPowerBombDeathDrawing), () => VerifyPowerBombDeathDrawing());
    return 0;
}
if (args is ["--power-bomb-death-radius"])
{
    VerifyPowerBombDeathRadius();
    return 0;
}
if (args is ["--respawned-enemy-contact"])
{
    VerifyRespawnedEnemyContact();
    return 0;
}
if (args is ["--vertical-offscreen-deletion"])
{
    VerifyVerticalOffScreenDeletion();
    return 0;
}
if (args is ["--botwoon-position-history"])
{
    VerifyBotwoonPositionHistory();
    return 0;
}
if (args is ["--draygon-turret-cadence"])
{
    VerifyDraygonTurretCadence();
    return 0;
}
if (args is ["--wall-jump-spin-exit"])
{
    VerifyWallJumpSpinExit();
    return 0;
}
if (args is ["--evir-init-timer"])
{
    VerifyEvirInitTimer();
    return 0;
}
if (args is ["--super-missile-enemy-hit-quake"])
{
    Suite(nameof(VerifySuperMissileEnemyHitQuake), () => VerifySuperMissileEnemyHitQuake());
    return 0;
}
if (args is ["--released-samus-falls-off-draygon"])
{
    Suite(nameof(VerifyReleasedSamusFallsOffDraygon), () => VerifyReleasedSamusFallsOffDraygon());
    return 0;
}
if (args is ["--draygon-escape-drag"])
{
    Suite(nameof(VerifyDraygonEscapeDrag), () => VerifyDraygonEscapeDrag());
    return 0;
}
if (args is ["--draygon-scrolling-speed-cap"])
{
    Suite(nameof(VerifyDraygonScrollingSpeedCap), () => VerifyDraygonScrollingSpeedCap());
    return 0;
}
if (args is ["--item-cancel-clears-charge"])
{
    Suite(nameof(VerifyItemCancelClearsCharge), () => VerifyItemCancelClearsCharge());
    return 0;
}
if (args is ["--frozen-time-enemy-projectiles"])
{
    Suite(nameof(VerifyFrozenTimeEnemyProjectiles), () => VerifyFrozenTimeEnemyProjectiles());
    return 0;
}
if (args is ["--unpause-reserve-blackout"])
{
    Suite(nameof(VerifyUnpauseReserveBlackout), () => VerifyUnpauseReserveBlackout());
    return 0;
}
if (args is ["--powamp-death-sequence"])
{
    Suite(nameof(VerifyPowampDeathSequence), () => VerifyPowampDeathSequence());
    return 0;
}
if (args is ["--ridley-shot-health-stage"])
{
    Suite(nameof(VerifyRidleyShotHealthStage), () => VerifyRidleyShotHealthStage());
    return 0;
}
if (args is ["--wall-jump-dust"])
{
    Suite(nameof(VerifyWallJumpDust), () => VerifyWallJumpDust());
    return 0;
}
if (args is ["--unpause-elevator-flags"])
{
    Suite(nameof(VerifyUnpauseElevatorFlags), () => VerifyUnpauseElevatorFlags());
    return 0;
}
if (args is ["--knockback-shinespark-launch"])
{
    Suite(nameof(VerifyKnockbackShinesparkLaunch), () => VerifyKnockbackShinesparkLaunch());
    return 0;
}
if (args is ["--door-sounds-during-power-bomb"])
{
    Suite(nameof(VerifyDoorSoundsDuringPowerBomb), () => VerifyDoorSoundsDuringPowerBomb());
    return 0;
}
if (args is ["--super-missile-death-animation"])
{
    Suite(nameof(VerifySuperMissileDeathAnimation), () => VerifySuperMissileDeathAnimation());
    return 0;
}
if (args is ["--implicit-scroll-residue"])
{
    Suite(nameof(VerifyImplicitScrollResidue), () => VerifyImplicitScrollResidue());
    return 0;
}
if (args is ["--elevator-stand-up-contact"])
{
    Suite(nameof(VerifyElevatorStandUpContact), () => VerifyElevatorStandUpContact());
    return 0;
}
if (args is ["--empty-extended-frame-shots"])
{
    Suite(nameof(VerifyEmptyExtendedFrameShots), () => VerifyEmptyExtendedFrameShots());
    return 0;
}
if (args is ["--kraid-arm-samus-contact"])
{
    Suite(nameof(VerifyKraidArmSamusContact), () => VerifyKraidArmSamusContact());
    return 0;
}
if (args is ["--air-spike-alpha-radius"])
{
    Suite(nameof(VerifyAirSpikeAlphaRadius), () => VerifyAirSpikeAlphaRadius());
    return 0;
}
if (args is ["--square-slope-beam-collision"])
{
    Suite(nameof(VerifySquareSlopeBeamCollision), () => VerifySquareSlopeBeamCollision());
    return 0;
}
if (args is ["--beam-impact-sound"])
{
    Suite(nameof(VerifyBeamImpactSound), () => VerifyBeamImpactSound());
    return 0;
}
if (args is ["--low-percent-intro-timeline"])
{
    Suite(nameof(VerifyLowPercentIntroTimeline), () => VerifyLowPercentIntroTimeline());
    return 0;
}
if (args is ["--ending-setup-nmi-waits"])
{
    VerifyEndingSetupNmiWaits();
    return 0;
}
if (args is ["--zebes-escape-fade"])
{
    VerifyZebesEscapeFade();
    return 0;
}
if (args is ["--crateria-mainstreet-escape-passage"])
{
    VerifyCrateriaMainstreetEscapePassage();
    return 0;
}
if (args is ["--old-tourian-escape-shaft-wall"])
{
    VerifyOldTourianEscapeShaftWall();
    return 0;
}
if (args is ["--mother-brain-inherited-explosion-index"])
{
    VerifyMotherBrainInheritedExplosionIndex();
    return 0;
}
if (args is ["--mother-brain-body-hitboxes"])
{
    VerifyMotherBrainBodyHitboxes();
    return 0;
}
if (args is ["--mother-brain-missile-walk-reset"])
{
    VerifyMotherBrainMissileWalkReset();
    return 0;
}
if (args is ["--baby-metroid-fatal-blow-shake"])
{
    VerifyBabyMetroidFatalBlowShake();
    return 0;
}
if (args is ["--baby-metroid-wrong-way-speed"])
{
    VerifyBabyMetroidWrongWaySpeed();
    return 0;
}
if (args is ["--mother-brain-ring-baby-health"])
{
    VerifyMotherBrainRingBabyHealth();
    return 0;
}
if (args is ["--mother-brain-walk-backwards-pose"])
{
    VerifyMotherBrainWalkBackwardsPose();
    return 0;
}
if (args is ["--baby-metroid-head-target"])
{
    VerifyBabyMetroidHeadTarget();
    return 0;
}
if (args is ["--baby-metroid-inherited-fractions"])
{
    VerifyBabyMetroidInheritedFractions();
    return 0;
}
if (args is ["--rainbow-release-knockback"])
{
    VerifyRainbowReleaseKnockback();
    return 0;
}
if (args is ["--mother-brain-small-purple-breath"])
{
    VerifyMotherBrainSmallPurpleBreath();
    return 0;
}
if (args is ["--mother-brain-head-hitbox"])
{
    VerifyMotherBrainHeadHitbox();
    return 0;
}
if (args is ["--mother-brain-raise-counter"])
{
    VerifyMotherBrainRaiseCounter();
    return 0;
}
if (args is ["--mother-brain-tube-hdma-deletion"])
{
    VerifyMotherBrainTubeHdmaDeletion();
    return 0;
}
if (args is ["--mother-brain-tube-timing"])
{
    VerifyMotherBrainTubeTiming();
    return 0;
}
if (args is ["--samus-solid-enemy-collision"])
{
    VerifySamusSolidEnemyCollision();
    Console.WriteLine("Samus solid-enemy collision: native target rounding, eligibility and edge clipping passed.");
    return 0;
}
if (args is ["--mother-brain-glass-super-missile"])
{
    VerifyMotherBrainGlassSuperMissile();
    return 0;
}
if (args is ["--mother-brain-rinka-door-spawn"])
{
    VerifyMotherBrainRinkaDoorSpawn();
    return 0;
}
if (args is ["--shitroid-drain-carry"])
{
    VerifyShitroidDrainCarry();
    return 0;
}
if (args is ["--shitroid-gradual-acceleration"])
{
    VerifyShitroidGradualAcceleration();
    return 0;
}
if (args is ["--metroid-death-drops"])
{
    VerifyMetroidDeathDrops();
    return 0;
}
if (args is ["--tourian-statue-descent-rounding"])
{
    VerifyTourianStatueDescentRounding();
    return 0;
}
if (args is ["--door-animated-tiles"])
{
    VerifyDoorAnimatedTiles();
    return 0;
}
if (args is ["--tourian-statue-xray-freeze"])
{
    VerifyTourianStatueXrayFreeze();
    return 0;
}
if (args is ["--save-confirmation-cadence"])
{
    VerifySaveConfirmationCadence();
    return 0;
}
if (args is ["--jump-no-x-movement"])
{
    VerifyJumpNoXMovement();
    return 0;
}
if (args is ["--shutter-screw-contact"])
{
    VerifyShutterScrewContact();
    return 0;
}
if (args is ["--fireflea-double-death"])
{
    VerifyFirefleaDoubleDeath();
    return 0;
}
if (args is ["--spring-ball-falling-fallback"])
{
    VerifySpringBallFallingFallback();
    return 0;
}
if (args is ["--door-entry-enemy-sound"])
{
    VerifyDoorEntryEnemySound();
    return 0;
}
if (args is ["--gold-ninja-death-drops"])
{
    VerifyGoldNinjaDeathDrops();
    return 0;
}
if (args is ["--door-entry-room-fx-sound"])
{
    VerifyDoorEntryRoomFxSound();
    return 0;
}
if (args is ["--main-game-loop-carry"])
{
    VerifyMainGameLoopCarry();
    return 0;
}
if (args is ["--spring-ball-bounce"])
{
    VerifySpringBallBounce();
    return 0;
}
if (args is ["--puromi-arc-position"])
{
    VerifyPuromiArcPosition();
    return 0;
}
if (args is ["--wall-probe-bomb-block"])
{
    VerifyWallProbeBombBlock();
    return 0;
}
if (args is ["--golden-torizo-attack-choice"])
{
    VerifyGoldenTorizoAttackChoice();
    return 0;
}
if (args is ["--magdollite-apex-threshold"])
{
    VerifyMagdolliteApexThreshold();
    return 0;
}
if (args is ["--refill-station-lock"])
{
    VerifyRefillStationLock();
    return 0;
}
if (args is ["--zoa-speeds"])
{
    VerifyCompiledZoaSpeeds(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--single-frame-enemy-visuals"])
{
    Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions());
    Suite(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions());
    Suite(nameof(VerifyPolypRockInstructionProgramDefinitions), () => VerifyPolypRockInstructionProgramDefinitions());
    Suite(nameof(VerifySingleFrameEnemyVisuals), () => VerifySingleFrameEnemyVisuals());
    return 0;
}
if (args is ["--lookup-stream-1-body-oam-bases"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "OAM base oracle revision");
    Suite(nameof(VerifyLookupStream1BodyOamBases), () => VerifyLookupStream1BodyOamBases(rom));
    return 0;
}
if (args is ["--lookup-stream-1-hurt-blend"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hurt palette oracle revision");
    Suite(nameof(VerifyLookupStream1HurtBlend), () => VerifyLookupStream1HurtBlend(rom));
    Suite(nameof(VerifySamusHurtColors), () => VerifySamusHurtColors());
    return 0;
}
if (args is ["--lookup-stream3-hand-beam-body-layout"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hand-beam body oracle revision");
    Suite(nameof(VerifyStream3HandBeamBodyLayout), () => VerifyStream3HandBeamBodyLayout());
    return 0;
}
if (args is ["--lookup-stream5-map-landmarks"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map landmark oracle revision");
    Suite(nameof(VerifyLookupStream5MapLandmarkCases), () => VerifyLookupStream5MapLandmarkCases(rom));
    return 0;
}
if (args is ["--lookup-stream3-baby-instruction-layout"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby instruction oracle revision");
    Suite(nameof(VerifyStream3BabyInstructionLayout), () => VerifyStream3BabyInstructionLayout(rom));
    Suite(nameof(VerifyMotherBrainBabyInstructionProgramDefinitions), () => VerifyMotherBrainBabyInstructionProgramDefinitions(rom));
    Console.WriteLine("Baby command layout: twelve native words, nine visual addresses and rejection domains pass; six hold roles remain required.");
    return 0;
}
if (args is ["--lookup-stream3-baby-body-sharing"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby OAM oracle revision");
    Suite(nameof(VerifyStream3BabySpriteReflection), () => VerifyStream3BabySpriteReflection(rom));
    Console.WriteLine("Baby upper-body sharing: native OAM/order/hash, independent symmetric edits and legacy schemas pass; geometry/artwork remain required.");
    return 0;
}
if (args is ["--lookup-stream3-hand-beam-layout"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hand-beam oracle revision");
    Suite(nameof(VerifyStream3HandBeamLayout), () => VerifyStream3HandBeamLayout());
    Suite(nameof(VerifyEnemyProjectileInstructionMechanicsDefinitions), () => VerifyEnemyProjectileInstructionMechanicsDefinitions());
    Console.WriteLine("Hand-beam command layout: stage/callback/visual order, exact byte ownership and production checks pass; selected holds remain required.");
    return 0;
}
if (args is ["--lookup-stream5-phantoon-timers"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Phantoon timer oracle revision");
    Suite(nameof(VerifyCompiledPhantoonTimers), () => VerifyCompiledPhantoonTimers(rom));
    Suite(nameof(VerifyLookupStream5PhantoonExposure), () => VerifyLookupStream5PhantoonExposure(rom));
    Suite(nameof(VerifyLookupStream5PhantoonClosedEye), () => VerifyLookupStream5PhantoonClosedEye(rom));
    Suite(nameof(VerifyLookupStream5PhantoonRainWait), () => VerifyLookupStream5PhantoonRainWait(rom));
    return 0;
}
if (args is ["--lookup-stream-1-power-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Projectile program oracle revision");
    Suite(nameof(VerifyLookupStream1PowerProgramLayout), () => VerifyLookupStream1PowerProgramLayout(rom));
    Suite(nameof(VerifyLookupStream1WaveIceProgramLayout), () => VerifyLookupStream1WaveIceProgramLayout(rom));
    Suite(nameof(VerifyLookupStream1SpazerProgramLayout), () => VerifyLookupStream1SpazerProgramLayout(rom));
    Suite(nameof(VerifyLookupStream1PlasmaProgramLayout), () => VerifyLookupStream1PlasmaProgramLayout(rom));
    Suite(nameof(VerifyLookupStream1ChargedProgramLayout), () => VerifyLookupStream1ChargedProgramLayout(rom));
    Suite(nameof(VerifyLookupStream1NonBeamProgramLayout), () => VerifyLookupStream1NonBeamProgramLayout(rom));
    return 0;
}
if (args is ["--lookup-stream5-phantoon-collision"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Phantoon collision oracle revision");
    Suite(nameof(VerifyLookupStream5PhantoonCollision), () => VerifyLookupStream5PhantoonCollision(rom));
    return 0;
}
if (args is ["--lookup-stream5-phantoon-fade"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Phantoon colors oracle revision");
    Suite(nameof(VerifyLookupStream5PhantoonFade), () => VerifyLookupStream5PhantoonFade(rom));
    return 0;
}
if (args is ["--lookup-stream-4-ending-font-spaces"] or ["--lookup-stream-4-ending-font-outline"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending font oracle revision");
    Suite(nameof(VerifyLookupStream4EndingFontSpaces), () => VerifyLookupStream4EndingFontSpaces(rom));
    return 0;
}
if (args is ["--lookup-stream5-map-buttons"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map buttons oracle revision");
    Suite(nameof(VerifyLookupStream5MapButtons), () => VerifyLookupStream5MapButtons(rom));
    return 0;
}
if (args is ["--lookup-stream5-mode7-transfers"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres Mode7 oracle revision");
    Suite(nameof(VerifyCompiledCeresRidleyMode7Transfers), () => VerifyCompiledCeresRidleyMode7Transfers(rom));
    return 0;
}
if (args is ["--lookup-stream3-menu-borders"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu borders oracle revision");
    Suite(nameof(VerifyStream3FileSelectBorders), () => VerifyStream3FileSelectBorders(rom, FileSelectPresentationExtractor.Extract(rom)));
    Suite(nameof(VerifyStream3OptionsBorders), () => VerifyStream3OptionsBorders(rom, GameOptionsPresentationExtractor.Extract(rom)));
    Console.WriteLine("Menu borders:258native parts,1032independent edits,six reordered/expanded compositions pass.");
    return 0;
}
if (args is ["--lookup-stream-4-maridia-colors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Maridia colors oracle revision");
    Suite(nameof(VerifyLookupStream4MaridiaColors), () => VerifyLookupStream4MaridiaColors(rom));
    return 0;
}
if (args is ["--lookup-stream-4-planet-text"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Planet text oracle revision");
    Suite(nameof(VerifyLookupStream4PlanetText), () => VerifyLookupStream4PlanetText(rom));
    return 0;
}
if (args is ["--lookup-stream3-file-select-borders"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyStream3FileSelectBorders), () => VerifyStream3FileSelectBorders(rom, FileSelectPresentationExtractor.Extract(rom)));
    Console.WriteLine("File-select borders:114native parts,456independent edits,reversed orders and expanded compositions pass;bounds,appearance and order remain required.");
    return 0;
}
if (args is ["--lookup-stream-4-ceres-health"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres health oracle revision");
    Suite(nameof(VerifyLookupStream4CeresHealth), () => VerifyLookupStream4CeresHealth(rom));
    return 0;
}
if (args is ["--lookup-stream-4-ceres-zoom-retreat"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres zoom oracle revision");
    Suite(nameof(VerifyLookupStream4CeresZoomAndRetreat), () => VerifyLookupStream4CeresZoomAndRetreat(rom));
    return 0;
}
if (args is ["--lookup-stream-4-ceres-retreat-shared"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres retreat oracle revision");
    Suite(nameof(VerifyLookupStream4CeresRetreatShared), () => VerifyLookupStream4CeresRetreatShared(rom));
    return 0;
}
if (args is ["--lookup-stream-4-ceres-start"] or ["--lookup-stream-4-ceres-baby"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres start oracle revision");
    Suite(nameof(VerifyLookupStream4CeresStart), () => VerifyLookupStream4CeresStart(rom));
    Suite(nameof(VerifyLookupStream4CeresBaby), () => VerifyLookupStream4CeresBaby(rom));
    return 0;
}
if (args is ["--lookup-stream-4-ceres-fades"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres fades oracle revision");
    Suite(nameof(VerifyLookupStream4CeresFades), () => VerifyLookupStream4CeresFades(rom));
    return 0;
}
if (args is ["--lookup-stream-4-beam-paint"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Beam paint oracle revision");
    Suite(nameof(VerifyLookupStream4BeamColorRelations), () => VerifyLookupStream4BeamColorRelations(rom));
    return 0;
}
if (args is ["--lookup-stream-4-norfair-initial"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Norfair initial oracle revision");
    Suite(nameof(VerifyLookupStream4NorfairInitial), () => VerifyLookupStream4NorfairInitial(rom));
    return 0;
}
if (args is ["--lookup-stream-4-botwoon-health"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Botwoon palette oracle revision");
    Suite(nameof(VerifyLookupStream4BotwoonColors), () => VerifyLookupStream4BotwoonColors(rom));
    return 0;
}
if (args is ["--lookup-stream-4-ceres-alarm"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres alarm oracle revision");
    Suite(nameof(VerifyLookupStream4CeresAlarm), () => VerifyLookupStream4CeresAlarm(rom));
    return 0;
}
if (args is ["--lookup-stream-4-maridia-palette"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Maridia palette oracle revision");
    Suite(nameof(VerifyLookupStream4MaridiaPaletteDefinitions), () => VerifyLookupStream4MaridiaPaletteDefinitions(rom));
    return 0;
}
if (args is ["--lookup-stream3-grapple-tile-patterns"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        GrappleTileTransfer[] expectedTransfers =
        [
            new(VramAssetId.GrapplePointFirstTiles, 0x9a8200, 0, 32),
            new(VramAssetId.GrapplePointSecondTiles, 0x9a8400, 32, 32),
            new(VramAssetId.GrapplePointThirdTiles, 0x9a8600, 64, 32),
            new(VramAssetId.GrapplePointFourthTiles, 0x9a8800, 96, 32),
            new(VramAssetId.GrappleHorizontalSegmentTiles, 0x9a8220, 128, 128),
            new(VramAssetId.GrappleDiagonalSegmentTiles, 0x9a8a20, 256, 128),
            new(VramAssetId.GrappleVerticalSegmentTiles, 0x9a9220, 384, 128),
        ];
        VerifyStream3GrappleTilePatterns(rom, expectedTransfers);
    Console.WriteLine("Grapple tiles:512 native bytes,64 plane edits,8 coverage edits and seven transfer bindings pass; only reviewed stroke masks and visual shape choices remain.");
    return 0;
}
if (args is ["--lookup-stream-4-ending-shake"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending shake oracle revision");
    Suite(nameof(VerifyLookupStream4EndingShake), () => VerifyLookupStream4EndingShake(rom));
    return 0;
}
if (args is ["--lookup-stream-4-beam-geometry"])
{
    Suite(nameof(VerifyLookupStream4BeamTileGeometry), () => VerifyLookupStream4BeamTileGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream3-baby-sprite-reflection"])
{
    Suite(nameof(VerifyStream3BabySpriteReflection), () => VerifyStream3BabySpriteReflection(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Console.WriteLine("Baby sprite reflection:90 native parts/OAM,90 isolated X edits,27 field edits,order/count/empty layouts,hashes/bounds and64 legacy schemas pass.");
    return 0;
}
if (args is ["--lookup-stream-1-visor-colors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyLookupStream1VisorColors), () => VerifyLookupStream1VisorColors(rom));
    Suite(nameof(VerifySamusVisorColors), () => VerifySamusVisorColors());
    return 0;
}
if (args is ["--lookup-stream-1-owtch-cadence"])
{
    Suite(nameof(VerifyOwtchInstructionProgramDefinitions), () => VerifyOwtchInstructionProgramDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-power-direction-bindings"])
{
    Suite(nameof(VerifyLookupStream2PowerDirectionBindings), () => VerifyLookupStream2PowerDirectionBindings(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream-1-escape-text"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyLookupStream1EscapeText), () => VerifyLookupStream1EscapeText(rom));
    return 0;
}
if (args is ["--lookup-stream-1-atmospheric-attributes"])
{
    string sourceRom = Path.GetFullPath("Super Metroid.smc");
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
    Suite(nameof(VerifyLookupStream1AtmosphericAttributes), () => VerifyLookupStream1AtmosphericAttributes(rom));
    Suite(nameof(VerifySamusAtmosphereArtworkBoundary), () => VerifySamusAtmosphereArtworkBoundary(sourceRom));
    return 0;
}
if (args is ["--lookup-stream-1-oam-pointers"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "OAM selector oracle revision");
    Suite(nameof(VerifyLookupStream1OamPointers), () => VerifyLookupStream1OamPointers(rom));
    return 0;
}
if (args is ["--lookup-stream-1-body-transfers"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Body transfer oracle revision");
    Suite(nameof(VerifyLookupStream1BodyTransfers), () => VerifyLookupStream1BodyTransfers(rom));
    return 0;
}
if (args is ["--lookup-stream-1-cannon-placement"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Cannon placement oracle revision");
    Suite(nameof(VerifyLookupStream1CannonPlacement), () => VerifyLookupStream1CannonPlacement(rom));
    return 0;
}
if (args is ["--lookup-stream-1-cannon-drawing"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyLookupStream1CannonDrawingControls), () => VerifyLookupStream1CannonDrawingControls(rom));
    Suite(nameof(VerifyLookupStream1CannonPoses), () => VerifyLookupStream1CannonPoses(rom));
    return 0;
}
if (args is ["--lookup-stream-1-cannon-poses"])
{
    Suite(nameof(VerifyLookupStream1CannonPoses), () => VerifyLookupStream1CannonPoses(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream-1-animation-pointers"])
{
    string sourceRom = Path.GetFullPath("Super Metroid.smc");
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Animation pointer oracle revision");
    Suite(nameof(VerifySamusAnimationDelayDefinitions), () => VerifySamusAnimationDelayDefinitions(rom, sourceRom));
    Suite(nameof(VerifyLookupStream1AnimationAliases), () => VerifyLookupStream1AnimationAliases(rom));
    return 0;
}
if (args is ["--lookup-stream-1-hud-posture"])
{
    Suite(nameof(VerifyLookupStream1HudPosture), () => VerifyLookupStream1HudPosture(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-projectile-identity-geometry"])
{
    Suite(nameof(VerifyLookupStream2ProjectileIdentityGeometry), () => VerifyLookupStream2ProjectileIdentityGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-narration-layout"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    var json = IntroNarrationExtractor.Extract(rom);
    Suite(nameof(VerifyStream3NarrationLayout), () => VerifyStream3NarrationLayout(rom, json, IntroNarrationPresentation.Load(new MemoryStream(json))));
    Console.WriteLine("Narration layout: 770 native glyph/coordinate records, six calculated pages, one retained hard break and 34 independent edits pass.");
    return 0;
}
if (args is ["--lookup-stream-1-dachora-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Dachora program oracle revision");
    Suite(nameof(VerifyLookupStream1EscapeDachoraPrograms), () => VerifyLookupStream1EscapeDachoraPrograms(rom));
    return 0;
}
if (args is ["--lookup-stream-1-powamp-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Powamp program oracle revision");
    Suite(nameof(VerifyLookupStream1PowampPrograms), () => VerifyLookupStream1PowampPrograms(rom));
    return 0;
}
if (args is ["--lookup-intro-narration-registry"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Timer layout oracle revision");
    Suite(nameof(VerifyLookupStream1TimerLayout), () => VerifyLookupStream1TimerLayout(rom));
    return 0;
}
if (args is ["--lookup-stream-1-flare-placement"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Flare placement oracle revision");
    Suite(nameof(VerifyLookupStream1FlarePlacement), () => VerifyLookupStream1FlarePlacement(rom));
    return 0;
}
if (args is ["--lookup-mochtroid-shake"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    foreach (ushort timer in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 0xfff8, 0xfff9, 0xfffa, 0xfffb, 0xfffc, 0xfffd, 0xfffe, 0xffff })
    {
        int offset = timer & 6;
        short x = unchecked((short)(rom.ReadByte(0xa3a76d + offset) | rom.ReadByte(0xa3a76e + offset) << 8));
        short y = unchecked((short)(rom.ReadByte(0xa3a775 + offset) | rom.ReadByte(0xa3a776 + offset) << 8));
        AssertEqual(((int)x, (int)y), MochtroidShakeDefinitions.Offset(timer), "Native cardinal shake and timer-bit mask");
    }
    Suite(nameof(VerifyMochtroidInstructionProgramDefinitions), () => VerifyMochtroidInstructionProgramDefinitions(rom));
    Console.WriteLine("Mochtroid shake: sixteen native masked-timer comparisons pass; amplitude remains required.");
    return 0;
}
if (args is ["--lookup-mochtroid-visuals"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mochtroid oracle revision");
    Suite(nameof(VerifyStream3MochtroidVisuals), () => VerifyStream3MochtroidVisuals(rom));
    Suite(nameof(VerifyMochtroidInstructionProgramDefinitions), () => VerifyMochtroidInstructionProgramDefinitions(rom));
    Console.WriteLine("Mochtroid: six native registrations, eight selectors, address rejection and production instruction checks pass.");
    return 0;
}
if (args is ["--lookup-stream2-environmental-catalogs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Environmental catalog oracle revision");
    Suite(nameof(VerifyLookupStream2EnvironmentalCatalogs), () => VerifyLookupStream2EnvironmentalCatalogs(rom));
    return 0;
}
if (args is ["--lookup-stream2-backdrop-geometry"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Backdrop oracle revision");
    Suite(nameof(VerifyLookupStream2BackdropGeometry), () => VerifyLookupStream2BackdropGeometry(rom));
    return 0;
}
if (args is ["--lookup-menu-sprite-geometry"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu geometry oracle revision");
    Suite(nameof(VerifyStream3MenuSpriteGeometry), () => VerifyStream3MenuSpriteGeometry(rom));
    Console.WriteLine("Menu geometry: native compositions, calculated stock storage, independent part edits, reordering and three loader bindings pass.");
    return 0;
}
if (args is ["--lookup-stream2-equipment-base-geometry"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Equipment template oracle revision");
    Suite(nameof(VerifyLookupStream2EquipmentBaseGeometry), () => VerifyLookupStream2EquipmentBaseGeometry(rom));
    return 0;
}
if (args is ["--lookup-stream2-wireframe-mirrors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Wireframe oracle revision");
    Suite(nameof(VerifyLookupStream2WireframeMirrors), () => VerifyLookupStream2WireframeMirrors(rom));
    return 0;
}
if (args is ["--lookup-intro-caret-registration"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Enemy registration oracle revision");
    Suite(nameof(VerifyStream3EnemyFrameRegistration), () => VerifyStream3EnemyFrameRegistration(rom));
    return 0;
}
if (args is ["--lookup-small-game-tables"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Small game tables oracle revision");
    Suite(nameof(VerifyLookupSmallGameTables), () => VerifyLookupSmallGameTables(rom));
    return 0;
}
if (args is ["--lookup-dachora-colors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Dachora colors oracle revision");
    Suite(nameof(VerifyLookupDachoraColors), () => VerifyLookupDachoraColors(rom));
    return 0;
}
if (args is ["--lookup-stream-4-spore-health-ramp"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore health ramp oracle revision");
    Suite(nameof(VerifyLookupStream4SporeHealthRamp), () => VerifyLookupStream4SporeHealthRamp(rom));
    return 0;
}
if (args is ["--lookup-stream-4-spore-fade"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore fade oracle revision");
    Suite(nameof(VerifyLookupStream4SporeSpriteFade), () => VerifyLookupStream4SporeSpriteFade(rom));
    Suite(nameof(VerifyLookupStream4SporeBackgroundFade), () => VerifyLookupStream4SporeBackgroundFade(rom));
    Suite(nameof(VerifyLookupStream4SporeLevelFade), () => VerifyLookupStream4SporeLevelFade(rom));
    Suite(nameof(VerifyLookupStream4SporeHealthyAlias), () => VerifyLookupStream4SporeHealthyAlias(rom));
    return 0;
}
if (args is ["--lookup-stream2-reserve-labels"])
{
    Suite(nameof(VerifyLookupStream2ReserveLabels), () => VerifyLookupStream2ReserveLabels(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-equipment-labels"])
{
    Suite(nameof(VerifyLookupStream2EquipmentLabels), () => VerifyLookupStream2EquipmentLabels(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-file-select-patches"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    var data = SuperMetroid.AssetExtraction.FileSelectPresentationExtractor.Extract(rom);
    Suite(nameof(VerifyStream3FileSelectPatches), () => VerifyStream3FileSelectPatches(rom, data, FileSelectPresentation.Load(new MemoryStream(data))));
    Console.WriteLine("File-select patches: native cells, zero stock fallback, independent edits and actual placement pass.");
    return 0;
}
if (args is ["--lookup-stream2-equipment-blank"])
{
    Suite(nameof(VerifyLookupStream2EquipmentBlank), () => VerifyLookupStream2EquipmentBlank(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream-4-beam-basis"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Beam basis oracle revision");
    Suite(nameof(VerifyLookupStream4BeamColorRelations), () => VerifyLookupStream4BeamColorRelations(rom));
    Console.WriteLine("Beam basis: exactly43 required stored colors and149 calculated values; all native outputs, independent edits and CGRAM isolation pass.");
    return 0;
}
if (args is ["--lookup-stream-1-cannon-basis"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Cannon basis oracle revision");
    Suite(nameof(VerifyLookupStream1ArmCannonTileSources), () => VerifyLookupStream1ArmCannonTileSources(rom));
    Console.WriteLine("Cannon calculated defaults, zero stock overrides, native selectors, edits and DMA checks passed.");
    return 0;
}
if (args is ["--lookup-stream-4-ending-subtitle"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Subtitle oracle revision");
    Suite(nameof(VerifyLookupStream4EndingSubtitle), () => VerifyLookupStream4EndingSubtitle(rom));
    return 0;
}
if (args is ["--lookup-stream-4-ending-panel"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Producer panel oracle revision");
    Suite(nameof(VerifyLookupStream4EndingResultPanel), () => VerifyLookupStream4EndingResultPanel(rom));
    Suite(nameof(VerifyLookupStream4EndingSubtitle), () => VerifyLookupStream4EndingSubtitle(rom));
    return 0;
}
if (args is ["--lookup-stream2-golden-awakening-layout"])
{
    Suite(nameof(VerifyGoldenTorizoAwakeningDefinitions), () => VerifyGoldenTorizoAwakeningDefinitions(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream-4-tail-rest"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tail rest oracle revision");
    Suite(nameof(VerifyLookupStream4TailRestGeometry), () => VerifyLookupStream4TailRestGeometry(rom));
    return 0;
}
if (args is ["--lookup-stream-4-baby-phase"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby phase oracle revision");
    Suite(nameof(VerifyLookupStream4BabyTransferPhase), () => VerifyLookupStream4BabyTransferPhase(rom));
    return 0;
}
if (args is ["--lookup-stream4-fly-escape"])
{
    var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyFlyFrameIdentities), () => VerifyFlyFrameIdentities(source));
    Suite(nameof(VerifyZebesEscapeExplosionDefinitions), () => VerifyZebesEscapeExplosionDefinitions(source));
    return 0;
}
if (args is ["--lookup-stream4-hud-auto-cells"])
{
    Suite(nameof(VerifyLookupStream4HudAutoCells), () => VerifyLookupStream4HudAutoCells(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream4-hud-anchors"])
{
    Suite(nameof(VerifyLookupStream4HudAnchors), () => VerifyLookupStream4HudAnchors(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream4-hud-digits"])
{
    Suite(nameof(VerifyLookupStream4HudDigits), () => VerifyLookupStream4HudDigits(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream4-title-identities"])
{
    Suite(nameof(VerifyTitleSpriteIdentities), () => VerifyTitleSpriteIdentities(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream4-title-card"])
{
    Suite(nameof(VerifyTitleCardLayout), () => VerifyTitleCardLayout(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream4-sprite-dispatch"])
{
    Suite(nameof(VerifyRoomSpriteDispatch), () => VerifyRoomSpriteDispatch(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream4-spore-collision"])
{
    Suite(nameof(VerifyCompiledSporeSpawnCollision), () => VerifyCompiledSporeSpawnCollision(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-dead-torizo-geometry"])
{
    Suite(nameof(VerifyLookupStream2DeadTorizoGeometry), () => VerifyLookupStream2DeadTorizoGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-crawler-ramps"])
{
    Suite(nameof(VerifyLookupStream2CrawlerRamps), () => VerifyLookupStream2CrawlerRamps(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-reserve-arrow-colors"])
{
    Suite(nameof(VerifyLookupStream2ReserveArrowColors), () => VerifyLookupStream2ReserveArrowColors(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-trail-appearance"])
{
    Suite(nameof(VerifyLookupStream2TrailAppearance), () => VerifyLookupStream2TrailAppearance(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-lava-jumper-layout"])
{
    Suite(nameof(VerifyLookupStream2LavaJumperLayout), () => VerifyLookupStream2LavaJumperLayout(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-chozo-layout"])
{
    Suite(nameof(VerifyLookupStream2ChozoLayout), () => VerifyLookupStream2ChozoLayout(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-torizo-movement"])
{
    Suite(nameof(VerifyBombTorizoMovementDefinitions), () => VerifyBombTorizoMovementDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-zebes-star-fields"])
{
    Suite(nameof(VerifyLookupStream5ZebesStarFields), () => VerifyLookupStream5ZebesStarFields(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-statue-ramps"])
{
    Suite(nameof(VerifyLookupStream5StatueRamps), () => VerifyLookupStream5StatueRamps(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-crocomire-rumble"])
{
    Suite(nameof(VerifyCrocomireRumbleDefinitions), () => VerifyCrocomireRumbleDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-phantoon-markers"])
{
    Suite(nameof(VerifyLookupStream5PhantoonMarkers), () => VerifyLookupStream5PhantoonMarkers(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-magdollite-pulse"])
{
    Suite(nameof(VerifyLookupStream5MagdollitePulse), () => VerifyLookupStream5MagdollitePulse(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-zebetite-pulse"])
{
    Suite(nameof(VerifyLookupStream5ZebetitePulse), () => VerifyLookupStream5ZebetitePulse(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-zebetite-geometry"])
{
    Suite(nameof(VerifyZebetiteDefinitions), () => VerifyZebetiteDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-xray-room-rules"])
{
    Suite(nameof(VerifyXrayRoomDisplayRules), () => VerifyXrayRoomDisplayRules());
    return 0;
}
if (args is ["--lookup-stream2-torizo-page-dispatch"])
{
    Suite(nameof(VerifyLookupStream2TorizoPageDispatch), () => VerifyLookupStream2TorizoPageDispatch(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-kraid-lint-initialization"])
{
    Suite(nameof(VerifyLookupStream2KraidLintInitialization), () => VerifyLookupStream2KraidLintInitialization(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-kago-frame-geometry"])
{
    Suite(nameof(VerifyLookupStream2KagoFrameGeometry), () => VerifyLookupStream2KagoFrameGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-pipe-bug-visual-geometry"])
{
    Suite(nameof(VerifyLookupStream2PipeBugVisualGeometry), () => VerifyLookupStream2PipeBugVisualGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-horizontal-camera-targets"])
{
    Suite(nameof(VerifyLookupStream2HorizontalCameraTargets), () => VerifyLookupStream2HorizontalCameraTargets(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-drop-selection"])
{
    Suite(nameof(VerifyLookupStream2DropSelection), () => VerifyLookupStream2DropSelection(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-pipe-bug-formation"])
{
    Suite(nameof(VerifyLookupStream2PipeBugFormation), () => VerifyLookupStream2PipeBugFormation(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream-1-body-pixels"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Body pixel oracle revision");
    Suite(nameof(VerifyLookupStream1BodyPixels), () => VerifyLookupStream1BodyPixels(rom));
    return 0;
}
if (args is ["--lookup-stream5-yapping-maw-offsets"])
{
    Suite(nameof(VerifyLookupStream5YappingMawOffsets), () => VerifyLookupStream5YappingMawOffsets(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-eye-geometry"])
{
    Suite(nameof(VerifyLookupStream5EyeGeometry), () => VerifyLookupStream5EyeGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-golden-control"])
{
    Suite(nameof(VerifyLookupStream2GoldenControl), () => VerifyLookupStream2GoldenControl(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-trail-programs"])
{
    Suite(nameof(VerifyLookupStream2TrailPrograms), () => VerifyLookupStream2TrailPrograms(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-trail-selectors"])
{
    Suite(nameof(VerifyLookupStream2TrailSelectors), () => VerifyLookupStream2TrailSelectors(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-tube-ramp"])
{
    Suite(nameof(VerifyLookupStream2TubeRamp), () => VerifyLookupStream2TubeRamp(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-pause-ownership"])
{
    Suite(nameof(VerifyLookupStream2PauseOwnership), () => VerifyLookupStream2PauseOwnership(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-dead-torizo-transfers"])
{
    Suite(nameof(VerifyLookupStream2DeadTorizoTransfers), () => VerifyLookupStream2DeadTorizoTransfers(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream2-pipe-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 pipe-program oracle revision");
    Suite(nameof(VerifyPipeBugAnimationDefinitions), () => VerifyPipeBugAnimationDefinitions(rom));
    Suite(nameof(VerifyNorfairPipeBugInstructionProgramDefinitions), () => VerifyNorfairPipeBugInstructionProgramDefinitions(rom));
    return 0;
}
if (args is ["--lookup-stream2-body-placements"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 body-placement oracle revision");
    Suite(nameof(VerifyBombTorizoAttackDefinitions), () => VerifyBombTorizoAttackDefinitions(rom));
    Suite(nameof(VerifyCompiledStatueWalking), () => VerifyCompiledStatueWalking(rom));
    return 0;
}
if (args is ["--lookup-stream2-crawler-animations"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 2 crawler original oracle revision");
    Suite(nameof(VerifyCrawlerAnimationDefinitions), () => VerifyCrawlerAnimationDefinitions(rom));
    Suite(nameof(VerifyWaverAnimationDefinitions), () => VerifyWaverAnimationDefinitions(rom));
    return 0;
}
if (args is ["--lookup-stream2-gunship-transfers"] or ["--lookup-stream2-reserve-geometry"] or ["--lookup-stream2-ghost-norfair"] or ["--lookup-stream2-palette-mechanics"] or ["--lookup-stream2-yard-directions"] or ["--lookup-stream2-crystal-body"] or ["--lookup-stream2-kraid-ramps"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 1 oracle revision");
    Suite(nameof(VerifySciserInstructionProgramDefinitions), () => VerifySciserInstructionProgramDefinitions());
    Suite(nameof(VerifySamusDeathSequence), () => VerifySamusDeathSequence());
    Suite(nameof(VerifyNuclearWaffleProjectileInstructionProgramDefinitions), () => VerifyNuclearWaffleProjectileInstructionProgramDefinitions());
    Suite(nameof(VerifyNuclearWaffleDefinitions), () => VerifyNuclearWaffleDefinitions(rom));
    Suite(nameof(VerifyOwtchInstructionProgramDefinitions), () => VerifyOwtchInstructionProgramDefinitions());
    Suite(nameof(VerifyStokeInstructionProgramDefinitions), () => VerifyStokeInstructionProgramDefinitions());
    Suite(nameof(VerifyPuyoInstructionProgramDefinitions), () => VerifyPuyoInstructionProgramDefinitions());
    Suite(nameof(VerifyMiscDustProjectileDefinitions), () => VerifyMiscDustProjectileDefinitions(rom));
    Suite(nameof(VerifyMetroidsClearedStatePlm), () => VerifyMetroidsClearedStatePlm(new TestAddressSpace()));
    Suite(nameof(VerifyMetroidInstructionProgramDefinitions), () => VerifyMetroidInstructionProgramDefinitions());
    Suite(nameof(VerifySamusAtmosphericEffectDefinitions), () => VerifySamusAtmosphericEffectDefinitions(rom));
    Suite(nameof(VerifySamusHudDefinitions), () => VerifySamusHudDefinitions(rom));
    Suite(nameof(VerifySamusStoredShineAndShinespark), () => VerifySamusStoredShineAndShinespark());
    Suite(nameof(VerifySamusArmCannonDefinitions), () => VerifySamusArmCannonDefinitions(rom));
    Suite(nameof(VerifyPoseDispatchDefinitions), () => VerifyPoseDispatchDefinitions(rom));
    Suite(nameof(VerifyPoseCollisionDefinitions), () => VerifyPoseCollisionDefinitions(rom));
    Suite(nameof(VerifyPoseProjectileOrigin), () => VerifyPoseProjectileOrigin(rom));
    Suite(nameof(VerifyProjectileOrigins), () => VerifyProjectileOrigins(rom));
    Suite(nameof(VerifyCompiledSpeedBoosterPlmPrograms), () => VerifyCompiledSpeedBoosterPlmPrograms());
    Suite(nameof(VerifyLookupStream1), () => VerifyLookupStream1(rom));
    Suite(nameof(VerifyBrinstarPipeBugInstructionProgramDefinitions), () => VerifyBrinstarPipeBugInstructionProgramDefinitions());
    Suite(nameof(VerifyMetroidBehaviorDefinitions), () => VerifyMetroidBehaviorDefinitions(rom));
    Suite(nameof(VerifyBrinstarBlueSporePaletteFxProgramMechanicsDefinitions), () => VerifyBrinstarBlueSporePaletteFxProgramMechanicsDefinitions(rom));
    Suite(nameof(VerifyDragonFireballInstructionProgramDefinitions), () => VerifyDragonFireballInstructionProgramDefinitions());
    Suite(nameof(VerifyEscapeEtecoonInstructionProgramDefinitions), () => VerifyEscapeEtecoonInstructionProgramDefinitions());
    Suite(nameof(VerifyDragonInstructionProgramDefinitions), () => VerifyDragonInstructionProgramDefinitions());
    Suite(nameof(VerifyCacatacInstructionProgramDefinitions), () => VerifyCacatacInstructionProgramDefinitions());
    Suite(nameof(VerifyBullInstructionProgramDefinitions), () => VerifyBullInstructionProgramDefinitions(rom));
    Suite(nameof(VerifyChargeFlareDefinitions), () => VerifyChargeFlareDefinitions(rom));
    Suite(nameof(VerifySamusIndexedSpeeds), () => VerifySamusIndexedSpeeds(rom));
    Suite(nameof(VerifyLookupStream1ProjectileMotion), () => VerifyLookupStream1ProjectileMotion(rom));
    Suite(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables(initializeOnly: true));
    Suite(nameof(VerifyLookupStream1Selection), () => VerifyLookupStream1Selection(rom));
    Suite(nameof(VerifyLookupStream1LaunchRoles), () => VerifyLookupStream1LaunchRoles(rom));
    Suite(nameof(VerifyLookupStream1SparkSpikePrograms), () => VerifyLookupStream1SparkSpikePrograms(rom));
    Suite(nameof(VerifyPoseInputDefinitions), () => VerifyPoseInputDefinitions(rom));
    Suite(nameof(VerifyProjectileDamage), () => VerifyProjectileDamage(rom));
    Suite(nameof(VerifyProjectileSoundRoutingDefinitions), () => VerifyProjectileSoundRoutingDefinitions(rom));
    Console.WriteLine("Stream 1 lookup conversions: focused original-source and domain checks pass.");
    return 0;
}
if (args is ["--lookup-stream-4"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 4 oracle revision");
    Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions());
    Suite(nameof(VerifyEyeDoorProjectileInstructionProgramDefinitions), () => VerifyEyeDoorProjectileInstructionProgramDefinitions());
    Suite(nameof(VerifyCompiledDraygonBg2Collision), () => VerifyCompiledDraygonBg2Collision(rom));
    Suite(nameof(VerifyRidleyCollisionDefinitions), () => VerifyRidleyCollisionDefinitions());
    Suite(nameof(VerifyLookupStream4), () => VerifyLookupStream4(rom));
    Suite(nameof(VerifyPowerBombFixedColors), () => VerifyPowerBombFixedColors());
    Suite(nameof(VerifyRidleyAttackChoices), () => VerifyRidleyAttackChoices(rom));
    Suite(nameof(VerifyRidleyMovementTargets), () => VerifyRidleyMovementTargets(rom));
    Suite(nameof(VerifyRidleyClawOffsets), () => VerifyRidleyClawOffsets(rom));
    Suite(nameof(VerifySaveRamLayout), () => VerifySaveRamLayout());
    Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions());
    Suite(nameof(VerifyDraygonProjectileInstructionProgramDefinitions), () => VerifyDraygonProjectileInstructionProgramDefinitions());
    Suite(nameof(VerifyBotwoonHoleRightBounds), () => VerifyBotwoonHoleRightBounds(rom));
    Suite(nameof(VerifyBotwoonHoleBottomBounds), () => VerifyBotwoonHoleBottomBounds(rom));
    Suite(nameof(VerifyBotwoonWallStockMapping), () => VerifyBotwoonWallStockMapping(rom));
    Suite(nameof(VerifyCompiledDraygonOamCollision), () => VerifyCompiledDraygonOamCollision(rom));
    Console.WriteLine("Stream 4 lookup conversions: focused original-source and domain checks pass.");
    return 0;
}
if (args is ["--lookup-stream5-map-highlight"])
{
    Suite(nameof(VerifyLookupStream5MapHighlight), () => VerifyLookupStream5MapHighlight(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-sidehopper-geometry"])
{
    Suite(nameof(VerifyLookupStream5SidehopperGeometry), () => VerifyLookupStream5SidehopperGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-phantoon-schedules"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyCompiledPhantoonCasualFlames), () => VerifyCompiledPhantoonCasualFlames(rom));
    Suite(nameof(VerifyPhantoonSoundDefinitions), () => VerifyPhantoonSoundDefinitions(rom));
    return 0;
}
if (args is ["--lookup-stream5-phantoon-rain"])
{
    Suite(nameof(VerifyLookupStream5PhantoonRain), () => VerifyLookupStream5PhantoonRain(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-ceres-door-ramp"])
{
    Suite(nameof(VerifyLookupStream5CeresDoorRamp), () => VerifyLookupStream5CeresDoorRamp(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--lookup-stream5-corpse-geometry"])
{
    Suite(nameof(VerifyCorpseMetadataDefinitions), () => VerifyCorpseMetadataDefinitions(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Suite(nameof(VerifyLookupStream5CorpseViews), () => VerifyLookupStream5CorpseViews());
    Console.WriteLine("Corpse geometry: all30 rotation offsets,32 sand offsets,32 four-field DMA descriptors and10 terminators match original ROM.");
    return 0;
}
if (args is ["--lookup-stream5-crocomire-order"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire destruction-order oracle revision");
    Suite(nameof(VerifyCrocomireBridgeFragmentDefinitions), () => VerifyCrocomireBridgeFragmentDefinitions(rom));
    return 0;
}
if (args is ["--lookup-stream5-actor-layouts"] or ["--lookup-stream5-initialization"] or ["--lookup-stream5-palette-entries"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "File-select identity fixture revision");
    byte[] source = FileSelectPresentationExtractor.Extract(rom);
    Suite(nameof(VerifyFileSelectPageNames), () => VerifyFileSelectPageNames(source));
    Suite(nameof(VerifyFileSelectPatchNames), () => VerifyFileSelectPatchNames(source));
    Suite(nameof(VerifyFileSelectBorderNames), () => VerifyFileSelectBorderNames(source));
    Suite(nameof(VerifyFileSelectDynamicAnchorNames), () => VerifyFileSelectDynamicAnchorNames(source));
    Suite(nameof(VerifyFileSelectSpriteNames), () => VerifyFileSelectSpriteNames(source));
    Console.WriteLine("File-select names: all five original identity domains, ordered cases, bounds and exact loader membership pass.");
    return 0;
}
if (args is ["--lookup-map-sprite-cases"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map sprite case oracle revision");
    Suite(nameof(VerifyMapSpriteCompositionCases), () => VerifyMapSpriteCompositionCases(rom));
    Console.WriteLine("Map sprite cases: all original OAM, independent role edits, required membership and invalid identities pass.");
    return 0;
}
if (args is ["--lookup-map-sprite-names"])
{
    Suite(nameof(VerifyMapSpriteNameCases), () => VerifyMapSpriteNameCases());
    Console.WriteLine("Map sprite names: all original identities and order, complete ushort membership and invalid names pass.");
    return 0;
}
if (args is ["--lookup-menu-title-font"] or ["--lookup-menu-large-font"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Large font pixel oracle revision");
    Suite(nameof(VerifyMenuLargeFontPixels), () => VerifyMenuLargeFontPixels(rom));
    Console.WriteLine("Large font pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-thin-border-pixels"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Thin border pixel oracle revision");
    Suite(nameof(VerifyMenuThinBorderPixels), () => VerifyMenuThinBorderPixels(rom));
    Console.WriteLine("Thin border pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-beveled-square-pixels"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Beveled square pixel oracle revision");
    Suite(nameof(VerifyMenuBeveledSquarePixels), () => VerifyMenuBeveledSquarePixels(rom));
    Console.WriteLine("Beveled square pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-map-arrow-pixels"] or ["--lookup-map-pulse-pixels"] or ["--lookup-defeated-boss-pixels"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Shoulder highlight pixel oracle revision");
    Suite(nameof(VerifyMenuShoulderHighlightPixels), () => VerifyMenuShoulderHighlightPixels(rom));
    Console.WriteLine("Shoulder highlight pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-shoulder-button-pixels"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Shoulder button pixel oracle revision");
    Suite(nameof(VerifyMenuShoulderButtonPixels), () => VerifyMenuShoulderButtonPixels(rom));
    Console.WriteLine("Shoulder button pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-panel-pixels"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu panel pixel oracle revision");
    Suite(nameof(VerifyMenuPanelPixels), () => VerifyMenuPanelPixels(rom));
    Console.WriteLine("Menu panel pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-elevator-lettering"] or ["--lookup-menu-outlined-lettering"] or ["--lookup-menu-compact-lettering"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevator lettering pixel oracle revision");
    Suite(nameof(VerifyMenuCompactLetteringPixels), () => VerifyMenuCompactLetteringPixels(rom));
    Console.WriteLine("Elevator lettering pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-menu-small-font"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Small font pixel oracle revision");
    Suite(nameof(VerifyMenuSmallFontPixels), () => VerifyMenuSmallFontPixels(rom));
    Console.WriteLine("Small font pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-highlight-tile-pixels"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Highlight pixel oracle revision");
    Suite(nameof(VerifyHighlightTilePixels), () => VerifyHighlightTilePixels(rom));
    Console.WriteLine("Highlight pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-reserve-tile-pixels"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Reserve pixel oracle revision");
    Suite(nameof(VerifyReserveTilePixels), () => VerifyReserveTilePixels(rom));
    Console.WriteLine("Reserve pixels: all original pixels, complete native uploads, independent edits, full custom region and bounds pass.");
    return 0;
}
if (args is ["--lookup-pause-reserve-frames"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Reserve frame oracle revision");
    Suite(nameof(VerifyPauseReserveFrameCases), () => VerifyPauseReserveFrameCases(rom));
    Console.WriteLine("Reserve frames: all native role identities and OAM, independent edits, required membership and invalid inputs pass.");
    return 0;
}
if (args is ["--lookup-pause-reserve-anchors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Reserve anchor oracle revision");
    Suite(nameof(VerifyPauseReserveAnchors), () => VerifyPauseReserveAnchors(rom));
    Console.WriteLine("Reserve anchors: native coordinate fields, independent edits, actual sprite output and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-explosion-frame-catalog"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending explosion frame catalog oracle revision");
    Suite(nameof(VerifyEndingExplosionFrameCatalog), () => VerifyEndingExplosionFrameCatalog(rom));
    Console.WriteLine("Ending explosion frame catalog: all16 native pointers/counts, published keys, enumeration and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-text-regions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending region oracle revision");
    Suite(nameof(VerifyEndingTextRegions), () => VerifyEndingTextRegions(rom));
    Console.WriteLine("Ending text regions: all six native text spans, positions and styles pass.");
    return 0;
}
if (args is ["--lookup-ending-glyphs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending glyph oracle revision");
    Suite(nameof(VerifyEndingGlyphMapping), () => VerifyEndingGlyphMapping(rom));
    Console.WriteLine("Ending glyphs: complete four-style alphabet/digit/blank mappings, both halves and all ushort decoder inputs pass.");
    return 0;
}
if (args is ["--lookup-ending-mode7-roles"])
{
    Suite(nameof(VerifyEndingMode7RoleSelection), () => VerifyEndingMode7RoleSelection());
    Console.WriteLine("Ending Mode7 roles: six original sources, filenames, supplied references, identity ordering and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-gunship-program"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending gunship program oracle revision");
    Suite(nameof(VerifyZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions), () => VerifyZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions(rom));
    Console.WriteLine("Ending gunship program:35 original control words,16 frame pointers,256 color pointers, complete ownership and384-frame lifetime pass.");
    return 0;
}
if (args is ["--lookup-ending-gunship-colors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending gunship color oracle revision");
    byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
    Suite(nameof(VerifyExtractedEndingGunshipPaletteFxPresentation), () => VerifyExtractedEndingGunshipPaletteFxPresentation(rom, RoomPaletteFxPresentation.Load(new MemoryStream(json))));
    Console.WriteLine("Ending gunship colors: all256 original words,239 calculated colors, complete pointer domain, guarded effect and independent edits pass.");
    return 0;
}
if (args is ["--lookup-ending-logo-glare-program"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Logo glare program oracle revision");
    Suite(nameof(VerifyPostCreditsIconGlarePaletteFxProgramMechanicsDefinitions), () => VerifyPostCreditsIconGlarePaletteFxProgramMechanicsDefinitions(rom));
    Console.WriteLine("Logo glare program: decoded31 control words,14 frame pointers,224 color pointers, complete ownership and lifetime pass.");
    return 0;
}
if (args is ["--lookup-ending-logo-glare"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Logo glare oracle revision");
    byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
    Suite(nameof(VerifyExtractedLogoGlarePaletteFxPresentation), () => VerifyExtractedLogoGlarePaletteFxPresentation(rom, RoomPaletteFxPresentation.Load(new MemoryStream(json))));
    Console.WriteLine("Logo glare: all224 native colors, full pointer domain, guarded program and independent edits pass.");
    return 0;
}
if (args is ["--lookup-ending-logo-fade"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo fade oracle revision");
    Suite(nameof(VerifyEndingLogoPaletteFade), () => VerifyEndingLogoPaletteFade(rom));
    Console.WriteLine("Ending logo fade: all512 original colors, computed transfers, independent edits and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-palette-metadata"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending palette metadata oracle revision");
    Suite(nameof(VerifyEndingPaletteMetadata), () => VerifyEndingPaletteMetadata(rom));
    Console.WriteLine("Ending palettes: six native sources, seven allocation sizes, published filenames and invalid roles pass.");
    return 0;
}
if (args is ["--lookup-ending-palette-roles"])
{
    Suite(nameof(VerifyEndingPaletteRoleSelection), () => VerifyEndingPaletteRoleSelection());
    Console.WriteLine("Ending palette roles: all seven supplied references, original identity order and invalid-role behavior pass.");
    return 0;
}
if (args is ["--lookup-ending-fragment-metadata"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending fragment oracle revision");
    Suite(nameof(VerifyEndingObjectFragmentMetadata), () => VerifyEndingObjectFragmentMetadata(rom));
    Console.WriteLine("Ending fragments: four native compressed sources, published filenames and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-cloud-definitions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending cloud oracle revision");
    Suite(nameof(VerifyEndingCloudDefinitions), () => VerifyEndingCloudDefinitions(rom));
    Console.WriteLine("Ending clouds: six catalog records,24 original program words, six actor streams and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-reward-actors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending reward actor oracle revision");
    Suite(nameof(VerifyEndingRewardActorDefinitions), () => VerifyEndingRewardActorDefinitions(rom));
    return 0;
}
if (args is ["--lookup-ending-reward-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending reward oracle revision");
    Suite(nameof(VerifyEndingRewardInstructions), () => VerifyEndingRewardInstructions(rom));
    Console.WriteLine("Ending reward programs: all160 native words,17 actor streams, callback timing, head record layout and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-explosion-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending explosion oracle revision");
    Suite(nameof(VerifyEndingExplosionPrograms), () => VerifyEndingExplosionPrograms(rom));
    Console.WriteLine("Ending explosion programs: all65 native words, eight actor streams, frame record layout and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-logo-actors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo actor oracle revision");
    Suite(nameof(VerifyEndingLogoDefinitions), () => VerifyEndingLogoDefinitions(rom));
    return 0;
}
if (args is ["--lookup-ending-logo-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo program oracle revision");
    Suite(nameof(VerifyEndingLogoPrograms), () => VerifyEndingLogoPrograms(rom));
    Console.WriteLine("Ending logo programs: all31 native words, four actor streams, callback timing and bounds pass.");
    return 0;
}
if (args is ["--lookup-ending-completion-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending completion oracle revision");
    Suite(nameof(VerifyEndingCompletionTextInstructions), () => VerifyEndingCompletionTextInstructions(rom));
    Console.WriteLine("Ending completion programs: all164 original words and all14 actor streams pass.");
    return 0;
}
if (args is ["--lookup-ending-logo-tables"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ending logo table oracle revision");
    Suite(nameof(VerifyEndingLogoPaletteSources), () => VerifyEndingLogoPaletteSources(rom));
    Suite(nameof(VerifyEndingPostShotTransferFields), () => VerifyEndingPostShotTransferFields(rom));
    Console.WriteLine("Ending logo tables: all32 original palette sources, six transfer records and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-audio-upload-catalog"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Audio upload oracle revision");
    Suite(nameof(VerifyAudioUploadCatalog), () => VerifyAudioUploadCatalog(rom));
    Console.WriteLine("Audio upload catalog: all25 original pointers, names, views and byte rejection domain pass.");
    return 0;
}
if (args is ["--lookup-spc-pan-interpolation"])
{
    Suite(nameof(VerifySpcPanInterpolation), () => VerifySpcPanInterpolation());
    return 0;
}
if (args is ["--lookup-spc-fir-addressing"])
{
    Suite(nameof(VerifySpcFirAddressing), () => VerifySpcFirAddressing());
    return 0;
}
if (args is ["--lookup-ceres-initial-metadata"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres destruction metadata oracle revision");
    Suite(nameof(VerifyCeresInitialActorMetadata), () => VerifyCeresInitialActorMetadata(rom));
    Console.WriteLine("Ceres destruction metadata: native spawn identities, asteroid aliases, stationary-vortex overrides and bounds pass.");
    return 0;
}
if (args is ["--lookup-ceres-flight-metadata"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres flight metadata oracle revision");
    Suite(nameof(VerifyCeresFlightActorMetadata), () => VerifyCeresFlightActorMetadata(rom));
    Console.WriteLine("Ceres flight metadata: native spawn identities, initializer branches, motion operands, star aliases and bounds pass.");
    return 0;
}
if (args is ["--lookup-ceres-zebes-metadata"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Zebes metadata oracle revision");
    Suite(nameof(VerifyCeresZebesActorMetadata), () => VerifyCeresZebesActorMetadata(rom));
    Console.WriteLine("Zebes actor metadata: all six native spawn identities, definitions, placements, motion policies and bounds pass.");
    return 0;
}
if (args is ["--lookup-ceres-placement-selectors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres placement oracle revision");
    Suite(nameof(VerifyCeresRearPlacementSelector), () => VerifyCeresRearPlacementSelector(rom));
    Suite(nameof(VerifyCeresRevealPlacementSelector), () => VerifyCeresRevealPlacementSelector(rom));
    Suite(nameof(VerifyCeresInitialPlacementSelector), () => VerifyCeresInitialPlacementSelector());
    Console.WriteLine("Ceres placement selectors: all 14 original entries, native operands and invalid-index contracts pass.");
    return 0;
}
if (args is ["--lookup-ceres-spawner-schedule"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres spawner oracle revision");
    Suite(nameof(VerifyCeresSpawnerSchedule), () => VerifyCeresSpawnerSchedule(rom));
    Console.WriteLine("Ceres spawner: native list timing, late countdown resets, simultaneous waves and departure boundaries pass.");
    return 0;
}
if (args is ["--lookup-ceres-flight-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres flight program oracle revision");
    Suite(nameof(VerifyCeresFlightPrograms), () => VerifyCeresFlightPrograms(rom));
    Suite(nameof(VerifyCeresFlightFrameCatalog), () => VerifyCeresFlightFrameCatalog(rom));
    Suite(nameof(VerifySpaceColonyCaption), () => VerifySpaceColonyCaption(rom));
    Suite(nameof(VerifyCeresStationParts), () => VerifyCeresStationParts(rom));
    Suite(nameof(VerifyCeresSmallAsteroidParts), () => VerifyCeresSmallAsteroidParts(rom));
    Suite(nameof(VerifyCeresVortexParts), () => VerifyCeresVortexParts(rom));
    Suite(nameof(VerifyCeresReflectedStarParts), () => VerifyCeresReflectedStarParts(rom));
    Suite(nameof(VerifyCeresStarPointParts), () => VerifyCeresStarPointParts(rom));
    Console.WriteLine("Ceres flight programs: five original streams, byte/word boundaries, shared aliases and interpreter loops pass.");
    return 0;
}
if (args is ["--lookup-ceres-backdrop-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres backdrop program oracle revision");
    Suite(nameof(VerifyCeresBackdropPrograms), () => VerifyCeresBackdropPrograms(rom));
    Suite(nameof(VerifyCeresDestructionFrameCatalog), () => VerifyCeresDestructionFrameCatalog(rom));
    Suite(nameof(VerifyPlanetZebesTitleParts), () => VerifyPlanetZebesTitleParts(rom));
    Suite(nameof(VerifyZebesPlanetBandParts), () => VerifyZebesPlanetBandParts(rom));
    Console.WriteLine("Ceres backdrop programs: seven original streams, word boundaries, loops and title callback timing pass.");
    return 0;
}
if (args is ["--lookup-ceres-explosion-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres explosion program oracle revision");
    Suite(nameof(VerifyCeresInitialExplosionProgram), () => VerifyCeresInitialExplosionProgram(rom));
    Suite(nameof(VerifyCeresRepeatingExplosionProgram), () => VerifyCeresRepeatingExplosionProgram(rom));
    Suite(nameof(VerifyCeresFinalWaveProgram), () => VerifyCeresFinalWaveProgram(rom));
    Suite(nameof(VerifyCeresStationBlastProgram), () => VerifyCeresStationBlastProgram(rom));
    Suite(nameof(VerifyCeresStationBlastParts), () => VerifyCeresStationBlastParts(rom));
    Suite(nameof(VerifyCeresLargeBlastParts), () => VerifyCeresLargeBlastParts(rom));
    Console.WriteLine("Ceres explosion programs: four original streams, byte/word bounds and actual interpreter timing/deletion pass.");
    return 0;
}
if (args is ["--lookup-ceres-burst-layout"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres burst layout oracle revision");
    Suite(nameof(VerifyCeresBurstLayout), () => VerifyCeresBurstLayout(rom));
    Console.WriteLine("Ceres burst layout: all original repeating X/Y and final Y words, common delay and bounds pass.");
    return 0;
}
if (args is ["--lookup-ceres-blast-placement"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres blast oracle revision");
    Suite(nameof(VerifyCeresInitialBlastX), () => VerifyCeresInitialBlastX(rom));
    Suite(nameof(VerifyCeresInitialBlastY), () => VerifyCeresInitialBlastY(rom));
    Suite(nameof(VerifyCeresInitialBlastDelay), () => VerifyCeresInitialBlastDelay(rom));
    Suite(nameof(VerifyCeresFinalBlastX), () => VerifyCeresFinalBlastX(rom));
    Suite(nameof(VerifyCeresFinalBlastDelay), () => VerifyCeresFinalBlastDelay(rom));
    Console.WriteLine("Ceres blast placement: five original geometry/delay mappings and invalid boundaries pass.");
    return 0;
}
if (args is ["--lookup-spc-sound-streams"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC stream oracle revision");
    Suite(nameof(VerifySpcSoundStream1), () => VerifySpcSoundStream1(rom));
    Suite(nameof(VerifySpcSoundStream2), () => VerifySpcSoundStream2(rom));
    Suite(nameof(VerifySpcSoundStream3), () => VerifySpcSoundStream3(rom));
    Console.WriteLine("SPC sound streams: all240 original pointers, counts, invalid inputs and runtime command boundaries pass.");
    return 0;
}
if (args is ["--lookup-spc-sound-policies"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC policy oracle revision");
    Suite(nameof(VerifySpcSoundPolicy1), () => VerifySpcSoundPolicy1(rom));
    Suite(nameof(VerifySpcSoundPolicy2), () => VerifySpcSoundPolicy2(rom));
    Suite(nameof(VerifySpcSoundPolicy3), () => VerifySpcSoundPolicy3(rom));
    Console.WriteLine("SPC sound policies: all240 native dispatches, handler writes, preserved fields, voice counts and invalid inputs pass.");
    return 0;
}
if (args is ["--lookup-spc-allocation-addresses"])
{
    Suite(nameof(VerifySpcAllocationAddresses), () => VerifySpcAllocationAddresses());
    Console.WriteLine("SPC allocation layout: all original field bases, channel addresses and rejected indices pass.");
    return 0;
}
if (args is ["--lookup-intro-egg-effect-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro egg-effect oracle revision");
    Suite(nameof(VerifyIntroEggEffectPrograms), () => VerifyIntroEggEffectPrograms(rom));
    Suite(nameof(VerifyIntroEggEffectFrameCatalog), () => VerifyIntroEggEffectFrameCatalog(rom));
    Suite(nameof(VerifyIntroEggEffectParts), () => VerifyIntroEggEffectParts(rom));
    Console.WriteLine("Intro egg effects: all76 bytes, eleven frame records/calculated parts, native OAM, independent edits and boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-rinka-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro Rinka oracle revision");
    Suite(nameof(VerifyIntroRinkaPrograms), () => VerifyIntroRinkaPrograms(rom));
    Suite(nameof(VerifyIntroRinkaFrameCatalog), () => VerifyIntroRinkaFrameCatalog(rom));
    Suite(nameof(VerifyIntroRinkaParts), () => VerifyIntroRinkaParts(rom));
    Console.WriteLine("Intro Rinka: program/catalog, twelve calculated parts, native OAM, independent edits and bounds pass.");
    return 0;
}
if (args is ["--lookup-intro-baby-discovery-instructions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro discovery oracle revision");
    Suite(nameof(VerifyIntroBabyDiscoveryInstructions), () => VerifyIntroBabyDiscoveryInstructions(rom));
    Suite(nameof(VerifyIntroDiscoveryFrameCatalog), () => VerifyIntroDiscoveryFrameCatalog(rom));
    Suite(nameof(VerifyIntroEggRemnantParts), () => VerifyIntroEggRemnantParts(rom));
    Suite(nameof(VerifyIntroEggRockingParts), () => VerifyIntroEggRockingParts(rom));
    Suite(nameof(VerifyIntroEggCrackingParts), () => VerifyIntroEggCrackingParts(rom));
    Suite(nameof(VerifyIntroConfusedBabyParts), () => VerifyIntroConfusedBabyParts(rom));
    Suite(nameof(VerifyCeresLargeAsteroidParts), () => VerifyCeresLargeAsteroidParts(rom));
    Suite(nameof(VerifyIntroBabyDiscoveryInput), () => VerifyIntroBabyDiscoveryInput(rom));
    Suite(nameof(VerifyIntroDiscoveryCollision), () => VerifyIntroDiscoveryCollision(rom));
    Console.WriteLine("Intro discovery programs: all138 bytes, overlapping words, operations and boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-scientist-instructions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro scientist oracle revision");
    Suite(nameof(VerifyIntroScientistInstructions), () => VerifyIntroScientistInstructions(rom));
    Suite(nameof(VerifyIntroScientistFrameCatalog), () => VerifyIntroScientistFrameCatalog(rom));
    Suite(nameof(VerifyIntroScientistParts), () => VerifyIntroScientistParts(rom));
    Console.WriteLine("Intro scientist programs/catalog: all142 bytes, overlapping views, ten original frame records, stable asset names and boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-eye-instructions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro eye oracle revision");
    Suite(nameof(VerifyIntroEyeInstructions), () => VerifyIntroEyeInstructions(rom));
    Console.WriteLine("Intro eye programs: all74 bytes, overlapping words, loop targets and read boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-mother-brain-instructions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain instruction oracle revision");
    Suite(nameof(VerifyIntroMotherBrainInstructions), () => VerifyIntroMotherBrainInstructions(rom));
    Suite(nameof(VerifyIntroMotherBrainFrameCatalog), () => VerifyIntroMotherBrainFrameCatalog(rom));
    Suite(nameof(VerifyIntroMotherBrainParts), () => VerifyIntroMotherBrainParts(rom));
    Console.WriteLine("Mother Brain programs/catalog: all46 bytes,45 word views,three frames and boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-mother-brain-input"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain input oracle revision");
    Suite(nameof(VerifyIntroMotherBrainInputSource), () => VerifyIntroMotherBrainInputSource(rom));
    Console.WriteLine("Mother Brain input: all112 bytes,110 word views and boundaries pass.");
    return 0;
}
if (args is ["--lookup-intro-mother-brain-collision"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain collision oracle revision");
    Suite(nameof(VerifyIntroMotherBrainCollisionSource), () => VerifyIntroMotherBrainCollisionSource(rom));
    Console.WriteLine("Mother Brain collision source: all448 bytes and independent mutable allocations pass.");
    return 0;
}
if (args is ["--lookup-intro-mother-brain-explosion-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro explosion oracle revision");
    Suite(nameof(VerifyIntroMotherBrainExplosionPrograms), () => VerifyIntroMotherBrainExplosionPrograms(rom));
    Suite(nameof(VerifyIntroMotherBrainExplosionFrameCatalog), () => VerifyIntroMotherBrainExplosionFrameCatalog(rom));
    Suite(nameof(VerifyIntroMotherBrainExplosionParts), () => VerifyIntroMotherBrainExplosionParts(rom));
    Console.WriteLine("Intro Mother Brain explosions: both native programs, word views, delete, twelve frame identities and bounds pass.");
    return 0;
}
if (args is ["--lookup-intro-caret-instructions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Intro caret oracle revision");
    Suite(nameof(VerifyIntroCaretInstructions), () => VerifyIntroCaretInstructions(rom));
    Console.WriteLine("Intro caret instructions: all20 bytes, overlapping word views and boundary behavior pass.");
    return 0;
}
if (args is ["--lookup-spc-dsp-publication"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC publication oracle revision");
    Suite(nameof(VerifySpcDspPublication), () => VerifySpcDspPublication(rom));
    Console.WriteLine("SPC DSP publication: native destination/source maps, echo gates and pending-key reset pass.");
    return 0;
}
if (args is ["--lookup-spc-pan-samples"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC pan sample oracle revision");
    Suite(nameof(VerifySpcPanSamples), () => VerifySpcPanSamples(rom));
    Console.WriteLine("SPC pan sample boundary: all21 curve bytes, the adjacent FIR coefficient and rejected indices pass; curve conversion remains pending.");
    return 0;
}
if (args is ["--lookup-pause-wireframe-selection"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause wireframe selection oracle revision");
    Suite(nameof(VerifyPauseWireframeSelection), () => VerifyPauseWireframeSelection(rom));
    Console.WriteLine("Pause wireframe selection: native mask and comparison-table operands and all65536 input words pass.");
    return 0;
}
if (args is ["--lookup-pause-equipment-masks"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause equipment mask oracle revision");
    Suite(nameof(VerifyPauseBeamMasks), () => VerifyPauseBeamMasks(rom));
    Suite(nameof(VerifyPauseSuitMasks), () => VerifyPauseSuitMasks(rom));
    Suite(nameof(VerifyPauseBootMasks), () => VerifyPauseBootMasks(rom));
    Console.WriteLine("Pause equipment masks: all fourteen native flags and category/item rejection contracts pass.");
    return 0;
}
if (args is ["--lookup-pause-selector-anchors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause selector anchor oracle revision");
    Suite(nameof(VerifyPauseSelectorAnchors), () => VerifyPauseSelectorAnchors(rom));
    Console.WriteLine("Pause selector anchors: original names and coordinates, independent axis edits, zero stock storage and bounds pass.");
    return 0;
}
if (args is ["--lookup-pause-selector-compositions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause selector composition oracle revision");
    Suite(nameof(VerifyPauseSelectorCompositions), () => VerifyPauseSelectorCompositions(rom));
    Console.WriteLine("Pause selector compositions: native OAM, all groups/phases, independent edits, cyclic indices and capacity pass.");
    return 0;
}
if (args is ["--lookup-pause-selector-durations"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause selector duration oracle revision");
    Suite(nameof(VerifyPauseSelectorDurations), () => VerifyPauseSelectorDurations(rom));
    Console.WriteLine("Pause selector durations: native program, sparse edits, custom lengths, cyclic indices and initial timer pass.");
    return 0;
}
if (args is ["--lookup-map-arrow-durations"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map arrow duration oracle revision");
    Suite(nameof(VerifyMapArrowDurations), () => VerifyMapArrowDurations(rom));
    Console.WriteLine("Map arrow durations: all original phases, sparse edits, custom lengths, bounds and actual timing pass.");
    return 0;
}
if (args is ["--lookup-map-arrow-cases"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map arrow oracle revision");
    Suite(nameof(VerifyMapArrowCases), () => VerifyMapArrowCases(rom));
    Console.WriteLine("Map arrow cases: original anchors and shapes, independent direction edits, phase selection, membership and bounds pass.");
    return 0;
}
if (args is ["--lookup-pause-button-spans"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause button span oracle revision");
    Suite(nameof(VerifyPauseButtonSpanWords), () => VerifyPauseButtonSpanWords(rom));
    Suite(nameof(VerifyPauseButtonSpanCounts), () => VerifyPauseButtonSpanCounts(rom));
    Console.WriteLine("Pause buttons: all six native row destinations and widths, enumeration order and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-pause-categories"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Pause category oracle revision");
    Suite(nameof(VerifyPauseCategoryCases), () => VerifyPauseCategoryCases(rom));
    Console.WriteLine("Pause categories: all native pointer fields, item counts, dispatched copy lengths, reserve contract and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-map-indicator"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map indicator oracle revision");
    Suite(nameof(VerifyMapIndicatorSprites), () => VerifyMapIndicatorSprites(rom));
    Suite(nameof(VerifyMapIndicatorDelays), () => VerifyMapIndicatorDelays(rom));
    Console.WriteLine("Map indicator: all native sprite/delay words, input bounds, first-tick order and two loop transitions pass.");
    return 0;
}
if (args is ["--lookup-save-marker-coordinates"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Save-marker coordinate oracle revision");
    Suite(nameof(VerifyMapSaveMarkerCoordinates), () => VerifyMapSaveMarkerCoordinates(rom));
    Console.WriteLine("Save-marker coordinates: all 68 native components, independent edits, JSON identity, actual marker binding and bounds pass.");
    return 0;
}
if (args is ["--lookup-map-area-cases"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map-area selector oracle revision");
    Suite(nameof(VerifyFileSelectMapAreaCases), () => VerifyFileSelectMapAreaCases(rom));
    Console.WriteLine("Map-area selection: all six native identity cases and rejected input boundaries pass.");
    return 0;
}
if (args is ["--lookup-save-marker-eligibility"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Save-marker eligibility oracle revision");
    Suite(nameof(VerifySaveMarkerEligibility), () => VerifySaveMarkerEligibility(rom));
    Console.WriteLine("Save-marker eligibility: all 96 native slots, 34 identities, all used masks and input boundaries pass.");
    return 0;
}
if (args is ["--lookup-map-load-anchors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map-load anchor oracle revision");
    Suite(nameof(VerifyMapLoadAnchorX), () => VerifyMapLoadAnchorX(rom));
    Suite(nameof(VerifyMapLoadAnchorY), () => VerifyMapLoadAnchorY(rom));
    Console.WriteLine("Map-load anchors: all 34 original X/Y projections, unused stations and invalid input boundaries pass.");
    return 0;
}
if (args is ["--lookup-options-headings"])
{
    Suite(nameof(VerifyGameOptionsHeadings), () => VerifyGameOptionsHeadings());
    Console.WriteLine("Options headings: all three original sprite selections and anchor pairs, imported records and enum bounds pass.");
    return 0;
}
if (args is ["--game-options-cursor-phases"])
{
    Suite(nameof(VerifyGameOptionsCursorPhases), () => VerifyGameOptionsCursorPhases());
    return 0;
}
if (args is ["--lookup-options-pages"])
{
    Suite(nameof(VerifyGameOptionsPageCases), () => VerifyGameOptionsPageCases());
    Console.WriteLine("Options pages: all five native source pairs, named imported page bytes, descriptions and rejected selectors pass.");
    return 0;
}
if (args is ["--lookup-file-select-slot-fields"])
{
    Suite(nameof(VerifyFileSelectSlotDestinations), () => VerifyFileSelectSlotDestinations());
    Console.WriteLine("File-select slot fields: all 24 original destinations, both slot-label source views, extracted anchors/text and bounds pass.");
    return 0;
}
if (args is ["--lookup-file-select-helmet"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Helmet animation oracle revision");
    Suite(nameof(VerifyFileSelectHelmetAnimation), () => VerifyFileSelectHelmetAnimation(rom));
    Console.WriteLine("File-select helmet: all nine original sprite IDs, native cadence/clamp and bounded selectors pass.");
    return 0;
}
if (args is ["--lookup-game-over-baby-animation"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby animation oracle revision");
    Suite(nameof(VerifyGameOverBabyAnimation), () => VerifyGameOverBabyAnimation(rom));
    Console.WriteLine("Baby animation: original durations, frames, palettes, sound/control handoffs, lazy enumeration and all ushort pointers pass.");
    return 0;
}
if (args is ["--lookup-game-over-baby-colors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Baby colors oracle revision");
    Suite(nameof(VerifyGameOverBabyColors), () => VerifyGameOverBabyColors(rom));
    Console.WriteLine("Baby colors: all 64 original words, CGRAM application, independent channel edits and bounds pass.");
    return 0;
}
if (args is ["--lookup-game-over-text"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Game-over text oracle revision");
    Suite(nameof(VerifyGameOverTextSources), () => VerifyGameOverTextSources(rom));
    Suite(nameof(VerifyGameOverTextDestinations), () => VerifyGameOverTextDestinations(rom));
    Console.WriteLine("Game-over text: all five original sources and destinations, enumeration order and rejected selectors pass.");
    return 0;
}
if (args is ["--lookup-file-select-navigation"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "File-select navigation oracle revision");
    Suite(nameof(VerifyFileSelectMainNavigation), () => VerifyFileSelectMainNavigation());
    Suite(nameof(VerifyFileSelectSourceNavigation), () => VerifyFileSelectSourceNavigation(rom));
    Suite(nameof(VerifyFileSelectDestinationNavigation), () => VerifyFileSelectDestinationNavigation(rom));
    return 0;
}
if (args is ["--lookup-map-window-motion"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Map window motion oracle revision");
    var labels = RetailPresentationFixture().Labels;
    Suite(nameof(VerifyMapWindowOriginX), () => VerifyMapWindowOriginX(rom, labels));
    Suite(nameof(VerifyMapWindowOriginY), () => VerifyMapWindowOriginY(rom, labels));
    Suite(nameof(VerifyMapWindowLeftVelocities), () => VerifyMapWindowLeftVelocities(rom));
    Suite(nameof(VerifyMapWindowRightVelocities), () => VerifyMapWindowRightVelocities(rom));
    Suite(nameof(VerifyMapWindowTopVelocities), () => VerifyMapWindowTopVelocities(rom));
    Suite(nameof(VerifyMapWindowBottomVelocities), () => VerifyMapWindowBottomVelocities(rom));
    Suite(nameof(VerifyMapWindowTimers), () => VerifyMapWindowTimers(rom));
    Console.WriteLine("Map window motion: all 24 original signed16.16 velocities, six timers, 12 named origin fields, independent label edits and rejected areas pass.");
    return 0;
}
if (args is ["--lookup-file-select-geometry"])
{
    Suite(nameof(VerifyFileSelectGeometryLookups), () => VerifyFileSelectGeometryLookups());
    return 0;
}
if (args is ["--lookup-options-toggle-geometry"])
{
    Suite(nameof(VerifyGameOptionsToggleGeometry), () => VerifyGameOptionsToggleGeometry());
    return 0;
}
if (args is ["--game-options-language-palettes"])
{
    Suite(nameof(VerifyGameOptionsLanguagePalettes), () => VerifyGameOptionsLanguagePalettes());
    return 0;
}
if (args is ["--lookup-menu-missile"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Menu missile oracle revision");
    Suite(nameof(VerifyMenuMissileSpritemapIds), () => VerifyMenuMissileSpritemapIds(rom));
    Suite(nameof(VerifyMenuMissileDurations), () => VerifyMenuMissileDurations(rom));
    Console.WriteLine("Menu missile: all four original frame IDs, four duration words, wrap mask and rejected indices pass.");
    return 0;
}
if (args is ["--lookup-options-geometry"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Options geometry oracle revision");
    Suite(nameof(VerifyOptionsPrimaryCursorY), () => VerifyOptionsPrimaryCursorY(rom));
    Suite(nameof(VerifyOptionsControllerCursorY), () => VerifyOptionsControllerCursorY(rom));
    Suite(nameof(VerifyOptionsSpecialCursorY), () => VerifyOptionsSpecialCursorY(rom));
    Suite(nameof(VerifyOptionsLabelDestinations), () => VerifyOptionsLabelDestinations(rom));
    Suite(nameof(VerifyOptionsLabelSources), () => VerifyOptionsLabelSources(rom));
    Console.WriteLine("Options geometry: all 31 native words across five logical mappings and their bounds pass.");
    return 0;
}
if (args is ["--lookup-controller-buttons"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Controller button oracle revision");
    Suite(nameof(VerifyAssignableControllerButtons), () => VerifyAssignableControllerButtons(rom));
    Suite(nameof(VerifyDefaultControllerButtons), () => VerifyDefaultControllerButtons(rom));
    Console.WriteLine("Controller buttons: native choices, inverse domain, swaps, rejection and default action mappings pass.");
    return 0;
}
if (args is ["--hyper-beam-fx-colors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hyper Beam FX oracle revision");
    Suite(nameof(VerifyHyperBeamFxColorArtwork), () => VerifyHyperBeamFxColorArtwork(rom));
    return 0;
}
if (args is ["--lookup-fallback-door-closing"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Fallback closing oracle revision");
    Suite(nameof(VerifyDoorClosingPlmDefinitions), () => VerifyDoorClosingPlmDefinitions(rom, fallbackOnly: true));
    Console.WriteLine("Fallback closing: all twelve original header/list selections, complete byte domain and production spawning pass.");
    return 0;
}
if (args is ["--lookup-resident-door-closing"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Resident closing oracle revision");
    Suite(nameof(VerifyResidentDoorClosingDefinitions), () => VerifyResidentDoorClosingDefinitions(rom));
    Console.WriteLine("Resident door closing: eighteen original header fields, complete selector domain and production redirect pass.");
    return 0;
}
if (args is ["--dynamic-collectible-graphics"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Dynamic collectible oracle revision");
    Suite(nameof(VerifyCompiledDynamicCollectibleGraphics), () => VerifyCompiledDynamicCollectibleGraphics(rom));
    Console.WriteLine("Dynamic collectible graphics: native palette selectors, tiles, pointers, guarded upload and installed artwork pass.");
    return 0;
}
if (args is ["--lookup-tourian-program"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Statue program oracle revision");
    Suite(nameof(VerifyTourianStatueProgramMappings), () => VerifyTourianStatueProgramMappings(rom));
    Suite(nameof(VerifyTourianStatueSpawnOrder), () => VerifyTourianStatueSpawnOrder(rom));
    Suite(nameof(VerifyTourianStatueDescriptorFields), () => VerifyTourianStatueDescriptorFields(rom));
    Suite(nameof(VerifyTourianStatueArtworkSources), () => VerifyTourianStatueArtworkSources(rom));
    Console.WriteLine("Tourian statue programs: all 36 operand positions, 184 mechanics words and full pointer domains pass.");
    return 0;
}
if (args is ["--lookup-tourian-descriptors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Statue descriptor oracle revision");
    Suite(nameof(VerifyTourianStatueDescriptorFields), () => VerifyTourianStatueDescriptorFields(rom));
    Suite(nameof(VerifyTourianStatueArtworkSources), () => VerifyTourianStatueArtworkSources(rom));
    Console.WriteLine("Tourian statue descriptors: all eleven native fields, mechanics aliases, full object domain and artwork sources pass.");
    return 0;
}
if (args is ["--lookup-tourian-artwork"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Statue artwork oracle revision");
    Suite(nameof(VerifyTourianStatueArtworkSources), () => VerifyTourianStatueArtworkSources(rom));
    Console.WriteLine("Tourian statue artwork: all 36 original sources, decoded operand views and complete ushort rejection domains pass.");
    return 0;
}
if (args is ["--lookup-treadmill-mechanics"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Treadmill oracle revision");
    Suite(nameof(VerifyRetailTreadmillMechanics), () => VerifyRetailTreadmillMechanics(rom));
    foreach (var program in OriginalTreadmillPrograms(rom))
        VerifyRetailTreadmillStream(rom, TreadmillDefinition(program.Header).Direction,
            program.Frames.Select(pointer => 0x870000 | ReadVerificationWord(rom, 0x870002 + pointer)).ToArray());
    Console.WriteLine("Treadmill: native header/control fields, calculated cursors, full ushort domains and both guarded loops pass.");
    return 0;
}
if (args is ["--lookup-fx-blends"] )
{
    Suite(nameof(VerifyRoomFxPaletteBlends), () => VerifyRoomFxPaletteBlends());
    return 0;
}
if (args is ["--samus-charge-phases"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Charge phase oracle revision");
    Suite(nameof(VerifySamusChargeColorPhases), () => VerifySamusChargeColorPhases(rom));
    return 0;
}
if (args is ["--samus-hyper-beam-colors"])
{
    Suite(nameof(VerifySamusHyperBeamColors), () => VerifySamusHyperBeamColors());
    return 0;
}
if (args is ["--lookup-full-body-colors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Full-body color oracle revision");
    Suite(nameof(VerifyFullBodyPaletteColorData), () => VerifyFullBodyPaletteColorData(rom, SamusFullBodyCycleColorExtractor.Extract(rom)));
    Console.WriteLine("Full-body palettes:48 native identities, all768 colors, complete pointer domain, edits and CGRAM copies pass.");
    return 0;
}
if (args is ["--normal-suit-catalog-boundary"])
{
    Suite(nameof(VerifyNormalSuitCatalogBoundary), () => VerifyNormalSuitCatalogBoundary());
    return 0;
}
if (args is ["--lookup-loading-layout"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Loading layout oracle revision");
    Suite(nameof(VerifySamusLoadingSuitPaletteFxProgramMechanicsDefinitions), () => VerifySamusLoadingSuitPaletteFxProgramMechanicsDefinitions(rom));
    Console.WriteLine("Suit loading: original decoded group/frame layout, complete pointer ownership and guarded programs pass.");
    return 0;
}
if (args is ["--lookup-loading-colors"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Loading colors oracle revision");
    byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
    Suite(nameof(VerifyExtractedSamusLoadingPaletteFxPresentation), () => VerifyExtractedSamusLoadingPaletteFxPresentation(rom, RoomPaletteFxPresentation.Load(new MemoryStream(json))));
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
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Heat color oracle revision");
    byte[] json = RoomPaletteFxPresentationExtractor.Extract(rom);
    var presentation = RoomPaletteFxPresentation.Load(new MemoryStream(json));
    Suite(nameof(VerifyExtractedSamusHeatPaletteFxPresentation), () => VerifyExtractedSamusHeatPaletteFxPresentation(rom, presentation));
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
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Heat selector oracle revision");
    Suite(nameof(VerifyPaletteFxHeatInstructionListDefinitions), () => VerifyPaletteFxHeatInstructionListDefinitions(rom));
    Suite(nameof(VerifyPaletteFxHeatProgramMechanicsDefinitions), () => VerifyPaletteFxHeatProgramMechanicsDefinitions(rom));
    Console.WriteLine("Heat selectors: all48 native words, program layout, bounds and full equipment selection pass.");
    return 0;
}
if (args is ["--lookup-fx-validation"])
{
    Suite(nameof(VerifyFxValidationBoundaries), () => VerifyFxValidationBoundaries());
    Console.WriteLine("FX validation: native dispatcher slots, every cartridge byte and every host blend ushort pass.");
    return 0;
}
if (args is ["--lookup-ceres-haze"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres haze oracle revision");
    Suite(nameof(VerifyCeresHazeNativeRamp), () => VerifyCeresHazeNativeRamp(rom));
    Suite(nameof(VerifyCeresHazeTintScaling), () => VerifyCeresHazeTintScaling(rom));
    Suite(nameof(VerifyCeresHazePhaseControl), () => VerifyCeresHazePhaseControl(rom));
    Console.WriteLine("Ceres haze: original HDMA bands, all17 native counters, both channels, captured/software views and RGB5 tint scaling pass.");
    return 0;
}
if (args is ["--lookup-animated-frames"])
{
    Suite(nameof(VerifyRoomFxAnimatedTileMechanicsDefinitions), () => VerifyRoomFxAnimatedTileMechanicsDefinitions());
    return 0;
}
if (args is ["--area-animated-tile-definitions"])
{
    Suite(nameof(VerifyAreaAnimatedTileObjectDefinitions), () => VerifyAreaAnimatedTileObjectDefinitions());
    return 0;
}
if (args is ["--lookup-palette-fx-areas"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Palette-FX area oracle revision");
    Suite(nameof(VerifyPaletteFxAreaListPointers), () => VerifyPaletteFxAreaListPointers(rom));
    Suite(nameof(VerifyPaletteFxAreaSelections), () => VerifyPaletteFxAreaSelections(rom));
    Console.WriteLine("Palette-FX areas: eight original list identities, all64 selections and rejection domains pass.");
    return 0;
}
if (args is ["--lookup-palette-fx-dispatch"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Palette-FX dispatch oracle revision");
    Suite(nameof(VerifyPaletteFxDispatch), () => VerifyPaletteFxDispatch(rom));
    Console.WriteLine("Palette-FX dispatch: all 63 native setup/list pairs, full identity domain and guarded spawning pass.");
    return 0;
}
if (args is ["--lookup-sky-chunk-pointers"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Sky chunk oracle revision");
    Suite(nameof(VerifyLandSkyChunkPointers), () => VerifyLandSkyChunkPointers(rom));
    Suite(nameof(VerifyOceanSkyChunkPointers), () => VerifyOceanSkyChunkPointers(rom));
    Console.WriteLine("Sky chunks: both native mappings, compatibility reads, rejections and all ushort camera inputs pass.");
    return 0;
}
if (args is ["--lookup-sky-sections"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Sky sections oracle revision");
    Suite(nameof(VerifySkySectionTopPositions), () => VerifySkySectionTopPositions(rom));
    Suite(nameof(VerifySkySectionSubspeeds), () => VerifySkySectionSubspeeds(rom));
    Suite(nameof(VerifySkySectionSpeeds), () => VerifySkySectionSpeeds(rom));
    Suite(nameof(VerifySkySectionDataSlots), () => VerifySkySectionDataSlots(rom));
    Suite(nameof(VerifyScrollingSkyState), () => VerifyScrollingSkyState());
    Console.WriteLine("Sky sections: four native fields, bounds, complete world-Y projection and existing integration pass.");
    return 0;
}
if (args is ["--lookup-quake-suppression"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Quake suppression oracle revision");
    Suite(nameof(VerifyQuakeSoundSuppression), () => VerifyQuakeSoundSuppression(rom));
    Console.WriteLine("Quake suppression: native branches, all 65536 room identities and sound consumer pass.");
    return 0;
}
if (args is ["--lookup-fx-tilemap-sources"])
{
    Suite(nameof(VerifyRoomFxLayer3Tilemaps), () => VerifyRoomFxLayer3Tilemaps());
    return 0;
}
if (args is ["--lookup-quake-sound-selection"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Quake sound oracle revision");
    Suite(nameof(VerifyQuakeSoundSelection), () => VerifyQuakeSoundSelection(rom));
    Console.WriteLine("Quake sound selection: all eight original identities, loop marker and production queue requests pass.");
    return 0;
}
if (args is ["--lookup-liquid-wave"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Liquid wave oracle revision");
    Suite(nameof(VerifyMirroredLiquidWave), () => VerifyMirroredLiquidWave(rom));
    Suite(nameof(VerifyHorizontalHeatWave), () => VerifyHorizontalHeatWave(rom));
    Console.WriteLine("Liquid waves: original mirrored and horizontal pulse samples, bounds, all phases and projection consumers pass.");
    return 0;
}
if (args is ["--lookup-rain-velocity"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Rain velocity oracle revision");
    Suite(nameof(VerifyRainHorizontalVelocity), () => VerifyRainHorizontalVelocity(rom));
    Console.WriteLine("Rain velocity: four signed 8.8 values, all 65536 RNG selections and invalid bounds pass.");
    return 0;
}
if (args is ["--room-fx-record-definitions"])
{
    string roomFxDefinitionsRom = RepositoryRomPath;
    Suite(nameof(VerifyRoomFxRecordDefinitions), () => VerifyRoomFxRecordDefinitions(roomFxDefinitionsRom));
    return 0;
}

if (args is ["--load-station-definitions"])
{
    Suite(nameof(VerifyCompiledLoadStationDefinitions), () => VerifyCompiledLoadStationDefinitions());
    return 0;
}

if (args is ["--door-definitions"])
{
    Suite(nameof(VerifyCompiledDoorDefinitions), () => VerifyCompiledDoorDefinitions());
    return 0;
}

if (args is ["--lookup-retail-door-lists"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Door-list oracle revision");
    Suite(nameof(VerifyRetailDoorListMapping), () => VerifyRetailDoorListMapping(rom));
    Suite(nameof(VerifyCompiledDoorListCollision), () => VerifyCompiledDoorListCollision());
    return 0;
}
if (args is ["--room-header-definitions"])
{
    Suite(nameof(VerifyCompiledRoomHeaderDefinitions), () => VerifyCompiledRoomHeaderDefinitions());
    return 0;
}

if (args is ["--room-state-definitions"])
{
    Suite(nameof(VerifyCompiledRoomStateSelectionDefinitions), () => VerifyCompiledRoomStateSelectionDefinitions());
    return 0;
}
if (args is ["--lookup-room-state-settings"])
{
    Suite(nameof(VerifyCompiledRoomStateDefinitions), () => VerifyCompiledRoomStateDefinitions());
    return 0;
}
if (args is ["--lookup-kraid-arm-hitbox-lists"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Kraid arm list oracle revision");
    Suite(nameof(VerifyKraidArmHitboxListSelection), () => VerifyKraidArmHitboxListSelection(rom));
    Console.WriteLine("Kraid arm hitbox selection: all 16 native lists, 24 ordered rectangles, full ushort rejection domain and slice bounds pass.");
    return 0;
}
if (args is ["--room-plm-populations"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Retail population oracle revision");
    Suite(nameof(VerifyRetailPopulationMappings), () => VerifyRetailPopulationMappings(rom));
    Suite(nameof(VerifyCompiledRoomPlmHeaderLoad), () => VerifyCompiledRoomPlmHeaderLoad(rom));
    Suite(nameof(VerifyPlmPopulationInputBoundary), () => VerifyPlmPopulationInputBoundary());
    Console.WriteLine("Retail populations: all 284 identities, 941 ordered placements and native terminators, sequential setup and historical state schemas pass.");
    return 0;
}
if (args is ["--lookup-scroll-programs"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Scroll program oracle revision");
    Suite(nameof(VerifyCompiledRoomScrollPrograms), () => VerifyCompiledRoomScrollPrograms(rom));
    Suite(nameof(VerifyRoomScrollPlms), () => VerifyRoomScrollPlms());
    Console.WriteLine("Scroll programs: all 173 identities, 285 ordered writes and terminators, guarded retail execution and constructed-room behavior pass.");
    return 0;
}
if (args is ["--eye-door-plm-draws"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Eye door oracle revision");
    Suite(nameof(VerifyEyeDoorPlmDrawDefinitions), () => VerifyEyeDoorPlmDrawDefinitions(rom));
    return 0;
}
if (args is ["--collectible-visuals"])
{
    Suite(nameof(VerifyCollectibleVisuals), () => VerifyCollectibleVisuals());
    return 0;
}
if (args is ["--station-animation-programs"])
{
    Suite(nameof(VerifyStationAnimationProgramDefinitions), () => VerifyStationAnimationProgramDefinitions());
    return 0;
}
if (args is ["--station-access-plm-definitions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Station access oracle revision");
    Suite(nameof(VerifyStationAccessPlmDefinitions), () => VerifyStationAccessPlmDefinitions(rom));
    return 0;
}
if (args is ["--elevator-platform-visuals"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevator platform oracle revision");
    Suite(nameof(VerifyElevatorPlatformPlmDefinitions), () => VerifyElevatorPlatformPlmDefinitions(rom));
    Suite(nameof(VerifyElevatorPlatformVisuals), () => VerifyElevatorPlatformVisuals());
    return 0;
}
if (args is ["--chozo-plm-definitions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Chozo program oracle revision");
    Suite(nameof(VerifyChozoStatuePlmDefinitions), () => VerifyChozoStatuePlmDefinitions(rom));
    return 0;
}
if (args is ["--draygon-cannon-plm-program"])
{
    Suite(nameof(VerifyDraygonCannonPlmProgram), () => VerifyDraygonCannonPlmProgram());
    return 0;
}
if (args is ["--draygon-cannon-plms"])
{
    Suite(nameof(VerifyDraygonCannonPlms), () => VerifyDraygonCannonPlms());
    return 0;
}
if (args is ["--mother-brain-glass-instruction-mechanics"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Glass projectile oracle revision");
    Suite(nameof(VerifyMotherBrainGlassInstructionProgramDefinitions), () => VerifyMotherBrainGlassInstructionProgramDefinitions(rom));
    return 0;
}
if (args is ["--mother-brain-glass-shard-definitions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Glass shard oracle revision");
    Suite(nameof(VerifyMotherBrainGlassShardDefinitions), () => VerifyMotherBrainGlassShardDefinitions(rom));
    return 0;
}
if (args is ["--mother-brain-glass-plm-draws"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Glass oracle revision");
    Suite(nameof(VerifyMotherBrainGlassPlmDrawDefinitions), () => VerifyMotherBrainGlassPlmDrawDefinitions(rom));
    return 0;
}
if (args is ["--noob-tube-plm-draws"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tube draw oracle revision");
    Suite(nameof(VerifyNoobTubePlmDrawDefinitions), () => VerifyNoobTubePlmDrawDefinitions(rom));
    return 0;
}
if (args is ["--noob-tube-plm-program"])
{
    Suite(nameof(VerifyNoobTubePlm), () => VerifyNoobTubePlm());
    return 0;
}
if (args is ["--colored-door-plm-draws"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Colored door oracle revision");
    Suite(nameof(VerifyColoredDoorPlmDrawDefinitions), () => VerifyColoredDoorPlmDrawDefinitions(rom));
    return 0;
}
if (args is ["--blue-door-plm-draws"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Blue door oracle revision");
    Suite(nameof(VerifyBlueDoorPlmDrawDefinitions), () => VerifyBlueDoorPlmDrawDefinitions(rom));
    return 0;
}
if (args is ["--grey-door-plm-draws"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Grey door oracle revision");
    Suite(nameof(VerifyGreyDoorPlmDrawDefinitions), () => VerifyGreyDoorPlmDrawDefinitions(rom));
    Console.WriteLine("Grey door definitions and guarded lifecycle pass, including all Bomb Torizo program fields.");
    return 0;
}
if (args is ["--bomb-torizo-hand-plm-program"])
{
    Suite(nameof(VerifyBombTorizoHandPlm), () => VerifyBombTorizoHandPlm());
    return 0;
}
if (args is ["--bomb-torizo-hand-artwork"])
{
    Suite(nameof(VerifyBombTorizoHandArtwork), () => VerifyBombTorizoHandArtwork());
    return 0;
}
if (args is ["--lookup-escape-gate"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Escape gate oracle revision");
    Suite(nameof(VerifyMotherBrainEscapeGateCompiledDefinitions), () => VerifyMotherBrainEscapeGateCompiledDefinitions(rom));
    Suite(nameof(VerifyMotherBrainEscapeRoomGate), () => VerifyMotherBrainEscapeRoomGate(new TestAddressSpace()));
    Suite(nameof(VerifyEscapeGateVisuals), () => VerifyEscapeGateVisuals(rom));
    Console.WriteLine("Escape gate: all original draw/program fields and complete domains pass; production door handoff and custom artwork pass.");
    return 0;
}
if (args is ["--lookup-retail-plm-headers"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Retail header oracle revision");
    Suite(nameof(VerifyRetailPlmHeaderSetups), () => VerifyRetailPlmHeaderSetups(rom));
    Suite(nameof(VerifyRetailPlmHeaderInstructions), () => VerifyRetailPlmHeaderInstructions(rom));
    Suite(nameof(VerifyCompiledRoomPlmHeaderLoad), () => VerifyCompiledRoomPlmHeaderLoad(rom));
    Console.WriteLine("Retail PLM headers: all seventy setup/list cases and full input domain match ROM; guarded population load passes.");
    return 0;
}
if (args is ["--downward-gate-definitions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Downward gate oracle revision");
    Suite(nameof(VerifyDownwardGateShotBlockDefinitions), () => VerifyDownwardGateShotBlockDefinitions(rom));
    return 0;
}
if (args is ["--speed-booster-escape-definitions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Speed escape stage oracle revision");
    Suite(nameof(VerifySpeedBoosterEscapeStageDefinitions), () => VerifySpeedBoosterEscapeStageDefinitions(rom));
    return 0;
}
if (args is ["--lookup-speed-escape-program"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Speed escape oracle revision");
    Suite(nameof(VerifySpeedEscapeProgramControls), () => VerifySpeedEscapeProgramControls(rom));
    Suite(nameof(VerifySpeedEscapeProgramCallbacks), () => VerifySpeedEscapeProgramCallbacks(rom));
    Suite(nameof(VerifySpeedBoosterEscapePlm), () => VerifySpeedBoosterEscapePlm(new TestAddressSpace()));
    Console.WriteLine("Speed escape program: all native fields and complete address domain pass; production handoffs and lava completion pass.");
    return 0;
}
if (args is ["--maridia-elevatube-plm"])
{
    Suite(nameof(VerifyMaridiaElevatubePlm), () => VerifyMaridiaElevatubePlm());
    return 0;
}
if (args is ["--maridia-elevatube-visuals"])
{
    Suite(nameof(VerifyMaridiaElevatubeVisuals), () => VerifyMaridiaElevatubeVisuals());
    return 0;
}
if (args is ["--mother-brain-fake-death-visuals"])
{
    Suite(nameof(VerifyMotherBrainFakeDeathVisuals), () => VerifyMotherBrainFakeDeathVisuals());
    return 0;
}
if (args is ["--mother-brain-fake-death-plms"])
{
    Suite(nameof(VerifyCompiledMotherBrainFakeDeathPlms), () => VerifyCompiledMotherBrainFakeDeathPlms());
    return 0;
}
if (args is ["--crocomire-arena-plms"])
{
    Suite(nameof(VerifyCompiledCrocomireArenaPlms), () => VerifyCompiledCrocomireArenaPlms());
    return 0;
}
if (args is ["--crocomire-arena-visuals"])
{
    Suite(nameof(VerifyCrocomireArenaVisuals), () => VerifyCrocomireArenaVisuals());
    return 0;
}
if (args is ["--tourian-access-plms"])
{
    Suite(nameof(VerifyCompiledTourianAccessPlmPrograms), () => VerifyCompiledTourianAccessPlmPrograms());
    return 0;
}

if (args is ["--tourian-access-visuals"])
{
    Suite(nameof(VerifyTourianAccessVisuals), () => VerifyTourianAccessVisuals());
    return 0;
}

if (args is ["--spore-spawn-ceiling-visuals"])
{
    Suite(nameof(VerifySporeSpawnCeilingVisuals), () => VerifySporeSpawnCeilingVisuals());
    return 0;
}
if (args is ["--spore-spawn-ceiling-plms"])
{
    Suite(nameof(VerifyCompiledSporeSpawnCeilingPlms), () => VerifyCompiledSporeSpawnCeilingPlms());
    return 0;
}
if (args is ["--samus-eater-plm-definitions"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "plant program NTSC J/U oracle");
    Suite(nameof(VerifySamusEaterPlmDefinitions), () => VerifySamusEaterPlmDefinitions(rom));
    return 0;
}
if (args is ["--samus-eater-visuals"])
{
    Suite(nameof(VerifySamusEaterVisuals), () => VerifySamusEaterVisuals());
    return 0;
}
if (args is ["--lookup-special-air"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Special-air NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyQuicksandDefinitions), () => VerifyQuicksandDefinitions(oracle));
    Suite(nameof(VerifyQuicksand), () => VerifyQuicksand());
    return 0;
}
if (args is ["--lookup-speed-blocks"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Speed blocks NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyCompiledSpeedBoosterPlmPrograms), () => VerifyCompiledSpeedBoosterPlmPrograms());
    return 0;
}
if (args is ["--lookup-grapple-blocks"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Grapple blocks NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyGrappleBlockPrograms), () => VerifyGrappleBlockPrograms());
    return 0;
}
if (args is ["--lookup-block-reaction-selectors"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Block reactions NTSC J/U v1.0 oracle");
    Suite(nameof(VerifySharedBreakAnimationSelection), () => VerifySharedBreakAnimationSelection(oracle));
    Suite(nameof(VerifyBombedRevealPhysicalDrawMapping), () => VerifyBombedRevealPhysicalDrawMapping(oracle));
    Suite(nameof(VerifyBombedRevealControlMapping), () => VerifyBombedRevealControlMapping(oracle));
    Suite(nameof(VerifyBombedRevealDrawMapping), () => VerifyBombedRevealDrawMapping(oracle));
    Suite(nameof(VerifyContactCrumbleHeaderSelection), () => VerifyContactCrumbleHeaderSelection(oracle));
    Suite(nameof(VerifyCollisionBombInstructionSelection), () => VerifyCollisionBombInstructionSelection(oracle));
    Suite(nameof(VerifyReactionBombInstructionSelection), () => VerifyReactionBombInstructionSelection(oracle));
    Suite(nameof(VerifyCrumbleRevealInstructionSelection), () => VerifyCrumbleRevealInstructionSelection(oracle));
    Suite(nameof(VerifyContactCrumbleInstructionSelection), () => VerifyContactCrumbleInstructionSelection(oracle));
    Suite(nameof(VerifyBombSpecialInstructionSelection), () => VerifyBombSpecialInstructionSelection(oracle));
    Suite(nameof(VerifyBombBlockPrograms), () => VerifyBombBlockPrograms());
    Suite(nameof(VerifyContactCrumblePrograms), () => VerifyContactCrumblePrograms());
    Console.WriteLine("Block reaction selectors: six complete native mappings and bomb/crumble production fixtures pass.");
    return 0;
}
if (args is ["--lookup-phase-two-rear-leg"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Phase-two rear-leg NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyStream3PhaseTwoRearLeg), () => VerifyStream3PhaseTwoRearLeg(oracle));
    Console.WriteLine("Phase-two rear leg: 15 native colors/destinations and 45 independent RGB edits pass.");
    return 0;
}
if (args is ["--lookup-auxiliary-palettes"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Auxiliary palette NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyStream3AuxiliaryPalettes), () => VerifyStream3AuxiliaryPalettes(oracle));
    Console.WriteLine("Auxiliary palettes: 393 native colors, 1179 independent edits, identities and bounds pass.");
    return 0;
}
if (args is ["--lookup-stream5-door-quake-decoding"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Door quake NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyLookupStream5DoorQuakeDecoding), () => VerifyLookupStream5DoorQuakeDecoding(oracle));
    return 0;
}
if (args is ["--lookup-arm-cannon-selectors"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Arm cannon NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyLookupStream1ArmCannonTileSources), () => VerifyLookupStream1ArmCannonTileSources(oracle));
    Console.WriteLine("Arm cannon selectors: native directions/frames, full reverse domain, independent edits/hash, DMA pixels and boundaries pass.");
    return 0;
}
if (args is ["--lookup-stream5-ceres-flight-palette"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Ceres flight NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyLookupStream5CeresFlightPalette), () => VerifyLookupStream5CeresFlightPalette(oracle));
    return 0;
}
if (args is ["--lookup-stream-4-oum-layout"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Oum NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyLookupStream4OumListLayout), () => VerifyLookupStream4OumListLayout(oracle));
    return 0;
}
if (args is ["--lookup-stream-4-elevatube"])
{
    Suite(nameof(VerifyMaridiaElevatubePlm), () => VerifyMaridiaElevatubePlm());
    Suite(nameof(VerifyMaridiaElevatubeVisuals), () => VerifyMaridiaElevatubeVisuals());
    return 0;
}
if (args is ["--lookup-stream-4-breakup-order"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Ridley breakup NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyRidleyExplosionDefinitions), () => VerifyRidleyExplosionDefinitions(oracle));
    Suite(nameof(VerifyLookupStream4BreakupOrder), () => VerifyLookupStream4BreakupOrder(oracle));
    return 0;
}
if (args is ["--lookup-stream-3"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Stream 3 NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyLookupStream3), () => VerifyLookupStream3(oracle));
    return 0;
}
if (args is ["--lookup-shot-block-draws"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shot-block NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyShotBlockPlmPrograms), () => VerifyShotBlockPlmPrograms());
    Suite(nameof(VerifyCompiledBotwoonWallPlms), () => VerifyCompiledBotwoonWallPlms());
    return 0;
}
if (args is ["--lookup-botwoon-wall"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon wall NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyCompiledBotwoonWallPlms), () => VerifyCompiledBotwoonWallPlms());
    Suite(nameof(VerifyBotwoonWallStockMapping), () => VerifyBotwoonWallStockMapping(oracle));
    Suite(nameof(VerifyBotwoonWallVisualIdMapping), () => VerifyBotwoonWallVisualIdMapping());
    Suite(nameof(VerifyBotwoonWallVisualSeparation), () => VerifyBotwoonWallVisualSeparation(new SuperMetroid.Core.Rooms.RoomPlmBotwoonWallVisualCatalog(
        [new("clear-wall", [0xff,0xff,0xff,0x58,0xff,0xff,0xff,0xff,0xff])])));
    return 0;
}
if (args is ["--lookup-botwoon-hole-bounds"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon hole NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyBotwoonHoleRightBounds), () => VerifyBotwoonHoleRightBounds(oracle));
    Suite(nameof(VerifyBotwoonHoleBottomBounds), () => VerifyBotwoonHoleBottomBounds(oracle));
    Console.WriteLine("Botwoon hole bounds: all native right/bottom edges and production inclusion/exclusion checks pass.");
    return 0;
}
if (args is ["--lookup-botwoon-path-descriptors"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon navigation NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyBotwoonPathDescriptorMappings), () => VerifyBotwoonPathDescriptorMappings(oracle));
    return 0;
}
if (args is ["--lookup-botwoon-projectile-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon projectile NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyBotwoonProjectileInstructionProgramDefinitions), () => VerifyBotwoonProjectileInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-botwoon-program-selection"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon selection NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyBotwoonInstructionDefinitions), () => VerifyBotwoonInstructionDefinitions(oracle));
    return 0;
}
if (args is ["--lookup-botwoon-head-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Botwoon NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyBotwoonInstructionProgramDefinitions), () => VerifyBotwoonInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-torizo-statue-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Bomb Torizo statue NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyBombTorizoStatueFragmentDefinitions), () => VerifyBombTorizoStatueFragmentDefinitions(oracle));
    Suite(nameof(VerifyBombTorizoStatueInstructionProgramDefinitions), () => VerifyBombTorizoStatueInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-torizo-dormant-and-drool"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Bomb Torizo NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyBombTorizoDormantDefinitions), () => VerifyBombTorizoDormantDefinitions(oracle));
    Suite(nameof(VerifyBombTorizoDroolInstructionProgramDefinitions), () => VerifyBombTorizoDroolInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-face-block-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Face block NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions), () => VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-beetom-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Beetom NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-platform-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Platform NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyPlatformInstructionProgramDefinitions), () => VerifyPlatformInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-elevator-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Elevator NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyElevatorInputDefinitions), () => VerifyElevatorInputDefinitions(oracle));
    Suite(nameof(VerifyElevatorInstructionProgramDefinitions), () => VerifyElevatorInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-growing-shutter-definitions"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Growing shutter NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyCompiledGrowingShutters), () => VerifyCompiledGrowingShutters(oracle));
    return 0;
}
if (args is ["--lookup-shutter-visuals"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shutter NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyShutterVisualPointerMapping), () => VerifyShutterVisualPointerMapping(oracle));
    Suite(nameof(VerifyGrowingShutterInstructionProgramDefinitions), () => VerifyGrowingShutterInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyHorizontalShutterInstructionProgramDefinitions), () => VerifyHorizontalShutterInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyVerticalShutterInstructionProgramDefinitions), () => VerifyVerticalShutterInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-shutter-pose-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shutter NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyGrowingShutterInstructionProgramDefinitions), () => VerifyGrowingShutterInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyHorizontalShutterInstructionProgramDefinitions), () => VerifyHorizontalShutterInstructionProgramDefinitions(oracle));
    return 0;
}
if (args is ["--lookup-shutter-initial-functions"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Shutter NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyVerticalShutterInitialFunctionSelection), () => VerifyVerticalShutterInitialFunctionSelection(oracle));
    Suite(nameof(VerifyHorizontalShutterInitialFunctionSelection), () => VerifyHorizontalShutterInitialFunctionSelection(oracle));
    Console.WriteLine("Shutter initial function selection: both native five-entry mappings, state semantics and invalid offsets pass.");
    return 0;
}
if (args is ["--lookup-vertical-shutter-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Vertical shutter NTSC J/U v1.0 oracle");
    Suite(nameof(VerifyVerticalShutterInstructionProgramDefinitions), () => VerifyVerticalShutterInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-alcoon-fireball-programs"])
{
    Suite(nameof(VerifyAlcoonFireballInstructionProgramDefinitions), () => VerifyAlcoonFireballInstructionProgramDefinitions());
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-fune-namihe-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)),
        "Fune/Namihe actor oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyFuneNamiheDefinitions), () => VerifyFuneNamiheDefinitions(oracle));
    Suite(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-fune-namihe-fireball-programs"])
{
    var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)),
        "Fune/Namihe fireball oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyFuneNamiheFireballInstructionProgramDefinitions), () => VerifyFuneNamiheFireballInstructionProgramDefinitions(oracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-enemy-fireball-launches"])
{
    var launchOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(launchOracle.Rom)),
        "Enemy fireball launch oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCompiledEnemyFireballLaunches), () => VerifyCompiledEnemyFireballLaunches(launchOracle));
    return 0;
}
if (args is ["--lookup-boyon-programs"])
{
    var boyonOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(boyonOracle.Rom)),
        "Boyon oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyBoyonInstructionProgramDefinitions), () => VerifyBoyonInstructionProgramDefinitions(boyonOracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-boulder-programs"])
{
    var boulderOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(boulderOracle.Rom)),
        "Boulder oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyBoulderInstructionProgramDefinitions), () => VerifyBoulderInstructionProgramDefinitions(boulderOracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-alcoon-programs"])
{
    var alcoonOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(alcoonOracle.Rom)),
        "Alcoon oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyAlcoonMechanicsMapping), () => VerifyAlcoonMechanicsMapping(alcoonOracle));
    Suite(nameof(VerifyAlcoonPresentationAddressMapping), () => VerifyAlcoonPresentationAddressMapping());
    Suite(nameof(VerifyAlcoonVisualSelectorMapping), () => VerifyAlcoonVisualSelectorMapping(alcoonOracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    Console.WriteLine("Alcoon programs: all 68 native words, 44 visual positions, full ownership domains and bounds pass.");
    return 0;
}
if (args is ["--lookup-atomic-programs"])
{
    var atomicOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(atomicOracle.Rom)),
        "Atomic oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyAtomicMovementDefinitions), () => VerifyAtomicMovementDefinitions(atomicOracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--lookup-draygon-intro-commands"])
{
    var danceOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(danceOracle.Rom)),
        "Draygon intro oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyDraygonIntroMovementDefinitions), () => VerifyDraygonIntroMovementDefinitions(danceOracle));
    Console.WriteLine("Draygon intro: explicit delete selection and all 1104 original X/Y results and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-draws"])
{
    var drawOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(drawOracle.Rom)),
        "Kraid draw oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidDrawAddresses), () => VerifyKraidDrawAddresses());
    Suite(nameof(VerifyKraidDrawOwnerClassification), () => VerifyKraidDrawOwnerClassification(drawOracle));
    Suite(nameof(VerifyKraidDrawShapes), () => VerifyKraidDrawShapes(drawOracle));
    Suite(nameof(VerifyKraidDrawWords), () => VerifyKraidDrawWords(drawOracle));
    Suite(nameof(VerifyKraidDrawVisualIds), () => VerifyKraidDrawVisualIds());
    Suite(nameof(VerifyKraidRoomVisualSelection), () => VerifyKraidRoomVisualSelection());
    Console.WriteLine("Kraid draw definitions: ten identities, eight owners, native shapes, all 45 words, visual IDs and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-room-programs"])
{
    var roomProgramOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(roomProgramOracle.Rom)),
        "Kraid room program oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidRoomProgramMapping), () => VerifyKraidRoomProgramMapping(roomProgramOracle));
    return 0;
}
if (args is ["--lookup-kraid-room-visual-selection"])
{
    Suite(nameof(VerifyKraidRoomVisualSelection), () => VerifyKraidRoomVisualSelection());
    return 0;
}
if (args is ["--lookup-fake-kraid-spike-rows"])
{
    var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Fake Kraid spike-row oracle revision");
    Suite(nameof(VerifyFakeKraidSpikeRowSelection), () => VerifyFakeKraidSpikeRowSelection(rom));
    Console.WriteLine("Fake Kraid spike rows: all three original signed launch positions and invalid selectors pass.");
    return 0;
}
if (args is ["--lookup-fake-kraid-spit-velocities"])
{
    var velocityOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(velocityOracle.Rom)),
        "Fake Kraid velocity oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyFakeKraidSpitHorizontalVelocity), () => VerifyFakeKraidSpitHorizontalVelocity(velocityOracle));
    Suite(nameof(VerifyFakeKraidSpitVerticalVelocity), () => VerifyFakeKraidSpitVerticalVelocity(velocityOracle));
    Console.WriteLine("Fake Kraid spit: both velocity fields match all four native launches; invalid ordinals pass.");
    return 0;
}
if (args is ["--lookup-fake-kraid-projectile-programs"])
{
    var projectileProgramOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(projectileProgramOracle.Rom)),
        "Fake Kraid projectile program oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyFakeKraidProjectileMechanicsMapping), () => VerifyFakeKraidProjectileMechanicsMapping(projectileProgramOracle));
    Suite(nameof(VerifyFakeKraidProjectilePresentationAddresses), () => VerifyFakeKraidProjectilePresentationAddresses());
    Console.WriteLine("Fake Kraid projectile programs: six native control words, complete bank ownership, three visual operand positions and bounds pass.");
    return 0;
}
if (args is ["--lookup-fake-kraid-programs"])
{
    var programOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(programOracle.Rom)),
        "Fake Kraid program oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyFakeKraidMechanicsMapping), () => VerifyFakeKraidMechanicsMapping(programOracle));
    Suite(nameof(VerifyFakeKraidPresentationAddresses), () => VerifyFakeKraidPresentationAddresses());
    Console.WriteLine("Fake Kraid programs: 48 native control words, complete bank ownership, 24 visual operand positions and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-visual-selectors"])
{
    var visualOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(visualOracle.Rom)),
        "Kraid visual oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidNailVisualSelectors), () => VerifyKraidNailVisualSelectors(visualOracle));
    Suite(nameof(VerifyFakeKraidVisualSelectors), () => VerifyFakeKraidVisualSelectors(visualOracle));
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    Console.WriteLine("Kraid visual selectors: eight nail operands for both actors,24 Fake Kraid operands, holes and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-spit-speeds"])
{
    var spitOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(spitOracle.Rom)),
        "Kraid spit oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidRockLaunchDefinitions), () => VerifyKraidRockLaunchDefinitions(spitOracle, definitionsOnly: true));
    return 0;
}
if (args is ["--lookup-kraid-palette-definitions"])
{
    var paletteOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(paletteOracle.Rom)),
        "Kraid palette oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidPaletteSourceAddresses), () => VerifyKraidPaletteSourceAddresses(paletteOracle));
    Suite(nameof(VerifyKraidPaletteSourceLengths), () => VerifyKraidPaletteSourceLengths());
    Console.WriteLine("Kraid palette definitions: five native source operands, complete extents and invalid enums pass.");
    return 0;
}
if (args is ["--lookup-kraid-color-sources"])
{
    Suite(nameof(VerifyKraidColorSourceSelection), () => VerifyKraidColorSourceSelection());
    return 0;
}
if (args is ["--lookup-kraid-health-thresholds"])
{
    var healthOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(healthOracle.Rom)),
        "Kraid health oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidHealthEighths), () => VerifyKraidHealthEighths(healthOracle));
    Suite(nameof(VerifyKraidHealthQuarters), () => VerifyKraidHealthQuarters(healthOracle));
    Console.WriteLine("Kraid health thresholds: both native recurrences across every initial health and ordinal, plus bounds, pass.");
    return 0;
}
if (args is ["--lookup-kraid-ceiling-order"])
{
    var ceilingOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ceilingOracle.Rom)),
        "Kraid ceiling oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidCeilingRockCoordinates), () => VerifyKraidCeilingRockCoordinates(ceilingOracle));
    Suite(nameof(VerifyKraidGrowthPlmColumns), () => VerifyKraidGrowthPlmColumns(ceilingOracle));
    Suite(nameof(VerifyKraidGrowthPlmRows), () => VerifyKraidGrowthPlmRows(ceilingOracle));
    Suite(nameof(VerifyKraidGrowthPlmHeaders), () => VerifyKraidGrowthPlmHeaders(ceilingOracle));
    Console.WriteLine("Kraid ceiling order: every native byte window and all derived PLM fields match; bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-sink-mappings"])
{
    var sinkOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(sinkOracle.Rom)),
        "Kraid sinking oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidSinkSchedule), () => VerifyKraidSinkSchedule(sinkOracle, definitionsOnly: true));
    return 0;
}
if (args is ["--lookup-kraid-defeated-plms"])
{
    var defeatedPlmOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(defeatedPlmOracle.Rom)),
        "Kraid defeated PLM oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidDefeatedPlmColumns), () => VerifyKraidDefeatedPlmColumns(defeatedPlmOracle));
    Suite(nameof(VerifyKraidDefeatedPlmRows), () => VerifyKraidDefeatedPlmRows(defeatedPlmOracle));
    Suite(nameof(VerifyKraidDefeatedPlmHeaders), () => VerifyKraidDefeatedPlmHeaders(defeatedPlmOracle));
    Console.WriteLine("Kraid defeated-room PLMs: both native requests, three fields, order and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-growth-plms"])
{
    var growthPlmOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(growthPlmOracle.Rom)),
        "Kraid growth PLM oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidGrowthPlmColumns), () => VerifyKraidGrowthPlmColumns(growthPlmOracle));
    Suite(nameof(VerifyKraidGrowthPlmRows), () => VerifyKraidGrowthPlmRows(growthPlmOracle));
    Suite(nameof(VerifyKraidGrowthPlmHeaders), () => VerifyKraidGrowthPlmHeaders(growthPlmOracle));
    Console.WriteLine("Kraid growth PLMs: all nine native columns, rows and headers, enumeration and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-movement"])
{
    var movementOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(movementOracle.Rom)),
        "Kraid movement oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidMovementChoices), () => VerifyKraidMovementChoices(movementOracle, definitionsOnly: true));
    return 0;
}
if (args is ["--lookup-kraid-nail-launch"])
{
    var launchOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(launchOracle.Rom)),
        "Kraid launch oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidNailLaunchXFraction), () => VerifyKraidNailLaunchXFraction(launchOracle));
    Suite(nameof(VerifyKraidNailLaunchXWhole), () => VerifyKraidNailLaunchXWhole(launchOracle));
    Suite(nameof(VerifyKraidNailLaunchYFraction), () => VerifyKraidNailLaunchYFraction(launchOracle));
    Suite(nameof(VerifyKraidNailLaunchYWhole), () => VerifyKraidNailLaunchYWhole(launchOracle));
    Console.WriteLine("Kraid nail launch: four fields, all sibling words and four RNG choices match native indirect records.");
    return 0;
}
if (args is ["--lookup-kraid-camera"])
{
    var cameraOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(cameraOracle.Rom)),
        "Kraid camera oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidInitialScrollMapping), () => VerifyKraidInitialScrollMapping(cameraOracle));
    Suite(nameof(VerifyKraidGrownScrollMapping), () => VerifyKraidGrownScrollMapping(cameraOracle));
    Console.WriteLine("Kraid camera: both four-screen mappings match native instruction operands; bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-arm-component-selectors"])
{
    var armComponentOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armComponentOracle.Rom)),
        "Kraid arm component oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidArmCollisionDefinitions), () => VerifyKraidArmCollisionDefinitions(armComponentOracle));
    return 0;
}
if (args is ["--lookup-kraid-arm-callbacks"])
{
    var armCallbackOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armCallbackOracle.Rom)),
        "Kraid arm callback oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidArmCollisionDefinitions), () => VerifyKraidArmCollisionDefinitions(armCallbackOracle));
    return 0;
}
if (args is ["--lookup-kraid-arm-physical-frames"])
{
    var armFrameOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armFrameOracle.Rom)),
        "Kraid arm physical-frame oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidArmComponentHitboxes), () => VerifyKraidArmComponentHitboxes(armFrameOracle));
    Suite(nameof(VerifyKraidArmPhysicalFramePointers), () => VerifyKraidArmPhysicalFramePointers(armFrameOracle));
    _ = VerifyKraidArmPhysicalLayoutSelection(armFrameOracle);
    Console.WriteLine("Kraid arm physical frames: all 22 roots, ordered native layouts, complete pointer domain and ordinal bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-foot-coordinates"])
{
    var footOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(footOracle.Rom)),
        "Kraid foot coordinate oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidFootFirstX), () => VerifyKraidFootFirstX(footOracle));
    Suite(nameof(VerifyKraidFootFirstY), () => VerifyKraidFootFirstY(footOracle));
    Suite(nameof(VerifyKraidFootSecondX), () => VerifyKraidFootSecondX(footOracle));
    Suite(nameof(VerifyKraidFootSecondY), () => VerifyKraidFootSecondY(footOracle));
    Suite(nameof(VerifyKraidFootSharedHitbox), () => VerifyKraidFootSharedHitbox(footOracle));
    Console.WriteLine("Kraid foot coordinates: four fields across 35 frames plus initial alias, shared geometry/callbacks, membership and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-contour-cases"])
{
    var contourOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(contourOracle.Rom)),
        "Kraid contour oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidBodyContourCases), () => VerifyKraidBodyContourCases(contourOracle));
    Suite(nameof(VerifyKraidGrowthResumeCases), () => VerifyKraidGrowthResumeCases(contourOracle));
    Console.WriteLine("Kraid body contour and growth resume: full signed-Y and tilemap domains match native records and instruction operands.");
    return 0;
}
if (args is ["--lookup-kraid-nail-contour"])
{
    var nailOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(nailOracle.Rom)),
        "Kraid nail oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidNailLeftOffsets), () => VerifyKraidNailLeftOffsets(nailOracle));
    Suite(nameof(VerifyKraidNailTopBoundaries), () => VerifyKraidNailTopBoundaries(nailOracle));
    Console.WriteLine("Kraid nail contour: all 12 original words, all 65536 wrapped relative-Y selections and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-mouth-mappings"])
{
    var mouthOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(mouthOracle.Rom)),
        "Kraid mouth oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidMouthShapeCases), () => VerifyKraidMouthShapeCases(mouthOracle));
    Suite(nameof(VerifyKraidLowHalfBoundaryMapping), () => VerifyKraidLowHalfBoundaryMapping(mouthOracle));
    Console.WriteLine("Kraid mouth mappings: 32 geometry words, 7 instruction bytes, head/mouth boundary crossings and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-head-programs"])
{
    var headOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(headOracle.Rom)),
        "Kraid head oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidHeadCommandMapping), () => VerifyKraidHeadCommandMapping(headOracle));
    Console.WriteLine("Kraid head programs: all 28 native commands, frame fields, sound IDs, enumeration, exact cursors and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-rock-programs"])
{
    var rockOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rockOracle.Rom)),
        "Kraid rock program oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidRockMechanicsMapping), () => VerifyKraidRockMechanicsMapping(rockOracle));
    Suite(nameof(VerifyKraidRockPresentationMapping), () => VerifyKraidRockPresentationMapping());
    Console.WriteLine("Kraid rock programs: 12 mechanics words, 7 presentation offsets, native domains and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-small-programs"])
{
    var smallOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(smallOracle.Rom)),
        "Kraid small-program oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidLintMechanicsMapping), () => VerifyKraidLintMechanicsMapping(smallOracle));
    Suite(nameof(VerifyKraidLintPresentationMapping), () => VerifyKraidLintPresentationMapping());
    Suite(nameof(VerifyKraidNailMechanicsMapping), () => VerifyKraidNailMechanicsMapping(smallOracle));
    Suite(nameof(VerifyKraidNailPresentationMapping), () => VerifyKraidNailPresentationMapping());
    Console.WriteLine("Kraid lint/nail programs: 14 mechanics words, 10 presentation offsets, native domains and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-arm-programs"])
{
    var armOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(armOracle.Rom)),
        "Kraid arm program oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidArmGeneratedMechanics), () => VerifyKraidArmGeneratedMechanics(armOracle));
    Suite(nameof(VerifyKraidArmGeneratedPresentation), () => VerifyKraidArmGeneratedPresentation(armOracle));
    Console.WriteLine("Kraid arm programs: 66 mechanics words, 57 presentation positions, native domains and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-foot-programs"])
{
    var programOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(programOracle.Rom)),
        "Kraid foot program oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidFootGeneratedMechanics), () => VerifyKraidFootGeneratedMechanics(programOracle));
    Suite(nameof(VerifyKraidFootGeneratedPresentation), () => VerifyKraidFootGeneratedPresentation(programOracle));
    Console.WriteLine("Kraid foot programs: 193 mechanics words, 106 presentation positions, native domains and bounds pass.");
    return 0;
}
if (args is ["--lookup-stream5-skeleton-transfers"]){ VerifyLookupStream5SkeletonTransfers(); return 0; }
if (args is ["--lookup-stream5-melting-tilemaps"]){ VerifyLookupStream5MeltingTilemaps(); return 0; }
if (args is ["--lookup-crocomire-skeleton-frames"])
{
    var skeletonOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(skeletonOracle.Rom)),
        "Skeleton frame oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCrocomireSkeletonFrameGeometry), () => VerifyCrocomireSkeletonFrameGeometry(skeletonOracle));
    Suite(nameof(VerifyExtendedFrameSequence), () => VerifyExtendedFrameSequence());
    Console.WriteLine("Crocomire skeleton catalog: 33 native roots, geometry, names, enumeration, membership and bounds pass.");
    return 0;
}
if (args is ["--lookup-kraid-foot-frames"])
{
    var footOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(footOracle.Rom)),
        "Kraid foot oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyKraidFootFrameGeometry), () => VerifyKraidFootFrameGeometry(footOracle));
    Suite(nameof(VerifyExtendedFrameSequence), () => VerifyExtendedFrameSequence());
    Console.WriteLine("Kraid foot catalog: 35 native roots, names, geometry, enumeration and bounds pass.");
    return 0;
}
if (args is ["--lookup-pirate-artwork-names"])
{
    Suite(nameof(VerifyWalkingPirateArtworkNames), () => VerifyWalkingPirateArtworkNames());
    Suite(nameof(VerifyWallPirateArtworkNames), () => VerifyWallPirateArtworkNames());
    Suite(nameof(VerifyExtendedFrameSequence), () => VerifyExtendedFrameSequence());
    Console.WriteLine("Pirate artwork names: all 55 published keys and bounds pass.");
    return 0;
}
if (args is ["--lookup-boss-oam-roots"])
{
    var rootOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rootOracle.Rom)),
        "Boss OAM root oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyRidleyOamRootGeometry), () => VerifyRidleyOamRootGeometry(rootOracle));
    Suite(nameof(VerifyDraygonOamRootGeometry), () => VerifyDraygonOamRootGeometry(rootOracle));
    Suite(nameof(VerifySporeSpawnOamRootGeometry), () => VerifySporeSpawnOamRootGeometry(rootOracle));
    Suite(nameof(VerifyExtendedFrameSequence), () => VerifyExtendedFrameSequence());
    Console.WriteLine("Boss OAM roots: 71 original identities, native geometry and bounds pass.");
    return 0;
}
if (args is ["--lookup-extended-frame-sequence"])
{
    Suite(nameof(VerifyExtendedFrameSequence), () => VerifyExtendedFrameSequence());
    return 0;
}
if (args is ["--lookup-mother-brain-visual-catalogs"])
{
    var catalogOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(catalogOracle.Rom)),
        "Mother Brain catalog oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyMotherBrainGeneratedOamCatalog), () => VerifyMotherBrainGeneratedOamCatalog(catalogOracle));
    Suite(nameof(VerifyMotherBrainGeneratedBg2Catalog), () => VerifyMotherBrainGeneratedBg2Catalog(catalogOracle));
    Console.WriteLine("Mother Brain catalogs: 17 OAM/16 BG2 identities and names, geometry, membership, exact JSON and loading pass.");
    return 0;
}
if (args is ["--lookup-phantoon-draygon-bg2-catalogs"])
{
    var catalogOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(catalogOracle.Rom)),
        "Boss BG2 catalog oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyPhantoonGeneratedBg2Catalog), () => VerifyPhantoonGeneratedBg2Catalog(catalogOracle));
    Suite(nameof(VerifyDraygonGeneratedBg2Catalog), () => VerifyDraygonGeneratedBg2Catalog(catalogOracle));
    Console.WriteLine("Phantoon/Draygon BG2: 56 original identities/names, geometry, selector domains, exact JSON and loading pass.");
    return 0;
}
if (args is ["--lookup-crocomire-bg2-catalog"])
{
    var catalogOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(catalogOracle.Rom)),
        "Crocomire BG2 catalog oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCrocomireBg2GeneratedCatalog), () => VerifyCrocomireBg2GeneratedCatalog(catalogOracle));
    Console.WriteLine("Crocomire BG2 catalog: 42 native identities/names, exact extracted JSON, loading and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-body-frames"])
{
    var bodyOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bodyOracle.Rom)),
        "Crocomire body oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCrocomireBodyFrameGeometry), () => VerifyCrocomireBodyFrameGeometry(bodyOracle));
    Console.WriteLine("Crocomire body: 50 original frame roots, native geometry, full BG2 membership domain and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-tongue-collision"])
{
    var tongueOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tongueOracle.Rom)),
        "Crocomire tongue collision oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCrocomireTongueFramePositions), () => VerifyCrocomireTongueFramePositions(tongueOracle));
    Suite(nameof(VerifyCrocomireTongueComponentCases), () => VerifyCrocomireTongueComponentCases(tongueOracle));
    Console.WriteLine("Crocomire tongue collision: nine native frame identities/components, empty hitbox cases and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-bg2-poses"])
{
    var bg2Oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bg2Oracle.Rom)),
        "Crocomire BG2 oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCrocomireBg2ScrollDefinitions), () => VerifyCrocomireBg2ScrollDefinitions(bg2Oracle));
    Console.WriteLine("Crocomire BG2: all 17 pointers and corrections, full selector domain and wrapped scroll values pass.");
    return 0;
}
if (args is ["--lookup-crocomire-melt-transfers"])
{
    var meltOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(meltOracle.Rom)),
        "Crocomire melt oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCrocomireMeltHeaders), () => VerifyCrocomireMeltHeaders(meltOracle));
    Suite(nameof(VerifyCrocomireMeltCopies), () => VerifyCrocomireMeltCopies(meltOracle));
    Suite(nameof(VerifyCrocomireMeltUploads), () => VerifyCrocomireMeltUploads(meltOracle));
    Console.WriteLine("Crocomire melt: two headers, 13 copies, 13 uploads, sentinels and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-projectile-programs"])
{
    var projectileOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(projectileOracle.Rom)),
        "Crocomire projectile oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCrocomireProjectileMechanicsDispatch), () => VerifyCrocomireProjectileMechanicsDispatch(projectileOracle));
    Suite(nameof(VerifyCrocomireProjectilePresentationPositions), () => VerifyCrocomireProjectilePresentationPositions(projectileOracle));
    Console.WriteLine("Crocomire projectiles: 22 native control words, 13 presentation positions, byte ownership and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-tongue-program"])
{
    var tongueOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tongueOracle.Rom)),
        "Crocomire tongue oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCrocomireTongueMechanicsDispatch), () => VerifyCrocomireTongueMechanicsDispatch(tongueOracle));
    Suite(nameof(VerifyCrocomireTonguePresentationPositions), () => VerifyCrocomireTonguePresentationPositions(tongueOracle));
    Console.WriteLine("Crocomire tongue: 14 native control words, nine presentation positions, byte ownership and bounds pass.");
    return 0;
}
if (args is ["--lookup-crocomire-spike-motion"])
{
    var spikeOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(spikeOracle.Rom)),
        "Crocomire spike oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCrocomireSpikeAccelerationDelta), () => VerifyCrocomireSpikeAccelerationDelta(spikeOracle));
    Suite(nameof(VerifyCrocomireSpikeMaximumAcceleration), () => VerifyCrocomireSpikeMaximumAcceleration(spikeOracle));
    Suite(nameof(VerifyCrocomireSpikeMaximumVelocity), () => VerifyCrocomireSpikeMaximumVelocity(spikeOracle));
    Console.WriteLine("Crocomire spike motion: all 54 native slot values and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-combo-sine-alias"])
{
    var comboOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(comboOracle.Rom)),
        "Combo sine oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyComboSineOffsetAlias), () => VerifyComboSineOffsetAlias(comboOracle));
    Console.WriteLine("Combo sine alias: all 65,536 byte-angle/radius pairs match native split multiplication.");
    return 0;
}
if (args is ["--lookup-zoa-animation"])
{
    var animationOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(animationOracle.Rom)),
        "Zoa animation oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyZoaAnimationDefinitions), () => VerifyZoaAnimationDefinitions(animationOracle, definitionsOnly: true));
    return 0;
}
if (args is ["--lookup-zoa-program"])
{
    var programOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(programOracle.Rom)),
        "Zoa program oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyZoaMechanicsDispatch), () => VerifyZoaMechanicsDispatch(programOracle));
    Suite(nameof(VerifyZoaPresentationPositions), () => VerifyZoaPresentationPositions(programOracle));
    Console.WriteLine("Zoa programs: 26 original control words, 12 presentation positions, byte ownership and bounds pass.");
    return 0;
}
if (args is ["--lookup-zoa-speed-cases"])
{
    var zoaOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(zoaOracle.Rom)),
        "Zoa oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCompiledZoaSpeeds), () => VerifyCompiledZoaSpeeds(zoaOracle, definitionsOnly: true));
    return 0;
}
if (args is ["--lookup-grapple-origin-cases"])
{
    var originOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(originOracle.Rom)),
        "Grapple origin oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyGrappleOriginXSelection), () => VerifyGrappleOriginXSelection(originOracle));
    Suite(nameof(VerifyGrappleOriginDefaultYSelection), () => VerifyGrappleOriginDefaultYSelection(originOracle));
    Suite(nameof(VerifyGrappleOriginRunningYSelection), () => VerifyGrappleOriginRunningYSelection(originOracle));
    Console.WriteLine("Grapple origin cases: all 40 original words across three mappings and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-suit-beam-curve"])
{
    var suitOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(suitOracle.Rom)),
        "Suit beam oracle is NTSC J/U v1.0");
    Suite(nameof(VerifySuitPickupBeamCurveDefinitions), () => VerifySuitPickupBeamCurveDefinitions(suitOracle));
    return 0;
}
if (args is ["--lookup-absolute-tangent"])
{
    var tangentOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tangentOracle.Rom)),
        "Tangent oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCompiledAbsoluteTangent), () => VerifyCompiledAbsoluteTangent(tangentOracle, definitionsOnly: true));
    Console.WriteLine("Absolute tangent algorithm: all 129 original words, direct caller aliases and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-shaktool-joint-algorithms"])
{
    var jointOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(jointOracle.Rom)),
        "Shaktool joint oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyShaktoolInitialAngleAlgorithm), () => VerifyShaktoolInitialAngleAlgorithm(jointOracle));
    Console.WriteLine("Shaktool joint algorithms: all 14 original words, velocity alias and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-power-bomb-base-curves"])
{
    var powerBombOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(powerBombOracle.Rom)),
        "Power Bomb oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyPowerBombWidthAlgorithm), () => VerifyPowerBombWidthAlgorithm(powerBombOracle));
    Suite(nameof(VerifyPowerBombTopOffsetAlgorithm), () => VerifyPowerBombTopOffsetAlgorithm(powerBombOracle));
    Console.WriteLine("Power Bomb base curves: all 64 original bytes and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-shaktool-orbit"])
{
    var orbitOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(orbitOracle.Rom)),
        "Shaktool oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyShaktoolOrbitAlgorithm), () => VerifyShaktoolOrbitAlgorithm(orbitOracle));
    Console.WriteLine("Shaktool orbit: all320 words, rounding intervals,256 displacement pairs and bounds pass.");
    return 0;
}
if (args is ["--liquid-tide-phase"])
{
    var tideOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyCompiledSignedTrigonometry), () => VerifyCompiledSignedTrigonometry(tideOracle));
    return 0;
}
if (args is ["--lookup-signed-sine-review"])
{
    var signedOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(signedOracle.Rom)),
        "Signed sine oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCompiledSignedTrigonometry), () => VerifyCompiledSignedTrigonometry(signedOracle, definitionsOnly: true));
    Suite(nameof(VerifySignedSixteenBitSineDefinitions), () => VerifySignedSixteenBitSineDefinitions(signedOracle));
    Suite(nameof(VerifyPhantoonWaveMath), () => VerifyPhantoonWaveMath(signedOracle, definitionsOnly: true));
    Console.WriteLine("Signed sine review: 320 signed words, 256 sixteen-bit words and every Phantoon byte phase pass.");
    return 0;
}
if (args is ["--lookup-half-wave-algorithms"])
{
    var sineOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(sineOracle.Rom)),
        "Sine oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyEightBitHalfWaveAlgorithm), () => VerifyEightBitHalfWaveAlgorithm(sineOracle));
    Suite(nameof(VerifyUnsignedHalfWaveAlgorithm), () => VerifyUnsignedHalfWaveAlgorithm(sineOracle));
    Console.WriteLine("Half-wave algorithms: all 256 original samples, rounding intervals and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-spc-pitch-basis"])
{
    var pitchOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pitchOracle.Rom)),
        "SPC pitch oracle is NTSC J/U v1.0");
    Suite(nameof(VerifySpcPitchBasisAlgorithm), () => VerifySpcPitchBasisAlgorithm(pitchOracle));
    Console.WriteLine("SPC pitch basis: all13 original words, root bounds and invalid indices pass.");
    return 0;
}
if (args is ["--lookup-dsp-rate-algorithm"])
{
    Suite(nameof(VerifyDspRateAlgorithm), () => VerifyDspRateAlgorithm());
    Console.WriteLine("DSP rates: all 32 original hardware periods and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-spc-effect-selection"])
{
    var effectOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(effectOracle.Rom)),
        "SPC effect oracle is NTSC J/U v1.0");
    Suite(nameof(VerifySpcEffectOperandSelection), () => VerifySpcEffectOperandSelection(effectOracle));
    Console.WriteLine("SPC effects: all 31 original operand counts and invalid index bounds pass.");
    return 0;
}
if (args is ["--lookup-spc-note-percentages"])
{
    var noteOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(noteOracle.Rom)),
        "SPC percentage oracle is NTSC J/U v1.0");
    Suite(nameof(VerifySpcNoteVolumeAlgorithm), () => VerifySpcNoteVolumeAlgorithm(noteOracle));
    Suite(nameof(VerifySpcNoteGateAlgorithm), () => VerifySpcNoteGateAlgorithm(noteOracle));
    Console.WriteLine("SPC note percentages: all24 original bytes, every timing-command selector and invalid bounds pass.");
    return 0;
}
if (args is ["--lookup-window-curves"])
{
    var curveOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(curveOracle.Rom)),
        "Window curve oracle is NTSC J/U v1.0");
    Suite(nameof(VerifySuitPickupBeamCurveDefinitions), () => VerifySuitPickupBeamCurveDefinitions(curveOracle));
    Suite(nameof(VerifyCompiledAbsoluteTangent), () => VerifyCompiledAbsoluteTangent(curveOracle, definitionsOnly: true));
    Console.WriteLine("Window curves: 128 suit bytes and129 tangent words, direct readers and bounds match original data.");
    return 0;
}
if (args is ["--lookup-running-cadence-review"])
{
    var cadenceOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(cadenceOracle.Rom)),
        "Cadence oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyRunningCadence), () => VerifyRunningCadence(cadenceOracle, definitionsOnly: true));
    Console.WriteLine("Running cadence: nine logical mappings, all 90 catalog bytes, selector bounds and mutable/wrapped aliases pass.");
    return 0;
}
if (args is ["--lookup-shared-speed-review"])
{
    var speedOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(speedOracle.Rom)),
        "Shared speed oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCompiledLinearEnemySpeeds), () => VerifyCompiledLinearEnemySpeeds(speedOracle, definitionsOnly: true));
    Suite(nameof(VerifyCompiledQuadraticEnemySpeeds), () => VerifyCompiledQuadraticEnemySpeeds(speedOracle, definitionsOnly: true));
    Console.WriteLine("Existing speed algorithms: 517 linear pairs, 759 quadratic words in both copies, 757 displacements and bounds match original bytes.");
    return 0;
}
if (args is ["--lookup-grapple-launch-algorithms"])
{
    var launchOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(launchOracle.Rom)),
        "Grapple launch oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyGrappleLaunchXSelection), () => VerifyGrappleLaunchXSelection(launchOracle));
    Suite(nameof(VerifyGrappleLaunchYSelection), () => VerifyGrappleLaunchYSelection(launchOracle));
    Suite(nameof(VerifyGrappleLaunchAngleAlgorithm), () => VerifyGrappleLaunchAngleAlgorithm(launchOracle));
    Console.WriteLine("Grapple launch: all thirty original words and each field's bounds pass.");
    return 0;
}
if (args is ["--lookup-fireflea-melt-algorithms"])
{
    var effectOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(effectOracle.Rom)),
        "Fireflea/melt oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyFirefleaFx), () => VerifyFirefleaFx(includeXrayCapture: false));
    Suite(nameof(VerifyCrocomireMaskAlgorithm), () => VerifyCrocomireMaskAlgorithm(effectOracle));
    Suite(nameof(VerifyCrocomireMeltingProductionSequence), () => VerifyCrocomireMeltingProductionSequence(effectOracle));
    return 0;
}
if (args is ["--lookup-fireflea-gunship-algorithms"])
{
    var motionOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(motionOracle.Rom)),
        "Fireflea/gunship oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyFirefleaMovementDefinitions), () => VerifyFirefleaMovementDefinitions(motionOracle));
    Suite(nameof(VerifyGunshipMotionDefinitions), () => VerifyGunshipMotionDefinitions(motionOracle));
    return 0;
}
if (args is ["--lookup-owtch-shake-algorithms"])
{
    var timingOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(timingOracle.Rom)),
        "Owtch/shake oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyOwtchMovementDefinitions), () => VerifyOwtchMovementDefinitions(timingOracle));
    Suite(nameof(VerifyRoomShakeDefinitions), () => VerifyRoomShakeDefinitions(timingOracle));
    return 0;
}
if (args is ["--lookup-slope-algorithms"])
{
    var slopeOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(slopeOracle.Rom)),
        "slope algorithm oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyCompiledSlopeHeights), () => VerifyCompiledSlopeHeights(slopeOracle));
    Suite(nameof(VerifyCompiledSquareSlopes), () => VerifyCompiledSquareSlopes(slopeOracle));
    return 0;
}
if (args is ["--shaktool-segment-algorithms"])
{
    var segmentOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(segmentOracle.Rom)),
        "Shaktool segment oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyShaktoolSegmentDefinitions), () => VerifyShaktoolSegmentDefinitions(segmentOracle));
    return 0;
}
if (args is ["--work-robot-initial-selection"])
{
    var robotOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(robotOracle.Rom)),
        "Work Robot selection oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyWorkRobotInitialSelection), () => VerifyWorkRobotInitialSelection(robotOracle));
    return 0;
}
if (args is ["--lookup-phase-algorithms"])
{
    var phaseOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(phaseOracle.Rom)),
        "lookup phase oracle is NTSC J/U v1.0");
    ushort PhaseWord(int address) => (ushort)(phaseOracle.ReadByte(address) | phaseOracle.ReadByte(address + 1) << 8);
    Suite(nameof(VerifyCompiledBotwoonSpeeds), () => VerifyCompiledBotwoonSpeeds(phaseOracle));
    Suite(nameof(VerifyComboPowerBombCostAlgorithm), () => VerifyComboPowerBombCostAlgorithm(PhaseWord));
    Suite(nameof(VerifyComboOriginAngleAlgorithm), () => VerifyComboOriginAngleAlgorithm(PhaseWord));
    Suite(nameof(VerifyDraygonIntroLatencyDefinitions), () => VerifyDraygonIntroLatencyDefinitions(phaseOracle));
    return 0;
}
if (args is ["--lookup-palette-algorithms"])
{
    var paletteOracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(paletteOracle.Rom)),
        "lookup palette oracle is NTSC J/U v1.0");
    Suite(nameof(VerifyDraygonHealthPaletteDefinitions), () => VerifyDraygonHealthPaletteDefinitions(paletteOracle));
    Suite(nameof(VerifyBotwoonHealthPaletteDefinitions), () => VerifyBotwoonHealthPaletteDefinitions(paletteOracle));
    Suite(nameof(VerifyWorkRobotPaletteTimingDefinitions), () => VerifyWorkRobotPaletteTimingDefinitions(paletteOracle));
    return 0;
}
if (args is ["--gunship-landing-compositions"])
{
    string gunshipInstallation = RepositoryInstallation.Installation.Root;
    using var gunshipOutput = new TestTempDirectory("map-catalog");
    Suite(nameof(VerifyExtractedGunshipCompositions), () => VerifyExtractedGunshipCompositions(gunshipInstallation, RepositoryRomPath, gunshipOutput.Root));
    Suite(nameof(VerifyGunshipLandingCompositions), () => VerifyGunshipLandingCompositions(gunshipInstallation));
    return 0;
}
if (args is ["--ceres-save-startup"])
{
    string ceresSaveInstallation = RepositoryInstallation.Installation.Root;
    Suite(nameof(VerifyCeresSaveStartup), () => VerifyCeresSaveStartup(ceresSaveInstallation));
    return 0;
}
if (args is ["--ceres-engine-palette-binding"])
{
    string ceresPaletteInstallation = RepositoryInstallation.Installation.Root;
    Suite(nameof(VerifyCeresEnginePaletteBinding), () => VerifyCeresEnginePaletteBinding(ceresPaletteInstallation));
    return 0;
}
Suite(nameof(VerifyRuntimeAddressSpaceHasNoCartridgeApi), () => VerifyRuntimeAddressSpaceHasNoCartridgeApi());
Suite(nameof(VerifyDebuggerVersionCompatibility), () => VerifyDebuggerVersionCompatibility());
Suite(nameof(VerifyCpuOperandOpenBus), () => VerifyCpuOperandOpenBus());
if (args is ["--title-artwork-references"])
{
    Suite(nameof(VerifyTitleArtworkReferences), () => VerifyTitleArtworkReferences());
    return 0;
}
if (args is ["--room-content-identity"])
{
    Suite(nameof(VerifyRoomContentIdentity), () => VerifyRoomContentIdentity());
    return 0;
}
if (args is ["--projectile-file-contracts"])
{
    string projectileInstallation = RepositoryInstallation.Installation.Root;
    Suite(nameof(VerifyProjectileFileContracts), () => VerifyProjectileFileContracts(projectileInstallation));
    return 0;
}
if (args is ["--gameplay-content-identity"])
{
    Suite(nameof(VerifyGameplayContentIdentity), () => VerifyGameplayContentIdentity());
    return 0;
}
if (args is ["--ending-content-identity"])
{
    Suite(nameof(VerifyEndingContentIdentity), () => VerifyEndingContentIdentity());
    return 0;
}
if (args is ["--intro-content-identity"])
{
    Suite(nameof(VerifyIntroContentIdentity), () => VerifyIntroContentIdentity());
    return 0;
}
if (args is ["--enemy-content-identity"])
{
    Suite(nameof(VerifyEnemyContentIdentity), () => VerifyEnemyContentIdentity());
    return 0;
}
if (args is ["--enemy-legacy-overrides"])
{
    Suite(nameof(VerifyEnemyLegacyOverrides), () => VerifyEnemyLegacyOverrides());
    Suite(nameof(VerifyEnemyExtendedLegacyOverrides), () => VerifyEnemyExtendedLegacyOverrides());
    Suite(nameof(VerifyEnemyProjectileLegacyOverrides), () => VerifyEnemyProjectileLegacyOverrides());
    return 0;
}
if (args is ["--enemy-animation-isolation"])
{
    Suite(nameof(VerifyEnemyAnimationIsolation), () => VerifyEnemyAnimationIsolation());
    return 0;
}
if (args is ["--enemy-effect-resources"])
{
    Suite(nameof(VerifyEnemyEffectResources), () => VerifyEnemyEffectResources());
    return 0;
}
if (args is ["--enemy-effect-isolation"])
{
    Suite(nameof(VerifyCrocomireEffectIsolation), () => VerifyCrocomireEffectIsolation());
    Suite(nameof(VerifyMotherBrainRotIsolation), () => VerifyMotherBrainRotIsolation());
    return 0;
}
if (args is ["--mother-brain-corpse-stock-artwork"])
{
    Suite(nameof(VerifyMotherBrainCorpseStockArtwork), () => VerifyMotherBrainCorpseStockArtwork());
    return 0;
}
if (args is ["--crocomire-melt-json"])
{
    Suite(nameof(VerifyCrocomireMeltJson), () => VerifyCrocomireMeltJson());
    return 0;
}
if (args is ["--mother-brain-body-presentation"])
{
    Suite(nameof(VerifyMotherBrainBodyPresentation), () => VerifyMotherBrainBodyPresentation());
    return 0;
}
if (args is ["--boss-display-bindings"])
{
    Suite(nameof(VerifyBossDisplayBindings), () => VerifyBossDisplayBindings());
    return 0;
}
if (args is ["--boss-display-stock"])
{
    Suite(nameof(VerifyBossDisplayStock), () => VerifyBossDisplayStock());
    return 0;
}
if (args is ["--tilemap-json-contracts"])
{
    Suite(nameof(VerifyTilemapJsonContracts), () => VerifyTilemapJsonContracts());
    return 0;
}
if (args is ["--room-asset-json-contracts"])
{
    string roomAssetRoot = RepositoryInstallation.Installation.Root;
    Suite(nameof(VerifyRoomAssetJsonContracts), () => VerifyRoomAssetJsonContracts(roomAssetRoot));
    return 0;
}
if (args is ["--palette-json-contracts"])
{
    Suite(nameof(VerifyPaletteJsonContracts), () => VerifyPaletteJsonContracts());
    return 0;
}
if (args is ["--kraid-installed-presentation"])
{
    Suite(nameof(VerifyKraidInstalledPresentation), () => VerifyKraidInstalledPresentation());
    return 0;
}
if (args is ["--mother-brain-body-stock-presentation"])
{
    Suite(nameof(VerifyMotherBrainBodyStockPresentation), () => VerifyMotherBrainBodyStockPresentation());
    return 0;
}
if (args is ["--enemy-animation-stock-parity"])
{
    Suite(nameof(VerifyEnemyAnimationStockParity), () => VerifyEnemyAnimationStockParity());
    return 0;
}
if (args is ["--catalog-identity-state-compatibility"])
{
    Suite(nameof(VerifyCatalogIdentityStateCompatibility), () => VerifyCatalogIdentityStateCompatibility());
    return 0;
}
if (args is ["--plm-content-identity"])
{
    Suite(nameof(VerifyPlmContentIdentity), () => VerifyPlmContentIdentity());
    return 0;
}
if (args is ["--samus-content-identity"])
{
    Suite(nameof(VerifySamusContentIdentity), () => VerifySamusContentIdentity());
    return 0;
}
if (args is ["--wram-helper-boundary"])
{
    Suite(nameof(VerifyWramHelperBoundary), () => VerifyWramHelperBoundary());
    return 0;
}
if (args is ["--plm-population-input-boundary"])
{
    Suite(nameof(VerifyPlmPopulationInputBoundary), () => VerifyPlmPopulationInputBoundary());
    return 0;
}
if (args is ["--mechanics-workram-boundary"])
{
    Suite(nameof(VerifyMechanicsWorkRamBoundary), () => VerifyMechanicsWorkRamBoundary());
    return 0;
}
if (args is ["--plm-draw-clone"])
{
    Suite(nameof(VerifyPlmDrawClone), () => VerifyPlmDrawClone());
    return 0;
}
if (args is ["--enemy-definition-boundary"])
{
    string enemyDefinitionRom = RepositoryRomPath;
    Suite(nameof(VerifyEnemyDefinitionBoundary), () => VerifyEnemyDefinitionBoundary(enemyDefinitionRom));
    return 0;
}
if (args is ["--enemy-gameplay-acceptance"])
{
    Suite(nameof(VerifyEnemyGameplayAcceptance), () => VerifyEnemyGameplayAcceptance());
    return 0;
}

if (args is ["--beam-palette-artwork"])
{
    var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyBeamPaletteArtwork), () => VerifyBeamPaletteArtwork(rom, BeamTileCatalog.Load(BeamTileExtractor.Extract(rom))));
    return 0;
}
if (args is ["--samus-rendering-slice"])
{
    Suite(nameof(VerifySamusRenderingSlice), () => VerifySamusRenderingSlice());
    return 0;
}
if (args is ["--samus-horizontal-speed"])
{
    Suite(nameof(VerifySamusHorizontalSpeed), () => VerifySamusHorizontalSpeed());
    return 0;
}
if (args is ["--samus-xray"])
{
    Suite(nameof(VerifySamusXray), () => VerifySamusXray());
    return 0;
}
if (args is ["--samus-aerial-movement"])
{
    Suite(nameof(VerifySamusAerialMovement), () => VerifySamusAerialMovement());
    return 0;
}
if (args is ["--samus-atmospheric-effects"])
{
    Suite(nameof(VerifySamusAtmosphericEffects), () => VerifySamusAtmosphericEffects());
    return 0;
}
if (args is ["--samus-atmosphere-artwork-boundary"])
{
    string atmosphereRom = RepositoryRomPath;
    Suite(nameof(VerifySamusAtmosphereArtworkBoundary), () => VerifySamusAtmosphereArtworkBoundary(atmosphereRom));
    return 0;
}
if (args is ["--samus-atmospheric-cadence"])
{
    string atmosphereCadenceRom = RepositoryRomPath;
    Suite(nameof(VerifySamusAtmosphericAnimationDefinitions), () => VerifySamusAtmosphericAnimationDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(atmosphereCadenceRom)));
    return 0;
}
if (args is ["--samus-aerial-turns-walljump"])
{
    Suite(nameof(VerifySamusAerialTurnsAndWallJump), () => VerifySamusAerialTurnsAndWallJump());
    return 0;
}
if (args is ["--samus-posture-movement"])
{
    Suite(nameof(VerifySamusPostureMovement), () => VerifySamusPostureMovement());
    return 0;
}
if (args is ["--samus-morph-ball"])
{
    Suite(nameof(VerifySamusMorphBallMovement), () => VerifySamusMorphBallMovement());
    return 0;
}
if (args is ["--samus-aimed-aerial"])
{
    Suite(nameof(VerifySamusAimedAerialMovement), () => VerifySamusAimedAerialMovement());
    return 0;
}
if (args is ["--samus-gun-extended"])
{
    Suite(nameof(VerifySamusGunExtendedMovement), () => VerifySamusGunExtendedMovement());
    return 0;
}
if (args is ["--samus-grounded-reversal"])
{
    Suite(nameof(VerifySamusGroundedReversal), () => VerifySamusGroundedReversal());
    return 0;
}
if (args is ["--enemy-angle-division"])
{
    Suite(nameof(VerifyEnemyAngleDivision), () => VerifyEnemyAngleDivision());
    return 0;
}

if (args is ["--sand-animated-tiles"])
{
    Suite(nameof(VerifySandAnimatedTiles), () => VerifySandAnimatedTiles());
    return 0;
}
if (args is ["--room-fx-animated-tiles"])
{
    Suite(nameof(VerifyRoomFxAnimatedTileMechanicsDefinitions), () => VerifyRoomFxAnimatedTileMechanicsDefinitions());
    Suite(nameof(VerifyRoomFxAnimatedTileArtwork), () => VerifyRoomFxAnimatedTileArtwork());
    return 0;
}
if (args is ["--tourian-statue-animated-tiles"])
{
    Suite(nameof(VerifyTourianStatueUnlockDefinitions), () => VerifyTourianStatueUnlockDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--treadmill-animated-tiles"])
{
    Suite(nameof(VerifyAnimatedTileInstructionCodeCatalog), () => VerifyAnimatedTileInstructionCodeCatalog());
    return 0;
}
if (args is ["--room-fx-layer3-tilemaps"])
{
    Suite(nameof(VerifyRoomFxLayer3Tilemaps), () => VerifyRoomFxLayer3Tilemaps());
    return 0;
}
if (args is ["--room-fx-palette-blends"])
{
    Suite(nameof(VerifyRoomFxPaletteBlends), () => VerifyRoomFxPaletteBlends());
    return 0;
}
if (args is ["--power-bomb-fixed-colors"])
{
    Suite(nameof(VerifyPowerBombFixedColors), () => VerifyPowerBombFixedColors());
    return 0;
}
if (args is ["--samus-visor-colors"])
{
    Suite(nameof(VerifySamusVisorColors), () => VerifySamusVisorColors());
    return 0;
}
if (args is ["--samus-hurt-colors"])
{
    Suite(nameof(VerifySamusHurtColors), () => VerifySamusHurtColors());
    return 0;
}




if (args is ["--normal-suit-palette-pointers"])
{
    Suite(nameof(VerifyNormalSuitPalettePointers), () => VerifyNormalSuitPalettePointers());
    return 0;
}
if (args is ["--speed-boost-palette-pointers"])
{
    Suite(nameof(VerifySpeedBoostPalettePointers), () => VerifySpeedBoostPalettePointers());
    return 0;
}
if (args is ["--full-body-palette-pointer-lists"])
{
    Suite(nameof(VerifyFullBodyPalettePointerLists), () => VerifyFullBodyPalettePointerLists());
    return 0;
}
if (args is ["--spc-sound-library-2-pointers"])
{
    Suite(nameof(VerifySpcSoundLibrary2Pointers), () => VerifySpcSoundLibrary2Pointers());
    return 0;
}
if (args is ["--room-fx-retail-inventory"])
{
    Suite(nameof(VerifyRetailRoomFxInventory), () => VerifyRetailRoomFxInventory());
    return 0;
}
if (args is ["--enemy-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyEnemyProjectileInstructionMechanicsDefinitions), () => VerifyEnemyProjectileInstructionMechanicsDefinitions());
    return 0;
}
if (args is ["--enemy-pickup-instruction-mechanics"])
{
    Suite(nameof(VerifyEnemyPickupInstructionProgramDefinitions), () => VerifyEnemyPickupInstructionProgramDefinitions());
    return 0;
}
if (args is ["--enemy-death-instruction-mechanics"])
{
    Suite(nameof(VerifyEnemyDeathInstructionProgramDefinitions), () => VerifyEnemyDeathInstructionProgramDefinitions());
    return 0;
}
if (args is ["--shaktool-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyShaktoolProjectileInstructionProgramDefinitions), () => VerifyShaktoolProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--chozo-tourian-dust-instruction-mechanics"])
{
    Suite(nameof(VerifyChozoTourianDustInstructionProgramDefinitions), () => VerifyChozoTourianDustInstructionProgramDefinitions());
    return 0;
}
if (args is ["--tourian-statue-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyTourianStatueProjectileInstructionProgramDefinitions), () => VerifyTourianStatueProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--spore-spawn-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifySporeSpawnProjectileInstructionProgramDefinitions), () => VerifySporeSpawnProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--botwoon-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyBotwoonProjectileInstructionProgramDefinitions), () => VerifyBotwoonProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--torizo-landing-dust-instruction-mechanics"])
{
    Suite(nameof(VerifyTorizoLandingDustInstructionProgramDefinitions), () => VerifyTorizoLandingDustInstructionProgramDefinitions());
    return 0;
}
if (args is ["--torizo-explosive-swipe-instruction-mechanics"])
{
    Suite(nameof(VerifyTorizoExplosiveSwipeInstructionProgramDefinitions), () => VerifyTorizoExplosiveSwipeInstructionProgramDefinitions());
    return 0;
}
if (args is ["--bomb-torizo-drool-instruction-mechanics"])
{
    Suite(nameof(VerifyBombTorizoDroolInstructionProgramDefinitions), () => VerifyBombTorizoDroolInstructionProgramDefinitions());
    return 0;
}
if (args is ["--torizo-explosion-instruction-mechanics"])
{
    Suite(nameof(VerifyTorizoExplosionInstructionProgramDefinitions), () => VerifyTorizoExplosionInstructionProgramDefinitions());
    return 0;
}
if (args is ["--torizo-chozo-orb-instruction-mechanics"])
{
    Suite(nameof(VerifyTorizoChozoOrbInstructionProgramDefinitions), () => VerifyTorizoChozoOrbInstructionProgramDefinitions());
    return 0;
}
if (args is ["--torizo-sonic-boom-instruction-mechanics"])
{
    Suite(nameof(VerifyTorizoSonicBoomInstructionProgramDefinitions), () => VerifyTorizoSonicBoomInstructionProgramDefinitions());
    return 0;
}
if (args is ["--spring-ball-equipment-jump"])
{
    Suite(nameof(VerifySpringBallEquipmentJump), () => VerifySpringBallEquipmentJump());
    return 0;
}
if (args is ["--power-bomb-afterglow-shape"])
{
    Suite(nameof(VerifyPowerBombAfterglowShape), () => VerifyPowerBombAfterglowShape());
    return 0;
}
if (args is ["--file-copy-arrow"])
{
    Suite(nameof(VerifyFileCopyArrow), () => VerifyFileCopyArrow());
    return 0;
}
if (args is ["--bomb-torizo-statue-instruction-mechanics"])
{
    Suite(nameof(VerifyBombTorizoStatueInstructionProgramDefinitions), () => VerifyBombTorizoStatueInstructionProgramDefinitions());
    return 0;
}
if (args is ["--golden-torizo-egg-instruction-mechanics"])
{
    Suite(nameof(VerifyGoldenTorizoEggInstructionProgramDefinitions), () => VerifyGoldenTorizoEggInstructionProgramDefinitions());
    return 0;
}
if (args is ["--golden-torizo-super-missile-instruction-mechanics"])
{
    Suite(nameof(VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions), () => VerifyGoldenTorizoSuperMissileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--golden-torizo-eye-beam-instruction-mechanics"])
{
    Suite(nameof(VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions), () => VerifyGoldenTorizoEyeBeamInstructionProgramDefinitions());
    return 0;
}
if (args is ["--enemy-projectile-instruction-owner-coverage"])
{
    Suite(nameof(VerifyEnemyProjectileInstructionOwnerCoverage), () => VerifyEnemyProjectileInstructionOwnerCoverage());
    return 0;
}
if (args is ["--enemy-instruction-owner-coverage"])
{
    Suite(nameof(VerifyEnemyInstructionOwnerCoverage), () => VerifyEnemyInstructionOwnerCoverage());
    return 0;
}
if (args is ["--mother-brain-body-instruction-mechanics"])
{
    Suite(nameof(VerifyMotherBrainBodyInstructionPrograms), () => VerifyMotherBrainBodyInstructionPrograms());
    return 0;
}
if (args is ["--gunship-instruction-mechanics"])
{
    Suite(nameof(VerifyGunshipInstructionProgramDefinitions), () => VerifyGunshipInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ridley-instruction-mechanics"])
{
    Suite(nameof(VerifyRidleyInstructionProgramDefinitions), () => VerifyRidleyInstructionProgramDefinitions());
    return 0;
}
if (args is ["--draygon-instruction-mechanics"])
{
    Suite(nameof(VerifyDraygonInstructionProgramDefinitions), () => VerifyDraygonInstructionProgramDefinitions());
    return 0;
}
if (args is ["--walking-space-pirate-instruction-mechanics"])
{
    Suite(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions());
    return 0;
}
if (args is ["--wall-space-pirate-instruction-mechanics"])
{
    Suite(nameof(VerifyWallSpacePirateInstructionProgramDefinitions), () => VerifyWallSpacePirateInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ninja-space-pirate-instruction-mechanics"])
{
    Suite(nameof(VerifyNinjaSpacePirateInstructionProgramDefinitions), () => VerifyNinjaSpacePirateInstructionProgramDefinitions());
    return 0;
}
if (args is ["--cacatac-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyCacatacProjectileInstructionProgramDefinitions), () => VerifyCacatacProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--falling-spark-instruction-mechanics"])
{
    Suite(nameof(VerifyFallingSparkInstructionProgramDefinitions), () => VerifyFallingSparkInstructionProgramDefinitions());
    return 0;
}
if (args is ["--fune-namihe-fireball-instruction-mechanics"])
{
    Suite(nameof(VerifyFuneNamiheFireballInstructionProgramDefinitions), () => VerifyFuneNamiheFireballInstructionProgramDefinitions());
    return 0;
}
if (args is ["--magdollite-lava-instruction-mechanics"])
{
    Suite(nameof(VerifyMagdolliteLavaInstructionProgramDefinitions), () => VerifyMagdolliteLavaInstructionProgramDefinitions());
    return 0;
}
if (args is ["--dragon-fireball-instruction-mechanics"])
{
    Suite(nameof(VerifyDragonFireballInstructionProgramDefinitions), () => VerifyDragonFireballInstructionProgramDefinitions());
    return 0;
}
if (args is ["--eye-door-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyEyeDoorProjectileInstructionProgramDefinitions), () => VerifyEyeDoorProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--eye-door-sweat-instruction-mechanics"])
{
    Suite(nameof(VerifyEyeDoorSweatInstructionProgramDefinitions), () => VerifyEyeDoorSweatInstructionProgramDefinitions());
    return 0;
}
if (args is ["--skree-metaree-particle-instruction-mechanics"])
{
    Suite(nameof(VerifySkreeMetareeParticleInstructionProgramDefinitions), () => VerifySkreeMetareeParticleInstructionProgramDefinitions());
    return 0;
}
if (args is ["--kraid-rock-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyKraidRockProjectileInstructionProgramDefinitions), () => VerifyKraidRockProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--fake-kraid-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyFakeKraidProjectileInstructionProgramDefinitions), () => VerifyFakeKraidProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--alcoon-fireball-instruction-mechanics"])
{
    Suite(nameof(VerifyAlcoonFireballInstructionProgramDefinitions), () => VerifyAlcoonFireballInstructionProgramDefinitions());
    return 0;
}
if (args is ["--work-robot-laser-instruction-mechanics"])
{
    Suite(nameof(VerifyWorkRobotLaserInstructionProgramDefinitions), () => VerifyWorkRobotLaserInstructionProgramDefinitions());
    return 0;
}
if (args is ["--powamp-spike-instruction-mechanics"])
{
    Suite(nameof(VerifyPowampSpikeInstructionProgramDefinitions), () => VerifyPowampSpikeInstructionProgramDefinitions());
    return 0;
}
if (args is ["--polyp-rock-instruction-mechanics"])
{
    Suite(nameof(VerifyPolypRockInstructionProgramDefinitions), () => VerifyPolypRockInstructionProgramDefinitions());
    return 0;
}
if (args is ["--kihunter-acid-spit-instruction-mechanics"])
{
    Suite(nameof(VerifyKiHunterAcidSpitInstructionProgramDefinitions), () => VerifyKiHunterAcidSpitInstructionProgramDefinitions());
    return 0;
}
if (args is ["--stoke-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyStokeProjectileInstructionProgramDefinitions), () => VerifyStokeProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--nuclear-waffle-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyNuclearWaffleProjectileInstructionProgramDefinitions), () => VerifyNuclearWaffleProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--kago-bug-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyKagoBugProjectileInstructionProgramDefinitions), () => VerifyKagoBugProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--yapping-maw-body-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyYappingMawBodyProjectileInstructionProgramDefinitions), () => VerifyYappingMawBodyProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--crocomire-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyCrocomireProjectileInstructionProgramDefinitions), () => VerifyCrocomireProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--phantoon-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyPhantoonProjectileInstructionProgramDefinitions), () => VerifyPhantoonProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--draygon-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyDraygonProjectileInstructionProgramDefinitions), () => VerifyDraygonProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ceres-debris-instruction-mechanics"])
{
    Suite(nameof(VerifyCeresFallingDebrisInstructionProgramDefinitions), () => VerifyCeresFallingDebrisInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ceres-debris-fixture"])
{
    Suite(nameof(VerifyCeresFallingDebrisInstructionProgramDefinitions), () => VerifyCeresFallingDebrisInstructionProgramDefinitions());
    return 0;
}
if (args is ["--save-station-electricity-instruction-mechanics"])
{
    Suite(nameof(VerifySaveStationElectricityInstructionProgramDefinitions), () => VerifySaveStationElectricityInstructionProgramDefinitions());
    return 0;
}
if (args is ["--downward-gate-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyDownwardGateProjectileInstructionProgramDefinitions), () => VerifyDownwardGateProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--noob-tube-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyNoobTubeProjectileInstructionProgramDefinitions), () => VerifyNoobTubeProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--mother-brain-top-tube-instruction-mechanics"])
{
    Suite(nameof(VerifyMotherBrainTopTubeInstructionProgramDefinitions), () => VerifyMotherBrainTopTubeInstructionProgramDefinitions());
    return 0;
}
if (args is ["--mother-brain-turret-instruction-mechanics"])
{
    Suite(nameof(VerifyMotherBrainTurretInstructionProgramDefinitions), () => VerifyMotherBrainTurretInstructionProgramDefinitions());
    return 0;
}
if (args is ["--gunship-dust-instruction-mechanics"])
{
    Suite(nameof(VerifyGunshipDustInstructionProgramDefinitions), () => VerifyGunshipDustInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ceres-ridley-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifyCeresRidleyProjectileInstructionProgramDefinitions), () => VerifyCeresRidleyProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--space-pirate-projectile-instruction-mechanics"])
{
    Suite(nameof(VerifySpacePirateProjectileInstructionProgramDefinitions), () => VerifySpacePirateProjectileInstructionProgramDefinitions());
    return 0;
}
if (args is ["--eye-door-plms"])
{
    Suite(nameof(VerifyEyeDoorPlms), () => VerifyEyeDoorPlms());
    return 0;
}
if (args is ["--hud-state"])
{
    Suite(nameof(VerifyHudStateAndBg3Rendering), () => VerifyHudStateAndBg3Rendering());
    return 0;
}
if (args is ["--area-map-assets"])
{
    Suite(nameof(VerifyAreaMapAssets), () => VerifyAreaMapAssets());
    return 0;
}
if (args is ["--file-select-fresh-save"])
{
    Suite(nameof(VerifyFileSelectFreshSaveTilemap), () => VerifyFileSelectFreshSaveTilemap());
    return 0;
}
if (args is ["--cacatac-instruction-mechanics"])
{
    Suite(nameof(VerifyCacatacInstructionProgramDefinitions), () => VerifyCacatacInstructionProgramDefinitions());
    return 0;
}
if (args is ["--magdollite-instruction-mechanics"])
{
    Suite(nameof(VerifyMagdolliteInstructionProgramDefinitions), () => VerifyMagdolliteInstructionProgramDefinitions());
    return 0;
}
if (args is ["--kihunter-instruction-mechanics"])
{
    Suite(nameof(VerifyKiHunterInstructionProgramDefinitions), () => VerifyKiHunterInstructionProgramDefinitions());
    return 0;
}
if (args is ["--owtch-instruction-mechanics"])
{
    Suite(nameof(VerifyOwtchInstructionProgramDefinitions), () => VerifyOwtchInstructionProgramDefinitions());
    return 0;
}
if (args is ["--stoke-instruction-mechanics"])
{
    Suite(nameof(VerifyStokeInstructionProgramDefinitions), () => VerifyStokeInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ripper-instruction-mechanics"])
{
    Suite(nameof(VerifyRipperInstructionProgramDefinitions), () => VerifyRipperInstructionProgramDefinitions());
    return 0;
}
if (args is ["--kzan-instruction-mechanics"])
{
    Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions());
    return 0;
}
if (args is ["--fly-instruction-mechanics"])
{
    Suite(nameof(VerifyFlyInstructionProgramDefinitions), () => VerifyFlyInstructionProgramDefinitions());
    return 0;
}
if (args is ["--bull-instruction-mechanics"])
{
    Suite(nameof(VerifyBullInstructionProgramDefinitions), () => VerifyBullInstructionProgramDefinitions());
    return 0;
}
if (args is ["--kago-instruction-mechanics"])
{
    Suite(nameof(VerifyKagoInstructionProgramDefinitions), () => VerifyKagoInstructionProgramDefinitions());
    return 0;
}
if (args is ["--horizontal-shutter-instruction-mechanics"])
{
    Suite(nameof(VerifyHorizontalShutterInstructionProgramDefinitions), () => VerifyHorizontalShutterInstructionProgramDefinitions());
    return 0;
}
if (args is ["--growing-shutter-instruction-mechanics"])
{
    Suite(nameof(VerifyGrowingShutterInstructionProgramDefinitions), () => VerifyGrowingShutterInstructionProgramDefinitions());
    return 0;
}
if (args is ["--vertical-shutter-instruction-mechanics"])
{
    Suite(nameof(VerifyVerticalShutterInstructionProgramDefinitions), () => VerifyVerticalShutterInstructionProgramDefinitions());
    return 0;
}
if (args is ["--choot-instruction-mechanics"])
{
    Suite(nameof(VerifyChootInstructionProgramDefinitions), () => VerifyChootInstructionProgramDefinitions());
    return 0;
}
if (args is ["--norfair-lava-jumper-instruction-mechanics"])
{
    Suite(nameof(VerifyNorfairLavaJumperInstructionProgramDefinitions), () => VerifyNorfairLavaJumperInstructionProgramDefinitions());
    return 0;
}
if (args is ["--beetom-instruction-mechanics"])
{
    Suite(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions());
    return 0;
}
if (args is ["--alcoon-instruction-mechanics"])
{
    Suite(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions());
    return 0;
}
if (args is ["--multiviola-instruction-mechanics"])
{
    Suite(nameof(VerifyMultiviolaInstructionProgramDefinitions), () => VerifyMultiviolaInstructionProgramDefinitions());
    return 0;
}
if (args is ["--polyp-instruction-mechanics"])
{
    Suite(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions());
    return 0;
}
if (args is ["--powamp-instruction-mechanics"])
{
    Suite(nameof(VerifyPowampInstructionProgramDefinitions), () => VerifyPowampInstructionProgramDefinitions());
    return 0;
}
if (args is ["--wrecked-ship-ghost-instruction-mechanics"])
{
    Suite(nameof(VerifyWreckedShipGhostInstructionProgramDefinitions), () => VerifyWreckedShipGhostInstructionProgramDefinitions());
    return 0;
}
if (args is ["--puyo-instruction-mechanics"])
{
    Suite(nameof(VerifyPuyoInstructionProgramDefinitions), () => VerifyPuyoInstructionProgramDefinitions());
    return 0;
}
if (args is ["--dead-torizo-instruction-mechanics"])
{
    Suite(nameof(VerifyDeadTorizoInstructionProgramDefinitions), () => VerifyDeadTorizoInstructionProgramDefinitions());
    return 0;
}
if (args is ["--dead-sidehopper-instruction-mechanics"])
{
    Suite(nameof(VerifyDeadSidehopperInstructionProgramDefinitions), () => VerifyDeadSidehopperInstructionProgramDefinitions());
    return 0;
}
if (args is ["--dead-tourian-corpse-instruction-mechanics"])
{
    Suite(nameof(VerifyDeadTourianCorpseInstructionProgramDefinitions), () => VerifyDeadTourianCorpseInstructionProgramDefinitions());
    return 0;
}
if (args is ["--shitroid-instruction-mechanics"])
{
    Suite(nameof(VerifyShitroidInstructionProgramDefinitions), () => VerifyShitroidInstructionProgramDefinitions());
    return 0;
}
if (args is ["--rio-instruction-mechanics"])
{
    Suite(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions());
    return 0;
}
if (args is ["--spore-spawn-instruction-mechanics"])
{
    Suite(nameof(VerifySporeSpawnInstructionProgramDefinitions), () => VerifySporeSpawnInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ceres-baby-instruction-mechanics"])
{
    Suite(nameof(VerifyCeresBabyInstructionProgramDefinitions), () => VerifyCeresBabyInstructionProgramDefinitions());
    return 0;
}
if (args is ["--rinka-instruction-mechanics"])
{
    Suite(nameof(VerifyRinkaInstructionProgramDefinitions), () => VerifyRinkaInstructionProgramDefinitions());
    return 0;
}
if (args is ["--fune-namihe-instruction-mechanics"])
{
    Suite(nameof(VerifyFuneNamiheInstructionProgramDefinitions), () => VerifyFuneNamiheInstructionProgramDefinitions());
    return 0;
}
if (args is ["--atomic-instruction-mechanics"])
{
    Suite(nameof(VerifyAtomicMovementDefinitions), () => VerifyAtomicMovementDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--sbug-instruction-mechanics"])
{
    Suite(nameof(VerifySbugMovementDefinitions), () => VerifySbugMovementDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--projectile-radius-instruction-fixtures"])
{
    var projectileFixtureRom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyProjectileRadii), () => VerifyProjectileRadii(projectileFixtureRom));
    Suite(nameof(VerifyProjectileInstructions), () => VerifyProjectileInstructions(projectileFixtureRom));
    return 0;
}
if (args is ["--beam-tile-artwork-fixture"])
{
    Suite(nameof(VerifyBeamTileArtwork), () => VerifyBeamTileArtwork(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--projectile-trail-artwork-fixture"])
{
    Suite(nameof(VerifyProjectileTrailArtwork), () => VerifyProjectileTrailArtwork(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--charge-flare-placement-fixture"])
{
    Suite(nameof(VerifyChargeFlarePlacement), () => VerifyChargeFlarePlacement(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--projectile-compositions-fixture"])
{
    Suite(nameof(VerifyProjectileCompositions), () => VerifyProjectileCompositions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--ridley-movement-target-fixture"])
{
    Suite(nameof(VerifyRidleyMovementTargets), () => VerifyRidleyMovementTargets(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--kraid-contour-head-fixtures"])
{
    var kraidFixtureRom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyKraidBodyContour), () => VerifyKraidBodyContour(kraidFixtureRom));
    Suite(nameof(VerifyKraidHeadInstructionDefinitions), () => VerifyKraidHeadInstructionDefinitions(kraidFixtureRom));
    return 0;
}
if (args is ["--power-bomb-shape-fixture"])
{
    Suite(nameof(VerifyCompiledPowerBombShape), () => VerifyCompiledPowerBombShape(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--absolute-tangent-runtime"])
{
    Suite(nameof(VerifyCompiledAbsoluteTangent), () => VerifyCompiledAbsoluteTangent(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--enemy-callback-definitions"])
{
    var callbackRom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyPowerBombCallbackDefinitions), () => VerifyPowerBombCallbackDefinitions(callbackRom));
    Suite(nameof(VerifyShotCallbackDefinitions), () => VerifyShotCallbackDefinitions(callbackRom));
    return 0;
}
if (args is ["--spark-instruction-mechanics"])
{
    Suite(nameof(VerifySparkMovementDefinitions), () => VerifySparkMovementDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--nuclear-waffle-instruction-mechanics"])
{
    Suite(nameof(VerifyNuclearWaffleDefinitions), () => VerifyNuclearWaffleDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--hibashi-instruction-mechanics"])
{
    Suite(nameof(VerifyHibashiDefinitions), () => VerifyHibashiDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--blue-brinstar-face-block-instruction-mechanics"])
{
    Suite(nameof(VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions), () => VerifyBlueBrinstarFaceBlockInstructionProgramDefinitions());
    return 0;
}
if (args is ["--boulder-instruction-mechanics"])
{
    Suite(nameof(VerifyBoulderInstructionProgramDefinitions), () => VerifyBoulderInstructionProgramDefinitions());
    return 0;
}
if (args is ["--boyon-instruction-mechanics"])
{
    Suite(nameof(VerifyBoyonInstructionProgramDefinitions), () => VerifyBoyonInstructionProgramDefinitions());
    return 0;
}
if (args is ["--skultera-instruction-mechanics"])
{
    Suite(nameof(VerifySkulteraInstructionProgramDefinitions), () => VerifySkulteraInstructionProgramDefinitions());
    return 0;
}
if (args is ["--waver-instruction-mechanics"])
{
    Suite(nameof(VerifyWaverInstructionProgramDefinitions), () => VerifyWaverInstructionProgramDefinitions());
    return 0;
}
if (args is ["--skree-metaree-instruction-mechanics"])
{
    Suite(nameof(VerifySkreeMetareeInstructionProgramDefinitions), () => VerifySkreeMetareeInstructionProgramDefinitions());
    return 0;
}
if (args is ["--zoa-instruction-mechanics"])
{
    Suite(nameof(VerifyZoaInstructionProgramDefinitions), () => VerifyZoaInstructionProgramDefinitions());
    return 0;
}
if (args is ["--dragon-instruction-mechanics"])
{
    Suite(nameof(VerifyDragonInstructionProgramDefinitions), () => VerifyDragonInstructionProgramDefinitions());
    return 0;
}
if (args is ["--brinstar-pipe-bug-instruction-mechanics"])
{
    Suite(nameof(VerifyBrinstarPipeBugInstructionProgramDefinitions), () => VerifyBrinstarPipeBugInstructionProgramDefinitions());
    return 0;
}
if (args is ["--norfair-pipe-bug-instruction-mechanics"])
{
    Suite(nameof(VerifyNorfairPipeBugInstructionProgramDefinitions), () => VerifyNorfairPipeBugInstructionProgramDefinitions());
    return 0;
}
if (args is ["--yellow-pipe-bug-instruction-mechanics"])
{
    Suite(nameof(VerifyYellowPipeBugInstructionProgramDefinitions), () => VerifyYellowPipeBugInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ceres-steam-instruction-mechanics"])
{
    Suite(nameof(VerifyCeresSteamInstructionProgramDefinitions), () => VerifyCeresSteamInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ceres-door-instruction-mechanics"])
{
    Suite(nameof(VerifyCeresDoorInstructionProgramDefinitions), () => VerifyCeresDoorInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ceres-door-artwork"])
{
    Suite(nameof(VerifyCeresDoorQuakeDefinitions), () => VerifyCeresDoorQuakeDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--ceres-escape-transfers"])
{
    Suite(nameof(VerifyCeresEscapeVramTransferDefinitions), () => VerifyCeresEscapeVramTransferDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--fake-kraid-instruction-mechanics"])
{
    Suite(nameof(VerifyFakeKraidInstructionProgramDefinitions), () => VerifyFakeKraidInstructionProgramDefinitions());
    return 0;
}
if (args is ["--chozo-statue-instruction-mechanics"])
{
    Suite(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions());
    return 0;
}
if (args is ["--crocomire-tongue-instruction-mechanics"])
{
    Suite(nameof(VerifyCrocomireTongueInstructionProgramDefinitions), () => VerifyCrocomireTongueInstructionProgramDefinitions());
    return 0;
}
if (args is ["--mother-brain-baby-instruction-mechanics"])
{
    Suite(nameof(VerifyMotherBrainBabyInstructionProgramDefinitions), () => VerifyMotherBrainBabyInstructionProgramDefinitions());
    return 0;
}
if (args is ["--botwoon-instruction-mechanics"])
{
    Suite(nameof(VerifyBotwoonInstructionProgramDefinitions), () => VerifyBotwoonInstructionProgramDefinitions());
    return 0;
}
if (args is ["--kraid-lint-instruction-mechanics"])
{
    Suite(nameof(VerifyKraidLintInstructionProgramDefinitions), () => VerifyKraidLintInstructionProgramDefinitions());
    return 0;
}
if (args is ["--ceres-elevator-arrival-definitions"])
{
    Suite(nameof(VerifyCeresElevatorArrivalGraphicsIndex), () => VerifyCeresElevatorArrivalGraphicsIndex());
    return 0;
}
if (args is ["--draygon-eye-effects"])
{
    Suite(nameof(VerifyDraygonEyeEffects), () => VerifyDraygonEyeEffects());
    return 0;
}
if (args is ["--acid-statue-first-entry"])
{
    Suite(nameof(VerifyAcidStatueFirstEntry), () => VerifyAcidStatueFirstEntry());
    return 0;
}
if (args is ["--demo-input-object"])
{
    Suite(nameof(VerifyDemoInputObject), () => VerifyDemoInputObject());
    return 0;
}
if (args is ["--door-alignment"])
{
    Suite(nameof(VerifyDoorOpeningTrajectories), () => VerifyDoorOpeningTrajectories());
    Suite(nameof(VerifyDoorAlignmentParity), () => VerifyDoorAlignmentParity());
    return 0;
}
if (args is ["--spark-crash-alignment"])
{
    Suite(nameof(VerifySparkCrashAlignment), () => VerifySparkCrashAlignment());
    Suite(nameof(VerifyMakeSamusFaceForward), () => VerifyMakeSamusFaceForward());
    return 0;
}
if (args is ["--gate-jump-traces"])
{
    Suite(nameof(VerifyGateJumpTraces), () => VerifyGateJumpTraces());
    return 0;
}
if (args is ["--gate-beam-collision"])
{
    Suite(nameof(VerifyKronicGateBeamCollision), () => VerifyKronicGateBeamCollision());
    return 0;
}
if (args is ["--right-facing-gate-glitch"])
{
    Suite(nameof(VerifyRightFacingGateGlitches), () => VerifyRightFacingGateGlitches());
    return 0;
}
if (args is ["--green-hill-gate-glitch"])
{
    Suite(nameof(VerifyGreenHillGrappleSpeedGateGlitch), () => VerifyGreenHillGrappleSpeedGateGlitch());
    return 0;
}
if (args is ["--gmode-gate-glitch"])
{
    Suite(nameof(VerifyGModeGateGlitch), () => VerifyGModeGateGlitch());
    return 0;
}
if (args is ["--frozen-gate-glitch"])
{
    Suite(nameof(VerifyFrozenEnemyGateGlitch), () => VerifyFrozenEnemyGateGlitch());
    return 0;
}
if (args is ["--mochtroid-botwoon-clip"])
{
    Suite(nameof(VerifyMochtroidBotwoonPipeClip), () => VerifyMochtroidBotwoonPipeClip());
    return 0;
}
if (args is ["--red-tower-hero"])
{
    Suite(nameof(VerifyControlledRedTowerHeroShot), () => VerifyControlledRedTowerHeroShot());
    return 0;
}
if (args is ["--hero-shot-runtime"])
{
    Suite(nameof(VerifyHeroShotRuntimeCamera), () => VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv"));
    return 0;
}
if (args is ["--hero-shot-runtime", var nativeHeroRuntimeTrace])
{
    Suite(nameof(VerifyHeroShotRuntimeCamera), () => VerifyHeroShotRuntimeCamera(nativeHeroRuntimeTrace));
    return 0;
}
if (args is ["--missile-edge"])
{
    Suite(nameof(VerifyMissileImpactCameraEdge), () => VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv"));
    return 0;
}
if (args is ["--wrap-shots"])
{
    Suite(nameof(VerifyWrapShotTrace), () => VerifyWrapShotTrace("csharp/test-fixtures/movement-release/wrap-shot-409.csv"));
    return 0;
}
if (args is ["--ceiling-wrap"])
{
    Suite(nameof(VerifyCeilingWrapPlmTrace), () => VerifyCeilingWrapPlmTrace("csharp/test-fixtures/movement-release/ceiling-plm-410.csv"));
    return 0;
}
if (args is ["--ceiling-wrap-room"])
{
    Suite(nameof(VerifyFrogSpeedwayPoolCollision), () => VerifyFrogSpeedwayPoolCollision());
    return 0;
}
if (args is ["--ceiling-wrap-runtime"])
{
    Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv"));
    return 0;
}
if (args is ["--ceiling-wrap-success"])
{
    Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-run-410.csv", 11, true));
    Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-walk-410.csv", 11, false));
    return 0;
}
if (args is ["--wrap-shot-rooms"])
{
    Suite(nameof(VerifyRetailWrapShotDoors), () => VerifyRetailWrapShotDoors());
    return 0;
}
if (args is ["--wrap-shot-enemies"])
{
    Suite(nameof(VerifyWrapShotEnemySeparation), () => VerifyWrapShotEnemySeparation());
    return 0;
}
if (args is ["--wrap-shot-widths"])
{
    Suite(nameof(VerifyWrapShotWidths), () => VerifyWrapShotWidths("csharp/test-fixtures/movement-release/wrap-width-409.csv"));
    return 0;
}
if (args is ["--hero-shots"])
{
    Suite(nameof(VerifyHeroShotCameraLifetime), () => VerifyHeroShotCameraLifetime());
    return 0;
}
if (args is ["--hero-shots", var nativeHeroTrace])
{
    Suite(nameof(VerifyHeroShotCameraLifetime), () => VerifyHeroShotCameraLifetime(nativeHeroTrace));
    return 0;
}
if (args is ["--projectile-inheritance-probe"])
{
    ProbeProjectileVelocityInheritance();
    return 0;
}
if (args is ["--samus-physics"])
{
    Suite(nameof(VerifySamusPhysicsBatch), () => VerifySamusPhysicsBatch());
    return 0;
}
if (args is ["--samus-projectiles"])
{
    Suite(nameof(VerifySamusPowerBeamProjectiles), () => VerifySamusPowerBeamProjectiles());
    Suite(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows());
    Suite(nameof(VerifyProjectileCooldowns), () => VerifyProjectileCooldowns());
    Suite(nameof(VerifyHeroShotCameraLifetime), () => VerifyHeroShotCameraLifetime("csharp/test-fixtures/movement-release/hero-shot-411.csv"));
    Suite(nameof(VerifyHeroShotRuntimeCamera), () => VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv"));
    Suite(nameof(VerifyMissileImpactCameraEdge), () => VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv"));
    Suite(nameof(VerifyControlledRedTowerHeroShot), () => VerifyControlledRedTowerHeroShot());
    return 0;
}
if (args is ["--projectile-cooldowns"])
{
    Suite(nameof(VerifyProjectileCooldowns), () => VerifyProjectileCooldowns());
    return 0;
}
if (args is ["--projectile-motion"])
{
    Suite(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows());
    return 0;
}
if (args is ["--botwoon-plm-identity"])
{
    Suite(nameof(VerifyBotwoonPlmIdentity), () => VerifyBotwoonPlmIdentity());
    return 0;
}
if (args is ["--compiled-enemy-sine"])
{
    Suite(nameof(VerifyCompiledEnemyTrigonometry), () => VerifyCompiledEnemyTrigonometry());
    return 0;
}
if (args is ["--charge-flare-compositions"])
{
    string flareRom = RepositoryRomPath;
    Suite(nameof(VerifyChargeFlareCompositions), () => VerifyChargeFlareCompositions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath(flareRom)),
        flareOnly: true, sourceRom: flareRom));
    return 0;
}
if (args is ["--projectile-sprite-compositions"])
{
    string projectileRom = RepositoryRomPath;
    var source = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath(projectileRom));
    Suite(nameof(VerifyProjectileVisualParts), () => VerifyProjectileVisualParts());
    Suite(nameof(VerifyProjectileCompositions), () => VerifyProjectileCompositions(source, compositionOnly: true,
        sourceRom: projectileRom));
    return 0;
}
if (args is ["--trail-mutable-alias"])
{
    Suite(nameof(VerifyTrailMutableAlias), () => VerifyTrailMutableAlias());
    return 0;
}
if (args is ["--projectile-trail-coordinates"])
{
    Suite(nameof(VerifyProjectileTrailCoordinates), () => VerifyProjectileTrailCoordinates(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--kraid-mouth-hitboxes"])
{
    Suite(nameof(VerifyKraidMouthHitboxes), () => VerifyKraidMouthHitboxes(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--shaktool-instruction-mechanics"])
{
    Suite(nameof(VerifyShaktoolInstructionProgramDefinitions), () => VerifyShaktoolInstructionProgramDefinitions());
    return 0;
}
if (args is ["--speed-booster-block-plms"])
{
    Suite(nameof(VerifySpeedBoosterCollisionBlocks), () => VerifySpeedBoosterCollisionBlocks());
    return 0;
}

if (args is ["--speed-booster-visuals"])
{
    Suite(nameof(VerifySpeedBoosterVisuals), () => VerifySpeedBoosterVisuals());
    return 0;
}

if (args is ["--botwoon-wall-plms"])
{
    Suite(nameof(VerifyCompiledBotwoonWallPlms), () => VerifyCompiledBotwoonWallPlms());
    return 0;
}
if (args is ["--botwoon-wall-visuals"])
{
    Suite(nameof(VerifyBotwoonWallVisuals), () => VerifyBotwoonWallVisuals());
    return 0;
}
if (args is ["--kraid-room-plms"])
{
    Suite(nameof(VerifyCompiledKraidRoomPlms), () => VerifyCompiledKraidRoomPlms());
    return 0;
}




if (args is ["--kraid-room-visuals"])
{
    Suite(nameof(VerifyKraidRoomVisuals), () => VerifyKraidRoomVisuals());
    return 0;
}
if (args is ["--door-closing-definitions"])
{
    Suite(nameof(VerifyDoorClosingPlmDefinitions), () => VerifyDoorClosingPlmDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--reported-shaft-momentum"])
{
    Suite(nameof(VerifyReportedShaftMomentum), () => VerifyReportedShaftMomentum());
    return 0;
}
if (args is ["--shot-block-program-operands"])
{
    Suite(nameof(VerifyShotBlockProgramOperands), () => VerifyShotBlockProgramOperands());
    return 0;
}
if (args is ["--shot-block-plm-programs"])
{
    Suite(nameof(VerifyShotBlockPlmPrograms), () => VerifyShotBlockPlmPrograms());
    return 0;
}
if (args is ["--grapple-block-programs"])
{
    Suite(nameof(VerifyGrappleBlockPrograms), () => VerifyGrappleBlockPrograms());
    return 0;
}
if (args is ["--bomb-block-programs"])
{
    Suite(nameof(VerifyBombBlockPrograms), () => VerifyBombBlockPrograms());
    return 0;
}
if (args is ["--contact-crumble-programs"])
{
    Suite(nameof(VerifyContactCrumblePrograms), () => VerifyContactCrumblePrograms());
    return 0;
}
if (args is ["--grapple-firing-definitions"])
{
    Suite(nameof(VerifyGrappleFiringDefinitions), () => VerifyGrappleFiringDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--mother-brain-baby-metroid-definitions"])
{
    Suite(nameof(VerifyMotherBrainBabyMetroidDefinitions), () => VerifyMotherBrainBabyMetroidDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--arm-cannon-definitions"])
{
    Suite(nameof(VerifySamusArmCannonDefinitions), () => VerifySamusArmCannonDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--tourian-access-definitions"])
{
    Suite(nameof(VerifyTourianAccessPlmDefinitions), () => VerifyTourianAccessPlmDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--quicksand-definitions"])
{
    Suite(nameof(VerifyQuicksandDefinitions), () => VerifyQuicksandDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Suite(nameof(VerifyQuicksand), () => VerifyQuicksand());
    return 0;
}
if (args is ["--save-station-animation-definitions"])
{
    Suite(nameof(VerifySaveStationAnimationDefinitions), () => VerifySaveStationAnimationDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--enemy-drop-chance-definitions"])
{
    Suite(nameof(VerifyEnemyDropChanceDefinitions), () => VerifyEnemyDropChanceDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--enemy-vulnerability-definitions"])
{
    Suite(nameof(VerifyEnemyVulnerabilityDefinitions), () => VerifyEnemyVulnerabilityDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--escape-etecoon-definitions"])
{
    Suite(nameof(VerifyEscapeEtecoonDefinitions), () => VerifyEscapeEtecoonDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--escape-etecoon-instruction-mechanics"])
{
    Suite(nameof(VerifyEscapeEtecoonInstructionProgramDefinitions), () => VerifyEscapeEtecoonInstructionProgramDefinitions());
    return 0;
}
if (args is ["--escape-dachora-instruction-mechanics"])
{
    Suite(nameof(VerifyEscapeDachoraInstructionProgramDefinitions), () => VerifyEscapeDachoraInstructionProgramDefinitions());
    return 0;
}
if (args is ["--crocomire-instruction-mechanics"])
{
    Suite(nameof(VerifyCrocomireInstructionProgramDefinitions), () => VerifyCrocomireInstructionProgramDefinitions());
    return 0;
}
if (args is ["--yard-turn-definitions"])
{
    Suite(nameof(VerifyYardTurnDefinitions), () => VerifyYardTurnDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--ridley-explosion-definitions"])
{
    Suite(nameof(VerifyRidleyExplosionDefinitions), () => VerifyRidleyExplosionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--choot-pattern-definitions"])
{
    Suite(nameof(VerifyChootPatternDefinitions), () => VerifyChootPatternDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--enemy-instruction-selectors"])
{
    Suite(nameof(VerifyEnemyRomTablePointerCatalog), () => VerifyEnemyRomTablePointerCatalog());
    return 0;
}
if (args is ["--kraid-head-instruction-definitions"])
{
    Suite(nameof(VerifyKraidHeadInstructionDefinitions), () => VerifyKraidHeadInstructionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--magic-number-audit"])
{
    Suite(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit());
    return 0;
}
if (args is ["--save-json-persistence"])
{
    Suite(nameof(VerifyGameSaveJsonPersistence), () => VerifyGameSaveJsonPersistence());
    return 0;
}
if (args is ["--explored-map-packing-definitions"])
{
    Suite(nameof(VerifyExploredMapPackingDefinitions), () => VerifyExploredMapPackingDefinitions());
    return 0;
}
if (args is ["--mutable-animation-aliases"])
{
    string sourceRom = Path.GetFullPath("Super Metroid.smc");
    SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
    Suite(nameof(VerifySamusAnimationDelayDefinitions), () => VerifySamusAnimationDelayDefinitions(bus, sourceRom));
    Suite(nameof(VerifyRunningCadence), () => VerifyRunningCadence(bus));
    return 0;
}
if (args is ["--enemy-death-explosion-definitions"])
{
    Suite(nameof(VerifyEnemyDeathExplosionDefinitions), () => VerifyEnemyDeathExplosionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--ceres-door-initialization-definitions"])
{
    Suite(nameof(VerifyCeresDoorInitializationDefinitions), () => VerifyCeresDoorInitializationDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--botwoon-instruction-definitions"])
{
    Suite(nameof(VerifyBotwoonInstructionDefinitions), () => VerifyBotwoonInstructionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--botwoon-navigation-definitions"])
{
    Suite(nameof(VerifyBotwoonNavigationDefinitions), () => VerifyBotwoonNavigationDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--enemy-projectile-definitions"])
{
    Suite(nameof(VerifyEnemyProjectileDefinitions), () => VerifyEnemyProjectileDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--room-palette-fx-definitions"])
{
    Suite(nameof(VerifyRoomPaletteFxDefinitions), () => VerifyRoomPaletteFxDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--ceres-ridley-eye-fade-definitions"])
{
    Suite(nameof(VerifyCeresRidleyEyeFadeDefinitions), () => VerifyCeresRidleyEyeFadeDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--crystal-flash-palette-timing-definitions"])
{
    Suite(nameof(VerifyCrystalFlashPaletteTimingDefinitions), () => VerifyCrystalFlashPaletteTimingDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--work-robot-palette-timing-definitions"])
{
    Suite(nameof(VerifyWorkRobotPaletteTimingDefinitions), () => VerifyWorkRobotPaletteTimingDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--samus-death-explosion-timing-definitions"])
{
    Suite(nameof(VerifySamusDeathExplosionTimingDefinitions), () => VerifySamusDeathExplosionTimingDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Suite(nameof(VerifySamusDeathSequence), () => VerifySamusDeathSequence());
    return 0;
}
if (args is ["--hyper-beam-palette-fx-program-definitions"])
{
    Suite(nameof(VerifyHyperBeamPaletteFxProgramDefinitions), () => VerifyHyperBeamPaletteFxProgramDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Suite(nameof(VerifySamusDrainedController), () => VerifySamusDrainedController());
    return 0;
}
if (args is ["--suit-pickup-beam-curve-definitions"])
{
    Suite(nameof(VerifySuitPickupBeamCurveDefinitions), () => VerifySuitPickupBeamCurveDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Suite(nameof(VerifyPermanentCollectibles), () => VerifyPermanentCollectibles());
    return 0;
}
if (args is ["--palette-fx-instruction-codes"])
{
    Suite(nameof(VerifyPaletteFxInstructionCodeCatalogs), () => VerifyPaletteFxInstructionCodeCatalogs());
    return 0;
}
if (args is ["--enemy-drops"])
{
    Suite(nameof(VerifyEnemyDrops), () => VerifyEnemyDrops());
    return 0;
}
if (args is ["--room-sprite-object-definitions"])
{
    Suite(nameof(VerifyRoomSpriteObjectDefinitions), () => VerifyRoomSpriteObjectDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--dead-sidehopper-corpse-definitions"])
{
    Suite(nameof(VerifyDeadSidehopperCorpseDefinitions), () => VerifyDeadSidehopperCorpseDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--dead-tourian-corpse-definitions"])
{
    Suite(nameof(VerifyDeadTourianCorpseDefinitions), () => VerifyDeadTourianCorpseDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--dead-torizo-corpse-definitions"])
{
    Suite(nameof(VerifyDeadTorizoCorpseDefinitions), () => VerifyDeadTorizoCorpseDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--gunship-motion-definitions"])
{
    Suite(nameof(VerifyGunshipMotionDefinitions), () => VerifyGunshipMotionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Suite(nameof(VerifyPostCeresGunshipLanding), () => VerifyPostCeresGunshipLanding());
    return 0;
}
if (args is ["--remaining-signed-sine-consumers"])
{
    SuperMetroidAddressSpace rom =
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyBombTorizoDroolSine), () => VerifyBombTorizoDroolSine(rom));
    Suite(nameof(VerifyMotherBrainNeckSine), () => VerifyMotherBrainNeckSine(rom));
    return 0;
}
if (args is ["--crocomire-bridge-fragment-definitions"])
{
    Suite(nameof(VerifyCrocomireBridgeFragmentDefinitions), () => VerifyCrocomireBridgeFragmentDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--crocomire-melting-definitions"])
{
    Suite(nameof(VerifyCrocomireMeltingDefinitions), () => VerifyCrocomireMeltingDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--draygon-intro-dance-definitions"])
{
    Suite(nameof(VerifyDraygonIntroDanceDefinitions), () => VerifyDraygonIntroDanceDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--phantoon-sound-definitions"])
{
    Suite(nameof(VerifyPhantoonSoundDefinitions), () => VerifyPhantoonSoundDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--baby-metroid-route-definitions"])
{
    Suite(nameof(VerifyBabyMetroidCutsceneEntrance), () => VerifyBabyMetroidCutsceneEntrance());
    return 0;
}
if (args is ["--mother-brain-contact-hitboxes"])
{
    Suite(nameof(VerifyMotherBrainContactHitboxes), () => VerifyMotherBrainContactHitboxes());
    return 0;
}
if (args is ["--mother-brain-turret-definitions"])
{
    Suite(nameof(VerifyMotherBrainTurretDefinitions), () => VerifyMotherBrainTurretDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--mama-turtle-shell-contour"])
{
    Suite(nameof(VerifyMamaTurtleShellContourDefinitions), () => VerifyMamaTurtleShellContourDefinitions());
    return 0;
}
if (args is ["--maridia-large-snail-instruction-definitions"])
{
    Suite(nameof(VerifyMaridiaLargeSnailInstructionDefinitions), () => VerifyMaridiaLargeSnailInstructionDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--etecoon-instruction-program-definitions"])
{
    Suite(nameof(VerifyEtecoonInstructionProgramDefinitions), () => VerifyEtecoonInstructionProgramDefinitions());
    return 0;
}
if (args is ["--elevator-instruction-program-definitions"])
{
    Suite(nameof(VerifyElevatorInstructionProgramDefinitions), () => VerifyElevatorInstructionProgramDefinitions());
    return 0;
}
if (args is ["--mochtroid-instruction-program-definitions"])
{
    Suite(nameof(VerifyMochtroidInstructionProgramDefinitions), () => VerifyMochtroidInstructionProgramDefinitions());
    return 0;
}
if (args is ["--platform-instruction-program-definitions"])
{
    Suite(nameof(VerifyPlatformInstructionProgramDefinitions), () => VerifyPlatformInstructionProgramDefinitions());
    return 0;
}
if (args is ["--hopper-instruction-program-definitions"])
{
    Suite(nameof(VerifyHopperAnimationDefinitions), () => VerifyHopperAnimationDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    Suite(nameof(VerifyStream3HopperOperandPositions), () => VerifyStream3HopperOperandPositions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--hzoomer-instruction-program-definitions"])
{
    Suite(nameof(VerifyHZoomerInstructionProgramDefinitions), () => VerifyHZoomerInstructionProgramDefinitions());
    return 0;
}
if (args is ["--sciser-instruction-program-definitions"])
{
    Suite(nameof(VerifySciserInstructionProgramDefinitions), () => VerifySciserInstructionProgramDefinitions());
    return 0;
}
if (args is ["--zero-instruction-program-definitions"])
{
    Suite(nameof(VerifyZeroInstructionProgramDefinitions), () => VerifyZeroInstructionProgramDefinitions());
    return 0;
}
if (args is ["--viola-instruction-program-definitions"])
{
    Suite(nameof(VerifyViolaInstructionProgramDefinitions), () => VerifyViolaInstructionProgramDefinitions());
    return 0;
}
if (args is ["--shared-crawler-instruction-program-definitions"])
{
    Suite(nameof(VerifySharedCrawlerInstructionProgramDefinitions), () => VerifySharedCrawlerInstructionProgramDefinitions());
    return 0;
}
if (args is ["--dachora-instruction-program-definitions"])
{
    Suite(nameof(VerifyDachoraInstructionProgramDefinitions), () => VerifyDachoraInstructionProgramDefinitions());
    return 0;
}
if (args is ["--fireflea-instruction-program-definitions"])
{
    Suite(nameof(VerifyFirefleaInstructionProgramDefinitions), () => VerifyFirefleaInstructionProgramDefinitions());
    return 0;
}
if (args is ["--zebetite-instruction-program-definitions"])
{
    Suite(nameof(VerifyZebetiteInstructionProgramDefinitions), () => VerifyZebetiteInstructionProgramDefinitions());
    return 0;
}
if (args is ["--evir-instruction-program-definitions"])
{
    Suite(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions());
    return 0;
}
if (args is ["--morph-ball-eye-instruction-program-definitions"])
{
    Suite(nameof(VerifyMorphBallEyeInstructionProgramDefinitions), () => VerifyMorphBallEyeInstructionProgramDefinitions());
    return 0;
}
if (args is ["--yapping-maw-instruction-program-definitions"])
{
    Suite(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions());
    return 0;
}
if (args is ["--metroid-instruction-program-definitions"])
{
    Suite(nameof(VerifyMetroidInstructionProgramDefinitions), () => VerifyMetroidInstructionProgramDefinitions());
    return 0;
}
if (args is ["--norfair-rio-instruction-program-definitions"])
{
    Suite(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions());
    return 0;
}
if (args is ["--lower-norfair-rio-instruction-program-definitions"])
{
    Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions());
    return 0;
}
if (args is ["--mama-turtle-instruction-program-definitions"])
{
    Suite(nameof(VerifyMamaTurtleInstructionProgramDefinitions), () => VerifyMamaTurtleInstructionProgramDefinitions());
    return 0;
}
if (args is ["--tourian-entrance-statue-instruction-program-definitions"])
{
    Suite(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions());
    return 0;
}
if (args is ["--kraid-nail-instruction-program-definitions"])
{
    Suite(nameof(VerifyKraidNailInstructionProgramDefinitions), () => VerifyKraidNailInstructionProgramDefinitions());
    return 0;
}
if (args is ["--kraid-arm-instruction-program-definitions"])
{
    Suite(nameof(VerifyKraidArmInstructionProgramDefinitions), () => VerifyKraidArmInstructionProgramDefinitions());
    return 0;
}
if (args is ["--kraid-foot-instruction-program-definitions"])
{
    Suite(nameof(VerifyKraidFootInstructionProgramDefinitions), () => VerifyKraidFootInstructionProgramDefinitions());
    return 0;
}
if (args is ["--phantoon-instruction-program-definitions"])
{
    Suite(nameof(VerifyPhantoonInstructionProgramDefinitions), () => VerifyPhantoonInstructionProgramDefinitions());
    return 0;
}
if (args is ["--work-robot-instruction-program-definitions"])
{
    Suite(nameof(VerifyWorkRobotInstructionProgramDefinitions), () => VerifyWorkRobotInstructionProgramDefinitions());
    return 0;
}
if (args is ["--yard-instruction-program-definitions"])
{
    Suite(nameof(VerifyYardInstructionProgramDefinitions), () => VerifyYardInstructionProgramDefinitions());
    return 0;
}
if (args is ["--mama-turtle-enemy-definitions"])
{
    Suite(nameof(VerifyMamaTurtleEnemyDefinitions), () => VerifyMamaTurtleEnemyDefinitions());
    return 0;
}
if (args is ["--pose-projectile-origins"])
{
    Suite(nameof(VerifyPoseProjectileOrigin), () => VerifyPoseProjectileOrigin(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--pose-dispatch-definitions"])
{
    var poseRom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    Suite(nameof(VerifyPoseDispatchDefinitions), () => VerifyPoseDispatchDefinitions(poseRom));
    Suite(nameof(VerifyPoseCollisionDefinitions), () => VerifyPoseCollisionDefinitions(poseRom));
    Suite(nameof(VerifyPoseProjectileOrigin), () => VerifyPoseProjectileOrigin(poseRom));
    Suite(nameof(VerifySamusHudDefinitions), () => VerifySamusHudDefinitions(poseRom));
    return 0;
}
if (args is ["--pose-input-definitions"])
{
    Suite(nameof(VerifyPoseInputDefinitions), () => VerifyPoseInputDefinitions(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--grapple-rope-geometry"])
{
    Suite(nameof(VerifyGrappleRopeGeometry), () => VerifyGrappleRopeGeometry(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--grapple-sprite-artwork"])
{
    Suite(nameof(VerifyGrappleSpriteArtwork), () => VerifyGrappleSpriteArtwork(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--grapple-tile-artwork"])
{
    Suite(nameof(VerifyGrappleTileArtwork), () => VerifyGrappleTileArtwork(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--grapple-flare-placement"])
{
    Suite(nameof(VerifyGrappleFlarePlacement), () => VerifyGrappleFlarePlacement(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--grapple-swing-frames"])
{
    Suite(nameof(VerifyGrappleBodyPlacement), () => VerifyGrappleBodyPlacement(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--stock-attract-scenes"])
{
    Suite(nameof(VerifyStockAttractScenes), () => VerifyStockAttractScenes());
    return 0;
}
if (args is ["--pause-boss-markers"])
{
    Suite(nameof(VerifyPauseBossMarkers), () => VerifyPauseBossMarkers());
    return 0;
}
if (args is ["--crystal-flash-runtime"])
{
    Suite(nameof(VerifyCrystalFlashRuntime), () => VerifyCrystalFlashRuntime());
    return 0;
}
if (args is ["--crystal-flash"])
{
    Suite(nameof(VerifySamusCrystalFlash), () => VerifySamusCrystalFlash());
    return 0;
}
if (args is ["--plasma-penetration"])
{
    Suite(nameof(VerifyPlasmaEnemyPenetration), () => VerifyPlasmaEnemyPenetration());
    return 0;
}
if (args is ["--pcm-loop-entry"])
{
    Suite(nameof(VerifyManagedDspUsesIndependentLoopEntry), () => VerifyManagedDspUsesIndependentLoopEntry());
    return 0;
}
if (args is ["--audio-bank-transition"])
{
    string audioDirectory = RepositoryInstallation.Installation.AudioDirectory;
    Suite(nameof(VerifyAudioBankTransition), () => VerifyAudioBankTransition(audioDirectory));
    return 0;
}
if (args is ["--wall-spread-transition"])
{
    Suite(nameof(VerifyAerialSpreadTransitions), () => VerifyAerialSpreadTransitions(wallRoute: true));
    return 0;
}
if (args is ["--wall-spread-transition"])
{
    Suite(nameof(VerifyAerialSpreadTransitions), () => VerifyAerialSpreadTransitions(wallRoute: true));
    return 0;
}
if (args is ["--aerial-spread-transition"])
{
    Suite(nameof(VerifyAerialSpreadTransitions), () => VerifyAerialSpreadTransitions());
    return 0;
}
if (args is ["--grounded-spread-transition"])
{
    Suite(nameof(VerifyGroundedSpreadTransition), () => VerifyGroundedSpreadTransition());
    return 0;
}
if (args is ["--grounded-spread-transition", var transitionTrace])
{
    Suite(nameof(VerifyGroundedSpreadTransition), () => VerifyGroundedSpreadTransition(transitionTrace));
    return 0;
}
if (args is ["--grounded-bomb-spread"])
{
    Suite(nameof(VerifyGroundedBombSpread), () => VerifyGroundedBombSpread());
    return 0;
}
if (args is ["--map-installation"])
{
    string installationRom = RepositoryRomPath;
    Suite(nameof(VerifyMapInstallation), () => VerifyMapInstallation(installationRom));
    return 0;
}
if (args is ["--map-presentation"])
{
    Suite(nameof(VerifyMapPresentation), () => VerifyMapPresentation());
    return 0;
}
if (args is ["--escape-timer-pointer-definitions"])
{
    string timerRom = RepositoryRomPath;
    Suite(nameof(VerifyEscapeTimerPointerDefinitions), () => VerifyEscapeTimerPointerDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath(timerRom))));
    return 0;
}
if (args is ["--cartridge-room-state-selection"])
{
    Suite(nameof(VerifyCartridgeRoomStateSelection), () => VerifyCartridgeRoomStateSelection());
    return 0;
}
if (args is ["--room-tileset-definitions"])
{
    Suite(nameof(VerifyRoomAssetRomData), () => VerifyRoomAssetRomData());
    return 0;
}
if (args is ["--room-character-atlases"])
{
    Suite(nameof(VerifyRoomCharacterAtlases), () => VerifyRoomCharacterAtlases());
    return 0;
}
if (args is ["--library-background-loader"])
{
    Suite(nameof(VerifyLibraryBackgroundLoader), () => VerifyLibraryBackgroundLoader());
    return 0;
}
if (args is ["--library-background-artwork-boundary"])
{
    string libraryArtworkRom = RepositoryRomPath;
    using var directory = new TestTempDirectory("map-catalog");
    var installation = GameAssetInstaller.Install(libraryArtworkRom, directory.Root);
    Suite(nameof(VerifyLibraryBackgroundLoader), () => VerifyLibraryBackgroundLoader());
    Suite(nameof(VerifyLibraryBackgroundInstalledParity), () => VerifyLibraryBackgroundInstalledParity(libraryArtworkRom, installation));
    return 0;
}
if (args is ["--oam-source-routing"])
{
    Suite(nameof(VerifyOamSpritemapPacking), () => VerifyOamSpritemapPacking());
    return 0;
}
if (args is ["--room-enemy-loading"])
{
    Suite(nameof(VerifyRoomEnemyLoading), () => VerifyRoomEnemyLoading());
    return 0;
}
if (args is ["--enemy-tile-artwork"])
{
    Suite(nameof(VerifyEnemyTileArtwork), () => VerifyEnemyTileArtwork());
    Suite(nameof(VerifyEnemyMappedSourceRouting), () => VerifyEnemyMappedSourceRouting());
    Suite(nameof(VerifyRoomEnemyLoading), () => VerifyRoomEnemyLoading());
    return 0;
}
if (args is ["--enemy-source-routing"])
{
    Suite(nameof(VerifyEnemyMappedSourceRouting), () => VerifyEnemyMappedSourceRouting());
    return 0;
}
if (args is ["--enemy-visual-selector-inventory"])
{
    InspectEnemyVisualSelectors();
    return 0;
}
if (args is ["--verify-enemy-visual-selectors"])
{
    Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
    return 0;
}
if (args is ["--verify-space-pirate-collision"])
{
    Suite(nameof(VerifySpacePirateCollisionDefinitions), () => VerifySpacePirateCollisionDefinitions());
    return 0;
}
if (args is ["--verify-ridley-collision"])
{
    Suite(nameof(VerifyRidleyCollisionDefinitions), () => VerifyRidleyCollisionDefinitions());
    return 0;
}
if (args is ["--verify-ceres-steam-collision"])
{
    Suite(nameof(VerifyCeresSteamCollisionDefinitions), () => VerifyCeresSteamCollisionDefinitions());
    return 0;
}
if (args is ["--verify-oum-collision"])
{
    Suite(nameof(VerifyMaridiaLargeSnailCollisionDefinitions), () => VerifyMaridiaLargeSnailCollisionDefinitions());
    return 0;
}
if (args is ["--verify-crocomire-tongue-collision"])
{
    Suite(nameof(VerifyCrocomireTongueCollisionDefinitions), () => VerifyCrocomireTongueCollisionDefinitions());
    return 0;
}
if (args is ["--verify-crocomire-body-collision"])
{
    Suite(nameof(VerifyCrocomireBodyCollisionDefinitions), () => VerifyCrocomireBodyCollisionDefinitions());
    return 0;
}
if (args is ["--kraid-foot-collision"])
{
    Suite(nameof(VerifyKraidFootCollisionDefinitions), () => VerifyKraidFootCollisionDefinitions());
    return 0;
}
if (args is ["--projectile-frame-bindings"])
{
    Suite(nameof(VerifyProjectileFrameBindings), () => VerifyProjectileFrameBindings(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    return 0;
}
if (args is ["--boss-reset-on-load"])
{
    Suite(nameof(VerifyBossResetOnLoad), () => VerifyBossResetOnLoad());
    return 0;
}
if (args is ["--speed-palette-overrun"]){
    Suite(nameof(VerifySpeedPaletteOverrun), () => VerifySpeedPaletteOverrun());
    return 0;
}
if (args is ["--pause-map-position"]){
    Suite(nameof(VerifyPauseMapPosition), () => VerifyPauseMapPosition());
    Suite(nameof(VerifyPauseHudLocation), () => VerifyPauseHudLocation());
    return 0;
}
if (args is ["--pause-map-area-labels"])
{
    Suite(nameof(VerifyPauseMapAreaLabels), () => VerifyPauseMapAreaLabels());
    return 0;
}
if (args is ["--pause-map-arrows"])
{
    Suite(nameof(VerifyPauseMapArrows), () => VerifyPauseMapArrows());
    return 0;
}
if (args is ["--pause-equipment-interaction-fixture"])
{
    Suite(nameof(VerifyPauseMenuEquipmentInteraction), () => VerifyPauseMenuEquipmentInteraction());
    return 0;
}
if (args is ["--intro-artwork-components-fixture"])
{
    Suite(nameof(VerifyIntroCinematicArtwork), () => VerifyIntroCinematicArtwork(Path.GetFullPath("Super Metroid.smc"), presentationOnly: true));
    return 0;
}
if (args is ["--intro-cinematic-artwork"])
{
    string introBackgroundRom = RepositoryRomPath;
    Suite(nameof(VerifyIntroCinematicArtwork), () => VerifyIntroCinematicArtwork(introBackgroundRom));
    return 0;
}
if (args is ["--samus-body-artwork"])
{
    string samusBodyRom = RepositoryRomPath;
    Suite(nameof(VerifyIntroCinematicArtwork), () => VerifyIntroCinematicArtwork(samusBodyRom, samusBodyOnly: true));
    return 0;
}
if (args is ["--generic-sprite-artwork"])
{
    string genericSpriteRom = RepositoryRomPath;
    Suite(nameof(VerifyGenericSpriteArtwork), () => VerifyGenericSpriteArtwork(genericSpriteRom));
    return 0;
}
if (args is ["--enemy-sprite-artwork-boundary"])
{
    string enemySpriteRom = RepositoryRomPath;
    Suite(nameof(VerifyEnemySpriteArtworkBoundary), () => VerifyEnemySpriteArtworkBoundary(enemySpriteRom));
    return 0;
}
if (args is ["--golden-torizo-rom-free"])
{
    string goldenTorizoRom = RepositoryRomPath;
    Suite(nameof(VerifyFrontendRomFreeGoldenTorizo), () => VerifyFrontendRomFreeGoldenTorizo(goldenTorizoRom, frameCount: 500));
    return 0;
}
if (args is ["--rom-free-startup"])
{
    Suite(nameof(VerifyFrontendRomFreeStartup), () => VerifyFrontendRomFreeStartup(RepositoryInstallation.Installation, RepositoryRomPath));
    return 0;
}
if (args is ["--rom-free-room-census"])
{
    Suite(nameof(VerifyFrontendRomFreeStartup), () => VerifyFrontendRomFreeStartup(RepositoryInstallation.Installation, RepositoryRomPath, roomCensus: true));
    return 0;
}
if (args is ["--rom-free-direct-room"])
{
    Suite(nameof(VerifyFrontendRomFreeDirectRoom), () => VerifyFrontendRomFreeDirectRoom());
    return 0;
}
if (args is ["--gameplay-base-palettes"])
{
    string gameplayPaletteRom = RepositoryRomPath;
    Suite(nameof(VerifyGameplayBasePalettes), () => VerifyGameplayBasePalettes(gameplayPaletteRom));
    return 0;
}
if (args is ["--standard-object-artwork"])
{
    string standardObjectRom = RepositoryRomPath;
    Suite(nameof(VerifyStandardObjectArtwork), () => VerifyStandardObjectArtwork(standardObjectRom));
    return 0;
}
if (args is ["--room-character-installation"])
{
    string roomCharacterRom = RepositoryRomPath;
    Suite(nameof(VerifyRoomArtworkInstallation), () => VerifyRoomArtworkInstallation(roomCharacterRom));
    return 0;
}
if (args is ["--room-artwork-installation"])
{
    string roomArtworkRom = RepositoryRomPath;
    Suite(nameof(VerifyRoomArtworkInstallation), () => VerifyRoomArtworkInstallation(roomArtworkRom));
    return 0;
}
if (args is ["--room-static-palettes"])
{
    Suite(nameof(VerifyRoomStaticPaletteExtraction), () => VerifyRoomStaticPaletteExtraction());
    return 0;
}
if (args is ["--room-metatiles"])
{
    Suite(nameof(VerifyRoomMetatileExtraction), () => VerifyRoomMetatileExtraction());
    return 0;
}
if (args is ["--library-background-inventory"])
{
    Suite(nameof(VerifyLibraryBackgroundSourceInventory), () => VerifyLibraryBackgroundSourceInventory());
    return 0;
}
if (args is ["--room-background-tilemaps"])
{
    Suite(nameof(VerifyRoomBackgroundTilemapExtraction), () => VerifyRoomBackgroundTilemapExtraction());
    return 0;
}
if (args is ["--room-sky-tilemaps"])
{
    Suite(nameof(VerifyScrollingSkyState), () => VerifyScrollingSkyState());
    Suite(nameof(VerifyRoomSkyTilemaps), () => VerifyRoomSkyTilemaps());
    return 0;
}
if (args is ["--room-state-payloads"])
{
    Suite(nameof(VerifyCompiledRoomStateDefinitions), () => VerifyCompiledRoomStateDefinitions());
    return 0;
}
if (args is ["--enemy-definitions"])
{
    Suite(nameof(VerifyCompiledEnemyDefinitions), () => VerifyCompiledEnemyDefinitions());
    return 0;
}
if (args is ["--enemy-room-lists"])
{
    Suite(nameof(VerifyCompiledEnemyRoomLists), () => VerifyCompiledEnemyRoomLists());
    return 0;
}
if (args is ["--room-scroll-definitions"])
{
    Suite(nameof(VerifyCompiledRoomScrollDefinitions), () => VerifyCompiledRoomScrollDefinitions());
    return 0;
}
if (args is ["--room-callback-definitions"])
{
    Suite(nameof(VerifyCompiledRoomCallbackDefinitions), () => VerifyCompiledRoomCallbackDefinitions());
    return 0;
}
if (args is ["--room-definition-integration"])
{
    Suite(nameof(VerifyCompiledRoomDefinitionIntegration), () => VerifyCompiledRoomDefinitionIntegration());
    Suite(nameof(VerifyPostCeresLandingHandOff), () => VerifyPostCeresLandingHandOff());
    return 0;
}
if (args is ["--gameplay-message-titles"])
{
    string messageTitleRom = RepositoryRomPath;
    Suite(nameof(VerifyGameplayMessageTitles), () => VerifyGameplayMessageTitles(messageTitleRom));
    return 0;
}
if (args is ["--gameplay-message-panels"])
{
    string messagePanelRom = RepositoryRomPath;
    Suite(nameof(VerifyGameplayMessagePanels), () => VerifyGameplayMessagePanels(messagePanelRom));
    return 0;
}
if (args is ["--gameplay-message-notices"])
{
    string messageNoticeRom = RepositoryRomPath;
    Suite(nameof(VerifyGameplayMessageNotices), () => VerifyGameplayMessageNotices(messageNoticeRom));
    return 0;
}
if (args is ["--gameplay-message-definitions"])
{
    return 0;
}
if (args is ["--escape-typewriter-presentation"])
{
    string escapeTextRom = RepositoryRomPath;
    Suite(nameof(VerifyEscapeTypewriterPresentation), () => VerifyEscapeTypewriterPresentation(escapeTextRom));
    return 0;
}
if (args is ["--intro-narration-presentation"])
{
    string narrationRom = RepositoryRomPath;
    Suite(nameof(VerifyIntroNarrationPresentation), () => VerifyIntroNarrationPresentation(narrationRom));
    return 0;
}
if (args is ["--ending-text-presentation"])
{
    string endingTextRom = RepositoryRomPath;
    Suite(nameof(VerifyEndingTextPresentation), () => VerifyEndingTextPresentation(endingTextRom));
    return 0;
}
if (args is ["--credits-presentation"])
{
    string creditsRom = RepositoryRomPath;
    Suite(nameof(VerifyCreditsPresentation), () => VerifyCreditsPresentation(creditsRom));
    return 0;
}
if (args is ["--pause-reserve-hud"])
{
    Suite(nameof(VerifyPauseReserveHud), () => VerifyPauseReserveHud());
    return 0;
}
if (args is ["--reserve-auto-frontend"])
{
    Suite(nameof(VerifyReserveAutoFrontend), () => VerifyReserveAutoFrontend());
    return 0;
}
if (args is ["--health-warning"])
{
    Suite(nameof(VerifyHealthWarning), () => VerifyHealthWarning());
    return 0;
}
if (args is ["--reserve-mode"])
{
    Suite(nameof(VerifyReserveMode), () => VerifyReserveMode("csharp/test-fixtures/movement-release/reserve-mode-433.csv"));
    return 0;
}
if (args is ["--cinematic-flash"])
{
    Suite(nameof(VerifyCinematicCrystalFlash), () => VerifyCinematicCrystalFlash("csharp/test-fixtures/movement-release/cinematic-flash-432.csv"));
    return 0;
}
if (args is ["--pause-reserve-manual"])
{
    Suite(nameof(VerifyPauseReserveManual), () => VerifyPauseReserveManual());
    return 0;
}
if (args is ["--pause-reserve-arrow"])
{
    Suite(nameof(VerifyPauseReserveArrow), () => VerifyPauseReserveArrow());
    Suite(nameof(VerifyPauseReserveArrowRebindKeepsLatch), () => VerifyPauseReserveArrowRebindKeepsLatch());
    return 0;
}
if (args is ["--pause-reserve-tanks"])
{
    Suite(nameof(VerifyPauseReserveTanks), () => VerifyPauseReserveTanks());
    return 0;
}
if (args is ["--draygon-tilemap-production"])
{
    Suite(nameof(VerifyDraygonTilemapProduction), () => VerifyDraygonTilemapProduction());
    return 0;
}
if (args is ["--projectile-runtime-phase"])
{
    Suite(nameof(VerifyProjectileRuntimePhase), () => VerifyProjectileRuntimePhase());
    return 0;
}
if (args is ["--projectile-contact-phase"] or ["--projectile-contact-damage"])
{
    Suite(nameof(VerifyProjectileContactPhase), () => VerifyProjectileContactPhase(args[0] == "--projectile-contact-phase"));
    return 0;
}
if (args is ["--enemy-contact-phase"])
{
    Suite(nameof(VerifyRipperEnemy), () => VerifyRipperEnemy(verifyDeferredContact: true));
    return 0;
}
if (args is ["--ripper-enemy"])
{
    Suite(nameof(VerifyRipperEnemy), () => VerifyRipperEnemy());
    return 0;
}
if (args is ["--ceres-elevator-platform"])
{
    Suite(nameof(VerifyCeresElevatorPlatformAnimation), () => VerifyCeresElevatorPlatformAnimation());
    return 0;
}
if (args is ["--ceres-door-boss"])
{
    Suite(nameof(VerifyCeresDoorBossBranch), () => VerifyCeresDoorBossBranch());
    return 0;
}
if (args is ["--x-plasma-timers"])
{
    Suite(nameof(VerifyRipperEnemy), () => VerifyRipperEnemy(verifyXrayTimers: true));
    return 0;
}
if (args is ["--window-pixels"])
{
    Suite(nameof(VerifyWindowPixels), () => VerifyWindowPixels());
    Suite(nameof(VerifyGameplaySnapshots), () => VerifyGameplaySnapshots());
    Suite(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit());
    return 0;
}
if (args is ["--hardware-windows"])
{
    Suite(nameof(VerifyHardwareWindows), () => VerifyHardwareWindows());
    Suite(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit());
    return 0;
}
if (args is ["--dma-source-routing"])
{
    Suite(nameof(VerifyVramWriteQueue), () => VerifyVramWriteQueue());
    Suite(nameof(VerifyDmaSourceRouting), () => VerifyDmaSourceRouting());
    Suite(nameof(VerifyQueuedVramAssets), () => VerifyQueuedVramAssets());
    return 0;
}
if (args is ["--dma-artwork-boundary"])
{
    string dmaArtworkRom = RepositoryRomPath;
    Suite(nameof(VerifyDmaArtworkBoundary), () => VerifyDmaArtworkBoundary(dmaArtworkRom));
    return 0;
}
if (args is ["--file-select-map-entry"])
{
    Suite(nameof(VerifyFileSelectMapEntry), () => VerifyFileSelectMapEntry());
    return 0;
}
if (args is ["--intro-artwork-post-slices"])
{
    string postSliceRom = RepositoryRomPath;
    Suite(nameof(VerifyIntroArtworkPostSlices), () => VerifyIntroArtworkPostSlices(postSliceRom));
    return 0;
}
if (args is ["--power-bomb-fuse"])
{
    Suite(nameof(VerifyPowerBombFuse), () => VerifyPowerBombFuse());
    Suite(nameof(VerifyPowerBombBoundary), () => VerifyPowerBombBoundary());
    Suite(nameof(VerifyPowerBombRuntimeRendererIntegration), () => VerifyPowerBombRuntimeRendererIntegration());
    Suite(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit());
    return 0;
}
if (args is ["--beam-callback-tables"])
{
    Suite(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables());
    return 0;
}
if (args is ["--beam-speed-rows"])
{
    Suite(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows());
    Suite(nameof(VerifySamusPowerBeamProjectiles), () => VerifySamusPowerBeamProjectiles());
    Suite(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit());
    return 0;
}
if (args is ["--chainsaw-firing"])
{
    Suite(nameof(VerifyChainsawFiring), () => VerifyChainsawFiring());
    return 0;
}
if (args is ["--spacetime-beam"])
{
    Suite(nameof(VerifySpacetimeBeam), () => VerifySpacetimeBeam());
    Suite(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit());
    return 0;
}
if (args is ["--murder-beam"])
{
    Suite(nameof(VerifyMurderBeam), () => VerifyMurderBeam());
    Suite(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit());
    return 0;
}
if (args is ["--invalid-beam-graphics"])
{
    Suite(nameof(VerifyInvalidBeamGraphics), () => VerifyInvalidBeamGraphics());
    Suite(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit());
    return 0;
}
if (args is ["--invalid-beam-selection"])
{
    Suite(nameof(VerifyPauseMenuEquipmentInteraction), () => VerifyPauseMenuEquipmentInteraction());
    Suite(nameof(VerifyInvalidBeamSelection), () => VerifyInvalidBeamSelection());
    return 0;
}
if (args is ["--spin-entry-audio"])
{
    Suite(nameof(VerifySamusSpaceJumpAndScrewAttack), () => VerifySamusSpaceJumpAndScrewAttack());
    Suite(nameof(VerifySamusAtmosphericEffects), () => VerifySamusAtmosphericEffects());
    return 0;
}
if (args is ["--boost-floor-scroll"])
{
    Suite(nameof(VerifyBoostFloorScroll), () => VerifyBoostFloorScroll());
    return 0;
}
if (args is ["--xray-controls"])
{
    Suite(nameof(VerifyXrayControls), () => VerifyXrayControls());
    return 0;
}
if (args is ["--elevatube-scrolling"])
{
    Suite(nameof(VerifyElevatubeScrolling), () => VerifyElevatubeScrolling());
    return 0;
}
if (args is ["--maridia-puyo-pile"])
{
    Suite(nameof(VerifyMaridiaPuyoPile), () => VerifyMaridiaPuyoPile());
    return 0;
}
if (args is ["--moat-first-entry"])
{
    Suite(nameof(VerifyMoatFirstEntry), () => VerifyMoatFirstEntry());
    return 0;
}
if (args is ["--pillar-first-entry"])
{
    Suite(nameof(VerifyPillarFirstEntry), () => VerifyPillarFirstEntry());
    return 0;
}
if (args is ["--ridley-acid"])
{
    Suite(nameof(VerifyRidleyAcid), () => VerifyRidleyAcid());
    return 0;
}
if (args is ["--treadmill-visual"])
{
    Suite(nameof(VerifyTreadmillVisual), () => VerifyTreadmillVisual());
    return 0;
}
if (args is ["--metroid-bomb-placement"])
{
    Suite(nameof(VerifyMetroidBombPlacement), () => VerifyMetroidBombPlacement());
    return 0;
}
if (args is ["--fireflea-eye"])
{
    Suite(nameof(VerifyFirefleaEye), () => VerifyFirefleaEye());
    return 0;
}
if (args is ["--yard-trajectories"])
{
    Suite(nameof(VerifyYardTrajectories), () => VerifyYardTrajectories());
    return 0;
}
if (args is ["--statue-splash"])
{
    Suite(nameof(VerifyStatueSplash), () => VerifyStatueSplash());
    return 0;
}
if (args is ["--projectile-quake"])
{
    Suite(nameof(VerifyProjectileQuake), () => VerifyProjectileQuake());
    return 0;
}
if (args is ["--pause-reserve-labels"])
{
    Suite(nameof(VerifyPauseReserveLabels), () => VerifyPauseReserveLabels());
    return 0;
}
if (args is ["--draygon-goop-drops"])
{
    Suite(nameof(VerifyDraygonGoopDrops), () => VerifyDraygonGoopDrops());
    return 0;
}
if (args is ["--ending-takeoff-wrap"] or ["--ending-takeoff-math"])
{
    Suite(nameof(VerifyEndingTakeoffColorMath), () => VerifyEndingTakeoffColorMath(args[0] == "--ending-takeoff-wrap"));
    return 0;
}
if (args is ["--ending-planet-boundary"])
{
    Suite(nameof(VerifyEndingPlanetBoundary), () => VerifyEndingPlanetBoundary());
    return 0;
}
if (args is ["--ending-native-ppu"])
{
    VerifyEndingNativePpu([512], burst: false);
    return 0;
}
if (args is ["--ending-native-finale"])
{
    VerifyEndingNativePpu(Enumerable.Range(0, 19).Select(step => 512 + step * 16), burst: false);
    return 0;
}
if (args is ["--ending-native-burst"])
{
    VerifyEndingNativePpu(Enumerable.Range(0, 5).Select(step => 400 + step * 16), burst: true);
    return 0;
}
if (args is ["--ending-reward-definitions"])
{
    Suite(nameof(VerifyEndingRewardGesture), () => VerifyEndingRewardGesture());
    return 0;
}
if (args is ["--intro-mother-brain-definitions"])
{
    Suite(nameof(VerifyIntroMotherBrainDefinitions), () => VerifyIntroMotherBrainDefinitions());
    return 0;
}
if (args is ["--intro-rinka-definitions"])
{
    Suite(nameof(VerifyIntroRinkaDefinitions), () => VerifyIntroRinkaDefinitions());
    return 0;
}
if (args is ["--intro-baby-actor-definitions"])
{
    Suite(nameof(VerifyIntroBabyActorDefinitions), () => VerifyIntroBabyActorDefinitions());
    return 0;
}
if (args is ["--intro-egg-effect-definitions"])
{
    Suite(nameof(VerifyIntroEggEffectDefinitions), () => VerifyIntroEggEffectDefinitions());
    return 0;
}
if (args is ["--ceres-explosion-definitions"])
{
    Suite(nameof(VerifyCeresExplosionDefinitions), () => VerifyCeresExplosionDefinitions());
    return 0;
}
if (args is ["--ceres-flight-actor-definitions"])
{
    Suite(nameof(VerifyCeresFlightActorDefinitions), () => VerifyCeresFlightActorDefinitions());
    return 0;
}
if (args is ["--room-fx-fixture"])
{
    Suite(nameof(VerifyRoomFxRomData), () => VerifyRoomFxRomData());
    return 0;
}
if (args is ["--ceres-destruction"])
{
    Suite(nameof(VerifyCeresDestructionCinematic), () => VerifyCeresDestructionCinematic());
    return 0;
}
if (args is ["--ceres-destruction-actor-definitions"])
{
    Suite(nameof(VerifyCeresDestructionActorDefinitions), () => VerifyCeresDestructionActorDefinitions());
    return 0;
}
if (args.Contains("--ending-dma"))
{
    Suite(nameof(VerifyEndingDma), () => VerifyEndingDma());
    Suite(nameof(VerifyEndingRenderSnapshots), () => VerifyEndingRenderSnapshots());
    return 0;
}
if (args.Length == 2 && args[0] == "--mother-brain-health")
{
    Suite(nameof(VerifyMotherBrainHealthPalette), () => VerifyMotherBrainHealthPalette(args[1]));
    return 0;
}
if (args.Length == 2 && args[0] == "--escape-animals")
{
    Suite(nameof(VerifyEscapeAnimalBlocks), () => VerifyEscapeAnimalBlocks(args[1]));
    Suite(nameof(VerifyEscapeRoomEffects), () => VerifyEscapeRoomEffects(args[1]));
    return 0;
}
if (args.Contains("--escape-timer"))
{
    Suite(nameof(VerifyEscapeTimerBcd), () => VerifyEscapeTimerBcd());
    Suite(nameof(VerifyEscapeTimerStateMachine), () => VerifyEscapeTimerStateMachine());
    Suite(nameof(VerifyEscapeTimerFloor), () => VerifyEscapeTimerFloor());
    Suite(nameof(VerifyControllerInputRecording), () => VerifyControllerInputRecording());
    return 0;
}
Suite(nameof(VerifyAndroidHostPolicies), () => VerifyAndroidHostPolicies());
Suite(nameof(VerifyBoostFloorScroll), () => VerifyBoostFloorScroll());
Suite(nameof(VerifyXrayControls), () => VerifyXrayControls());
Suite(nameof(VerifyIntroPoseHistory), () => VerifyIntroPoseHistory());
Suite(nameof(VerifyCinematicTextGlow), () => VerifyCinematicTextGlow());
Suite(nameof(VerifyProjectileContactPhase), () => VerifyProjectileContactPhase(verifyPhase: true));
Suite(nameof(VerifyProjectileRuntimePhase), () => VerifyProjectileRuntimePhase());
Suite(nameof(VerifyMorphedSpikeRelease), () => VerifyMorphedSpikeRelease());
Suite(nameof(VerifySpikeShinesparkSuit), () => VerifySpikeShinesparkSuit());
Suite(nameof(VerifyReserveMode), () => VerifyReserveMode("csharp/test-fixtures/movement-release/reserve-mode-433.csv"));
Suite(nameof(VerifyCinematicCrystalFlash), () => VerifyCinematicCrystalFlash("csharp/test-fixtures/movement-release/cinematic-flash-432.csv"));
Suite(nameof(VerifyElevatubeScrolling), () => VerifyElevatubeScrolling());
Suite(nameof(VerifyIniEditing), () => VerifyIniEditing());
Suite(nameof(VerifyBackgroundSampler), () => VerifyBackgroundSampler());
if (args is ["--ini-edit"]) return 0;
if (args is ["--ceres-ridley-room-entry"])
{
    Suite(nameof(VerifyCeresRidleyRoomEntry), () => VerifyCeresRidleyRoomEntry());
    return 0;
}
if (args is ["--ceres-escape-handoff"])
{
    VerifyCeresEscapeHandoff();
    return 0;
}
if (args is ["--mother-brain"])
{
    Suite(nameof(VerifyMotherBrainHandBeamBodyInstructionDefinitions), () => VerifyMotherBrainHandBeamBodyInstructionDefinitions());
    Suite(nameof(VerifyMotherBrainFallingTubeInstructionDefinitions), () => VerifyMotherBrainFallingTubeInstructionDefinitions());
    Suite(nameof(VerifyMotherBrainHeadInstructionProgramDefinitions), () => VerifyMotherBrainHeadInstructionProgramDefinitions());
    Suite(nameof(VerifyMotherBrainBeamWindow), () => VerifyMotherBrainBeamWindow());
    Suite(nameof(VerifyMotherBrainDeathHandoff), () => VerifyMotherBrainDeathHandoff());
    Suite(nameof(VerifySamusDrainedController), () => VerifySamusDrainedController());
    Suite(nameof(VerifyMotherBrainRainbowBeamSamusMovement), () => VerifyMotherBrainRainbowBeamSamusMovement());
    Suite(nameof(VerifyMotherBrainRainbowBeamAttackSequence), () => VerifyMotherBrainRainbowBeamAttackSequence());
    Suite(nameof(VerifyEnemyProjectileInstructionMechanicsDefinitions), () => VerifyEnemyProjectileInstructionMechanicsDefinitions());
    Suite(nameof(VerifyBabyMetroidCutsceneEntrance), () => VerifyBabyMetroidCutsceneEntrance());
    return 0;
}
if (args is ["--mother-brain-death-handoff"])
{
    Suite(nameof(VerifyMotherBrainDeathHandoff), () => VerifyMotherBrainDeathHandoff());
    return 0;
}
if (args is ["--mother-brain-transfer-sources"])
{
    Suite(nameof(VerifyMotherBrainSpriteTransferSources), () => VerifyMotherBrainSpriteTransferSources());
    return 0;
}
if (args is ["--grapple-movement"])
{
    Suite(nameof(VerifySamusGrappleRomData), () => VerifySamusGrappleRomData());
    Suite(nameof(VerifySamusGrappleSwingAndRelease), () => VerifySamusGrappleSwingAndRelease());
    return 0;
}
if (args is ["--shinespark"])
{
    Suite(nameof(VerifySamusStoredShineAndShinespark), () => VerifySamusStoredShineAndShinespark());
    Console.WriteLine("PASS shinespark: native movement, energy cutoff, invincibility, and crash lifecycle.");
    return 0;
}
if (args is ["--file-select-sound"])
{
    Suite(nameof(VerifyFileSelectSound), () => VerifyFileSelectSound());
    return 0;
}
if (args is ["--enemy-contact-death"])
{
    Suite(nameof(VerifyContactDeathStopsEnemyDispatch), () => VerifyContactDeathStopsEnemyDispatch());
    return 0;
}
if (args is ["--android-host"])
    return 0;
if (args is ["--audio-queues"])
{
    Suite(nameof(VerifyCartridgeAudioQueues), () => VerifyCartridgeAudioQueues());
    return 0;
}
if (args is ["--audio-instruments"])
{
    Suite(nameof(VerifyEditableAudioInstruments), () => VerifyEditableAudioInstruments());
    return 0;
}
if (args is ["--audio-overrides"])
{
    Suite(nameof(VerifyPersistentAudioOverrides), () => VerifyPersistentAudioOverrides());
    return 0;
}
if (args is ["--audio-sfx-programs"])
{
    Suite(nameof(VerifyEditableSoundEffectPrograms), () => VerifyEditableSoundEffectPrograms());
    return 0;
}
if (args is ["--audio-music-programs"])
{
    Suite(nameof(VerifyEditableMusicPrograms), () => VerifyEditableMusicPrograms());
    return 0;
}
if (args is ["--bomb-wall"])
{
    Suite(nameof(VerifyBombJumpWallContact), () => VerifyBombJumpWallContact());
    return 0;
}
if (args is ["--shutter-morph-repro"])
{
    Suite(nameof(VerifyShutterMorphBombArc), () => VerifyShutterMorphBombArc());
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
    Suite(nameof(VerifyOceanSky), () => VerifyOceanSky());
    return 0;
}
if (args is ["--shutter-riding"])
{
    Suite(nameof(VerifyShutterRiding), () => VerifyShutterRiding());
    return 0;
}
if (args is ["--shutter-embedding"])
{
    Suite(nameof(VerifyShutterEmbedding), () => VerifyShutterEmbedding());
    return 0;
}
if (args is ["--xray-input"])
{
    Suite(nameof(VerifyXrayInput), () => VerifyXrayInput());
    return 0;
}
if (args is ["--xray-setup"])
{
    Suite(nameof(VerifyXraySetupBuffers), () => VerifyXraySetupBuffers());
    return 0;
}
if (args is ["--fireflea-fx"])
{
    Suite(nameof(VerifyFirefleaFx), () => VerifyFirefleaFx());
    return 0;
}
if (args is ["--xray-window-geometry"])
{
    Suite(nameof(VerifyXrayWindowGeometry), () => VerifyXrayWindowGeometry());
    return 0;
}
if (args is ["--xray-reveal"])
{
    Suite(nameof(VerifyXrayRoomDisplayRules), () => VerifyXrayRoomDisplayRules());
    Suite(nameof(VerifyXrayRevealTable), () => VerifyXrayRevealTable());
    Suite(nameof(VerifyXrayExtensions), () => VerifyXrayExtensions());
    Suite(nameof(VerifyXrayTilemap), () => VerifyXrayTilemap());
    Suite(nameof(VerifyXrayOverlays), () => VerifyXrayOverlays());
    return 0;
}
if (args is ["--grapple-enemy-death"])
{
    Suite(nameof(VerifyGrappleEnemyDeath), () => VerifyGrappleEnemyDeath());
    return 0;
}
if (args is ["--samus-grapple"])
{
    Suite(nameof(VerifySamusGrappleSwingAndRelease), () => VerifySamusGrappleSwingAndRelease());
    return 0;
}
if (args is ["--samus-xray"])
{
    Suite(nameof(VerifySamusXray), () => VerifySamusXray());
    return 0;
}
if (args is ["--grapple-resident-trigger"])
{
    Suite(nameof(VerifyNoobTubePlm), () => VerifyNoobTubePlm());
    Suite(nameof(VerifyPermanentCollectibles), () => VerifyPermanentCollectibles());
    return 0;
}
if (args is ["--lower-norfair-hand"])
{
    Suite(nameof(VerifyLowerNorfairHand), () => VerifyLowerNorfairHand());
    return 0;
}
if (args is ["--draygon-defeated-room"])
{
    Suite(nameof(VerifyDraygonDefeatedRoom), () => VerifyDraygonDefeatedRoom());
    return 0;
}
if (args is ["--grapple-gates"])
{
    Suite(nameof(VerifyGrappleGreenGateVisibility), () => VerifyGrappleGreenGateVisibility());
    return 0;
}
if (args is ["--grapple-spin"])
{
    Suite(nameof(VerifyGrappleSpinInput), () => VerifyGrappleSpinInput());
    return 0;
}
if (args is ["--grapple-sounds"])
{
    Suite(nameof(VerifyGrappleSounds), () => VerifyGrappleSounds());
    return 0;
}
if (args is ["--grapple-doors"])
{
    Suite(nameof(VerifyGrappleBlueDoors), () => VerifyGrappleBlueDoors());
    Suite(nameof(VerifyGrapplePoseRefire), () => VerifyGrapplePoseRefire());
    return 0;
}
if (args is ["--phantoon-position"])
{
    Suite(nameof(VerifyPhantoonPosition), () => VerifyPhantoonPosition());
    return 0;
}
if (args is ["--title-instruction-definitions"])
{
    Suite(nameof(VerifyTitleGradientTables), () => VerifyTitleGradientTables(SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"))));
    Suite(nameof(VerifyTitleSequenceRomData), () => VerifyTitleSequenceRomData());
    return 0;
}
if (args.Length > 1 || (args.Length == 1 && args[0] != "--render-contract"))
    throw new ArgumentException("Usage: SuperMetroid.Verification [--render-contract | --title-instruction-definitions | --phantoon-position | --samus-grapple | --samus-xray | --grapple-doors | --grapple-sounds | --grapple-spin | --grapple-gates | --grapple-enemy-death | --shutter-riding | --shutter-embedding]");
Console.WriteLine("Verifying translated Super Metroid routines...");
Suite(nameof(VerifyPlmDrawClone), () => VerifyPlmDrawClone());
Suite(nameof(VerifyGameConfigurationIni), () => VerifyGameConfigurationIni());
Suite(nameof(VerifyViewportTileRowParity), () => VerifyViewportTileRowParity());
Suite(nameof(VerifyPpuMemorySnapshotOwnership), () => VerifyPpuMemorySnapshotOwnership());
Suite(nameof(VerifyTitleRenderSnapshots), () => VerifyTitleRenderSnapshots());
Suite(nameof(VerifyPauseRenderSnapshots), () => VerifyPauseRenderSnapshots());
Suite(nameof(VerifyPauseBossMarkers), () => VerifyPauseBossMarkers());
Suite(nameof(VerifyFileMenuRenderSnapshots), () => VerifyFileMenuRenderSnapshots());
Suite(nameof(VerifyRenderFrameHandoff), () => VerifyRenderFrameHandoff());
Suite(nameof(VerifyRenderPresentationGate), () => VerifyRenderPresentationGate());
Suite(nameof(VerifyRenderPacketCodec), () => VerifyRenderPacketCodec());
Suite(nameof(VerifyFrontendRenderCapture), () => VerifyFrontendRenderCapture());
Suite(nameof(VerifyCinematicRenderSnapshots), () => VerifyCinematicRenderSnapshots());
Suite(nameof(VerifyGameplaySnapshots), () => VerifyGameplaySnapshots());
Suite(nameof(VerifyWindowPixels), () => VerifyWindowPixels());
Suite(nameof(VerifyColorWindowSnapshots), () => VerifyColorWindowSnapshots());
Suite(nameof(VerifyMessageSnapshots), () => VerifyMessageSnapshots());
Suite(nameof(VerifyEyeWindowSnapshots), () => VerifyEyeWindowSnapshots());
Suite(nameof(VerifyRoomFxSnapshots), () => VerifyRoomFxSnapshots());
Suite(nameof(VerifyGameplayCaptureIntegration), () => VerifyGameplayCaptureIntegration());
Suite(nameof(VerifyMode7GameplaySnapshots), () => VerifyMode7GameplaySnapshots());
Suite(nameof(VerifyEndingRenderSnapshots), () => VerifyEndingRenderSnapshots());
Suite(nameof(VerifyFileMapSnapshots), () => VerifyFileMapSnapshots());
Suite(nameof(VerifyAttractCapture), () => VerifyAttractCapture());

// This portable gate retains every snapshot, codec, publication, scene-capture and
// software parity check above. It deliberately excludes unrelated gameplay audits,
// and never loads the Windows desktop or a graphics backend.
if (args.Length == 1)
{
    Console.WriteLine($"Portable render contract passed on {RuntimeInformation.OSDescription}; {RuntimeInformation.FrameworkDescription}.");
    return 0;
}

Suite(nameof(VerifyRandomNumberGeneratorExhaustively), () => VerifyRandomNumberGeneratorExhaustively());
Suite(nameof(SaveLoadRandomAudit), () => SaveLoadRandomAudit.Run(Path.GetFullPath("Super Metroid.smc"), RepositoryInstallation.BindGame));
Suite(nameof(VerifySandAnimatedTiles), () => VerifySandAnimatedTiles());
Suite(nameof(VerifyQuicksand), () => VerifyQuicksand());
Suite(nameof(VerifyTreadmillPhysics), () => VerifyTreadmillPhysics());
Suite(nameof(VerifyPhantoonPosition), () => VerifyPhantoonPosition());
Suite(nameof(VerifyPausePaletteSound), () => VerifyPausePaletteSound());
Suite(nameof(VerifyKnownRandomSequence), () => VerifyKnownRandomSequence());
Suite(nameof(VerifyTimedHeldInputTimeline), () => VerifyTimedHeldInputTimeline());
Suite(nameof(VerifyEventBitfield), () => VerifyEventBitfield());
Suite(nameof(VerifyBossBitfield), () => VerifyBossBitfield());
Suite(nameof(VerifyMultiplicationExhaustively), () => VerifyMultiplicationExhaustively());
Suite(nameof(VerifySmCompressionFormat), () => VerifySmCompressionFormat());
Suite(nameof(VerifyVramWriteQueue), () => VerifyVramWriteQueue());
Suite(nameof(VerifyEscapeTimerBcd), () => VerifyEscapeTimerBcd());
Suite(nameof(VerifyEscapeTimerStateMachine), () => VerifyEscapeTimerStateMachine());
Suite(nameof(VerifyEscapeTimerFloor), () => VerifyEscapeTimerFloor());
Suite(nameof(VerifyControllerInputLatch), () => VerifyControllerInputLatch());
Suite(nameof(VerifyGameOptionsRomDataCatalog), () => VerifyGameOptionsRomDataCatalog());
Suite(nameof(VerifyControllerBindingsAndOptionsSubmenus), () => VerifyControllerBindingsAndOptionsSubmenus());
Suite(nameof(VerifyGameOverRomData), () => VerifyGameOverRomData());
Suite(nameof(VerifyTitleSequenceRomData), () => VerifyTitleSequenceRomData());
Suite(nameof(VerifyStrictFailureBoundaries), () => VerifyStrictFailureBoundaries());
Suite(nameof(VerifyReserveAutoRecovery), () => VerifyReserveAutoRecovery());
Suite(nameof(VerifyHealthWarning), () => VerifyHealthWarning());
Suite(nameof(VerifyReserveAutoFrontend), () => VerifyReserveAutoFrontend());
Suite(nameof(VerifyPauseReserveManual), () => VerifyPauseReserveManual());
Suite(nameof(VerifyPauseReserveArrow), () => VerifyPauseReserveArrow());
Suite(nameof(VerifyPauseReserveArrowRebindKeepsLatch), () => VerifyPauseReserveArrowRebindKeepsLatch());
Suite(nameof(VerifyPauseReserveTanks), () => VerifyPauseReserveTanks());
Suite(nameof(VerifyPauseReserveHud), () => VerifyPauseReserveHud());
Suite(nameof(VerifyDoorMusicTiming), () => VerifyDoorMusicTiming());
Suite(nameof(VerifyDoorAlignmentParity), () => VerifyDoorAlignmentParity());
Suite(nameof(VerifyDoorOpeningTrajectories), () => VerifyDoorOpeningTrajectories());
Suite(nameof(VerifyCreditsObjectInterpreter), () => VerifyCreditsObjectInterpreter());
Suite(nameof(VerifyEndingCreditsState), () => VerifyEndingCreditsState());
Suite(nameof(VerifyGenericGamepadInput), () => VerifyGenericGamepadInput());
Suite(nameof(VerifyDemoInputObject), () => VerifyDemoInputObject());
Suite(nameof(VerifyAttractDemoScene), () => VerifyAttractDemoScene());
Suite(nameof(VerifyAttractDemoControllerOverride), () => VerifyAttractDemoControllerOverride());
Suite(nameof(VerifyGrappleDemoTrajectory), () => VerifyGrappleDemoTrajectory());
Suite(nameof(VerifyFrameRuntime), () => VerifyFrameRuntime());
Suite(nameof(VerifyGameTimeState), () => VerifyGameTimeState());
Suite(nameof(VerifySuperMetroidAddressSpace), () => VerifySuperMetroidAddressSpace());
Suite(nameof(VerifyCartridgeAudioQueues), () => VerifyCartridgeAudioQueues());
Suite(nameof(VerifyManagedSnesDsp), () => VerifyManagedSnesDsp());
Suite(nameof(VerifyTypedNativeWords), () => VerifyTypedNativeWords());
Suite(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit());
Suite(nameof(VerifyPhantoonWaveLifecycle), () => VerifyPhantoonWaveLifecycle());
Suite(nameof(VerifyBackgroundMosaicSampling), () => VerifyBackgroundMosaicSampling());
Suite(nameof(VerifyOamSpritemapPacking), () => VerifyOamSpritemapPacking());
Suite(nameof(VerifyCeresElevatorArrivalGraphicsIndex), () => VerifyCeresElevatorArrivalGraphicsIndex());
Suite(nameof(VerifySamusRenderingSlice), () => VerifySamusRenderingSlice());
Suite(nameof(VerifySamusMovementRomData), () => VerifySamusMovementRomData());
Suite(nameof(VerifySamusRenderingRomData), () => VerifySamusRenderingRomData());
Suite(nameof(VerifySamusPaletteRomData), () => VerifySamusPaletteRomData());
Suite(nameof(VerifySamusSpecialSequenceRomData), () => VerifySamusSpecialSequenceRomData());
Suite(nameof(VerifySamusArmCannon), () => VerifySamusArmCannon());
Suite(nameof(VerifySamusProjectileRomData), () => VerifySamusProjectileRomData());
Suite(nameof(VerifySamusGrappleRomData), () => VerifySamusGrappleRomData());
Suite(nameof(VerifySamusXrayRomData), () => VerifySamusXrayRomData());
Suite(nameof(VerifySamusHudSelection), () => VerifySamusHudSelection());
Suite(nameof(VerifySamusVisorPalette), () => VerifySamusVisorPalette());
Suite(nameof(VerifySamusHurtFlashPalette), () => VerifySamusHurtFlashPalette());
Suite(nameof(VerifySamusPoseTransitionMatching), () => VerifySamusPoseTransitionMatching());
Suite(nameof(VerifySamusHorizontalSpeed), () => VerifySamusHorizontalSpeed());
Suite(nameof(VerifySamusExtraDisplacement), () => VerifySamusExtraDisplacement());
Suite(nameof(VerifySamusStoredShineAndShinespark), () => VerifySamusStoredShineAndShinespark());
Suite(nameof(VerifySamusCrystalFlash), () => VerifySamusCrystalFlash());
Suite(nameof(VerifyCrystalFlashRuntime), () => VerifyCrystalFlashRuntime());
Suite(nameof(VerifySamusXray), () => VerifySamusXray());
Suite(nameof(VerifySamusDeathSequence), () => VerifySamusDeathSequence());
Suite(nameof(VerifyMotherBrainBeamWindow), () => VerifyMotherBrainBeamWindow());
Suite(nameof(VerifyMotherBrainDeathHandoff), () => VerifyMotherBrainDeathHandoff());
Suite(nameof(VerifySamusDrainedController), () => VerifySamusDrainedController());
Suite(nameof(VerifySamusGrabbedByDraygon), () => VerifySamusGrabbedByDraygon());
Suite(nameof(VerifyMotherBrainRainbowBeamSamusMovement), () => VerifyMotherBrainRainbowBeamSamusMovement());
Suite(nameof(VerifyMotherBrainHandBeamBodyInstructionDefinitions), () => VerifyMotherBrainHandBeamBodyInstructionDefinitions());
Suite(nameof(VerifyMotherBrainFallingTubeInstructionDefinitions), () => VerifyMotherBrainFallingTubeInstructionDefinitions());
Suite(nameof(VerifyMotherBrainHeadInstructionProgramDefinitions), () => VerifyMotherBrainHeadInstructionProgramDefinitions());
Suite(nameof(VerifyMotherBrainRainbowBeamAttackSequence), () => VerifyMotherBrainRainbowBeamAttackSequence());
Suite(nameof(VerifyEnemyProjectileInstructionMechanicsDefinitions), () => VerifyEnemyProjectileInstructionMechanicsDefinitions());
Suite(nameof(VerifyBabyMetroidCutsceneEntrance), () => VerifyBabyMetroidCutsceneEntrance());
Suite(nameof(VerifySamusSolidEnemyCollision), () => VerifySamusSolidEnemyCollision());
Suite(nameof(VerifySamusAerialMovement), () => VerifySamusAerialMovement());
Suite(nameof(VerifyZeroDistanceJumpContact), () => VerifyZeroDistanceJumpContact());
Suite(nameof(VerifyShinesparkEnemyStop), () => VerifyShinesparkEnemyStop());
Suite(nameof(VerifyKnockbackHorizontalStop), () => VerifyKnockbackHorizontalStop());
Suite(nameof(VerifyRetailFallingSpeedRecurrence), () => VerifyRetailFallingSpeedRecurrence());
Suite(nameof(VerifyCrampedAerialLandingPoseCollision), () => VerifyCrampedAerialLandingPoseCollision());
Suite(nameof(VerifySamusSpaceJumpAndScrewAttack), () => VerifySamusSpaceJumpAndScrewAttack());
Suite(nameof(VerifySamusLiquidPhysics), () => VerifySamusLiquidPhysics());
Suite(nameof(VerifyShallowWaterJump), () => VerifyShallowWaterJump());
Suite(nameof(VerifySamusAtmosphericEffects), () => VerifySamusAtmosphericEffects());
Suite(nameof(VerifySamusAerialTurnsAndWallJump), () => VerifySamusAerialTurnsAndWallJump());
Suite(nameof(VerifySamusPoseHistory), () => VerifySamusPoseHistory());
Suite(nameof(VerifyWallJumpDust), () => VerifyWallJumpDust());
Suite(nameof(VerifyCeresHazeLifecycle), () => VerifyCeresHazeLifecycle());
Suite(nameof(VerifyCeresRidleyWallImpact), () => VerifyCeresRidleyWallImpact());
Suite(nameof(VerifySamusKnockbackAndDamageBoost), () => VerifySamusKnockbackAndDamageBoost());
Suite(nameof(VerifyYappingMawGrappleRelease), () => VerifyYappingMawGrappleRelease());
Suite(nameof(VerifyKiHunterSpitAudio), () => VerifyKiHunterSpitAudio());
Suite(nameof(VerifySamusGrappleSwingAndRelease), () => VerifySamusGrappleSwingAndRelease());
Suite(nameof(VerifyGrappleBlueDoors), () => VerifyGrappleBlueDoors());
Suite(nameof(VerifyGrappleSounds), () => VerifyGrappleSounds());
Suite(nameof(VerifyGrapplePoseRefire), () => VerifyGrapplePoseRefire());
Suite(nameof(VerifyGrappleSpinInput), () => VerifyGrappleSpinInput());
Suite(nameof(VerifyGrappleGreenGateVisibility), () => VerifyGrappleGreenGateVisibility());
Suite(nameof(VerifyGrappleEnemyDeath), () => VerifyGrappleEnemyDeath());
Suite(nameof(VerifyShutterRiding), () => VerifyShutterRiding());
Suite(nameof(VerifyXrayInput), () => VerifyXrayInput());
Suite(nameof(VerifyDraygonPrebattleXray), () => VerifyDraygonPrebattleXray());
Suite(nameof(VerifyXrayWindowGeometry), () => VerifyXrayWindowGeometry());
Suite(nameof(VerifyXraySetupBuffers), () => VerifyXraySetupBuffers());
Suite(nameof(VerifyFirefleaFx), () => VerifyFirefleaFx());
Suite(nameof(VerifyBombJumpWallContact), () => VerifyBombJumpWallContact());
Suite(nameof(VerifyXrayRoomDisplayRules), () => VerifyXrayRoomDisplayRules());
Suite(nameof(VerifyXrayRevealTable), () => VerifyXrayRevealTable());
Suite(nameof(VerifyXrayExtensions), () => VerifyXrayExtensions());
Suite(nameof(VerifyXrayTilemap), () => VerifyXrayTilemap());
Suite(nameof(VerifyXrayOverlays), () => VerifyXrayOverlays());
Suite(nameof(VerifyBreakableGrapplePlms), () => VerifyBreakableGrapplePlms());
Suite(nameof(VerifyBombBlockPrograms), () => VerifyBombBlockPrograms());
Suite(nameof(VerifyContactCrumblePrograms), () => VerifyContactCrumblePrograms());
Suite(nameof(VerifyStationAnimationProgramDefinitions), () => VerifyStationAnimationProgramDefinitions());
Suite(nameof(VerifyCollectibleMessageTiming), () => VerifyCollectibleMessageTiming());
Suite(nameof(VerifyPermanentCollectibles), () => VerifyPermanentCollectibles());
Suite(nameof(VerifyCollectibleVisuals), () => VerifyCollectibleVisuals());
Suite(nameof(VerifyEnemyDrops), () => VerifyEnemyDrops());
Suite(nameof(VerifySamusPostureMovement), () => VerifySamusPostureMovement());
Suite(nameof(VerifySamusPowerBeamProjectiles), () => VerifySamusPowerBeamProjectiles());
Suite(nameof(ProbeProjectileVelocityInheritance), () => ProbeProjectileVelocityInheritance());
Suite(nameof(VerifyProjectileCooldowns), () => VerifyProjectileCooldowns());
Suite(nameof(VerifyWrapShotTrace), () => VerifyWrapShotTrace("csharp/test-fixtures/movement-release/wrap-shot-409.csv"));
Suite(nameof(VerifyRetailWrapShotDoors), () => VerifyRetailWrapShotDoors());
Suite(nameof(VerifyWrapShotEnemySeparation), () => VerifyWrapShotEnemySeparation());
Suite(nameof(VerifyWrapShotWidths), () => VerifyWrapShotWidths("csharp/test-fixtures/movement-release/wrap-width-409.csv"));
Suite(nameof(VerifyCeilingWrapPlmTrace), () => VerifyCeilingWrapPlmTrace("csharp/test-fixtures/movement-release/ceiling-plm-410.csv"));
Suite(nameof(VerifyKronicGateBeamCollision), () => VerifyKronicGateBeamCollision());
Suite(nameof(VerifyGateJumpTraces), () => VerifyGateJumpTraces());
Suite(nameof(VerifyRightFacingGateGlitches), () => VerifyRightFacingGateGlitches());
Suite(nameof(VerifyGreenHillGrappleSpeedGateGlitch), () => VerifyGreenHillGrappleSpeedGateGlitch());
Suite(nameof(VerifyGModeGateGlitch), () => VerifyGModeGateGlitch());
Suite(nameof(VerifyFrozenEnemyGateGlitch), () => VerifyFrozenEnemyGateGlitch());
Suite(nameof(VerifyMochtroidBotwoonPipeClip), () => VerifyMochtroidBotwoonPipeClip());
Suite(nameof(VerifySparkCrashAlignment), () => VerifySparkCrashAlignment());
Suite(nameof(VerifyEnemyAngleDivision), () => VerifyEnemyAngleDivision());
Suite(nameof(VerifyDraygonEyeEffects), () => VerifyDraygonEyeEffects());
Suite(nameof(VerifyDraygonTilemapProduction), () => VerifyDraygonTilemapProduction());
Suite(nameof(VerifyFrogSpeedwayPoolCollision), () => VerifyFrogSpeedwayPoolCollision());
Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv"));
Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv", 9));
Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-run-410.csv", 11));
Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-walk-410.csv", 11, false));
Suite(nameof(VerifyHeroShotCameraLifetime), () => VerifyHeroShotCameraLifetime("csharp/test-fixtures/movement-release/hero-shot-411.csv"));
Suite(nameof(VerifyHeroShotRuntimeCamera), () => VerifyHeroShotRuntimeCamera("csharp/test-fixtures/movement-release/hero-runtime-603.csv"));
Suite(nameof(VerifyMissileImpactCameraEdge), () => VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv"));
Suite(nameof(VerifyControlledRedTowerHeroShot), () => VerifyControlledRedTowerHeroShot());
Suite(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows());
Suite(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables());
Suite(nameof(VerifySamusMorphBallMovement), () => VerifySamusMorphBallMovement());
Suite(nameof(VerifyShotBlockPlmPrograms), () => VerifyShotBlockPlmPrograms());
Suite(nameof(VerifyCompactWalkOffCollision), () => VerifyCompactWalkOffCollision());
Suite(nameof(VerifyPauseMomentumReconciliation), () => VerifyPauseMomentumReconciliation());
Suite(nameof(VerifyBombChargeRejection), () => VerifyBombChargeRejection());
Suite(nameof(VerifyGroundedBombSpread), () => VerifyGroundedBombSpread());
Suite(nameof(VerifyGroundedSpreadTransition), () => VerifyGroundedSpreadTransition());
Suite(nameof(VerifyAerialSpreadTransitions), () => VerifyAerialSpreadTransitions());
Suite(nameof(VerifyAerialSpreadTransitions), () => VerifyAerialSpreadTransitions(wallRoute: true));
Suite(nameof(VerifySamusStandingAimMovement), () => VerifySamusStandingAimMovement());
Suite(nameof(VerifySamusAimedAerialMovement), () => VerifySamusAimedAerialMovement());
Suite(nameof(VerifySamusGunExtendedMovement), () => VerifySamusGunExtendedMovement());
Suite(nameof(VerifySamusSlopePhysics), () => VerifySamusSlopePhysics());
Suite(nameof(VerifySamusBlockCollision), () => VerifySamusBlockCollision());
Suite(nameof(VerifySpeedBoosterCollisionBlocks), () => VerifySpeedBoosterCollisionBlocks());
Suite(nameof(VerifyMaridiaElevatubePlm), () => VerifyMaridiaElevatubePlm());
Suite(nameof(VerifyCompiledTourianAccessPlmPrograms), () => VerifyCompiledTourianAccessPlmPrograms());
Suite(nameof(VerifyTourianAccessVisuals), () => VerifyTourianAccessVisuals());
Suite(nameof(VerifySpeedBoosterVisuals), () => VerifySpeedBoosterVisuals());
Suite(nameof(VerifyMaridiaElevatubeVisuals), () => VerifyMaridiaElevatubeVisuals());
Suite(nameof(VerifyCompiledSporeSpawnCeilingPlms), () => VerifyCompiledSporeSpawnCeilingPlms());
Suite(nameof(VerifyCompiledBotwoonWallPlms), () => VerifyCompiledBotwoonWallPlms());
Suite(nameof(VerifyBotwoonWallVisuals), () => VerifyBotwoonWallVisuals());
Suite(nameof(VerifyCompiledKraidRoomPlms), () => VerifyCompiledKraidRoomPlms());
Suite(nameof(VerifyCompiledCrocomireArenaPlms), () => VerifyCompiledCrocomireArenaPlms());
Suite(nameof(VerifyCompiledMotherBrainFakeDeathPlms), () => VerifyCompiledMotherBrainFakeDeathPlms());
Suite(nameof(VerifyMotherBrainFakeDeathVisuals), () => VerifyMotherBrainFakeDeathVisuals());
Suite(nameof(VerifyCrocomireArenaVisuals), () => VerifyCrocomireArenaVisuals());
Suite(nameof(VerifyKraidRoomVisuals), () => VerifyKraidRoomVisuals());
Suite(nameof(VerifySporeSpawnCeilingVisuals), () => VerifySporeSpawnCeilingVisuals());
Suite(nameof(VerifySamusEaterVisuals), () => VerifySamusEaterVisuals());
Suite(nameof(VerifySamusGroundedMovement), () => VerifySamusGroundedMovement());
Suite(nameof(VerifySamusGroundedReversal), () => VerifySamusGroundedReversal());
Suite(nameof(VerifySamusMoonwalking), () => VerifySamusMoonwalking());
Suite(nameof(VerifySamusRanIntoWall), () => VerifySamusRanIntoWall());
Suite(nameof(VerifyObjRendering), () => VerifyObjRendering());
Suite(nameof(VerifyHudStateAndBg3Rendering), () => VerifyHudStateAndBg3Rendering());
Suite(nameof(VerifyRoomScrollGridAndBoundaryCamera), () => VerifyRoomScrollGridAndBoundaryCamera());
Suite(nameof(VerifyRoomScrollPlms), () => VerifyRoomScrollPlms());
Suite(nameof(VerifyRoomPlmHeaderCatalog), () => VerifyRoomPlmHeaderCatalog());
Suite(nameof(VerifyRoomPlmInstructionListCatalog), () => VerifyRoomPlmInstructionListCatalog());
Suite(nameof(VerifySequentialRoomPlmPopulationLoader), () => VerifySequentialRoomPlmPopulationLoader());
Suite(nameof(VerifyMotherBrainEscapeGateCompiledDefinitions), () => VerifyMotherBrainEscapeGateCompiledDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
Suite(nameof(VerifyEscapeGateVisuals), () => VerifyEscapeGateVisuals(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
Suite(nameof(VerifyCompiledRoomPlmPopulationDefinitions), () => VerifyCompiledRoomPlmPopulationDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
Suite(nameof(VerifyNoobTubePlm), () => VerifyNoobTubePlm());
Suite(nameof(VerifyDownwardGatePlms), () => VerifyDownwardGatePlms());
Suite(nameof(VerifyEyeDoorPlms), () => VerifyEyeDoorPlms());
Suite(nameof(VerifyDraygonCannonPlms), () => VerifyDraygonCannonPlms());
Suite(nameof(VerifyBombTorizoHandPlm), () => VerifyBombTorizoHandPlm());
Suite(nameof(VerifyPauseMenuEquipmentInteraction), () => VerifyPauseMenuEquipmentInteraction());
Suite(nameof(VerifyPauseMapArrows), () => VerifyPauseMapArrows());
Suite(nameof(VerifyPauseMapAreaLabels), () => VerifyPauseMapAreaLabels());
Suite(nameof(VerifyPauseMapPosition), () => VerifyPauseMapPosition());
Suite(nameof(VerifySpeedPaletteOverrun), () => VerifySpeedPaletteOverrun());
Suite(nameof(VerifyInvalidBeamSelection), () => VerifyInvalidBeamSelection());
Suite(nameof(VerifyInvalidBeamGraphics), () => VerifyInvalidBeamGraphics());
Suite(nameof(VerifyMovedSamusCameraTracking), () => VerifyMovedSamusCameraTracking());
Suite(nameof(VerifyBackgroundScrollState), () => VerifyBackgroundScrollState());
Suite(nameof(VerifyLevelBlockTilemapExpansion), () => VerifyLevelBlockTilemapExpansion());
Suite(nameof(VerifyRoomLevelData), () => VerifyRoomLevelData());
Suite(nameof(VerifyCartridgeRoomStateSelection), () => VerifyCartridgeRoomStateSelection());
Suite(nameof(VerifyCompiledRoomHeaderDefinitions), () => VerifyCompiledRoomHeaderDefinitions());
Suite(nameof(VerifyCompiledRoomStateDefinitions), () => VerifyCompiledRoomStateDefinitions());
Suite(nameof(VerifyCompiledRoomStateSelectionDefinitions), () => VerifyCompiledRoomStateSelectionDefinitions());
Suite(nameof(VerifyCompiledLoadStationDefinitions), () => VerifyCompiledLoadStationDefinitions());
Suite(nameof(VerifyCompiledDoorDefinitions), () => VerifyCompiledDoorDefinitions());
Suite(nameof(VerifyCompiledEnemyDefinitions), () => VerifyCompiledEnemyDefinitions());
Suite(nameof(VerifyCompiledEnemyRoomLists), () => VerifyCompiledEnemyRoomLists());
Suite(nameof(VerifyCompiledRoomScrollDefinitions), () => VerifyCompiledRoomScrollDefinitions());
Suite(nameof(VerifyCompiledRoomCallbackDefinitions), () => VerifyCompiledRoomCallbackDefinitions());
Suite(nameof(VerifyCompiledRoomDefinitionIntegration), () => VerifyCompiledRoomDefinitionIntegration());
Suite(nameof(VerifyRoomMainCodeCatalog), () => VerifyRoomMainCodeCatalog());
Suite(nameof(VerifyRoomSetupCodeCatalog), () => VerifyRoomSetupCodeCatalog());
Suite(nameof(VerifyRoomAssetRomData), () => VerifyRoomAssetRomData());
Suite(nameof(VerifyAreaMapAssets), () => VerifyAreaMapAssets());
Suite(nameof(VerifyMapPresentation), () => VerifyMapPresentation());
Suite(nameof(VerifyBackgroundTilemapStreamer), () => VerifyBackgroundTilemapStreamer());
Suite(nameof(VerifyFourBitBackgroundRendering), () => VerifyFourBitBackgroundRendering());
Suite(nameof(VerifyLoRomCrossBankCompressedData), () => VerifyLoRomCrossBankCompressedData());
Suite(nameof(VerifyMode7Rendering), () => VerifyMode7Rendering());
Suite(nameof(VerifyLayerCompositorBackdrop), () => VerifyLayerCompositorBackdrop());
Suite(nameof(VerifyBgPriorityPlaneRendering), () => VerifyBgPriorityPlaneRendering());
Suite(nameof(VerifyLibraryBackgroundLoader), () => VerifyLibraryBackgroundLoader());
Suite(nameof(VerifyControllerInputRecording), () => VerifyControllerInputRecording());
Suite(nameof(VerifySaveRamLayout), () => VerifySaveRamLayout());
Suite(nameof(VerifyExploredMapPackingDefinitions), () => VerifyExploredMapPackingDefinitions());
Suite(nameof(VerifyGameSaveJsonPersistence), () => VerifyGameSaveJsonPersistence());
Suite(nameof(VerifyFileSelectFreshSaveTilemap), () => VerifyFileSelectFreshSaveTilemap());
Suite(nameof(VerifyFileSelectMapWindow), () => VerifyFileSelectMapWindow());
Suite(nameof(VerifySavedGameLoadAppearance), () => VerifySavedGameLoadAppearance());
Suite(nameof(VerifyBossResetOnLoad), () => VerifyBossResetOnLoad());
Suite(nameof(VerifyIntroCinematicRomData), () => VerifyIntroCinematicRomData());
Suite(nameof(VerifyIntroCinematicArtwork), () => VerifyIntroCinematicArtwork(Path.GetFullPath("Super Metroid.smc")));
Suite(nameof(VerifyIntroGameplayFlashbackVerticalScroll), () => VerifyIntroGameplayFlashbackVerticalScroll());
Suite(nameof(VerifyIntroMotherBrainDefinitions), () => VerifyIntroMotherBrainDefinitions());
Suite(nameof(VerifyIntroRinkaDefinitions), () => VerifyIntroRinkaDefinitions());
Suite(nameof(VerifyIntroBabyActorDefinitions), () => VerifyIntroBabyActorDefinitions());
Suite(nameof(VerifyIntroEggEffectDefinitions), () => VerifyIntroEggEffectDefinitions());
Suite(nameof(VerifyCeresExplosionDefinitions), () => VerifyCeresExplosionDefinitions());
Suite(nameof(VerifyCeresFlightActorDefinitions), () => VerifyCeresFlightActorDefinitions());
Suite(nameof(VerifyCeresDestructionActorDefinitions), () => VerifyCeresDestructionActorDefinitions());
Suite(nameof(VerifyCinematicPaletteFader), () => VerifyCinematicPaletteFader());
Suite(nameof(VerifyHostRoomViewportAlignment), () => VerifyHostRoomViewportAlignment());
Suite(nameof(VerifyPowerBombColorMathWindow), () => VerifyPowerBombColorMathWindow());
Suite(nameof(VerifyHardwareWindows), () => VerifyHardwareWindows());
Suite(nameof(VerifyChainsawFiring), () => VerifyChainsawFiring());
Suite(nameof(VerifySpacetimeBeam), () => VerifySpacetimeBeam());
Suite(nameof(VerifyMurderBeam), () => VerifyMurderBeam());
Suite(nameof(VerifyPowerBombRuntimeRendererIntegration), () => VerifyPowerBombRuntimeRendererIntegration());
Suite(nameof(VerifyPowerBombFuse), () => VerifyPowerBombFuse());
Suite(nameof(VerifyPowerBombBoundary), () => VerifyPowerBombBoundary());
Suite(nameof(VerifyPowerBombDeathRadius), () => VerifyPowerBombDeathRadius());
Suite(nameof(VerifyRespawnedEnemyContact), () => VerifyRespawnedEnemyContact());
Suite(nameof(VerifyVerticalOffScreenDeletion), () => VerifyVerticalOffScreenDeletion());
Suite(nameof(VerifyBotwoonPositionHistory), () => VerifyBotwoonPositionHistory());
Suite(nameof(VerifyDraygonTurretCadence), () => VerifyDraygonTurretCadence());
Suite(nameof(VerifyWallJumpSpinExit), () => VerifyWallJumpSpinExit());
Suite(nameof(VerifyEvirInitTimer), () => VerifyEvirInitTimer());
Suite(nameof(VerifyRefillStationLock), () => VerifyRefillStationLock());
Suite(nameof(VerifyMagdolliteApexThreshold), () => VerifyMagdolliteApexThreshold());
Suite(nameof(VerifyGoldenTorizoAttackChoice), () => VerifyGoldenTorizoAttackChoice());
Suite(nameof(VerifyWallProbeBombBlock), () => VerifyWallProbeBombBlock());
Suite(nameof(VerifyPuromiArcPosition), () => VerifyPuromiArcPosition());
Suite(nameof(VerifySpringBallBounce), () => VerifySpringBallBounce());
Suite(nameof(VerifyMainGameLoopCarry), () => VerifyMainGameLoopCarry());
Suite(nameof(VerifyDoorEntryRoomFxSound), () => VerifyDoorEntryRoomFxSound());
Suite(nameof(VerifyGoldNinjaDeathDrops), () => VerifyGoldNinjaDeathDrops());
Suite(nameof(VerifyDoorEntryEnemySound), () => VerifyDoorEntryEnemySound());
Suite(nameof(VerifySpringBallFallingFallback), () => VerifySpringBallFallingFallback());
Suite(nameof(VerifyFirefleaDoubleDeath), () => VerifyFirefleaDoubleDeath());
Suite(nameof(VerifyShutterScrewContact), () => VerifyShutterScrewContact());
Suite(nameof(VerifyJumpNoXMovement), () => VerifyJumpNoXMovement());
Suite(nameof(VerifySaveConfirmationCadence), () => VerifySaveConfirmationCadence());
Suite(nameof(VerifyTourianStatueXrayFreeze), () => VerifyTourianStatueXrayFreeze());
Suite(nameof(VerifyDoorAnimatedTiles), () => VerifyDoorAnimatedTiles());
Suite(nameof(VerifyTourianStatueDescentRounding), () => VerifyTourianStatueDescentRounding());
Suite(nameof(VerifyMetroidDeathDrops), () => VerifyMetroidDeathDrops());
Suite(nameof(VerifyShitroidGradualAcceleration), () => VerifyShitroidGradualAcceleration());
Suite(nameof(VerifyShitroidDrainCarry), () => VerifyShitroidDrainCarry());
Suite(nameof(VerifyMotherBrainRinkaDoorSpawn), () => VerifyMotherBrainRinkaDoorSpawn());
Suite(nameof(VerifyMotherBrainGlassSuperMissile), () => VerifyMotherBrainGlassSuperMissile());
Suite(nameof(VerifyMotherBrainTubeTiming), () => VerifyMotherBrainTubeTiming());
Suite(nameof(VerifyMotherBrainTubeHdmaDeletion), () => VerifyMotherBrainTubeHdmaDeletion());
Suite(nameof(VerifyMotherBrainRaiseCounter), () => VerifyMotherBrainRaiseCounter());
Suite(nameof(VerifyMotherBrainHeadHitbox), () => VerifyMotherBrainHeadHitbox());
Suite(nameof(VerifyMotherBrainSmallPurpleBreath), () => VerifyMotherBrainSmallPurpleBreath());
Suite(nameof(VerifyRainbowReleaseKnockback), () => VerifyRainbowReleaseKnockback());
Suite(nameof(VerifyBabyMetroidInheritedFractions), () => VerifyBabyMetroidInheritedFractions());
Suite(nameof(VerifyBabyMetroidHeadTarget), () => VerifyBabyMetroidHeadTarget());
Suite(nameof(VerifyMotherBrainWalkBackwardsPose), () => VerifyMotherBrainWalkBackwardsPose());
Suite(nameof(VerifyMotherBrainRingBabyHealth), () => VerifyMotherBrainRingBabyHealth());
Suite(nameof(VerifyBabyMetroidWrongWaySpeed), () => VerifyBabyMetroidWrongWaySpeed());
Suite(nameof(VerifyBabyMetroidFatalBlowShake), () => VerifyBabyMetroidFatalBlowShake());
Suite(nameof(VerifyMotherBrainMissileWalkReset), () => VerifyMotherBrainMissileWalkReset());
Suite(nameof(VerifyMotherBrainBodyHitboxes), () => VerifyMotherBrainBodyHitboxes());
Suite(nameof(VerifyMotherBrainInheritedExplosionIndex), () => VerifyMotherBrainInheritedExplosionIndex());
Suite(nameof(VerifyOldTourianEscapeShaftWall), () => VerifyOldTourianEscapeShaftWall());
Suite(nameof(VerifyCrateriaMainstreetEscapePassage), () => VerifyCrateriaMainstreetEscapePassage());
Suite(nameof(VerifyZebesEscapeFade), () => VerifyZebesEscapeFade());
Suite(nameof(VerifyEndingSetupNmiWaits), () => VerifyEndingSetupNmiWaits());
Suite(nameof(VerifyLowPercentIntroTimeline), () => VerifyLowPercentIntroTimeline());
Suite(nameof(VerifyBeamImpactSound), () => VerifyBeamImpactSound());
Suite(nameof(VerifySquareSlopeBeamCollision), () => VerifySquareSlopeBeamCollision());
Suite(nameof(VerifyAirSpikeAlphaRadius), () => VerifyAirSpikeAlphaRadius());
Suite(nameof(VerifyKraidArmSamusContact), () => VerifyKraidArmSamusContact());
Suite(nameof(VerifyEmptyExtendedFrameShots), () => VerifyEmptyExtendedFrameShots());
Suite(nameof(VerifyElevatorStandUpContact), () => VerifyElevatorStandUpContact());
Suite(nameof(VerifyImplicitScrollResidue), () => VerifyImplicitScrollResidue());
Suite(nameof(VerifySuperMissileDeathAnimation), () => VerifySuperMissileDeathAnimation());
Suite(nameof(VerifyDoorSoundsDuringPowerBomb), () => VerifyDoorSoundsDuringPowerBomb());
Suite(nameof(VerifyKnockbackShinesparkLaunch), () => VerifyKnockbackShinesparkLaunch());
Suite(nameof(VerifyUnpauseElevatorFlags), () => VerifyUnpauseElevatorFlags());
Suite(nameof(VerifyRidleyShotHealthStage), () => VerifyRidleyShotHealthStage());
Suite(nameof(VerifyPowampDeathSequence), () => VerifyPowampDeathSequence());
Suite(nameof(VerifyUnpauseReserveBlackout), () => VerifyUnpauseReserveBlackout());
Suite(nameof(VerifyFrozenTimeEnemyProjectiles), () => VerifyFrozenTimeEnemyProjectiles());
Suite(nameof(VerifyItemCancelClearsCharge), () => VerifyItemCancelClearsCharge());
Suite(nameof(VerifyDraygonScrollingSpeedCap), () => VerifyDraygonScrollingSpeedCap());
Suite(nameof(VerifyDraygonEscapeDrag), () => VerifyDraygonEscapeDrag());
Suite(nameof(VerifyReleasedSamusFallsOffDraygon), () => VerifyReleasedSamusFallsOffDraygon());
Suite(nameof(VerifySuperMissileEnemyHitQuake), () => VerifySuperMissileEnemyHitQuake());
Suite(nameof(VerifyRoomFxRomData), () => VerifyRoomFxRomData());
Suite(nameof(VerifyPowerBombFixedColors), () => VerifyPowerBombFixedColors());
Suite(nameof(VerifySamusVisorColors), () => VerifySamusVisorColors());
Suite(nameof(VerifySamusHurtColors), () => VerifySamusHurtColors());
Suite(nameof(VerifySamusHyperBeamColors), () => VerifySamusHyperBeamColors());
Suite(nameof(VerifySpcSoundLibrary2Pointers), () => VerifySpcSoundLibrary2Pointers());
Suite(nameof(VerifyScrollingSkyState), () => VerifyScrollingSkyState());
Suite(nameof(VerifyOceanSky), () => VerifyOceanSky());
Suite(nameof(AuditBoostFloor), () => AuditBoostFloor());
Suite(nameof(VerifyEnemyAiCodePointerCatalog), () => VerifyEnemyAiCodePointerCatalog());
Suite(nameof(VerifyEnemyInstructionCodePointerCatalogs), () => VerifyEnemyInstructionCodePointerCatalogs());
Suite(nameof(VerifyEnemyRomTablePointerCatalog), () => VerifyEnemyRomTablePointerCatalog());
Suite(nameof(VerifyMotherBrainContactHitboxes), () => VerifyMotherBrainContactHitboxes());
Suite(nameof(VerifyMamaTurtleShellContourDefinitions), () => VerifyMamaTurtleShellContourDefinitions());
Suite(nameof(VerifyMamaTurtleEnemyDefinitions), () => VerifyMamaTurtleEnemyDefinitions());
Suite(nameof(VerifyPaletteFxInstructionCodeCatalogs), () => VerifyPaletteFxInstructionCodeCatalogs());
Suite(nameof(VerifyAnimatedTileInstructionCodeCatalog), () => VerifyAnimatedTileInstructionCodeCatalog());
Suite(nameof(VerifyEnemyProjectileCodePointerCatalog), () => VerifyEnemyProjectileCodePointerCatalog());
Suite(nameof(VerifyEnemyMappedSourceRouting), () => VerifyEnemyMappedSourceRouting());
Suite(nameof(VerifyRoomEnemyLoading), () => VerifyRoomEnemyLoading());
Suite(nameof(VerifyEnemyTileArtwork), () => VerifyEnemyTileArtwork());
Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
Suite(nameof(VerifySpacePirateCollisionDefinitions), () => VerifySpacePirateCollisionDefinitions());
Suite(nameof(VerifyRidleyCollisionDefinitions), () => VerifyRidleyCollisionDefinitions());
Suite(nameof(VerifyCeresSteamCollisionDefinitions), () => VerifyCeresSteamCollisionDefinitions());
Suite(nameof(VerifyMaridiaLargeSnailCollisionDefinitions), () => VerifyMaridiaLargeSnailCollisionDefinitions());
Suite(nameof(VerifyCrocomireTongueCollisionDefinitions), () => VerifyCrocomireTongueCollisionDefinitions());
Suite(nameof(VerifyCrocomireBodyCollisionDefinitions), () => VerifyCrocomireBodyCollisionDefinitions());
Suite(nameof(VerifyBotwoonPlmIdentity), () => VerifyBotwoonPlmIdentity());
Suite(nameof(VerifyCompiledEnemyTrigonometry), () => VerifyCompiledEnemyTrigonometry());
Suite(nameof(VerifyRidleyExplosionDefinitions), () => VerifyRidleyExplosionDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
Suite(nameof(VerifyCrocomireMeltingDefinitions), () => VerifyCrocomireMeltingDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
Suite(nameof(VerifyDraygonIntroDanceDefinitions), () => VerifyDraygonIntroDanceDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
Suite(nameof(VerifyEnemyProjectileCollisionLifecycle), () => VerifyEnemyProjectileCollisionLifecycle());
Suite(nameof(VerifyRipperEnemy), () => VerifyRipperEnemy());
Suite(nameof(VerifyRipperEnemy), () => VerifyRipperEnemy(verifyXrayTimers: true));
Suite(nameof(VerifyPostCeresGunshipLanding), () => VerifyPostCeresGunshipLanding());
Suite(nameof(VerifyCeresElevatorPlatformAnimation), () => VerifyCeresElevatorPlatformAnimation());
Suite(nameof(VerifyCeresDoorBossBranch), () => VerifyCeresDoorBossBranch());
Suite(nameof(VerifyCeresRidleyRoomEntry), () => VerifyCeresRidleyRoomEntry());
Suite(nameof(VerifyCeresEscapeVramTransferDefinitions), () => VerifyCeresEscapeVramTransferDefinitions(
    SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
Suite(nameof(VerifyCeresEscapeHandoff), () => VerifyCeresEscapeHandoff());
Suite(nameof(VerifyCeresDestructionCinematic), () => VerifyCeresDestructionCinematic());

if (failedSuites.Count == 0)
    Console.WriteLine("All bank $80 verification checks passed.");
return 0;
}
catch (Exception exception)
{
    // A failure outside any suite ends the flag here. Windows never receives an unhandled CLR exception that it can
    // turn into a focus-stealing dialog. ToString() retains the type, message, inner exception,
    // and complete stack trace in the terminal where the failure is actually actionable.
    Console.Error.WriteLine(exception);
    return 1;
}
finally
{
    flagWatch.Stop();
    Console.WriteLine($"TIME flag {(args.Length == 0 ? "(all)" : string.Join(' ', args))}: {flagWatch.Elapsed.TotalSeconds:0.00} s, {PeakMemoryReport()}");
}

}

}
