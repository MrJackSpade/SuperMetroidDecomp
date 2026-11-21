using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Verification.Audio;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Audio;

/// <summary>Checks the Sciser contact-damage sound that delayed a door transition in the 13% recording.</summary>
internal static class ContactDamageSoundVerification
{
    /// <summary>
    /// When a speed-boosting, sparking or screw-attacking Samus damages an enemy by touch,
    /// OrdinaryTouchAI queues library-two $0B with QueueSound_Lib2_Max1 ($A0:A55A). In the
    /// 13% movie Samus speed-boosts through an enemy before a door; the queued sound holds
    /// the door's sound wait for an extra frame.
    /// </summary>
    internal static void VerifyContactDamageSound()
    {
        // QueueSound_Lib2_Max1 admits the request only while library two's queue is empty: behind no other request the
        // hit's sound joins, and behind one request made earlier in the update the queue turns it away.
        AssertSequenceEqual<SoundEffectId>([EnemySound.ContactKill], TouchSciser(competing: 0), "the hit queues library-two $0B");
        AssertSequenceEqual<SoundEffectId>([Competitor], TouchSciser(competing: 1),
            "behind one waiting library-two request the hit's sound is turned away, as QueueSound_Lib2_Max1 does");
        Console.WriteLine("  Contact-damage sound: Samus's damaging touch queues library-two $0B only into an empty queue.");
    }

    /// <summary>A library-two sound the check queues ahead of the hit's.</summary>
    private static SoundEffectId Competitor => EnemySound.Beacon;

    /// <summary>
    /// Has a speed-boosting Samus damage Mt. Everest's Sciser by touch behind <paramref name="competing"/> library-two
    /// requests made earlier in the update, and returns the sounds library two's queue admitted from the competitors on.
    /// </summary>
    private static IReadOnlyList<SoundEffectId> TouchSciser(int competing)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MtEverest);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.HorizontalSpeed.BodyAttack = SamusBodyAttack.SpeedBooster;
        RoomEnemyPlace sciser = runtime.Enemies.Places.InUpdateOrder.First(place => place.Holds(EnemyDefinitionId.Sciser));
        int healthBefore = sciser.Health;

        runtime.AudioRequests.BeginRecordedUpdate();
        runtime.AudioRequests.FillQueueAhead(Competitor, competing);
        runtime.Enemies.Collision.ResolveOrdinaryTouch(sciser.HeldOccupant(), samus, RoomEnemyFrameFixtures.StandalonePseudoScrew(), TouchDeathAnimation.Play);

        AssertTrue(sciser.Health < healthBefore, "the speed booster damages the Sciser");
        return runtime.AudioRequests.SoundsQueued;
    }

    /// <summary>Runs the focused Speed Booster contact-sound check from the verification command line.</summary>
    [VerificationCommand("--contact-damage-sound")]
    internal static int ContactDamageSoundCommand()
    {
        Suite(nameof(VerifyContactDamageSound), () => VerifyContactDamageSound());
        return 0;
    }
}
