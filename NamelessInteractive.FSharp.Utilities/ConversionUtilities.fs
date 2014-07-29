module NamelessInteractive.FSharp.Utilities.ConversionUtilities

open NamelessInteractive.FSharp.Utilities.Maybe

let InvariantCulture = System.Globalization.CultureInfo.InvariantCulture

let As<'T> (parseFunc : string -> 'T) (value: string option)  =
    maybe {
        let! x = value
        return parseFunc(x)
    }

let AShort (value: string) =
    int16 value

let ALong (value: string) =
    int64 value

let ADecimal (value: string) =
    System.Convert.ToDecimal(value, InvariantCulture)

let ADate (value: string) =
    System.DateTime.Parse(value, InvariantCulture)