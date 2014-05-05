namespace NamelessInteractive.FSharp.MongoDB.Serializers

open System.Collections.Generic
open NamelessInteractive.FSharp.MongoDB
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

type MapSerializer<'Key, 'Value when 'Key: comparison>() =
    inherit BsonBaseSerializer()

    let serializer = DictionarySerializer<'Key, 'Value>()

    override this.Serialize(writer, nominalType, value, options) =
        let dictValue =
            value
            :?> Map<'Key, 'Value>
            |> Map.toSeq
            |> dict

        serializer.Serialize(writer,typeof<IDictionary<'Key, 'Value>>, dictValue, options)

    override this.Deserialize(reader, nominalType, actualType, options) =
        serializer.Deserialize(reader,typeof<IDictionary<'Key,'Value>>, options) 
        :?> IDictionary<'Key,'Value>
        |> Seq.map(|KeyValue|)
        |> Map.ofSeq<'Key,'Value>
        |> box