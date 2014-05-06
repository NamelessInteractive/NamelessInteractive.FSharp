namespace NamelessInteractive.FSharp.MongoDB.Serializers

open System.Collections.Generic
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

type RecordSerializer(objType) = 
    inherit BsonBaseSerializer()
    let classMap = BsonClassMap.LookupClassMap(objType)
    let classMapSerializer = BsonClassMapSerializer(classMap)

    override this.Serialize(writer, nominalType, value, options) =
        classMapSerializer.Serialize(writer,nominalType,value,options)

    override this.Deserialize(reader, nominalType, actualType, options) =
        classMapSerializer.Deserialize(reader,nominalType,actualType,options)
