namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks sound admission during a door transition without suppressing queued audio or music.</summary>
internal static class DoorSoundDisableVerification
{
    /// <summary>Verifies the native rejected-call accumulator, prior sound drain, music admission, re-enable path, and reset.</summary>
    internal static void VerifyDoorSoundDisableGuard()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var audio = AudioQueues.Standalone();
        audio.AdvanceFrame(default);
        audio.QueueSound(SamusSound.SpinJumpEnd, SoundRequestPriority.Essential);
        audio.DoorTransitionSoundsDisabled = true;
        // The disabled queue still reports the threshold's outcome: admitted with one unread request
        // (formerly asserted as the post-threshold accumulator word $0102).
        SoundQueueAdmission admitted = audio.QueueSound(SamusSound.Silence, SoundRequestPriority.Essential);
        AssertEqual(SoundQueueAdmission.Admitted(1), admitted,
            "Disabled queue retains the native post-threshold outcome");
        var commands = audio.AdvanceFrame(default);
        AssertTrue(commands.Any(c => c == new AudioRequest.RequestSound(SamusSound.SpinJumpEnd)),
            "Disabling new sounds still submits an already queued request");
        AssertTrue(!audio.HasQueuedSounds, "Rejected request does not refill the drained ring");
        audio.QueueMusic(MusicRequest.Stop);
        AssertTrue(audio.HasQueuedMusic, "Door disable flag does not reject music commands");
        audio.DoorTransitionSoundsDisabled = false;
        audio.QueueSound(SamusSound.Silence, SoundRequestPriority.Essential);
        AssertTrue(audio.HasQueuedSounds, "Clearing the door flag restores sound admission");
        audio.DoorTransitionSoundsDisabled = true;
        audio.Reset();
        AssertTrue(!audio.DoorTransitionSoundsDisabled, "Reset clears the reconstructed door guard");
        Console.WriteLine("PASS door sound-disable: native accumulator, existing queue drain, music, re-enable and reset.");
    }
}
