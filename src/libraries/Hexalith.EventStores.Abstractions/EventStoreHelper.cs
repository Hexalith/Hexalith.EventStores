// <copyright file="EventStoreHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

using System;
using System.Diagnostics.CodeAnalysis;

using Hexalith.Commons.Metadatas;
using Hexalith.Commons.UniqueIds;
using Hexalith.PolymorphicSerializations;

/// <summary>
/// Provides helper methods for creating event messages and metadata for event stores.
/// </summary>
public static class EventStoreHelper
{
    /// <summary>
    /// Creates an <see cref="EventMessage"/> from the specified event, message metadata, and context metadata.
    /// </summary>
    /// <param name="messageEvent">The polymorphic event to include in the message.</param>
    /// <param name="message">The metadata of the message.</param>
    /// <param name="context">The context metadata of the message.</param>
    /// <returns>A new <see cref="EventMessage"/> instance.</returns>
    public static EventMessage CreateMessage(this Polymorphic messageEvent, MessageMetadata message, ContextMetadata context)
    {
        Metadata metadata = new(message, context);

        return new EventMessage(messageEvent, metadata);
    }

    /// <summary>
    /// Creates an <see cref="EventMessage"/> with optional parameters for sequence number, user ID, partition ID, and time provider.
    /// </summary>
    /// <param name="messageEvent">The polymorphic event to include in the message.</param>
    /// <param name="sequenceNumber">The optional sequence number of the message.</param>
    /// <param name="userId">The user ID associated with the message. Defaults to "system".</param>
    /// <param name="partitionId">The partition ID associated with the message. Defaults to "default".</param>
    /// <param name="timeProvider">The optional time provider for generating timestamps. Defaults to <see cref="TimeProvider.System"/>.</param>
    /// <returns>A new <see cref="EventMessage"/> instance.</returns>
    public static EventMessage CreateMessage(
        this Polymorphic messageEvent,
        long? sequenceNumber = null,
        string userId = "system",
        string partitionId = "default",
        TimeProvider? timeProvider = null)
    {
        MessageMetadata message = messageEvent.CreateMessageMetadata(timeProvider);
        ContextMetadata context = new(
            message.Id,
            userId,
            partitionId,
            message.CreatedDate,
            null,
            sequenceNumber,
            null,
            null,
            []);
        return new EventMessage(
            messageEvent,
            new Metadata(
                message,
                context));
    }

    /// <summary>
    /// Creates a <see cref="MessageMetadata"/> instance for the specified message.
    /// </summary>
    /// <param name="message">The message object to generate metadata for.</param>
    /// <param name="timeProvider">The optional time provider for generating timestamps. Defaults to <see cref="TimeProvider.System"/>.</param>
    /// <returns>A new <see cref="MessageMetadata"/> instance.</returns>
    public static MessageMetadata CreateMessageMetadata([NotNull] this object message, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(message);
        timeProvider ??= TimeProvider.System;
        (string name, string _, int version) = message.GetType().GetPolymorphicTypeDiscriminator();

        return new MessageMetadata(
            UniqueIdHelper.GenerateUniqueStringId(),
            name,
            version,
            message.CreateDomainMetadata(),
            timeProvider.GetLocalNow());
    }
}