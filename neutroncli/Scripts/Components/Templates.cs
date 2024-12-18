﻿using NeutronCli.Scripts.DataStructures;
using System.Text.Json;

namespace NeutronCli.Scripts.Components;


public static class Templates
{
    public static string BackendProgramCSBuilder(string projectName, string frontendName)
    {
        return $$"""
using Neutron.Scripts;
using System.Diagnostics;

namespace {{projectName}};

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
        {
            ProcessStartInfo processInfo = new()
            {
                FileName = "cmd.exe",
                Arguments = "/c CheckNetIsolation LoopbackExempt -a -n=Microsoft.win32webviewhost_cw5n1h2txyewy",
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            Process.Start(processInfo);
        }

        bool debug;

#if DEBUG
        debug = true;
#else
        debug = false;
#endif

        Application application = new Application(title: "{{projectName}}", width: 960, height: 540, webContentPath: Path.Combine(AppContext.BaseDirectory, "dist"), debug);

        application.Center();
        application.Run();
    }
}
""".Trim();
    }

    public static string BackendCSProjBuilder(string projectName, string dotnetVersion, string frontendName)
    {
        return $$"""
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <ApplicationIcon>icon.ico</ApplicationIcon>
    <OutputType>Exe</OutputType>
    <TargetFramework>{{dotnetVersion}}</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <WarningsAsErrors>Nullable</WarningsAsErrors>
    <InvariantGlobalization>True</InvariantGlobalization>
    <DisableFastUpToDateCheck>true</DisableFastUpToDateCheck>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)'=='Release'">
    <DebugSymbols>False</DebugSymbols>
    <DebugType>None</DebugType>
  </PropertyGroup>
  
  <Target Name="BuildFrontend">
    <Message Text="Working Directory: $([System.IO.Path]::GetFullPath('$(MSBuildThisFileDirectory)../'))"/>
    <Message Text="Executing Command: neutroncli build --frontend" Importance="high" />

    <Exec 
        Command="neutroncli build --frontend 2>&amp;1"
        WorkingDirectory="../"
        ConsoleToMSBuild="true"
        StandardOutputImportance="High"
        StandardErrorImportance="High"
        EchoOff="false"
        IgnoreExitCode="true">
        <Output TaskParameter="ConsoleOutput" PropertyName="RawOutput" />
        <Output TaskParameter="ExitCode" PropertyName="ErrorCode" />
    </Exec>

    <PropertyGroup>
        <FormattedOutput>$([System.String]::Join('%0D%0A    ', $(RawOutput.Split(';'))))</FormattedOutput>
    </PropertyGroup>

    <Message Text="Build Output:%0D%0A    $(FormattedOutput)" Importance="high" />
    <Error 
        Text="Frontend build failed with exit code $(ErrorCode).%0D%0A%0D%0AOutput:%0D%0A    $(FormattedOutput)" 
        Condition="'$(ErrorCode)' != '0'" />

    <ItemGroup>
        <DistFiles Include="dist\**" />
    </ItemGroup>
  </Target>

  <Target Name="CopyDistFolderOnBuild" AfterTargets="Build" DependsOnTargets="BuildFrontend">
    <Copy SourceFiles="@(DistFiles)" DestinationFolder="$(OutDir)dist\%(RecursiveDir)" />
  </Target>

  <Target Name="CopyDistFolderOnPublish" AfterTargets="Publish" DependsOnTargets="BuildFrontend">
    <Copy SourceFiles="@(DistFiles)" DestinationFolder="$(PublishDir)dist\%(RecursiveDir)" />
  </Target>
</Project>
""".Trim();
    }

    public static void ProjectConfigBuilder(string projectName, string dotnetVersion, string frontendName, string backendName)
    {
        ProjectConfig projectConfig = new ProjectConfig(projectName, dotnetVersion, frontendName, backendName);

        string jsonString = JsonSerializer.Serialize(projectConfig, SourceGenerationContext.Default.ProjectConfig);

        File.WriteAllText(Path.Combine(projectName, "config.json"), jsonString);
    }
}
