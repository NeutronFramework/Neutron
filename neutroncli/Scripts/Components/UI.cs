namespace NeutronCli.Scripts.Components;

public static class UI
{
    static List<string> uiStacks = new();

    public static string InputField(string label, HashSet<string>? options = null)
    {
        string possibleValuesStr;

        if (options is null)
        {
            possibleValuesStr = "";
        }
        else
        {
            possibleValuesStr = $" ({string.Join(", ", options)})";
        }

        Console.Write($"{label}{possibleValuesStr}: ");
        string? result = Console.ReadLine();

        if (result is not null)
        {
            result = result.Trim();
        }

        if (options is null)
        {
            while (result is null || result.Trim() == "")
            {
                for (int i = uiStacks.Count - 1; i >= 0; i--)
                {
                    Console.WriteLine(uiStacks[i]);
                }

                Console.Write($"{label}{possibleValuesStr}: ");
                result = Console.ReadLine();

                if (result is not null)
                {
                    result = result.Trim();
                }
            }
        }
        else
        {
            while (result is null || result.Trim() == "" || !options.Contains(result))
            {
                for (int i = uiStacks.Count - 1; i >= 0; i--)
                {
                    Console.WriteLine(uiStacks[i]);
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{result} is not on the options");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{label}{possibleValuesStr}: ");

                result = Console.ReadLine();

                if (result is not null)
                {
                    result = result.Trim();
                }
            }
        }

        uiStacks.Insert(0, $"{label}{possibleValuesStr}: {result}");

        return result;
    }
}
