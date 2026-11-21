namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Whether Ridley's body moves and his wings and tail update (#2560): Ceres's eye fade starts them, Lower Norfair's death stops them.</summary>
public enum RidleyMotion
{
    /// <summary>His body holds still and his wings and tail do not update.</summary>
    Still,

    /// <summary>His body moves and his wings and tail update.</summary>
    Running,
}
