namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>The class that receives an owner's relocated members, and the file it is written to.</summary>
/// <param name="Name">The class name, in the owner's namespace.</param>
/// <param name="Path">The full path of the file that declares the class: an existing class's own file, or a new one.</param>
internal sealed record RelocationHolder(string Name, string Path);
