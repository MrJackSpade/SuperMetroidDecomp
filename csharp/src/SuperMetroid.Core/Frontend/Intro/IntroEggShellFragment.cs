namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The six pieces the Metroid egg's shell breaks into when it hatches (#1768), by where each starts on the shell: each
/// has its own picture, start and horizontal launch speed.
/// </summary>
internal enum IntroEggShellFragment
{
    /// <summary>The piece at the upper left of the shell.</summary>
    UpperLeft,

    /// <summary>The piece at the upper right of the shell.</summary>
    UpperRight,

    /// <summary>The piece at the left of the shell.</summary>
    Left,

    /// <summary>The piece at the centre of the shell.</summary>
    Centre,

    /// <summary>The piece at the right of the shell.</summary>
    Right,

    /// <summary>The piece at the bottom of the shell.</summary>
    Lower,
}
