// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Binary.Messages.Meta;
using Binary.Messages.Meta.Structure;

namespace Binary.DataPoints
{
    public class ComplexMeasuredValue : ProcessDataPointValue
    {
        public const string     AngleAttributeName = "Angle";
        public const NodeIDType PreDefinedType     = NodeIDType.TimeSeries;
        /// <summary>
        ///     The NodeID for this type therefore this member is static and we
        ///     cannot use "NodeID" as the name because this would hide the instance
        ///     member "NodeID".
        /// </summary>
        public static NodeID PreDefinedNodeID = new NodeID( 1, 21 );

        public ComplexMeasuredValue() : base( "Value_qc", "_time" )
        {
            Value = 0.0F;
            SetAttributeValue( AngleAttributeName, 0.0F );
        }

        public override NodeIDType NodeIDType
        {
            get
            {
                return PreDefinedType;
            }
        }

        public float? Angle
        {
            get
            {
                return GetAttributeValue( AngleAttributeName ) as float?;
            }
            set
            {
                SetAttributeValue( AngleAttributeName, value );
            }
        }

        public static StructureDescription ComplexMeasuredValueStructureDescription
        {
            get
            {
                StructureDescription desc = new StructureDescription( "ComplexMeasuredValue",
                                                                      PreDefinedNodeID,
                                                                      WellKnownNodeIDs.DefaultEncoding,
                                                                      WellKnownNodeIDs.BaseDataType,
                                                                      0,
                                                                      new List<StructureField>
                                                                      {
                                                                              new StructureField( "Value_qc",         WellKnownNodeIDs.UInt32 ),
                                                                              new StructureField( "_time",            WellKnownNodeIDs.DateTime ),
                                                                              new StructureField( "Value",            WellKnownNodeIDs.Float ),
                                                                              new StructureField( AngleAttributeName, WellKnownNodeIDs.Float )
                                                                      }
                                                                    );
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
                return ComplexMeasuredValueStructureDescription;
            }
        }

        public new float? Value
        {
            get
            {
                return base.Value as float?;
            }
            set
            {
                base.Value = value;
            }
        }

        public override bool Update( ProcessDataPointValue newValue )
        {
            bool valueChanged = base.Update( newValue );
            if ( newValue is ComplexMeasuredValue newCMV )
            {
                if ( Value != newCMV.Value )
                {
                    Value        = newCMV.Value;
                    valueChanged = true;
                }
            }
            return valueChanged;
        }
    }
}