using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NoviCode.Application.Wallets.DTOs;
using NoviCode.Application.Wallets.Interfaces;

namespace NoviCode.Api.Controllers
{

    [ApiController]
    [Route("api/wallets")]
    public sealed class WalletsController : ControllerBase
    {

        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }


        [HttpPost]
        [EnableRateLimiting("wallet-create-sliding-ip")]
        [ProducesResponseType(typeof(CreateWalletResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateWalletResponse>> Create([FromBody] CreateWalletRequest request,CancellationToken cancellationToken)
        {
            var result = await _walletService.CreateAsync(request, cancellationToken);
            return Created($"/api/wallets/{result.Id}", result);
        }

        [HttpGet("{walletId:long}")]
        [EnableRateLimiting("wallet-read-sliding-ip")]
        [ProducesResponseType(typeof(GetWalletBalanceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetWalletBalanceResponse>> GetBalance(long walletId, [FromQuery] string? currency,CancellationToken cancellationToken)
        {
            var result = await _walletService.GetBalanceAsync(walletId, currency, cancellationToken);
            return Ok(result);
        }


        [HttpPost("{walletId:long}/adjustbalance")]
        [EnableRateLimiting("wallet-adjust-sliding-ip")]
        [ProducesResponseType(typeof(AdjustWalletBalanceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AdjustWalletBalanceResponse>> AdjustBalance([FromRoute] long walletId,[FromQuery] decimal amount,
                                                                                   [FromQuery] string strategy,
                                                                                   [FromQuery] string currency,
                                                                                   CancellationToken cancellationToken)
        {
            var result = await _walletService.AdjustBalanceAsync(walletId, amount, currency, strategy, cancellationToken);
            return Ok(result);
        }
    }
}
