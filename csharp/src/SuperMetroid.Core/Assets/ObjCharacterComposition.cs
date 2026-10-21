namespace SuperMetroid.Core.Assets;

/// <summary>The uploads and source bytes a composed object character page was built from.</summary>
/// <param name="Uploads">The uploads.</param>
/// <param name="Sources">The sources.</param>
public readonly record struct ObjCharacterComposition(ObjCharacterUpload[] Uploads, ReadOnlyMemory<byte>[] Sources);
