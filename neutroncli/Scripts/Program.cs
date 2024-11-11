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
        var frontend = new Option<bool>(name: "--frontend", description: "Build only the frontend and put the result in dist folder and copy it to the backend");

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
                Enum.TryParse(UI.InputField("Frontend framework", Enum.GetNames(typeof(FrontendFramework)).ToHashSet()), out FrontendFramework frontendFrameworkTemp);
                frontendFramework = frontendFrameworkTemp;
            }

            if (dotnetVersion is null)
            {
                Enum.TryParse(UI.InputField("Dotnet version", Enum.GetNames(typeof(DotnetVersion)).ToHashSet()), out DotnetVersion dotnetVersionTemp);
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

            string commandStr;

            if (frontendFramework.ToString() == "svelte_kit")
            {
                commandStr = $"sv create {frontendName} --template minimal --types ts --no-add-ons --no-install";

                Console.WriteLine($"Calling npx {commandStr}");

                await Cli.Wrap("npx").WithArguments(commandStr)
                                     .WithWorkingDirectory(Path.Combine(projectName))
                                     .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                                     .ExecuteBufferedAsync();
            }
            else
            {
                commandStr = $"create vite@latest {frontendName} -- --template {frontendFramework.ToString()!.Replace("_", "-")}";

                Console.WriteLine($"Calling npm {commandStr}");

                await Cli.Wrap("npm").WithArguments(commandStr)
                                     .WithWorkingDirectory(Path.Combine(projectName))
                                     .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                                     .ExecuteBufferedAsync();
            }

            commandStr = "install -D @sveltejs/adapter-static";

            Console.WriteLine($"Calling npm {commandStr}");
            await Cli.Wrap("npm").WithArguments(commandStr)
                                    .WithWorkingDirectory(Path.Combine(projectName, frontendName))
                                    .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                                    .ExecuteBufferedAsync();

            string modifiedSvelteConfig = """
            import adapter from '@sveltejs/adapter-static';
            import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

            /** @type {import('@sveltejs/kit').Config} */
            const config = {
            	// Consult https://svelte.dev/docs/kit/integrations
            	// for more information about preprocessors
            	preprocess: vitePreprocess(),

            	kit: {
            		// adapter-auto only supports some environments, see https://svelte.dev/docs/kit/adapter-auto for a list.
            		// If your environment is not supported, or you settled on a specific environment, switch out the adapter.
            		// See https://svelte.dev/docs/kit/adapters for more information about adapters.
            		adapter: adapter()
            	}
            };

            export default config;
            
            """;

            File.WriteAllText(Path.Combine(projectName, frontendName, "svelte.config.js"), modifiedSvelteConfig);

            string layout = "export const prerender = true;";

            File.WriteAllText(Path.Combine(projectName, frontendName, "src", "routes", "+layout.ts"), layout);

            Directory.CreateDirectory(Path.Combine(projectName, projectName));

            File.WriteAllText(Path.Combine(projectName, projectName, "Program.cs"), Templates.BackendProgramCSBuilder(projectName, frontendName));
            File.WriteAllText(Path.Combine(projectName, projectName, $"{projectName}.csproj"), Templates.BackendCSProjBuilder(projectName, dotnetVersion.ToString()!, frontendName));

            File.Copy(Path.Combine(AppContext.BaseDirectory, "icon.ico"), Path.Combine(projectName, projectName, "icon.ico"));


            Console.WriteLine("Calling dotnet new sln");
            await Cli.Wrap("dotnet").WithArguments("new sln")
                                    .WithWorkingDirectory(Path.Combine(projectName, projectName))
                                    .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                                    .ExecuteBufferedAsync();


            Console.WriteLine($"Calling dotnet sln add {projectName}.csproj");
            await Cli.Wrap("dotnet").WithArguments($"sln add {projectName}.csproj")
                                    .WithWorkingDirectory(Path.Combine(projectName, projectName))
                                    .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                                    .ExecuteBufferedAsync();

            Console.WriteLine("Calling dotnet add package Neutron");
            await Cli.Wrap("dotnet").WithArguments("add package Neutron")
                                    .WithWorkingDirectory(Path.Combine(projectName, projectName))
                                    .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
                                    .ExecuteBufferedAsync();

            Templates.ProjectConfigBuilder(projectName, dotnetVersion.ToString()!, frontendName, projectName);

            string gitIgnoreContent = $"""
            {projectName}/.vs
            {projectName}/bin
            {projectName}/obj
            {projectName}/dist
            {frontendName}/bin
            {frontendName}/obj
            {frontendName}/node_modules
            .vs/
            """.Trim();

            File.WriteAllText(Path.Combine(projectName, ".gitignore"), gitIgnoreContent);


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
            selfContained,
            frontend
        };

        rootCommand.AddCommand(buildProject);

        buildProject.SetHandler(async (platform, buildMode, selfContained, frontend) =>
        {
            ProjectConfig? projectConfigBuild = Builder.GetProjectConfig();

            if (projectConfigBuild is null)
            {
                return;
            }

            if (frontend)
            {
                string diskOutputPath = Path.Combine(projectConfigBuild.BackendName, "bin", "Debug", projectConfigBuild.DotnetVersion, "dist");

                if (Directory.Exists(diskOutputPath))
                {
                    Console.WriteLine($"Deleting old {diskOutputPath}");
                    Directory.Delete(diskOutputPath, recursive: true);
                }

                if (File.Exists(".timestamp") && (!Directory.Exists(Path.Combine(projectConfigBuild.BackendName, "dist")) || !Directory.Exists(Path.Combine(projectConfigBuild.BackendName, "bin"))))
                {
                    File.Delete(".timestamp");
                }

                await Builder.BuildAndMoveFrontendAsync(projectConfigBuild);
                return;
            }

            if (platform is null)
            {
                Enum.TryParse(UI.InputField("Target platform", Enum.GetNames(typeof(TargetPlatform)).ToHashSet()), out TargetPlatform platformTemp);

                platform = platformTemp;
            }

            if (buildMode is null)
            {
                Enum.TryParse(UI.InputField("Build mode", Enum.GetNames(typeof(BuildMode)).ToHashSet()), out BuildMode buildModeTemp);
                buildMode = buildModeTemp;
            }

            await Builder.BuildBackendAsync(projectConfigBuild, buildMode.ToString()!.FirstCharToUpper(), platform.ToString()!.Replace("_", "-"), selfContained);
        }, platform, buildMode, selfContained, frontend);

        return await rootCommand.InvokeAsync(args);
    }
}