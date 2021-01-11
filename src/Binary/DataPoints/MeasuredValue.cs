// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;

namespace opc.ua.pubsub.dotnet.binary.DataPoints
{
    public class MeasuredValue : ProcessDataPointValue
    {
        public const NodeIDType PreDefinedType = NodeIDType.TimeSeries;
        /// <summary>
        ///     The NodeID for this type therefore this member is static and we
        ///     cannot use "NodeID" as the name because this would hide the instance
        ///     member "NodeID".
        /// </summary>
        public static NodeID PreDefinedNodeID = new NodeID( 1, 3 );

        public MeasuredValue() : base( "Value_qc", "_time" )
        {
            Value = 0.0F;
        }

        public override NodeIDType NodeIDType
        {
            get
            {
                return PreDefinedType;
            }
        }

        public static StructureDescription MeasuredValueStructureDescription
        {
            get
            {
                StructureDescription desc = new StructureDescription( "MeasuredValue",
                                                                      PreDefinedNodeID,
                                                                      WellKnownNodeIDs.DefaultEncoding,
                                                                      WellKnownNodeIDs.BaseDataType,
                                                                      0,
                                                                      new List<StructureField>
                                                                      {
                                                                              new StructureField( "Value_qc", WellKnownNodeIDs.UInt32 ),
                                                                              new StructureField( "_time",    WellKnownNodeIDs.DateTime ),
                                                                              new StructureField( "Value",    WellKnownNodeIDs.Float )
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
                return MeasuredValueStructureDescription;
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
            if ( newValue is MeasuredValue newMV )
            {
                if ( Value != newMV.Value )
                {
                    Value        = newMV.Value;
                    valueChanged = true;
                }
            }
            return valueChanged;
        }
    }
}