namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>A shaking Mochtroid's displacement.</summary>
internal static class MochtroidShake
{
    /// <summary>The displacement the shake timer's <see cref="MetroidCadenceDefinitions.MochtroidShakeStep"/> selects.</summary>
    internal static Offset16<RoomPixels> Offset(int timer) =>
        MochtroidShakeDefinitions.Offsets[MetroidCadenceDefinitions.MochtroidShakeStep.Of(timer)];
}
