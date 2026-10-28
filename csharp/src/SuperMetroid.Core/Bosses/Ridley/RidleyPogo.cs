namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's pogo launches by speed pattern and bounce stage.</summary>
public static class RidleyPogo
{
    /// <summary>The launch of speed <paramref name="pattern"/> 0..3 at bounce <paramref name="stage"/> 0..5.</summary>
    public static RidleyPogoDefinition Read(int pattern, int stage)
    {
        if ((uint)pattern >= RidleyPogoDefinitions.Speeds.Length) throw new ArgumentOutOfRangeException(nameof(pattern));
        if ((uint)stage >= RidleyPogoDefinitions.Speeds[pattern].Length) throw new ArgumentOutOfRangeException(nameof(stage));
        return RidleyPogoDefinitions.Speeds[pattern][stage];
    }
}
