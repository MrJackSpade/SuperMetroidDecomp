namespace SuperMetroid.Core.Game;

/// <summary>Bank-qualified decoding of the raw 16-bit callback words enemy headers and hitboxes store.</summary>
internal static class EnemyInteractionCallbacks
{
    /// <summary>Bank of the shared-prologue copies that value the common members.</summary>
    private const int CommonPrologueBank = 0xa0;

    /// <summary>
    /// Names the callback a raw word selects in <paramref name="bank"/>, or null for a routine
    /// the port does not name. A shared-prologue entry is the same routine in every enemy bank;
    /// any other word is a bank-local routine, so equal words in different banks differ.
    /// </summary>
    internal static EnemyInteractionCallback? TryDecode(byte bank, ushort pointer)
    {
        var common = (EnemyInteractionCallback)(((int)new SnesAddress(checked((byte)CommonPrologueBank), pointer)));
        if (common is EnemyInteractionCallback.NormalEnemyTouch or EnemyInteractionCallback.NormalEnemyShot or
            EnemyInteractionCallback.DudShot or EnemyInteractionCallback.NoOp or EnemyInteractionCallback.NoOpShortReturn)
            return common;
        int local = ((int)new SnesAddress(bank, pointer));
        return Enum.IsDefined((EnemyInteractionCallback)local) ? (EnemyInteractionCallback)local : null;
    }
}
