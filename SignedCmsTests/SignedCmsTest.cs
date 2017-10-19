using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace SignedCmsTests
{
    public class SignedCmsTest
    {
        [ConfigFact]
        public void SignedCmsRoundTripWithLocalCertificate()
        {
            var content = "This is some content";
            
            // Get cert

            var signedCms = new SignedCms(new ContentInfo(Encoding.UTF8.GetBytes(content)));

            var cert = GetLocalSignerCert();

            Assert.True(cert.HasPrivateKey);
            var signer = new CmsSigner(cert);

            signedCms.ComputeSignature(signer, true);


            var signature = signedCms.Encode();


            // verify
            var rCms = new SignedCms();
            rCms.Decode(signature);


            // This will throw if invalid
            rCms.CheckSignature(true); // don't validate the certiciate itself here

            var cContent = rCms.ContentInfo.Content;
            var str = Encoding.UTF8.GetString(cContent);

            Assert.Equal(content, str);
        }

        static public X509Certificate2 GetLocalSignerCert()
        {
            //  Open the My certificate store.
            X509Store storeMy = new X509Store(StoreName.My,
                                              StoreLocation.CurrentUser);
            storeMy.Open(OpenFlags.ReadOnly);
            

            //  Find the signer's certificate.
            var certColl = storeMy.Certificates.Find(X509FindType.FindByThumbprint, Config.Values.Thumbprint, false);
            

            //  Check to see if the certificate suggested by the example
            //  requirements is not present.
            if (certColl.Count == 0)
            {
                throw new ArgumentException("Certificate not found");
            }

            storeMy.Close();

            //  If more than one matching cert, return the first one.
            return certColl[0];
        }

    }
}
