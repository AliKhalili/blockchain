﻿using System.Net;
using System.Threading.Tasks;
using SHPA.Blockchain.Blocks;
using SHPA.Blockchain.CQRS;
using SHPA.Blockchain.CQRS.Domain.Commands;
using SHPA.Blockchain.Server.ActionResult;

namespace SHPA.Blockchain.Server.Actions.Custom
{
    public class AddTransactionAction : ActionBase
    {
        private readonly IMediatorHandler _bus;
        public AddTransactionAction(IMediatorHandler bus)
        {
            _bus = bus;
        }
        public override async Task<IActionResult> Execute(HttpListenerRequest request)
        {
            var input = ParseBody<Transaction>(request);
            if (input != null)
            {
                var result = await _bus.Send(new AddTransactionCommand(input));
                if (result.IsSuccess())
                    return new ActionResult<Transaction>().AddResult(input);
                return new ActionResult<Transaction>().AddErrors(result.Errors());

            }
            return new NotFoundActionResult();
        }
    }
}