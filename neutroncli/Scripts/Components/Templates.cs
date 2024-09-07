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
        Application application = new Application(title: "{{projectName}}", width: 960, height: 540, webContentPath: Path.Combine(AppContext.BaseDirectory, "dist"));

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
    </PropertyGroup>

    <PropertyGroup Condition="'$(Configuration)'=='Release'">
        <DebugSymbols>False</DebugSymbols>
        <DebugType>None</DebugType>
    </PropertyGroup>

    <Target Name="BuildFrontend" BeforeTargets="BeforeBuild">
        <Exec Command="neutroncli build --frontend" WorkingDirectory="../" />
    </Target>

    <Target Name="CopyDistFolderOnBuild" AfterTargets="Build">
        <ItemGroup>
            <DistFiles Include="dist\**" />
        </ItemGroup>
        <Copy SourceFiles="@(DistFiles)" DestinationFolder="$(OutDir)dist\%(RecursiveDir)" SkipUnchangedFiles="true" />
    </Target>

    <Target Name="CopyDistFolderOnPublish" AfterTargets="Publish">
        <ItemGroup>
            <DistFiles Include="dist\**" />
        </ItemGroup>
        <Copy SourceFiles="@(DistFiles)" DestinationFolder="$(PublishDir)dist\%(RecursiveDir)" SkipUnchangedFiles="true" />
    </Target>

    <Target Name="CopyDistFolderOnRun" BeforeTargets="Build">
        <ItemGroup>
            <DistFiles Include="dist\**" />
        </ItemGroup>
        <Copy SourceFiles="@(DistFiles)" DestinationFolder="$(OutDir)dist\%(RecursiveDir)" SkipUnchangedFiles="true" />
    </Target>

    <ItemGroup>
        <Content Include="dist\**">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </Content>
    </ItemGroup>
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
