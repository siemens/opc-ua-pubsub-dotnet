// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;

namespace Binary
{
    public static class Extensions
    {
        public static bool ContentEquals<TKey, TValue>( this Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b )
        {
            return ( b ?? new Dictionary<TKey, TValue>() )
                  .OrderBy( kvp => kvp.Key )
                  .SequenceEqual( ( a ?? new Dictionary<TKey, TValue>() )
                                 .OrderBy( kvp => kvp.Key )
                                );
        }

        public static bool NullableSequenceEquals<T>( this IEnumerable<T> a, IEnumerable<T> b )
        {
            if ( a != null && b != null )
            {
                return a.SequenceEqual( b );
            }
            return a == null && b == null;
        }
    }
}