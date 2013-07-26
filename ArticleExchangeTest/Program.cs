namespace ArticleExchangeTest
{
    using System;
    using Oclc.ArticleExchange;
    using Oclc.Exceptions;

    class Program
    {
        static void Main(string[] args)
        {
            const string filename = @"214045USB_worldsharewebservices.pdf";

            try
            {
                // Make sure to update your autho and password in AERequest.cs
                // And your ClientId and Secret in WSKeyV2Request.cs
                AERequest request = new AERequest();

                Console.WriteLine(String.Format("Posting {0}...", filename));
                AEResponse response = request.PostDocument(filename);

                Console.WriteLine("Success.");
                Console.WriteLine();

                Console.WriteLine(response.Url);
                Console.WriteLine(response.Password);
            }
            catch (WSKeyClientException ex)
            {
                Console.WriteLine("Trouble with the WSKey creation: " + ex.Message);
            }
            catch (AEException ex)
            {
                Console.WriteLine("Trouble posting the file: " + ex.Message);
            }
            catch (Exception)
            {
                Console.WriteLine("Trouble all around.");
            }
        }
    }
}
