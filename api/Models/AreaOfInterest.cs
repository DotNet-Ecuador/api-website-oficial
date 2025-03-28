﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace api.Models;

public class AreaOfInterest
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; set; } = string.Empty;

	[BsonElement("Name")]
	public string Name { get; set; } = string.Empty;

	[BsonElement("Description")]
	public string Description { get; set; } = string.Empty;
}
