using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public enum TokenType
    {
        Symbol,
        Operation,
        Keyword,
        String,
        Number,
        OpenParens,
        CloseParens,
        Semicolon,
        Comma,
        Equals,
        NotEquals,
        OpenCurly,
        CloseCurly,
        Type,
        GreaterThan,
        LessThan,
        Not,
        //Math
        Add,
        Subtract,
    }
}
