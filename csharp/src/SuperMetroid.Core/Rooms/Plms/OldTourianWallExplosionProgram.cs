namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A program of the old Tourian escape-shaft fake-wall explosion.</summary>
internal enum OldTourianWallExplosionProgram
{
    /// <summary>Stops the explosion's movement and loads the burst count ($86:B443).</summary>
    Start,

    /// <summary>Scatters, sounds and plays the six-pose burst, repeating while the count lasts ($86:B449).</summary>
    Burst,
}
