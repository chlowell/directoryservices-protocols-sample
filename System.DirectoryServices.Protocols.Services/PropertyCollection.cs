// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.DirectoryServices.Protocols.Services
{
    /// <summary>
    /// Properties for a direcetory entry
    /// </summary>
    public class PropertyCollection : IDictionary<string, PropertyValueCollection>
    {
        private const string DefaultFilter = "(objectClass=*)";
        private DirectoryEntry _entry;
        private Dictionary<string, PropertyValueCollection> _valueTable;

        public PropertyCollection(DirectoryEntry entry)
        {
            this._entry = entry;

            // System.DirectoryServices seems to query properties on a one-off basis (_entry.AdsObject.GetEx(_propertyName, out var);)
            // but I don't see an S.DS.P equivalent for that, so just load all properties once a PropertyCollection is needed.
            // It's possible that ADSI (which System.DirectoryServices uses) does the same thing under the covers, but I haven't looked into it yet.
            // More ADSI information: https://msdn.microsoft.com/en-us/library/aa772170(v=vs.85).aspx
            PopulateTable();
        }

        public IEnumerable<string> PropertyNames => Keys;

        /// <summary>
        /// Retrieves a property by name
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Property value(s) as a PropertyValueCollection</returns>
        public PropertyValueCollection this[string propertyName]
        {
            get
            {
                if (propertyName == null)
                {
                    throw new ArgumentNullException(nameof(propertyName));
                }

                // Return the values if they exist in the cached values table
                if (_valueTable.ContainsKey(propertyName))
                {
                    return _valueTable[propertyName];
                }
                else
                {
                    // To match System.DirectoryServices behavior, non-existent properties should
                    // just return an empty values collection
                    var value = new PropertyValueCollection(_entry, propertyName);
                    _valueTable.Add(propertyName, value);
                    return value;
                }
            }

            set
            {
                // This sample only supports read-only scenarios, currently
                throw new NotImplementedException();
            }
        }

        // Request properties for a given entity via LDAP and store them as PropertyValueCollection objects
        private void PopulateTable()
        {
            _valueTable = new Dictionary<string, PropertyValueCollection>();

            // Construct and send the LDAP request
            var req = new SearchRequest(_entry.DistinguishedName, DefaultFilter, SearchScope.Base, null);
            var res = _entry.Connection.SendRequest(req) as SearchResponse;

            if (res.ResultCode != ResultCode.Success)
            {
                throw new InvalidOperationException($"Error connecting to Active Directory: {res.ResultCode.ToString()}: {res.ErrorMessage}");
            }

            var entry = res.Entries?[0];
            if (entry == null)
            {
                throw new InvalidOperationException($"Error retrieving properties for {_entry.DistinguishedName}; entry not found in AD.");
            }

            foreach (DirectoryAttribute attr in entry.Attributes.Values)
            {
                _valueTable.Add(attr.Name, new PropertyValueCollection(_entry, attr));
            }
        }

        public bool Contains(string key) => ContainsKey(key);

        // IDictionary<string, PropertyValueCollection> implementation
        //////////////////////////////////////////////////////////////

#pragma warning disable SA1201 // Elements must appear in the correct order; disabled so that IDictionary implementation can be kept together
        public IEnumerable<string> Keys => _valueTable.Keys;
#pragma warning restore SA1201 // Elements must appear in the correct order

        public IEnumerable<PropertyValueCollection> Values => _valueTable.Values;

        public int Count => _valueTable.Count;

        ICollection<string> IDictionary<string, PropertyValueCollection>.Keys => _valueTable.Keys;

        ICollection<PropertyValueCollection> IDictionary<string, PropertyValueCollection>.Values => _valueTable.Values;

        public bool IsReadOnly => true; // For now...

        public bool ContainsKey(string key) => _valueTable.ContainsKey(key);

        public bool TryGetValue(string key, out PropertyValueCollection value) => _valueTable.TryGetValue(key, out value);

        public bool Contains(KeyValuePair<string, PropertyValueCollection> item) => _valueTable.Contains(item);

        public void CopyTo(KeyValuePair<string, PropertyValueCollection>[] array, int arrayIndex) => ((ICollection)_valueTable).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, PropertyValueCollection>> GetEnumerator() => _valueTable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _valueTable.GetEnumerator();

        // This sample currently only supports read-only scenarios, so the following
        // APIs are unimplemented.

        public void Add(string key, PropertyValueCollection value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, PropertyValueCollection> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, PropertyValueCollection> item)
        {
            throw new NotImplementedException();
        }
    }
}
