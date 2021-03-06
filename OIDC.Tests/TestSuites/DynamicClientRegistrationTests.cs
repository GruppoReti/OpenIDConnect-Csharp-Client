﻿namespace OIDC.Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using OpenIDClient;

    [TestFixture]
    public class DynamicClientRegistrationTests : OIDCTests
    {
        [TestFixtureSetUp]
        public void SetupTests()
        {
            StartWebServer();
        }

        /// <summary>
        /// Uses dynamic registration
        /// 
        /// Description:	
        /// Use the client registration endpoint in order to dynamically register the Relying Party.
        /// Expected result:	
        /// Get a Client Registration Response.
        /// </summary>
        [TestCase]
        [Category("DynamicClientRegistrationTests")]
        public void Should_Client_Be_Able_To_Register()
        {
            rpid = "rp-registration-dynamic";

            // given
            string registrationEndopoint = GetBaseUrl("/registration");
            OIDCClientInformation clientMetadata = new OIDCClientInformation();
            clientMetadata.ApplicationType = "web";
            clientMetadata.RedirectUris = new List<string>() { myBaseUrl + "code_flow_callback" };
            clientMetadata.ResponseTypes = new List<ResponseType>() { ResponseType.Code };
            OpenIdRelyingParty rp = new OpenIdRelyingParty();

            // when
            OIDCClientInformation response = rp.RegisterClient(registrationEndopoint, clientMetadata);

            // then
            response.Validate();
        }

        /// <summary>
        /// Registration request has redirect_uris
        /// 
        /// Description:	
        /// Set the redirect_uris parameter of the Client Metadata in a registration request.
        /// Expected result:	
        /// Get a Client Registration Response.
        /// </summary>
        [TestCase]
        [Category("DynamicClientRegistrationTests")]
        public void Should_Registration_Request_Has_RedirectUris()
        {
            rpid = "rp-registration-redirect_uris";

            // given
            string registrationEndopoint = GetBaseUrl("/registration");
            OIDCClientInformation clientMetadata = new OIDCClientInformation();
            clientMetadata.ApplicationType = "web";
            clientMetadata.RedirectUris = new List<string>() { myBaseUrl + "code_flow_callback" };
            clientMetadata.ResponseTypes = new List<ResponseType>() { ResponseType.Code };
            OpenIdRelyingParty rp = new OpenIdRelyingParty();

            // when
            OIDCClientInformation response = rp.RegisterClient(registrationEndopoint, clientMetadata);

            // then
            response.Validate();
            CollectionAssert.AreEquivalent(clientMetadata.RedirectUris, response.RedirectUris);
        }

        /// <summary>
        /// Keys are published as a well-formed JWK Set
        /// 
        /// Description:	
        /// The keys published by the Relying Party should follow the JSON Web Key Set (JWK Set) Format.
        /// Expected result:	
        /// Get a Client Registration Response.
        /// </summary>
        [TestCase]
        [Category("DynamicClientRegistrationTests")]
        public void Should_Keys_Be_Published_As_JWK()
        {
            rpid = "rp-registration-well_formed_jwk";

            // given
            string registrationEndopoint = GetBaseUrl("/registration");
            OIDCClientInformation clientMetadata = new OIDCClientInformation();
            clientMetadata.ApplicationType = "web";
            clientMetadata.RedirectUris = new List<string>() { myBaseUrl + "code_flow_callback" };
            clientMetadata.ResponseTypes = new List<ResponseType>() { ResponseType.Code };
            clientMetadata.JwksUri = myBaseUrl + "my_public_keys.jwks";
            OpenIdRelyingParty rp = new OpenIdRelyingParty();

            // when
            OIDCClientInformation response = rp.RegisterClient(registrationEndopoint, clientMetadata);

            // then
            response.Validate();
        }

        /// <summary>
        /// Uses HTTPS for all endpoints
        /// 
        /// Description:	
        /// Only register URLs using the https scheme for all endpoints in the Client Metadata.
        /// Expected result:	
        /// No endpoints not supporting HTTPS.
        /// </summary>
        [TestCase]
        [Category("DynamicClientRegistrationTests")]
        [ExpectedException(typeof(OIDCException), ExpectedMessage = "Some of the URIs for the client is not on https")]
        public void Should_Client_Only_Use_Https_Endpoints()
        {
            rpid = "rp-registration-uses_https_endpoints";

            // given
            string registrationEndopoint = GetBaseUrl("/registration");
            OIDCClientInformation clientMetadata = new OIDCClientInformation();
            clientMetadata.ApplicationType = "web";
            clientMetadata.RedirectUris = new List<string>() { myBaseUrl + "code_flow_callback" };
            clientMetadata.ResponseTypes = new List<ResponseType>() { ResponseType.Code };
            clientMetadata.JwksUri = myBaseUrl + "my_public_keys.jwks";
            OpenIdRelyingParty rp = new OpenIdRelyingParty();

            // when
            OIDCClientInformation response = rp.RegisterClient(registrationEndopoint, clientMetadata);
            response.JwksUri = clientMetadata.JwksUri.Replace("https", "http");

            // then
            response.Validate();
        }
    }
}