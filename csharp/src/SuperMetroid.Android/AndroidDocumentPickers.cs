using AndroidX.Activity.Result;

namespace SuperMetroid.Android;

/// <summary>The activity's registered document pickers, one for each request.</summary>
/// <param name="DiagnosticExport">Chooses where the private diagnostic ZIP is written.</param>
/// <param name="StateImport">Chooses a debugger state to import into the selected slot.</param>
/// <param name="SaveImport">Chooses a regular save to activate on next launch.</param>
/// <param name="RomImport">Chooses the user's cartridge during setup.</param>
internal sealed record AndroidDocumentPickers(
    ActivityResultLauncher DiagnosticExport,
    ActivityResultLauncher StateImport,
    ActivityResultLauncher SaveImport,
    ActivityResultLauncher RomImport)
{
    /// <summary>The picker that serves <paramref name="request"/>.</summary>
    internal ActivityResultLauncher For(AndroidDocumentRequest request) => request switch
    {
        AndroidDocumentRequest.DiagnosticExport => DiagnosticExport,
        AndroidDocumentRequest.StateImport => StateImport,
        AndroidDocumentRequest.SaveImport => SaveImport,
        AndroidDocumentRequest.RomImport => RomImport,
        _ => throw new InvalidOperationException($"Undefined {nameof(AndroidDocumentRequest)} {request}."),
    };
}
