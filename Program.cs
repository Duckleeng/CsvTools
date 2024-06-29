﻿using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Data.Common;

namespace CsvTools
{
    internal class Program
    {
        const string version = "v1.0.0";
        
        static readonly string[] operators = { "==", "!=", "!", "<=", ">=", "<", ">", "(", ")", "||", "&&", "+", "-", "*", "/", "%", ",",
                                                ".ToString()", ".Contains(", ".EndsWith(", ".IndexOf(", ".LastIndexOf(", ".Replace(", ".StartsWith(", ".Substring(", ".ToLower()", ".ToUpper()" };

        static readonly char[] dChars = { '.', ',', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };

        static char sep = ',';
        static string metadataLine = "";

        static List<string> identifiers = new List<string>();
        static List<string> dataTypes = new List<string>();
        static List<List<string>> data = new List<List<string>>();

        static void Main(string[] args)
        {
            Console.WriteLine();
            if(args.Length == 0)
            {
                DefaultHelpScreen();
            }

            string inFile, outFile;
            bool ignore, overriden;

            switch (args[0])
            {
                case "help":

                    if(args.Length == 1)
                    {
                        DefaultHelpScreen();
                    }

                    switch (args[1])
                    {
                        case "delete-line":

                            Console.WriteLine("Deletes the lines which meet the specified criteria from the file.");
                            Console.WriteLine();
                            Console.WriteLine("Usage: CsvTools delete-line [arguments]");
                            Console.WriteLine();
                            Console.WriteLine("Supported arguments:");
                            Console.WriteLine("\t--condition [condition] REQUIRED");
                            Console.WriteLine("\t\tBoolean expression to check for each line. If the returned value is true, the line will be deleted.");
                            Console.WriteLine("\t\tUse column identifier names as variables to refer to specific values, keep the type of the value in mind (string/double).");
                            Console.WriteLine("\t\tCondition example: id1 == \"example\" && id2 + id3 > 5");
                            Console.WriteLine("\t\tMethods allowed in the condition: ToString(), Contains, EndsWith, IndexOf, LastIndexOf, Replace, StartsWith, Substring, ToLower(), ToUpper()");
                            Console.WriteLine("\t--input-file [input file] REQUIRED");
                            Console.WriteLine("\t\tFile from which the data is read from.");
                            Console.WriteLine("\t--output-file [output file]");
                            Console.WriteLine("\t\tFile to which the modified data is written to.");
                            Console.WriteLine("\t\tDefault output file: *inputfile*-Output.csv");
                            Console.WriteLine("\t--overwrite");
                            Console.WriteLine("\t\tOverwrite input file instead of writing to a new file.");
                            Console.WriteLine("\t--force, -f");
                            Console.WriteLine("\t\tDo not show overwriting confirmation prompt.");
                            Console.WriteLine("\t--separator [separator]");
                            Console.WriteLine("\t\tThe character that is treated as the CSV separator, takes priority over separator specified in the file.");
                            Console.WriteLine("\t\tThe default separator is a comma (,).");
                            Console.WriteLine("\t--ignore-identifiers");
                            Console.WriteLine("\t\tTreat the first line of the CSV as a data line instead of an identifier line.");
                            Console.WriteLine("\t\tWhen this argument is present column identifiers names are assigned as follows: id1, id2, id3...");
                            Console.WriteLine("\t\tBy default column identifiers are read from the first line of the CSV.");

                            break;
                        case "delete-column":

                            Console.WriteLine("Deletes the selected columns from the file.");
                            Console.WriteLine();
                            Console.WriteLine("Usage: CsvTools delete-column [arguments]");
                            Console.WriteLine();
                            Console.WriteLine("Supported arguments:");
                            Console.WriteLine("\t--identifiers [identifier list] REQUIRED");
                            Console.WriteLine("\t\tList of column identifiers separated by comma that specifies which columns should be deleted.");
                            Console.WriteLine("\t\tIdentifier list example: id1,id2,id3");
                            Console.WriteLine("\t--input-file [input file] REQUIRED");
                            Console.WriteLine("\t\tFile from which the data is read from.");
                            Console.WriteLine("\t--output-file [output file]");
                            Console.WriteLine("\t\tFile to which the modified data is written to.");
                            Console.WriteLine("\t\tDefault output file: *inputfile*-Output.csv");
                            Console.WriteLine("\t--overwrite");
                            Console.WriteLine("\t\tOverwrite input file instead of writing to a new file.");
                            Console.WriteLine("\t--force, -f");
                            Console.WriteLine("\t\tDo not show overwriting confirmation prompt.");
                            Console.WriteLine("\t--separator [separator]");
                            Console.WriteLine("\t\tThe character that is treated as the CSV separator, takes priority over separator specified in the file.");
                            Console.WriteLine("\t\tThe default separator is a comma (,).");
                            Console.WriteLine("\t--ignore-identifiers");
                            Console.WriteLine("\t\tTreat the first line of the CSV as a data line instead of an identifier line.");
                            Console.WriteLine("\t\tWhen this argument is present column identifiers names are assigned as follows: id1, id2, id3...");
                            Console.WriteLine("\t\tBy default column identifiers are read from the first line of the CSV.");

                            break;
                        case "analyze":
                        case "analyse": //for the british fellas

                            Console.WriteLine("Analyzes selected rows and outputs statictics about them.");
                            Console.WriteLine();
                            Console.WriteLine("Usage: CsvTools analyze [arguments]");
                            Console.WriteLine();
                            Console.WriteLine("Supported arguments:");
                            Console.WriteLine("\t--identifiers [identifier list] REQUIRED");
                            Console.WriteLine("\t\tList of column identifiers separated by comma that specifies which columns should be analyzed.");
                            Console.WriteLine("\t\tIdentifier list example: id1,id2,id3");
                            Console.WriteLine("\t--input-file [input file] REQUIRED");
                            Console.WriteLine("\t\tFile from which the data is read from.");
                            Console.WriteLine("\t--separator [separator]");
                            Console.WriteLine("\t\tThe character that is treated as the CSV separator, takes priority over separator specified in the file.");
                            Console.WriteLine("\t\tThe default separator is a comma (,).");
                            Console.WriteLine("\t--ignore-identifiers");
                            Console.WriteLine("\t\tTreat the first line of the CSV as a data line instead of an identifier line.");
                            Console.WriteLine("\t\tWhen this argument is present column identifiers names are assigned as follows: id1, id2, id3...");
                            Console.WriteLine("\t\tBy default column identifiers are read from the first line of the CSV.");

                            break;
                        default:

                            Console.WriteLine("Invalid argument!");
                            Console.WriteLine();
                            DefaultHelpScreen();

                            break;
                    }

                    break;
                case "delete-line":
                    
                    inFile = InputFile(args);
                    ignore = IgnoreIdentifiers(args);
                    overriden = Separator(args);
                    LoadCsv(inFile, ignore, overriden);

                    DeleteLines(args);

                    outFile = OutputFile(args);
                    OutputToCsv(outFile, ignore);
                    Console.WriteLine("Action successful.");

                    break;
                case "delete-column":

                    inFile = InputFile(args);
                    ignore = IgnoreIdentifiers(args);
                    overriden = Separator(args);
                    LoadCsv(inFile, ignore, overriden);

                    DeleteColumns(args);

                    outFile = OutputFile(args);
                    OutputToCsv(outFile, ignore);
                    Console.WriteLine("Action successful.");

                    break;
                case "analyze":
                case "analyse":

                    inFile = InputFile(args);
                    ignore = IgnoreIdentifiers(args);
                    overriden = Separator(args);
                    LoadCsv(inFile, ignore, overriden);

                    Analyze(args);

                    break;

                default:
                    Console.WriteLine("Invalid action!");
                    Console.WriteLine();
                    DefaultHelpScreen();
                    break;
            }
        }

        static void DefaultHelpScreen()
        {
            Console.WriteLine("CsvTools " + version);
            Console.WriteLine("REMINDER: Special characters need to be escaped to be passed as an argument!");
            Console.WriteLine();
            Console.WriteLine("Usage: CsvTools [action] [arguments]");
            Console.WriteLine();
            Console.WriteLine("Supported actions: delete-line, delete-column, analyze, help");
            Console.WriteLine("Use \"CsvTools help *action*\" for more details");
            Environment.Exit(0);
        }

        static void ExitMessage(string message)
        {
            Console.WriteLine(message);
            Environment.Exit(0);
        }

        static string InputFile(string[] args)
        {
            string file = "";
            if (args.Contains("--input-file"))
            {
                int argIndex = Array.IndexOf(args, "--input-file") + 1;
                if(argIndex < args.Length)
                {
                    file = args[argIndex];
                }
                else
                {
                    ExitMessage("Input file required.");
                }
                if(!File.Exists(file))
                {
                    ExitMessage("Input file doesn't exist.");
                }
            }
            else
            {
                ExitMessage("Use --input-file to specify the input file.");
            }
            return file;
        }

        static string OutputFile(string[] args)
        {
            bool overwritten = false;
            string inFile = args[Array.IndexOf(args, "--input-file")+1];
            string extension = inFile.Substring(inFile.LastIndexOf("."));
            string outFile = inFile + "-Output" + extension;
            if (args.Contains("--output-file"))
            {
                int argIndex = Array.IndexOf(args, "--output-file") + 1;
                if(argIndex < args.Length)
                {
                    outFile = args[argIndex];
                }
                else
                {
                    ExitMessage("Invalid output file.");
                }
                if(File.Exists(outFile))
                {
                    overwritten = true;
                    if(!args.Contains("--force") && !args.Contains("-f"))
                    {
                        Console.Write("The specified output file already exists, do you wish to overwrite it? [Y/N] ");
                        string? input = Console.ReadLine();
                        if (input != "Y" && input != "y")
                        {
                            Environment.Exit(0);
                        }
                    }
                }
                if (!overwritten)
                {
                    try
                    {
                        File.Create(outFile).Close();
                    }
                    catch(Exception e)
                    {
                        ExitMessage(e.Message);
                    }
                }
            }
            else if (args.Contains("--overwrite"))
            {
                if (!args.Contains("--force") && !args.Contains("-f"))
                {
                    Console.Write("Are you sure you wish to overwrite the existing input file? [Y/N] ");
                    string? input = Console.ReadLine();
                    if(input != "Y" && input != "y")
                    {
                        Environment.Exit(0);
                    }
                }
                outFile = inFile;
            }
            else
            {
                if(File.Exists(outFile))
                {
                    if (!args.Contains("--force") && !args.Contains("-f"))
                    {
                        Console.Write("A file with the name of the automatically chosen output file already exists, do you wish to overwrite it? [Y/N] ");
                        string? input = Console.ReadLine();
                        if (input != "Y" && input != "y")
                        {
                            Environment.Exit(0);
                        }
                    }
                }
            }
            return outFile;
        }

        static bool Separator(string[] args)
        {
            bool overriden = false;
            if (args.Contains("--separator"))
            {
                int argIndex = Array.IndexOf(args, "--separator") + 1;
                if(argIndex < args.Length)
                {
                    string sepInput = args[argIndex];
                    if (sepInput.Length == 1)
                    {
                        sep = sepInput[0];
                        overriden = true;
                    }
                    else
                    {
                        ExitMessage("Separator must be a single character.");
                    }
                }
                else
                {
                    ExitMessage("Separator not entered.");
                }
            }
            return overriden;
        }

        static bool IgnoreIdentifiers(string[] args)
        {
            bool ignore = false;
            if (args.Contains("--ignore-identifiers"))
            {
                ignore = true;
            }
            return ignore;
        }

        static void LoadCsv(string fileName, bool ignoreLineIdentifier, bool overrideSeparator)
        {
            string[] lines = File.ReadAllLines(fileName);

            if(lines.Length == 0)
            {
                ExitMessage("Input file is empty.");
            }

            int position = 0;

            bool sepDefinedInFile = false;

            if (Regex.IsMatch(lines[0], "sep=."))
            {
                position++;
                sepDefinedInFile = true;
                metadataLine = lines[0];
            }

            if(!overrideSeparator && sepDefinedInFile)
            {
                sep = lines[0][4];
            }

            //read identifier line
            string currentChar;
            int currentCharIndex = 0;
            bool insideQuotes = false;
            string currentValue = "";
            if (!ignoreLineIdentifier)
            {
                while(true)
                {
                    if(currentCharIndex == lines[position].Length)
                    {
                        currentChar = Environment.NewLine;
                    }
                    else
                    {
                        currentChar = lines[position][currentCharIndex].ToString();
                    }

                    if(!insideQuotes)
                    {
                        if(currentChar == "\"")
                        {
                            insideQuotes = true;
                        }else if(currentChar == sep.ToString())
                        {
                            identifiers.Add(currentValue);
                            currentValue = "";
                        }else if(currentChar == Environment.NewLine)
                        {
                            identifiers.Add(currentValue);
                            position++;
                            break;
                        }
                        else
                        {
                            currentValue += currentChar;
                        }
                    }
                    else
                    {
                        if(currentChar == Environment.NewLine)
                        {
                            currentValue += Environment.NewLine;
                            currentCharIndex = -1;
                            position++;
                        }else if(currentChar == "\"" && currentCharIndex + 1 < lines[position].Length && lines[position][currentCharIndex + 1] == '\"')
                        {
                            currentValue += "\"\"";
                            currentCharIndex++;
                        }else if(currentChar == "\"")
                        {
                            insideQuotes = false;
                        }
                        else
                        {
                            currentValue += currentChar;
                        }
                    }
                    
                    currentCharIndex++;
                }
            }

            //load data
            currentCharIndex = 0;
            insideQuotes = false;
            currentValue = "";

            int dataCounter = 0;
            bool newLineStart = true;
            while (position < lines.Length)
            {
                if (newLineStart)
                {
                    data.Add(new List<string>());
                }

                if(currentCharIndex == lines[position].Length)
                {
                    currentChar = Environment.NewLine;
                }
                else
                {
                    currentChar = lines[position][currentCharIndex].ToString();
                }

                if (!insideQuotes)
                {
                    if(currentChar == "\"")
                    {
                        insideQuotes = true;
                    }else if(currentChar == sep.ToString())
                    {
                        data[dataCounter].Add(currentValue);
                        currentValue = "";
                    }else if(currentChar == Environment.NewLine)
                    {
                        data[dataCounter].Add(currentValue);
                        currentValue = "";
                        position++;
                        dataCounter++;
                        newLineStart = true;
                        currentCharIndex = 0;
                        continue;
                    }
                    else
                    {
                        currentValue += currentChar;
                    }
                }
                else
                {
                    if(currentChar == Environment.NewLine)
                    {
                        currentValue += Environment.NewLine;
                        position++;
                        currentCharIndex = -1;
                    }else if(currentChar == "\"" && currentCharIndex + 1 < lines[position].Length && lines[position][currentCharIndex + 1] == '\"')
                    {
                        currentValue += "\"\"";
                        currentCharIndex++;
                    }else if(currentChar == "\"")
                    {
                        insideQuotes = false;
                    }
                    else
                    {
                        currentValue += currentChar;
                    }
                }

                currentCharIndex++;
                newLineStart = false;
            }

            if(data.Count == 0)
            {
                Console.WriteLine("No data found in the input file.");
                ExitMessage("Are you missing --ignore-identifiers?");
            }

            //identify line with most values
            int maxIndex = 0;
            int maxLen = data[0].Count;
            for (int i = 1; i < data.Count; i++)
            {
                if (data[i].Count > maxLen)
                {
                    maxLen = data[i].Count;
                    maxIndex = i;
                }
            }

            //load identifiers if there is no identifier line
            if (ignoreLineIdentifier)
            {
                for(int i = 1; i <= maxLen; i++)
                {
                    identifiers.Add("id" + i.ToString());
                }
            }

            //load data types
            bool validD;
            for(int i = 0; i < data[maxIndex].Count; i++)
            {
                validD = true;
                try
                {
                    Convert.ToDouble(data[maxIndex][i], System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    validD = false;
                }

                if (validD)
                {
                    dataTypes.Add("double");
                }
                else
                {
                    dataTypes.Add("string");
                }
            }
        }

        static void DeleteLines(string[] args)
        {
            //read the condition
            if (args.Contains("--condition"))
            {
                int argIndex = Array.IndexOf(args, "--condition") + 1;
                string condition = "";
                if(argIndex < args.Length)
                {
                    condition = args[argIndex];
                }
                else
                {
                    ExitMessage("A condition is required.");
                }

                if(condition.Length == 0)
                {
                    ExitMessage("Condition is empty.");
                }

                int index = 0;
                while (index < condition.Length)
                {
                    bool valid = false;

                    if (condition[index] == ' ')
                    {
                        index++;
                        continue;
                    }

                    //identifier check
                    foreach (string identifier in identifiers)
                    {
                        if (condition.Substring(index).StartsWith(identifier))
                        {
                            string value = $"values[{identifiers.IndexOf(identifier)}]";
                            condition = condition.Remove(index, identifier.Length).Insert(index, value);
                            index += value.Length;
                            valid = true;
                            break;
                        }
                    }

                    if (valid) continue;

                    //double check
                    string doubleValue = "";
                    int indexBackup = index;
                    if (condition[index] == '-')
                    {
                        doubleValue += '-';
                        index++;
                    }
                    while (index < condition.Length && dChars.Contains(condition[index]))
                    {
                        doubleValue += condition[index];
                        index++;
                    }

                    if(doubleValue != "")
                    {
                        valid = true;
                        try
                        {
                            Convert.ToDouble(doubleValue, System.Globalization.CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            valid = false;
                            index = indexBackup;
                        }
                    }

                    if (valid) continue;

                    //string check
                    if (condition[index] == '\"')
                    {
                        index++;

                        while (true)
                        {
                            if (index >= condition.Length)
                            {
                                break;
                            }
                            else if (condition[index] == '\"' && index + 1 < condition.Length && condition[index + 1] == '\"')
                            {
                                index++;
                            }
                            else if (condition[index] == '\"')
                            {
                                valid = true;
                                index++;
                                break;
                            }
                            index++;
                        }
                    }

                    if (valid) continue;

                    //operator check
                    foreach (string op in operators)
                    {
                        if (condition.Substring(index).StartsWith(op))
                        {
                            index += op.Length;
                            valid = true;
                            break;
                        }
                    }

                    if (!valid)
                    {
                        ExitMessage("Invalid condition!");
                    }
                }

                for(int i = 0; i < dataTypes.Count; i++)
                {
                    if (dataTypes[i] == "double")
                    {
                        condition = condition.Replace($"values[{i}]", $"System.Convert.ToDouble(values[{i}], System.Globalization.CultureInfo.InvariantCulture)");
                    }
                }

                var script = CSharpScript.Create<bool>(condition, globalsType: typeof(ArgumentValues));
                script.Compile();

                for (int i = 0; i < data.Count; i++)
                {
                    ArgumentValues argumentValues = new ArgumentValues();
                    for(int j = 0; j < data[i].Count; j++)
                    {
                        argumentValues.values.Add(data[i][j]);
                    }

                    try
                    {
                        if (script.RunAsync(argumentValues).GetAwaiter().GetResult().ReturnValue)
                        {
                            data.RemoveAt(i);
                            i--;
                        }
                    }
                    catch
                    {
                        ExitMessage("Invalid condition!");
                    }
                }
            }
            else
            {
                Console.WriteLine("Use --condition to specify which lines should be deleted.");
                ExitMessage("Use \"CsvTools help delete-line\" for more information.");
            }
        }

        static List<string> ReadListFromArg(string arg)
        {
            List<string> list = new List<string>();

            int index = 0;
            string currentChar;
            string currentValue = "";
            bool insideQuotes = false;
            while(index < arg.Length)
            {
                if (arg.Substring(index).StartsWith(Environment.NewLine))
                {
                    currentChar = Environment.NewLine;
                    index += Environment.NewLine.Length - 1;
                }
                else
                {
                    currentChar = arg[index].ToString();
                }

                if(!insideQuotes)
                {
                    if(currentChar == "\"")
                    {
                        insideQuotes = true;
                    }else if(currentChar == ",")
                    {
                        list.Add(currentValue);
                        currentValue = "";
                    }
                    else
                    {
                        currentValue += currentChar;
                    }
                }
                else
                {
                    if (currentChar == "\"" && index + 1 < arg.Length && arg[index + 1] == '\"')
                    {
                        currentValue += "\"\"";
                        index++;
                    }else if (currentChar == "\"")
                    {
                        insideQuotes = false;
                    }
                    else
                    {
                        currentValue += currentChar;
                    }
                }
                index++;
            }

            list.Add(currentValue);

            return list;
        }

        static void DeleteColumns(string[] args)
        {
            if (args.Contains("--identifiers"))
            {
                int argIndex = Array.IndexOf(args, "--identifiers") + 1;
                string columns = "";
                if(argIndex < args.Length)
                {
                    columns = args[argIndex];
                }
                else
                {
                    ExitMessage("A list of identifiers is required.");
                }

                if(columns.Length == 0)
                {
                    ExitMessage("List of identifiers is empty.");
                }

                List<string> columnList = ReadListFromArg(columns);
                List<string> columnListNoDuplicate = new List<string>();

                foreach (string column in columnList)
                {
                    if (!identifiers.Contains(column))
                    {
                        ExitMessage("Identifier list is invalid.");
                    }

                    if (!columnListNoDuplicate.Contains(column))
                    {
                        columnListNoDuplicate.Add(column);
                    }
                }

                foreach(string column in columnListNoDuplicate)
                {
                    int index = identifiers.IndexOf(column);

                    identifiers.RemoveAt(index);

                    for (int i = 0; i < data.Count; i++)
                    {
                        data[i].RemoveAt(index);
                    }
                }
            }
            else
            {
                Console.WriteLine("Use --identifiers to list which columns you wish to delete.");
                ExitMessage("Use \"CsvTools help delete-column\" for more information.");
            }
        }

        static void Analyze(string[] args)
        {
            if (args.Contains("--identifiers"))
            {
                int argIndex = Array.IndexOf(args, "--identifiers") + 1;
                string columns = "";
                if (argIndex < args.Length)
                {
                    columns = args[argIndex];
                }
                else
                {
                    ExitMessage("A list of identifiers is required.");
                }

                if (columns.Length == 0)
                {
                    ExitMessage("List of identifiers is empty.");
                }

                List<string> columnList = ReadListFromArg(columns);

                foreach(string column in columnList)
                {
                    if (!identifiers.Contains(column))
                    {
                        ExitMessage("Identifier list is invalid.");
                    }

                    if (dataTypes[identifiers.IndexOf(column)] != "double")
                    {
                        ExitMessage("All listed identifiers must be of type double.");
                    }
                }

                foreach(string column in columnList)
                {
                    int index = identifiers.IndexOf(column);

                    List<double> values = new List<double>();
                    for(int i = 0; i < data.Count; i++)
                    {
                        values.Add(Convert.ToDouble(data[i][index], System.Globalization.CultureInfo.InvariantCulture));
                    }
                    values.Sort();

                    int NoV = values.Count;
                    double SUM = values.Sum();
                    double AVG = SUM / NoV;
                    double MIN = values[0];
                    double MAX = values.Last();
                    double STDEV = Math.Sqrt(values.Average(x => Math.Pow(x-AVG, 2)));

                    double midIndex = (NoV - 1) / 2.0;
                    double MEDIAN = (values[(int)midIndex] + values[(int)(midIndex + 0.5)]) / 2.0;

                    Console.WriteLine();
                    Console.WriteLine(column);
                    Console.WriteLine("NUMBER OF VALUES: " + NoV);
                    Console.WriteLine("SUM: " + SUM);
                    Console.WriteLine("MEAN (AVG): " + AVG);
                    Console.WriteLine("MEDIAN: " + MEDIAN);
                    Console.WriteLine("MIN: " + MIN);
                    Console.WriteLine("MAX: " + MAX);
                    Console.WriteLine("STDEV: " +  STDEV);
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Use --identifiers to list which columns you wish to analyze.");
                ExitMessage("Use \"CsvTools help analyze\" for more information.");
            }
        }

        static void OutputToCsv(string file, bool ignoreLineIdentifier)
        {
            bool writingStarted = false;
            
            if(metadataLine != "")
            {
                File.WriteAllText(file, metadataLine + Environment.NewLine);
                writingStarted = true;
            }
            
            if(!ignoreLineIdentifier)
            {
                string outputLine = "";
                for(int i = 0; i < identifiers.Count; i++)
                {
                    if(i == identifiers.Count - 1)
                    {
                        outputLine += AddQuotes(identifiers[i]);
                    }
                    else
                    {
                        outputLine += AddQuotes(identifiers[i]) + sep;
                    }
                }
                outputLine += Environment.NewLine;

                if (writingStarted)
                {
                    File.AppendAllText(file, outputLine);
                }
                else
                {
                    File.WriteAllText(file, outputLine);
                }
                writingStarted = true;
            }

            string[] output = new string[data.Count];
            for(int i = 0; i < data.Count; i++)
            {
                for(int j = 0; j < data[i].Count; j++)
                {
                    if(j == data[i].Count - 1)
                    {
                        output[i] += AddQuotes(data[i][j]);
                    }
                    else
                    {
                        output[i] += AddQuotes(data[i][j]) + sep;
                    }
                }
            }

            if(writingStarted)
            {
                File.AppendAllLines(file, output);
            }
            else
            {
                File.WriteAllLines(file, output);
            }
        }

        static string AddQuotes(string input, bool force = false)
        {
            if(input.Contains('\"') || input.Contains(sep) || input.Contains(Environment.NewLine) || force)
            {
                input = '\"' + input + '\"';
            }
            return input;
        }
    }

    //used to pass values into the on-runtime compiled script to avoid recompiling it for every line
    public class ArgumentValues
    {
        public List<string> values = new List<string>();
    }
}