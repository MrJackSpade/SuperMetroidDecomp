namespace SuperMetroid.RenderVerification.Samus;

/// <summary>The red, green and blue levels of a power bomb explosion.</summary>
/// <param name="Red">The red.</param>
/// <param name="Green">The green.</param>
/// <param name="Blue">The blue.</param>
internal readonly record struct ExplosionColors(int Red, int Green, int Blue);
