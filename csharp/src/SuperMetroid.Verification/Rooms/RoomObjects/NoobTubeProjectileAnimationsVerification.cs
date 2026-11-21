using SuperMetroid.Core.Enemies;
using static SuperMetroid.TestSupport.MovieReplay.NoobTubeProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>#641: the n00b tube's crack, shards and bubbles typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class NoobTubeProjectileAnimationsVerification
{
    /// <summary>
    /// Every program matches its native list, each shard's animation (#2044) from the native entry its spawn starts at, which
    /// only loads the flight's count; a recording's pointer at that entry, or in the flight, reads back as the shard's
    /// animation, and the shard's animation before its first play stands at that entry.
    /// </summary>
    internal static void VerifyNoobTubeProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = rom;
        int steps = Family.CompareAll(cartridge);
        Func<EnemyProjectileAnimation, ushort?> pointers = Family.NativePointers(cartridge);
        // The pieces' animations are bound to the tube's own piece behaviour, which carries out their events.
        RoomEnemySystem enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance);
        NoobTubeBehavior.NoobTubeProjectileBehavior pieces = enemies.NoobTubeBehavior.TubePieces;
        foreach ((NoobTubeShard shard, ushort entry) in ShardEntries)
        {
            Animation<NoobTubeProjectileSegment> flight = NoobTubeProjectileAnimations.ShardAnimations[shard];
            AssertEqual<ushort?>(entry, pointers(new NoobTubeProjectileAnimation(pieces, flight)), $"a spawned {shard} shard stands at its native entry");

            var spawned = new NoobTubeProjectileAnimation(pieces, NoobTubeProjectileAnimations.StartingAt[NoobTubeProjectileSegment.Crack]);
            AssertTrue(Family.TryResumeAt(cartridge, spawned, entry, nativeLoopCounter: 0)
                    && ReferenceEquals(spawned.Playback.Animation, flight) && spawned.Playback.Place == AnimationPlace.StartOfAnimation,
                $"a recording at {shard}'s native entry resumes before the shard's first flicker");

            // Between two flickers of the flight the recording rests on the flight's count-down, its loop counter holding
            // the flickers left.
            var flying = new NoobTubeProjectileAnimation(pieces, flight);
            flying.SetPlayback(new AnimationPlayback<NoobTubeProjectileSegment>(flight, new AnimationPlace(0, flight.Phases[0].BeatCount).WithPlaysLeft(20)));
            var flown = new NoobTubeProjectileAnimation(pieces, NoobTubeProjectileAnimations.StartingAt[NoobTubeProjectileSegment.Crack]);
            AssertTrue(pointers(flying) is { } resting && resting != entry && Family.TryResumeAt(cartridge, flown, resting, nativeLoopCounter: 20)
                    && ReferenceEquals(flown.Playback.Animation, flight) && flown.Playback.Place == flying.Playback.Place,
                $"a recording between {shard}'s flickers resumes the shard's flight with the flickers it has left");
        }
        Console.WriteLine($"  n00b tube projectile programs: {Entries.Length} programs and {ShardEntries.Count} shard entries ({steps} steps) match bank $86.");
    }
}
