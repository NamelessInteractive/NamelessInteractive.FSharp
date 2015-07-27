namespace NamelessInteractive.FSharp.MongoDB.Serializers

open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

type RecordSerializer<'TRecord>() = 
    inherit SerializerBase<'TRecord>()
    let classMap = BsonClassMap.LookupClassMap(typeof<'TRecord>)
    let serializer = BsonClassMapSerializer(classMap)

    override this.Serialize(context, args, value) =
        let mutable nargs = args
        nargs.NominalType <- typeof<'TRecord>
        serializer.Serialize(context, nargs, value)

    override this.Deserialize(context, args) =
        let mutable nargs = args
        nargs.NominalType <- typeof<'TRecord> 
        serializer.Deserialize(context, nargs)