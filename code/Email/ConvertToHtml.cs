// <copyright file="ConvertToHtml.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Email
{
    using System;
    using System.Collections.Generic;

    using OBAService.Storage.Model;

    /// <summary>
    /// Utility methods to convert input parameters to HTML for humans to read
    /// </summary>
    public static class ConvertToHtml
    {
        /// <summary>
        /// Converts a set of download metadata records to HTML for humans to read
        /// </summary>
        /// <param name="records">download metadata</param>
        /// <returns>HTML formatted string</returns>
        public static string Convert(IEnumerable<DownloadMetadataEntity> records)
        {
            // initialize string
            string result = TableStyle();

            // add header
            result += "<h1>Download Metadata</h1>";
            result += "<br>" + Environment.NewLine;

            // create a table and header
            result += "<table>" + Environment.NewLine;
            result += "<tr>" + Environment.NewLine;
            result += "     <th>RunId</th>" + Environment.NewLine;
            result += "     <th>RegionId</th>" + Environment.NewLine;
            result += "     <th>AgencyId</th>" + Environment.NewLine;
            result += "     <th>RecordType</th>" + Environment.NewLine;
            result += "     <th>Count</th>" + Environment.NewLine;
            result += "</tr>" + Environment.NewLine;

            // create a row for each record
            foreach (DownloadMetadataEntity record in records)
            {
                result += "<tr>" + Environment.NewLine;
                result += "     <td>" + record.RunId + "</td>" + Environment.NewLine;
                result += "     <td>" + record.RegionId + "</td>" + Environment.NewLine;
                result += "     <td>" + record.AgencyId + "</td>" + Environment.NewLine;
                result += "     <td>" + record.RecordType + "</td>" + Environment.NewLine;
                result += "     <td>" + record.Count + "</td>" + Environment.NewLine;
                result += "</tr>" + Environment.NewLine;
            }

            // end the table
            result += "</table>" + Environment.NewLine;

            return result;
        }

        /// <summary>
        /// Converts a set of diff metadata records to HTML for humans to read
        /// </summary>
        /// <param name="records">diff metadata</param>
        /// <returns>HTML formatted string</returns>
        public static string Convert(IEnumerable<DiffMetadataEntity> records)
        {
            // initialize string
            string result = TableStyle();

            // add header
            result += "<h1>Diff Metadata</h1>";
            result += "<br>" + Environment.NewLine;

            // create a table and header
            result += "<table>" + Environment.NewLine;
            result += "<tr>" + Environment.NewLine;
            result += "     <th>RunId</th>" + Environment.NewLine;
            result += "     <th>RegionId</th>" + Environment.NewLine;
            result += "     <th>AgencyId</th>" + Environment.NewLine;
            result += "     <th>RecordType</th>" + Environment.NewLine;
            result += "     <th>AddedCount</th>" + Environment.NewLine;
            result += "     <th>UpdatedCount</th>" + Environment.NewLine;
            result += "     <th>DeletedCount</th>" + Environment.NewLine;
            result += "     <th>ResurrectedCount</th>" + Environment.NewLine;
            result += "</tr>" + Environment.NewLine;

            // create a row for each record
            foreach (DiffMetadataEntity record in records)
            {
                result += "<tr>" + Environment.NewLine;
                result += "     <td>" + record.RunId + "</td>" + Environment.NewLine;
                result += "     <td>" + record.RegionId + "</td>" + Environment.NewLine;
                result += "     <td>" + record.AgencyId + "</td>" + Environment.NewLine;
                result += "     <td>" + record.RecordType + "</td>" + Environment.NewLine;
                result += "     <td>" + record.AddedCount + "</td>" + Environment.NewLine;
                result += "     <td>" + record.UpdatedCount + "</td>" + Environment.NewLine;
                result += "     <td>" + record.DeletedCount + "</td>" + Environment.NewLine;
                result += "     <td>" + record.ResurrectedCount + "</td>" + Environment.NewLine;
                result += "</tr>" + Environment.NewLine;
            }

            // end the table
            result += "</table>" + Environment.NewLine;

            return result;
        }

        /// <summary>
        /// Converts a set of publish metadata records to HTML for humans to read
        /// </summary>
        /// <param name="records">publish metadata</param>
        /// <returns>HTML formatted string</returns>
        public static string Convert(IEnumerable<PublishMetadataEntity> records)
        {
            // initialize string
            string result = TableStyle();

            // add header
            result += "<h1>Publish Metadata</h1>";
            result += "<br>" + Environment.NewLine;

            // create a table and header
            result += "<table>" + Environment.NewLine;
            result += "<tr>" + Environment.NewLine;
            result += "     <th>RunId</th>" + Environment.NewLine;
            result += "     <th>RegionId</th>" + Environment.NewLine;
            result += "     <th>AgencyId</th>" + Environment.NewLine;
            result += "     <th>RecordType</th>" + Environment.NewLine;
            result += "     <th>AddedCount</th>" + Environment.NewLine;
            result += "     <th>UpdatedCount</th>" + Environment.NewLine;
            result += "     <th>DeletedCount</th>" + Environment.NewLine;
            result += "     <th>ResurrectedCount</th>" + Environment.NewLine;
            result += "</tr>" + Environment.NewLine;

            // create a row for each record
            foreach (PublishMetadataEntity record in records)
            {
                result += "<tr>" + Environment.NewLine;
                result += "     <td>" + record.RunId + "</td>" + Environment.NewLine;
                result += "     <td>" + record.RegionId + "</td>" + Environment.NewLine;
                result += "     <td>" + record.AgencyId + "</td>" + Environment.NewLine;
                result += "     <td>" + record.RecordType + "</td>" + Environment.NewLine;
                result += "     <td>" + record.AddedCount + "</td>" + Environment.NewLine;
                result += "     <td>" + record.UpdatedCount + "</td>" + Environment.NewLine;
                result += "     <td>" + record.DeletedCount + "</td>" + Environment.NewLine;
                result += "     <td>" + record.ResurrectedCount + "</td>" + Environment.NewLine;
                result += "</tr>" + Environment.NewLine;
            }

            // end the table
            result += "</table>" + Environment.NewLine;

            return result;
        }

        /// <summary>
        /// Converts an exception to HTML for humans to read
        /// </summary>
        /// <param name="e">Exception</param>
        /// <returns>HTML formatted string</returns>
        public static string Convert(Exception e)
        {
            // initialize string
            string result = string.Empty;

            // add header
            result += "<h1>Exception</h1>";
            result += "<br>" + Environment.NewLine;

            // add exception
            result += "<pre>" + Environment.NewLine;
            result += SocialPlus.Utils.ExceptionHelper.FlattenException(e);
            result += "</pre>" + Environment.NewLine;
            result += "<br>" + Environment.NewLine;

            return result;
        }

        /// <summary>
        /// Produces a table style for HTML
        /// </summary>
        /// <returns>string with HTML table style</returns>
        private static string TableStyle()
        {
            string result = string.Empty;
            result += "<head>" + Environment.NewLine;
            result += "<style>" + Environment.NewLine;
            result += "table {" + Environment.NewLine;
            result += "    border: 1px solid black;" + Environment.NewLine;
            result += "    border-collapse: collapse;" + Environment.NewLine;
            result += "}" + Environment.NewLine;
            result += "th, td {" + Environment.NewLine;
            result += "    border: 1px solid black;" + Environment.NewLine;
            result += "    border-collapse: collapse;" + Environment.NewLine;
            result += "    padding: 15px;" + Environment.NewLine;
            result += "}" + Environment.NewLine;
            result += "</style>" + Environment.NewLine;
            result += "</head>" + Environment.NewLine;
            return result;
        }
    }
}
