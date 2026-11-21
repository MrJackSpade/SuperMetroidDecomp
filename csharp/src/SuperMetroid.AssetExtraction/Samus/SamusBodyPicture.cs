namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// One of the half pictures the import composes Samus's whole body pictures from: the characters one half of the
/// cartridge's split body shows. The import names each picture once, when it decodes the body's tables (#2556), by the
/// half and the first pose frame that shows it; the body sheet holds only the whole pictures composed from them (#1585).
/// </summary>
/// <param name="Name">The picture's name, which also names its import-data image.</param>
public readonly record struct SamusBodyPicture(string Name);
