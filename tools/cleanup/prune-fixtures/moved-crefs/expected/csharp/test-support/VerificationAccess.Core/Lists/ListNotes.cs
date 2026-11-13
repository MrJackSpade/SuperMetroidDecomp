namespace SuperMetroid.Verification.Lists;

/// <summary>Notes on the checked values, written without the lists' namespace in scope.</summary>
internal static class ListNotes
{
    /// <summary>The seed of four lists, as <see cref="SuperMetroid.AssetExtraction.Lists.ListShapesConstants.Seed(int)"/> computes it.</summary>
    internal static int Seed => SuperMetroid.AssetExtraction.Lists.ListShapesConstants.Seed(4);

    /// <summary>
    /// The checked weight, as
    /// <see cref="SuperMetroid.AssetExtraction.Lists.ListShapesConstants.extension(SuperMetroid.AssetExtraction.Lists.ListLayout[]).Weight"/>
    /// computes it.
    /// </summary>
    internal static int Weight => ListChecks.Weight;
}
