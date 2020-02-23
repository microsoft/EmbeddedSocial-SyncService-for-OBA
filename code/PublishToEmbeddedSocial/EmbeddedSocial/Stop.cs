// <copyright file="Stop.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.PublishToEmbeddedSocial
{
    using System;
    using System.Threading.Tasks;

    using OBAService.Storage.Model;
    using OBAService.Utils;

    /// <summary>
    /// Publishes a stop to Embedded Social
    /// </summary>
    public partial class EmbeddedSocial
    {
        /// <summary>
        /// Publish a new stop to Embedded Social
        /// </summary>
        /// <param name="stop">new stop information</param>
        /// <returns>task that publishes a new stop and returns the topic name</returns>
        public async Task<string> CreateStop(StopEntity stop)
        {
            // check the input
            this.CheckInputEntity(stop);

            // publish the new topic
            string topicName = this.TopicName(stop);
            await this.CreateTopic(topicName, this.TopicTitle(stop), this.TopicText(stop), stop.RegionId);

            // return the topic name
            return topicName;
        }

        /// <summary>
        /// Publish an updated stop to Embedded Social
        /// </summary>
        /// <param name="stop">updated stop information</param>
        /// <returns>task that updates the stop</returns>
        public async Task UpdateStop(StopEntity stop)
        {
            // check the input
            this.CheckInputEntity(stop);

            // update the topic
            await this.UpdateTopic(this.TopicName(stop), this.TopicTitle(stop), this.TopicText(stop), stop.RegionId);
        }

        /// <summary>
        /// Delete a published stop from Embedded Social.
        /// The way we delete a stop is to simply indicate it as deleted
        /// in the Embedded Social topic title. That way, existing
        /// comments live on, and users who visit the topic will know
        /// that this stop is no longer valid.
        /// </summary>
        /// <param name="stop">stop that no longer exists</param>
        /// <returns>task that deletes the stop</returns>
        public async Task DeleteStop(StopEntity stop)
        {
            // check the input
            this.CheckInputEntity(stop);

            // update the topic title to indicate this has been deleted
            await this.UpdateTopic(this.TopicName(stop), DeletedTopicTitlePrefix + this.TopicTitle(stop), this.TopicText(stop), stop.RegionId);
        }

        /// <summary>
        /// Resurrect a published stop from Embedded Social.
        /// The way we delete a stop is to simply indicate it as deleted
        /// in the Embedded Social topic title. To resurrect it, that
        /// text has to be removed from the topic title.
        /// </summary>
        /// <param name="stop">stop that has been restored</param>
        /// <returns>task that deletes the stop</returns>
        public async Task ResurrectStop(StopEntity stop)
        {
            await this.UpdateStop(stop);
        }

        /// <summary>
        /// Constructs the topic name for a stop
        /// </summary>
        /// <param name="stop">stop information</param>
        /// <returns>topic name</returns>
        private string TopicName(StopEntity stop)
        {
            string name = "stop_" + stop.RegionId + "_" + stop.Id;

            // Embedded Social's named topics can store only strings that are safe as an Azure Table key.
            name = name.StringToTableKey();

            return name;
        }

        /// <summary>
        /// Constructs the topic title for a stop
        /// </summary>
        /// <param name="stop">stop information</param>
        /// <returns>topic title</returns>
        private string TopicTitle(StopEntity stop)
        {
            // in the common case, the topic title will be:
            //      Name (Direction)
            string title = string.Empty;

            if (!string.IsNullOrWhiteSpace(stop.Name))
            {
                title += stop.Name;
            }

            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(stop.Direction))
            {
                title += " (" + stop.Direction + ")";
            }

            return TopicUtils.RemoveHashtags(title);
        }

        /// <summary>
        /// Constructs the topic text for a stop
        /// </summary>
        /// <param name="stop">stop information</param>
        /// <returns>topic text</returns>
        private string TopicText(StopEntity stop)
        {
            // in the common case, the topic text will be:
            //      Discuss the stop at Name (Direction)
            string text = string.Empty;

            if (!string.IsNullOrWhiteSpace(stop.Name))
            {
                text += "Discuss the stop at " + stop.Name;
            }

            if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(stop.Direction))
            {
                text += " (" + stop.Direction + ")";
            }

            return TopicUtils.RemoveHashtags(text);
        }

        /// <summary>
        /// Checks a stop for validity before publishing to Embedded Social.
        /// Will throw an exception if invalid.
        /// </summary>
        /// <param name="stop">stop</param>
        private void CheckInputEntity(StopEntity stop)
        {
            if (stop == null)
            {
                throw new ArgumentNullException("stop");
            }
            else if (string.IsNullOrWhiteSpace(stop.Id))
            {
                throw new ArgumentException("stop id is null or whitespace", "stop");
            }
            else if (string.IsNullOrWhiteSpace(stop.RegionId))
            {
                throw new ArgumentException("stop region id is null or whitespace", "stop");
            }
            else if (string.IsNullOrWhiteSpace(stop.Name))
            {
                throw new ArgumentException("stop name is null or whitespace", "stop");
            }
        }
    }
}