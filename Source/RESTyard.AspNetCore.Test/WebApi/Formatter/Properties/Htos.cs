﻿using System;
using System.Collections.Generic;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Test.Helpers;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter.Properties
{
    public class EmptyHypermediaObject : HypermediaObject
    {
    }

    [HypermediaObject(Title = "A Title", Classes = new[] { "CustomClass1", "CustomClass2" })]
    public class AttributedEmptyHypermediaObject : HypermediaObject
    {
    }

    public class PropertyDuplicateHypermediaObject : HypermediaObject
    {
        [HypermediaProperty(Name = "DuplicateRename")]
        public bool Property1 { get; set; }

        [HypermediaProperty(Name = "DuplicateRename")]
        public bool Property2 { get; set; }
    }

    public class PropertyNestedClassHypermediaObject : HypermediaObject
    {
        public AttributedPropertyHypermediaObject AChild { get; set; }
    }

    public class AttributedPropertyHypermediaObject : HypermediaObject
    {
        [HypermediaProperty(Name = "Property1Renamed")]
        public bool Property1 { get; set; }

        [HypermediaProperty(Name = "Property2Renamed")]
        public bool Property2 { get; set; }

        [FormatterIgnoreHypermediaProperty]
        public bool IgnoredProperty { get; set; }

        public bool NotRenamed { get; set; }
    }

    public class PropertyHypermediaObject : HypermediaObject
    {
        public bool ABool { get; set; }
        public string AString { get; set; }
        public int AnInt { get; set; }
        public long ALong { get; set; }
        public float AFloat { get; set; }
        public double ADouble { get; set; }

        public TestEnum AnEnum { get; set; }
        public TestEnum? ANullableEnum { get; set; }
        public TestEnumWithNames AnEnumWithNames { get; set; }

        public DateTime ADateTime { get; set; }
        public DateTimeOffset ADateTimeOffset { get; set; }
        public TimeSpan ATimeSpan { get; set; }
        public decimal ADecimal { get; set; }
        public int? ANullableInt { get; set; }
        public Uri AnUri { get; set; }
    }

    public class HypermediaObjectWithListProperties : HypermediaObject
    {
        public IEnumerable<int> AValueList { get; set; }

        public IEnumerable<int?> ANullableList { get; set; }

        public IEnumerable<string> AReferenceList { get; set; }

        public int[] AValueArray { get; set; } // arrays need special treatment

        public IEnumerable<Nested> AObjectList { get; set; }

        public IEnumerable<IEnumerable<int>> ListOfLists { get; set; }
    }

    public class Nested
    {
        public Nested(int i)
        {
            AInt = i;
        }

        public int AInt { get; set; }
    }

}