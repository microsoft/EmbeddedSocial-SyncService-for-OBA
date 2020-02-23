// <copyright file="Route.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.PublishToEmbeddedSocial
{
    using System;
    using System.Threading.Tasks;

    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Publishes a route to Embedded Social
    /// </summary>
    public partial class EmbeddedSocial
    {
        /// <summary>
        /// Publish a new route to Embedded Social
        /// </summary>
        /// <param name="route">new route information</param>
        /// <returns>task that publishes a new route and returns the topic name</returns>
        public async Task<string> CreateRoute(RouteEntity route)
        {
            // check the input
            this.CheckInputEntity(route);

            // publish the new topic
            string topicName = this.TopicName(route);
            await this.CreateTopic(topicName, this.TopicTitle(route), this.TopicText(route), route.RegionId);

            // return the topic name
            return topicName;
        }

        /// <summary>
        /// Publish an updated route to Embedded Social
        /// </summary>
        /// <param name="route">updated route information</param>
        /// <returns>task that updates the route</returns>
        public async Task UpdateRoute(RouteEntity route)
        {
            // check the input
            this.CheckInputEntity(route);

            // update the topic
            await this.UpdateTopic(this.TopicName(route), this.TopicTitle(route), this.TopicText(route), route.RegionId);
        }

        /// <summary>
        /// Delete a published route from Embedded Social.
        /// The way we delete a route is to simply indicate it as deleted
        /// in the Embedded Social topic title. That way, existing
        /// comments live on, and users who visit the topic will know
        /// that this route is no longer valid.
        /// </summary>
        /// <param name="route">route that no longer exists</param>
        /// <returns>task that deletes the route</returns>
        public async Task DeleteRoute(RouteEntity route)
        {
            // check the input
            this.CheckInputEntity(route);

            // update the topic title to indicate this has been deleted
            await this.UpdateTopic(this.TopicName(route), DeletedTopicTitlePrefix + this.TopicTitle(route), this.TopicText(route), route.RegionId);
        }

        /// <summary>
        /// Resurrect a published route that was deleted from Embedded Social.
        /// The way we delete a route is to simply indicate it as deleted
        /// in the Embedded Social topic title. To resurrect, that text
        /// has to be removed from the title.
        /// </summary>
        /// <param name="route">route that needs to be restored</param>
        /// <returns>task that resurrects the route</returns>
        public async Task ResurrectRoute(RouteEntity route)
        {
            await this.UpdateRoute(route);
        }

        /// <summary>
        /// Constructs the topic name for a route
        /// </summary>
        /// <param name="route">route information</param>
        /// <returns>topic name</returns>
        private string TopicName(RouteEntity route)
        {
            string name = "route_" + route.RegionId + "_" + route.Id;

            // Embedded Social's named topics can store only strings that are safe as an Azure Table key.
            name = name.StringToTableKey();

            return name;
        }

        /// <summary>
        /// Constructs the topic title for a route.
        /// </summary>
        /// <param name="route">route information</param>
        /// <returns>topic title</returns>
        private string TopicTitle(RouteEntity route)
        {
            // in the common case, the topic title will be:
            //      ShortName - LongName
            string title = string.Empty;

            if (!string.IsNullOrWhiteSpace(route.ShortName))
            {
                title += route.ShortName;
            }

            if (!string.IsNullOrWhiteSpace(route.ShortName) && !string.IsNullOrWhiteSpace(route.LongName))
            {
                title += " - ";
            }

            if (!string.IsNullOrWhiteSpace(route.LongName))
            {
                title += route.LongName;
            }

            return TopicUtils.RemoveHashtags(title);
        }

        /// <summary>
        /// Constructs the topic text for a route
        /// </summary>
        /// <param name="route">route information</param>
        /// <returns>topic text</returns>
        private string TopicText(RouteEntity route)
        {
            // in the common case, the topic text will be:
            //      Discuss the LongName route
            string text = string.Empty;

            if (!string.IsNullOrWhiteSpace(route.ShortName) || !string.IsNullOrWhiteSpace(route.LongName))
            {
                text += "Discuss the ";
            }

            if (!string.IsNullOrWhiteSpace(route.LongName))
            {
                text += route.LongName;
            }
            else if (!string.IsNullOrWhiteSpace(route.ShortName))
            {
                text += route.ShortName;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                text += " route";
            }

            return TopicUtils.RemoveHashtags(text);
        }

        /// <summary>
        /// Checks a route for validity before publishing to Embedded Social.
        /// Will throw an exception if invalid.
        /// </summary>
        /// <param name="route">route</param>
        private void CheckInputEntity(RouteEntity route)
        {
            if (route == null)
            {
                throw new ArgumentNullException("route");
            }
            else if (string.IsNullOrWhiteSpace(route.Id))
            {
                throw new ArgumentException("route id is null or whitespace", "route");
            }
            else if (string.IsNullOrWhiteSpace(route.RegionId))
            {
                throw new ArgumentException("route region id is null or whitespace", "route");
            }
            else if (string.IsNullOrWhiteSpace(route.AgencyId))
            {
                throw new ArgumentException("route agency id is null or whitespace", "route");
            }
            else if (string.IsNullOrWhiteSpace(route.LongName) && string.IsNullOrWhiteSpace(route.ShortName))
            {
                throw new ArgumentException("route long name and short name are both null or whitespace", "route");
            }
        }
    }
}