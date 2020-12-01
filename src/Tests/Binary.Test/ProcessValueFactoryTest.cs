// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Collections.Generic;
using Binary.DataPoints;
using NUnit.Framework;

namespace opc.ua.pubsub.dotnet.binary.test
{
    public class ProcessValueFactoryTest : ProcessValueFactory
    {
        public static IEnumerable<Type> KnownTypes = new List<Type>
                                                     {
                                                             typeof(DPSEvent),
                                                             typeof(DPSValue),
                                                             typeof(IntegerEvent),
                                                             typeof(IntegerValue),
                                                             typeof(MeasuredValue),
                                                             typeof(MeasuredValueEvent),
                                                             typeof(MeasuredValuesArray50),
                                                             typeof(SPSEvent),
                                                             typeof(SPSValue),
                                                             typeof(StepPosEvent),
                                                             typeof(StepPosValue),
                                                             typeof(StringEvent),
                                                             typeof(CounterValue),
                                                             typeof(ComplexMeasuredValue),
                                                             typeof(File)
                                                     };

        public static IEnumerable CreateDataPointsTestCases
        {
            get
            {
                foreach ( Type type in KnownTypes )
                {
                    if ( BaseType.IsAssignableFrom( type ) && type != BaseType )
                    {
                        yield return new TestCaseData( type ).SetName( $"Create_{type.Name}" );
                    }
                }
            }
        }

        [TestCaseSource( nameof(CreateDataPointsTestCases) )]
        public void CreateInstanceFromNodeID( Type expectedType )
        {
            Assert.That( s_NodeIdValueTypes, Is.Not.Null );
            Assert.That( s_NodeIdValueTypes, Contains.Value( expectedType ), $"Type \"{nameof(expectedType)}\" is not found in NodeID - Type mapping." );
        }

        [Test]
        public void TestForAllTypesPresent()
        {
            Assert.That( s_NodeIdValueTypes.Values, Is.EquivalentTo( KnownTypes ) );
        }
    }
}