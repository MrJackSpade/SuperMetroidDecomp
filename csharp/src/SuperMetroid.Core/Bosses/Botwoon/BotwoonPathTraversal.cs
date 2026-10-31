namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Which way Botwoon walks an authored path.</summary>
public enum BotwoonPathTraversal
{
    /// <summary>From the path's first step toward its last, taking each step as authored.</summary>
    Forward,
    /// <summary>From the path's end toward its first step, taking each step reversed.</summary>
    Backward,
}
