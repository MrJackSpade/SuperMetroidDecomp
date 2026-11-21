using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verification access to <see cref="SuperMetroidGame"/> members production does not use.</summary>
internal static class SuperMetroidGameAccess
{
    /// <summary>Exposes selected frontend identity and direct-room verification setup.</summary>
    extension(SuperMetroidGame self)
    {
        /// <summary>Identity of the installed area-map presentation.</summary>
        internal string MapPresentationIdentity => self.Artwork.ScreenContent.ContentIdentity;

        /// <summary>Random word retained by the frontend dispatcher.</summary>
        internal int DispatcherRandomNumber => self.Session.Random.Read();

        /// <summary>
        /// Initialize the ordinary gameplay runtime for a friend verifier that
        /// directly loads a retail room. This uses the same runtime setup as a new
        /// game, then omits only the title/intro/fade dispatcher frames. It is not
        /// an alternate playable entry point or a substitute for the separate
        /// full-startup integration test.
        /// </summary>
        internal void InitializeDirectRoomVerification()
        {
            if (self.Session.Runtime is not null || !self.IsAt(TitleStep.Starting))
                throw new InvalidOperationException(
                    "Direct-room verification requires a fresh frontend.");
            if (!self.Session.SetupSelectedGame())
                throw new InvalidOperationException(
                    "New-game runtime setup did not select the Ceres arrival path.");
            self.Session.Enter(SuperMetroidGameState.MainGameplay);
        }

        /// <summary>
        /// Installs a prepared runtime as the session's live runtime. A session has one random-number word (#1330), so
        /// the runtime must be built over the game's word, and one set of sound and music queues (#2057), so it must be
        /// built over the game's queues (the runtime fixture's <c>audio</c>). The queues return to the state the game's
        /// power-on left them in: what the runtime requested while it was prepared reaches no game, as a runtime the session
        /// creates requests nothing before the game runs it.
        /// </summary>
        /// <param name="runtime">A runtime built over <c>Session.Random</c> and <c>Session.audio</c>.</param>
        /// <exception cref="InvalidOperationException">The runtime was built over a different word.</exception>
        internal void AdoptRuntime(SuperMetroidRuntime runtime)
        {
            Ensure.NotNull(runtime);
            if (!ReferenceEquals(runtime.System.Random, self.Session.Random))
                throw new InvalidOperationException(
                    "A prepared runtime must be built over its game's random-number word; pass the game's word to the runtime fixture.");
            self.Session.audio.Reset();
            self.Session.Runtime = runtime;
        }

        /// <summary>
        /// Adopts a prepared runtime as the live game, establishing the dispatcher state a production gameplay entry
        /// does: the room whose music is already queued and the main-gameplay state.
        /// </summary>
        /// <param name="runtime">A runtime with a loaded room.</param>
        /// <exception cref="InvalidOperationException">The dispatcher has left reset or already holds a runtime.</exception>
        internal void EnterMainGameplay(SuperMetroidRuntime runtime)
        {
            Ensure.NotNull(runtime);
            if (self.Session.Runtime is not null || !self.IsAt(TitleStep.Starting))
                throw new InvalidOperationException($"Main gameplay can be entered only from a fresh dispatcher; this one is in {self.GameState}.");
            RoomStateSettings roomState = runtime.Room.LoadedRoom.State;
            self.AdoptRuntime(runtime);
            self.Session.LastAudioRoomState = (global::SuperMetroid.Core.World.RoomStates.RoomStateSettings?)(roomState);
            self.Session.Enter(SuperMetroidGameState.MainGameplay);
        }

        /// <summary>
        /// Exposes the cartridge coroutine phase only to the friend verification/debug hosts.
        /// Production UI code continues to consume the coarser public game state.
        /// </summary>
        internal DoorTransitionPhase DoorTransitionPhaseForVerification => self.Session.doorTransition.Phase;
        /// <summary>The installed catalogs the host bound (#2224).</summary>
        public GameArtwork Artwork { get => self.Session.Artwork; }
    }
}
