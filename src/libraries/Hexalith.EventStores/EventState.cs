// <copyright file="EventState.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

using Hexalith.KeyValueStorages;

/// <summary>
/// Represents the state of an event in the event store.
/// </summary>
/// <param name="Value">The event message associated with the state.</param>
public record EventState(EventMessage Value) : State<EventMessage>(Value, null, null);