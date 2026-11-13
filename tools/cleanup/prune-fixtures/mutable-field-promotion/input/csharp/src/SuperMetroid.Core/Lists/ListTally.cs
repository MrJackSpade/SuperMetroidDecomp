namespace SuperMetroid.Core.Lists;

/// <summary>A running tally of a list's entries.</summary>
internal sealed class ListTally
{
    /// <summary>How many entries the tally has counted.</summary>
    private int counted;

    /// <summary>Counts one more entry.</summary>
    internal void Add() => counted++;

    /// <summary>The entries counted, which production reads.</summary>
    internal int Total => counted;

    /// <summary>Twice the entries counted, which only checks read.</summary>
    internal int Doubled => counted * 2;
}
