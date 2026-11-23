namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The installed key of each <see cref="EndingTypedText"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class EndingTypedTextKeys
{
    /// <summary>Each EndingTypedText member's installed key.</summary>
    internal static InstalledKeys<EndingTypedText> Installed { get; } = new(new Dictionary<EndingTypedText, string>
    {
        [EndingTypedText.OperationWas] = "OperationWas",
        [EndingTypedText.CompletedSuccessfully] = "CompletedSuccessfully",
        [EndingTypedText.ClearTime] = "ClearTime",
    });
}
