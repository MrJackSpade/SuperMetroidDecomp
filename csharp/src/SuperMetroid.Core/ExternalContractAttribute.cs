namespace SuperMetroid.Core;

/// <summary>
/// Justifies, at its one site, a method whose signature belongs to a contract outside the repository (a framework
/// interface or an engine callback) that cannot be narrowed, so it may ignore or only forward its parameters
/// (SME6426, SME6427). The reason names the contract.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ExternalContractAttribute : Attribute
{
    /// <summary>Marks an external contract; the analyzers read the reason from the attribute argument.</summary>
    /// <param name="reason">The external contract whose shape the method fulfils.</param>
    public ExternalContractAttribute(string reason) => ArgumentException.ThrowIfNullOrWhiteSpace(reason);
}
