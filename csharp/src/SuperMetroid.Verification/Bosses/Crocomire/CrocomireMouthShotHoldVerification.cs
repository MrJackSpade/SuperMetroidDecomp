using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Checks the hold a damage-reaction mouth shot adds to Crocomire's animation frame (#1632): eight frames in every fight
/// function, the value the game kept for the projectile attack and for every other function alike, and nothing while a
/// reaction is already pending; and that the reaction reads the camera of the pass that runs it (#1875).
/// </summary>
internal static class CrocomireMouthShotHoldVerification
{
    /// <summary>The frames the body's hold starts at before each shot.</summary>
    private const int StartingHold = 5;

    /// <summary>Shoots the mouth from off screen and with an on-screen Super Missile in every fight phase.</summary>
    internal static void VerifyCrocomireMouthShotHold()
    {
        int expected = WrappingCountdown.Start(StartingHold).AddFrames(8).RemainingFrames;
        int cases = 0;
        (RoomEnemySystem enemies, RoomEnemyPlace body) = NewCrocomireBody();
        foreach (CrocomirePhase phase in Enum.GetValues<CrocomirePhase>())
        {
            foreach ((string place, int cameraX) in new[] { ("off screen", 0), ("on screen", 400) })
            {
                var state = new CrocomireEnemyState(body) { Phase = phase };
                enemies.CrocomireBehavior.Access.Crocomire = state;
                body.AnimationClock.HoldFor(StartingHold);
                body.FlashTimer = 0;

                SamusProjectileType superMissile = SamusProjectileType.Of(SamusProjectileFamily.SuperMissile);
                var camera = new Position16<RoomPixels>(cameraX, 0);
                enemies.CrocomireBehavior.Combat.ResolveCrocomireMouthShot(state, body, superMissile, body.Position.Whole, camera);
                AssertEqual(expected, body.AnimationClock.Hold.RemainingFrames, $"a {place} mouth shot during {phase} holds the frame eight more");

                enemies.CrocomireBehavior.Combat.ResolveCrocomireMouthShot(state, body, superMissile, body.Position.Whole, camera);
                AssertEqual(expected, body.AnimationClock.Hold.RemainingFrames, $"a {place} mouth shot during {phase}'s pending reaction adds no hold");
                cases++;
            }
        }
        Console.WriteLine($"Crocomire mouth shot hold: {cases} shots across every fight function add eight frames once per reaction.");
    }

    /// <summary>
    /// A bomb on Crocomire's mouth tests whether his body's left edge is on screen with the camera the bomb pass carries,
    /// the live camera the original reads there (#1875), and nothing a main update left behind: on the same state, a bomb
    /// with the camera at the room's left edge finds the body off screen and starts a damage reaction, and a bomb with the
    /// camera at column 400 finds it on screen, where a bomb adds no step and starts none.
    /// </summary>
    internal static void VerifyCrocomireMouthShotReadsPassCamera()
    {
        (RoomEnemySystem enemies, RoomEnemyPlace body) = NewCrocomireBody();
        var state = new CrocomireEnemyState(body) { Phase = CrocomirePhase.SteppingForward };
        enemies.CrocomireBehavior.Access.Crocomire = state;
        var ordinaryProjectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads,
            new LeftMuzzleSubpixelCarry(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        var bomb = new SamusBomb(0)
        {
            Type = SamusProjectileType.Of(SamusProjectileFamily.Bomb),
            Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole),
        };

        int expected = WrappingCountdown.Start(StartingHold).AddFrames(CrocomireCombat.MouthShotReactionHoldFrames).RemainingFrames;
        foreach ((string place, int cameraX, bool reacts) in new[] { ("off screen", 0, true), ("on screen", 400, false) })
        {
            state.DamageReactionPending = false;
            body.AnimationClock.HoldFor(StartingHold);
            body.FlashTimer = 0;
            enemies.CrocomireBehavior.Combat.ReactToCrocomireBomb(new EnemyBombContext(body, bomb, ordinaryProjectiles, RoomEnemyFrameFixtures.StandaloneSamus(),
                CrocomireReactions.MouthShot, UsesFrameHitboxes: true, new Position16<RoomPixels>(cameraX, 0))
            {
                StruckOccupant = body.OccupantStates,
            });
            AssertEqual(reacts, state.DamageReactionPending, $"a bomb with the camera at column {cameraX} finds his body {place}");
            AssertEqual(reacts ? expected : StartingHold, body.AnimationClock.Hold.RemainingFrames,
                $"a bomb with the camera at column {cameraX} {(reacts ? "holds" : "leaves")} his frame");
        }
        Console.WriteLine("  Crocomire mouth shot: the reaction reads the pass's camera.");
    }

    /// <summary>A room enemy system whose first actor is Crocomire's body, standing at column 576.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemyPlace Body) NewCrocomireBody()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.Crocomire, body.Definition);
        body.Position = body.Position.WithWhole(new(576, 144));
        return (enemies, body);
    }
}
