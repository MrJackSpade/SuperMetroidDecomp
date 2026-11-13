using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>
/// The shapes the checks compare: a run's <see cref="ListShapesConstants.extension(ListLayout[]).Weight"/>, the first of
/// <see cref="ListShapesConstants.extension{T}(T[]).Head"/>, and a shape's <see cref="ListShapesConstants.Shape{T}.Size"/>.
/// </summary>
internal static class ListChecks
{
    /// <summary>The checked layout.</summary>
    private static readonly ListLayout layout = new([4, 12]);

    /// <inheritdoc cref="ListShapesConstants.extension(ListLayout[]).Weight"/>
    internal static int Weight => new[] { layout }.Weight;

    /// <summary>The first of three items.</summary>
    /// <seealso cref="ListShapesConstants.extension{T}(T[]).Head"/>
    internal static int Head => new[] { 3, 4, 5 }.Head;

    /// <inheritdoc cref="SuperMetroid.AssetExtraction.Lists.ListShapesConstants.Shape{T}.Size"/>
    internal static int Size => new ListShapesConstants.Shape<int>().Size + new ListShapesConstants.Shape<int>().Kind.Length;
}
