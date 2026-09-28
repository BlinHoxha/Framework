using BuildingBlocks.AI.Core.Abstractions;
using BuildingBlocks.AI.Core.Models;
using Framework.Application.AI.Services;
using Framework.Contracts.AI;
using Framework.Contracts.Common;
using Framework.Api.Security;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
public sealed class AiController(
    KnowledgeAnswerService knowledgeAnswerService,
    IDocumentProfileCatalog profileCatalog,
    IHostEnvironment environment) : ControllerBase
{
    [HttpPost("answers")]
    public async Task<ActionResult<CommonResult<GroundedAnswerResponse>>> Answer(
        [FromBody] AskKnowledgeRequest request,
        CancellationToken cancellationToken)
    {
        if (!TenantAccess.IsAllowed(User, request.TenantId, request.ScopeId, environment.IsDevelopment()))
        {
            return Forbid();
        }

        GroundedAnswerResponse response = await knowledgeAnswerService.AnswerAsync(request, cancellationToken);
        return Ok(CommonResult<GroundedAnswerResponse>.Success(response));
    }

    [HttpGet("document-profiles")]
    public ActionResult<CommonResult<IReadOnlyCollection<DocumentProfileResponse>>> GetDocumentProfiles()
    {
        DocumentProfileResponse[] profiles = profileCatalog.GetAll()
            .Select(MapProfile)
            .OrderBy(profile => profile.Name)
            .ToArray();

        return Ok(CommonResult<IReadOnlyCollection<DocumentProfileResponse>>.Success(profiles));
    }

    private static DocumentProfileResponse MapProfile(DocumentProfile profile) =>
        new(
            profile.Name,
            profile.Description,
            profile.AllowedOperations,
            profile.SupportedLanguages,
            profile.RequiresHumanReview);
}

