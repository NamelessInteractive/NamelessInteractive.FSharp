module NamelessInteractive.FSharp.Utilities.XLinqExtensions

open System.Xml.Linq

let AsXName = XName.Get

type XElement with
    member this.Descendants(xname: string) =
        this.Descendants(AsXName(xname))
    member this.Elements(xname: string) =
        this.Elements(AsXName(xname))
    member this.Element(xname: string) =
        this.Element(AsXName(xname))
    member this.AttributeValue (attributeName) =
        let attribute = this.Attribute(AsXName(attributeName))
        match attribute with
        | null -> None
        | _ -> Some attribute.Value
    member this.AttributeValueOrDefault (attributeName, ?defaultValue: string)  =
        let defaultVal = defaultArg defaultValue System.String.Empty
        match this.AttributeValue(attributeName) with
        | None -> defaultVal
        | Some (value) -> value
    member this.AttributeValueNotNull (attributeName) =
        match this.AttributeValue (attributeName) with
        | None -> failwith (sprintf "Mandatory Value %s was null" attributeName)
        | Some value -> value
    member this.SetAttributeValue(xname: string, value: #obj) =
        this.SetAttributeValue(AsXName(xname), value)
    member this.Field(childName) = 
        if (childName = System.String.Empty) then
            Some this
        else
            let childField = this.Elements(childName)
            match childField |> Seq.length with
            | 0 -> None
            | 1 -> childField |> Seq.head |> Some
            | _ -> None
    member this.FieldValue(childName) =
        if (childName = System.String.Empty) then
            this.Value |> Some
        else
            let childField = this.Elements(childName)
            match childField |> Seq.length with
            | 0 -> None
            | 1 -> childField |> Seq.map (fun f -> f.Value) |> Seq.head |> Some
            | _ -> None
    member this.FieldValueNotNull(childName) =
        match this.FieldValue(childName) with
        | None -> failwith (sprintf "Mandatory Field '%s' was not found" childName)
        | Some value -> value
    member this.FieldValueOrDefault (childName, ?defaultValue) =
        let defaultVal = defaultArg defaultValue System.String.Empty
        match this.FieldValue(childName) with
        | None -> defaultVal
        | Some value -> value
    member this.ReadType<'T> fieldName (parseFunc: XElement -> 'T) =
        let value = this.Field(fieldName)
        match value with
        | None -> None
        | Some value -> parseFunc (value) |> Some
    
