namespace neutroncli.Scripts.Components;

public static class ConsoleError
{

    public static void NoNeutronProjectPresent()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: No neutron project present in this directory");
        Console.WriteLine("You can create new project using \"neutroncli init\" or the faster way using \"neutroncli init --name Testing --dotnet-version .net8.0 --framework react_ts\"");
    }

    public static void FolderWithSameNameAsProjectAlreadyExist(string projectName)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: A folder in this directory already exist with the name {projectName}");
    }
}
