using CliWrap;
using CliWrap.Buffered;
using neutroncli.Scripts.Components;
using neutroncli.Scripts.DataStructures;
using NeutronCli.Scripts.Components;
using NeutronCli.Scripts.DataStructures;
using NeutronCli.Scripts.Extensions;
using System.CommandLine;
using System.Text.RegularExpressions;
using Command = System.CommandLine.Command;

namespace NeutronCli.Scripts;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        var projectName = new Option<string?>(name: "--name",description: "The name of the neutron project");
        var dotnetVersion = new Option<DotnetVersion?>(name: "--dotnet-version", description: "The version of dotnet to use");
        var frontendFramework = new Option<FrontendFramework?>(name: "--framework", description: "The frontend framework to use");
        var platform = new Option<TargetPlatform?>(name: "--platform", description: "The target platform");
        var buildMode = new Option<BuildMode?>(name: "--build-mode", description: "The build mode");
        var selfContained = new Option<bool>(name: "--self-contained", description: "Toggle self contained or not");

        var rootCommand = new RootCommand("Create apps with c# and webview");

        Command initProject = new Command("init", "Initialize a neutron project")
        {
            projectName,
            dotnetVersion,
            frontendFramework
        };

        rootCommand.AddCommand(initProject);

        initProject.SetHandler(async (projectName, dotnetVersion, frontendFramework) => {
            if (projectName is null)
            {
                projectName = UI.InputField("Project name");

                while (projectName.Contains(" "))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Project name can't contain spaces");
                    Console.ForegroundColor = ConsoleColor.White;
                    projectName = UI.InputField("Project name");
                }
            }

            if (frontendFramework is null)
            {
                Enum.TryParse(UI.InputField("Frontend framework", ["vanilla", "vanilla_ts", "vue", "vue_ts", "react", "react_ts", "react_swc", "react_swc_ts", "preact", "preact_ts", "lit", "lit_ts", "svelte", "svelte_ts", "solid", "solid_ts", "qwik", "qwik_ts"]), out FrontendFramework frontendFrameworkTemp);
                frontendFramework = frontendFrameworkTemp;
            }

            if (dotnetVersion is null)
            {
                Enum.TryParse(UI.InputField("Dotnet version", ["net5", "net6", "net7", "net8"]), out DotnetVersion dotnetVersionTemp);
                dotnetVersion = dotnetVersionTemp;
            }

            if (Directory.Exists(projectName))
            {
                ConsoleError.FolderWithSameNameAsProjectAlreadyExist(projectName);
                return;
            }

            Directory.CreateDirectory(projectName);

            string[] frontendNameParts = Regex.Split(projectName.Replace("_", "-"), @"(?<!^)(?=[A-Z])");
            string frontendName = $"{string.Join("-", frontendNameParts).ToLower()}-frontend";
            frontendName = Regex.Replace(frontendName, "-{2,}", "-");

            Console.WriteLine($"Calling npm create vite@latest {frontendName} -- --template {frontendFramework.ToString()!.Replace("_", "-")}");

            await Cli.Wrap("npm").WithArguments($"create vite@latest {frontendName} -- --template {frontendFramework.ToString()!.Replace("_", "-")}")
                                 .WithWorkingDirectory(Path.Combine(projectName))
                                 .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                                 .ExecuteBufferedAsync();

            Directory.CreateDirectory(Path.Combine(projectName, projectName));

            File.WriteAllText(Path.Combine(projectName, projectName, "Program.cs"), Templates.BackendProgramCSBuilder(projectName, frontendName));
            File.WriteAllText(Path.Combine(projectName, projectName, $"{projectName}.csproj"), Templates.BackendCSProjBuilder(projectName, dotnetVersion.ToString()!, frontendName));

            File.Copy(Path.Combine(AppContext.BaseDirectory, "icon.ico"), Path.Combine(projectName, projectName, "icon.ico"));

            Console.WriteLine("Calling dotnet add package Neutron");
            await Cli.Wrap("dotnet").WithArguments("add package Neutron")
                                    .WithWorkingDirectory(Path.Combine(projectName, projectName))
                                    .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                                    .ExecuteBufferedAsync();

            await Cli.Wrap("dotnet").WithArguments("add package CliWrap")
                     .WithWorkingDirectory(Path.Combine(projectName, projectName))
                     .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                     .ExecuteBufferedAsync();

            Templates.ProjectConfigBuilder(projectName, dotnetVersion.ToString()!, frontendName, projectName);

        }, projectName, dotnetVersion, frontendFramework);

        Command runProject = new Command("run", "Run a neutron project, should be run in the project directory root, aka the one containing config.json");

        rootCommand.AddCommand(runProject);

        runProject.SetHandler(async () =>
        {
            ProjectConfig? projectConfigRun = Builder.GetProjectConfig();

            if (projectConfigRun is null)
            {
                return;
            }

            string osName = "unknown";

            if (OperatingSystem.IsWindows())
            {
                osName = "win-x64";
            }
            else if (OperatingSystem.IsLinux())
            {
                osName = "linux-x64";
            }

            if (osName == "unknown")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Platform are not supported");
                Console.ForegroundColor = ConsoleColor.White;
            }

            await Builder.RunBackendAsync(projectConfigRun, "debug", osName);
        });

        Command buildProject = new Command("build", "Build the project for production")
        {
            platform,
            buildMode,
            selfContained
        };

        rootCommand.AddCommand(buildProject);

        buildProject.SetHandler(async (platform, buildMode, selfContained) =>
        {
            ProjectConfig? projectConfigBuild = Builder.GetProjectConfig();

            if (projectConfigBuild is null)
            {
                return;
            }

            if (platform is null)
            {
                Enum.TryParse(UI.InputField("Target platform", ["win_x64", "linux_x64"]), out TargetPlatform platformTemp);

                platform = platformTemp;
            }

            if (buildMode is null)
            {
                Enum.TryParse(UI.InputField("Build mode", ["debug", "release"]), out BuildMode buildModeTemp);
                buildMode = buildModeTemp;
            }

            await Builder.BuildBackendAsync(projectConfigBuild, buildMode.ToString()!.FirstCharToUpper(), platform.ToString()!.Replace("_", "-"), selfContained);
        }, platform, buildMode, selfContained);

        return await rootCommand.InvokeAsync(args);
    }
}