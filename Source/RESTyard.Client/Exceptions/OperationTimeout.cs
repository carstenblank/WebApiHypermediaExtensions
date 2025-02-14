﻿using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// The requested operation timed out.
    /// </summary>
    public class OperationTimeout : HypermediaProblemException
    {
        public OperationTimeout(ProblemDescription problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}