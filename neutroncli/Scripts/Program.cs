using CliWrap;
using CliWrap.Buffered;
using McMaster.Extensions.CommandLineUtils;
using neutroncli.Scripts.Components;
using neutroncli.Scripts.DataStructures;
using NeutronCli.Scripts.Components;
using NeutronCli.Scripts.DataStructures;
using NeutronCli.Scripts.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NeutronCli.Scripts;

class Program
{
    static async Task Main(string[] args)
    {
        CommandLineApplication.Execute<Program>(args);
    }

    [Argument(0)]
    [Required]
    public string? Command { get; }

    [Option(ShortName = "name", LongName = "name", Description = "The name of the neutron project")]
    public string? ProjectName { get; set; }

    [Option(ShortName = "dotnet-version", LongName = "dotnet-version", Description = "The version of dotnet, the options are net5, net6, net7, and net8")]
    public DotnetVersion? DotnetVersion { get; set; }

    [Option(ShortName = "framework", LongName = "framework", Description = "The frontend framework, the options are vanilla, vanilla_ts, vue, vue_ts, react, react_ts, react_swc, react_swc_ts, preact, preact_ts, lit, lit_ts, svelte, svelte_ts, solid, solid_ts, qwik, qwik_ts}")]
    public FrontendFramework? FrontendFramework { get; set; }

    [Option(ShortName = "platform", LongName = "platform", Description = "The target platform, the options are win_x64, linux_x64")]
    public TargetPlatform? TargetPlatform { get; set; }

    [Option(ShortName = "build-mode", LongName = "build-mode", Description = "The build mode, the options are debug, release")]
    public BuildMode? BuildMode { get; set; }

    [Option(ShortName = "self-contained", LongName = "self-contained", Description = "Toggle self contained or not")]
    public bool SelfContained { get; set; }

    [Option(ShortName = "frontend", LongName = "frontend", Description = "Toggle only build frontend")]
    public bool Frontend { get; set; }

    private async Task OnExecuteAsync()
    {
        switch (Command)
        {
            case "init":
                if (ProjectName is null)
                {
                    ProjectName = UI.InputField("Project name");

                    while (ProjectName.Contains(" "))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Project name can't contain spaces");
                        Console.ForegroundColor = ConsoleColor.White;
                        ProjectName = UI.InputField("Project name");
                    }
                }

                if (FrontendFramework is null)
                {
                    Enum.TryParse(UI.InputField("Frontend framework", ["vanilla", "vanilla_ts", "vue", "vue_ts", "react", "react_ts", "react_swc", "react_swc_ts", "preact", "preact_ts", "lit", "lit_ts", "svelte", "svelte_ts", "solid", "solid_ts", "qwik", "qwik_ts"]), out FrontendFramework frontendFramework);
                    FrontendFramework = frontendFramework;
                }

                if (DotnetVersion is null)
                {
                    Enum.TryParse(UI.InputField("Dotnet version", ["net5", "net6", "net7", "net8"]), out DotnetVersion dotnetVersion);
                    DotnetVersion = dotnetVersion;
                }

                if (Directory.Exists(ProjectName))
                {
                    ConsoleError.FolderWithSameNameAsProjectAlreadyExist(ProjectName);
                    return;
                }

                Directory.CreateDirectory(ProjectName);

                string[] frontendNameParts = Regex.Split(ProjectName.Replace("_", "-"), @"(?<!^)(?=[A-Z])");
                string frontendName = $"{string.Join("-", frontendNameParts).ToLower()}-frontend";
                frontendName = Regex.Replace(frontendName, "-{2,}", "-");

                Directory.CreateDirectory(Path.Combine(ProjectName, frontendName));

                Console.WriteLine("Calling npm create with vite");

                await Cli.Wrap("npm").WithArguments($"create vite@latest . -- --template {FrontendFramework.ToString()!.Replace("_", "-")} .")
                                     .WithWorkingDirectory(Path.Combine(ProjectName, frontendName))
                                     .ExecuteAsync();

                Directory.CreateDirectory(Path.Combine(ProjectName, ProjectName));

                File.WriteAllText(Path.Combine(ProjectName, ProjectName, "Program.cs"), Templates.BackendProgramCSBuilder(ProjectName, frontendName));
                File.WriteAllText(Path.Combine(ProjectName, ProjectName, $"{ProjectName}.csproj"), Templates.BackendCSProjBuilder(ProjectName, DotnetVersion.ToString()!, frontendName));

                List<string> cliBinaryPath = Environment.GetCommandLineArgs().First().Split(Path.DirectorySeparatorChar).ToList();
                cliBinaryPath.RemoveAt(cliBinaryPath.Count - 1);

                File.Copy(Path.Combine(string.Join(Path.DirectorySeparatorChar, cliBinaryPath), "icon.ico"), Path.Combine(ProjectName, ProjectName, "icon.ico"));

                Console.WriteLine("Calling dotnet add package Neutron");
                await Cli.Wrap("dotnet").WithArguments("add package Neutron")
                                        .WithWorkingDirectory(Path.Combine(ProjectName, ProjectName))
                                        .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                                        .ExecuteBufferedAsync();

                await Cli.Wrap("dotnet").WithArguments("add package CliWrap")
                         .WithWorkingDirectory(Path.Combine(ProjectName, ProjectName))
                         .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                         .ExecuteBufferedAsync();

                Templates.ProjectConfigBuilder(ProjectName, DotnetVersion.ToString()!, frontendName, ProjectName);
                break;

            case "run":
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
                }

                await Builder.RunBackendAsync(projectConfigRun, "debug", osName);
                break;

            case "build":
                ProjectConfig? projectConfigBuild = Builder.GetProjectConfig();

                if (projectConfigBuild is null)
                {
                    return;
                }

                if (Frontend)
                {
                    await Builder.BuildFrontendAsync(projectConfigBuild);
                    Builder.MoveDistToBackend(projectConfigBuild);
                    return;
                }

                if (TargetPlatform is null)
                {
                    Enum.TryParse(UI.InputField("Target platform", ["win_x64", "linux_x64"]), out TargetPlatform targetPlatform);

                    TargetPlatform = targetPlatform;
                }

                if (BuildMode is null)
                {
                    Enum.TryParse(UI.InputField("Build mode", ["debug", "release"]), out BuildMode buildMode);
                    BuildMode = buildMode;
                }

                await Builder.BuildBackendAsync(projectConfigBuild, BuildMode.ToString()!.FirstCharToUpper(), TargetPlatform.ToString()!.Replace("_", "-"), SelfContained);
                break;
        }
    }
}