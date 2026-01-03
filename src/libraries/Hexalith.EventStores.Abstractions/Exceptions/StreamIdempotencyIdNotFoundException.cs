// <copyright file="StreamIdempotencyIdNotFoundException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Streams.Exceptions;

using System;

/// <summary>
/// Exception thrown when a stream with the specified idempotency ID is not found.
/// </summary>
public sealed class StreamIdempotencyIdNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamIdempotencyIdNotFoundException"/> class.
    /// </summary>
    /// <param name="streamId">The stream identifier.</param>
    /// <param name="idempotencyId">The idempotency identifier.</param>
    public StreamIdempotencyIdNotFoundException(string streamId, string idempotencyId)
        : base($"Stream '{streamId}' item with idempotency ID '{idempotencyId}' not found.")
    {
        IdempotencyId = idempotencyId;
        StreamId = streamId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamIdempotencyIdNotFoundException"/> class.
    /// </summary>
    public StreamIdempotencyIdNotFoundException()
        : base()
    {
        IdempotencyId = string.Empty;
        StreamId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamIdempotencyIdNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public StreamIdempotencyIdNotFoundException(string? message)
        : base(message)
    {
        IdempotencyId = string.Empty;
        StreamId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamIdempotencyIdNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public StreamIdempotencyIdNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
        IdempotencyId = string.Empty;
        StreamId = string.Empty;
    }

    /// <summary>
    /// Gets the idempotency identifier.
    /// </summary>
    public string IdempotencyId { get; }

    /// <summary>
    /// Gets the stream identifier.
    /// </summary>
    public string StreamId { get; }
}