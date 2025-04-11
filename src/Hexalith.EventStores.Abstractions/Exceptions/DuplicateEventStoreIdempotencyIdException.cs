// <copyright file="DuplicateEventStoreIdempotencyIdException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Exceptions;

using System;

using Hexalith.EventStores;

/// <summary>
/// Exception thrown when a stream with the specified idempotency ID is not found.
/// </summary>
public sealed class DuplicateEventStoreIdempotencyIdException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateEventStoreIdempotencyIdException"/> class.
    /// </summary>
    /// <param name="ev">The event message.</param>
    public DuplicateEventStoreIdempotencyIdException(EventMessage ev)
        : base($"Stream '{ev.Metadata.Context.PartitionId}/{ev.Metadata.Message.Domain.Name}/{ev.Metadata.Message.Domain.Id}' item already contains an item with idempotency ID '{ev.Metadata.Message.Id}'.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateEventStoreIdempotencyIdException"/> class.
    /// </summary>
    public DuplicateEventStoreIdempotencyIdException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateEventStoreIdempotencyIdException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DuplicateEventStoreIdempotencyIdException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateEventStoreIdempotencyIdException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DuplicateEventStoreIdempotencyIdException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the idempotency identifier.
    /// </summary>
    public EventMessage? Event { get; }
}