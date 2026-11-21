using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>
/// Brings the Crateria Ki Hunter room to its Ki Hunter's acid spit as the game does (#2554): its green Ki Hunter is placed
/// wingless, as a room places the ground-bound form, so it falls, lands, waits and spits at Samus within its range, its
/// own AI and animation leading it to the spit cue.
/// </summary>
internal static class KiHunterSpitStaging
{
    /// <summary>Where the wingless Ki Hunter is placed: above the floor, within its spit range of Samus at x 64.</summary>
    private static readonly Position16<RoomPixels> WinglessPlacement = new(128, 96);

    /// <summary>Places the room's green Ki Hunter wingless, set up as the population load sets it up, and returns its place.</summary>
    /// <param name="runtime">The runtime whose loaded room holds the Ki Hunter.</param>
    internal static RoomEnemyPlace PlaceWingless(SuperMetroidRuntime runtime)
    {
        RoomEnemyPlace body = runtime.Enemies.Places.InUpdateOrder.First(place => place.Holds(EnemyDefinitionId.KihunterGreen));
        _ = body.PlaceAs<KiHunterBodyEntity>(EnemyDefinitionId.KihunterGreen, new KiHunterSpawnSettings(KiHunterWings.Wingless), runtime.Player.Samus,
            WinglessPlacement, runtime.Room.LoadedLevelData);
        return body;
    }

    /// <summary>
    /// How many main gameplay updates, nothing pressed, a run of the room <paramref name="room"/> builds takes before the
    /// update on which its wingless Ki Hunter spits: every run of the same room and placement takes as many.
    /// </summary>
    /// <param name="room">Builds the room, its Ki Hunter placed wingless, as the check builds it.</param>
    internal static int UpdatesBeforeTheSpit(Func<SuperMetroidRuntime> room)
    {
        SuperMetroidRuntime runtime = Ensure.NotNull(room)();
        for (int update = 0; ; update++)
        {
            runtime.AudioRequests.BeginRecordedUpdate();
            runtime.StepFrame(0, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.MainGameplay });
            if (runtime.AudioRequests.SoundRequests.Any(request => request.SoundEffect == EnemySound.AcidSpit))
                return update;
        }
    }

    /// <summary>Runs <paramref name="runtime"/> <paramref name="updates"/> main gameplay updates, nothing pressed.</summary>
    internal static void Run(SuperMetroidRuntime runtime, int updates)
    {
        for (int update = 0; update < updates; update++)
            Ensure.NotNull(runtime).StepFrame(0, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.MainGameplay });
    }
}
