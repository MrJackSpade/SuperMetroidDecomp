namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The way an Alcoon walks and faces, airborne and firing too.</summary>
public enum AlcoonWalk
{
    /// <summary>
    /// From the start of each jump until it picks the side Samus is on: a stride moves it nowhere, it faces right, and a
    /// wall it meets leaves it undecided, continuing at the left walk.
    /// </summary>
    Undecided,

    /// <summary>It walks and faces left.</summary>
    Left,

    /// <summary>It walks and faces right.</summary>
    Right,
}
