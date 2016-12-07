using Newtonsoft.Json.Linq;
using System;

namespace ManagementApp
{
    public class JSON
    {
        public Type Type { get; set; }
        public JToken Value { get; set; }

        public static JSON FromValue<T>(T value)
        {
            return new JSON { Type = typeof(T), Value = JToken.FromObject(value) };
        }

        public static string Serialize(JSON message)
        {
            return JToken.FromObject(message).ToString();
        }

        public static JSON Deserialize(string data)
        {
            Console.WriteLine(data);
            return JToken.Parse(data).ToObject<JSON>();
        }
    }
}
