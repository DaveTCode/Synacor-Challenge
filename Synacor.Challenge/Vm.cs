using Serilog.Core;
using System.Collections;

namespace Synacor.Challenge;

internal enum VmState
{
    Running,
    Halted,
    Error,
}


internal class Vm : IEnumerable<(VmState, int, int[])>
{
    private int _instructionPointer;

    private readonly int[] _registers = new int[8];

    private readonly Stack<int> _stack = new();

    private readonly int[] _memory = new int[32768];

    private readonly Queue<char> _inputBuffer = new();
    
    private readonly Action<char> _consoleWrite;

    private readonly Logger _debug;

    internal Vm(byte[] binary, Logger debug, Action<char> consoleWrite, List<(int, int)>? patches=null)
    {
        if (binary == null) throw new ArgumentNullException(nameof(binary));
        if (binary.Length > _memory.Length * 2) throw new ArgumentException("Binary must not be more than 2x size of memory", nameof(binary));
        _debug = debug ?? throw new ArgumentNullException(nameof(debug));

        for (var ii = 0; ii < binary.Length; ii += 2)
        {
            _memory[ii / 2] = binary[ii] | (binary[ii + 1] << 8);
        }
        _consoleWrite = consoleWrite;

        if (patches != null)
        {
            foreach (var (add, val) in patches)
            {
                _memory[add] = val;
            }
        }
    }

    private (VmState, int, int[]) Step()
    {
        var op = (Operation)_memory[_instructionPointer];
        var a = _memory[(_instructionPointer + 1) & 0x7FFF];
        var b = _memory[(_instructionPointer + 2) & 0x7FFF];
        var c = _memory[(_instructionPointer + 3) & 0x7FFF];
        _debug.Debug("IP: {0:X4} {1}", _instructionPointer, op.DebugLog(a, b, c));

        _instructionPointer = (_instructionPointer + op.OperationLength()) & 0x7FFF;

        switch (op)
        {
            case Operation.Halt:
                return (VmState.Halted, _instructionPointer, _registers);
            case Operation.Set:
                Write(a, Read(b));
                break;
            case Operation.Push:
                _stack.Push(Read(a));
                break;
            case Operation.Pop:
                if (_stack.TryPop(out var r))
                {
                    Write(a, r);
                }
                else
                {
                    return (VmState.Error, _instructionPointer, _registers);
                }
                break;
            case Operation.Eq:
                Write(a, (Read(b) == Read(c)) ? 1 : 0);
                break;
            case Operation.Gt:
                Write(a, (Read(b) > Read(c)) ? 1 : 0);
                break;
            case Operation.Jmp:
                _instructionPointer = a;
                break;
            case Operation.Jt:
                if (Read(a) != 0) _instructionPointer = b;
                break;
            case Operation.Jf:
                if (Read(a) == 0) _instructionPointer = b;
                break;
            case Operation.Add:
                Write(a, (Read(b) + Read(c)) & 0x7FFF);
                break;
            case Operation.Mult:
                Write(a, (Read(b) * Read(c)) & 0x7FFF);
                break;
            case Operation.Mod:
                Write(a, Read(b) % Read(c));
                break;
            case Operation.And:
                Write(a, Read(b) & Read(c));
                break;
            case Operation.Or:
                Write(a, Read(b) | Read(c));
                break;
            case Operation.Not:
                Write(a, (~Read(b)) & 0x7FFF);
                break;
            case Operation.Rmem:
                Write(a, _memory[Read(b)]);
                break;
            case Operation.Wmem:
                Write(Read(a), Read(b));
                break;
            case Operation.Call:
                _stack.Push(_instructionPointer);
                _instructionPointer = Read(a);
                break;
            case Operation.Ret:
                if (!_stack.TryPop(out _instructionPointer))
                {
                    return (VmState.Halted, _instructionPointer, _registers);
                }
                break;
            case Operation.Out:
                _consoleWrite((char)Read(a));
                break;
            case Operation.In:
                if (_inputBuffer.Count > 0)
                {
                    var chr = _inputBuffer.Dequeue();
                    if (chr == '$')
                    {
                        _registers[7] = 25734; // Code calculated as ack(4, 1, r7) = 6 (mod 2^15)
                    }
                    else
                    {
                        Write(a, chr);
                    }
                }
                else
                {
                    var line = Console.ReadLine();
                    if (line == null) throw new Exception("How is input empty?");

                    foreach (var chr in line.ToCharArray())
                    {
                        _inputBuffer.Enqueue(chr);
                    }
                    _inputBuffer.Enqueue('\n');
                    Write(a, _inputBuffer.Dequeue());
                }
                break;
            case Operation.Noop:
                break;
            default:
                throw new NotImplementedException();
        }

        return (VmState.Running, _instructionPointer, _registers);
    }

    internal void LoadInputs(string inputs)
    {
        foreach (var chr in inputs.ToCharArray().Where(c => c != '\r'))
        {
            _inputBuffer.Enqueue(chr);
        }
    }

    private void Write(int address, int value)
    {
        if (address < 0) throw new ArgumentOutOfRangeException(nameof(address));

        if (address < _memory.Length)
        {
            _memory[address] = value;
        }
        else if (address < 32775)
        {
            _registers[address - 32768] = value;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(address));
        }
    }

    private int Read(int address) => address switch
    {
        var x when x < 0 => throw new ArgumentOutOfRangeException(nameof(address)),
        var x when x < _memory.Length => x,
        var x when x <= 32775 => _registers[address - 32768],
        _ => throw new ArgumentOutOfRangeException(nameof(address)),
    };

    public IEnumerator<(VmState, int, int[])> GetEnumerator()
    {
        while (true)
        {
            yield return Step();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return $"IP: {_instructionPointer:X4}";
    }
}