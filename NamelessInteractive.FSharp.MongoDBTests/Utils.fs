[<AutoOpen>]
module Utils

open System
open System.Linq.Expressions
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

/// <summary>
/// Converts a lambda quotation into a lambda expression
/// </summary>
/// <param name="expr">Quotation to convert</param>
let propertyEx (expr: Expr<'a -> 'b>): Expression<Func<'a, obj>> =
    match expr with
    | Lambda(_, body) ->
        match body with
        | PropertyGet(_, pinfo, _) ->
            let arg = Expression.Parameter(typeof<'a>, "arg")
            let getter = Expression.Property(arg, pinfo) :> Expression
            let convert = Expression.Convert(getter, typeof<obj>)
            Expression.Lambda<Func<'a, obj>>(convert, arg)
        | _ -> failwith "getProperty translator accepts only quotations with property getters"
    | _ -> failwith "Quotation does not contain lambda function"