using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Areas.Maridia.MaridiaElevatubeVisualsVerification;
using static SuperMetroid.Verification.Areas.Maridia.LookupElevatubeVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Areas.Maridia;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Maridia elevatube PLM draw, hold, sound, and deletion timing.</summary>
internal static class MaridiaElevatubePlmVerification
{
    /// <summary>Requires the physical block on the first pass and sound after sixteen frames.</summary>
    internal static void VerifyMaridiaElevatubePlm()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevatube oracle revision");
        Suite(nameof(VerifyElevatubePhysicalDraw), () => VerifyElevatubePhysicalDraw(rom));
        Suite(nameof(VerifyElevatubeProgramControls), () => VerifyElevatubeProgramControls(rom));
        Suite(nameof(VerifyElevatubeProgramDraw), () => VerifyElevatubeProgramDraw(rom));
        Suite(nameof(VerifyElevatubeProgramSound), () => VerifyElevatubeProgramSound(rom));

        RoomLevelData level = CreateRoom(new(4, 4), new ushort[16], new byte[16]);
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.WreckedShipTreadmill.TrySpawnMaridiaElevatube(level),
            "native door setup allocates the elevatube PLM");
        var guarded = NewMaridiaElevatubeSourceGuard(new TestAddressSpace());
        int blockIndex = level.GetBlockIndex(MaridiaElevatubeDefinitions.PlmBlock);
        int heardAtFrame = -1;
        for (int frame = 0; frame < 24 && plms.SlotTable.ActiveCount != 0; frame++)
        {
            plms.Step(level);
            if (frame == 0)
                AssertEqual((ushort)0x8180,
                    level.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw,
                    "first elevatube handler pass draws its physical block");
            if (plms.Sounds.SoundRequests.Any(request =>
                    request.SoundEffect == MaridiaElevatubePlmDefinitionsConstants.SoundId))
                heardAtFrame = frame;
        }
        AssertEqual(16, heardAtFrame,
            "elevatube sound follows the native sixteen-frame hold");
        AssertEqual(0, plms.SlotTable.ActiveCount,
            "elevatube PLM deletes after the sound instruction");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "elevatube program and draw execute without their source ROM bytes");
        Console.WriteLine(
            "Maridia elevatube PLM: native delay, draw, sound, and delete pass with source reads forbidden.");
    }

    /// <summary>Forbids runtime reads of the compiled bank $84 program and draw words.</summary>
    internal static CartridgeReadGuard NewMaridiaElevatubeSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
        {
            int pointer = SnesAddressMath.OffsetInBank(address);
            return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                ((pointer >= MaridiaElevatubePlmDefinitionsConstants.InstructionList &&
                  pointer < MaridiaElevatubePlmDefinitionsConstants.InstructionList + 9) ||
                 (pointer >= PlmDrawSourceDefinitions.MaridiaElevatubeBlock.Offset &&
                  pointer < PlmDrawSourceDefinitions.MaridiaElevatubeBlock.Offset + 6));
        }, address => $"Elevatube PLM reread compiled source ${address:X6}.");

    /// <summary>Runs the focused elevatube PLM behavior check.</summary>
    [VerificationCommand("--maridia-elevatube-plm")]
    internal static int MaridiaElevatubePlmCommand()
    {
        Suite(nameof(VerifyMaridiaElevatubePlm), () => VerifyMaridiaElevatubePlm());
        return 0;
    }

    /// <summary>Runs both physical PLM behavior and visual mapping checks.</summary>
    [VerificationCommand("--lookup-stream-4-elevatube")]
    internal static int LookupStream4ElevatubeCommand()
    {
        Suite(nameof(VerifyMaridiaElevatubePlm), () => VerifyMaridiaElevatubePlm());
        Suite(nameof(VerifyMaridiaElevatubeVisuals), () => VerifyMaridiaElevatubeVisuals());
        return 0;
    }
}
