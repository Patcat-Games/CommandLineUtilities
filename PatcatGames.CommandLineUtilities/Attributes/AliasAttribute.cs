// -----------------------------------------------------------------------
// <copyright file="AliasAttribute.cs" company="Patcat Games LTD">
//     Patcat Command Line Utilities - A library for building CLI applications
//     Copyright © 2025 Patcat Games LTD. All rights reserved.
//     Originally created for PatcatCLI and PatcatDB projects.
//     Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------

namespace PatcatGames.CommandLineUtilities.Attributes;

/// <summary>
///     Defines one or more aliases (alternative names) for a command line option parameter.
/// </summary>
/// <remarks>
///     This attribute can be applied multiple times to a single parameter to create multiple aliases.
///     Aliases will automatically be prefixed with '-' if not provided.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class AliasAttribute : Attribute
{
    /// <summary>
    ///     Gets the alias string for the parameter.
    /// </summary>
    public readonly string Alias;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AliasAttribute" /> class.
    /// </summary>
    /// <param name="alias">The alias name for the parameter. Will be automatically prefixed with '-' if not provided.</param>
    public AliasAttribute(string alias)
    {
        if (!alias.StartsWith("-")) alias = '-' + alias;
        Alias = alias;
    }
}