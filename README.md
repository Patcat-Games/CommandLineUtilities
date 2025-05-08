[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/PatcatGames.CommandLineUtilities)](https://www.nuget.org/packages/PatcatGames.CommandLineUtilities)

# Patcat Command Line Utilities

Extensions and Attributes for `System.CommandLine` for ease of use, originally created for PatcatCLI & PatcatDB.

## ⚠️ Limitations

- This library does **NOT** support AoT compilation due to its use of Reflection (future support planned)
- This library does not support array options yet

## Features

### Extension Methods

#### `AddCommandFromMethod`

```csharp
var rootCommand = new RootCommand(description: @"This is the root level command description.");
rootCommand.AddCommandFromMethod(typeof(Program).GetMethod(nameof(MyCommand)));
```

#### `ConfigureFromMethod`

```csharp
var rootCommand = new RootCommand(description: @"This is the root level command description.");
var myCommand = new Command("my-command");
myCommand.ConfigureFromMethod(typeof(Program).GetMethod(nameof(MyCommand)));
rootCommand.AddCommand(myCommand);
```

### Attributes

| Attribute               | Description                                                                                                           |
|-------------------------|-----------------------------------------------------------------------------------------------------------------------|
| `[Alias(string)]`       | Defines alternate (usually shorter) names for options. Multiple aliases allowed. Only works on `[Option]` parameters. |
| `[Description(string)]` | Sets description text for methods and parameters in CLI help.                                                         |
| `[Option]`              | Marks a parameter as optional (must have default value).                                                              |
| `[PrettyName(string)]`  | Provides human-readable names for parameters in help pages (doesn't affect functionality).                            |

## Complete Example

```csharp
using System.CommandLine;
using Patcat.CommandLineUtils;

class Program
{
    static Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand(description: @"This is the root level command description.");
        rootCommand.AddCommandFromMethod(typeof(Program).GetMethod(nameof(MyCommand)));
        return rootCommand.InvokeAsync(args);
    }
    
    [Description("This is the description of my-command")]
    public static void MyCommand(
        [Description("The first argument.")] string firstArgument,
        [Description("The second argument, but this one is optional.")] string secondArgument = "default!",
        [Option][Alias("-o1")][PrettyName("Option 1 Pretty Name")] string optionOne = "1",
        [Option][Alias("-o2")][Alias("-2")][PrettyName("Option 2 Pretty Name")] string optionTwo = "2",
        [Option][Alias("-y")][Description("Skips confirmations.")] bool skipConfirmations = false
    )
    {
        Console.WriteLine($"firstArgument = {firstArgument}");
        Console.WriteLine($"secondArgument = {secondArgument}");
        Console.WriteLine($"optionOne = {optionOne}");
        Console.WriteLine($"optionTwo = {optionTwo}");
        Console.WriteLine($"skipConfirmations = {skipConfirmations}");
    }
}
```

## Help Output Examples

**Root command help:**

```
Description:
  This is the root level command description.

Usage:
  PatcatDB.CLI [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  my-command <FirstArgument> <SecondArgument>  This is the description of my-command [default: default!]
```

**Command-specific help:**

```
Description:
  Publishes a PatcatDB module to a database.

Usage:
  PatcatDB.CLI publish <FirstArgument> [<SecondArgument>] [options]

Arguments:
  <FirstArgument>  The first argument.
  <SecondArgument>  The second argument, but this one is optional. [default: default!]

Options:
  --optionOne, -o1 <Option 1 Pretty Name>  [default: 1]
  --optionTwo, -o2, -2 <Option 2>  [default: 2]
  -y, --skip-confirmations         Skips confirmations. [default: False]
  -?, -h, --help                   Show help and usage information
```

## Setting Up as a .NET Tool

1. Add to your `.csproj` file:

```xml
<PackAsTool>true</PackAsTool>
<ToolCommandName>command-name</ToolCommandName>
<PackageOutputPath>./nupkg</PackageOutputPath>
```

Example project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <PackAsTool>true</PackAsTool>
        <ToolCommandName>command-name</ToolCommandName>
        <PackageOutputPath>./nupkg</PackageOutputPath>
        <Version>1.0.0</Version>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="System.CommandLine" Version="2.*" />
        <ProjectReference Include="PatcatGames.CommandLineUtilities" Version="1.*" />
    </ItemGroup>
</Project>
```

2. Pack and install:

```shell
dotnet pack
dotnet tool install --global --add-source ./nupkg ProjectName
```

## License

MIT License

Copyright (c) 2025 Patcat Games LTD

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.