namespace Game.Tests.Movies;

/// <summary>
/// How each kind of enemy the cartridge keeps in its list is held against the game: the oracle's side of a movie's
/// check, built up as the movies reach each kind. The cartridge names a kind by the address of its header.
/// </summary>
/// <remarks>
/// <para>
/// The game has no list of places as the cartridge has. Enemies are compared as sets of places: every entry of the
/// cartridge's list whose kind names a type of the game, against every living enemy of that type in the room.
/// </para>
/// <para>
/// Adding a kind: find what it is (<c>csharp/src/SuperMetroid.AssetExtraction/CartridgeTables/EnemyHeaderWords.cs</c>
/// names every header), find what plays it in the game, and say so here. A kind the game plays as one of its enemies
/// names that enemy's type. A kind the game plays as something that is not one of the room's enemies (a room object,
/// a part of a boss that the boss draws itself, a thing a cinematic owns) names no type and says what plays it; its
/// place is then not compared. A kind met in a movie and not listed here is reported when the movie ends, and the
/// movie has not passed while one is.
/// </para>
/// </remarks>
public static class CartridgeEnemies
{
    /// <summary>
    /// How one kind is compared: the name of the game's type that plays it (<c>nameof</c> the enemy's class), or none
    /// with <paramref name="PlayedBy"/> saying what plays it instead.
    /// </summary>
    public sealed record Kind(string? GameType, string PlayedBy = "");

    /// <summary>The kinds decided so far, by the address of the kind's header.</summary>
    public static IReadOnlyDictionary<int, Kind> Kinds { get; } = new Dictionary<int, Kind>
    {
        // Ceres's doors and walls: room objects, each door a Barrier (game/objects/ceres_door), the blocked way to the
        // elevator a CeresElevatorDoor (game/objects/ceres_elevator_door).
        [0xE23F] = new(null, "the room objects of Ceres's doors and walls"),

        // The Waver.
        [0xD63F] = new(nameof(Game.Enemies.Waver)),
    };
}
