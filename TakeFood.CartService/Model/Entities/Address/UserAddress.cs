﻿using MongoDB.Bson.Serialization.Attributes;

namespace CartService.Model.Entities.Address;

public class UserAddress : ModelMongoDB
{
    /// <summary>
    /// UserId
    /// </summary>
    [BsonElement("userId")]
    public string UserId;
    /// <summary>
    /// AddressId
    /// </summary>
    [BsonElement("addressId")]
    public string AddressId;
}
