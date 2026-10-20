using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verification access to <see cref="SuperMetroidGame"/> members production does not use.</summary>
internal static class SuperMetroidGameAccess
{
    /// <summary>Exposes selected frontend identity and direct-room verification setup.</summary>
    extension(SuperMetroidGame self)
    {
        /// <summary>Identity of the installed area-map presentation, if present.</summary>
        internal string? MapPresentationIdentity => self.Artwork.MapPresentation?.ContentIdentity;

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
            if (self.Session.runtime is not null || self.GameState != SuperMetroidGameState.Reset)
                throw new InvalidOperationException(
                    "Direct-room verification requires a fresh frontend.");
            if (!self.Session.SetupSelectedGame())
                throw new InvalidOperationException(
                    "New-game runtime setup did not select the Ceres arrival path.");
            self.GameState = SuperMetroidGameState.MainGameplay;
        }

        /// <summary>
        /// Installs a prepared runtime as the session's live runtime. A session has one random-number word (#1330), so
        /// the runtime must be built over the game's word.
        /// </summary>
        /// <param name="runtime">A runtime built over <c>Session.Random</c>.</param>
        /// <exception cref="InvalidOperationException">The runtime was built over a different word.</exception>
        internal void AdoptRuntime(SuperMetroidRuntime runtime)
        {
            Ensure.NotNull(runtime);
            if (!ReferenceEquals(runtime.System.Random, self.Session.Random))
                throw new InvalidOperationException(
                    "A prepared runtime must be built over its game's random-number word; pass the game's word to the runtime fixture.");
            self.Session.runtime = runtime;
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
            if (self.Session.runtime is not null || self.GameState != SuperMetroidGameState.Reset)
                throw new InvalidOperationException($"Main gameplay can be entered only from a fresh dispatcher; this one is in {self.GameState}.");
            CartridgeRoomState roomState = runtime.Room.LoadedRoom.State;
            self.AdoptRuntime(runtime);
            self.Session.lastAudioRoomState = roomState;
            self.GameState = SuperMetroidGameState.MainGameplay;
        }

        /// <summary>
        /// Exposes the cartridge coroutine phase only to the friend verification/debug hosts.
        /// Production UI code continues to consume the coarser public game state.
        /// </summary>
        internal DoorTransitionPhase DoorTransitionPhaseForVerification => self.Session.doorTransition.Phase;
        /// <summary>The installed catalogs currently bound; <see cref="GameArtwork.Unbound"/> until a host binds them.</summary>
        public GameArtwork Artwork { get => self.Session.Artwork; }
    }
}
