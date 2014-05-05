namespace NamelessInteractive.FSharp.MongoDB.Serializers

open Microsoft.FSharp.Reflection
open MongoDB.Bson
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers
open NamelessInteractive.FSharp.MongoDB

type OptionSerializer(objType) =
    inherit BsonBaseSerializer()

    let Cases = GetUnionCases objType

    override this.Serialize(writer, nominalType, value, options) =
        if (value <> null) then
            let v2 = objType.GetProperty("Value").GetValue(value, [| |]) |> Some

            match unbox v2 with
            | None -> BsonSerializer.Serialize(writer,typeof<obj>, null, options)            
            | Some x -> BsonSerializer.Serialize(writer,x.GetType(), x, options)
        else
            BsonSerializer.Serialize(writer,typeof<obj>, null, options)

    override this.Deserialize(reader, nominalType, actualType, options) =
        let genericTypeArgument = objType.GenericTypeArguments.[0]
        
        let (case, args) =
                let value = if (genericTypeArgument.IsPrimitive) then
                                BsonSerializer.Deserialize(reader, typeof<obj>, options)        
                            else
                                BsonSerializer.Deserialize(reader, genericTypeArgument, options)
                match value with
                | null -> (Cases.["None"], [||])
                | _ -> (Cases.["Some"], [| value |])
        FSharpValue.MakeUnion(case, args)
