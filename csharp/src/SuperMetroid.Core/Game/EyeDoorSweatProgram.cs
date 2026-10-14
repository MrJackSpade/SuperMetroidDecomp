namespace SuperMetroid.Core.Game;

/// <summary>The eye-door sweat drop's two typed programs (#1296).</summary>
internal enum EyeDoorSweatProgram
{
    /// <summary>The falling drop: one pose, held while the drop falls.</summary>
    Falling,
    /// <summary>The splash on the floor: three poses, then the drop is deleted.</summary>
    HitFloor,
}