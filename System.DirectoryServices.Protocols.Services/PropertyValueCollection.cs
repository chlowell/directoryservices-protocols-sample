// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace System.DirectoryServices.Protocols.Services
{
    /// <summary>
    /// A value which can either be a single object or an array of objects
    /// </summary>
    public class PropertyValueCollection : IList<object>
    {
        private DirectoryEntry _entry;
        private string _propertyName;
        private List<object> _values;

        public PropertyValueCollection(DirectoryEntry entry, DirectoryAttribute attr)
            : this(entry, attr?.Name)
        {
            // Populate the _values list
            for (var i = 0; i < attr.Count; i++)
            {
                _values.Add(attr[i]);
            }
        }

        public PropertyValueCollection(DirectoryEntry entry, string name)
        {
            _entry = entry;
            _propertyName = name;

            _values = new List<object>();
        }

        public string PropertyName => _propertyName;

        public object Value
        {
            // Return either null, a single object, or an array
            // depending on the number of values in _values
            get
            {
                if (Count == 0)
                {
                    return null;
                }
                else if (Count == 1)
                {
                    return _values[0];
                }
                else
                {
                    return _values.ToArray();
                }
            }

            set
            {
                // This sample currently only supports read-only scenarios
                throw new NotImplementedException();
            }
        }

        // IList<object> implementation
        ///////////////////////////////

        public int Count => _values.Count;

        public bool IsReadOnly => true; // For now...

        public object this[int index] { get => _values[index]; set => throw new NotImplementedException(); }

        public bool Contains(object item) => _values.Contains(item);

        public void CopyTo(object[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);

        public IEnumerator<object> GetEnumerator() => _values.GetEnumerator();

        public int IndexOf(object item) => _values.IndexOf(item);

        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

        // This sample is read-only for now
        public void Add(object item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(object item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}
