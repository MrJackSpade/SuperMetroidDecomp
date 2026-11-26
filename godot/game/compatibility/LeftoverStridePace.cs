namespace Game.Compatibility;

/// <summary>
/// The pace of a stride the cartridge never wrote. When the speed booster's echo is let in with as many sounds waiting
/// as the boost has stages, the cartridge picks a stage past its last, and the first picture of the stride that
/// follows lasts what it finds there: what Ridley's getaway from Ceres left as it last drew his wings, one length for
/// each picture of the stride, or nought before it ever has.
/// </summary>
/// <remarks>
/// The cartridge reads it from where the getaway lays out its wings' pictures, which nothing but power-on clears.
/// </remarks>
public sealed class LeftoverStridePace
{
    private int[]? _updates;

    /// <summary>Takes the lengths, one for each picture of the stride, that a beat of the wings leaves behind.</summary>
    public void TakeLeftByWings(IReadOnlyList<int> updates) => _updates = [.. updates];

    /// <summary>How many updates the stride's picture <paramref name="picture"/> lasts at this pace: nought while no wings have been drawn.</summary>
    public int Updates(int picture) => _updates is null ? 0 : _updates[picture];
}
