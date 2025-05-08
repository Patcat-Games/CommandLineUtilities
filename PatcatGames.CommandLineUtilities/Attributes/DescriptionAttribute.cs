// -----------------------------------------------------------------------
// <copyright file="DescriptionAttribute.cs" company="Patcat Games LTD">
//     Patcat Command Line Utilities - A library for building CLI applications
//     Copyright © 2025 Patcat Games LTD. All rights reserved.
//     Originally created for PatcatCLI and PatcatDB projects.
//     Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------

namespace PatcatGames.CommandLineUtilities.Attributes;

/// <summary>
///     Provides a description for a command or parameter that appears in help documentation.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
public sealed class DescriptionAttribute : Attribute
{
    /// <summary>
    ///     Gets the description text.
    /// </summary>
    public readonly string Description;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DescriptionAttribute" /> class.
    /// </summary>
    /// <param name="description">The description text to display in help documentation.</param>
    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}