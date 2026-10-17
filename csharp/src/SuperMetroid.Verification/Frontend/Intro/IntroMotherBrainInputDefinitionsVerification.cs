using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Input;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the Mother Brain flashback's typed demo program against its bank-$91 object and list (#1300).</summary>
internal static class IntroMotherBrainInputDefinitionsVerification
{
    /// <summary>Compares every demo input edge, timer and list position with the native interpreter, then the live intro owner's first record.</summary>
    internal static void VerifyIntroMotherBrainDemoInput(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock)
    {
        ushort ReferenceWord(ushort pointer) => CartridgeImportSource.Require(bus).ReadCartridgeWord(DemoInputRomData.BankBase | pointer);
        var compiled = new DemoInputState();
        var reference = new NativeDemoInputReference(ReferenceWord);
        compiled.Clear();
        compiled.Enable();
        reference.Enable();
        compiled.Load(IntroDemoInputObjectDefinitions.IntroMotherBrainFlashback);
        reference.Load(DemoInputListWords.IntroMotherBrainFlashbackObject, initializationParameter: 0);
        int endFrame = -1;
        for (int frame = 0; frame < 1024; frame++)
        {
            compiled.Step(DemoInputState.RunReturnOnlyPreInstruction, cue =>
            {
                AssertEqual(DemoInputCue.EndIntroMotherBrainFlashback, cue, $"intro Mother Brain demo routine frame {frame}");
                compiled.Disable();
            });
            reference.Step(NativeDemoInputReference.ReturnOnly, (state, routine) =>
            {
                AssertEqual(DemoInputListWords.EndIntroMotherBrainFlashbackRoutine, routine, $"intro Mother Brain native routine frame {frame}");
                state.Disable();
            });
            AssertEqual(reference.Snapshot(), NativeDemoInputReference.Of(compiled), $"intro Mother Brain demo object frame {frame}");
            if (!compiled.IsActive)
            {
                endFrame = frame;
                break;
            }
        }
        AssertTrue(endFrame >= 0 && !compiled.Enabled,
            "intro Mother Brain demo reaches its native disable/delete instruction");

        // The actual cinematic owner loads the same object and plays its first record in the flashback's first dispatch.
        var guarded = NewIntroArtworkSourceReadGuard(bus);
        var intro = CreateRetailIntroFixture(characterArtwork: stock);
        intro.SetupFirstIllustratedPage();
        intro.SetupMotherBrainFlashback();
        intro.StepMotherBrainDemo();
        var live = intro.flashbackDemoInput ?? throw new InvalidOperationException("Verification requires the live flashback demo.");
        var nativeFirst = new NativeDemoInputReference(ReferenceWord);
        nativeFirst.Enable();
        nativeFirst.Load(DemoInputListWords.IntroMotherBrainFlashbackObject, initializationParameter: 0);
        nativeFirst.Step(NativeDemoInputReference.ReturnOnly, NativeDemoInputReference.NoRoutines);
        AssertEqual(nativeFirst.Snapshot(), NativeDemoInputReference.Of(live), "production flashback plays the native first record");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "production flashback reads no guarded intro source");
    }
}
