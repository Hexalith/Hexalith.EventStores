// <copyright file="EventStoreSettings.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Configurations;

using System;

using Hexalith.Commons.Configurations;

/// <summary>
/// Represents the settings for the event store configuration.
/// </summary>
public class EventStoreSettings : ISettings
{
    /// <summary>
    /// Gets or sets the default database name for the event store.
    /// </summary>
    public string DefaultDatabase { get; set; } = nameof(EventStores);

    /// <summary>
    /// Gets or sets the default timeout for opening the event store.
    /// </summary>
    public TimeSpan DefaultOpenTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the default session timeout for the event store.
    /// </summary>
    public TimeSpan DefaultSessionTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets the configuration name for the event store settings.
    /// </summary>
    /// <returns>The configuration name as a string.</returns>
    public static string ConfigurationName() => $"{nameof(Hexalith)}:{nameof(EventStores)}";
}