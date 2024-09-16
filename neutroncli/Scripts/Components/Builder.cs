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

    public static async Task BuildAndMoveFrontendAsync(ProjectConfig projectConfig)
    {
        var buildTimestampFile = Path.Combine(projectConfig.FrontendName, "build.timestamp");

        if (!Directory.Exists(Path.Combine(projectConfig.FrontendName, "node_modules")))
        {
            Console.WriteLine("Running npm install");
            await Cli.Wrap("npm").WithArguments("install")
                                 .WithWorkingDirectory(projectConfig.FrontendName)
                                 .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                                 .ExecuteBufferedAsync();
        }

        bool shouldRunBuild = true;

        if (File.Exists(buildTimestampFile))
        {
            var lastBuildTime = File.GetLastWriteTime(buildTimestampFile);
            var sourceFiles = Directory.GetFiles(Path.Combine(projectConfig.FrontendName, "src"), "*.*", SearchOption.AllDirectories);

            shouldRunBuild = sourceFiles.Any(file => File.GetLastWriteTime(file) > lastBuildTime);
        }

        if (shouldRunBuild)
        {
            Console.WriteLine("Running npm run build");

            await Cli.Wrap("npm").WithArguments("run build")
                .WithWorkingDirectory(projectConfig.FrontendName)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                .ExecuteBufferedAsync();

            File.WriteAllText(buildTimestampFile, DateTime.Now.ToString());

            if (Directory.Exists(Path.Combine(projectConfig.BackendName, "dist")))
            {
                Console.WriteLine("Deleting old backend dist folder");
                Directory.Delete(Path.Combine(projectConfig.BackendName, "dist"), recursive: true);
            }

            Console.WriteLine("Copying dist folder");
            Directory.Move(Path.Combine(projectConfig.FrontendName, "dist"), Path.Combine(projectConfig.BackendName, "dist"));
        }
        else
        {
            Console.WriteLine("No changes detected, skipping npm run build.");
        }
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