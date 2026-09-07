namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    private bool _gunshipExitSoundRequested;

    /// <summary>
    /// Consumes the $A2:AB58 entrance-pad sound queued when the suspended gunship message
    /// returns. Message NMIs do not publish the ordinary EnemyMain sound list.
    /// </summary>
    public bool ConsumeGunshipExitSoundRequest()
    {
        bool requested = _gunshipExitSoundRequested;
        _gunshipExitSoundRequested = false;
        return requested;
    }
}
