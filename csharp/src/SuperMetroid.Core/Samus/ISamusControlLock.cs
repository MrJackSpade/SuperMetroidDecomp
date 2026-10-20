namespace SuperMetroid.Core.Samus;

/// <summary>The stationary-script lock on Samus's controls.</summary>
public interface ISamusControlLock
{
    /// <summary>Gives Samus her controls back.</summary>
    void EnableControls();

    /// <summary>Locks Samus in place, taking her controls away.</summary>
    void DisableControls();
}
