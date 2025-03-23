## HardHat Commands

Initialize HardHat project

```bash
npx hardhat init
```

Compile

```bash
npx hardhat compile
```

Run

```bash
npx hardhat run scripts/deploy.js --network sepolia
```

## hardhat.cofig.js model

```python
require("@nomicfoundation/hardhat-toolbox");
const ALCHEMY_API_KEY = ""
const METAMASK_PRIVATE_KEY = ""

module.exports = {
  solidity: "0.8.28",

  networks:{
    sepolia: {
      url: `https://eth-sepolia.g.alchemy.com/v2/${ALCHEMY_API_KEY}`,
      accounts: [`0x${METAMASK_PRIVATE_KEY}`]
    }
  }
};
```
