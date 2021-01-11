// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;

namespace opc.ua.pubsub.dotnet.binary.DataPoints
{
    public class SPSValue : ProcessDataPointValue
    {
        public const NodeIDType PreDefinedType = NodeIDType.TimeSeries;
        /// <summary>
        ///     The NodeID for this type therefore this member is static and we
        ///     cannot use "NodeID" as the name because this would hide the instance
        ///     member "NodeID".
        /// </summary>
        public static NodeID PreDefinedNodeID = new NodeID( 1, 1 );

        public SPSValue() : base( "ValueQc", "originalTimestamp" )
        {
            Value = false;
        }

        public override NodeIDType NodeIDType
        {
            get
            {
                return PreDefinedType;
            }
        }

        public static StructureDescription SPSEventStructureDescription
        {
            get
            {
                StructureDescription desc = new StructureDescription( "SPSValue",
                                                                      PreDefinedNodeID,
                                                                      WellKnownNodeIDs.DefaultEncoding,
                                                                      WellKnownNodeIDs.BaseDataType,
                                                                      0,
                                                                      new List<StructureField>
                                                                      {
                                                                              new StructureField( "ValueQc",           WellKnownNodeIDs.UInt32 ),
                                                                              new StructureField( "originalTimestamp", WellKnownNodeIDs.DateTime ),
                                                                              new StructureField( "Value",             WellKnownNodeIDs.Boolean )
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
                return SPSEventStructureDescription;
            }
        }

        public new bool? Value
        {
            get
            {
                return base.Value as bool?;
            }
            set
            {
                base.Value = value;
            }
        }

        public override bool Update( ProcessDataPointValue newValue )
        {
            bool valueChanged = base.Update( newValue );
            if ( newValue is SPSEvent newSPS )
            {
                if ( Value != newSPS.Value )
                {
                    Value        = newSPS.Value;
                    valueChanged = true;
                }
            }
            return valueChanged;
        }
    }
}