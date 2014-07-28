module NamelessInteractive.FSharp.Utilities.StringExtensions

let private EmptyString = System.String.Empty

let private IsNullOrEmpty (strVal: string) =
    System.String.IsNullOrEmpty(strVal) 

let private StringCaster (strVal: string) defaultValue (castFun) = 
    match IsNullOrEmpty (strVal) with
    | true -> defaultValue
    | false -> 
        match castFun (strVal) with
        | false, _ -> defaultValue
        | true, value -> value
         

let private IsNumeric (strVal: string) =
    match IsNullOrEmpty(strVal) with
    | true -> false
    | false -> 
        match System.Double.TryParse(strVal) with
        | false, _ -> false
        | true, _ -> true
               
    

type System.String with
    member this.IsNullOrEmpty() = 
            System.String.IsNullOrEmpty(this)
    member this.IsNumeric() =
        IsNumeric this
    member this.ToDouble() =
        StringCaster 
            this
            0.0 
            (fun str -> System.Double.TryParse(str))
    member this.ToInt32() =
        StringCaster
            this
            0
            (fun str -> System.Int32.TryParse(str))
    member this.StripNonNumeric() =
        System.Text.RegularExpressions.Regex.Replace(this, "[^0-9.]", EmptyString)
