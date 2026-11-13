using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// Confirms the ending's turning floor repeats beyond its edges exactly while the cartridge keeps its Mode 7 settings 0
/// (#1674): <c>Setup_PPU_ZebesDestruction</c> writes 0 at <c>$8B:82AB</c>, the explosion's setup writes 0 again at
/// <c>$8B:DA66</c>, and <c>SpaceView_GunshipEmergence_Setup</c> writes <c>$80</c> at <c>$8B:DBD1</c> once the planet escape's
/// music is queued. Every phase from the escape's setup through that music wait repeats the floor; the planet escape and
/// everything after show nothing beyond it.
/// </summary>
internal static class EndingFloorOverflowVerification
{
    /// <summary>Fails unless the cartridge writes the settings where the ending's phases change them, and each phase repeats its floor as they say.</summary>
    internal static void VerifyFloorRepeatsWhileSettingsAreZero()
    {
        ISnesAddressSpace bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        // LDA #$00; STA $211A; STA $5F (Setup_PPU_ZebesDestruction), STZ $5F (the explosion's setup), LDA #$80; STA $5F (gunship emergence).
        AssertBytes(bus, 0x8B82AB, [0xA9, 0x00, 0x8D, 0x1A, 0x21, 0x85, 0x5F], "the escape's setup writes Mode 7 settings 0");
        AssertBytes(bus, 0x8BDA66, [0x64, 0x5F], "the explosion's setup writes Mode 7 settings 0 again");
        AssertBytes(bus, 0x8BDBD1, [0xA9, 0x80, 0x85, 0x5F], "the gunship's emergence writes Mode 7 settings $80");
        EndingCreditsState ending = CreateRetailEndingFixture(AudioQueues.Standalone(), gameTimeHours: 2, gameTimeMinutes: 0);
        var wrong = new List<string>();
        int repeating = 0;
        // The phases run in their declared order: every one before the planet escape keeps settings 0.
        HashSet<EndingCreditsPhase> beforeEscape = [.. Enum.GetValues<EndingCreditsPhase>().TakeWhile(phase => phase != EndingCreditsPhase.PlanetEscapeFast)];
        foreach (EndingCreditsPhase phase in Enum.GetValues<EndingCreditsPhase>())
        {
            ending.Stage.Phase = phase;
            bool repeats = EndingCreditsPresentation.FloorRepeatsBeyondEdges(ending);
            bool expected = beforeEscape.Contains(phase);
            if (repeats != expected)
                wrong.Add($"{phase} {(repeats ? "repeats" : "does not repeat")} its floor");
            if (repeats)
                repeating++;
        }
        AssertTrue(wrong.Count == 0, $"ending phases disagree with the cartridge's Mode 7 settings:{Environment.NewLine}{string.Join(Environment.NewLine, wrong)}");
        AssertTrue(EndingCreditsPresentation.FloorRepeatsBeyondEdges(PhaseOf(ending, EndingCreditsPhase.ZebesExplosionAnimation)),
            "the planet's explosion repeats its floor beyond its edges");
        Console.WriteLine($"Ending floor: {repeating} phases from the escape's setup through the planet escape's music wait repeat the floor, as the cartridge's Mode 7 settings 0 do.");
    }

    /// <summary>The ending at <paramref name="phase"/>.</summary>
    private static EndingCreditsState PhaseOf(EndingCreditsState ending, EndingCreditsPhase phase)
    {
        ending.Stage.Phase = phase;
        return ending;
    }

    /// <summary>Fails unless the cartridge holds <paramref name="expected"/> from <paramref name="address"/>.</summary>
    private static void AssertBytes(ISnesAddressSpace bus, int address, byte[] expected, string what)
    {
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], bus.ReadCartridgeByte(address + index), $"{what}: byte ${address + index:X6}");
    }

    /// <summary>Runs the ending floor overflow check.</summary>
    [VerificationCommand("--ending-floor-overflow")]
    internal static int EndingFloorOverflowCommand()
    {
        Suite(nameof(VerifyFloorRepeatsWhileSettingsAreZero), VerifyFloorRepeatsWhileSettingsAreZero);
        return 0;
    }
}
