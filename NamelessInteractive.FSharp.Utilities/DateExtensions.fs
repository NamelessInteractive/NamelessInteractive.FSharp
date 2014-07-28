module NamelessInteractive.FSharp.Utilities.DateExtensions 

type System.DateTime with
    member this.IsWeekend 
        with get() : bool = 
            match this.DayOfWeek with
            | System.DayOfWeek.Saturday
            | System.DayOfWeek.Sunday -> true
            | _ -> false
    member this.ToISODateString 
        with get() : string =
            this.ToString("yyyy-MM-dd")