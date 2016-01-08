
namespace Simpro.Expr
{
    internal class AToken
    {
        internal int start_pos;
        //internal int end_pos;
        internal TOKEN_TYPE tok_type;
        internal AnOperator op;
        internal string value;

        internal AToken(int start_pos/*, int end_pos*/, TOKEN_TYPE tok_type, AnOperator op, string value)
        {
            this.start_pos = start_pos;
            //this.end_pos = end_pos;
            this.tok_type = tok_type;
            this.op = op;
            this.value = value;
        }


    }
}
