using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>
/// Checks the Zebes reveal's pixelation (#1774) against the cartridge's fly-to-Zebes routines: the reveal's planet picture
/// is published pixelated in the blocks the cartridge's mosaic setting shows on its planet layer, on every update from the
/// reveal's setup to the update the mosaic transition clears it, and the reveal ends on exactly that update.
/// </summary>
internal static class ZebesRevealPixelationVerification
{
    /// <summary>The operand of the reveal's setup's <c>LDA.B #$81</c> at <c>$8B:C77B</c>, stored to the mosaic setting.</summary>
    private const int StartingSettingOperand = 0x8bc77c;
    /// <summary>The operand of the fading-in routine's <c>BIT.W #$0003</c> frame counter test at <c>$8B:C79F</c>.</summary>
    private const int FadeInCadenceOperand = 0x8bc7a0;
    /// <summary>The operand of the fading-in routine's <c>SBC.B #$10</c> at <c>$8B:C7A9</c>.</summary>
    private const int FadeInStepOperand = 0x8bc7aa;
    /// <summary>The operand of the mosaic transition's <c>BIT.W #$0003</c> frame counter test at <c>$8B:C7CD</c>.</summary>
    private const int TransitionCadenceOperand = 0x8bc7ce;
    /// <summary>The operand of the mosaic transition's <c>SBC.B #$10</c> at <c>$8B:C7D7</c>.</summary>
    private const int TransitionStepOperand = 0x8bc7d8;
    /// <summary>The operand of the mosaic transition's <c>BIT.B #$F0</c> clear test at <c>$8B:C7DB</c>.</summary>
    private const int TransitionClearOperand = 0x8bc7dc;

    /// <summary>
    /// Runs the Ceres destruction through the Zebes reveal beside a model of the cartridge's mosaic setting, built from the
    /// routines' own operands: set as the reveal is set up, lowered on the updates the frame counter test passes in the
    /// fading-in and transition routines, the transition ending the reveal once its size clears. The planet picture is
    /// the setting's first background (the reveal's PPU setup at <c>$8B:8230</c> puts the planet's tilemap there), so the
    /// published pixelation is the setting's first-background block width on every update.
    /// </summary>
    internal static void VerifyZebesRevealPixelation(CartridgeImportAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        byte startingSetting = cartridge.ReadCartridgeByte(StartingSettingOperand);
        ushort fadeInCadence = cartridge.ReadCartridgeWord(FadeInCadenceOperand);
        byte fadeInStep = cartridge.ReadCartridgeByte(FadeInStepOperand);
        ushort transitionCadence = cartridge.ReadCartridgeWord(TransitionCadenceOperand);
        byte transitionStep = cartridge.ReadCartridgeByte(TransitionStepOperand);
        byte transitionClear = cartridge.ReadCartridgeByte(TransitionClearOperand);

        CeresDestructionCinematicState scene = InstalledFixtures.CreateDestruction();
        byte? setting = null;
        bool ended = false;
        var shownWidths = new List<int>();
        for (int update = 0; !ended; update++)
        {
            AssertTrue(update < 6_000, "the Ceres destruction reaches the end of the Zebes reveal");
            // The scene's frame counter counts its updates from its setup, after its opening waits, and its phase routine
            // reads it before the update's increment.
            int frameCounter = update - CeresDestructionSceneDefinitions.InitialFrameWaits;
            CeresDestructionPhase before = scene.Phase;
            scene.Step();
            bool cleared = false;
            if (before == CeresDestructionPhase.FlyToZebesInitial)
                setting = startingSetting;
            else if (setting is { } shown && before == CeresDestructionPhase.FadeInZebes && (frameCounter & fadeInCadence) == 0)
                setting = unchecked((byte)(shown - fadeInStep));
            else if (setting is { } held && before == CeresDestructionPhase.RemoveZebesMosaic && (frameCounter & transitionCadence) == 0)
            {
                setting = unchecked((byte)(held - transitionStep));
                cleared = (setting.Value & transitionClear) == 0;
            }
            if (setting is not { } current)
                continue;

            LayeredScene snapshot = scene.CaptureRenderSnapshot();
            PicturedPlaneLayer[] planes = snapshot.LayersOf<PicturedPlaneLayer>();
            if (before == CeresDestructionPhase.RemoveZebesMosaic)
            {
                AssertEqual(cleared ? CeresDestructionPhase.PlanetZebesTitle : CeresDestructionPhase.RemoveZebesMosaic, scene.Phase,
                    $"the reveal ends on the update the cartridge's mosaic transition clears its size (update {update})");
            }
            if (before == CeresDestructionPhase.FadeInZebes && scene.Phase == CeresDestructionPhase.RemoveZebesMosaic)
                AssertEqual(DisplayBrightness.Full, snapshot.Brightness, $"the reveal's fade-in ends at full brightness (update {update})");
            if (cleared)
            {
                AssertEqual(0, planes.Length, $"the reveal's planet picture gives way to the turning floor as it clears (update {update})");
                ended = true;
                continue;
            }
            AssertEqual(1, planes.Length, $"the reveal shows its planet picture (update {update})");
            int expected = BackgroundMosaicSamplingConstants.ForBg1(current).Size;
            AssertEqual(expected, planes[0].Pixelation.Size,
                $"the reveal's planet picture shows the cartridge's mosaic block width (update {update}, setting ${current:X2})");
            if (shownWidths.Count == 0 || shownWidths[^1] != expected)
                shownWidths.Add(expected);
        }
        Console.WriteLine($"  Zebes reveal pixelation: block widths {string.Join(", ", shownWidths)} follow the cartridge's mosaic setting on every update.");
    }
}
