// <copyright file="IEventStoreProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

/// <summary>
/// Event store interface.
/// </summary>
public interface IEventStoreProvider
{
    /// <summary>
    /// Gets an event stream. If the stream does not exist, it is created.
    /// </summary>
    /// <param name="name">The stream name.</param>
    /// <param name="id">The stream identifier.</param>
    /// <returns>The event stream.</returns>
    IEventStore GetStream(string name, string id);
}