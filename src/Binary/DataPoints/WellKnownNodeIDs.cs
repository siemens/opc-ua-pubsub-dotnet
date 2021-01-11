// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using opc.ua.pubsub.dotnet.binary.Messages.Meta;

/**
 * 
 * Procedure to add a new data type / node id: 
 * 
 * 1. Create the NodeID object on respective #region
 * 2. Update ConvertNodeIDSToString() method to return string value
 * 3. Update ConvetStringToNodeID() method to return NodID object corresponds to string value
 * 4. Update GetNodeIDType() to ge the node id type
 * 
 * 
 */

namespace opc.ua.pubsub.dotnet.binary.DataPoints
{
    public static class WellKnownNodeIDs
    {
        internal static NodeIDType GetBaseNodeIDType( NodeID nodeID )
        {
            switch ( nodeID.Namespace )
            {
                case 0:
                    switch ( nodeID.Value )
                    {
                        case 1:
                        case 3:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 10:
                        case 12:
                        case 13:
                            return NodeIDType.BuildInDataType;

                        case 22:
                        case 76:
                            return NodeIDType.None;
                    }
                    break;

                case 1:
                    switch ( nodeID.Value )
                    {
                        case 8:
                        case 9:
                        case 13:
                            return NodeIDType.Event;

                        default:
                            return NodeIDType.TimeSeries;
                    }

                case 2:
                    switch ( nodeID.Value )
                    {
                        case 7:
                            return NodeIDType.File;
                    }
                    break;

                default:
                    return NodeIDType.TimeSeries;
            }
            return NodeIDType.None;
        }

        #region build-in-datatype

        // Build-in data type
        public static NodeID Boolean = new NodeID
                                       {
                                               Namespace = 0,
                                               Value     = 1
                                       };
        public static NodeID Byte = new NodeID
                                    {
                                            Namespace = 0,
                                            Value     = 3
                                    };
        public static NodeID UInt16 = new NodeID
                                      {
                                              Namespace = 0,
                                              Value     = 5
                                      };
        public static NodeID Int32 = new NodeID
                                     {
                                             Namespace = 0,
                                             Value     = 6
                                     };
        public static NodeID UInt32 = new NodeID
                                      {
                                              Namespace = 0,
                                              Value     = 7
                                      };
        public static NodeID Int64 = new NodeID
                                     {
                                             Namespace = 0,
                                             Value     = 8
                                     };
        public static NodeID Float = new NodeID
                                     {
                                             Namespace = 0,
                                             Value     = 10
                                     };
        public static NodeID String = new NodeID
                                      {
                                              Namespace = 0,
                                              Value     = 12
                                      };
        public static NodeID DateTime = new NodeID
                                        {
                                                Namespace = 0,
                                                Value     = 13
                                        };
        public static NodeID DefaultEncoding = new NodeID
                                               {
                                                       Namespace = 0,
                                                       Value     = 76
                                               };
        public static NodeID BaseDataType = new NodeID
                                            {
                                                    Namespace = 0,
                                                    Value     = 22
                                            };

        #endregion
    }
}