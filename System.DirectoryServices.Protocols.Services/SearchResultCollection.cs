// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace System.DirectoryServices.Protocols.Services
{
    /// <summary>
    /// A collection of SearchResult objects populated from a SearchResultEntryCollection and
    /// a list of properties to load for each entry.
    /// </summary>
    public class SearchResultCollection : IEnumerable<SearchResult>
    {
        private DirectoryEntry _searchRoot;

        // This List holds the results and this type just wraps the List
        private List<SearchResult> _entries;
        private List<string> _properties;

        internal SearchResultCollection(DirectoryEntry searchRoot, SearchResultEntryCollection entries, List<string> propertiesLoaded)
        {
            _searchRoot = searchRoot;
            _entries = new List<SearchResult>();
            foreach (SearchResultEntry entry in entries)
            {
                // Create a search result for each entry, populating its user, auth type, and properties
                var result = new SearchResult(_searchRoot.GetCredentials(), _searchRoot.AuthenticationType);
                result.Properties["distinguishedName"] = new List<string>(new[] { entry.DistinguishedName });

                // Translate SearchResultEntryCollection attribute values into SearchResult properties
                foreach (DirectoryAttribute attr in entry.Attributes.Values)
                {
                    var properties = new List<object>();

                    // Use for instead of foreach because DirectoryAttribute's index
                    // property does fancy casting to string as needed.
                    for (var i = 0; i < attr.Count; i++)
                    {
                        properties.Add(attr[i]);
                    }

                    result.Properties[attr.Name] = properties;
                }

                _entries.Add(result);
            }

            _properties = propertiesLoaded;
        }

        // Methods wrapping _entries (List<SearchResult>)
        /////////////////////////////////////////////////

        public int Count => _entries.Count;

        public string[] PropertiesLoaded => _properties.ToArray();

        public SearchResult this[int index] => _entries[index];

        public int IndexOf(SearchResult result) => _entries.IndexOf(result);

        public void CopyTo(SearchResult[] results, int index) => _entries.CopyTo(results, index);

        public bool Contains(SearchResult result) => _entries.Contains(result);

        public IEnumerator<SearchResult> GetEnumerator() => _entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();
    }
}
