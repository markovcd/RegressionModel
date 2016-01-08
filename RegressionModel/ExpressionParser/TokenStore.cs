using System;
using System.Collections.Generic;
using System.Text;

namespace Simpro.Expr
{
    internal class TokenStore
    {
        internal string original_string;
        internal List<AToken> token_list;
        internal int current_token_idx;

        private TokenStore(string exp_string, List<AToken> token_list)
        {
            this.original_string = exp_string;
            this.token_list = token_list;
            current_token_idx = 0;
        }

        internal static TokenStore Parse(string exp_string, out TokenStore declare_ts)
        {
            List<AToken> list = new List<AToken>();
            List<AToken> declare_list = null;
            int start_pos = 0;
            TOKEN_TYPE tok_type = TOKEN_TYPE.NONE;
            bool isHex = false;         // 0xFFFF
            bool isExponent = false;    // 1e10, 1E-2
            bool isVerbatim = false;    // @"xxx"
            char start_quotation = '"';
            StringBuilder text_value = new StringBuilder();
            // blank : c <= ' '
            // text: '"'
            // number : 0-9
            // decimal : .
            // variable : A-Z,a-z,_
            // operator: else
            for (int idx = 0; idx < exp_string.Length; idx++)
            {
                char c = exp_string[idx];
                char c2 = (idx + 1) < exp_string.Length ? exp_string[idx + 1] : (char)0;
                switch (tok_type)
                {
                    case TOKEN_TYPE.NONE:
                        #region NONE
                        if (c <= ' ') continue; // skip space
                        start_pos = idx;
                        if (c == '/' && c2 == '/') { idx++; tok_type = TOKEN_TYPE.COMMENT; }
                        else if (c == '/' && c2 == '*') { idx++; tok_type = TOKEN_TYPE.COMMENT_BLOCK; }
                        else if (c == '@' && (c2 == '"' || c2 == '\'')) { idx++; start_quotation = c2; tok_type = TOKEN_TYPE.TEXT; isVerbatim = true; }
                        else if (c == '"' || c == '\'') { start_quotation = c; tok_type = TOKEN_TYPE.TEXT; }
                        else if (c == '0' && (c2 == 'x' || c2 == 'X')) { idx++; tok_type = TOKEN_TYPE.INT; isHex = true; }
                        else if (c >= '0' && c <= '9') tok_type = TOKEN_TYPE.INT;
                        else if (c == '.')
                        {
                            if (c2 >= '0' && c2 <= '9')
                                tok_type = TOKEN_TYPE.DOUBLE;  // no leading digit number
                            else
                                tok_type = TOKEN_TYPE.OPERATOR; // (x).y; x[idx].y
                        }
                        else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_') tok_type = TOKEN_TYPE.IDENTIFIER;
                        else tok_type = TOKEN_TYPE.OPERATOR;
                        break;
                    #endregion NONE
                    case TOKEN_TYPE.COMMENT:    // // xxxx \n
                        if (c == '\n') tok_type = TOKEN_TYPE.NONE;  // skip eveything until new line;
                        break;
                    case TOKEN_TYPE.COMMENT_BLOCK:  // /* xxxx */
                        if (c == '*' && c2 == '/') { idx++; tok_type = TOKEN_TYPE.NONE; }   // skip eveything until "*/";
                        break;
                    case TOKEN_TYPE.TEXT:   // "xxxx" or 'xxxx'
                        #region TEXT
                        if (!isVerbatim && c == '\\')  // escape for string
                        {
                            idx++;  // use c2
                            switch (c2)
                            {
                                case '0': text_value.Append('\0'); break;   // \0
                                case 'n': text_value.Append('\n'); break;   // new line
                                case 't': text_value.Append('\t'); break;   // tab
                                case 'r': text_value.Append('\r'); break;   // carriage return
                                case 'a': text_value.Append('\a'); break;   // alert
                                case 'b': text_value.Append('\b'); break;   // back space
                                case 'v': text_value.Append('\v'); break;   // vertical tab
                                case 'f': text_value.Append('\f'); break;   // Form Feed
                                case 'x': // hexadecimal \xF \xFF \xFFF \xFFFF
                                case 'u': // unicode \uFFFF
                                case 'U': // unicode \UFFFFFFFF
                                    {
                                        int additional_digits = 0;
                                        while (additional_digits < (c2 == 'U' ? 8 : 4)
                                            && (c = exp_string.Length > idx + additional_digits + 1 ? exp_string[idx + additional_digits + 1] : (char)0) != (char)0
                                            && (c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f'))
                                            additional_digits++;
                                        if (additional_digits == 0 || c2 == 'u' && additional_digits < 4 || c2 == 'U' && additional_digits < 8)
                                            throw new ExprException("Expecting hexadecimal for operator: '\\" + c2 + "'");
                                        else
                                        {
                                            text_value.Append((char)Convert.ToInt16("0x" + exp_string.Substring(idx + 1, additional_digits), 16));
                                            idx += additional_digits;
                                        }
                                    }
                                    break;
                                default: text_value.Append(c2); break;   // '\\', '\"', '\''
                            }
                        }
                        else if (isVerbatim && c == start_quotation && c2 == start_quotation) // escape for verbatim string
                        {
                            text_value.Append(c);
                            idx++;
                        }
                        else if (c == start_quotation)  // end text
                        {
                            list.Add(new AToken(start_pos, TOKEN_TYPE.TEXT, null, text_value.ToString()));
                            text_value = new StringBuilder();
                            tok_type = TOKEN_TYPE.NONE;
                            isVerbatim = false;
                        }
                        else
                            text_value.Append(c);
                        break;
                    #endregion TEXT
                    case TOKEN_TYPE.INT:
                        #region INT
                        if (c >= '0' && c <= '9') { }
                        else if (isHex && (c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F')) { }
                        else if (c == '.' && !isHex) tok_type = TOKEN_TYPE.DOUBLE;
                        else if ((c == 'e' || c == 'E') && !isHex) { tok_type = TOKEN_TYPE.DOUBLE; isExponent = true; }
                        else
                        {
                            string val = isHex ? Convert.ToUInt64(exp_string.Substring(start_pos, idx - start_pos), 16).ToString() : exp_string.Substring(start_pos, idx - start_pos);
                            if (c <= ' ')  // space, \t, \n, ... control charaters
                            {
                                list.Add(new AToken(start_pos, tok_type, null, val));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c == '/' && c2 == '/')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, val));
                                tok_type = TOKEN_TYPE.COMMENT;
                                idx++;
                            }
                            else if (c == '/' && c2 == '*')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, val));
                                tok_type = TOKEN_TYPE.COMMENT_BLOCK;
                                idx++;
                            }
                            else if (c == 'U' || c == 'u')
                            {
                                if (c2 == 'L' || c2 == 'l')
                                {
                                    list.Add(new AToken(start_pos, TOKEN_TYPE.ULONG, null, val));
                                    tok_type = TOKEN_TYPE.NONE;
                                    idx++;
                                }
                                else
                                {
                                    list.Add(new AToken(start_pos, TOKEN_TYPE.UINT, null, val));
                                    tok_type = TOKEN_TYPE.NONE;
                                }
                            }
                            else if (c == 'L' || c == 'l')
                            {
                                if (c2 == 'U' || c2 == 'u')
                                {
                                    list.Add(new AToken(start_pos, TOKEN_TYPE.ULONG, null, val));
                                    tok_type = TOKEN_TYPE.NONE;
                                    idx++;
                                }
                                else
                                {
                                    list.Add(new AToken(start_pos, TOKEN_TYPE.LONG, null, val));
                                    tok_type = TOKEN_TYPE.NONE;
                                }
                            }
                            else if (!isHex && (c == 'F' || c == 'f'))
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.FLOAT, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (!isHex && (c == 'D' || c == 'd'))
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.DOUBLE, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (!isHex && (c == 'M' || c == 'm'))
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.DECIMAL, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, val));
                                start_pos = idx;
                                tok_type = TOKEN_TYPE.IDENTIFIER; // report error later
                            }
                            else
                            {
                                // operator, add previous token
                                list.Add(new AToken(start_pos, tok_type, null, val));
                                start_pos = idx;
                                tok_type = TOKEN_TYPE.OPERATOR;
                            }
                            isHex = false;
                        }
                        break;
                    #endregion NUMBER
                    case TOKEN_TYPE.DOUBLE:
                        #region DOUBLE
                        if (c >= '0' && c <= '9') { }
                        else if ((c == '-' || c == '+') && isExponent) { }  // 1e-2. can only have one sign
                        else
                        {
                            if (c <= ' ')  // space, \t, \n, ... control charaters
                            {
                                list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c == '/' && c2 == '/')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.COMMENT;
                                idx++;
                            }
                            else if (c == '/' && c2 == '*')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.COMMENT_BLOCK;
                                idx++;
                            }
                            else if (c == 'F' || c == 'f')
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.FLOAT, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c == 'D' || c == 'd')
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.DOUBLE, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c == 'M' || c == 'm')
                            {
                                list.Add(new AToken(start_pos, TOKEN_TYPE.DECIMAL, null, exp_string.Substring(start_pos, idx - start_pos)));
                                tok_type = TOKEN_TYPE.NONE;
                            }
                            else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_')
                            {
                                list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, idx - start_pos)));
                                start_pos = idx;
                                tok_type = TOKEN_TYPE.IDENTIFIER; // report error later
                            }
                            else
                            {
                                // token change, save previous token
                                list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, idx - start_pos)));
                                start_pos = idx;
                                tok_type = TOKEN_TYPE.OPERATOR;
                            }
                        }
                        isExponent = false; // can only have one sign right after 'e'
                        break;
                    #endregion DOUBLE
                    case TOKEN_TYPE.IDENTIFIER:
                        #region VARIABLE
                        // VARIABLE(x or ns.x) can be a token of
                        //      - boolean:      true / false    => BOOL
                        //      - null:         null            => TEXT
                        //      - is                            => operator
                        //      - new                           => operator
                        //      - property:     .x              => VARIABLE. continue to see operator '[' and '(' for x[] and x()
                        //      - static type:  ns.x            => TYPE. continue to see operator '[' and '(' for x[] and x()
                        //      - parameter:    x               => VARIABLE
                        //      - un-typed parameter: error     => VARIABLE, will report error later
                        if (c >= '0' && c <= '9' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_') { }
                        else
                        {
                            string variable_name = exp_string.Substring(start_pos, idx - start_pos);
                            if (variable_name == "true" || variable_name == "false") // boolean
                                list.Add(new AToken(start_pos, TOKEN_TYPE.BOOL, null, variable_name));
                            else if (variable_name == "null")  // null
                                list.Add(new AToken(start_pos, TOKEN_TYPE.TEXT, null, null));
                            else if (variable_name == "is" // is
                                     || variable_name == "as" // as
                                     || variable_name == "new" // new
                                     || variable_name == "typeof" // typeof
                                     || variable_name == "sizeof" // sizeof
                                )
                                ParseOperatorTokens(start_pos, variable_name, ref list, ref declare_list);
                            else // un-typed parameter, will return error later
                            {
                                IdentifierAlias(new AToken(start_pos, tok_type, null, variable_name), list);
                            }

                            // set next token type
                            if (c <= ' ') tok_type = TOKEN_TYPE.NONE;
                            else if (c == '/' && c2 == '/')
                            {
                                tok_type = TOKEN_TYPE.COMMENT;
                                idx++;
                            }
                            else if (c == '/' && c2 == '*')
                            {
                                tok_type = TOKEN_TYPE.COMMENT_BLOCK;
                                idx++;
                            }
                            else
                                tok_type = TOKEN_TYPE.OPERATOR;
                            start_pos = idx;
                        }
                        break;
                    #endregion VARIABLE
                    case TOKEN_TYPE.OPERATOR:
                        #region OPERATOR
                        if (c <= ' ')  // space, \t, \n, ... control charaters
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            tok_type = TOKEN_TYPE.NONE;
                        }
                        else if (c == '/' && c2 == '/')
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            tok_type = TOKEN_TYPE.COMMENT;
                            idx++;
                        }
                        else if (c == '/' && c2 == '*')
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            tok_type = TOKEN_TYPE.COMMENT_BLOCK;
                            idx++;
                        }
                        else if (c == '0' && (c2 == 'x' || c2 == 'X'))
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            start_pos = idx;
                            isHex = true;
                            tok_type = TOKEN_TYPE.INT;
                            idx++;
                        }
                        else if (c >= '0' && c <= '9')
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            start_pos = idx;
                            tok_type = TOKEN_TYPE.INT;
                        }
                        else if (c == '@' && (c2 == '"' || c2 == '\''))
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            idx++;
                            start_pos = idx;    // start from " instead of @
                            tok_type = TOKEN_TYPE.TEXT;
                            start_quotation = c2;
                            isVerbatim = true;
                        }
                        else if (c == '"' || c == '\'')
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            start_pos = idx;
                            tok_type = TOKEN_TYPE.TEXT;
                            start_quotation = c;
                        }
                        else if (c == '.' && c2 >= '0' && c2 <= '9')  // no leading digit decimal, like .999
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            start_pos = idx;
                            OPERATOR_TYPE previous_token_op_type = (list.Count > 0 && list[list.Count - 1].op != null) ? list[list.Count - 1].op.op_type : OPERATOR_TYPE.UNKNOWN;
                            if (previous_token_op_type == OPERATOR_TYPE.CLOSE)
                                tok_type = TOKEN_TYPE.OPERATOR; // (x).y; x[idx].y
                            else
                                tok_type = TOKEN_TYPE.DOUBLE;  // no leading digit number
                        }
                        else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_')
                        {
                            ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, idx - start_pos), ref list, ref declare_list);
                            start_pos = idx;
                            tok_type = TOKEN_TYPE.IDENTIFIER;
                        }
                        else { } // continue operator
                        break;
                        #endregion OPERATOR
                }
            }
            if (tok_type == TOKEN_TYPE.COMMENT_BLOCK || tok_type == TOKEN_TYPE.TEXT)
                throw new ExprException("Unexpected end of the expression.");
            else if (tok_type != TOKEN_TYPE.NONE && tok_type != TOKEN_TYPE.COMMENT)
            {
                if (tok_type == TOKEN_TYPE.OPERATOR)
                    ParseOperatorTokens(start_pos, exp_string.Substring(start_pos, exp_string.Length - start_pos), ref list, ref declare_list);
                else
                    list.Add(new AToken(start_pos, tok_type, null, exp_string.Substring(start_pos, exp_string.Length - start_pos)));
            }
            declare_ts = declare_list != null && declare_list.Count > 0 ? new TokenStore(exp_string, declare_list) : null;
            return new TokenStore(exp_string, list);
        }

        private static void IdentifierAlias(AToken aToken, ICollection<AToken> list)
        {
            var t = typeof(Math);
            if (t.GetMethod(aToken.value) != null)
            {
                list.Add(new AToken(aToken.start_pos, TOKEN_TYPE.IDENTIFIER, null, t.Name));
                list.Add(new AToken(aToken.start_pos, TOKEN_TYPE.OPERATOR, new AnOperator(OPERATOR_TYPE.PRIMARY, 0), "."));

            }
            list.Add(aToken);
        }

        // for operator
        internal static void ParseOperatorTokens(int start_pos, string operator_string, ref List<AToken> list, ref List<AToken> declare_list)
        {
            int op_start = 0;
            int op_len = operator_string.Length;
            // need check Prefix Unary operator if there is no operand before or there is non-CLOSE operator before
            AToken lastToken = list.Count > 0 ? list[list.Count - 1] : null;
            bool checkPreUnary = lastToken == null || lastToken.op != null && lastToken.op.op_type != OPERATOR_TYPE.CLOSE;

            while (op_len > 0)
            {
                string op_str = operator_string.Substring(op_start, op_len);
                AnOperator exp_op = null;

                if (checkPreUnary && AnOperator.dicSystemOperator.ContainsKey(op_str + "U"))
                    exp_op = AnOperator.dicSystemOperator[op_str + "U"];
                if (exp_op == null && AnOperator.dicSystemOperator.ContainsKey(op_str))
                    exp_op = AnOperator.dicSystemOperator[op_str];
                if (exp_op != null)
                {
                    //if (op_str == "(" && lastToken != null && lastToken.tok_type == TOKEN_TYPE.VARIABLE) lastToken.tok_type = TOKEN_TYPE.FUNC;
                    AToken tok = new AToken(start_pos + op_start, TOKEN_TYPE.OPERATOR, exp_op, op_str);
                    if (tok.value == "=>" && declare_list == null)
                    {
                        declare_list = list;
                        list = new List<AToken>();
                        lastToken = null;
                    }
                    else
                    {
                        list.Add(tok);
                        lastToken = tok;
                    }
                    op_start += op_len;
                    op_len = operator_string.Length - op_start;
                    checkPreUnary = exp_op.op_type != OPERATOR_TYPE.CLOSE;
                }
                else
                    op_len--;
            }
            if (op_len == 0 && op_start < operator_string.Length)
            {
                // unknown operator
                string op_str = operator_string.Substring(op_start);
                list.Add(new AToken(start_pos + op_start, TOKEN_TYPE.OPERATOR, new AnOperator(OPERATOR_TYPE.UNKNOWN, 0), op_str));
            }
        }
        //internal bool IsEnd() { return current_token_idx >= token_list.Count; }
        internal AToken Current { get { return current_token_idx >= token_list.Count ? null : token_list[current_token_idx]; } }
        internal string CurrentValue { get { return current_token_idx >= token_list.Count ? null : token_list[current_token_idx].value; } }
        //internal AToken Previous { get { return current_token_idx > 0 && token_list.Count > 0 ? token_list[current_token_idx - 1] : null; } }
        internal AToken Next()
        {
            if (current_token_idx < token_list.Count) current_token_idx++;
            return current_token_idx < token_list.Count ? token_list[current_token_idx] : null;
        }
        internal AToken PeekToken(int offsetIdx)
        {
            return (current_token_idx + offsetIdx >= 0 && current_token_idx + offsetIdx < token_list.Count) ? token_list[current_token_idx + offsetIdx] : null;
        }
        internal string PeekTokenValue(int offsetIdx)
        {
            AToken tok = this.PeekToken(offsetIdx);
            return tok == null ? null : tok.value;
        }
        internal TokenStore NextUntilOperatorClose()
        {
            AToken start_tok = this.Current;
            int level = 1;
            List<AToken> list = new List<AToken>();
            for (this.current_token_idx++; this.current_token_idx < this.token_list.Count; this.current_token_idx++)
            {
                AToken curr_tok = this.token_list[this.current_token_idx];
                if (curr_tok.op != null)
                {
                    if (start_tok.value == curr_tok.value) level++;
                    else if (start_tok.value == "(" && curr_tok.value == ")" // ( )
                            || start_tok.value == "[" && curr_tok.value == "]"       // [ ]
                            || start_tok.value == "{" && curr_tok.value == "}"         // { }
                            || start_tok.value == "<" && curr_tok.value == ">")          // < >
                        level--;
                    else if (start_tok.value == "<" && curr_tok.value == ">>" && level >= 2)    // v<xxx<xx>>
                    {
                        level -= 2;
                        list.Add(new AToken(curr_tok.start_pos, TOKEN_TYPE.OPERATOR, AnOperator.dicSystemOperator[">"], ">"));
                        curr_tok = new AToken(curr_tok.start_pos + 1, TOKEN_TYPE.OPERATOR, AnOperator.dicSystemOperator[">"], ">");
                    }
                    if (level == 0) break;
                }
                list.Add(curr_tok);
            }
            if (level != 0) throw new ExprException("Expecting closing operator for operator: '" + start_tok.value + "'");
            this.current_token_idx++;   // move to next token after close
            return new TokenStore(this.original_string, list);
        }
    }

}
