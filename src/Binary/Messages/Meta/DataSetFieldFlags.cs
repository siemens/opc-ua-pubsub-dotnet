// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.IO;

namespace Binary.Messages.Meta
{
    public class DataSetFieldFlags : OptionSet
    {

        public DataSetFieldFlags( EncodingOptions options ) : base ( options )
        {
            Value     = new[] { (byte)0 };
            ValidBits = new[] { (byte)0 };
        }

        public bool PromotedField
        {
            get
            {
                return Value[0] != 0;
            }
            set
            {
                if ( value )
                {
                    Value[0] = 1;
                }
                else
                {
                    Value[0] = 0;
                }
            }
        }

        public new static DataSetFieldFlags Decode( Stream inputStream, EncodingOptions options )
        {
            DataSetFieldFlags instance = new DataSetFieldFlags( options );
            if ( options.LegacyFieldFlagEncoding )
            {
                inputStream.Position += 4;
                instance.Value = new[] { (byte)inputStream.ReadByte() };
                inputStream.Position += 4;
                instance.ValidBits = new[] { (byte)inputStream.ReadByte() };
                return instance;
            }
            instance.Value     = new[] { (byte)inputStream.ReadByte() };
            instance.ValidBits = new[] { (byte)inputStream.ReadByte() };
            return instance;
        }

        #region Overrides of Object

        public override string ToString()
        {
            return $"PromotedField: {PromotedField}";
        }

        #endregion
    }
}