# Bountyhunter
The Epsilon Bountyhunter Guild is proud to present you the second generation of Bountyhunting! Additionally to the well established "kill"-bounty on faction members or single individuals you can now put up literally anything as a bounty on kills and even on Griddamage.



## Commands
All Commands start with /bountyhunt or /bh.

## Put up a new Bounty

/bh new (mission_type) (target_type) (target_name) (payment_amount) (payment) [factor]

- mission_type: Determines what has to be done to earn the bounty 
  - **kill** means kill players
  - **damage** means destroy blocks
- target_type: Determines what is the target
  - **player** means target an individual player (or grids owned by the player)
  - **faction** means all members of the factions (or their grids) are valid targets
- target_name: The name of the target player or the *tag* of the targeted faction
- payment_amount: The Amount of whatever you want to pay. Only whole numbers are allowed. If you use Space Credits they will be taken from your account or from your inventory.
- payment: The Item you want to pay. You can use the Displayname of the item or ay part of it that cleary identifies it.
- factor: (optional) 
  - For kill missions it determines how many kills are required to earn the complete bounty.
  - For damage missions it a factor of the value you set up as payment. Example: You've set up a payment worth 1000 Points (see Item- and Blockvalues) and a factor of 500. That means that a Bountyhunter can claim the whole 1000 Points worth of reward after he destroyed blocks worth 500 Points on the targets grids. So each point of damage is worth 2 points of bounty. The other way would be to set a factor of say 5000. This means that each point of destruction is only worth 0.2 points of the bounty.

## Item- and Blockvalues
Especially for damage missions these Values are relevant. Each Item has a configurable value. You can set up Ingot values and calculate everything else automatically with commands. For example you set up, that an Iron ingot has an value of 1. Then at 1x Assembler efficiency a Steel Plate will have an Value of 21 (it requires 21 Iron Ingots). From that we can see, that an light armor block has a value of 525 as it requires 25 Steel Plates. To check what an Item, block or even a whole grid is woth you can use the value commands:

- /bh value item (searchterm)
- /bh value block (searchterm)
- /bh value grid
  - Stand near the grid, that you want to evaluate

## Claim your bounty
Claiming your bounty is simple. Use the following command. Any Space Credits you earned will be added to your account. Any Items will be either put in your Inventory, a Cargo Container near you, that is owned by you or (if the config wishes so) spawns a Bountybox near you, that contains your claimed bounties. You *can* claim partial bounties. For example if someone hat put up 100 Uranium up for 5 kills of player X and you killed the target 2 times you can claim 40 Uranium.
- /bh claim

