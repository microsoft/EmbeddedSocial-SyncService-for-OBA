// <copyright file="AbstractEntityAdapter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// An Adapter class that maps a Type to a Table storage entity and vice versa.
    /// This code is reused from msdn link "https://code.msdn.microsoft.com/windowsapps/Windows-Azure-Storage-675fe55b/sourcecode?fileId=100480&amp;pathId=191588782"
    /// Check EntityAdapter class in TechEd13.Storage.Tables.CoolFeatures
    /// </summary>
    public abstract class AbstractEntityAdapter
    {
        private static ConcurrentDictionary<Type, EntityMapperBase> mapperDictionary = new ConcurrentDictionary<Type, EntityMapperBase>();

        /// <summary>
        /// Gets or sets a dictionary of types a keys and EntityMapperBase as values
        /// </summary>
        internal static ConcurrentDictionary<Type, EntityMapperBase> MapperDictionary
        {
            get
            {
                return mapperDictionary;
            }

            set
            {
                mapperDictionary = value;
            }
        }

        /// <summary>
        /// Registers a Type for mapping to Partion and Row keys
        /// </summary>
        /// <typeparam name="TElement">Type T</typeparam>
        /// <param name="objectToPartitionKey">A function to map object to partition key</param>
        /// <param name="objectToRowKey">A function to map object to row key</param>
        /// <param name="partitionKeyToObject">A function to map partition key to object</param>
        /// <param name="rowKeyToObject">A function to map row key to object</param>
        /// <returns>bool</returns>
        public static bool RegisterType<TElement>(Func<TElement, string> objectToPartitionKey, Func<TElement, string> objectToRowKey, Action<TElement, string> partitionKeyToObject, Action<TElement, string> rowKeyToObject)
        {
            return MapperDictionary.TryAdd(typeof(TElement), new EntityMapperSet<TElement>() { ObjectToPartitionKey = objectToPartitionKey, ObjectToRowKey = objectToRowKey, PartitionKeyToObject = partitionKeyToObject, RowKeyToObject = rowKeyToObject });
        }

        /// <summary>
        /// Returns an EntityAdapter class based on the parameters
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="partitionKey">partition key</param>
        /// <param name="rowKey">row key</param>
        /// <param name="timestamp">timestamp</param>
        /// <param name="properties">properties bag of entity</param>
        /// <param name="etag">etag</param>
        /// <returns>Type instance</returns>
        public static T AdapterResolver<T>(string partitionKey, string rowKey, DateTimeOffset timestamp, IDictionary<string, EntityProperty> properties, string etag)
            where T : new()
        {
            EntityAdapter<T> adapterEnt = new EntityAdapter<T>();
            adapterEnt.PartitionKey = partitionKey;
            adapterEnt.RowKey = rowKey;
            adapterEnt.Timestamp = timestamp;
            adapterEnt.ETag = etag;
            adapterEnt.ReadEntity(properties, null /*operationContext*/);
            return adapterEnt.InnerObject;
        }

        /// <summary>
        /// EntityMapperBase class
        /// </summary>
        internal abstract class EntityMapperBase
        {
        }

        /// <summary>
        /// EntityMapperSet class for a type that holds mapping functions
        /// </summary>
        /// <typeparam name="TElement">Type</typeparam>
        internal class EntityMapperSet<TElement> : EntityMapperBase
        {
            /// <summary>
            /// Gets or sets a function to map object to partition key
            /// </summary>
            public Func<TElement, string> ObjectToPartitionKey { get; set; }

            /// <summary>
            /// Gets or sets a function to map object to row key
            /// </summary>
            public Func<TElement, string> ObjectToRowKey { get; set; }

            /// <summary>
            /// Gets or sets a function to map partition key to object
            /// </summary>
            public Action<TElement, string> PartitionKeyToObject { get; set; }

            /// <summary>
            /// Gets or sets a function to map row key to object
            /// </summary>
            public Action<TElement, string> RowKeyToObject { get; set; }
        }
    }
}
