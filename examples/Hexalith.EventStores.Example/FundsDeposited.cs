// <copyright file="FundsDeposited.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Example;

using System.Runtime.Serialization;

using Hexalith.PolymorphicSerializations;

/// <summary>
/// Represents an event where funds are deposited into a bank account.
/// </summary>
/// <param name="Id">The unique identifier of the bank account.</param>
/// <param name="Amount">The amount of funds deposited.</param>
/// <param name="Date">The date and time of the deposit.</param>
[PolymorphicSerialization]
public partial record FundsDeposited(
    string Id,
    [property: DataMember] decimal Amount,
    [property: DataMember] DateTimeOffset Date) : BankAccountEvent(Id);