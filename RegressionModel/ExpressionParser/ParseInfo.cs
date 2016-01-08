using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simpro.Expr
{
    internal class ParseInfo
    {
        public LabelExpression returnLabel = null;
        public List<ParameterExpression> referredParameterList = new List<ParameterExpression>();
        public Stack<Dictionary<string, ParameterExpression>> localVariableStack = new Stack<Dictionary<string, ParameterExpression>>();   // local variables in every statement block
        public Stack<JumpInfo> jumpInfoStack = new Stack<JumpInfo>();

        public Expression GetReferredVariable(string var_name)
        {
            foreach (Dictionary<string, ParameterExpression> dic in localVariableStack)
                if (dic.ContainsKey(var_name)) return dic[var_name];
            return null;
        }
        public Expression GetReferredParameter(Type t, string name)
        {
            Expression exp = null;
            // Parameter reference
            foreach (ParameterExpression pe in referredParameterList)
            {
                if (pe.Type == t && pe.Name == name) { exp = pe; break; }
            }
            if (exp == null)
            {
                exp = Expression.Parameter(t, name);
                referredParameterList.Add((ParameterExpression)exp);
            }
            return exp;
        }
    }
}
