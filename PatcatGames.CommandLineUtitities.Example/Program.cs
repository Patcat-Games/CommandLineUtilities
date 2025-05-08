using System.CommandLine;
using PatcatGames.CommandLineUtilities;
using PatcatGames.CommandLineUtilities.Attributes;

internal sealed class Program
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