﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Exceptions;
using Bluehands.Hypermedia.Client.ParameterSerializer;
using Bluehands.Hypermedia.Client.Reader;
using Bluehands.Hypermedia.Client.Resolver;
using Bluehands.Hypermedia.MediaTypes;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public class HttpHypermediaResolver
        : HypermediaResolverBase<HttpResponseMessage, string>
    {
        private readonly HttpClient httpClient;
        private readonly bool disposeHttpClient;

        private bool alreadyDisposed = false;

        public HttpHypermediaResolver(
            HttpClient httpClient,
            bool disposeHttpClient,
            IHypermediaReader hypermediaReader,
            IParameterSerializer parameterSerializer,
            IProblemStringReader problemReader,
            ILinkHcoCache<string> linkHcoCache)
            : base(hypermediaReader, parameterSerializer, problemReader, linkHcoCache)
        {
            this.httpClient = httpClient;
            this.disposeHttpClient = disposeHttpClient;
        }

        protected override async Task<HttpResponseMessage> ResolveAsync(Uri uriToResolve)
        {
            return await this.httpClient.GetAsync(uriToResolve);
        }

        protected override async Task<(HttpResponseMessage response, bool wasModified)> ResolveWithCheckForModificationAsync(
            Uri uriToResolve,
            string identifier)
        {
            string Quoted(string value)
            {
                const string doubleQuoteString = "\"";
                if (!value.StartsWith(doubleQuoteString))
                {
                    value = doubleQuoteString + value;
                }

                if (!value.EndsWith(doubleQuoteString))
                {
                    value = value + doubleQuoteString;
                }

                return value;
            }
            var request = new HttpRequestMessage()
            {
                RequestUri = uriToResolve,
                Method = HttpMethod.Get,
                Headers =
                {
                    IfNoneMatch =
                    {
                        new EntityTagHeaderValue(Quoted(identifier)),
                    },
                },
            };
            var response = await this.httpClient.SendAsync(request, CancellationToken.None);
            return (response, response.StatusCode != HttpStatusCode.NotModified);
        }

        protected override bool HasCacheIdentifier(
            HttpResponseMessage responseMessage,
            out string identifier)
        {
            if (!string.IsNullOrEmpty(responseMessage.Headers.ETag?.Tag))
            {
                const char doubleQuoteChar = '"';
                var unquoted = responseMessage.Headers.ETag.Tag.Trim(doubleQuoteChar);
                identifier = unquoted;
                return true;
            }

            identifier = string.Empty;
            return false;
        }

        protected override async Task<HttpResponseMessage> SendCommandAsync(
            Uri uri,
            string method,
            string payload = null)
        {
            var httpMethod = GetHttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, uri);

            if (!string.IsNullOrEmpty(payload))
            {
                request.Content = new StringContent(payload, Encoding.UTF8, DefaultMediaTypes.ApplicationJson);//CONTENT-TYPE header    
            }

            var responseMessage = await httpClient.SendAsync(request);

            return responseMessage;
        }

        protected override async Task EnsureRequestIsSuccessfulAsync(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return;
            }

            var innerException = GetInnerException(responseMessage);

            var (hasProblemDescription, problemDescription) = await this.TryReadProblemStringAsync(responseMessage);
            if (hasProblemDescription)
            {
                throw new HypermediaProblemException(problemDescription, innerException);
            }

            var message = innerException.Message ?? string.Empty;
            throw new RequestNotSuccessfulException(message, (int)responseMessage.StatusCode, innerException);
        }

        private async Task<(bool hasProblemDescription, ProblemDescription problemDescription)> TryReadProblemStringAsync(HttpResponseMessage response)
        {
            if (response.Content == null)
            {
                return (false, null);
            }
            try
            {
                var contentAsString = await response.Content.ReadAsStringAsync();
                var result = this.ProblemReader.TryReadProblemString(contentAsString, out var problemDescription);
                return (result, problemDescription);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }

        private static Exception GetInnerException(HttpResponseMessage result)
        {
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception inner)
            {
                return inner;
            }

            return null;
        }

        protected override async Task<Stream> ResponseAsStreamAsync(HttpResponseMessage responseMessage)
        {
            return await responseMessage.Content.ReadAsStreamAsync();
        }

        protected override Uri GetLocation(HttpResponseMessage responseMessage)
        {
            return responseMessage.Headers.Location;
        }

        private static HttpMethod GetHttpMethod(string method)
        {
            switch (method.ToUpperInvariant())
            {
                case "DELETE":
                    return HttpMethod.Delete;
                case "GET":
                    return HttpMethod.Get;
                case "HEAD":
                    return HttpMethod.Head;
                case "OPTIONS":
                    return HttpMethod.Options;
                case "POST":
                    return HttpMethod.Post;
                case "PUT":
                    return HttpMethod.Put;
                case "TRACE":
                    return HttpMethod.Trace;
                case "PATCH":
                    return new HttpMethod("PATCH");
                default:
                    throw new Exception($"Unknown method: '{method}'");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.alreadyDisposed)
            {
                return;
            }
            if (disposing)
            {
                if (this.disposeHttpClient)
                {
                    this.httpClient.Dispose();
                }
            }

            this.alreadyDisposed = true;
            base.Dispose(disposing);
        }
    }
}