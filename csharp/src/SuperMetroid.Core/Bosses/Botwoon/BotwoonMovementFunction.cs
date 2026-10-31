namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's movement callback, held in its variable E. Members start at one, as for <see cref="BotwoonEnemyFunction"/>.</summary>
public enum BotwoonMovementFunction
{
    /// <summary><c>$B3:9BB7 Function_Botwoon_Movement_DirectlyTowardTargetHole</c>: computes a clamped target vector and moves until the inside-hole transition is observed.</summary>
    MoveTowardHole = 1,
    /// <summary><c>$B3:E250 Function_Botwoon_Movement_StartMovingAccordingToMovementData</c>: loads the chosen path, the way it is walked and the target hole, then takes its first steps on the same call.</summary>
    LoadAuthoredPath,
    /// <summary><c>$B3:E28C Function_Botwoon_Movement_MoveAccordingToMovementData</c>: takes speed-count path steps in the chosen direction until walking off the path's end marks completion.</summary>
    FollowAuthoredPath,
}
