using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Markovcd.Tests
{
    using Classes;

    [TestClass]
    public class FunctionParserTest
    {
        /*[TestMethod]
        public void ParseToLambda_SimpleSingleArgument()
        {
            var actual = FunctionParser.ParseToLambda("f(x)=2*x", typeof(int));
            var expected = "(Int32 x) => 2*x";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseToLambda_MultipleArguments()
        {
            var actual = FunctionParser.ParseToLambda("f(x ,  x2, x3, x4)=    x4/x - x2 * x3  ", typeof(double));
            var expected = "(Double x, Double x2, Double x3, Double x4) => x4/x - x2 * x3";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseLambda_SimpleSingleArgument()
        {
            var actual = FunctionParser.ParseLambda("f(x)=2*x", typeof(int)).ToString();
            var expected = "x => (2 * x)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseLambda_MultipleArguments()
        {
            var actual = FunctionParser.ParseLambda("f(x1 ,  x2, x3, x4)=    x4/x1 - x2 * x3  ", typeof(int)).ToString();
            var expected = "x => (2 * x)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TrimParenthesesTest_Ordinary()
        { 
            var actual = FunctionParser.TrimParentheses("(some expression)");
            var expected = "some expression";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TrimParenthesesTest_NoParentheses()
        {
            var actual = FunctionParser.TrimParentheses("some expression");
            var expected = "some expression";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TrimParenthesesTest_LeftParenthesis()
        {
            var actual = FunctionParser.TrimParentheses("(some expression");
            var expected = "(some expression";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TrimParenthesesTest_RightParenthesis()
        {
            var actual = FunctionParser.TrimParentheses("some expression)");
            var expected = "some expression)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TrimParenthesesTest_FullExpression()
        {
            var actual = FunctionParser.TrimParentheses("((x1, x2) => x2 * Sin(x1/x2) - 34 + x1)");
            var expected = "(x1, x2) => x2 * Sin(x1/x2) - 34 + x1";

            Assert.AreEqual(expected, actual);
        }*/
    }
}
