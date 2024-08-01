using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DataLayer
{
    public class ColumnConverter : JsonConverter<Column>
    {
        public Column ReadJson(JsonReader reader, Type objectType, Column existingValue, JsonSerializer serializer)

        {
            var jObject = JObject.Load(reader);
            Column? column = null;

            // Determine the column type
            var typeId = (int)jObject["typeId"];

            column = typeId switch
            {
                (int)ColumnType.YesNo => new YesNoColumn(),
                (int)ColumnType.Number => new NumberColumn(),
                // Add cases for other column types
                _ => new Column(),
            };

            // Populate common properties
            serializer.Populate(jObject.CreateReader(), column);

            // Deserialize DefaultValue based on column type
            var defaultValue = jObject["DefaultValue"];
            if (defaultValue != null)
            {
                switch (column)
                {
                    case YesNoColumn yesNoColumn:
                        yesNoColumn.DefaultValue = defaultValue.ToObject<bool>();
                        break;
                    case NumberColumn numberColumn:
                        numberColumn.DefaultValue = defaultValue.ToObject<double>();
                        break;
                    case TextColumn textColumn:
                        textColumn.DefaultValue = defaultValue.ToObject<string>();
                        break;
                        // Add cases for other column types
                }
            }

            return column;
        }

        public override Column? ReadJson(JsonReader reader, Type objectType, Column? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, Column value, JsonSerializer serializer)
        {
            // Implement if needed
            throw new NotImplementedException();
        }
    }
}
