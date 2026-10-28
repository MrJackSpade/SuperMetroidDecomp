namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>A shaking Mochtroid's displacement.</summary>
internal static class MochtroidShake
{
    /// <summary>The displacement timer bits 1..2 select.</summary>
    internal static Offset16<RoomPixels> Offset(int timer) => MochtroidShakeDefinitions.Offsets[(timer & 6) >> 1];
}
