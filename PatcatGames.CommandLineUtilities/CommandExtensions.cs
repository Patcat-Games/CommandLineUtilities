// -----------------------------------------------------------------------
// <copyright file="CommandExtensions.cs" company="Patcat Games LTD">
//     Patcat Command Line Utilities - A library for building CLI applications
//     Copyright © 2025 Patcat Games LTD. All rights reserved.
//     Originally created for PatcatCLI and PatcatDB projects.
//     Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using PatcatGames.CommandLineUtilities.Attributes;

namespace PatcatGames.CommandLineUtilities;

/// <summary>
///     Provides extension methods for <see cref="Command" /> to configure commands from reflected methods.
/// </summary>
public static class CommandExtensions
{
    /// <summary>
    ///     Creates and adds a subcommand configured from the specified method.
    /// </summary>
    /// <param name="command">The parent command to add the subcommand to.</param>
    /// <param name="methodInfo">The method to use for command configuration.</param>
    /// <param name="target">The target object instance for instance methods (null for static).</param>
    /// <param name="treatUnmatchedTokensAsErrors">Whether unmatched tokens should be treated as errors.</param>
    /// <returns>The newly created subcommand.</returns>
    /// <exception cref="ArgumentNullException">Thrown if methodInfo is null.</exception>
    [RequiresDynamicCode(
        "Sorry, AoT compilation is not supported yet due to the use of reflection to resolve method parameter names. AoT support through Roslyn is planned for the 2.0.0 release as it will introduce some breaking changes.")]
    public static Command AddCommandFromMethod(this Command command, MethodInfo? methodInfo, object? target = null,
        bool treatUnmatchedTokensAsErrors = true)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        // Create subcommand with dashed naming convention
        var subCommand = new Command(AddDashesToName(methodInfo.Name));
        subCommand.TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;

        // Configure the command from the method
        subCommand.ConfigureFromMethod(methodInfo, target);
        command.Add(subCommand);
        return subCommand;
    }

    /// <summary>
    ///     Configures a command from the specified method, setting up arguments, options, and handler.
    /// </summary>
    /// <param name="command">The command to configure.</param>
    /// <param name="methodInfo">The method to use for configuration.</param>
    /// <param name="target">The target object instance for instance methods (null for static).</param>
    /// <exception cref="ArgumentNullException">Thrown if methodInfo is null.</exception>
    [RequiresDynamicCode(
        "Sorry, AoT compilation is not supported yet due to the use of reflection to resolve method parameter names. AoT support through Roslyn is planned for the 2.0.0 release as it will introduce some breaking changes.")]
    public static void ConfigureFromMethod(this Command command, MethodInfo? methodInfo, object? target = null)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        // Set command description from attribute if present
        var descriptionAttr = methodInfo.GetCustomAttribute<DescriptionAttribute>();
        if (descriptionAttr != null)
            command.Description = descriptionAttr.Description;

        var parameters = methodInfo.GetParameters();
        var bindings = new Dictionary<string, int>();

        foreach (var parameterInfo in parameters)
        {
            // Generate pretty names and aliases
            var prettyPrintedName = AddSpacesToName(parameterInfo.Name!);
            var aliases = new HashSet<string>();
            aliases.Add("--" + AddDashesToName(parameterInfo.Name!));

            // Add any aliases from attributes
            foreach (var aliasAttribute in parameterInfo.GetCustomAttributes<AliasAttribute>())
                aliases.Add(aliasAttribute.Alias);

            var description = parameterInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var isOption = parameterInfo.GetCustomAttribute<OptionAttribute>() != null;

            if (isOption)
            {
                // Create option
                var option = (Option)InstantiateGenericConstructor(
                    typeof(Option<>),
                    parameterInfo.ParameterType,
                    [typeof(string[]), typeof(string)],
                    aliases.ToArray(),
                    description);

                if (parameterInfo.HasDefaultValue)
                    option.SetDefaultValue(parameterInfo.DefaultValue);

                option.IsHidden = false;

                // Special handling for boolean flags
                if (parameterInfo.ParameterType == typeof(bool) && parameterInfo.HasDefaultValue == false)
                    option.Arity = ArgumentArity.Zero;
                else
                    option.Arity = ArgumentArity.ZeroOrOne;

                option.ArgumentHelpName = prettyPrintedName;
                option.Description = description;
                command.AddOption(option);
                bindings.Add(option.Name, parameterInfo.Position);
            }
            else
            {
                // Create argument
                var argument = (Argument)InstantiateGenericConstructor(
                    typeof(Argument<>),
                    parameterInfo.ParameterType,
                    [typeof(string), typeof(string)],
                    prettyPrintedName,
                    description);

                if (parameterInfo.HasDefaultValue)
                    argument.SetDefaultValue(parameterInfo.DefaultValue);

                command.AddArgument(argument);
                bindings.Add(argument.Name, parameterInfo.Position);
            }
        }

        // Set the handler to invoke the method
        command.Handler = new AutoCommandHandler(command, bindings, methodInfo, target);
    }

    /// <summary>
    ///     Instantiates a generic type with the specified type argument and constructor parameters.
    /// </summary>
    /// <param name="genericType">The open generic type to construct.</param>
    /// <param name="typeArgument">The type argument for the generic type.</param>
    /// <param name="argTypes">The constructor parameter types.</param>
    /// <param name="constructorArgs">The constructor arguments.</param>
    /// <returns>An instance of the constructed type.</returns>
    /// <exception cref="ArgumentException">Thrown if genericType is not generic.</exception>
    /// <exception cref="MissingMethodException">Thrown if no matching constructor is found.</exception>
    [RequiresDynamicCode(
        "Sorry, AoT compilation is not supported yet due to the use of reflection to resolve method parameter names. AoT support through Roslyn is planned for the 2.0.0 release as it will introduce some breaking changes.")]
    private static object InstantiateGenericConstructor(
        Type genericType,
        Type typeArgument,
        Type[] argTypes,
        params object?[] constructorArgs)
    {
        if (!genericType.IsGenericType)
            throw new ArgumentException($"Type {genericType.Name} is not a generic type");

        // Construct the closed generic type
        var constructedType = genericType.MakeGenericType(typeArgument);
        var constructor = constructedType.GetConstructor(argTypes);

        if (constructor == null)
            throw new MissingMethodException($"No matching constructor found for {constructedType.Name}");

        return constructor.Invoke(constructorArgs);
    }

    /// <summary>
    ///     Converts camelCase or PascalCase to space-separated words.
    /// </summary>
    /// <example>
    ///     "someCamelCaseName" becomes "Some Camel Case Name"
    ///     "someAPI" becomes "Some API"
    /// </example>
    /// <param name="name">The name to convert.</param>
    /// <returns>The converted string with spaces.</returns>
    private static string AddSpacesToName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var builder = new StringBuilder();
        builder.Append(char.ToUpper(name[0])); // Capitalize first letter

        for (var i = 1; i < name.Length; i++)
            if (char.IsUpper(name[i]))
            {
                // Check if we're in the middle of an acronym
                var isMiddleOfAcronym = i < name.Length - 1 && char.IsUpper(name[i + 1]) &&
                                        i > 0 && char.IsUpper(name[i - 1]);

                if (!isMiddleOfAcronym)
                    builder.Append(' ');
                builder.Append(name[i]);
            }
            else
            {
                builder.Append(name[i]);
            }

        return builder.ToString();
    }

    /// <summary>
    ///     Converts camelCase or PascalCase to kebab-case.
    /// </summary>
    /// <example>
    ///     "someCamelCaseName" becomes "some-camel-case-name"
    ///     "someAPI" becomes "some-api"
    /// </example>
    /// <param name="name">The name to convert.</param>
    /// <returns>The converted string with dashes.</returns>
    private static string AddDashesToName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var builder = new StringBuilder();
        builder.Append(char.ToLower(name[0]));

        for (var i = 1; i < name.Length; i++)
            if (char.IsUpper(name[i]))
            {
                // Check if we're at the start of an acronym (multiple uppercase letters)
                var isAcronym = (i < name.Length - 1 && char.IsUpper(name[i + 1])) ||
                                (i > 1 && char.IsUpper(name[i - 1]));

                if (!isAcronym)
                    builder.Append('-');
                builder.Append(char.ToLower(name[i]));
            }
            else
            {
                builder.Append(name[i]);
            }

        return builder.ToString();
    }
}