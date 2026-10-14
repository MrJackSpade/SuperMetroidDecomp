namespace SuperMetroid.Android;

/// <summary>Host-owned Android document-picker request identities, unrelated to cartridge IDs.</summary>
internal enum AndroidDocumentRequest
{
    /// <summary>Result for choosing a destination for the private diagnostic ZIP export.</summary>
    DiagnosticExport = 1,
    /// <summary>Result for selecting a private debugger state to replace the chosen slot.</summary>
    StateImport = 2,
    /// <summary>Result for selecting a JSON regular save to activate on next launch.</summary>
    SaveImport = 3,
    /// <summary>Result for selecting the user's cartridge during first-launch setup.</summary>
    RomImport = 4,
}
