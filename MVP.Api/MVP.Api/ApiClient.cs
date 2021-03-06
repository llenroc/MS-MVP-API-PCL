﻿namespace MVP.Api
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using MVP.Api.Models.MicrosoftAccount;

    using WinUX.Data.Serialization;
    using WinUX.Networking.Requests.Json;

    /// <summary>
    /// Defines a mechanism to call into the MVP API from a client application.
    /// </summary>
    public partial class ApiClient
    {
        private const string BaseApiUri = "https://mvpapi.azure-api.net/mvp/api";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClient"/> class.
        /// </summary>
        /// <param name="clientId">
        /// The Microsoft application client ID.
        /// </param>
        /// <param name="clientSecret">
        /// The Microsoft application client secret.
        /// </param>
        /// <param name="subscriptionKey">
        /// The MVP API subscription key.
        /// </param>
        public ApiClient(string clientId, string clientSecret, string subscriptionKey, bool isLiveSdkApp = false)
        {
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.SubscriptionKey = subscriptionKey;
            this.IsLiveSdkApp = isLiveSdkApp;
        }

        /// <summary>
        /// Gets a value indicating whether the client ID and secret are from an older Live SDK application.
        /// </summary>
        /// <remarks>
        /// If you're using a newer 'Converged application', this should be false.
        /// If you're using an older 'Live SDK application', this should be true.
        /// Find your apps here: https://apps.dev.microsoft.com/?mkt=en-us#/appList
        /// </remarks>
        public bool IsLiveSdkApp { get; }

        /// <summary>
        /// Gets the Microsoft application client ID.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Gets the Microsoft application client secret.
        /// </summary>
        public string ClientSecret { get; }

        /// <summary>
        /// Gets the MVP API subscription key.
        /// </summary>
        public string SubscriptionKey { get; }

        /// <summary>
        /// Gets or sets the credentials associated with the logged in Microsoft account.
        /// </summary>
        public MSACredentials Credentials { get; set; }

        private async Task<TResponse> GetAsync<TResponse>(
            string endpoint,
            bool useCredentials = true,
            string overrideUri = null,
            CancellationTokenSource cts = null)
        {
            var uri = string.IsNullOrWhiteSpace(overrideUri) ? $"{BaseApiUri}/{endpoint}" : overrideUri;

            var getRequest = useCredentials
                                 ? new JsonGetNetworkRequest(new HttpClient(), uri, this.GetRequestHeaders())
                                 : new JsonGetNetworkRequest(new HttpClient(), uri);

            var retryCall = false;

            try
            {
                return await getRequest.ExecuteAsync<TResponse>(cts);
            }
            catch (HttpRequestException hre) when (hre.Message.Contains("401"))
            {
                var tokenRefreshed = await this.ExchangeRefreshTokenAsync();
                if (tokenRefreshed != null)
                {
                    retryCall = true;
                }
            }

            if (retryCall)
            {
                getRequest = useCredentials
                                 ? new JsonGetNetworkRequest(new HttpClient(), uri, this.GetRequestHeaders())
                                 : new JsonGetNetworkRequest(new HttpClient(), uri);
                return await getRequest.ExecuteAsync<TResponse>(cts);
            }

            return default(TResponse);
        }

        private async Task<TResponse> PostAsync<TResponse>(
            string endpoint,
            object data,
            bool useCredentials = true,
            string overrideUri = null,
            CancellationTokenSource cts = null)
        {
            var uri = string.IsNullOrWhiteSpace(overrideUri) ? $"{BaseApiUri}/{endpoint}" : overrideUri;

            var json = SerializationService.Json.Serialize(data);

            var postRequest = useCredentials
                                  ? new JsonPostNetworkRequest(new HttpClient(), uri, json, this.GetRequestHeaders())
                                  : new JsonPostNetworkRequest(new HttpClient(), uri, json);

            var retryCall = false;

            try
            {
                return await postRequest.ExecuteAsync<TResponse>(cts);
            }
            catch (HttpRequestException hre) when (hre.Message.Contains("401"))
            {
                var tokenRefreshed = await this.ExchangeRefreshTokenAsync();
                if (tokenRefreshed != null)
                {
                    retryCall = true;
                }
            }

            if (retryCall)
            {
                postRequest = useCredentials
                                  ? new JsonPostNetworkRequest(new HttpClient(), uri, json, this.GetRequestHeaders())
                                  : new JsonPostNetworkRequest(new HttpClient(), uri, json);
                return await postRequest.ExecuteAsync<TResponse>(cts);
            }

            return default(TResponse);
        }

        private async Task<bool> PutAsync(
            string endpoint,
            object data,
            bool useCredentials = true,
            string overrideUri = null,
            CancellationTokenSource cts = null)
        {
            var uri = string.IsNullOrWhiteSpace(overrideUri) ? $"{BaseApiUri}/{endpoint}" : overrideUri;

            var json = SerializationService.Json.Serialize(data);

            var putRequest = useCredentials
                                 ? new JsonPutNetworkRequest(new HttpClient(), uri, json, this.GetRequestHeaders())
                                 : new JsonPutNetworkRequest(new HttpClient(), uri, json);

            var retryCall = false;

            try
            {
                await putRequest.ExecuteAsync<bool>(cts);
                return true;
            }
            catch (HttpRequestException hre) when (hre.Message.Contains("401"))
            {
                var tokenRefreshed = await this.ExchangeRefreshTokenAsync();
                if (tokenRefreshed != null)
                {
                    retryCall = true;
                }
            }

            if (retryCall)
            {
                putRequest = useCredentials
                                 ? new JsonPutNetworkRequest(new HttpClient(), uri, json, this.GetRequestHeaders())
                                 : new JsonPutNetworkRequest(new HttpClient(), uri, json);
                await putRequest.ExecuteAsync<bool>(cts);
                return true;
            }

            return false;
        }

        private async Task<bool> DeleteAsync(
            string endpoint,
            bool useCredentials = true,
            string overrideUri = null,
            CancellationTokenSource cts = null)
        {
            var uri = string.IsNullOrWhiteSpace(overrideUri) ? $"{BaseApiUri}/{endpoint}" : overrideUri;

            var deleteRequest = useCredentials
                                    ? new JsonDeleteNetworkRequest(new HttpClient(), uri, this.GetRequestHeaders())
                                    : new JsonDeleteNetworkRequest(new HttpClient(), uri);

            var retryCall = false;

            try
            {
                await deleteRequest.ExecuteAsync<bool>(cts);
                return true;
            }
            catch (HttpRequestException hre) when (hre.Message.Contains("401"))
            {
                var tokenRefreshed = await this.ExchangeRefreshTokenAsync();
                if (tokenRefreshed != null)
                {
                    retryCall = true;
                }
            }

            if (retryCall)
            {
                deleteRequest = useCredentials
                                    ? new JsonDeleteNetworkRequest(new HttpClient(), uri, this.GetRequestHeaders())
                                    : new JsonDeleteNetworkRequest(new HttpClient(), uri);
                await deleteRequest.ExecuteAsync<bool>(cts);
                return true;
            }

            return false;
        }

        private Dictionary<string, string> GetRequestHeaders()
        {
            if (!string.IsNullOrWhiteSpace(this.Credentials?.AccessToken))
            {
                var headers = new Dictionary<string, string>
                                  {
                                      {
                                          "Authorization",
                                          $"Bearer {this.Credentials.AccessToken}"
                                      },
                                      { "Ocp-Apim-Subscription-Key", this.SubscriptionKey }
                                  };
                return headers;
            }

            return new Dictionary<string, string>();
        }
    }
}