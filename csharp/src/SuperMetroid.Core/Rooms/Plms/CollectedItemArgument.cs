namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The collected-item bit a collectible sets when Samus picks it up.</summary>
/// <param name="ItemBit">The bit's index in the collected-items record.</param>
public sealed record CollectedItemArgument(int ItemBit) : PlmRoomArgument;
