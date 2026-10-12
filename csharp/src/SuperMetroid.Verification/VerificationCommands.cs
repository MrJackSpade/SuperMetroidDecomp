using System.Reflection;
using System.Text;

/// <summary>
/// Declares a verification command on the handler method that owns it (#621). The dispatcher discovers every
/// attributed handler, so adding one makes it dispatchable and listed in usage and help without editing a registry.
/// Several attributes on one handler declare aliases or intentional arity variants.
/// </summary>
/// <param name="name">The command-line name, such as <c>--crystal-flash</c>.</param>
/// <param name="arguments">
/// The arguments that follow the name, in order: a placeholder binds one handler parameter; an entry starting with
/// <c>--</c> is a literal keyword that must appear at that position, such as <c>--trace-from</c>.
/// </param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
internal sealed class VerificationCommandAttribute(string name, params string[] arguments) : Attribute
{
    /// <summary>The command-line name.</summary>
    public string Name { get; } = name;

    /// <summary>The placeholders and literal keywords that follow the name, in order.</summary>
    public IReadOnlyList<string> Arguments { get; } = arguments;

    /// <summary>A one-line description shown by <c>--help</c>.</summary>
    public string Description { get; init; } = "";
}

/// <summary>
/// The verification command registry: every <see cref="VerificationCommandAttribute"/> handler in this assembly,
/// validated before any dispatch. A handler is <c>static int</c> and takes either one <see cref="string"/> per
/// placeholder, or the complete argument array (<c>string[] args</c>, command name first) when one handler
/// serves several names.
/// </summary>
internal static class VerificationCommands
{
    /// <summary>One registered command: its name, argument shape, description and handler.</summary>
    internal sealed record Command(string Name, IReadOnlyList<string> Arguments, string Description, Type Owner, MethodInfo Handler)
    {
        /// <summary>The handler as <c>Owner.Method</c>, for diagnostics.</summary>
        public string HandlerName => $"{Owner.Name}.{Handler.Name}";

        /// <summary>Whether the handler receives the whole argument array.</summary>
        public bool TakesArgumentArray => Handler.GetParameters() is [{ ParameterType: var type }] && type == typeof(string[]);

        /// <summary>The placeholders that bind handler parameters, in order (literal keywords excluded).</summary>
        public IReadOnlyList<string> Placeholders => Arguments.Where(argument => !IsLiteral(argument)).ToList();

        /// <summary>The argument shape: literal keywords as written, each placeholder as one position.</summary>
        public string Shape => string.Join(' ', Arguments.Select(argument => IsLiteral(argument) ? argument : "_"));

        /// <summary>The usage line: name, literal keywords and placeholders.</summary>
        public string Usage => Arguments.Count == 0 ? Name
            : $"{Name} {string.Join(' ', Arguments.Select(argument => IsLiteral(argument) ? argument : $"<{argument}>"))}";

        /// <summary>Whether <paramref name="args"/> names this command with its arity and literal keywords.</summary>
        public bool Matches(string[] args) => args.Length == Arguments.Count + 1 && args[0] == Name &&
            Arguments.Select((argument, index) => !IsLiteral(argument) || args[index + 1] == argument).All(match => match);

        /// <summary>The handler's arguments: the whole array, or the values at the placeholder positions.</summary>
        public object?[] Bind(string[] args) => TakesArgumentArray ? [args]
            : Arguments.Select((argument, index) => (argument, value: args[index + 1])).Where(pair => !IsLiteral(pair.argument))
                .Select(pair => (object?)pair.value).ToArray();

        private static bool IsLiteral(string argument) => argument.StartsWith("--", StringComparison.Ordinal);
    }

    private static readonly Lazy<List<Command>> registry = new(Discover);

    /// <summary>Every registered command, ordered by name then shape.</summary>
    internal static IReadOnlyList<Command> All => registry.Value;

    /// <summary>
    /// Runs the command <paramref name="args"/> names. Returns false when no command matches; the handler's
    /// exceptions propagate unchanged, with their original stack traces.
    /// </summary>
    internal static bool TryDispatch(string[] args, out int exitCode)
    {
        exitCode = 0;
        if (args.Length == 0)
            return false;
        Command? command = All.SingleOrDefault(candidate => candidate.Matches(args));
        if (command is null)
            return false;
        object? result = command.Handler.Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, command.Bind(args), culture: null);
        exitCode = result is int code ? code : throw new InvalidOperationException($"{command.HandlerName} returned no exit code.");
        return true;
    }

    /// <summary>The unknown-command diagnostic: the rejected arguments, then every command's usage line.</summary>
    internal static string UsageFor(string[] args)
    {
        var text = new StringBuilder();
        text.AppendLine($"Unknown verification command: {string.Join(' ', args)}");
        AppendUsage(text, withDescriptions: false);
        return text.ToString();
    }

    /// <summary>Detailed help: every command's usage line, description and handler.</summary>
    [VerificationCommand("--help", Description = "Lists every verification command with its description and handler.")]
    internal static int HelpCommand()
    {
        var text = new StringBuilder();
        AppendUsage(text, withDescriptions: true);
        Console.Write(text.ToString());
        return 0;
    }

    private static void AppendUsage(StringBuilder text, bool withDescriptions)
    {
        text.AppendLine("Usage: SuperMetroid.Verification [<command>]");
        text.AppendLine("  (no command)  Runs every registered suite across worker processes.");
        foreach (Command command in All)
        {
            text.AppendLine("  " + command.Usage);
            if (withDescriptions)
            {
                if (command.Description.Length > 0)
                    text.AppendLine("      " + command.Description);
                text.AppendLine("      handler: " + command.HandlerName);
            }
        }
    }

    private static List<Command> Discover()
    {
        var commands = new List<Command>();
        foreach (Type type in typeof(VerificationCommands).Assembly.GetTypes())
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                foreach (VerificationCommandAttribute attribute in method.GetCustomAttributes<VerificationCommandAttribute>())
                    commands.Add(Validate(new Command(attribute.Name, attribute.Arguments, attribute.Description, type, method)));
            }
        }
        var duplicate = commands.GroupBy(command => (command.Name, command.Shape)).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                $"Verification command {duplicate.First().Usage} is registered by " +
                string.Join(" and ", duplicate.Select(command => command.HandlerName)) + ".");
        }
        return commands.OrderBy(command => command.Name, StringComparer.Ordinal).ThenBy(command => command.Shape, StringComparer.Ordinal).ToList();
    }

    private static Command Validate(Command command)
    {
        if (!command.Name.StartsWith("--", StringComparison.Ordinal))
            throw new InvalidOperationException($"{command.HandlerName}: command name {command.Name} must start with --.");
        if (command.Handler.ReturnType != typeof(int))
            throw new InvalidOperationException($"{command.HandlerName}: a verification command returns its int exit code.");
        ParameterInfo[] parameters = command.Handler.GetParameters();
        bool positional = parameters.Length == command.Placeholders.Count && parameters.All(parameter => parameter.ParameterType == typeof(string));
        if (!positional && !command.TakesArgumentArray)
        {
            throw new InvalidOperationException(
                $"{command.HandlerName}: takes {parameters.Length} parameter(s) but {command.Usage} declares {command.Placeholders.Count} placeholder(s); " +
                "use one string per placeholder or a single string[] args.");
        }
        return command;
    }
}
