namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Moves Phantoon along its figure-eight path, one authored unit step per movement cursor.</summary>
public static class PhantoonPath
{
    /// <summary>Reads the authored signed displacement at the native movement cursor.</summary>
    public static Offset16<RoomPixels> Step(int index)
    {
        if ((uint)index >= PhantoonPathDefinitions.Length) throw new ArgumentOutOfRangeException(nameof(index));
        return PhantoonPathDefinitions.Steps.TryGetValue(PhantoonPathDefinitions.Directions[index], out Offset16<RoomPixels> step)
            ? step
            : throw new InvalidOperationException("Invalid compiled Phantoon path direction.");
    }
}
