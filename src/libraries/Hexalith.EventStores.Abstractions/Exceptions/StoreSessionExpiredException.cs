// <copyright file="StoreSessionExpiredException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Exceptions;

using System;

/// <summary>
/// Exception thrown when a store session has expired.
/// </summary>
public class StoreSessionExpiredException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoreSessionExpiredException"/> class.
    /// </summary>
    /// <param name="storeId">The ID of the store whose session has expired.</param>
    /// <param name="sessionId">The ID of the session that has expired.</param>
    /// <param name="expiration">The expiration date and time of the session.</param>
    public StoreSessionExpiredException(
        string storeId,
        string sessionId,
        DateTimeOffset expiration)
        : base($"The session '{sessionId}' for store '{storeId}' has expired at {expiration}. Please refresh the session or set a longuer expiration time.")
    {
        StoreId = storeId;
        Expiration = expiration;
        SessionId = sessionId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreSessionExpiredException"/> class.
    /// </summary>
    public StoreSessionExpiredException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreSessionExpiredException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public StoreSessionExpiredException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreSessionExpiredException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public StoreSessionExpiredException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Gets the expiration date and time of the session.
    /// </summary>
    public DateTimeOffset? Expiration { get; }

    /// <summary>
    /// Gets the ID of the session that has expired.
    /// </summary>
    public string? SessionId { get; }

    /// <summary>
    /// Gets the ID of the store whose session has expired.
    /// </summary>
    public string? StoreId { get; }
}