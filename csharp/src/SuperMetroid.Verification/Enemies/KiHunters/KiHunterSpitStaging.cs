using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>
/// Brings the Wrecked Ship's Attic, once Phantoon is dead, to its Ki-Hunter's acid spit as the game does (#2554): the room's
/// population places two yellow Ki-Hunters wingless, the ground-bound form, which fall to the floor, wait, and spit at Samus
/// when she stands within their spit range across. Samus stands left of the one placed at x 1049, within its range and out
/// of the other's, so that one's own AI and animation lead it to the room's first spit.
/// </summary>
internal static class KiHunterSpitStaging
{
    /// <summary>
    /// Where Samus stands: 49 pixels left of the wingless Ki-Hunter placed at x 1049, within its spit range
    /// (<see cref="SuperMetroid.Core.Enemies.KiHunters.KiHunterMotionDefinitions.SpitRange"/>), and far out of the range of
    /// the one placed at x 1256.
    /// </summary>
    private static readonly Position16<RoomPixels> SamusPlace = new(1000, 190);

    /// <summary>
    /// Records the Wrecked Ship's boss defeated, as Phantoon's death records it, loads the Attic into
    /// <paramref name="runtime"/>, which selects the room's state for a dead area boss and its wingless Ki-Hunters, and stands
    /// Samus at <see cref="SamusPlace"/> facing <paramref name="facing"/>.
    /// </summary>
    /// <param name="runtime">The runtime the room loads into.</param>
    /// <param name="facing">The pose Samus stands in.</param>
    internal static SamusState LoadAtticAfterPhantoon(SuperMetroidRuntime runtime, SamusPoseId facing)
    {
        Ensure.NotNull(runtime).System.RecordBossesDefeated(AreaId.WreckedShip, DefeatedBosses.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Attic);
        SamusState samus = runtime.Player.Samus;
        samus.PoseId = facing;
        samus.Position = samus.Position.WithWhole(SamusPlace);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        return samus;
    }

    /// <summary>
    /// A game in main gameplay in the Attic after Phantoon (<see cref="LoadAtticAfterPhantoon"/>), its Samus locked out of
    /// control, with the session's own runtime: the game sets a new game up and the room loads into its runtime.
    /// </summary>
    /// <param name="hostServices">The game's hardware-timing policies, or none for the fixture's.</param>
    internal static (SuperMetroidGame Game, SuperMetroidRuntime Runtime) AtticGame(SuperMetroidGameHostServices? hostServices = null)
    {
        SuperMetroidGame game = Harness.RetailGameFixtureVerification.CreateRetailGameFixture(
            new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps),
            new SuperMetroid.Core.Gameplay.GameplayAssists { SamusCannotDie = true }, renderGameplayFrames: false, hostServices: hostServices,
            random: new SharedRandom());
        game.InitializeDirectRoomVerification();
        SuperMetroidRuntime runtime = game.GameplayRuntimeForVerification;
        LoadAtticAfterPhantoon(runtime, SamusPoseId.FacingLeftNormalPose).Control.LockOutPlayer();
        return (game, runtime);
    }

    /// <summary>
    /// How many updates a run of the room <paramref name="room"/> builds takes before the update on which its Ki-Hunter's
    /// spit reaches the enemies' sound library: every run of the same room, Samus and updates takes as many.
    /// </summary>
    /// <param name="room">Builds the room, as the check builds it, and returns the session's queues and one of its updates.</param>
    internal static int UpdatesBeforeTheSpit(Func<(AudioQueues Audio, Action Update)> room)
    {
        (AudioQueues audio, Action step) = Ensure.NotNull(room)();
        for (int update = 0; ; update++)
        {
            SoundRequestsMark mark = audio.MarkSoundRequestsForVerification(SoundEffectLibrary.Enemies);
            step();
            if (audio.SoundsQueuedSinceForVerification(mark).Any(sound => sound == EnemySound.AcidSpit))
                return update;
        }
    }
}
