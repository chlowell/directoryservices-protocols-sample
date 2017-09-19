// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.DirectoryServices.Protocols.Services
{
    public class DirectorySearcher : IDisposable
    {
        // Private fields
        /////////////////

        private const string DefaultFilter = "(objectClass=*)";

        private DirectoryEntry _searchRoot;
        private string _filter = DefaultFilter;
        private List<string> _propertiesToLoad;
        private bool _disposed = false;
        private bool _rootEntryAllocated = false;
        private SearchScope _scope = SearchScope.Subtree;

        // Constructors
        ///////////////

        public DirectorySearcher()
            : this(null, DefaultFilter, null, SearchScope.Subtree)
        {
        }

        public DirectorySearcher(DirectoryEntry searchRoot)
            : this(searchRoot, DefaultFilter, null, SearchScope.Subtree)
        {
        }

        public DirectorySearcher(DirectoryEntry searchRoot, string filter)
            : this(searchRoot, filter, null, SearchScope.Subtree)
        {
        }

        public DirectorySearcher(DirectoryEntry searchRoot, string filter, string[] propertiesToLoad)
            : this(searchRoot, filter, propertiesToLoad, SearchScope.Subtree)
        {
        }

        public DirectorySearcher(string filter)
            : this(null, filter, null, SearchScope.Subtree)
        {
        }

        public DirectorySearcher(string filter, string[] propertiesToLoad)
            : this(null, filter, propertiesToLoad, SearchScope.Subtree)
        {
        }

        public DirectorySearcher(string filter, string[] propertiesToLoad, SearchScope scope)
            : this(null, filter, propertiesToLoad, scope)
        {
        }

        public DirectorySearcher(DirectoryEntry searchRoot, string filter, string[] propertiesToLoad, SearchScope scope)
        {
            _searchRoot = searchRoot;
            _filter = filter;
            if (propertiesToLoad != null)
            {
                PropertiesToLoad.AddRange(propertiesToLoad);
            }

            this.SearchScope = scope;
        }

        // Public properties
        ////////////////////

        public string Filter
        {
            get
            {
                return _filter;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = DefaultFilter;
                }

                _filter = value;
            }
        }

        public List<string> PropertiesToLoad
        {
            get
            {
                if (_propertiesToLoad == null)
                {
                    _propertiesToLoad = new List<string>();
                }

                return _propertiesToLoad;
            }
        }

        public DirectoryEntry SearchRoot
        {
            get
            {
                if (_searchRoot == null)
                {
                    // get the default naming context. This should be the default root for the search.
                    using (var rootDSE = new DirectoryEntry(string.Empty, null, null, AuthenticationTypes.Secure))
                    {
                        var defaultNamingContext = (string)rootDSE.Properties["defaultNamingContext"][0];
                        _searchRoot = new DirectoryEntry("LDAP://" + defaultNamingContext, null, null, AuthenticationTypes.Secure);
                        _rootEntryAllocated = true;
                    }
                }

                return _searchRoot;
            }

            set
            {
                if (_rootEntryAllocated)
                {
                    _searchRoot.Dispose();
                }

                _rootEntryAllocated = false;
                _searchRoot = value;
            }
        }

        public SearchScope SearchScope
        {
            get
            {
                return _scope;
            }

            set
            {
                if (value < SearchScope.Base || value > SearchScope.Subtree)
                {
                    throw new ArgumentException($"Invalid Scope value ({(int)value})", nameof(value));
                }

                // If AttributeSearchQuery is supported, it should be confirmed that ASQ isn't set
                // while requesting Scope other than base (as that configuration is unsupported)

                _scope = value;

                // If AttributeSearchQuery is used, additional information must be tracked regarding
                // whether scope was set explicitly or implicitly (since a non-base scope can't
                // be explicitly set at the same time as ASQ)
            }
        }

        // Public methods
        /////////////////

        public SearchResult FindOne() => FindAll(1).FirstOrDefault();

        public SearchResultCollection FindAll() => FindAll(-1);

        /// <summary>
        /// Queries for directory entries matching a given filter up to a specified count
        /// </summary>
        /// <param name="count">The number of entries to find (-1 for no limit)</param>
        /// <returns>A SearchResultCollection containing matching directory entries</returns>
        public SearchResultCollection FindAll(int count)
        {
            // Always load ADsPath so that DirectoryEntry entities can be created
            if (!PropertiesToLoad.Contains("ADsPath"))
            {
                PropertiesToLoad.Add("ADsPath");
            }

            // Create an LDAP SearchRequest using the root's name and the provided filter, scope, and properties
            var req = new SearchRequest(SearchRoot.DistinguishedName, Filter, SearchScope, PropertiesToLoad.ToArray());

            if (count > 0)
            {
                // Limit the result size, if necessary
                req.SizeLimit = count;
            }

            // Use the SearchRoot's connection since it should already exist.
            var res = SearchRoot.Connection.SendRequest(req) as SearchResponse;

            if (res.ResultCode != ResultCode.Success)
            {
                throw new InvalidOperationException($"Error connecting to Active Directory: {res.ResultCode.ToString()}: {res.ErrorMessage}");
            }

            // Create and return a SearchResultCollection with the returned entries
            return new SearchResultCollection(SearchRoot, res.Entries, PropertiesToLoad);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_rootEntryAllocated)
                {
                    _searchRoot.Dispose();
                }

                _rootEntryAllocated = false;
                _disposed = true;
            }
        }
    }
}
