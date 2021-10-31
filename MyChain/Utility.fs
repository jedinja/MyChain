namespace MyChain

open System
open System.Security.Cryptography
open System.Text
open Newtonsoft.Json
open Suave

module Utility =
    
    let jsonSerialize obj = JsonConvert.SerializeObject obj
    let jsonDeserialize<'a> str = JsonConvert.DeserializeObject<'a> str
    
    let _hash<'a> =
        jsonSerialize
        >> Encoding.UTF8.GetBytes
        >> (new SHA256Managed()).ComputeHash
        >> Array.map (fun b -> b.ToString("x2"))
        >> String.concat ""
        
    let startsWith (prefix: string) (str: string) = str.StartsWith(prefix)
    
    let unixTime () = DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()
    
    type JsonWebPart = obj -> WebPart
    let JSON :JsonWebPart = jsonSerialize >> Successful.OK
    
    let requestBody (req : HttpRequest) =
        let getString (rawForm: byte []) = System.Text.Encoding.UTF8.GetString(rawForm)
        req.rawForm
        |> getString

    let logOne<'a> (obj: 'a) =
        printfn "%A" obj
        obj
        
    let logOneName<'a> name (obj: 'a) =
        printfn "%s" name
        logOne obj
        
    