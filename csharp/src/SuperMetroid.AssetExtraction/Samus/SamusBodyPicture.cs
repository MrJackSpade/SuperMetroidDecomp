namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// One of the half pictures the import composes Samus's whole body pictures from: the characters one half of the
/// cartridge's split body shows. The import numbers the pictures once, in the order it stores them; the body sheet holds
/// only the whole pictures composed from them (#1585).
/// </summary>
/// <param name="Ordinal">The picture's place in the import's stored order.</param>
public readonly record struct SamusBodyPicture(int Ordinal);
