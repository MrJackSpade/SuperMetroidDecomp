namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Where a liquid's transfer is cut (#1706): its surface strip is rows 0 up to <paramref name="BodyFirstRow"/>, its body the <paramref name="BodyRows"/> rows from there.</summary>
/// <param name="BodyFirstRow">The first row of the body, the rows above it the surface strip.</param>
/// <param name="BodyRows">The rows of the body, which the transfer repeats down to its end.</param>
internal readonly partial record struct LiquidTransferRows(int BodyFirstRow, int BodyRows);
