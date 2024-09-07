using CliWrap;
using CliWrap.Buffered;
using Microsoft.VisualBasic.FileIO;
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

        try
        {
            projectConfig = JsonSerializer.Deserialize<ProjectConfig>(File.ReadAllText("config.json"));
        }
        catch (Exception exception)
        {
            ConsoleError.NoNeutronProjectPresent();
            return null;
        }

        if (projectConfig is null)
        {
            ConsoleError.NoNeutronProjectPresent();
            return null;
        }

        return projectConfig;
    }

    public static async Task BuildFrontendAsync(ProjectConfig projectConfig)
    {
        if (!Directory.Exists(Path.Combine(projectConfig.FrontendName, "node_modules")))
        {
            Console.WriteLine("Running npm install");

            await Cli.Wrap("npm").WithArguments("install")
                     .WithWorkingDirectory(projectConfig.FrontendName)
                     .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                     .ExecuteBufferedAsync();
        }

        Console.WriteLine("Running npm run build");
        await Cli.Wrap("npm").WithArguments("run build")
                 .WithWorkingDirectory(projectConfig.FrontendName)
                 .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                 .ExecuteBufferedAsync();
    }

    public static void MoveDistToBackend(ProjectConfig projectConfig)
    {
        if (Directory.Exists(Path.Combine(projectConfig.BackendName, "dist")))
        {
            Console.WriteLine("Deleting old dist folder");
            Directory.Delete(Path.Combine(projectConfig.BackendName, "dist"), recursive: true);
        }

        Console.WriteLine("Moving dist folder");
        Directory.Move(Path.Combine(projectConfig.FrontendName, "dist"), Path.Combine(projectConfig.BackendName, "dist"));
    }

    public static void CopyAdditionalLibs(ProjectConfig projectConfig, string targetPlatform, string buildMode)
    {
        if (targetPlatform == "linux-x64")
        {
            string outputPath = Path.Combine(projectConfig.BackendName, "bin", buildMode, projectConfig.DotnetVersion, "linux-x64", "publish");

            if (File.Exists(Path.Combine(outputPath, "libs", "linux", "libwebkit2gtk-4.0.so")))
            {
                Console.WriteLine("Prevent copying libwebkit2gtk-4.0.so already exist");
                return;
            }

            if (Directory.Exists(Path.Combine(outputPath, "libs")))
            {
                Directory.Delete(Path.Combine(outputPath, "libs"), true);
            }

            Console.WriteLine($"Copying libwebkit2gtk-4.0.so to {Path.Combine(outputPath, "libs", "linux")}");

            FileSystem.CopyDirectory(Path.Combine(AppContext.BaseDirectory, "libs", "linux"), Path.Combine(outputPath, "libs", "linux"));
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

        CopyAdditionalLibs(projectConfig, targetPlatform, buildMode);
    }

    public static async Task RunBackendAsync(ProjectConfig projectConfig, string buildMode, string targetPlatform)
    {
        CopyAdditionalLibs(projectConfig, targetPlatform, buildMode);

        Console.WriteLine("Running dotnet run");
        await Cli.Wrap("dotnet").WithArguments("run")
                 .WithWorkingDirectory(projectConfig.BackendName)
                 .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                 .ExecuteBufferedAsync();
    }

}
