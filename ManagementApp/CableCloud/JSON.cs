﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;

namespace CableCloud
{
    class JSON
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
            return JToken.Parse(data).ToObject<JSON>();
        }
    }
}
