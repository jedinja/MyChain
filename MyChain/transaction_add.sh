#!/bin/bash
curl -X POST localhost:4444/transaction -H 'Content-Type: application/json' -d '{"From": "X", "To": "Y", "Amount": 5}' | jq