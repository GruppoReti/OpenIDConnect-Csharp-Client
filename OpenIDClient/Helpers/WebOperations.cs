﻿namespace OpenIDClient
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.Net.Security;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using OpenIDClient.Messages;

    public static class WebOperations
    {
        private static RemoteCertificateValidationCallback oldValiadtionCallback;
        public static bool checkCertificate = true;
        private static CookieContainer cookies = new CookieContainer();

        static WebOperations()
        {
            oldValiadtionCallback = ServicePointManager.ServerCertificateValidationCallback;
            ServicePointManager.ServerCertificateValidationCallback = ValidateSSL;
        }

        /// <summary>
        /// Method generating a random string with numbers or letters.
        /// </summary>
        /// <param name="length">The length of the string to generate.</param>
        /// <returns>The random string generated.</returns>
        public static string RandomString(int length = 16)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random(new System.DateTime().Millisecond);
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Method that performs an HTTP GET and returns the Json deserialization
        /// of the content returned from the call.
        /// </summary>
        /// <param name="webRequest">The WebRequest object to be used for the call.</param>
        /// <param name="returnJson">(optional) Specify whether the output page should be parsed as a JSON.
        /// In case this should not happen, a dictionary with a single "body" key will be returned with the
        /// complete HTML text returned from HTTP call.</param>
        /// <returns>Json deserialization of the content returned from the call.</returns>
        public static Dictionary<string, object> GetUrlContent(WebRequest webRequest, bool returnJson = true, bool checkCertificate = true)
        {
            ((HttpWebRequest)webRequest).CookieContainer = cookies;
            HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
            Stream content = resp.GetResponseStream();

            string returnedText = new StreamReader(content).ReadToEnd();
            if (returnJson)
            {
                return Deserializer.DeserializeFromJson<Dictionary<string, object>>(returnedText);
            }
            else
            {
                Dictionary<string, object> response = new Dictionary<string, object>();
                response.Add("body", returnedText);
                return response;
            }
        }

        /// <summary>
        /// Method that performs an HTTP POST and returns the Json deserialization
        /// of the content returned from the call.
        /// </summary>
        /// <param name="webRequest">The WebRequest object to be used for the call.</param>
        /// <param name="message">The message to be passed as content of the call.</param>
        /// <param name="json">A flag indicating whether the message has a Json format or not.
        /// In the first case the message is posted as a serialization of the Json.
        /// In the second case the messge is serialized to query string.</param>
        /// <returns>Json deserialization of the content returned from the call.</returns>
        public static string PostUrlContent(WebRequest webRequest, OIDClientSerializableMessage message, bool json = false)
        {
            ((HttpWebRequest)webRequest).CookieContainer = cookies;
            webRequest.Method = "POST";

            string postData = "";
            if (message != null)
            {
                if (json)
                {
                    postData = message.SerializeToJsonString();
                }
                else
                {
                    postData = message.SerializeToQueryString();
                }
            }
            byte[] postBytes = Encoding.UTF8.GetBytes(postData);

            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = postBytes.Length;

            Stream postStream = webRequest.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Close();

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

            StreamReader rdr = new StreamReader(response.GetResponseStream());
            return rdr.ReadToEnd();
        }

        private static bool ValidateSSL(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return !checkCertificate || oldValiadtionCallback(sender, certificate, chain, errors);
        }
    }
}
