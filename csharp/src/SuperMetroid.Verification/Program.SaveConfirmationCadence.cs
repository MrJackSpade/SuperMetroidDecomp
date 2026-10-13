using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using static RetailPresentationFixtureVerification;

/// <summary>Checks the extra wait between save-confirmation cursor moves and the next controller read.</summary>
internal static class SaveConfirmationCadenceVerification
{
    /// <summary>Confirms selection redraw waits one frame, queues sound, and makes the next controller read three frames after a move.</summary>
    /// <remarks>#1269: native $85:84BA waits twice and $85:8532 adds the redraw wait. Missing it shifted the gunship's A read in the 100% movie.</remarks>
    internal static void VerifySaveConfirmationCadence()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var box = CreateGameplayMessageFixture();
        box.Begin(retail, GameplayMessageId.GunshipSaveConfirmation, 0);
        int guard = 0;
        while (box.Phase != GameplayMessageBoxPhase.AwaitingInput)
        {
            box.Step(0);
            AssertTrue(++guard < 200, "the confirmation opens");
        }

        // Find the frame of a read by holding Right until the cursor moves.
        int ReadsUntilToggle(ushort hold)
        {
            bool before = box.ConfirmationSelectionYes;
            for (int frame = 1; frame <= 8; frame++)
            {
                box.Step(hold);
                if (box.ConfirmationSelectionYes != before)
                    return frame;
            }
            throw new InvalidOperationException("The cursor never moved.");
        }
        ReadsUntilToggle((ushort)SnesButton.Right);

        // The frame after the toggling read is the redraw's wait: no audio handlers, and the
        // selection sound is queued there.
        box.Step(0);
        AssertTrue(box.ConfirmationSelectionChangedThisFrame, "the selection sound follows the redraw wait");
        AssertEqual(0, box.LastFrameAudio.MusicHandlerCalls, "the redraw wait runs no music handler");
        AssertEqual(0, box.LastFrameAudio.SoundHandlerCalls, "the redraw wait runs no sound handler");

        // Then the loop's first wait; its second ends in the read, three frames after the
        // toggling read.
        box.Step(0);
        AssertEqual(1, ReadsUntilToggle((ushort)SnesButton.Left),
            "the next read lands on the third frame after the toggling read");
        Console.WriteLine("Save confirmation cadence: a cursor move adds the redraw's lag frame before the next read.");
    }

    /// <summary>Runs the focused save-confirmation timing verification command.</summary>
    [VerificationCommand("--save-confirmation-cadence")]
    internal static int SaveConfirmationCadenceCommand()
    {
        VerifySaveConfirmationCadence();
        return 0;
    }
}
