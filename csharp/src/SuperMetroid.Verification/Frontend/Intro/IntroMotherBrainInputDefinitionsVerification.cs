using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Input;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the Mother Brain flashback's typed demo program against its bank-$91 object and list (#1300).</summary>
internal static class IntroMotherBrainInputDefinitionsVerification
{
    /// <summary>Compares every demo input edge, timer and list position with the native interpreter, then the live intro owner's first record.</summary>
    internal static void VerifyIntroMotherBrainDemoInput(CartridgeImportAddressSpace bus,
        ImportIntroCinematicArtwork stock)
    {
        ushort ReferenceWord(ushort pointer) => bus.ReadCartridgeWord(DemoInputRomData.BankBase | pointer);
        var compiled = new DemoInputState();
        var reference = new NativeDemoInputReference(ReferenceWord);
        compiled.Clear();
        compiled.Enable();
        reference.Enable();
        compiled.Load(DemoInputRecordingId.IntroMotherBrainFlashback);
        reference.Load(DemoInputListWords.IntroMotherBrainFlashbackObject, initializationParameter: 0);
        int endFrame = -1;
        for (int frame = 0; frame < 1024; frame++)
        {
            compiled.Step(cue =>
            {
                AssertEqual(DemoInputCue.EndIntroMotherBrainFlashback, cue, $"intro Mother Brain demo routine frame {frame}");
                compiled.Disable();
            });
            reference.Step(NativeDemoInputReference.ReturnOnly, (state, routine) =>
            {
                AssertEqual(DemoInputListWords.EndIntroMotherBrainFlashbackRoutine, routine, $"intro Mother Brain native routine frame {frame}");
                state.Disable();
            });
            AssertEqual(reference.Snapshot(), NativeDemoInputReference.Of(compiled, DemoInputNativeListAddresses.ReturnOnlyAddress), $"intro Mother Brain demo object frame {frame}");
            if (!compiled.IsActive)
            {
                endFrame = frame;
                break;
            }
        }
        AssertTrue(endFrame >= 0 && !compiled.Enabled,
            "intro Mother Brain demo reaches its native disable/delete instruction");

        // The actual cinematic plays the same demo from its first record in the flashback's setup update (#2554): the shown
        // Samus fires no shot before the update the demo first presses the shot on, and her shot shows on that update.
        var played = new DemoInputState();
        played.Clear();
        played.Enable();
        played.Load(DemoInputRecordingId.IntroMotherBrainFlashback);
        int firstShot = 0;
        do
        {
            firstShot++;
            AssertTrue(firstShot < 1024, "the intro Mother Brain demo presses the shot");
            played.Step(_ => { });
        }
        while ((played.NewlyPressed & GameplayAction.Shoot) == 0);
        var guarded = NewIntroArtworkSourceReadGuard(bus);
        var audio = AudioQueues.Standalone();
        var intro = CreateRetailIntroFixture(audio, characterArtwork: stock);
        _ = intro.StepToMotherBrainFlashback(audio);
        for (int update = 1; update <= firstShot; update++)
        {
            if (update > 1)
            {
                intro.Step(ControllerButton.None);
                audio.AdvanceFrame(default);
            }
            bool shotShown = intro.ComposeScene().SpriteDraws.Any(draw => draw.Source.Kind == SpriteSourceKind.SamusProjectile);
            AssertEqual(update == firstShot, shotShown, $"flashback update {update} shows Samus's shot only once the demo's first shot press plays");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts, "production flashback reads no guarded intro source");
    }
}
