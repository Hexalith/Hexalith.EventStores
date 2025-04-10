// <copyright file="IEventStoreProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

using Hexalith.Commons.Metadatas;

/// <summary>
/// Event store interface.
/// </summary>
public interface IEventStoreProvider
{
    /// <summary>
    /// Gets an event stream. If the stream does not exist, it is created.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="name">The stream name.</param>
    /// <param name="id">The stream identifier.</param>
    /// <returns>The event stream.</returns>
    IEventStore GetStore(string database, string name, string id);

    /// <summary>
    /// Gets an event stream. If the stream does not exist, it is created.
    /// </summary>
    /// <param name="name">The stream name.</param>
    /// <param name="id">The stream identifier.</param>
    /// <returns>The event stream.</returns>
    IEventStore GetStore(string name, string id);

    /// <summary>
    /// Gets an event stream. If the stream does not exist, it is created.
    /// </summary>
    /// <param name="metadata">The metadata.</param>
    /// <returns>The event stream.</returns>
    /// <remarks>
    /// The database name is the partition identifier.
    /// The container name is the domain name.
    /// The id is the domain identifier.
    /// </remarks>
    IEventStore GetStore(Metadata metadata);
}