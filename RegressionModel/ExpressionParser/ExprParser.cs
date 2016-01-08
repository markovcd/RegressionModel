using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;


/* Supported by Simpro.Expr but not lambda
 *      - "**" power operator
 *      - 'xxxx' same with "xxxx"
 *      - implicit conversion for string add with non-string object: s+num => s+num.ToString()
 *      - implicit conversion for string compare with non-string object : s>num => s>num.ToString()
 * */
namespace Simpro.Expr
{
    
    /// <summary>
    /// ExprParser has 3 naming scope
    ///     Global : available for all Parser instance.
    ///     Parser : available for current Parser instance.
    ///     Inline : available for current expression.
    ///     
    /// User can create multiple parser instance for different naming scope
    /// One parser can create multiple expression with the same naming scope
    /// One expression can define in-line namespace and parameter only for current expression scope
    /// </summary>
    public class ExprParser
    {
        #region namespace, parameter and type: case sensitive

        internal static Dictionary<string, Type> SystemType = SetSystemType();
        private static Dictionary<string, Type> SetSystemType()
        {
            //  system value types : http://msdn.microsoft.com/en-us/library/s1ax56ch.aspx
            Dictionary<string, Type> dic = new Dictionary<string, Type>();
            dic.Add("bool", typeof(bool));
            dic.Add("char", typeof(char));
            dic.Add("string", typeof(string));
            dic.Add("byte", typeof(byte));
            dic.Add("sbyte", typeof(sbyte));
            dic.Add("short", typeof(short));
            dic.Add("ushort", typeof(ushort));
            dic.Add("int", typeof(int));
            dic.Add("uint", typeof(uint));
            dic.Add("long", typeof(long));
            dic.Add("ulong", typeof(ulong));
            dic.Add("float", typeof(float));
            dic.Add("double", typeof(double));
            dic.Add("decimal", typeof(decimal));
            dic.Add("enum", typeof(Enum));
            dic.Add("object", typeof(object));
            return dic;
        }
        internal static Dictionary<Type, Type> NullableType = SetNullableType();
        private static Dictionary<Type, Type> SetNullableType()
        {
            Dictionary<Type, Type> dic = new Dictionary<Type, Type>();
            dic.Add(typeof(bool), typeof(bool?));
            dic.Add(typeof(char), typeof(char?));
            dic.Add(typeof(byte), typeof(byte?));
            dic.Add(typeof(sbyte), typeof(sbyte?));
            dic.Add(typeof(short), typeof(short?));
            dic.Add(typeof(ushort), typeof(ushort?));
            dic.Add(typeof(int), typeof(int?));
            dic.Add(typeof(uint), typeof(uint?));
            dic.Add(typeof(long), typeof(long?));
            dic.Add(typeof(ulong), typeof(ulong?));
            dic.Add(typeof(float), typeof(float?));
            dic.Add(typeof(double), typeof(double?));
            dic.Add(typeof(decimal), typeof(decimal?));
            return dic;
        }


        // namespace for static data type
        public static Using Using = new Using();
        internal Assembly caller_assembly = null;
        
        // parameter type
        public static Dictionary<string, Type> GlobalParameterType = new Dictionary<string, Type>();    // Global scope
        public Dictionary<string, Type> ParameterType = new Dictionary<string,Type>();                  // Parser scope
        private Dictionary<string, Type> inlineParameterType = null;                                    // Expr scope
        // parameter value
        public static Dictionary<string, object> GlobalParameterValue = new Dictionary<string, object>();   // Global scope
        public Dictionary<string, object> ParameterValue = new Dictionary<string, object>();                // Parser scope


        /// <summary>
        /// check by order:
        ///     - System Type               // user type cannot be the same with system type. check system type first is for performance
        ///     - Type.GetType(type_name)
        ///     - Type from Local Namespace
        ///     - Type from Globel Namespace
        /// limitation:
        ///     Type.GetType use AssmblyQulifiedName: "ns.type, ns". not include version info, "type" part not include "."
        /// </summary>
        /// <param name="name">type or instance object name</param>
        /// <returns></returns>
        internal Type QueryStaticType(string type_name)
        {
            // check with dicSystemType
            if (SystemType.ContainsKey(type_name))
                return SystemType[type_name];

            // check type_name itself
            Type t = null;
            if ((t = Type.GetType(type_name)) != null) return t;    // check System(mscorlib) assembly
            if (caller_assembly != null && (t = caller_assembly.GetType(type_name)) != null) return t;  // check caller assembly

            foreach (string ns in Using.namespace_list.Keys)
            {
                string full_type_name = ns + "." + type_name;
                if (Using.namespace_list[ns] == null)
                {
                    if ((t = Type.GetType(full_type_name)) != null) return t;    // check System(mscorlib) assembly
                    if (caller_assembly != null && (t = caller_assembly.GetType(full_type_name)) != null) return t;  // check caller assembly
                    if ((t = Type.GetType(full_type_name + ", " + ns)) != null) return t;   // check namespace as assembly name
                }
                else if (Using.namespace_list[ns] != null && (t = Using.namespace_list[ns].GetType(full_type_name)) != null) return t;
            }

            if (type_name.Contains(".") && (t = Type.GetType(type_name + ", " + type_name.Substring(0, type_name.LastIndexOf('.')))) != null)
                return t;

            //// check with Namespace
            //if (this.Namespace != null)
            //{
            //    foreach (string ns in this.Namespace)
            //        if ((t = Type.GetType(ns + "." + type_name)) != null
            //            || (t = Type.GetType(ns + "." + type_name + ", " + ns)) != null) return t;
            //}
            //// check with GlobalNamespace
            //if (GlobalNamespace != null)
            //{
            //    foreach (string ns in GlobalNamespace)
            //        if ((t = Type.GetType(ns + "." + type_name)) != null
            //            || (t = Type.GetType(ns + "." + type_name + ", " + ns)) != null) return t;
            //}
            //// check with localNamespace
            //if (inlineNamespace != null)
            //{
            //    foreach (string ns in inlineNamespace)
            //        if ((t = Type.GetType(ns + "." + type_name)) != null
            //            || (t = Type.GetType(ns + "." + type_name + ", " + ns)) != null) return t;
            //}
            return null;
        }

        /// <summary>
        /// check by order
        ///     Parameter Type
        ///     Globel Parameter Type
        ///     queryParameterValue(parameter_name)
        /// </summary>
        /// <param name="parameter_name"></param>
        /// <returns></returns>
        internal Type QueryParameterType(string parameter_name)
        {
            if (ParameterType != null && ParameterType.ContainsKey(parameter_name))
                return ParameterType[parameter_name];
            else if (GlobalParameterType.ContainsKey(parameter_name))
                return GlobalParameterType[parameter_name];
            else if (inlineParameterType != null && inlineParameterType.ContainsKey(parameter_name))
                return inlineParameterType[parameter_name];
            return null;
        }


        // implicit conversion
        internal static Dictionary<Type, List<Type>> dicImplicitConversion = SetImplicitConversionMap();
        internal static Dictionary<Type, List<Type>> SetImplicitConversionMap()
        {
            Dictionary<Type, List<Type>> dic = new Dictionary<Type, List<Type>>();
            dic.Add(typeof(char), new List<Type>(new Type[] { typeof(char), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(char?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(byte), new List<Type>(new Type[] { typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(byte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(sbyte), new List<Type>(new Type[] { typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(sbyte?), typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(short), new List<Type>(new Type[] { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(ushort), new List<Type>(new Type[] { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(int), new List<Type>(new Type[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(uint), new List<Type>(new Type[] { typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(long), new List<Type>(new Type[] { typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(ulong), new List<Type>(new Type[] { typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(float), new List<Type>(new Type[] { typeof(float), typeof(double), typeof(decimal), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(double), new List<Type>(new Type[] { typeof(double), typeof(double?) }));
            dic.Add(typeof(decimal), new List<Type>(new Type[] { typeof(decimal), typeof(decimal?) }));

            dic.Add(typeof(char?), new List<Type>(new Type[] { typeof(char?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(byte?), new List<Type>(new Type[] { typeof(byte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(sbyte?), new List<Type>(new Type[] { typeof(sbyte?), typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(short?), new List<Type>(new Type[] { typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(ushort?), new List<Type>(new Type[] { typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(int?), new List<Type>(new Type[] { typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(uint?), new List<Type>(new Type[] { typeof(uint?), typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(long?), new List<Type>(new Type[] { typeof(long?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(ulong?), new List<Type>(new Type[] { typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(float?), new List<Type>(new Type[] { typeof(float?), typeof(double?), typeof(decimal?) }));
            dic.Add(typeof(double?), new List<Type>(new Type[] { typeof(double?) }));
            dic.Add(typeof(decimal?), new List<Type>(new Type[] { typeof(decimal?) }));

            return dic;
        }
        internal Type QueryImplicitConversionType(Expression ex1, Expression ex2)
        {
            if (dicImplicitConversion.ContainsKey(ex1.Type) && dicImplicitConversion.ContainsKey(ex2.Type))
            {
                foreach (Type t in dicImplicitConversion[ex1.Type])
                    if (dicImplicitConversion[ex2.Type].Contains(t)) return t;
            }
            if (ex1 is ConstantExpression && ((ConstantExpression)ex1).Value == null && !ex2.Type.IsValueType) return ex2.Type;
            if (ex2 is ConstantExpression && ((ConstantExpression)ex2).Value == null && !ex1.Type.IsValueType) return ex1.Type;
            return null;
        }

        #endregion parameter and type

        public LambdaExpression Parse(string expression_string)
        {
            caller_assembly = Assembly.GetCallingAssembly();       // need to reset by every parsing
            inlineParameterType = new Dictionary<string, Type>();   // need to reset by every parsing
            TokenStore declare_ts;
            ParseInfo parseInfo = new ParseInfo();

            

            TokenStore ts = TokenStore.Parse(expression_string, out declare_ts);
            if (declare_ts != null)
            {
                //ParseDeclaration(declare_ts);
                parseInfo.localVariableStack.Push(new Dictionary<string, ParameterExpression>());
                Assert(ParseNextExpression(declare_ts, null, parseInfo) == null, "Incorrect parameter declaration.");
                var lv = parseInfo.localVariableStack.Pop();
                foreach (string name in lv.Keys)
                    inlineParameterType.Add(name, lv[name].Type);
            }
            return Expression.Lambda(ParseStatementBlock(ts, parseInfo, false), parseInfo.referredParameterList.ToArray());
        }

        internal Expression ParseStatementBlock(TokenStore ts, ParseInfo parseInfo, bool isVoid)
        {
            List<Expression> expr_list = new List<Expression>();
            parseInfo.localVariableStack.Push(new Dictionary<string, ParameterExpression>());
            //AToken tok;
            //Expression test, exp1, exp2, statement, statement2;
            if (ts != null)
                while (ts.Current != null)
                {
                    Expression statement = ParseNextStatement(ts, parseInfo);
                    if (statement != null) expr_list.Add(statement);
                }
            if (isVoid && expr_list.Count > 0 && expr_list[expr_list.Count - 1].Type != typeof(void)) expr_list.Add(Expression.Empty());
            var localVariable = parseInfo.localVariableStack.Pop();
            if (parseInfo.localVariableStack.Count == 0 && parseInfo.returnLabel != null) // this is the initial block, add return label if has been referred inside the block.
                expr_list.Add(parseInfo.returnLabel);
            if (expr_list.Count == 0) return Expression.Empty();    //.Constant(null);
            else if (expr_list.Count == 1 && localVariable.Count==0) return expr_list[0];
            else
                return Expression.Block(localVariable.Values, expr_list.ToArray());
        }
        internal Expression ParseNextStatement(TokenStore ts, ParseInfo parseInfo)
        {
            Expression test, exp1, exp2, statement = null, statement2;
            AToken tok = ts.Current;
            if (tok.tok_type == TOKEN_TYPE.IDENTIFIER)
            {
                switch (tok.value)
                {
                    case "if":
                        #region if(test) statement else statement2
                        Assert((tok = ts.Next()) != null && tok.value == "(", tok);
                        Assert((test = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null && ts.Current != null, ts.Current);
                        statement = ParseNextStatement(ts, parseInfo);
                        //if (ts.CurrentValue == "{")
                        //    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                        //else
                        //{
                        //    statement = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        //    Assert(ts.CurrentValue == ";", ts.Current); ts.Next();
                        //}
                        if (statement == null) statement = Expression.Empty();
                        if (ts.CurrentValue == "else")
                        {
                            ts.Next();  // skip "else"
                            statement2 = ParseNextStatement(ts, parseInfo);
                            //if (ts.CurrentValue == "{")
                            //    statement2 = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                            //else
                            //    statement2 = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                            if (statement2 == null) statement2 = Expression.Empty();
                            statement = Expression.IfThenElse(test, statement, statement2);
                        }
                        else
                            statement = Expression.IfThen(test, statement);
                        break;
                        #endregion
                    case "while":
                        #region while(test) statement : loop { if(test) statement; continueLabel; } breakLabel;
                        parseInfo.jumpInfoStack.Push(new JumpInfo());
                        Assert((tok = ts.Next()) != null && tok.value == "(", tok); // skip "while"
                        Assert((test = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null && ts.Current != null, ts.Current);
                        statement = ParseNextStatement(ts, parseInfo);
                        //if (ts.CurrentValue == "{")
                        //    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                        //else
                        //    statement = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (statement == null) statement = Expression.Empty();
                        JumpInfo jumpInfo = parseInfo.jumpInfoStack.Pop();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());
                        statement = Expression.Loop(Expression.IfThenElse(test, statement, Expression.Break(jumpInfo.breakLabel.Target)), jumpInfo.breakLabel.Target, (jumpInfo.continueLabel == null) ? null : jumpInfo.continueLabel.Target);
                        break;
                        #endregion
                    case "do":
                        #region do statement while(test) : loop { statement; if(test) break; continueLabel } breakLabel
                        ts.Next(); // skip "do"
                        parseInfo.jumpInfoStack.Push(new JumpInfo());
                        statement = ParseNextStatement(ts, parseInfo);
                        //if (ts.CurrentValue == "{")
                        //    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                        //else
                        //    statement = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (statement == null) statement = Expression.Empty();
                        Assert(ts.CurrentValue == "while", ts.Current);
                        Assert((tok = ts.Next()) != null && tok.value == "(", tok); // skip "while"
                        Assert((test = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null, ts.Current);
                        //Assert(ts.CurrentValue == ";", ts.Current); ts.Next();  // skip ";"
                        jumpInfo = parseInfo.jumpInfoStack.Pop();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());
                        statement = Expression.Loop(Expression.Block(statement, Expression.IfThenElse(test, Expression.Empty(), Expression.Break(jumpInfo.breakLabel.Target))), jumpInfo.breakLabel.Target, (jumpInfo.continueLabel == null) ? null : jumpInfo.continueLabel.Target);
                        break;
                        #endregion
                    case "for":
                        #region for(exp1,test,exp2) statement : exp1; loop{if(test) statement; continuelabel;exp2;} breaklabel;
                        parseInfo.localVariableStack.Push(new Dictionary<string, ParameterExpression>());   // exp1 can contain local variable only for "for statement"
                        parseInfo.jumpInfoStack.Push(new JumpInfo());
                        Assert((tok = ts.Next()) != null && tok.value == "(", tok); // skip "for"
                        TokenStore sub_ts = ts.NextUntilOperatorClose();
                        exp1 = ParseNextExpression(sub_ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (exp1 == null) exp1 = Expression.Empty();
                        Assert(sub_ts.CurrentValue == ";", sub_ts.Current);
                        sub_ts.Next();  // skip ";"
                        test = ParseNextExpression(sub_ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        Assert(sub_ts.CurrentValue == ";", sub_ts.Current);
                        sub_ts.Next();  // skip ";"
                        exp2 = ParseNextExpression(sub_ts, null, parseInfo);

                        statement = ParseNextStatement(ts, parseInfo);
                        //if (ts.CurrentValue == "{")
                        //    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                        //else
                        //    statement = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (statement == null) statement = Expression.Empty();
                        var lv = parseInfo.localVariableStack.Pop();
                        jumpInfo = parseInfo.jumpInfoStack.Pop();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());

                        //if (exp1 != null) expr_list.Add(exp1);
                        List<Expression> sub_exp_list = new List<Expression>();
                        if (test != null) statement = Expression.IfThenElse(test, statement, Expression.Break(jumpInfo.breakLabel.Target));
                        sub_exp_list.Add(statement);
                        if (jumpInfo.continueLabel != null) sub_exp_list.Add(jumpInfo.continueLabel);
                        if (exp2 != null) sub_exp_list.Add(exp2);
                        statement = Expression.Loop(Expression.Block(sub_exp_list), jumpInfo.breakLabel.Target);
                        //expr_list.Add(jumpInfo.breakLabel);
                        statement = Expression.Block(lv.Values, exp1, statement);
                        break;
                        #endregion
                    case "foreach":
                        #region foreach(exp1 var in exp2) statement : IEnumerator ie=exp2.GetEnumerator(); Loop { if(ie.MoveNext()) {var=ie.Current; statement;} continueLabel } breakLabel

                        // begin stack
                        lv = new Dictionary<string, ParameterExpression>();
                        parseInfo.localVariableStack.Push(lv);   // exp1 can contain local variable only for "for statement"
                        parseInfo.jumpInfoStack.Push(new JumpInfo());
                        Assert(ts.Next() != null && ts.CurrentValue == "(", ts.Current);  // skip "foreach" to "("
                        sub_ts = ts.NextUntilOperatorClose();
                        // parse T
                        Assert((exp1 = ParseReferredParameterOrType(sub_ts, parseInfo, true, false, true)) != null && exp1 is ConstantExpression && ((ConstantExpression)exp1).Value is Type, sub_ts.Current);
                        Type t = (Type)((ConstantExpression)exp1).Value;
                        // parse var
                        Assert(sub_ts.Current != null && sub_ts.Current.tok_type == TOKEN_TYPE.IDENTIFIER, sub_ts.Current);    // to var
                        ParameterExpression var = Expression.Variable(t, sub_ts.Current.value);
                        parseInfo.localVariableStack.Peek().Add(sub_ts.Current.value, var);
                        // parse exp - IEnumerator
                        Assert((tok = sub_ts.Next()) != null && tok.value == "in", tok);
                        sub_ts.Next();  // skip "in"
                        Assert((exp2 = ParseNextExpression(sub_ts, null, parseInfo)) != null, (AToken)null);
                        // organize expressions
                        ParameterExpression ie = Expression.Variable(typeof(IEnumerator));
                        lv.Add("$$$$ie$$$$", ie);
                        exp1 = Expression.Assign(ie, Expression.Call(exp2, "GetEnumerator", null, null));   // IEnumerator ie=exp2.GetEnumerator();
                        test = Expression.Call(ie, "MoveNext", null, null);     // ie.MoveNext()
                        exp2 = Expression.Assign(var, Expression.Convert(Expression.Property(ie, "Current"), var.Type));  // var=ie.Current;
                        // parse statement
                        statement = ParseNextStatement(ts, parseInfo);
                        //if (ts.CurrentValue == "{")
                        //    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo);
                        //else
                        //    statement = GetNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (statement == null) statement = Expression.Empty();
                        // end stack
                        lv = parseInfo.localVariableStack.Pop();
                        jumpInfo = parseInfo.jumpInfoStack.Pop();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());
                        // create foreach
                        //expr_list.Add(exp1);
                        statement = Expression.Loop(Expression.IfThenElse(test, Expression.Block(exp2, statement), Expression.Break(jumpInfo.breakLabel.Target)), jumpInfo.breakLabel.Target, jumpInfo.continueLabel == null ? null : jumpInfo.continueLabel.Target);
                        statement = Expression.Block(lv.Values, exp1, statement);
                        break;
                        #endregion
                    case "break":
                        #region break;
                        ts.Next();  // skip "break"
                        jumpInfo = parseInfo.jumpInfoStack.Peek();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());
                        statement = Expression.Break(jumpInfo.breakLabel.Target);
                        break;
                        #endregion
                    case "continue":
                        #region continue;
                        ts.Next();  // skip "continue"
                        jumpInfo = parseInfo.jumpInfoStack.Peek();
                        if (jumpInfo.continueLabel == null) jumpInfo.continueLabel = Expression.Label(Expression.Label());
                        statement = Expression.Continue(jumpInfo.continueLabel.Target);
                        break;
                        #endregion
                    case "switch":
                        #region switch(test) { case test_values: case_statements; default:default_statements; }

                        // start stack
                        parseInfo.localVariableStack.Push(new Dictionary<string, ParameterExpression>());
                        parseInfo.jumpInfoStack.Push(new JumpInfo());
                        // parse exp
                        Assert(ts.Next() != null && ts.CurrentValue == "(", ts.Current);  // skip "switch" to "("
                        Assert((test = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null, ts.Current);

                        // parse case list
                        Assert(ts.CurrentValue == "{", ts.Current);
                        sub_ts = ts.NextUntilOperatorClose();
                        List<SwitchCase> case_list = new List<SwitchCase>();
                        List<Expression> test_values = new List<Expression>();
                        List<Expression> case_statements = new List<Expression>();
                        Expression default_statements = null;
                        bool isDefault = false;
                        while ((tok = sub_ts.Current) != null)
                        {
                            if (tok.value == "case")
                            {
                                Assert(!isDefault, tok);
                                if (test_values.Count > 0 && case_statements.Count > 0)
                                {
                                    case_list.Add(Expression.SwitchCase(Expression.Block(case_statements), test_values));
                                    case_statements.Clear();
                                    test_values.Clear();
                                }
                                sub_ts.Next();  // skip "case"
                                test_values.Add(ParseNextExpression(sub_ts, AnOperator.dicSystemOperator[":"], parseInfo));
                                Assert(sub_ts.CurrentValue == ":" && sub_ts.Next() != null, sub_ts.Current);  // skip :
                            }
                            else if (tok.value == "default")
                            {
                                isDefault = true;
                                if (test_values.Count > 0 && case_statements.Count > 0)
                                {
                                    case_list.Add(Expression.SwitchCase(Expression.Block(case_statements), test_values));
                                    case_statements.Clear();
                                    test_values.Clear();
                                }
                                Assert(sub_ts.Next() != null && sub_ts.CurrentValue == ":" && sub_ts.Next() != null, sub_ts.Current);  // skip "default" and ":"
                            }
                            else
                            {
                                statement = ParseNextStatement(sub_ts, parseInfo);
                                if (statement != null) case_statements.Add(statement);
                            }
                            //else if (tok.value == "{")
                            //    case_statements.Add(ParseStatementBlock(sub_ts.NextUntilOperatorClose(), parseInfo));
                            //else
                            //    case_statements.Add(GetNextExpression(sub_ts, AnOperator.dicSystemOperator[";"], parseInfo));
                        }
                        if (case_statements.Count > 0)
                        {
                            if (test_values.Count > 0)
                                case_list.Add(Expression.SwitchCase(Expression.Block(case_statements), test_values));
                            if (isDefault)
                                default_statements = Expression.Block(case_statements);
                        }
                        // end stack
                        lv = parseInfo.localVariableStack.Pop();
                        jumpInfo = parseInfo.jumpInfoStack.Pop();
                        if (jumpInfo.breakLabel == null) jumpInfo.breakLabel = Expression.Label(Expression.Label());
                        // create switch
                        statement = Expression.Switch(test, default_statements, case_list.ToArray());
                        statement = Expression.Block(lv.Values, statement, jumpInfo.breakLabel);
                        break;
                        #endregion
                    case "try":
                        #region try { statement } catch(T var) { statement2 } finally { statement2 }
                        // parse try block
                        Assert((tok = ts.Next()) != null && tok.value == "{", tok);  // skip "try" to "{"
                        statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo, true);
                        if (statement == null) statement = Expression.Empty();
                        // parse catch blocks
                        List<CatchBlock> catch_list = new List<CatchBlock>();
                        while (ts.CurrentValue == "catch")
                        {
                            ts.Next();  // skip "catch"
                            bool hasParameter = false;
                            if (ts.CurrentValue == "(")
                            {
                                hasParameter = true;
                                parseInfo.localVariableStack.Push(new Dictionary<string, ParameterExpression>());
                                Assert(ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo) == null && parseInfo.localVariableStack.Peek().Count == 1, "Invalid syntax for catch."); // can only have one variable declare and no value assignment
                            }
                            Assert(ts.CurrentValue == "{", ts.Current);
                            statement2 = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo, true);
                            if (statement2 == null) statement2 = Expression.Empty();
                            if (hasParameter)
                            {
                                Dictionary<string, ParameterExpression> dic = parseInfo.localVariableStack.Pop();
                                foreach (string key in dic.Keys)    // there is only one key
                                    catch_list.Add(Expression.Catch(dic[key], statement2));
                            }
                            else
                                catch_list.Add(Expression.Catch(typeof(Exception), statement2));
                        }
                        // parse finally block
                        statement2 = null;
                        if (ts.CurrentValue == "finally")
                        {
                            Assert((tok = ts.Next()) != null && tok.value == "{", tok);  // skip "finally"
                            statement2 = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo, false);
                            if (statement2 == null) statement2 = Expression.Empty();
                        }
                        statement = Expression.TryCatchFinally(statement, statement2, catch_list.ToArray());
                        break;
                        #endregion
                    case "throw":
                        #region throw new T;
                        ts.Next();  // skip "throw"
                        statement2 = ParseNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (statement2 == null)
                            statement = Expression.Rethrow();
                        else
                            statement = Expression.Throw(statement2);
                        break;
                        #endregion
                    case "return":
                        #region return exp;
                        ts.Next();  // skip "return"
                        exp1 = ParseNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
                        if (exp1 == null)
                        {
                            if (parseInfo.returnLabel == null) parseInfo.returnLabel = Expression.Label(Expression.Label());
                            statement = Expression.Return(parseInfo.returnLabel.Target);
                        }
                        else
                        {
                            if (exp1.Type != typeof(object)) exp1 = Expression.Convert(exp1, typeof(object));   // convert all return value to object in case return different type
                            if (parseInfo.returnLabel == null) parseInfo.returnLabel = Expression.Label(Expression.Label(exp1.Type), Expression.Default(exp1.Type));
                            statement = Expression.Return(parseInfo.returnLabel.Target, exp1);
                        }
                        break;
                        #endregion

                }
            }
            if (statement == null)
            {
                if (tok.value == "{")
                    statement = ParseStatementBlock(ts.NextUntilOperatorClose(), parseInfo, false);
                else
                    statement = ParseNextExpression(ts, AnOperator.dicSystemOperator[";"], parseInfo);
            }
            if (ts.CurrentValue == ";") ts.Next();  // skip ";"
            return statement;
        }

        internal Expression ParseNextExpression(TokenStore ts, AnOperator leftOperator, ParseInfo parseInfo)
        {
            Expression exp = null, exp2 = null, exp3 = null;
            Type t = null;
            MethodInfo mi = null;
            AToken tok;
            while ((tok = ts.Current) != null)
            {
                if (leftOperator != null && tok.op != null && tok.op.precedence >= leftOperator.precedence) return exp;

                // current operator has high precedence than leftOp
                switch (tok.tok_type)
                {
                    case TOKEN_TYPE.INT: Assert(exp == null, tok); exp = Expression.Constant(int.Parse(tok.value)); break;
                    case TOKEN_TYPE.UINT: Assert(exp == null, tok); exp = Expression.Constant(uint.Parse(tok.value)); break;
                    case TOKEN_TYPE.LONG: Assert(exp == null, tok); exp = Expression.Constant(long.Parse(tok.value)); break;
                    case TOKEN_TYPE.ULONG: Assert(exp == null, tok); exp = Expression.Constant(ulong.Parse(tok.value)); break;
                    case TOKEN_TYPE.FLOAT: Assert(exp == null, tok); exp = Expression.Constant(float.Parse(tok.value)); break;
                    case TOKEN_TYPE.DOUBLE: Assert(exp == null, tok); exp = Expression.Constant(double.Parse(tok.value)); break;
                    case TOKEN_TYPE.DECIMAL: Assert(exp == null, tok); exp = Expression.Constant(decimal.Parse(tok.value, System.Globalization.NumberStyles.AllowExponent)); break;
                    case TOKEN_TYPE.BOOL: Assert(exp == null, tok); exp = Expression.Constant(bool.Parse(tok.value)); break;
                    case TOKEN_TYPE.TEXT: Assert(exp == null, tok); exp = Expression.Constant(tok.value); break;
                    case TOKEN_TYPE.IDENTIFIER:   // x, x[]
                        #region Parameter or Static Type reference
                        if (exp != null && exp is ConstantExpression && ((ConstantExpression)exp).Value is Type) // static type
                        {
                            #region declare local variable: T var1=exp1, var2=exp2;
                            t = (Type)((ConstantExpression)exp).Value;
                            Dictionary<string, ParameterExpression> localVaribles = parseInfo.localVariableStack.Peek();
                            List<Expression> exp_list = new List<Expression>();
                            while (ts.Current != null)
                            {
                                Assert(ts.Current.tok_type == TOKEN_TYPE.IDENTIFIER, ts.Current);
                                ParameterExpression var = Expression.Variable(t, ts.Current.value);
                                localVaribles.Add(ts.Current.value, var);
                                ts.Next();   // skip variable

                                if (ts.CurrentValue == "=") // variable initializer
                                {
                                    ts.Next();  // skip "="
                                    if (var.Type.IsArray && ts.CurrentValue == "{") // array initializer: T[] var = {1,2,3};
                                        exp = Expression.NewArrayInit(var.Type.GetElementType(), ParseParameters(ts, parseInfo, null, false));
                                    else
                                        exp = ParseNextExpression(ts, AnOperator.dicSystemOperator[","], parseInfo);
                                    if (exp.Type != t) exp = Expression.Convert(exp, t);
                                    exp_list.Add(Expression.Assign(var, exp));
                                }

                                if (ts.Current == null || ts.Current.value == ";") break;  // end of statement
                                if (ts.Current.value == ",") ts.Next(); // skip "," to next variable
                            }
                            if (exp_list.Count == 0) exp = null;
                            else if (exp_list.Count == 1) exp = exp_list[0];
                            else exp = Expression.Block(exp_list); 
                            #endregion
                        }
                        else
                        {
                            #region Parameter or Static Type reference
                            Assert(exp == null && (exp = ParseReferredParameterOrType(ts, parseInfo, true, true, true)) != null, ts.Current);
                            #endregion
                        }
                        break;
                        #endregion
                    case TOKEN_TYPE.OPERATOR:
                        #region Operator
                        switch (tok.op.op_type)
                        {
                            case OPERATOR_TYPE.OPEN:
                                #region Open operators: (, [. "{" will be proccessed in ParseStatement()
                                switch (tok.value)
                                {
                                    case "(":
                                        #region ParenthesisLeft: (nested expression)
                                        Assert(exp == null, tok);
                                        exp = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo);
                                        if (exp is ConstantExpression && ((ConstantExpression)exp).Value is Type) // (T)x
                                        {
                                            exp2 = ParseNextExpression(ts, AnOperator.dicSystemOperator["(T)"], parseInfo);
                                            if (exp2 != null)
                                                exp = Expression.Convert(exp2, (Type)((ConstantExpression)exp).Value);
                                        }
                                        break;
                                        #endregion
                                    case "[":
                                        #region BracketLeft: indexer[]
                                        Assert(exp != null, tok);
                                        {
                                            List<Type> param_types = new List<Type>();
                                            List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, false);

                                            //TokenStore sub_ts = ts.NextUntilOperatorClose();
                                            //List<Expression> param_list = new List<Expression>();
                                            //while ((exp2 = GetNextExpression(sub_ts, AnOperator.dicSystemOperator[","], exprParameterList)) != null)
                                            //{
                                            //    param_list.Add(exp2);
                                            //    sub_ts.Next();   // to ','
                                            //}
                                            ////List<TokenStore> ts_list = ts.NextUntilOperatorClose().Split(OPERATOR_NAME.Comma);
                                            ////List<Expression> param_list = new List<Expression>();
                                            ////foreach (TokenStore one_ts in ts_list)
                                            ////{
                                            ////    if(one_ts.token_list.Count>0)
                                            ////        param_list.Add(GetNextExpression(one_ts, null, exprParameterList));
                                            ////}
                                            //List<Type> param_types = new List<Type>();
                                            //foreach (Expression e in param_list) param_types.Add(e.Type);
                                            if ((mi = exp.Type.GetMethod(tok.op.overload_name, BindingFlags.Instance | BindingFlags.Public, null, param_types.ToArray(), null)) != null)
                                                exp = Expression.Call(exp, mi, param_list.ToArray());
                                            else
                                                exp = Expression.ArrayIndex(exp, param_list.ToArray());
                                        }
                                        break;
                                        #endregion
                                    default:
                                        Assert(false, tok);
                                        break;
                                }
                                break; 
                                #endregion
                            case OPERATOR_TYPE.PREFIX_UNARY:
                                #region prefix unary operators
                                Assert(exp == null && ts.Next() != null, tok);
                                Assert((exp2 = ParseNextExpression(ts, tok.op, parseInfo)) != null, ts.Current);
                                mi = tok.op.overload_name == null ? null : exp2.Type.GetMethod(tok.op.overload_name, new Type[] { exp2.Type });
                                exp = ((d1m)tok.op.exp_call)(exp2, mi);
                                break;
                                #endregion
                            case OPERATOR_TYPE.POST_UNARY:
                                #region POST_UNARY
		                        Assert(exp != null, tok);
                                mi = tok.op.overload_name == null ? null : exp.Type.GetMethod(tok.op.overload_name, new Type[] { exp.Type });
                                exp = ((d1m)tok.op.exp_call)(exp, mi); 
                                break;
	                            #endregion
                            case OPERATOR_TYPE.BINARY:
                            case OPERATOR_TYPE.ASSIGN:
                                #region Binary or Assign Operators
                                Assert(exp != null && ts.Next() != null && (exp2 = ParseNextExpression(ts, tok.op, parseInfo)) != null, tok);
                                mi = null; t = null;
                                if (tok.value == "is")
                                {
                                    Assert(exp2 is ConstantExpression && ((ConstantExpression)exp2).Value is Type, tok);
                                    exp = Expression.TypeIs(exp, (Type)((ConstantExpression)exp2).Value);
                                    break;
                                }
                                else if (tok.value == "as")
                                {
                                    Assert(exp2 is ConstantExpression && ((ConstantExpression)exp2).Value is Type, tok);
                                    exp = Expression.TypeAs(exp, (Type)((ConstantExpression)exp2).Value);
                                    break;
                                }
                                if (tok.value == "+" && exp.Type == typeof(string) || exp2.Type == typeof(string))
                                {
                                    if (exp.Type != typeof(string)) exp = Expression.Convert(exp, typeof(string));
                                    if (exp2.Type != typeof(string)) exp2 = Expression.Convert(exp2, typeof(string));
                                    mi = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                                }
                                else
                                {
                                    if (tok.op.overload_name != null)
                                    {
                                        mi = exp.Type.GetMethod(tok.op.overload_name, new Type[] { exp.Type, exp2.Type });
                                        if (mi == null)
                                            mi = exp2.Type.GetMethod(tok.op.overload_name, new Type[] { exp.Type, exp2.Type });
                                    }
                                    if (mi == null)
                                    {
                                        // use default operator, need implicit type conversion
                                        if (tok.op.required1stType != null) exp = Expression.Convert(exp, tok.op.required1stType);
                                        if (tok.op.required2ndType != null) exp2 = Expression.Convert(exp2, tok.op.required2ndType);
                                        if (tok.op.requiredOperandType == RequiredOperandType.SAME
                                            && exp.Type != exp2.Type
                                            && (t = QueryImplicitConversionType(exp, exp2)) != null)
                                        {
                                            if (exp.Type != t) exp = Expression.Convert(exp, t);
                                            if (exp2.Type != t) exp2 = Expression.Convert(exp2, t);
                                        }
                                    }
                                }
                                if (tok.op.exp_call is d2) exp = ((d2)tok.op.exp_call)(exp, exp2);
                                else if (tok.op.exp_call is d2m) exp = ((d2m)tok.op.exp_call)(exp, exp2, mi);
                                else exp = ((d2bm)tok.op.exp_call)(exp, exp2, false, mi);
                                break;
                                #endregion
                             case OPERATOR_TYPE.CONDITIONAL:
                                #region Conditional
                                Assert(exp != null && ts.Next()!=null && (exp2 = ParseNextExpression(ts, tok.op, parseInfo)) != null, tok);
                                Assert(ts.CurrentValue == ":" && ts.Next() != null, tok);
                                Assert((exp3 = ParseNextExpression(ts, tok.op, parseInfo)) != null, tok);
                                if (exp2.Type != exp3.Type && (t = QueryImplicitConversionType(exp2, exp3)) != null)
                                {
                                    if (exp2.Type != t) exp2 = Expression.Convert(exp2, t);
                                    if (exp3.Type != t) exp3 = Expression.Convert(exp3, t);
                                }
                                exp = Expression.Condition(exp, exp2, exp3);
                                break;
                                #endregion
                            case OPERATOR_TYPE.PRIMARY:
                                #region Primary: 
                                switch (tok.value)
                                {
                                    case "new":
                                        #region New
                                        {
                                            Assert(exp == null && ts.Next() != null && ts.Current.tok_type == TOKEN_TYPE.IDENTIFIER, tok); // skip "new"
                                            Assert((exp = ParseReferredParameterOrType(ts, parseInfo, true, false, false)) != null, tok);
                                            t = (Type)((ConstantExpression)exp).Value;
                                            Assert(ts.Current != null && ts.Current.value == "(" || ts.Current.value == "[", ts.Current);    // next is '(' or '['
                                            if (ts.Current.value == "(")   // new object().
                                            {
                                                List<Type> param_types = new List<Type>();
                                                List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, false);

                                                ConstructorInfo ci = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, param_types.ToArray(), null);
                                                Assert(ci != null, ts.Current);
                                                // check parameter implicit conversion
                                                ParameterInfo[] pis = ci.GetParameters();
                                                for (int idx = 0; idx < param_list.Count; idx++)
                                                {
                                                    if (param_list[idx].Type != pis[idx].ParameterType)
                                                        param_list[idx] = Expression.Convert(param_list[idx], pis[idx].ParameterType);
                                                }
                                                exp = Expression.New(ci, param_list.ToArray());
                                            }
                                            else // new int[x,y] or new type[]{x,y}
                                            {
                                                List<Expression> param_list = ParseParameters(ts, parseInfo, null, false);  // [x,y]
                                                if (ts.CurrentValue == "{")   // array initializer {1,2,3}
                                                    exp = Expression.NewArrayInit(t, ParseParameters(ts, parseInfo, null, false));   // new array[] {x,y}
                                                else
                                                    exp = Expression.NewArrayBounds(t, param_list); // new array[x,y]
                                            }
                                        }
                                        break; 
                                        #endregion
                                    case "typeof":
                                        #region typeof
                                        Assert(exp == null && ts.Next() != null && ts.Current.value == "(", tok); // must be "("
                                        Assert((exp = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null, tok);
                                        Assert(exp is ConstantExpression && ((ConstantExpression)exp).Value is Type, "Invalid parameter for typeof(T)"); // typeof(T)
                                        break; 
                                        #endregion
                                    case "sizeof":
                                        #region sizeof
                                        Assert(exp == null, tok);
                                        Assert(ts.Next() != null && ts.Current.value == "(", tok); // must be "("
                                        Assert((exp = ParseNextExpression(ts.NextUntilOperatorClose(), null, parseInfo)) != null, tok);
                                        Assert(exp is ConstantExpression
                                            && ((ConstantExpression)exp).Value is Type
                                            && ((Type)((ConstantExpression)exp).Value).IsValueType, "Invalid parameter for sizeof(T)"); // sizeof(T)
                                        exp = Expression.Constant(System.Runtime.InteropServices.Marshal.SizeOf((Type)((ConstantExpression)exp).Value));
                                        break; 
                                        #endregion
                                    case ".":
                                        #region Property, Field or method call
                                        Assert(exp != null && (tok=ts.Next())!=null && tok.tok_type == TOKEN_TYPE.IDENTIFIER, tok); // skip "." => tok=variable
                                        ts.Next();  // skip variable
                                        if (ts.CurrentValue == "(")  // method call
                                        {
                                            #region Method Call
                                            List<Type> param_types = new List<Type>();
                                            List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, false);

                                            if (exp is ConstantExpression && ((ConstantExpression)exp).Value is Type) // static call
                                            {
                                                t = (Type)(((ConstantExpression)exp).Value);
                                                Assert((mi = t.GetMethod(tok.value, BindingFlags.Static | BindingFlags.Public, null, param_types.ToArray(), null)) != null, tok);
                                                // check parameter implicit conversion
                                                ParameterInfo[] pis = mi.GetParameters();
                                                for (int idx = 0; idx < param_list.Count; idx++)
                                                {
                                                    Type et = (pis[idx].ParameterType.IsByRef) ? pis[idx].ParameterType.GetElementType() : pis[idx].ParameterType;
                                                    if (param_list[idx].Type != et)
                                                        param_list[idx] = Expression.Convert(param_list[idx], et);
                                                }
                                                exp = Expression.Call(mi, param_list.ToArray());
                                            }
                                            else // instance object method call
                                            {
                                                mi = exp.Type.GetMethod(tok.value, BindingFlags.Instance | BindingFlags.Public, null, param_types.ToArray(), null);
                                                Assert(mi != null, tok);
                                                // check parameter implicit conversion
                                                ParameterInfo[] pis = mi.GetParameters();
                                                for (int idx = 0; idx < param_list.Count; idx++)
                                                {
                                                    Type et = (pis[idx].ParameterType.IsByRef) ? pis[idx].ParameterType.GetElementType() : pis[idx].ParameterType;
                                                    if (param_list[idx].Type != et)
                                                        param_list[idx] = Expression.Convert(param_list[idx], et);
                                                }
                                                exp = Expression.Call(exp, mi, param_list.ToArray());
                                            } 
                                            #endregion
                                        }
                                        else if (exp is ConstantExpression && ((ConstantExpression)exp).Value is Type)
                                        {
                                            #region Static property of field
                                            PropertyInfo pi = null; FieldInfo fi = null;
                                            t = (Type)(((ConstantExpression)exp).Value);
                                            if ((pi = t.GetProperty(tok.value, BindingFlags.Public | BindingFlags.Static)) != null)
                                                exp = Expression.Property(null, pi);
                                            else if ((fi = t.GetField(tok.value, BindingFlags.Public | BindingFlags.Static)) != null)
                                                exp = Expression.Field(null, fi);
                                            else if ((pi = exp.Type.GetProperty(tok.value, BindingFlags.Instance | BindingFlags.Public)) != null)
                                                exp = Expression.Property(exp, pi);
                                            else if ((fi = exp.Type.GetField(tok.value, BindingFlags.Instance | BindingFlags.Public)) != null)
                                                exp = Expression.Field(exp, fi);
                                            else if (ts.CurrentValue == "<")
                                            {
                                                // v1.1: Static Generic Method call
                                                List<Type> param_types = new List<Type>();
                                                List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, true);

                                                Assert((mi = t.GetMethod(tok.value, BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(param_types.ToArray())) != null, tok);
                                                //Assert((mi = mi.MakeGenericMethod(param_types.ToArray())) != null, tok);
                                                Assert(ts.CurrentValue == "(", ts.Current);    // to "("

                                                param_list = ParseParameters(ts, parseInfo, null, false);
                                                exp = Expression.Call(mi, param_list.ToArray());
                                            }
                                            else
                                                Assert(false, tok); 
                                            #endregion
                                        }
                                        else
                                        {
                                            #region instance property or field
                                            PropertyInfo pi = null; FieldInfo fi = null;
                                            if ((pi = exp.Type.GetProperty(tok.value, BindingFlags.Instance | BindingFlags.Public)) != null)
                                                exp = Expression.Property(exp, pi);
                                            else if ((fi = exp.Type.GetField(tok.value, BindingFlags.Instance | BindingFlags.Public)) != null)
                                                exp = Expression.Field(exp, fi);
                                            else if (ts.CurrentValue == "<")
                                            {
                                                // v1.1: Instance Generic method call
                                                List<Type> param_types = new List<Type>();
                                                List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, true);
                                                Assert((mi = exp.Type.GetMethod(tok.value, BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(param_types.ToArray())) != null, tok);
                                                //Assert((mi = mi.MakeGenericMethod(param_types.ToArray())) != null, tok);
                                                Assert(ts.CurrentValue == "(", ts.Current);    // to "("
                                                param_list = ParseParameters(ts, parseInfo, null, false);
                                                exp = Expression.Call(exp, mi, param_list.ToArray());
                                            }
                                            else
                                                Assert(false, tok); 
                                            #endregion
                                        }
                                        break; 
                                        #endregion
                                    default:
                                        // ,(not in method and indexer) and ;(need ASP.NET4.0)
                                        Assert(false, tok);
                                        break;
                                }
                                break;
                                #endregion
                        }
                        break;
                        #endregion
                    default:
                        Assert(false, tok);
                        break;
                }
                if (ts.Current == tok) ts.Next();
            }
            return exp;
        }

        internal Type ParseStaticTypeExtension(TokenStore ts, Type t, bool allowArrayType)
        {
            // check nullable
            if (t.IsValueType && ts.CurrentValue == "?" && NullableType.ContainsKey(t))
            {
                t = NullableType[t];
                ts.Next();  // skip "?"
            }
            // check nested type
            Type t2=null;
            while (ts.CurrentValue == "."
                && ts.PeekToken(1) != null && ts.PeekToken(1).tok_type == TOKEN_TYPE.IDENTIFIER
                && (t2 = t.GetNestedType(ts.PeekToken(1).value)) != null)
            {
                t = t2;
                ts.Next();  // skip '.'
                ts.Next();  // skip nested type
            }
            // check array: t[,][], if after new operater, do not check array here
            if (allowArrayType)
            {
                while (ts.CurrentValue == "[")
                {
                    StringBuilder arr = new StringBuilder("[");
                    AToken tok = ts.Next(); // skip "["
                    while ((tok = ts.Current) != null && tok.value != "]") arr.Append(tok.value);
                    Assert((t = t.Assembly.GetType(t.FullName + arr.Append("]").ToString())) != null, tok);
                    Assert(tok.value == "]", tok);
                    tok = ts.Next();  // skip "]"
                }
            }
            return t;
        }
        internal List<Expression> ParseParameters(TokenStore ts, ParseInfo parseInfo, List<Type> param_types, bool typeByValue)
        {
            TokenStore sub_ts = ts.NextUntilOperatorClose();
            List<Expression> param_list = new List<Expression>();
            while (sub_ts.Current != null)
            {
                bool isRef = false;
                if (sub_ts.CurrentValue == "ref" || sub_ts.CurrentValue == "out")
                {
                    isRef = true;
                    sub_ts.Next();
                }
                Expression e = ParseNextExpression(sub_ts, AnOperator.dicSystemOperator[","], parseInfo);
                param_list.Add(e);
                if (param_types != null)
                {
                    if (isRef)
                    {
                        if (typeByValue)
                            param_types.Add(((Type)((ConstantExpression)e).Value).MakeByRefType());
                        else
                            param_types.Add(e.Type.MakeByRefType());
                    }
                    else
                    {
                        if (typeByValue)
                            param_types.Add((Type)((ConstantExpression)e).Value);
                        else
                            param_types.Add(e.Type);
                    }
                }
                Assert(sub_ts.Current == null || sub_ts.CurrentValue == "," && sub_ts.Next() != null, sub_ts.Current);    // skip ','
            }
            //if (param_types != null)
            //{
            //    if(typeByValue)
            //        foreach (Expression e in param_list) param_types.Add((Type)((ConstantExpression)e).Value);
            //    else
            //        foreach (Expression e in param_list) param_types.Add(e.Type);
            //}
            return param_list;
        }
        internal Expression ParseReferredParameterOrType(TokenStore ts, ParseInfo parseInfo, bool allowType, bool allowVariable, bool allowArrayType)
        {
            Type t = null;
            Expression exp = null;
            StringBuilder var_fullname = new StringBuilder();
            AToken tok = ts.Current;
            while (exp == null && tok != null && tok.tok_type == TOKEN_TYPE.IDENTIFIER)
            {
                var_fullname.Append(tok.value);
                // try local variable
                if (allowVariable && (exp = parseInfo.GetReferredVariable(var_fullname.ToString())) != null) { ts.Next(); break; }
                // try referred parameter
                else if (allowVariable && (t = this.QueryParameterType(var_fullname.ToString())) != null)
                {
                    exp = parseInfo.GetReferredParameter(t, tok.value);
                    ts.Next();
                    break;
                }
                // try static Generic Type
                else if (allowType && ts.PeekTokenValue(1) == "<")
                {
                    // v1.1: Generic Type reference
                    tok = ts.Next();    // to "<"
                    List<Type> param_types = new List<Type>();
                    List<Expression> param_list = ParseParameters(ts, parseInfo, param_types, true);
                    Assert((t = this.QueryStaticType(var_fullname.Append("`").Append(param_list.Count).ToString()).MakeGenericType(param_types.ToArray())) != null, tok);
                    t = ParseStaticTypeExtension(ts, t, allowArrayType);
                    exp = Expression.Constant(t);   // Type: (T)x or T.xxx
                    break;
                }
                // try static Type
                else if (allowType && (t = this.QueryStaticType(var_fullname.ToString())) != null)
                {
                    // Static Type reference
                    ts.Next();
                    t = ParseStaticTypeExtension(ts, t, allowArrayType);
                    exp = Expression.Constant(t);
                    break;
                }
                // neither parameter or static type
                if ((tok = ts.Next()) == null || tok.value != ".") break;   // unexpected
                var_fullname.Append(".");
                tok = ts.Next();    // skip "."
            }
            return exp;
        }
        
        public object Run(LambdaExpression lambda, params object[] inline_parameter_values)
        {
            Delegate f = lambda.Compile();
            if (inline_parameter_values != null && inline_parameter_values.Length > 0)
                return f.DynamicInvoke(inline_parameter_values);

            List<object> parameterList = new List<object>();
            if (lambda.Parameters != null && lambda.Parameters.Count > 0)
            {
                foreach (ParameterExpression pe in lambda.Parameters)
                {
                    if (this.ParameterValue.ContainsKey(pe.Name))
                        parameterList.Add(this.ParameterValue[pe.Name]);
                    else if (GlobalParameterValue.ContainsKey(pe.Name))
                        parameterList.Add(GlobalParameterValue[pe.Name]);
                    else
                        throw new ExprException(string.Format("The value for {0} is undefined.", pe.Name));
                }
            }
            return f.DynamicInvoke(parameterList.ToArray());
        }

        private void Assert(bool check, AToken tok)
        {
            if (!check)
                if (tok == null)
                    throw new ExprException("Unexpected end.");
                else
                    throw new ExprException(string.Format("Unexpected token({0}) at: {1}.", tok.value, tok.start_pos+1));
        }
        private void Assert(bool check, string message)
        {
            if (!check)
                throw new ExprException(message);
        }
    }
    
}
