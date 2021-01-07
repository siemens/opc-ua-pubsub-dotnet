// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using Binary.Messages.Meta;
using Binary.Messages.Meta.Structure;

namespace Binary.DataPoints
{
    public class MeasuredValuesArray50 : ProcessDataPointValue
    {
        public const    NodeIDType PreDefinedType           = NodeIDType.TimeSeries;
        protected const string     ArrayElementFormatString = "MV{0:D2}";
        /// <summary>
        ///     The NodeID for this type therefore this member is static and we
        ///     cannot use "NodeID" as the name because this would hide the instance
        ///     member "NodeID".
        /// </summary>
        public static readonly NodeID PreDefinedNodeID = new NodeID( 1, 20 );

        public MeasuredValuesArray50() : base( "MV00_qc", "_time" )
        {
            ArraySize             = 50;
            Value                 = new float[50];
            m_QualityPropertyName = "MV00_qc";
        }

        public override NodeIDType NodeIDType
        {
            get
            {
                return PreDefinedType;
            }
        }

        public static StructureDescription MeasuredValuesArray50StructureDescription
        {
            get
            {
                StructureDescription desc = new StructureDescription( "MeasuredValuesArray50",
                                                                      PreDefinedNodeID,
                                                                      WellKnownNodeIDs.DefaultEncoding,
                                                                      WellKnownNodeIDs.BaseDataType,
                                                                      0,
                                                                      new List<StructureField>()
                                                                    );
                desc.Fields.Add( new StructureField( "MV00_qc", WellKnownNodeIDs.UInt32 ) );
                desc.Fields.Add( new StructureField( "_time",   WellKnownNodeIDs.DateTime ) );
                for ( int i = 0; i < 50; i++ )
                {
                    desc.Fields.Add( new StructureField( string.Format( CultureInfo.InvariantCulture, ArrayElementFormatString, i ), WellKnownNodeIDs.Float ) );
                }
                return desc;
            }
        }

        /// <inheritdoc />
        public override NodeID NodeID
        {
            get
            {
                return PreDefinedNodeID;
            }
        }

        public override StructureDescription StructureDescription
        {
            get
            {
                return MeasuredValuesArray50StructureDescription;
            }
        }

        public new float[] Value
        {
            get
            {
                return Array.ConvertAll<object, float>(base.Value as object[], v => Convert.ToSingle(v, CultureInfo.InvariantCulture));
            }
            set
            {
                base.Value = value;
            }
        }

        public override string GetArrayElementName( int index )
        {
            return string.Format( CultureInfo.InvariantCulture, ArrayElementFormatString, index );
        }

        public object GetArrayValue( int index )
        {
            if ( index >= 0 && index < ArraySize )
            {
                return GetAttributeValue( GetArrayElementName( index ) );
            }
            return null;
        }

        public void SetArrayValue( int index, object value )
        {
            if ( index >= 0 && index < ArraySize )
            {
                SetAttributeValue( GetArrayElementName( index ), value );
            }
        }

        public override bool Update( ProcessDataPointValue newValue )
        {
            bool valueChanged = base.Update( newValue );
            if ( newValue is MeasuredValuesArray50 newMVA50 )
            {
                float[] thisValues = Value;
                float[] newValues  = newMVA50.Value;
                if ( thisValues.Length == newValues.Length )
                {
                    for ( int i = 0; i < thisValues.Length; i++ )
                    {
                        if ( thisValues[i] != newValues[i] )
                        {
                            valueChanged = true;
                            break;
                        }
                    }
                }
                else
                {
                    valueChanged = true;
                }

                if ( valueChanged )
                {
                    Value = newValues;
                }
            }
            return valueChanged;
        }
    }
}