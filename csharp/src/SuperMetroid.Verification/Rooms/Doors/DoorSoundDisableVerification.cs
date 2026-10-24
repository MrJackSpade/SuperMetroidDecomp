namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks sound admission during a door transition without suppressing queued audio or music.</summary>
internal static class DoorSoundDisableVerification
{
    /// <summary>Verifies the native rejected-call accumulator, prior sound drain, music admission, re-enable path, and reset.</summary>
    internal static void VerifyDoorSoundDisableGuard()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var audio = new CartridgeAudioState();
        audio.AdvanceFrame(default);
        audio.QueueSound(SoundEffectLibrary1Sounds.StopSpinJump, 15);
        audio.DoorTransitionSoundsDisabled = true;
        ushort rejected = audio.QueueSoundAndGetAccumulator(SoundEffectLibrary1Sounds.CancelAll, 15);
        AssertEqual((ushort)(0x0100 | SoundEffectLibrary1Sounds.CancelAll.Value), rejected,
            "Disabled queue retains the native post-threshold accumulator");
        var commands = audio.AdvanceFrame(default);
        AssertTrue(commands.Any(c => c == new AudioRequest.RequestSound(SoundEffectLibrary1Sounds.StopSpinJump)),
            "Disabling new sounds still submits an already queued request");
        AssertTrue(!audio.HasQueuedSounds, "Rejected request does not refill the drained ring");
        audio.QueueMusicDelayed8(MusicCommand.Stop);
        AssertTrue(audio.HasQueuedMusic, "Door disable flag does not reject music commands");
        audio.DoorTransitionSoundsDisabled = false;
        audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, 15);
        AssertTrue(audio.HasQueuedSounds, "Clearing the door flag restores sound admission");
        audio.DoorTransitionSoundsDisabled = true;
        audio.Reset();
        AssertTrue(!audio.DoorTransitionSoundsDisabled, "Reset clears the reconstructed door guard");
        Console.WriteLine("PASS door sound-disable: native accumulator, existing queue drain, music, re-enable and reset.");
    }
}
