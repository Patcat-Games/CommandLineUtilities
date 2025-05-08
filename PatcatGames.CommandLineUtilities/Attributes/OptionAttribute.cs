// -----------------------------------------------------------------------
// <copyright file="OptionAttribute.cs" company="Patcat Games LTD">
//     Patcat Command Line Utilities - A library for building CLI applications
//     Copyright © 2025 Patcat Games LTD. All rights reserved.
//     Originally created for PatcatCLI and PatcatDB projects.
//     Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------

namespace PatcatGames.CommandLineUtilities.Attributes;

/// <summary>
///     Marks a method parameter as an optional command line option.
///     Parameters marked with this attribute must have default values.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class OptionAttribute : Attribute
{
}