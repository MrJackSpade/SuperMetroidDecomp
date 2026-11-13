namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's movement behaviour: how his body travels each update. Members start at one, as for <see cref="BotwoonPhase"/>.</summary>
public enum BotwoonMovement
{
    /// <summary>Computes a clamped target vector and moves until the inside-hole transition is observed.</summary>
    MoveTowardHole = 1,
    /// <summary>Loads the chosen path, the way it is walked and the target hole, then takes its first steps on the same call.</summary>
    LoadAuthoredPath,
    /// <summary>Takes speed-count path steps in the chosen direction until walking off the path's end marks completion.</summary>
    FollowAuthoredPath,
}
