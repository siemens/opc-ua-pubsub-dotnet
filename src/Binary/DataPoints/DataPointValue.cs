// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;

namespace opc.ua.pubsub.dotnet.binary.DataPoints
{
    public abstract class DataPointValue : IEquatable<DataPointValue>
    {
        public virtual EnumDescription      EnumDescription      { get; set; }
        public         Guid                 FieldID              { get; set; }
        public         int                  Index                { get; set; }
        public         string               Name                 { get; set; }
        public virtual NodeID               NodeID               { get; }
        public         List<Messages.Meta.KeyValuePair>   Properties           { get; set; }
        public virtual StructureDescription StructureDescription { get; }

        public bool Equals( DataPointValue other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }
            bool areEqual = Index == other.Index
                         && FieldID.Equals( other.FieldID )
                         && Name == other.Name
                         && Properties.NullableSequenceEquals( other.Properties )
                         && Equals( StructureDescription, other.StructureDescription )
                         && Equals( EnumDescription,      other.EnumDescription )
                         && Equals( NodeID,               other.NodeID );
            return areEqual;
        }

        public abstract void Decode( Stream inputStream );
        public abstract void Encode( Stream outputStream );

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) )
            {
                return false;
            }
            if ( ReferenceEquals( this, obj ) )
            {
                return true;
            }
            if ( obj.GetType() != GetType() )
            {
                return false;
            }
            return Equals( (DataPointValue)obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Index;
                hashCode = ( hashCode * 397 ) ^ FieldID.GetHashCode();
                hashCode = ( hashCode * 397 ) ^ ( Name                 != null ? Name.GetHashCode(StringComparison.Ordinal) : 0 );
                hashCode = ( hashCode * 397 ) ^ ( Properties           != null ? Properties.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( StructureDescription != null ? StructureDescription.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( EnumDescription      != null ? EnumDescription.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( NodeID               != null ? NodeID.GetHashCode() : 0 );
                return hashCode;
            }
        }

        public static bool operator ==( DataPointValue left, DataPointValue right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( DataPointValue left, DataPointValue right )
        {
            return !Equals( left, right );
        }

        protected virtual void ValueToString( StringBuilder sb ) { }
    }
}