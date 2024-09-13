namespace neutroncli.Scripts.Components;

public static class ConsoleError
{
    public static void NoNeutronProjectPresent()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No neutron project present in this directory");
        Console.WriteLine("You can create new project using \"neutroncli init --name ProjectName --dotnet-version net8 --framework react_ts\"");
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void FolderWithSameNameAsProjectAlreadyExist(string projectName)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"A folder in this directory already exist with the name {projectName}");
        Console.ForegroundColor = ConsoleColor.White;
    }
}
