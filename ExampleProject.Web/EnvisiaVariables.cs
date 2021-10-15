using System;
using System.Runtime.InteropServices;

namespace ExampleProject.Web
{
    public class EnvisiaVariables
    {
        public static string Get(string name)
        {
            var env = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User) ??
                      Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);

            if (env == null && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                env = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
            }

            return env;
        }

        public static string GetOrDefault(string name, string defaultValue)
        {
            var env = Get(name);
            if (string.IsNullOrEmpty(env))
            {
                env = defaultValue;
            }

            return env;
        }
    }
}