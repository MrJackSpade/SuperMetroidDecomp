namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>The shaking Mochtroid's four displacements, stepped through by its shake timer.</summary>
internal static class MochtroidShakeDefinitions
{
    /// <summary>How far a shaking Mochtroid moves each update, two pixels; the timer chooses the axis and the way.</summary>
    internal const int Amplitude = 2;

    /// <summary>
    /// The shake's displacements in the order <see cref="MetroidCadenceDefinitions.MochtroidShakeStep"/> steps through
    /// them: right, up, left and down.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] Offsets =
        [new(Amplitude, 0), new(0, -Amplitude), new(-Amplitude, 0), new(0, Amplitude)];
}
