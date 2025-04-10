// <copyright file="BankAccountEvent.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Example;

using System.Runtime.Serialization;

using Hexalith.PolymorphicSerializations;

/// <summary>
/// Represents an event related to a bank account.
/// </summary>
/// <param name="Id">The unique identifier of the bank account event.</param>
[PolymorphicSerialization]
public partial record BankAccountEvent([property: DataMember] string Id)
{
    /// <summary>
    /// Gets the aggregate identifier for the bank account event.
    /// </summary>
    public string AggregateId => Id;

    /// <summary>
    /// Gets the name of the aggregate associated with the bank account event.
    /// </summary>
    public static string AggregateName => "BankAccount";
}