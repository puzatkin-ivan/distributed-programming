using System;
using System.Collections.Generic;

namespace Core
{
    public class Message
    {
        private const string EuInstance = "eu";
        private const string EnInstance = "us";
        private const string RuInstance = "ru";
    
        private readonly string _message;

        private readonly string _location;

        private string _id;

        private readonly int _database;

        public Message(string message, string location)
        {
            _message = message;
            _location = location;
            _id = "";
            _database = GetDatabaseNumber(location);
        }

        public string GetMessage()
        {
            return _message;
        }

        public string GetLocation()
        {
            return _location;
        }

        public string GetId()
        {
            return _id;
        }

        public void SetID(string id)
        {
            _id = id;
        }

        public int GetDatabase()
        {
            return _database;
        }

        public static int GetDatabaseNumber(string contextId)
        {
            switch (contextId.ToLower())
            {
                case EuInstance:
                    return 1;
                case RuInstance:
                    return 2;
                case EnInstance:
                    return 3;
                default:
                    throw new Exception("Unknown redis contextId: " + contextId);
            }
        }
    }
}