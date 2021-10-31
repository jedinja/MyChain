#!/bin/bash
curl -X POST localhost:4445/register-node -H 'Content-Type: application/json' -d '"http://localhost:4444/blockchain"' | jq