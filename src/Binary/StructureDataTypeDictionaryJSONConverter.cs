// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using Binary.Messages.Meta;
using Binary.Messages.Meta.Structure;
using Newtonsoft.Json;

namespace Binary
{
    public class StructureDataTypeDictionaryJSONConverter : JsonConverter
    {
        /// <summary>
        ///     https://www.newtonsoft.com/json/help/html/SerializationGuide.htm
        ///     https://stackoverflow.com/questions/18579427/newtonsoft-json-serialize-collection-with-indexer-as-dictionary
        ///     or
        ///     https://stackoverflow.com/questions/24504245/not-ableto-serialize-dictionary-with-complex-key-using-json-net
        ///     https://msdn.microsoft.com/en-us/library/ayybcxe5.aspx
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>

        #region Overrides of JsonConverter

        public override bool CanConvert( Type objectType )
        {
            if ( objectType == typeof(Dictionary<NodeID, StructureDescription>) )
            {
                return true;
            }
            return false;
        }

        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            Dictionary<NodeID, StructureDescription> dictionary = value as Dictionary<NodeID, StructureDescription>;
            if ( dictionary == null )
            {
                return;
            }
            List<StructureDescription> list = dictionary.Values.ToList();
            serializer.Serialize( writer, list );
        }

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            if ( objectType != typeof(Dictionary<NodeID, StructureDescription>) )
            {
                return null;
            }
            Dictionary<NodeID, StructureDescription> dictionary = new Dictionary<NodeID, StructureDescription>();
            List<StructureDescription>               list       = serializer.Deserialize<List<StructureDescription>>( reader );
            foreach ( StructureDescription structureDescription in list )
            {
                dictionary.Add( structureDescription.DataTypeId, structureDescription );
            }
            return dictionary;
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        #endregion
    }
}