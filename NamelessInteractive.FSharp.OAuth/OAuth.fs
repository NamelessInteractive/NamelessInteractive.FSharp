namespace NamelessInteractive.FSharp.OAuth

[<AutoOpen>]
module Shared = 

    open System
    open System.Text
    open System.Text.RegularExpressions

    type HttpRequestType =  
        | Delete
        | Get
        | Post

    let private httpMethodToString httpMethod = 
        match httpMethod with 
        | Delete -> "DELETE"
        | Get -> "GET"
        | Post -> "POST"

    let private InvariantCulture = System.Globalization.CultureInfo.InvariantCulture
    let private UTF8 = System.Text.Encoding.UTF8

    let private MapCat a b = 
        Map(Seq.concat [Map.toSeq a; Map.toSeq b])

    let private UrlEncode (str: string) = 
        let whitelistCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~"
        let encoded = str |> Seq.map(fun i -> match whitelistCharacters.Contains(i.ToString()) with
                                              | true -> i.ToString()
                                              | false -> String.Format("%{0:X2}", i |> int))
        String.Join("",encoded)

    let private GetUriParameters (uri: Uri) =
        let queryStringRegex = "(?<varName>[^&?=]+)=(?<value>[^&?=]*)"
        match String.IsNullOrEmpty(uri.Query) with
        | true -> Map.empty<string,string>
        | false -> 
            Regex.Matches(uri.Query,queryStringRegex)
            |> Seq.cast<Match>
            |> Seq.map(fun mat -> mat.Groups.["varName"].Value, Uri.EscapeDataString(mat.Groups.["value"].Value))
            |> Map.ofSeq

    [<Flags>]
    type internal OAuthParameterRequirements = 
        | RequiredForHeader = 1
        | RequiredForSignature = 2
        | IsPartOfSecretKey = 4

    let private hasFlag flag value = 
        (value &&& flag) = flag

    type OAuthCredentials = 
        {
            AccessToken: string
            AccessTokenSecret: string
            ConsumerKey: string
            ConsumerSecret: string
        }

    type internal OAuthQueryParameter = 
        {
            Key: string
            Value: string
            ParameterRequirements: OAuthParameterRequirements
        
        }

    let private GenerateOAuthQueryParameter key value parameterRequirements  = 
        {
            Key = key
            Value = value
            ParameterRequirements = parameterRequirements
        }

    let private GenerateOAuthQueryParameters credentials = 
        [
            GenerateOAuthQueryParameter "oauth_consumer_key" (UrlEncode credentials.ConsumerKey) (OAuthParameterRequirements.RequiredForSignature ||| OAuthParameterRequirements.RequiredForHeader)
            GenerateOAuthQueryParameter "oauth_consumer_secret" (UrlEncode credentials.ConsumerSecret) OAuthParameterRequirements.IsPartOfSecretKey
            GenerateOAuthQueryParameter "oauth_token" (UrlEncode credentials.AccessToken) (OAuthParameterRequirements.RequiredForSignature ||| OAuthParameterRequirements.RequiredForHeader)
            GenerateOAuthQueryParameter "oauth_token_secret" (UrlEncode credentials.AccessTokenSecret) OAuthParameterRequirements.IsPartOfSecretKey
        ]

    let private GenerateOAuthHeaderParameters parameters = 
        let timestamp = ((DateTime.UtcNow - DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc)).TotalSeconds |> int64).ToString(InvariantCulture)
        let nonce = Random().Next(123400,9999999).ToString(InvariantCulture)
        [
            GenerateOAuthQueryParameter "oauth_nonce" nonce (OAuthParameterRequirements.RequiredForSignature ||| OAuthParameterRequirements.RequiredForHeader)
            GenerateOAuthQueryParameter "oauth_timestamp" timestamp (OAuthParameterRequirements.RequiredForSignature ||| OAuthParameterRequirements.RequiredForHeader)
            GenerateOAuthQueryParameter "oauth_version" "1.0" (OAuthParameterRequirements.RequiredForSignature ||| OAuthParameterRequirements.RequiredForHeader)
            GenerateOAuthQueryParameter "oauth_signature_method" "HMAC-SHA1" (OAuthParameterRequirements.RequiredForSignature ||| OAuthParameterRequirements.RequiredForHeader)
        ] @ parameters

    let private GenerateOAuthSignature (uri: Uri) httpMethod queryParameters (urlParameters:Map<string,string>) = 
        let sortedUrlParameters = urlParameters 
        let headerParameters = queryParameters 
                                |> Seq.filter(fun q -> q.ParameterRequirements |> hasFlag (OAuthParameterRequirements.RequiredForSignature) )
                                |> Seq.sortBy(fun q->q.Key) 
                                |> Seq.map(fun q -> q.Key, q.Value)
                                |> Map.ofSeq
        let allParams = MapCat sortedUrlParameters headerParameters |> Seq.sortBy(fun p -> p.Key)
        let builder = StringBuilder()
        allParams |> Seq.iter (fun p -> if builder.Length > 0 then
                                            builder.Append("&") |> ignore
                                        builder.Append((sprintf "%s=%s" p.Key p.Value)) |> ignore
                              )
        let str = match uri.Query with
                  | "" -> uri.AbsoluteUri
                  | _ -> uri.AbsoluteUri.Replace(uri.Query,"")
        let source = sprintf "%s&%s&%s" (httpMethodToString httpMethod) (UrlEncode str) (UrlEncode (builder.ToString()))
        let secretKeyParams = queryParameters 
                              |> Seq.filter(fun p -> p.ParameterRequirements |> hasFlag (OAuthParameterRequirements.IsPartOfSecretKey))
                              |> Seq.sortBy(fun p -> p.Key)
        let keyBuilder = StringBuilder()
        let secretKeysLength = secretKeyParams |> Seq.length
        secretKeyParams 
        |> Seq.iteri(fun i p -> keyBuilder.Append (sprintf "%s%s" (UrlEncode p.Value) (if i = secretKeysLength - 1 then "" else "&") ) |> ignore)
        let key = keyBuilder.ToString()
        use hasher = new System.Security.Cryptography.HMACSHA1(UTF8.GetBytes(key))
        UrlEncode (Convert.ToBase64String(hasher.ComputeHash(UTF8.GetBytes(source))))

    let private GenerateOAuthHeader uri httpMethod queryParameters urlParameters = 
        let signature = GenerateOAuthSignature uri httpMethod queryParameters urlParameters
        let builder = new StringBuilder()
        builder.Append("OAuth ") |> ignore
        queryParameters 
        |> Seq.filter(fun p -> p.ParameterRequirements |> hasFlag (OAuthParameterRequirements.RequiredForHeader))
        |> Seq.iter (fun p -> if builder.Length > 6 then
                                builder.Append(",") |> ignore
                              builder.Append(sprintf """%s="%s" """ p.Key p.Value) |> ignore
                    )
        builder.Append (sprintf """,oauth_signature="%s" """ signature) |> ignore
        builder.ToString()

    let private GenerateOAuthAuthorizationHeader uri httpMethod parameters = 
        let queryParameters = GenerateOAuthHeaderParameters parameters
        let uriParameters = GetUriParameters uri
        GenerateOAuthHeader uri httpMethod queryParameters uriParameters

    type WebRequest = System.Net.HttpWebRequest

    let GenerateOAuthWebRequest url httpMethod credentials = 
        let parameters = GenerateOAuthQueryParameters credentials
        let uri = Uri(url)
        let authHeader=  GenerateOAuthAuthorizationHeader uri httpMethod parameters
        let request = WebRequest.CreateHttp(uri.AbsoluteUri)
        request.Method <- httpMethodToString httpMethod 
        request.Headers.Set(System.Net.HttpRequestHeader.Authorization,authHeader)
        request