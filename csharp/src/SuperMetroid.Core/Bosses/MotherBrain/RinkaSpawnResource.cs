namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One literal (X, Y, extra-RAM selector) record from $A2:B75B.</summary>
/// <param name="Position">Authored spawn centre in whole room pixels (not an OAM or camera-relative coordinate), tested against the current layer-one camera when reserving a Mother Brain Rinka location.</param>
/// <param name="Token">The location's reservation token: twice its one-based position in the table, 2 through 22, held until the owning actor leaves, dies, or is terminated.</param>
public readonly record struct RinkaSpawnResource(
    Position16<RoomPixels> Position,
    int Token);
