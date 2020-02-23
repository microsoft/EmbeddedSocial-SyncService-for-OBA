// <copyright file="EmbeddedSocial.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.PublishToEmbeddedSocial
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Rest;
    using SocialPlus.Client.Models;

    /// <summary>
    /// Interfaces with the Embedded Social service and publishes OneBusAway topics
    /// </summary>
    public partial class EmbeddedSocial
    {
        /// <summary>
        /// The language that route, stop, and agency topics will have
        /// </summary>
        private const string TopicLanguage = "en-us";

        /// <summary>
        /// The string that precedes the title of a route, stop, or agency that has been deleted from OBA
        /// </summary>
        private const string DeletedTopicTitlePrefix = "DELETED: ";

        /// <summary>
        /// Embedded Social client interface
        /// </summary>
        private SocialPlus.Client.SocialPlusClient client;

        /// <summary>
        /// Embedded Social authorization
        /// </summary>
        private string authorization;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedSocial"/> class.
        /// </summary>
        /// <param name="embeddedSocialBaseUri">URI to the Embedded Social service</param>
        /// <param name="appKey">Embedded Social application key</param>
        /// <param name="aadToken">AAD token for signing into Embedded Social</param>
        /// <param name="userHandle">Administrative user handle that will publish topics</param>
        public EmbeddedSocial(Uri embeddedSocialBaseUri, string appKey, string aadToken, string userHandle)
        {
            this.client = new SocialPlus.Client.SocialPlusClient(embeddedSocialBaseUri);
            string aadAuthorization = CreateAADS2SAuth(aadToken, appKey, userHandle);
            this.authorization = this.CreateEmbeddedSocialAuth(aadAuthorization, userHandle).Result;
        }

        /// <summary>
        /// Creates the proper value for the authorization field for Embedded Social clients using AAD S2S
        /// </summary>
        /// <param name="aadToken">AAD token</param>
        /// <param name="appKey">app key</param>
        /// <param name="userHandle">user handle (default value of null)</param>
        /// <returns>authorization value</returns>
        private static string CreateAADS2SAuth(string aadToken, string appKey, string userHandle = null)
        {
            if (string.IsNullOrEmpty(userHandle))
            {
                return "AADS2S AK=" + appKey + "|TK=" + aadToken;
            }

            return "AADS2S AK=" + appKey + "|UH=" + userHandle + "|TK=" + aadToken;
        }

        /// <summary>
        /// Creates an Embedded Social authorization field
        /// </summary>
        /// <param name="aadAuthorization">AAD authorization field</param>
        /// <param name="userHandle">user handle that corresponds to the AAD authorization field</param>
        /// <returns>authorization value</returns>
        private async Task<string> CreateEmbeddedSocialAuth(string aadAuthorization, string userHandle)
        {
            PostSessionRequest sessionRequest = new PostSessionRequest()
            {
                InstanceId = "OneBusAway Service",
                UserHandle = userHandle
            };
            HttpOperationResponse<PostSessionResponse> response = await this.client.Sessions.PostSessionWithHttpMessagesAsync(request: sessionRequest, authorization: aadAuthorization);
            if (response == null || response.Response == null)
            {
                throw new Exception("did not get a valid response to POST session");
            }
            else if (!response.Response.IsSuccessStatusCode)
            {
                throw new Exception("POST session failed with HTTP code " + response.Response.StatusCode);
            }
            else if (response.Body == null || string.IsNullOrWhiteSpace(response.Body.SessionToken))
            {
                throw new Exception("POST session resulted in invalid post session response structure");
            }

            return "SocialPlus TK=" + response.Body.SessionToken;
        }

        /// <summary>
        /// Publish a new app-published topic with a topic name to Embedded Social
        /// </summary>
        /// <param name="topicName">name of the new topic</param>
        /// <param name="topicTitle">title of the new topic</param>
        /// <param name="topicText">text of the new topic</param>
        /// <param name="topicCategory">category of the new topic</param>
        /// <returns>task that publishes a new topic</returns>
        private async Task CreateTopic(string topicName, string topicTitle, string topicText, string topicCategory)
        {
            // format the input into an Embedded Social topic request
            PostTopicRequest topicRequest = new PostTopicRequest()
            {
                PublisherType = PublisherType.App,
                Text = topicText,
                Title = topicTitle,
                BlobType = BlobType.Unknown,
                BlobHandle = string.Empty,
                Categories = topicCategory,
                Language = TopicLanguage,
                DeepLink = string.Empty,
                FriendlyName = topicName,
                Group = string.Empty
            };

            // publish to Embedded Social
            HttpOperationResponse<PostTopicResponse> postTopicOperationResponse = await this.client.Topics.PostTopicWithHttpMessagesAsync(request: topicRequest, authorization: this.authorization);

            // check response
            if (postTopicOperationResponse == null || postTopicOperationResponse.Response == null)
            {
                throw new Exception("got null response");
            }
            else if (!postTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                throw new Exception("request failed with HTTP code: " + postTopicOperationResponse.Response.StatusCode + ", and reason: " + postTopicOperationResponse.Response.ReasonPhrase);
            }
            else if (postTopicOperationResponse.Body == null)
            {
                throw new Exception("got null response body");
            }
            else if (string.IsNullOrWhiteSpace(postTopicOperationResponse.Body.TopicHandle))
            {
                throw new Exception("topicHandle is null or whitespace");
            }

            // publish the name for the topic
            string topicHandle = postTopicOperationResponse.Body.TopicHandle;
            PostTopicNameRequest topicNameRequest = new PostTopicNameRequest()
            {
                PublisherType = PublisherType.App,
                TopicName = topicName,
                TopicHandle = topicHandle
            };

            HttpOperationResponse postTopicNameOperationResponse = await this.client.Topics.PostTopicNameWithHttpMessagesAsync(request: topicNameRequest, authorization: this.authorization);

            // check response
            if (postTopicNameOperationResponse == null || postTopicNameOperationResponse.Response == null)
            {
                // attempt to clean up the topic first
                HttpOperationResponse deleteTopicOperationResponse = await this.client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: this.authorization);

                throw new Exception("got null response");
            }
            else if (!postTopicNameOperationResponse.Response.IsSuccessStatusCode)
            {
                // attempt to clean up the topic first
                HttpOperationResponse deleteTopicOperationResponse = await this.client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: this.authorization);

                throw new Exception("request failed with HTTP code: " + postTopicNameOperationResponse.Response.StatusCode + ", and reason: " + postTopicNameOperationResponse.Response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Publish an updated topic to Embedded Social
        /// </summary>
        /// <param name="topicName">name of the existing topic</param>
        /// <param name="topicTitle">new title of topic</param>
        /// <param name="topicText">new text of the topic</param>
        /// <param name="topicCategory">new category of the topic</param>
        /// <returns>task that updates the topic</returns>
        private async Task UpdateTopic(string topicName, string topicTitle, string topicText, string topicCategory)
        {
            // get the handle for the topic name
            HttpOperationResponse<GetTopicByNameResponse> getTopicByNameOperationResponse = await this.client.Topics.GetTopicByNameWithHttpMessagesAsync(topicName: topicName, publisherType: PublisherType.App, authorization: this.authorization);

            // check response
            if (getTopicByNameOperationResponse == null || getTopicByNameOperationResponse.Response == null)
            {
                throw new Exception("got null response");
            }
            else if (!getTopicByNameOperationResponse.Response.IsSuccessStatusCode)
            {
                throw new Exception("request failed with HTTP code: " + getTopicByNameOperationResponse.Response.StatusCode + ", and reason: " + getTopicByNameOperationResponse.Response.ReasonPhrase);
            }
            else if (getTopicByNameOperationResponse.Body == null)
            {
                throw new Exception("got null response body");
            }
            else if (string.IsNullOrWhiteSpace(getTopicByNameOperationResponse.Body.TopicHandle))
            {
                throw new Exception("topicHandle is null or whitespace");
            }

            // format the updated topic request
            string topicHandle = getTopicByNameOperationResponse.Body.TopicHandle;
            PutTopicRequest updatedTopic = new PutTopicRequest()
            {
                Text = topicText,
                Title = topicTitle,
                Categories = topicCategory,
            };

            // submit the updated topic request
            HttpOperationResponse putTopicOperationResponse = await this.client.Topics.PutTopicWithHttpMessagesAsync(topicHandle: topicHandle, request: updatedTopic, authorization: this.authorization);

            // check response
            if (putTopicOperationResponse == null || putTopicOperationResponse.Response == null)
            {
                throw new Exception("got null response");
            }
            else if (!putTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                throw new Exception("request failed with HTTP code: " + putTopicOperationResponse.Response.StatusCode + ", and reason: " + putTopicOperationResponse.Response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Get a topic from Embedded Social
        /// </summary>
        /// <param name="topicName">name of the existing topic</param>
        /// <returns>topic view</returns>
        private async Task<TopicView> GetTopic(string topicName)
        {
            // get the handle for the topic name
            HttpOperationResponse<GetTopicByNameResponse> getTopicByNameOperationResponse = await this.client.Topics.GetTopicByNameWithHttpMessagesAsync(topicName: topicName, publisherType: PublisherType.App, authorization: this.authorization);

            // check response
            if (getTopicByNameOperationResponse == null || getTopicByNameOperationResponse.Response == null)
            {
                throw new Exception("got null response");
            }
            else if (!getTopicByNameOperationResponse.Response.IsSuccessStatusCode)
            {
                throw new Exception("request failed with HTTP code: " + getTopicByNameOperationResponse.Response.StatusCode + ", and reason: " + getTopicByNameOperationResponse.Response.ReasonPhrase);
            }
            else if (getTopicByNameOperationResponse.Body == null)
            {
                throw new Exception("got null response body");
            }
            else if (string.IsNullOrWhiteSpace(getTopicByNameOperationResponse.Body.TopicHandle))
            {
                throw new Exception("topicHandle is null or whitespace");
            }

            // get the topic
            string topicHandle = getTopicByNameOperationResponse.Body.TopicHandle;
            HttpOperationResponse<TopicView> getTopicOperationResponse = await this.client.Topics.GetTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: this.authorization);

            // check response
            if (getTopicOperationResponse == null || getTopicOperationResponse.Response == null)
            {
                throw new Exception("got null response");
            }
            else if (!getTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                throw new Exception("request failed with HTTP code: " + getTopicOperationResponse.Response.StatusCode + ", and reason: " + getTopicOperationResponse.Response.ReasonPhrase);
            }

            return getTopicOperationResponse.Body;
        }

        /// <summary>
        /// Delete a topic from Embedded Social
        /// </summary>
        /// <param name="topicName">name of the existing topic</param>
        /// <returns>task that deletes the topic name and the topic</returns>
        private async Task DeleteTopic(string topicName)
        {
            // get the handle for the topic name
            HttpOperationResponse<GetTopicByNameResponse> getTopicByNameOperationResponse = await this.client.Topics.GetTopicByNameWithHttpMessagesAsync(topicName: topicName, publisherType: PublisherType.App, authorization: this.authorization);

            // check response
            if (getTopicByNameOperationResponse == null || getTopicByNameOperationResponse.Response == null)
            {
                throw new Exception("got null response");
            }
            else if (!getTopicByNameOperationResponse.Response.IsSuccessStatusCode)
            {
                throw new Exception("request failed with HTTP code: " + getTopicByNameOperationResponse.Response.StatusCode + ", and reason: " + getTopicByNameOperationResponse.Response.ReasonPhrase);
            }
            else if (getTopicByNameOperationResponse.Body == null)
            {
                throw new Exception("got null response body");
            }
            else if (string.IsNullOrWhiteSpace(getTopicByNameOperationResponse.Body.TopicHandle))
            {
                throw new Exception("topicHandle is null or whitespace");
            }

            // delete the topic
            string topicHandle = getTopicByNameOperationResponse.Body.TopicHandle;
            HttpOperationResponse deleteTopicOperationResponse = await this.client.Topics.DeleteTopicWithHttpMessagesAsync(topicHandle: topicHandle, authorization: this.authorization);

            // check response
            if (deleteTopicOperationResponse == null || deleteTopicOperationResponse.Response == null)
            {
                throw new Exception("got null response");
            }
            else if (!deleteTopicOperationResponse.Response.IsSuccessStatusCode)
            {
                throw new Exception("request failed with HTTP code: " + deleteTopicOperationResponse.Response.StatusCode + ", and reason: " + deleteTopicOperationResponse.Response.ReasonPhrase);
            }

            // delete the topic name
            DeleteTopicNameRequest deleteTopicNameRequest = new DeleteTopicNameRequest()
            {
                PublisherType = PublisherType.App
            };
            HttpOperationResponse deleteTopicNameOperationResponse = await this.client.Topics.DeleteTopicNameWithHttpMessagesAsync(topicName: topicName, request: deleteTopicNameRequest, authorization: this.authorization);

            // check response
            if (deleteTopicNameOperationResponse == null || deleteTopicNameOperationResponse.Response == null)
            {
                throw new Exception("got null response");
            }
            else if (!deleteTopicNameOperationResponse.Response.IsSuccessStatusCode)
            {
                throw new Exception("request failed with HTTP code: " + deleteTopicNameOperationResponse.Response.StatusCode + ", and reason: " + deleteTopicNameOperationResponse.Response.ReasonPhrase);
            }
        }
    }
}
