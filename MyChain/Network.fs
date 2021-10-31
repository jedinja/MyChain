module MyChain.Distribution

open System
open System.Net
open MyChain.Domain

type NetworkAddress = NetworkAddress of string

type Network = {
    Nodes: Set<NetworkAddress>
}

let nodeBlockchainClient (NetworkAddress url) =
    async {
        try 
            let! html = (new WebClient()).AsyncDownloadString(Uri(url))
            
            return html |> Utility.jsonDeserialize<Block list>
        with
            | ex ->
                printfn "%s" (ex.Message)
                return List.empty<Block>
    }
    

module Network =
    
    let init address =
        { Nodes = Set.ofList [address] }
    
    let register network node = { network with Nodes = network.Nodes.Add node }
    
    let invokeConsensus network =
        network.Nodes
        |> Set.toSeq
        |> Seq.map nodeBlockchainClient
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Array.map (fun chain -> { Blocks = chain; CurrentTransactions = [] })
        |> Array.filter Blockchain.isValid
        |> Array.maxBy Blockchain.consensusCriteria