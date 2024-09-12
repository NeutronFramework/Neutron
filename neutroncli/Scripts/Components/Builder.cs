using CliWrap;
using CliWrap.Buffered;
using NeutronCli.Scripts.DataStructures;
using System.Text.Json;

namespace neutroncli.Scripts.Components;

public static class Builder
{
    public static ProjectConfig? GetProjectConfig()
    {
        if (!File.Exists("config.json"))
        {
            ConsoleError.NoNeutronProjectPresent();
            return null;
        }

        ProjectConfig? projectConfig = null;

        projectConfig = JsonSerializer.Deserialize(File.ReadAllText("config.json"), SourceGenerationContext.Default.ProjectConfig);

        if (projectConfig is null)
        {
            ConsoleError.NoNeutronProjectPresent();
            return null;
        }

        return projectConfig;
    }

    public static async Task BuildBackendAsync(ProjectConfig projectConfig, string buildMode, string targetPlatform, bool selfContained)
    {
        string selfContainedStr = selfContained ? "--self-contained" : "";

        Console.WriteLine($"Running dotnet publish --configuration {buildMode} --runtime {targetPlatform} {selfContainedStr}".Trim());
        await Cli.Wrap("dotnet").WithArguments($"publish --configuration {buildMode} --runtime {targetPlatform} {selfContainedStr}".Trim())
                 .WithWorkingDirectory(projectConfig.BackendName)
                 .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                 .ExecuteBufferedAsync();

    }

    public static async Task RunBackendAsync(ProjectConfig projectConfig, string buildMode, string targetPlatform)
    {
        Console.WriteLine("Running dotnet run");

        await Cli.Wrap("dotnet").WithArguments("run")
                 .WithWorkingDirectory(projectConfig.BackendName)
                 .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                 .ExecuteBufferedAsync();
    }

}