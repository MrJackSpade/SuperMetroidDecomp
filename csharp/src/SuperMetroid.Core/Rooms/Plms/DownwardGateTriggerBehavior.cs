namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Exclusive shootable-block behaviour identities used by downward gate shot blocks.</summary>
public enum DownwardGateTriggerBehavior
{
    /// <summary><c>$84:C610 Setup_LeftBlueGateTrigger</c>.</summary>
    BlueLeft = 70,
    /// <summary><c>$84:C627 Setup_RightBlueGateTrigger</c>.</summary>
    BlueRight = 71,
    /// <summary><c>$84:C58B Setup_LeftRedGateTrigger</c>.</summary>
    RedLeft = 72,
    /// <summary><c>$84:C5AF Setup_RightRedGateTrigger</c>.</summary>
    RedRight = 73,
    /// <summary><c>$84:C54D Setup_LeftGreenGateTrigger</c>.</summary>
    GreenLeft = 74,
    /// <summary><c>$84:C56C Setup_RightGreenGateTrigger</c>.</summary>
    GreenRight = 75,
    /// <summary><c>$84:C5D3 Setup_LeftYellowGateTrigger</c>.</summary>
    YellowLeft = 76,
    /// <summary><c>$84:C5F1 RightYellowGateTrigger</c>.</summary>
    YellowRight = 77,
}
