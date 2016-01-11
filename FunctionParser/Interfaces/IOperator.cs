
namespace Markovcd.Classes
{
    public enum Associativity { None, Left, Right }

    public interface IOperator
    {
        int Precedence { get; }
        Associativity Associativity { get; }
    }
}
