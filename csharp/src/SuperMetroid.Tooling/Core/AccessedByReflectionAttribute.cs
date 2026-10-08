namespace SuperMetroid.Core;

/// <summary>
/// Marks a member or type that is invoked, read, written or instantiated through reflection by
/// name. Such a symbol can have no call site in source and still be required at run time.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property
    | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class AccessedByReflectionAttribute : Attribute;
