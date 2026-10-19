namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="CreditsPresentation"/> members production does not use.</summary>
internal static class CreditsPresentationAccess
{
    /// <summary>Constructs a credits presenter from explicit compiled tilemap rows.</summary>
    extension(CreditsPresentation)
    {
        /// <summary>Copies nonempty 32-word rows into a verification-only presentation.</summary>
        internal static CreditsPresentation FromCompiledRowsForVerification(
            params ushort[][] rows)
        {
            if (rows.Length == 0 || rows.Any(row =>
                row.Length != CreditsPresentationDefinitions.TilemapWidth))
            {
                throw new ArgumentException(
                    "Verification credits rows must be nonempty 32-word rows.", nameof(rows));
            }
            return new CreditsPresentation(null, rows.Select(row => row.ToArray()).ToArray(), "VERIFICATION");
        }
    }
}
