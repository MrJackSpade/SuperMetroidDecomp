namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>The handling of the compiler's read-only lists of one element type.</summary>
internal interface ICompilerReadOnlyList
{
    /// <summary>Whether an array <paramref name="array"/> is a read-only list of this element type.</summary>
    bool HoldsElementsOf(object array);
}
