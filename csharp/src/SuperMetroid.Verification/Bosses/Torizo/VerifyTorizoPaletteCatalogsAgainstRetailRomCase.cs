using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>The source, name and catalog of one verify torizo palette catalogs against retail rom case.</summary>
/// <param name="Source">The source.</param>
/// <param name="Name">The name.</param>
/// <param name="Catalog">The catalog.</param>
internal readonly record struct VerifyTorizoPaletteCatalogsAgainstRetailRomCase(int Source, string Name, RgbColor Catalog);
