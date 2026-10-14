namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>DXGI device-lifetime failures and bounded recreation policy, not generic error suppression.</summary>
internal static class D3D11RecoveryPolicy
{
    /// <summary>Fail loudly after repeated recreation without any successful submission.</summary>
    internal const int MaximumConsecutiveRecreations = 3;
    /// <summary>Recognizes the two DXGI results for which renderer device recreation is appropriate.</summary>
    internal static bool IsDeviceLoss(int result) => Enum.IsDefined((DxgiDeviceLoss)result);
}
