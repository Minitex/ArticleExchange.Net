namespace Oclc.ArticleExchange
{
    using System.IO;
    using System.Xml.Serialization;
    using System.Xml;
    using System.Text;

    /// <summary>
    /// 
    /// </summary>
    public class AEResponse
    {
        #region Properties

        private string _Url;
        public string Url
        {
            get
            {
                return _Url;
            }
        }

        private string _Password;
        public string Password
        {
            get
            {
                return _Password;
            }
        }

        private string rawResponse { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Parameter-less constructor
        /// </summary>
        public AEResponse() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="response">Raw response from the web service.</param>
        public AEResponse(byte[] response)
        {
            this.rawResponse = Encoding.UTF8.GetString(response);
            this.Process();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="response">Raw response from the web service.</param>
        public AEResponse(string response)
        {
            this.rawResponse = response;
            this.Process();
        }

        #endregion

        #region Methods

        void Process()
        {
            string temp = this.rawResponse;
            temp = temp.Replace(" xmlns=\"\"", "");

            MemoryStream xmlStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(temp));

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(AEResponseXml), "http://www.w3.org/2005/Atom");

            AEResponseXml x = (AEResponseXml)xmlSerializer.Deserialize(xmlStream);

            this._Url = x.content.uploadResponse.accessInformationResponse.url;
            this._Password = x.content.uploadResponse.accessInformationResponse.password;
        }

        #endregion

    }

    // get rid of compiler warnings for unassigned field values for the internal classes
#pragma warning disable 0649

    #region Xml Data Contract

    [XmlRoot(ElementName="entry")]
    public class AEResponseXml
    {
        public string title { get; set; }
        public string updated { get; set; }
        public ContentXml content { get; set; }
    }

    public class ContentXml
    {
        [XmlAttribute]
        public string type { get; set; }

        [XmlElement(Namespace="http://worldcat.org/uploadResponse")]
        public UploadResponseXml uploadResponse { get; set; }
    }

    public class UploadResponseXml
    {
        public AccessInformationResponseXml accessInformationResponse { get; set; }
        public ArticleInformationResponseXml articleInformationResponse { get; set; }
        public BorrowerInfoResponseXml borrowerInfoResponse { get; set; }
        public RequestIdResponseXml requestIdResponse { get; set; }
    }

    public class AccessInformationResponseXml
    {
        public string password { get; set; }
        public string url { get; set; }
    }

    public class ArticleInformationResponseXml
    {
        public string number { get; set; }
        public string date { get; set; }
        public string title { get; set; }
        public string articleAuthor { get; set; }
        public string articleTitle { get; set; }
        public string volume { get; set; }
        public string pages { get; set; }
    }

    public class BorrowerInfoResponseXml
    {
        public string borrowingSymbol { get; set; }
        public string borrowingEmail { get; set; }
    }

    public class RequestIdResponseXml
    {
        public string oclcId { get; set; }
        public string illiadId { get; set; }
        public string vdxId { get; set; }
    }
    #endregion
}
