﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ComcastUsageMeter.Shared.Responses;
using KeyValue = System.Collections.Generic.KeyValuePair<System.String, System.String>;

namespace ComcastUsageMeter.Shared
{
    public sealed class UsageMeterClient
    {
        private static readonly string UrlAuthentication = "http://umcs.comcast.net/usage_meter/login/uid?callback=?";
        private static readonly string UrlUsageCurrent = "http://umcs.comcast.net/usage_meter/usage/current";

        // TODO: DisplayWiFiUsage
        // https://umcs.comcast.net/usage_meter/usage/displayWiFiUsage
        // username, access_token, version

        // TODO: GetUsageHistory
        // http://umcs.comcast.net/usage_meter/usage/history
        // username, access_token, version

        private readonly String _username;
        private readonly String _accessToken;
        private readonly String _version;

        public UsageMeterClient(String username, String accessToken, String version)
        {
            _username = username;
            _accessToken = accessToken;
            _version = version;
        }

        public async Task<UsageResponse> GetUsageCurrentAsync()
        {
            var client = new HttpClient();

            List<KeyValue> postData = new List<KeyValue>
            {
                new KeyValue("username", _username),
                new KeyValue("access_token", _accessToken),
                new KeyValue("version", _version)
            };

            var postContent = new FormUrlEncodedContent(postData);
            var response = await client.PostAsync(UrlUsageCurrent, postContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            return Deserialize<UsageResponse>(responseContent);
        }

        #region Static

        public static async Task<AuthenticationResponse> AuthenticateAsync(String username, String password, String version)
        {  
            var client = new HttpClient();

            List<KeyValue> postData = new List<KeyValue>
            {
                new KeyValue("username", username),
                new KeyValue("password", password),
                new KeyValue("version", version)
            };

            var postContent = new FormUrlEncodedContent(postData);
            var response = await client.PostAsync(UrlAuthentication, postContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            return Deserialize<AuthenticationResponse>(responseContent);
        }

        private static T Deserialize<T>(String serializedData) where T : class
        {
            T deserialized;
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader stringReader = new StringReader(serializedData))
            {
                deserialized = serializer.Deserialize(stringReader) as T;
            }

            return deserialized;
        }

        #endregion
    }
}