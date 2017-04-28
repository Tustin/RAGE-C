﻿using System;
using System.Text.RegularExpressions;

namespace RAGE.Parser.Opcodes
{
	public static class Push
	{
		public static string Float(string value)
		{
			if (float.TryParse(value, out float val))
			{
				if (val >= -1.0f && val <= 7.0f && val % 1 == 0)
				{
					return $"Fpush_{val}";
				}
				else
				{
					return $"Fpush {val}";
				}
			}
			else
			{
				throw new Exception("Assumed float, but unable to parse");
			}
		}

		public static string Bool(string value)
		{
			if (value.ToLower() == "true" || value.ToLower() == "false")
			{
				return value.ToLower() == "true" ? "Push_1" : "Push_0";
			}
			else
			{
				throw new Exception("Assumed bool, but unable to parse");
			}
		}

		public static string String(string value)
		{
			Regex regex = new Regex(@"^""[^""]*""$");
			if (!regex.IsMatch(value))
			{
				throw new Exception("Assumed string, but unable to parse");
			}
			return $"PushString {value}";
		}

		public static string Int(string value)
		{
			if (int.TryParse(value, out int ival))
			{
				if (ival >= -1 && ival <= 7)
				{
					return $"Push_{ival}";
				}
				else if (ival <= 255 && ival >= -255)
				{
					return $"Push1 {ival}";
				} //short (16 bits)
				else if (ival > 255 && ival <= Int16.MaxValue)
				{
					return $"PushS {ival}";
				} //24 bits (why rockstar?)
				else if (ival > Int16.MaxValue && ival <= 16777215)
				{
					return $"PushI24 {ival}";
				}
				else
				{
					return $"Push {ival}";
				}
			}
			else
			{
				throw new Exception("Assumed int, but unable to parse");
			}
		}

		public static string Generate(string value, DataType valueType)
		{
			switch (valueType)
			{
				case DataType.Bool:
				return Bool(value);
				case DataType.Float:
				return Float(value);
				case DataType.String:
				return String(value);
				case DataType.Int:
				return Int(value);
				default:
				return null;
			}
		}
	}
}