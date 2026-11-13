using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Powamp;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Powamp's initial animations and the balloon phase its AI derives from the typed cursor.</summary>
internal static class PowampBehaviorVerification
{
    /// <summary>
    /// The real initializers start the balloon deflated and the body on its slow pulse. At every place of the balloon's
    /// animations, the phase the AI reads matches the native pointer it stands for: that sequence's first list plus four
    /// bytes per shown pose.
    /// </summary>
    internal static void VerifyPowampBehavior(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot balloon = enemies.SlotTable.Slots[0];
        balloon.Bind(EnemyDefinitionId.Powamp, balloon.Definition);
        balloon.Bind(balloon.DefinitionId, default(RoomEnemyDefinition));
        balloon.SetSpawnParameter1(EnemyDefinitionId.Powamp, 1);
        enemies.PowampBehavior.InitializePowamp(balloon);
        RoomEnemySlot body = enemies.SlotTable.Slots[1];
        body.Bind(EnemyDefinitionId.Powamp, body.Definition);
        body.Bind(body.DefinitionId, default(RoomEnemyDefinition));
        body.SetSpawnSettings(new PowampSpawnSettings(PowampPart.Body, GrappleTravelDistance: 0));
        enemies.PowampBehavior.InitializePowamp(body);
        AssertEqual(new AnimationPlayback<PowampProgram>(PowampAnimations.StartingWith(PowampProgram.BalloonDeflated), AnimationPlace.StartOfAnimation), balloon.RequireSpeciesState<PowampEnemyState>().Playback,
            "the balloon starts deflated");
        AssertEqual(new AnimationPlayback<PowampProgram>(PowampAnimations.StartingWith(PowampProgram.BodySlow), AnimationPlace.StartOfAnimation), body.RequireSpeciesState<PowampEnemyState>().Playback,
            "the body starts on its slow pulse");

        // The ROM oracle holds every balloon beat to a four-byte native frame record, so the native pointer at a place
        // is its segment's entry plus four bytes per beat passed, the list's closing control at its end included; the
        // native record there is the pose's own.
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        foreach (PowampProgram start in new[] { PowampProgram.BalloonInflate0, PowampProgram.BalloonInflate1, PowampProgram.BalloonStartSinking, PowampProgram.BalloonDeflated })
        {
            Animation<PowampProgram> animation = PowampAnimations.StartingWith(start);
            AssertBalloonPhase(cartridge, new(animation, AnimationPlace.StartOfAnimation), start, 0, $"Powamp balloon {start} before it starts");
            for (int phase = 0; phase < animation.Phases.Count; phase++)
            for (int beat = 0; beat <= animation.Phases[phase].BeatCount; beat++)
            {
                AssertBalloonPhase(cartridge, new(animation, new AnimationPlace(phase, beat)), animation.Phases[phase].Segment, animation.Phases[phase].FirstBeat + beat,
                    $"Powamp balloon {start} phase {phase} beat {beat}");
            }
        }
        Console.WriteLine("  Powamp behavior: both initial animations and every balloon step's native phase pass.");
    }

    /// <summary>
    /// The balloon phase the AI reads at <paramref name="playback"/> stands for the native pointer <paramref name="passed"/>
    /// four-byte records into <paramref name="segment"/>'s list, where the native record is the pose's own.
    /// </summary>
    private static void AssertBalloonPhase(IImportCartridgeSource cartridge, AnimationPlayback<PowampProgram> playback, PowampProgram segment, int passed, string context)
    {
        (bool sinking, int shown) = PowampBehavior.PowampBalloonPhase(playback);
        ushort sequence = NativeEnemyAnimationOracle.EntryOf(PowampNativeListAddresses.Entries, sinking ? PowampProgram.BalloonStartSinking : PowampProgram.BalloonInflate0);
        ushort pointer = (ushort)(NativeEnemyAnimationOracle.EntryOf(PowampNativeListAddresses.Entries, segment) + 4 * passed);
        AssertEqual(pointer, (ushort)(sequence + 4 * shown), context);
        IReadOnlyList<AnimationBeat<PowampFrame, NoAnimationCue>> beats = PowampAnimations.Segments.Beats(segment);
        if (passed < beats.Count)
        {
            AssertEqual(AssertIs<AnimationBeat<PowampFrame, NoAnimationCue>.Frame>(beats[passed], context).Duration,
                (int)cartridge.ReadCartridgeWord((PowampNativeListAddresses.Bank << 16) | pointer), $"{context}: native pose record");
        }
    }

    /// <summary>Runs the Powamp behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--powamp-instruction-mechanics")]
    internal static int PowampBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPowampBehavior), () => VerifyPowampBehavior(rom));
        Suite(nameof(PowampAnimationsVerification.VerifyPowampAnimations), () => PowampAnimationsVerification.VerifyPowampAnimations(rom));
        return 0;
    }
}
