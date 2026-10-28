namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Compiled NTSC beam speeds and beam/missile acceleration definitions.</summary>
internal static class SamusProjectileMotionDefinitions
{
    /// <summary>$90:C2D1 BeamSpeeds_Horizontal_Vertical: all twelve authored rows use 4 px/frame.</summary>
    internal const ushort CardinalSpeed = 0x0400;
    /// <summary>$90:C2D3 BeamSpeeds_Diagonal: all twelve authored rows use 683/256 px/frame.</summary>
    internal const ushort DiagonalSpeed = 0x02ab;
    /// <summary>$90:C301 MissileInitializedBitset: adjacent word reached by invalid beam combinations.</summary>
    internal const ushort MissileInitialized = 0x0100;
}
