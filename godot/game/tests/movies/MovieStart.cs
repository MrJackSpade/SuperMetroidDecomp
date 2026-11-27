using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.World;

namespace Game.Tests.Movies;

/// <summary>
/// Begins the game where a movie that starts from a snapshot begins: mid-game, from the cartridge's whole memory as
/// the movie's first update began. The start is made only through what the game has for starting somewhere: a saved
/// game, play's arrival in a room at a place, the random sequence restarted from a number, the view placed, a pause
/// asked for. Nothing is added to the game to be written here.
/// </summary>
/// <remarks>
/// <para>
/// The owner's rule for it (2026-11-27): "You should be able to set up the full state from the snapshots. If you
/// cant, then something is wrong. Every state is either required, and therefor an omission, or not required and
/// therefor not needed". What this cannot yet say through the game is listed by <see cref="NotCarried"/> and printed
/// as the movie begins; each is either carried here once the way is found, or an omission of the game's to fix.
/// </para>
/// </remarks>
public static class MovieStart
{
    /// <summary>What of the snapshot the start does not carry into the game yet.</summary>
    public static IReadOnlyList<string> NotCarried { get; } =
    [
        "progress: the events, the bosses beaten, the items collected, the doors opened and the map explored (the cartridge's bits at $D820-$D913 are not yet turned into the game's facts)",
        "the room's enemies as they stood: each one's place, health and what it was doing (the room is entered afresh)",
        "Samus's pose, her speeds, and her shots in flight (she arrives standing, at rest)",
        "the room's own state: blocks broken, gates, liquids, a boss's fight under way",
        "a pause's own state: which screen is up and where its cursor is",
    ];

    /// <summary>
    /// Brings <paramref name="session"/>, just begun, to where <paramref name="memory"/> has the cartridge:
    /// <paramref name="step"/> plays one update of the game with the input it is given.
    /// </summary>
    public static void FromSnapshot(GameSession session, byte[] memory, Action<GameInput> step)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(memory);
        ArgumentNullException.ThrowIfNull(step);
        int Word(int address) => memory[address] | (memory[address + 1] << 8);

        // What she carries, as a saved game the game loads as a player's.
        var save = new SaveData
        {
            Room = session.Definition.FirstRoom,
            EntryPoint = session.Definition.FirstEntryPoint,
            Toggles = new Dictionary<string, bool>(session.Options.Toggles),
            Language = session.Options.Language,
            Controls = new Dictionary<string, string>(session.Options.Controls),
        };
        foreach ((string name, bool owned, bool on) in MovieCheck.EquipmentOf(
            Word(Cartridge.EquipmentOn), Word(Cartridge.EquipmentOwned), Word(Cartridge.BeamsOn), Word(Cartridge.BeamsOwned)))
        {
            if (owned)
                save.Owned.Add(name);
            if (on)
                save.SwitchedOn.Add(name);
        }

        foreach ((int amount, int capacity, string name) in MovieCheck.AmountsKept)
        {
            save.Amounts[name] = Word(amount);
            save.Capacities[name] = Word(capacity);
        }

        session.Saves.Write(session.Slot, save);

        // Through the title and the choice of saved game, as a player gets to play: confirming until play has control.
        var confirm = new GameInput([GameAction.Confirm], [GameAction.Confirm]);
        for (long update = 0; session.Mode is not PlayingMode; update++)
            step(update % 20 == 0 ? confirm : GameInput.None);
        var playing = (PlayingMode)session.Mode;

        // Into the snapshot's room, at her place, facing as she faced.
        Facing facing = (memory[Cartridge.SamusFacing] & 0xff) == 4 ? Facing.Left : Facing.Right;
        var at = new PixelPosition(
            Pixels.FromParts(Word(Cartridge.SamusX), Word(Cartridge.SamusXPart)),
            Pixels.FromParts(Word(Cartridge.SamusY), Word(Cartridge.SamusYPart)));
        playing.ChangeRoom(CartridgeRooms.SceneOf(Word(Cartridge.Room)), new RoomEntry("", PixelVelocity.Zero, facing) { At = at });
        session.Camera.Place(Word(Cartridge.ViewX), Word(Cartridge.ViewY));

        // A movie that begins paused: the pause is asked for as a player asks, and played until its screen has control.
        if (Word(Cartridge.GameState) == Cartridge.Paused)
        {
            session.RequestPause();
            while (session.Mode is PlayingMode)
                step(GameInput.None);
        }
        else if (Word(Cartridge.GameState) != Cartridge.InPlay)
        {
            throw new NotSupportedException($"The snapshot is of the game state {Word(Cartridge.GameState):X2}, which a start is not yet made for.");
        }

        // Last, so that nothing of the way here has drawn from it.
        session.Random.Restart((uint)Word(Cartridge.Random));
    }
}
