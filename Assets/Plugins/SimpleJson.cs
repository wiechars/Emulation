//#define USE_SharpZipLib
#if !UNITY_WEBPLAYER
#define USE_FileIO
#endif

/* * * * *
 * A simple Json Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple Json parser. It can build a Json string
 * from the node-tree, or generate a node tree from any valid Json string.
 * 
 * If you want to use compression when saving to file / stream / B64 you have to include
 * SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ ) in your project and
 * define "USE_SharpZipLib" at the top of the file
 * 
 * Written by Bunny83 
 * 2012-06-09
 * 
 * Features / attributes:
 * - provides strongly typed node classes and lists / dictionaries
 * - provides easy access to class members / array items / data values
 * - the parser ignores data types. Each value is a string.
 * - only double quotes (") are used for quoting strings.
 * - values and names are not restricted to quoted strings. They simply add up and are trimmed.
 * - There are only 3 types: arrays(JsonArray), objects(JsonClass) and values(JsonData)
 * - provides "casting" properties to easily convert to / from those types:
 *   int / float / double / bool
 * - provides a common interface for each node so no explicit casting is required.
 * - the parser try to avoid errors, but if malformed Json is parsed the result is undefined
 * 
 * 
 * 2012-12-17 Update:
 * - Added internal JSONLazyCreator class which simplifies the construction of a Json tree
 *   Now you can simple reference any item that doesn't exist yet and it will return a JSONLazyCreator
 *   The class determines the required type by it's further use, creates the type and removes itself.
 * - Added binary serialization / deserialization.
 * - Added support for BZip2 zipped binary format. Requires the SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ )
 *   The usage of the SharpZipLib library can be disabled by removing or commenting out the USE_SharpZipLib define at the top
 * - The serializer uses different types when it comes to store the values. Since my data values
 *   are all of type string, the serializer will "try" which format fits best. The order is: int, float, double, bool, string.
 *   It's not the most efficient way but for a moderate amount of data it should work on all platforms.
 * 
 * * * * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
 
 
namespace SimpleJson
{
    public enum JSONBinaryTag
    {
        Array            = 1,
        Class            = 2,
        Value            = 3,
        IntValue        = 4,
        DoubleValue        = 5,
        BoolValue        = 6,
        FloatValue        = 7,
    }
 
    public class JsonNode
    {
        #region common interface
        public virtual void Add(string aKey, JsonNode aItem){ }
        public virtual JsonNode this[int aIndex]   { get { return null; } set { } }
        public virtual JsonNode this[string aKey]  { get { return null; } set { } }
        public virtual string Value                { get { return "";   } set { } }
        public virtual int Count                   { get { return 0;    } }
 
        public virtual void Add(JsonNode aItem)
        {
            Add("", aItem);
        }
 
        public virtual JsonNode Remove(string aKey) { return null; }
        public virtual JsonNode Remove(int aIndex) { return null; }
        public virtual JsonNode Remove(JsonNode aNode) { return aNode; }
 
        public virtual IEnumerable<JsonNode> Childs { get { yield break;} }
        public IEnumerable<JsonNode> DeepChilds
        {
            get
            {
                foreach (var C in Childs)
                    foreach (var D in C.DeepChilds)
                        yield return D;
            }
        }
 
        public override string ToString()
        {
            return "JsonNode";
        }
        public virtual string ToString(string aPrefix)
        {
            return "JsonNode";
        }
 
        #endregion common interface
 
        #region typecasting properties
        public virtual int AsInt
        {
            get
            {
                int v = 0;
                if (int.TryParse(Value,out v))
                    return v;
                return 0;
            }
            set
            {
                Value = value.ToString();
            }
        }
        public virtual float AsFloat
        {
            get
            {
                float v = 0.0f;
                if (float.TryParse(Value,out v))
                    return v;
                return 0.0f;
            }
            set
            {
                Value = value.ToString();
            }
        }
        public virtual double AsDouble
        {
            get
            {
                double v = 0.0;
                if (double.TryParse(Value,out v))
                    return v;
                return 0.0;
            }
            set
            {
                Value = value.ToString();
            }
        }
        public virtual bool AsBool
        {
            get
            {
                bool v = false;
                if (bool.TryParse(Value,out v))
                    return v;
                return !string.IsNullOrEmpty(Value);
            }
            set
            {
                Value = (value)?"true":"false";
            }
        }
        public virtual JsonArray AsArray
        {
            get
            {
                return this as JsonArray;
            }
        }
        public virtual JsonClass AsObject
        {
            get
            {
                return this as JsonClass;
            }
        }
 
 
        #endregion typecasting properties
 
        #region operators
        public static implicit operator JsonNode(string s)
        {
            return new JsonData(s);
        }
        public static implicit operator string(JsonNode d)
        {
            return (d == null)?null:d.Value;
        }
        public static bool operator ==(JsonNode a, object b)
        {
            if (b == null && a is JsonLazyCreator)
                return true;
            return System.Object.ReferenceEquals(a,b);
        }
 
        public static bool operator !=(JsonNode a, object b)
        {
            return !(a == b);
        }
        public override bool Equals (object obj)
        {
            return System.Object.ReferenceEquals(this, obj);
        }
        public override int GetHashCode ()
        {
            return base.GetHashCode();
        }
 
 
        #endregion operators
 
        internal static string Escape(string aText)
        {
            string result = "";
            foreach(char c in aText)
            {
                switch(c)
                {
                    case '\\' : result += "\\\\"; break;
                    case '\"' : result += "\\\""; break;
                    case '\n' : result += "\\n" ; break;
                    case '\r' : result += "\\r" ; break;
                    case '\t' : result += "\\t" ; break;
                    case '\b' : result += "\\b" ; break;
                    case '\f' : result += "\\f" ; break;
                    default   : result += c     ; break;
                }
            }
            return result;
        }
 
        public static JsonNode Parse(string aJSON)
        {
            Stack<JsonNode> stack = new Stack<JsonNode>();
            JsonNode ctx = null;
            int i = 0;
            string Token = "";
            string TokenName = "";
            bool QuoteMode = false;
            while (i < aJSON.Length)
            {
                switch (aJSON[i])
                {
                    case '{':
                        if (QuoteMode)
                        {
                            Token += aJSON[i];
                            break;
                        }
                        stack.Push(new JsonClass());
                        if (ctx != null)
                        {
                            TokenName = TokenName.Trim();
                            if (ctx is JsonArray)
                                ctx.Add(stack.Peek());
                            else if (TokenName != "")
                                ctx.Add(TokenName,stack.Peek());
                        }
                        TokenName = "";
                        Token = "";
                        ctx = stack.Peek();
                    break;
 
                    case '[':
                        if (QuoteMode)
                        {
                            Token += aJSON[i];
                            break;
                        }
 
                        stack.Push(new JsonArray());
                        if (ctx != null)
                        {
                            TokenName = TokenName.Trim();
                            if (ctx is JsonArray)
                                ctx.Add(stack.Peek());
                            else if (TokenName != "")
                                ctx.Add(TokenName,stack.Peek());
                        }
                        TokenName = "";
                        Token = "";
                        ctx = stack.Peek();
                    break;
 
                    case '}':
                    case ']':
                        if (QuoteMode)
                        {
                            Token += aJSON[i];
                            break;
                        }
                        if (stack.Count == 0)
                            throw new Exception("Json Parse: Too many closing brackets");
 
                        stack.Pop();
                        if (Token != "")
                        {
                            TokenName = TokenName.Trim();
                            if (ctx is JsonArray)
                                ctx.Add(Token);
                            else if (TokenName != "")
                                ctx.Add(TokenName,Token);
                        }
                        TokenName = "";
                        Token = "";
                        if (stack.Count>0)
                            ctx = stack.Peek();
                    break;
 
                    case ':':
                        if (QuoteMode)
                        {
                            Token += aJSON[i];
                            break;
                        }
                        TokenName = Token;
                        Token = "";
                    break;
 
                    case '"':
                        QuoteMode ^= true;
                    break;
 
                    case ',':
                        if (QuoteMode)
                        {
                            Token += aJSON[i];
                            break;
                        }
                        if (Token != "")
                        {
                            if (ctx is JsonArray)
                                ctx.Add(Token);
                            else if (TokenName != "")
                                ctx.Add(TokenName, Token);
                        }
                        TokenName = "";
                        Token = "";
                    break;
 
                    case '\r':
                    case '\n':
                    break;
 
                    case ' ':
                    case '\t':
                        if (QuoteMode)
                            Token += aJSON[i];
                    break;
 
                    case '\\':
                        ++i;
                        if (QuoteMode)
                        {
                            char C = aJSON[i];
                            switch (C)
                            {
                                case 't' : Token += '\t'; break;
                                case 'r' : Token += '\r'; break;
                                case 'n' : Token += '\n'; break;
                                case 'b' : Token += '\b'; break;
                                case 'f' : Token += '\f'; break;
                                case 'u':
                                {
                                    string s = aJSON.Substring(i+1,4);
                                    Token += (char)int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
                                    i += 4;
                                    break;
                                }
                                default  : Token += C; break;
                            }
                        }
                    break;
 
                    default:
                        Token += aJSON[i];
                    break;
                }
                ++i;
            }
            if (QuoteMode)
            {
                throw new Exception("Json Parse: Quotation marks seems to be messed up.");
            }
            return ctx;
        }
 
        public virtual void Serialize(System.IO.BinaryWriter aWriter) {}
 
        public void SaveToStream(System.IO.Stream aData)
        {
            var W = new System.IO.BinaryWriter(aData);
            Serialize(W);
        }
 
        #if USE_SharpZipLib
        public void SaveToCompressedStream(System.IO.Stream aData)
        {
            using (var gzipOut = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(aData))
            {
                gzipOut.IsStreamOwner = false;
                SaveToStream(gzipOut);
                gzipOut.Close();
            }
        }
 
        public void SaveToCompressedFile(string aFileName)
        {
            #if USE_FileIO
            System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
            using(var F = System.IO.File.OpenWrite(aFileName))
            {
                SaveToCompressedStream(F);
            }
            #else
            throw new Exception("Can't use File IO stuff in webplayer");
            #endif
        }
        public string SaveToCompressedBase64()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                SaveToCompressedStream(stream);
                stream.Position = 0;
                return System.Convert.ToBase64String(stream.ToArray());
            }
        }
 
        #else
        public void SaveToCompressedStream(System.IO.Stream aData)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJson");
        }
        public void SaveToCompressedFile(string aFileName)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJson");
        }
        public string SaveToCompressedBase64()
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJson");
        }
        #endif
        
        public void SaveToFile(string aFileName)
        {
            #if USE_FileIO
            System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
            using(var F = System.IO.File.OpenWrite(aFileName))
            {
                SaveToStream(F);
            }
            #else
            throw new Exception("Can't use File IO stuff in webplayer");
            #endif
        }
        public string SaveToBase64()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                SaveToStream(stream);
                stream.Position = 0;
                return System.Convert.ToBase64String(stream.ToArray());
            }
        }
        public static JsonNode Deserialize(System.IO.BinaryReader aReader)
        {
            JSONBinaryTag type = (JSONBinaryTag)aReader.ReadByte();
            switch(type)
            {
            case JSONBinaryTag.Array:
            {
                int count = aReader.ReadInt32();
                JsonArray tmp = new JsonArray();
                for(int i = 0; i < count; i++)
                    tmp.Add(Deserialize(aReader));
                return tmp;
            }
            case JSONBinaryTag.Class:
            {
                int count = aReader.ReadInt32();                
                JsonClass tmp = new JsonClass();
                for(int i = 0; i < count; i++)
                {
                    string key = aReader.ReadString();
                    var val = Deserialize(aReader);
                    tmp.Add(key, val);
                }
                return tmp;
            }
            case JSONBinaryTag.Value:
            {
                return new JsonData(aReader.ReadString());
            }
            case JSONBinaryTag.IntValue:
            {
                return new JsonData(aReader.ReadInt32());
            }
            case JSONBinaryTag.DoubleValue:
            {
                return new JsonData(aReader.ReadDouble());
            }
            case JSONBinaryTag.BoolValue:
            {
                return new JsonData(aReader.ReadBoolean());
            }
            case JSONBinaryTag.FloatValue:
            {
                return new JsonData(aReader.ReadSingle());
            }
 
            default:
            {
                throw new Exception("Error deserializing Json. Unknown tag: " + type);
            }
            }
        }
 
        #if USE_SharpZipLib
        public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
        {
            var zin = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(aData);
            return LoadFromStream(zin);
        }
        public static JSONNode LoadFromCompressedFile(string aFileName)
        {
            #if USE_FileIO
            using(var F = System.IO.File.OpenRead(aFileName))
            {
                return LoadFromCompressedStream(F);
            }
            #else
            throw new Exception("Can't use File IO stuff in webplayer");
            #endif
        }
        public static JSONNode LoadFromCompressedBase64(string aBase64)
        {
            var tmp = System.Convert.FromBase64String(aBase64);
            var stream = new System.IO.MemoryStream(tmp);
            stream.Position = 0;
            return LoadFromCompressedStream(stream);
        }
        #else
        public static JsonNode LoadFromCompressedFile(string aFileName)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJson");
        }
        public static JsonNode LoadFromCompressedStream(System.IO.Stream aData)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJson");
        }
        public static JsonNode LoadFromCompressedBase64(string aBase64)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJson");
        }
        #endif
 
        public static JsonNode LoadFromStream(System.IO.Stream aData)
        {
            using(var R = new System.IO.BinaryReader(aData))
            {
                return Deserialize(R);
            }
        }
        public static JsonNode LoadFromFile(string aFileName)
        {
            #if USE_FileIO
            using(var F = System.IO.File.OpenRead(aFileName))
            {
                return LoadFromStream(F);
            }
            #else
            throw new Exception("Can't use File IO stuff in webplayer");
            #endif
        }
        public static JsonNode LoadFromBase64(string aBase64)
        {
            var tmp = System.Convert.FromBase64String(aBase64);
            var stream = new System.IO.MemoryStream(tmp);
            stream.Position = 0;
            return LoadFromStream(stream);
        }
    } // End of JsonNode
 
    public class JsonArray : JsonNode, IEnumerable
    {
        private List<JsonNode> m_List = new List<JsonNode>();
        public override JsonNode this[int aIndex]
        {
            get
            {
                if (aIndex<0 || aIndex >= m_List.Count)
                    return new JsonLazyCreator(this);
                return m_List[aIndex];
            }
            set
            {
                if (aIndex<0 || aIndex >= m_List.Count)
                    m_List.Add(value);
                else
                    m_List[aIndex] = value;
            }
        }
        public override JsonNode this[string aKey]
        {
            get{ return new JsonLazyCreator(this);}
            set{ m_List.Add(value); }
        }
        public override int Count
        {
            get { return m_List.Count; }
        }
        public override void Add(string aKey, JsonNode aItem)
        {
            m_List.Add(aItem);
        }
        public override JsonNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_List.Count)
                return null;
            JsonNode tmp = m_List[aIndex];
            m_List.RemoveAt(aIndex);
            return tmp;
        }
        public override JsonNode Remove(JsonNode aNode)
        {
            m_List.Remove(aNode);
            return aNode;
        }
        public override IEnumerable<JsonNode> Childs
        {
            get
            {
                foreach(JsonNode N in m_List)
                    yield return N;
            }
        }
        public IEnumerator GetEnumerator()
        {
            foreach(JsonNode N in m_List)
                yield return N;
        }
        public override string ToString()
        {
            string result = "[ ";
            foreach (JsonNode N in m_List)
            {
                if (result.Length > 2)
                    result += ", ";
                result += N.ToString();
            }
            result += " ]";
            return result;
        }
        public override string ToString(string aPrefix)
        {
            string result = "[ ";
            foreach (JsonNode N in m_List)
            {
                if (result.Length > 3)
                    result += ", ";
                result += "\n" + aPrefix + "   ";                
                result += N.ToString(aPrefix+"   ");
            }
            result += "\n" + aPrefix + "]";
            return result;
        }
        public override void Serialize (System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((byte)JSONBinaryTag.Array);
            aWriter.Write(m_List.Count);
            for(int i = 0; i < m_List.Count; i++)
            {
                m_List[i].Serialize(aWriter);
            }
        }
    } // End of JsonArray
 
    public class JsonClass : JsonNode, IEnumerable
    {
        private Dictionary<string,JsonNode> m_Dict = new Dictionary<string,JsonNode>();
        public override JsonNode this[string aKey]
        {
            get
            {
                if (m_Dict.ContainsKey(aKey))
                    return m_Dict[aKey];
                else
                    return new JsonLazyCreator(this, aKey);
            }
            set
            {
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = value;
                else
                    m_Dict.Add(aKey,value);
            }
        }
        public override JsonNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return null;
                return m_Dict.ElementAt(aIndex).Value;
            }
            set
            {
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return;
                string key = m_Dict.ElementAt(aIndex).Key;
                m_Dict[key] = value;
            }
        }
        public override int Count
        {
            get { return m_Dict.Count; }
        }
 
 
        public override void Add(string aKey, JsonNode aItem)
        {
            if (!string.IsNullOrEmpty(aKey))
            {
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = aItem;
                else
                    m_Dict.Add(aKey, aItem);
            }
            else
                m_Dict.Add(Guid.NewGuid().ToString(), aItem);
        }
 
        public override JsonNode Remove(string aKey)
        {
            if (!m_Dict.ContainsKey(aKey))
                return null;
            JsonNode tmp = m_Dict[aKey];
            m_Dict.Remove(aKey);
            return tmp;        
        }
        public override JsonNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_Dict.Count)
                return null;
            var item = m_Dict.ElementAt(aIndex);
            m_Dict.Remove(item.Key);
            return item.Value;
        }
        public override JsonNode Remove(JsonNode aNode)
        {
            try
            {
                var item = m_Dict.Where(k => k.Value == aNode).First();
                m_Dict.Remove(item.Key);
                return aNode;
            }
            catch
            {
                return null;
            }
        }
 
        public override IEnumerable<JsonNode> Childs
        {
            get
            {
                foreach(KeyValuePair<string,JsonNode> N in m_Dict)
                    yield return N.Value;
            }
        }
 
        public IEnumerator GetEnumerator()
        {
            foreach(KeyValuePair<string, JsonNode> N in m_Dict)
                yield return N;
        }
        public override string ToString()
        {
            string result = "{";
            foreach (KeyValuePair<string, JsonNode> N in m_Dict)
            {
                if (result.Length > 2)
                    result += ", ";
                result += "\"" + Escape(N.Key) + "\":" + N.Value.ToString();
            }
            result += "}";
            return result;
        }
        public override string ToString(string aPrefix)
        {
            string result = "{ ";
            foreach (KeyValuePair<string, JsonNode> N in m_Dict)
            {
                if (result.Length > 3)
                    result += ", ";
                result += "\n" + aPrefix + "   ";
                result += "\"" + Escape(N.Key) + "\" : " + N.Value.ToString(aPrefix+"   ");
            }
            result += "\n" + aPrefix + "}";
            return result;
        }
        public override void Serialize (System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((byte)JSONBinaryTag.Class);
            aWriter.Write(m_Dict.Count);
            foreach(string K in m_Dict.Keys)
            {
                aWriter.Write(K);
                m_Dict[K].Serialize(aWriter);
            }
        }
    } // End of JsonClass
 
    public class JsonData : JsonNode
    {
        private string m_Data;
        public override string Value
        {
            get { return m_Data; }
            set { m_Data = value; }
        }
        public JsonData(string aData)
        {
            m_Data = aData;
        }
        public JsonData(float aData)
        {
            AsFloat = aData;
        }
        public JsonData(double aData)
        {
            AsDouble = aData;
        }
        public JsonData(bool aData)
        {
            AsBool = aData;
        }
        public JsonData(int aData)
        {
            AsInt = aData;
        }
 
        public override string ToString()
        {
            return "\"" + Escape(m_Data) + "\"";
        }
        public override string ToString(string aPrefix)
        {
            return "\"" + Escape(m_Data) + "\"";
        }
        public override void Serialize (System.IO.BinaryWriter aWriter)
        {
            var tmp = new JsonData("");
 
            tmp.AsInt = AsInt;
            if (tmp.m_Data == this.m_Data)
            {
                aWriter.Write((byte)JSONBinaryTag.IntValue);
                aWriter.Write(AsInt);
                return;
            }
            tmp.AsFloat = AsFloat;
            if (tmp.m_Data == this.m_Data)
            {
                aWriter.Write((byte)JSONBinaryTag.FloatValue);
                aWriter.Write(AsFloat);
                return;
            }
            tmp.AsDouble = AsDouble;
            if (tmp.m_Data == this.m_Data)
            {
                aWriter.Write((byte)JSONBinaryTag.DoubleValue);
                aWriter.Write(AsDouble);
                return;
            }
 
            tmp.AsBool = AsBool;
            if (tmp.m_Data == this.m_Data)
            {
                aWriter.Write((byte)JSONBinaryTag.BoolValue);
                aWriter.Write(AsBool);
                return;
            }
            aWriter.Write((byte)JSONBinaryTag.Value);
            aWriter.Write(m_Data);
        }
    } // End of JsonData
 
    internal class JsonLazyCreator : JsonNode
    {
        private JsonNode m_Node = null;
        private string m_Key = null;
 
        public JsonLazyCreator(JsonNode aNode)
        {
            m_Node = aNode;
            m_Key  = null;
        }
        public JsonLazyCreator(JsonNode aNode, string aKey)
        {
            m_Node = aNode;
            m_Key = aKey;
        }
 
        private void Set(JsonNode aVal)
        {
            if (m_Key == null)
            {
                m_Node.Add(aVal);
            }
            else
            {
                m_Node.Add(m_Key, aVal);
            }
            m_Node = null; // Be GC friendly.
        }
 
        public override JsonNode this[int aIndex]
        {
            get
            {
                return new JsonLazyCreator(this);
            }
            set
            {
                var tmp = new JsonArray();
                tmp.Add(value);
                Set(tmp);
            }
        }
 
        public override JsonNode this[string aKey]
        {
            get
            {
                return new JsonLazyCreator(this, aKey);
            }
            set
            {
                var tmp = new JsonClass();
                tmp.Add(aKey, value);
                Set(tmp);
            }
        }
        public override void Add (JsonNode aItem)
        {
            var tmp = new JsonArray();
            tmp.Add(aItem);
            Set(tmp);
        }
        public override void Add (string aKey, JsonNode aItem)
        {
            var tmp = new JsonClass();
            tmp.Add(aKey, aItem);
            Set(tmp);
        }
        public static bool operator ==(JsonLazyCreator a, object b)
        {
            if (b == null)
                return true;
            return System.Object.ReferenceEquals(a,b);
        }
 
        public static bool operator !=(JsonLazyCreator a, object b)
        {
            return !(a == b);
        }
        public override bool Equals (object obj)
        {
            if (obj == null)
                return true;
            return System.Object.ReferenceEquals(this, obj);
        }
        public override int GetHashCode ()
        {
            return base.GetHashCode();
        }
 
        public override string ToString()
        {
            return "";
        }
        public override string ToString(string aPrefix)
        {
            return "";
        }
 
        public override int AsInt
        {
            get
            {
                JsonData tmp = new JsonData(0);
                Set(tmp);
                return 0;
            }
            set
            {
                JsonData tmp = new JsonData(value);
                Set(tmp);
            }
        }
        public override float AsFloat
        {
            get
            {
                JsonData tmp = new JsonData(0.0f);
                Set(tmp);
                return 0.0f;
            }
            set
            {
                JsonData tmp = new JsonData(value);
                Set(tmp);
            }
        }
        public override double AsDouble
        {
            get
            {
                JsonData tmp = new JsonData(0.0);
                Set(tmp);
                return 0.0;
            }
            set
            {
                JsonData tmp = new JsonData(value);
                Set(tmp);
            }
        }
        public override bool AsBool
        {
            get
            {
                JsonData tmp = new JsonData(false);
                Set(tmp);
                return false;
            }
            set
            {
                JsonData tmp = new JsonData(value);
                Set(tmp);
            }
        }
        public override JsonArray AsArray
        {
            get
            {
                JsonArray tmp = new JsonArray();
                Set(tmp);
                return tmp;
            }
        }
        public override JsonClass AsObject
        {
            get
            {
                JsonClass tmp = new JsonClass();
                Set(tmp);
                return tmp;
            }
        }
    } // End of JSONLazyCreator
 
    public static class Json
    {
        public static JsonNode Parse(string aJSON)
        {
            return JsonNode.Parse(aJSON);
        }
    }
}