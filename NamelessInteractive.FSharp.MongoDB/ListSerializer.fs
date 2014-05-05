namespace NamelessInteractive.FSharp.MongoDB.Serializers

open System.Collections.Generic
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

type ListSerializer<'ElemType>() = 
    inherit BsonBaseSerializer()
    
    let serializer = EnumerableSerializer<'ElemType>()

    override this.Serialize(writer, nominalType, value, options) =
        serializer.Serialize(writer, typeof<IEnumerable<'ElemType>>, value, options)

    override this.Deserialize(reader, nominalType, actualType, options) =
        let res = serializer.Deserialize(reader, typeof<IEnumerable<'ElemType>>, options)
        res |> unbox |> List.ofSeq<'ElemType> |> box