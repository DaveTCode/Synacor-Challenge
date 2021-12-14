using CommandLine;
using Serilog;

namespace Synacor.Challenge;

public class CliOptions
{
    [Option('f', "file", Required = true, HelpText = "Path to the binary")]
    public string File { get; }

    public CliOptions(string file)
    {
        File = file;
    }
}

[Verb("disassemble")]
public class DisassembleOptions : CliOptions
{
    public DisassembleOptions(string file) : base(file)
    {
    }
}

[Verb("run")]
public class RunOptions : CliOptions
{
    [Option('i', "inputsFile", Required = false, HelpText = "One line per input to submit to program")]
    public string InputsFile { get; }

    public RunOptions(string inputsFile, string file) : base(file)
    {
        InputsFile = inputsFile;
    }
}

[Verb("find_teleporter_code")]
public class FindCodeOptions : CliOptions
{
    [Option('i', "inputsFile", Required = false, HelpText = "One line per input to submit to program")]
    public string InputsFile { get; }

    public FindCodeOptions(string inputsFile, string file) : base(file)
    {
        InputsFile = inputsFile;
    }
}

internal static class Program
{
    internal static void Main(string[] args)
    {
        Parser.Default.ParseArguments<DisassembleOptions, RunOptions, FindCodeOptions>(args)
                   .WithParsed<DisassembleOptions>(o =>
                   {
                       var rom = File.ReadAllBytes(o.File);

                       Console.WriteLine(Disassembler.Disassemble(rom));
                   })
                   .WithParsed<FindCodeOptions>(o =>
                   {
                       var patches = new List<(int, int)>
                       {
                           (0x156B, 21),// NOP out the check for teleporter
                           (0x156C, 21),
                           (0x156D, 21),
                           (0x156E, 21),
                           (0x156F, 21),
                           (0x1570, 21),
                           (0x1571, 21), 
                           (0x1572, 21),
                       };
                       var debug = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .WriteTo.File("debug.log", buffered: true)
                        .CreateLogger();
                       var output = "";
                       var rom = File.ReadAllBytes(o.File);
                       var inputs = File.ReadAllText(o.InputsFile);
                       var vm = new Vm(rom, debug, (c) => output += c, patches);
                       vm.LoadInputs(inputs);

                       var codeBad = false;
                       for (var code = 1; code < 0x7FFF; code++)
                       {
                           while (true)
                           {
                               var (vmState, instructionPointer, registers) = vm.First();
                               if (instructionPointer == 0x0708) // Force r7 to code just before comparison
                               {
                                   registers[7] = code;
                               }
                               else if (instructionPointer == 6065 && output.Contains("Nothing else seems to happen")) // Think this is bad code
                               {
                                   codeBad = true;
                                   Console.WriteLine($"Code {code} known bad");
                                   break;
                               }

                               if (vmState != VmState.Running) break;
                           }

                           if (!codeBad)
                           {
                               Console.WriteLine($"Code is {code}");
                               break;
                           }
                       }
                   })
                   .WithParsed<RunOptions>(o =>
                   {
                       var patches = new List<(int, int)>
                       {
                           (0x156B, 21),// NOP out the check for teleporter
                           (0x156C, 21),
                           (0x156D, 21),
                           (0x156E, 21),
                           (0x156F, 21),
                           (0x1570, 21),
                           (0x1571, 21),
                           (0x1572, 21),
                           (0x1573, 21),
                           (0x1574, 21),
                           (0x1575, 21),
                           (0x1576, 21),
                           (0x1577, 21),
                           (0x1578, 21),
                           (0x1579, 21),
                       };
                       var debug = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .WriteTo.File("debug.log", buffered: true)
                        .CreateLogger();
                       var rom = File.ReadAllBytes(o.File);
                       var inputs = File.ReadAllText(o.InputsFile);
                       var vm = new Vm(rom, debug, Console.Write, patches);
                       vm.LoadInputs(inputs);

                       while (true)
                       {
                           var (vmState, _, _) = vm.First();
                           if (vmState != VmState.Running) break;
                       }

                       Console.WriteLine("-----");
                       Console.WriteLine(vm.ToString());
                   });
    }
}
