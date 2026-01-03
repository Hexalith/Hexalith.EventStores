// <copyright file="OpenStoreFailedException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Exceptions;

using System;

/// <summary>
/// Exception that is thrown when opening a session for a store fails.
/// </summary>
public class OpenStoreFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenStoreFailedException"/> class.
    /// </summary>
    /// <param name="storeId">The ID of the store whose session has failed to open.</param>
    /// <param name="timeout">The timeout duration for the session opening.</param>
    public OpenStoreFailedException(string storeId, TimeSpan timeout)
        : base($"Open session for store '{storeId}' with timeout {timeout} failed.")
    {
        StoreId = storeId;
        Timeout = timeout;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenStoreFailedException"/> class.
    /// </summary>
    public OpenStoreFailedException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenStoreFailedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public OpenStoreFailedException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenStoreFailedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public OpenStoreFailedException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Gets the ID of the store whose session has failed to open.
    /// </summary>
    public string? StoreId { get; }

    /// <summary>
    /// Gets the timeout duration for the session opening.
    /// </summary>
    public TimeSpan? Timeout { get; }
}