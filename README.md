Oclc.ArticleExchange
====================

Oclc.ArticleExchange is a shared library written in C# that will post any given file to OCLC's [Article Exchange web service](http://www.oclc.org/developer/services/article-exchange-api).  It currently targets .NET 4.0 but should work fine on any .NET version back to 2.0.  The necessary credentials that are required to use Article Exchange are an OCLC authorization and password, and an Article Exchange web service key and secret.  Both of these are hard coded into the library in AERequest.cs and WSKeyV2Request.cs files.

To Do
====================

1. Implement the JSON response format, currently it only uses XML.
2. Catch all possible exceptions appropriately to pass out of the library.
3. Convert stored credentials in memory to SecureString.

Bug Fixes
====================
8/13/2013 - Implemented multipart http post of documents.  This was done so we can set the uploaded file's content-type appropriately.  Without this, interoperability with Illiad wasn't working correctly.


