namespace NamelessInteractive.FSharp.MongoDB.Serializers

open System.Collections.Generic
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

type SetSerializer<'Type when 'Type: comparison>() =
    inherit BsonBaseSerializer()

    let serializer = EnumerableSerializer<'Type>()

    override this.Serialize(writer,nominalType, value, options) =
        serializer.Serialize(writer,typeof<IEnumerable<'Type>>, value, options)

    override this.Deserialize(reader,nominalType,actualType,options) =
        let res = serializer.Deserialize(reader,typeof<IEnumerable<'Type>>, options)
        res |> unbox |> Set.ofSeq<'Type> |> box

