using System.Linq.Expressions;

namespace Simpro.Expr
{
    internal class JumpInfo
    {
        public LabelExpression breakLabel;
        public LabelExpression continueLabel;
    }
}
