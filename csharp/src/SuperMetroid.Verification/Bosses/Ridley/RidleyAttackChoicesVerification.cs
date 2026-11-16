using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Norfair Ridley's situation-dependent attack table and same-frame selection behavior.</summary>
internal static class RidleyAttackChoicesVerification
{
    /// <summary>
    /// Compares the 40 native choices of the five reachable rows and verifies RNG consumption, branch priority, and
    /// chosen-action setup. The cartridge's second row, a reversed distribution its selector tests for below 9000 health
    /// only after it has already taken the first row below 14400, is never read, so no situation holds it (#1934).
    /// </summary>
    internal static void VerifyRidleyAttackChoices(CartridgeImportAddressSpace rom)
    {
        RidleyAttackSituation? SituationOf(int row) => row switch
        {
            0 => RidleyAttackSituation.Ordinary,
            1 => null,
            2 => RidleyAttackSituation.DamageBoosting,
            3 => RidleyAttackSituation.PogoZone,
            4 => RidleyAttackSituation.SpinJumping,
            _ => RidleyAttackSituation.ZeroHealth,
        };
        var native = new NorfairRidleyPhase[6, 8];
        for (int row = 0; row < 6; row++)
        for (int choice = 0; choice < 8; choice++)
        {
            if (SituationOf(row) is not { } situation)
                continue;
            int address = 0xa6b38c + row * 16 + choice * 2;
            native[row, choice] = RidleyNativeWords.NorfairPhases.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                "Ridley action");
            AssertEqual(native[row, choice], RidleyAttackChoices.Resolve(situation, choice), "Every reachable native Ridley action pointer");
        }

        var actual = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var reference = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        // The power bomb explosion of idle bombs, none exploding, which a missed lunge would dodge.
        var powerBomb = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandalonePowerBombExplosion();
        Action<RoomEnemyPlace, RidleyEnemyState, SamusState, GameplayAction, RoomLevelData> select = (ridley, ridleyState, target, input, level) =>
            actual.RidleyBehavior.SelectNorfairRidleyAttack(ridley, ridleyState, target, input, level, powerBomb);
        Action<RoomEnemyPlace, RidleyEnemyState, SamusState, GameplayAction, RoomLevelData> runReference = (ridley, ridleyState, target, input, level) =>
            reference.RidleyBehavior.RunNorfairRidleyPhase(ridley, ridleyState, target, input, level, powerBomb);
        ushort random = 0;
        int advances = 0, actualReads = 0, referenceReads = 0;
        actual.VerificationServices().NextRandom = () => { advances++; return random; };
        actual.VerificationServices().ReadRandom = () => { actualReads++; return random; };
        reference.VerificationServices().ReadRandom = () => { referenceReads++; return random; };
        var standing = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 256)) };
        var low = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 352)) };
        var spinning = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = SamusPoseId.SpinJumpRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(1000, 256)) };
        // Explicit native branch cases, including spin priority over zero health,
        // health thresholds, and Y immediately before the pogo zone.
        RidleyAttackChoicesCase[] cases =
        [
            new RidleyAttackChoicesCase(0, standing, 5), new RidleyAttackChoicesCase(0, spinning, 4), new RidleyAttackChoicesCase(1, standing, 0),
            new RidleyAttackChoicesCase(8999, standing, 0), new RidleyAttackChoicesCase(9000, standing, 0), new RidleyAttackChoicesCase(14399, low, 0),
            new RidleyAttackChoicesCase(14400, standing, 2), new RidleyAttackChoicesCase(18000, low, 3),
            new RidleyAttackChoicesCase(18000, spinning, 4),
        ];
        // The selector draws its choice through the attack field, which reads exactly the native table's index, the sample
        // modulo 8, on every one of the 65536 samples.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            AssertEqual(raw & 7, RidleyRandomDefinitions.AttackChoice.Of(raw), "The attack field reads the native choice index");
        // The selection frame reads its sample only through these fields: the attack choice, and the wall sweep's time limit
        // and fireball chance, which the spin-jumping situation's same-frame setup reads; every other chosen setup reads none.
        // Two samples that agree on the fields' bits therefore select and set up alike, so every combination of those bits
        // covers every sample, and each combination of the bits no field reads, paired with a different combination of the
        // read bits, confirms that they change nothing.
        int readBits = new[]
        {
            RidleyRandomDefinitions.AttackChoice,
            RidleyRandomDefinitions.WallSweepTimer,
            RidleyRandomDefinitions.WallSweepFireballs.Field,
        }.Aggregate(0, (bits, field) => bits | (field.Outcomes - 1) << field.Shift);
        int[] readCombinations = [.. Enumerable.Range(0, ushort.MaxValue + 1).Where(raw => (raw & ~readBits) == 0)];
        int[] unreadCombinations = [.. Enumerable.Range(0, ushort.MaxValue + 1).Where(raw => raw != 0 && (raw & readBits) == 0)];
        int[] samples = [.. readCombinations, .. unreadCombinations.Select((unread, index) => unread | readCombinations[index % readCombinations.Length])];
        foreach (var sample in cases)
        foreach (int raw in samples)
        {
            random = (ushort)raw;
            advances = actualReads = referenceReads = 0;
            var state = new RidleyEnemyState { NorfairPhase = NorfairRidleyPhase.SelectAttack, ZeroHealthLungeCount = 0 };
            var expected = new RidleyEnemyState
            {
                NorfairPhase = native[sample.Row, raw & 7],
                ZeroHealthLungeCount = sample.Row == 5 ? 1 : 0,
            };
            foreach (var place in new[] { actual.Places.InUpdateOrder[0], reference.Places.InUpdateOrder[0] })
            {
                place.Health = sample.Health;
                place.Position = place.Position.WithWhole(new(128, 352));
            }
            runReference(reference.Places.InUpdateOrder[0], expected, sample.Samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
            select(actual.Places.InUpdateOrder[0], state, sample.Samus, 0, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)));
            AssertEqual(1, advances, "Selector advances RNG exactly once");
            AssertEqual(referenceReads, actualReads, "Chosen setup reads the same current RNG word");
            AssertEqual(expected.NorfairPhase, state.NorfairPhase, "Native chosen callback executes on the selection frame");
            AssertEqual(expected.PhaseTimer, state.PhaseTimer, "Same-frame selected timer");
            AssertEqual(expected.Velocity.X.RawValue, state.Velocity.X.RawValue, "Same-frame selected X motion");
            AssertEqual(expected.Velocity.Y.RawValue, state.Velocity.Y.RawValue, "Same-frame selected Y motion");
            AssertEqual(expected.ZeroHealthLungeCount, state.ZeroHealthLungeCount, "Only a zero-health selection counts a lunge; spin takes priority");
        }
        Console.WriteLine($"Ridley choices: the 40 reachable native pointers, the attack field on all 65536 samples and {cases.Length * samples.Length} actual selections " +
            $"over every combination of the selection frame's read bits pass with table reads forbidden, exact RNG and same-frame setup checks.");
    }

}
