/*
    NiceJson 1.3.1 (2017-07-26)

    MIT License
    ===========

    Copyright (C) 2015 Ángel Quiroga Mendoza <me@angelqm.com>

    Permission is hereby granted, free of charge, to any person obtaining a copy of
    this software and associated documentation files (the "Software"), to deal in
    the Software without restriction, including without limitation the rights to
    use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
    of the Software, and to permit persons to whom the Software is furnished to do
    so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

    Appreciation Contributions:
        Rayco Sánchez García <raycosanchezgarcia@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Text;

namespace NiceJson
{
	[Serializable]
	public abstract class JsonNode
	{
		protected const char PP_IDENT_CHAR = '\t'; //Modify this to spaces or whatever char you want to be the ident one
		protected const int PP_IDENT_COUNT = 1; //Modify this to be the numbers of IDENT_CHAR x identation
		protected const bool ESCAPE_SOLIDUS = false; //If you are going to to embed this json in html, you can turn this on ref: http://andowebsit.es/blog/noteslog.com/post/the-solidus-issue/

		protected const char CHAR_CURLY_OPEN = '{';
		protected const char CHAR_CURLY_CLOSED = '}';
		protected const char CHAR_SQUARED_OPEN = '[';
		protected const char CHAR_SQUARED_CLOSED = ']';

		protected const char CHAR_COLON = ':';
		protected const char CHAR_COMMA = ',';
		protected const char CHAR_QUOTE = '"';

		protected const char CHAR_NULL_LITERAL = 'n';
		protected const char CHAR_TRUE_LITERAL = 't';
		protected const char CHAR_FALSE_LITERAL = 'f';

		protected const char CHAR_SPACE = ' ';

		protected const char CHAR_BS = '\b';
		protected const char CHAR_FF = '\f';
		protected const char CHAR_RF = '\r';
		protected const char CHAR_NL = '\n';
		protected const char CHAR_HT = '\t';
		protected const char CHAR_ESCAPE = '\\';
		protected const char CHAR_SOLIDUS = '/';
		protected const char CHAR_ESCAPED_QUOTE = '\"';

		protected const char CHAR_N = 'n';
		protected const char CHAR_R = 'r';
		protected const char CHAR_B = 'b';
		protected const char CHAR_T = 't';
		protected const char CHAR_F = 'f';
		protected const char CHAR_U = 'u';

		protected const string STRING_ESCAPED_BS = "\\b";
		protected const string STRING_ESCAPED_FF = "\\f";
		protected const string STRING_ESCAPED_RF = "\\r";
		protected const string STRING_ESCAPED_NL = "\\n";
		protected const string STRING_ESCAPED_TAB = "\\t";
		protected const string STRING_ESCAPED_ESCAPE = "\\\\";
		protected const string STRING_ESCAPED_SOLIDUS = "\\/";
		protected const string STRING_ESCAPED_ESCAPED_QUOTE = "\\\"";

		protected const string STRING_SPACE = " ";
		protected const string STRING_LITERAL_NULL = "null";
		protected const string STRING_LITERAL_TRUE = "true";
		protected const string STRING_LITERAL_FALSE = "false";

		protected const string STRING_ESCAPED_UNICODE_INIT = "\\u00";

		//Indexers and accesors
		public JsonNode this[string key]
		{
			get
			{
				if (this is JsonObject)
				{
					return ((JsonObject)this)[key];
				}
				else
				{
					return null;
				}
			}

			set
			{
				if (this is JsonObject)
				{
					((JsonObject)this)[key] = value;
				}
			}
		}

		public JsonNode this[int index]
		{
			get
			{
				if (this is JsonArray)
				{
					return ((JsonArray)this)[index];
				}
				else
				{
					return null;
				}
			}

			set
			{
				if (this is JsonArray)
				{
					((JsonArray)this)[index] = value;
				}
			}
		}

		public bool ContainsKey (string key)
		{
			if (this is JsonObject)
			{
				return ((JsonObject)this).ContainsKey(key);
			}
			else
			{
				return false;
			}
		}

		//escaping logic

		//Escaping/Unescaping logic
		protected static string EscapeString(string s)
		{
			StringBuilder result = new StringBuilder();

			foreach (char c in s)
			{
				switch (c)
				{
				case CHAR_ESCAPE:
					{
						result.Append(STRING_ESCAPED_ESCAPE);
					}
					break;
				case CHAR_SOLIDUS:
					{
						#pragma warning disable
						if (ESCAPE_SOLIDUS)
						{
							result.Append(STRING_ESCAPED_SOLIDUS);
						}
						else
						{
							result.Append(c);
						}
						#pragma warning restore
					}
					break;
				case CHAR_ESCAPED_QUOTE:
					{
						result.Append (STRING_ESCAPED_ESCAPED_QUOTE);
					}
					break;
				case CHAR_NL:
					{
						result.Append (STRING_ESCAPED_NL);
					}
					break;
				case CHAR_RF:
					{
						result.Append (STRING_ESCAPED_RF);
					}
					break;
				case CHAR_HT:
					{
						result.Append (STRING_ESCAPED_TAB);
					}
					break;
				case CHAR_BS:
					{
						result.Append (STRING_ESCAPED_BS);
					}
					break;
				case CHAR_FF:
					{
						result.Append (STRING_ESCAPED_FF);
					}
					break;
				default:
					if (c < CHAR_SPACE)
					{
						result.Append (STRING_ESCAPED_UNICODE_INIT + Convert.ToByte(c).ToString("x2").ToUpper());
					}
					else
					{
						result.Append(c);
					}
					break;
				}
			}

			return result.ToString();
		}

		protected static string UnescapeString(string s)
		{
			StringBuilder result = new StringBuilder(s.Length);

			for (int i=0;i<s.Length;i++)
			{
				if (s[i] == CHAR_ESCAPE)
				{
					i++;

					switch(s[i])
					{
					case CHAR_ESCAPE:
						{
							result.Append(s[i]);
						}
						break;
					case CHAR_SOLIDUS:
						{
							result.Append(s[i]);
						}
						break;
					case CHAR_ESCAPED_QUOTE:
						{
							result.Append(s[i]);
						}
						break;
					case CHAR_N:
						{
							result.Append(CHAR_NL);
						}
						break;
					case CHAR_R:
						{
							result.Append(CHAR_RF);
						}
						break;
					case CHAR_T:
						{
							result.Append(CHAR_HT);
						}
						break;
					case CHAR_B:
						{
							result.Append(CHAR_BS);
						}
						break;
					case CHAR_F:
						{
							result.Append(CHAR_FF);
						}
						break;
					case CHAR_U:
						{
							result.Append((char) int.Parse(s.Substring(i+1,4),NumberStyles.AllowHexSpecifier));
							i = i + 4;
						}
						break;
					default:
						{
							result.Append(s[i]);
						}
						break;
					}
				}
				else
				{
					result.Append(s[i]);
				}
			}

			return result.ToString();
		}

		//setter implicit casting 

		public static implicit operator JsonNode(string value)
		{
			return new JsonBasic(value);
		}

		public static implicit operator JsonNode(int value)
		{
			return new JsonBasic(value);
		}

		public static implicit operator JsonNode(long value)
		{
			return new JsonBasic(value);
		}

		public static implicit operator JsonNode(float value)
		{
			return new JsonBasic(value);
		}

		public static implicit operator JsonNode(double value)
		{
			return new JsonBasic(value);
		}

		public static implicit operator JsonNode(decimal value)
		{
			return new JsonBasic(value);
		}

		public static implicit operator JsonNode(bool value)
		{
			return new JsonBasic(value);
		}

		//getter implicit casting 

		public static implicit operator string(JsonNode value)
		{
			if (value != null)
			{
				return value.ToString();
			}
			else
			{
				return null;
			}
		}

		public static implicit operator int(JsonNode value)
		{
			return (int) Convert.ChangeType(((JsonBasic)value).ValueObject,typeof(int));
		}

		public static implicit operator long (JsonNode value)
		{
			return (long) Convert.ChangeType(((JsonBasic)value).ValueObject, typeof(long));
		}

		public static implicit operator float(JsonNode value)
		{
			return (float) Convert.ChangeType(((JsonBasic)value).ValueObject, typeof(float));
		}

		public static implicit operator double(JsonNode value)
		{
			return (double) Convert.ChangeType(((JsonBasic)value).ValueObject, typeof(double));
		}

		public static implicit operator decimal (JsonNode value)
		{
			return (decimal) Convert.ChangeType(((JsonBasic)value).ValueObject, typeof(decimal));
		}

		public static implicit operator bool(JsonNode value)
		{
			return (bool) Convert.ChangeType(((JsonBasic)value).ValueObject, typeof(bool));
		}

		//Parsing logic
		public static JsonNode ParseJsonString(string jsonString)
		{
			return ParseJsonPart(RemoveNonTokenChars(jsonString));
		}

		private static JsonNode ParseJsonPart(string jsonPart)
		{
			JsonNode jsonPartValue = null;

			if (jsonPart.Length == 0) 
			{
				return jsonPartValue;   
			}

			switch (jsonPart[0])
			{
			case JsonNode.CHAR_CURLY_OPEN:
				{
					JsonObject jsonObject = new JsonObject();
					List<string> splittedParts = SplitJsonParts(jsonPart.Substring(1, jsonPart.Length - 2));

					string[] keyValueParts = new string[2];
					foreach (string keyValuePart in splittedParts) 
					{
						keyValueParts = SplitKeyValuePart (keyValuePart);
						if (keyValueParts [0] != null) 
						{
							jsonObject [JsonNode.UnescapeString (keyValueParts [0])] = ParseJsonPart (keyValueParts [1]);
						}
					}
					jsonPartValue = jsonObject;
				}
				break;
			case JsonNode.CHAR_SQUARED_OPEN:
				{
					JsonArray jsonArray = new JsonArray();
					List<string> splittedParts = SplitJsonParts(jsonPart.Substring(1, jsonPart.Length - 2));

					foreach (string part in splittedParts)
					{
						if (part.Length > 0) 
						{
							jsonArray.Add (ParseJsonPart (part));
						}
					}
					jsonPartValue = jsonArray;
				}
				break;
			case JsonNode.CHAR_QUOTE:
				{
					jsonPartValue = new JsonBasic(JsonNode.UnescapeString(jsonPart.Substring(1, jsonPart.Length - 2)));
				}
				break;
			case JsonNode.CHAR_FALSE_LITERAL://false
				{
					jsonPartValue = new JsonBasic(false);
				}
				break;
			case JsonNode.CHAR_TRUE_LITERAL://true
				{
					jsonPartValue = new JsonBasic(true);
				}
				break;
			case JsonNode.CHAR_NULL_LITERAL://null
				{
					jsonPartValue = null;
				}
				break;
			default://it must be a number or it will fail
				{
					long longValue = 0;
					if (long.TryParse(jsonPart, NumberStyles.Any, CultureInfo.InvariantCulture, out longValue))
					{
						if (longValue > int.MaxValue || longValue < int.MinValue)
						{
							jsonPartValue = new JsonBasic(longValue);
						}
						else
						{
							jsonPartValue = new JsonBasic((int)longValue);
						}
					}
					else
					{
						decimal decimalValue = 0;
						if (decimal.TryParse(jsonPart, NumberStyles.Any, CultureInfo.InvariantCulture, out decimalValue))
						{
							jsonPartValue = new JsonBasic(decimalValue);
						}
					}
				}
				break;
			}

			return jsonPartValue;
		}

		private static List<string> SplitJsonParts(string json)
		{
			List<string> jsonParts = new List<string>();
			int identLevel = 0;
			int lastPartChar = 0;
			bool inString = false;

			for (int i = 0; i < json.Length; i++)
			{
				switch (json[i])
				{
				case JsonNode.CHAR_COMMA:
					{
						if (!inString && identLevel == 0)
						{
							jsonParts.Add(json.Substring(lastPartChar, i - lastPartChar));
							lastPartChar = i + 1;
						}
					}
					break;
				case JsonNode.CHAR_QUOTE:
					{
						if (i == 0 || (json[i - 1] != JsonNode.CHAR_ESCAPE))
						{
							inString = !inString;
						}
					}
					break;
				case JsonNode.CHAR_CURLY_OPEN:
				case JsonNode.CHAR_SQUARED_OPEN:
					{
						if (!inString)
						{
							identLevel++;
						}
					}
					break;
				case JsonNode.CHAR_CURLY_CLOSED:
				case JsonNode.CHAR_SQUARED_CLOSED:
					{
						if (!inString)
						{
							identLevel--;
						}
					}
					break;
				}
			}

			jsonParts.Add(json.Substring(lastPartChar));

			return jsonParts;
		}

		private static string[] SplitKeyValuePart(string json)
		{
			string[] parts = new string[2];
			bool inString = false;

			bool found = false;
			int index = 0;

			while (index < json.Length && !found)
			{
				if (json[index] == JsonNode.CHAR_QUOTE && (index == 0 || (json[index - 1] != JsonNode.CHAR_ESCAPE)))
				{
					if (!inString)
					{
						inString = true;
						index++;
					}
					else
					{
						parts[0] = json.Substring(1, index - 1);
						parts[1] = json.Substring(index + 2);//+2 because of the :
						found = true;
					}
				}
				else
				{
					index++;
				}
			}

			return parts;
		}

		private static string RemoveNonTokenChars(string s)
		{
			int len = s.Length;
			char[] s2 = new char[len];
			int currentPos = 0;
			bool outString = true;
			for (int i = 0; i < len; i++)
			{
				char c = s[i];
				if (c == JsonNode.CHAR_QUOTE)
				{
					if (i == 0 || (s[i - 1] != JsonNode.CHAR_ESCAPE))
					{
						outString = !outString;
					}
				}

				if (!outString || (
					(c != JsonNode.CHAR_SPACE) &&
					(c != JsonNode.CHAR_RF) &&
					(c != JsonNode.CHAR_NL) &&
					(c != JsonNode.CHAR_HT) &&
					(c != JsonNode.CHAR_BS) &&
					(c != JsonNode.CHAR_FF)
				))
				{
					s2[currentPos++] = c;
				}
			}
			return new String(s2, 0, currentPos);
		}

		//Object logic

		public abstract string ToJsonString();

		public string ToJsonPrettyPrintString()
		{
			string jsonString = this.ToJsonString();

			string identStep = string.Empty;
			for (int i = 0; i < PP_IDENT_COUNT; i++)
			{
				identStep += PP_IDENT_CHAR;
			}

			bool inString = false;

			string currentIdent = string.Empty;
			for (int i = 0; i < jsonString.Length; i++)
			{
				switch (jsonString[i])
				{
				case CHAR_COLON:
					{
						if (!inString)
						{
							jsonString = jsonString.Insert(i + 1, STRING_SPACE);
						}
					}
					break;
				case CHAR_QUOTE:
					{
						if (i == 0 || (jsonString[i - 1] != CHAR_ESCAPE))
						{
							inString = !inString;
						}
					}
					break;
				case CHAR_COMMA:
					{
						if (!inString)
						{
							jsonString = jsonString.Insert(i + 1, CHAR_NL + currentIdent);
						}
					}
					break;
				case CHAR_CURLY_OPEN:
				case CHAR_SQUARED_OPEN:
					{
						if (!inString)
						{
							currentIdent += identStep;
							jsonString = jsonString.Insert(i + 1, CHAR_NL + currentIdent);
						}
					}
					break;
				case CHAR_CURLY_CLOSED:
				case CHAR_SQUARED_CLOSED:
					{
						if (!inString)
						{
							currentIdent = currentIdent.Substring(0, currentIdent.Length - identStep.Length);
							jsonString = jsonString.Insert(i, CHAR_NL + currentIdent);
							i += currentIdent.Length + 1;
						}
					}
					break;
				}
			}

			return jsonString;
		}
	}

	[Serializable]
	public class JsonBasic : JsonNode
	{
		public object ValueObject
		{
			get
			{
				return m_value;
			}
		}

		private object m_value;

		public JsonBasic(object value)
		{
			m_value = value;
		}

		public override string ToString()
		{
			return m_value.ToString();
		}

		public override string ToJsonString ()
		{
			if (m_value == null)
			{
				return STRING_LITERAL_NULL;
			}
			else if (m_value is string)
			{
				return CHAR_QUOTE + EscapeString(m_value.ToString()) + CHAR_QUOTE;
			}
			else if (m_value is bool)
			{
				if ((bool) m_value)
				{
					return STRING_LITERAL_TRUE;
				}
				else
				{
					return STRING_LITERAL_FALSE;
				}
			}
			else
			{
				return m_value.ToString();
			}
		}

	}

	[Serializable]
	public class JsonObject : JsonNode, IEnumerable
	{
		private Dictionary<string,JsonNode> m_dictionary = new Dictionary<string, JsonNode>();

		public Dictionary<string,JsonNode>.KeyCollection Keys
		{
			get
			{
				return m_dictionary.Keys;
			}
		}

		public Dictionary<string, JsonNode>.ValueCollection Values
		{
			get
			{
				return m_dictionary.Values;
			}
		}

		public new JsonNode this[string key]
		{
			get
			{
				return m_dictionary[key];
			}

			set
			{
				m_dictionary[key] = value;
			}
		}

		public void Add(string key, JsonNode value)
		{
			m_dictionary.Add(key, value);
		}

		public bool Remove(string key)
		{
			return m_dictionary.Remove(key);
		}

		public new bool ContainsKey(string key)
		{
			return m_dictionary.ContainsKey(key);
		}

		public bool ContainsValue(JsonNode value)
		{
			return m_dictionary.ContainsValue(value);
		}

		public void Clear()
		{
			m_dictionary.Clear();
		}

		public int Count
		{
			get
			{
				return m_dictionary.Count;
			}
		}

		public IEnumerator GetEnumerator()
		{
			foreach (KeyValuePair<string, JsonNode> jsonKeyValue in m_dictionary)
			{
				yield return jsonKeyValue;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToJsonString ()
		{
			if (m_dictionary == null)
			{
				return STRING_LITERAL_NULL;
			}
			else
			{
				StringBuilder jsonString = new StringBuilder();
				jsonString.Append(CHAR_CURLY_OPEN);
				foreach (string key in m_dictionary.Keys)
				{
					jsonString.Append(CHAR_QUOTE);
					jsonString.Append(EscapeString(key));
					jsonString.Append(CHAR_QUOTE);
					jsonString.Append(CHAR_COLON);

					if (m_dictionary[key] != null)
					{
						jsonString.Append(m_dictionary[key].ToJsonString());
					}
					else
					{
						jsonString.Append(STRING_LITERAL_NULL);
					}
					jsonString.Append(CHAR_COMMA);
				}
				if (jsonString[jsonString.Length -1] == CHAR_COMMA)
				{
					jsonString.Remove(jsonString.Length -1,1);
				}
				jsonString.Append(CHAR_CURLY_CLOSED);

				return jsonString.ToString();
			}

		}
	}

	[Serializable]
	public class JsonArray : JsonNode, IEnumerable<JsonNode>
	{
		private List<JsonNode> m_list = new List<JsonNode>();

		public int Count
		{
			get
			{
				return m_list.Count;
			}
		}

		public new JsonNode this[int index]
		{
			get
			{
				return m_list[index];
			}

			set
			{
				m_list[index] = value;
			}
		}

		public IEnumerator<JsonNode> GetEnumerator()
		{
			foreach (JsonNode value in m_list)
			{
				yield return value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		//expose some methods of list extends with needs

		public void Add(JsonNode item)
		{
			m_list.Add(item);
		}

		public void AddRange(IEnumerable<JsonNode> collection)
		{
			m_list.AddRange(collection);
		}

		public void Insert(int index,JsonNode item)
		{
			m_list.Insert(index,item);
		}

		public void InsertRange(int index,IEnumerable<JsonNode> collection)
		{
			m_list.InsertRange(index,collection);
		}

		public void RemoveAt(int index)
		{
			m_list.RemoveAt(index);
		}

		public bool Remove(JsonNode item)
		{
			return m_list.Remove(item);
		}

		public void Clear()
		{
			m_list.Clear();
		}

		//end exposed methods


		public override string ToJsonString ()
		{
			if (m_list == null)
			{
				return STRING_LITERAL_NULL;
			}
			else
			{
				StringBuilder jsonString = new StringBuilder();
				jsonString.Append (CHAR_SQUARED_OPEN);
				foreach (JsonNode value in m_list)
				{
					if (value != null)
					{
						jsonString.Append (value.ToJsonString());
					}
					else
					{
						jsonString.Append (STRING_LITERAL_NULL);
					}

					jsonString.Append (CHAR_COMMA);
				}
				if (jsonString[jsonString.Length-1] == CHAR_COMMA)
				{
					jsonString.Remove(jsonString.Length -1,1);
				}
				jsonString.Append (CHAR_SQUARED_CLOSED);

				return jsonString.ToString();
			}
		}
	}
}