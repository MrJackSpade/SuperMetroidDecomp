namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>What a baby turtle starts from (#2554): the Mama that heads it, where she is, and the way it first crawls.</summary>
/// <param name="MamaPlace">Its Mama's place.</param>
/// <param name="Mama">Its Mama.</param>
/// <param name="Heading">The way it first crawls, as its Mama's placement names for it.</param>
internal sealed record BabyTurtleStart(RoomEnemyPlace MamaPlace, MamaTurtleEnemyState Mama, HorizontalDirection Heading);
