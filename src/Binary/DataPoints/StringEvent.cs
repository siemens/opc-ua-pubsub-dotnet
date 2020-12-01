// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Binary.Messages.Meta;
using Binary.Messages.Meta.Structure;

namespace Binary.DataPoints
{
    public class StringEvent : ProcessDataPointValue
    {
        public const NodeIDType PreDefinedType = NodeIDType.Event;
        /// <summary>
        ///     The NodeID for this type therefore this member is static and we
        ///     cannot use "NodeID" as the name because this would hide the instance
        ///     member "NodeID".
        /// </summary>
        public static NodeID PreDefinedNodeID = new NodeID( 1, 10 );

        public StringEvent() : base( "ValueQc", "originalTimestamp" )
        {
            Value = string.Empty;
        }

        public override NodeIDType NodeIDType
        {
            get
            {
                return PreDefinedType;
            }
        }

        public static StructureDescription StringEventStructureDescription
        {
            get
            {
                StructureDescription desc = new StructureDescription( "StringEvent",
                                                                      PreDefinedNodeID,
                                                                      WellKnownNodeIDs.DefaultEncoding,
                                                                      WellKnownNodeIDs.BaseDataType,
                                                                      0,
                                                                      new List<StructureField>
                                                                      {
                                                                              new StructureField( "ValueQc",           WellKnownNodeIDs.UInt32 ),
                                                                              new StructureField( "originalTimestamp", WellKnownNodeIDs.DateTime ),
                                                                              new StructureField( "Value",             WellKnownNodeIDs.String )
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
                return StringEventStructureDescription;
            }
        }

        public new string Value
        {
            get
            {
                return base.Value as string;
            }
            set
            {
                base.Value = value;
            }
        }

        public override bool Update( ProcessDataPointValue newValue )
        {
            bool valueChanged = base.Update( newValue );
            if ( newValue is StringEvent newString )
            {
                if ( !Value.Equals( newString.Value, StringComparison.InvariantCulture ) )
                {
                    Value        = newString.Value;
                    valueChanged = true;
                }
            }
            return valueChanged;
        }
    }
}