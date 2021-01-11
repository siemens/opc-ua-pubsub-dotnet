﻿// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;

namespace opc.ua.pubsub.dotnet.binary.DataPoints
{
    public class StepPosEvent : ProcessDataPointValue
    {
        public const string     TransientAttributeName = "Transient";
        public const NodeIDType PreDefinedType         = NodeIDType.Event;
        /// <summary>
        ///     The NodeID for this type therefore this member is static and we
        ///     cannot use "NodeID" as the name because this would hide the instance
        ///     member "NodeID".
        /// </summary>
        public static NodeID PreDefinedNodeID = new NodeID( 1, 9 );

        public StepPosEvent() : base( "ValueQc", "originalTimestamp" )
        {
            Value = 0;
            SetAttributeValue( TransientAttributeName, false );
        }

        public override NodeIDType NodeIDType
        {
            get
            {
                return PreDefinedType;
            }
        }

        public static StructureDescription StepPosValueStructureDescription
        {
            get
            {
                StructureDescription desc = new StructureDescription( "StepPosValue",
                                                                      PreDefinedNodeID,
                                                                      WellKnownNodeIDs.DefaultEncoding,
                                                                      WellKnownNodeIDs.BaseDataType,
                                                                      0,
                                                                      new List<StructureField>
                                                                      {
                                                                              new StructureField( "ValueQc",              WellKnownNodeIDs.UInt32 ),
                                                                              new StructureField( "originalTimestamp",    WellKnownNodeIDs.DateTime ),
                                                                              new StructureField( "Value",                WellKnownNodeIDs.Int32 ),
                                                                              new StructureField( TransientAttributeName, WellKnownNodeIDs.Boolean )
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
                return StepPosValueStructureDescription;
            }
        }

        public bool? Transient
        {
            get
            {
                return GetAttributeValue( TransientAttributeName ) as bool?;
            }
            set
            {
                SetAttributeValue( TransientAttributeName, value );
            }
        }

        public new int? Value
        {
            get
            {
                return base.Value as int?;
            }
            set
            {
                base.Value = value;
            }
        }

        public override bool Update( ProcessDataPointValue newValue )
        {
            bool valueChanged = base.Update( newValue );
            if ( newValue is StepPosValue newStepPos )
            {
                if ( Value != newStepPos.Value )
                {
                    Value        = newStepPos.Value;
                    valueChanged = true;
                }
                if ( Transient != newStepPos.Transient )
                {
                    Transient    = newStepPos.Transient;
                    valueChanged = true;
                }
            }
            return valueChanged;
        }
    }
}