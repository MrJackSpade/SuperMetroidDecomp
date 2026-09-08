namespace SuperMetroid.Android;

/// <summary>Host-owned Android document-picker result identities, unrelated to cartridge IDs.</summary>
internal static class AndroidDocumentRequests
{
    /// <summary>Result for choosing a destination for the private diagnostic ZIP export.</summary>
    public const int DiagnosticExport = 1;
    /// <summary>Result for selecting a private debugger state to replace the chosen slot.</summary>
    public const int StateImport = 2;
    /// <summary>Result for selecting a JSON regular save to activate on next launch.</summary>
    public const int SaveImport = 3;
}
