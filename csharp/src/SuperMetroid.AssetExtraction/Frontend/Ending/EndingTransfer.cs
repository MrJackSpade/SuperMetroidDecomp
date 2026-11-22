namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// One of the ending's character and map transfers the import executes once to compose the pages its sprite sheets are
/// cut from (#1467); the game selects the composed sheets by scene state and keeps no transfer history. Each kind of
/// transfer carries exactly what it loads.
/// </summary>
internal abstract partial record EndingTransfer;
