using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simpro.Expr
{
    internal class AnOperator
    {
        //public OPERATOR_NAME op_name;
        public OPERATOR_TYPE op_type;
        public int precedence;
        public Type required1stType;
        public Type required2ndType;
        public RequiredOperandType requiredOperandType;
        public Delegate exp_call;
        public string overload_name;    // http://msdn.microsoft.com/en-us/library/2sk3x8a7.aspx

        public AnOperator(OPERATOR_TYPE op_type, int precedence) : this(op_type, precedence, null, null, RequiredOperandType.NONE, null, null) { }
        public AnOperator(OPERATOR_TYPE op_type, int precedence, Delegate exp_call, string overload_name) : this(op_type, precedence, null, null, RequiredOperandType.NONE, exp_call, overload_name) { }
        public AnOperator(OPERATOR_TYPE op_type, int precedence, Type required1stType, Type required2ndType, RequiredOperandType requiredOperandType, Delegate exp_call, string overload_name)
        {
            //this.op_name = op_name;
            this.op_type = op_type;
            this.precedence = precedence;
            this.required1stType = required1stType;
            this.required2ndType = required2ndType;
            this.requiredOperandType = requiredOperandType;
            this.exp_call = exp_call;
            this.overload_name = overload_name;
        }

        internal static Dictionary<string, AnOperator> dicSystemOperator = GetSystemOperators();
        internal static Dictionary<string, AnOperator> GetSystemOperators()
        {
            Dictionary<string, AnOperator> dic = new Dictionary<string, AnOperator>();
            // special
            dic.Add(";", new AnOperator(OPERATOR_TYPE.PRIMARY, 999)); // Semicolon,   // end of statement; lowest priority
            dic.Add(",", new AnOperator(OPERATOR_TYPE.PRIMARY, 990));  // "op_Comma")); // end of parameter. ex. x,y
            dic.Add("[", new AnOperator(OPERATOR_TYPE.OPEN, 0, null, "get_Item")); // BracketLeft,     //a[x]	0.ArrayAccess; ArrayIndex; ArrayLength
            dic.Add("]", new AnOperator(OPERATOR_TYPE.CLOSE, 0)); // BracketRight,     //a[x]	0.ArrayAccess; ArrayIndex; ArrayLength
            dic.Add("(", new AnOperator(OPERATOR_TYPE.OPEN, 0)); // ParenthesisLeft,    //f(x) 0.Call; (T)x	10.??; () 200.??
            dic.Add(")", new AnOperator(OPERATOR_TYPE.CLOSE, 0)); // ParenthesisRight,    //f(x) 0.Call; (T)x	10.??; () 200.??
            dic.Add("{", new AnOperator(OPERATOR_TYPE.OPEN, 0)); // BlockLeft,      // { block; }
            dic.Add("}", new AnOperator(OPERATOR_TYPE.CLOSE, 0)); // BlockRight,      // { block; }
            dic.Add(".", new AnOperator(OPERATOR_TYPE.PRIMARY, 0)); // Dot,                           //x.y	0.FieldOrProperty; x.y(); x.y[]
            dic.Add("new", new AnOperator(OPERATOR_TYPE.PRIMARY, 0));    // new
            dic.Add("typeof", new AnOperator(OPERATOR_TYPE.PRIMARY, 0));    // TypeOf
            //dic.Add("as", new AnOperator(OPERATOR_TYPE.PRIMARY, 0)); // var as T : Expression.TypeAs(exp, Type)
            dic.Add("sizeof", new AnOperator(OPERATOR_TYPE.PRIMARY, 1));   // SizeOf, it should be PREFIX_UNARY

            dic.Add("=>", new AnOperator(OPERATOR_TYPE.PRIMARY, 0));  // LINQ goes to
            //[Unary] : U - unary operator, xU-prefix operator, only available when there is no previous operand; 
            dic.Add("(T)", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 1));   // Cast
            dic.Add("+U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.UnaryPlus, "op_UnaryPlus")); // UnaryPlus,                     //+x		10.UnaryPlus			// Unary
            dic.Add("-U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.Negate, "op_UnaryNegation")); // UnaryMinus,                    //-x		10.Negate				// Unary
            dic.Add("!U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.Not, null)); // UnaryLogicalNot,               //!		10.Not					// Unary
            dic.Add("~U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.Not, "op_OnesComplement")); // UnaryBitNot,                   //~		10.Not					// Unary
            dic.Add("--", new AnOperator(OPERATOR_TYPE.POST_UNARY, 0, (d1m)Expression.PostDecrementAssign, "op_Decrement")); // PostDecrementAssign,    ////x--		0.PostDecrementAssign	// Unary
            dic.Add("++", new AnOperator(OPERATOR_TYPE.POST_UNARY, 0, (d1m)Expression.PostIncrementAssign, "op_Increment")); // PostIncrementAssign,    ////x++		0.PostIncrementAssign	// Unary
            dic.Add("--U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.PreDecrementAssign, null)); // PreDecrementAssign,     ////--x		10.PreDecrementAssign   // Unary
            dic.Add("++U", new AnOperator(OPERATOR_TYPE.PREFIX_UNARY, 10, (d1m)Expression.PreIncrementAssign, null)); // PreIncrementAssign,     ////++x		10.PreIncrementAssign   // Unary
            //[Binary]
            dic.Add("**", new AnOperator(OPERATOR_TYPE.BINARY, 15, typeof(double), typeof(double), RequiredOperandType.NONE, (d2)Expression.Power, null)); // Power,     //x**y	15.Power
            dic.Add("*", new AnOperator(OPERATOR_TYPE.BINARY, 20, null, null, RequiredOperandType.SAME, (d2m)Expression.Multiply, "op_Multiply")); // Multiply,     //x*y		20.Multiply
            dic.Add("/", new AnOperator(OPERATOR_TYPE.BINARY, 20, null, null, RequiredOperandType.SAME, (d2m)Expression.Divide, "op_Division")); // Divide,    //x/y		20.Divide
            dic.Add("%", new AnOperator(OPERATOR_TYPE.BINARY, 20, null, null, RequiredOperandType.SAME, (d2m)Expression.Modulo, "op_Modulus")); // Modulo,      //x%y		20.Modulo(Reminder)
            dic.Add("+", new AnOperator(OPERATOR_TYPE.BINARY, 30, null, null, RequiredOperandType.SAME, (d2m)Expression.Add, "op_Addition")); // Add,   //x+y		30.Add
            dic.Add("-", new AnOperator(OPERATOR_TYPE.BINARY, 30, null, null, RequiredOperandType.SAME, (d2m)Expression.Subtract, "op_Subtraction")); // Subtract,  //x-y		30.Subtract
            dic.Add("<<", new AnOperator(OPERATOR_TYPE.BINARY, 40, null, typeof(int), RequiredOperandType.NONE, (d2m)Expression.LeftShift, "op_LeftShift")); // LeftShift,   //x<<y	40.LeftShift
            dic.Add(">>", new AnOperator(OPERATOR_TYPE.BINARY, 40, null, typeof(int), RequiredOperandType.NONE, (d2m)Expression.RightShift, "op_RightShift")); // RightShift,    //x>>y	40.RightShift
            dic.Add(">", new AnOperator(OPERATOR_TYPE.BINARY, 50, null, null, RequiredOperandType.SAME, (d2bm)Expression.GreaterThan, "op_GreaterThan")); // GreaterThan,   //x>y		50.GreaterThan
            dic.Add(">=", new AnOperator(OPERATOR_TYPE.BINARY, 50, null, null, RequiredOperandType.SAME, (d2bm)Expression.GreaterThanOrEqual, "op_GreaterThanOrEqual")); // GreaterThanOrEqual,    //x>=y	50.GreaterThanOrEqual
            dic.Add("<", new AnOperator(OPERATOR_TYPE.BINARY, 50, null, null, RequiredOperandType.SAME, (d2bm)Expression.LessThan, "op_LessThan")); // LessThan,  //x<y		50.LessThan
            dic.Add("<=", new AnOperator(OPERATOR_TYPE.BINARY, 50, null, null, RequiredOperandType.SAME, (d2bm)Expression.LessThanOrEqual, "op_LessThanOrEqual")); // LessThanOrEqual,   //x<=y	50.LessThanOrEqual
            dic.Add("is", new AnOperator(OPERATOR_TYPE.BINARY, 50)); // is,   //x is T	50.TypeIs
            dic.Add("as", new AnOperator(OPERATOR_TYPE.BINARY, 50)); // as,   //x as T	50.TypeAs
            dic.Add("==", new AnOperator(OPERATOR_TYPE.BINARY, 60, null, null, RequiredOperandType.SAME, (d2bm)Expression.Equal, "op_Equality")); // Equal, //x==y	30.Equal
            dic.Add("!=", new AnOperator(OPERATOR_TYPE.BINARY, 60, null, null, RequiredOperandType.SAME, (d2bm)Expression.NotEqual, "op_Inequality")); // NotEqual,  //x!=y	30.NotEqual
            dic.Add("&", new AnOperator(OPERATOR_TYPE.BINARY, 70, null, null, RequiredOperandType.SAME, (d2m)Expression.And, "op_BitwiseAnd")); // BitAnd,   //x&y		30.And
            dic.Add("^", new AnOperator(OPERATOR_TYPE.BINARY, 72, null, null, RequiredOperandType.SAME, (d2m)Expression.ExclusiveOr, "op_ExclusiveOr")); // BitExclusiveOr,    //x^y		80.ExclusiveOr(XOR)
            dic.Add("|", new AnOperator(OPERATOR_TYPE.BINARY, 74, null, null, RequiredOperandType.SAME, (d2m)Expression.Or, "op_BitwiseOr")); // BitOr, //x|y		90.Or
            dic.Add("&&", new AnOperator(OPERATOR_TYPE.BINARY, 80, null, null, RequiredOperandType.NONE, (d2m)Expression.AndAlso, "op_LogicalAnd")); // And,   //x&&y	100.AndAlso
            dic.Add("||", new AnOperator(OPERATOR_TYPE.BINARY, 85, null, null, RequiredOperandType.NONE, (d2m)Expression.OrElse, "op_LogicalOr")); // Or,    //x||y	110.OrElse
            dic.Add("??", new AnOperator(OPERATOR_TYPE.BINARY, 90, null, null, RequiredOperandType.NONE, (d2)Expression.Coalesce, null)); // Coalesce,  //x??y	120.Coalesce
            //[Conditional]
            dic.Add("?", new AnOperator(OPERATOR_TYPE.CONDITIONAL, 90)); // Condition, //  c?x:y	130.Condition
            dic.Add(":", new AnOperator(OPERATOR_TYPE.BINARY, 90)); // ConditionElse, // c?x:y	130.Condition
            //[Assignment]
            dic.Add("+=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.AddAssign, "op_AdditionAssignment")); // AddAssign, //x+=y	140.AddAssign
            dic.Add("-=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.SubtractAssign, "op_SubtractionAssignment")); // SubtractAssign,    //x-=y	140.SubtractAssign
            dic.Add("&=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.AddAssign, "op_BitwiseAndAssignment")); // BitAndAssign, //x&=y	140.AndAssign
            dic.Add("=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2)Expression.Assign, "op_Assign")); // Assign,    //x=y		140.Assign
            dic.Add("/=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.DivideAssign, "op_DivisionAssignment")); // DivideAssign,  //x/=y	140.DivideAssign
            dic.Add("^=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.ExclusiveOrAssign, "op_ExclusiveOrAssignment")); // BitExclusiveOrAssign,  //x^=y	140.ExclusiveOrAssign
            dic.Add("<<=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, typeof(int), RequiredOperandType.NONE, (d2m)Expression.LeftShiftAssign, "op_LeftShiftAssignment")); // LeftShiftAssign,   //x<<=y	140.LeftShiftAssign
            dic.Add(">>=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, typeof(int), RequiredOperandType.NONE, (d2m)Expression.RightShiftAssign, "op_RightShiftAssignment")); // RightShiftAssign,  //x>>=y	140.RightShiftAssign
            dic.Add("%=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.ModuloAssign, "op_ModulusAssignment")); // ModuloAssign,  //x%=y	140.ModuloAssign
            dic.Add("*=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.MultiplyAssign, "op_MultiplicationAssignment")); // MultiplyAssign,    //x*=y	140.MultiplyAssign
            dic.Add("**=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.PowerAssign, null)); // PowserAssign,    //x**=y	140.PowerAssign
            dic.Add("|=", new AnOperator(OPERATOR_TYPE.ASSIGN, 100, null, null, RequiredOperandType.SAME, (d2m)Expression.OrAssign, "op_BitwiseOrAssignment")); // BitOrAssign   //x|=y	140.OrAssign
            return dic;
        }
    }

}
