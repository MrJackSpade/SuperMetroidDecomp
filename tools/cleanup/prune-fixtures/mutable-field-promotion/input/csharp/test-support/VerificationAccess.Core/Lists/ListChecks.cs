using SuperMetroid.Core.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The tallies the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>Twice a tally of one entry.</summary>
    internal static int Doubled
    {
        get
        {
            ListTally tally = new();
            tally.Add();
            return tally.Doubled;
        }
    }
}
