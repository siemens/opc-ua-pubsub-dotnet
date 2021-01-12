// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;

namespace opc.ua.pubsub.dotnet.binary.DataPoints
{
    public class MeasuredValueEvent : ProcessDataPointValue
    {
        public const NodeIDType PreDefinedType = NodeIDType.Event;
        /// <summary>
        ///     The NodeID for this type therefore this member is static and we
        ///     cannot use "NodeID" as the name because this would hide the instance
        ///     member "NodeID".
        /// </summary>
        public static NodeID PreDefinedNodeID = new NodeID( 1, 8 );

        public MeasuredValueEvent() : base( "ValueQc", "originalTimestamp" )
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

        public static StructureDescription MeasuredValueEventStructureDescription
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
                                                                              new StructureField( "ValueQc",           WellKnownNodeIDs.UInt32 ),
                                                                              new StructureField( "originalTimestamp", WellKnownNodeIDs.DateTime ),
                                                                              new StructureField( "Value",             WellKnownNodeIDs.Float )
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
                return MeasuredValueEventStructureDescription;
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