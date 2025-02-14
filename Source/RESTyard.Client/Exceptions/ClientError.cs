﻿using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// Error of the client.
    /// </summary>
    public class ClientError : HypermediaProblemException
    {
        public ClientError(ProblemDescription problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}