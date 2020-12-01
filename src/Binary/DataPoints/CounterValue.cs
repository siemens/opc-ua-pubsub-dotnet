// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Binary.Messages.Meta;
using Binary.Messages.Meta.Structure;

namespace Binary.DataPoints
{
    public class CounterValue : ProcessDataPointValue
    {
        public const string     QuantityAttributeName = "QTY";
        public const NodeIDType PreDefinedType        = NodeIDType.TimeSeries;
        /// <summary>
        ///     The NodeID for this type therefore this member is static and we
        ///     cannot use "NodeID" as the name because this would hide the instance
        ///     member "NodeID".
        /// </summary>
        public static NodeID PreDefinedNodeID = new NodeID( 1, 5 );

        public CounterValue() : base( "Value_qc", "_time" )
        {
            Value = 0L;
            SetAttributeValue( QuantityAttributeName, 0.0F );
        }

        public override NodeIDType NodeIDType
        {
            get
            {
                return PreDefinedType;
            }
        }

        public static StructureDescription CounterValueStructureDescription
        {
            get
            {
                StructureDescription desc = new StructureDescription( "CounterValue",
                                                                      PreDefinedNodeID,
                                                                      WellKnownNodeIDs.DefaultEncoding,
                                                                      WellKnownNodeIDs.BaseDataType,
                                                                      0,
                                                                      new List<StructureField>
                                                                      {
                                                                              new StructureField( "Value_qc",            WellKnownNodeIDs.UInt32 ),
                                                                              new StructureField( "_time",               WellKnownNodeIDs.DateTime ),
                                                                              new StructureField( "Value",               WellKnownNodeIDs.Int64 ),
                                                                              new StructureField( QuantityAttributeName, WellKnownNodeIDs.Float )
                                                                      }
                                                                    );
                return desc;
            }
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public override NodeID NodeID
        {
            get
            {
                return PreDefinedNodeID;
            }
        }

        public float? Quantity
        {
            get
            {
                return GetAttributeValue( QuantityAttributeName ) as float?;
            }
            set
            {
                SetAttributeValue( QuantityAttributeName, value );
            }
        }

        public override StructureDescription StructureDescription
        {
            get
            {
                return CounterValueStructureDescription;
            }
        }

        public new long? Value
        {
            get
            {
                return base.Value as long?;
            }
            set
            {
                base.Value = value;
            }
        }

        public override bool Update( ProcessDataPointValue newValue )
        {
            bool valueChanged = base.Update( newValue );
            if ( newValue is CounterValue newCounter )
            {
                if ( Value != newCounter.Value )
                {
                    Value        = newCounter.Value;
                    valueChanged = true;
                }
                if ( Quantity != newCounter.Quantity )
                {
                    Quantity     = newCounter.Quantity;
                    valueChanged = true;
                }
            }
            return valueChanged;
        }
    }
}