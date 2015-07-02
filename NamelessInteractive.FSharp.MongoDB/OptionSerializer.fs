namespace NamelessInteractive.FSharp.MongoDB.Serializers

open Microsoft.FSharp.Reflection
open MongoDB.Bson
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers
open NamelessInteractive.FSharp.MongoDB

type OptionSerializer<'ElemType when 'ElemType: equality>() =
    inherit SerializerBase<'ElemType option>()

    let Cases = GetUnionCases typeof<'ElemType option>

    override this.Serialize(context, args, value) =
        match value with
        | None -> BsonSerializer.Serialize(context.Writer, null)            
        | Some x -> BsonSerializer.Serialize(context.Writer, x)

    override this.Deserialize(context, args) =
        let genericTypeArgument = typeof<'ElemType>
        
        let (case, args) =
                let value = if (genericTypeArgument.IsPrimitive) then
                                BsonSerializer.Deserialize(context.Reader, typeof<obj>)        
                            else
                                BsonSerializer.Deserialize(context.Reader, genericTypeArgument)
                match value with
                | null -> (Cases.["None"], [||])
                | _ -> (Cases.["Some"], [| value |])
        FSharpValue.MakeUnion(case, args) :?> 'ElemType option
