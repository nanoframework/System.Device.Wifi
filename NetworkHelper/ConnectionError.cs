// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.Networking
{
    /// <summary>
    /// Connection Error class
    /// </summary>
    public class ConnectionError
    {
        /// <summary>
        /// Connection Error class constructor
        /// </summary>
        /// <param name="error">Error message.</param>
        /// <param name="ex">Exception</param>
        public ConnectionError (string error, Exception ex = null)
        {
            Error = error;
            Exception = ex;
        }

        /// <summary>
        /// The error message
        /// </summary>
        public string Error { get; internal set; }

        /// <summary>
        /// The possible Exception
        /// </summary>
        public Exception Exception { get; internal set;  }
    }
}
