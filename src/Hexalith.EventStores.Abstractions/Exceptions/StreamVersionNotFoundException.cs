// <copyright file="StreamVersionNotFoundException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Streams.Exceptions;

using System;

/// <summary>
/// Exception thrown when a stream with the specified version is not found.
/// </summary>
public sealed class StreamVersionNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamVersionNotFoundException"/> class.
    /// </summary>
    /// <param name="streamId">The stream identifier.</param>
    /// <param name="version">The version of the stream.</param>
    public StreamVersionNotFoundException(string streamId, long version)
        : base($"Stream '{streamId}' with version '{version}' not found.")
    {
        StreamId = streamId;
        Version = version;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamVersionNotFoundException"/> class.
    /// </summary>
    public StreamVersionNotFoundException()
        : base() => StreamId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamVersionNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public StreamVersionNotFoundException(string? message)
        : base(message) => StreamId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamVersionNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public StreamVersionNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) => StreamId = string.Empty;

    /// <summary>
    /// Gets the stream identifier.
    /// </summary>
    public string StreamId { get; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    public long Version { get; }
}