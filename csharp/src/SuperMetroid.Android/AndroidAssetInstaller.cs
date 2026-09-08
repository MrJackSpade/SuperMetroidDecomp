using Android.Content.Res;

namespace SuperMetroid.Android;

/// <summary>
/// Copies packaged immutable assets into private filesystem storage for existing Core
/// readers. Saves/configuration are intentionally not packaged or overwritten here.
/// </summary>
internal static class AndroidAssetInstaller
{
    public static void Install(AssetManager assets, string source, string destination)
    {
        string[] children = assets.List(source) ?? throw new IOException($"Cannot list packaged asset {source}.");
        if (children.Length != 0)
        {
            Directory.CreateDirectory(destination);
            foreach (string child in children)
                Install(assets, source + "/" + child, Path.Combine(destination, child));
            return;
        }

        using Stream input = assets.Open(source);
        string temporary = destination + ".installing";
        using (var output = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            input.CopyTo(output);
            output.Flush(flushToDisk: true);
        }
        File.Move(temporary, destination, overwrite: true);
    }
}
