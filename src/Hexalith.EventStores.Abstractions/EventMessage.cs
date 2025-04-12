// <copyright file="EventMessage.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

using System.Runtime.Serialization;

using Hexalith.Commons.Metadatas;
using Hexalith.PolymorphicSerializations;

/// <summary>
/// Represents an object in a stream store.
/// </summary>
/// <param name="Event">The event associated with the object.</param>
/// <param name="Metadata">The metadata associated with the object.</param>
[DataContract]
public sealed record EventMessage(
    [property: DataMember(Order = 1)] Polymorphic Event,
    [property: DataMember(Order = 2)] Metadata Metadata);