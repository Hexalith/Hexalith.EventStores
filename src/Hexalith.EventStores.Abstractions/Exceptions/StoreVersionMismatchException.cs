// <copyright file="StoreVersionMismatchException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Exceptions;

using System;

using Hexalith.Commons.Metadatas;

/// <summary>
/// Exception thrown when the stream version does not match the added item version.
/// </summary>
public sealed class StoreVersionMismatchException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoreVersionMismatchException"/> class.
    /// </summary>
    /// <param name="metadata">The message metadata.</param>
    /// <param name="expectedVersion">The expected version of the stream.</param>
    public StoreVersionMismatchException(Metadata metadata, long expectedVersion)
        : base($"The added item version {metadata.Context.SequenceNumber} does not match the Stream '{metadata.Message.Domain.Name}/{metadata.Message.Domain.Id}' expected version '{expectedVersion}'.")
    {
        Metadata = metadata;
        ExpectedVersion = expectedVersion;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreVersionMismatchException"/> class.
    /// </summary>
    public StoreVersionMismatchException()
        : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreVersionMismatchException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public StoreVersionMismatchException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreVersionMismatchException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public StoreVersionMismatchException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Gets the stream next item expected version.
    /// </summary>
    public long ExpectedVersion { get; }

    /// <summary>
    /// Gets the message metadata.
    /// </summary>
    public Metadata? Metadata { get; }
}