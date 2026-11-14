namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's movement behaviour: how his body travels each update. Each member names that behaviour.</summary>
public enum BotwoonMovement
{
    /// <summary>Computes a clamped target vector and moves until the inside-hole transition is observed.</summary>
    MoveTowardHole,
    /// <summary>Takes speed-count path steps in the chosen direction until walking off the path's end marks completion.</summary>
    FollowAuthoredPath,
}
