namespace SuperMetroid.Diagnostics.Configuration;

/// <summary>
/// Marks a static partial string property the compiler implements as the text of one of the project's additional files
/// (#2602), so the assembly carries the file's text and nothing looks it up by name at run time.
/// </summary>
/// <param name="fileName">The additional file's name.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class GeneratedTextAttribute(string fileName) : Attribute
{
}
