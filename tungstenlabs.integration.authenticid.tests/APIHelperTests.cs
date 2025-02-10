using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tungstenlabs.integration.authenticid.tests
{
    [TestClass]
    public class APIHelperTests
    {
        private APIHelper aid = new APIHelper();

        [TestMethod]
        public void AuthenticateDocument()
        {
            string documentID = @"6c6cb780-8ad4-48b3-bf9a-b27e000f1195";
            string result = aid.AuthenticateDocument(Constants.TOTALAGILITY_SESSION_ID,
                documentID,
                Constants.TOTALAGILITY_API_URL,
                Constants.AUTHENTICID_API_URL,
                Constants.AUTHENTICID_ACCESSKEY,
                Constants.AUTHENTICID_SECRETTOKEN,
                "License", false, "TotalAgility", false, "Portrait", "TotalAgility", "JobID");
        }
    }
}