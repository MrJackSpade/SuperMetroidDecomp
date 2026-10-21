namespace SuperMetroid.Verification.Samus.Palettes;

/// <summary>The name, top, first list, first palette, phases and ping pong of one full body palette pointer lists family.</summary>
/// <param name="Name">The name.</param>
/// <param name="Top">The top.</param>
/// <param name="FirstList">The first list.</param>
/// <param name="FirstPalette">The first palette.</param>
/// <param name="Phases">The phases.</param>
/// <param name="PingPong">The ping pong.</param>
internal readonly record struct FullBodyPalettePointerListsFamily(string Name, int Top, ushort FirstList, ushort FirstPalette, int Phases, bool PingPong);
