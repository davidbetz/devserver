using System;
using System.Collections.Generic;
using System.Linq;
//+
namespace DevServer.Client
{
    internal static class CommandLineParser
    {
        //- ~Parse -//
        internal static CommandLineDictionary Parse(String[] args, String[] allowedArguments)
        {
            CommandLineDictionary dictionary = new CommandLineDictionary();
            List<String> arguments = new List<String>(args);
            foreach (var arg in arguments)
            {
                String[] parts = arg.Split(':');
                if (parts.Length == 2)
                {
                    String parameter = parts[0].Replace("-", "").Trim();
                    String value = parts[1].Replace("\"", "").Trim();
                    if (allowedArguments.Count(p => p == parameter) == 0)
                    {
                        continue;
                    }
                    dictionary.Add(parameter, value);
                }
            }
            return dictionary;
        }
    }
}