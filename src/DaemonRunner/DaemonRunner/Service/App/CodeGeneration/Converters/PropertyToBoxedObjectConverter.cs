// using System;
// using System.Text.Json;
// using System.Text.Json.Serialization;
// namespace NetDaemon.Service.App.CodeGeneration.Converters
// {
//     public class PropertyToBoxedObjectConverter : JsonConverter<object>
//     {
//         public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//         {
//             switch (reader.TokenType)
//             {
//                 case Json.String:
//                     example = fieldProperty.Value.GetString();
//                     break;
//                 case JsonValueKind.Number:
//                     if (fieldProperty.Value.TryGetInt64(out long longVal))
//                         example = longVal;
//                     else
//                         example = fieldProperty.Value.GetDouble();
//                     break;
//                 case JsonValueKind.Object:
//                     reader.
//                     example = fieldProperty.Value;
//                     break;
//                 case JsonValueKind.True:
//                     example = true;
//                     break;
//                 case JsonValueKind.False:
//                     example = false;
//                     break;
//                 case JsonValueKind.Array:
//                     example = fieldProperty.Value;
//                     break;
//             }
//         }
//
//
//         public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
//         {
//             throw new NotImplementedException();
//         }
//     }
// }