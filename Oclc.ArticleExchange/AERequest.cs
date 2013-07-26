namespace Oclc.ArticleExchange
{
    using System;
    using System.Net;
    using System.Text;
    using Oclc.Exceptions;

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
            // Create a WSKeyV2 object for the AE url
            WSKeyV2Request wskey = new WSKeyV2Request()
            {
                Url = new Uri(this.Url),
                Method = method
            };

            string headerName = wskey.GetAuthorizationHeader();
            string headerValue = wskey.GetAuthorizationHeaderValue();

            AEResponse response = null;

            using (WebClient serviceRequest = new WebClient())
            {
                try
                {
                    // Add the WSKeyV2 Authorization header to the request
                    serviceRequest.Headers.Add(headerName, headerValue);

                    // Upload the file
                    byte[] responseBytes = serviceRequest.UploadFile(this.Url, AERequest.method, filename);

                    // Parse out the response
                    response = new AEResponse(responseBytes);
                }
                catch (Exception e)
                {
                    AEException ex = new AEException(e.Message);
                    throw ex;
                }
            }

            return response;
        }

        #endregion
    }
}
