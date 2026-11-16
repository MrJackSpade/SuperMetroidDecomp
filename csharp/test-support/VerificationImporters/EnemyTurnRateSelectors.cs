using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Turns a population entry's turn-rate selector into the turning speed it selects, and back (#2042), for
/// <see cref="EnemySpawnSettingsCodec"/>. A Nuclear Waffle's sweep speed selects a row of the shared linear speed
/// table, whose pixels an update the game's sweep reads as steps of its angle an update; that reading is the import's
/// knowledge, so the species holds a turning speed.
/// </summary>
internal static class EnemyTurnRateSelectors
{
    /// <summary>The turning speed linear-speed row <paramref name="row"/> gives, its pixels read as steps.</summary>
    /// <exception cref="InvalidDataException"><paramref name="row"/> lies past the shared table.</exception>
    public static AngularSpeed LinearTurnRate(string setting, int row) =>
        AsTurnRate(setting, EnemySpawnSelectors.LinearSpeed(setting, row));

    /// <summary>The linear-speed row whose speed, its pixels read as steps, is <paramref name="speed"/>.</summary>
    /// <exception cref="ArgumentException">No row turns at the speed.</exception>
    public static int LinearTurnRateRow(string setting, AngularSpeed speed) =>
        EnemySpawnSelectors.LinearSpeedRow(setting, SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(speed.FractionsOfStep));

    /// <summary>
    /// <paramref name="speed"/>'s pixels an update read as steps an update. Every row of the shared table is whole
    /// sixteenths of a pixel, so the reading is exact; a speed finer than a 256th is rejected rather than rounded.
    /// </summary>
    /// <exception cref="InvalidDataException">The speed is finer than a 256th of a pixel.</exception>
    private static AngularSpeed AsTurnRate(string setting, SignedSixteenSixteen speed)
    {
        AngularSpeed turnRate = AngularSpeed.FromFractionsOfStep(speed.InTwoFiftySixths);
        return SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(turnRate.FractionsOfStep) == speed
            ? turnRate
            : throw new InvalidDataException($"Population words select {setting} {speed.RawValue}/65536, finer than a turning speed holds.");
    }
}
