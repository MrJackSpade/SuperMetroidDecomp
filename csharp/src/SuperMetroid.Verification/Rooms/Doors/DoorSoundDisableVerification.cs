namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks sound admission during a door transition without suppressing queued audio or music.</summary>
internal static class DoorSoundDisableVerification
{
    /// <summary>
    /// Verifies the native rejected-call accumulator, prior sound drain, music admission, the re-enable path, and that the
    /// queues keep no door switch of their own (#2499).
    /// </summary>
    internal static void VerifyDoorSoundDisableGuard()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var door = new CheckGameplaySoundState();
        var audio = AudioQueues.Standalone(door);
        audio.AdvanceFrame(default);
        audio.QueueSound(SamusSoundStop.SpinJump, SoundRequestPriority.Essential);
        door.DoorTransitionDisablesSounds = true;
        // The disabled queue still reports the threshold's outcome: admitted with one unread request
        // (formerly asserted as the post-threshold accumulator word $0102).
        SoundQueueAdmission admitted = audio.QueueSound(SamusSoundStop.Any, SoundRequestPriority.Essential);
        AssertEqual(SoundQueueAdmission.Admitted(1), admitted,
            "Disabled queue retains the native post-threshold outcome");
        var commands = audio.AdvanceFrame(default);
        AssertTrue(commands.Any(c => c == new AudioRequest.RequestSound(SamusSoundStop.SpinJump)),
            "Disabling new sounds still submits an already queued request");
        AssertTrue(!audio.HasQueuedSounds, "Rejected request does not refill the drained ring");
        audio.QueueMusic(MusicRequest.Stop);
        AssertTrue(audio.HasQueuedMusic, "Door disable flag does not reject music commands");
        door.DoorTransitionDisablesSounds = false;
        audio.QueueSound(SamusSoundStop.Any, SoundRequestPriority.Essential);
        AssertTrue(audio.HasQueuedSounds, "Clearing the door flag restores sound admission");
        // The switch is the door transition's own state, which the queues read as each request is made: a reset of the
        // queues leaves a door that still disables sounds disabling them (#2499).
        door.DoorTransitionDisablesSounds = true;
        audio.Reset();
        audio.AdvanceFrame(default);
        audio.QueueSound(SamusSoundStop.Any, SoundRequestPriority.Essential);
        AssertTrue(!audio.HasQueuedSounds, "The queues read the door's switch from the door, not from state of their own");
        Console.WriteLine("PASS door sound-disable: native accumulator, existing queue drain, music, re-enable and the door's own switch.");
    }
}
