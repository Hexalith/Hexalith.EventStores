// <copyright file="InvalidStoreSessionException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Exceptions;

using System;

/// <summary>
/// Exception thrown when a store session identifier is invalid.
/// </summary>
public class InvalidStoreSessionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStoreSessionException"/> class.
    /// </summary>
    /// <param name="storeId">The ID of the store whose session has expired.</param>
    /// <param name="sessionId">The ID of the session that has expired.</param>
    /// <param name="expectedSession">The expected session ID.</param>
    public InvalidStoreSessionException(
        string storeId,
        string sessionId,
        string expectedSession)
        : base($"The session '{sessionId}' for store '{storeId}' is invalid. Expected session '{expectedSession}'. It may have been closed or expired.")
    {
        StoreId = storeId;
        ExpectedSession = expectedSession;
        SessionId = sessionId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStoreSessionException"/> class.
    /// </summary>
    public InvalidStoreSessionException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStoreSessionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidStoreSessionException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStoreSessionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidStoreSessionException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Gets the expected session ID.
    /// </summary>
    public string? ExpectedSession { get; }

    /// <summary>
    /// Gets the ID of the session that has expired.
    /// </summary>
    public string? SessionId { get; }

    /// <summary>
    /// Gets the ID of the store whose session has expired.
    /// </summary>
    public string? StoreId { get; }
}