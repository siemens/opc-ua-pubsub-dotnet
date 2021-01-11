// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using opc.ua.pubsub.dotnet.binary.DataPoints;
using NUnit.Framework;

namespace opc.ua.pubsub.dotnet.binary.test
{
    public static class Common
    {
        public static void AssertDataPointsAreEqual( ProcessDataPointValue encoded, ProcessDataPointValue decoded )
        {
            Assert.That( encoded.Name,      Is.EqualTo( decoded.Name ) );
            Assert.That( encoded.Orcat,     Is.EqualTo( decoded.Orcat ) );
            Assert.That( encoded.Quality,   Is.EqualTo( decoded.Quality ) );
            Assert.That( encoded.Value,     Is.EqualTo( decoded.Value ) );
            Assert.That( encoded.Timestamp, Is.EqualTo( decoded.Timestamp ) );
        }
    }
}