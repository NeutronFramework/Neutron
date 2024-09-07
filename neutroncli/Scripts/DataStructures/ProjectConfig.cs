namespace NeutronCli.Scripts.DataStructures;

public class ProjectConfig
{
    public string ProjectName { get; set; }
    public string DotnetVersion { get; set; }
    public string FrontendName { get; set; }
    public string BackendName { get; set; }

    public ProjectConfig(string projectName, string dotnetVersion, string frontendName, string backendName)
    {
        ProjectName = projectName;
        DotnetVersion = dotnetVersion;
        FrontendName = frontendName;
        BackendName = backendName;
    }
}
