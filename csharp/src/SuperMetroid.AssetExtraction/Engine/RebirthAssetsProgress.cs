namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// How far the making of the Godot game's assets has come (<see cref="RebirthAssets.Make"/>). The making has two
/// halves: the ROM is read, whose work is counted in pieces as its loops do them, and then the assets are written,
/// in a fixed number of parts and file by file.
/// </summary>
/// <param name="Doing">What is being done, in words to show.</param>
/// <param name="Read">How many pieces of the reading are done.</param>
/// <param name="ToRead">How many pieces of the reading are known so far; it grows as the reading finds its work.</param>
/// <param name="Part">How many parts of the writing have started; none while the ROM is still being read.</param>
/// <param name="Parts">How many parts the writing has.</param>
/// <param name="Files">How many files have been written.</param>
public readonly partial record struct RebirthAssetsProgress(string Doing, int Read, int ToRead, int Part, int Parts, int Files);
