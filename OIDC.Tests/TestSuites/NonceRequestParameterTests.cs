﻿namespace OIDC.Tests
{
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Collections.Generic;
    using NUnit.Framework;
    using OpenIDClient;
    using OpenIDClient.Messages;

    [TestFixture]
    public class NonceRequestParameterTests : OIDCTests
    {
        [TestFixtureSetUp]
        public void SetupTests()
        {
            StartWebServer();
            RegisterClient(ResponseType.IdToken);
            GetProviderMetadata();
        }

        /// <summary>
        /// Sends 'nonce' unless using code flow
        /// 
        /// Description:	
        /// Always send a 'nonce' value as a request parameter while using implicit or hybrid flow.
        /// Verify the 'nonce' value returned in the ID Token.
        /// Expected result:	
        /// An ID Token, either from the Authorization Endpoint or from the Token Endpoint, containing the
        /// same 'nonce' value as passed in the authentication request when using hybrid flow or
        /// implicit flow.
        /// </summary>
        [TestCase]
        [Category("NonceRequestParameterTests")]
        public void Should_Nonce_Be_Present_In_Implicit()
        {
            rpid = "rp-nonce-unless_code_flow";

            // given
            OIDCAuthorizationRequestMessage requestMessage = new OIDCAuthorizationRequestMessage();
            requestMessage.ClientId = clientInformation.ClientId;

            OIDClaims requestClaims = new OIDClaims();
            requestClaims.Userinfo = new Dictionary<string, OIDClaimData>();
            requestClaims.Userinfo.Add("name", new OIDClaimData());

            requestMessage.Scope = new List<MessageScope>() { MessageScope.Openid };
            requestMessage.ResponseType = new List<ResponseType>() { ResponseType.IdToken, ResponseType.Token };
            requestMessage.RedirectUri = clientInformation.RedirectUris[0];
            requestMessage.Nonce = WebOperations.RandomString();
            requestMessage.State = WebOperations.RandomString();
            requestMessage.Claims = requestClaims;
            requestMessage.Validate();

            OpenIdRelyingParty rp = new OpenIdRelyingParty();

            rp.Authenticate(GetBaseUrl("/authorization"), requestMessage);
            semaphore.WaitOne();
            OIDCAuthImplicitResponseMessage response = rp.ParseAuthImplicitResponse(result, requestMessage.Scope, requestMessage.State);
            OIDCIdToken idToken = response.GetIdToken(providerMetadata.Keys, clientInformation.ClientSecret);

            // then
            idToken.Validate();
        }

        /// <summary>
        /// Sends 'nonce' unless using code flow
        /// 
        /// Description:	
        /// Always send a 'nonce' value as a request parameter while using implicit or hybrid flow.
        /// Verify the 'nonce' value returned in the ID Token.
        /// Expected result:	
        /// An ID Token, either from the Authorization Endpoint or from the Token Endpoint, containing the
        /// same 'nonce' value as passed in the authentication request when using hybrid flow or
        /// implicit flow.
        /// </summary>
        [TestCase]
        [Category("NonceRequestParameterTests")]
        public void Should_Nonce_Be_Present_In_Self_Issued()
        {
            rpid = "rp-nonce-unless_code_flow";
            WebRequest.RegisterPrefix("openid", new OIDCWebRequestCreate());

            // given
            OIDCAuthorizationRequestMessage requestMessage = new OIDCAuthorizationRequestMessage();
            requestMessage.ClientId = clientInformation.RedirectUris[0];
            requestMessage.Scope = new List<MessageScope>() { MessageScope.Openid, MessageScope.Profile, MessageScope.Email, MessageScope.Address, MessageScope.Phone };
            requestMessage.State = WebOperations.RandomString();
            requestMessage.Nonce = WebOperations.RandomString();
            requestMessage.ResponseType = new List<ResponseType>() { ResponseType.IdToken };
            requestMessage.RedirectUri = clientInformation.RedirectUris[0];
            requestMessage.Validate();

            X509Certificate2 certificate = new X509Certificate2("server.pfx", "", X509KeyStorageFlags.Exportable);
            OpenIdRelyingParty rp = new OpenIdRelyingParty();

            // when
            OIDCAuthImplicitResponseMessage response = rp.Authenticate("openid://", requestMessage, certificate);
            OIDCIdToken idToken = response.GetIdToken();

            // then
            response.Validate();
        }

        /// <summary>
        /// Rejects ID Token with invalid 'nonce' when valid 'nonce' sent
        /// 
        /// Description:	
        /// Pass a 'nonce' value in the Authentication Request. Verify the 'nonce' value returned in the ID Token.
        /// Expected result:
        /// Identity that the 'nonce' value in the ID Token is invalid and reject the ID Token.
        /// </summary>
        [TestCase]
        [Category("NonceRequestParameterTests")]
        [ExpectedException(typeof(OIDCException), ExpectedMessage = "Wrong nonce value in token.")]
        public void Should_Reject_Id_Token_With_Wrong_Nonce()
        {
            rpid = "rp-nonce-invalid";

            // given
            OIDCAuthorizationRequestMessage requestMessage = new OIDCAuthorizationRequestMessage();
            requestMessage.ClientId = clientInformation.ClientId;

            OIDClaims requestClaims = new OIDClaims();
            requestClaims.Userinfo = new Dictionary<string, OIDClaimData>();
            requestClaims.Userinfo.Add("name", new OIDClaimData());

            requestMessage.Scope = new List<MessageScope>() { MessageScope.Openid };
            requestMessage.ResponseType = new List<ResponseType>() { ResponseType.IdToken, ResponseType.Token };
            requestMessage.RedirectUri = clientInformation.RedirectUris[0];
            requestMessage.Nonce = WebOperations.RandomString();
            requestMessage.State = WebOperations.RandomString();
            requestMessage.Claims = requestClaims;
            requestMessage.Validate();

            OpenIdRelyingParty rp = new OpenIdRelyingParty();

            rp.Authenticate(GetBaseUrl("/authorization"), requestMessage);
            semaphore.WaitOne();
            OIDCAuthImplicitResponseMessage response = rp.ParseAuthImplicitResponse(result, requestMessage.Scope, requestMessage.State);
            OIDCIdToken idToken = response.GetIdToken(providerMetadata.Keys, clientInformation.ClientSecret);

            // then
            rp.ValidateIdToken(idToken, clientInformation, idToken.Iss, "wrong-nonce");
        }
    }
}