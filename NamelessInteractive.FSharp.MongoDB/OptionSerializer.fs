namespace NamelessInteractive.FSharp.MongoDB.Serializers

open Microsoft.FSharp.Reflection
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers
open NamelessInteractive.FSharp.MongoDB

type OptionSerializer(objType) =
    inherit BsonBaseSerializer()

    let Cases = GetUnionCases objType

    override this.Serialize(writer, nominalType, value, options) =
        let value = objType.GetProperty("Value").GetValue(value, [| |]) |> Some

        match unbox value with
        | Some x -> BsonSerializer.Serialize(writer,x.GetType(), x, options)
        | None -> BsonSerializer.Serialize(writer,typeof<obj>, null, options)

    override this.Deserialize(reader, nominalType, actualType, options) =
        let value = BsonSerializer.Deserialize(reader, objType.GenericTypeArguments.[0], options)

        let (case, args) =
            match value with
            | null -> (Cases.["None"], [||])
            | _ -> (Cases.["Some"], [| value |])
        FSharpValue.MakeUnion(case, args)
