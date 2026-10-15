namespace SuperMetroid.Core.Samus;

/// <summary>The stationary-script lock on Samus's controls.</summary>
public interface ISamusControlLock
{
    /// <summary>Enables or disables Samus's controls.</summary>
    void SetControlsEnabled(bool enabled);
}
