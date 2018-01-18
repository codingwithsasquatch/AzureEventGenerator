using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using EventGeneratorAPI.MessageEngine;

namespace EventGeneratorAPI.MessageEngine
{
    static partial class Messages
    {
        public static string[] CreateMessages(int numOfMessages, string scheme)
        {
            string[] messages = new string[numOfMessages];

            switch (scheme.ToLower())
            {
                case "ninjaattack":
                    messages = CreateNinjaMessages(numOfMessages);
                    break;
                default:
                    for (int i = 0; i < numOfMessages; i++)
                    {
                        var data = new { message = $"Message {i}" };
                        messages[i] = JsonConvert.SerializeObject(data);
                    }
                    break;
            }
            return messages;
        }
    }
}
