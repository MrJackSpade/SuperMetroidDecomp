namespace SuperMetroid.Core.Game;

/// <summary>Botwoon's movement callback, held in its variable E. Members start at one, as for <see cref="BotwoonEnemyFunction"/>.</summary>
public enum BotwoonMovementFunction : ushort
{
    /// <summary><c>$B3:9BB7 Function_Botwoon_Movement_DirectlyTowardTargetHole</c>: computes a clamped target vector and moves until the inside-hole transition is observed.</summary>
    MoveTowardHole = 1,
    /// <summary><c>$B3:E250 Function_Botwoon_Movement_StartMovingAccordingToMovementData</c>: loads the chosen descriptor's pointer, direction, and target hole, then follows its first samples on the same call.</summary>
    LoadAuthoredPath,
    /// <summary><c>$B3:E28C Function_Botwoon_Movement_MoveAccordingToMovementData</c>: consumes speed-count signed X/Y byte pairs in the selected direction until a $80 terminator marks completion.</summary>
    FollowAuthoredPath,
}
