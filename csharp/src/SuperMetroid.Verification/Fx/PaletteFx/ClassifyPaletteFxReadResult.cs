namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>The forbidden and presentation reads of one classify palette fx read result.</summary>
/// <param name="Forbidden">The forbidden.</param>
/// <param name="PresentationReads">The presentation reads.</param>
internal readonly record struct ClassifyPaletteFxReadResult(string? Forbidden, int PresentationReads);
