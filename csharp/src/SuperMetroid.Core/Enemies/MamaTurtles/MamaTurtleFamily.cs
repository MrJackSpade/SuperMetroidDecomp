namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// A Mama Turtle's family (#1816, #2554): her place, the state she holds from her setup and the places of the four babies
/// her room lists right after her, which she found as she was placed and offers each baby as it is built.
/// </summary>
/// <param name="Mama">Her place, whose position her babies' crawl over her shell reads.</param>
/// <param name="State">Her state, which her babies wake and read as they crawl.</param>
/// <param name="Babies">Her babies' places, in the order her room lists them.</param>
internal sealed record MamaTurtleFamily(RoomEnemyPlace Mama, MamaTurtleEnemyState State, IReadOnlyList<RoomEnemyPlace> Babies);
