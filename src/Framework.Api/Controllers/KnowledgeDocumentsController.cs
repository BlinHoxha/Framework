using Framework.Application.AI.Services;
using Framework.Contracts.AI;
using Framework.Contracts.Common;
using Framework.Api.Security;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Api.Controllers;

[ApiController]
[Route("api/v1/knowledge/documents")]
public sealed class KnowledgeDocumentsController(DocumentIngestionService documentIngestionService, IHostEnvironment environment) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CommonResult<KnowledgeDocumentResponse>>> Create(
        [FromBody] CreateKnowledgeDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TenantAccess.IsAllowed(User, request.TenantId, request.ScopeId, environment.IsDevelopment()))
        {
            return Forbid();
        }

        KnowledgeDocumentResponse response = await documentIngestionService.IngestAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            CommonResult<KnowledgeDocumentResponse>.Success(response));
    }
}

