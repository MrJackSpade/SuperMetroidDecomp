namespace SuperMetroid.DesktopVerification.Window;

/// <summary>Result of updating one persistent GDI frame surface twice.</summary>
public readonly record struct RgbaBitmapSmokeTestResult(Size BitmapSize, Color FinalPixel);
