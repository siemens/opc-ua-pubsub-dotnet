// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;

namespace opc.ua.pubsub.dotnet.binary.DataPoints
{
    public class DPSEvent : ProcessDataPointValue
    {
        public const NodeIDType PreDefinedType = NodeIDType.Event;
        /// <summary>
        ///     The NodeID for this type therefore this member is static and we
        ///     cannot use "NodeID" as the name because this would hide the instance
        ///     member "NodeID".
        /// </summary>
        public static NodeID PreDefinedNodeID = new NodeID( 1, 7 );

        public DPSEvent() : base( "ValueQc", "originalTimestamp" )
        {
            Value = 0;
        }

        public override NodeIDType NodeIDType
        {
            get
            {
                return PreDefinedType;
            }
        }

        public static StructureDescription DPSEventStructureDescription
        {
            get
            {
                StructureDescription desc = new StructureDescription( "DPSEvent",
                                                                      PreDefinedNodeID,
                                                                      WellKnownNodeIDs.DefaultEncoding,
                                                                      WellKnownNodeIDs.BaseDataType,
                                                                      0,
                                                                      new List<StructureField>
                                                                      {
                                                                              new StructureField( "ValueQc",           WellKnownNodeIDs.UInt32 ),
                                                                              new StructureField( "originalTimestamp", WellKnownNodeIDs.DateTime ),
                                                                              new StructureField( "Value",             WellKnownNodeIDs.Byte )
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
                return DPSEventStructureDescription;
            }
        }

        public new byte? Value
        {
            get
            {
                return base.Value as byte?;
            }
            set
            {
                base.Value = value;
            }
        }

        public override bool Update( ProcessDataPointValue newValue )
        {
            bool valueChanged = base.Update( newValue );
            if ( newValue is DPSEvent newDPS )
            {
                if ( Value != newDPS.Value )
                {
                    Value        = newDPS.Value;
                    valueChanged = true;
                }
            }
            return valueChanged;
        }
    }
}