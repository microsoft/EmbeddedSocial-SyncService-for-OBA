// <copyright file="Email.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Email
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OBAService.Storage.Model;
    using SocialPlus.Logging;

    /// <summary>
    /// Email for sending OBA Service status to operations team
    /// </summary>
    public class Email : SocialPlus.Server.Email.Email
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// Sets default values for email parameters.
        /// </summary>
        public Email()
        {
            this.TextBody = "Text is not available. Please read the HTML portion of this email.";
            this.Category = "OneBusAway Service Status";
        }

        /// <summary>
        /// Adds an exception to the email body
        /// </summary>
        /// <param name="e">Exception</param>
        public void Add(Exception e)
        {
            // initialize string
            if (this.HtmlBody == null)
            {
                this.HtmlBody = string.Empty;
            }

            this.HtmlBody += ConvertToHtml.Convert(e);
        }

        /// <summary>
        /// Adds download metadata to the email body
        /// </summary>
        /// <param name="records">Download metadata</param>
        public void Add(IEnumerable<DownloadMetadataEntity> records)
        {
            // initialize string
            if (this.HtmlBody == null)
            {
                this.HtmlBody = string.Empty;
            }

            this.HtmlBody += ConvertToHtml.Convert(records);
        }

        /// <summary>
        /// Adds diff metadata to the email body
        /// </summary>
        /// <param name="records">Diff metadata</param>
        public void Add(IEnumerable<DiffMetadataEntity> records)
        {
            // initialize string
            if (this.HtmlBody == null)
            {
                this.HtmlBody = string.Empty;
            }

            this.HtmlBody += ConvertToHtml.Convert(records);
        }

        /// <summary>
        /// Adds publish metadata to the email body
        /// </summary>
        /// <param name="records">Publish metadata</param>
        public void Add(IEnumerable<PublishMetadataEntity> records)
        {
            // initialize string
            if (this.HtmlBody == null)
            {
                this.HtmlBody = string.Empty;
            }

            this.HtmlBody += ConvertToHtml.Convert(records);
        }

        /// <summary>
        /// Adds the runId to the subject line
        /// </summary>
        /// <param name="runId">uniquely identifies the run</param>
        public void Add(string runId)
        {
            this.Subject = "OneBusAway service status from runId " + runId;
        }

        /// <summary>
        /// Adds the Embedded Social URI to the email body
        /// </summary>
        /// <param name="embeddedSocialUri">URI to the Embedded Social Service</param>
        public void Add(Uri embeddedSocialUri)
        {
            // initialize string
            if (this.HtmlBody == null)
            {
                this.HtmlBody = string.Empty;
            }

            // add header
            this.HtmlBody += "<h1>Embedded Social URI</h1>";
            this.HtmlBody += "<br>" + Environment.NewLine;

            // add URI
            this.HtmlBody += embeddedSocialUri.ToString() + Environment.NewLine;
            this.HtmlBody += "<br>" + Environment.NewLine;
        }

        /// <summary>
        /// Removes a text string in the subject and body if it is present
        /// and replaces it with a string of 'x' characters of same length
        /// </summary>
        /// <param name="text">string to look for and replace</param>
        public void RemoveString(string text)
        {
            string newText = new string('x', text.Length);
            this.Subject = this.Subject.Replace(text, newText);
            this.HtmlBody = this.HtmlBody.Replace(text, newText);
        }

        /// <summary>
        /// Sends this email
        /// </summary>
        /// <param name="sendGridKey">SendGrid key</param>
        /// <returns>task that sends the email</returns>
        public Task Send(string sendGridKey)
        {
            var log = new Log(LogDestination.Debug, Log.DefaultCategoryName);
            SocialPlus.Server.Email.SendGridEmail sendGridEmail = new SocialPlus.Server.Email.SendGridEmail(log, sendGridKey);
            return sendGridEmail.SendEmail(this);
        }
    }
}
