// <copyright file="DataRowState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage.Model
{
    /// <summary>
    /// Reflects the state of a row in the download, diff, or publish table.
    /// </summary>
    public enum DataRowState
    {
        /// <summary>
        /// Used only in the download table, this is a default state that does not convey any special meaning.
        /// </summary>
        Default,

        /// <summary>
        /// In the diff table, it means this is a new stop or route that needs to be added to Embedded Social.
        /// In the publish table, it means this stop or route has been added to Embedded Social.
        /// </summary>
        Create,

        /// <summary>
        /// In the diff table, it means the details of this stop or route have changed.
        /// In the publish table, it means this stop or route has been updated in Embedded Social.
        /// </summary>
        Update,

        /// <summary>
        /// In the diff table, it means this stop or route has dissapeared from OBA.
        /// In the publish table, it means this stop or route has been updated in Embedded Social to reflect that the stop or route is gone.
        /// </summary>
        Delete,

        /// <summary>
        /// In the diff table, it means this stop or route has re-appeared in OBA.
        /// In the publish table, it means this stop or route has been updated in Embedded Social to reflect that the stop or route is back.
        /// </summary>
        Resurrect
    }
}
