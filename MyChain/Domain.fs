module MyChain.Domain

open System

type Address = Address of string

module Address =
    let New () = Guid.NewGuid().ToString().Replace("-", "") |> Address
    let System = Address "0"

type Transaction = {
    Sender: Address
    Recipient: Address
    Amount: double
}

module Transaction =
        
    type RequestTransaction = {
        From: string
        To: string
        Amount: double
    }
    
    let FromRequest rt = {
        Sender = Address rt.From
        Recipient = Address rt.To
        Amount = rt.Amount
    }
    
    let System = { Sender = Address.System; Recipient = Address.System; Amount = 1.0 }

type Proof = Proof of int64
type Hash = Hash of string

type Block = {
    Index: int
    Timestamp: int64
    Transactions: Transaction list
    Proof: Proof
    Previous_Hash: Hash
}

module Block =
    
    let ToHash (block: Block) = block |> Utility._hash |> Hash
        
    let GetIndex block = block.Index
    
    let GetProof block = block.Proof

type Blockchain = {
    Blocks: Block list
    CurrentTransactions: Transaction list
}

module Blockchain =
    
    let LastBlock blockchain = blockchain.Blocks |> List.head
    let LastTransacton blockchain = blockchain.CurrentTransactions |> List.head
    
    let private _newTransaction blockchain transaction =
        { blockchain with CurrentTransactions = transaction::blockchain.CurrentTransactions }
        
    let newTransaction = _newTransaction
    
    let private addMinedTransaction minerAddress blockchain =
        _newTransaction blockchain { Transaction.System with Recipient = minerAddress }
        
    let newBlock blockchain proof =
        let newBlock = {
            Index = blockchain |> LastBlock |> Block.GetIndex |> (+) 1
            Timestamp = Utility.unixTime()
            Transactions = blockchain.CurrentTransactions
            Proof = proof
            Previous_Hash = blockchain |> LastBlock |> Block.ToHash
        }
        
        { CurrentTransactions = []; Blocks = newBlock::blockchain.Blocks }

    let init () = {
        CurrentTransactions = []
        Blocks = [{
            Index = 1
            Timestamp = Utility.unixTime()
            Transactions = []
            Proof = Proof 100L
            Previous_Hash = Hash "1"
        }]
    }
    
    let private proofOfWorkCondition (Proof lastProof) (Proof currentProof) =
        sprintf "%d%d" lastProof currentProof |> Utility._hash |> Utility.startsWith "0"
    
    let private proveWork lastProof =
        seq { for x in 0L .. Int64.MaxValue do Proof x }
        |> Seq.find 
            ( proofOfWorkCondition lastProof )
        
    let mine nodeAddress blockchain =
        blockchain
        |> addMinedTransaction nodeAddress
        |> LastBlock
        |> Block.GetProof
        |> proveWork
        |> newBlock blockchain
         
    let rec private isValidChain = function
        | [] | [_] -> true
        | currentBlock::previousBlock::rest ->
            currentBlock.Previous_Hash = Block.ToHash previousBlock &&
            proofOfWorkCondition previousBlock.Proof currentBlock.Proof &&
            isValidChain (previousBlock::rest)
            
    let isValid { Blocks = blocks } = isValidChain blocks
   
    let consensusCriteria = LastBlock >> Block.GetIndex