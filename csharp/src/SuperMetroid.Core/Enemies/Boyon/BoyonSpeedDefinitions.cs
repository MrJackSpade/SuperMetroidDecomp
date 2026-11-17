namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>How far down the Boyon bounce curve rises before it saturates, and the bounces its arcs allow.</summary>
public static class BoyonSpeedDefinitions
{
    /// <summary>How many places from the curve's top rise: from the twenty-fourth place down, the curve saturates.</summary>
    public const int StoredSampleCount = 23;

    /// <summary>The sample at every place from <see cref="StoredSampleCount"/> down: the curve's saturated height.</summary>
    public const int SaturatedSample = 255;

}
