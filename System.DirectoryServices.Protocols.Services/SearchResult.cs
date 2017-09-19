// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System.Collections.Generic;
using System.Net;

namespace System.DirectoryServices.Protocols.Services
{
    /// <summary>
    /// Search result entity (with a path and collection of properties)
    /// </summary>
    public class SearchResult
    {
        private NetworkCredential _parentCredentials;
        private AuthenticationTypes _parentAuthenticationType;
        private IDictionary<string, IReadOnlyList<object>> _properties = new Dictionary<string, IReadOnlyList<object>>();

        internal SearchResult(NetworkCredential parentCredentials, AuthenticationTypes parentAuthenticationType)
        {
            _parentCredentials = parentCredentials;
            _parentAuthenticationType = parentAuthenticationType;
        }

        public string Path
        {
            get
            {
                // Path corresponds to the ADsPath property
                return (string)Properties["ADsPath"][0];
            }
        }

        // The proeprties dictionary is the primary data stored in this type
        public IDictionary<string, IReadOnlyList<object>> Properties
        {
            get
            {
                return _properties;
            }
        }

        /// <summary>
        /// Retrieves the directory entry corresponding to a search result
        /// </summary>
        /// <returns>Directory entry for the given search result's path</returns>
        public DirectoryEntry GetDirectoryEntry()
        {
            if (_parentCredentials != null)
            {
                return new DirectoryEntry(Path, _parentCredentials.UserName, _parentCredentials.Password, _parentAuthenticationType);
            }
            else
            {
                var newEntry = new DirectoryEntry(Path, null, null, _parentAuthenticationType);
                return newEntry;
            }
        }
    }
}
