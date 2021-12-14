namespace Synacor.Challenge;

public enum Operation
{
    Halt,
    Set,
    Push,
    Pop,
    Eq,
    Gt,
    Jmp,
    Jt,
    Jf,
    Add,
    Mult,
    Mod,
    And,
    Or,
    Not,
    Rmem,
    Wmem,
    Call,
    Ret,
    Out,
    In,
    Noop,
}

public static class OperationExtensions
{
    public static int OperationLength(this Operation operation) => operation switch
    {
        Operation.Halt => 1,
        Operation.Set => 3,
        Operation.Push => 2,
        Operation.Pop => 2,
        Operation.Eq => 4,
        Operation.Gt => 4,
        Operation.Jmp => 2,
        Operation.Jt => 3,
        Operation.Jf => 3,
        Operation.Add => 4,
        Operation.Mult => 4,
        Operation.Mod => 4,
        Operation.And => 4,
        Operation.Or => 4,
        Operation.Not => 3,
        Operation.Rmem => 3,
        Operation.Wmem => 3,
        Operation.Call => 2,
        Operation.Ret => 1,
        Operation.Out => 2,
        Operation.In => 2,
        Operation.Noop => 1,
        _ => throw new NotImplementedException()
    };

    public static string DebugLog(this Operation operation, int a, int b, int c) => operation switch
    {
        Operation.Halt => "HLT",
        Operation.Set => $"SET <{a:X4}>,<{b:X4}>",
        Operation.Push => $"PUSH <{a:X4}>",
        Operation.Pop => $"POP <{a:X4}>",
        Operation.Eq => $"EQ <{a:X4}>,<{b:X4}>,<{c:X4}>",
        Operation.Gt => $"GT <{a:X4}>,<{b:X4}>,<{c:X4}>",
        Operation.Jmp => $"JMP <{a:X4}>",
        Operation.Jt => $"JT <{a:X4}>,<{b:X4}>",
        Operation.Jf => $"JF <{a:X4}>,<{b:X4}>",
        Operation.Add => $"ADD <{a:X4}>,<{b:X4}>,<{c:X4}>",
        Operation.Mult => $"MULT<{a:X4}>,<{b:X4}>,<{c:X4}>",
        Operation.Mod => $"MOD <{a:X4}>,<{b:X4}>,<{c:X4}>",
        Operation.And => $"AND <{a:X4}>,<{b:X4}>,<{c:X4}>",
        Operation.Or => $"OR <{a:X4}>,<{b:X4}>,<{c:X4}>",
        Operation.Not => $"NOT <{a:X4}>,<{b:X4}>",
        Operation.Rmem => $"RMEM <{a:X4}>,<{b:X4}>",
        Operation.Wmem => $"WMEM <{a:X4}>,<{b:X4}>",
        Operation.Call => $"CALL <{a:X4}>",
        Operation.Ret => "RET",
        Operation.Out => $"OUT <{a:X4}>",
        Operation.In => $"IN <{a:X4}>",
        Operation.Noop => "NOOP",
        _ => throw new NotImplementedException()
    };
}