#!/bin/bash
curl -X POST localhost:4445/update -H 'Content-Type: application/json' | jq