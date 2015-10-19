﻿using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using JWT;

namespace OpenIDClient.Messages
{
    /// <summary>
    /// Abstract class extended by all messages between RP e OP.
    /// </summary>
    public class OIDClientSerializableMessage
    {
        /// <summary>
        /// Method that returns true or false if the type passed is one of the types supported
        /// in serialization and deserialization.
        /// </summary>
        /// <param name="t">The type to be checked to verify serializability</param>
        /// <returns>True or false, true if the type can be serialized</returns>
        public static bool IsSupportedType(Type t)
        {
            List<Type> supportedTypes = new List<Type>() {
                typeof(string),
                typeof(List<>),
                typeof(Dictionary<, >),
                typeof(DateTime),
                typeof(long),
                typeof(int),
                typeof(bool),
                typeof(OIDCKey),
                typeof(OIDClaims),
                typeof(OIDClaimData)
            };

            if (t.IsGenericType)
            {
                return supportedTypes.Contains(t.GetGenericTypeDefinition());
            }
            else
            {
                return supportedTypes.Contains(t);
            }
        }

        /// <summary>
        /// Method used to validate the message according to the rules specified in the
        /// protocol specification.
        /// </summary>
        public virtual void Validate()
        {
            // Empty, method that can be overloaded by children to check if deserialized data is correct
            // or throw an exception if not.
        }

        /// <summary>
        /// Method that deserializes message property values from a dynamic object as input.
        /// </summary>
        /// <param name="data">Dictionary object with the property values for the current message.</param>
        public void DeserializeFromDictionary(Dictionary<string, object> data)
        {
            Deserializer.DeserializeFromDictionary(this, data);
            Validate();
        }

        /// <summary>
        /// Method that deserializes message property values from a string obtained from query string.
        /// </summary>
        /// <param name="query">The query string.</param>
        public void DeserializeFromQueryString(string query)
        {
            Deserializer.DeserializeFromQueryString(this, query);
            Validate();
        }

        /// <summary>
        /// Method that serializes message property values to a Dictionary object.
        /// </summary>
        /// <returns>A dictionary serialization of the message.</returns>
        public Dictionary<string, object> SerializeToDictionary()
        {
            return Serializer.SerializeToDictionary(this);
        }

        /// <summary>
        /// Method that serializes message property values to a JSON string.
        /// </summary>
        /// <returns>A JSON string serialization of the message.</returns>
        public string SerializeToJsonString()
        {
            return Serializer.SerializeToJsonString(this);
        }

        /// <summary>
        /// Method that serializes message property values to a query string.
        /// </summary>
        /// <returns>A query string serialization of the message.</returns>
        public string SerializeToQueryString()
        {
            return Serializer.SerializeToQueryString(this);
        }
    }

    /// <summary>
    /// Message describing a registration request.
    /// </summary>
    public class OIDCClientRegistrationRequest : OIDClientSerializableMessage
    {
        private WebRequest PostRequest { get; set; }

        public string ApplicationType { get; set; }
        public List<string> RedirectUris { get; set; }
        public string ClientName { get; set; }
        public string LogoUri { get; set; }
        public string SubjectType { get; set; }
        public List<string> SectorIdentifierUri { get; set; }
        public string TokenEndpointAuthMethod { get; set; }
        public string JwksUri { get; set; }
        public string UserInfoEncryptedResponseAlg { get; set; }
        public string UserInfoEncryptedResponseEnc { get; set; }
        public List<string> Contacts { get; set; }
        public List<string> RequestUris { get; set; }
        public List<string> ResponseTypes { get; set; }
    }

    /// <summary>
    /// Message describing an authorization request.
    /// </summary>
    public class OIDCAuthorizationRequestMessage : OIDClientSerializableMessage
    {
        public string Scope { get; set; }
        public string ResponseType { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string State { get; set; }
        public string ResponseMode { get; set; }
        public string Nonce { get; set; }
        public string Display { get; set; }
        public string Page { get; set; }
        public string Popup { get; set; }
        public string Touch { get; set; }
        public string Wap { get; set; }
        public string Prompt { get; set; }
        public string None { get; set; }
        public string Login { get; set; }
        public string Consent { get; set; }
        public int MaxAge { get; set; }
        public string UiLocales { get; set; }
        public string IdTokenHint { get; set; }
        public string LoginHint { get; set; }
        public string AcrValues { get; set; }
        public OIDClaims Claims { get; set; }

        /// <summary>
        /// <see cref="OIDClientSerializableMessage.Validate()"/>
        /// </summary>
        public override void Validate()
        {
            if (Scope == null)
            {
                throw new OIDCException("Mising scope required parameter.");
            }

            if (ResponseType == null)
            {
                throw new OIDCException("Mising response_type required parameter.");
            }

            if (ClientId == null)
            {
                throw new OIDCException("Mising client_id required parameter.");
            }

            if (RedirectUri == null)
            {
                throw new OIDCException("Mising redirect_uri required parameter.");
            }
        }
    }

    /// <summary>
    /// Message describing an authentication code response.
    /// </summary>
    public class OIDCAuthCodeResponseMessage : OIDClientSerializableMessage
    {
        public string Code { get; set; }
        public string State { get; set; }
        public string Scope { get; set; }

        /// <summary>
        /// <see cref="OIDClientSerializableMessage.Validate()"/>
        /// </summary>
        public override void Validate()
        {
            if (Code == null)
            {
                throw new OIDCException("Mising code required parameter.");
            }
        }
    }

    /// <summary>
    /// Message describing an authentication implicit response.
    /// </summary>
    public class OIDCAuthImplicitResponseMessage : OIDClientSerializableMessage
    {
        public string AccessToken { get; set; }
        public long ExpiresIn { get; set; }
        public string TokenType { get; set; }
        public string IdToken { get; set; }
        public string Scope { get; set; }
        public string State { get; set; }

        /// <summary>
        /// <see cref="OIDClientSerializableMessage.Validate()"/>
        /// </summary>
        public override void Validate()
        {
            if (IdToken == null)
            {
                throw new OIDCException("Mising id_token required parameter.");
            }

            if (State == null)
            {
                throw new OIDCException("Mising state required parameter.");
            }
        }
    }

    /// <summary>
    /// Abstract class describing a client authenticated message.
    /// </summary>
    public class OIDCAuthenticatedMessage : OIDClientSerializableMessage
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientAssertionType { get; set; }
        public string ClientAssertion { get; set; }
    }

    /// <summary>
    /// Message describing a token request.
    /// </summary>
    public class OIDCTokenRequestMessage : OIDCAuthenticatedMessage
    {
        public string GrantType { get; set; }
        public string Code { get; set; }
        public string RedirectUri { get; set; }
        public string State { get; set; }
        public string Scope { get; set; }
    }

    /// <summary>
    /// Message describing an the client secret JWT.
    /// </summary>
    public class OIDCClientSecretJWT : OIDClientSerializableMessage
    {
        public string Iss { get; set; }
        public string Sub { get; set; }
        public string Aud { get; set; }
        public string Jti { get; set; }
        public DateTime Exp { get; set; }
        public DateTime Iat { get; set; }
    }

    /// <summary>
    /// Message describing a token response.
    /// </summary>
    public class OIDCTokenResponseMessage : OIDClientSerializableMessage
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string RefreshToken { get; set; }
        public long ExpiresIn { get; set; }
        public string IdToken { get; set; }

        /// <summary>
        /// <see cref="OIDClientSerializableMessage.Validate()"/>
        /// </summary>
        public override void Validate()
        {
            if (AccessToken == null)
            {
                throw new OIDCException("Mising access_token required parameter.");
            }

            if (TokenType == null)
            {
                throw new OIDCException("Mising token_type required parameter.");
            }
        }
    }

    /// <summary>
    /// Message describing a user info request.
    /// </summary>
    public class OIDCUserInfoRequestMessage : OIDCAuthenticatedMessage
    {
        public string Scope { get; set; }
        public string State { get; set; }
        public OIDClaims Claims { get; set; }
    }

    /// <summary>
    /// Message describing a user info response.
    /// </summary>
    public class OIDCUserInfoResponseMessage : OIDClientSerializableMessage
    {
        public string Sub { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string MiddleName { get; set; }
        public string Nickname { get; set; }
        public string PreferredUsername { get; set; }
        public string Profile { get; set; }
        public string Picture { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string EmailVerified { get; set; }
        public string Gender { get; set; }
        public string Birthdate { get; set; }
        public string Zoneinfo { get; set; }
        public string Locale { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberVerified { get; set; }
        public string Address { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Message describing an ID token.
    /// </summary>
    public class OIDCIdToken : OIDClientSerializableMessage
    {
        public string Iss { get; set; }
        public string Sub { get; set; }
        public List<string> Aud { get; set; }
        public DateTime Exp { get; set; }
        public DateTime Iat { get; set; }
        public DateTime AuthTime { get; set; }
        public string Nonce { get; set; }
        public string Acr { get; set; }
        public List<string> Amr { get; set; }
        public string Azp { get; set; }
        public string AtHash { get; set; }
        public OIDCKey SubJkw { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string MiddleName { get; set; }
        public string Nickname { get; set; }
        public string PreferredUsername { get; set; }
        public string Profile { get; set; }
        public string Picture { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string EmailVerified { get; set; }
        public string Gender { get; set; }
        public string Birthdate { get; set; }
        public string Zoneinfo { get; set; }
        public string Locale { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberVerified { get; set; }
        public string Address { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// <see cref="OIDClientSerializableMessage.Validate()"/>
        /// </summary>
        public override void Validate()
        {   
            if (Iss == null)
            {
                throw new OIDCException("Mising iss required parameter.");
            }

            if (Iss == "https://self-issued.me")
            {
                ValidateSelfIssued();
            }
            else
            {
                ValidateGeneric();
            }
        }

        private void ValidateSelfIssued()
        {
            if (SubJkw != null && SubJkw.N != null)
            {
                byte[] key = Convert.FromBase64String(SubJkw.N);
                X509Certificate2 certificate = new X509Certificate2(key);

                if (Sub != Convert.ToBase64String(Encoding.UTF8.GetBytes(certificate.Thumbprint)))
                {
                    throw new OIDCException("Wrong signature for subject.");
                }
            }
        }

        private void ValidateGeneric()
        {
            if (Sub == null)
            {
                throw new OIDCException("Mising sub required parameter.");
            }

            if (Aud == null)
            {
                throw new OIDCException("Mising aud required parameter.");
            }

            if (Exp == null)
            {
                throw new OIDCException("Mising exp required parameter.");
            }

            if (Iat == null)
            {
                throw new OIDCException("Mising iat required parameter.");
            }
        }
    }

    /// <summary>
    /// Message describing claims for a request
    /// </summary>
    public class OIDClaimData : OIDClientSerializableMessage
    {
        public bool Essential { get; set; }
        public string Value { get; set; }
        public List<string> Values { get; set; }
    }

    /// <summary>
    /// Message describing claims for a request
    /// </summary>
    public class OIDClaims : OIDClientSerializableMessage
    {
        public Dictionary<string, OIDClaimData> Userinfo { get; set; }
        public Dictionary<string, OIDClaimData> IdToken { get; set; }
    }

    /// <summary>
    /// Message describing an error from the OP.
    /// </summary>
    public class OIDCResponseError : OIDClientSerializableMessage
    {
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorUri { get; set; }
        public string State { get; set; }
    }
}
