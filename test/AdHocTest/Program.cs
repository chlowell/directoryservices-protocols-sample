// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System;
using System.Linq;
#if LEGACY_SDS
// Set LEGACY_SDS to use the .NET Framework's System.DirectoryServices library
using System.DirectoryServices;
#else
// Otherwise, the System.DirectoryServices.Protocols.Services wrapper library will
// be used (which depends on System.DirectoryServices.Protocols, but not System.DirectoryServices)
using System.DirectoryServices.Protocols.Services;
#endif
using System.Collections.Generic;

/// <summary>
/// Simple test app exercising common System.DirectoryServices APIs
/// </summary>
public class Program
{
    // The LDAP path to access
    private const string ADPath = "LDAP://DOMAIN/OU=UserAccounts,DC=DOMAIN,DC=corp,DC=fabrikam,DC=com";
    private static string alias = "user";

    // Used by the simple logging API to synchronize console access
    private static object logLock = new object();

    /// <summary>
    /// Simple logging API for writing to the console with a timestamp and color
    /// </summary>
    /// <param name="message">Message to log</param>
    /// <param name="color">(optional) Color to write the message in</param>
    private static void Log(string message = "", ConsoleColor? color = null)
    {
        lock (logLock)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }

            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
            if (color.HasValue)
            {
                Console.ResetColor();
            }
        }
    }

    /// <summary>
    /// Test app entrypoint
    /// </summary>
    public static void Main()
    {
        Log("AD Exploratory Program");
        Log();

        Log($"Creating AD node object using path {ADPath}");
        var deNode = new DirectoryEntry(ADPath);
        Log($"Loaded {deNode.Name}", ConsoleColor.Cyan);
        Log();

        Log($"Finding path for {alias}");
        var searcher = new DirectorySearcher()
        {
            Filter = $"(mailNickname={alias})"
        };
        searcher.PropertiesToLoad.Clear();
        searcher.PropertiesToLoad.Add("distinguishedName");

        var results = searcher.FindAll();

        var userLdapPath = results?[0].Properties["distinguishedName"][0];
        var user = new DirectoryEntry($"GC://{userLdapPath}");
        Log($"Found LDAP path {userLdapPath}", ConsoleColor.Cyan);

        Log($"User information:", ConsoleColor.Cyan);

        // Limit the number of properties listed to keep the console
        // spew from being too overwhelming
        var maxPropesToList = 10;
        var propsListed = 0;
        foreach (var name in user.Properties.PropertyNames)
        {
            if (propsListed++ >= maxPropesToList)
            {
                break;
            }

            // Some property values have a single value while others
            // have multiple values. Display them accordingly
            var value = user.Properties[name.ToString()];
            if (value.Count == 1)
            {
                Log($"{name}: {value.Value.ToString()}", ConsoleColor.DarkGray);
            }

            if (value.Count > 1)
            {
                Log($"{name}:", ConsoleColor.DarkGray);
                foreach (var val in value)
                {
                    Log($"\t\t{val.ToString()}", ConsoleColor.DarkGray);
                }
            }
        }

        Log();

        Log("Searching for user's management chain");
        var managementChain = GetManagementChain(user);
        Log($"  {string.Join(" -> ", managementChain.Select(de => de.Properties["mailNickname"].Value.ToString()))}", ConsoleColor.Cyan);

        Log("Finding user's group memberships");
        var groups = user.Properties["memberOf"];
        Log($"Found {groups.Count} memberships", ConsoleColor.Cyan);
        foreach (var group in groups)
        {
            Log(group.ToString(), ConsoleColor.DarkCyan);
        }

        Log();
        Log("- Done -");
    }

    /// <summary>
    /// Recursively follows 'manager' properties to build a user's management chain
    /// </summary>
    /// <param name="user">The user whose management chain should be returned</param>
    /// <returns>Enumerable of managers (ordered nearest to farthest up the chain)</returns>
    private static IEnumerable<DirectoryEntry> GetManagementChain(DirectoryEntry user)
    {
        string managerPath = null;
        var managerPathProperties = user.Properties["manager"];
        if (managerPathProperties?.Count > 0)
        {
            managerPath = managerPathProperties[0].ToString();
        }

        if (string.IsNullOrEmpty(managerPath))
        {
            return new DirectoryEntry[] { user };
        }

        return Enumerable.Concat(new DirectoryEntry[] { user }, GetManagementChain(new DirectoryEntry($"GC://{managerPath}")));
    }
}
