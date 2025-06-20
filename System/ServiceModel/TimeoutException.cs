﻿
namespace System.ServiceModel
{
    [Serializable]
    internal class TimeoutException : Exception
    {
        public TimeoutException()
        {
        }

        public TimeoutException(string? message) : base(message)
        {
        }

        public TimeoutException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}