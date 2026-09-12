namespace SuperMetroid.Core.Game;

/// <summary>Mutable native movement records used by projectile velocity initialization.</summary>
internal static class SamusProjectileInheritanceAddresses
{
    /// <summary>$0DA8 CameraYSubSpeed; its high byte leaks into leftward launch velocity.</summary>
    internal const int CameraYSubspeed = 0x0da8;
    /// <summary>$0DAA ProjSpeed_DistanceSamusMovedLeft, followed by its fractional word.</summary>
    internal const int Left = 0x0daa;
    /// <summary>$0DAE ProjSpeed_DistanceSamusMovedRight, followed by its fractional word.</summary>
    internal const int Right = 0x0dae;
    /// <summary>$0DB2 ProjSpeed_DistanceSamusMovedUp, followed by its fractional word.</summary>
    internal const int Up = 0x0db2;
    /// <summary>$0DB6 ProjSpeed_DistanceSamusMovedDown, followed by its fractional word.</summary>
    internal const int Down = 0x0db6;
}
