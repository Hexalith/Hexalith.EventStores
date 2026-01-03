// <copyright file="TestMessage.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Tests;

using System.Runtime.Serialization;

using Hexalith.PolymorphicSerializations;

/// <summary>
/// Represents a test message.
/// </summary>
/// <param name="Id">The unique identifier of the test message.</param>
/// <param name="Value">The value associated with the test message.</param>
/// /// <param name="IsSnapshot">Indicates whether the message is a snapshot.</param>
[PolymorphicSerialization]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "Need for generated code")]
public partial record TestMessage(
    [property: DataMember] string Id,
    [property: DataMember] string Value,
    [property: DataMember] bool IsSnapshot)
{
    /// <summary>
    /// Gets the aggregate identifier for the test message.
    /// </summary>
    public string DomainId => Id;

    /// <summary>
    /// Gets the name of the aggregate associated with the test message.
    /// </summary>
    public static string DomainName => "EventStoreTest";
}