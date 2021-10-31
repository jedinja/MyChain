open Suave
    
open Suave.RequestErrors
open Suave.Filters
open Suave.Operators
open Suave.Logging

open MyChain
open Domain
open Distribution

[<EntryPoint>]
let main argv =
    
    // SYSTEM // TODO: Create combinators for data access
    let nodeAddress = Address.New ()
    let mutable blockchain = Blockchain.init ()
    let persist bl = blockchain <- bl; bl

    let mutable network = Network.init (argv.[0] |> sprintf "http://localhost:%s/blockchain" |> NetworkAddress)
    let persistNetwork net = network <- net; net

    // ENDPOINTS
    let doNewTransaction =
        Utility.requestBody
        >> Utility.jsonDeserialize<Transaction.RequestTransaction>
        >> Transaction.FromRequest
        >> (fun trans -> Blockchain.newTransaction blockchain trans)
        >> persist
        >> Blockchain.LastTransacton
        >> Utility.JSON

    let mine =
        Blockchain.mine nodeAddress
        >> persist
        >> Blockchain.LastBlock
        >> Utility.JSON
        
    let doMine _ = mine blockchain 
        
    let doRegisterNode =
        Utility.requestBody
        >> Utility.jsonDeserialize<string>
        >> NetworkAddress
        >> (fun node -> Network.register network node)
        >> persistNetwork
        >> Utility.JSON
        
    let update =
        Network.invokeConsensus
        >> persist
        >> Utility.JSON
        
    let doUpdate _ = update network
    
    // SERVER
    let cfg = { defaultConfig with bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" (int argv.[0]) ] }
    let routes =
        choose [
            OPTIONS >=> Successful.OK ""
            
            GET >=> pathStarts "/blockchain" >=> request (fun _ -> blockchain.Blocks |> Utility.JSON)
            POST >=> pathStarts "/transaction" >=> request doNewTransaction
            POST >=> pathStarts "/mine" >=> context doMine
            
            POST >=> pathStarts "/register-node" >=> request doRegisterNode
            POST >=> pathStarts "/update" >=> context doUpdate
            
            NOT_FOUND "NOT FOUND"
        ] >=> logStructured (Targets.create Error [||]) logFormatStructured
    
    startWebServer cfg routes
    0