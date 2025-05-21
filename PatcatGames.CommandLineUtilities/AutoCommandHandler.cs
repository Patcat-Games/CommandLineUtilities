// -----------------------------------------------------------------------
// <copyright file="AutoCommandHandler.cs" company="Patcat Games LTD">
//     Patcat Command Line Utilities - A library for building CLI applications
//     Copyright © 2025 Patcat Games LTD. All rights reserved.
//     Originally created for PatcatCLI and PatcatDB projects.
//     Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PatcatGames.CommandLineUtilities;

/// <summary>
///     Handles command execution by mapping command line arguments to method parameters
///     and invoking the target method with the parsed values.
/// </summary>
internal sealed class AutoCommandHandler : ICommandHandler
{
    private readonly Dictionary<string, int> _bindings;
    private readonly Command _command;
    private readonly MethodInfo _methodInfo;
    private readonly object? _target;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoCommandHandler" /> class.
    /// </summary>
    /// <param name="command">The command being executed.</param>
    /// <param name="bindings">Mapping of parameter names to their positions.</param>
    /// <param name="methodInfo">The method to invoke when the command executes.</param>
    /// <param name="target">The target object instance for instance methods (null for static).</param>
    public AutoCommandHandler(Command command, Dictionary<string, int> bindings, MethodInfo methodInfo, object? target)
    {
        _command = command;
        _bindings = bindings;
        _methodInfo = methodInfo;
        _target = target;
    }

    /// <inheritdoc />
    [RequiresDynamicCode(
        "Sorry, AoT compilation is not supported yet due to the use of reflection to resolve method parameter names. AoT support through Roslyn is planned for the 2.0.0 release as it will introduce some breaking changes.")]
    public int Invoke(InvocationContext context)
    {
        return InvokeAsync(context).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    [RequiresDynamicCode(
        "Sorry, AoT compilation is not supported yet due to the use of reflection to resolve method parameter names. AoT support through Roslyn is planned for the 2.0.0 release as it will introduce some breaking changes.")]
    public async Task<int> InvokeAsync(InvocationContext context)
    {
        try
        {
            // Get the cancellation token from the context
            var cancellationToken = context.GetCancellationToken();

            // Prepare method parameters array
            var parameters = _methodInfo.GetParameters();
            var arguments = new object?[parameters.Length];

            // Process command arguments
            foreach (var commandArgument in _command.Arguments)
            {
                // Get value from parsed command line
                var value = context.ParseResult.GetValueForArgument(commandArgument);
                if (!_bindings.TryGetValue(commandArgument.Name, out var i)) continue;
                var parameter = parameters[i];

                // Handle default values if not provided
                if (value == null && parameter.HasDefaultValue)
                    value = parameter.DefaultValue;

                // Convert value to parameter type if needed
                if (value != null && value.GetType() != parameter.ParameterType)
                    value = Convert.ChangeType(value, parameter.ParameterType);

                arguments[i] = value;
            }

            // Process command options
            foreach (var commandOption in _command.Options)
            {
                // Get value from parsed command line
                var value = context.ParseResult.GetValueForOption(commandOption);
                if (!_bindings.TryGetValue(commandOption.Name, out var i)) continue;
                var parameter = parameters[i];

                // Handle default values if not provided
                if (value == null && parameter.HasDefaultValue)
                    value = parameter.DefaultValue;

                // Convert value to parameter type if needed
                if (value != null && value.GetType() != parameter.ParameterType)
                    value = Convert.ChangeType(value, parameter.ParameterType);

                arguments[i] = value;
            }

            // Invoke the target method with prepared arguments
            var result = _methodInfo.Invoke(_target, arguments);

            // Handle different return types
            return result switch
            {
                Task<int> task => await task,
                Task task => await task.ContinueWith(_ => 0, cancellationToken),
                int exitCode => exitCode,
                _ => 0
            };
        }
        catch (Exception ex)
        {
            // Handle exceptions and show error to user
            context.Console.Error.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
                context.Console.Error.WriteLine($"Details: {ex.InnerException.Message}");
            return 1;
        }
    }
}