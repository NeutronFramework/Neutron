using NeutronCli.Scripts.DataStructures;
using System.Text.Json;

namespace NeutronCli.Scripts.Components;
public static class Templates
{
    public static string BackendProgramCSBuilder(string projectName, string frontendName)
    {
        return $$"""
using Neutron.Scripts;

namespace {{projectName}};

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application application = new Application(title: "{{projectName}}", width: 960, height: 540, webContentPath: Path.Combine(AppContext.BaseDirectory, "dist", debug: true));

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
    <Exec Command="neutroncli build --frontend" WorkingDirectory="../" />
    
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
