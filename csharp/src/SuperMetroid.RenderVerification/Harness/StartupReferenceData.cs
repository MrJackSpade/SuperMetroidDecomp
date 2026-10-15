namespace SuperMetroid.RenderVerification.Harness;

/// <summary>Independent Snes9x pixel digests used by the startup renderer checks.</summary>
internal static class StartupReferenceData
{
    /// <summary>SHA-256 of row-major RGBA8 decoded from issue-321-snes9x-startup/1994.png (256x224), not PNG file bytes.</summary>
    internal const string YearRgbaSha256 = "6F57F4B36C71E9112DE91312B6EDAAD0004D676C4C38B9EB61F0D399C88771A8";
    /// <summary>SHA-256 of row-major RGBA8 decoded from issue-321-snes9x-title-pan/title-pan.png (256x224).</summary>
    internal const string PanRgbaSha256 = "D3B61C24CF7D6D4FECC0E44CBC29D46A92CD9606EA4787A191151E6EAAAE045C";
}
