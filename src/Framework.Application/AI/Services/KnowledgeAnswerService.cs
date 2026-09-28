using BuildingBlocks.AI.Core.Abstractions;
using BuildingBlocks.AI.Core.Models;
using Framework.Contracts.AI;

namespace Framework.Application.AI.Services;

public sealed class KnowledgeAnswerService(
    IDocumentProfileCatalog profileCatalog,
    IKnowledgeRetriever knowledgeRetriever,
    IChatModel chatModel)
{
    private const string NoEvidenceAnswer = "I could not find sufficient evidence in the authorized documents to answer this question.";

    public async Task<GroundedAnswerResponse> AnswerAsync(
        AskKnowledgeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId is required.", nameof(request));
        }

        DocumentProfile profile = profileCatalog.GetRequired(request.Profile);
        if (!profile.AllowedOperations.Contains("question-answering", StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Profile '{profile.Name}' does not allow question answering.");
        }

        KnowledgeSearchQuery query = new(
            request.Question.Trim(),
            profile.Name,
            request.TenantId,
            request.ScopeId,
            request.DocumentIds,
            request.MaxResults);

        IReadOnlyCollection<KnowledgeChunk> chunks = await knowledgeRetriever.SearchAsync(query, cancellationToken);
        if (chunks.Count == 0)
        {
            return new GroundedAnswerResponse(NoEvidenceAnswer, false, "none", []);
        }

        GroundedGenerationResult generation = await chatModel.GenerateAsync(
            new GroundedGenerationRequest(request.Question.Trim(), profile, chunks),
            cancellationToken);

        CitationResponse[] citations = chunks
            .Select(chunk => new CitationResponse(
                chunk.DocumentId,
                chunk.Title,
                CreateExcerpt(chunk.Content),
                chunk.SourceUri,
                chunk.ChunkNumber))
            .ToArray();

        return new GroundedAnswerResponse(generation.Answer, true, generation.Provider, citations);
    }

    private static string CreateExcerpt(string content) =>
        content.Length <= 240 ? content : $"{content[..237]}...";
}

