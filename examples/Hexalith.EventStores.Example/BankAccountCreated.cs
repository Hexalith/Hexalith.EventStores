// <copyright file="BankAccountCreated.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Example;

using System.Runtime.Serialization;

using Hexalith.PolymorphicSerializations;

/// <summary>
/// Event representing the creation of a bank account.
/// </summary>
/// <param name="Id">The unique identifier of the bank account.</param>
/// <param name="Name">The name associated with the bank account.</param>
/// <param name="Currency">The currency of the bank account.</param>
[PolymorphicSerialization]
public partial record BankAccountCreated(
    string Id,
    [property: DataMember] string Name,
    [property: DataMember] string Currency) : BankAccountEvent(Id);