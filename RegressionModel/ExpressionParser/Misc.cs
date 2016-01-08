using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Simpro.Expr
{ 
    internal enum TOKEN_TYPE { NONE, COMMENT, COMMENT_BLOCK, TEXT, INT, UINT, LONG, ULONG, FLOAT, DOUBLE, DECIMAL, BOOL, IDENTIFIER, OPERATOR }
    public enum OPERATOR_TYPE
    {
        UNKNOWN,
        OPEN,   // (,[,{
        CLOSE,  // ),],}
        PREFIX_UNARY,  // +,-,++,--
        POST_UNARY, // ++, --
        BINARY,  // +,-,*,/
        CONDITIONAL,    // (c)?x:y
        ASSIGN, // =, +=
        PRIMARY   // , ;
    }

    internal delegate Expression d1m(Expression exp, MethodInfo mi);
    internal delegate Expression d2(Expression exp1, Expression exp2);
    internal delegate Expression d2m(Expression exp1, Expression exp2, MethodInfo mi);
    internal delegate Expression d2bm(Expression exp1, Expression exp2, bool LiftToNull, MethodInfo mi);

    internal enum RequiredOperandType { NONE, SAME }

    public class ExprException : ApplicationException
    {
        public ExprException(string message) : base(message) { }
    }
}
