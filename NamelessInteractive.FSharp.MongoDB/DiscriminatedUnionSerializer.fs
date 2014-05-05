namespace NamelessInteractive.FSharp.MongoDB.Serializers

open NamelessInteractive.FSharp.MongoDB
open Microsoft.FSharp.Reflection
open System
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers

type DiscriminatedUnionSerializer(objType: Type) =
    inherit BsonBaseSerializer()
    let CaseNameField = "Case"
    let ValueFieldName = "Fields"
    let Cases = GetUnionCases objType
    let ReadItems (reader) (types) (options) =
        types 
        |> Seq.fold(
            fun state t ->
                let serializer = BsonSerializer.LookupSerializer(t)
                let item = serializer.Deserialize(reader,t, options)
                item::state
            ) []
        |> Seq.toArray
        |> Array.rev

    override this.Serialize(writer, nominalType, value, options) =
        let (case, fields) = FSharpValue.GetUnionFields(value,objType)
        writer.WriteStartDocument()
        writer.WriteString(CaseNameField, case.Name)
        writer.WriteStartArray(ValueFieldName)
        fields 
        |> Seq.zip(case.GetFields()) 
        |> Seq.iter(fun (field, value) -> 
            let itemSerializer = BsonSerializer.LookupSerializer(field.PropertyType)
            itemSerializer.Serialize(writer,field.PropertyType, value, options)
        )
        writer.WriteEndArray()
        writer.WriteEndDocument()

    override this.Deserialize(reader, nominalType, actualType, options) =
        reader.ReadStartDocument()
        let name = reader.ReadString(CaseNameField)
        let union = Cases.[name]
        reader.ReadStartArray()
        let items = ReadItems (reader) (union.GetFields() |> Seq.map(fun f->f.PropertyType)) options
        reader.ReadEndArray()
        reader.ReadEndDocument()
        FSharpValue.MakeUnion(union, items)
