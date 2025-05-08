// -----------------------------------------------------------------------
// <copyright file="PrettyNameAttribute.cs" company="Patcat Games LTD">
//     Patcat Command Line Utilities - A library for building CLI applications
//     Copyright © 2025 Patcat Games LTD. All rights reserved.
//     Originally created for PatcatCLI and PatcatDB projects.
//     Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------

namespace PatcatGames.CommandLineUtilities.Attributes;

/// <summary>
///     Provides a human-readable display name for a parameter in help documentation.
///     This does not affect the actual parameter name used in command line parsing.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class PrettyNameAttribute : Attribute
{
    /// <summary>
    ///     Gets the human-readable display name.
    /// </summary>
    public readonly string PrettyName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PrettyNameAttribute" /> class.
    /// </summary>
    /// <param name="prettyName">The display name to use in help documentation.</param>
    public PrettyNameAttribute(string prettyName)
    {
        PrettyName = prettyName;
    }
}