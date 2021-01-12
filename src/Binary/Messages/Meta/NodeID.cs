// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.IO;

namespace opc.ua.pubsub.dotnet.binary.Messages.Meta
{
    public class NodeID : IEquatable<NodeID>
    {
        public NodeID() : this( 0, 0 ) { }

        public NodeID( byte nameSpace, ushort value, byte encoding = 1 )
        {
            Encoding  = encoding;
            Value     = value;
            Namespace = nameSpace;
        }

        public byte   Encoding  { get; set; }
        public byte   Namespace { get; set; }
        public ushort Value     { get; set; }

        public static NodeID Decode( Stream inputStream )
        {
            NodeID instance = new NodeID();
            instance.Encoding = (byte)inputStream.ReadByte();
            if ( instance.Encoding == 3 ) // File structure
            {
                instance.Namespace = (byte)BaseType.ReadUInt16( inputStream );
                instance.Value     = 7;
                String _ = String.Decode( inputStream );
            }
            else
            {
                instance.Namespace = (byte)inputStream.ReadByte();
                instance.Value     = (ushort)BaseType.ReadUInt16( inputStream );
            }
            return instance;
        }

        public void Encode( Stream outputStream )
        {
            if ( outputStream == null || !outputStream.CanWrite )
            {
                return;
            }
            outputStream.WriteByte( Encoding );
            if ( Encoding == 3 ) // File structure
            {
                ushort fileNamespace = 2;
                BaseType.WriteToStream( outputStream, BitConverter.GetBytes( fileNamespace ) );
                String value = new String( "File" );
                value.Encode( outputStream );
            }
            else
            {
                outputStream.WriteByte( Namespace );
                BaseType.WriteToStream( outputStream, BitConverter.GetBytes( Value ) );
            }
        }

        #region Overrides of Object

        public override bool Equals( object obj )
        {
            NodeID other = obj as NodeID;
            if ( other == null )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Namespace == other.Namespace && Value == other.Value;
        }

        #region Overrides of Object

        public override string ToString()
        {
            if ( Namespace == 2 && Value == 7 )
            {
                return $"[{Namespace}]-[File]";
            }
            return $"[{Namespace}]-[i={Value}]";
        }

        #endregion

        #region Equality members

        public bool Equals( NodeID other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            return Namespace == other.Namespace && Value == other.Value;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ( Namespace.GetHashCode() * 397 ) ^ Value.GetHashCode();
            }
        }

        public static bool operator ==( NodeID left, NodeID right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( NodeID left, NodeID right )
        {
            return !Equals( left, right );
        }

        #endregion

        #endregion
    }
}