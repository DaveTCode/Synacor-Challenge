using System.Text;

namespace Synacor.Challenge
{
    internal class Disassembler
    {
        public static string Disassemble(byte[] binary)
        {
            int[] memory = new int[32768];
            if (binary == null) throw new ArgumentNullException(nameof(binary));
            if (binary.Length > memory.Length * 2) throw new ArgumentException(nameof(binary));

            for (var ii = 0; ii < binary.Length; ii += 2)
            {
                memory[ii / 2] = binary[ii] | (binary[ii + 1] << 8);
            }
            var disassembly = new StringBuilder();
            var pointer = 0;

            while (pointer < memory.Length)
            {
                var instruction = memory[pointer];

                disassembly.Append($"{pointer:X4}: ");

                // Entries which aren't operations are assumed to be data blocks and displayed as a single byte
                if (!Enum.IsDefined(typeof(Operation), instruction))
                {
                    disassembly.Append($"{instruction:X4}\n");
                    pointer++;
                    continue;
                }

                var operation = (Operation)instruction;
                var ps = Enumerable.Range(1, operation.OperationLength() - 1)
                        .Select(ii => (memory[pointer + ii], operation) switch
                            {
                                (var x, _) when x < 0 => throw new Exception("Negative memory address"),
                                (var x, Operation.Out) when x < memory.Length => ((char)x).ToString(),
                                (var x, _) when x < memory.Length => $"{x:X4}",
                                (var x, _) when x <= 32775 => $"r{x - 32768}",
                                _ => throw new Exception("Invalid memory address"),
                            })
                        .ToArray();
                disassembly.Append(
                    string.Join(" ", 
                                Enumerable.Range(0, operation.OperationLength())
                                    .Select(ii => $"{memory[pointer + ii]:X4}"))
                                .PadRight(20, ' '));
                disassembly.Append(operation switch
                {
                    Operation.Halt => "HLT",
                    Operation.Set => $"SET {ps[0]} {ps[1]}",
                    Operation.Push => $"PUSH {ps[0]}",
                    Operation.Pop => $"POP {ps[0]}",
                    Operation.Eq => $"EQ {ps[0]} {ps[1]} {ps[2]}",
                    Operation.Gt => $"GT {ps[0]} {ps[1]} {ps[2]}",
                    Operation.Jmp => $"JMP {ps[0]}",
                    Operation.Jt => $"JT {ps[0]} {ps[1]}",
                    Operation.Jf => $"JF {ps[0]} {ps[1]}",
                    Operation.Add => $"ADD {ps[0]} {ps[1]} {ps[2]}",
                    Operation.Mult => $"MULT {ps[0]} {ps[1]} {ps[2]}",
                    Operation.Mod => $"MOD {ps[0]} {ps[1]} {ps[2]}",
                    Operation.And => $"AND {ps[0]} {ps[1]} {ps[2]}",
                    Operation.Or => $"OR {ps[0]} {ps[1]} {ps[2]}",
                    Operation.Not => $"NOT {ps[0]} {ps[1]}",
                    Operation.Rmem => $"RMEM {ps[0]} {ps[1]}",
                    Operation.Wmem => $"WMEM {ps[0]} {ps[1]}",
                    Operation.Call => $"CALL {ps[0]}",
                    Operation.Ret => "RET",
                    Operation.Out => $"OUT {ps[0]}",
                    Operation.In => $"IN {ps[0]}",
                    Operation.Noop => "NOOP",
                    _ => throw new Exception(),
                });
                disassembly.Append('\n');

                pointer += operation.OperationLength();
            }

            return disassembly.ToString();
        }
    }
}
