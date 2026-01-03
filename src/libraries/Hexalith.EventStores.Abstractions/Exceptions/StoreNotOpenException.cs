// <copyright file="StoreNotOpenException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Exceptions;

using System;

/// <summary>
/// Exception that is thrown when a store is not open.
/// </summary>
public class StoreNotOpenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoreNotOpenException"/> class.
    /// </summary>
    /// <param name="storeId">The ID of the store whose session has failed to open.</param>
    /// <param name="sessionId">The ID of the session that has failed to open.</param>
    public StoreNotOpenException(string storeId, string? sessionId)
        : base($"Store '{storeId}' is not open." +
            (string.IsNullOrWhiteSpace(sessionId)
                ? string.Empty
                : $" Session : {sessionId}."))
    {
        StoreId = storeId;
        SessionId = sessionId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreNotOpenException"/> class.
    /// </summary>
    public StoreNotOpenException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreNotOpenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public StoreNotOpenException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreNotOpenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public StoreNotOpenException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Gets the session ID of the store whose session has failed to open.
    /// </summary>
    public string? SessionId { get; }

    /// <summary>
    /// Gets the ID of the store whose session has failed to open.
    /// </summary>
    public string? StoreId { get; }
}