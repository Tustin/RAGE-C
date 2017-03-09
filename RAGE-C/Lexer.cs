using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGE
{
    public class Lexer
    {
        public static List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            foreach (string line in Core.SourceCode)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];

                    if (c == '\n' || c == ' ') continue;

                    if (c == '(') tokens.Add(new Token(TokenType.OpenParens, c));
                    else if (c == ')') tokens.Add(new Token(TokenType.CloseParens, c));
                    else if (c == '{') tokens.Add(new Token(TokenType.OpenCurly, c));
                    else if (c == '}') tokens.Add(new Token(TokenType.CloseCurly, c));
                    else if (c == ';') tokens.Add(new Token(TokenType.Semicolon, c));
                    else if (c == ',') tokens.Add(new Token(TokenType.Comma, c));
                    else if (c == '=') tokens.Add(new Token(TokenType.Equals, c));
                    else if (c == '+') tokens.Add(new Token(TokenType.Add, c));
                    else if (c == '-') tokens.Add(new Token(TokenType.Subtract, c));
                    else if (c == '<') tokens.Add(new Token(TokenType.LessThan, c));
                    else if (c == '>') tokens.Add(new Token(TokenType.GreaterThan, c));
                    else if (c == '!') tokens.Add(new Token(TokenType.Not, c));
                    else if (Regex.IsMatch(c.ToString(), "[.0-9]"))
                    {
                        tokens.Add(new Token(TokenType.Number, ScanNumber(line, i, out i)));
                        continue;
                    }
                    else if (c == '"' || c == '\'')
                    {
                        tokens.Add(new Token(TokenType.String, ScanString(line, c, i + 1, out i)));
                        continue;
                    } //Symbols
                    else if (Regex.IsMatch(c.ToString(), "[_a-zA-Z]"))
                    {
                        tokens.Add(ScanSymbol(line, i, out i));
                        continue;
                    }
                }

            }
            return tokens;
        }

        private static Token ScanSymbol(string line, int index, out int endingIndex)
        {
            Regex reg = new Regex("[_a-zA-Z]");
            string final = null;
            endingIndex = 0;
            for (int i = index; i < line.Length; i++)
            {
                endingIndex = i - 1;
                if (!reg.IsMatch(line[i].ToString())) break;
                final += line[i];
                endingIndex = i;
            }
            if (Keyword.IsMatch(final, out string keyword)) return new Token(TokenType.Keyword, keyword);
            if (Core.IsDataType(final)) return new Token(TokenType.Type, final);
            return new Token(TokenType.Symbol, final);
        }

        private static string ScanString(string line, char stringType, int index, out int endingIndex)
        {
            string final = null;
            for (int i = index; i < line.Length; i++)
            {
                if (line[i] == stringType)
                {
                    endingIndex = i;
                    return final;
                }
                final += line[i];

            }
            throw new Exception($"String has no ending {stringType}");
        }

        private static string ScanNumber(string line, int index, out int endingIndex)
        {
            Regex reg = new Regex("[.0-9]");
            string final = null;
            endingIndex = 0;
            for (int i = index; i < line.Length; i++)
            {
                endingIndex = i;
                if (!reg.IsMatch(line[i].ToString())) return final;
                final += line[i];
            }
            return final;
        }
    }
}
