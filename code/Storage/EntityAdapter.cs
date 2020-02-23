// <copyright file="EntityAdapter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Storage
{
    using System;
    using System.Collections.Generic;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// An Adapter class that maps a Type to a Table storage entity and vice versa.
    /// This code is reused from msdn link "https://code.msdn.microsoft.com/windowsapps/Windows-Azure-Storage-675fe55b/sourcecode?fileId=100480&amp;pathId=191588782"
    /// Check EntityAdapter class in TechEd13.Storage.Tables.CoolFeatures
    /// </summary>
    /// <typeparam name="T">Type T</typeparam>
    public class EntityAdapter<T> : AbstractEntityAdapter, ITableEntity
        where T : new()
    {
        private EntityMapperSet<T> mapperSet = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAdapter{T}"/> class.
        /// </summary>
        public EntityAdapter()
        {
            // If you would like to work with objects that do not have a default Ctor you can use (T)Activator.CreateInstance(typeof(T));
            this.InnerObject = new T();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAdapter{T}"/> class.
        /// </summary>
        /// <param name="innerObject">inner Object</param>
        public EntityAdapter(T innerObject)
        {
            this.InnerObject = innerObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAdapter{T}"/> class.
        /// </summary>
        /// <param name="innerObject">innerObject</param>
        /// <param name="objectToPartitionKey">objectToPartitionKey</param>
        /// <param name="objectToRowKey">objectToRowKey</param>
        /// <param name="partitionKeyToObject">partitionKeyToObject</param>
        /// <param name="rowKeyToObject">rowKeyToObject</param>
        public EntityAdapter(T innerObject, Func<T, string> objectToPartitionKey, Func<T, string> objectToRowKey, Action<T, string> partitionKeyToObject, Action<T, string> rowKeyToObject)
            : this(innerObject)
        {
            AbstractEntityAdapter.RegisterType(objectToPartitionKey, objectToRowKey, partitionKeyToObject, rowKeyToObject);
        }

        /// <summary>
        /// Gets or sets InnerObject
        /// </summary>
        public T InnerObject { get; set; }

        /// <summary>
        /// Gets or sets the entity's partition key.
        /// </summary>
        /// <value>The partition key of the entity.</value>
        public string PartitionKey
        {
            get
            {
                return this.MapperSet.ObjectToPartitionKey(this.InnerObject);
            }

            set
            {
                if (this.MapperSet.PartitionKeyToObject != null)
                {
                    this.MapperSet.PartitionKeyToObject(this.InnerObject, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity's row key.
        /// </summary>
        /// <value>The row key of the entity.</value>
        public string RowKey
        {
            get
            {
                return this.MapperSet.ObjectToRowKey(this.InnerObject);
            }

            set
            {
                if (this.MapperSet.RowKeyToObject != null)
                {
                    this.MapperSet.RowKeyToObject(this.InnerObject, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity's timestamp.
        /// </summary>
        /// <value>The timestamp of the entity.</value>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the entity's current ETag.  Set this value to '*' in order to blindly overwrite an entity as part of an update operation.
        /// </summary>
        /// <value>The ETag of the entity.</value>
        public string ETag { get; set; }

        private EntityMapperSet<T> MapperSet
        {
            get
            {
                if (this.mapperSet == null)
                {
                    EntityMapperBase tempBase;
                    if (MapperDictionary.TryGetValue(typeof(T), out tempBase))
                    {
                        this.mapperSet = tempBase as EntityMapperSet<T>;
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("No suitable mapper set could be located for type {0}", typeof(T).FullName));
                    }
                }

                return this.mapperSet;
            }
        }

        /// <summary>
        /// Deserializes a custom entity instance using the specified System.Collections.Generic.IDictionary`2 of property names to
        /// Microsoft.WindowsAzure.Storage.Table.EntityProperty data typed values.
        /// </summary>
        /// <param name="properties">properties</param>
        /// <param name="operationContext">operationContext</param>
        public virtual void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            TableEntity.ReadUserObject(this.InnerObject, properties, operationContext);
        }

        /// <summary>
        /// Create a System.Collections.Generic.IDictionary`2 of Microsoft.WindowsAzure.Storage.Table.EntityProperty
        /// objects for all the properties of the specified entity object.
        /// </summary>
        /// <param name="operationContext">operationContext</param>
        /// <returns>Dictionary</returns>
        public virtual IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return TableEntity.WriteUserObject(this.InnerObject, operationContext);
        }
    }
}
