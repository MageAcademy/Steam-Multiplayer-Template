using Newtonsoft.Json;
using System;
using UnityEngine;

public class ConverterVector3 : JsonConverter
{
    public override bool CanRead => true;

    public override bool CanWrite => true;


    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector3);
    }


    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return JsonConvert.DeserializeObject<Vector3>(serializer.Deserialize(reader).ToString());
    }


    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        Vector3 vector3 = (Vector3)value;
        writer.WritePropertyName("x");
        writer.WriteValue(vector3.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vector3.y);
        writer.WritePropertyName("z");
        writer.WriteValue(vector3.z);
        writer.WriteEndObject();
    }
}