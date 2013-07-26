namespace Oclc
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Security.Cryptography;
    using System.Web;
    using Oclc.Exceptions;

    /// <summary>
    /// Creates the pieces required to perform WSKey V2 authorization with OCLC
    /// This is a direct port of java/php code from Oclc (http://www.oclc.org/developer/platform/wskey2-client-libraries)
    /// </summary>
    public class WSKeyV2Request
    {
        /*
         * Building a signature for the Authorization Header

        To build the signature normalize the request string by concatenating together the following elements each followed by a new line
            The WSKey
            The timestamp value calculated for the request.
            The nonce value generated for the request.
            The request entity-body hash if one was calculated and included in the request, otherwise, an empty string.
            The HTTP request method in upper case. For example: "HEAD", "GET", "POST", etc.
            www.oclc.org
            443
            /wskey
            The query component of the web service request URI normalized as described in Section 3.3.1.1 of HTTP MAC authentication scheme.
        HMAC-SHA-256 hash the normalized request string using the secret
        Base_64 encode the hmac-sha-256 normalized request */

        #region Properties

        private long _Timestamp;
        /// <summary>
        /// Current unix (posix) timestap
        /// </summary>
        public long Timestamp
        {
            get
            {
                if (_Timestamp == 0) _Timestamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                return _Timestamp;
            }
            set
            {
            	_Timestamp = value;
            }
        }

        private string _Nonce;
        /// <summary>
        /// Random eight character hex field
        /// </summary>
        public string Nonce
        {
            get
            {
                if (String.IsNullOrEmpty(_Nonce))
                {
                    Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                    _Nonce = random.Next().ToString("X");
                }
                return _Nonce;
            }
            set
            {
            	_Nonce = value;
            }
        }

        /// <summary>
        /// Url of the web service that we're authorizing against.
        /// </summary>
        public Uri Url { get; set; }
        
        private Uri _SignatureUrl;
        /// <summary>
        /// Url used to build the request signature.
        /// </summary>
        public Uri SignatureUrl
        {
            get
            {
                if (_SignatureUrl == null)
                {
                    _SignatureUrl = new Uri("https://www.oclc.org/wskey");
                }
                return _SignatureUrl;
            }
        }

        private string _Method;
        /// <summary>
        /// POST or GET
        /// </summary>
        public string Method
        {
            get { return _Method; }
            set
            {
                if (value != "POST" && value != "GET")
                {
                    throw new WSKeyClientException("Method must be POST or GET");
                }
                else
                {
                    _Method = value;
                }
            }
        }

        /// <summary>
        /// HMACSHA256 Base 64 encoded hash of the request body.
        /// </summary>
        public string BodyHash { get; set; }
        
        #endregion

        #region Constants

        /// <summary>
        /// WSKey V2 - Replace this with your AE webservice key
        /// </summary>
        private const string ClientId = "";

        /// <summary>
        /// Secret - Replace this with your AE secret
        /// </summary>
        // Sandbox secret
        private const string Secret = "";

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public WSKeyV2Request() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">GET or POST</param>
        /// <param name="url">The url for the service request</param>
        /// <param name="bodyHash">Hash of message body - may be blank.</param>
        public WSKeyV2Request(string method, string url, string bodyHash)
        {
            this.Method = method;
            this.Url = new Uri(url);
            this.BodyHash = bodyHash;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the header key value for WSKeyV2 auth.
        /// </summary>
        /// <returns>String</returns>
        public string GetAuthorizationHeader()
        {
            return "Authorization";
        }

        /// <summary>
        /// Returns the header value for WSKeyV2 auth, including signature.
        /// </summary>
        /// <returns>String</returns>
        public string GetAuthorizationHeaderValue()
        {
            VerifyParams();

            StringBuilder header = new StringBuilder();

            // SCHEME URL
            header.Append(@"http://www.worldcat.org/wskey/v2/hmac/v1 ");

            // WSKEY -> ClientID
            header.Append(String.Format("clientId=\"{0}\", ", WSKeyV2Request.ClientId));

            // POSIX TIMESTAMP
            header.Append(String.Format("timestamp=\"{0}\", ", this.Timestamp));

            // NONCE
            header.Append(String.Format("nonce=\"{0}\", ", this.Nonce));

            // SIGNATURE
            header.Append(String.Format("signature=\"{0}\"", GetSignature()));

            return header.ToString();
        }

        /// <summary>
        /// Calculates the HMACSHA256 Base 64 encoded hash of the supplied string.
        /// </summary>
        /// <param name="body">String</param>
        /// <returns>String</returns>
        public string CalculateBodyHash(string body)
        {
            return CalculateBodyHash(Encoding.UTF8.GetBytes(body));
        }

        /// <summary>
        /// Calculates the HMACSHA256 Base 64 encoded hash of the supplied byte array.
        /// </summary>
        /// <param name="body">Byte array</param>
        /// <returns>String</returns>
        public string CalculateBodyHash(byte[] body)
        {
            HMACSHA256 hash = new HMACSHA256();  // randomly generated key?
            byte[] digest = hash.ComputeHash(body);

            return Convert.ToBase64String(digest);
        }

        /// <summary>
        /// Returns the request signature -> base64(hashed(normalized request))
        /// </summary>
        /// <returns></returns>
        private string GetSignature()
        {
            HMACSHA256 hash = new HMACSHA256(Encoding.UTF8.GetBytes(WSKeyV2Request.Secret));

            string normalizedRequest = GetNormalizedRequest();

            byte[] digest = hash.ComputeHash(Encoding.UTF8.GetBytes(normalizedRequest));
            
            return Convert.ToBase64String(digest);
        }

        /// <summary>
        /// Request normalized as described in Section 3.3.1.1 of HTTP MAC authentication scheme
        /// </summary>
        /// <returns></returns>
        public string GetNormalizedRequest()
        {
            StringBuilder normalizedRequest = new StringBuilder();
            normalizedRequest.Append(WSKeyV2Request.ClientId).Append("\n");
            normalizedRequest.Append(this.Timestamp).Append("\n");
            normalizedRequest.Append(this.Nonce).Append("\n");
            normalizedRequest.Append(this.BodyHash == null ? "" : this.BodyHash).Append("\n");
            normalizedRequest.Append(this.Method).Append("\n");
            normalizedRequest.Append(this.SignatureUrl.Host).Append("\n");
            normalizedRequest.Append(this.SignatureUrl.Port).Append("\n");  // may need to check if Port == -1
            normalizedRequest.Append(this.SignatureUrl.PathAndQuery).Append("\n"); 

            SortedDictionary<string, string> queryParams = new SortedDictionary<string, string>(StringComparer.Ordinal);  // check if comparer needed
            
            string query = this.Url.Query;

            if (!String.IsNullOrEmpty(query)) query = query.Remove(0, 1);  // query always starts with a ?, lose it

            if (!String.IsNullOrEmpty(query))
            {
                string[] pairs = query.Split(new char[] { '&' });
                foreach (string pair in pairs)
                {
                    string value, name;
                    string[] parts = pair.Split(new char[] { '=' });
                                        
                    if (parts.Length == 1)
                    {
                        name = parts[0];
                        value = "";
                    }
                    else
                    {
                        name = parts[0];
                        value = parts[1];
                    }

                    name = HttpUtility.UrlDecode(name);
                    value = HttpUtility.UrlDecode(value);

                    name = HttpUtility.UrlEncode(name, Encoding.UTF8).Replace("+", "%20").Replace("*", "%2A").Replace("%7E", "~");
                    value = HttpUtility.UrlEncode(value, Encoding.UTF8).Replace("+", "%20").Replace("*", "%2A").Replace("%7E", "~");

                    queryParams.Add(name, value);  // the collection is sorted as items are added
                }

                foreach (var queryParam in queryParams)
                {
                    normalizedRequest.Append(String.Format("{0}={1}\n", queryParam.Key, queryParam.Value));
                }
            }

            return normalizedRequest.ToString();
        }

        private void VerifyParams()
        {
            if (this.Method == null)
            {
                this.Method = "POST";
            }

            if (String.IsNullOrEmpty(ClientId) || String.IsNullOrEmpty(Secret))
            {
                throw new WSKeyClientException("You must supply your web service authentication key and secret.");
            }

            if (this.Url == null)
            {
                throw new WSKeyClientException("Url not supplied.");
            }
        }

        #endregion
    }
}
