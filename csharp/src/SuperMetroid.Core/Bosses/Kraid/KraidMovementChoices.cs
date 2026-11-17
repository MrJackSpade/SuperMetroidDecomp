namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's second-phase walks: where he walks next from where he stands, and how long he then thinks.</summary>
public static class KraidMovementChoices
{
    /// <summary>The starting destination: where Kraid's grown body walks first, once he has reached his start.</summary>
    internal static int StartingX => KraidMovementDefinitions.StartingX;

    /// <summary>
    /// The walk Kraid takes from <paramref name="position"/> on the random sample <paramref name="random"/>: his
    /// destination's walk for the sample's choice (<see cref="KraidMovementDefinitions.Rows"/>), the starting destination's
    /// where he stands on none of them, and the last walk for every choice past the listed ones.
    /// </summary>
    public static KraidMovementChoice Select(int position, int random)
    {
        KraidMovementRow row = KraidMovementDefinitions.Rows.FirstOrDefault(destination => destination.X == position)
            ?? KraidMovementDefinitions.Starting;
        return row.Moves[Math.Min(KraidRandomDefinitions.MovementChoice.Of(random), row.Moves.Length - 1)];
    }
}
