﻿using System;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;
using Bluehands.Hypermedia.Relations;

namespace Benchmarking.Hcos
{
    [HypermediaClientObject("EventCaseReport")]
    public class EventCaseReportHco : HypermediaClientObject
    {
        public new string Title { get; set; }

        public int CategoryId { get; set; }

        public Guid EventCaseId { get; set; }

        public int TotalScore { get; set; }

        public int TimesAccomplished { get; set; }

        [HypermediaRelations(new[] { DefaultHypermediaRelations.Self })]
        public MandatoryHypermediaLink<EventCaseReportHco> Self { get; set; }
    }
}