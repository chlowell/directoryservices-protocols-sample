// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System.Linq;
using System.Net;

namespace System.DirectoryServices.Protocols.Services
{
    /// <summary>
    /// Encapsulates a node or an object in the Active Directory hierarchy
    /// </summary>
    public class DirectoryEntry : IDisposable
    {
        // Private fields
        /////////////////

        private string _path = string.Empty;
        private NetworkCredential _credentials;
        private AuthenticationTypes _authenticationType = AuthenticationTypes.Secure;
        private bool _userNameIsNull = false;
        private bool _passwordIsNull = false;
        private bool _disposed = false;
        private LdapConnection _ldapConnection;
        private PropertyCollection _propertyCollection;
        private string _distinguishedName;
        private string _relativeDistinguishedName;

        // Constructors/Destructor
        ///////////////

        public DirectoryEntry()
        {
        }

        public DirectoryEntry(string path)
            : this()
        {
            Path = path;
        }

        public DirectoryEntry(string path, string username, string password, AuthenticationTypes authenticationType)
            : this(path)
        {
            // Store credentials separate from _ldapConnection since LdapConnection.Credential is read-only
            _credentials = new NetworkCredential(username, password);

            if (username == null)
            {
                _userNameIsNull = true;
            }

            if (password == null)
            {
                _passwordIsNull = true;
            }

            _authenticationType = authenticationType;
        }

        ~DirectoryEntry()
        {
            Dispose(false);
        }

        // Non-public properties
        ////////////////////

        /// <summary>
        /// Gets the underlying LDAP connection used for communicating with the AD server
        /// </summary>
        internal LdapConnection Connection
        {
            get
            {
                Bind();
                return _ldapConnection;
            }
        }

        // Public properties
        ////////////////////

        public string Name
        {
            get
            {
                // Instead of using ADSI, just parse the relative name
                // from the LDAP path. Would be intersting to compare this
                // to what ADSI is doing, though.
                return RelativeDistinguishedName;
            }
        }

        /// <summary>
        /// Gets the distinguished name of the entity based on its path
        /// </summary>
        public string DistinguishedName
        {
            get
            {
                if (string.IsNullOrEmpty(_distinguishedName))
                {
                    _distinguishedName = Path?.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                }

                return _distinguishedName;
            }
        }

        /// <summary>
        /// Gets the relative distinguished name of the entity based on its path
        /// </summary>
        public string RelativeDistinguishedName
        {
            get
            {
                if (string.IsNullOrEmpty(_relativeDistinguishedName))
                {
                    _relativeDistinguishedName = DistinguishedName?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                }

                return _relativeDistinguishedName;
            }
        }

        public AuthenticationTypes AuthenticationType
        {
            get
            {
                return _authenticationType;
            }

            set
            {
                if (_authenticationType == value)
                {
                    return;
                }

                _authenticationType = value;

                // LDAP connection should be re-created after changing auth type
                Unbind();
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }

            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                // Setting the path to the existing path value results in no change
                if (string.Equals(_path, value, StringComparison.CurrentCultureIgnoreCase))
                {
                    return;
                }

                _path = value;

                // LDAP connection should be re-created after changing the path
                Unbind();
            }
        }

        /// <summary>
        /// Gets a property collection for the entry
        /// </summary>
        public PropertyCollection Properties
        {
            get
            {
                if (_propertyCollection == null)
                {
                    // PropertyCollection can be initialized from a DirectoryEntry
                    _propertyCollection = new PropertyCollection(this);
                }

                return _propertyCollection;
            }
        }

        public string Username
        {
            get
            {
                if (_credentials == null || _userNameIsNull)
                {
                    return null;
                }

                return _credentials.UserName;
            }

            set
            {
                if (value == GetUsername())
                {
                    return;
                }

                if (_credentials == null)
                {
                    _credentials = new NetworkCredential();
                    _passwordIsNull = true;
                }

                if (value == null)
                {
                    _userNameIsNull = true;
                }
                else
                {
                    _userNameIsNull = false;
                }

                _credentials.UserName = value;

                Unbind();
            }
        }

        // Non-public methods
        /////////////////////

        /// <summary>
        /// Creates the underlying LDAP connection
        /// </summary>
        private void Bind()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (_ldapConnection == null)
            {
                // Connect to the specified domain
                _ldapConnection = new LdapConnection(GetDomainFromPath());
                if (_credentials != null)
                {
                    _ldapConnection.Credential = _credentials;
                }

                _ldapConnection.SessionOptions.AutoReconnect = true;

                // _ldapConnection.SessionOptions.HostReachable could be use as a sanity/health check here
            }
        }

        private string GetDomainFromPath()
        {
            // Domain components appear in the path prefixed with DC=
            // This selects and concatenates those components
            var domainComponents = Path?.Split(new[] { ',', ':', '/' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(rdn => rdn.StartsWith("DC=", StringComparison.OrdinalIgnoreCase))
                        .Select(rdn => rdn.Substring(3).Trim())
                        ?? Enumerable.Empty<string>();
            return string.Join(".", domainComponents);
        }

        private void Unbind()
        {
            _ldapConnection?.Dispose();
            _ldapConnection = null;
            _distinguishedName = null;
            _relativeDistinguishedName = null;
        }

        internal string GetUsername()
        {
            if (_credentials == null || _userNameIsNull)
            {
                return null;
            }

            return _credentials.UserName;
        }

        internal string GetPassword()
        {
            if (_credentials == null || _passwordIsNull)
            {
                return null;
            }

            return _credentials.Password;
        }

        internal NetworkCredential GetCredentials()
        {
            return _credentials;
        }

        protected void Dispose(bool disposing)
        {
            Unbind();

            if (!_disposed)
            {
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
