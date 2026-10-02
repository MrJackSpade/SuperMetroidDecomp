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
if (args is ["--normal-suit-catalog-boundary"])
{
    VerifyNormalSuitCatalogBoundary();
    return 0;
}
if (args is ["--hyper-beam-fx-colors"])
{
    VerifyHyperBeamFxColorArtwork(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
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
if (args is ["--area-animated-tile-definitions"])
{
    VerifyAreaAnimatedTileObjectDefinitions();
    return 0;
}
if (args is ["--collectible-visuals"])
{
    VerifyCollectibleVisuals();
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
if (args is ["--samus-hyper-beam-colors"])
{
    VerifySamusHyperBeamColors();
    return 0;
}
if (args is ["--game-options-language-palettes"])
{
    VerifyGameOptionsLanguagePalettes();
    return 0;
}
if (args is ["--game-options-cursor-phases"])
{
    VerifyGameOptionsCursorPhases();
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
if (args is ["--room-fx-record-definitions", var roomFxDefinitionsRom])
{
    VerifyRoomFxRecordDefinitions(roomFxDefinitionsRom);
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
if (args is ["--mother-brain-glass-instruction-mechanics"])
{
    VerifyMotherBrainGlassInstructionProgramDefinitions();
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
if (args is ["--downward-gate-definitions"])
{
    VerifyDownwardGateShotBlockDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--speed-booster-escape-definitions"])
{
    VerifySpeedBoosterEscapeStageDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--speed-booster-block-plms"])
{
    VerifySpeedBoosterCollisionBlocks();
    return 0;
}
if (args is ["--maridia-elevatube-plm"])
{
    VerifyMaridiaElevatubePlm();
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
if (args is ["--speed-booster-visuals"])
{
    VerifySpeedBoosterVisuals();
    return 0;
}
if (args is ["--maridia-elevatube-visuals"])
{
    VerifyMaridiaElevatubeVisuals();
    return 0;
}
if (args is ["--spore-spawn-ceiling-plms"])
{
    VerifyCompiledSporeSpawnCeilingPlms();
    return 0;
}
if (args is ["--spore-spawn-ceiling-visuals"])
{
    VerifySporeSpawnCeilingVisuals();
    return 0;
}
if (args is ["--samus-eater-visuals"])
{
    VerifySamusEaterVisuals();
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
if (args is ["--crocomire-arena-plms"])
{
    VerifyCompiledCrocomireArenaPlms();
    return 0;
}
if (args is ["--mother-brain-fake-death-plms"])
{
    VerifyCompiledMotherBrainFakeDeathPlms();
    return 0;
}
if (args is ["--mother-brain-fake-death-visuals"])
{
    VerifyMotherBrainFakeDeathVisuals();
    return 0;
}
if (args is ["--crocomire-arena-visuals"])
{
    VerifyCrocomireArenaVisuals();
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
if (args is ["--blue-door-plm-draws"])
{
    VerifyBlueDoorPlmDrawDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--colored-door-plm-draws"])
{
    VerifyColoredDoorPlmDrawDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--grey-door-plm-draws"])
{
    VerifyGreyDoorPlmDrawDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--eye-door-plm-draws"])
{
    VerifyEyeDoorPlmDrawDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--mother-brain-glass-plm-draws"])
{
    VerifyMotherBrainGlassPlmDrawDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--draygon-cannon-plm-program"])
{
    VerifyDraygonCannonPlmProgram();
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
if (args is ["--noob-tube-plm-draws"])
{
    VerifyNoobTubePlmDrawDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--noob-tube-plm-program"])
{
    VerifyNoobTubePlm();
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
if (args is ["--station-animation-programs"])
{
    VerifyStationAnimationProgramDefinitions();
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
if (args is ["--chozo-plm-definitions"])
{
    VerifyChozoStatuePlmDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--samus-eater-plm-definitions"])
{
    VerifySamusEaterPlmDefinitions(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    return 0;
}
if (args is ["--station-access-plm-definitions"])
{
    VerifyStationAccessPlmDefinitions(
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
if (args is ["--room-plm-populations"])
{
    SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
        Path.GetFullPath("Super Metroid.smc"));
    VerifyCompiledRoomPlmPopulationDefinitions(rom);
    VerifyElevatorPlatformPlmDefinitions(rom);
    return 0;
}
if (args is ["--elevator-platform-visuals"])
{
    VerifyElevatorPlatformVisuals();
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
if (args is ["--mother-brain-glass-shard-definitions"])
{
    VerifyMotherBrainGlassShardDefinitions(
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
if (args is ["--load-station-definitions"])
{
    VerifyCompiledLoadStationDefinitions();
    return 0;
}
if (args is ["--room-header-definitions"])
{
    VerifyCompiledRoomHeaderDefinitions();
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
if (args is ["--door-definitions"])
{
    VerifyCompiledDoorDefinitions();
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
if (args is ["--room-state-definitions"])
{
    VerifyCompiledRoomStateSelectionDefinitions();
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
