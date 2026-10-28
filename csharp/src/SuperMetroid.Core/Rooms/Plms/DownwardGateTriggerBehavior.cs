namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Exclusive shootable-block BTS identities used by downward gate shot blocks.</summary>
public enum DownwardGateTriggerBehavior
{
    /// <summary><c>$84:C610 Setup_LeftBlueGateTrigger</c>.</summary>
    BlueLeft = 0x46,
    /// <summary><c>$84:C627 Setup_RightBlueGateTrigger</c>.</summary>
    BlueRight = 0x47,
    /// <summary><c>$84:C58B Setup_LeftRedGateTrigger</c>.</summary>
    RedLeft = 0x48,
    /// <summary><c>$84:C5AF Setup_RightRedGateTrigger</c>.</summary>
    RedRight = 0x49,
    /// <summary><c>$84:C54D Setup_LeftGreenGateTrigger</c>.</summary>
    GreenLeft = 0x4a,
    /// <summary><c>$84:C56C Setup_RightGreenGateTrigger</c>.</summary>
    GreenRight = 0x4b,
    /// <summary><c>$84:C5D3 Setup_LeftYellowGateTrigger</c>.</summary>
    YellowLeft = 0x4c,
    /// <summary><c>$84:C5F1 RightYellowGateTrigger</c>.</summary>
    YellowRight = 0x4d,
}
