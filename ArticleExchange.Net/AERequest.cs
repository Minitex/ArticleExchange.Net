namespace Oclc.ArticleExchange
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using Oclc.WSKeyV2;

    /// <summary>
    /// Encapsulates a request to the Article Exchange web service
    /// </summary>
    public class AERequest
    {
        #region Properties

        /// <summary>
        /// Requester OCLC symbol
        /// </summary>
        public string requesterOCLCSymbol { get; set; }
        /// <summary>
        /// Requester email address
        /// </summary>
        public string requesterEmail { get; set; }
        /// <summary>
        /// OCLC ILL Request number
        /// </summary>
        public string oclcRequestId { get; set; }
        /// <summary>
        /// Illiad transaction number
        /// </summary>
        public string illiadRequestId { get; set; }
        /// <summary>
        /// VDX request number
        /// </summary>
        public string vdxRequestId { get; set; }
        /// <summary>
        /// Journal Title
        /// </summary>
        public string journalTitle { get; set; }
        /// <summary>
        /// Article Title
        /// </summary>
        public string articleTitle { get; set; }
        /// <summary>
        /// Article Author
        /// </summary>
        public string articleAuthor { get; set; }
        /// <summary>
        /// Article Volume
        /// </summary>
        public string articleVolume { get; set; }
        /// <summary>
        /// Article Issue
        /// </summary>
        public string articleIssue { get; set; }
        /// <summary>
        /// Article Date
        /// </summary>
        public string articleDate { get; set; }
        /// <summary>
        /// Article Pages
        /// </summary>
        public string articlePages { get; set; }

        /// <summary>
        /// Returns service endpoint and query string
        /// Throws AEException
        /// </summary>
        public string Url
        {
            get
            {
                StringBuilder url = new StringBuilder();
                url.Append("https://ill.sd00.worldcat.org/articleexchange/?");

                // Required parameters
                url.Append("autho=" + AERequest.authorization + "&");
                url.Append("password=" + AERequest.password);

                // Non-required parameters
                if (!String.IsNullOrEmpty(this.requesterOCLCSymbol)) url.Append("&requesterInstSymbol=" + this.requesterOCLCSymbol);
                if (!String.IsNullOrEmpty(this.requesterEmail)) url.Append("&requesterEmail=" + this.requesterEmail);
                if (!String.IsNullOrEmpty(this.oclcRequestId)) url.Append("&oclcRequestId=" + this.oclcRequestId);
                if (!String.IsNullOrEmpty(this.illiadRequestId)) url.Append("&illiadRequestId=" + this.illiadRequestId);
                if (!String.IsNullOrEmpty(this.vdxRequestId)) url.Append("&vdxRequestId=" + this.vdxRequestId);
                if (!String.IsNullOrEmpty(this.journalTitle)) url.Append("&jTitle=" + this.journalTitle);
                if (!String.IsNullOrEmpty(this.articleTitle)) url.Append("&aTitle=" + this.articleTitle);
                if (!String.IsNullOrEmpty(this.articleAuthor)) url.Append("&aAuthor=" + this.articleAuthor);
                if (!String.IsNullOrEmpty(this.articleVolume)) url.Append("&aVolume=" + this.articleVolume);
                if (!String.IsNullOrEmpty(this.articleIssue)) url.Append("&aIssue=" + this.articleIssue);
                if (!String.IsNullOrEmpty(this.articleDate)) url.Append("&aDate=" + this.articleDate);
                if (!String.IsNullOrEmpty(this.articlePages)) url.Append("&aPages=" + this.articlePages);

                return url.ToString();
            }
        }

        #endregion

        #region Constructors and Constants

        /// <summary>
        /// Parameter-less constructor
        /// </summary>
        public AERequest() { }

        /// <summary>
        /// OCLC Authorization - Replace with your Oclc Autho
        /// </summary>
        private const string authorization = "autho";

        /// <summary>
        /// OCLC Authorization Password - Replace with your Oclc Autho Password
        /// </summary>
        private const string password = "password";

        /// <summary>
        /// HTTP method - always POST for Article Exchange
        /// </summary>
        private const string method = "POST";

        #endregion

        #region Methods
        
        /// <summary>
        /// Post the document to Article Exchange
        /// </summary>
        /// <param name="filename">The full path to the document</param>
        /// <returns>AEResponse</returns>
        public AEResponse PostDocument(string filename)
        {
            string responseString = this.UploadFile(filename, new Uri(this.Url));
            AEResponse response = new AEResponse(responseString);

            return response;
        }

        // IMPLEMENTATION OF MULTIPART HTTP POST
        // http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
        public string UploadFile(string uploadFile, Uri uri)
        {
            // GENERATE THE WSKEYV2 HEADER
            WSKeyV2Request wskey = new WSKeyV2Request()
            {
                Url = uri,
                Method = "POST"
            };

            string authHeaderName = wskey.GetAuthorizationHeader();
            string authHeaderValue = wskey.GetAuthorizationHeaderValue();

            // DETERMINE THE CONTENT TYPE OF THE UPLOADED FILE BASED ON ITS EXTENSION
            string contentType = this.GetContentType(Path.GetExtension(uploadFile));

            // DEFINE THE MULTIPART BOUNDARY STRING
            string boundaryDef = String.Format("---HTTPCLIENT-{0:x}", DateTime.Now.Ticks);
            string boundaryItem = String.Format("--{0}", boundaryDef);
            string finalBoundary = String.Format("{0}--", boundaryItem);
            byte[] finalBoundaryBytes = Encoding.UTF8.GetBytes(String.Format("\r\n{0}\r\n", finalBoundary));  // ENSURES ITS ON ITS OWN LINE

            // REQUEST HEADERS
            // POST /articleexchange//articleexchange/?autho=102-900-527&password=test HTTP/1.1
            // Host: ill.sd00.worldcat.org
            // Connection: close
            // User-Agent: Platform PHP Test Client
            // Accept: application/atom+xml
            // Authorization: http://www.worldcat.org/wskey/v2/hmac/v1 clientId="tsFsoBXToV1uR8GEMJCcxz9NYpVvutsA5cJAD9cnKUc4FGYEntM6UkcIVlYp4ZhYFteVLAxOWJDUV85W", timestamp="1362760533", nonce="44543195", signature="jgyjfRoPEzDotNq+yEzjRXydp/sFGxWtOPJQ2gBnnQE="
            // Content-Type: multipart/form-data; boundary=---HTTPCLIENT-364df2d228844ecc0b45d28d93758f98
            // Content-Length: 245
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(uri);
            webrequest.ContentType = String.Format("multipart/form-data; boundary={0}", boundaryDef);
            webrequest.Method = "POST";
            webrequest.Headers.Add(authHeaderName, authHeaderValue);

            // HEADER FOR POST
            // -----HTTPCLIENT-8d0633913c8481a
            // Content-Disposition: form-data; name="uploadFile"; filename="12345.pdf"
            // Content-Type: application/pdf
            StringBuilder postHeader = new StringBuilder();
            postHeader.Append(boundaryItem);
            postHeader.Append(Environment.NewLine);
            postHeader.Append(String.Format("Content-Disposition: form-data; name=\"uploadFile\"; filename=\"{0}\"", Path.GetFileName(uploadFile)));
            postHeader.Append(Environment.NewLine);
            postHeader.Append(String.Format("Content-Type: {0}", contentType));
            postHeader.Append(Environment.NewLine);
            postHeader.Append(Environment.NewLine);

            // CONVERT THE POST HEADER INTO A BYTE ARRAY TO WRITE INTO THE REQUEST STREAM
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader.ToString());

            // OPEN THE FILE TO BE UPLOADED
            FileStream fileStream = new FileStream(uploadFile, FileMode.Open, FileAccess.Read);

            // MUST SET THE CONTENT LENGTH BEFORE CALLING GETREQUESTSTREAM()
            webrequest.ContentLength = postHeaderBytes.Length + fileStream.Length + finalBoundaryBytes.Length;

            // OPEN THE REQUEST STREAM, WE WILL WRITE THE POST HEADER AND FILE TO IT AS BYTE ARRAYS
            Stream requestStream = webrequest.GetRequestStream();

            // WRITE THE POST HEADER TO THE REQUEST STREAM
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

            // WRITE THE UPLOADED FILE TO THE REQUEST STREAM
            byte[] buffer = new Byte[8192];

            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                requestStream.Write(buffer, 0, bytesRead);
            }

            // WRITE THE TRAILING BOUNDARY
            requestStream.Write(finalBoundaryBytes, 0, finalBoundaryBytes.Length);

            // CLOSE UP ALL OF THE OPEN STREAMS
            fileStream.Close();
            requestStream.Close();

            // POST THE FILE 
            WebResponse response = webrequest.GetResponse();

            // GRAB THE RESPONSE AND RETURN IT AS A STRING
            Stream s = response.GetResponseStream();
            StreamReader sr = new StreamReader(s, Encoding.UTF8);
            string output = sr.ReadToEnd();
            response.Close();
            sr.Close();

            return output;
        }

        private static readonly Dictionary<string, string> ContentTypeMap = new Dictionary<string, string>
        {
            { ".jpeg", "image/jpeg" },
            { ".jpg", "image/jpeg" },
            { ".jp2", "image/jp2" },
            { ".jpx", "image/jpx" },
            { ".jpm", "image/jpm" },
            { ".tiff", "image/tiff" },
            { ".tif", "image/tiff" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".pdf", "application/pdf" },
            { ".mdi", "image/vnd.ms-modi" },
            { ".zip", "application/zip" }
        };

        private string GetContentType(string extension)
        {
            string value;
            return ContentTypeMap.TryGetValue(extension, out value) ? value : "";
        }

        #endregion
    }
}
