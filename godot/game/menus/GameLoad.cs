using Engine.Core;
using Engine.Input;
using Game.Compatibility;

namespace Game.Menus;

/// <summary>
/// The update a game is loaded on, as the cartridge spends one in black: the update's random number is drawn, then
/// the game is set up and play is made, its room with it. Play does not begin at once: what is made is laid under
/// an <see cref="Arrival"/>, which holds the black and then brings play up. It takes the place of the engine's
/// <see cref="LoadingMode"/> wherever this game starts play.
/// </summary>
public partial class GameLoad : GameMode
{
    private readonly Func<GameSession, GameMode> _makes;

    private GameLoad(Func<GameSession, GameMode> makes) => _makes = makes;

    /// <summary>
    /// The game in the session's save slot. <paramref name="appears"/> says Samus appears where she saved, as when
    /// the game is taken up from its map: she faces out of the screen while her suit comes to its colours
    /// (<see cref="AppearingState"/>), her fanfare plays, and the room's music behind it. A game saved at the
    /// arrival on Ceres does not. A game saved as she left Ceres is taken up with Ceres blowing up behind her, and
    /// she lands from there as she did then. With nothing in the slot a new game begins as this game's new games do.
    /// </summary>
    public static GameLoad OfSlot(bool appears) => new(session =>
    {
        if (session.LoadSlot() is not { } save)
            return session.Definition.CreateModeForNewGame();

        // Nothing of Ceres is set up for the game saved as Ceres was left.
        if (SavedLeavingCeres(session))
            return new Cinematics.CeresDestruction { LandingRoom = save.Room, LandingEntry = save.EntryPoint };
        CartridgeSounds.Of(session).SavedGameAppears = appears;
        return new Arrival(new PlayingMode(save.Room, save.EntryPoint), Arrival.OrdinaryWaits, samusAppears: appears);
    });

    /// <summary>
    /// Whether the game in <paramref name="session"/>'s save slot is the one the cartridge saved by itself as Ceres
    /// was left: it is saved for the place she lands, which is no place the map marks and not where a game begins.
    /// </summary>
    public static bool SavedLeavingCeres(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.Saves.Has(session.Slot) && session.Saves.Read(session.Slot).Room != session.Definition.FirstRoom && !SaveMapMode.Shows(session);
    }

    /// <summary>
    /// A new game, in the game's first room. Whatever came before did with Samus is forgotten: she is as she starts.
    /// The game is saved to the session's slot as it is made, so that it is taken up again at its arrival there.
    /// </summary>
    public static GameLoad OfNewGame() => new(session =>
    {
        session.BeginAgain();
        return new Arrival(new PlayingMode(), Arrival.OrdinaryWaits, savesFirstArrival: true);
    });

    /// <summary>
    /// Play going on in <paramref name="room"/>, entered at <paramref name="entryPoint"/>, with the game as it
    /// stands and Samus made anew: the landing on Zebes after Ceres, whose black lasts longer than any other load's.
    /// </summary>
    public static GameLoad OfLanding(string room, string entryPoint)
    {
        ArgumentException.ThrowIfNullOrEmpty(room);
        return new(session =>
        {
            // She is made anew for the landing, as the cartridge's is: only what she carries comes with her, with her
            // energy full, and whatever else Ceres left her (her speeds, a charge held, the safety of a hurt, what
            // she had selected) is gone. That starts the view's measure of her last move across again too.
            session.RenewPlayer();
            session.Player.Inventory.Fill(Engine.Actors.Inventory.Energy);
            return new Arrival(new PlayingMode(room, entryPoint), Arrival.LandingWaits);
        });
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        // The number is drawn before anything is set up, so what is set up reads this update's.
        Session.OpenUpdate();
        return _makes(Session);
    }
}
