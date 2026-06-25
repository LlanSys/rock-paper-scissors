# Rock Paper Scissors API

This repository contains a game that allows the caller to play rock paper scissors against the API. An example cURL request would be:

```
curl --request POST \
  --url https://localhost:7114/game/play \
  --header 'Content-Type: application/json' \
  --data '{
	"action": "rock"
}'
```

And the API responds with a text response such as:

```
I play scissors. Winner!
```

# Task 1 - Code Review

Download a copy of the code in this repository and perform a code review. Write your review in a document and include it with Task 2 in a private repository.

# Task 2 -Refactor

Refactor the code so that it supports a 5-way version of rock paper scissors, featuring Fire, Water, Wood, Metal and Earth, as shown in the below rules. To submit your solution, do not submit any PRs, instead invite us to view your own private repository.

- Fire burns Wood
- Fire melts Metal
- Wood displaces Earth
- Wood absorbs Water
- Water extinguishes Fire
- Water rusts Metal
- Earth extinguishes Fire
- Earth absorbs Water
- Metal cuts Wood
- Metal displaces Earth
