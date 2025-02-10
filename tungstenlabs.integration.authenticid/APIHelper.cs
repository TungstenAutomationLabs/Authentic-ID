using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace tungstenlabs.integration.authenticid
{
    public class APIHelper
    {
        public string AuthenticateDocument(string SessionID, string DocumentID, string TOTALAGILITY_URL, string AUTHENTICID_URL, string AccessKey, string SecretToken, string DocumentType, bool AutoCrop, string submitterName, bool getImages, string Orientation, string senderIP, string JobID)
        {
            byte[] fileBytes = GetKTADocumentFile(DocumentID, TOTALAGILITY_URL, SessionID);
            string result = AuthenticateDocumentImage(fileBytes, AUTHENTICID_URL, AccessKey, SecretToken, DocumentType, AutoCrop, submitterName, getImages, Orientation, senderIP, JobID);

            return result;
        }

        #region "Private Methods"

        private byte[] GetKTADocumentFile(string docID, string ktaSDKUrl, string sessionID)
        {
            byte[] result = new byte[1];
            byte[] buffer = new byte[4096];

            try
            {
                //Setting the URi and calling the get document API
                var KTAGetDocumentFile = ktaSDKUrl + "/CaptureDocumentService.svc/json/GetDocumentFile2";
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(KTAGetDocumentFile);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                // CONSTRUCT JSON Payload
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"sessionId\":\"" + sessionID + "\",\"reportingData\": {\"Station\": \"\", \"MarkCompleted\": false }, \"documentId\":\"" + docID + "\", \"documentFileOptions\": { \"FileType\": \"\", \"IncludeAnnotations\": 0 } }";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                }

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream receiveStream = httpWebResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader readStream = new StreamReader(receiveStream, encode);
                int streamContentLength = unchecked((int)httpWebResponse.ContentLength);

                using (Stream responseStream = httpWebResponse.GetResponseStream())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = responseStream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);
                        } while (count != 0);

                        result = memoryStream.ToArray();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception GetKTADocumentFile: " + ex.ToString(), ex);
            }
        }

        private string AuthenticateDocumentImage(byte[] fileBytes, string apiURL, string accessKey, string secretToken, string documentType, bool autoCrop, string submitterName, bool getImages, string orientation, string senderIP, string jobID)
        {
            try
            {
                //Setting the API Endpoint to Authenticate the Document
                var apiEndpoint = apiURL + "/AuthenticateDocument";

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    throw new Exception("Invalid Document. Provide valid file data.");
                }

                string boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x"); // Unique boundary

                using (var client = new HttpClient())
                {
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sv[ATT_ACCESS_TOKEN].Value);
                    client.DefaultRequestHeaders.Add("SecretToken", secretToken);
                    client.DefaultRequestHeaders.Add("AccountAccessKey", accessKey);
                    client.DefaultRequestHeaders.Add("RequestIdentifier", "1");
                    client.DefaultRequestHeaders.Add("DeviceType", "");
                    client.DefaultRequestHeaders.Add("DocumentType", documentType);
                    client.DefaultRequestHeaders.Add("DocumentID", "");
                    client.DefaultRequestHeaders.Add("DeviceDetails", "");
                    client.DefaultRequestHeaders.Add("AutoCrop", autoCrop.ToString());
                    client.DefaultRequestHeaders.Add("getImages", "False");
                    client.DefaultRequestHeaders.Add("senderIPAddress", senderIP);
                    client.DefaultRequestHeaders.Add("Orientation", orientation);
                    client.DefaultRequestHeaders.Add("UID", jobID);
                    using (var content = new MultipartFormDataContent())
                    {
                        var imageContent = new ByteArrayContent(fileBytes);
                        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpg");
                        content.Add(imageContent, "Front", "IDDoc.jpg");

                        // Send the request and await the response
                        var response = client.PostAsync(apiEndpoint, content).GetAwaiter().GetResult();

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new WebException($"Error analyzing ID: {response.StatusCode}", WebExceptionStatus.ProtocolError);
                        }

                        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception AuthenticateDocumentImage: " + ex.ToString(), ex);
            }
        }

        #endregion "Private Methods"

        public class AuthClass
        {
            public string country;
            public string state;
            public string type;
            public string year;
            public string issuerCountry;
            public string issuerRegion;
            public string issuerContintent;
            public string issuerCode;
            public string documentClassName;
        }
    }
}