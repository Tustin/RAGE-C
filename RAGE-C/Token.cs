using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Token
    {
        public TokenType TokenType { get; set; }
        public string TokenValue { get; set; }

        public Token(TokenType type, string value)
        {
            TokenType = type;
            TokenValue = value;
        }
        public Token(TokenType type, char value)
        {
            TokenType = type;
            TokenValue = value.ToString();
        }
    }
}
