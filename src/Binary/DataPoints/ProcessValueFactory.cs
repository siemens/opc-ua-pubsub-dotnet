// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using Binary.Messages.Meta;

namespace Binary.DataPoints
{
    public class ProcessValueFactory
    {
        protected static Dictionary<NodeID, NodeIDType> NodeIdTypes        = new Dictionary<NodeID, NodeIDType>();
        protected static Dictionary<NodeID, Type>       s_NodeIdValueTypes = new Dictionary<NodeID, Type>();

        //private readonly static Dictionary<NodeID, NodeIDType> NodeID2Types = new Dictionary<NodeID, NodeIDType>();
        static ProcessValueFactory()
        {
            foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                foreach ( Type type in assembly.GetTypes() )
                {
                    if ( BaseType.IsAssignableFrom( type ) && type != BaseType )
                    {
                        RegisterType( type );
                    }
                }
            }
        }

        public static Type BaseType { get; } = typeof(DataPointValue);

        private static void RegisterType( Type type )
        {
            FieldInfo nodeIdfFieldInfo = type.GetField( "PreDefinedNodeID", BindingFlags.Public | BindingFlags.Static );
            NodeID    nodeID           = null;
            if ( nodeIdfFieldInfo != null )
            {
                nodeID = nodeIdfFieldInfo.GetValue( null ) as NodeID;
            }
            if ( nodeID == null )
            {
                return;
            }
            s_NodeIdValueTypes[nodeID] = type;
            FieldInfo fieldInfo = type.GetField( "PreDefinedType", BindingFlags.Public | BindingFlags.Static );
            if ( fieldInfo != null )
            {
                NodeIDType? idType;
                try
                {
                    idType = (NodeIDType)fieldInfo.GetValue( null );
                }
                catch ( Exception )
                {
                    return;
                }
                NodeIdTypes[nodeID] = idType.Value;
            }
        }

        public static ProcessDataPointValue CreateValue( NodeID dataType )
        {
            ProcessDataPointValue resultValue = null;
            if ( s_NodeIdValueTypes != null && s_NodeIdValueTypes.TryGetValue( dataType, out Type targetType ) )
            {
                resultValue = Activator.CreateInstance( targetType ) as ProcessDataPointValue;
            }
            return resultValue;
        }

        public static NodeIDType GetNodeIDType( NodeID nodeID )
        {
            if ( NodeIdTypes.TryGetValue( nodeID, out NodeIDType type ) )
            {
                return type;
            }
            return WellKnownNodeIDs.GetBaseNodeIDType( nodeID );
        }
    }
}