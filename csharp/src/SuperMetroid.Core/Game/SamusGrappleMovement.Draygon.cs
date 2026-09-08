using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

public static partial class SamusGrappleMovement
{
    /// <summary>$9B:C6B2: held moving poses aim away from Draygon; Down wins over Up.</summary>
    private static byte ReadDraygonHeldDirection(byte pose, ushort input)
    {
        bool left = pose == SamusPoseIds.DraygonGrabbedMovingLeftPose;
        bool outward = (input & (ushort)(left ? SnesButton.Left : SnesButton.Right)) != 0;
        SamusProjectileDirection direction = left ? SamusProjectileDirection.Left : SamusProjectileDirection.Right;
        if (outward && (input & (ushort)SnesButton.Down) != 0)
            direction = left ? SamusProjectileDirection.DownLeft : SamusProjectileDirection.DownRight;
        else if (outward && (input & (ushort)SnesButton.Up) != 0)
            direction = left ? SamusProjectileDirection.UpLeft : SamusProjectileDirection.UpRight;
        return (byte)direction;
    }
}
