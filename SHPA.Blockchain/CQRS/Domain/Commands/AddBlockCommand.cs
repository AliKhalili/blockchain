﻿using SHPA.Blockchain.Blocks;

namespace SHPA.Blockchain.CQRS.Domain.Commands
{
    public class AddBlockCommand:IRequest<DefaultResponse>
    {
        public Block<Transaction> NewBlock { get; }

        public AddBlockCommand(Block<Transaction> newBlock)
        {
            NewBlock = newBlock;
        }
    }
}