namespace NamelessInteractive.FSharp.MongoDB.Serializers

open System.Collections.Generic
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

type ListSerializer<'ElemType>() = 
    inherit SerializerBase<'ElemType list>()
    
    let serializer = EnumerableInterfaceImplementerSerializer<List<'ElemType>, 'ElemType>()

    override this.Serialize(context, args, value) =
        serializer.Serialize(context, args, new List<'ElemType>(value))

    override this.Deserialize(context, args) =
        let res = serializer.Deserialize(context, args)
        res |> unbox |> List.ofSeq<'ElemType>